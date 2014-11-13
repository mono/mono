//------------------------------------------------------------------------------
// <copyright file="ISAPIWorkerRequest.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Hosting {
    using System.Text;
    using System.Configuration.Assemblies;
    using System.Runtime.InteropServices;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Security.Authentication.ExtendedProtection;
    using System.IO;
    using System.Globalization;
    using System.Threading;
    using Microsoft.Win32;
    using System.Web;
    using System.Web.Management;
    using System.Web.Util;
    using System.Web.Configuration;
    using System.Web.Caching;

//
// recyclable buffers for IntPtr[] and int[]
// to avoid pinning gen0
//

internal class RecyclableArrayHelper {
    private const int ARRAY_SIZE = 128;
    private const int MAX_FREE_ARRAYS = 64;
    private static IntegerArrayAllocator s_IntegerArrayAllocator;
    private static IntPtrArrayAllocator s_IntPtrArrayAllocator;

    static RecyclableArrayHelper() {
        s_IntegerArrayAllocator = new IntegerArrayAllocator(ARRAY_SIZE, MAX_FREE_ARRAYS);
        s_IntPtrArrayAllocator  = new IntPtrArrayAllocator(ARRAY_SIZE, MAX_FREE_ARRAYS);
    }

    internal static int[] GetIntegerArray(int minimumLength) {
        if( minimumLength <= ARRAY_SIZE )
            return(int[])s_IntegerArrayAllocator.GetBuffer();
        else
            return new int[minimumLength];
    }

    internal static IntPtr[] GetIntPtrArray(int minimumLength) {
        if( minimumLength <= ARRAY_SIZE )
            return(IntPtr[])s_IntPtrArrayAllocator.GetBuffer();
        else
            return new IntPtr[minimumLength];
    }

    internal static void ReuseIntegerArray(int[] array) {
        if (array != null && array.Length == ARRAY_SIZE)
            s_IntegerArrayAllocator.ReuseBuffer(array);
    }

    internal static void ReuseIntPtrArray(IntPtr[] array) {
        if (array != null && array.Length == ARRAY_SIZE)
            s_IntPtrArrayAllocator.ReuseBuffer(array);
    }
}

//
// char[] appendable buffer. Recyclable up to 1K
// Also encapsulates encoding (using utf-8) into recyclable byte[] buffer.
//
// Usage:
//      new RecyclableCharBuffer
//      Append
//      ...
//      GetEncodedBytesBuffer
//      Dispose
//

internal class RecyclableCharBuffer {
    private const int BUFFER_SIZE       = 1024;
    private const int MAX_FREE_BUFFERS  = 64;
    private static CharBufferAllocator  s_CharBufferAllocator;
    private static UbyteBufferAllocator s_ByteBufferAllocator;

    private char[]  _charBuffer;
    private int     _size;
    private int     _freePos;
    private bool    _recyclable;


    private byte[]  _byteBuffer;

    static RecyclableCharBuffer() {
        s_CharBufferAllocator = new CharBufferAllocator(BUFFER_SIZE, MAX_FREE_BUFFERS);
        s_ByteBufferAllocator = new UbyteBufferAllocator(Encoding.UTF8.GetMaxByteCount(BUFFER_SIZE), MAX_FREE_BUFFERS);
    }

    internal RecyclableCharBuffer() {
        _charBuffer = (char[])s_CharBufferAllocator.GetBuffer();
        _size = _charBuffer.Length;
        _freePos = 0;
        _recyclable = true;
    }

    internal void Dispose() {
        if (_recyclable) {
            if (_charBuffer != null)
                s_CharBufferAllocator.ReuseBuffer(_charBuffer);

            if (_byteBuffer != null)
                s_ByteBufferAllocator.ReuseBuffer(_byteBuffer);
        }

        _charBuffer = null;
        _byteBuffer = null;
    }

    private void Grow(int newSize) {
        if (newSize <= _size)
            return;

        if (newSize < _size*2)
            newSize = _size*2;

        char[] newBuffer = new char[newSize];

        if (_freePos > 0)
            Array.Copy(_charBuffer, newBuffer, _freePos);

        _charBuffer = newBuffer;
        _size = newSize;
        _recyclable = false;
    }

    internal void Append(char ch) {
        if (_freePos >= _size)
            Grow(_freePos+1);

        _charBuffer[_freePos++] = ch;
    }

    internal void Append(String s) {
        int l = s.Length;
        int newFreePos = _freePos + l;

        if (newFreePos > _size)
            Grow(newFreePos);

        s.CopyTo(0, _charBuffer, _freePos, l);
        _freePos = newFreePos;
    }

    internal byte[] GetEncodedBytesBuffer() {
        return GetEncodedBytesBuffer(Encoding.UTF8);
    }

    internal byte[] GetEncodedBytesBuffer(Encoding encoding) {
        if (_byteBuffer != null)
            return _byteBuffer;

        if (encoding == null)
            encoding = Encoding.UTF8;

        // null terminate

        Append('\0');

        // convert to bytes

        if (_recyclable) {
            // still using the original recyclable char buffer
            // -- can use recyclable byte buffer

            _byteBuffer = (byte[])s_ByteBufferAllocator.GetBuffer();

            if (_freePos > 0)
                encoding.GetBytes(_charBuffer, 0, _freePos, _byteBuffer, 0);
        }
        else {
            _byteBuffer = encoding.GetBytes(_charBuffer, 0, _freePos);
        }

        return _byteBuffer;
    }

    public override String ToString() {
        return (_charBuffer != null && _freePos > 0) ? new String(_charBuffer, 0, _freePos) : null;
    }
}

//
// byte[] buffer of encoded chars bytes. Recyclable up to 4K
// Also encapsulates decoding into recyclable char[] buffer.
//
// Usage:
//      new RecyclableByteBuffer
//      fill .Buffer up
//      GetDecodedTabSeparatedStrings
//      Dispose
//

internal class RecyclableByteBuffer {
    private const int BUFFER_SIZE       = 4096;
    private const int MAX_FREE_BUFFERS  = 64;
    private static UbyteBufferAllocator s_ByteBufferAllocator;
    private static CharBufferAllocator  s_CharBufferAllocator;

    private int     _offset;
    private byte[]  _byteBuffer;
    private bool    _recyclable;

    private char[]  _charBuffer;

    static RecyclableByteBuffer() {
        s_ByteBufferAllocator = new UbyteBufferAllocator(BUFFER_SIZE, MAX_FREE_BUFFERS);
        s_CharBufferAllocator = new CharBufferAllocator(BUFFER_SIZE, MAX_FREE_BUFFERS);
    }

    internal RecyclableByteBuffer() {
        _byteBuffer = (byte[])s_ByteBufferAllocator.GetBuffer();
        _recyclable = true;
    }

    internal void Dispose() {
        if (_recyclable) {
            if (_byteBuffer != null)
                s_ByteBufferAllocator.ReuseBuffer(_byteBuffer);

            if (_charBuffer != null)
                s_CharBufferAllocator.ReuseBuffer(_charBuffer);
        }

        _byteBuffer = null;
        _charBuffer = null;
    }

    internal byte[] Buffer {
        get { return _byteBuffer; }
    }

    internal void Resize(int newSize) {
        _byteBuffer = new byte[newSize];
        _recyclable = false;
    }

    private void Skip(int count) {
        if (count <= 0)
            return;

        // adjust offset
        int l = _byteBuffer.Length;
        int c = 0;

        for (int i = 0; i < l; i++) {
            if (_byteBuffer[i] == (byte)'\t') {
                if (++c == count) {
                    _offset = i+1;
                    return;
                }
            }
        }
    }


    private int CalcLength()
    {
        // calculate null termitated length

        if (_byteBuffer != null) {
            int l = _byteBuffer.Length;

            for (int i = _offset; i < l; i++) {
                if (_byteBuffer[i] == 0)
                    return i - _offset;
            }
        }

        return 0;
    }

    private char[] GetDecodedCharBuffer(Encoding encoding, ref int len) {
        if (_charBuffer != null)
            return _charBuffer;

        if (len == 0) {
            _charBuffer = new char[0];
        }
        else if (_recyclable) {
            _charBuffer = (char[])s_CharBufferAllocator.GetBuffer();
            len = encoding.GetChars(_byteBuffer, _offset, len, _charBuffer, 0);
        }
        else {
            _charBuffer = encoding.GetChars(_byteBuffer, _offset, len);
            len = _charBuffer.Length;
        }

        return _charBuffer;
    }

    internal string GetDecodedString(Encoding encoding, int len) {
        return encoding.GetString(_byteBuffer, 0, len);
    }

    internal String[] GetDecodedTabSeparatedStrings(Encoding encoding, int numStrings, int numSkipStrings) {
        if (numSkipStrings > 0)
            Skip(numSkipStrings);

        int len = CalcLength();
        char[] s = GetDecodedCharBuffer(encoding, ref len);

        String[] ss = new String[numStrings];

        int iStart = 0;
        int iEnd;
        int foundStrings = 0;

        for (int iString = 0; iString < numStrings; iString++) {
            iEnd = len;

            for (int i = iStart; i < len; i++) {
                if (s[i] == '\t') {
                    iEnd = i;
                    break;
                }
            }

            if (iEnd > iStart)
                ss[iString] = new String(s, iStart, iEnd-iStart);
            else
                ss[iString] = String.Empty;

            foundStrings++;

            if (iEnd == len)
                break;

            iStart = iEnd+1;
        }

        if (foundStrings < numStrings) {
            len = CalcLength();
            iStart = _offset;

            for (int iString = 0; iString < numStrings; iString++) {
                iEnd = len;

                for (int i = iStart; i < len; i++) {
                    if (_byteBuffer[i] == (byte)'\t') {
                        iEnd = i;
                        break;
                    }
                }

                if (iEnd > iStart)
                    ss[iString] = encoding.GetString(_byteBuffer, iStart, iEnd-iStart);
                else
                    ss[iString] = String.Empty;

                if (iEnd == len)
                    break;

                iStart = iEnd+1;
            }

        }

        return ss;
    }
}


//
// class to encapsulate writing from byte[], IntPtr (resource or filehandle)
//

internal enum BufferType: byte {
    Managed = 0,
    UnmanagedPool = 1,
    IISAllocatedRequestMemory = 2,
    TransmitFile = 3
}

internal class MemoryBytes {
    private int         _size;
    private byte[]      _arrayData;
    private GCHandle    _pinnedArrayData;
    private IntPtr      _intptrData;
    private long        _fileSize;
    private IntPtr      _fileHandle;
    private string      _fileName;
    private long        _offset;
    private BufferType  _bufferType; // 0 managed, 1 native pool, 2 IIS allocated request memory, 3 TransmitFile

    internal MemoryBytes(string fileName, long offset, long fileSize) {
        _bufferType = BufferType.TransmitFile;
        _intptrData = IntPtr.Zero;
        _fileHandle = IntPtr.Zero;
        _fileSize = fileSize;
        _fileName = fileName;
        _offset = offset;
        // _cachedResponseBodyLength will be wrong if we don't set _size now.
        _size = IntPtr.Size;
    }

    internal MemoryBytes(byte[] data, int size): this(data, size, false, 0) {
    }

    internal MemoryBytes(byte[] data, int size, bool useTransmitFile, long fileSize) {
        _size = size;
        _arrayData = data;
        _intptrData = IntPtr.Zero;
        _fileHandle = IntPtr.Zero;
        if (useTransmitFile) {
            _bufferType = BufferType.TransmitFile;
        }
        _fileSize = fileSize;
    }

    internal MemoryBytes(IntPtr data, int size, BufferType bufferType) {
        _size = size;
        _arrayData = null;
        _intptrData = data;
        _fileHandle = IntPtr.Zero;
        _bufferType = bufferType;
    }

    internal long FileSize {
        get { return _fileSize; }
    }

    internal bool IsBufferFromUnmanagedPool {
        get { return _bufferType == BufferType.UnmanagedPool; }
    }

    internal BufferType BufferType {
        get { return _bufferType; }
    }

    internal int Size {
        get { return _size; }
    }

    internal bool UseTransmitFile {
        get { return _bufferType == BufferType.TransmitFile; }
    }

    private void CloseHandle() {
        if (_fileHandle != IntPtr.Zero && _fileHandle != UnsafeNativeMethods.INVALID_HANDLE_VALUE) {
            UnsafeNativeMethods.CloseHandle(_fileHandle);
            // don't allow 'this' to be GC'd before CloseHandle returns.
            _fileHandle = IntPtr.Zero;
        }
    }

