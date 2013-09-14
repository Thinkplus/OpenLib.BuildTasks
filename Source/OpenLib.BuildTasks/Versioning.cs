using GAFRI.Common;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GAFRI.CustomBuildTasks
{
    /// <summary>
    /// The <c>Versioning</c> type provides a custom MSBuild task for
    /// dynamically versioning artifacts from its version info file.
    /// </summary>
    public class Versioning : Task
    {
        /// <summary>
        /// Defines a dictionary of supported version info files.
        /// </summary>
        private Dictionary<string, string> supportedVersionInfo = new Dictionary<string, string>
        {
            { "CS", @"Properties\AssemblyInfo.cs" },
            { "TSQL", @"Properties\DbInfo.db" },
            { "ETL", "EtlInfo.etl" },
            { "VB", @"My Project\AssemblyInfo.vb" }
        };

        /// <summary>
        /// Defines a list of values that should be contained on a line.
        /// </summary>
        private List<String> lineContains = new List<String>()
        {
            "AssemblyVersion",
            "AssemblyFileVersion",
            "AssemblyInformationalVersion",
            "DbVersion",
            "DbInformationalVersion",
            "EtlVersion",
            "EtlInformationalVersion"
        };

        /// <summary>
        /// Defines a list of values that not should be contained on a line.
        /// </summary>
        private List<string> lineNotContains = new List<string>() { "//", "'" };

        /// <summary>
        /// Defines a list of values that should have versions to exclude.
        /// </summary>
        private List<string> versionsToExclude = new List<string>() { "AssemblyFileVersion" };

        /// <summary>
        /// Defines a list of values that indicate semantic versioning.
        /// </summary>
        private List<string> semanticVersioning = new List<string>()
        {
            "AssemblyInformationalVersion",
            "DbInformationalVersion",
            "EtlInformationalVersion"
        };

        /// <summary>
        /// Defines the semantic versioning indicator.
        /// </summary>
        private const string semanticVersioningIndicator = "-d";

        /// <summary>
        /// Gets or sets a reference to the IO utilities.
        /// </summary>
        public IIOUtilities IOUtilities { get; set; }

        /// <summary>
        /// Gets or sets the directory location of the project as a required
        /// task property.
        /// </summary>
        [Required]
        public string ProjectDir { get; set; }

        /// <summary>
        /// Gets or sets the programming language to use as a required task
        /// property.
        /// </summary>
        [Required]
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the version number part as an optional task property.
        /// </summary>
        public string VersionPart { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if the version is a release
        /// version as an optional task property.
        /// </summary>
        public bool IsRelease { get; set; }

        /// <summary>
        /// Gets or sets the version to use for the release as an optional
        /// task property.
        /// </summary>
        public string ReleaseVersion { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if the version is a new development
        /// version as an optional task property.
        /// </summary>
        public bool IsNewDevelopmentVersion { get; set; }

        /// <summary>
        /// Gets or sets the new development version to use after a release
        /// version is created as an optional task property.
        /// </summary>
        public string NewDevelopmentVersion { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if the full version is semantic
        /// as a task output property.
        /// </summary>
        [Output]
        public bool IsSemanticVersion { get; set; }

        /// <summary>
        /// Gets or sets the full version number as a task output property.
        /// </summary>
        [Output]
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the next release version number as a task output
        /// property.
        /// </summary>
        [Output]
        public string NextReleaseVersion { get; set; }

        /// <summary>
        /// Gets or sets the next new development version number as a task
        /// output property.
        /// </summary>
        [Output]
        public string NextNewDevelopmentVersion { get; set; }

        /// <summary>
        /// Gets or sets the path to the version info file as a task output
        /// property.
        /// </summary>
        [Output]
        public string VersionInfoPath { get; set; }

        /// <summary>
        /// Gets or sets the path to the output file that contains the full
        /// version number as a task output property.
        /// </summary>
        [Output]
        public string OutputFilePath { get; set; }

        /// <summary>
        /// Creates a new instance of the <c>Versioning</c> class.
        /// </summary>
        public Versioning()
        {
            this.IOUtilities = new IOUtilities();
        }

        /// <summary>
        /// Executes the custom task when it is invoked in a MSBuild script.
        /// </summary>
        /// <returns>A value indicating if the task completely successfully.</returns>
        public override bool Execute()
        {
            Console.WriteLine("Executing Versioning MSBuild task...");

            if (this.ProjectDir != null)
            {
                this.SetPaths();

                Console.WriteLine("Attempting to version '{0}'", this.OutputFilePath);

                string contents = this.Apply(this.OutputFilePath, this.VersionPart);

                if (!string.IsNullOrWhiteSpace(contents))
                {
                    Boolean wasWritten = this.IOUtilities.WriteFile(this.OutputFilePath, contents);

                    if (wasWritten)
                    {
                        Console.WriteLine("SUCCESSFULLY applied version {0}!", this.Version);
                        return true;
                    }
                }
                else
                    Console.WriteLine("ERROR: Unable to locate version info file");
            }

            Console.WriteLine("FAILED to apply version");

            return false;
        }

        /// <summary>
        /// Sets the version info path for the defined programming language and
        /// output file path based on the version info path.
        /// </summary>
        private void SetPaths()
        {
            if (string.IsNullOrWhiteSpace(this.VersionInfoPath))
            {
                this.VersionInfoPath = supportedVersionInfo[this.Language];
            }

            this.OutputFilePath = Path.Combine(this.ProjectDir, this.VersionInfoPath);
        }

        /// <summary>
        /// Reads the version info file into a stream, applies the version
        /// optionally using the specified verion number part, and returns the
        /// contents of the version info file so it can be overwritten with
        /// the full version number.
        /// </summary>
        /// <param name="path">The path to the version info file.</param>
        /// <param name="versionPart">The optional version number part to replace in the version info file.</param>
        /// <returns>The updated contents of the version info file.</returns>
        private string Apply(string path, string versionPart)
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

                                    if (!data.ContainedIn(lineNotContains) && data.ContainedIn(lineContains))
                                    {
                                        this.SetVersionType(data);

                                        data = this.Format(data, versionPart);

                                        if (!data.ContainedIn(versionsToExclude))
                                        {
                                            this.Version = this.GetVersion(data);
                                        }
                                    }

                                    contents.AppendLine(data);
                                }

                                this.SetNextVersions();

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
        /// Sets the type of the version number.
        /// </summary>
        /// <param name="data">The data containing the version number.</param>
        private void SetVersionType(string data)
        {
            if (data.ContainedIn(semanticVersioning))
            {
                this.IsSemanticVersion = true;
            }
        }

        /// <summary>
        /// Formats the version number using the specified data to contain
        /// the full version number.
        /// </summary>
        /// <param name="data">The data containing the version number.</param>
        /// <param name="versionPart">The optional version number part.</param>
        /// <returns>The data containing the full version number.</returns>
        private string Format(string data, string versionPart)
        {
            string version = data;

            if (!version.ContainedIn(versionsToExclude))
            {
                if (!this.IsSemanticVersion)
                {
                    version = this.FormatVersion(data, versionPart);
                }
                else
                {
                    if (this.IsRelease)
                        version = this.FormatSemanticReleaseVersion(data);
                    else
                        version = this.FormatSemanticVersion(data);
                }
            }

            if (this.IsNewDevelopmentVersion)
            {
                version = this.ReplaceVersion(data, this.NewDevelopmentVersion);
            }

            return version;
        }

        /// <summary>
        /// Formats the version number using the specified data and optional
        /// version number part to contain the full version number.
        /// </summary>
        /// <param name="data">The data containing the version number.</param>
        /// <param name="versionPart">The optional version number part.</param>
        /// <returns>The data containing the full version number.</returns>
        private string FormatVersion(string data, string versionPart)
        {
            int versionPartCount = data.ToCharArray().Count(c => c.Equals('.'));

            switch (versionPartCount)
            {
                case 3:
                    return data.Replace("*", versionPart);

                case 2:
                    return data.Replace("*", string.Format("{0}.0", versionPart));

                case 1:
                    return data.Replace("*", string.Format("{0}.0.0", versionPart));

                default:
                    return data.Replace("*", string.Format("{0}.0.0.0", versionPart));
            }
        }

        /// <summary>
        /// Formats the semantic version number using the specified data to
        /// contain the full version number.
        /// </summary>
        /// <param name="data">The data containing the version number.</param>
        /// <returns>The data containing the full version number.</returns>
        private string FormatSemanticVersion(string data)
        {
            string version = data.Replace("*", "0");

            if (version.Contains(semanticVersioningIndicator))
            {
                int index = version.IndexOf(semanticVersioningIndicator) + semanticVersioningIndicator.Length;

                version = version.Insert(index, DateTime.Now.ToString("yyyyMMddHHmm"));
            }

            return version;
        }

        /// <summary>
        /// Formats the semantic release version number using the specified data
        /// to contain the full version number.
        /// </summary>
        /// <param name="data">The data containing the version number.</param>
        /// <returns>The data containing the full version number.</returns>
        private string FormatSemanticReleaseVersion(string data)
        {
            string version = data.Replace("*", "0");

            if (version.Contains(semanticVersioningIndicator))
            {
                if (!string.IsNullOrWhiteSpace(this.ReleaseVersion))
                {
                    version = version.Replace(this.GetVersion(version), this.ReleaseVersion).Replace(semanticVersioningIndicator, "");
                }
                else
                    version = version.Replace(semanticVersioningIndicator, "");
            }

            return version;
        }

        /// <summary>
        /// Replaces the version number in the specified data with the supplied
        /// new version number.
        /// </summary>
        /// <param name="data">The data containing the version number.</param>
        /// <param name="newVersion">The new version number to replace with.</param>
        /// <returns>The data containing the new version number.</returns>
        private string ReplaceVersion(string data, string newVersion)
        {
            int startIndex = data.IndexOf("\"") + 1;
            int endIndex = data.LastIndexOf("\"");

            string value = data.Substring(startIndex, endIndex - startIndex);
            string version = data;

            if (version.ContainedIn(semanticVersioning))
            {
                if (newVersion.Contains(semanticVersioningIndicator))
                    version = version.Replace(value, newVersion);
                else
                    version = version.Replace(value, string.Concat(newVersion, semanticVersioningIndicator));
            }
            else
            {
                version = version.Replace(value, string.Concat(newVersion.Replace(semanticVersioningIndicator, ""), ".0"));
            }

            return version;
        }

        /// <summary>
        /// Gets the full version number from the specified data.
        /// </summary>
        /// <param name="data">The data containing the version number.</param>
        /// <returns>The full version number.</returns>
        private string GetVersion(string data)
        {
            int startIndex = data.IndexOf("\"") + 1;
            int length = data.LastIndexOf("\"") - startIndex;

            return data.Substring(startIndex, length);
        }

        /// <summary>
        /// Sets the next release and new development versions using the full
        /// version number.
        /// </summary>
        /// <remarks>
        /// By default, the minor version number part is incremeneted by one
        /// for the next new development version number.
        /// </remarks>
        private void SetNextVersions()
        {
            string version = this.Version;

            if (version.Contains(semanticVersioningIndicator))
                version = version.Substring(0, version.IndexOf("-"));

            string[] versionParts = version.Split('.');

            if (versionParts.Length == 3)
            {
                versionParts[1] = (versionParts[1].ToInt32Current() + 1).ToString();
                versionParts[2] = "0";
            }

            this.NextReleaseVersion = version;
            this.NextNewDevelopmentVersion = string.Concat(versionParts.Join("."), semanticVersioningIndicator);
        }
    }
}
