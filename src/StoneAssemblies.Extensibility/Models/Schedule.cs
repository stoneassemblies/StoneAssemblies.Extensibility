namespace StoneAssemblies.Extensibility
{
    using System.Collections.Generic;

    public sealed class Schedule : ISchedule
    {
        /// <summary>
        /// Gets the install.
        /// </summary>
        public List<string> Install { get; } = new List<string>();

        /// <summary>
        /// Gets the un install.
        /// </summary>
        public List<string> UnInstall { get; } = new List<string>();

        /// <summary>
        /// The install.
        /// </summary>
        IReadOnlyCollection<string> ISchedule.Install => Install?.AsReadOnly();

        /// <summary>
        /// The un install.
        /// </summary>
        IReadOnlyCollection<string> ISchedule.UnInstall => UnInstall?.AsReadOnly();
    }
}