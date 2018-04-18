
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace System.Xml {

    internal class XmlAsyncCheckReader : XmlReader {

        private readonly XmlReader coreReader = null;
        private Task lastTask = AsyncHelper.DoneTask;

        internal XmlReader CoreReader {
            get {
                return coreReader;
            }
        }

        public static XmlAsyncCheckReader CreateAsyncCheckWrapper(XmlReader reader)
        {
            if (reader is IXmlLineInfo) {
                if (reader is IXmlNamespaceResolver) {
#if !FEATURE_NETCORE
                    if (reader is IXmlSchemaInfo) {
                        return new XmlAsyncCheckReaderWithLineInfoNSSchema(reader);
                    }
#endif // !FEATURE_NETCORE
                    return new XmlAsyncCheckReaderWithLineInfoNS(reader);
                }
#if !FEATURE_NETCORE
                Debug.Assert(!(reader is IXmlSchemaInfo));
#endif // !FEATURE_NETCORE
                return new XmlAsyncCheckReaderWithLineInfo(reader);
            }
            else if (reader is IXmlNamespaceResolver) {
#if !FEATURE_NETCORE
                Debug.Assert(!(reader is IXmlSchemaInfo));
#endif // !FEATURE_NETCORE
                return new XmlAsyncCheckReaderWithNS(reader);
            }
#if !FEATURE_NETCORE
            Debug.Assert(!(reader is IXmlSchemaInfo));
#endif // !FEATURE_NETCORE
            return new XmlAsyncCheckReader(reader);
        }

        public XmlAsyncCheckReader(XmlReader reader) {
            coreReader = reader;
        }

        private void CheckAsync() {
            if (!lastTask.IsCompleted) {
                throw new InvalidOperationException(Res.GetString(Res.Xml_AsyncIsRunningException));
            }
        }

        #region Sync Methods, Properties Check
        
        public override XmlReaderSettings Settings {
            get {
                XmlReaderSettings settings = coreReader.Settings;
                if (null != settings) {
                    settings = settings.Clone();
                }
                else {
                    settings = new XmlReaderSettings();
                }
                settings.Async = true;
                settings.ReadOnly = true;
                return settings;
            }
        }
        
        public override XmlNodeType NodeType {
            get {
                CheckAsync();
                return coreReader.NodeType;
            }
        }
        
        public override string Name {
            get {
                CheckAsync();
                return coreReader.Name;
            }
        }
        
        public override string LocalName {
            get {
                CheckAsync();
                return coreReader.LocalName;
            }
        }
        
        public override string NamespaceURI {
            get {
                CheckAsync();
                return coreReader.NamespaceURI;
            }
        }
        
        public override string Prefix {
            get {
                CheckAsync();
                return coreReader.Prefix;
            }
        }
        
        public override bool HasValue {
            get {
                CheckAsync();
                return coreReader.HasValue;
            }
        }
        
        public override string Value {
            get {
                CheckAsync();
                return coreReader.Value;
            }
        }
        
        public override int Depth {
            get {
                CheckAsync();
                return coreReader.Depth;
            }
        }
        
        public override string BaseURI {
            get {
                CheckAsync();
                return coreReader.BaseURI;
            }
        }
        
        public override bool IsEmptyElement {
            get {
                CheckAsync();
                return coreReader.IsEmptyElement;
            }
        }
        
        public override bool IsDefault {
            get {
                CheckAsync();
                return coreReader.IsDefault;
            }
        }
        
#if !SILVERLIGHT
        public override char QuoteChar {
            get {
                CheckAsync();
                return coreReader.QuoteChar;
            }
        }
#endif // !SILVERLIGHT
        
        public override XmlSpace XmlSpace {
            get {
                CheckAsync();
                return coreReader.XmlSpace;
            }
        }
        
        public override string XmlLang {
            get {
                CheckAsync();
                return coreReader.XmlLang;
            }
        }
        
#if !FEATURE_NETCORE
        public override IXmlSchemaInfo SchemaInfo {
            get {
                CheckAsync();
                return coreReader.SchemaInfo;
            }
        }
#endif // !FEATURE_NETCORE
        
        public override System.Type ValueType {
            get {
                CheckAsync();
                return coreReader.ValueType;
            }
        }
        
        public override object ReadContentAsObject() {
            CheckAsync();
            return coreReader.ReadContentAsObject();
        }
        
        public override bool ReadContentAsBoolean() {
            CheckAsync();
            return coreReader.ReadContentAsBoolean();
        }
        
        public override DateTime ReadContentAsDateTime() {
            CheckAsync();
            return coreReader.ReadContentAsDateTime();
        }
        
        public override double ReadContentAsDouble() {
            CheckAsync();
            return coreReader.ReadContentAsDouble();
        }
        
        public override float ReadContentAsFloat() {
            CheckAsync();
            return coreReader.ReadContentAsFloat();
        }
        
        public override decimal ReadContentAsDecimal() {
            CheckAsync();
            return coreReader.ReadContentAsDecimal();
        }
        
        public override int ReadContentAsInt() {
            CheckAsync();
            return coreReader.ReadContentAsInt();
        }
        
        public override long ReadContentAsLong() {
            CheckAsync();
            return coreReader.ReadContentAsLong();
        }
        
        public override string ReadContentAsString() {
            CheckAsync();
            return coreReader.ReadContentAsString();
        }
        
        public override object ReadContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver) {
            CheckAsync();
            return coreReader.ReadContentAs(returnType, namespaceResolver);
        }
        
        public override object ReadElementContentAsObject() {
            CheckAsync();
            return coreReader.ReadElementContentAsObject();
        }
        
        public override object ReadElementContentAsObject(string localName, string namespaceURI) {
            CheckAsync();
            return coreReader.ReadElementContentAsObject(localName, namespaceURI);
        }
        
        public override bool ReadElementContentAsBoolean() {
            CheckAsync();
            return coreReader.ReadElementContentAsBoolean();
        }
        
        public override bool ReadElementContentAsBoolean(string localName, string namespaceURI) {
            CheckAsync();
            return coreReader.ReadElementContentAsBoolean(localName, namespaceURI);
        }
        
        public override DateTime ReadElementContentAsDateTime() {
            CheckAsync();
            return coreReader.ReadElementContentAsDateTime();
        }
        
        public override DateTime ReadElementContentAsDateTime(string localName, string namespaceURI) {
            CheckAsync();
            return coreReader.ReadElementContentAsDateTime(localName, namespaceURI);
        }

        public override DateTimeOffset ReadContentAsDateTimeOffset() {
            CheckAsync();
            return coreReader.ReadContentAsDateTimeOffset();
        }
        
        public override double ReadElementContentAsDouble() {
            CheckAsync();
            return coreReader.ReadElementContentAsDouble();
        }
        
        public override double ReadElementContentAsDouble(string localName, string namespaceURI) {
            CheckAsync();
            return coreReader.ReadElementContentAsDouble(localName, namespaceURI);
        }
        
        public override float ReadElementContentAsFloat() {
            CheckAsync();
            return coreReader.ReadElementContentAsFloat();
        }
        
        public override float ReadElementContentAsFloat(string localName, string namespaceURI) {
            CheckAsync();
            return coreReader.ReadElementContentAsFloat(localName, namespaceURI);
        }
        
        public override decimal ReadElementContentAsDecimal() {
            CheckAsync();
            return coreReader.ReadElementContentAsDecimal();
        }
        
        public override decimal ReadElementContentAsDecimal(string localName, string namespaceURI) {
            CheckAsync();
            return coreReader.ReadElementContentAsDecimal(localName, namespaceURI);
        }
        
        public override int ReadElementContentAsInt() {
            CheckAsync();
            return coreReader.ReadElementContentAsInt();
        }
        
        public override int ReadElementContentAsInt(string localName, string namespaceURI) {
            CheckAsync();
            return coreReader.ReadElementContentAsInt(localName, namespaceURI);
        }
        
        public override long ReadElementContentAsLong() {
            CheckAsync();
            return coreReader.ReadElementContentAsLong();
        }
        
        public override long ReadElementContentAsLong(string localName, string namespaceURI) {
            CheckAsync();
            return coreReader.ReadElementContentAsLong(localName, namespaceURI);
        }
        
        public override string ReadElementContentAsString() {
            CheckAsync();
            return coreReader.ReadElementContentAsString();
        }
        
        public override string ReadElementContentAsString(string localName, string namespaceURI) {
            CheckAsync();
            return coreReader.ReadElementContentAsString(localName, namespaceURI);
        }
        
        public override object ReadElementContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver) {
            CheckAsync();
            return coreReader.ReadElementContentAs(returnType, namespaceResolver);
        }
        
        public override object ReadElementContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver, string localName, string namespaceURI) {
            CheckAsync();
            return coreReader.ReadElementContentAs(returnType, namespaceResolver, localName, namespaceURI);
        }
        
        public override int AttributeCount {
            get {
                CheckAsync();
                return coreReader.AttributeCount;
            }
        }
        
        public override string GetAttribute(string name) {
            CheckAsync();
            return coreReader.GetAttribute(name);
        }
        
        public override string GetAttribute(string name, string namespaceURI) {
            CheckAsync();
            return coreReader.GetAttribute(name, namespaceURI);
        }
        
        public override string GetAttribute(int i) {
            CheckAsync();
            return coreReader.GetAttribute(i);
        }

        public override string this[int i] {
            get {
                CheckAsync();
                return coreReader[i];
            }
        }

        public override string this[string name] {
            get {
                CheckAsync();
                return coreReader[name];
            }
        }

        public override string this[string name, string namespaceURI] {
            get {
                CheckAsync();
                return coreReader[name, namespaceURI];
            }
        }
        
        public override bool MoveToAttribute(string name) {
            CheckAsync();
            return coreReader.MoveToAttribute(name);
        }
        
        public override bool MoveToAttribute(string name, string ns) {
            CheckAsync();
            return coreReader.MoveToAttribute(name, ns);
        }
        
        public override void MoveToAttribute(int i) {
            CheckAsync();
            coreReader.MoveToAttribute(i);
        }
        
        public override bool MoveToFirstAttribute() {
            CheckAsync();
            return coreReader.MoveToFirstAttribute();
        }
        
        public override bool MoveToNextAttribute() {
            CheckAsync();
            return coreReader.MoveToNextAttribute();
        }
        
        public override bool MoveToElement() {
            CheckAsync();
            return coreReader.MoveToElement();
        }
        
        public override bool ReadAttributeValue() {
            CheckAsync();
            return coreReader.ReadAttributeValue();
        }
        
        public override bool Read() {
            CheckAsync();
            return coreReader.Read();
        }
        
        public override bool EOF {
            get {
                CheckAsync();
                return coreReader.EOF;
            }
        }
        
        public override void Close() {
            CheckAsync();
            coreReader.Close();
        }
        
        public override ReadState ReadState {
            get {
                CheckAsync();
                return coreReader.ReadState;
            }
        }
        
        public override void Skip() {
            CheckAsync();
            coreReader.Skip();
        }
        
        public override XmlNameTable NameTable {
            get {
                CheckAsync();
                return coreReader.NameTable;
            }
        }
        
        public override string LookupNamespace(string prefix) {
            CheckAsync();
            return coreReader.LookupNamespace(prefix);
        }
        
        public override bool CanResolveEntity {
            get {
                CheckAsync();
                return coreReader.CanResolveEntity;
            }
        }
        
        public override void ResolveEntity() {
            CheckAsync();
            coreReader.ResolveEntity();
        }
        
        public override bool CanReadBinaryContent {
            get {
                CheckAsync();
                return coreReader.CanReadBinaryContent;
            }
        }
        
        public override int ReadContentAsBase64(byte[] buffer, int index, int count) {
            CheckAsync();
            return coreReader.ReadContentAsBase64(buffer, index, count);
        }
        
        public override int ReadElementContentAsBase64(byte[] buffer, int index, int count) {
            CheckAsync();
            return coreReader.ReadElementContentAsBase64(buffer, index, count);
        }
        
        public override int ReadContentAsBinHex(byte[] buffer, int index, int count) {
            CheckAsync();
            return coreReader.ReadContentAsBinHex(buffer, index, count);
        }
        
        public override int ReadElementContentAsBinHex(byte[] buffer, int index, int count) {
            CheckAsync();
            return coreReader.ReadElementContentAsBinHex(buffer, index, count);
        }
        
        public override bool CanReadValueChunk {
            get {
                CheckAsync();
                return coreReader.CanReadValueChunk;
            }
        }
        
        public override int ReadValueChunk(char[] buffer, int index, int count) {
            CheckAsync();
            return coreReader.ReadValueChunk(buffer, index, count);
        }
        
