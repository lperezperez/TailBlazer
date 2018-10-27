namespace TailBlazer.Infrastucture
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Controls;
    using DynamicData;
    using TailBlazer.Views.Tail;
    public interface IAttachedListBox
    {
        #region Methods
        void Receive(ListBox selector);
        #endregion
    }
    public interface ISelectionMonitor : IDisposable
    {
        #region Properties
        IObservableList<LineProxy> Selected { get; }
        #endregion
        #region Methods
        IEnumerable<string> GetSelectedItems();
        string GetSelectedText();
        #endregion
    }
}