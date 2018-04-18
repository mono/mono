//------------------------------------------------------------------------------
// <copyright file="HttpBufferlessInputStream.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Bufferless Input stream used in response and uploaded file objects
 *
 * Copyright (c) 2009 Microsoft Corporation
 */

namespace System.Web {

    using System.IO;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;
    using System.Web.Hosting;
    using System.Web.Configuration;
    using System.Web.Management;
    using System.Web.Util;

    internal class HttpBufferlessInputStream : Stream {
        private long                     _position;
        private long                     _length;
        private long                     _maxRequestLength;
        private bool                     _disableMaxRequestLength;
        private int                      _fileThreshold;
        private bool                     _preloadedContentRead;
        private HttpContext              _context;
        private int                      _preloadedBytesRead;
        private bool                     _persistEntityBody;
        private HttpRawUploadedContent   _rawContent;
        private byte[]                   _buffer;
        private int                      _offset;
        private int                      _count;
        private int                      _remainingBytes;

        internal HttpBufferlessInputStream(HttpContext context, bool persistEntityBody, bool disableMaxRequestLength) {
            _context = context;
            _persistEntityBody = persistEntityBody;
            _disableMaxRequestLength = disableMaxRequestLength;

            // Check max-request-length for preloaded content
            HttpRuntimeSection section = RuntimeConfig.GetConfig(_context).HttpRuntime;
            _maxRequestLength = section.MaxRequestLengthBytes;
            _fileThreshold = section.RequestLengthDiskThresholdBytes;

            if (_persistEntityBody) {
                _rawContent = new HttpRawUploadedContent(_fileThreshold, _context.Request.ContentLength);
            }
            
            int contentLength = _context.Request.ContentLength;
            _remainingBytes = (contentLength > 0) ? contentLength : Int32.MaxValue;
            _length = contentLength;
        }

        internal bool PersistEntityBody { 
            get { 
                return _persistEntityBody; 
            } 
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////

        // A call to Close is required for proper operation of a stream. Normally the
        // raw content will have already been set, but if it was not, we set it now,
        // even if the user of GetBufferedInputStream did not read the entire request
        // entity body.  Since HttpRequest.Dispose will dispose the raw content, this
        // helps to ensure that any temporary files are deleted.
        protected override void Dispose(bool disposing) {
            if (disposing && _persistEntityBody) {
                SetRawContentOnce();
            }
            base.Dispose(disposing);
        }

        public override bool CanRead {
            get {
                return true;
            }
        }

        public override bool CanSeek {
            get {
                return false;
            }
        }

        public override bool CanWrite {
            get {
                return false;
            }
        }

        public override long Length {
            get {
                return _length;
            }
        }

        public override long Position {
            get {
                return _position;
            }
            set {
                throw new NotSupportedException();
            }
        }

        public override void Flush() {
        }

        public override void SetLength(long length) {
            throw new NotSupportedException();
        }
        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            throw new NotSupportedException();
        }

        // Caller may invoke this repeatedly until EndRead returns zero, at which point the entire entity has been read.
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, Object state) {
            HttpWorkerRequest wr = _context.WorkerRequest;
            // Only perform an async read if the worker request supports it and we're not in a cancellable period.
            // If we were to allow async read in a cancellable period, the timeout manager could raise a ThreadAbortEx and
            // corrupt the state of the request.
            if (wr != null && wr.SupportsAsyncRead && !_context.IsInCancellablePeriod) {
                if (!_preloadedContentRead) {
                    if (buffer == null)
                        throw new ArgumentNullException("buffer");
                    if (offset < 0)
                        throw new ArgumentOutOfRangeException("offset");
                    if (count < 0)
                        throw new ArgumentOutOfRangeException("count");
                    if (buffer.Length - offset < count)
                        throw new ArgumentException(SR.GetString(SR.InvalidOffsetOrCount, "offset", "count"));
                    _preloadedBytesRead = GetPreloadedContent(buffer, ref offset, ref count);
                }
                if (_remainingBytes == 0) {
                    // set count to zero and invoke BeginRead to return an async result
                    count = 0;
                }
                if (_persistEntityBody) {
                    // hold a reference so we can add bytes to _rawContent when EndRead is called
                    _buffer = buffer;
                    _offset = offset;
                    _count = count;
                }
                try {
                    return wr.BeginRead(buffer, offset, count, callback, state);
                }
                catch(HttpException) {
                    if (_persistEntityBody) {
                        SetRawContentOnce();
                    }
                    throw;
                }
            }
            else {
                // perform a sync read
                return base.BeginRead(buffer, offset, count, callback, state);
            }
        }
        
