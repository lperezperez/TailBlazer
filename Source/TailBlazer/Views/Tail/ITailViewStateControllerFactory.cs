namespace TailBlazer.Views.Tail
{
    using System;
    using TailBlazer.Domain.Annotations;
    public interface ITailViewStateControllerFactory
    {
        #region Methods
        IDisposable Create([NotNull] TailViewModel tailView, bool loadDefaults);
        #endregion
    }
}