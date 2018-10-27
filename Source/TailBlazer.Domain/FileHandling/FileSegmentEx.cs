namespace TailBlazer.Domain.FileHandling
{
    using System;
    using System.Reactive.Linq;
    public static class FileSegmentEx
    {
        #region Methods
        public static IObservable<FileSegmentCollection> WithSegments(this IObservable<FileNotification> source, int initialTail = 100000)
        {
            var shared = source.Replay(1).RefCount();
            return Observable.Create<FileSegmentCollection>
                (
                 observer =>
                     {
                         var filtered = source.Where(f => f.Exists);
                         var segmenter = new FileSegmenter(filtered, initialTail);
                         return segmenter.Segments.SubscribeSafe(observer);
                     }).TakeUntil(shared.Where(f => !f.Exists));
        }
        #endregion
    }
}