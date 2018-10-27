namespace TailBlazer.Views.WindowManagement
{
    using System;
    using System.Collections;
    using System.Linq;
    using DynamicData;
    using TailBlazer.Infrastucture;
    public class FileHeaderNamingJob : IDisposable
    {
        #region Fields
        private readonly IDisposable _cleanUp;
        #endregion
        #region Constructors
        public FileHeaderNamingJob(IWindowsController windowsController)
        {
            if (windowsController == null) throw new ArgumentNullException(nameof(windowsController));
            this._cleanUp = windowsController.Views.Connect(vc => vc.Header is FileHeader).Transform(vc => (FileHeader)vc.Header).ToCollection().Subscribe
                (
                 files =>
                     {
                         var renamer = new FileNamer(files.Select(f => f.FullName));
                         files.Select(page => new { Item = page, Label = renamer.GetName(page.FullName) }).ForEach(x => x.Item.DisplayName = x.Label);
                     });
        }
        #endregion
        #region Methods
        public void Dispose() { this._cleanUp.Dispose(); }
        #endregion
    }
}