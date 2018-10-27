namespace TailBlazer.Views.WindowManagement
{
    using System;
    using DynamicData;
    using TailBlazer.Infrastucture;
    public interface IWindowsController
    {
        #region Properties
        IObservableCache<HeaderedView, Guid> Views { get; }
        #endregion
        #region Methods
        void Register(HeaderedView item);
        void Remove(HeaderedView item);
        #endregion
    }
}