    private static byte[] IntPtrToBytes(IntPtr p, long offset, long length) {
        // This method converts the given pointer and offset to a byte[] representation
        // of the C struct EcbFileAndOffset (32 and 64-bit specific):
        //
        // struct FileAndOffset
        // {
        //     ULONGLONG cbOffset;
        //     ULONGLONG cbLength;
        //     HANDLE hFile;
        // }
        //

        byte[] bytes = new byte[2 * sizeof(long) + IntPtr.Size];

        // Put the offset value first
        for (int i = 0; i < 8; i++)
            bytes[i] = (byte)((offset >> 8*i) & 0xFF );

        // Put the file value next
        for (int i = 0; i < 8; i++)
            bytes[8+i] = (byte)((length >> 8*i) & 0xFF );
        
        if (IntPtr.Size == 4) {
            int temp = p.ToInt32();
            for (int i = 0; i < 4; i++)
                bytes[16+i] = (byte)((temp >> 8*i) & 0xFF );
        }  else {
            long temp = p.ToInt64();
            for (int i = 0; i < 8; i++)
                bytes[16+i] = (byte)((temp >> 8*i) & 0xFF );
        }
        return bytes;
    }

    private void SetHandle() {
        if (_fileName != null) {
            _fileHandle = UnsafeNativeMethods.GetFileHandleForTransmitFile(_fileName);
        }
        if (_fileHandle != IntPtr.Zero) {
            _arrayData = IntPtrToBytes(_fileHandle, _offset, _fileSize);
        }
    }

    internal IntPtr LockMemory() {
        SetHandle();
        if (_arrayData != null) {
            _pinnedArrayData = GCHandle.Alloc(_arrayData, GCHandleType.Pinned);
            return Marshal.UnsafeAddrOfPinnedArrayElement(_arrayData, 0);
        }
        else {
            return _intptrData;
        }
    }

    internal void UnlockMemory() {
        CloseHandle();
        if (_arrayData != null) {
            _pinnedArrayData.Free();
        }
    }
}

//
// recyclable pinnable char[] buffer to get Unicode server variables
//
// Usage:
//      new ServerVarCharBuffer
//      get PinnedAddress, Length
//      [Resize]
//      Dispose
//

internal class ServerVarCharBuffer {
    private const int BUFFER_SIZE       = 1024;
    private const int MAX_FREE_BUFFERS  = 64;
    private static CharBufferAllocator  s_CharBufferAllocator;

    private bool        _recyclable;
    private char[]      _charBuffer;
    private bool        _pinned;
    private GCHandle    _pinnedCharBufferHandle;
    private IntPtr      _pinnedAddr;

    static ServerVarCharBuffer() {
        s_CharBufferAllocator = new CharBufferAllocator(BUFFER_SIZE, MAX_FREE_BUFFERS);
    }

    internal ServerVarCharBuffer() {
        _charBuffer = (char[])s_CharBufferAllocator.GetBuffer();
        _recyclable = true;
    }

    internal void Dispose() {
        if (_pinned) {
            _pinnedCharBufferHandle.Free();
            _pinned = false;
        }

        if (_recyclable) {
            if (_charBuffer != null)
                s_CharBufferAllocator.ReuseBuffer(_charBuffer);
        }

        _charBuffer = null;
    }

    internal IntPtr PinnedAddress {
        get {
            if (!_pinned) {
                _pinnedCharBufferHandle = GCHandle.Alloc(_charBuffer, GCHandleType.Pinned);
                _pinnedAddr = Marshal.UnsafeAddrOfPinnedArrayElement(_charBuffer, 0);
                _pinned = true;
            }

            return _pinnedAddr;
        }
    }

    internal int Length {
        get {
            return _charBuffer.Length;
        }
    }

    internal void Resize(int newSize) {
        if (_pinned) {
            _pinnedCharBufferHandle.Free();
            _pinned = false;
        }

        _charBuffer = new char[newSize];
        _recyclable = false;
    }
}

//
// Async IO completion callback from IIS
//
internal delegate void ISAPIAsyncCompletionCallback(IntPtr ecb, int byteCount, int error);

internal delegate void AsyncCompletionCallback(int bytesCompleted, int hresult, IntPtr pbAsyncReceiveBuffer);

//
// Implementation of HttpWorkerRequest based on ECB
//
internal abstract class ISAPIWorkerRequest : HttpWorkerRequest {

    protected IntPtr _ecb;     // ECB as integer
    protected IntPtr _token;   // user token as integer
    protected Guid _traceId;   // ETW traceId
    protected AsyncResultBase _asyncResultBase;
    protected AsyncCompletionCallback _asyncCompletionCallback;

    // Request data obtained during initialization (basics)

    protected String _method;
    protected String _path;
    protected String _filePath;
    protected String _pathInfo;
    protected String _pathTranslated;
    protected String _appPath;
    protected String _appPathTranslated;

    protected int _contentType;
    protected int _contentTotalLength;
    protected int _contentAvailLength;

    protected int _queryStringLength;

    protected bool _ignoreMinAsyncSize;
    protected bool _requiresAsyncFlushCallback;

    // Request data obtained later on

    private bool _preloadedContentRead;
    private byte[] _preloadedContent;

    private bool _requestHeadersAvailable;
    private String[][] _unknownRequestHeaders;
    private String[] _knownRequestHeaders;

    private bool      _clientCertFetched;
    private DateTime  _clientCertValidFrom;
    private DateTime  _clientCertValidUntil;
    private byte []   _clientCert;
    private int       _clientCertEncoding;
    private byte []   _clientCertPublicKey;
    private byte []   _clientCertBinaryIssuer;

    // Outgoing headers storage

    private bool _headersSent;
    private Encoding _headerEncoding;
    private bool _contentLengthSent;
    private bool _chunked;
    private RecyclableCharBuffer _headers = new RecyclableCharBuffer();
    private RecyclableCharBuffer _status  = new RecyclableCharBuffer();
    private bool _statusSet = true;

    // Outgoing data cached for a single FlushCore

    private byte[]      _cachedResponseStatus;
    private byte[]      _cachedResponseHeaders;
    private int         _cachedResponseKeepConnected;
    private int         _cachedResponseBodyLength;
    private ArrayList   _cachedResponseBodyBytes;
    private int         _cachedResponseBodyBytesIoLockCount;

    // Notification about the end of IO

    private HttpWorkerRequest.EndOfSendNotification _endOfRequestCallback;
    private Object                                  _endOfRequestCallbackArg;
    private int                                     _endOfRequestCallbackLockCount;

    //  Constants for posted content type

    private const int CONTENT_NONE = 0;
    private const int CONTENT_FORM = 1;
    private const int CONTENT_MULTIPART = 2;
    private const int CONTENT_OTHER = 3;

    //
    // ISAPI status constants (for DoneWithSession)
    //

    private const int STATUS_SUCCESS = 1;
    private const int STATUS_SUCCESS_AND_KEEP_CONN = 2;
    private const int STATUS_PENDING = 3;
    private const int STATUS_ERROR = 4;

    //
    // Private helpers
    //

    private String[] ReadBasics(int[] contentInfo) {
        // call getbasics

        RecyclableByteBuffer buf = new RecyclableByteBuffer();

        int r = GetBasicsCore(buf.Buffer, buf.Buffer.Length, contentInfo);

        while (r < 0) {
            buf.Resize(-r);     // buffer not big enough
            r = GetBasicsCore(buf.Buffer, buf.Buffer.Length, contentInfo);
        }

        if (r == 0)
            throw new HttpException(SR.GetString(SR.Cannot_retrieve_request_data));

        // convert to characters and split the buffer into strings

        String[] ss = buf.GetDecodedTabSeparatedStrings(Encoding.Default, 6, 0);

        // recycle buffers

        buf.Dispose();

        return ss;
    }

    private static readonly char[] s_ColonOrNL = { ':', '\n' };

    private void ReadRequestHeaders() {
        if (_requestHeadersAvailable)
            return;

        _knownRequestHeaders = new String[RequestHeaderMaximum];

        // construct unknown headers as array list of name1,value1,...

        ArrayList headers = new ArrayList();

        String s = GetServerVariable("ALL_RAW");
        int l = (s != null) ? s.Length : 0;
        int i = 0;

        while (i < l)
        {
            //  find next :

            int ci = s.IndexOfAny(s_ColonOrNL, i);

            if (ci < 0)
                break;

            if (s[ci] == '\n') {
                // ignore header without :
                i = ci+1;
                continue;
            }

            if (ci == i) {
                i++;
                continue;
            }

            // add extract name
            String name = s.Substring(i, ci-i).Trim();

            //  find next \n
            int ni = s.IndexOf('\n', ci+1);
            if (ni < 0)
                ni = l;

            while (ni < l-1 && s[ni+1] == ' ')  {   // continuation of header (ASURT 115064)
                ni = s.IndexOf('\n', ni+1);
                if (ni < 0)
                    ni = l;
            }

            // extract value
            String value = s.Substring(ci+1, ni-ci-1).Trim();

            // remember
            int knownIndex = GetKnownRequestHeaderIndex(name);
            if (knownIndex >= 0) {
                _knownRequestHeaders[knownIndex] = value;
            }
            else {
                headers.Add(name);
                headers.Add(value);
            }

            i = ni+1;
        }

        // copy to array unknown headers

        int n = headers.Count / 2;
        _unknownRequestHeaders = new String[n][];
        int j = 0;

        for (i = 0; i < n; i++) {
            _unknownRequestHeaders[i] = new String[2];
            _unknownRequestHeaders[i][0] = (String)headers[j++];
            _unknownRequestHeaders[i][1] = (String)headers[j++];
        }

        _requestHeadersAvailable = true;
    }

    private void SendHeaders() {
        if (!_headersSent) {
            if (_statusSet) {
                _headers.Append("\r\n");

                AddHeadersToCachedResponse(
                    _status.GetEncodedBytesBuffer(),
                    _headers.GetEncodedBytesBuffer(_headerEncoding),
                    (_contentLengthSent || _chunked) ? 1 : 0);

                _headersSent = true;
            }
        }
    }

    private void SendResponseFromFileStream(FileStream f, long offset, long length)  {
        long fileSize = f.Length;

        if (length == -1)
            length = fileSize - offset;

        if (offset < 0 || length > fileSize - offset)
            throw new HttpException(SR.GetString(SR.Invalid_range));

        if (length > 0) {
            if (offset > 0)
                f.Seek(offset, SeekOrigin.Begin);

            byte[] fileBytes = new byte[(int)length];
            int bytesRead = f.Read(fileBytes, 0, (int)length);
            if (bytesRead > 0)
                AddBodyToCachedResponse(new MemoryBytes(fileBytes, bytesRead));
        }
    }

    private void ResetCachedResponse() {
        _cachedResponseStatus = null;
        _cachedResponseHeaders = null;
        _cachedResponseBodyLength = 0;
        _cachedResponseBodyBytes = null;

        // DDBugs 162981: ASP.NET leaks requests when page calls TransmitFile and Flush
        // This happens because FlushCachedResponse may set _requiresAsyncFlushCallback and 
        // _ignoreMinAsyncSize to true and then it "forgets" to reset them after Flush is done. 
        // When the final flush is being executed it uses incorrect values 
        // to determine that it needs an async completion during the final flush.
        // The fix is to reset async flags after each Flush
        _requiresAsyncFlushCallback = false;
        _ignoreMinAsyncSize = false;
    }

    private void AddHeadersToCachedResponse(byte[] status, byte[] header, int keepConnected) {
        _cachedResponseStatus = status;
        _cachedResponseHeaders = header;
        _cachedResponseKeepConnected = keepConnected;
    }

    private void AddBodyToCachedResponse(MemoryBytes bytes) {
        if (_cachedResponseBodyBytes == null)
            _cachedResponseBodyBytes = new ArrayList();
        _cachedResponseBodyBytes.Add(bytes);
        _cachedResponseBodyLength += bytes.Size;
    }

    internal void UnlockCachedResponseBytesOnceAfterIoComplete() {
        if (Interlocked.Decrement(ref _cachedResponseBodyBytesIoLockCount) == 0) {
            // unlock pinned memory
            if (_cachedResponseBodyBytes != null) {
                int numFragments = _cachedResponseBodyBytes.Count;
                for (int i = 0; i < numFragments; i++) {
                    try {
                        ((MemoryBytes)_cachedResponseBodyBytes[i]).UnlockMemory();
                    }
                    catch {
                    }
                }
            }

            // don't remember cached data anymore
            ResetCachedResponse();

            FlushAsyncResult flushAsyncResult = _asyncResultBase as FlushAsyncResult;
            if (flushAsyncResult != null) {
                _endOfRequestCallbackLockCount--;
                _asyncCompletionCallback(0, flushAsyncResult.HResult, IntPtr.Zero);
            }
        }
    }

