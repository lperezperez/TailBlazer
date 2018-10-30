namespace TailBlazer.Views.FileDrop
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Media;
    using TailBlazer.Domain.Infrastructure;
    using TailBlazer.Infrastucture;
    public class FileDropMonitor : IDependencyObjectReceiver, IDisposable
    {
        #region Fields
        private readonly SerialDisposable _cleanUp = new SerialDisposable();
        private readonly ISubject<FileInfo> _fileDropped = new Subject<FileInfo>();
        private bool _isLoaded;
        #endregion
        #region Properties
        public IObservable<FileInfo> Dropped => this._fileDropped;
        #endregion
        #region Methods
        public void Dispose() { this._cleanUp.Dispose(); }
        public void Receive(DependencyObject value)
        {
            if (this._isLoaded || null == value)
                return;
            this._isLoaded = true;
            if (!(value is UIElement control))
                return;
            control.AllowDrop = true;
            var window = Window.GetWindow(value);
            DragAdorner adorner = null;
            var createAdorner = Observable.FromEventPattern<DragEventHandler, DragEventArgs>(h => control.PreviewDragEnter += h, h => control.PreviewDragEnter -= h).Select(ev => ev.EventArgs).Subscribe
                (
                 e =>
                     {
                         var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                         var container = new FileDropContainer(files);
                         var contentPresenter = new ContentPresenter { Content = container };
                         adorner = new DragAdorner(control, contentPresenter);
                     });
            var clearAdorner = Observable.FromEventPattern<DragEventHandler, DragEventArgs>(h => control.PreviewDragLeave += h, h => control.PreviewDragLeave -= h).ToUnit().Merge(Observable.FromEventPattern<DragEventHandler, DragEventArgs>(h => control.PreviewDrop += h, h => control.PreviewDrop -= h).ToUnit()).Subscribe
                (
                 e =>
                     {
                         if (adorner == null) return;
                         adorner.Detatch();
                         adorner = null;
                     });
            var updatePositionOfAdornment = Observable.FromEventPattern<DragEventHandler, DragEventArgs>(h => control.PreviewDragOver += h, h => control.PreviewDragOver -= h).Select(ev => ev.EventArgs).Where(_ => adorner != null).Subscribe(e => adorner.MousePosition = e.GetPosition(window));
            var dropped = Observable.FromEventPattern<DragEventHandler, DragEventArgs>(h => control.Drop += h, h => control.Drop -= h).Select(ev => ev.EventArgs).SelectMany
                (
                 e =>
                     {
                         if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                             return Enumerable.Empty<FileInfo>();

                         // Note that you can have more than one file.
                         var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                         return files.Select(f => new FileInfo(f));
                     }).SubscribeSafe(this._fileDropped);
            this._cleanUp.Disposable = Disposable.Create
                (
                 () =>
                     {
                         updatePositionOfAdornment.Dispose();
                         clearAdorner.Dispose();
                         createAdorner.Dispose();
                         dropped.Dispose();
                         this._fileDropped.OnCompleted();
                     });
        }
        #endregion
        #region Classes
        /// <summary>Taken shamelessly from https://github.com/punker76/gong-wpf-dragdrop Thanks</summary>
        private class DragAdorner : Adorner
        {
            #region Fields
            private readonly AdornerLayer _adornerLayer;
            private readonly UIElement _adornment;
            private Point _mousePositon;
            #endregion
            #region Constructors
            public DragAdorner(UIElement adornedElement, UIElement adornment, DragDropEffects effects = DragDropEffects.None)
                : base(adornedElement)
            {
                this._adornerLayer = AdornerLayer.GetAdornerLayer(adornedElement);
                this._adornerLayer.Add(this);
                this._adornment = adornment;
                this.IsHitTestVisible = false;
                this.Effects = effects;
            }
            #endregion
            #region Properties
            public Point MousePosition
            {
                get => this._mousePositon;
                set
                {
                    if (this._mousePositon == value) return;
                    this._mousePositon = value;
                    this._adornerLayer.Update(this.AdornedElement);
                }
            }
            protected override int VisualChildrenCount => 1;
            private DragDropEffects Effects { get; }
            #endregion
            #region Methods
            public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
            {
                var result = new GeneralTransformGroup();
                result.Children.Add(base.GetDesiredTransform(transform));
                result.Children.Add(new TranslateTransform(this.MousePosition.X - 4, this.MousePosition.Y - 4));
                return result;
            }
            public void Detatch() { this._adornerLayer.Remove(this); }
            protected override Size ArrangeOverride(Size finalSize)
            {
                this._adornment.Arrange(new Rect(finalSize));
                return finalSize;
            }
            protected override Visual GetVisualChild(int index) => this._adornment;
            protected override Size MeasureOverride(Size constraint)
            {
                this._adornment.Measure(constraint);
                return this._adornment.DesiredSize;
            }
            #endregion
        }
        #endregion
    }
}