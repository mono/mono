//------------------------------------------------------------------------------
// <copyright file="HttpWriter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Response Writer and Stream implementation
 *
 * Copyright (c) 1998 Microsoft Corporation
 */

namespace System.Web {
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Serialization.Formatters;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Web.Util;
    using System.Web.Hosting;

    using IIS = System.Web.Hosting.UnsafeIISMethods;

    //
    //  HttpWriter buffer recycling support
    //

    /*
     * Constants for buffering
     */
    internal static class BufferingParams {
        internal static readonly int INTEGRATED_MODE_BUFFER_SIZE = 16*1024 - 4*IntPtr.Size; // native buffer size for integrated mode
        internal const int OUTPUT_BUFFER_SIZE         = 31*1024;    // output is a chain of this size buffers
        internal const int MAX_FREE_BYTES_TO_CACHE    = 4096;       // don't compress when taking snapshot if free bytes < this
        internal const int MAX_FREE_OUTPUT_BUFFERS    = 64;         // keep this number of unused buffers
        internal const int CHAR_BUFFER_SIZE           = 1024;       // size of the buffers for chat conversion to bytes
        internal const int MAX_FREE_CHAR_BUFFERS      = 64;         // keep this number of unused buffers
        internal const int MAX_BYTES_TO_COPY          = 128;        // copy results of char conversion vs using recycleable buffers
        internal const int MAX_RESOURCE_BYTES_TO_COPY = 4*1024;       // resource strings below this size are copied to buffers
        internal const int INT_BUFFER_SIZE            = 128;        // default size for int[] buffers
        internal const int INTPTR_BUFFER_SIZE         = 128;        // default size for IntPtr[] buffers
    }

    /*
     * Interface implemented by elements of the response buffer list
     */
    internal interface IHttpResponseElement {
        long GetSize();
        byte[] GetBytes();                   // required for filtering
        void Send(HttpWorkerRequest wr);
    }

    /*
     * Base class for recyclable memory buffer elements
     */
    internal abstract class HttpBaseMemoryResponseBufferElement {

        protected int _size;
        protected int _free;
        protected bool _recycle;

        internal int FreeBytes {
            get { return _free;}
        }

        internal void DisableRecycling() {
            _recycle = false;
        }

        // abstract methods

        internal abstract void Recycle();

        internal abstract HttpResponseBufferElement Clone();

        internal abstract int Append(byte[] data, int offset, int size);

        internal abstract int Append(IntPtr data, int offset, int size);

        internal abstract void AppendEncodedChars(char[] data, int offset, int size, Encoder encoder, bool flushEncoder);
    }

    /*
     * Memory response buffer
     */
    internal sealed class HttpResponseBufferElement : HttpBaseMemoryResponseBufferElement, IHttpResponseElement {
        private byte[] _data;

        /*
         * Constructor that accepts the data buffer and holds on to it
         */
        internal HttpResponseBufferElement(byte[] data, int size) {
            _data = data;
            _size = size;
            _free = 0;
            _recycle = false;
        }

        /*
         *  Close the buffer copying the data
         *  (needed to 'compress' buffers for caching)
         */

        internal override HttpResponseBufferElement Clone() {
            int clonedSize = _size - _free;
            byte[] clonedData = new byte[clonedSize];
            Buffer.BlockCopy(_data, 0, clonedData, 0, clonedSize);
            return new HttpResponseBufferElement(clonedData, clonedSize);
        }

        internal override void Recycle() {
            
        }

        internal override int Append(byte[] data, int offset, int size) {
            if (_free == 0 || size == 0)
                return 0;
            int n = (_free >= size) ? size : _free;
            Buffer.BlockCopy(data, offset, _data, _size-_free, n);
            _free -= n;
            return n;
        }

        internal override int Append(IntPtr data, int offset, int size) {
            if (_free == 0 || size == 0)
                return 0;
            int n = (_free >= size) ? size : _free;
            Misc.CopyMemory(data, offset, _data, _size-_free, n);
            _free -= n;
            return n;
        }

        internal override void AppendEncodedChars(char[] data, int offset, int size, Encoder encoder, bool flushEncoder) {
            int byteSize = encoder.GetBytes(data, offset, size, _data, _size-_free, flushEncoder);
            _free -= byteSize;
        }

        //
        // IHttpResponseElement implementation
        //

        /*
         * Get number of bytes
         */
        long IHttpResponseElement.GetSize() {
            return(_size - _free);
        }

        /*
         * Get bytes (for filtering)
         */
        byte[] IHttpResponseElement.GetBytes() {
            return _data;
        }

        /*
         * Write HttpWorkerRequest
         */
        void IHttpResponseElement.Send(HttpWorkerRequest wr) {
            int n = _size - _free;
            if (n > 0)
                wr.SendResponseFromMemory(_data, n);
        }
    }

#if !FEATURE_PAL // FEATURE_PAL does not enable IIS-based hosting features
    /*
     * Unmanaged memory response buffer
     */
    internal sealed class HttpResponseUnmanagedBufferElement : HttpBaseMemoryResponseBufferElement, IHttpResponseElement {
        private IntPtr _data;
        private static IntPtr s_Pool;

        static HttpResponseUnmanagedBufferElement() {
            if (HttpRuntime.UseIntegratedPipeline) {
                s_Pool = IIS.MgdGetBufferPool(BufferingParams.INTEGRATED_MODE_BUFFER_SIZE);
            }
            else {
                s_Pool = UnsafeNativeMethods.BufferPoolGetPool(BufferingParams.OUTPUT_BUFFER_SIZE, 
                    BufferingParams.MAX_FREE_OUTPUT_BUFFERS);

            }          
        }

        /*
         * Constructor that creates an empty buffer
         */
        internal HttpResponseUnmanagedBufferElement() {
            if (HttpRuntime.UseIntegratedPipeline) {
                _data = IIS.MgdGetBuffer(s_Pool);
                _size = BufferingParams.INTEGRATED_MODE_BUFFER_SIZE;
            }
            else {
                _data = UnsafeNativeMethods.BufferPoolGetBuffer(s_Pool);
                _size = BufferingParams.OUTPUT_BUFFER_SIZE;
            }
            if (_data == IntPtr.Zero) {
                throw new OutOfMemoryException();
            }
            _free = _size;
            _recycle = true;
        }

