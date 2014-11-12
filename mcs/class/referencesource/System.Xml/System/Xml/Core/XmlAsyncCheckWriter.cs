

using System.Threading.Tasks;
#if !FEATURE_NETCORE
using System.Xml.XPath;
#endif // !FEATURE_NETCORE

namespace System.Xml {

    internal class XmlAsyncCheckWriter : XmlWriter {

        private readonly XmlWriter coreWriter = null;
        private Task lastTask = AsyncHelper.DoneTask;

        internal XmlWriter CoreWriter {
            get {
                return coreWriter;
            }
        }

        public XmlAsyncCheckWriter(XmlWriter writer) {
            coreWriter = writer;
        }

        private void CheckAsync() {
            if (!lastTask.IsCompleted) {
                throw new InvalidOperationException(Res.GetString(Res.Xml_AsyncIsRunningException));
            }
        }

        #region [....] Methods, Properties Check

        public override XmlWriterSettings Settings {
            get {
                XmlWriterSettings settings = coreWriter.Settings;

                if (null != settings) {
                    settings = settings.Clone();
                }
                else {
                    settings = new XmlWriterSettings();
                }

                settings.Async = true;

                settings.ReadOnly = true;
                return settings;
            }
        }

        public override void WriteStartDocument() {
            CheckAsync();
            coreWriter.WriteStartDocument();
        }

        public override void WriteStartDocument(bool standalone) {
            CheckAsync();
            coreWriter.WriteStartDocument(standalone);
        }

        public override void WriteEndDocument() {
            CheckAsync();
            coreWriter.WriteEndDocument();
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset) {
            CheckAsync();
            coreWriter.WriteDocType(name, pubid, sysid, subset);
        }

        public override void WriteStartElement(string prefix, string localName, string ns) {
            CheckAsync();
            coreWriter.WriteStartElement(prefix, localName, ns);
        }

        public override void WriteEndElement() {
            CheckAsync();
            coreWriter.WriteEndElement();
        }

        public override void WriteFullEndElement() {
            CheckAsync();
            coreWriter.WriteFullEndElement();
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns) {
            CheckAsync();
            coreWriter.WriteStartAttribute(prefix, localName, ns);
        }

        public override void WriteEndAttribute() {
            CheckAsync();
            coreWriter.WriteEndAttribute();
        }

        public override void WriteCData(string text) {
            CheckAsync();
            coreWriter.WriteCData(text);
        }

        public override void WriteComment(string text) {
            CheckAsync();
            coreWriter.WriteComment(text);
        }

        public override void WriteProcessingInstruction(string name, string text) {
            CheckAsync();
            coreWriter.WriteProcessingInstruction(name, text);
        }

        public override void WriteEntityRef(string name) {
            CheckAsync();
            coreWriter.WriteEntityRef(name);
        }

        public override void WriteCharEntity(char ch) {
            CheckAsync();
            coreWriter.WriteCharEntity(ch);
        }

        public override void WriteWhitespace(string ws) {
            CheckAsync();
            coreWriter.WriteWhitespace(ws);
        }

        public override void WriteString(string text) {
            CheckAsync();
            coreWriter.WriteString(text);
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar) {
            CheckAsync();
            coreWriter.WriteSurrogateCharEntity(lowChar, highChar);
        }

        public override void WriteChars(char[] buffer, int index, int count) {
            CheckAsync();
            coreWriter.WriteChars(buffer, index, count);
        }

        public override void WriteRaw(char[] buffer, int index, int count) {
            CheckAsync();
            coreWriter.WriteRaw(buffer, index, count);
        }

        public override void WriteRaw(string data) {
            CheckAsync();
            coreWriter.WriteRaw(data);
        }

        public override void WriteBase64(byte[] buffer, int index, int count) {
            CheckAsync();
            coreWriter.WriteBase64(buffer, index, count);
        }

        public override void WriteBinHex(byte[] buffer, int index, int count) {
            CheckAsync();
            coreWriter.WriteBinHex(buffer, index, count);
        }

        public override WriteState WriteState {
            get {
                CheckAsync();
                return coreWriter.WriteState;
            }
        }

        public override void Close() {
            CheckAsync();
            coreWriter.Close();
        }

        public override void Flush() {
            CheckAsync();
            coreWriter.Flush();
        }

