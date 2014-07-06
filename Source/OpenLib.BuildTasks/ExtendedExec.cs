using Microsoft.Build.Framework;
using Microsoft.Build.Tasks;
using System.Text;

namespace OpenLib.BuildTasks
{
    /// <summary>
    /// The <c>ExtendedExec</c> type provides a custom MSBuild task that extends
    /// the standard MSBuild <c>Exec</c> task by executing a shell command and
    /// provides access to the standard and error output of the command.
    /// </summary>
    /// <remarks>
    /// This class inherits from the <c>Exec</c> MSBuild task class and uses
    /// that class as a base class in which to extend its behavior.
    /// </remarks>
    public class ExtendedExec : Exec
    {
        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        /// <summary>
        /// Gets the standard and/or error output of the command executed by
        /// the task.
        /// </summary>
        [Output]
        public string Output { get; internal set; }

        /// <summary>
        /// Gets or sets the text string to search for in the standard and
        /// error output by which task execution will fail if the text string
        /// is found in the output.
        /// </summary>
        public string FailExecIfOutputContainsText { get; set; }

        /// <summary>
        /// Gets or sets the text string to search for in the standard and
        /// error output by which task execution will fail if the text string
        /// is not found in the output.
        /// </summary>
        public string FailExecIfOutputDoesNotContainText { get; set; }

        //---------------------------------------------------------------------
        // Constructors
        //---------------------------------------------------------------------

        /// <summary>
        /// Creates a new instance of the <c>ExtendedExec</c> class.
        /// </summary>
        public ExtendedExec()
        {
            // enable console output access
            base.ConsoleToMSBuild = true;
        }

        //---------------------------------------------------------------------
        // Abstract Implementation Methods
        //---------------------------------------------------------------------

        /// <summary>
        /// Executes the custom task when it is invoked in a MSBuild script.
        /// </summary>
        /// <returns>A value indicating if the task completely successfully.</returns>
        public override bool Execute()
        {
            // execute base MSBuild Exec task
            bool returnValue = base.Execute();

            // extract output
            this.ExtractOutput();

            // fail execution if text is found in output string
            if (!string.IsNullOrWhiteSpace(this.FailExecIfOutputContainsText))
                return this.ContainsOutputText();

            // fail execution if text is not found in output string
            if (!string.IsNullOrWhiteSpace(this.FailExecIfOutputDoesNotContainText))
                return this.DoesNotContainOutputText();

            return returnValue;
        }

        //---------------------------------------------------------------------
        // Other Methods
        //---------------------------------------------------------------------

        /// <summary>
        /// Extracts the standard and/or error output into a single output text
        /// string.
        /// </summary>
        private void ExtractOutput()
        {
            if (base.ConsoleOutput != null && base.ConsoleOutput.Length > 0)
            {
                StringBuilder output = new StringBuilder();

                foreach (ITaskItem i in base.ConsoleOutput)
                    output.Append(i.ToString() + "\n");

                this.Output = output.ToString();
            }
        }

        /// <summary>
        /// Gets a value indicating if the text string set for
        /// <c>FailExecIfOutputContainsText</c> is found in the output string.
        /// </summary>
        /// <returns>A value indicating if the text string was found in the
        /// output string.</returns>
        private bool ContainsOutputText()
        {
            if (!string.IsNullOrWhiteSpace(this.Output))
            {
                return !this.Output.Contains(this.FailExecIfOutputContainsText);
            }

            return false;
        }

        /// <summary>
        /// Gets a value indicating if the text string set for
        /// <c>FailExecIfOutputDoesNotContainText</c> is not found in the
        /// output string.
        /// </summary>
        /// <returns>A value indicating if the text string was not found in
        /// the output string.</returns>
        private bool DoesNotContainOutputText()
        {
            if (!string.IsNullOrWhiteSpace(this.Output))
            {
                return this.Output.Contains(this.FailExecIfOutputDoesNotContainText);
            }

            return false;
        }
    }
}