        /*
         * dtor - frees the unmanaged buffer
         */
        ~HttpResponseUnmanagedBufferElement() {
            IntPtr data = Interlocked.Exchange(ref _data, IntPtr.Zero);
            if (data != IntPtr.Zero) {
                if (HttpRuntime.UseIntegratedPipeline) {
                    IIS.MgdReturnBuffer(data);
                }
                else {
                    UnsafeNativeMethods.BufferPoolReleaseBuffer(data);
                }
            }
        }

        /*
         *  Clone the buffer copying the data int managed buffer
         *  (needed to 'compress' buffers for caching)
         */
        internal override HttpResponseBufferElement Clone() {
            int clonedSize = _size - _free;
            byte[] clonedData = new byte[clonedSize];
            Misc.CopyMemory(_data, 0, clonedData, 0, clonedSize);
            return new HttpResponseBufferElement(clonedData, clonedSize);
        }

        internal override void Recycle() {
            if (_recycle)
                ForceRecycle();
        }

        private void ForceRecycle() {
            IntPtr data = Interlocked.Exchange(ref _data, IntPtr.Zero);
            if (data != IntPtr.Zero) {
                _free = 0;
                _recycle = false;
                if (HttpRuntime.UseIntegratedPipeline) {
                    IIS.MgdReturnBuffer(data);
                }
                else {
                    UnsafeNativeMethods.BufferPoolReleaseBuffer(data);
                }
                System.GC.SuppressFinalize(this);
            }
        }

        internal override int Append(byte[] data, int offset, int size) {
            if (_free == 0 || size == 0)
                return 0;
            int n = (_free >= size) ? size : _free;
            Misc.CopyMemory(data, offset, _data, _size-_free, n);
            _free -= n;
            return n;
        }

        internal override int Append(IntPtr data, int offset, int size) {
            if (_free == 0 || size == 0)
                return 0;
            int n = (_free >= size) ? size : _free;
            Misc.CopyMemory(data, offset, _data, _size-_free, n);
            _free -= n;
            return n;
        }
       
        // manually adjust the size
        // used after file reads directly into a buffer
        internal void AdjustSize(int size) {
            _free -= size;
        }

        internal override void AppendEncodedChars(char[] data, int offset, int size, Encoder encoder, bool flushEncoder) {
            int byteSize = UnsafeAppendEncodedChars(data, offset, size, _data, _size - _free, _free, encoder, flushEncoder);
            _free -= byteSize;
#if DBG
            Debug.Trace("UnmanagedBuffers", "Encoding chars, charCount=" + size + ", byteCount=" + byteSize);
#endif
        }

        private unsafe static int UnsafeAppendEncodedChars(char[] src, int srcOffset, int srcSize, IntPtr dest, int destOffset, int destSize, Encoder encoder, bool flushEncoder) {
            int numBytes = 0;

            byte* destBytes = ((byte*)dest) + destOffset;

            fixed (char* charSrc = src) {
                numBytes = encoder.GetBytes(charSrc+srcOffset, srcSize, destBytes, destSize, flushEncoder);
            }

            return numBytes;
        }

        //
        // IHttpResponseElement implementation
        //

        /*
         * Get number of bytes
         */
        long IHttpResponseElement.GetSize() {
            return (_size - _free);
        }

        /*
         * Get bytes (for filtering)
         */
        byte[] IHttpResponseElement.GetBytes() {
            int n = (_size - _free);

            if (n > 0) {
                byte[] data = new byte[n];
                Misc.CopyMemory(_data, 0, data, 0, n);
                return data;
            }
            else {
                return null;
            }
        }

        /*
         * Write HttpWorkerRequest
         */
        void IHttpResponseElement.Send(HttpWorkerRequest wr) {
            int n = _size - _free;

            if (n > 0) {
                wr.SendResponseFromMemory(_data, n, true);
            }

#if DBG
            Debug.Trace("UnmanagedBuffers", "Sending data, byteCount=" + n + ", freeBytes=" + _free);
#endif
        }

        internal unsafe IntPtr FreeLocation {
            get {
                int n = _size - _free;
                byte * p = (byte*) _data.ToPointer();
                p += n;
                
                return new IntPtr(p);
            }
        }
    }

#endif // !FEATURE_PAL
    /*
     * Response element where data comes from resource
     */
    internal sealed class HttpResourceResponseElement : IHttpResponseElement {
        private IntPtr _data;
        private int   _offset;
        private int   _size;

        internal HttpResourceResponseElement(IntPtr data, int offset, int size) {
            _data = data;
            _offset = offset;
            _size = size;
        }

        //
        // IHttpResponseElement implementation
        //

        /*
         * Get number of bytes
         */
        long IHttpResponseElement.GetSize() {
            return _size;
        }

        /*
         * Get bytes (used only for filtering)
         */
        byte[] IHttpResponseElement.GetBytes() {
            if (_size > 0) {
                byte[] data = new byte[_size];
                Misc.CopyMemory(_data, _offset, data, 0, _size);
                return data;
            }
            else {
                return null;
            }
        }

        /*
         * Write HttpWorkerRequest
         */
        void IHttpResponseElement.Send(HttpWorkerRequest wr) {
            if (_size > 0) {
                wr.SendResponseFromMemory(new IntPtr(_data.ToInt64()+_offset), _size, isBufferFromUnmanagedPool: false);
            }
        }
    }

    /*
     * Response element where data comes from file
     */
    internal sealed class HttpFileResponseElement : IHttpResponseElement {
        private String _filename;
        private long   _offset;
        private long   _size;
        private bool   _isImpersonating;
        private bool   _useTransmitFile;
         
        /**
         * Constructor from filename, uses TransmitFile
         */
        internal HttpFileResponseElement(String filename, long offset, long size, bool isImpersonating, bool supportsLongTransmitFile) :
            this (filename, offset, size, isImpersonating, true, supportsLongTransmitFile) {
        }

        /*
         * Constructor from filename and range (doesn't use TransmitFile)
         */
        internal HttpFileResponseElement(String filename, long offset, long size) :
            this (filename, offset, size, false, false, false) {
        }

        private HttpFileResponseElement(string filename,
                                        long offset, 
                                        long size, 
                                        bool isImpersonating, 
                                        bool useTransmitFile,
                                        bool supportsLongTransmitFile)
        {
            if ((!supportsLongTransmitFile && size > Int32.MaxValue) || (size < 0)) {
                throw new ArgumentOutOfRangeException("size", size, SR.GetString(SR.Invalid_size));
            }
            if ((!supportsLongTransmitFile && offset > Int32.MaxValue) || (offset < 0)) {
                throw new ArgumentOutOfRangeException("offset", offset, SR.GetString(SR.Invalid_size));
            }
            _filename = filename;
            _offset = offset;
            _size = size;
            _isImpersonating = isImpersonating;
            _useTransmitFile = useTransmitFile;
        }

        
        internal string FileName { get { return _filename; } }

