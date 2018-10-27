﻿namespace TailBlazer
{
    using System;
    using System.Reactive.Concurrency;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Threading;
    using StructureMap;
    using TailBlazer.Domain.Infrastructure;
    using TailBlazer.Infrastucture;
    using TailBlazer.Infrastucture.AppState;
    using TailBlazer.Views.Layout;
    using TailBlazer.Views.WindowManagement;
    public class BootStrap
    {
        #region Methods
        [STAThread]
        public static void Main(string[] args)
        {
            BootStrap.FreeConsole();
            var app = new App { ShutdownMode = ShutdownMode.OnLastWindowClose };
            app.InitializeComponent();
            var tempWindowToGetDispatcher = new Window();
            var container = new Container(x => x.AddRegistry<AppRegistry>());
            container.Configure(x => x.For<Dispatcher>().Add(tempWindowToGetDispatcher.Dispatcher));
            container.GetInstance<StartupController>();
            var factory = container.GetInstance<WindowFactory>();
            var window = factory.Create(args);
            tempWindowToGetDispatcher.Close();
            var layoutServce = container.GetInstance<ILayoutService>();
            var scheduler = container.GetInstance<ISchedulerProvider>();
            scheduler.MainThread.Schedule(window.Show);
            var appStatePublisher = container.GetInstance<IApplicationStatePublisher>();
            app.Exit += (sender, e) => appStatePublisher.Publish(ApplicationState.ShuttingDown);
            app.Run();
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeConsole();
        #endregion
    }
}