namespace TailBlazer.Domain.FileHandling
{
    public interface IProgressInfo
    {
        #region Properties
        bool IsSearching { get; }
        int Segments { get; }
        int SegmentsCompleted { get; }
        #endregion
    }
}