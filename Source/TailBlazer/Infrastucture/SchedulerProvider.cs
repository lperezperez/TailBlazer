namespace TailBlazer.Infrastucture
{
    using System.Reactive.Concurrency;
    using System.Windows.Threading;
    using TailBlazer.Domain.Infrastructure;
    public class SchedulerProvider : ISchedulerProvider
    {
        #region Constructors
        public SchedulerProvider(Dispatcher dispatcher) { this.MainThread = new DispatcherScheduler(dispatcher); }
        #endregion
        #region Properties
        public IScheduler Background { get; } = TaskPoolScheduler.Default;
        public IScheduler MainThread { get; }
        #endregion
    }
}