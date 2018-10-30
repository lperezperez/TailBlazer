namespace TailBlazer.Domain.Formatting
{
    using System.Collections.Generic;
    using DynamicData.Kernel;
    public interface IColourProvider
    {
        #region Properties
        Hue DefaultAccent { get; }
        IEnumerable<Hue> Hues { get; }
        #endregion
        #region Methods
        Hue GetAccent(Theme theme);
        Optional<Hue> Lookup(HueKey key);
        #endregion
    }
}