#if !SILVERLIGHT
        public override string ReadString() {
            CheckAsync();
            return coreReader.ReadString();
        }
#endif // !SILVERLIGHT
        
        public override XmlNodeType MoveToContent() {
            CheckAsync();
            return coreReader.MoveToContent();
        }
        
        public override void ReadStartElement() {
            CheckAsync();
            coreReader.ReadStartElement();
        }
        
        public override void ReadStartElement(string name) {
            CheckAsync();
            coreReader.ReadStartElement(name);
        }
        
        public override void ReadStartElement(string localname, string ns) {
            CheckAsync();
            coreReader.ReadStartElement(localname, ns);
        }
        
#if !SILVERLIGHT
        public override string ReadElementString() {
            CheckAsync();
            return coreReader.ReadElementString();
        }
        
        public override string ReadElementString(string name) {
            CheckAsync();
            return coreReader.ReadElementString(name);
        }
        
        public override string ReadElementString(string localname, string ns) {
            CheckAsync();
            return coreReader.ReadElementString(localname, ns);
        }
#endif // !SILVERLIGHT
        
        public override void ReadEndElement() {
            CheckAsync();
            coreReader.ReadEndElement();
        }
        
        public override bool IsStartElement() {
            CheckAsync();
            return coreReader.IsStartElement();
        }
        
        public override bool IsStartElement(string name) {
            CheckAsync();
            return coreReader.IsStartElement(name);
        }
        
        public override bool IsStartElement(string localname, string ns) {
            CheckAsync();
            return coreReader.IsStartElement(localname, ns);
        }
        
        public override bool ReadToFollowing(string name) {
            CheckAsync();
            return coreReader.ReadToFollowing(name);
        }
        
        public override bool ReadToFollowing(string localName, string namespaceURI) {
            CheckAsync();
            return coreReader.ReadToFollowing(localName, namespaceURI);
        }
        
        public override bool ReadToDescendant(string name) {
            CheckAsync();
            return coreReader.ReadToDescendant(name);
        }
        
        public override bool ReadToDescendant(string localName, string namespaceURI) {
            CheckAsync();
            return coreReader.ReadToDescendant(localName, namespaceURI);
        }
        
        public override bool ReadToNextSibling(string name) {
            CheckAsync();
            return coreReader.ReadToNextSibling(name);
        }
        
        public override bool ReadToNextSibling(string localName, string namespaceURI) {
            CheckAsync();
            return coreReader.ReadToNextSibling(localName, namespaceURI);
        }
        
        public override string ReadInnerXml() {
            CheckAsync();
            return coreReader.ReadInnerXml();
        }
        
        public override string ReadOuterXml() {
            CheckAsync();
            return coreReader.ReadOuterXml();
        }
        
        public override XmlReader ReadSubtree() {
            CheckAsync();
            XmlReader subtreeReader = coreReader.ReadSubtree();
            return CreateAsyncCheckWrapper(subtreeReader);
        }
        
        public override bool HasAttributes {
            get {
                CheckAsync();
                return coreReader.HasAttributes;
            }
        }

        protected override void Dispose(bool disposing) {
            CheckAsync();
            //since it is protected method, we can't call coreReader.Dispose(disposing). 
            //Internal, it is always called to Dipose(true). So call coreReader.Dispose() is OK.
            coreReader.Dispose();
        }

