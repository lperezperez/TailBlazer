namespace TailBlazer.Views.Tail
{
    using System;
    using System.Collections.ObjectModel;
    using System.Windows.Input;
    using TailBlazer.Domain.Infrastructure;
    using TailBlazer.Infrastucture;
    using TailBlazer.Infrastucture.Virtualisation;
    public interface ILinesVisualisation : IScrollReceiver, IDisposable
    {
        #region Properties
        ICommand CopyToClipboardCommand { get; }
        IProperty<int> Count { get; }
        int FirstIndex { get; set; }
        TextScrollDelegate HorizonalScrollChanged { get; }
        ReadOnlyObservableCollection<LineProxy> Lines { get; }
        IProperty<int> MaximumChars { get; }
        int PageSize { get; set; }
        ISelectionMonitor SelectionMonitor { get; }
        #endregion
    }
}