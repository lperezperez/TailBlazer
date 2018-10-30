namespace TailBlazer.Infrastucture.KeyboardNavigation
{
    using System;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using DynamicData.Kernel;
    public class KeyboardNavigationHandler : IDependencyObjectReceiver, IKeyboardNavigationHandler
    {
        #region Fields
        private readonly IDisposable _cleanUp;
        private readonly ISubject<KeyboardNavigationType> _keyStream = new Subject<KeyboardNavigationType>();
        private readonly SerialDisposable _keySubscriber = new SerialDisposable();
        #endregion
        #region Constructors
        public KeyboardNavigationHandler()
        {
            this._cleanUp = Disposable.Create
                (
                 () =>
                     {
                         this._keySubscriber.Dispose();
                         this._keyStream.OnCompleted();
                     });
        }
        #endregion
        #region Properties
        public IObservable<KeyboardNavigationType> NavigationKeys => this._keyStream.AsObservable();
        #endregion
        #region Methods
        public void Dispose() { this._cleanUp.Dispose(); }
        private void Control_IsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //   Console.WriteLine(e.NewValue);
            // var focused = (Boolean) e.NewValue;
            //if (!focused)
            //{
            //    IInputElement focusedControl = Keyboard.FocusedElement;
            //    Console.WriteLine(focusedControl.GetType());

            //  //  ((Control) focusedControl).Background =new SolidColorBrush(Colors.PaleVioletRed);;
            //}
        }
        private Optional<KeyboardNavigationType> Map(KeyEventArgs keyEventArgs)
        {
            switch (keyEventArgs.Key)
            {
                case Key.PageDown:
                    return KeyboardNavigationType.PageDown;
                case Key.PageUp:
                    return KeyboardNavigationType.PageUp;
                case Key.Up:
                    return KeyboardNavigationType.Up;
                case Key.Down:
                    return KeyboardNavigationType.Down;
                case Key.Home:
                    return KeyboardNavigationType.Home;
                case Key.End:
                    return KeyboardNavigationType.End;
                default:
                    return Optional<KeyboardNavigationType>.None;
            }
        }
        void IDependencyObjectReceiver.Receive(DependencyObject value)
        {
            var control = (Control)value;
            control.IsKeyboardFocusedChanged += this.Control_IsKeyboardFocusedChanged;
            this._keySubscriber.Disposable = Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(h => control.PreviewKeyDown += h, h => control.PreviewKeyDown -= h).Select(e => this.Map(e.EventArgs)).Where(e => e.HasValue).Select(e => e.Value).SubscribeSafe(this._keyStream);
        }
        #endregion
    }
}