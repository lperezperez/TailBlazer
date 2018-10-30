namespace TailBlazer.Views.WindowManagement
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Input;
    using Dragablz;
    using DynamicData;
    using DynamicData.Binding;
    using Microsoft.Win32;
    using TailBlazer.Domain.Infrastructure;
    using TailBlazer.Infrastucture;
    using TailBlazer.Infrastucture.AppState;
    using TailBlazer.Views.FileDrop;
    using TailBlazer.Views.Options;
    using TailBlazer.Views.Recent;
    using TailBlazer.Views.Tail;
    public class WindowViewModel : AbstractNotifyPropertyChanged, IDisposable, IViewOpener
    {
        #region Fields
        private readonly IDisposable _cleanUp;
        private readonly ILogger _logger;
        private readonly IObjectProvider _objectProvider;
        private readonly ISchedulerProvider _schedulerProvider;
        private readonly IWindowsController _windowsController;
        private bool _isEmpty;
        private bool _menuIsOpen;
        private HeaderedView _selected;
        #endregion
        #region Constructors
        public WindowViewModel(IObjectProvider objectProvider, IWindowFactory windowFactory, ILogger logger, IWindowsController windowsController, RecentFilesViewModel recentFilesViewModel, GeneralOptionsViewModel generalOptionsViewModel, ISchedulerProvider schedulerProvider, IApplicationStatePublisher applicationStatePublisher)
        {
            this._logger = logger;
            this._windowsController = windowsController;
            this.RecentFiles = recentFilesViewModel;
            this.GeneralOptions = generalOptionsViewModel;
            this._schedulerProvider = schedulerProvider;
            this._objectProvider = objectProvider;
            this.InterTabClient = new InterTabClient(windowFactory);
            this.OpenFileCommand = new Command(this.OpenFile);
            this.ShowInGitHubCommand = new Command(() => Process.Start("https://github.com/RolandPheasant"));
            this.ZoomOutCommand = new Command(() => { this.GeneralOptions.Scale = this.GeneralOptions.Scale + 5; });
            this.ZoomInCommand = new Command(() => { this.GeneralOptions.Scale = this.GeneralOptions.Scale - 5; });
            this.CollectMemoryCommand = new Command
                (
                 () =>
                     {
                         //Diagnostics [useful for memory testing]
                         GC.Collect();
                         GC.WaitForPendingFinalizers();
                         GC.Collect();
                     });
            this.ExitCommmand = new Command
                (
                 () =>
                     {
                         applicationStatePublisher.Publish(ApplicationState.ShuttingDown);
                         Application.Current.Shutdown();
                     });
            this.WindowExiting = () => { applicationStatePublisher.Publish(ApplicationState.ShuttingDown); };
            this.Version = $"v{Assembly.GetEntryAssembly().GetName().Version.ToString(3)}";
            var fileDropped = this.DropMonitor.Dropped.Subscribe(this.OpenFile);
            var isEmptyChecker = this.Views.ToObservableChangeSet().ToCollection().Select(items => items.Count).StartWith(0).Select(count => count == 0).LogErrors(logger).Subscribe(isEmpty => this.IsEmpty = isEmpty);
            var openRecent = recentFilesViewModel.OpenFileRequest.LogErrors(logger).Subscribe
                (
                 file =>
                     {
                         this.MenuIsOpen = false;
                         this.OpenFile(file);
                     });
            var selectedChange = this.WhenValueChanged(vm => vm.Selected).Subscribe
                (
                 selected =>
                     {
                         this.Views.Where(hv => !hv.Equals(selected)).Select(hv => hv.Content).OfType<ISelectedAware>().ForEach(selectedAware => selectedAware.IsSelected = false);
                         if (selected?.Content is ISelectedAware currentSelection)
                             currentSelection.IsSelected = true;
                     });
            this._cleanUp = new CompositeDisposable(recentFilesViewModel, isEmptyChecker, fileDropped, this.DropMonitor, openRecent, selectedChange, Disposable.Create(() => { this.Views.Select(vc => vc.Content).OfType<IDisposable>().ForEach(d => d.Dispose()); }));
        }
        #endregion
        #region Properties
        public ItemActionCallback ClosingTabItemHandler => this.ClosingTabItemHandlerImpl;
        public Command CollectMemoryCommand { get; }
        public FileDropMonitor DropMonitor { get; } = new FileDropMonitor();
        public ICommand ExitCommmand { get; }
        public GeneralOptionsViewModel GeneralOptions { get; }
        public IInterTabClient InterTabClient { get; }
        public bool IsEmpty { get => this._isEmpty; set => this.SetAndRaise(ref this._isEmpty, value); }
        public bool MenuIsOpen { get => this._menuIsOpen; set => this.SetAndRaise(ref this._menuIsOpen, value); }
        public ICommand OpenFileCommand { get; }
        public RecentFilesViewModel RecentFiles { get; }
        public HeaderedView Selected { get => this._selected; set => this.SetAndRaise(ref this._selected, value); }
        public Command ShowInGitHubCommand { get; }
        public string Version { get; }
        public ObservableCollection<HeaderedView> Views { get; } = new ObservableCollection<HeaderedView>();
        public ApplicationExitingDelegate WindowExiting { get; }
        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }
        #endregion
        #region Methods
        public void Dispose() { this._cleanUp.Dispose(); }
        public void OnWindowClosing()
        {
            this._logger.Info("Window is closing. {0} view to close", this.Views.Count);
            this.Views.ForEach(v => this._windowsController.Remove(v));
            this.Views.Select(vc => vc.Content).OfType<IDisposable>().ForEach(x => x.Dispose());
        }
        public void OpenFiles(IEnumerable<string> files = null)
        {
            if (files == null) return;
            foreach (var file in files)
                this.OpenFile(new FileInfo(file));
        }

        //TODO: Abstract this
        public void OpenView(HeaderedView headeredView)
        {
            this._schedulerProvider.Background.Schedule
                (
                 () =>
                     {
                         try
                         {
                             this._logger.Info($"Attempting to open a restored view {headeredView.Header}");
                             //var HeaderedView = new HeaderedView(view);

                             //TODO: Factory should create the HeaderedView
                             this._windowsController.Register(headeredView);

                             //do the work on the ui thread
                             this._schedulerProvider.MainThread.Schedule
                                 (
                                  () =>
                                      {
                                          this.Views.Add(headeredView);
                                          if (this.Selected == null)
                                              this.Selected = headeredView;
                                      });
                         }
                         catch (Exception ex)
                         {
                             //TODO: Create a failed to load view
                             this._logger.Error(ex, $"There was a problem opening '{headeredView.Header}'");
                         }
                     });
        }
        private void ClosingTabItemHandlerImpl(ItemActionCallbackArgs<TabablzControl> args)
        {
            this._logger.Info("Tab is closing. {0} view to close", this.Views.Count);
            var container = (HeaderedView)args.DragablzItem.DataContext;
            this._windowsController.Remove(container);
            if (container.Equals(this.Selected))
                this.Selected = this.Views.FirstOrDefault(vc => vc != container);
            var disposable = container.Content as IDisposable;
            disposable?.Dispose();
        }
        private void OpenFile()
        {
            // open dialog to select file [get rid of this shit and create a material design file selector]
            var dialog = new OpenFileDialog { Filter = "All files (*.*)|*.*" };
            var result = dialog.ShowDialog();
            if (result != true) return;
            this.OpenFile(new FileInfo(dialog.FileName));
        }
        private void OpenFile(FileInfo file)
        {
            this._schedulerProvider.Background.Schedule
                (
                 () =>
                     {
                         try
                         {
                             this._logger.Info($"Attempting to open '{file.FullName}'");
                             this.RecentFiles.Add(file);

                             //1. resolve TailViewModel
                             var factory = this._objectProvider.Get<TailViewModelFactory>();
                             var newItem = factory.Create(file);

                             //2. Display it
                             this._windowsController.Register(newItem);
                             this._logger.Info($"Objects for '{file.FullName}' has been created.");
                             //do the work on the ui thread
                             this._schedulerProvider.MainThread.Schedule
                                 (
                                  () =>
                                      {
                                          this.Views.Add(newItem);
                                          this._logger.Info($"Opened '{file.FullName}'");
                                          this.Selected = newItem;
                                      });
                         }
                         catch (Exception ex)
                         {
                             //TODO: Create a failed to load view
                             this._logger.Error(ex, $"There was a problem opening '{file.FullName}'");
                         }
                     });
        }
        #endregion
    }
}