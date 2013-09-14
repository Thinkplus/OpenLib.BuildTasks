using GAFRI.Common;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GAFRI.CustomBuildTasks
{
    /// <summary>
    /// The <c>SonarVersioning</c> type provides a custom MSBuild task for
    /// dynamically versioning a Sonar project configuration file.
    /// </summary>
    public class SonarVersioning : Task
    {
        /// <summary>
        /// Defines the name of the Sonar project configuration file.
        /// </summary>
        private const string SonarProjectConfig = "sonar-project.properties";

        /// <summary>
        /// Defines a list of values that should be contained on a line.
        /// </summary>
        private List<string> lineContains = new List<string>() { "sonar.projectVersion" };

        /// <summary>
        /// Gets or sets a reference to the IO utilities.
        /// </summary>
        public IIOUtilities IOUtilities { get; set; }

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

        /// <summary>
        /// Creates a new instance of the <c>SonarVersioning</c> class.
        /// </summary>
        public SonarVersioning()
        {
            this.IOUtilities = new IOUtilities();
        }

        /// <summary>
        /// Executes the custom task when it is invoked in a MSBuild script.
        /// </summary>
        /// <returns>A value indicating if the task completely successfully.</returns>
        public override bool Execute()
        {
            Console.WriteLine("Executing SonarVersioning MSBuild task...");

            if (this.SolutionDir != null && this.ProjectDir != null && this.Version != null)
            {
                this.SetPaths();

                Console.WriteLine("Attempting to apply version '{0}' to Sonar project configuration", this.OutputFilePath);

                string contents = this.Apply(this.OutputFilePath);

                if (!string.IsNullOrWhiteSpace(contents))
                {
                    bool wasWritten = this.IOUtilities.WriteFile(this.OutputFilePath, contents);

                    if (wasWritten)
                    {
                        Console.WriteLine("SUCCESSFULLY applied version {0} to Sonar project configuration!", this.Version);
                        return true;
                    }
                }
                else
                    Console.WriteLine("ERROR: Unable to locate Sonar project configuration file");
            }

            Console.WriteLine("FAILED to apply version to Sonar project configuration");

            return false;
        }

        /// <summary>
        /// Sets the output file path first using the solution directory. If
        /// output file path is not found, it is then set using the project
        /// directory.
        /// </summary>
        private void SetPaths()
        {
            this.OutputFilePath = Path.Combine(this.SolutionDir, SonarProjectConfig);

            if (!(new FileInfo(this.OutputFilePath).Exists))
            {
                this.OutputFilePath = Path.Combine(this.ProjectDir, SonarProjectConfig);
            }
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
            if (!string.IsNullOrWhiteSpace(path) && (new FileInfo(path).Exists))
            {
                using (FileStream stream = this.IOUtilities.ReadFileAsStream(path))
                {
                    if (stream != null)
                    {
                        StringBuilder contents = new StringBuilder();

                        using (StreamReader reader = new StreamReader(stream))
                        {
                            if (reader != null)
                            {
                                while (!reader.EndOfStream)
                                {
                                    string data = reader.ReadLine();

                                    if (data.ContainedIn(lineContains))
                                        data = this.Format(data);

                                    contents.AppendLine(data);
                                }

                                reader.Close();
                            }
                        }

                        stream.Close();

                        return contents.ToString();
                    }
                }
            }

            return null;
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
