namespace TailBlazer.Infrastucture
{
    using System;
    using TailBlazer.Domain.Infrastructure;
    public class LogFactory : ILogFactory
    {
        #region Methods
        public ILogger Create(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return new Log4NetLogger(name);
        }
        public ILogger Create<T>() => new Log4NetLogger(typeof(T));
        #endregion
    }
}