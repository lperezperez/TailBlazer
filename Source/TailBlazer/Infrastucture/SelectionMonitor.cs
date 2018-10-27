namespace TailBlazer.Infrastucture
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Windows.Controls;
    using System.Windows.Input;
    using DynamicData;
    using DynamicData.Binding;
    using TailBlazer.Domain.Infrastructure;
    using TailBlazer.Views.Tail;
    public enum MouseKeyState
    {
        Down,
        Up
    }
    /// <summary>
    ///     Tail Blazer is fast because it uses true data virtualisation. However this causes a huge headache when trying to copy and paste items which are selected but no longer visible to the clip-board This drawn out and unsophisticated code attempts to deal with that. BTW: I hear you shout this code should be an abstraction but frankly I cannot be bothered (as this is such a specialisation).
    /// </summary>
    public class SelectionMonitor : ISelectionMonitor, IAttachedListBox
    {
        #region Fields
        private readonly IDisposable _cleanUp;
        private readonly SerialDisposable _controlSubscriber = new SerialDisposable();
        private readonly ILogger _logger;
        private readonly ISourceList<LineProxy> _recentlyRemovedFromVisibleRange = new SourceList<LineProxy>();
        private readonly ISchedulerProvider _schedulerProvider;
        private readonly ISourceList<LineProxy> _selected = new SourceList<LineProxy>();
        private bool _isSelecting;
        private LineProxy _lastSelected;
        private ListBox _selector;
        #endregion
        #region Constructors
        public SelectionMonitor(ILogger logger, ISchedulerProvider schedulerProvider)
        {
            this._logger = logger;
            this._schedulerProvider = schedulerProvider;
            this.Selected = this._selected.AsObservableList();
            this._cleanUp = new CompositeDisposable
                (
                 this._selected,
                 this._recentlyRemovedFromVisibleRange,
                 this._controlSubscriber,
                 this.Selected,
                 //keep recent items only up to a certain number
                 this._recentlyRemovedFromVisibleRange.LimitSizeTo(100).Subscribe());
        }
        #endregion
        #region Properties
        public IObservableList<LineProxy> Selected { get; }
        #endregion
        #region Methods
        public void Dispose() { this._cleanUp.Dispose(); }
        public IEnumerable<string> GetSelectedItems() { return this._selected.Items.OrderBy(proxy => proxy.Start).Select(proxy => proxy.Line.Text); }
        public string GetSelectedText() => this.GetSelectedItems().ToDelimited(Environment.NewLine);
        private void ClearAllSelections()
        {
            this._selected.Clear();
            this._recentlyRemovedFromVisibleRange.Clear();
            this._lastSelected = null;
        }
        private void OnSelectedItemsChanged(SelectionChangedEventArgs args)
        {
            //Logic - by default when items scroll out of view they are no longer selected.
            //this is because the panel is virtualised and and automatically unselected due
            //to the control thinking that the item is not longer part of the overall collection
            if (this._isSelecting) return;
            try
            {
                this._isSelecting = true;
                this._selected.Edit
                    (
                     innerList =>
                         {
                             var toAdd = args.AddedItems.OfType<LineProxy>().ToList();
                             //if (toAdd.Count == 0)
                             //{
                             //    ClearAllSelections();
                             //    return;
                             //}

                             //may need to track if last selected is off the page:
                             var isShiftKeyDown = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

                             //get the last item at then end of the list
                             if (!isShiftKeyDown)
                             {
                                 // ClearAllSelections();
                                 //if mouse down, we need to prevent items being cleated
                                 ////add items to list
                                 foreach (var lineProxy in toAdd)
                                 {
                                     if (innerList.Contains(lineProxy)) continue;
                                     this._lastSelected = null;
                                     innerList.Add(lineProxy);
                                 }
                             }
                             else
                             {
                                 if (this._lastSelected == null)
                                 {
                                     foreach (var lineProxy in toAdd)
                                     {
                                         if (innerList.Contains(lineProxy)) continue;
                                         this._lastSelected = lineProxy;
                                         innerList.Add(lineProxy);
                                     }
                                     args.Handled = true;
                                     return;
                                 }

                                 //if shift down we need to override selected and manually select our selves
                                 var last = this._lastSelected.Index;
                                 var allSelectedItems = this._selector.SelectedItems.OfType<LineProxy>().ToArray();
                                 var currentPage = this._selector.Items.OfType<LineProxy>().ToArray();

                                 //1. Determine whether all selected items are on the current page [plus whether last is on the current page]
                                 var allOnCurrentPage = allSelectedItems.Intersect(currentPage).ToArray();
                                 var lastInONcurrentPage = currentPage.Contains(this._lastSelected);
                                 if (lastInONcurrentPage && allOnCurrentPage.Length == allSelectedItems.Length)
                                 {
                                     innerList.Clear();
                                     innerList.AddRange(allSelectedItems);
                                     return;
                                 }
                                 args.Handled = true;
                                 var maxOfRecent = toAdd.Max(lp => lp.Index);
                                 int min;
                                 int max;
                                 if (last < maxOfRecent)
                                 {
                                     min = last;
                                     max = maxOfRecent;
                                 }
                                 else
                                 {
                                     min = maxOfRecent;
                                     max = last;
                                 }

                                 //maintain selection
                                 this._selector.SelectedItems.Clear();
                                 var fromCurrentPage = this._selector.Items.OfType<LineProxy>().Where(lp => lp.Index >= min && lp.Index <= max).ToArray();
                                 var fromPrevious = this._recentlyRemovedFromVisibleRange.Items.Where(lp => lp.Index >= min && lp.Index <= max).ToArray();
                                 this._recentlyRemovedFromVisibleRange.Clear();

                                 //maintain our record
                                 innerList.Clear();
                                 innerList.AddRange(fromCurrentPage);
                                 foreach (var previous in fromPrevious)
                                     if (!innerList.Contains(previous))
                                         innerList.Add(previous);
                             }
                         });
            }
            catch (Exception ex)
            {
                this._logger.Error(ex, "There has been a problem with manual selection");
            }
            finally
            {
                this._isSelecting = false;
            }
        }
        void IAttachedListBox.Receive(ListBox selector)
        {
            this._selector = selector;
            var dataSource = ((ReadOnlyObservableCollection<LineProxy>)selector.ItemsSource).ToObservableChangeSet().ObserveOn(this._schedulerProvider.Background).Publish();

            //Re-select any selected items which are scrolled back into view
            var itemsAdded = dataSource.WhereReasonsAre(ListChangeReason.Add, ListChangeReason.AddRange).Subscribe
                (
                 changes =>
                     {
                         var alreadySelected = this._selected.Items.ToArray();
                         var newItems = changes.Flatten().Select(c => c.Current).ToArray();

                         //var toSelect
                         this._schedulerProvider.MainThread.Schedule
                             (
                              () =>
                                  {
                                      foreach (var item in newItems)
                                          if (alreadySelected.Contains(item) && !this._selector.SelectedItems.Contains(item))
                                              this._selector.SelectedItems.Add(item);
                                  });
                     });

            //monitor items which have moved off screen [store these for off screen multi-select]
            var itemsRemoved = dataSource.WhereReasonsAre(ListChangeReason.Remove, ListChangeReason.RemoveRange, ListChangeReason.Clear).ObserveOn(this._schedulerProvider.Background).Subscribe
                (
                 changes =>
                     {
                         var oldItems = changes.Flatten().Select(c => c.Current).ToArray();
                         //edit ensures items are added in 1 batch
                         this._recentlyRemovedFromVisibleRange.Edit
                             (
                              innerList =>
                                  {
                                      foreach (var item in oldItems.Where(item => !innerList.Contains(item)))
                                          innerList.Add(item);
                                  });
                     });

            //clear selection when the mouse is clicked and no other key is pressed
            var mouseDownHandler = Observable.FromEventPattern<MouseButtonEventHandler, MouseButtonEventArgs>(h => selector.PreviewMouseDown += h, h => selector.PreviewMouseDown -= h).Select(evt => evt.EventArgs).Where(mouseArgs => mouseArgs.ChangedButton == MouseButton.Left).Subscribe
                (
                 mouseArgs =>
                     {
                         var isKeyDown = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) || Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightCtrl);
                         if (!isKeyDown)
                             this.ClearAllSelections();
                     });
            var mouseUpHandler = Observable.FromEventPattern<MouseButtonEventHandler, MouseButtonEventArgs>(h => selector.MouseUp += h, h => selector.MouseUp -= h).Select(x => MouseKeyState.Up);
            var selectedChanged = Observable.FromEventPattern<SelectionChangedEventHandler, SelectionChangedEventArgs>(h => selector.SelectionChanged += h, h => selector.SelectionChanged -= h).Select(evt => evt.EventArgs).Publish();
            var mouseDown = Observable.FromEventPattern<MouseButtonEventHandler, MouseButtonEventArgs>(h => selector.PreviewMouseDown += h, h => selector.PreviewMouseDown -= h).Select(evt => evt.EventArgs);

            ////Handle selecting multiple rows with the mouse
            //// TODO: Scroll up when the mouse it at the top of the screen
            var mouseDragSelector = selectedChanged.CombineLatest(mouseDown, (slct, down) => new { slct, down }).Scan
                (
                 new ImmutableList<LineProxy>(),
                 (state, latest) =>
                     {
                         if (latest.slct.AddedItems.Count != 0)
                             state = state.Add(latest.slct.AddedItems.OfType<LineProxy>().ToList());
                         return state;
                     }).Select(list => list.Data.Distinct().ToArray()).TakeUntil(mouseUpHandler).Repeat().Where(selection => selection.Length > 0).Subscribe
                (
                 selection =>
                     {
                         //DISABLE FOR NOW AT IT IS CAUSING ISSUES WITH MULTI-SELECTION
                         //return;
                         if (this._isSelecting)
                             return;
                         try
                         {
                             this._isSelecting = true;
                             var first = selection.OrderBy(proxy => proxy.Start).First();
                             var last = selection.OrderBy(proxy => proxy.Start).Last();
                             var fromCurrentPage = this._selector.Items.OfType<LineProxy>().Where(lp => lp.Start >= first.Start && lp.Start <= last.Start).ToArray();
                             foreach (var item in fromCurrentPage)
                                 this._selector.SelectedItems.Add(item);
                             this._logger.Debug($"{selection.Length} selected. Page={fromCurrentPage.Length}");
                         }
                         finally
                         {
                             this._isSelecting = false;
                         }
                     });
            var selectionChanged = selectedChanged.Subscribe(this.OnSelectedItemsChanged);
            this._controlSubscriber.Disposable = new CompositeDisposable(mouseDownHandler, mouseDragSelector, selectionChanged, itemsAdded, itemsRemoved, selectedChanged.Connect(), dataSource.Connect());
        }
        #endregion
    }
}