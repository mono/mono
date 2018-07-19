//------------------------------------------------------------------------------
// <copyright file="SoapHeader.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Protocols {
    using System.Web.Services;
    using System.Xml.Serialization;
    using System;
    using System.Reflection;
    using System.Xml;
    using System.Collections;
    using System.IO;
    using System.ComponentModel;
    using System.Threading;
    using System.Security.Permissions;
    using System.Runtime.InteropServices;
    using System.Web.Services.Diagnostics;

    /// <include file='doc\SoapHeader.uex' path='docs/doc[@for="SoapHeader"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [XmlType(IncludeInSchema = false), SoapType(IncludeInSchema = false)]
    public abstract class SoapHeader {
        string actor;
        bool mustUnderstand;
        bool didUnderstand;
        bool relay;
        // prop getters should return a value when version == Default or when version == correctVersion.
        // all version tests in getters should use != incorrectVersion
        internal SoapProtocolVersion version = SoapProtocolVersion.Default;

        /// <include file='doc\SoapHeader.uex' path='docs/doc[@for="SoapHeader.MustUnderstandEncoded"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("mustUnderstand", Namespace = "http://schemas.xmlsoap.org/soap/envelope/"),
        SoapAttribute("mustUnderstand", Namespace = "http://schemas.xmlsoap.org/soap/envelope/"),
        DefaultValue("0")]
        public string EncodedMustUnderstand {
            get { return version != SoapProtocolVersion.Soap12 && MustUnderstand ? "1" : "0"; }
            set {
                switch (value) {
                    case "false":
                    case "0": MustUnderstand = false; break;
                    case "true":
                    case "1": MustUnderstand = true; break;
                    default: throw new ArgumentException(Res.GetString(Res.WebHeaderInvalidMustUnderstand, value));
                }
            }
        }

        /// <include file='doc\SoapHeader.uex' path='docs/doc[@for="SoapHeader.EncodedMustUnderstand12"]/*' />
        [XmlAttribute("mustUnderstand", Namespace = Soap12.Namespace),
        SoapAttribute("mustUnderstand", Namespace = Soap12.Namespace),
        DefaultValue("0"),
        ComVisible(false)]
        public string EncodedMustUnderstand12 {
            get { return version != SoapProtocolVersion.Soap11 && MustUnderstand ? "1" : "0"; }
            set {
                EncodedMustUnderstand = value;
            }
        }

        /// <include file='doc\SoapHeader.uex' path='docs/doc[@for="SoapHeader.MustUnderstand"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore, SoapIgnore]
        public bool MustUnderstand {
            get { return InternalMustUnderstand; }
            set { InternalMustUnderstand = value; }
        }

        internal virtual bool InternalMustUnderstand {
            [PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
            get { return mustUnderstand; }
            [PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
            set { mustUnderstand = value; }
        }

        /// <include file='doc\SoapHeader.uex' path='docs/doc[@for="SoapHeader.Actor"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlAttribute("actor", Namespace = "http://schemas.xmlsoap.org/soap/envelope/"),
        SoapAttribute("actor", Namespace = "http://schemas.xmlsoap.org/soap/envelope/"),
        DefaultValue("")]
        public string Actor {
            get { return version != SoapProtocolVersion.Soap12 ? InternalActor : ""; }
            set { InternalActor = value; }
        }

        /// <include file='doc\SoapHeader.uex' path='docs/doc[@for="SoapHeader.Role"]/*' />
        [XmlAttribute("role", Namespace = Soap12.Namespace),
        SoapAttribute("role", Namespace = Soap12.Namespace),
        DefaultValue(""),
        ComVisible(false)]
        public string Role {
            get { return version != SoapProtocolVersion.Soap11 ? InternalActor : ""; }
            set { InternalActor = value; }
        }

        internal virtual string InternalActor {
            [PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
            get { return actor == null ? string.Empty : actor; }
            [PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
            set { actor = value; }
        }

        /// <include file='doc\SoapHeader.uex' path='docs/doc[@for="SoapHeader.DidUnderstand"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [XmlIgnore, SoapIgnore]
        public bool DidUnderstand {
            get { return didUnderstand; }
            set { didUnderstand = value; }
        }

        /// <include file='doc\SoapHeader.uex' path='docs/doc[@for="SoapHeader.EncodedRelay"]/*' />
        [XmlAttribute("relay", Namespace = Soap12.Namespace),
        SoapAttribute("relay", Namespace = Soap12.Namespace),
        DefaultValue("0"),
        ComVisible(false)]
        public string EncodedRelay {
            get { return version != SoapProtocolVersion.Soap11 && Relay ? "1" : "0"; }
            set {
                switch (value) {
                    case "false":
                    case "0": Relay = false; break;
                    case "true":
                    case "1": Relay = true; break;
                    default: throw new ArgumentException(Res.GetString(Res.WebHeaderInvalidRelay, value));
                }
            }
        }

        /// <include file='doc\SoapHeader.uex' path='docs/doc[@for="SoapHeader.Relay"]/*' />
        [XmlIgnore, SoapIgnore, ComVisible(false)]
        public bool Relay {
            get { return InternalRelay; }
            set { InternalRelay = value; }
        }

        internal virtual bool InternalRelay {
            [PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
            get { return relay; }
            [PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
            set { relay = value; }
        }
    }

    [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
    public sealed class SoapHeaderMapping {
        //
        // Block external construction
        //
        internal SoapHeaderMapping() {
        }

        internal Type headerType;
        internal bool repeats;
        internal bool custom;
        internal SoapHeaderDirection direction;
        internal MemberInfo memberInfo;

        public Type HeaderType {
            get {
                return headerType;
            }
        }

        public bool Repeats {
            get {
                return repeats;
            }
        }

        public bool Custom {
            get {
                return custom;
            }
        }

        public SoapHeaderDirection Direction {
            get {
                return direction;
            }
        }

        public MemberInfo MemberInfo {
            get {
                return memberInfo;
            }
        }
    }

    [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
    public sealed class SoapHeaderHandling {
        SoapHeaderCollection unknownHeaders;
        SoapHeaderCollection unreferencedHeaders;
        int currentThread;
        string envelopeNS;

        void OnUnknownElement(object sender, XmlElementEventArgs e) {
            if (Thread.CurrentThread.GetHashCode() != this.currentThread) return;
            if (e.Element == null) return;
            SoapUnknownHeader header = new SoapUnknownHeader();
            header.Element = e.Element;
            unknownHeaders.Add(header);
        }

        void OnUnreferencedObject(object sender, UnreferencedObjectEventArgs e) {
            if (Thread.CurrentThread.GetHashCode() != this.currentThread) return;
            object o = e.UnreferencedObject;
            if (o == null) return;
            if (typeof(SoapHeader).IsAssignableFrom(o.GetType())) {
                unreferencedHeaders.Add((SoapHeader)o);
            }
        }

        // return first missing header name;
        public string ReadHeaders(XmlReader reader, XmlSerializer serializer, SoapHeaderCollection headers, SoapHeaderMapping[] mappings, SoapHeaderDirection direction, string envelopeNS, string encodingStyle, bool checkRequiredHeaders) {
            string missingHeader = null;
            reader.MoveToContent();
            if (!reader.IsStartElement(Soap.Element.Header, envelopeNS)) {
                if (checkRequiredHeaders && mappings != null && mappings.Length > 0)
                    missingHeader = GetHeaderElementName(mappings[0].headerType);
                return missingHeader;
            }
            if (reader.IsEmptyElement) { reader.Skip(); return missingHeader; }

            this.unknownHeaders = new SoapHeaderCollection();
            this.unreferencedHeaders = new SoapHeaderCollection();
            // thread hash code is used to differentiate between deserializations in event callbacks
            this.currentThread = Thread.CurrentThread.GetHashCode();
            this.envelopeNS = envelopeNS;

            int depth = reader.Depth;
            reader.ReadStartElement();
            reader.MoveToContent();

            XmlDeserializationEvents events = new XmlDeserializationEvents();
            events.OnUnknownElement = new XmlElementEventHandler(this.OnUnknownElement);
            events.OnUnreferencedObject = new UnreferencedObjectEventHandler(this.OnUnreferencedObject);

            TraceMethod caller = Tracing.On ? new TraceMethod(this, "ReadHeaders") : null;
            if (Tracing.On) Tracing.Enter(Tracing.TraceId(Res.TraceReadHeaders), caller, new TraceMethod(serializer, "Deserialize", reader, encodingStyle));
            object[] headerValues = (object[])serializer.Deserialize(reader, encodingStyle, events);
            if (Tracing.On) Tracing.Exit(Tracing.TraceId(Res.TraceReadHeaders), caller);
            for (int i = 0; i < headerValues.Length; i++) {
                if (headerValues[i] != null) {
                    SoapHeader header = (SoapHeader)headerValues[i];
                    header.DidUnderstand = true;
                    headers.Add(header);
                }
                else if (checkRequiredHeaders) {
                    // run time check for R2738 A MESSAGE MUST include all soapbind:headers specified on a wsdl:input or wsdl:output of a wsdl:operationwsdl:binding that describes it. 
                    if (missingHeader == null)
                        missingHeader = GetHeaderElementName(mappings[i].headerType);
                }
            }
            this.currentThread = 0;
            this.envelopeNS = null;

            foreach (SoapHeader header in this.unreferencedHeaders) {
                headers.Add(header);
            }
            this.unreferencedHeaders = null;

            foreach (SoapHeader header in this.unknownHeaders) {
                headers.Add(header);
            }
            this.unknownHeaders = null;

            // Consume soap:Body and soap:Envelope closing tags
            while (depth < reader.Depth && reader.Read()) {
                // Nothing, just read on
            }
            // consume end tag
            if (reader.NodeType == XmlNodeType.EndElement) {
                reader.Read();
            }

            return missingHeader;
        }

        public static void WriteHeaders(XmlWriter writer, XmlSerializer serializer, SoapHeaderCollection headers, SoapHeaderMapping[] mappings, SoapHeaderDirection direction, bool isEncoded, string defaultNS, bool serviceDefaultIsEncoded, string envelopeNS) {
            if (headers.Count == 0) return;
            writer.WriteStartElement(Soap.Element.Header, envelopeNS);
            SoapProtocolVersion version;
            string encodingStyle;
            if (envelopeNS == Soap12.Namespace) {
                version = SoapProtocolVersion.Soap12;
                encodingStyle = Soap12.Encoding;
            }
            else {
                version = SoapProtocolVersion.Soap11;
                encodingStyle = Soap.Encoding;
            }

            int unknownHeaderCount = 0;
            ArrayList otherHeaders = new ArrayList();
            SoapHeader[] headerArray = new SoapHeader[mappings.Length];
            bool[] headerSet = new bool[headerArray.Length];
            for (int i = 0; i < headers.Count; i++) {
                SoapHeader header = headers[i];
                if (header == null) continue;
                int headerPosition;
                header.version = version;
                if (header is SoapUnknownHeader) {
                    otherHeaders.Add(header);
                    unknownHeaderCount++;
                }
                else if ((headerPosition = FindMapping(mappings, header, direction)) >= 0 && !headerSet[headerPosition]) {
                    headerArray[headerPosition] = header;
                    headerSet[headerPosition] = true;
                }
                else {
                    otherHeaders.Add(header);
                }
            }
            int otherHeaderCount = otherHeaders.Count - unknownHeaderCount;
            if (isEncoded && otherHeaderCount > 0) {
                SoapHeader[] newHeaderArray = new SoapHeader[mappings.Length + otherHeaderCount];
                headerArray.CopyTo(newHeaderArray, 0);

                // fill in the non-statically known headers (otherHeaders) starting after the statically-known ones
                int count = mappings.Length;
                for (int i = 0; i < otherHeaders.Count; i++) {
                    if (!(otherHeaders[i] is SoapUnknownHeader))
                        newHeaderArray[count++] = (SoapHeader)otherHeaders[i];
                }

                headerArray = newHeaderArray;
            }

            TraceMethod caller = Tracing.On ? new TraceMethod(typeof(SoapHeaderHandling), "WriteHeaders") : null;
            if (Tracing.On) Tracing.Enter(Tracing.TraceId(Res.TraceWriteHeaders), caller, new TraceMethod(serializer, "Serialize", writer, headerArray, null, isEncoded ? encodingStyle : null, "h_"));
            serializer.Serialize(writer, headerArray, null, isEncoded ? encodingStyle : null, "h_");
            if (Tracing.On) Tracing.Exit(Tracing.TraceId(Res.TraceWriteHeaders), caller);

            foreach (SoapHeader header in otherHeaders) {
                if (header is SoapUnknownHeader) {
                    SoapUnknownHeader unknown = (SoapUnknownHeader)header;
                    if (unknown.Element != null)
                        unknown.Element.WriteTo(writer);
                }
                else if (!isEncoded) { // encoded headers already appended to members mapping
                    string ns = SoapReflector.GetLiteralNamespace(defaultNS, serviceDefaultIsEncoded);
                    XmlSerializer headerSerializer = new XmlSerializer(header.GetType(), ns);

                    if (Tracing.On) Tracing.Enter(Tracing.TraceId(Res.TraceWriteHeaders), caller, new TraceMethod(headerSerializer, "Serialize", writer, header));
                    headerSerializer.Serialize(writer, header);
                    if (Tracing.On) Tracing.Exit(Tracing.TraceId(Res.TraceWriteHeaders), caller);
                }
            }

            // reset the soap version
            for (int i = 0; i < headers.Count; i++) {
                SoapHeader header = headers[i];
                if (header != null)
                    header.version = SoapProtocolVersion.Default;
            }

            writer.WriteEndElement();
            writer.Flush();
        }

        public static void WriteUnknownHeaders(XmlWriter writer, SoapHeaderCollection headers, string envelopeNS) {
            bool first = true;
            foreach (SoapHeader header in headers) {
                SoapUnknownHeader unknown = header as SoapUnknownHeader;
                if (unknown != null) {
                    if (first) {
                        writer.WriteStartElement(Soap.Element.Header, envelopeNS);
                        first = false;
                    }
                    if (unknown.Element != null)
                        unknown.Element.WriteTo(writer);
                }
            }
            if (!first)
                writer.WriteEndElement(); // </soap:Header>
        }

        public static void SetHeaderMembers(SoapHeaderCollection headers, object target, SoapHeaderMapping[] mappings, SoapHeaderDirection direction, bool client) {
            bool[] headerHandled = new bool[headers.Count];
            if (mappings != null) {
                for (int i = 0; i < mappings.Length; i++) {
                    SoapHeaderMapping mapping = mappings[i];
                    if ((mapping.direction & direction) == 0) continue;
                    if (mapping.repeats) {
                        ArrayList list = new ArrayList();
                        for (int j = 0; j < headers.Count; j++) {
                            SoapHeader header = headers[j];
                            if (headerHandled[j]) continue;
                            if (mapping.headerType.IsAssignableFrom(header.GetType())) {
                                list.Add(header);
                                headerHandled[j] = true;
                            }
                        }
                        MemberHelper.SetValue(mapping.memberInfo, target, list.ToArray(mapping.headerType));
                    }
                    else {
                        bool handled = false;
                        for (int j = 0; j < headers.Count; j++) {
                            SoapHeader header = headers[j];
                            if (headerHandled[j]) continue;
                            if (mapping.headerType.IsAssignableFrom(header.GetType())) {
                                if (handled) {
                                    header.DidUnderstand = false;
                                    continue;
                                }
                                handled = true;
                                MemberHelper.SetValue(mapping.memberInfo, target, header);
                                headerHandled[j] = true;
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < headerHandled.Length; i++) {
                if (!headerHandled[i]) {
                    SoapHeader header = headers[i];
                    if (header.MustUnderstand && !header.DidUnderstand) {
                        throw new SoapHeaderException(Res.GetString(Res.WebCannotUnderstandHeader, GetHeaderElementName(header)),
                            new XmlQualifiedName(Soap.Code.MustUnderstand, Soap.Namespace));
                    }
                }
            }
        }

        public static void GetHeaderMembers(SoapHeaderCollection headers, object target, SoapHeaderMapping[] mappings, SoapHeaderDirection direction, bool client) {
            if (mappings == null || mappings.Length == 0) return;
            for (int i = 0; i < mappings.Length; i++) {
                SoapHeaderMapping mapping = mappings[i];
                if ((mapping.direction & direction) == 0) continue;
                object value = MemberHelper.GetValue(mapping.memberInfo, target);
                if (mapping.repeats) {
                    object[] values = (object[])value;
                    if (values == null) continue;
                    for (int j = 0; j < values.Length; j++) {
                        if (values[j] != null) headers.Add((SoapHeader)values[j]);
                    }
                }
                else {
                    if (value != null) headers.Add((SoapHeader)value);
                }
            }
        }

        public static void EnsureHeadersUnderstood(SoapHeaderCollection headers) {
            for (int i = 0; i < headers.Count; i++) {
                SoapHeader header = headers[i];
                if (header.MustUnderstand && !header.DidUnderstand) {
                    throw new SoapHeaderException(Res.GetString(Res.WebCannotUnderstandHeader, GetHeaderElementName(header)),
                        new XmlQualifiedName(Soap.Code.MustUnderstand, Soap.Namespace));
                }
            }
        }

        static int FindMapping(SoapHeaderMapping[] mappings, SoapHeader header, SoapHeaderDirection direction) {
            if (mappings == null || mappings.Length == 0) return -1;
            Type headerType = header.GetType();
            for (int i = 0; i < mappings.Length; i++) {
                SoapHeaderMapping mapping = mappings[i];
                if ((mapping.direction & direction) == 0) continue;
                if (!mapping.custom) continue;
                if (mapping.headerType.IsAssignableFrom(headerType)) {
                    return i;
                }
            }
            return -1;
        }

        static string GetHeaderElementName(Type headerType) {
            XmlReflectionImporter importer = SoapReflector.CreateXmlImporter(null, false);

            XmlTypeMapping mapping = importer.ImportTypeMapping(headerType);
            return mapping.XsdElementName;
        }

        static string GetHeaderElementName(SoapHeader header) {
            if (header is SoapUnknownHeader) {
                return ((SoapUnknownHeader)header).Element.LocalName;
            }
            else {
                return GetHeaderElementName(header.GetType());
            }
        }
    }
}
