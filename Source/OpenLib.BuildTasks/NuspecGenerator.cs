using GAFRI.Common;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace GAFRI.CustomBuildTasks
{
    /// <summary>
    /// The <c>NuspecGenerator</c> type provides a custom MSBuild task for
    /// generating a Nuspec file based on an assembly and package directory.
    /// </summary>
    public class NuspecGenerator : Task
    {
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

        /// <summary>
        /// Defines the extension of the Nuspec file.
        /// </summary>
        private const string Extension = "nuspec";

        /// <summary>
        /// Gets or sets a reference to the IO utilities.
        /// </summary>
        public IIOUtilities IOUtilities { get; set; }

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
        /// Gets or sets the programming language to use as a required task
        /// property.
        /// </summary>
        [Required]
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the configuration as an optional task property.
        /// </summary>
        public string Configuration { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if the default set of files should
        /// be overriden using the array of custom file task items.
        /// </summary>
        public bool OverrideDefaultFiles { get; set; }

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

        /// <summary>
        /// Creates a new instance of the <c>NuspecGenerator</c> class.
        /// </summary>
        public NuspecGenerator()
        {
            this.IOUtilities = new IOUtilities();
        }

        /// <summary>
        /// Executes the custom task when it is invoked in a MSBuild script.
        /// </summary>
        /// <returns>A value indicating if the task completely successfully.</returns>
        public override bool Execute()
        {
            Console.WriteLine("Executing Nuspec generator MSBuild task...");

            if (this.PackageDir != null &&
                this.ProjectDir != null &&
                !string.IsNullOrWhiteSpace(this.OutputPath) &&
                !string.IsNullOrWhiteSpace(this.Language))
            {
                Console.WriteLine("Package directory: {0}", this.PackageDir);
                Console.WriteLine("Project directory: {0}", this.ProjectDir);
                Console.WriteLine("Output path: {0}", this.OutputPath);
                Console.WriteLine("Language: {0}", this.Language);
                Console.WriteLine("Configuration: {0}", this.Configuration);

                Dictionary<string, string> attributes = this.GetAttributes();

                XDocument nuspec = new XDocument
                (
                    new XElement("package",
                        new XElement("metadata",
                            new XElement("id", attributes["Id"]),
                            new XElement("description", attributes["Description"]),
                            new XElement("authors", attributes["Authors"]),
                            new XElement("version", attributes["Version"])
                        ),
                        new XElement("files")
                    )
                );

                this.AddFiles(nuspec);

                this.NuspecFile = Path.Combine(this.PackageDir, string.Concat(attributes["Id"], ".", Extension));

                Console.WriteLine("Nuspec file: {0}", this.NuspecFile);

                nuspec.Save(this.NuspecFile);

                Console.WriteLine("SUCCESSFULLY generated Nuspec file!");

                return true;
            }

            Console.WriteLine("FAILED to generate Nuspec file");

            return false;
        }

        /// <summary>
        /// Gets attributes required to generate the Nuspec file.
        /// </summary>
        /// <returns>A dictionary of attributes for the Nuspec file.</returns>
        private Dictionary<string, string> GetAttributes()
        {
            Dictionary<string, string> attributes = new Dictionary<string, string>();

            if (this.Language.Equals("TSQL"))
            {
                DbInfo dbInfo = new DbInfo { ProjectDir = this.ProjectDir };

                dbInfo.Execute();

                attributes.Add("Id", this.GetId(dbInfo.Title));
                attributes.Add("Description", dbInfo.Description);
                attributes.Add("Authors", dbInfo.Company);
                attributes.Add("Version", dbInfo.Version);
            }
            else if (this.Language.Equals("ETL"))
            {
                EtlInfo etlInfo = new EtlInfo { ProjectDir = this.ProjectDir };

                etlInfo.Execute();

                attributes.Add("Id", this.GetId(etlInfo.Title));
                attributes.Add("Description", etlInfo.Description);
                attributes.Add("Authors", etlInfo.Company);
                attributes.Add("Version", etlInfo.Version);
            }
            else
            {
                Assembly assembly = Assembly.LoadFrom(this.OutputPath);

                attributes.Add("Id", this.GetId(assembly));
                attributes.Add("Description", this.GetDescription(assembly));
                attributes.Add("Authors", this.GetCompany(assembly));
                attributes.Add("Version", this.GetVersion(assembly));
            }

            if (attributes.ContainsKey("Id"))
                attributes["Id"] = attributes["Id"].Replace(" ", "");

            return attributes;
        }

        /// <summary>
        /// Gets the identifier for the specified database.
        /// </summary>
        /// <param name="title">The title of the database.</param>
        /// <returns>The identifier of the database.</returns>
        private string GetId(string title)
        {
            if (!string.IsNullOrWhiteSpace(this.Configuration))
                return string.Concat(title, ".", this.Configuration);
            else
                return title;
        }

        /// <summary>
        /// Gets the identifier for the specified assembly.
        /// </summary>
        /// <param name="assembly">A reference to the assembly.</param>
        /// <returns>The identifier of the assembly.</returns>
        private string GetId(Assembly assembly)
        {
            if (!string.IsNullOrWhiteSpace(this.Configuration))
                return string.Concat(assembly.GetName().Name, ".", this.Configuration);
            else
                return assembly.GetName().Name;
        }

        /// <summary>
        /// Gets the description for the specified assembly.
        /// </summary>
        /// <param name="assembly">A reference to the assembly.</param>
        /// <returns>The description of the assembly.</returns>
        private string GetDescription(Assembly assembly)
        {
            return ((AssemblyDescriptionAttribute) (Attribute.GetCustomAttribute(assembly, typeof(AssemblyDescriptionAttribute)))).Description;
        }

        /// <summary>
        /// Gets the company for the specified assembly.
        /// </summary>
        /// <param name="assembly">A reference to the assembly.</param>
        /// <returns>The company of the assembly.</returns>
        private string GetCompany(Assembly assembly)
        {
            return ((AssemblyCompanyAttribute)(Attribute.GetCustomAttribute(assembly, typeof(AssemblyCompanyAttribute)))).Company;
        }

        /// <summary>
        /// Gets the version for the specified assembly.
        /// </summary>
        /// <param name="assembly">A reference to the assembly.</param>
        /// <returns>The version of the assembly.</returns>
        private string GetVersion(Assembly assembly)
        {
            AssemblyInformationalVersionAttribute semanticVersion = Attribute.GetCustomAttribute(assembly, typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute;

            if (semanticVersion != null)
                return semanticVersion.InformationalVersion;
            else
                return assembly.GetName().Version.ToString();
        }

        /// <summary>
        /// Adds files to the specified Nuspec document.
        /// </summary>
        /// <param name="nuspec">The Nuspec document in which to add files to.</param>
        /// <remarks>
        /// If the CustomFiles property is set, the default list of files will
        /// not be added. The custom files specified will be added in their
        /// place.
        /// </remarks>
        private void AddFiles(XDocument nuspec)
        {
            List<Tuple<string, string>> items = new List<Tuple<string, string>>();

            if (!this.OverrideDefaultFiles)
                items.AddRange(DefaultFiles);

            if (this.CustomFiles != null && this.CustomFiles.Length > 0)
            {
                items.AddRange(this.CustomFiles.Select(i => new Tuple<string, string>(i.ItemSpec, this.GetTarget(i.ItemSpec, i.GetMetadata("FileType")))));
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
        /// <param name="fileType">The type of the file.</param>
        /// <returns>The target directory or file based on the source.</returns>
        private string GetTarget(string source, string fileType)
        {
            if (this.IOUtilities.IsDirectory(source))
                return Path.Combine(fileType, source);

            return fileType;
        }
    }
}
