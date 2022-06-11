namespace StoneAssemblies.Extensibility
{
    using System.Collections.Generic;

    public interface ISchedule
    {
        IReadOnlyCollection<string> Install { get; }

        IReadOnlyCollection<string> UnInstall { get; }
    }
}