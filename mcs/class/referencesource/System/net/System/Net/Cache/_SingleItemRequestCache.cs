/*++
Copyright (c) Microsoft Corporation

Module Name:

    _SingleItemRequestCache.cs

Abstract:
    Request Caching subsystem capable of caching one file at a time.
    Used by, for example, auto-proxy script downloading.

Author:
    Justin Brown - Aug 2, 2004

Revision History:

--*/

namespace System.Net.Cache
{
    using System;
    using System.Net;
    using System.Diagnostics;
    using System.Text;
    using System.IO;
    using System.Collections.Specialized;
    using System.Threading;
    using System.Collections;

    internal class SingleItemRequestCache :
#if !FEATURE_PAL
        Microsoft.Win32.WinInetCache
#else
        RequestCache
#endif
    {
        bool _UseWinInet;
        FrozenCacheEntry _Entry;

        private sealed class FrozenCacheEntry: RequestCacheEntry {
            byte[] _StreamBytes;
            string _Key;
            
            public FrozenCacheEntry(string key, RequestCacheEntry entry, Stream stream): this(key, entry, GetBytes(stream))
            {
            }
            public FrozenCacheEntry(string key, RequestCacheEntry entry, byte[] streamBytes): base()
            {
                _Key = key;
                _StreamBytes = streamBytes;
                 IsPrivateEntry = entry.IsPrivateEntry;
                 StreamSize = entry.StreamSize;
                 ExpiresUtc = entry.ExpiresUtc;
                 HitCount = entry.HitCount;
                 LastAccessedUtc = entry.LastAccessedUtc;
                 entry.LastModifiedUtc = entry.LastModifiedUtc;
                 LastSynchronizedUtc = entry.LastSynchronizedUtc;
                 MaxStale = entry.MaxStale;
                 UsageCount = entry.UsageCount;
                 IsPartialEntry = entry.IsPartialEntry;
                 EntryMetadata = entry.EntryMetadata;
                 SystemMetadata  = entry.SystemMetadata;
            }

            static byte[] GetBytes(Stream stream)
            {
                byte[] bytes;
                bool   resize = false;
                if (stream.CanSeek)
                    bytes = new byte[stream.Length];
                else
                {
                    resize = true;
                    bytes = new byte[4096*2];
                }
                
                int offset = 0;
                while (true)
                {   int read = stream.Read(bytes, offset, bytes.Length-offset);
                    if (read == 0)
                        break;
                    if ((offset+=read) == bytes.Length && resize)
                    {
                        byte[] newBytes = new byte[bytes.Length+4096*2];
                        Buffer.BlockCopy(bytes, 0, newBytes, 0, offset);
                        bytes = newBytes;
                    }
                }
                if (resize)
                {
                    byte[] newBytes = new byte[offset];
                    Buffer.BlockCopy(bytes, 0, newBytes, 0, offset);
                    bytes = newBytes;
                }
                return bytes;
            }

            public static FrozenCacheEntry Create(FrozenCacheEntry clonedObject)
            {
                return (object)clonedObject == (object)null? null: (FrozenCacheEntry) clonedObject.MemberwiseClone();
            }

            public byte[] StreamBytes { get {return _StreamBytes;}}
            public string Key         { get  {return _Key;}}
        }


        internal SingleItemRequestCache(bool useWinInet) :
#if !FEATURE_PAL
            base(true, true, false)
#else
            base(true, true)
#endif
        {
            _UseWinInet = useWinInet;
        }

        //  Returns a read data stream and metadata associated with a cached entry.
        //  Returns Stream.Null if there is no entry found.
        // <remarks> An opened cache entry be preserved until the stream is closed. </remarks>
        //
        internal override Stream Retrieve(string key, out RequestCacheEntry cacheEntry)
        {
            Stream result;
            if (!TryRetrieve(key, out cacheEntry, out result))
            {
                FileNotFoundException fileNotFoundException = new FileNotFoundException(null, key);
                throw new IOException(SR.GetString(SR.net_cache_retrieve_failure, fileNotFoundException.Message), fileNotFoundException);
            }

            return result;
        }

        // Returns a write cache stream associated with the string Key.
        // Passed parameters allow cache to update an entry metadata accordingly.
        // <remarks>  The commit operation should happen on the stream closure. </remarks>
        //
        internal override Stream Store(string key, long contentLength, DateTime expiresUtc, DateTime lastModifiedUtc, TimeSpan maxStale, StringCollection entryMetadata, StringCollection systemMetadata)
        {
            Stream result;
            if (!TryStore(key, contentLength, expiresUtc, lastModifiedUtc, maxStale, entryMetadata, systemMetadata, out result))
            {
                FileNotFoundException fileNotFoundException = new FileNotFoundException(null, key);
                throw new IOException(SR.GetString(SR.net_cache_retrieve_failure, fileNotFoundException.Message), fileNotFoundException);
            }

            return result;
        }

        //
        // Removes an entry from the cache.
        //
        internal override void Remove(string key)
        {
            if (!TryRemove(key))
            {
                FileNotFoundException fileNotFoundException = new FileNotFoundException(null, key);
                throw new IOException(SR.GetString(SR.net_cache_retrieve_failure, fileNotFoundException.Message), fileNotFoundException);
            }
        }

        //
        // Updates only metadata associated with a cached entry.
        //
        internal override void Update(string key, DateTime expiresUtc, DateTime lastModifiedUtc, DateTime lastSynchronizedUtc, TimeSpan maxStale, StringCollection entryMetadata, StringCollection systemMetadata)
        {
            if (!TryUpdate(key, expiresUtc, lastModifiedUtc, lastSynchronizedUtc, maxStale, entryMetadata, systemMetadata))
            {
                FileNotFoundException fileNotFoundException = new FileNotFoundException(null, key);
                throw new IOException(SR.GetString(SR.net_cache_retrieve_failure, fileNotFoundException.Message), fileNotFoundException);
            }
        }

        internal override bool TryRetrieve(string key, out RequestCacheEntry cacheEntry, out Stream readStream)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            FrozenCacheEntry chkEntry = _Entry;
            cacheEntry = null;
            readStream = null;

            if (chkEntry == null || chkEntry.Key != key)
            {
#if !FEATURE_PAL
                Stream realCacheStream;
                RequestCacheEntry realCacheEntry;
                if (!_UseWinInet || !base.TryRetrieve(key, out realCacheEntry, out realCacheStream))
                    return false;
                
                chkEntry = new FrozenCacheEntry(key, realCacheEntry, realCacheStream);
                // Relasing the WinInet entry earlier because we don't forward metadata-only updates ot it.
                realCacheStream.Close();
                _Entry = chkEntry;
#else
                return false;
#endif
            }
            cacheEntry = FrozenCacheEntry.Create(chkEntry);
            readStream = new ReadOnlyStream(chkEntry.StreamBytes);
            return true;
        }

