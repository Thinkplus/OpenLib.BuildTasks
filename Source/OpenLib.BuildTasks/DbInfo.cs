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
    /// The <c>DbInfo</c> type provides a custom MSBuild task for reading
    /// database information attributes for a database project.
    /// </summary>
    public class DbInfo : Task
    {
        /// <summary>
        /// Defines the path to the database information file.
        /// </summary>
        private const string DbInfoFile = @"Properties\DbInfo.db";

        /// <summary>
        /// Defines a dictionary of attributes to read from the database
        /// information file.
        /// </summary>
        private readonly Dictionary<string, string> Attributes = new Dictionary<string, string>()
        {
            { "Title" , "DbTitle" },
            { "Description", "DbDescription" },
            { "Company", "DbCompany" },
            { "Version", "DbVersion" },
            { "SemanticVersion", "DbInformationalVersion" }
        };

        /// <summary>
        /// Defines a list of values that not should be contained on a line.
        /// </summary>
        private readonly List<string> LineNotContains = new List<string>() { "//", "--" };

        /// <summary>
        /// Defines the first version attribute found in the database
        /// information file.
        /// </summary>
        private string firstVersion;

        /// <summary>
        /// Gets or sets a reference to the I/O utilities.
        /// </summary>
        public IIoUtils IoUtils { get; set; }

        /// <summary>
        /// Gets or sets the directory location of the database project as a
        /// required task property.
        /// </summary>
        [Required]
        public string ProjectDir { get; set; }

        /// <summary>
        /// Gets or sets the path to the database information file as a task
        /// output property.
        /// </summary>
        [Output]
        public string DbInfoPath { get; set; }

        /// <summary>
        /// Gets or sets the database title as a task output property.
        /// </summary>
        [Output]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the database description as a task output property.
        /// </summary>
        [Output]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the database company as a task output property.
        /// </summary>
        [Output]
        public string Company { get; set; }

        /// <summary>
        /// Gets or sets the database version as a task output property.
        /// </summary>
        [Output]
        public string Version { get; set; }

        /// <summary>
        /// Creates a new instance of the <c>DbInfo</c> class.
        /// </summary>
        public DbInfo()
        {
            this.IoUtils = new IoUtils();
        }

        /// <summary>
        /// Executes the custom task when it is invoked in a MSBuild script.
        /// </summary>
        /// <returns>A value indicating if the task completely successfully.</returns>
        public override bool Execute()
        {
            Console.WriteLine("Executing DbInfo MSBuild task...");

            if (this.ProjectDir != null)
            {
                this.SetPaths();

                Console.WriteLine("Attempting to obtain database information for '{0}'", this.DbInfoPath);

                bool obtained = this.Obtain();

                Console.WriteLine("Title: {0}", this.Title);
                Console.WriteLine("Description: {0}", this.Description);
                Console.WriteLine("Company: {0}", this.Company);
                Console.WriteLine("Version: {0}", this.Version);

                if (obtained)
                {
                    Console.WriteLine("SUCCESSFULLY obtained database information!");

                    return true;
                }
            }

            Console.WriteLine("FAILED to obtain database information");

            return false;
        }

        /// <summary>
        /// Sets the database information path.
        /// </summary>
        private void SetPaths()
        {
            if (string.IsNullOrWhiteSpace(this.DbInfoPath))
            {
                this.DbInfoPath = Path.Combine(this.ProjectDir, DbInfoFile);
            }
        }

        /// <summary>
        /// Obtains the database information from the database information file.
        /// </summary>
        /// <returns>A value indicating if the database information was obtained.</returns>
        private bool Obtain()
        {
            if (this.IoUtils.FileExists(this.DbInfoPath))
            {
                using (FileStream stream = this.IoUtils.ReadFileAsStream(this.DbInfoPath))
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
        /// Sets database information attributes using the specified data.
        /// </summary>
        /// <param name="data">Data containing database information.</param>
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
        /// Extracts a database information attribute from the specified data.
        /// </summary>
        /// <param name="data">Data containing database information.</param>
        /// <returns>The value of a database information attribute.</returns>
        private string Extract(string data)
        {
            int startIndex = data.IndexOf("\"") + 1;
            int length = data.LastIndexOf("\"") - startIndex;

            return data.Substring(startIndex, length);
        }

        /// <summary>
        /// Extracts the database version number from the specified data.
        /// </summary>
        /// <param name="attribute">The name of the database information attribute.</param>
        /// <param name="data">Data containing database information.</param>
        /// <returns>The database version number.</returns>
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
