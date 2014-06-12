using OpenLib.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenLib.BuildTasks
{
    /// <summary>
    /// The <c>CobolInfo</c> type provides a custom MSBuild task for reading
    /// COBOL assembly information attributes for a COBOL project.
    /// </summary>
    /// <remarks>
    /// This class inherits from the <c>AbstractInfo</c> abstract base class.
    /// </remarks>
    public class CobolInfo : AbstractInfo
    {
        //---------------------------------------------------------------------
        // Fields
        //---------------------------------------------------------------------

        /// <summary>
        /// Defines a dictionary of attributes to read from the COBOL assembly
        /// information file.
        /// </summary>
        private Dictionary<string, string> attributes;

        /// <summary>
        /// Defines a list of values that should be contained on a line.
        /// </summary>
        private readonly List<string> LineContains = new List<string>() { "CUSTOM-ATTRIBUTE" };

        //---------------------------------------------------------------------
        // Abstract Implementation Properties
        //---------------------------------------------------------------------

        /// <inheritdoc/>
        public override string TaskName { get { return this.GetType().Name; } }

        /// <inheritdoc/>
        public override string TaskType { get { return "COBOL assembly"; } }

        /// <inheritdoc/>
        public override string InfoFile { get { return @"Properties\AssemblyInfo.cob"; } }

        /// <inheritdoc/>
        protected override Dictionary<string, string> Attributes
        {
            get
            {
                if (attributes == null)
                    attributes = new Dictionary<string,string>()
                    {
                        { "Title" , "CA-ASSEMBLYTITLE" },
                        { "Description", "CA-ASSEMBLYDESCRIPTION" },
                        { "Company", "CA-ASSEMBLYCOMPANY" },
                        { "Version", "CA-ASSEMBLYVERSION" },
                        { "SemanticVersion", "CA-ASSEMBLYINFORMATIONALVERSION" }
                    };

                return attributes; 
            }
        }

        //---------------------------------------------------------------------
        // Constructors
        //---------------------------------------------------------------------

        /// <summary>
        /// Creates a new instance of the <c>CobolInfo</c> class.
        /// </summary>
        public CobolInfo() : base() { }

        //---------------------------------------------------------------------
        // Abstract Implementation Methods
        //---------------------------------------------------------------------

        /// <inheritdoc/>
        protected override bool ContainsData(StreamReader reader, ref string data)
        {
            if (data.ContainedIn(LineContains) &&
                data.ContainedIn(this.Attributes.Select(a => a.Value).ToList()))
            {
                data += reader.ReadLine(); 
                return true;
            }

            return false;
        }
    }
}
