namespace TailBlazer.Domain.Infrastructure
{
    using System.Reactive.Concurrency;
    public interface ISchedulerProvider
    {
        #region Properties
        IScheduler Background { get; }
        IScheduler MainThread { get; }
        #endregion
    }
}