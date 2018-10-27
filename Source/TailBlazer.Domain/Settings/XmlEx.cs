namespace TailBlazer.Domain.Settings
{
    using System;
    using System.Reactive.Disposables;
    using System.Xml;
    using System.Xml.Linq;
    using DynamicData.Kernel;
    public static class XmlEx
    {
        #region Methods
        public static string AttributeOrThrow(this XElement source, string elementName)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (elementName == null) throw new ArgumentNullException(nameof(elementName));
            var element = source.Attribute(elementName);
            if (element == null)
                throw new ArgumentNullException($"{elementName} does not exist");
            return element.Value;
        }
        public static XElement ElementOrThrow(this XDocument source, string elementName)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (elementName == null) throw new ArgumentNullException(nameof(elementName));
            var element = source.Element(elementName);
            if (element == null)
                throw new ArgumentNullException($"{elementName} does not exist");
            return element;
        }
        public static string ElementOrThrow(this XElement source, string elementName)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (elementName == null) throw new ArgumentNullException(nameof(elementName));
            var element = source.Element(elementName);
            if (element == null)
                throw new ArgumentNullException($"{elementName} does not exist");
            return element.Value;
        }
        public static Optional<string> OptionalElement(this XElement source, string elementName)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (elementName == null) throw new ArgumentNullException(nameof(elementName));
            var element = source.Element(elementName);
            return element?.Value ?? Optional<string>.None;
        }
        public static IDisposable WriteElement(this XmlTextWriter source, string elementName)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (elementName == null) throw new ArgumentNullException(nameof(elementName));
            source.WriteStartElement(elementName);
            return Disposable.Create(source.WriteEndElement);
        }
        #endregion
    }
}