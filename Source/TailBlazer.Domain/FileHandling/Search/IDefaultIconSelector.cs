namespace TailBlazer.Domain.FileHandling.Search
{
    public interface IDefaultIconSelector
    {
        #region Methods
        string GetIconFor(string text, bool useRegex);
        string GetIconOrDefault(string text, bool useRegex, string iconKind);
        #endregion
    }
}