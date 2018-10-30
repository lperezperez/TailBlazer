namespace TailBlazer.Domain.Ratings
{
    using System;
    using TailBlazer.Domain.Settings;
    public interface IRatingService
    {
        #region Properties
        IObservable<RatingsMetaData> Metrics { get; }
        #endregion
    }
}