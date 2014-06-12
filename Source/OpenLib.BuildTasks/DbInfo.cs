using OpenLib.Extensions;
using System.Collections.Generic;
using System.IO;

namespace OpenLib.BuildTasks
{
    /// <summary>
    /// The <c>DbInfo</c> type provides a custom MSBuild task for reading
    /// database information attributes for a database project.
    /// </summary>
    /// <remarks>
    /// This class inherits from the <c>AbstractInfo</c> abstract base class.
    /// </remarks>
    public class DbInfo : AbstractInfo
    {
        //---------------------------------------------------------------------
        // Fields
        //---------------------------------------------------------------------

        /// <summary>
        /// Defines a dictionary of attributes to read from the database
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
        public override string TaskType { get { return "Database"; } }

        /// <inheritdoc/>
        public override string InfoFile { get { return @"Properties\DbInfo.db"; } }

        /// <inheritdoc/>
        protected override Dictionary<string, string> Attributes
        {
            get
            {
                if (attributes == null)
                    attributes = new Dictionary<string, string>()
                    {
                        { "Title" , "DbTitle" },
                        { "Description", "DbDescription" },
                        { "Company", "DbCompany" },
                        { "Version", "DbVersion" },
                        { "SemanticVersion", "DbInformationalVersion" }
                    };

                return attributes;
            }
        }

        //---------------------------------------------------------------------
        // Constructors
        //---------------------------------------------------------------------

        /// <summary>
        /// Creates a new instance of the <c>DbInfo</c> class.
        /// </summary>
        public DbInfo() : base() { }

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
