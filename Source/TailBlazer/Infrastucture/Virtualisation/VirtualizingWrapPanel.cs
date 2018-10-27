namespace TailBlazer.Infrastucture.Virtualisation
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media;
    public class ItemLayoutInfo
    {
        #region Fields
        public int FirstRealizedItemIndex;
        public double FirstRealizedItemLeft;
        public double FirstRealizedLineTop;
        public int LastRealizedItemIndex;
        #endregion
    }
    public class VirtualizingWrapPanel : VirtualizingPanel, IScrollInfo
    {
        #region Constants
        private const double ScrollLineAmount = 16.0;
        #endregion
        #region Fields
        public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register("ItemWidth", typeof(double), typeof(VirtualizingWrapPanel), new PropertyMetadata(1.0, VirtualizingWrapPanel.HandleItemDimensionChanged));
        public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register("ItemHeight", typeof(double), typeof(VirtualizingWrapPanel), new PropertyMetadata(1.0, VirtualizingWrapPanel.HandleItemDimensionChanged));
        private static readonly DependencyProperty VirtualItemIndexProperty = DependencyProperty.RegisterAttached("VirtualItemIndex", typeof(int), typeof(VirtualizingWrapPanel), new PropertyMetadata(-1));
        private readonly Dictionary<UIElement, Rect> _childLayouts = new Dictionary<UIElement, Rect>();
        private Size _extentSize;
        private bool _isInMeasure;
        private ItemsControl _itemsControl;
        private IRecyclingItemContainerGenerator _itemsGenerator;
        private Point _offset;
        private Size _viewportSize;
        #endregion
        #region Constructors
        public VirtualizingWrapPanel()
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
        public double ItemWidth { get => (double)this.GetValue(ItemWidthProperty); set => this.SetValue(ItemWidthProperty, value); }
        public ScrollViewer ScrollOwner { get; set; }
        public double VerticalOffset => this._offset.Y;
        public double ViewportHeight => this._viewportSize.Height;
        public double ViewportWidth => this._viewportSize.Width;
        #endregion
        #region Methods
        private static int GetVirtualItemIndex(DependencyObject obj) => (int)obj.GetValue(VirtualItemIndexProperty);
        private static void HandleItemDimensionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var wrapPanel = d as VirtualizingWrapPanel;
            wrapPanel.InvalidateMeasure();
        }
        private static void SetVirtualItemIndex(DependencyObject obj, int value) { obj.SetValue(VirtualItemIndexProperty, value); }
        public ItemLayoutInfo GetVisibleItemsRange() => this.GetLayoutInfo(this._viewportSize, this.ItemHeight, this.GetExtentInfo(this._viewportSize, this.ItemHeight));
        public void LineDown() { this.SetVerticalOffset(this.VerticalOffset + ScrollLineAmount); }
        public void LineLeft() { this.SetHorizontalOffset(this.HorizontalOffset + ScrollLineAmount); }
        public void LineRight() { this.SetHorizontalOffset(this.HorizontalOffset - ScrollLineAmount); }
        public void LineUp() { this.SetVerticalOffset(this.VerticalOffset - ScrollLineAmount); }
        public Rect MakeVisible(Visual visual, Rect rectangle) => new Rect();
        public Rect MakeVisible(UIElement visual, Rect rectangle) => new Rect();
        public void MouseWheelDown() { this.SetVerticalOffset(this.VerticalOffset + ScrollLineAmount * SystemParameters.WheelScrollLines); }
        public void MouseWheelLeft() { this.SetHorizontalOffset(this.HorizontalOffset - ScrollLineAmount * SystemParameters.WheelScrollLines); }
        public void MouseWheelRight() { this.SetHorizontalOffset(this.HorizontalOffset + ScrollLineAmount * SystemParameters.WheelScrollLines); }
        public void MouseWheelUp() { this.SetVerticalOffset(this.VerticalOffset - ScrollLineAmount * SystemParameters.WheelScrollLines); }
        public void PageDown() { this.SetVerticalOffset(this.VerticalOffset + this.ViewportHeight); }
        public void PageLeft() { this.SetHorizontalOffset(this.HorizontalOffset + this.ItemWidth); }
        public void PageRight() { this.SetHorizontalOffset(this.HorizontalOffset - this.ItemWidth); }
        public void PageUp() { this.SetVerticalOffset(this.VerticalOffset - this.ViewportHeight); }
        public void SetHorizontalOffset(double offset)
        {
            if (this._isInMeasure)
                return;
            offset = this.Clamp(offset, 0, this.ExtentWidth - this.ViewportWidth);
            this._offset = new Point(offset, this._offset.Y);
            this.InvalidateScrollInfo();
            this.InvalidateMeasure();
        }
        public void SetVerticalOffset(double offset)
        {
            if (this._isInMeasure)
                return;
            offset = this.Clamp(offset, 0, this.ExtentHeight - this.ViewportHeight);
            this._offset = new Point(this._offset.X, offset);
            this.InvalidateScrollInfo();
            this.InvalidateMeasure();
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (var child in this.Children.OfType<UIElement>())
                child.Arrange(this._childLayouts[child]);
            return finalSize;
        }
        protected override Size MeasureOverride(Size availableSize)
        {
            if (this._itemsControl == null)
                return new Size(double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width, double.IsInfinity(availableSize.Height) ? 0 : availableSize.Height);
            this._isInMeasure = true;
            this._childLayouts.Clear();
            var extentInfo = this.GetExtentInfo(availableSize, this.ItemHeight);
            this.EnsureScrollOffsetIsWithinConstrains(extentInfo);
            var layoutInfo = this.GetLayoutInfo(availableSize, this.ItemHeight, extentInfo);
            this.RecycleItems(layoutInfo);

            // Determine where the first item is in relation to previously realized items
            var generatorStartPosition = this._itemsGenerator.GeneratorPositionFromIndex(layoutInfo.FirstRealizedItemIndex);
            var visualIndex = 0;
            var currentX = layoutInfo.FirstRealizedItemLeft;
            var currentY = layoutInfo.FirstRealizedLineTop;
            using (this._itemsGenerator.StartAt(generatorStartPosition, GeneratorDirection.Forward, true))
                for (var itemIndex = layoutInfo.FirstRealizedItemIndex; itemIndex <= layoutInfo.LastRealizedItemIndex; itemIndex++, visualIndex++)
                {
                    bool newlyRealized;
                    var child = (UIElement)this._itemsGenerator.GenerateNext(out newlyRealized);
                    VirtualizingWrapPanel.SetVirtualItemIndex(child, itemIndex);
                    if (newlyRealized)
                    {
                        this.InsertInternalChild(visualIndex, child);
                    }
                    else
                    {
                        // check if item needs to be moved into a new position in the Children collection
                        if (visualIndex < this.Children.Count)
                        {
                            if (this.Children[visualIndex] != child)
                            {
                                var childCurrentIndex = this.Children.IndexOf(child);
                                if (childCurrentIndex >= 0)
                                    this.RemoveInternalChildRange(childCurrentIndex, 1);
                                this.InsertInternalChild(visualIndex, child);
                            }
                        }
                        else
                        {
                            // we know that the child can't already be in the children collection
                            // because we've been inserting children in correct visualIndex order,
                            // and this child has a visualIndex greater than the Children.Count
                            this.AddInternalChild(child);
                        }
                    }

                    // only prepare the item once it has been added to the visual tree
                    this._itemsGenerator.PrepareItemContainer(child);
                    child.Measure(new Size(this.ItemWidth, this.ItemHeight));
                    this._childLayouts.Add(child, new Rect(currentX, currentY, this.ItemWidth, this.ItemHeight));
                    if (currentX + this.ItemWidth * 2 >= availableSize.Width)
                    {
                        // wrap to a new line
                        currentY += this.ItemHeight;
                        currentX = 0;
                    }
                    else
                    {
                        currentX += this.ItemWidth;
                    }
                }
            this.RemoveRedundantChildren();
            this.UpdateScrollInfo(availableSize, extentInfo);
            var desiredSize = new Size(double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width, double.IsInfinity(availableSize.Height) ? 0 : availableSize.Height);
            this._isInMeasure = false;
            return desiredSize;
        }
        protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
        {
            base.OnItemsChanged(sender, args);
            this.InvalidateMeasure();
        }
        private double Clamp(double value, double min, double max) => Math.Min(Math.Max(value, min), max);
        private void EnsureScrollOffsetIsWithinConstrains(ExtentInfo extentInfo) { this._offset.Y = this.Clamp(this._offset.Y, 0, extentInfo.MaxVerticalOffset); }
        private ExtentInfo GetExtentInfo(Size viewPortSize, double itemHeight)
        {
            if (this._itemsControl == null)
                return new ExtentInfo();
            var itemsPerLine = Math.Max((int)Math.Floor(viewPortSize.Width / this.ItemWidth), 1);
            var totalLines = (int)Math.Ceiling((double)this._itemsControl.Items.Count / itemsPerLine);
            var extentHeight = Math.Max(totalLines * this.ItemHeight, viewPortSize.Height);
            return new ExtentInfo { ItemsPerLine = itemsPerLine, TotalLines = totalLines, ExtentHeight = extentHeight, MaxVerticalOffset = extentHeight - viewPortSize.Height };
        }
        private ItemLayoutInfo GetLayoutInfo(Size availableSize, double itemHeight, ExtentInfo extentInfo)
        {
            if (this._itemsControl == null)
                return new ItemLayoutInfo();

            // we need to ensure that there is one realized item prior to the first visible item, and one after the last visible item,
            // so that keyboard navigation works properly. For example, when focus is on the first visible item, and the user
            // navigates up, the ListBox selects the previous item, and the scrolls that into view - and this triggers the loading of the rest of the items 
            // in that row
            var firstVisibleLine = (int)Math.Floor(this.VerticalOffset / itemHeight);
            var firstRealizedIndex = Math.Max(extentInfo.ItemsPerLine * firstVisibleLine - 1, 0);
            var firstRealizedItemLeft = firstRealizedIndex % extentInfo.ItemsPerLine * this.ItemWidth - this.HorizontalOffset;
            var firstRealizedItemTop = firstRealizedIndex / extentInfo.ItemsPerLine * itemHeight - this.VerticalOffset;
            var firstCompleteLineTop = firstVisibleLine == 0 ? firstRealizedItemTop : firstRealizedItemTop + this.ItemHeight;
            var completeRealizedLines = (int)Math.Ceiling((availableSize.Height - firstCompleteLineTop) / itemHeight);
            var lastRealizedIndex = Math.Min(firstRealizedIndex + completeRealizedLines * extentInfo.ItemsPerLine + 2, this._itemsControl.Items.Count - 1);
            return new ItemLayoutInfo { FirstRealizedItemIndex = firstRealizedIndex, FirstRealizedItemLeft = firstRealizedItemLeft, FirstRealizedLineTop = firstRealizedItemTop, LastRealizedItemIndex = lastRealizedIndex };
        }
        private void Initialize()
        {
            this._itemsControl = ItemsControl.GetItemsOwner(this);
            this._itemsGenerator = (IRecyclingItemContainerGenerator)this.ItemContainerGenerator;
            this.InvalidateMeasure();
        }
        private void InvalidateScrollInfo() { this.ScrollOwner?.InvalidateScrollInfo(); }
        private void RecycleItems(ItemLayoutInfo layoutInfo)
        {
            foreach (var child in this.Children.OfType<UIElement>())
            {
                var virtualItemIndex = VirtualizingWrapPanel.GetVirtualItemIndex(child);
                if (virtualItemIndex < layoutInfo.FirstRealizedItemIndex || virtualItemIndex > layoutInfo.LastRealizedItemIndex)
                {
                    var generatorPosition = this._itemsGenerator.GeneratorPositionFromIndex(virtualItemIndex);
                    if (generatorPosition.Index >= 0)
                        this._itemsGenerator.Recycle(generatorPosition, 1);
                }
                VirtualizingWrapPanel.SetVirtualItemIndex(child, -1);
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
                if (VirtualizingWrapPanel.GetVirtualItemIndex(child) == -1)
                    this.RemoveInternalChildRange(i, 1);
            }
        }
        private void UpdateScrollInfo(Size availableSize, ExtentInfo extentInfo)
        {
            this._viewportSize = availableSize;
            this._extentSize = new Size(availableSize.Width, extentInfo.ExtentHeight);
            this.InvalidateScrollInfo();
        }
        #endregion
        #region Classes
        internal class ExtentInfo
        {
            #region Fields
            public double ExtentHeight;
            public int ItemsPerLine;
            public double MaxVerticalOffset;
            public int TotalLines;
            #endregion
        }
        #endregion
    }
}