namespace TailBlazer.Domain.Formatting
{
    using System;
    public interface IThemeProvider
    {
        #region Properties
        IObservable<Hue> Accent { get; }
        IObservable<Theme> Theme { get; }
        #endregion
    }
}