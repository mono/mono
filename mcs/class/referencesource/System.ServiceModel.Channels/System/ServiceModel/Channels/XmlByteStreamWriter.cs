//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.Runtime;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;

    sealed class XmlByteStreamWriter : XmlDictionaryWriter
    {
        bool ownsStream; 
        ByteStreamWriterState state;
        Stream stream;
        XmlWriterSettings settings;

        public XmlByteStreamWriter(Stream stream, bool ownsStream)
        {
            Fx.Assert(stream != null, "stream is null");

            this.stream = stream;
            this.ownsStream = ownsStream;
            this.state = ByteStreamWriterState.Start;
        }

        public override WriteState WriteState
        {
            get { return ByteStreamWriterStateToWriteState(this.state); }
        }

        public override XmlWriterSettings Settings
        {
            get
            {
                if (settings == null)
                {
                    XmlWriterSettings newSettings = new XmlWriterSettings()
                    {
                        Async = true
                    };

                    Interlocked.CompareExchange<XmlWriterSettings>(ref this.settings, newSettings, null);
                }

                return this.settings;
            }
        }

        public override void Close()
        {
            if (this.state != ByteStreamWriterState.Closed)
            {
                try
                {
                    if (ownsStream)
                    {
                        this.stream.Close();
                    }
                    this.stream = null;
                }
                finally
                {
                    this.state = ByteStreamWriterState.Closed;
                }
            }
        }

        void EnsureWriteBase64State(byte[] buffer, int index, int count)
        {
            ThrowIfClosed();
            ByteStreamMessageUtility.EnsureByteBoundaries(buffer, index, count, false);

            if (this.state != ByteStreamWriterState.Content && this.state != ByteStreamWriterState.StartElement)
            {
                throw FxTrace.Exception.AsError(
                    new InvalidOperationException(SR.XmlWriterMustBeInElement(ByteStreamWriterStateToWriteState(this.state))));
            }
        }

        public override void Flush()
        {
            ThrowIfClosed(); 
            this.stream.Flush();
        }

        void InternalWriteEndElement()
        {
            ThrowIfClosed();
            if (this.state != ByteStreamWriterState.StartElement && this.state != ByteStreamWriterState.Content)
            {
                throw FxTrace.Exception.AsError(
                    new InvalidOperationException(SR.XmlUnexpectedEndElement));
            }
            this.state = ByteStreamWriterState.EndElement;
        }

        public override string LookupPrefix(string ns)
        {
            if (ns == string.Empty)
            {
                return string.Empty;
            }
            else if (ns == ByteStreamMessageUtility.XmlNamespace)
            {
                return "xml";
            }
            else if (ns == ByteStreamMessageUtility.XmlNamespaceNamespace)
            {
                return "xmlns";
            }
            else
            {
                return null;
            }
        }

        public override void WriteBase64(byte[] buffer, int index, int count)
        {
            EnsureWriteBase64State(buffer, index, count);
            this.stream.Write(buffer, index, count);
            this.state = ByteStreamWriterState.Content;
        }

        public override Task WriteBase64Async(byte[] buffer, int index, int count)
        {
            return Task.Factory.FromAsync(this.BeginWriteBase64, this.EndWriteBase64, buffer, index, count, null);
        }

        internal IAsyncResult BeginWriteBase64(byte[] buffer, int index, int count, AsyncCallback callback, object state)
        {
            EnsureWriteBase64State(buffer, index, count); 
            return new WriteBase64AsyncResult(buffer, index, count, this, callback, state); 
        }

        internal void EndWriteBase64(IAsyncResult result)
        {
            WriteBase64AsyncResult.End(result); 
        }

        class WriteBase64AsyncResult : AsyncResult
        {
            XmlByteStreamWriter writer;

            public WriteBase64AsyncResult(byte[] buffer, int index, int count, XmlByteStreamWriter writer, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.writer = writer;

                IAsyncResult result = writer.stream.BeginWrite(buffer, index, count, PrepareAsyncCompletion(HandleWriteBase64), this);
                bool completeSelf = SyncContinue(result); 

                if (completeSelf)
                {
                    this.Complete(true);
                }
            }

            static bool HandleWriteBase64(IAsyncResult result)
            {
                WriteBase64AsyncResult thisPtr = (WriteBase64AsyncResult)result.AsyncState; 
                thisPtr.writer.stream.EndWrite(result);
                thisPtr.writer.state = ByteStreamWriterState.Content;

                return true; 
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<WriteBase64AsyncResult>(result); 
            }
        }

        public override void WriteCData(string text)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }

        public override void WriteCharEntity(char ch)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }

        public override void WriteChars(char[] buffer, int index, int count)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }

        public override void WriteComment(string text)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }

        public override void WriteEndAttribute()
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }

        public override void WriteEndDocument()
        {
            return;
        }

        public override void WriteEndElement()
        {
            this.InternalWriteEndElement();
        }

        public override void WriteEntityRef(string name)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }

        public override void WriteFullEndElement()
        {
            this.InternalWriteEndElement();
        }

        public override void WriteProcessingInstruction(string name, string text)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }

        public override void WriteRaw(string data)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }

        public override void WriteRaw(char[] buffer, int index, int count)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }

        public override void WriteStartDocument(bool standalone)
        {
            ThrowIfClosed();
        }

        public override void WriteStartDocument()
        {
            ThrowIfClosed();
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            ThrowIfClosed();
            if (this.state != ByteStreamWriterState.Start)
            {
                throw FxTrace.Exception.AsError(
                    new InvalidOperationException(SR.ByteStreamWriteStartElementAlreadyCalled));
            }

            if (!string.IsNullOrEmpty(prefix) || !string.IsNullOrEmpty(ns) || localName != ByteStreamMessageUtility.StreamElementName)
            {
                throw FxTrace.Exception.AsError(
                    new XmlException(SR.XmlStartElementNameExpected(ByteStreamMessageUtility.StreamElementName, localName)));
            }
            this.state = ByteStreamWriterState.StartElement;
        }

        public override void WriteString(string text)
        {
            // no state checks here - WriteBase64 will take care of this. 
            byte[] buffer = Convert.FromBase64String(text);
            WriteBase64(buffer, 0, buffer.Length);
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }

        public override void WriteWhitespace(string ws)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }

        void ThrowIfClosed()
        {
            if (this.state == ByteStreamWriterState.Closed)
            {
                throw FxTrace.Exception.AsError(
                    new InvalidOperationException(SR.XmlWriterClosed));
            }
        }

        static WriteState ByteStreamWriterStateToWriteState(ByteStreamWriterState byteStreamWriterState)
        {
            // Converts the internal ByteStreamWriterState to an Xml WriteState
            switch (byteStreamWriterState)
            {
                case ByteStreamWriterState.Start:
                    return WriteState.Start;
                case ByteStreamWriterState.StartElement:
                    return WriteState.Element;
                case ByteStreamWriterState.Content:
                    return WriteState.Content;
                case ByteStreamWriterState.EndElement:
                    return WriteState.Element;
                case ByteStreamWriterState.Closed:
                    return WriteState.Closed;
                default:
                    return WriteState.Error;
            }
        }

        enum ByteStreamWriterState
        {
            Start,
            StartElement,
            Content,
            EndElement,
            Closed
        }
    }
}
