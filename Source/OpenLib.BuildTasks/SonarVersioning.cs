using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using OpenLib.Extensions;
using OpenLib.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenLib.BuildTasks
{
    /// <summary>
    /// The <c>SonarVersioning</c> type provides a custom MSBuild task for
    /// dynamically versioning a Sonar project configuration file.
    /// </summary>
    public class SonarVersioning : Task
    {
        //---------------------------------------------------------------------
        // Fields
        //---------------------------------------------------------------------

        /// <summary>
        /// Defines the name of the Sonar project configuration file.
        /// </summary>
        public const string SonarProjectConfig = "sonar-project.properties";

        /// <summary>
        /// Defines a list of values that should be contained on a line.
        /// </summary>
        private readonly List<string> LineContains = new List<string>() { "sonar.projectVersion" };

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        /// <summary>
        /// Gets or sets a reference to the I/O utilities.
        /// </summary>
        public IIoUtils IoUtils { get; set; }

        /// <summary>
        /// Gets or sets the directory location of the solution as a required
        /// task property.
        /// </summary>
        [Required]
        public string SolutionDir { get; set; }

        /// <summary>
        /// Gets or sets the directory location of the project as a required
        /// task property.
        /// </summary>
        [Required]
        public string ProjectDir { get; set; }

        /// <summary>
        /// Gets or sets the version number as a required task property.
        /// </summary>
        [Required]
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the path to the output file that contains the Sonar
        /// project version number as a task output property.
        /// </summary>
        [Output]
        public string OutputFilePath { get; set; }

        //---------------------------------------------------------------------
        // Constructors
        //---------------------------------------------------------------------

        /// <summary>
        /// Creates a new instance of the <c>SonarVersioning</c> class.
        /// </summary>
        public SonarVersioning()
        {
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
            Console.WriteLine("Executing SonarVersioning MSBuild task...");

            if ((!string.IsNullOrWhiteSpace(this.SolutionDir) ||
                !string.IsNullOrWhiteSpace(this.ProjectDir)) &&
                !string.IsNullOrWhiteSpace(this.Version))
            {
                bool isSet = this.SetPaths();

                if (isSet)
                {
                    Console.WriteLine("Attempting to apply version '{0}' to Sonar project configuration file", this.OutputFilePath);
                    string contents = this.Apply(this.OutputFilePath);

                    if (!string.IsNullOrWhiteSpace(contents))
                    {
                        bool applied = this.IoUtils.WriteFile(this.OutputFilePath, contents);

                        if (applied)
                        {
                            Console.WriteLine("SUCCESSFULLY applied version {0} to Sonar project configuration file!", this.Version);
                            return true;
                        }
                    }
                }
                else
                    Console.WriteLine("ERROR: Unable to locate Sonar project configuration file");
            }

            Console.WriteLine("FAILED to apply version to Sonar project configuration file");
            return false;
        }

        //---------------------------------------------------------------------
        // Other Methods
        //---------------------------------------------------------------------

        /// <summary>
        /// Sets the output file path first using the solution directory. If
        /// output file path is not found, it is then set using the project
        /// directory.
        /// </summary>
        /// <returns>A value indicating if the output file path was set.</returns>
        private bool SetPaths()
        {
            bool isSolutionDirSet = !string.IsNullOrWhiteSpace(this.SolutionDir);
            bool isProjectDirSet = !string.IsNullOrWhiteSpace(this.ProjectDir);

            if (isSolutionDirSet)
            {
                this.OutputFilePath = Path.Combine(this.SolutionDir, SonarProjectConfig);

                if (this.IoUtils.FileExists(this.OutputFilePath))
                    return true;
            }

            if (!isSolutionDirSet && isProjectDirSet)
            {
                this.OutputFilePath = Path.Combine(this.ProjectDir, SonarProjectConfig);

                if (this.IoUtils.FileExists(this.OutputFilePath))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Reads the Sonar project configuration file into a stream, applies the
        /// version and returns the contents of the Sonar project configuration
        /// file so it can be overwritten with the version number.
        /// </summary>
        /// <param name="path">The path to the Sonar project configuration file.</param>
        /// <returns>The updated contents of the Sonar project configuration file.</returns>
        private string Apply(string path)
        {
            string contents = null;

            if (!string.IsNullOrWhiteSpace(path) && this.IoUtils.FileExists(path))
            {
                using (FileStream stream = this.IoUtils.ReadFileAsStream(path))
                {
                    if (stream != null)
                    {
                        StringBuilder fileBuilder = new StringBuilder();

                        using (StreamReader reader = new StreamReader(stream))
                        {
                            if (reader != null)
                            {
                                while (!reader.EndOfStream)
                                {
                                    string data = reader.ReadLine();

                                    if (data.ContainedIn(LineContains))
                                        data = this.Format(data);

                                    fileBuilder.AppendLine(data);
                                }

                                reader.Close();
                            }
                        }

                        stream.Close();
                        contents = fileBuilder.ToString();
                    }
                }
            }

            return contents;
        }

        /// <summary>
        /// Formats the version number using the specified data containing
        /// the version number.
        /// </summary>
        /// <param name="data">The data containing the version number.</param>
        /// <returns>The data containing the version number.</returns>
        private string Format(string data)
        {
            int startIndex = data.IndexOf("=") + 1;
            int length = data.Length - startIndex;

            string version = data;
            string value = version.Substring(startIndex, length);

            version = version.Replace(value, this.Version);
            return version;
        }
    }
}