        public override string LookupPrefix(string ns) {
            CheckAsync();
            return coreWriter.LookupPrefix(ns);
        }

        public override XmlSpace XmlSpace {
            get {
                CheckAsync();
                return coreWriter.XmlSpace;
            }
        }

        public override string XmlLang {
            get {
                CheckAsync();
                return coreWriter.XmlLang;
            }
        }

        public override void WriteNmToken(string name) {
            CheckAsync();
            coreWriter.WriteNmToken(name);
        }

        public override void WriteName(string name) {
            CheckAsync();
            coreWriter.WriteName(name);
        }

        public override void WriteQualifiedName(string localName, string ns) {
            CheckAsync();
            coreWriter.WriteQualifiedName(localName, ns);
        }

        public override void WriteValue(object value) {
            CheckAsync();
            coreWriter.WriteValue(value);
        }

        public override void WriteValue(string value) {
            CheckAsync();
            coreWriter.WriteValue(value);
        }

        public override void WriteValue(bool value) {
            CheckAsync();
            coreWriter.WriteValue(value);
        }

        public override void WriteValue(DateTime value) {
            CheckAsync();
            coreWriter.WriteValue(value);
        }

        public override void WriteValue(DateTimeOffset value) {
            CheckAsync();
            coreWriter.WriteValue(value);
        }

        public override void WriteValue(double value) {
            CheckAsync();
            coreWriter.WriteValue(value);
        }

        public override void WriteValue(float value) {
            CheckAsync();
            coreWriter.WriteValue(value);
        }

        public override void WriteValue(decimal value) {
            CheckAsync();
            coreWriter.WriteValue(value);
        }

        public override void WriteValue(int value) {
            CheckAsync();
            coreWriter.WriteValue(value);
        }

        public override void WriteValue(long value) {
            CheckAsync();
            coreWriter.WriteValue(value);
        }

        public override void WriteAttributes(XmlReader reader, bool defattr) {
            CheckAsync();
            coreWriter.WriteAttributes(reader, defattr);
        }

        public override void WriteNode(XmlReader reader, bool defattr) {
            CheckAsync();
            coreWriter.WriteNode(reader, defattr);
        }

#if !FEATURE_NETCORE
        public override void WriteNode(XPathNavigator navigator, bool defattr) {
            CheckAsync();
            coreWriter.WriteNode(navigator, defattr);
        }
#endif

        protected override void Dispose(bool disposing) {
            CheckAsync();
            //since it is protected method, we can't call coreWriter.Dispose(disposing). 
            //Internal, it is always called to Dipose(true). So call coreWriter.Dispose() is OK.
            coreWriter.Dispose();
        }

        #endregion

        #region Async Methods

        public override Task WriteStartDocumentAsync() {
            CheckAsync();
            var task = coreWriter.WriteStartDocumentAsync();
            lastTask = task;
            return task;
        }

        public override Task WriteStartDocumentAsync(bool standalone) {
            CheckAsync();
            var task = coreWriter.WriteStartDocumentAsync(standalone);
            lastTask = task;
            return task;
        }

        public override Task WriteEndDocumentAsync() {
            CheckAsync();
            var task = coreWriter.WriteEndDocumentAsync();
            lastTask = task;
            return task;
        }

        public override Task WriteDocTypeAsync(string name, string pubid, string sysid, string subset) {
            CheckAsync();
            var task = coreWriter.WriteDocTypeAsync(name, pubid, sysid, subset);
            lastTask = task;
            return task;
        }

        public override Task WriteStartElementAsync(string prefix, string localName, string ns) {
            CheckAsync();
            var task = coreWriter.WriteStartElementAsync(prefix, localName, ns);
            lastTask = task;
            return task;
        }

        public override Task WriteEndElementAsync() {
            CheckAsync();
            var task = coreWriter.WriteEndElementAsync();
            lastTask = task;
            return task;
        }

        public override Task WriteFullEndElementAsync() {
            CheckAsync();
            var task = coreWriter.WriteFullEndElementAsync();
            lastTask = task;
            return task;
        }

        protected internal override Task WriteStartAttributeAsync(string prefix, string localName, string ns) {
            CheckAsync();
            var task = coreWriter.WriteStartAttributeAsync(prefix, localName, ns);
            lastTask = task;
            return task;
        }