        internal long   Offset   { get { return _offset; } }

        //
        // IHttpResponseElement implementation
        //

        /*
         * Get number of bytes
         */
        long IHttpResponseElement.GetSize() {
            return _size;
        }

        /*
         * Get bytes (for filtering)
         */
        byte[] IHttpResponseElement.GetBytes() {
            if (_size == 0)
                return null;

            byte[] data = null;
            FileStream f = null;

            try {
                f = new FileStream(_filename, FileMode.Open, FileAccess.Read, FileShare.Read);

                long fileSize = f.Length;

                if (_offset < 0 || _size > fileSize - _offset)
                    throw new HttpException(SR.GetString(SR.Invalid_range));

                if (_offset > 0)
                    f.Seek(_offset, SeekOrigin.Begin);

                int intSize = (int)_size;
                data = new byte[intSize];
                int bytesRead = 0;
                do {
                    int n = f.Read(data, bytesRead, intSize);
                    if (n == 0) {
                        break;
                    }
                    bytesRead += n;
                    intSize -= n;
                } while (intSize > 0);
                // Technically here, the buffer may not be full after the loop, but we choose to ignore
                // this very rare condition (the file became shorter between the time we looked at its length
                // and the moment we read it). In this case, we would just have a few zero bytes at the end
                // of the byte[], which is fine.
            }
            finally {
                if (f != null)
                    f.Close();
            }

            return data;
        }

        /*
         * Write HttpWorkerRequest
         */
        void IHttpResponseElement.Send(HttpWorkerRequest wr) {
            if (_size > 0) {
                if (_useTransmitFile) {
                    wr.TransmitFile(_filename, _offset, _size, _isImpersonating); // This is for IIS 6, in-proc TransmitFile
                }
                else {
                    wr.SendResponseFromFile(_filename, _offset, _size);
                }
            }
        }

    }

    /*
     * Response element for substituiton
     */
    internal sealed class HttpSubstBlockResponseElement : IHttpResponseElement {
        private HttpResponseSubstitutionCallback _callback;
        private IHttpResponseElement _firstSubstitution;
        private IntPtr _firstSubstData;
        private int _firstSubstDataSize;
        private bool _isIIS7WorkerRequest;

        // used by OutputCache
        internal HttpResponseSubstitutionCallback Callback { get { return _callback; } }

        /*
         * Constructor given the name and the data (fill char converted to bytes)
         * holds on to the data
         */
        internal HttpSubstBlockResponseElement(HttpResponseSubstitutionCallback callback, Encoding encoding, Encoder encoder, IIS7WorkerRequest iis7WorkerRequest) {
            _callback = callback;
            if (iis7WorkerRequest != null) {
                _isIIS7WorkerRequest = true;
                String s = _callback(HttpContext.Current);
                if (s == null) {
                    throw new ArgumentNullException("substitutionString");
                }
                CreateFirstSubstData(s, iis7WorkerRequest, encoder);
            }
            else {
                _firstSubstitution = Substitute(encoding);
            }
        }

        // special constructor used by OutputCache
        internal HttpSubstBlockResponseElement(HttpResponseSubstitutionCallback callback) {
            _callback = callback;
        }

        // WOS 1926509: ASP.NET:  WriteSubstitution in integrated mode needs to support callbacks that return String.Empty
        private unsafe void CreateFirstSubstData(String s, IIS7WorkerRequest iis7WorkerRequest, Encoder encoder) {
            Debug.Assert(s != null, "s != null");

            IntPtr pbBuffer;
            int numBytes = 0;
            int cch = s.Length;
            if (cch > 0) {
                fixed (char * pch = s) {
                    int cbBuffer = encoder.GetByteCount(pch, cch, true /*flush*/);
                    pbBuffer = iis7WorkerRequest.AllocateRequestMemory(cbBuffer);
                    if (pbBuffer != IntPtr.Zero) {
                        numBytes = encoder.GetBytes(pch, cch, (byte*)pbBuffer, cbBuffer, true /*flush*/);
                    }
                }
            }
            else {
                // deal with empty string
                pbBuffer = iis7WorkerRequest.AllocateRequestMemory(1);
            }

            if (pbBuffer == IntPtr.Zero) {
                throw new OutOfMemoryException();
            }
            _firstSubstData = pbBuffer;
            _firstSubstDataSize = numBytes;
        }

        /*
         * Performs substition -- return the resulting HttpResponseBufferElement
         * holds on to the data
         */
        internal IHttpResponseElement Substitute(Encoding e) {
            String s = _callback(HttpContext.Current);
            byte[] data = e.GetBytes(s);
            return new HttpResponseBufferElement(data, data.Length);
        }

        internal bool PointerEquals(IntPtr ptr) {
            Debug.Assert(HttpRuntime.UseIntegratedPipeline, "HttpRuntime.UseIntegratedPipeline");
            return _firstSubstData == ptr;
        }

        //
        // IHttpResponseElement implementation (doesn't do anything)
        //

        /*
         * Get number of bytes
         */
        long IHttpResponseElement.GetSize() {
            if (_isIIS7WorkerRequest) {
                return _firstSubstDataSize;
            }
            else {
                return _firstSubstitution.GetSize();
            }
        }

        /*
         * Get bytes (for filtering)
         */
        byte[] IHttpResponseElement.GetBytes() {
            if (_isIIS7WorkerRequest) {
                if (_firstSubstDataSize > 0) {
                    byte[] data = new byte[_firstSubstDataSize];
                    Misc.CopyMemory(_firstSubstData, 0, data, 0, _firstSubstDataSize);
                    return data;
                }
                else {
                    // WOS 1926509: ASP.NET:  WriteSubstitution in integrated mode needs to support callbacks that return String.Empty
                    return (_firstSubstData == IntPtr.Zero) ? null : new byte[0];
                }
            }
            else {
                return _firstSubstitution.GetBytes();
            }
        }

