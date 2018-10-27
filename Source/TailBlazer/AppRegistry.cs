namespace TailBlazer.Infrastucture
{
    using System;
    using System.IO;
    using System.Text;
    using log4net.Config;
    using StructureMap;
    using TailBlazer.Domain.FileHandling;
    using TailBlazer.Domain.FileHandling.Search;
    using TailBlazer.Domain.Formatting;
    using TailBlazer.Domain.Infrastructure;
    using TailBlazer.Domain.Settings;
    using TailBlazer.Infrastucture.AppState;
    using TailBlazer.Infrastucture.KeyboardNavigation;
    using TailBlazer.Properties;
    using TailBlazer.Views.Options;
    using TailBlazer.Views.Tail;
    internal class AppRegistry : Registry
    {
        #region Constructors
        public AppRegistry()
        {
            //set up logging
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config");
            if (!File.Exists(path))
                using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(Resources.log4net)))
                    XmlConfigurator.Configure(stream);
            else
                XmlConfigurator.ConfigureAndWatch(new FileInfo(path));
            this.For<ILogger>().Use<Log4NetLogger>().Ctor<Type>("type").Is(x => x.ParentType).AlwaysUnique();
            this.For<ISelectionMonitor>().Use<SelectionMonitor>();
            this.For<ISearchInfoCollection>().Use<SearchInfoCollection>();
            this.For<ISearchMetadataCollection>().Use<SearchMetadataCollection>().Transient();
            this.For<ICombinedSearchMetadataCollection>().Use<CombinedSearchMetadataCollection>().Transient();
            this.For<ITextFormatter>().Use<TextFormatter>().Transient();
            this.For<ILineMatches>().Use<LineMatches>();
            this.For<ISettingsStore>().Use<FileSettingsStore>().Singleton();
            this.For<IFileWatcher>().Use<FileWatcher>();
            this.For<GeneralOptionsViewModel>().Singleton();
            this.For<UhandledExceptionHandler>().Singleton();
            this.For<ObjectProvider>().Singleton();
            this.Forward<ObjectProvider, IObjectProvider>();
            this.Forward<ObjectProvider, IObjectRegister>();
            this.For<ViewFactoryService>().Singleton();
            this.Forward<ViewFactoryService, IViewFactoryRegister>();
            this.Forward<ViewFactoryService, IViewFactoryProvider>();
            this.For<ApplicationStateBroker>().Singleton();
            this.Forward<ApplicationStateBroker, IApplicationStateNotifier>();
            this.Forward<ApplicationStateBroker, IApplicationStatePublisher>();
            this.For<TailViewModelFactory>().Singleton();
            this.For<IKeyboardNavigationHandler>().Use<KeyboardNavigationHandler>();
            this.Scan
                (
                 scanner =>
                     {
                         scanner.ExcludeType<ILogger>();

                         //to do, need a auto-exclude these from AppConventions
                         scanner.ExcludeType<SelectionMonitor>();
                         scanner.ExcludeType<SearchInfoCollection>();
                         scanner.ExcludeType<SearchMetadataCollection>();
                         scanner.ExcludeType<CombinedSearchMetadataCollection>();
                         scanner.ExcludeType<TextFormatter>();
                         scanner.ExcludeType<LineMatches>();
                         scanner.ExcludeType<ViewFactoryService>();
                         scanner.ExcludeType<FileWatcher>();
                         scanner.LookForRegistries();
                         scanner.Convention<AppConventions>();
                         scanner.AssemblyContainingType<ILogFactory>();
                         scanner.AssemblyContainingType<AppRegistry>();
                     });
        }
        #endregion
    }
}