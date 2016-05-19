//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.Xml
{
    using System.IO;
    using System.Collections;
    using System.Text;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Runtime;
    using System.Threading;
    using System.Collections.Generic;

    abstract class XmlNodeWriter
    {
        static XmlNodeWriter nullNodeWriter;

        static public XmlNodeWriter Null
        {
            get
            {
                if (nullNodeWriter == null)
                    nullNodeWriter = new XmlNullNodeWriter();
                return nullNodeWriter;
            }
        }

        internal virtual AsyncCompletionResult WriteBase64TextAsync(AsyncEventArgs<XmlNodeWriterWriteBase64TextArgs> state)
        {
            // We do not guard this invocation. The caller of the NodeWriter should ensure that that 
            // they override the FastAsync guard clause for the XmlDictionaryWriter and that the 
            // nodeWriter has an implemenation for WriteBase64TextAsync.
            throw Fx.AssertAndThrow("WriteBase64TextAsync not implemented.");
        }

        public virtual IAsyncResult BeginWriteBase64Text(byte[] trailBuffer, int trailCount, byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return new WriteBase64TextAsyncResult(trailBuffer, trailCount, buffer, offset, count, this, callback, state);
        }

        public virtual void EndWriteBase64Text(IAsyncResult result)
        {
            WriteBase64TextAsyncResult.End(result);
        }

        public abstract void Flush();
        public abstract void Close();
        public abstract void WriteDeclaration();
        public abstract void WriteComment(string text);
        public abstract void WriteCData(string text);
        public abstract void WriteStartElement(string prefix, string localName);
        public virtual void WriteStartElement(byte[] prefixBuffer, int prefixOffset, int prefixLength, byte[] localNameBuffer, int localNameOffset, int localNameLength)
        {
            WriteStartElement(Encoding.UTF8.GetString(prefixBuffer, prefixOffset, prefixLength), Encoding.UTF8.GetString(localNameBuffer, localNameOffset, localNameLength));
        }
        public abstract void WriteStartElement(string prefix, XmlDictionaryString localName);
        public abstract void WriteEndStartElement(bool isEmpty);
        public abstract void WriteEndElement(string prefix, string localName);
        public virtual void WriteEndElement(byte[] prefixBuffer, int prefixOffset, int prefixLength, byte[] localNameBuffer, int localNameOffset, int localNameLength)
        {
            WriteEndElement(Encoding.UTF8.GetString(prefixBuffer, prefixOffset, prefixLength), Encoding.UTF8.GetString(localNameBuffer, localNameOffset, localNameLength));
        }
        public abstract void WriteXmlnsAttribute(string prefix, string ns);
        public virtual void WriteXmlnsAttribute(byte[] prefixBuffer, int prefixOffset, int prefixLength, byte[] nsBuffer, int nsOffset, int nsLength)
        {
            WriteXmlnsAttribute(Encoding.UTF8.GetString(prefixBuffer, prefixOffset, prefixLength), Encoding.UTF8.GetString(nsBuffer, nsOffset, nsLength));
        }
        public abstract void WriteXmlnsAttribute(string prefix, XmlDictionaryString ns);
        public abstract void WriteStartAttribute(string prefix, string localName);
        public virtual void WriteStartAttribute(byte[] prefixBuffer, int prefixOffset, int prefixLength, byte[] localNameBuffer, int localNameOffset, int localNameLength)
        {
            WriteStartAttribute(Encoding.UTF8.GetString(prefixBuffer, prefixOffset, prefixLength), Encoding.UTF8.GetString(localNameBuffer, localNameOffset, localNameLength));
        }
        public abstract void WriteStartAttribute(string prefix, XmlDictionaryString localName);
        public abstract void WriteEndAttribute();
        public abstract void WriteCharEntity(int ch);
        public abstract void WriteEscapedText(string value);
        public abstract void WriteEscapedText(XmlDictionaryString value);
        public abstract void WriteEscapedText(char[] chars, int offset, int count);
        public abstract void WriteEscapedText(byte[] buffer, int offset, int count);
        public abstract void WriteText(string value);
        public abstract void WriteText(XmlDictionaryString value);
        public abstract void WriteText(char[] chars, int offset, int count);
        public abstract void WriteText(byte[] buffer, int offset, int count);
        public abstract void WriteInt32Text(int value);
        public abstract void WriteInt64Text(Int64 value);
        public abstract void WriteBoolText(bool value);
        public abstract void WriteUInt64Text(UInt64 value);
        public abstract void WriteFloatText(float value);
        public abstract void WriteDoubleText(double value);
        public abstract void WriteDecimalText(decimal value);
        public abstract void WriteDateTimeText(DateTime value);
        public abstract void WriteUniqueIdText(UniqueId value);
        public abstract void WriteTimeSpanText(TimeSpan value);
        public abstract void WriteGuidText(Guid value);
        public abstract void WriteStartListText();
        public abstract void WriteListSeparator();
        public abstract void WriteEndListText();
        public abstract void WriteBase64Text(byte[] trailBuffer, int trailCount, byte[] buffer, int offset, int count);
        public abstract void WriteQualifiedName(string prefix, XmlDictionaryString localName);

        class XmlNullNodeWriter : XmlNodeWriter
        {
            public override void Flush() { }
            public override void Close() { }
            public override void WriteDeclaration() { }
            public override void WriteComment(string text) { }
            public override void WriteCData(string text) { }
            public override void WriteStartElement(string prefix, string localName) { }
            public override void WriteStartElement(byte[] prefixBuffer, int prefixOffset, int prefixLength, byte[] localNameBuffer, int localNameOffset, int localNameLength) { }
            public override void WriteStartElement(string prefix, XmlDictionaryString localName) { }
            public override void WriteEndStartElement(bool isEmpty) { }
            public override void WriteEndElement(string prefix, string localName) { }
            public override void WriteEndElement(byte[] prefixBuffer, int prefixOffset, int prefixLength, byte[] localNameBuffer, int localNameOffset, int localNameLength) { }
            public override void WriteXmlnsAttribute(string prefix, string ns) { }
            public override void WriteXmlnsAttribute(byte[] prefixBuffer, int prefixOffset, int prefixLength, byte[] nsBuffer, int nsOffset, int nsLength) { }
            public override void WriteXmlnsAttribute(string prefix, XmlDictionaryString ns) { }
            public override void WriteStartAttribute(string prefix, string localName) { }
            public override void WriteStartAttribute(byte[] prefixBuffer, int prefixOffset, int prefixLength, byte[] localNameBuffer, int localNameOffset, int localNameLength) { }
            public override void WriteStartAttribute(string prefix, XmlDictionaryString localName) { }
            public override void WriteEndAttribute() { }
            public override void WriteCharEntity(int ch) { }
            public override void WriteEscapedText(string value) { }
            public override void WriteEscapedText(XmlDictionaryString value) { }
            public override void WriteEscapedText(char[] chars, int offset, int count) { }
            public override void WriteEscapedText(byte[] buffer, int offset, int count) { }
            public override void WriteText(string value) { }
            public override void WriteText(XmlDictionaryString value) { }
            public override void WriteText(char[] chars, int offset, int count) { }
            public override void WriteText(byte[] buffer, int offset, int count) { }
            public override void WriteInt32Text(int value) { }
            public override void WriteInt64Text(Int64 value) { }
            public override void WriteBoolText(bool value) { }
            public override void WriteUInt64Text(UInt64 value) { }
            public override void WriteFloatText(float value) { }
            public override void WriteDoubleText(double value) { }
            public override void WriteDecimalText(decimal value) { }
            public override void WriteDateTimeText(DateTime value) { }
            public override void WriteUniqueIdText(UniqueId value) { }
            public override void WriteTimeSpanText(TimeSpan value) { }
            public override void WriteGuidText(Guid value) { }
            public override void WriteStartListText() { }
            public override void WriteListSeparator() { }
            public override void WriteEndListText() { }
            public override void WriteBase64Text(byte[] trailBuffer, int trailCount, byte[] buffer, int offset, int count) { }
            public override void WriteQualifiedName(string prefix, XmlDictionaryString localName) { }
        }

        class WriteBase64TextAsyncResult : ScheduleActionItemAsyncResult
        {
            byte[] trailBuffer;
            int trailCount;
            byte[] buffer;
            int offset;
            int count;
            XmlNodeWriter nodeWriter;

            public WriteBase64TextAsyncResult(byte[] trailBuffer, int trailCount, byte[] buffer, int offset, int count, XmlNodeWriter nodeWriter, AsyncCallback callback, object state)
                : base(callback, state)
            {
                Fx.Assert(nodeWriter != null, "nodeWriter should never be null");

                this.trailBuffer = trailBuffer;
                this.trailCount = trailCount;
                this.buffer = buffer;
                this.offset = offset;
                this.count = count;
                this.nodeWriter = nodeWriter;

                Schedule();
            }

            protected override void OnDoWork()
            {
                this.nodeWriter.WriteBase64Text(this.trailBuffer, this.trailCount, this.buffer, this.offset, this.count);
            }
        }
    }
}