        // When this returns zero, the entire entity has been read.
        public override int EndRead(IAsyncResult asyncResult) {
            HttpWorkerRequest wr = _context.WorkerRequest;
            if (wr != null && wr.SupportsAsyncRead && !_context.IsInCancellablePeriod) {
                int totalBytesRead = _preloadedBytesRead;
                if (_preloadedBytesRead > 0) {
                    _preloadedBytesRead = 0;
                }
                int bytesRead = 0;
                try {
                    bytesRead = wr.EndRead(asyncResult);
                }
                catch(HttpException) {
                    if (_persistEntityBody) {
                        SetRawContentOnce();
                    }
                    throw;
                }
                totalBytesRead += bytesRead;
                if (bytesRead > 0) {
                    if (_persistEntityBody) {
                        if (_rawContent != null) {
                            _rawContent.AddBytes(_buffer, _offset, bytesRead);
                        }
                        _buffer = null;
                        _offset = 0;
                        _count = 0;
                    }
                    int dummy1 = 0, dummy2 = 0, dummy3 = 0;
                    UpdateCounters(bytesRead, ref dummy1, ref dummy2, ref dummy3);
                }
                if (_persistEntityBody
                    // we might attempt a read with count == 0, in which case bytesRead will
                    // be zero but we may not be done reading the entity body. Don't set raw
                    // content until bytesRead is 0 and count is not 0 or _remainingBytes is 0
                    && ((bytesRead == 0 && _count != 0) || _remainingBytes == 0)) {
                    SetRawContentOnce();
                }
                return totalBytesRead;
            }
            else {
                return base.EndRead(asyncResult);
            }
        }

        // Caller may invoke this repeatedly until it returns zero, at which point the entire entity has been read.
        public override int Read(byte[] buffer, int offset, int count) {
            HttpWorkerRequest wr = _context.WorkerRequest;
            if (wr == null || count == 0)
                return 0;
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset < 0 || offset + count > buffer.Length)
                throw new ArgumentException(null, "offset");
            if (count < 0)
                throw new ArgumentException(null, "count");

            int totalBytesRead = GetPreloadedContent(buffer, ref offset, ref count);
            int bytesRead = 0;
            // We are done if the count == 0 or there is no more content
            while (count > 0 && _remainingBytes != 0) {
                // Do the actual read
                bytesRead = wr.ReadEntityBody(buffer, offset, count);
                if (bytesRead <= 0) {
                    if (!_context.Response.IsClientConnected) {
                        if (_persistEntityBody) {
                            SetRawContentOnce();
                        }
                        throw new HttpException(SR.GetString(SR.HttpBufferlessInputStream_ClientDisconnected));
                    }
                    break;
                }
                if (_persistEntityBody) {
                    if (_rawContent != null) {
                        _rawContent.AddBytes(buffer, offset, bytesRead);
                    }
                }
                UpdateCounters(bytesRead, ref offset, ref count, ref totalBytesRead);
            }
            if (_persistEntityBody 
                // we might attempt a read with count == 0, in which case bytesRead will
                // be zero but we may not be done reading the entity body. Don't set raw
                // content until bytesRead is 0 and count is not 0 or _remainingBytes is 0
                && ((bytesRead == 0 && count != 0) || _remainingBytes == 0)) {
                SetRawContentOnce();
            }
            return totalBytesRead;
        }

        private int GetPreloadedContent(byte[] buffer, ref int offset, ref int count) {
            if (_preloadedContentRead) {
                return 0;
            }

            // validate once before reading preloaded bytes
            if (_position == 0) {
                ValidateRequestEntityLength();
            }

            int totalBytesRead = 0;
            int preloadedRemaining = 0;            
            byte [] preloadedContent = _context.WorkerRequest.GetPreloadedEntityBody();
            if (preloadedContent != null) {
                // Read preloaded content
                preloadedRemaining = preloadedContent.Length - (int) _position;
                int bytesRead = Math.Min(count, preloadedRemaining);
                Buffer.BlockCopy(preloadedContent, (int) _position, buffer, offset, bytesRead);
                if (_persistEntityBody) {
                    if (_rawContent != null) {
                        _rawContent.AddBytes(preloadedContent, (int) _position, bytesRead);
                    }
                }
                UpdateCounters(bytesRead, ref offset, ref count, ref totalBytesRead);
            }
            // are we done reading preloaded content
            if (totalBytesRead == preloadedRemaining) {
                _preloadedContentRead = true;
                if (_context.WorkerRequest.IsEntireEntityBodyIsPreloaded()) {
                    _remainingBytes = 0;
                }
            }
            return totalBytesRead;
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        // Helper function to increment variables in Read API
        private void UpdateCounters(int bytesRead, ref int offset, ref int count, ref int totalBytesRead) {
            _context.WorkerRequest.UpdateRequestCounters(bytesRead);
            count -= bytesRead;
            offset += bytesRead;
            _position += bytesRead;
            _remainingBytes -= bytesRead;
            totalBytesRead += bytesRead;
            if (_length < _position)
                _length = _position;
            ValidateRequestEntityLength();
        }

        private void ValidateRequestEntityLength() {
            if (!_disableMaxRequestLength && Length > _maxRequestLength) {
                if ( !(_context.WorkerRequest is IIS7WorkerRequest) ) {
                    _context.Response.CloseConnectionAfterError();
                }
                throw new HttpException(SR.GetString(SR.Max_request_length_exceeded), null, WebEventCodes.RuntimeErrorPostTooLarge);
            }
        }

        private void SetRawContentOnce() {
            if (_persistEntityBody && _rawContent != null) {
                _rawContent.DoneAddingBytes();
                _context.Request.SetRawContent(_rawContent);
                _rawContent = null;
            }
        }
    }
}