        /*
         * Write HttpWorkerRequest
         */
        void IHttpResponseElement.Send(HttpWorkerRequest wr) {
            if (_isIIS7WorkerRequest) {
                IIS7WorkerRequest iis7WorkerRequest = wr as IIS7WorkerRequest;
                if (iis7WorkerRequest != null) {
                    // buffer can have size of zero if the subst block is an emptry string
                    iis7WorkerRequest.SendResponseFromIISAllocatedRequestMemory(_firstSubstData, _firstSubstDataSize);
                }
            }
            else {
                _firstSubstitution.Send(wr);
            }
        }
    }

    /*
     * Stream object synchronized with Writer
     */
    internal class HttpResponseStream : Stream {
        private HttpWriter _writer;

        internal HttpResponseStream(HttpWriter writer) {
            _writer = writer;
        }

        //
        // Public Stream method implementations
        //

        public override bool CanRead {
            get { return false;}
        }

        public override bool CanSeek {
            get { return false;}
        }

        public override bool CanWrite {
            get { return true;}
        }

        public override long Length {
            get {throw new NotSupportedException();}
        }

        public override long Position {
            get {throw new NotSupportedException();}

            set {throw new NotSupportedException();}
        }

        protected override void Dispose(bool disposing) {
            try {
                if (disposing)
                    _writer.Close();
            }
            finally {
                base.Dispose(disposing);
            }
        }

        public override void Flush() {
            _writer.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotSupportedException();
        }

        public override void SetLength(long value) {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count) {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            if (_writer.IgnoringFurtherWrites) {
                return;
            }

            // Dev10 
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");
            if (buffer.Length - offset < count)
                throw new ArgumentException(SR.GetString(SR.InvalidOffsetOrCount, "offset", "count"));
            if (count == 0)
                return;

            _writer.WriteFromStream(buffer, offset, count);
        }
    }

    /*
     * Stream serving as sink for filters
     */
    internal sealed class HttpResponseStreamFilterSink : HttpResponseStream {
        private bool _filtering = false;

        internal HttpResponseStreamFilterSink(HttpWriter writer) : base(writer) {
        }

        private void VerifyState() {
            // throw exception on unexpected filter writes

            if (!_filtering)
                throw new HttpException(SR.GetString(SR.Invalid_use_of_response_filter));
        }

        internal bool Filtering {
            get { return _filtering;}
            set { _filtering = value;}
        }

        //
        // Stream methods just go to the base class with exception of Close and Flush that do nothing
        //

        protected override void Dispose(bool disposing) {
            // do nothing
            base.Dispose(disposing);
        }

        public override void Flush() {
            // do nothing (this is not a buffering stream)
        }

        public override void Write(byte[] buffer, int offset, int count) {
            VerifyState();
            base.Write(buffer, offset, count);
        }
    }

    /*
     * TextWriter synchronized with the response object
     */

    /// <devdoc>
    ///    <para>A TextWriter class synchronized with the Response object.</para>
    /// </devdoc>
    public sealed class HttpWriter : TextWriter {
        private HttpResponse _response;
        private HttpResponseStream _stream;

        private HttpResponseStreamFilterSink _filterSink;       // sink stream for the filter writes
        private Stream                       _installedFilter;  // installed filtering stream

        private HttpBaseMemoryResponseBufferElement _lastBuffer;
        private ArrayList _buffers;

        private char[] _charBuffer;
        private int _charBufferLength;
        private int _charBufferFree;
        private ArrayList _substElements = null;

        static IAllocatorProvider s_DefaultAllocator = null;
        IAllocatorProvider _allocator = null; // Use only via HttpWriter.AllocationProvider to ensure proper fallback

        // cached data from the response
        // can be invalidated via UpdateResponseXXX methods

        private bool _responseBufferingOn;
        private Encoding _responseEncoding;
        private bool     _responseEncodingUsed;
        private bool     _responseEncodingUpdated;
        private Encoder  _responseEncoder;
        private int      _responseCodePage;
        private bool     _responseCodePageIsAsciiCompat;

        private bool _ignoringFurtherWrites;

        private bool _hasBeenClearedRecently;

        internal HttpWriter(HttpResponse response): base(null) {
            _response = response;
            _stream = new HttpResponseStream(this);

            _buffers = new ArrayList();
            _lastBuffer = null;

            // Setup the buffer on demand using CharBuffer property
            _charBuffer = null;
            _charBufferLength = 0;
            _charBufferFree = 0;

            UpdateResponseBuffering();
            
            // delay getting response encoding until it is really needed
            // UpdateResponseEncoding();
        }

        internal ArrayList SubstElements {
            get {
                if (_substElements == null) {
                    _substElements = new ArrayList();
                    // dynamic compression is not compatible with post cache substitution
                    _response.Context.Request.SetDynamicCompression(false /*enable*/);
                }
                return _substElements;
            }
        }

        /// <devdov>
        /// True if the writer is ignoring all writes
        /// </devdoc>
        internal bool IgnoringFurtherWrites {
            get {
                return _ignoringFurtherWrites;
            }
        }

        /// <devdov>
        /// </devdoc>
        internal void IgnoreFurtherWrites() {
            _ignoringFurtherWrites = true;
        }

        internal void UpdateResponseBuffering() {
            _responseBufferingOn = _response.BufferOutput;
        }