#if !SILVERLIGHT
        internal override XmlNamespaceManager NamespaceManager {
            get {
                CheckAsync();
                return coreReader.NamespaceManager;
            }
        }

        internal override IDtdInfo DtdInfo {
            get {
                CheckAsync();
                return coreReader.DtdInfo;
            }
        }
#endif

        #endregion

        #region Async Methods
        
        public override Task<string> GetValueAsync() {
            CheckAsync();
            var task = coreReader.GetValueAsync();
            lastTask = task;
            return task;
        }
        
        public override Task<object> ReadContentAsObjectAsync() {
            CheckAsync();
            var task = coreReader.ReadContentAsObjectAsync();
            lastTask = task;
            return task;
        }
        
        public override Task<string> ReadContentAsStringAsync() {
            CheckAsync();
            var task = coreReader.ReadContentAsStringAsync();
            lastTask = task;
            return task;
        }
        
        public override Task<object> ReadContentAsAsync(Type returnType, IXmlNamespaceResolver namespaceResolver) {
            CheckAsync();
            var task = coreReader.ReadContentAsAsync(returnType, namespaceResolver);
            lastTask = task;
            return task;
        }
        
        public override Task<object> ReadElementContentAsObjectAsync() {
            CheckAsync();
            var task = coreReader.ReadElementContentAsObjectAsync();
            lastTask = task;
            return task;
        }
        
        public override Task<string> ReadElementContentAsStringAsync() {
            CheckAsync();
            var task = coreReader.ReadElementContentAsStringAsync();
            lastTask = task;
            return task;
        }
        
        public override Task<object> ReadElementContentAsAsync(Type returnType, IXmlNamespaceResolver namespaceResolver) {
            CheckAsync();
            var task = coreReader.ReadElementContentAsAsync(returnType, namespaceResolver);
            lastTask = task;
            return task;
        }
        
        public override Task<bool> ReadAsync() {
            CheckAsync();
            var task = coreReader.ReadAsync();
            lastTask = task;
            return task;
        }
        
        public override Task SkipAsync() {
            CheckAsync();
            var task = coreReader.SkipAsync();
            lastTask = task;
            return task;
        }
        
        public override Task<int> ReadContentAsBase64Async(byte[] buffer, int index, int count) {
            CheckAsync();
            var task = coreReader.ReadContentAsBase64Async(buffer, index, count);
            lastTask = task;
            return task;
        }
        
        public override Task<int> ReadElementContentAsBase64Async(byte[] buffer, int index, int count) {
            CheckAsync();
            var task = coreReader.ReadElementContentAsBase64Async(buffer, index, count);
            lastTask = task;
            return task;
        }
        
        public override Task<int> ReadContentAsBinHexAsync(byte[] buffer, int index, int count) {
            CheckAsync();
            var task = coreReader.ReadContentAsBinHexAsync(buffer, index, count);
            lastTask = task;
            return task;
        }
        
        public override Task<int> ReadElementContentAsBinHexAsync(byte[] buffer, int index, int count) {
            CheckAsync();
            var task = coreReader.ReadElementContentAsBinHexAsync(buffer, index, count);
            lastTask = task;
            return task;
        }
        
        public override Task<int> ReadValueChunkAsync(char[] buffer, int index, int count) {
            CheckAsync();
            var task = coreReader.ReadValueChunkAsync(buffer, index, count);
            lastTask = task;
            return task;
        }
        
        public override Task<XmlNodeType> MoveToContentAsync() {
            CheckAsync();
            var task = coreReader.MoveToContentAsync();
            lastTask = task;
            return task;
        }
      
        public override Task<string> ReadInnerXmlAsync() {
            CheckAsync();
            var task = coreReader.ReadInnerXmlAsync();
            lastTask = task;
            return task;
        }
        
        public override Task<string> ReadOuterXmlAsync() {
            CheckAsync();
            var task = coreReader.ReadOuterXmlAsync();
            lastTask = task;
            return task;
        }

        #endregion

    }

    internal class XmlAsyncCheckReaderWithNS : XmlAsyncCheckReader, IXmlNamespaceResolver {
        private readonly IXmlNamespaceResolver readerAsIXmlNamespaceResolver;

        public XmlAsyncCheckReaderWithNS(XmlReader reader)
            : base(reader) {

            readerAsIXmlNamespaceResolver = (IXmlNamespaceResolver)reader;
        }

        #region IXmlNamespaceResolver members
        IDictionary<string, string> IXmlNamespaceResolver.GetNamespacesInScope(XmlNamespaceScope scope)
        {
            return readerAsIXmlNamespaceResolver.GetNamespacesInScope(scope);
        }

        string IXmlNamespaceResolver.LookupNamespace(string prefix)
        {
            return readerAsIXmlNamespaceResolver.LookupNamespace(prefix);
        }

        string IXmlNamespaceResolver.LookupPrefix(string namespaceName)
        {
            return readerAsIXmlNamespaceResolver.LookupPrefix(namespaceName);
        }
        #endregion
    }

    internal class XmlAsyncCheckReaderWithLineInfo : XmlAsyncCheckReader, IXmlLineInfo {

        private readonly IXmlLineInfo readerAsIXmlLineInfo;

        public XmlAsyncCheckReaderWithLineInfo(XmlReader reader)
            : base(reader) {

            readerAsIXmlLineInfo = (IXmlLineInfo)reader;
        }

        #region IXmlLineInfo members
        public virtual bool HasLineInfo() {
            return readerAsIXmlLineInfo.HasLineInfo();
        }

        public virtual int LineNumber {
            get {
                return readerAsIXmlLineInfo.LineNumber;
            }
        }

        public virtual int LinePosition {
            get {
                return readerAsIXmlLineInfo.LinePosition;
            }
        }
        #endregion
    }

    internal class XmlAsyncCheckReaderWithLineInfoNS : XmlAsyncCheckReaderWithLineInfo, IXmlNamespaceResolver {

        private readonly IXmlNamespaceResolver readerAsIXmlNamespaceResolver;

        public XmlAsyncCheckReaderWithLineInfoNS(XmlReader reader)
            : base(reader) {

            readerAsIXmlNamespaceResolver = (IXmlNamespaceResolver)reader;
        }

        #region IXmlNamespaceResolver members
        IDictionary<string, string> IXmlNamespaceResolver.GetNamespacesInScope(XmlNamespaceScope scope) {
            return readerAsIXmlNamespaceResolver.GetNamespacesInScope(scope);
        }

        string IXmlNamespaceResolver.LookupNamespace(string prefix) {
            return readerAsIXmlNamespaceResolver.LookupNamespace(prefix);
        }

        string IXmlNamespaceResolver.LookupPrefix(string namespaceName) {
            return readerAsIXmlNamespaceResolver.LookupPrefix(namespaceName);
        }
        #endregion
    }

