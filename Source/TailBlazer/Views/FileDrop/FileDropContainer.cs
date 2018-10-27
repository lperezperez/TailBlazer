namespace TailBlazer.Views.FileDrop
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    public class FileDropContainer
    {
        #region Constructors
        public FileDropContainer(IEnumerable<string> files)
        {
            if (null == files)
            {
                this.Files = new string[0];
                return;
            }
            this.Files = files.Where(x => null != x).Select(Path.GetFileName).ToArray();
        }
        #endregion
        #region Properties
        public IEnumerable<string> Files { get; }
        #endregion
    }
}