    // ISAPIWorkerRequest
    private void FlushCachedResponse(bool isFinal) {
        if (_ecb == IntPtr.Zero)
            return;

        bool        asyncFlush = false;
        int         numFragments = 0;
        IntPtr[]    fragments = null;
        int[]       fragmentLengths = null;
        long        bytesOut = 0;

        try {
            // prepare body fragments as IntPtr[] of pointers and int[] of lengths
            if (_cachedResponseBodyLength > 0) {
                numFragments = _cachedResponseBodyBytes.Count;
                fragments = RecyclableArrayHelper.GetIntPtrArray(numFragments);
                fragmentLengths = RecyclableArrayHelper.GetIntegerArray(numFragments);

                for (int i = 0; i < numFragments; i++) {
                    MemoryBytes bytes = (MemoryBytes)_cachedResponseBodyBytes[i];
                    fragments[i] = bytes.LockMemory();

                    if (!isFinal || !bytes.IsBufferFromUnmanagedPool)
                        _requiresAsyncFlushCallback = true;

                    if (bytes.UseTransmitFile) {
                        fragmentLengths[i] = -bytes.Size; // use negative length for TransmitFile
                        _ignoreMinAsyncSize = true;
                        bytesOut += bytes.FileSize;
                    }
                    else {
                        fragmentLengths[i] = bytes.Size;
                        bytesOut += bytes.Size;
                    }
                }
            }

            // prepare doneWithSession and finalStatus
            int doneWithSession = isFinal ? 1 : 0;
            int finalStatus = isFinal ? ((_cachedResponseKeepConnected != 0) ? STATUS_SUCCESS_AND_KEEP_CONN : STATUS_SUCCESS) : 0;

            // set the count to two - one for return from FlushCore and one for async IO completion
            // the cleanup should happen on the later of the two
            _cachedResponseBodyBytesIoLockCount = 2;

            // increment the lock count controlling end of request callback
            // so that the callback would be called at the later of EndRequest
            // and the async IO completion
            // (doesn't need to be interlocked as only one thread could start the IO)
            _endOfRequestCallbackLockCount++;

            if (isFinal)
                PerfCounters.DecrementCounter(AppPerfCounter.REQUESTS_EXECUTING);

            // perf counters are DWORDs, so it makes no sense to update REQUEST_BYTES_OUT with a value greater than Int32.MaxValue
            int delta = (int) bytesOut;
            if (delta > 0) {
                PerfCounters.IncrementCounterEx(AppPerfCounter.REQUEST_BYTES_OUT, delta);
            }

            try {
                // send to unmanaged code
                FlushCore(
                    _cachedResponseStatus, _cachedResponseHeaders, _cachedResponseKeepConnected,
                    _cachedResponseBodyLength, numFragments, fragments, fragmentLengths,
                    doneWithSession, finalStatus, out asyncFlush);
            }
            finally {
                if (isFinal) {
                    Close();
                    _ecb = IntPtr.Zero;
                }
            }
        }
        finally {
            // in case of synchronous IO adjust down the lock counts
            if (!asyncFlush) {
                _cachedResponseBodyBytesIoLockCount--;
                _endOfRequestCallbackLockCount--;
            }

            // unlock pinned memory
            UnlockCachedResponseBytesOnceAfterIoComplete();

            // recycle buffers
            RecyclableArrayHelper.ReuseIntPtrArray(fragments);
            RecyclableArrayHelper.ReuseIntegerArray(fragmentLengths);
        }
    }

    internal void CallEndOfRequestCallbackOnceAfterAllIoComplete() {
        if (_endOfRequestCallback != null) {
            // only call the callback on the latest of EndRequest and async IO completion
            if (Interlocked.Decrement(ref _endOfRequestCallbackLockCount) == 0) {
                try {
                    _endOfRequestCallback(this, _endOfRequestCallbackArg);
                }
                catch {
                }
            }
        }
    }

    //
    // ctor
    //

    internal ISAPIWorkerRequest(IntPtr ecb) {
        _ecb = ecb;
        PerfCounters.IncrementCounter(AppPerfCounter.REQUESTS_TOTAL);
    }

    public override Guid RequestTraceIdentifier {
        get { return _traceId; }
    }

    internal IntPtr Ecb {
        get {
            return _ecb;
        }
    }

    internal void Initialize() {
        // setup basic values

        ReadRequestBasics();

        if (_appPathTranslated != null && _appPathTranslated.Length > 2 && !StringUtil.StringEndsWith(_appPathTranslated, '\\'))
            _appPathTranslated += "\\";  // IIS 6.0 doesn't add the trailing '\'

        // Increment incoming request length
        PerfCounters.IncrementCounterEx(AppPerfCounter.REQUEST_BYTES_IN, _contentTotalLength);
    }

    internal virtual void ReadRequestBasics() {

        // Get requests basics

        int[] contentInfo = new int[4];
        String[] basicStrings = ReadBasics(contentInfo);

        if (basicStrings == null || basicStrings.Length != 6)
            throw new HttpException(SR.GetString(SR.Cannot_retrieve_request_data));

        // Remember content info

        _contentType        = contentInfo[0];
        _contentTotalLength = contentInfo[1];
        _contentAvailLength = contentInfo[2];
        _queryStringLength  = contentInfo[3];

        // Remember basic strings

        _method             = basicStrings[0];
        _filePath           = basicStrings[1];
        _pathInfo           = basicStrings[2];
        _path = (_pathInfo.Length > 0) ? (_filePath + _pathInfo) : _filePath;
        _pathTranslated     = basicStrings[3];
        _appPath            = basicStrings[4];
        _appPathTranslated  = basicStrings[5];
    }

    //
    // Public methods
    //

    internal static ISAPIWorkerRequest CreateWorkerRequest(IntPtr ecb, bool useOOP) {

        ISAPIWorkerRequest wr = null;
        if (useOOP) {
            EtwTrace.TraceEnableCheck(EtwTraceConfigType.DOWNLEVEL, IntPtr.Zero);

            if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Infrastructure)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_APPDOMAIN_ENTER, ecb, Thread.GetDomain().FriendlyName, null, false);

