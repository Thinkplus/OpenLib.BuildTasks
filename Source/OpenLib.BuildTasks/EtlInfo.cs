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
        //---------------------------------------------------------------------
        // Fields
        //---------------------------------------------------------------------

        /// <summary>
        /// Defines a dictionary of attributes to read from the ETL
        /// information file.
        /// </summary>
        private Dictionary<string, string> attributes;

        /// <summary>
        /// Defines a list of values that not should be contained on a line.
        /// </summary>
        private readonly List<string> LineNotContains = new List<string>() { "//", "--" };

        //---------------------------------------------------------------------
        // Abstract Implementation Properties
        //---------------------------------------------------------------------

        /// <inheritdoc/>
        public override string TaskName { get { return this.GetType().Name; } }

        /// <inheritdoc/>
        public override string TaskType { get { return "ETL"; } }

        /// <inheritdoc/>
        public override string InfoFile { get { return "EtlInfo.etl"; } }

        /// <inheritdoc/>
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

        //---------------------------------------------------------------------
        // Constructors
        //---------------------------------------------------------------------

        /// <summary>
        /// Creates a new instance of the <c>EtlInfo</c> class.
        /// </summary>
        public EtlInfo() : base() { }

        //---------------------------------------------------------------------
        // Abstract Implementation Methods
        //---------------------------------------------------------------------

        /// <inheritdoc/>
        protected override bool ContainsData(StreamReader reader, ref string data)
        {
            return !data.ContainedIn(LineNotContains);
        }
    }
}
