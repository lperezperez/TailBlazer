namespace TailBlazer.Views.Formatting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MaterialDesignThemes.Wpf;
    using TailBlazer.Domain.FileHandling.Search;
    public sealed class DefaultIconSelector : IDefaultIconSelector
    {
        #region Constructors
        public DefaultIconSelector() { this.DefaultMatches = this.LoadIcons().ToArray(); }
        #endregion
        #region Properties
        private IEnumerable<DefaultIcons> DefaultMatches { get; }
        private string RegEx { get; } = PackIconKind.Regex.ToString();
        private string Search { get; } = PackIconKind.Magnify.ToString();
        #endregion
        #region Methods
        public string GetIconFor(string text, bool useRegex)
        {
            var match = this.DefaultMatches.FirstOrDefault(icon => icon.MatchTextOnCase ? icon.Text.Equals(text) : icon.Text.Equals(text, StringComparison.OrdinalIgnoreCase));
            return match != null ? match.IconName : useRegex ? this.RegEx : this.Search;
        }
        public string GetIconOrDefault(string text, bool useRegex, string iconKind)
        {
            var existing = this.DefaultMatches.FirstOrDefault(icon => icon.IconName.Equals(iconKind, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
                return existing.IconName;
            return this.GetIconFor(text, useRegex);
        }
        private IEnumerable<DefaultIcons> LoadIcons()
        {
            yield return new DefaultIcons("DEBUG", PackIconKind.Bug.ToString());
            yield return new DefaultIcons("INFO", PackIconKind.InformationOutline.ToString());
            yield return new DefaultIcons("WARN", PackIconKind.AlertOutline.ToString());
            yield return new DefaultIcons("WARNING", PackIconKind.AlertOutline.ToString());
            yield return new DefaultIcons("ERROR", PackIconKind.SquareInc.ToString());
            yield return new DefaultIcons("FATAL", PackIconKind.ExitToApp.ToString());
            yield return new DefaultIcons("BANK", PackIconKind.Bank.ToString());
            yield return new DefaultIcons("PERSON", PackIconKind.Account.ToString());
            yield return new DefaultIcons("PEOPLE", PackIconKind.AccountMultiple.ToString());
            yield return new DefaultIcons("USD", PackIconKind.CurrencyUsd.ToString());
            yield return new DefaultIcons("GBP", PackIconKind.CurrencyGbp.ToString());
            yield return new DefaultIcons("EUR", PackIconKind.CurrencyEur.ToString());
            yield return new DefaultIcons("FUCK", PackIconKind.EmoticonDevil.ToString());
            yield return new DefaultIcons("SHIT", PackIconKind.EmoticonPoop.ToString());
            yield return new DefaultIcons("POOP", PackIconKind.EmoticonPoop.ToString());
            yield return new DefaultIcons("PISS", PackIconKind.EmoticonDevil.ToString());
            yield return new DefaultIcons("WANK", PackIconKind.EmoticonDevil.ToString());
        }
        #endregion
        #region Classes
        private class DefaultIcons
        {
            #region Constructors
            public DefaultIcons(string text, string iconName, bool matchTextOnCase = false)
            {
                this.Text = text;
                this.IconName = iconName;
                this.MatchTextOnCase = matchTextOnCase;
            }
            #endregion
            #region Properties
            public string IconName { get; }
            public bool MatchTextOnCase { get; }
            public string Text { get; }
            #endregion
        }
        #endregion
    }
}