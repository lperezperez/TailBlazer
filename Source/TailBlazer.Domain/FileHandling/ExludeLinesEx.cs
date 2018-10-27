namespace TailBlazer.Domain.FileHandling
{
    using System;
    using System.Reactive.Linq;
    using TailBlazer.Domain.Annotations;
    public static class ExludeLinesEx
    {
        #region Methods
        public static IObservable<ILineProvider> Exclude([NotNull] this IObservable<ILineProvider> source, [NotNull] Func<string, bool> exlusionFilter)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (exlusionFilter == null) throw new ArgumentNullException(nameof(exlusionFilter));
            return source.Select(lineProvider => new ExludedLinesProvider(lineProvider, exlusionFilter));
        }
        #endregion
    }
}