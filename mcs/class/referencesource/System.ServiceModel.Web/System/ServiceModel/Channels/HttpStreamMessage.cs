//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

#pragma warning disable 1634 // Stops compiler from warning about unknown warnings (for Presharp)
namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.Runtime;
    using System.ServiceModel;
    using System.Xml;
    using DiagnosticUtility = System.ServiceModel.DiagnosticUtility;

    class HttpStreamMessage : Message
    {
        internal const string StreamElementName = "Binary";
        BodyWriter bodyWriter;
        MessageHeaders headers;
        MessageProperties properties;

        public HttpStreamMessage(BodyWriter writer)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            this.bodyWriter = writer;
            this.headers = new MessageHeaders(MessageVersion.None, 1);
            this.properties = new MessageProperties();
        }

        public HttpStreamMessage(MessageHeaders headers, MessageProperties properties, BodyWriter bodyWriter)
        {
            if (bodyWriter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("bodyWriter");
            }
            this.headers = new MessageHeaders(headers);
            this.properties = new MessageProperties(properties);
            this.bodyWriter = bodyWriter;
        }

        public override MessageHeaders Headers
        {
            get
            {
                if (IsDisposed)
                {
#pragma warning suppress 56503
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateDisposedException());
                }
                return headers;
            }
        }

        public override bool IsEmpty
        {
            get
            {
                return false;
            }
        }

        public override bool IsFault
        {
            get { return false; }
        }

        public override MessageProperties Properties
        {
            get
            {
                if (IsDisposed)
                {
#pragma warning suppress 56503
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateDisposedException());
                }
                return properties;
            }
        }

        public override MessageVersion Version
        {
            get
            {
                if (IsDisposed)
                {
#pragma warning suppress 56503
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateDisposedException());
                }
                return MessageVersion.None;
            }
        }

        protected override void OnBodyToString(XmlDictionaryWriter writer)
        {
            if (this.bodyWriter.IsBuffered)
            {
                bodyWriter.WriteBodyContents(writer);
            }
            else
            {
                writer.WriteString(SR2.GetString(SR2.MessageBodyIsStream));
            }
        }

        protected override void OnClose()
        {
            Exception ex = null;
            try
            {
                base.OnClose();
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                ex = e;
            }

            try
            {
                if (properties != null)
                {
                    properties.Dispose();
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
                if (ex == null)
                {
                    ex = e;
                }
            }

            if (ex != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(ex);
            }

            this.bodyWriter = null;
        }

        protected override MessageBuffer OnCreateBufferedCopy(int maxBufferSize)
        {
            BodyWriter bufferedBodyWriter;
            if (this.bodyWriter.IsBuffered)
            {
                bufferedBodyWriter = this.bodyWriter;
            }
            else
            {
                bufferedBodyWriter = this.bodyWriter.CreateBufferedCopy(maxBufferSize);
            }
            return new HttpStreamMessageBuffer(this.Headers, new MessageProperties(this.Properties), bufferedBodyWriter);
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            this.bodyWriter.WriteBodyContents(writer);
        }

        Exception CreateDisposedException()
        {
            return new ObjectDisposedException("", SR2.GetString(SR2.MessageClosed));
        }

        class HttpStreamMessageBuffer : MessageBuffer
        {
            BodyWriter bodyWriter;
            bool closed;
            MessageHeaders headers;
            MessageProperties properties;
            object thisLock = new object();

            public HttpStreamMessageBuffer(MessageHeaders headers,
                MessageProperties properties, BodyWriter bodyWriter)
                : base()
            {
                this.bodyWriter = bodyWriter;
                this.headers = headers;
                this.properties = properties;
            }

            public override int BufferSize
            {
                get { return 0; }
            }

            object ThisLock
            {
                get { return thisLock; }
            }

            public override void Close()
            {
                lock (ThisLock)
                {
                    if (!closed)
                    {
                        closed = true;
                        bodyWriter = null;
                        headers = null;
                        properties = null;
                    }
                }
            }

            public override Message CreateMessage()
            {
                lock (ThisLock)
                {
                    if (closed)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateDisposedException());
                    }
                    return new HttpStreamMessage(this.headers, this.properties, this.bodyWriter);
                }
            }

            Exception CreateDisposedException()
            {
                return new ObjectDisposedException("", SR2.GetString(SR2.MessageBufferIsClosed));
            }
        }
    }
}
