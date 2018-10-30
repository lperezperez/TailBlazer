namespace TailBlazer.Infrastucture.Virtualisation
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reactive.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media;
    /// <summary>
    ///     This is adapted (butchered!) from VirtualWrapPanel in https://github.com/samueldjack/VirtualCollection See http://blog.hibernatingrhinos.com/12515/implementing-a-virtualizingwrappanel
    /// </summary>
    public class VirtualScrollPanel : VirtualizingPanel, IScrollInfo
    {
        #region Constants
        private const double ScrollLineAmount = 16.0;
        #endregion
        #region Fields
        public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register("ItemHeight", typeof(double), typeof(VirtualScrollPanel), new PropertyMetadata(1.0, VirtualScrollPanel.OnRequireMeasure));
        private static readonly DependencyProperty VirtualItemIndexProperty = DependencyProperty.RegisterAttached("VirtualItemIndex", typeof(int), typeof(VirtualScrollPanel), new PropertyMetadata(-1));
        public static readonly DependencyProperty TotalItemsProperty = DependencyProperty.Register("TotalItems", typeof(int), typeof(VirtualScrollPanel), new PropertyMetadata(default(int), VirtualScrollPanel.OnRequireMeasure));
        public static readonly DependencyProperty StartIndexProperty = DependencyProperty.Register("StartIndex", typeof(int), typeof(VirtualScrollPanel), new PropertyMetadata(default(int), VirtualScrollPanel.OnStartIndexChanged));
        public static readonly DependencyProperty LeftPositionProperty = DependencyProperty.Register("LeftPosition", typeof(double), typeof(VirtualScrollPanel), new PropertyMetadata(default(double)));
        public static readonly DependencyProperty ScrollReceiverProperty = DependencyProperty.Register("ScrollReceiver", typeof(IScrollReceiver), typeof(VirtualScrollPanel), new PropertyMetadata(default(IScrollReceiver)));
        public static readonly DependencyProperty TextWidthProperty = DependencyProperty.Register("TextWidth", typeof(int), typeof(VirtualScrollPanel), new PropertyMetadata(default(int)));
        private readonly Dictionary<UIElement, Rect> _childLayouts = new Dictionary<UIElement, Rect>();
        private ExtentInfo _extentInfo;
        private Size _extentSize;
        private int _firstIndex;
        private bool _isInMeasure;
        private ItemsControl _itemsControl;
        private IRecyclingItemContainerGenerator _itemsGenerator;
        private Point _offset;
        private int _size;
        private Size _viewportSize;
        #endregion
        #region Constructors
        public VirtualScrollPanel()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
                this.Dispatcher.BeginInvoke(new Action(this.Initialize));
        }
        #endregion
        #region Properties
        public bool CanHorizontallyScroll { get; set; }
        public bool CanVerticallyScroll { get; set; }
        public double ExtentHeight => this._extentSize.Height;
        public double ExtentWidth => this._extentSize.Width;
        public double HorizontalOffset => this._offset.X;
        public double ItemHeight { get => (double)this.GetValue(ItemHeightProperty); set => this.SetValue(ItemHeightProperty, value); }
        public double ItemWidth => this._extentSize.Width;
        public double LeftPosition { get => (double)this.GetValue(LeftPositionProperty); set => this.SetValue(LeftPositionProperty, value); }
        public ScrollViewer ScrollOwner { get; set; }
        public IScrollReceiver ScrollReceiver { get => (IScrollReceiver)this.GetValue(ScrollReceiverProperty); set => this.SetValue(ScrollReceiverProperty, value); }
        public int StartIndex { get => (int)this.GetValue(StartIndexProperty); set => this.SetValue(StartIndexProperty, value); }
        public int TextWidth { get => (int)this.GetValue(TextWidthProperty); set => this.SetValue(TextWidthProperty, value); }
        public int TotalItems { get => (int)this.GetValue(TotalItemsProperty); set => this.SetValue(TotalItemsProperty, value); }
        public double VerticalOffset => this._offset.Y + this._extentInfo.VerticalOffset;
        public double ViewportHeight => this._viewportSize.Height;
        public double ViewportWidth => this._viewportSize.Width;
        #endregion
        #region Methods
        private static int GetVirtualItemIndex(DependencyObject obj) => (int)obj.GetValue(VirtualItemIndexProperty);
        private static void OnRequireMeasure(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = (VirtualScrollPanel)d;
            panel.InvalidateMeasure();
            panel.InvalidateScrollInfo();
        }
        private static void OnStartIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = (VirtualScrollPanel)d;
            panel.CallbackStartIndexChanged(Convert.ToInt32(e.NewValue));
            panel.InvalidateMeasure();
        }
        private static void SetVirtualItemIndex(DependencyObject obj, int value) { obj.SetValue(VirtualItemIndexProperty, value); }
        public void LineDown()
        {
            //  InvokeStartIndexCommand(1);
            this.ScrollReceiver?.ScrollDiff(1);
        }
        public void LineLeft() { this.SetHorizontalOffset(this.HorizontalOffset + ScrollLineAmount); }
        public void LineRight() { this.SetHorizontalOffset(this.HorizontalOffset - ScrollLineAmount); }
        public void LineUp()
        {
            // InvokeStartIndexCommand(-1);
            this.ScrollReceiver?.ScrollDiff(-1);
        }
        public Rect MakeVisible(Visual visual, Rect rectangle) => new Rect();
        public void MouseWheelDown() { this.InvokeStartIndexCommand(SystemParameters.WheelScrollLines); }
        public void MouseWheelLeft() { this.SetHorizontalOffset(this.HorizontalOffset - ScrollLineAmount * SystemParameters.WheelScrollLines); }
        public void MouseWheelRight() { this.SetHorizontalOffset(this.HorizontalOffset + ScrollLineAmount * SystemParameters.WheelScrollLines); }
        public void MouseWheelUp() { this.InvokeStartIndexCommand(-SystemParameters.WheelScrollLines); }
        public void PageDown() { this.SetVerticalOffset(this.VerticalOffset + this.ViewportHeight); }
        public void PageLeft() { this.SetHorizontalOffset(this.HorizontalOffset + this.ItemWidth); }
        public void PageRight() { this.SetHorizontalOffset(this.HorizontalOffset - this.ItemWidth); }
        public void PageUp() { this.SetVerticalOffset(this.VerticalOffset - this.ViewportHeight); }
        public void SetHorizontalOffset(double offset)
        {
            offset = this.Clamp(offset, 0, this.ExtentWidth - this.ViewportWidth);
            if (offset < 0)
                this._offset.X = 0;
            else
                this._offset = new Point(offset, this._offset.Y);
            this.InvalidateScrollInfo();
            this.InvalidateMeasure();
        }
        public void SetVerticalOffset(double offset)
        {
            if (double.IsInfinity(offset)) return;
            var diff = (int)((offset - this._extentInfo.VerticalOffset) / this.ItemHeight);
            this.InvokeStartIndexCommand(diff);

            //stop the control from losing focus on page up / down
            Observable.Timer(TimeSpan.FromMilliseconds(125)).ObserveOn(this.Dispatcher).Subscribe
                (
                 l =>
                     {
                         if (this._itemsControl.Items.Count == 0) return;
                         var index = diff < 0 ? 0 : this._itemsControl.Items.Count - 1;
                         var generator = (ItemContainerGenerator)this._itemsGenerator;
                         this._itemsControl?.Focus();
                         var item = generator.ContainerFromIndex(index) as UIElement;
                         item?.Focus();
                     });
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement child in this.Children)
                child.Arrange(this._childLayouts[child]);
            return finalSize;
        }
        protected override Size MeasureOverride(Size availableSize)
        {
            if (this._itemsControl == null)
                return new Size(double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width, double.IsInfinity(availableSize.Height) ? 0 : availableSize.Height);
            this._isInMeasure = true;
            this._childLayouts.Clear();
            this._extentInfo = this.GetVerticalExtentInfo(availableSize);
            this.EnsureScrollOffsetIsWithinConstrains(this._extentInfo);
            var layoutInfo = this.GetLayoutInfo(availableSize, this.ItemHeight, this._extentInfo);
            this.RecycleItems(layoutInfo);

            // Determine where the first item is in relation to previously realized items
            var generatorStartPosition = this._itemsGenerator.GeneratorPositionFromIndex(layoutInfo.FirstRealizedItemIndex);
            var visualIndex = 0;
            double widestWidth = 0;
            var currentX = layoutInfo.FirstRealizedItemLeft;
            var currentY = layoutInfo.FirstRealizedLineTop;
            using (this._itemsGenerator.StartAt(generatorStartPosition, GeneratorDirection.Forward, true))
            {
                var children = new List<UIElement>();
                for (var itemIndex = layoutInfo.FirstRealizedItemIndex; itemIndex <= layoutInfo.LastRealizedItemIndex; itemIndex++, visualIndex++)
                {
                    bool newlyRealized;
                    var child = (UIElement)this._itemsGenerator.GenerateNext(out newlyRealized);
                    if (child == null) continue;
                    children.Add(child);
                    VirtualScrollPanel.SetVirtualItemIndex(child, itemIndex);
                    if (newlyRealized)
                    {
                        this.InsertInternalChild(visualIndex, child);
                    }
                    else
                    {
                        // check if item needs to be moved into a new position in the Children collection
                        if (visualIndex < this.Children.Count)
                        {
                            if (object.Equals(this.Children[visualIndex], child)) continue;
                            var childCurrentIndex = this.Children.IndexOf(child);
                            if (childCurrentIndex >= 0)
                                this.RemoveInternalChildRange(childCurrentIndex, 1);
                            this.InsertInternalChild(visualIndex, child);
                        }
                        else
                        {
                            // we know that the child can't already be in the children collection
                            // because we've been inserting children in correct visualIndex order,
                            // and this child has a visualIndex greater than the Children.Count
                            this.AddInternalChild(child);
                        }
                    }
                }

                //part 2: do the measure
                foreach (var child in children)
                {
                    this._itemsGenerator.PrepareItemContainer(child);
                    child.Measure(new Size(double.PositiveInfinity, this.ItemHeight));
                    widestWidth = Math.Max(widestWidth, child.DesiredSize.Width);
                }

                //part 3: Create the elements
                foreach (var child in children)
                {
                    this._childLayouts.Add(child, new Rect(currentX, currentY, Math.Max(widestWidth, this._viewportSize.Width), this.ItemHeight));
                    currentY += this.ItemHeight;
                }
            }
            this.RemoveRedundantChildren();
            this.UpdateScrollInfo(availableSize, this._extentInfo, widestWidth);
            this._isInMeasure = false;
            return new Size(double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width, double.IsInfinity(availableSize.Height) ? 0 : availableSize.Height);
            //return availableSize;
        }
        protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
        {
            base.OnItemsChanged(sender, args);
            this.InvalidateMeasure();
        }
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (!sizeInfo.HeightChanged) return;
            var items = (int)(sizeInfo.NewSize.Height / this.ItemHeight);
            this.InvokeSizeCommand(items);
        }
        private void CallbackStartIndexChanged(int index)
        {
            if (this._firstIndex == index) return;
            this._firstIndex = index;
            this.ReportChanges();
        }
        private double Clamp(double value, double min, double max) => Math.Min(Math.Max(value, min), max);
        private void EnsureScrollOffsetIsWithinConstrains(ExtentInfo extentInfo) { this._offset.Y = this.Clamp(this._offset.Y, 0, extentInfo.MaxVerticalOffset); }
        private ItemLayoutInfo GetLayoutInfo(Size availableSize, double itemHeight, ExtentInfo extentInfo)
        {
            if (this._itemsControl == null)
                return new ItemLayoutInfo();

            // we need to ensure that there is one realized item prior to the first visible item, and one after the last visible item,
            // so that keyboard navigation works properly. For example, when focus is on the first visible item, and the user
            // navigates up, the ListBox selects the previous item, and the scrolls that into view - and this triggers the loading of the rest of the items 
            // in that row
            var firstVisibleLine = (int)Math.Floor(this._offset.Y / itemHeight);
            var firstRealizedIndex = Math.Max(firstVisibleLine - 1, 0);
            var firstRealizedItemLeft = firstRealizedIndex * this.ItemWidth - this.HorizontalOffset;
            var firstRealizedItemTop = firstRealizedIndex * itemHeight - this._offset.Y;
            var firstCompleteLineTop = firstVisibleLine == 0 ? firstRealizedItemTop : firstRealizedItemTop + this.ItemHeight;
            var completeRealizedLines = (int)Math.Ceiling((availableSize.Height - firstCompleteLineTop) / itemHeight);
            var lastRealizedIndex = Math.Min(firstRealizedIndex + completeRealizedLines + 2, this._itemsControl.Items.Count - 1);
            return new ItemLayoutInfo(firstRealizedIndex, firstRealizedItemTop, firstRealizedItemLeft, lastRealizedIndex);
        }
        private ExtentInfo GetVerticalExtentInfo(Size viewPortSize)
        {
            if (this._itemsControl == null)
                return new ExtentInfo();
            var extentHeight = Math.Max(this.TotalItems * this.ItemHeight, viewPortSize.Height);
            var maxVerticalOffset = extentHeight; // extentHeight - viewPortSize.Height;
            var verticalOffset = this.StartIndex / (double)this.TotalItems * maxVerticalOffset;
            return new ExtentInfo(this.TotalItems, this._itemsControl.Items.Count, verticalOffset, maxVerticalOffset, extentHeight);
        }
        private void Initialize()
        {
            this._itemsControl = ItemsControl.GetItemsOwner(this);
            this._itemsGenerator = (IRecyclingItemContainerGenerator)this.ItemContainerGenerator;
            this.InvalidateMeasure();
        }
        private void InvalidateScrollInfo() { this.ScrollOwner?.InvalidateScrollInfo(); }
        private void InvokeSizeCommand(int size)
        {
            if (this._size == size) return;
            this._size = size;
            this.ReportChanges();
        }
        private void InvokeStartIndexCommand(int lines)
        {
            if (this._isInMeasure) return;
            var firstIndex = this.StartIndex + lines;
            if (firstIndex < 0)
                firstIndex = 0;
            else if (firstIndex + this._extentInfo.VirtualCount >= this._extentInfo.TotalCount)
                firstIndex = this._extentInfo.TotalCount - this._extentInfo.VirtualCount;
            if (firstIndex == this._firstIndex) return;
            if (this._firstIndex == firstIndex) return;
            this._firstIndex = firstIndex;
            this.OnOffsetChanged(lines > 0 ? ScrollDirection.Down : ScrollDirection.Up, lines);
            this.ReportChanges();
        }
        private void OnOffsetChanged(ScrollDirection direction, int firstRow) { this.ScrollReceiver?.ScrollChanged(new ScrollChangedArgs(direction, firstRow)); }
        private void RecycleItems(ItemLayoutInfo layoutInfo)
        {
            foreach (UIElement child in this.Children)
            {
                var virtualItemIndex = VirtualScrollPanel.GetVirtualItemIndex(child);
                if (virtualItemIndex < layoutInfo.FirstRealizedItemIndex || virtualItemIndex > layoutInfo.LastRealizedItemIndex)
                {
                    var generatorPosition = this._itemsGenerator.GeneratorPositionFromIndex(virtualItemIndex);
                    if (generatorPosition.Index >= 0)
                        try
                        {
                            this._itemsGenerator.Recycle(generatorPosition, 1);
                        }
                        catch (ArgumentException)
                        {
                            //I have seen the following exception which appears to be a non-issue
                            // GeneratorPosition '0,10' passed to Remove does not have Offset equal to 0.
                        }
                }
                VirtualScrollPanel.SetVirtualItemIndex(child, -1);
            }
        }
        private void RemoveRedundantChildren()
        {
            // iterate backwards through the child collection because we're going to be
            // removing items from it
            for (var i = this.Children.Count - 1; i >= 0; i--)
            {
                var child = this.Children[i];

                // if the virtual item index is -1, this indicates
                // it is a recycled item that hasn't been reused this time round
                if (VirtualScrollPanel.GetVirtualItemIndex(child) == -1)
                    this.RemoveInternalChildRange(i, 1);
            }
        }
        private void ReportChanges() { this.ScrollReceiver?.ScrollBoundsChanged(new ScrollBoundsArgs(this._size, this._firstIndex)); }
        private void UpdateScrollInfo(Size availableSize, ExtentInfo extentInfo, double actualWidth)
        {
            this._viewportSize = availableSize;
            this._extentSize = new Size(actualWidth, extentInfo.Height);
            this.InvalidateScrollInfo();
        }
        #endregion
        #region Structs
        private struct ExtentInfo
        {
            #region Constructors
            public ExtentInfo(int totalCount, int virtualCount, double verticalOffset, double maxVerticalOffset, double height)
                : this()
            {
                this.TotalCount = totalCount;
                this.VirtualCount = virtualCount;
                this.VerticalOffset = verticalOffset;
                this.MaxVerticalOffset = maxVerticalOffset;
                this.Height = height;
            }
            #endregion
            #region Properties
            public double Height { get; }
            public double MaxVerticalOffset { get; }
            public int TotalCount { get; }
            public double VerticalOffset { get; }
            public int VirtualCount { get; }
            #endregion
        }
        private struct ItemLayoutInfo
        {
            #region Constructors
            public ItemLayoutInfo(int firstRealizedItemIndex, double firstRealizedLineTop, double firstRealizedItemLeft, int lastRealizedItemIndex)
                : this()
            {
                this.FirstRealizedItemIndex = firstRealizedItemIndex;
                this.FirstRealizedLineTop = firstRealizedLineTop;
                this.FirstRealizedItemLeft = firstRealizedItemLeft;
                this.LastRealizedItemIndex = lastRealizedItemIndex;
            }
            #endregion
            #region Properties
            public int FirstRealizedItemIndex { get; }
            public double FirstRealizedItemLeft { get; }
            public double FirstRealizedLineTop { get; }
            public int LastRealizedItemIndex { get; }
            #endregion
        }
        #endregion
    }
}