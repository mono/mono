//Copyright 2010 Microsoft Corporation
//
//Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
//You may obtain a copy of the License at 
//
//http://www.apache.org/licenses/LICENSE-2.0 
//
//Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and limitations under the License.


namespace System.Data.Services.Client
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text;

#if !ASTORIA_LIGHT    
    using System.Net;
#else
    using System.Data.Services.Http;
#endif

    internal class BatchStream : Stream
    {
        private const int DefaultBufferSize = 8000;

        private readonly bool batchRequest;

        private readonly byte[] byteBuffer;

        private Stream reader;

        private int byteLength;

        private int bytePosition;

        private Encoding batchEncoding;

        private bool checkPreamble;

        private string batchBoundary;

        private int batchLength;

        private int totalCount;

        private string changesetBoundary;

        private Encoding changesetEncoding;

        private Dictionary<string, string> contentHeaders;

        private Stream contentStream;

        private bool disposeWithContentStreamDispose;


        private string statusCode;

        private BatchStreamState batchState;

#if DEBUG && !ASTORIA_LIGHT
        private MemoryStream writer = new MemoryStream();
#else
#pragma warning disable 649
        private MemoryStream writer;
#pragma warning restore 649
#endif

        internal BatchStream(Stream stream, string boundary, Encoding batchEncoding, bool requestStream)
        {
            Debug.Assert(null != stream, "null stream");

            this.reader = stream;
            this.byteBuffer = new byte[DefaultBufferSize];
            this.batchBoundary = VerifyBoundary(boundary);
            this.batchState = BatchStreamState.StartBatch;
            this.batchEncoding = batchEncoding;
            this.checkPreamble = (null != batchEncoding);
            this.batchRequest = requestStream;
        }

        #region batch properties ContentHeaders, ContentStream, Encoding, Sate
        public Dictionary<string, string> ContentHeaders
        {
            get { return this.contentHeaders; }
        }

        public Encoding Encoding
        {
            get { return this.changesetEncoding ?? this.batchEncoding; }
        }

        public BatchStreamState State
        {
            get { return this.batchState; }
        }
        #endregion

        #region Stream properties
        public override bool CanRead
        {
            get { return (null != this.reader && this.reader.CanRead); }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override long Length
        {
            get { throw Error.NotSupported(); }
        }

        public override long Position
        {
            get { throw Error.NotSupported(); }
            set { throw Error.NotSupported(); }
        }
        #endregion

        #region Stream methods
        public override void Flush()
        {
            this.reader.Flush();
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw Error.NotSupported();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw Error.NotSupported();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            this.AssertOpen();

            if (offset < 0)
            {
                throw Error.ArgumentOutOfRange("offset");
            }

            if (SeekOrigin.Current != origin)
            {
                throw Error.ArgumentOutOfRange("origin");
            }

            if (Int32.MaxValue == offset)
            {                byte[] buffer = new byte[256];                while (0 < this.ReadDelimiter(buffer, 0, buffer.Length))
                {
                }
            }
            else if (0 < offset)
            {                do
                {
                    int count = Math.Min(checked((int)offset), Math.Min(this.byteLength, this.batchLength));
                    this.totalCount += count;
                    this.bytePosition += count;
                    this.byteLength -= count;
                    this.batchLength -= count;
                    offset -= count;

                }
                while ((0 < offset) && (this.batchLength != 0) && this.ReadBuffer());
            }

            Debug.Assert(0 <= this.byteLength, "negative byteLength");
            Debug.Assert(0 <= this.batchLength, "negative batchLength");
            return 0;
        }

        public override void SetLength(long value)
        {
            throw Error.NotSupported();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw Error.NotSupported();
        }
        #endregion

        internal static bool GetBoundaryAndEncodingFromMultipartMixedContentType(string contentType, out string boundary, out Encoding encoding)
        {
            boundary = null;
            encoding = null;

            string mime;
            KeyValuePair<string, string>[] parameters = HttpProcessUtility.ReadContentType(contentType, out mime, out encoding);

            if (String.Equals(XmlConstants.MimeMultiPartMixed, mime, StringComparison.OrdinalIgnoreCase))
            {
                if (null != parameters)
                {
                    foreach (KeyValuePair<string, string> parameter in parameters)
                    {
                        if (String.Equals(parameter.Key, XmlConstants.HttpMultipartBoundary, StringComparison.OrdinalIgnoreCase))
                        {
                            if (boundary != null)
                            {                                boundary = null;
                                break;
                            }

                            boundary = parameter.Value;
                        }
                    }
                }

                if (String.IsNullOrEmpty(boundary))
                {                    throw Error.BatchStreamMissingBoundary();
                }
            }

            return (null != boundary);
        }


        internal string GetResponseVersion()
        {
            string result;
            this.ContentHeaders.TryGetValue(XmlConstants.HttpDataServiceVersion, out result);
            return result;
        }

        internal HttpStatusCode GetStatusCode()
        {
            return (HttpStatusCode)(null != this.statusCode ? Int32.Parse(this.statusCode, CultureInfo.InvariantCulture) : 500);
        }

        internal bool MoveNext()
        {
            #region dispose previous content stream
            if (null == this.reader || this.disposeWithContentStreamDispose)
            {
                return false;
            }

            if (null != this.contentStream)
            {
                this.contentStream.Dispose();
            }

            Debug.Assert(0 <= this.byteLength, "negative byteLength");
            Debug.Assert(0 <= this.batchLength, "negative batchLength");
            #endregion

            #region initialize start state to EndBatch or EndChangeSet
            switch (this.batchState)
            {
                case BatchStreamState.EndBatch:
                    Debug.Assert(null == this.batchBoundary, "non-null batch boundary");
                    Debug.Assert(null == this.changesetBoundary, "non-null changesetBoundary boundary");
                    throw Error.BatchStreamInvalidBatchFormat();

                case BatchStreamState.Get:
                case BatchStreamState.GetResponse:
                    this.ClearPreviousOperationInformation();
                    goto case BatchStreamState.StartBatch;

                case BatchStreamState.StartBatch:
                case BatchStreamState.EndChangeSet:
                    Debug.Assert(null != this.batchBoundary, "null batch boundary");
                    Debug.Assert(null == this.changesetBoundary, "non-null changeset boundary");
                    this.batchState = BatchStreamState.EndBatch;
                    this.batchLength = Int32.MaxValue;
                    break;

                case BatchStreamState.BeginChangeSet:
                    Debug.Assert(null != this.batchBoundary, "null batch boundary");
                    Debug.Assert(null != this.contentHeaders, "null contentHeaders");
                    Debug.Assert(null != this.changesetBoundary, "null changeset boundary");
                    this.contentHeaders = null;
                    this.changesetEncoding = null;
                    this.batchState = BatchStreamState.EndChangeSet;
                    break;

                case BatchStreamState.ChangeResponse:
                case BatchStreamState.Delete:
                    Debug.Assert(null != this.changesetBoundary, "null changeset boundary");
                    this.ClearPreviousOperationInformation();
                    this.batchState = BatchStreamState.EndChangeSet;
                    break;

                case BatchStreamState.Post:
                case BatchStreamState.Put:
                case BatchStreamState.Merge:
                    Debug.Assert(null != this.changesetBoundary, "null changeset boundary");
                    this.batchState = BatchStreamState.EndChangeSet;
                    break;

                default:
                    Debug.Assert(false, "unknown state");
                    throw Error.BatchStreamInvalidBatchFormat();
            }

            Debug.Assert(null == this.contentHeaders, "non-null content headers");
            Debug.Assert(null == this.contentStream, "non-null content stream");

            Debug.Assert(null == this.statusCode, "non-null statusCode");

            Debug.Assert(
                this.batchState == BatchStreamState.EndBatch ||
                this.batchState == BatchStreamState.EndChangeSet,
                "unexpected state at start");
            #endregion

            #region read --delimiter
            string delimiter = this.ReadLine();
            if (String.IsNullOrEmpty(delimiter))
            {                delimiter = this.ReadLine();
            }

            if (String.IsNullOrEmpty(delimiter))
            {
                throw Error.BatchStreamInvalidBatchFormat();
            }

            if (delimiter.EndsWith("--", StringComparison.Ordinal))
            {
                delimiter = delimiter.Substring(0, delimiter.Length - 2);

                if ((null != this.changesetBoundary) && (delimiter == this.changesetBoundary))
                {
                    Debug.Assert(this.batchState == BatchStreamState.EndChangeSet, "bad changeset boundary state");

                    this.changesetBoundary = null;
                    return true;
                }
                else if (delimiter == this.batchBoundary)
                {
                    if (BatchStreamState.EndChangeSet == this.batchState)
                    {                        throw Error.BatchStreamMissingEndChangesetDelimiter();
                    }

                    this.changesetBoundary = null;
                    this.batchBoundary = null;
                    if (this.byteLength != 0)
                    {
                        throw Error.BatchStreamMoreDataAfterEndOfBatch();
                    }

                    return false;
                }
                else
                {
                    throw Error.BatchStreamInvalidDelimiter(delimiter);
                }
            }
            else if ((null != this.changesetBoundary) && (delimiter == this.changesetBoundary))
            {
                Debug.Assert(this.batchState == BatchStreamState.EndChangeSet, "bad changeset boundary state");
            }
            else if (delimiter == this.batchBoundary)
            {
                if (this.batchState != BatchStreamState.EndBatch)
                {
                    if (this.batchState == BatchStreamState.EndChangeSet)
                    {                        throw Error.BatchStreamMissingEndChangesetDelimiter();
                    }
                    else
                    {
                        throw Error.BatchStreamInvalidBatchFormat();
                    }
                }
            }
            else
            {                throw Error.BatchStreamInvalidDelimiter(delimiter);
            }

            #endregion

            #region read header with values in this form (([^:]*:.*)\r\n)*\r\n
            this.ReadContentHeaders();
            #endregion

            #region should start changeset?
            string contentType;
            bool readHttpHeaders = false;
            if (this.contentHeaders.TryGetValue(XmlConstants.HttpContentType, out contentType))
            {
                if (String.Equals(contentType, XmlConstants.MimeApplicationHttp, StringComparison.OrdinalIgnoreCase))
                {
                    if (this.contentHeaders.Count != 2)
                    {
                        throw Error.BatchStreamInvalidNumberOfHeadersAtOperationStart(
                            XmlConstants.HttpContentType,
                            XmlConstants.HttpContentTransferEncoding);
                    }

                    string transferEncoding;
                    if (!this.contentHeaders.TryGetValue(XmlConstants.HttpContentTransferEncoding, out transferEncoding) ||
                        XmlConstants.BatchRequestContentTransferEncoding != transferEncoding)
                    {
                        throw Error.BatchStreamMissingOrInvalidContentEncodingHeader(
                            XmlConstants.HttpContentTransferEncoding,
                            XmlConstants.BatchRequestContentTransferEncoding);
                    }

                    readHttpHeaders = true;
                }
                else if (BatchStreamState.EndBatch == this.batchState)
                {
                    string boundary;
                    Encoding encoding;
                    if (GetBoundaryAndEncodingFromMultipartMixedContentType(contentType, out boundary, out encoding))
                    {
                        this.changesetBoundary = VerifyBoundary(boundary);
                        this.changesetEncoding = encoding;
                        this.batchState = BatchStreamState.BeginChangeSet;
                    }
                    else
                    {
                        throw Error.BatchStreamInvalidContentTypeSpecified(
                            XmlConstants.HttpContentType,
                            contentType,
                            XmlConstants.MimeApplicationHttp,
                            XmlConstants.MimeMultiPartMixed);
                    }

                    if (this.contentHeaders.Count > 2 ||
                        (this.contentHeaders.Count == 2 && !this.contentHeaders.ContainsKey(XmlConstants.HttpContentLength)))
                    {
                        throw Error.BatchStreamInvalidNumberOfHeadersAtChangeSetStart(XmlConstants.HttpContentType, XmlConstants.HttpContentLength);
                    }
                }
                else
                {
                    throw Error.BatchStreamInvalidContentTypeSpecified(
                        XmlConstants.HttpContentType,
                        contentType,
                        XmlConstants.MimeApplicationHttp,
                        XmlConstants.MimeMultiPartMixed);
                }
            }
            else
            {
                throw Error.BatchStreamMissingContentTypeHeader(XmlConstants.HttpContentType);
            }
            #endregion

            #region what is the operation and uri?
            if (readHttpHeaders)
            {
                this.ReadHttpHeaders();

                this.contentHeaders.TryGetValue(XmlConstants.HttpContentType, out contentType);
            }
            #endregion


            #region does content have a fixed length?
            string text = null;
            int length = -1;
            if (this.contentHeaders.TryGetValue(XmlConstants.HttpContentLength, out text))
            {
                length = Int32.Parse(text, CultureInfo.InvariantCulture);
                if (length < 0)
                {
                    throw Error.BatchStreamInvalidContentLengthSpecified(text);
                }

                if (this.batchState == BatchStreamState.BeginChangeSet)
                {
                    this.batchLength = length;
                }
                else if (length != 0)
                {
                    Debug.Assert(
                        this.batchState == BatchStreamState.Delete ||
                        this.batchState == BatchStreamState.Get ||
                        this.batchState == BatchStreamState.Post ||
                        this.batchState == BatchStreamState.Put ||
                        this.batchState == BatchStreamState.Merge,
                        "unexpected contentlength location");
                    this.contentStream = new StreamWithLength(this, length);
                }
            }
            else
            {
                if (this.batchState == BatchStreamState.EndBatch)
                {
                    this.batchLength = Int32.MaxValue;
                }

                if (this.batchState != BatchStreamState.BeginChangeSet)
                {
                    this.contentStream = new StreamWithDelimiter(this);
                }
            }

            #endregion

            Debug.Assert(
                this.batchState == BatchStreamState.BeginChangeSet ||
                (this.batchRequest && (this.batchState == BatchStreamState.Delete ||
                                       this.batchState == BatchStreamState.Get ||
                                       this.batchState == BatchStreamState.Post ||
                                       this.batchState == BatchStreamState.Put ||
                                       this.batchState == BatchStreamState.Merge)) ||
                (!this.batchRequest && (this.batchState == BatchStreamState.GetResponse ||
                                        this.batchState == BatchStreamState.ChangeResponse)),
                "unexpected state at return");

            #region enforce if contentStream is expected, caller needs to enforce if contentStream is not expected
            if (null == this.contentStream)
            {
                switch (this.batchState)
                {
                    case BatchStreamState.BeginChangeSet:
                    case BatchStreamState.Delete:
                    case BatchStreamState.Get:
                    case BatchStreamState.ChangeResponse:                    case BatchStreamState.GetResponse:                        break;

                    case BatchStreamState.Post:
                    case BatchStreamState.Put:
                    case BatchStreamState.Merge:
                    default:
                        throw Error.BatchStreamContentExpected(this.batchState);
                }
            }
            #endregion

            #region enforce if contentType not is expected, caller needs to enforce if contentType is expected
            if (!String.IsNullOrEmpty(contentType))
            {
                switch (this.batchState)
                {
                    case BatchStreamState.BeginChangeSet:
                    case BatchStreamState.Post:
                    case BatchStreamState.Put:
                    case BatchStreamState.Merge:
                    case BatchStreamState.GetResponse:
                    case BatchStreamState.ChangeResponse:
                        break;

                    case BatchStreamState.Get:                    case BatchStreamState.Delete:                    default:
                        throw Error.BatchStreamContentUnexpected(this.batchState);
                }
            }
            #endregion

            return true;
        }

        internal Stream GetContentStream()
        {
            return this.contentStream;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (null != this.contentStream)
                {                    this.disposeWithContentStreamDispose = true;
                }
                else
                {
                    this.byteLength = 0;
                    if (null != this.reader)
                    {
                        this.reader.Dispose();
                        this.reader = null;
                    }

                    this.contentHeaders = null;
                    if (null != this.contentStream)
                    {
                        this.contentStream.Dispose();
                    }

                    if (null != this.writer)
                    {
                        this.writer.Dispose();
                    }
                }
            }

            base.Dispose(disposing);
        }

        private static BatchStreamState GetStateBasedOnHttpMethodName(string methodName)
        {
            if (XmlConstants.HttpMethodGet.Equals(methodName, StringComparison.Ordinal))
            {
                return BatchStreamState.Get;
            }
            else if (XmlConstants.HttpMethodDelete.Equals(methodName, StringComparison.Ordinal))
            {
                return BatchStreamState.Delete;
            }
            else if (XmlConstants.HttpMethodPost.Equals(methodName, StringComparison.Ordinal))
            {
                return BatchStreamState.Post;
            }
            else if (XmlConstants.HttpMethodPut.Equals(methodName, StringComparison.Ordinal))
            {
                return BatchStreamState.Put;
            }
            else if (XmlConstants.HttpMethodMerge.Equals(methodName, StringComparison.Ordinal))
            {
                return BatchStreamState.Merge;
            }
            else
            {
                throw Error.BatchStreamInvalidHttpMethodName(methodName);
            }
        }

        private static string VerifyBoundary(string boundary)
        {
            if ((null == boundary) || (70 < boundary.Length))
            {
                throw Error.BatchStreamInvalidDelimiter(boundary);
            }

            foreach (char c in boundary)
            {
                if ((127 < (int)c) || Char.IsWhiteSpace(c) || Char.IsControl(c))
                {                    throw Error.BatchStreamInvalidDelimiter(boundary);
                }
            }

            return "--" + boundary;
        }

        private void ClearPreviousOperationInformation()
        {
            this.contentHeaders = null;
            this.contentStream = null;

            this.statusCode = null;
        }

        private void Append(ref byte[] buffer, int count)
        {
            int oldSize = (null != buffer) ? buffer.Length : 0;

            byte[] tmp = new byte[oldSize + count];
            if (0 < oldSize)
            {
                Buffer.BlockCopy(buffer, 0, tmp, 0, oldSize);
            }

            Buffer.BlockCopy(this.byteBuffer, this.bytePosition, tmp, oldSize, count);
            buffer = tmp;

            this.totalCount += count;
            this.bytePosition += count;
            this.byteLength -= count;
            this.batchLength -= count;

            Debug.Assert(0 <= this.byteLength, "negative byteLength");
            Debug.Assert(0 <= this.batchLength, "negative batchLength");
        }

        private void AssertOpen()
        {
            if (null == this.reader)
            {
                Error.ThrowObjectDisposed(this.GetType());
            }
        }

        private bool ReadBuffer()
        {
            this.AssertOpen();

            if (0 == this.byteLength)
            {
                this.bytePosition = 0;
                this.byteLength = this.reader.Read(this.byteBuffer, this.bytePosition, this.byteBuffer.Length);
                if (null != this.writer)
                {
                    this.writer.Write(this.byteBuffer, this.bytePosition, this.byteLength);
                }

                if (null == this.batchEncoding)
                {
                    this.batchEncoding = this.DetectEncoding();
                }
                else if (null != this.changesetEncoding)
                {
                    this.changesetEncoding = this.DetectEncoding();
                }
                else if (this.checkPreamble)
                {
                    bool match = true;
                    byte[] preamble = this.batchEncoding.GetPreamble();
                    if (preamble.Length <= this.byteLength)
                    {
                        for (int i = 0; i < preamble.Length; ++i)
                        {
                            if (preamble[i] != this.byteBuffer[i])
                            {
                                match = false;
                                break;
                            }
                        }

                        if (match)
                        {
                            this.byteLength -= preamble.Length;
                            this.bytePosition += preamble.Length;
                        }
                    }

                    this.checkPreamble = false;
                }

                return (0 < this.byteLength);
            }

            return true;
        }

        private String ReadLine()
        {
            if ((0 == this.batchLength) || !this.ReadBuffer())
            {
                return null;
            }

            byte[] buffer = null;
            do
            {
                Debug.Assert(0 < this.byteLength, "out of bytes");
                Debug.Assert(this.bytePosition + this.byteLength <= this.byteBuffer.Length, "byte tracking out of range");
                int i = this.bytePosition;
                int end = i + Math.Min(this.byteLength, this.batchLength);
                do
                {
                    char ch = (char)this.byteBuffer[i];

                    if (('\r' == ch) || ('\n' == ch))
                    {
                        string s;

                        i -= this.bytePosition;
                        if (null != buffer)
                        {
                            this.Append(ref buffer, i);
                            s = this.Encoding.GetString(buffer, 0, buffer.Length);
                        }
                        else
                        {
                            s = this.Encoding.GetString(this.byteBuffer, this.bytePosition, i);

                            this.totalCount += i;
                            this.bytePosition += i;
                            this.byteLength -= i;
                            this.batchLength -= i;
                        }

                        this.totalCount++;
                        this.bytePosition++;
                        this.byteLength--;
                        this.batchLength--;
                        if (('\r' == ch) && ((0 < this.byteLength) || this.ReadBuffer()) && (0 < this.batchLength))
                        {
                            ch = (char)this.byteBuffer[this.bytePosition];
                            if ('\n' == ch)
                            {
                                this.totalCount++;
                                this.bytePosition++;
                                this.byteLength--;
                                this.batchLength--;
                            }
                        }

                        Debug.Assert(0 <= this.byteLength, "negative byteLength");
                        Debug.Assert(0 <= this.batchLength, "negative batchLength");
                        return s;
                    }

                    i++;
                }
                while (i < end);

                i -= this.bytePosition;
                this.Append(ref buffer, i);
            }
            while (this.ReadBuffer() && (0 < this.batchLength));

            Debug.Assert(0 <= this.byteLength, "negative byteLength");
            Debug.Assert(0 <= this.batchLength, "negative batchLength");
            return this.Encoding.GetString(buffer, 0, buffer.Length);
        }

        private Encoding DetectEncoding()
        {
            if (this.byteLength < 2)
            {
#if !ASTORIA_LIGHT                
                return Encoding.ASCII;
#else
                return HttpProcessUtility.FallbackEncoding;
#endif
            }
            else if (this.byteBuffer[0] == 0xFE && this.byteBuffer[1] == 0xFF)
            {                this.bytePosition = 2;
                this.byteLength -= 2;
                return new UnicodeEncoding(true, true);
            }
            else if (this.byteBuffer[0] == 0xFF && this.byteBuffer[1] == 0xFE)
            {                if (this.byteLength >= 4 &&
                    this.byteBuffer[2] == 0 &&
                    this.byteBuffer[3] == 0)
                {
#if !ASTORIA_LIGHT                    
                this.bytePosition = 4;
                    this.byteLength -= 4;
                    return new UTF32Encoding(false, true);
#else
                    throw Error.NotSupported();
#endif
                }
                else
                {
                    this.bytePosition = 2;
                    this.byteLength -= 2;
                    return new UnicodeEncoding(false, true);
                }
            }
            else if (this.byteLength >= 3 &&
                     this.byteBuffer[0] == 0xEF &&
                     this.byteBuffer[1] == 0xBB &&
                     this.byteBuffer[2] == 0xBF)
            {                this.bytePosition = 3;
                this.byteLength -= 3;
                return Encoding.UTF8;
            }
            else if (this.byteLength >= 4 &&
                     this.byteBuffer[0] == 0 &&
                     this.byteBuffer[1] == 0 &&
                     this.byteBuffer[2] == 0xFE &&
                     this.byteBuffer[3] == 0xFF)
            {
#if !ASTORIA_LIGHT                
                this.bytePosition = 4;
                this.byteLength -= 4;
                return new UTF32Encoding(true, true);
#else
                throw Error.NotSupported();
#endif
            }
            else
            {
#if !ASTORIA_LIGHT                
                return Encoding.ASCII;
#else
                return HttpProcessUtility.FallbackEncoding;
#endif
            }
        }

        private int ReadDelimiter(byte[] buffer, int offset, int count)
        {
            Debug.Assert(null != buffer, "null != buffer");
            Debug.Assert(0 <= offset, "0 <= offset");
            Debug.Assert(0 <= count, "0 <= count");
            Debug.Assert(offset + count <= buffer.Length, "offset + count <= buffer.Length");
            int copied = 0;

            string boundary = null;
            string boundary1 = this.batchBoundary;
            string boundary2 = this.changesetBoundary;

            while ((0 < count) && (0 < this.batchLength) && this.ReadBuffer())
            {
                int boundaryIndex = 0;
                int boundary1Index = 0;
                int boundary2Index = 0;

                int size = Math.Min(Math.Min(count, this.byteLength), this.batchLength) + this.bytePosition;

                byte[] data = this.byteBuffer;
                for (int i = this.bytePosition; i < size; ++i)
                {
                    byte value = data[i];
                    buffer[offset++] = value;
                    if ((char)value == boundary1[boundary1Index])
                    {
                        if (boundary1.Length == ++boundary1Index)
                        {                            size = (1 + i) - boundary1Index;
                            offset -= boundary1Index;
                            Debug.Assert(this.bytePosition <= size, "negative size");
                            break;
                        }
                    }
                    else
                    {
                        boundary1Index = 0;
                    }

                    if ((null != boundary2) && ((char)value == boundary2[boundary2Index]))
                    {
                        if (boundary2.Length == ++boundary2Index)
                        {                            size = (1 + i) - boundary2Index;
                            offset -= boundary2Index;
                            Debug.Assert(this.bytePosition <= size, "negative size");
                            break;
                        }
                    }
                    else
                    {
                        boundary2Index = 0;
                    }
                }

                size -= this.bytePosition;
                Debug.Assert(0 <= size, "negative size");

                if (boundary1Index < boundary2Index)
                {
                    boundaryIndex = boundary2Index;
                    boundary = boundary2;
                }
                else
                {
                    Debug.Assert(null != boundary1, "batch boundary shouldn't be null");
                    boundaryIndex = boundary1Index;
                    boundary = boundary1;
                }

                if (size == this.batchLength)
                {                    boundaryIndex = 0;
                }

                if ((0 < boundaryIndex) && (boundary.Length != boundaryIndex))
                {                    if ((size + copied == boundaryIndex) && (boundaryIndex < this.byteLength))
                    {
                        throw Error.BatchStreamInternalBufferRequestTooSmall();
                    }
                    else
                    {                        size -= boundaryIndex;
                        offset -= boundaryIndex;
                    }
                }

                this.totalCount += size;
                this.bytePosition += size;
                this.byteLength -= size;
                this.batchLength -= size;

                count -= size;
                copied += size;

                if (boundaryIndex > 0 && copied >= 2 && buffer[copied - 2] == '\r' && buffer[copied - 1] == '\n')
                {
                    copied -= 2;
                }

                if (boundary.Length == boundaryIndex)
                {
                    break;
                }
                else if (0 < boundaryIndex)
                {
                    if (boundaryIndex == this.byteLength)
                    {                        if (0 < this.bytePosition)
                        {                            Buffer.BlockCopy(data, this.bytePosition, data, 0, this.byteLength);
                            this.bytePosition = 0;
                        }

                        int tmp = this.reader.Read(this.byteBuffer, this.byteLength, this.byteBuffer.Length - this.byteLength);
                        if (null != this.writer)
                        {
                            this.writer.Write(this.byteBuffer, this.byteLength, tmp);
                        }

                        if (0 == tmp)
                        {                            this.totalCount += boundaryIndex;
                            this.bytePosition += boundaryIndex;
                            this.byteLength -= boundaryIndex;
                            this.batchLength -= boundaryIndex;

                            offset += boundaryIndex;
                            count -= boundaryIndex;
                            copied += boundaryIndex;
                            break;
                        }

                        this.byteLength += tmp;
                    }
                    else
                    {                        break;
                    }
                }
            }

            return copied;
        }

        private int ReadLength(byte[] buffer, int offset, int count)
        {
            Debug.Assert(null != buffer, "null != buffer");
            Debug.Assert(0 <= offset, "0 <= offset");
            Debug.Assert(0 <= count, "0 <= count");
            Debug.Assert(offset + count <= buffer.Length, "offset + count <= buffer.Length");
            int copied = 0;

            if (0 < this.byteLength)
            {                int size = Math.Min(Math.Min(count, this.byteLength), this.batchLength);
                Buffer.BlockCopy(this.byteBuffer, this.bytePosition, buffer, offset, size);
                this.totalCount += size;
                this.bytePosition += size;
                this.byteLength -= size;
                this.batchLength -= size;

                offset += size;
                count -= size;
                copied = size;
            }

            if (0 < count && this.batchLength > 0)
            {                int size = this.reader.Read(buffer, offset, Math.Min(count, this.batchLength));
                if (null != this.writer)
                {
                    this.writer.Write(buffer, offset, size);
                }

                this.totalCount += size;
                this.batchLength -= size;
                copied += size;
            }

            Debug.Assert(0 <= this.byteLength, "negative byteLength");
            Debug.Assert(0 <= this.batchLength, "negative batchLength");
            return copied;
        }

        private void ReadContentHeaders()
        {
            this.contentHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            while (true)
            {
                string line = this.ReadLine();
                if (0 < line.Length)
                {
                    int colon = line.IndexOf(':');
                    if (colon <= 0)
                    {                        throw Error.BatchStreamInvalidHeaderValueSpecified(line);
                    }

                    string name = line.Substring(0, colon).Trim();
                    string value = line.Substring(colon + 1).Trim();
                    this.contentHeaders.Add(name, value);
                }
                else
                {
                    break;
                }
            }
        }

        private void ReadHttpHeaders()
        {
            string line = this.ReadLine();


            int index1 = line.IndexOf(' ');
            if ((index1 <= 0) || ((line.Length - 3) <= index1))
            {
                throw Error.BatchStreamInvalidMethodHeaderSpecified(line);
            }

            int index2 = (this.batchRequest ? line.LastIndexOf(' ') : line.IndexOf(' ', index1 + 1));
            if ((index2 < 0) || (index2 - index1 - 1 <= 0) || ((line.Length - 1) <= index2))
            {
                throw Error.BatchStreamInvalidMethodHeaderSpecified(line);
            }

            string segment1 = line.Substring(0, index1);            string segment2 = line.Substring(index1 + 1, index2 - index1 - 1);            string segment3 = line.Substring(index2 + 1);
            #region validate HttpVersion
            string httpVersion = this.batchRequest ? segment3 : segment1;
            if (httpVersion != XmlConstants.HttpVersionInBatching)
            {
                throw Error.BatchStreamInvalidHttpVersionSpecified(httpVersion, XmlConstants.HttpVersionInBatching);
            }
            #endregion

            this.ReadContentHeaders();

            BatchStreamState state;
            if (this.batchRequest)
            {
                state = GetStateBasedOnHttpMethodName(segment1);
            }
            else
            {
                state = (BatchStreamState.EndBatch == this.batchState) ? BatchStreamState.GetResponse : BatchStreamState.ChangeResponse;
                this.statusCode = segment2;
            }

            #region validate state change
            Debug.Assert(
                BatchStreamState.EndBatch == this.batchState ||
                BatchStreamState.EndChangeSet == this.batchState,
                "unexpected BatchStreamState");

            if (this.batchState == BatchStreamState.EndBatch)
            {
                if ((this.batchRequest && (state == BatchStreamState.Get)) ||
                    (!this.batchRequest && (state == BatchStreamState.GetResponse)))
                {
                    this.batchState = state;
                }
                else
                {
                    throw Error.BatchStreamOnlyGETOperationsCanBeSpecifiedInBatch();
                }
            }
            else if (this.batchState == BatchStreamState.EndChangeSet)
            {
                if ((this.batchRequest && ((BatchStreamState.Post == state) || (BatchStreamState.Put == state) || (BatchStreamState.Delete == state) || (BatchStreamState.Merge == state))) ||
                    (!this.batchRequest && (state == BatchStreamState.ChangeResponse)))
                {
                    this.batchState = state;
                }
                else
                {
                    this.batchState = BatchStreamState.Post;

                    throw Error.BatchStreamGetMethodNotSupportInChangeset();
                }
            }
            else
            {                throw Error.BatchStreamInvalidOperationHeaderSpecified();
            }
            #endregion
        }

        private sealed class StreamWithDelimiter : StreamWithLength
        {
            internal StreamWithDelimiter(BatchStream stream)
                : base(stream, Int32.MaxValue)
            {
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (null == this.Target)
                {
                    Error.ThrowObjectDisposed(this.GetType());
                }

                int result = this.Target.ReadDelimiter(buffer, offset, count);
                return result;
            }
        }

        private class StreamWithLength : Stream
        {
            private BatchStream target;

            private int length;

            internal StreamWithLength(BatchStream stream, int contentLength)
            {
                Debug.Assert(null != stream, "null != stream");
                Debug.Assert(0 < contentLength, "0 < contentLength");
                this.target = stream;
                this.length = contentLength;
            }

            public override bool CanRead
            {
                get { return (null != this.target && this.target.CanRead); }
            }

            public override bool CanSeek
            {
                get { return false; }
            }

            public override bool CanWrite
            {
                get { return false; }
            }

            public override long Length
            {
                get { throw Error.NotSupported(); }
            }

            public override long Position
            {
                get { throw Error.NotSupported(); }
                set { throw Error.NotSupported(); }
            }

            internal BatchStream Target
            {
                get { return this.target; }
            }

            public override void Flush()
            {
            }

#if DEBUG && !ASTORIA_LIGHT            
            public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                throw Error.NotSupported();
            }
#endif

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (null == this.target)
                {
                    Error.ThrowObjectDisposed(this.GetType());
                }

                int result = this.target.ReadLength(buffer, offset, Math.Min(count, this.length));
                this.length -= result;
                Debug.Assert(0 <= this.length, "Read beyond expected length");
                return result;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw Error.NotSupported();
            }

            public override void SetLength(long value)
            {
                throw Error.NotSupported();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw Error.NotSupported();
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                if (disposing && (null != this.target))
                {
                    if (this.target.disposeWithContentStreamDispose)
                    {
                        this.target.contentStream = null;
                        this.target.Dispose();
                    }
                    else if (0 < this.length)
                    {
                        if (null != this.target.reader)
                        {
                            this.target.Seek(this.length, SeekOrigin.Current);
                        }

                        this.length = 0;
                    }

                    this.target.ClearPreviousOperationInformation();
                }

                this.target = null;
            }
        }
    }
}