            wr = new ISAPIWorkerRequestOutOfProc(ecb);
        }
        else {
            int version = UnsafeNativeMethods.EcbGetVersion(ecb) >> 16;
            
            if (version >= 7) {
                EtwTrace.TraceEnableCheck(EtwTraceConfigType.IIS7_ISAPI, ecb);
            }
            else {
                EtwTrace.TraceEnableCheck(EtwTraceConfigType.DOWNLEVEL, IntPtr.Zero);
            }

            if (EtwTrace.IsTraceEnabled(EtwTraceLevel.Verbose, EtwTraceFlags.Infrastructure)) EtwTrace.Trace(EtwTraceType.ETW_TYPE_APPDOMAIN_ENTER, ecb, Thread.GetDomain().FriendlyName, null, true);

            if (version >= 7) {
                wr = new ISAPIWorkerRequestInProcForIIS7(ecb);
            }
            else if (version == 6) {
                wr = new ISAPIWorkerRequestInProcForIIS6(ecb);
            }
            else {
                wr = new ISAPIWorkerRequestInProc(ecb);
            }
        }
        return wr;
    }

    public override String GetUriPath() {
        return _path;
    }

    public override String GetQueryString() {
        if (_queryStringLength == 0)
            return String.Empty;

        int size = _queryStringLength + 2;
        StringBuilder buf = new StringBuilder(size);

        int r = GetQueryStringCore(0, buf, size);

        if (r != 1)
            throw new HttpException(SR.GetString(SR.Cannot_get_query_string));

        return buf.ToString();
    }

    public override byte[] GetQueryStringRawBytes() {
        if (_queryStringLength == 0)
            return null;

        byte[] buf = new byte[_queryStringLength];
        int r = GetQueryStringRawBytesCore(buf, _queryStringLength);
        if (r != 1)
            throw new HttpException(SR.GetString(SR.Cannot_get_query_string_bytes));

        return buf;
    }


    public override String GetRawUrl() {
        String qs = GetQueryString();

        if (!String.IsNullOrEmpty(qs))
            return _path + "?" + qs;
        else
            return _path;
    }

    public override String GetHttpVerbName() {
        return _method;
    }

    public override String GetHttpVersion() {
        return GetServerVariable("SERVER_PROTOCOL");
    }

    public override String GetRemoteAddress() {
        return GetServerVariable("REMOTE_ADDR");
    }

    public override String GetRemoteName() {
        return GetServerVariable("REMOTE_HOST");
    }

    public override int GetRemotePort() {
        return 0;   // unknown in ISAPI
    }

    public override String GetLocalAddress() {
        return GetServerVariable("LOCAL_ADDR");
    }

    public override int GetLocalPort() {
        return Int32.Parse(GetServerVariable("SERVER_PORT"));
    }

    internal override String GetLocalPortAsString() {
        return GetServerVariable("SERVER_PORT");
    }

    public override String GetServerName() {
        return GetServerVariable("SERVER_NAME");
    }

    public override bool IsSecure() {
        String https = GetServerVariable("HTTPS");
        return (https != null && https.Equals("on"));
    }

    public override String GetFilePath() {
        return _filePath;
    }

    public override String GetFilePathTranslated() {
        return _pathTranslated;
    }

    public override String GetPathInfo() {
        return _pathInfo;
    }

    public override String GetAppPath() {
        return _appPath;
    }

    public override String GetAppPathTranslated() {
        return _appPathTranslated;
    }

    public override int GetPreloadedEntityBodyLength() {
        return _contentAvailLength;
    }

    public override byte[] GetPreloadedEntityBody() {
        if (!_preloadedContentRead) {
            if (_contentAvailLength > 0) {
                _preloadedContent = new byte[_contentAvailLength];

                int r = GetPreloadedPostedContentCore(_preloadedContent, 0, _contentAvailLength);

                if (r < 0)
                    throw new HttpException(SR.GetString(SR.Cannot_read_posted_data));
            }

            _preloadedContentRead = true;
        }

        return _preloadedContent;
    }

    public override bool IsEntireEntityBodyIsPreloaded() {
        return (_contentAvailLength == _contentTotalLength);
    }

    public override int GetTotalEntityBodyLength() {
        return _contentTotalLength;
    }

    public override int ReadEntityBody(byte[] buffer, int size)  {
        return ReadEntityBody(buffer, 0, size);
    }

    public override int ReadEntityBody(byte[] buffer, int offset, int size) {
        if (buffer.Length - offset < size) {
            throw new ArgumentOutOfRangeException("offset");
        }

        int r = GetAdditionalPostedContentCore(buffer, offset, size);

        if (r < 0) {
            throw new HttpException(SR.GetString(SR.Cannot_read_posted_data));
        }

        return r;
    }

    public override long GetBytesRead() {
        throw new HttpException(SR.GetString(SR.Not_supported));
    }

    public override String GetKnownRequestHeader(int index)  {
        if (!_requestHeadersAvailable) {
            // special case important ones so that no all headers parsing is required

            switch (index) {
                case HeaderContentType:
                    if (_contentType == CONTENT_FORM)
                        return "application/x-www-form-urlencoded";
                    break;

                case HeaderContentLength:
                    if (_contentType != CONTENT_NONE)
                        return (_contentTotalLength).ToString();
                    break;
            }

            // parse all headers
            ReadRequestHeaders();
        }

        return _knownRequestHeaders[index];
    }

    public override String GetUnknownRequestHeader(String name) {
        if (!_requestHeadersAvailable)
            ReadRequestHeaders();

        int n = _unknownRequestHeaders.Length;

        for (int i = 0; i < n; i++) {
            if (StringUtil.EqualsIgnoreCase(name, _unknownRequestHeaders[i][0]))
                return _unknownRequestHeaders[i][1];
        }

        return null;
    }

    public override String[][] GetUnknownRequestHeaders() {
        if (!_requestHeadersAvailable)
            ReadRequestHeaders();

        return _unknownRequestHeaders;
    }

    public override void SendStatus(int statusCode, String statusDescription) {
        _status.Append(statusCode.ToString());
        _status.Append(" ");
        _status.Append(statusDescription);
        _statusSet = true;
    }

    internal override void SetHeaderEncoding(Encoding encoding) {
        _headerEncoding = encoding;
    }

    public override void SendKnownResponseHeader(int index, String value) {
        if (_headersSent)
            throw new HttpException(SR.GetString(SR.Cannot_append_header_after_headers_sent));

        if (index == HttpWorkerRequest.HeaderSetCookie) {
            DisableKernelCache();
        }

        _headers.Append(GetKnownResponseHeaderName(index));
        _headers.Append(": ");
        _headers.Append(value);
        _headers.Append("\r\n");

        if (index == HeaderContentLength)
            _contentLengthSent = true;
        else if (index == HeaderTransferEncoding && (value != null && value.Equals("chunked")))
            _chunked = true;
    }

    public override void SendUnknownResponseHeader(String name, String value) {
        if (_headersSent)
            throw new HttpException(SR.GetString(SR.Cannot_append_header_after_headers_sent));

        if (StringUtil.EqualsIgnoreCase(name, "Set-Cookie")) {
            DisableKernelCache();
        }

        _headers.Append(name);
        _headers.Append(": ");
        _headers.Append(value);
        _headers.Append("\r\n");
    }

    public override void SendCalculatedContentLength(int contentLength) {
        SendCalculatedContentLength((long)contentLength);
    }

    // VSWhidbey 559473: need to support Content-Length response header values > 2GB
    public override void SendCalculatedContentLength(long contentLength) {
        if (!_headersSent)
        {
            _headers.Append("Content-Length: ");
            _headers.Append(contentLength.ToString(CultureInfo.InvariantCulture));
            _headers.Append("\r\n");
            _contentLengthSent = true;
        }
    }

    public override bool HeadersSent() {
        return _headersSent;
    }

    public override bool IsClientConnected() {
        return (IsClientConnectedCore() == 0) ? false : true;
    }

    public override void CloseConnection() {
        CloseConnectionCore();
    }

    public override void SendResponseFromMemory(byte[] data, int length) {
        if (!_headersSent)
            SendHeaders();

        if (length > 0)
            AddBodyToCachedResponse(new MemoryBytes(data, length));
    }

    public override void SendResponseFromMemory(IntPtr data, int length) {
        SendResponseFromMemory(data, length, false);
    }

    internal override void SendResponseFromMemory(IntPtr data, int length, bool isBufferFromUnmanagedPool) {
        if (!_headersSent)
            SendHeaders();

        if (length > 0)
            AddBodyToCachedResponse(new MemoryBytes(data, length, isBufferFromUnmanagedPool ? BufferType.UnmanagedPool : BufferType.Managed));
    }

    // PackageFile for in-proc case
    internal virtual MemoryBytes PackageFile(String filename, long offset64, long length64, bool isImpersonating) {
        // The offset and length must be less than Int32.MaxValue for in-proc. 
        // This should be true, since HttpFileResponseElement.ctor throws ArgumentOutOfRangeException for in-proc
        Debug.Assert(offset64 < Int32.MaxValue);
        Debug.Assert(length64 < Int32.MaxValue);
        int offset = Convert.ToInt32(offset64);
        int length = Convert.ToInt32(length64);

        FileStream f = null;
        MemoryBytes bytes = null;
        try {
            Debug.Assert(offset < length);
            f = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            Debug.Assert((f.Length - offset) == length);
            int size = (int) (f.Length - offset);
            byte[] fileBytes = new byte[size];
            int bytesRead = f.Read(fileBytes, offset, size);
            bytes = new MemoryBytes(fileBytes, bytesRead);
        }
        finally {
            if (f != null)
                f.Close();
        }

        return bytes;
    }

    internal override void TransmitFile(string filename, long offset, long length, bool isImpersonating) {
        if (!_headersSent)
            SendHeaders();

        if (length == 0)
            return;

        AddBodyToCachedResponse(PackageFile(filename, offset, length, isImpersonating));
        return;
    }

    public override void SendResponseFromFile(String filename, long offset, long length) {
        if (!_headersSent)
            SendHeaders();

        if (length == 0)
            return;

        FileStream f = null;

        try {
            f = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            SendResponseFromFileStream(f, offset, length);
        }
        finally {
            if (f != null)
                f.Close();
        }
    }

    public override void SendResponseFromFile(IntPtr handle, long offset, long length) {
        if (!_headersSent)
            SendHeaders();

        if (length == 0)
            return;

        FileStream f = null;

        try {
            f = new FileStream(new Microsoft.Win32.SafeHandles.SafeFileHandle(handle,false), FileAccess.Read);
            SendResponseFromFileStream(f, offset, length);
        }
        finally {
            if (f != null)
                f.Close();
        }
    }

    // ISAPIWorkerRequest
    public override void FlushResponse(bool finalFlush) {
        // only flush headers - the data is write through

        if (!_headersSent)
            SendHeaders();

        FlushCachedResponse(finalFlush);
    }

    public override void EndOfRequest() {
        FlushCachedResponse(true);

        // recycle the headers and status buffers
        if (_headers != null) {
            _headers.Dispose();
            _headers = null;
        }

        if (_status != null) {
            _status.Dispose();
            _status = null;
        }
        CallEndOfRequestCallbackOnceAfterAllIoComplete();
    }

    public override void SetEndOfSendNotification(HttpWorkerRequest.EndOfSendNotification callback, Object extraData) {
        if (_endOfRequestCallback != null)
            throw new InvalidOperationException();
        _endOfRequestCallback = callback;
        _endOfRequestCallbackArg = extraData;
        _endOfRequestCallbackLockCount = 1;   // when goes to 0 the callback is called
    }

    public override String MapPath(String path) {
        return HostingEnvironment.MapPathInternal(path);
    }

    public override String MachineConfigPath {
        get {
            return HttpConfigurationSystem.MachineConfigurationFilePath;
        }
    }

    public override String RootWebConfigPath {
        get {
            return HttpConfigurationSystem.RootWebConfigurationFilePath;
        }
    }

    public override String MachineInstallDirectory {
        get {
            return HttpRuntime.AspInstallDirectory;
        }
    }

    public override IntPtr GetUserToken() {
        return GetUserTokenCore();
    }

    public override IntPtr GetVirtualPathToken() {
        return GetVirtualPathTokenCore();
    }

    public override byte[] GetClientCertificate() {
        if (!_clientCertFetched)
            FetchClientCertificate();

        return _clientCert;
    }

    public override DateTime GetClientCertificateValidFrom() {
        if (!_clientCertFetched)
            FetchClientCertificate();

        return _clientCertValidFrom;
    }

    public override DateTime GetClientCertificateValidUntil() {
        if (!_clientCertFetched)
            FetchClientCertificate();

        return _clientCertValidUntil;
    }

    public override byte [] GetClientCertificateBinaryIssuer() {
        if (!_clientCertFetched)
            FetchClientCertificate();
        return _clientCertBinaryIssuer;
    }

    public override int GetClientCertificateEncoding() {
        if (!_clientCertFetched)
            FetchClientCertificate();
        return _clientCertEncoding;
    }

    public override byte [] GetClientCertificatePublicKey() {
        if (!_clientCertFetched)
            FetchClientCertificate();
        return _clientCertPublicKey;
    }

    private void FetchClientCertificate() {
        if (_clientCertFetched)
            return;

        _clientCertFetched = true;

        byte[]         buf        = new byte[8192];
        int[]          pInts      = new int[4];
        long[]         pDates     = new long[2];
        int            iRet       = GetClientCertificateCore(buf, pInts, pDates);

        if (iRet < 0 && (-iRet) > 8192) {
            iRet = -iRet + 100;
            buf  = new byte[iRet];
            iRet = GetClientCertificateCore(buf, pInts, pDates);
        }
        if (iRet > 0) {
            _clientCertEncoding = pInts[0];

            if (pInts[1] < buf.Length && pInts[1] > 0) {
                _clientCert = new byte[pInts[1]];
                Array.Copy(buf, _clientCert, pInts[1]);

                if (pInts[2] + pInts[1] < buf.Length && pInts[2] > 0) {
                    _clientCertBinaryIssuer = new byte[pInts[2]];
                    Array.Copy(buf, pInts[1], _clientCertBinaryIssuer, 0, pInts[2]);
                }

                if (pInts[2] + pInts[1] + pInts[3] < buf.Length && pInts[3] > 0) {
                    _clientCertPublicKey = new byte[pInts[3]];
                    Array.Copy(buf, pInts[1] + pInts[2], _clientCertPublicKey, 0, pInts[3]);
                }
            }
        }

        if (iRet > 0 && pDates[0] != 0)
            _clientCertValidFrom = DateTime.FromFileTime(pDates[0]);
        else
            _clientCertValidFrom = DateTime.Now;

        if (iRet > 0 && pDates[1] != 0)
            _clientCertValidUntil = DateTime.FromFileTime(pDates[1]);
        else
            _clientCertValidUntil = DateTime.Now;
    }

    //
    // internal methods specific to ISAPI
    //

    internal void AppendLogParameter(String logParam) {
        AppendLogParameterCore(logParam);
    }

    internal virtual void SendEmptyResponse() {
    }

    //
    // PInvoke callback wrappers -- overridden by the derived classes
    //

    internal abstract int GetBasicsCore(byte[] buffer, int size, int[] contentInfo);
    internal abstract int GetQueryStringCore(int encode, StringBuilder buffer, int size);
    internal abstract int GetQueryStringRawBytesCore(byte[] buffer, int size);
    internal abstract int GetPreloadedPostedContentCore(byte[] bytes, int offset, int numBytesToRead);
    internal abstract int GetAdditionalPostedContentCore(byte[] bytes, int offset, int bufferSize);
    // ISAPIWorkerRequest
    internal abstract void FlushCore(byte[]     status,
                                     byte[]     header,
                                     int        keepConnected,
                                     int        totalBodySize,
                                     int        numBodyFragments,
                                     IntPtr[]   bodyFragments,
                                     int[]      bodyFragmentLengths,
                                     int        doneWithSession,
                                     int        finalStatus,
                                     out bool   async);
    internal abstract int IsClientConnectedCore();
    internal abstract int CloseConnectionCore();
    internal abstract int MapUrlToPathCore(String url, byte[] buffer, int size);
    internal abstract IntPtr GetUserTokenCore();
    internal abstract IntPtr GetVirtualPathTokenCore();
    internal abstract int AppendLogParameterCore(String logParam);
    internal abstract int GetClientCertificateCore(byte[] buffer, int [] pInts, long [] pDates);
    internal abstract int CallISAPI(UnsafeNativeMethods.CallISAPIFunc iFunction, byte [] bufIn, byte [] bufOut);
    internal virtual void Close() {}
}

//
// In-process ISAPIWorkerRequest
//
// Does queueing of IO operations. ISAPI only support one async IO at a time.
//

internal class ISAPIWorkerRequestInProc : ISAPIWorkerRequest {

    protected const int NUM_SERVER_VARIABLES = 35; // total number of variables that we commonly get
    protected const int NUM_BASIC_SERVER_VARIABLES = 12; // needed on every request
    protected const int NUM_ADDITIONAL_SERVER_VARIABLES = 23; // needed when HttpRequest.ServerVariables is populated

    // These constants must be kept in [....] with g_szServerVariables and g_szUnicodeServerVariables in ecbdirect.cxx

    protected const int LOGON_USER = 0;
    protected const int AUTH_TYPE = 1;
    protected const int APPL_PHYSICAL_PATH = 2;
    protected const int REQUEST_METHOD = 3;
    protected const int PATH_INFO = 4;
    protected const int PATH_TRANSLATED = 5;
    protected const int URL = 6;
    protected const int CACHE_URL = 7;
    protected const int SERVER_NAME = 8;
    protected const int SERVER_PORT = 9;
    protected const int HTTPS = 10;
    protected const int ALL_RAW = 11;
    protected const int REMOTE_ADDR = 12;
    protected const int AUTH_PASSWORD = 13;
    protected const int CERT_COOKIE = 14;
    protected const int CERT_FLAGS = 15;
    protected const int CERT_ISSUER = 16;
    protected const int CERT_KEYSIZE = 17;
    protected const int CERT_SECRETKEYSIZE = 18;
    protected const int CERT_SERIALNUMBER = 19;
    protected const int CERT_SERVER_ISSUER = 20;
    protected const int CERT_SERVER_SUBJECT = 21;
    protected const int CERT_SUBJECT = 22;
    protected const int GATEWAY_INTERFACE = 23;
    protected const int HTTPS_KEYSIZE = 24;
    protected const int HTTPS_SECRETKEYSIZE = 25;
    protected const int HTTPS_SERVER_ISSUER = 26;
    protected const int HTTPS_SERVER_SUBJECT = 27;
    protected const int INSTANCE_ID = 28;
    protected const int INSTANCE_META_PATH = 29;
    protected const int LOCAL_ADDR = 30;
    protected const int REMOTE_HOST = 31;
    protected const int REMOTE_PORT = 32;
    protected const int SERVER_PROTOCOL = 33;
    protected const int SERVER_SOFTWARE = 34;

    // storage for common server variables
    protected string[] _basicServerVars = null;
    protected string[] _additionalServerVars = null;
    private ChannelBinding   _channelBindingToken;

    internal ISAPIWorkerRequestInProc(IntPtr ecb) : base(ecb) {
        if (ecb == IntPtr.Zero || UnsafeNativeMethods.EcbGetTraceContextId(ecb, out _traceId) != 1) {
            _traceId = Guid.Empty;
        }
    }

