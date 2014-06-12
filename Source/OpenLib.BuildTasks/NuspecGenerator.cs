using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using OpenLib.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace OpenLib.BuildTasks
{
    /// <summary>
    /// The <c>NuspecGenerator</c> type provides a custom MSBuild task for
    /// generating a Nuspec file.
    /// </summary>
    public class NuspecGenerator : Task
    {
        //---------------------------------------------------------------------
        // Fields
        //---------------------------------------------------------------------

        /// <summary>
        /// Defines the extension of the Nuspec file.
        /// </summary>
        public const string Extension = "nuspec";

        /// <summary>
        /// Defines a list of default files to include in the Nuspec file.
        /// </summary>
        private readonly List<Tuple<string, string>> DefaultFiles = new List<Tuple<string,string>>
        {
            new Tuple<string, string>(@"**\*.dll", "lib"),
            new Tuple<string, string>(@"**\*.exe", "lib"),
            new Tuple<string, string>(@"**\*.sql", "lib"),
            new Tuple<string, string>(@"**\*.dtsx", "lib"),
            new Tuple<string, string>(@"**\*.config", "content"),
            new Tuple<string, string>(@"**\*.xml", "content"),
            new Tuple<string, string>(@"**\*.dtsConfig", "content"),
            new Tuple<string, string>(@"**\*.zip", "content"),
            new Tuple<string, string>(@"**\*.cmd", "content"),
            new Tuple<string, string>(@"**\*.bat", "content")
        };

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        /// <summary>
        /// Gets or sets a reference to the code information utilities.
        /// </summary>
        private CodeInfoUtils CodeInfoUtils { get; set; }

        /// <summary>
        /// Gets or sets the code language used.
        /// </summary>
        private CodeLanguage CodeLang { get; set; }

        /// <summary>
        /// Gets or sets a reference to the I/O utilities.
        /// </summary>
        public IIoUtils IoUtils { get; set; }

        /// <summary>
        /// Gets or sets the directory location of the package as a required
        /// task property.
        /// </summary>
        [Required]
        public string PackageDir { get; set; }

        /// <summary>
        /// Gets or sets the directory location of the project as a required
        /// task property.
        /// </summary>
        [Required]
        public string ProjectDir { get; set; }

        /// <summary>
        /// Gets or sets the path to the output directory as a required
        /// task property.
        /// </summary>
        [Required]
        public string OutputPath { get; set; }

        /// <summary>
        /// Gets or sets the code language to use as a required task property.
        /// </summary>
        [Required]
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the configuration as an optional task property.
        /// </summary>
        public string Configuration { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if the default set of files should
        /// be excluded using an array of custom file task items.
        /// </summary>
        public bool ExcludeDefaultFiles { get; set; }

        /// <summary>
        /// Gets or sets an array of dependency task items from an item
        /// group as an optional task property.
        /// </summary>
        public ITaskItem[] Dependencies { get; set; }

        /// <summary>
        /// Gets or sets an array of custom file task items from an item
        /// group as an optional task property.
        /// </summary>
        public ITaskItem[] CustomFiles { get; set; }

        /// <summary>
        /// Gets or sets the location of the Nuspec file as a task output
        /// property.
        /// </summary>
        [Output]
        public string NuspecFile { get; set; }

        //---------------------------------------------------------------------
        // Constructors
        //---------------------------------------------------------------------

        /// <summary>
        /// Creates a new instance of the <c>NuspecGenerator</c> class.
        /// </summary>
        public NuspecGenerator()
        {
            this.CodeInfoUtils = new CodeInfoUtils();
            this.IoUtils = new IoUtils();
        }

        //---------------------------------------------------------------------
        // Abstract Implementation Methods
        //---------------------------------------------------------------------

        /// <summary>
        /// Executes the custom task when it is invoked in a MSBuild script.
        /// </summary>
        /// <returns>A value indicating if the task completely successfully.</returns>
        public override bool Execute()
        {
            Console.WriteLine("Executing NuspecGenerator MSBuild task...");

            if (this.PackageDir != null &&
                this.ProjectDir != null &&
                !string.IsNullOrWhiteSpace(this.OutputPath) &&
                !string.IsNullOrWhiteSpace(this.Language))
            {
                this.CodeLang = this.CodeInfoUtils.GetCodeLanguage(this.Language);

                Console.WriteLine("Package directory: {0}", this.PackageDir);
                Console.WriteLine("Project directory: {0}", this.ProjectDir);
                Console.WriteLine("Output path: {0}", this.OutputPath);
                Console.WriteLine("Code Language: {0}", this.CodeLang.ToString());
                Console.WriteLine("Configuration: {0}", this.Configuration);

                Dictionary<string, string> attributes = this.GetAttributes();

                XDocument nuspec = new XDocument
                (
                    new XElement("package",
                        new XElement("metadata",
                            new XElement("id", attributes["Id"]),
                            new XElement("description", attributes["Description"]),
                            new XElement("authors", attributes["Authors"]),
                            new XElement("version", attributes["Version"]),
                            new XElement("dependencies")
                        ),
                        new XElement("files")
                    )
                );

                this.AddDependencies(nuspec);
                this.AddCustomFiles(nuspec);

                this.NuspecFile = Path.Combine(this.PackageDir, string.Concat(attributes["Id"], ".", Extension));
                Console.WriteLine("Nuspec file: {0}", this.NuspecFile);

                nuspec.Save(this.NuspecFile);
                Console.WriteLine("SUCCESSFULLY generated Nuspec file!");
                return true;
            }

            Console.WriteLine("FAILED to generate Nuspec file");
            return false;
        }

        //---------------------------------------------------------------------
        // Other Methods
        //---------------------------------------------------------------------

        /// <summary>
        /// Gets attributes required to generate the Nuspec file.
        /// </summary>
        /// <returns>A dictionary of attributes for the Nuspec file.</returns>
        private Dictionary<string, string> GetAttributes()
        {
            Dictionary<string, string> attributes = new Dictionary<string, string>();
            IProjectInfo projectInfo = null;

            switch (this.CodeLang)
            {
                case CodeLanguage.TSql:
                    projectInfo = new DbInfo { ProjectDir = this.ProjectDir };
                    break;

                case CodeLanguage.Etl:
                    projectInfo = new EtlInfo { ProjectDir = this.ProjectDir };
                    break;

                case CodeLanguage.Cobol:
                    projectInfo = new CobolInfo { ProjectDir = this.ProjectDir };
                    break;

                default:
                    projectInfo = new AssemblyInfo { AssemblyPath = this.OutputPath };
                    break;
            }

            projectInfo.Execute();

            attributes.Add("Id", CleanId(this.GetId(projectInfo.Title)));
            attributes.Add("Description", projectInfo.Description);
            attributes.Add("Authors", projectInfo.Company);
            attributes.Add("Version", projectInfo.Version);

            return attributes;
        }

        /// <summary>
        /// Gets the identifier from the specified title.
        /// </summary>
        /// <param name="title">The title containing the identifier.</param>
        /// <returns>The identifier from the title.</returns>
        private string GetId(string title)
        {
            if (!string.IsNullOrWhiteSpace(this.Configuration))
                return string.Concat(title, ".", this.Configuration);
            else
                return title;
        }

        /// <summary>
        /// Cleans the specified identifier of unwanted characters.
        /// </summary>
        /// <param name="id">The identifier to clean.</param>
        /// <returns>The identifier cleansed of unwanted characters.</returns>
        private string CleanId(string id)
        {
            return id.Replace(" ", "");
        }

        /// <summary>
        /// Adds dependencies to the specified Nuspec document.
        /// </summary>
        /// <param name="nuspec">The Nuspec document in which to add dependencies
        /// to.</param>
        private void AddDependencies(XDocument nuspec)
        {
            List<Tuple<string, string, string>> items = new List<Tuple<string, string, string>>();

            if (this.Dependencies != null && this.Dependencies.Length > 0)
            {
                items.AddRange(this.Dependencies.Select(i =>
                    new Tuple<string, string, string>(
                        i.ItemSpec,
                        i.GetMetadata("Version"),
                        i.GetMetadata("TargetFramework"))
                    ));
            }

            XElement e = nuspec.Element("package").Element("metadata").Element("dependencies");

            if (e != null)
            {
                e.Add(new XElement("group"));
                XElement eDefaultGroup = (XElement)e.LastNode;

                items.ForEach(i =>
                {
                    if (!string.IsNullOrWhiteSpace(i.Item3))
                    {
                        XElement eGroup = (from g in e.Descendants("group")
                                           where g != null &&
                                                g.Attribute("targetFramework") != null &&
                                                g.Attribute("targetFramework").Value.Equals(i.Item3)
                                           select g
                                          ).SingleOrDefault();

                        if (eGroup == null)
                        {
                            e.Add(new XElement("group", new XAttribute("targetFramework", i.Item3)));
                            eGroup = (XElement)e.LastNode;
                        }

                        eGroup.Add(new XElement("dependency",
                            new XAttribute("id", i.Item1),
                            new XAttribute("version", i.Item2)
                        ));
                    }
                    else
                    {
                        eDefaultGroup.Add(new XElement("dependency",
                            new XAttribute("id", i.Item1),
                            new XAttribute("version", i.Item2)
                        ));
                    }
                });
            }
        }

        /// <summary>
        /// Adds custom files to the specified Nuspec document.
        /// </summary>
        /// <param name="nuspec">The Nuspec document in which to add custom
        /// files to.</param>
        /// <remarks>
        /// If the ExcludeDefaultFiles property is set to false, the default
        /// list of files will be added, otherwise, they will not be added. If
        /// the CustomFiles property is set, the custom files specified will be
        /// added as well.
        /// </remarks>
        private void AddCustomFiles(XDocument nuspec)
        {
            List<Tuple<string, string>> items = new List<Tuple<string, string>>();

            if (!this.ExcludeDefaultFiles)
                items.AddRange(DefaultFiles);

            if (this.CustomFiles != null && this.CustomFiles.Length > 0)
            {
                items.AddRange(this.CustomFiles.Select(i =>
                    new Tuple<string, string>
                        (i.ItemSpec, this.GetTarget(
                            i.ItemSpec,
                            i.GetMetadata("FileType")))
                        ));
            }

            XElement e = nuspec.Element("package").Element("files");

            if (e != null)
            {
                items.ForEach(i =>
                    e.Add(new XElement("file",
                            new XAttribute("src", i.Item1),
                            new XAttribute("target", i.Item2)
                        )));
            }
        }

        /// <summary>
        /// Gets the target directory or file based on the source.
        /// </summary>
        /// <param name="source">The source directory or file.</param>
        /// <param name="target">The target directory or file.</param>
        /// <returns>The target directory or file based on the source.</returns>
        private string GetTarget(string source, string target)
        {
            if (this.IoUtils.IsDirectory(source))
                return Path.Combine(target, source);

            return target;
        }
    }
}