        protected internal override Task WriteEndAttributeAsync() {
            CheckAsync();
            var task = coreWriter.WriteEndAttributeAsync();
            lastTask = task;
            return task;
        }

        public override Task WriteCDataAsync(string text) {
            CheckAsync();
            var task = coreWriter.WriteCDataAsync(text);
            lastTask = task;
            return task;
        }

        public override Task WriteCommentAsync(string text) {
            CheckAsync();
            var task = coreWriter.WriteCommentAsync(text);
            lastTask = task;
            return task;
        }

        public override Task WriteProcessingInstructionAsync(string name, string text) {
            CheckAsync();
            var task = coreWriter.WriteProcessingInstructionAsync(name, text);
            lastTask = task;
            return task;
        }

        public override Task WriteEntityRefAsync(string name) {
            CheckAsync();
            var task = coreWriter.WriteEntityRefAsync(name);
            lastTask = task;
            return task;
        }

        public override Task WriteCharEntityAsync(char ch) {
            CheckAsync();
            var task = coreWriter.WriteCharEntityAsync(ch);
            lastTask = task;
            return task;
        }

        public override Task WriteWhitespaceAsync(string ws) {
            CheckAsync();
            var task = coreWriter.WriteWhitespaceAsync(ws);
            lastTask = task;
            return task;
        }

        public override Task WriteStringAsync(string text) {
            CheckAsync();
            var task = coreWriter.WriteStringAsync(text);
            lastTask = task;
            return task;
        }

        public override Task WriteSurrogateCharEntityAsync(char lowChar, char highChar) {
            CheckAsync();
            var task = coreWriter.WriteSurrogateCharEntityAsync(lowChar, highChar);
            lastTask = task;
            return task;
        }

        public override Task WriteCharsAsync(char[] buffer, int index, int count) {
            CheckAsync();
            var task = coreWriter.WriteCharsAsync(buffer, index, count);
            lastTask = task;
            return task;
        }

        public override Task WriteRawAsync(char[] buffer, int index, int count) {
            CheckAsync();
            var task = coreWriter.WriteRawAsync(buffer, index, count);
            lastTask = task;
            return task;
        }

        public override Task WriteRawAsync(string data) {
            CheckAsync();
            var task = coreWriter.WriteRawAsync(data);
            lastTask = task;
            return task;
        }

        public override Task WriteBase64Async(byte[] buffer, int index, int count) {
            CheckAsync();
            var task = coreWriter.WriteBase64Async(buffer, index, count);
            lastTask = task;
            return task;
        }

        public override Task WriteBinHexAsync(byte[] buffer, int index, int count) {
            CheckAsync();
            var task = coreWriter.WriteBinHexAsync(buffer, index, count);
            lastTask = task;
            return task;
        }

        public override Task FlushAsync() {
            CheckAsync();
            var task = coreWriter.FlushAsync();
            lastTask = task;
            return task;
        }

        public override Task WriteNmTokenAsync(string name) {
            CheckAsync();
            var task = coreWriter.WriteNmTokenAsync(name);
            lastTask = task;
            return task;
        }

        public override Task WriteNameAsync(string name) {
            CheckAsync();
            var task = coreWriter.WriteNameAsync(name);
            lastTask = task;
            return task;
        }

        public override Task WriteQualifiedNameAsync(string localName, string ns) {
            CheckAsync();
            var task = coreWriter.WriteQualifiedNameAsync(localName, ns);
            lastTask = task;
            return task;
        }

        public override Task WriteAttributesAsync(XmlReader reader, bool defattr) {
            CheckAsync();
            var task = coreWriter.WriteAttributesAsync(reader, defattr);
            lastTask = task;
            return task;
        }

        public override Task WriteNodeAsync(XmlReader reader, bool defattr) {
            CheckAsync();
            var task = coreWriter.WriteNodeAsync(reader, defattr);
            lastTask = task;
            return task;
        }

#if !FEATURE_NETCORE
        public override Task WriteNodeAsync(XPathNavigator navigator, bool defattr) {
            CheckAsync();
            var task = coreWriter.WriteNodeAsync(navigator, defattr);
            lastTask = task;
            return task;
        }
#endif // !FEATURE_NETCORE

        #endregion

    }
}
