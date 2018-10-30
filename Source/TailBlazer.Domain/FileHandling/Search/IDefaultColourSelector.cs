namespace TailBlazer.Domain.FileHandling.Search
{
    using TailBlazer.Domain.Formatting;
    public interface IDefaultColourSelector
    {
        #region Methods
        Hue Lookup(HueKey key);
        Hue Select(string text);
        #endregion
    }
}