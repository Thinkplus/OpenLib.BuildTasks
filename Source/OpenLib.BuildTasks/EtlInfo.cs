using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using OpenLib.Extensions;
using OpenLib.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenLib.BuildTasks
{
    /// <summary>
    /// The <c>EtlInfo</c> type provides a custom MSBuild task for reading
    /// ETL information attributes for an ETL project.
    /// </summary>
    public class EtlInfo : Task
    {
        /// <summary>
        /// Defines the path to the ETL information file.
        /// </summary>
        public const string EtlInfoFile = "EtlInfo.etl";

        /// <summary>
        /// Defines a dictionary of attributes to read from the ETL
        /// information file.
        /// </summary>
        private readonly Dictionary<string, string> Attributes = new Dictionary<string, string>()
        {
            { "Title" , "EtlTitle" },
            { "Description", "EtlDescription" },
            { "Company", "EtlCompany" },
            { "Version", "EtlVersion" },
            { "SemanticVersion", "EtlInformationalVersion" }
        };

        /// <summary>
        /// Defines a list of values that not should be contained on a line.
        /// </summary>
        private readonly List<string> LineNotContains = new List<string>() { "//", "--" };

        /// <summary>
        /// Defines the first version attribute found in the ETL
        /// information file.
        /// </summary>
        private string firstVersion;

        /// <summary>
        /// Gets or sets a reference to the I/O utilities.
        /// </summary>
        public IIoUtils IoUtils { get; set; }

        /// <summary>
        /// Gets or sets the directory location of the ETL project as a
        /// required task property.
        /// </summary>
        [Required]
        public string ProjectDir { get; set; }

        /// <summary>
        /// Gets or sets the path to the ETL information file as a task
        /// output property.
        /// </summary>
        [Output]
        public string EtlInfoPath { get; set; }

        /// <summary>
        /// Gets or sets the ETL title as a task output property.
        /// </summary>
        [Output]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the ETL description as a task output property.
        /// </summary>
        [Output]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the ETL company as a task output property.
        /// </summary>
        [Output]
        public string Company { get; set; }

        /// <summary>
        /// Gets or sets the ETL version as a task output property.
        /// </summary>
        [Output]
        public string Version { get; set; }

        /// <summary>
        /// Creates a new instance of the <c>EtlInfo</c> class.
        /// </summary>
        public EtlInfo()
        {
            this.IoUtils = new  IoUtils();
        }

        /// <summary>
        /// Executes the custom task when it is invoked in a MSBuild script.
        /// </summary>
        /// <returns>A value indicating if the task completely successfully.</returns>
        public override bool Execute()
        {
            Console.WriteLine("Executing EtlInfo MSBuild task...");

            if (this.ProjectDir != null)
            {
                this.SetPaths();

                Console.WriteLine("Attempting to obtain ETL information for '{0}'", this.EtlInfoPath);

                bool obtained = this.Obtain();

                Console.WriteLine("Title: {0}", this.Title);
                Console.WriteLine("Description: {0}", this.Description);
                Console.WriteLine("Company: {0}", this.Company);
                Console.WriteLine("Version: {0}", this.Version);

                if (obtained)
                {
                    Console.WriteLine("SUCCESSFULLY obtained ETL information!");

                    return true;
                }
            }

            Console.WriteLine("FAILED to obtain ETL information");

            return false;
        }

        /// <summary>
        /// Sets the ETL information path.
        /// </summary>
        private void SetPaths()
        {
            if (string.IsNullOrWhiteSpace(this.EtlInfoPath))
            {
                this.EtlInfoPath = Path.Combine(this.ProjectDir, EtlInfoFile);
            }
        }

        /// <summary>
        /// Obtains the ETL information from the ETL information file.
        /// </summary>
        /// <returns>A value indicating if the ETL information was obtained.</returns>
        private bool Obtain()
        {
            if (this.IoUtils.FileExists(this.EtlInfoPath))
            {
                using (FileStream stream = this.IoUtils.ReadFileAsStream(this.EtlInfoPath))
                {
                    if (stream != null)
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            if (reader != null)
                            {
                                while (!reader.EndOfStream)
                                {
                                    string data = reader.ReadLine();

                                    if (!data.ContainedIn(LineNotContains))
                                        this.SetAttribute(data);
                                }

                                reader.Close();
                            }
                        }

                        stream.Close();
                    }
                }
            }

            return (!string.IsNullOrWhiteSpace(this.Title) &&
                    !string.IsNullOrWhiteSpace(this.Description) &&
                    !string.IsNullOrWhiteSpace(this.Company) &&
                    !string.IsNullOrWhiteSpace(this.Version));
        }

        /// <summary>
        /// Sets ETL information attributes using the specified data.
        /// </summary>
        /// <param name="data">Data containing ETL information.</param>
        private void SetAttribute(string data)
        {
            if (data.Contains(Attributes["Title"]))
            {
                this.Title = this.Extract(data);
            }
            else if (data.Contains(Attributes["Description"]))
            {
                this.Description = this.Extract(data);
            }
            else if (data.Contains(Attributes["Company"]))
            {
                this.Company = this.Extract(data);
            }
            else if (data.Contains(Attributes["Version"]))
            {
                this.Version = this.ExtractVersion("Version", data);
            }
            else if (data.Contains(Attributes["SemanticVersion"]))
            {
                this.Version = this.ExtractVersion("SemanticVersion", data);
            }
        }

        /// <summary>
        /// Extracts an ETL information attribute from the specified data.
        /// </summary>
        /// <param name="data">Data containing ETL information.</param>
        /// <returns>The value of an ETL information attribute.</returns>
        private string Extract(string data)
        {
            int startIndex = data.IndexOf("\"") + 1;
            int length = data.LastIndexOf("\"") - startIndex;

            return data.Substring(startIndex, length);
        }

        /// <summary>
        /// Extracts the ETL version number from the specified data.
        /// </summary>
        /// <param name="attribute">The name of the ETL information attribute.</param>
        /// <param name="data">Data containing ETL information.</param>
        /// <returns>The ETL version number.</returns>
        private string ExtractVersion(string attribute, string data)
        {
            if (string.IsNullOrWhiteSpace(firstVersion))
                firstVersion = attribute;
            
            if (!string.IsNullOrWhiteSpace(this.Version) && firstVersion.Equals("SemanticVersion"))
            {
                return this.Version;
            }
            else
            {
                return this.Extract(data);
            }
        }
    }
}
