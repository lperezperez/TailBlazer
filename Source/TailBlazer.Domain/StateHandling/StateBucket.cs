namespace TailBlazer.Domain.StateHandling
{
    using System;
    using TailBlazer.Domain.Settings;
    public class StateBucket
    {
        #region Constructors
        public StateBucket(string type, string id, State state, DateTime timeStamp)
        {
            this.Key = new StateBucketKey(type, id);
            this.Type = type;
            this.Id = id;
            this.State = state;
            this.TimeStamp = timeStamp;
        }
        #endregion
        #region Properties
        public string Id { get; }
        public StateBucketKey Key { get; }
        public State State { get; }
        public DateTime TimeStamp { get; }
        public string Type { get; }
        #endregion
    }
}