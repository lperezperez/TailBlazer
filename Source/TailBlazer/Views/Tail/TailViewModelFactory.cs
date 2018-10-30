namespace TailBlazer.Views.Tail
{
    using System;
    using System.IO;
    using System.Reactive.Concurrency;
    using TailBlazer.Domain.Annotations;
    using TailBlazer.Domain.FileHandling;
    using TailBlazer.Domain.Infrastructure;
    using TailBlazer.Domain.Settings;
    using TailBlazer.Infrastucture;
    public class TailViewModelFactory : IViewModelFactory
    {
        #region Fields
        private readonly IObjectProvider _objectProvider;
        private readonly ISchedulerProvider _schedulerProvider;
        #endregion
        #region Constructors
        public TailViewModelFactory([NotNull] IObjectProvider objectProvider, [NotNull] ISchedulerProvider schedulerProvider)
        {
            this._objectProvider = objectProvider ?? throw new ArgumentNullException(nameof(objectProvider));
            this._schedulerProvider = schedulerProvider ?? throw new ArgumentNullException(nameof(schedulerProvider));
        }
        #endregion
        #region Properties
        public string Key => TailViewModelConstants.ViewKey;
        #endregion
        #region Methods
        public HeaderedView Create(ViewState state)
        {
            var converter = new TailViewToStateConverter();
            var converted = converter.Convert(state.State);
            var file = converted.FileName;
            var viewModel = this.CreateView(new FileInfo(file));
            var restorer = (IPersistentView)viewModel;
            restorer.Restore(state);
            return new HeaderedView(new FileHeader(new FileInfo(file)), viewModel);
        }
        public HeaderedView Create(FileInfo fileInfo)
        {
            var viewModel = this.CreateView(fileInfo);
            viewModel.ApplySettings(); //apply default values
            return new HeaderedView(new FileHeader(fileInfo), viewModel);
        }
        private TailViewModel CreateView(FileInfo fileInfo)
        {
            if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));
            var fileWatcher = this._objectProvider.Get<IFileWatcher>(new IArgument[] { new Argument<FileInfo>(fileInfo), new Argument<IScheduler>(this._schedulerProvider.Background) });

            // var combined = _objectProvider.Get<ICombinedSearchMetadataCollection>();
            var args = new IArgument[]
                           {
                               new Argument<IFileWatcher>(fileWatcher)
                               // new Argument<ICombinedSearchMetadataCollection>(combined)
                           };
            return this._objectProvider.Get<TailViewModel>(args);
        }
        #endregion
    }
}