//------------------------------------------------------------------------------
// <copyright file="WsRuntime.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System.Web.Services;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using System;
    using System.Text;
    using System.IO;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web.Services.Diagnostics;

    internal class RuntimeUtils {
        private RuntimeUtils() { }

        internal static XmlDeserializationEvents GetDeserializationEvents() {
            XmlDeserializationEvents events = new XmlDeserializationEvents();
            events.OnUnknownElement = new XmlElementEventHandler(OnUnknownElement);
            events.OnUnknownAttribute = new XmlAttributeEventHandler(OnUnknownAttribute);
            return events;
        }

        static void OnUnknownAttribute(object sender, XmlAttributeEventArgs e) {
            if (e.Attr == null)
                return;
            // ignore attributes from known namepsaces
            if (IsKnownNamespace(e.Attr.NamespaceURI))
                return;
            Tracing.OnUnknownAttribute(sender, e);
            if (e.ExpectedAttributes == null)
                throw new InvalidOperationException(Res.GetString(Res.WebUnknownAttribute, e.Attr.Name, e.Attr.Value));
            else if (e.ExpectedAttributes.Length == 0)
                throw new InvalidOperationException(Res.GetString(Res.WebUnknownAttribute2, e.Attr.Name, e.Attr.Value));
            else
                throw new InvalidOperationException(Res.GetString(Res.WebUnknownAttribute3, e.Attr.Name, e.Attr.Value, e.ExpectedAttributes));
        }

        internal static string ElementString(XmlElement element) {
            StringWriter xml = new StringWriter(CultureInfo.InvariantCulture);
            xml.Write("<");
            xml.Write(element.Name);
            if (element.NamespaceURI != null && element.NamespaceURI.Length > 0) {
                xml.Write(" xmlns");
                if (element.Prefix != null && element.Prefix.Length > 0) {
                    xml.Write(":");
                    xml.Write(element.Prefix);
                }
                xml.Write("='");
                xml.Write(element.NamespaceURI);
                xml.Write("'");
            }
            xml.Write(">..</");
            xml.Write(element.Name);
            xml.Write(">");

            return xml.ToString();
        }

        internal static void OnUnknownElement(object sender, XmlElementEventArgs e) {
            if (e.Element == null)
                return;
            string xml = RuntimeUtils.ElementString(e.Element);
            Tracing.OnUnknownElement(sender, e);
            if (e.ExpectedElements == null)
                throw new InvalidOperationException(Res.GetString(Res.WebUnknownElement, xml));
            else if (e.ExpectedElements.Length == 0)
                throw new InvalidOperationException(Res.GetString(Res.WebUnknownElement1, xml));
            else
                throw new InvalidOperationException(Res.GetString(Res.WebUnknownElement2, xml, e.ExpectedElements));
        }

        internal static bool IsKnownNamespace(string ns) {
            if (ns == XmlSchema.InstanceNamespace)
                return true;
            if (ns == Soap.XmlNamespace)
                return true;
            if (ns == Soap.Encoding || ns == Soap.Namespace)
                return true;
            if (ns == Soap12.Namespace || ns == Soap12.Encoding || ns == Soap12.RpcNamespace)
                return true;
            return false;
        }

        internal static string EscapeUri(Uri uri) {
            if (null == uri) {
                throw new ArgumentNullException("uri");
            }

            return uri.GetComponents(UriComponents.SerializationInfoString, UriFormat.UriEscaped).Replace("#", "%23");
        }
    }
}
