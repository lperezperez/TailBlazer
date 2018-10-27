namespace TailBlazer.Domain.FileHandling
{
    using System;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    public static class StartFromEx
    {
        #region Methods
        public static IObservable<FileNotification> ScanFrom(this IObservable<FileNotification> source, long startFrom, TimeSpan? refreshPeriod = null, IScheduler scheduler = null) => new FileRewriter(source, startFrom, refreshPeriod, scheduler).Notifications;
        public static IObservable<FileNotification> ScanFromEnd(this IObservable<FileNotification> source, TimeSpan? refreshPeriod = null, IScheduler scheduler = null) { return Observable.Create<FileNotification>(observer => { return new FileRewriter(source, refreshPeriod: refreshPeriod, scheduler: scheduler).Notifications.SubscribeSafe(observer); }).Replay(1).RefCount(); }
        #endregion
    }
}