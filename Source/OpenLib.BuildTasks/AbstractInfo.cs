using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using OpenLib.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenLib.BuildTasks
{
    /// <summary>
    /// The <c>AbstractInfo</c> type provides an abstract base class containing
    /// reusable methods for creating custom MSBuild tasks that read project
    /// information attributes.
    /// </summary>
    public abstract class AbstractInfo : Task
    {
        /// <summary>
        /// Defines the abstract name of the MSBuild task.
        /// </summary>
        /// <remarks>
        /// This must be overriden in derived classes to provide the name of
        /// the MSBuild task.
        /// </remarks>
        public abstract string TaskName { get; }

        /// <summary>
        /// Defines the abstract type of the MSBuild task.
        /// </summary>
        /// <remarks>
        /// This must be overriden in derived classes to provide the type of
        /// the MSBuild task.
        /// </remarks>
        public abstract string TaskType { get; }

        /// <summary>
        /// Defines the abstract path to the information file.
        /// </summary>
        /// <remarks>
        /// This must be overriden in derived classes to provide the path to the
        /// information file.
        /// </remarks>
        public abstract string InfoFile { get; }

        /// <summary>
        /// Defines an abstract dictionary of attributes to read from the
        /// information file.
        /// </summary>
        /// <remarks>
        /// This must be overriden in derived classes to provide the attributes
        /// to read from the information file.
        /// </remarks>
        protected abstract Dictionary<string, string> Attributes { get; }

        /// <summary>
        /// Defines the first version attribute found in the information file.
        /// </summary>
        private string FirstVersionAttribute { get; set; }

        /// <summary>
        /// Gets or sets a reference to the I/O utilities.
        /// </summary>
        protected IIoUtils IoUtils { get; set; }

        /// <summary>
        /// Gets or sets the directory location of the project as a required
        /// task property.
        /// </summary>
        [Required]
        public string ProjectDir { get; set; }

        /// <summary>
        /// Gets or sets the path to the information file as a task output
        /// property.
        /// </summary>
        [Output]
        public string InfoPath { get; set; }

        /// <summary>
        /// Gets or sets the title as a task output property.
        /// </summary>
        [Output]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the description as a task output property.
        /// </summary>
        [Output]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the company as a task output property.
        /// </summary>
        [Output]
        public string Company { get; set; }

        /// <summary>
        /// Gets or sets the version as a task output property.
        /// </summary>
        [Output]
        public string Version { get; set; }

        /// <summary>
        /// Creates a new instance of the <c>AbstractInfo</c> class.
        /// </summary>
        public AbstractInfo()
        {
            this.IoUtils = new IoUtils();
        }

        /// <summary>
        /// Sets the path to the information file.
        /// </summary>
        private void SetPaths()
        {
            if (string.IsNullOrWhiteSpace(this.InfoPath))
            {
                this.InfoPath = Path.Combine(this.ProjectDir, InfoFile);
            }
        }

        /// <summary>
        /// Executes the custom task when it is invoked in a MSBuild script.
        /// </summary>
        /// <returns>A value indicating if the task completely successfully.</returns>
        public override bool Execute()
        {
            Console.WriteLine("Executing {0} MSBuild task...", this.TaskName);

            if (this.ProjectDir != null)
            {
                this.SetPaths();

                Console.WriteLine("Attempting to obtain {0} information for '{1}'", this.TaskType, this.InfoPath);

                bool obtained = this.Obtain();

                Console.WriteLine("Title: {0}", this.Title);
                Console.WriteLine("Description: {0}", this.Description);
                Console.WriteLine("Company: {0}", this.Company);
                Console.WriteLine("Version: {0}", this.Version);

                if (obtained)
                {
                    Console.WriteLine("SUCCESSFULLY obtained {0} information!", this.TaskType);

                    return true;
                }
            }

            Console.WriteLine("FAILED to obtain {0} information", this.TaskType);

            return false;
        }

        /// <summary>
        /// Obtains the information from the information file.
        /// </summary>
        /// <returns>A value indicating if the information was obtained.</returns>
        private bool Obtain()
        {
            if (this.IoUtils.FileExists(this.InfoPath))
            {
                using (FileStream stream = this.IoUtils.ReadFileAsStream(this.InfoPath))
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

                                    if (this.ContainsData(reader, ref data))
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
        /// Gets a value indicating if the data contains the correct information
        /// from the information file.
        /// </summary>
        /// <param name="reader">A reference to the <see cref="StreamReader" /> used to read the information file.</param>
        /// <param name="data">Data containing the information.</param>
        /// <remarks>
        /// This must be overriden in derived classes to provide a value
        /// indicating if the correct information is contained in the specified
        /// data.
        /// </remarks>
        /// <returns>A value indicating if the correct information is contained in the specified data.</returns>
        protected abstract bool ContainsData(StreamReader reader, ref string data);

        /// <summary>
        /// Sets information attributes using the specified data.
        /// </summary>
        /// <param name="data">Data containing the information.</param>
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
        /// Extracts an information attribute from the specified data.
        /// </summary>
        /// <param name="data">Data containing the information.</param>
        /// <returns>The value of an information attribute.</returns>
        private string Extract(string data)
        {
            int startIndex = data.IndexOf("\"") + 1;
            int length = data.LastIndexOf("\"") - startIndex;
            
            return data.Substring(startIndex, length);
        }

        /// <summary>
        /// Extracts the version number from the specified data.
        /// </summary>
        /// <param name="attribute">The name of the information attribute.</param>
        /// <param name="data">Data containing the information.</param>
        /// <returns>The version number.</returns>
        private string ExtractVersion(string attribute, string data)
        {
            if (string.IsNullOrWhiteSpace(this.FirstVersionAttribute))
                this.FirstVersionAttribute = attribute;
            
            if (!string.IsNullOrWhiteSpace(this.Version) &&
                this.FirstVersionAttribute.Equals("SemanticVersion"))
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
