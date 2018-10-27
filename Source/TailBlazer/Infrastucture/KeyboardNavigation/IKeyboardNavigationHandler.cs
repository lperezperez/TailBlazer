namespace TailBlazer.Infrastucture.KeyboardNavigation
{
    using System;
    using System.Reactive.Linq;
    using TailBlazer.Domain.FileHandling;
    public interface IKeyboardNavigationHandler : IDisposable
    {
        #region Properties
        IObservable<KeyboardNavigationType> NavigationKeys { get; }
        #endregion
    }
    public static class KeyboardNavigationHandlerEx
    {
        #region Methods
        public static IObservable<ScrollRequest> ToScrollRequest(this IObservable<KeyboardNavigationType> source, IPageProvider pageProvider)
        {
            return source.Select
                (
                 keys =>
                     {
                         var size = pageProvider.PageSize;
                         var firstIndex = pageProvider.FirstIndex;
                         switch (keys)
                         {
                             case KeyboardNavigationType.Up:
                                 return new ScrollRequest(ScrollReason.User, size, firstIndex - 1);
                             case KeyboardNavigationType.Down:
                                 return new ScrollRequest(ScrollReason.User, size, firstIndex + 1);
                             case KeyboardNavigationType.PageUp:
                                 return new ScrollRequest(ScrollReason.User, size, firstIndex - size);
                             case KeyboardNavigationType.PageDown:
                                 return new ScrollRequest(ScrollReason.User, size, firstIndex + size);
                             case KeyboardNavigationType.Home:
                                 return new ScrollRequest(ScrollReason.User, size, 0);
                             case KeyboardNavigationType.End:
                                 return new ScrollRequest(size);
                             default:
                                 throw new ArgumentOutOfRangeException(nameof(keys), keys, null);
                         }
                     });
        }
        #endregion
    }
}