        internal override bool TryStore(string key, long contentLength, DateTime expiresUtc, DateTime lastModifiedUtc, TimeSpan maxStale, StringCollection entryMetadata, StringCollection systemMetadata, out Stream writeStream)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            RequestCacheEntry requestCacheEntry = new RequestCacheEntry();
            requestCacheEntry.IsPrivateEntry = this.IsPrivateCache;
            requestCacheEntry.StreamSize = contentLength;
            requestCacheEntry.ExpiresUtc = expiresUtc;
            requestCacheEntry.LastModifiedUtc = lastModifiedUtc;
            requestCacheEntry.LastAccessedUtc = DateTime.UtcNow;
            requestCacheEntry.LastSynchronizedUtc = DateTime.UtcNow;
            requestCacheEntry.MaxStale = maxStale;
            requestCacheEntry.HitCount = 0;
            requestCacheEntry.UsageCount = 0;
            requestCacheEntry.IsPartialEntry = false;
            requestCacheEntry.EntryMetadata = entryMetadata;
            requestCacheEntry.SystemMetadata = systemMetadata;

            writeStream = null;
            Stream realWriteStream = null;

#if !FEATURE_PAL
            if (_UseWinInet)
            {
                base.TryStore(key, contentLength, expiresUtc, lastModifiedUtc, maxStale, entryMetadata, systemMetadata, out realWriteStream);
            }
#endif
            