#if !FEATURE_NETCORE
    internal class XmlAsyncCheckReaderWithLineInfoNSSchema : XmlAsyncCheckReaderWithLineInfoNS, IXmlSchemaInfo {

        private readonly IXmlSchemaInfo readerAsIXmlSchemaInfo;

        public XmlAsyncCheckReaderWithLineInfoNSSchema(XmlReader reader)
            : base(reader) {

            readerAsIXmlSchemaInfo = (IXmlSchemaInfo)reader;
        }


        #region IXmlSchemaInfo members

        XmlSchemaValidity IXmlSchemaInfo.Validity {
            get {
                return readerAsIXmlSchemaInfo.Validity;
            }
        }

        bool IXmlSchemaInfo.IsDefault {
            get {
                return readerAsIXmlSchemaInfo.IsDefault;
            }
        }

        bool IXmlSchemaInfo.IsNil {
            get {
                return readerAsIXmlSchemaInfo.IsNil;
            }
        }

        XmlSchemaSimpleType IXmlSchemaInfo.MemberType {
            get {
                return readerAsIXmlSchemaInfo.MemberType;
            }
        }

        XmlSchemaType IXmlSchemaInfo.SchemaType {
            get {
                return readerAsIXmlSchemaInfo.SchemaType;
            }
        }

        XmlSchemaElement IXmlSchemaInfo.SchemaElement {
            get {
                return readerAsIXmlSchemaInfo.SchemaElement;
            }
        }

        XmlSchemaAttribute IXmlSchemaInfo.SchemaAttribute {
            get {
                return readerAsIXmlSchemaInfo.SchemaAttribute;
            }
        }
        #endregion
    }
#endif // !FEATURE_NETCORE
}