        internal void UpdateResponseEncoding() {
            if (_responseEncodingUpdated) {  // subsequent update
                if (_charBufferLength != _charBufferFree)
                    FlushCharBuffer(true);
            }

            _responseEncoding = _response.ContentEncoding;
            _responseEncoder = _response.ContentEncoder;
            _responseCodePage = _responseEncoding.CodePage;
            _responseCodePageIsAsciiCompat = CodePageUtils.IsAsciiCompatibleCodePage(_responseCodePage);
            _responseEncodingUpdated = true;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override Encoding Encoding {
            get {
                if (!_responseEncodingUpdated) {
                    UpdateResponseEncoding();
                }

                return _responseEncoding;
            }
        }

        internal Encoder Encoder {
            get {
                if (!_responseEncodingUpdated) {
                    UpdateResponseEncoding();
                }
                return _responseEncoder;
            }
        }

        private HttpBaseMemoryResponseBufferElement CreateNewMemoryBufferElement() {
            return new HttpResponseUnmanagedBufferElement(); /* using unmanaged buffers */
        }

    internal void DisposeIntegratedBuffers() {
            Debug.Assert(HttpRuntime.UseIntegratedPipeline);

            // don't recycle char buffers here (ClearBuffers will)
            // do recycle native output buffers
            if (_buffers != null) {

                int n = _buffers.Count;
                for (int i = 0; i < n; i++) {
                    HttpBaseMemoryResponseBufferElement buf = _buffers[i] as HttpBaseMemoryResponseBufferElement;

                    // if this is a native buffer, this will bump down the ref count
                    // the native side also keeps a ref count (see mgdhandler.cxx)
                    if (buf != null) {
                        buf.Recycle();
                    }
                }
                
                _buffers = null;
            }

            // finish by clearing buffers
            ClearBuffers();
    }
        
        internal void RecycleBuffers() {
            // recycle char buffers

            if (_charBuffer != null) {
                AllocatorProvider.CharBufferAllocator.ReuseBuffer(_charBuffer);
                _charBuffer = null;
            }

            // recycle output buffers
            RecycleBufferElements();
        }

        internal static void ReleaseAllPooledBuffers() {
            if (s_DefaultAllocator != null) {
                s_DefaultAllocator.TrimMemory();
            }
        }

        internal void ClearSubstitutionBlocks() {
            _substElements = null;
        }

        internal IAllocatorProvider AllocatorProvider {
            private get {
                if (_allocator == null) {
                    if (s_DefaultAllocator == null) { 
                        // Create default static allocator
                        IBufferAllocator charAllocator = new CharBufferAllocator(BufferingParams.CHAR_BUFFER_SIZE, BufferingParams.MAX_FREE_CHAR_BUFFERS);

                        AllocatorProvider alloc = new AllocatorProvider();
                        alloc.CharBufferAllocator = new BufferAllocatorWrapper<char>(charAllocator);

                        Interlocked.CompareExchange(ref s_DefaultAllocator, alloc, null);
                    }

                    _allocator = s_DefaultAllocator;
                }

                return _allocator;
            }

            set {
                _allocator = value;
            }
        }

        private void RecycleBufferElements() {
            if (_buffers != null) {

                int n = _buffers.Count;
                for (int i = 0; i < n; i++) {
                    HttpBaseMemoryResponseBufferElement buf = _buffers[i] as HttpBaseMemoryResponseBufferElement;
                    if (buf != null) {
                        buf.Recycle();
                    }
                }
                
                _buffers = null;
            }
        }

        private void ClearCharBuffer() {
            _charBufferFree = _charBufferLength;
        }

        private char[] CharBuffer { 
            get {
                if (_charBuffer == null) {
                    _charBuffer = AllocatorProvider.CharBufferAllocator.GetBuffer();

                    _charBufferLength = _charBuffer.Length;
                    _charBufferFree = _charBufferLength;
                }

                return _charBuffer;
            }
        }

        private void FlushCharBuffer(bool flushEncoder) {
            int numChars = _charBufferLength - _charBufferFree;

            Debug.Assert(numChars > 0);

            // remember that response required encoding (to indicate the charset= is needed)
            if (!_responseEncodingUpdated) {
                UpdateResponseEncoding();
            }

            _responseEncodingUsed = true;

            // estimate conversion size
            int estByteSize = _responseEncoding.GetMaxByteCount(numChars);

            if (estByteSize <= BufferingParams.MAX_BYTES_TO_COPY || !_responseBufferingOn) {
                // small size -- allocate intermediate buffer and copy into the output buffer
                byte[] byteBuffer = new byte[estByteSize];
                int realByteSize = _responseEncoder.GetBytes(CharBuffer, 0, numChars,
                                                             byteBuffer, 0, flushEncoder);
                BufferData(byteBuffer, 0, realByteSize, false);
            }
            else {
                // convert right into the output buffer

                int free = (_lastBuffer != null) ? _lastBuffer.FreeBytes : 0;

                if (free < estByteSize) {
                    // need new buffer -- last one doesn't have enough space
                    _lastBuffer = CreateNewMemoryBufferElement();
                    _buffers.Add(_lastBuffer);
                    free = _lastBuffer.FreeBytes;
                }

                // byte buffers must be long enough to keep everything in char buffer
                Debug.Assert(free >= estByteSize);
                _lastBuffer.AppendEncodedChars(CharBuffer, 0, numChars, _responseEncoder, flushEncoder);
            }

            _charBufferFree = _charBufferLength;
        }

        private void BufferData(byte[] data, int offset, int size, bool needToCopyData) {
            int n;

            // try last buffer
            if (_lastBuffer != null) {
                n = _lastBuffer.Append(data, offset, size);
                size -= n;
                offset += n;
            }
            else if (!needToCopyData && offset == 0 && !_responseBufferingOn) {
                // when not buffering, there is no need for big buffer accumulating multiple writes
                // the byte[] data can be sent as is

                _buffers.Add(new HttpResponseBufferElement(data, size));
                return;
            }

            // do other buffers if needed
            while (size > 0) {
                _lastBuffer = CreateNewMemoryBufferElement();
                _buffers.Add(_lastBuffer);
                n = _lastBuffer.Append(data, offset, size);
                offset += n;
                size -= n;
            }
        }

        private void BufferResource(IntPtr data, int offset, int size) {
            if (size > BufferingParams.MAX_RESOURCE_BYTES_TO_COPY || !_responseBufferingOn) {
                // for long response strings create its own buffer element to avoid copy cost
                // also, when not buffering, no need for an extra copy (nothing will get accumulated anyway)
                _lastBuffer = null;
                _buffers.Add(new HttpResourceResponseElement(data, offset, size));
                return;
            }

            int n;

            // try last buffer
            if (_lastBuffer != null) {
                n = _lastBuffer.Append(data, offset, size);
                size -= n;
                offset += n;
            }

            // do other buffers if needed
            while (size > 0) {
                _lastBuffer = CreateNewMemoryBufferElement();
                _buffers.Add(_lastBuffer);
                n = _lastBuffer.Append(data, offset, size);
                offset += n;
                size -= n;
            }
        }

        //
        // 'Write' methods to be called from other internal classes
        //

        internal void WriteFromStream(byte[] data, int offset, int size) {
            if (_charBufferLength != _charBufferFree)
                FlushCharBuffer(true);

            BufferData(data, offset, size, true);

            if (!_responseBufferingOn)
                _response.Flush();
        }

        internal void WriteUTF8ResourceString(IntPtr pv, int offset, int size, bool asciiOnly) {

            if (!_responseEncodingUpdated) {
                UpdateResponseEncoding();
            }

            if (_responseCodePage == CodePageUtils.CodePageUT8 || // response encoding is UTF8
                (asciiOnly && _responseCodePageIsAsciiCompat)) {  // ASCII resource and ASCII-compat encoding

                _responseEncodingUsed = true;  // note the we used encoding (means that we need to generate charset=) see RAID#93415

                // write bytes directly
                if (_charBufferLength != _charBufferFree)
                    FlushCharBuffer(true);

                BufferResource(pv, offset, size);

                if (!_responseBufferingOn)
                    _response.Flush();
            }
            else {
                // have to re-encode with response's encoding -- use public Write(String)
                Write(StringResourceManager.ResourceToString(pv, offset, size));
            }
        }

        internal void TransmitFile(string filename, long offset, long size, bool isImpersonating, bool supportsLongTransmitFile) {
            if (_charBufferLength != _charBufferFree)
                FlushCharBuffer(true);
            
            _lastBuffer = null;
            _buffers.Add(new HttpFileResponseElement(filename, offset, size, isImpersonating, supportsLongTransmitFile));
            
            if (!_responseBufferingOn)
                _response.Flush();
        }

        internal void WriteFile(String filename, long offset, long size) {
            if (_charBufferLength != _charBufferFree)
                FlushCharBuffer(true);

            _lastBuffer = null;
            _buffers.Add(new HttpFileResponseElement(filename, offset, size));

            if (!_responseBufferingOn)
                _response.Flush();
        }

        //
        // Support for substitution blocks
        //

        internal void WriteSubstBlock(HttpResponseSubstitutionCallback callback, IIS7WorkerRequest iis7WorkerRequest) {
            if (_charBufferLength != _charBufferFree)
                FlushCharBuffer(true);
            _lastBuffer = null;

            // add new substitution block to the buffer list
            IHttpResponseElement element = new HttpSubstBlockResponseElement(callback, Encoding, Encoder, iis7WorkerRequest);
            _buffers.Add(element);

            if (iis7WorkerRequest != null) {
                SubstElements.Add(element);
            }

            if (!_responseBufferingOn)
                _response.Flush();
        }

        //
        // Support for response buffer manipulation: HasBeenClearedRecently, GetResponseBufferCountAfterFlush,
        // and MoveResponseBufferRangeForward.  The intended use of these functions is to rearrange 
        // the order of the buffers.  Improper use of these functions may result in excessive memory use.
        // They were added specifically so that custom hidden form data could be moved to the beginning
        // of the form.

        internal bool HasBeenClearedRecently {
            get {
                return _hasBeenClearedRecently;
            }
            set {
                _hasBeenClearedRecently = value;
            }
        }

        // Gets the response buffer count after flushing the char buffer.  Note that _lastBuffer is cleared,
        // and therefore may not be filled, so calling this can lead to inefficient use of response buffers.
        internal int GetResponseBufferCountAfterFlush() {
            if (_charBufferLength != _charBufferFree) {
                FlushCharBuffer(true);
            }

            // set _lastBuffer to null to prevent more data from being added to it
            _lastBuffer = null;

            return _buffers.Count;
        }

        // Move the specified range of buffers forward in the buffer list.
        internal void MoveResponseBufferRangeForward(int srcIndex, int srcCount, int dstIndex) {
            Debug.Assert(dstIndex <= srcIndex);

            // DevDiv Bugs 154630: No need to copy the form between temporary array and the buffer list when
            // no hidden fields are written.
            if (srcCount > 0) {
                // create temporary storage for buffers that will be moved backwards
                object[] temp = new object[srcIndex - dstIndex];

                // copy buffers that will be moved backwards
                _buffers.CopyTo(dstIndex, temp, 0, temp.Length);

                // move the range forward from srcIndex to dstIndex
                for (int i = 0; i < srcCount; i++) {
                    _buffers[dstIndex + i] = _buffers[srcIndex + i];
                }

                // insert buffers that were placed in temporary storage
                for (int i = 0; i < temp.Length; i++) {
                    _buffers[dstIndex + srcCount + i] = temp[i];
                }
            }

            // set _lastBuffer
            HttpBaseMemoryResponseBufferElement buf = _buffers[_buffers.Count-1] as HttpBaseMemoryResponseBufferElement;
            if (buf != null && buf.FreeBytes > 0) {
                _lastBuffer = buf;
            }
            else {
                _lastBuffer = null;
            }
        }

        //
        // Buffer management
        //

        internal void ClearBuffers() {
            ClearCharBuffer();

            // re-enable dynamic compression if we are about to clear substitution blocks
            if (_substElements != null) {
                _response.Context.Request.SetDynamicCompression(true /*enable*/);
            }

            //VSWhidbey 559434: Private Bytes goes thru roof because unmanaged buffers are not recycled when Response.Flush is called
            RecycleBufferElements();

            _buffers = new ArrayList();
            _lastBuffer = null;
            _hasBeenClearedRecently = true;
        }

        internal long GetBufferedLength() {
            if (_charBufferLength != _charBufferFree)
                FlushCharBuffer(true);

            long size = 0;
            if (_buffers != null) {
                int n = _buffers.Count;
                for (int i = 0; i < n; i++) {
                    size += ((IHttpResponseElement)_buffers[i]).GetSize();
                }
            }
            return size;
        }

        internal bool ResponseEncodingUsed {
            get { return _responseEncodingUsed; }
        }

        // in integrated mode, snapshots need to pull the chunks from the IIS
        // buffers since they may have already been pushed through
        // Therefore, we can't rely solely on what's in the HttpWriter
        // at the moment
        internal ArrayList GetIntegratedSnapshot(out bool hasSubstBlocks, IIS7WorkerRequest wr) {
            ArrayList buffers = null;

            // first, get what's in our buffers
            ArrayList writerBuffers = GetSnapshot(out hasSubstBlocks);

            // now, get what's in the IIS buffers
            ArrayList nativeBuffers = wr.GetBufferedResponseChunks(true, _substElements, ref hasSubstBlocks);
                 
            // try to append the current buffers to what we just
            // got from the native buffer
            if (null != nativeBuffers) {
                for (int i = 0; i < writerBuffers.Count; i++) {
                    nativeBuffers.Add(writerBuffers[i]);
                }
                buffers = nativeBuffers;
            }
            else {
                buffers = writerBuffers;
            }

            // if we have substitution blocks:
            // 1) throw exception if someone modified the subst blocks
            // 2) re-enable compression
            if (_substElements != null && _substElements.Count > 0) {
                int substCount = 0;
                // scan buffers for subst blocks
                for(int i = 0; i < buffers.Count; i++) {
                    if (buffers[i] is HttpSubstBlockResponseElement) {
                        substCount++;
                        if (substCount == _substElements.Count) {
                            break;
                        }
                    }
                }
                
                if (substCount != _substElements.Count) {
                    throw new InvalidOperationException(SR.GetString(SR.Substitution_blocks_cannot_be_modified));
                }

                // re-enable dynamic compression when we have a snapshot of the subst blocks.
                _response.Context.Request.SetDynamicCompression(true /*enable*/);
            }

            return buffers;
        }
        
        //
        //  Snapshot for caching
        //        

        internal ArrayList GetSnapshot(out bool hasSubstBlocks) {
            if (_charBufferLength != _charBufferFree)
                FlushCharBuffer(true);

            _lastBuffer = null; // to make sure nothing gets appended after

            hasSubstBlocks = false;

            ArrayList buffers = new ArrayList();

            // copy buffer references to the returned list, make non-recyclable
            int n = _buffers.Count;
            for (int i = 0; i < n; i++) {
                Object responseElement = _buffers[i];

                HttpBaseMemoryResponseBufferElement buffer = responseElement as HttpBaseMemoryResponseBufferElement;

                if (buffer != null) {
                    if (buffer.FreeBytes > BufferingParams.MAX_FREE_BYTES_TO_CACHE) {
                        // copy data if too much is free
                        responseElement = buffer.Clone();
                    }
                    else {
                        // cache the buffer as is with free bytes
                        buffer.DisableRecycling();
                    }
                }
                else if (responseElement is HttpSubstBlockResponseElement) {
                    hasSubstBlocks = true;
                }

                buffers.Add(responseElement);
            }
            return buffers;
        }

        internal void UseSnapshot(ArrayList buffers) {
            ClearBuffers();

            // copy buffer references to the internal buffer list
            // make substitution if needed

            int n = buffers.Count;
            for (int i = 0; i < n; i++) {
                Object responseElement = buffers[i];
                HttpSubstBlockResponseElement substBlock = (responseElement as HttpSubstBlockResponseElement);

                if (substBlock != null) {
                    _buffers.Add(substBlock.Substitute(Encoding));
                }
                else {
                    _buffers.Add(responseElement);
                }
            }
        }

        //
        //  Support for response stream filters
        //

        internal Stream GetCurrentFilter() {
            if (_installedFilter != null)
                return _installedFilter;

            if (_filterSink == null)
                _filterSink = new HttpResponseStreamFilterSink(this);

            return _filterSink;
        }

        internal bool FilterInstalled {
            get { return (_installedFilter != null); }
        }

        internal void InstallFilter(Stream filter) {
            if (_filterSink == null)  // have to redirect to the sink -- null means sink wasn't ever asked for
                throw new HttpException(SR.GetString(SR.Invalid_response_filter));

            _installedFilter = filter;
        }

        internal void Filter(bool finalFiltering) {
            // no filter?
            if (_installedFilter == null)
                return;

            // flush char buffer and remember old buffers
            if (_charBufferLength != _charBufferFree)
                FlushCharBuffer(true);

            _lastBuffer = null;

            // no content to filter
            // Allow the filter to be closed (Dev10 
            if (_buffers.Count == 0 && !finalFiltering)
                return;

            // remember old buffers
            ArrayList oldBuffers = _buffers;
            _buffers = new ArrayList();

            // push old buffer list through the filter

            Debug.Assert(_filterSink != null);

            _filterSink.Filtering = true;

            try {
                int n = oldBuffers.Count;
                for (int i = 0; i < n; i++) {
                    IHttpResponseElement buf = (IHttpResponseElement)oldBuffers[i];

                    long len = buf.GetSize();

                    if (len > 0) {
                        // Convert.ToInt32 will throw for sizes larger than Int32.MaxValue.
                        // Filtering large response sizes is not supported
                        _installedFilter.Write(buf.GetBytes(), 0, Convert.ToInt32(len));
                    }
                }

                _installedFilter.Flush();

            }
            finally {
                try {
                    if (finalFiltering)
                        _installedFilter.Close();
                }
                finally {
                    _filterSink.Filtering = false;
                }
            }
        }

        internal void FilterIntegrated(bool finalFiltering, IIS7WorkerRequest wr) {
            // no filter?
            if (_installedFilter == null)
                return;

            // flush char buffer and remember old buffers
            if (_charBufferLength != _charBufferFree)
                FlushCharBuffer(true);

            _lastBuffer = null;

            // ISAPI mode bails if it has no buffers
            // to filter, in integrated mode we need
            // to check the unified response buffers
            // maintained by IIS for content, as well

            // remember current buffers (if any) that might be
            // response entity from this transition 
            // (not yet pushed through to IIS response buffers)
            ArrayList oldBuffers = _buffers;
            _buffers = new ArrayList();
            
            // now, get what's in the IIS buffers
            ArrayList nativeBuffers = null;            
            bool fDummy = false;
            nativeBuffers = wr.GetBufferedResponseChunks(false, null, ref fDummy);
            
            Debug.Assert(_filterSink != null);
            _filterSink.Filtering = true;
           
            try {
                // push buffers through installed filters
                // push the IIS ones through first since we need to maintain order
                if (null != nativeBuffers) {
                    for (int i = 0; i < nativeBuffers.Count; i++) {
                        IHttpResponseElement buf = (IHttpResponseElement)nativeBuffers[i];

                        long len = buf.GetSize();

                        if (len > 0)
                            _installedFilter.Write(buf.GetBytes(), 0, Convert.ToInt32(len));

                    }

                    // if we had stuff there, we now need to clear it since we may have
                    // transformed it
                    wr.ClearResponse(true /* entity */, false /* headers */);
                }

                // current buffers, if any
                if (null != oldBuffers) {
                    for (int i = 0; i < oldBuffers.Count; i++) {
                        IHttpResponseElement buf = (IHttpResponseElement)oldBuffers[i];

                        long len = buf.GetSize();

                        if (len > 0)
                            _installedFilter.Write(buf.GetBytes(), 0, Convert.ToInt32(len));

                    }
                }

                _installedFilter.Flush();
            }
            finally {
                try {
                    if (finalFiltering)
                        _installedFilter.Close();
                }
                finally {
                    _filterSink.Filtering = false;
                }
            }
        }

        //
        //  Send via worker request
        //

        internal void Send(HttpWorkerRequest wr) {
            if (_charBufferLength != _charBufferFree)
                FlushCharBuffer(true);

            int n = _buffers.Count;

            if (n > 0) {
                // write data
                for (int i = 0; i < n; i++) {
                    ((IHttpResponseElement)_buffers[i]).Send(wr);
                }
            }
        }

        //
        // Public TextWriter method implementations
        //


        /// <devdoc>
        ///    <para> Sends all buffered output to the client and closes the socket connection.</para>
        /// </devdoc>
        public override void Close() {
            // don't do anything (this could called from a wrapping text writer)
        }


        /// <devdoc>
        ///    <para> Sends all buffered output to the client.</para>
        /// </devdoc>
        public override void Flush() {
            // don't flush the response
        }


        /// <devdoc>
        ///    <para> Sends a character to the client.</para>
        /// </devdoc>
        public override void Write(char ch) {
            if (_ignoringFurtherWrites) {
                return;
            }

            char[] buffer = CharBuffer;

            if (_charBufferFree == 0) {
                FlushCharBuffer(false);
            }

            buffer[_charBufferLength - _charBufferFree] = ch;
            _charBufferFree--;

            if (!_responseBufferingOn) {
                _response.Flush();
            }
        }


        /// <devdoc>
        ///    <para> Sends a stream of buffered characters to the client
        ///       using starting position and number of characters to send. </para>
        /// </devdoc>
        public override void Write(char[] buffer, int index, int count) {
            if (_ignoringFurtherWrites) {
                return;
            }

            // Dev10 
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (index < 0)
                throw new ArgumentOutOfRangeException("index");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");
            if (buffer.Length - index < count)
                throw new ArgumentException(SR.GetString(SR.InvalidOffsetOrCount, "index", "count"));
            if (count == 0)
                return;

            char[] charBuffer = CharBuffer;

            while (count > 0) {
                if (_charBufferFree == 0) {
                    FlushCharBuffer(false);
                }

                int n = (count < _charBufferFree) ? count : _charBufferFree;
                System.Array.Copy(buffer, index, charBuffer, _charBufferLength - _charBufferFree, n);
                _charBufferFree -= n;
                index += n;
                count -= n;
            }

            if (!_responseBufferingOn) {
                _response.Flush();
            }
        }


        /// <devdoc>
        ///    <para>Sends a string to the client.</para>
        /// </devdoc>
        public override void Write(String s) {
            if (_ignoringFurtherWrites)
                return;

            if (s == null)
                return;

            char[] buffer = CharBuffer;

            if (s.Length == 0) {
                // Ensure flush if string is empty
            }
            else if (s.Length < _charBufferFree) {
                // fast path - 99% of string writes will not overrun the buffer
                // avoid redundant arg checking in string.CopyTo
                StringUtil.UnsafeStringCopy(s, 0, buffer, _charBufferLength - _charBufferFree, s.Length);
                _charBufferFree -= s.Length;
            }
            else {
                int count = s.Length;
                int index = 0;
                int n;

                while (count > 0) {
                    if (_charBufferFree == 0) {
                        FlushCharBuffer(false);
                    }

                    n = (count < _charBufferFree) ? count : _charBufferFree;

                    // avoid redundant arg checking in string.CopyTo
                    StringUtil.UnsafeStringCopy(s, index, buffer, _charBufferLength - _charBufferFree, n);

                    _charBufferFree -= n;
                    index += n;
                    count -= n;
                }
            }

            if (!_responseBufferingOn) {
                _response.Flush();
            }
        }


        /// <devdoc>
        ///    <para>Sends a string or a sub-string to the client.</para>
        /// </devdoc>
        public void WriteString(String s, int index, int count) {
            if (s == null)
                return;

            if (index < 0) {
                throw new ArgumentOutOfRangeException("index");
            }

            if (count < 0) {
                throw new ArgumentOutOfRangeException("count");
            }

            if (index + count > s.Length) {
                throw new ArgumentOutOfRangeException("index");
            }

            if (_ignoringFurtherWrites) {
                return;
            }

            char[] buffer = CharBuffer;

            if (count == 0) {
                // Ensure flush if string is empty
            }
            else if (count < _charBufferFree) {
                // fast path - 99% of string writes will not overrun the buffer
                // avoid redundant arg checking in string.CopyTo
                StringUtil.UnsafeStringCopy(s, index, buffer, _charBufferLength - _charBufferFree, count);
                _charBufferFree -= count;
            }
            else {
                int n;
    
                while (count > 0) {
                    if (_charBufferFree == 0) {
                        FlushCharBuffer(false);
                    }

                    n = (count < _charBufferFree) ? count : _charBufferFree;
    
                    // avoid redundant arg checking in string.CopyTo
                    StringUtil.UnsafeStringCopy(s, index, buffer, _charBufferLength - _charBufferFree, n);

                    _charBufferFree -= n;
                    index += n;
                    count -= n;
                }
            }

            if (!_responseBufferingOn) {
                _response.Flush();
            }
        }


        /// <devdoc>
        ///    <para>Sends an object to the client.</para>
        /// </devdoc>
        public override void Write(Object obj) {
            if (_ignoringFurtherWrites) {
                return;
            }

            if (obj != null)
                Write(obj.ToString());
        }

        //
        // Support for binary data
        //


        /// <devdoc>
        ///    <para>Sends a buffered stream of bytes to the client.</para>
        /// </devdoc>
        public void WriteBytes(byte[] buffer, int index, int count) {
            if (_ignoringFurtherWrites) {
                return;
            }

            WriteFromStream(buffer, index, count);
        }


        /// <devdoc>
        ///    <para>Writes out a CRLF pair into the the stream.</para>
        /// </devdoc>
        public override void WriteLine() {
            if (_ignoringFurtherWrites) {
                return;
            }

            // It turns out this is way more efficient than the TextWriter version of
            // WriteLine which ends up calling Write with a 2 char array

            char[] buffer = CharBuffer;

            if (_charBufferFree < 2)
                FlushCharBuffer(false);

            int pos = _charBufferLength - _charBufferFree;
            buffer[pos] = '\r';
            buffer[pos + 1] = '\n';
            _charBufferFree -= 2;

            if (!_responseBufferingOn)
                _response.Flush();
        }

        /*
         * The Stream for writing binary data
         */

        /// <devdoc>
        ///    <para> Enables binary output to the client.</para>
        /// </devdoc>
            public Stream OutputStream {
            get { return _stream;}
        }

    }
}