    internal override int GetBasicsCore(byte[] buffer, int size, int[] contentInfo) {
        if (_ecb == IntPtr.Zero)
            return 0;

        return UnsafeNativeMethods.EcbGetBasics(_ecb, buffer, size, contentInfo);
    }

    internal override int GetQueryStringCore(int encode, StringBuilder buffer, int size) {
        if (_ecb == IntPtr.Zero)
            return 0;

        return UnsafeNativeMethods.EcbGetQueryString(_ecb, encode, buffer, size);
    }

    internal override int GetQueryStringRawBytesCore(byte[] buffer, int size) {
        if (_ecb == IntPtr.Zero)
            return 0;

        return UnsafeNativeMethods.EcbGetQueryStringRawBytes(_ecb, buffer, size);
    }

    internal override int GetPreloadedPostedContentCore(byte[] bytes, int offset, int numBytesToRead) {
        if (_ecb == IntPtr.Zero)
            return 0;

        int rc = UnsafeNativeMethods.EcbGetPreloadedPostedContent(_ecb, bytes, offset, numBytesToRead);
        if (rc > 0)
            PerfCounters.IncrementCounterEx(AppPerfCounter.REQUEST_BYTES_IN, rc);
        return rc;
    }

    internal override int GetAdditionalPostedContentCore(byte[] bytes, int offset, int bufferSize) {
        if (_ecb == IntPtr.Zero)
            return 0;

        int rc = 0;

        try {
            // Acquire blocking call
            IsInReadEntitySync = true;
  
            rc = UnsafeNativeMethods.EcbGetAdditionalPostedContent(_ecb, bytes, offset, bufferSize);
        }
        finally {
            // Release blocking call
            IsInReadEntitySync = false;
        }

        if (rc > 0)
            PerfCounters.IncrementCounterEx(AppPerfCounter.REQUEST_BYTES_IN, rc);
        return rc;
    }

    internal override int GetClientCertificateCore(byte[] buffer, int [] pInts, long [] pDates) {
        if (_ecb == IntPtr.Zero)
            return 0;

        return UnsafeNativeMethods.EcbGetClientCertificate(_ecb, buffer, buffer.Length, pInts, pDates);
    }

    internal override int IsClientConnectedCore()
    {
        if (_ecb == IntPtr.Zero)
            return 0;

        return UnsafeNativeMethods.EcbIsClientConnected(_ecb);
    }

    // ISAPIWorkerRequestInProc
    internal override void FlushCore(byte[]     status,
                                     byte[]     header,
                                     int        keepConnected,
                                     int        totalBodySize,
                                     int        numBodyFragments,
                                     IntPtr[]   bodyFragments,
                                     int[]      bodyFragmentLengths,
                                     int        doneWithSession,
                                     int        finalStatus,
                                     out bool   async) {
        async = false;

        if (_ecb == IntPtr.Zero)
            return;

        UnsafeNativeMethods.EcbFlushCore(
                        _ecb,
                        status,
                        header,
                        keepConnected,
                        totalBodySize,
                        numBodyFragments,
                        bodyFragments,
                        bodyFragmentLengths,
                        doneWithSession,
                        finalStatus,
                        0,
                        0,
                        null);
    }

    internal override int CloseConnectionCore() {
        if (_ecb == IntPtr.Zero)
            return 0;

        return UnsafeNativeMethods.EcbCloseConnection(_ecb);
    }

    internal override int MapUrlToPathCore(String url, byte[] buffer, int size) {
        if (_ecb == IntPtr.Zero)
            return 0;

        return UnsafeNativeMethods.EcbMapUrlToPath(_ecb, url, buffer, size);
    }

    internal override IntPtr GetUserTokenCore() {
        if (_token == IntPtr.Zero && _ecb != IntPtr.Zero)
            _token = UnsafeNativeMethods.EcbGetImpersonationToken(_ecb, IntPtr.Zero);
        return _token;
    }

    internal override IntPtr GetVirtualPathTokenCore() {
        if (_token == IntPtr.Zero && _ecb != IntPtr.Zero)
            _token = UnsafeNativeMethods.EcbGetVirtualPathToken(_ecb, IntPtr.Zero);

        return _token;
    }

    internal override int AppendLogParameterCore(String logParam) {
        if (_ecb == IntPtr.Zero)
            return 0;

        return UnsafeNativeMethods.EcbAppendLogParameter(_ecb, logParam);
    }

    // ISAPIWorkerRequestInProc
    protected virtual String GetServerVariableCore(String name) {
        if (_ecb == IntPtr.Zero)
            return null;

        String value = null;

        RecyclableByteBuffer buf = new RecyclableByteBuffer();

        int retVal = UnsafeNativeMethods.EcbGetServerVariable(_ecb, name, buf.Buffer, buf.Buffer.Length);

        while (retVal < 0) {
            buf.Resize(-retVal);     // buffer not big enough
            retVal = UnsafeNativeMethods.EcbGetServerVariable(_ecb, name, buf.Buffer, buf.Buffer.Length);
        }

        if (retVal > 0)
            value = buf.GetDecodedString(Encoding.UTF8, retVal);

        buf.Dispose();

        return value;
    }

    // ISAPIWorkerRequestInProc
    protected virtual void GetAdditionalServerVariables() {
        if (_ecb == IntPtr.Zero)
            return;

        // _additionalServerVars should only be initialized once
        Debug.Assert(_additionalServerVars == null);
        if (_additionalServerVars != null)
            return;

        _additionalServerVars = new string[NUM_ADDITIONAL_SERVER_VARIABLES];

        for(int i = 0; i < _additionalServerVars.Length; i++) {
            int nameIndex = i + NUM_BASIC_SERVER_VARIABLES;

            RecyclableByteBuffer buf = new RecyclableByteBuffer();

            int retVal = UnsafeNativeMethods.EcbGetServerVariableByIndex(_ecb, nameIndex, buf.Buffer, buf.Buffer.Length);

            while (retVal < 0) {
                buf.Resize(-retVal);     // buffer not big enough
                retVal = UnsafeNativeMethods.EcbGetServerVariableByIndex(_ecb, nameIndex, buf.Buffer, buf.Buffer.Length);
            }

            if (retVal > 0)
                _additionalServerVars[i] = buf.GetDecodedString(Encoding.UTF8, retVal);

            buf.Dispose();
        }
    }

    [System.Runtime.TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
    private String GetAdditionalServerVar(int index) {
        if (_additionalServerVars == null)
            GetAdditionalServerVariables();

        return _additionalServerVars[index - NUM_BASIC_SERVER_VARIABLES];
    }

    public override String GetServerVariable(String name) {
        // this switch statement is a little more than twice as fast as a Hashtable lookup
        if (name != null) {
            switch (name.Length) {
                case 20:
                    if (name == "HTTPS_SERVER_SUBJECT")
                        return GetAdditionalServerVar(HTTPS_SERVER_SUBJECT);
                    break;

                case 19:
                    if (name == "HTTPS_SECRETKEYSIZE")
                        return GetAdditionalServerVar(HTTPS_SECRETKEYSIZE);
                    else if (name == "CERT_SERVER_SUBJECT")
                        return GetAdditionalServerVar(CERT_SERVER_SUBJECT);
                    else if (name == "HTTPS_SERVER_ISSUER")
                        return GetAdditionalServerVar(HTTPS_SERVER_ISSUER);
                    break;

                case 18:
                    if (name == "INSTANCE_META_PATH")
                        return GetAdditionalServerVar(INSTANCE_META_PATH);
                    else if (name == "CERT_SECRETKEYSIZE")
                        return GetAdditionalServerVar(CERT_SECRETKEYSIZE);
                    else if (name == "CERT_SERVER_ISSUER")
                        return GetAdditionalServerVar(CERT_SERVER_ISSUER);
                    break;

                case 17:
                    if (name == "CERT_SERIALNUMBER")
                        return GetAdditionalServerVar(CERT_SERIALNUMBER);
                    else if (name == "GATEWAY_INTERFACE")
                        return GetAdditionalServerVar(GATEWAY_INTERFACE);
                    break;

                case 15:
                    if (name == "HTTP_USER_AGENT")
                        return GetKnownRequestHeader(HeaderUserAgent);
                    else if (name == "SERVER_PROTOCOL")
                        return GetAdditionalServerVar(SERVER_PROTOCOL);
                    else if (name == "SERVER_SOFTWARE")
                        return GetAdditionalServerVar(SERVER_SOFTWARE);
                    break;

                case 13:
                    if (name == "AUTH_PASSWORD")
                        return GetAdditionalServerVar(AUTH_PASSWORD);
                    else if (name == "HTTPS_KEYSIZE")
                        return GetAdditionalServerVar(HTTPS_KEYSIZE);
                    break;

                case 12:
                    if (name == "CERT_KEYSIZE")
                        return GetAdditionalServerVar(CERT_KEYSIZE);
                    else if (name == "CERT_SUBJECT")
                        return GetAdditionalServerVar(CERT_SUBJECT);
                    break;

                case 11:
                    if (name == "SERVER_NAME")
                        return _basicServerVars[SERVER_NAME];
                    else if (name == "SERVER_PORT")
                        return _basicServerVars[SERVER_PORT];
                    else if (name == "REMOTE_HOST")
                        return GetAdditionalServerVar(REMOTE_HOST);
                    else if (name == "REMOTE_PORT")
                        return GetAdditionalServerVar(REMOTE_PORT);
                    else if (name == "REMOTE_ADDR")
                        return GetAdditionalServerVar(REMOTE_ADDR);
                    else if (name == "CERT_COOKIE")
                        return GetAdditionalServerVar(CERT_COOKIE);
                    else if (name == "CERT_ISSUER")
                        return GetAdditionalServerVar(CERT_ISSUER);
                    else if (name == "INSTANCE_ID")
                        return GetAdditionalServerVar(INSTANCE_ID);
                    break;

                case 10:
                    if (name == "LOGON_USER")
                        return _basicServerVars[LOGON_USER];
                    else if (name == "LOCAL_ADDR")
                        return GetAdditionalServerVar(LOCAL_ADDR);
                    else if (name == "CERT_FLAGS")
                        return GetAdditionalServerVar(CERT_FLAGS);
                    break;

                case 9:
                    if (name == "AUTH_TYPE")
                        return _basicServerVars[AUTH_TYPE];
                    break;

                case 7:
                    if (name == "ALL_RAW") {
                        return _basicServerVars[ALL_RAW];
                    }
                    break;

                case 5:
                    if (name == "HTTPS")
                        return _basicServerVars[HTTPS];
                    break;
            }
        }

        // this is not a common server variable
        return GetServerVariableCore(name);
    }

    internal override int CallISAPI(UnsafeNativeMethods.CallISAPIFunc iFunction, byte [] bufIn, byte [] bufOut) {
        if (_ecb == IntPtr.Zero)
            return 0;

        return UnsafeNativeMethods.EcbCallISAPI(_ecb, iFunction, bufIn, bufIn.Length, bufOut, bufOut.Length);
    }
    internal override void Close() {
        if (_channelBindingToken != null && !_channelBindingToken.IsInvalid)
            _channelBindingToken.Dispose();
    }
    internal ChannelBinding HttpChannelBindingToken {
        get {
            if (_channelBindingToken == null) {
                IntPtr token       = IntPtr.Zero;
                int    tokenSize   = 0;
                int    hr          = HResults.S_OK;

                hr = UnsafeNativeMethods.EcbGetChannelBindingToken(_ecb, out token, out tokenSize);
                if (hr == HResults.E_NOTIMPL)
                    throw new PlatformNotSupportedException();
                Misc.ThrowIfFailedHr(hr);
                _channelBindingToken = new HttpChannelBindingToken(token, tokenSize);
            }
            return _channelBindingToken;
        }
    }
}

//
// In-process ISAPIWorkerRequest specific for IIS7
//
// Uses unicode server vars
//

internal class ISAPIWorkerRequestInProcForIIS7 : ISAPIWorkerRequestInProcForIIS6 {

    internal ISAPIWorkerRequestInProcForIIS7(IntPtr ecb) : base(ecb) {
        _trySkipIisCustomErrors = true;
    }

    internal override bool TrySkipIisCustomErrors {
        get { return _trySkipIisCustomErrors;  }
        set { _trySkipIisCustomErrors = value; }
    }

    internal override void RaiseTraceEvent(IntegratedTraceType traceType, string eventData) {
        if (IntPtr.Zero != _ecb) {
            // the area is derivative of the type, either page or module
            int areaFlag = (traceType < IntegratedTraceType.DiagCritical) ? EtwTraceFlags.Page : EtwTraceFlags.Module;
            if (EtwTrace.IsTraceEnabled(EtwTrace.InferVerbosity(traceType), areaFlag)) {
                string message = String.IsNullOrEmpty(eventData) ? String.Empty : eventData;
                UnsafeNativeMethods.EcbEmitSimpleTrace(_ecb, (int)traceType, message);
            }
        }
    }

