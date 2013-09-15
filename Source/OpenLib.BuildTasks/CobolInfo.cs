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
        /// <summary>
        /// Defines the name of the MSBuild task.
        /// </summary>
        public override string TaskName { get { return this.GetType().Name; } }

        /// <summary>
        /// Defines the type of the MSBuild task.
        /// </summary>
        public override string TaskType { get { return "COBOL assembly"; } }

        /// <summary>
        /// Defines the path to the COBOL assembly information file.
        /// </summary>
        public override string InfoFile { get { return @"Properties\AssemblyInfo.cob"; } }

        /// <summary>
        /// Defines a dictionary of attributes to read from the COBOL assembly
        /// information file.
        /// </summary>
        private Dictionary<string, string> attributes;

        /// <summary>
        /// Gets a dictionary of attributes to read from the COBOL assembly
        /// information file.
        /// </summary>
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

        /// <summary>
        /// Defines a list of values that should be contained on a line.
        /// </summary>
        private readonly List<string> LineContains = new List<string>() { "CUSTOM-ATTRIBUTE" };

        /// <summary>
        /// Creates a new instance of the <c>CobolInfo</c> class.
        /// </summary>
        public CobolInfo() : base() { }

        /// <summary>
        /// Gets a value indicating if the data contains the correct information
        /// from the information file.
        /// </summary>
        /// <param name="reader">A reference to the <see cref="StreamReader" /> used to read the information file.</param>
        /// <param name="data">Data containing the information.</param>
        /// <returns>A value indicating if the correct information is contained in the specified data.</returns>
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
