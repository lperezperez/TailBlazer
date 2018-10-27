namespace TailBlazer.Domain.FileHandling
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using DynamicData.Kernel;
    using TailBlazer.Domain.Annotations;
    internal class FileRewriter
    {
        #region Constructors
        public FileRewriter([NotNull] IObservable<FileNotification> fileWatcher, long startFrom = -1, TimeSpan? refreshPeriod = null, IScheduler scheduler = null)
        {
            if (fileWatcher == null) throw new ArgumentNullException(nameof(fileWatcher));
            var locker = new object();

            //TODO: WHEN ORIGINAL FILE HAS ROLLED OVER, WE NEED TO CLEAR OUT THE LOCAL TEMP FILE
            this.Notifications = Observable.Create<FileNotification>
                (
                 observer =>
                     {
                         var newFile = Path.GetTempFileName();
                         Console.WriteLine("created {0}", newFile);
                         var info = new FileInfo(newFile);
                         var origStream = fileWatcher.Synchronize(locker).Publish();
                         var newStream = info.WatchFile(refreshPeriod, scheduler).Synchronize(locker);

                         //Watch the new file
                         var resultStream = newStream.CombineLatest(origStream, (NewStream, OldStream) => { return !OldStream.Exists ? OldStream : NewStream; }).SubscribeSafe(observer);

                         //   var resultStream = newStream.SubscribeSafe(observer);

                         //Create a new file from the old one, starting at the spcified index
                         var fileWriter = origStream.TakeWhile(notification => notification.Exists).Repeat().Scan
                             (
                              new FileReadResult(Enumerable.Empty<string>(), startFrom),
                              (state, notification) =>
                                  {
                                      //read lines from the source file.
                                      return this.ReadLines(notification.FullName, state.EndPosition);
                                  }).Subscribe
                             (
                              result =>
                                  {
                                      //Write lines to the file
                                      this.WriteLines(newFile, result.Lines.AsArray());
                                  });
                         var connected = origStream.Connect();
                         return Disposable.Create
                             (
                              () =>
                                  {
                                      Console.WriteLine("deleting {0}", newFile);
                                      connected.Dispose();
                                      fileWriter.Dispose();
                                      resultStream.Dispose();
                                      File.Delete(newFile);
                                  });
                     });
        }
        #endregion
        #region Properties
        public IObservable<FileNotification> Notifications { get; }
        #endregion
        #region Methods
        private FileReadResult ReadLines(string file, long firstPosition)
        {
            var list = new List<string>();
            long endPosition;
            using (var stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {
                using (var reader = new StreamReaderExtended(stream))
                {
                    //go to starting point
                    if (firstPosition == -1)
                        stream.Seek(0, SeekOrigin.End);
                    else
                        stream.Seek(firstPosition, SeekOrigin.Begin);
                    string line;
                    while ((line = reader.ReadLine()) != null)
                        list.Add(line);
                    endPosition = reader.AbsolutePosition();
                }
            }
            return new FileReadResult(list, endPosition);
        }
        private void WriteLines(string file, string[] lines)
        {
            if (!lines.Any()) return;
            using (var stream = File.Open(file, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Delete | FileShare.ReadWrite))
            {
                using (var sw = new StreamWriter(stream))
                {
                    sw.BaseStream.Seek(0, SeekOrigin.End);
                    lines.ForEach(line => { sw.WriteLine(line); });
                }
            }
        }
        #endregion
        #region Classes
        private class FileReadResult
        {
            #region Constructors
            public FileReadResult(IEnumerable<string> lines, long endPosition)
            {
                this.Lines = lines;
                this.EndPosition = endPosition;
            }
            #endregion
            #region Properties
            public long EndPosition { get; }
            public IEnumerable<string> Lines { get; }
            #endregion
        }
        #endregion
    }
}