    internal override void RaiseTraceEvent(WebBaseEvent webEvent) {
        if (IntPtr.Zero != _ecb) {
            if (EtwTrace.IsTraceEnabled(webEvent.InferEtwTraceVerbosity(), EtwTraceFlags.Infrastructure)) {
                int fieldCount;
                string[] fieldNames;
                int[] fieldTypes;
                string[] fieldData;
                int webEventType;
                webEvent.DeconstructWebEvent(out webEventType, out fieldCount, out fieldNames, out fieldTypes, out fieldData);
            UnsafeNativeMethods.EcbEmitWebEventTrace(_ecb, webEventType, fieldCount, fieldNames, fieldTypes, fieldData);
            }
        }
    }
}

//
// In-process ISAPIWorkerRequest specific for IIS6
//
// Uses unicode server vars
//

internal class ISAPIWorkerRequestInProcForIIS6 : ISAPIWorkerRequestInProc {

    private static int _asyncIoCount;
    private bool _disconnected;

    internal ISAPIWorkerRequestInProcForIIS6(IntPtr ecb) : base(ecb) {
    }

    internal static void WaitForPendingAsyncIo() {
        while(_asyncIoCount != 0) {
            Thread.Sleep(250);
        }
    }

    internal override void SendEmptyResponse() {
        // facilitate health monitoring for IIS6 -- update last activity timestamp
        // to avoid deadlock detection
        UnsafeNativeMethods.UpdateLastActivityTimeForHealthMonitor();
    }

    public override String GetRawUrl() {
        // CACHE_URL is the original URI, unaffected by any rewriting or routing
        // that may have occurred on the server
        string rawUrl = GetRawUrlHelper(GetUnicodeServerVariable(CACHE_URL));
        Debug.Trace("ClientUrl", "*** GetRawUrl --> " + rawUrl + " ***");
        return rawUrl;
    }

    internal override void ReadRequestBasics() {
        if (_ecb == IntPtr.Zero)
            return;
        // set server variables needed for request basics and Indigo (VSWhidbey 352117,344580)
        GetBasicServerVariables();

        // _pathInfo is the difference between UNICODE_PATH_INFO and UNICODE_URL
        int lengthDiff = _path.Length - _filePath.Length;
        if (lengthDiff > 0) {
            _pathInfo = _path.Substring(_filePath.Length);
            int pathTranslatedLength = _pathTranslated.Length - lengthDiff;
            if (pathTranslatedLength > 0) {
                _pathTranslated = _pathTranslated.Substring(0, pathTranslatedLength);
            }
        }
        else {
            _filePath = _path;
            _pathInfo = String.Empty;
        }

        _appPath = HostingEnvironment.ApplicationVirtualPath;

        //
        // other (int) request basics
        //

        int[] contentInfo = null;

        try {
            contentInfo = RecyclableArrayHelper.GetIntegerArray(4);
            UnsafeNativeMethods.EcbGetBasicsContentInfo(_ecb, contentInfo);

            _contentType        = contentInfo[0];
            _contentTotalLength = contentInfo[1];
            _contentAvailLength = contentInfo[2];
            _queryStringLength  = contentInfo[3];
        }
        finally {
            RecyclableArrayHelper.ReuseIntegerArray(contentInfo);
        }
    }

    private void GetBasicServerVariables() {

        if (_ecb == IntPtr.Zero)
            return;
        // _basicServerVars should only be initialized once
        Debug.Assert(_basicServerVars == null);
        if (_basicServerVars != null)
            return;

        _basicServerVars = new string[NUM_BASIC_SERVER_VARIABLES];

        ServerVarCharBuffer buffer = new ServerVarCharBuffer();

        try {
            int[] serverVarLengths = new int[NUM_BASIC_SERVER_VARIABLES];
            int r = 0;
            int hresult = UnsafeNativeMethods.EcbGetUnicodeServerVariables(_ecb, buffer.PinnedAddress, buffer.Length,
                                                                     serverVarLengths, serverVarLengths.Length, 0, ref r);
            if (r > buffer.Length)
            {
                buffer.Resize(r);
                hresult = UnsafeNativeMethods.EcbGetUnicodeServerVariables(_ecb, buffer.PinnedAddress, buffer.Length,
                                                                     serverVarLengths, serverVarLengths.Length, 0, ref r);
            }

            Misc.ThrowIfFailedHr(hresult);

            IntPtr current = buffer.PinnedAddress;

            for(int i = 0; i < _basicServerVars.Length; i++) {
                _basicServerVars[i] = Marshal.PtrToStringUni(current, serverVarLengths[i]);
                current = new IntPtr((long)current + 2L * (1L + serverVarLengths[i]));
            }
            // special case variables
            _appPathTranslated = _basicServerVars[APPL_PHYSICAL_PATH];
            _method = _basicServerVars[REQUEST_METHOD];
            _path = _basicServerVars[PATH_INFO];
            _pathTranslated = _basicServerVars[PATH_TRANSLATED];
            _filePath = _basicServerVars[URL];
        }
        finally {
            buffer.Dispose();
        }
    }

    // ISAPIWorkerRequestInProcForIIS6
    protected override void GetAdditionalServerVariables() {
        if (_ecb == IntPtr.Zero)
            return;

        // _additionalServerVars should only be initialized once
        Debug.Assert(_additionalServerVars == null);
        if (_additionalServerVars != null)
            return;

        _additionalServerVars = new string[NUM_ADDITIONAL_SERVER_VARIABLES];

        ServerVarCharBuffer buffer = new ServerVarCharBuffer();

        try {
            int[] serverVarLengths = new int[NUM_ADDITIONAL_SERVER_VARIABLES];
            int r = 0;
            int hresult = UnsafeNativeMethods.EcbGetUnicodeServerVariables(_ecb, buffer.PinnedAddress, buffer.Length,
                                                                      serverVarLengths, serverVarLengths.Length, NUM_BASIC_SERVER_VARIABLES, ref r);
            if (r > buffer.Length) {
                buffer.Resize(r);
                hresult = UnsafeNativeMethods.EcbGetUnicodeServerVariables(_ecb, buffer.PinnedAddress, buffer.Length,
                                                                     serverVarLengths, serverVarLengths.Length, NUM_BASIC_SERVER_VARIABLES, ref r);
            }
            if (hresult != 0)
                Marshal.ThrowExceptionForHR(hresult);
            IntPtr current = buffer.PinnedAddress;

            for(int i = 0; i < _additionalServerVars.Length; i++) {
                _additionalServerVars[i] = Marshal.PtrToStringUni(current, serverVarLengths[i]);
                current = new IntPtr((long)current + 2L * (1L + serverVarLengths[i]));
            }
        }
        finally {
            buffer.Dispose();
        }
    }

    // ISAPIWorkerRequestInProcForIIS6
    protected override string GetServerVariableCore(string name) {
        if (StringUtil.StringStartsWith(name, "HTTP_"))
            // fall back for headers (IIS6 doesn't support them as UNICODE_XXX)
            return base.GetServerVariableCore(name);
        else
            return GetUnicodeServerVariable("UNICODE_" + name);
    }

    private String GetUnicodeServerVariable(String name) {
        String value = null;
        ServerVarCharBuffer buf = new ServerVarCharBuffer();

        try {
            value = GetUnicodeServerVariable(name, buf);
        }
        finally {
            buf.Dispose();
        }

        return value;
    }

    private String GetUnicodeServerVariable(int nameIndex) {
        String value = null;
        ServerVarCharBuffer buf = new ServerVarCharBuffer();

        try {
            value = GetUnicodeServerVariable(nameIndex, buf);
        }
        finally {
            buf.Dispose();
        }

        return value;
    }

    private String GetUnicodeServerVariable(String name, ServerVarCharBuffer buffer) {
        if (_ecb == IntPtr.Zero)
            return null;
        int r = UnsafeNativeMethods.EcbGetUnicodeServerVariable(_ecb, name, buffer.PinnedAddress, buffer.Length);

        if (r < 0) {
            buffer.Resize(-r);
            r = UnsafeNativeMethods.EcbGetUnicodeServerVariable(_ecb, name, buffer.PinnedAddress, buffer.Length);
        }

        if (r > 0)
            return Marshal.PtrToStringUni(buffer.PinnedAddress, r);
        else
            return null;
    }

    private String GetUnicodeServerVariable(int nameIndex, ServerVarCharBuffer buffer) {
        if (_ecb == IntPtr.Zero)
            return null;
        int r = UnsafeNativeMethods.EcbGetUnicodeServerVariableByIndex(_ecb, nameIndex, buffer.PinnedAddress, buffer.Length);

        if (r < 0) {
            buffer.Resize(-r);
            r = UnsafeNativeMethods.EcbGetUnicodeServerVariableByIndex(_ecb, nameIndex, buffer.PinnedAddress, buffer.Length);
        }

        if (r > 0)
            return Marshal.PtrToStringUni(buffer.PinnedAddress, r);
        else
            return null;
    }

    //
    // Support for async VectorSend and kernel mode cache on IIS6
    //

    private const int MIN_ASYNC_SIZE = 2048;
    private GCHandle _rootedThis;      // for the duration of async
    private ISAPIAsyncCompletionCallback _asyncFlushCompletionCallback;
    private int _asyncFinalStatus;
    private bool _serverSupportFunctionError = false;
    private IntPtr _entity;  // pointer to HSE_entity

    private bool _cacheInKernelMode = false;
    private bool _disableKernelCache = false;
    protected bool _trySkipIisCustomErrors = false;
    private const int TRY_SKIP_IIS_CUSTOM_ERRORS = 0x40;

    // PackageFile for IIS6
    internal override MemoryBytes PackageFile(string filename, long offset, long size, bool isImpersonating) {
        return new MemoryBytes(filename, offset, size);
    }

    // VSWhidbey 555203: support 64-bit file sizes for TransmitFile on IIS6
    internal override bool SupportsLongTransmitFile {
        get { return true; }
    }

    // ISAPIWorkerRequestInProcForIIS6
    internal override void FlushCore(byte[]     status,
                                     byte[]     header,
                                     int        keepConnected,
                                     int        totalBodySize,
                                     int        numBodyFragments,
                                     IntPtr[]   bodyFragments,
                                     int[]      bodyFragmentLengths,
                                     int        doneWithSession,
                                     int        finalStatus,
                                     out bool   async) {
        async = false;

        if (_ecb == IntPtr.Zero)
            return;

        if (_headersSentFromExecuteUrl) {
            // don't send headers twice
            status = null;
            header = null;
        }

        bool inAsyncFlush = false;

        // async only for large responses and only on the last flush or if inAsyncFlush is true
        // don't do async if shutting down (async IO holds up app domain shutdown)
        if (doneWithSession != 0 && !HttpRuntime.ShutdownInProgress && (_ignoreMinAsyncSize || (totalBodySize >= MIN_ASYNC_SIZE))) {
            if (_requiresAsyncFlushCallback) {
                _asyncFlushCompletionCallback = new ISAPIAsyncCompletionCallback(OnAsyncFlushCompletion);
                _asyncFinalStatus = finalStatus;    // remember to pass to DoneWithSession on completion
                _rootedThis = GCHandle.Alloc(this); // root for the duration of IO
                doneWithSession = 0;                // will do on completion
                async = true;
                Interlocked.Increment(ref _asyncIoCount);  // increment async io count
            }
            else {
                // buffers are native, so we don't need to return to managed code
                _asyncFlushCompletionCallback = null;
                doneWithSession = 0;                // will do on completion
                async = true;
            }
        }
        else {
            inAsyncFlush = (_asyncResultBase is FlushAsyncResult);
            if (inAsyncFlush) {
                _requiresAsyncFlushCallback = true;
                _asyncFlushCompletionCallback = new ISAPIAsyncCompletionCallback(OnAsyncFlushCompletion);
                _asyncFinalStatus = finalStatus;    // remember to pass to DoneWithSession on completion
                _rootedThis = GCHandle.Alloc(this); // root for the duration of IO
                async = true;
                Interlocked.Increment(ref _asyncIoCount);  // increment async io count
            }
        }

        // finalStatus is either 0 to force for a flush, 1 to indicate HSE_STATUS_SUCCESS, or 2 to indicate HSE_STATUS_SUCCESS_AND_KEEP_CONN
        Debug.Assert(0 <= finalStatus && finalStatus <= 2);
        int flags = _trySkipIisCustomErrors ? finalStatus|TRY_SKIP_IIS_CUSTOM_ERRORS : finalStatus;

        int rc = UnsafeNativeMethods.EcbFlushCore(
                        _ecb,
                        status,
                        header,
                        keepConnected,
                        totalBodySize,
                        numBodyFragments,
                        bodyFragments,
                        bodyFragmentLengths,
                        doneWithSession,
                        flags,
                        _cacheInKernelMode ? 1 : 0,
                        async ? 1 : 0,
                        _asyncFlushCompletionCallback);

        if (!_requiresAsyncFlushCallback && rc == 0 && async) {

            // unlock and reset cached response
            UnlockCachedResponseBytesOnceAfterIoComplete();

            CallEndOfRequestCallbackOnceAfterAllIoComplete();
        }
        else if (rc != 0 && async) {
            // on async failure default to [....] path
            async = false;
            
            if (!inAsyncFlush) {
                // call DoneWithSession
                UnsafeNativeMethods.EcbFlushCore(_ecb, null, null, 0, 0, 0, null, null, 1, _asyncFinalStatus, 0, 0, null);
            }
            
            if (_asyncFlushCompletionCallback != null) {
                // unroot
                _rootedThis.Free();

                // decrement async io count
                Interlocked.Decrement(ref _asyncIoCount);
            }

            if (inAsyncFlush) {
                _asyncResultBase = null;
                // treat every error as if the client disconnected
                IncrementRequestsDisconnected();
                throw new HttpException(SR.GetString(SR.ClientDisconnected), rc);
            }
        }
        else if (rc != 0 && !async && doneWithSession == 0 && !_serverSupportFunctionError) {
            //on non-async failure stop executing the request

            //only throw once
            _serverSupportFunctionError = true;

            string message = SR.Server_Support_Function_Error;

            //give different error if connection was closed
            if (rc == HResults.WSAECONNABORTED || rc == HResults.WSAECONNRESET) {
                message = SR.Server_Support_Function_Error_Disconnect;
                IncrementRequestsDisconnected();
            }

            throw new HttpException(SR.GetString(message, rc.ToString("X8", CultureInfo.InvariantCulture)), rc);
        }
    }

