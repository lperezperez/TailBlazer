namespace TailBlazer.Infrastucture.Virtualisation

    //TODO: 1) Clamp offset.X. 
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media;
    public static class MeasureEx
    {
        #region Methods
        public static Size MeasureString(this Control source, string candidate)
        {
            var formattedText = new FormattedText(candidate, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface(source.FontFamily, source.FontStyle, source.FontWeight, source.FontStretch), source.FontSize, Brushes.Black);
            return new Size(formattedText.Width, formattedText.Height);
        }
        #endregion
    }
    /// <summary>
    ///     This is adapted (butchered!) from VirtualWrapPanel in https://github.com/samueldjack/VirtualCollection See http://blog.hibernatingrhinos.com/12515/implementing-a-virtualizingwrappanel
    /// </summary>
    public class LinesScrollPanel : VirtualizingPanel, IScrollInfo
    {
        #region Constants
        private const double ScrollLineAmount = 16.0;
        #endregion
        #region Fields
        //For Horizonal scroll we need
        //1. Max number of chars of all the lines []
        //2. Starting Character
        //3. Number of visible characters required
        //4. Plus we need to be supplied 

        //We need 2 calcs - First visible char + number of visible chars (+ overflow)
        public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register("ItemHeight", typeof(double), typeof(LinesScrollPanel), new PropertyMetadata(1.0, LinesScrollPanel.OnRequireMeasure));
        private static readonly DependencyProperty VirtualItemIndexProperty = DependencyProperty.RegisterAttached("VirtualItemIndex", typeof(int), typeof(LinesScrollPanel), new PropertyMetadata(-1));
        public static readonly DependencyProperty TotalItemsProperty = DependencyProperty.Register("TotalItems", typeof(int), typeof(LinesScrollPanel), new PropertyMetadata(default(int), LinesScrollPanel.OnRequireMeasure));
        public static readonly DependencyProperty StartIndexProperty = DependencyProperty.Register("StartIndex", typeof(int), typeof(LinesScrollPanel), new PropertyMetadata(default(int), LinesScrollPanel.OnStartIndexChanged));
        public static readonly DependencyProperty HorizontalScrollChangedProperty = DependencyProperty.Register("HorizontalScrollChanged", typeof(TextScrollDelegate), typeof(LinesScrollPanel), new PropertyMetadata(default(TextScrollDelegate)));
        public static readonly DependencyProperty ScrollReceiverProperty = DependencyProperty.Register("ScrollReceiver", typeof(IScrollReceiver), typeof(LinesScrollPanel), new PropertyMetadata(default(IScrollReceiver)));
        public static readonly DependencyProperty CharacterWidthProperty = DependencyProperty.Register("CharacterWidth", typeof(double), typeof(LinesScrollPanel), new PropertyMetadata(default(double), LinesScrollPanel.OnCharactersChanged));
        public static readonly DependencyProperty TotalCharactersProperty = DependencyProperty.Register("TotalCharacters", typeof(int), typeof(LinesScrollPanel), new PropertyMetadata(default(int), LinesScrollPanel.OnCharactersChanged));
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
        public LinesScrollPanel()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
                this.Dispatcher.BeginInvoke(new Action(this.Initialize));
        }
        #endregion
        #region Properties
        public bool CanHorizontallyScroll { get; set; }
        public bool CanVerticallyScroll { get; set; }
        public double CharacterWidth { get => (double)this.GetValue(CharacterWidthProperty); set => this.SetValue(CharacterWidthProperty, value); }
        public double ExtentHeight => this._extentSize.Height;
        public double ExtentWidth => this._extentSize.Width;
        public double HorizontalOffset => this._offset.X;
        public TextScrollDelegate HorizontalScrollChanged { get => (TextScrollDelegate)this.GetValue(HorizontalScrollChangedProperty); set => this.SetValue(HorizontalScrollChangedProperty, value); }
        public double ItemHeight { get => (double)this.GetValue(ItemHeightProperty); set => this.SetValue(ItemHeightProperty, value); }
        public double ItemWidth => this._viewportSize.Width;
        public ScrollViewer ScrollOwner { get; set; }
        public IScrollReceiver ScrollReceiver { get => (IScrollReceiver)this.GetValue(ScrollReceiverProperty); set => this.SetValue(ScrollReceiverProperty, value); }
        public int StartIndex { get => (int)this.GetValue(StartIndexProperty); set => this.SetValue(StartIndexProperty, value); }
        public int TotalCharacters { get => (int)this.GetValue(TotalCharactersProperty); set => this.SetValue(TotalCharactersProperty, value); }
        public int TotalItems { get => (int)this.GetValue(TotalItemsProperty); set => this.SetValue(TotalItemsProperty, value); }
        public double VerticalOffset => this._offset.Y + this._extentInfo.VerticalOffset;
        public double ViewportHeight => this._viewportSize.Height;
        public double ViewportWidth => this._viewportSize.Width;
        #endregion
        #region Methods
        private static int GetVirtualItemIndex(DependencyObject obj) => (int)obj.GetValue(VirtualItemIndexProperty);
        private static void OnCharactersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = (LinesScrollPanel)d;
            panel.InvalidateScrollInfo();
            panel.InvalidateMeasure();
        }
        private static void OnRequireMeasure(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = (LinesScrollPanel)d;
            panel.InvalidateMeasure();
            panel.InvalidateScrollInfo();
        }
        private static void OnStartIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = (LinesScrollPanel)d;
            panel.CallbackStartIndexChanged(Convert.ToInt32(e.NewValue));
            panel.InvalidateMeasure();
        }
        private static void SetVirtualItemIndex(DependencyObject obj, int value) { obj.SetValue(VirtualItemIndexProperty, value); }
        public void LineDown()
        {
            //  InvokeStartIndexCommand(1);
            this.ScrollReceiver?.ScrollDiff(1);
        }
        public void LineLeft() { this.SetHorizontalOffset(this.HorizontalOffset - ScrollLineAmount); }
        public void LineRight() { this.SetHorizontalOffset(this.HorizontalOffset + ScrollLineAmount); }
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
        public void PageLeft() { this.SetHorizontalOffset(this.HorizontalOffset - this.ItemWidth); }
        public void PageRight() { this.SetHorizontalOffset(this.HorizontalOffset + this.ItemWidth); }
        public void PageUp() { this.SetVerticalOffset(this.VerticalOffset - this.ViewportHeight); }
        public void SetHorizontalOffset(double offset)
        {
            offset = this.Clamp(offset, 0, this.ExtentWidth - this.ViewportWidth);
            if (offset < 0)
                this._offset.X = 0;
            else
                this._offset = new Point(offset, this._offset.Y);
            this.CalculateHorizonalScrollInfo();
            this.InvalidateScrollInfo();
            this.InvalidateMeasure();
        }
        public void SetVerticalOffset(double offset)
        {
            if (double.IsInfinity(offset)) return;
            var diff = (int)((offset - this._extentInfo.VerticalOffset) / this.ItemHeight);
            this.InvokeStartIndexCommand(diff);

            ////stop the control from losing focus on page up / down
            //Observable.Timer(TimeSpan.FromMilliseconds(25))
            //    .ObserveOn(Dispatcher)
            //    .Subscribe(_ =>
            //    {
            //        if (_itemsControl.Items.Count == 0) return;

            //        var index = diff < 0 ? 0 : _itemsControl.Items.Count - 1;
            //        var generator = (ItemContainerGenerator)_itemsGenerator;
            //        _itemsControl?.Focus();
            //        var item = generator.ContainerFromIndex(index) as UIElement;
            //        item?.Focus();
            //    });
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
            this._extentInfo = this.GetExtentInfo(availableSize);
            this.EnsureScrollOffsetIsWithinConstrains(this._extentInfo);
            var layoutInfo = this.GetLayoutInfo(availableSize, this.ItemHeight, this._extentInfo);
            this.RecycleItems(layoutInfo);

            // Determine where the first item is in relation to previously realized items
            var generatorStartPosition = this._itemsGenerator.GeneratorPositionFromIndex(layoutInfo.FirstRealizedItemIndex);
            var visualIndex = 0;
            double widestWidth = 0;
            var currentX = 0; //layoutInfo.FirstRealizedItemLeft;
            var currentY = layoutInfo.FirstRealizedLineTop;

            ////1. Calc width, Call back available chars + first char
            //var width = TotalCharacters * CharacterWidth + 22;
            using (this._itemsGenerator.StartAt(generatorStartPosition, GeneratorDirection.Forward, true))
            {
                var children = new List<UIElement>();
                for (var itemIndex = layoutInfo.FirstRealizedItemIndex; itemIndex <= layoutInfo.LastRealizedItemIndex; itemIndex++, visualIndex++)
                {
                    var child = (UIElement)this._itemsGenerator.GenerateNext(out var newlyRealized);
                    if (child == null) continue;
                    children.Add(child);
                    LinesScrollPanel.SetVirtualItemIndex(child, itemIndex);
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
                    //TODO: Widest = Chars + Additional space = 20
                    //[ideally should scroll from where the text begins]
                    this._itemsGenerator.PrepareItemContainer(child);
                    child.Measure(new Size(this._viewportSize.Width, this.ItemHeight));
                    widestWidth = Math.Max(widestWidth, child.DesiredSize.Width);
                }

                //    Console.WriteLine("Widest={0} Calc={1}", widestWidth, width);

                //part 3: Create the elements
                foreach (var child in children)
                {
                    this._childLayouts.Add(child, new Rect(currentX, currentY, Math.Max(this._viewportSize.Width, this._viewportSize.Width), this.ItemHeight));
                    currentY += this.ItemHeight;
                }
            }
            this.RemoveRedundantChildren();
            this.UpdateScrollInfo(availableSize, this._extentInfo);

            //NotifyHorizonalScroll(_extentInfo);
            this._isInMeasure = false;
            return new Size(double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width, double.IsInfinity(availableSize.Height) ? 0 : availableSize.Height);
        }
        protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
        {
            base.OnItemsChanged(sender, args);
            this.InvalidateMeasure();
        }
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (sizeInfo.WidthChanged)
                this.CalculateHorizonalScrollInfo();
            if (!sizeInfo.HeightChanged) return;
            var items = (int)(sizeInfo.NewSize.Height / this.ItemHeight);
            this.InvokeSizeCommand(items);
        }
        private void CalculateHorizonalScrollInfo()
        {
            this._extentInfo = this.GetExtentInfo(this.RenderSize);
            this.UpdateScrollInfo(this.RenderSize, this._extentInfo);
            this.EnsureScrollOffsetIsWithinConstrains(this._extentInfo);
            this.NotifyHorizonalScroll(this._extentInfo);
        }
        private void CallbackStartIndexChanged(int index)
        {
            if (this._firstIndex == index) return;
            this._firstIndex = index;
            this.ReportChanges();
        }
        private double Clamp(double value, double min, double max) => Math.Min(Math.Max(value, min), max);
        private void EnsureScrollOffsetIsWithinConstrains(ExtentInfo extentInfo)
        {
            this._offset.Y = this.Clamp(this._offset.Y, 0, extentInfo.MaxVerticalOffset);
            this._offset.X = this.Clamp(this._offset.X, 0, extentInfo.MaxHorizontalOffset);
        }
        private ExtentInfo GetExtentInfo(Size viewPortSize)
        {
            if (this._itemsControl == null)
                return new ExtentInfo();
            var extentHeight = Math.Max(this.TotalItems * this.ItemHeight, viewPortSize.Height);
            var maxVerticalOffset = extentHeight; // extentHeight - viewPortSize.Height;
            var verticalOffset = this.StartIndex / (double)this.TotalItems * maxVerticalOffset;

            //widest width
            //TOO: Big fat hack 39 = Icon column width + scrollbar width: DO IT PROPERLY
            var extentWidth = this.TotalCharacters * this.CharacterWidth + 39;
            var maximumChars = Math.Ceiling((viewPortSize.Width - 39) / this.CharacterWidth);
            var maxHorizontalOffset = extentWidth;
            return new ExtentInfo(this.TotalItems, this._itemsControl.Items.Count, verticalOffset, maxVerticalOffset, extentHeight, extentWidth, maximumChars, maxHorizontalOffset);
        }
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
        private void NotifyHorizonalScroll(ExtentInfo extentInfo)
        {
            var startCharacter = Math.Ceiling(this._offset.X / this.CharacterWidth);

            //clamp when required
            if (startCharacter + extentInfo.MaximumChars > this.TotalCharacters)
                startCharacter = Math.Max(0, this.TotalCharacters - extentInfo.MaximumChars);
            this.HorizontalScrollChanged?.Invoke(new TextScrollInfo((int)startCharacter, (int)extentInfo.MaximumChars));
        }
        private void OnOffsetChanged(ScrollDirection direction, int firstRow) { this.ScrollReceiver?.ScrollChanged(new ScrollChangedArgs(direction, firstRow)); }
        private void RecycleItems(ItemLayoutInfo layoutInfo)
        {
            foreach (UIElement child in this.Children)
            {
                var virtualItemIndex = LinesScrollPanel.GetVirtualItemIndex(child);
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
                LinesScrollPanel.SetVirtualItemIndex(child, -1);
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
                if (LinesScrollPanel.GetVirtualItemIndex(child) == -1)
                    this.RemoveInternalChildRange(i, 1);
            }
        }
        private void ReportChanges() { this.ScrollReceiver?.ScrollBoundsChanged(new ScrollBoundsArgs(this._size, this._firstIndex)); }
        private void UpdateScrollInfo(Size availableSize, ExtentInfo extentInfo)
        {
            this._viewportSize = availableSize;
            this._extentSize = new Size(extentInfo.Width, extentInfo.Height);
            this.InvalidateScrollInfo();
        }
        #endregion
        #region Structs
        private struct ExtentInfo
        {
            #region Constructors
            public ExtentInfo(int totalCount, int virtualCount, double verticalOffset, double maxVerticalOffset, double height, double width, double maximumChars, double maxHorizontalOffset)
                : this()
            {
                this.MaximumChars = maximumChars;
                this.TotalCount = totalCount;
                this.VirtualCount = virtualCount;
                this.VerticalOffset = verticalOffset;
                this.MaxVerticalOffset = maxVerticalOffset;
                this.Height = height;
                this.Width = width;
                this.MaxHorizontalOffset = maxHorizontalOffset;
            }
            #endregion
            #region Properties
            public double Height { get; }
            public double MaxHorizontalOffset { get; }
            public double MaximumChars { get; }
            public double MaxVerticalOffset { get; }
            public int TotalCount { get; }
            public double VerticalOffset { get; }
            public int VirtualCount { get; }
            public double Width { get; }
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