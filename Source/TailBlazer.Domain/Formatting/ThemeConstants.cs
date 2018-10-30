namespace TailBlazer.Domain.Formatting
{
    public static class ThemeConstants
    {
        #region Constants
        public const string DarkThemeAccent = "yellow";
        public const string LightThemeAccent = "indigo";
        #endregion
        #region Fields
        public static readonly string[] Themes = { "yellow", "amber", "lightgreen", "green", "lime", "teal", "cyan", "lightblue", "blue", "indigo", "orange", "deeporange", "pink", "red", "purple", "deeppurple", "gray" };
        #endregion
        #region Methods
        public static string GetAccentColor(this Theme theme) => theme == Theme.Dark ? DarkThemeAccent : LightThemeAccent;
        #endregion
    }
}