    private void OnAsyncFlushCompletion(IntPtr ecb, int byteCount, int error) {
        try {
            FlushAsyncResult flushAsyncResult = _asyncResultBase as FlushAsyncResult;
            
            // unroot
            _rootedThis.Free();            

            if (flushAsyncResult == null) {
                // call DoneWithSession
                UnsafeNativeMethods.EcbFlushCore(ecb, null, null, 0, 0, 0, null, null, 1, _asyncFinalStatus, 0, 0, null);
            }
            else {
                flushAsyncResult.HResult = error;
            }

            // unlock pinned memory (at the latest of this completion and exit from the FlushCore on stack)
            UnlockCachedResponseBytesOnceAfterIoComplete();

            // Revert any impersonation set by IIS
            UnsafeNativeMethods.RevertToSelf();

            if (flushAsyncResult == null) {
                // call the HttpRuntime to recycle buffers (at the latest of this completion and EndRequest)
                CallEndOfRequestCallbackOnceAfterAllIoComplete();
            }
        }
        finally {
            // decrement async io count
            Interlocked.Decrement(ref _asyncIoCount);
        }
    }

    // WOS 1555777: kernel cache support
    // If the response can be kernel cached, return the kernel cache key;
    // otherwise return null.  The kernel cache key is used to invalidate
    // the entry if a dependency changes or the item is flushed from the
    // managed cache for any reason.
    internal override string SetupKernelCaching(int secondsToLive, string originalCacheUrl, bool enableKernelCacheForVaryByStar) {
        // if someone called DisableKernelCache, don't setup kernel caching
        if (_ecb == IntPtr.Zero || _disableKernelCache)
            return null;

        string cacheUrl = GetUnicodeServerVariable(CACHE_URL);

        // if we're re-inserting the response into the kernel cache, the original key must match
        if (originalCacheUrl != null && originalCacheUrl != cacheUrl) {
            return null;
        }

        // If the request contains a query string, don't kernel cache the entry
        if (String.IsNullOrEmpty(cacheUrl) || (!enableKernelCacheForVaryByStar && cacheUrl.IndexOf('?') != -1)) {
            return null;
        }

        // enable kernel caching (IIS will set the HTTP_CACHE_POLICY when we call VectorSend)
        _cacheInKernelMode = true;
        
        // okay, the response will be kernel cached, here's the key
        return cacheUrl;
    }

    // WOS 1555777: kernel cache support
    internal override void DisableKernelCache() {
        _disableKernelCache = true;
        _cacheInKernelMode = false;
    }

    //
    // Async Flush support
    //

    public override bool SupportsAsyncFlush { get { return true; } }

    // Begin an asynchronous flush of the response.  To support this, the worker request buffers
    // the status, headers, and resonse body until an asynchronous flush operation is initiated.
    public override IAsyncResult BeginFlush(AsyncCallback callback, Object state) {
        if (_ecb == IntPtr.Zero) {
            throw new InvalidOperationException();
        }
        
        FlushAsyncResult ar = new FlushAsyncResult(callback, state);
        
        // we only allow one async operation at a time
        if (Interlocked.CompareExchange(ref _asyncResultBase, ar, null) != null)
            throw new InvalidOperationException(SR.GetString(SR.Async_operation_pending));

        // initiate async operation here
        if (_asyncCompletionCallback == null) {
            _asyncCompletionCallback = new AsyncCompletionCallback(OnAsyncCompletion);
        }

        try {
            ar.MarkCallToBeginMethodStarted();
            FlushResponse(finalFlush: false);
        }
        finally {
            ar.MarkCallToBeginMethodCompleted();
        }

        return ar;
    }

    // Finish an asynchronous flush.
    public override void EndFlush(IAsyncResult asyncResult) {
        if (asyncResult == null)
            throw new ArgumentNullException("asyncResult");
        FlushAsyncResult ar = asyncResult as FlushAsyncResult;
        if (ar == null)
            throw new ArgumentException(null, "asyncResult");

        ar.ReleaseWaitHandleWhenSignaled();
        if (ar.HResult < 0) {
            // treat every error as if the client disconnected
            IncrementRequestsDisconnected();
            throw new HttpException(SR.GetString(SR.ClientDisconnected), ar.HResult);
        }
    }


    //
    // Async Read support
    //
    
    public override bool SupportsAsyncRead { get { return true; } }

    internal void OnAsyncCompletion(int bytesCompleted, int hresult, IntPtr pAsyncCompletionContext) {
        if (_asyncResultBase is ReadAsyncResult) 
            _rootedThis.Free();
        // clear _asyncResultBase because when we call Complete the user's callback
        // may begin another async operation
        AsyncResultBase ar = _asyncResultBase;
        _asyncResultBase = null;
        ar.Complete(bytesCompleted, hresult, pAsyncCompletionContext, synchronous: false);
    }

    // Begin an asynchronous read of the request entity body.  To read the entire entity, invoke
    // repeatedly until total bytes read is equal to Request.ContentLength or EndRead indicates
    // that zero bytes were read.  If Request.ContentLength is zero and the request is chunked,
    // then invoke repeatedly until EndRead indicates that zero bytes were read.
    //
    // If an error occurs and the client is no longer connected, an HttpException will be thrown.
    //
    // This implements Stream.BeginRead, and as such, should throw
    // exceptions as described on MSDN when errors occur.
    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, Object state) {
        // Do as Stream does.
        if (buffer == null)
            throw new ArgumentNullException("buffer");
        if (offset < 0)
            throw new ArgumentOutOfRangeException("offset");
        if (count < 0)
            throw new ArgumentOutOfRangeException("count");
        if (buffer.Length - offset < count)
            throw new ArgumentException(SR.GetString(SR.InvalidOffsetOrCount, "offset", "count"));
        if (_ecb == IntPtr.Zero)
            throw new InvalidOperationException();

        ReadAsyncResult ar = new ReadAsyncResult(callback, state, buffer, offset, count, updatePerfCounter: false);

        if (count == 0) {
            ar.Complete(0, HResults.S_OK, IntPtr.Zero, synchronous: true);
            return ar;
        }

        // we only allow one async operation at a time
        if (Interlocked.CompareExchange(ref _asyncResultBase, ar, null) != null)
            throw new InvalidOperationException(SR.GetString(SR.Async_operation_pending));
        
        // initiate async operation here
        if (_asyncCompletionCallback == null) {
            _asyncCompletionCallback = new AsyncCompletionCallback(OnAsyncCompletion);
        }
        _rootedThis = GCHandle.Alloc(this); // root for duration of async operation

        int hresult;
        try {
            ar.MarkCallToBeginMethodStarted();
            hresult = UnsafeNativeMethods.EcbReadClientAsync(_ecb,
                                                             count,
                                                             _asyncCompletionCallback);
        }
        finally {
            ar.MarkCallToBeginMethodCompleted();
        }

        if (hresult < 0) {
            _rootedThis.Free();
            _asyncResultBase = null;
            // treat every error as if the client disconnected
            IncrementRequestsDisconnected();
            throw new HttpException(SR.GetString(SR.ClientDisconnected), hresult);
        }
        else {
            return ar;
        }
    }

    // Finish an asynchronous read.  When this returns zero there is no more to be read.  If Request.ContentLength is non-zero,
    // do not read more bytes then specified by ContentLength, or an error will occur.
    // This implements Stream.EndRead on HttpBufferlessInputStream, and as such, should throw
    // exceptions as described on MSDN when errors occur.
    public override int EndRead(IAsyncResult asyncResult) {
        if (asyncResult == null)
            throw new ArgumentNullException("asyncResult");
        ReadAsyncResult ar = asyncResult as ReadAsyncResult;
        if (ar == null)
            throw new ArgumentException(null, "asyncResult");
        ar.ReleaseWaitHandleWhenSignaled();
        if (ar.HResult < 0) {
            // treat every error as if the client disconnected
            IncrementRequestsDisconnected();
            throw new HttpException(SR.GetString(SR.ClientDisconnected), ar.HResult);
        }
        else {
            return ar.BytesRead;
        }
    }

    private void IncrementRequestsDisconnected() {
        if (!_disconnected) {
            PerfCounters.IncrementGlobalCounter(GlobalPerfCounter.REQUESTS_DISCONNECTED);
            _disconnected = true;
        }
    }

    //
    // ExecuteUrl support
    //

    private ISAPIAsyncCompletionCallback _executeUrlCompletionCallback;
    private HttpAsyncResult _asyncResultOfExecuteUrl;
    private bool _headersSentFromExecuteUrl;

    internal override bool SupportsExecuteUrl {
        get { return true; }
    }

    internal override IAsyncResult BeginExecuteUrl(
                                        String url, String method, String childHeaders,
                                        bool sendHeaders,
                                        bool addUserIndo, IntPtr token, String name, String authType,
                                        byte[] entity,
                                        AsyncCallback cb, Object state) {

        if (_ecb == IntPtr.Zero ||              // after done with session
            _asyncResultOfExecuteUrl != null || // another ExecuteUrl in progress
            (sendHeaders && HeadersSent()))     // asked to send headers, but already sent them
        {
            throw new InvalidOperationException(SR.GetString(SR.Cannot_execute_url_in_this_context));
        }

        if (entity != null && entity.Length > 0) {
            int ret = UnsafeNativeMethods.EcbGetExecUrlEntityInfo(entity.Length, entity, out _entity);
            if (ret != 1) {
                throw new HttpException(SR.GetString(SR.Failed_to_execute_url));
            }
        }

        Debug.Trace("ExecuteUrl", "ISAPIWorkerRequestInProcForIIS6.BeginExecuteUrl: url=\"" + url + "\".");


        HttpAsyncResult ar = new HttpAsyncResult(cb, state);
        _asyncResultOfExecuteUrl = ar;

        _executeUrlCompletionCallback = new ISAPIAsyncCompletionCallback(OnExecuteUrlCompletion);
        _rootedThis = GCHandle.Alloc(this); // root for the duration of ExecuteUrl

        int rc;
        try {
            ar.MarkCallToBeginMethodStarted();
            rc = UnsafeNativeMethods.EcbExecuteUrlUnicode(_ecb,
                                        url, method, childHeaders,
                                        sendHeaders,
                                        addUserIndo, token, name, authType,
                                        _entity,
                                        _executeUrlCompletionCallback);
        }
        finally {
            ar.MarkCallToBeginMethodCompleted();
        }

        if (rc != 1) {
            if (_entity != IntPtr.Zero) {
                UnsafeNativeMethods.EcbFreeExecUrlEntityInfo(_entity);
            }
            _rootedThis.Free();
            _asyncResultOfExecuteUrl = null;

            Debug.Trace("ExecuteUrl", "ISAPIWorkerRequestInProcForIIS6.BeginExecuteUrl: failed!");

            throw new HttpException(SR.GetString(SR.Failed_to_execute_url));
        }

        if (sendHeaders) {
            // ExecuteUrl will send headers, worker request should not
            _headersSentFromExecuteUrl = true;
        }

        return ar;
    }

