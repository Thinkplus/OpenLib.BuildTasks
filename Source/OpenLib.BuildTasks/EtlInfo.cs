using OpenLib.Extensions;
using System.Collections.Generic;
using System.IO;

namespace OpenLib.BuildTasks
{
    /// <summary>
    /// The <c>EtlInfo</c> type provides a custom MSBuild task for reading
    /// ETL information attributes for an ETL project.
    /// </summary>
    /// <remarks>
    /// This class inherits from the <c>AbstractInfo</c> abstract base class.
    /// </remarks>
    public class EtlInfo : AbstractInfo
    {
        /// <summary>
        /// Defines the name of the MSBuild task.
        /// </summary>
        public override string TaskName { get { return this.GetType().Name; } }

        /// <summary>
        /// Defines the type of the MSBuild task.
        /// </summary>
        public override string TaskType { get { return "ETL"; } }

        /// <summary>
        /// Defines the path to the ETL information file.
        /// </summary>
        public override string InfoFile { get { return "EtlInfo.etl"; } }

        /// <summary>
        /// Defines a dictionary of attributes to read from the ETL
        /// information file.
        /// </summary>
        private Dictionary<string, string> attributes;

        /// <summary>
        /// Gets a dictionary of attributes to read from the ETL
        /// information file.
        /// </summary>
        protected override Dictionary<string, string> Attributes
        {
            get
            {
                if (attributes == null)
                    attributes = new Dictionary<string, string>()
                    {
                        { "Title" , "EtlTitle" },
                        { "Description", "EtlDescription" },
                        { "Company", "EtlCompany" },
                        { "Version", "EtlVersion" },
                        { "SemanticVersion", "EtlInformationalVersion" }
                    };

                return attributes;
            }
        }

        /// <summary>
        /// Defines a list of values that not should be contained on a line.
        /// </summary>
        private readonly List<string> LineNotContains = new List<string>() { "//", "--" };

        /// <summary>
        /// Creates a new instance of the <c>EtlInfo</c> class.
        /// </summary>
        public EtlInfo() : base() { }

        /// <summary>
        /// Gets a value indicating if the data contains the correct information
        /// from the information file.
        /// </summary>
        /// <param name="reader">A reference to the <see cref="StreamReader" /> used to read the information file.</param>
        /// <param name="data">Data containing the information.</param>
        /// <returns>A value indicating if the correct information is contained in the specified data.</returns>
        protected override bool ContainsData(StreamReader reader, ref string data)
        {
            return !data.ContainedIn(LineNotContains);
        }
    }
}
