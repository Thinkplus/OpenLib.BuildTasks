
namespace OpenLib.BuildTasks
{
    /// <summary>
    /// The <c>IProjectInfo</c> type provides an interface for defining
    /// reusable methods for creating custom MSBuild tasks that read
    /// project information attributes.
    /// </summary>
    public interface IProjectInfo
    {
        /// <summary>
        /// Gets or sets the project information title.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Gets or sets the project information description.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Gets or sets the project information company.
        /// </summary>
        string Company { get; set; }

        /// <summary>
        /// Gets or sets the project information version.
        /// </summary>
        string Version { get; set; }

        /// <summary>
        /// Executes the custom task.
        /// </summary>
        /// <returns>A value indicating if the task completely successfully.</returns>
        bool Execute();
    }
}
