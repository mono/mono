//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Runtime;
    using System.Xml;

    abstract class XmlStreamedByteStreamReader : XmlByteStreamReader
    {
        protected XmlStreamedByteStreamReader(XmlDictionaryReaderQuotas quotas)
            : base(quotas)
        {
        }

        public static XmlStreamedByteStreamReader Create(Stream stream, XmlDictionaryReaderQuotas quotas)
        {
            return new StreamXmlStreamedByteStreamReader(stream, quotas);
        }

        public static XmlStreamedByteStreamReader Create(HttpRequestMessage httpRequestMessage, XmlDictionaryReaderQuotas quotas)
        {
            return new HttpRequestMessageStreamedBodyReader(httpRequestMessage, quotas);
        }

        public static XmlStreamedByteStreamReader Create(HttpResponseMessage httpResponseMessage, XmlDictionaryReaderQuotas quotas)
        {
            return new HttpResponseMessageStreamedBodyReader(httpResponseMessage, quotas);
        }

        protected override void OnClose()
        {
            this.ReleaseStream();
            base.OnClose();
        }

        public override int ReadContentAsBase64(byte[] buffer, int index, int count)
        {
            EnsureInContent();
            ByteStreamMessageUtility.EnsureByteBoundaries(buffer, index, count, true);

            if (count == 0)
            {
                return 0;
            }

            Stream stream = this.GetStream();
            int numBytesRead = stream.Read(buffer, index, count);
            if (numBytesRead == 0)
            {
                this.position = ReaderPosition.EndElement;
            }
            return numBytesRead;
        }

        protected override byte[] OnToByteArray()
        {
            throw FxTrace.Exception.AsError(
                  new InvalidOperationException(SR.GetByteArrayFromStreamContentNotAllowed));
        }

        protected override Stream OnToStream()
        {
            Stream result = this.GetStream();

            Fx.Assert(result != null, "The inner stream is null. Please check if the reader is closed or the ToStream method was already called before.");

            this.ReleaseStream();
            return result;
        }

        protected abstract Stream GetStream();

        protected abstract void ReleaseStream();

        public override bool TryGetBase64ContentLength(out int length)
        {
            // in ByteStream encoder, we're not concerned about individual xml nodes
            // therefore we can just return the entire length of the stream
            Stream stream = this.GetStream();
            if (!this.IsClosed && stream.CanSeek)
            {
                long streamLength = stream.Length;
                if (streamLength <= int.MaxValue)
                {
                    length = (int)streamLength;
                    return true;
                }
            }
            length = -1;
            return false;
        }

        class StreamXmlStreamedByteStreamReader : XmlStreamedByteStreamReader
        {
            private Stream stream;

            public StreamXmlStreamedByteStreamReader(Stream stream, XmlDictionaryReaderQuotas quotas)
                : base(quotas)
            {
                Fx.Assert(stream != null, "The 'stream' parameter should not be null.");

                this.stream = stream;
            }

            protected override Stream GetStream()
            {
                return this.stream;
            }

            protected override void ReleaseStream()
            {
                this.stream = null;
            }
        }

        class HttpRequestMessageStreamedBodyReader : XmlStreamedByteStreamReader
        {
            private HttpRequestMessage httpRequestMessage;

            public HttpRequestMessageStreamedBodyReader(HttpRequestMessage httpRequestMessage, XmlDictionaryReaderQuotas quotas)
                : base(quotas)
            {
                Fx.Assert(httpRequestMessage != null, "The 'httpRequestMessage' parameter should not be null.");

                this.httpRequestMessage = httpRequestMessage;
            }

            protected override Stream GetStream()
            {
                if (this.httpRequestMessage == null)
                {
                    return null;
                }

                HttpContent content = this.httpRequestMessage.Content;
                if (content != null)
                {
                    return content.ReadAsStreamAsync().Result;
                }

                return new MemoryStream(EmptyArray<byte>.Instance);
            }

            protected override void ReleaseStream()
            {
                this.httpRequestMessage = null;
            }
        }

        class HttpResponseMessageStreamedBodyReader : XmlStreamedByteStreamReader
        {
            private HttpResponseMessage httpResponseMessage;

            public HttpResponseMessageStreamedBodyReader(HttpResponseMessage httpResponseMessage, XmlDictionaryReaderQuotas quotas)
                : base(quotas)
            {
                Fx.Assert(httpResponseMessage != null, "The 'httpResponseMessage' parameter should not be null.");

                this.httpResponseMessage = httpResponseMessage;
            }

            protected override Stream GetStream()
            {
                if (this.httpResponseMessage == null)
                {
                    return null;
                }

                HttpContent content = this.httpResponseMessage.Content;
                if (content != null)
                {
                    return content.ReadAsStreamAsync().Result;
                }

                return new MemoryStream(EmptyArray<byte>.Instance);
            }

            protected override void ReleaseStream()
            {
                this.httpResponseMessage = null;
            }
        }
    }
}