            writeStream = new WriteOnlyStream(key, this, requestCacheEntry, realWriteStream);
            return true;
        }

        private void Commit(string key, RequestCacheEntry tempEntry, byte[] allBytes)
        {
            FrozenCacheEntry chkEntry = new FrozenCacheEntry(key, tempEntry, allBytes);
            _Entry = chkEntry;
        }

        internal override bool TryRemove(string key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

#if !FEATURE_PAL
            if (_UseWinInet)
            {
                base.TryRemove(key);
            }
#endif

            FrozenCacheEntry chkEntry = _Entry;
            
            if (chkEntry != null && chkEntry.Key == key)
                _Entry = null;

            return true;
        }

        internal override bool TryUpdate(string key, DateTime expiresUtc, DateTime lastModifiedUtc, DateTime lastSynchronizedUtc, TimeSpan maxStale, StringCollection entryMetadata, StringCollection systemMetadata)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            FrozenCacheEntry chkEntry = FrozenCacheEntry.Create(_Entry);

            //
            // This class does not forward metadata updates to WinInet to simplify the design and avoid interlocked ops
            //

            if (chkEntry == null || chkEntry.Key != key)
                return true;

            chkEntry.ExpiresUtc = expiresUtc;
            chkEntry.LastModifiedUtc = lastModifiedUtc;
            chkEntry.LastSynchronizedUtc = lastSynchronizedUtc;
            chkEntry.MaxStale = maxStale;
            chkEntry.EntryMetadata = entryMetadata;
            chkEntry.SystemMetadata = systemMetadata;
            _Entry = chkEntry;
            return true;
        }
        //
        // We've chosen to no forward to WinInet metadata-only updates
        // Hence our entries are never locked and this method does nothing
        //
        internal override void UnlockEntry(Stream stream)
        {
        }

        //
        //
        //
        internal class ReadOnlyStream : Stream, IRequestLifetimeTracker {
            private byte[] _Bytes;
            private int    _Offset;
            private bool   _Disposed;
            private int    _ReadTimeout;
            private int    _WriteTimeout;
            private RequestLifetimeSetter m_RequestLifetimeSetter;

            internal ReadOnlyStream(byte[] bytes): base()
            {
                _Bytes  = bytes;
                _Offset = 0;
                _Disposed = false;
                _ReadTimeout = _WriteTimeout = -1;
            }

            public override bool CanRead {get {return true;}}
            public override bool CanSeek {get {return true;}}
            public override bool CanTimeout {get {return true;}}
            public override bool CanWrite  {get {return false;}}
            public override long Length {get {return _Bytes.Length;}}
            public override long Position {
                get {return _Offset;}
                set {
                    if (value < 0 || value > (long)_Bytes.Length)
                        throw new ArgumentOutOfRangeException("value");
                    _Offset = (int)value;
                }
            }

            public override int ReadTimeout {
                get {return _ReadTimeout;}
                set {
                    if (value<=0 && value!=System.Threading.Timeout.Infinite) 
                        throw new ArgumentOutOfRangeException("value", SR.GetString(SR.net_io_timeout_use_gt_zero));
                    _ReadTimeout = value;
                }
            }
            public override int WriteTimeout {
                get {return _WriteTimeout;}
                set {
                    if (value<=0 && value!=System.Threading.Timeout.Infinite)
                        throw new ArgumentOutOfRangeException("value", SR.GetString(SR.net_io_timeout_use_gt_zero));
                    _WriteTimeout = value;
                }
            }

            public override void Flush() {}

            public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, Object state)
            {
                int result = Read(buffer, offset, count);

                LazyAsyncResult ar = new LazyAsyncResult(null, state, callback);
                ar.InvokeCallback(result);
                return ar;
            }
            public override int EndRead(IAsyncResult asyncResult)
            {
                if (asyncResult == null)
                    throw new ArgumentNullException("asyncResult");
                LazyAsyncResult ar = (LazyAsyncResult) asyncResult;
                if (ar.EndCalled) throw new InvalidOperationException(SR.GetString(SR.net_io_invalidendcall, "EndRead"));
                ar.EndCalled = true;
                return (int)ar.InternalWaitForCompletion();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (_Disposed) throw new ObjectDisposedException(GetType().Name);
                if (buffer==null) throw new ArgumentNullException("buffer");
                if (offset<0 || offset>buffer.Length) throw new ArgumentOutOfRangeException("offset");
                if (count<0 || count>buffer.Length-offset) throw new ArgumentOutOfRangeException("count");
                if (_Offset == _Bytes.Length) return 0;
                
                int chkOffset = (int)_Offset;
                count = Math.Min(count, _Bytes.Length - chkOffset);
                System.Buffer.BlockCopy(_Bytes, chkOffset, buffer, offset, count);
                chkOffset += count;
                _Offset = chkOffset;
                return count;
            }
            public override IAsyncResult BeginWrite(byte[] buffer, int offset, int size, AsyncCallback callback, Object state)
            {
                throw new NotSupportedException(SR.GetString(SR.net_readonlystream));
            }
            public override void EndWrite(IAsyncResult asyncResult)
            {
                throw new NotSupportedException(SR.GetString(SR.net_readonlystream));
            }
            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException(SR.GetString(SR.net_readonlystream));
            }


            public override long Seek(long offset, SeekOrigin origin)
            {
                switch (origin)
                {
                case SeekOrigin.Begin:
                        return Position = offset;
                case SeekOrigin.Current:
                        return Position += offset;
                    /// <include file='doc\SeekOrigin.uex' path='docs/doc[@for="SeekOrigin.End"]/*' />
                case SeekOrigin.End:  return Position = _Bytes.Length-offset;
                default:
                    throw new ArgumentException(SR.GetString(SR.net_invalid_enum, "SeekOrigin"), "origin");
                }
            }

            public override void SetLength(long length)
            {
                throw new NotSupportedException(SR.GetString(SR.net_readonlystream));
            }


            protected override void Dispose(bool disposing)
            {
                try {
                    if (!_Disposed) {
                        _Disposed = true;
                       
                        if (disposing) {
                            RequestLifetimeSetter.Report(m_RequestLifetimeSetter);
                        }
                    }
                }
                finally {
                    base.Dispose(disposing);
                }
            }

            internal byte[] Buffer
            {
                get
                {
                    return _Bytes;
                }
            }

            void IRequestLifetimeTracker.TrackRequestLifetime(long requestStartTimestamp)
            {
                Debug.Assert(m_RequestLifetimeSetter == null, "TrackRequestLifetime called more than once.");
                m_RequestLifetimeSetter = new RequestLifetimeSetter(requestStartTimestamp);
            }
        }

        //
        //
        //
        private class WriteOnlyStream: Stream {
            private string                 _Key;
            private SingleItemRequestCache _Cache;
            private RequestCacheEntry      _TempEntry;
            private Stream                 _RealStream;
            private long                   _TotalSize;
            private ArrayList              _Buffers;

            private bool   _Disposed;
            private int    _ReadTimeout;
            private int    _WriteTimeout;

            public WriteOnlyStream(string key, SingleItemRequestCache cache, RequestCacheEntry cacheEntry, Stream realWriteStream)
            {
                _Key = key;
                _Cache = cache;
                _TempEntry = cacheEntry;
                _RealStream = realWriteStream;
                _Buffers = new ArrayList();
            }

            public override bool CanRead {get {return false;}}
            public override bool CanSeek {get {return false;}}

            public override bool CanTimeout {get {return true;}}
            public override bool CanWrite  {get {return true;}}

            public override long Length {get {throw new NotSupportedException(SR.GetString(SR.net_writeonlystream));}}
            
            public override long Position {
                get {throw new NotSupportedException(SR.GetString(SR.net_writeonlystream));}
                set {throw new NotSupportedException(SR.GetString(SR.net_writeonlystream));}
            }

            public override int ReadTimeout {
                get {return _ReadTimeout;}
                set {
                    if (value<=0 && value!=System.Threading.Timeout.Infinite)
                        throw new ArgumentOutOfRangeException("value", SR.GetString(SR.net_io_timeout_use_gt_zero));
                    _ReadTimeout = value;
                }
            }
            public override int WriteTimeout {
                get {return _WriteTimeout;}
                set {
                    if (value<=0 && value!=System.Threading.Timeout.Infinite)
                        throw new ArgumentOutOfRangeException("value", SR.GetString(SR.net_io_timeout_use_gt_zero));
                    _WriteTimeout = value;
                }
            }

            public override void Flush() {}

            public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, Object state)
            {throw new NotSupportedException(SR.GetString(SR.net_writeonlystream));}

            public override int EndRead(IAsyncResult asyncResult)
            {throw new NotSupportedException(SR.GetString(SR.net_writeonlystream));}

            public override int Read(byte[] buffer, int offset, int count)
            {throw new NotSupportedException(SR.GetString(SR.net_writeonlystream));}

            public override long Seek(long offset, SeekOrigin origin)
            {throw new NotSupportedException(SR.GetString(SR.net_writeonlystream));}
            public override void SetLength(long length)
            {throw new NotSupportedException(SR.GetString(SR.net_writeonlystream));}


            public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, Object state)
            {
                Write(buffer, offset, count);
                LazyAsyncResult ar = new LazyAsyncResult(null, state, callback);
                ar.InvokeCallback(null);
                return ar;
            }
            public override void EndWrite(IAsyncResult asyncResult)
            {
                if (asyncResult == null)
                    throw new ArgumentNullException("asyncResult");
                LazyAsyncResult ar = (LazyAsyncResult) asyncResult;
                if (ar.EndCalled) throw new InvalidOperationException(SR.GetString(SR.net_io_invalidendcall, "EndWrite"));
                ar.EndCalled = true;
                ar.InternalWaitForCompletion();
            }
            public override void Write(byte[] buffer, int offset, int count)
            {
                if (_Disposed) throw new ObjectDisposedException(GetType().Name);
                if (buffer==null) throw new ArgumentNullException("buffer");
                if (offset<0 || offset>buffer.Length) throw new ArgumentOutOfRangeException("offset");
                if (count<0  || count>buffer.Length-offset) throw new ArgumentOutOfRangeException("count");
                
                if (_RealStream != null)
                    try {
                        _RealStream.Write(buffer, offset, count);
                    }
                    catch {
                        _RealStream.Close();
                        _RealStream = null;
                    }

                byte[] chunk = new byte[count];
                System.Buffer.BlockCopy(buffer, offset, chunk, 0, count);
                _Buffers.Add(chunk);
                _TotalSize += count;
            }

            protected override void Dispose(bool disposing)
            {
                _Disposed = true;
                base.Dispose(disposing);  // Do we mean to do this here????
                if (disposing) {
                    if (_RealStream != null)
                        try {
                            _RealStream.Close();
                        }
                        catch {
                        }

                    byte[] allBytes = new byte[_TotalSize];
                    int offset = 0;
                    for (int i = 0; i < _Buffers.Count; ++i)
                    {
                        byte[] buffer = (byte[])_Buffers[i];
                        Buffer.BlockCopy(buffer, 0, allBytes, offset, buffer.Length);
                        offset += buffer.Length;
                    }

                    _Cache.Commit(_Key, _TempEntry, allBytes);
                }
            }

        }
    }
}