    internal override void EndExecuteUrl(IAsyncResult result) {

        Debug.Trace("ExecuteUrl", "ISAPIWorkerRequestInProcForIIS6.EndExecuteUrl");

        HttpAsyncResult asyncResult = result as HttpAsyncResult;
        if (asyncResult != null) {
            asyncResult.End();
        }
    }

    private void OnExecuteUrlCompletion(IntPtr ecb, int byteCount, int error) {
        if (_entity != IntPtr.Zero) {
            UnsafeNativeMethods.EcbFreeExecUrlEntityInfo(_entity);
        }

        _rootedThis.Free();

        Debug.Trace("ExecuteUrl", "ISAPIWorkerRequestInProcForIIS6.OnExecuteUrlCompletion");

        // signal async caller to resume work
        HttpAsyncResult asyncResult = _asyncResultOfExecuteUrl;
        _asyncResultOfExecuteUrl = null;
        asyncResult.Complete(false, null, null);
    }
}

//
// Out-of-process worker request
//

internal class ISAPIWorkerRequestOutOfProc : ISAPIWorkerRequest {

    // sends chunks separately if the total length exceeds the following
    // to relieve the memory pressure on named pipes
    const int PM_FLUSH_THRESHOLD = 31*1024;

    internal ISAPIWorkerRequestOutOfProc(IntPtr ecb) : base(ecb) {
        UnsafeNativeMethods.PMGetTraceContextId(ecb, out _traceId);
    }

    private bool _useBaseTime = false;
    private const int _numServerVars = 32;
    private IDictionary _serverVars;

    private static String[] _serverVarNames =
        new String[_numServerVars] {
            "APPL_MD_PATH", /* this one is not UTF8 so we don't decode it here */
            "ALL_RAW",
            "AUTH_PASSWORD",
            "AUTH_TYPE",
            "CERT_COOKIE",
            "CERT_FLAGS",
            "CERT_ISSUER",
            "CERT_KEYSIZE",
            "CERT_SECRETKEYSIZE",
            "CERT_SERIALNUMBER",
            "CERT_SERVER_ISSUER",
            "CERT_SERVER_SUBJECT",
            "CERT_SUBJECT",
            "GATEWAY_INTERFACE",
            "HTTP_COOKIE",
            "HTTP_USER_AGENT",
            "HTTPS",
            "HTTPS_KEYSIZE",
            "HTTPS_SECRETKEYSIZE",
            "HTTPS_SERVER_ISSUER",
            "HTTPS_SERVER_SUBJECT",
            "INSTANCE_ID",
            "INSTANCE_META_PATH",
            "LOCAL_ADDR",
            "LOGON_USER",
            "REMOTE_ADDR",
            "REMOTE_HOST",
            "SERVER_NAME",
            "SERVER_PORT",
            "SERVER_PROTOCOL",
            "SERVER_SOFTWARE",
            "REMOTE_PORT"
        };


    private void GetAllServerVars() {
        if (_ecb == IntPtr.Zero)
            return;
        RecyclableByteBuffer buf = new RecyclableByteBuffer();

        int r = UnsafeNativeMethods.PMGetAllServerVariables(_ecb, buf.Buffer, buf.Buffer.Length);

        while (r < 0) {
            buf.Resize(-r);     // buffer not big enough
            r = UnsafeNativeMethods.PMGetAllServerVariables(_ecb, buf.Buffer, buf.Buffer.Length);
        }

        if (r == 0)
            throw new HttpException(SR.GetString(SR.Cannot_retrieve_request_data));

        // stub out first server var is it could contain non-UTF8 data
        // convert to characters and split the buffer into strings using default request encoding

        String[] ss = buf.GetDecodedTabSeparatedStrings(Encoding.Default, _numServerVars-1, 1);

        // recycle buffers

        buf.Dispose();

        // fill in the hashtable

        _serverVars = new Hashtable(_numServerVars, StringComparer.OrdinalIgnoreCase);

        _serverVars.Add("APPL_MD_PATH", HttpRuntime.AppDomainAppId);

        for (int i = 1; i < _numServerVars; i++) {       // starting with 1 to skip APPL_MD_PATH
            _serverVars.Add(_serverVarNames[i], ss[i-1]);
        }
    }


    internal override int GetBasicsCore(byte[] buffer, int size, int[] contentInfo) {
        if (_ecb == IntPtr.Zero)
            return 0;

        return UnsafeNativeMethods.PMGetBasics(_ecb, buffer, size, contentInfo);
    }

    internal override int GetQueryStringCore(int encode, StringBuilder buffer, int size) {
        if (_ecb == IntPtr.Zero)
            return 0;

        return UnsafeNativeMethods.PMGetQueryString(_ecb, encode, buffer, size);
    }

    internal override int GetQueryStringRawBytesCore(byte[] buffer, int size) {
        if (_ecb == IntPtr.Zero)
            return 0;

        return UnsafeNativeMethods.PMGetQueryStringRawBytes(_ecb, buffer, size);
    }

    internal override int GetPreloadedPostedContentCore(byte[] bytes, int offset, int numBytesToRead) {
        if (_ecb == IntPtr.Zero)
            return 0;
        int rc = UnsafeNativeMethods.PMGetPreloadedPostedContent(_ecb, bytes, offset, numBytesToRead);
        if (rc > 0)
            PerfCounters.IncrementCounterEx(AppPerfCounter.REQUEST_BYTES_IN, rc);
        return rc;
    }

    internal override int GetAdditionalPostedContentCore(byte[] bytes, int offset, int bufferSize) {
        if (_ecb == IntPtr.Zero)
            return 0;
        int rc = UnsafeNativeMethods.PMGetAdditionalPostedContent(_ecb, bytes, offset, bufferSize);
        if (rc > 0)
            PerfCounters.IncrementCounterEx(AppPerfCounter.REQUEST_BYTES_IN, rc);
        return rc;
    }

    internal override int IsClientConnectedCore() {
        if (_ecb == IntPtr.Zero)
            return 0;

        return UnsafeNativeMethods.PMIsClientConnected(_ecb);
    }

    // PackageFile for IIS5
    internal override MemoryBytes PackageFile(string filename, long offset64, long length64, bool isImpersonating) {
        // The offset and length must be less than Int32.MaxValue for IIS5. 
        // This should be true, since HttpFileResponseElement.ctor throws ArgumentOutOfRangeException for IIS5.
        Debug.Assert(offset64 < Int32.MaxValue);
        Debug.Assert(length64 < Int32.MaxValue);
        int offset = Convert.ToInt32(offset64);
        int length = Convert.ToInt32(length64);

        byte[] offsetBytes = BitConverter.GetBytes(offset);
        byte[] lengthBytes = BitConverter.GetBytes(length);
        byte[] nameBytes = Encoding.Unicode.GetBytes(filename);
        // buffer consists of 1 byte for impersonation flag, 3 bytes for alignment, 
        // 4 bytes for offset, 4 bytes for length, n bytes for file name, 2 bytes for null terminator
        byte[] buffer = new byte[4 + offsetBytes.Length + lengthBytes.Length + nameBytes.Length + 2];

        // first byte indicates whether impersonation is used
        if (isImpersonating)
            buffer[0] = 0x31;
        else
            buffer[0] = 0x30;

        // bytes 2 thru 4 are unused for alignment
        // bytes 5 thru 8 are the offset
        Buffer.BlockCopy(offsetBytes, 0, buffer, 4, offsetBytes.Length);
        // bytes 9 thru 12 are the length
        Buffer.BlockCopy(lengthBytes, 0, buffer, 4 + offsetBytes.Length, lengthBytes.Length);
        // last two bytes are 0 for null terminator
        Buffer.BlockCopy(nameBytes, 0, buffer, 4 + offsetBytes.Length + lengthBytes.Length, nameBytes.Length);

        return new MemoryBytes(buffer, buffer.Length, true, length);
    }

    // ISAPIWorkerRequestOutOfProc
    internal override void FlushCore(byte[]     status,
                                     byte[]     header,
                                     int        keepConnected,
                                     int        totalBodySize,
                                     int        numBodyFragments,
                                     IntPtr[]   bodyFragments,
                                     int[]      bodyFragmentLengths,
                                     int        doneWithSession,
                                     int        finalStatus,
                                     out bool   async) {
        async = false;

        if (_ecb == IntPtr.Zero)
            return;


        if (numBodyFragments > 1) {
            // Don't flush all at once if the length is over the threshold

            int i = 0;
            while (i < numBodyFragments) {
                bool first = (i == 0);

                int size = bodyFragmentLengths[i];
                bool useTransmitFile = (bodyFragmentLengths[i] < 0);
                int idx = i+1;
                if (!useTransmitFile) {
                    while (idx < numBodyFragments
                           && size + bodyFragmentLengths[idx] < PM_FLUSH_THRESHOLD
                           && bodyFragmentLengths[idx] >= 0) {
                        size += bodyFragmentLengths[idx];
                        idx++;
                    }
                }

                bool last = (idx == numBodyFragments);

                // bodyFragmentLength is negative for TransmitFile, but totalBodySize argument must be positive.
                if (useTransmitFile)
                    size = -size;

                UnsafeNativeMethods.PMFlushCore(
                                        _ecb,
                                        first ? status : null,
                                        first ? header : null,
                                        keepConnected,
                                        size,
                                        i,
                                        idx-i,
                                        bodyFragments,
                                        bodyFragmentLengths,
                                        last ? doneWithSession : 0,
                                        last ? finalStatus : 0);

                i = idx;
            }
        }
        else {
            // Everything in one chunk
            UnsafeNativeMethods.PMFlushCore(
                                    _ecb,
                                    status,
                                    header,
                                    keepConnected,
                                    totalBodySize,
                                    0,
                                    numBodyFragments,
                                    bodyFragments,
                                    bodyFragmentLengths,
                                    doneWithSession,
                                    finalStatus);
        }
    }

    internal override int CloseConnectionCore() {
        if (_ecb == IntPtr.Zero)
            return 0;

        return UnsafeNativeMethods.PMCloseConnection(_ecb);
    }

    internal override int MapUrlToPathCore(String url, byte[] buffer, int size) {
        if (_ecb == IntPtr.Zero)
            return 0;

        return UnsafeNativeMethods.PMMapUrlToPath(_ecb, url, buffer, size);
    }

    internal override IntPtr GetUserTokenCore() {
        if (_token == IntPtr.Zero && _ecb != IntPtr.Zero)
            _token = UnsafeNativeMethods.PMGetImpersonationToken(_ecb);

        return _token;
    }

    internal override IntPtr GetVirtualPathTokenCore() {
        if (_token == IntPtr.Zero && _ecb != IntPtr.Zero)
            _token = UnsafeNativeMethods.PMGetVirtualPathToken(_ecb);

        return _token;
    }

    internal override int AppendLogParameterCore(String logParam) {
        if (_ecb == IntPtr.Zero)
            return 0;

        return UnsafeNativeMethods.PMAppendLogParameter(_ecb, logParam);
    }

    internal override int GetClientCertificateCore(byte[] buffer, int [] pInts, long [] pDates) {
        if (_ecb == IntPtr.Zero)
            return 0;

        return UnsafeNativeMethods.PMGetClientCertificate(_ecb, buffer, buffer.Length, pInts, pDates);
    }

    public override String GetServerVariable(String name) {
        // PATH_TRANSLATED is mangled -- do not use the original server variable
        if (name.Equals("PATH_TRANSLATED"))
            return GetFilePathTranslated();

        if (_serverVars == null)
            GetAllServerVars();

        return (String)_serverVars[name];
    }

    internal override int CallISAPI(UnsafeNativeMethods.CallISAPIFunc iFunction, byte [] bufIn, byte [] bufOut) {
        if (_ecb == IntPtr.Zero)
            return 0;

        return UnsafeNativeMethods.PMCallISAPI(_ecb, iFunction, bufIn, bufIn.Length, bufOut, bufOut.Length);
    }

    internal override void SendEmptyResponse() {
        if (_ecb == IntPtr.Zero)
            return;

        UnsafeNativeMethods.PMEmptyResponse(_ecb);
    }

    internal override DateTime GetStartTime() {
        if (_ecb == IntPtr.Zero || _useBaseTime)
            return base.GetStartTime();

        long fileTime = UnsafeNativeMethods.PMGetStartTimeStamp(_ecb);

        return DateTimeUtil.FromFileTimeToUtc(fileTime);
    }

    internal override void ResetStartTime() {
        base.ResetStartTime();
        _useBaseTime = true;
    }

}

}
