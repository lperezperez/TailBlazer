namespace TailBlazer.Infrastucture
{
    using System.Collections;
    using System.Collections.Generic;
    using StructureMap;
    using StructureMap.Pipeline;
    using TailBlazer.Domain.Infrastructure;
    public class ObjectProvider : IObjectProvider, IObjectRegister
    {
        #region Fields
        private readonly IContainer _container;
        #endregion
        #region Constructors
        public ObjectProvider(IContainer container) { this._container = container; }
        #endregion
        #region Methods
        public T Get<T>() => this._container.GetInstance<T>();
        public T Get<T>(NamedArgument namedArgument) => this.Get<T>(namedArgument.YieldOne());
        public T Get<T>(IEnumerable<NamedArgument> explictArgs)
        {
            var args = new ExplicitArguments();
            foreach (var explictArg in explictArgs)
                args.SetArg(explictArg.Key, explictArg.Instance);
            return this._container.GetInstance<T>(args);
        }
        public T Get<T>(IArgument arg) => this.Get<T>(arg.YieldOne());
        public T Get<T>(IEnumerable<IArgument> args)
        {
            return this._container.With
                (
                 x =>
                     {
                         foreach (var parameter in args)
                             x.With(parameter.TargetType, parameter.Value);
                     }).GetInstance<T>();
        }
        public void Register<T>(T instance)
            where T : class
        {
            this._container.Configure(x => x.For<T>().Use(instance));
        }
        #endregion
    }
}