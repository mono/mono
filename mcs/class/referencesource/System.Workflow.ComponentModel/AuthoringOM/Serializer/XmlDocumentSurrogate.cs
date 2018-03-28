namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Xml;
    using System.Runtime.Serialization;
    using System.Reflection;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    #region XmlDocumentSurrogate
    internal sealed class XmlDocumentSurrogate : ISerializationSurrogate
    {
        internal XmlDocumentSurrogate() { }
        void ISerializationSurrogate.GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            XmlDocument doc = obj as XmlDocument;
            if (doc == null)
                throw new ArgumentException(SR.GetString(SR.Error_InvalidArgumentValue), "obj");

            info.AddValue("innerXml", doc.InnerXml);
            info.SetType(typeof(XmlDocumentReference));
        }
        object ISerializationSurrogate.SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            return null;
        }

        #region XmlDocumentReference
        [Serializable]
        private sealed class XmlDocumentReference : IObjectReference
        {
            private string innerXml = string.Empty;

            Object IObjectReference.GetRealObject(StreamingContext context)
            {
                XmlDocument doc = new XmlDocument();

                if (!string.IsNullOrEmpty(this.innerXml))
                    doc.InnerXml = this.innerXml;

                return doc;
            }
        }
        #endregion
    }
    #endregion
}
