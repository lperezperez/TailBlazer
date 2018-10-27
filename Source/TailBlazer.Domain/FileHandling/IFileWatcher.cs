namespace TailBlazer.Domain.FileHandling
{
    using System;
    public interface IFileWatcher
    {
        #region Properties
        string Folder { get; }
        string FullName { get; }
        IObservable<FileNotification> Latest { get; }
        string Name { get; }
        IObservable<FileStatus> Status { get; }
        #endregion
        #region Methods
        void Clear();
        void Reset();
        void ScanFrom(long scanFrom);
        #endregion
    }
}