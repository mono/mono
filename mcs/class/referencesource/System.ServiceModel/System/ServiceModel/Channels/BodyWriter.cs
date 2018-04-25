//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Xml;
    using System.Diagnostics;
    using System.Runtime;
    using System.Threading;

    public abstract class BodyWriter
    {
        bool isBuffered;
        bool canWrite;
        object thisLock;

        protected BodyWriter(bool isBuffered)
        {
            this.isBuffered = isBuffered;
            this.canWrite = true;
            if (!this.isBuffered)
            {
                this.thisLock = new object();
            }
        }

        public bool IsBuffered
        {
            get { return this.isBuffered; }
        }

        internal virtual bool IsEmpty
        {
            get { return false; }
        }

        internal virtual bool IsFault
        {
            get { return false; }
        }

        public BodyWriter CreateBufferedCopy(int maxBufferSize)
        {
            if (maxBufferSize < 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("maxBufferSize", maxBufferSize,
                                                    SR.GetString(SR.ValueMustBeNonNegative)));
            if (this.isBuffered)
            {
                return this;
            }
            else
            {
                lock (this.thisLock)
                {
                    if (!this.canWrite)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.BodyWriterCanOnlyBeWrittenOnce)));
                    this.canWrite = false;
                }
                BodyWriter bodyWriter = OnCreateBufferedCopy(maxBufferSize);
                if (!bodyWriter.IsBuffered)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.BodyWriterReturnedIsNotBuffered)));
                return bodyWriter;
            }
        }

        protected virtual BodyWriter OnCreateBufferedCopy(int maxBufferSize)
        {
            return OnCreateBufferedCopy(maxBufferSize, XmlDictionaryReaderQuotas.Max);
        }

        internal BodyWriter OnCreateBufferedCopy(int maxBufferSize, XmlDictionaryReaderQuotas quotas)
        {
            XmlBuffer buffer = new XmlBuffer(maxBufferSize);
            using (XmlDictionaryWriter writer = buffer.OpenSection(quotas))
            {
                writer.WriteStartElement("a");
                OnWriteBodyContents(writer);
                writer.WriteEndElement();
            }
            buffer.CloseSection();
            buffer.Close();
            return new BufferedBodyWriter(buffer);
        }

        protected abstract void OnWriteBodyContents(XmlDictionaryWriter writer);

        protected virtual IAsyncResult OnBeginWriteBodyContents(XmlDictionaryWriter writer, AsyncCallback callback, object state)
        {
            return new OnWriteBodyContentsAsyncResult(writer, this, callback, state);
        }

        protected virtual void OnEndWriteBodyContents(IAsyncResult result)
        {
            OnWriteBodyContentsAsyncResult.End(result);
        }

        void EnsureWriteBodyContentsState(XmlDictionaryWriter writer)
        {
            if (writer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("writer"));
            if (!this.isBuffered)
            {
                lock (this.thisLock)
                {
                    if (!this.canWrite)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.BodyWriterCanOnlyBeWrittenOnce)));
                    this.canWrite = false;
                }
            }
        }

        public void WriteBodyContents(XmlDictionaryWriter writer)
        {
            EnsureWriteBodyContentsState(writer);
            OnWriteBodyContents(writer);
        }

        public IAsyncResult BeginWriteBodyContents(XmlDictionaryWriter writer, AsyncCallback callback, object state)
        {
            EnsureWriteBodyContentsState(writer);
            return OnBeginWriteBodyContents(writer, callback, state);
        }

        public void EndWriteBodyContents(IAsyncResult result)
        {
            OnEndWriteBodyContents(result);
        }

        class BufferedBodyWriter : BodyWriter
        {
            XmlBuffer buffer;

            public BufferedBodyWriter(XmlBuffer buffer)
                : base(true)
            {
                this.buffer = buffer;
            }

            protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
            {
                XmlDictionaryReader reader = this.buffer.GetReader(0);
                using (reader)
                {
                    reader.ReadStartElement();
                    while (reader.NodeType != XmlNodeType.EndElement)
                    {
                        writer.WriteNode(reader, false);
                    }
                    reader.ReadEndElement();
                }
            }
        }

        class OnWriteBodyContentsAsyncResult : ScheduleActionItemAsyncResult
        {
            BodyWriter bodyWriter;
            XmlDictionaryWriter writer;

            public OnWriteBodyContentsAsyncResult(XmlDictionaryWriter writer, BodyWriter bodyWriter, AsyncCallback callback, object state)
                : base(callback, state)
            {
                Fx.Assert(bodyWriter != null, "bodyWriter should never be null");

                this.writer = writer;
                this.bodyWriter = bodyWriter;

                Schedule();
            }

            protected override void OnDoWork()
            {
                this.bodyWriter.OnWriteBodyContents(this.writer);
            }
        }
    }
}
