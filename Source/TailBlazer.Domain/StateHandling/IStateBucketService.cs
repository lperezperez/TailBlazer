namespace TailBlazer.Domain.StateHandling
{
    using DynamicData.Kernel;
    using TailBlazer.Domain.Settings;
    /// <summary>A simple means for dumping stuff to a file</summary>
    public interface IStateBucketService
    {
        #region Methods
        Optional<State> Lookup(string type, string id);
        void Write(string type, string id, State state);
        #endregion
    }
}