// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  BufferedStream2
**
**
===========================================================*/
using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Versioning;
using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Runtime.CompilerServices;

using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System.Security.Permissions;

namespace System.IO {

// This abstract implementation adds thread safe buffering on top 
// of the underlying stream. For most streams, having this intermediate
// buffer translates to better performance due to the costly nature of 
// underlying IO, P/Invoke (such as disk IO). This also improves the locking 
// efficiency when operating under heavy concurrency. The synchronization 
// technique used in this implementation is specifically optimized for IO


// The main differences between this implementation and the existing System.IO.BufferedStream
//  - the design allows for inheritance as opposed to wrapping streams
//  - it is thread safe, though currently only synchronous Write is optimized 


[HostProtection(Synchronization=true)]
internal abstract class BufferedStream2 : Stream
{
    protected internal const int DefaultBufferSize = 32*1024; //32KB or 64KB seems to give the best throughput 

    protected int bufferSize;                       // Length of internal buffer, if it's allocated.
    private byte[] _buffer;                         // Internal buffer.  Alloc on first use.
    
    // At present only concurrent buffer writing is optimized implicitly 
    // while reading relies on explicit locking. 
    
    // Ideally we want these fields to be volatile
    private /*volatile*/ int _pendingBufferCopy;    // How many buffer writes are pending.
    private /*volatile*/ int _writePos;             // Write pointer within shared buffer.
    
    // Should we use a separate buffer for reading Vs writing?
    private /*volatile*/ int _readPos;              // Read pointer within shared buffer.
    private /*volatile*/ int _readLen;              // Number of bytes read in buffer from file.

    // Ideally we want this field to be volatile but Interlocked operations 
    // on 64bit int is not guaranteed to be atomic especially on 32bit platforms
    protected long pos;                             // Cache current location in the underlying stream.

    // Derived streams should override CanRead/CanWrite/CanSeek to enable/disable functionality as desired

    [ResourceExposure(ResourceScope.None)]
    [ResourceConsumption(ResourceScope.AppDomain, ResourceScope.AppDomain)]
    public override void Write(byte[] array, int offset, int count) 
    {
        if (array==null)
            throw new ArgumentNullException("array", SR.GetString(SR.ArgumentNull_Buffer));
        if (offset < 0)
            throw new ArgumentOutOfRangeException("offset", SR.GetString(SR.ArgumentOutOfRange_NeedNonNegNum));
        if (count < 0)
            throw new ArgumentOutOfRangeException("count", SR.GetString(SR.ArgumentOutOfRange_NeedNonNegNum));
        if (array.Length - offset < count)
            throw new ArgumentException(SR.GetString(SR.Argument_InvalidOffLen));

        Debug.Assert((_readPos==0 && _readLen==0 && _writePos >= 0) || (_writePos==0 && _readPos <= _readLen), "We're either reading or writing, but not both.");

        if (_writePos==0) {
            // Ensure we can write to the stream, and ready buffer for writing.
            if (!CanWrite) __Error.WriteNotSupported();
            if (_readPos < _readLen) FlushRead();
            _readPos = 0;
            _readLen = 0;
        }

        // Optimization: 
        // Don't allocate a buffer then call memcpy for 0 bytes.
        if (count == 0) 
            return;  

        do {
            // Avoid contention around spilling over the buffer, the locking mechanism here is bit unconventional.
            // Let's call this a YieldLock. It is closer to a spin lock than a semaphore but not quite a spin lock. 
            // Forced thread context switching is better than a tight spin lock here for several reasons. 
            // We utilize less CPU, yield to other threads (potentially the one doing the write, this is 
            // especially important under heavy thread/processor contention environment) and also yield to
            // runtime thread aborts (important when run from a high pri thread like finalizer).
            if (_writePos > bufferSize) {
                Thread.Sleep(1);
                continue;
            }

            // Optimization: 
            // For input chunk larger than internal buffer size, write directly
            // It is okay to have a ---- here with the _writePos check, which means
            // we have a loose order between flushing the intenal cache Vs writing 
            // this larger chunk but that is fine. This step will nicely optimize 
            // repeated writing of larger chunks by skipping the interlocked operation
            if ((_writePos == 0) && (count >= bufferSize)) {
                WriteCore(array, offset, count, true);
                return;
            }

            // We should review whether we need critical region markers for hosts. 
            Thread.BeginCriticalRegion();

            Interlocked.Increment(ref _pendingBufferCopy);
            int newPos = Interlocked.Add(ref _writePos, count);
            int oldPos = (newPos - count);

            // Clear the buffer
            if (newPos > bufferSize) {
                Interlocked.Decrement(ref _pendingBufferCopy);
                Thread.EndCriticalRegion();
                
                // Though the lock below is not necessary for correctness, when operating in a heavy 
                // thread contention environment, augmenting the YieldLock techinique with a critical 
                // section around write seems to be giving slightly better performance while 
                // not having noticable impact in the less contended situations.
                // Perhaps we can build a technique that keeps track of the contention?
                //lock (this) 
                {
                    // Make sure we didn't get pre-empted by another thread
                    if (_writePos  > bufferSize) {
                        if ((oldPos  <= bufferSize) && (oldPos  > 0)) {
                            while (_pendingBufferCopy != 0) {
                                Thread.SpinWait(1);
                            }
                            WriteCore(_buffer, 0, oldPos, true);
                            _writePos = 0;
                        }
                    }
                }
            }
            else {
                if (_buffer == null)
                    Interlocked.CompareExchange(ref _buffer, new byte[bufferSize], null);

                // Copy user data into buffer, to write at a later date.
                Buffer.BlockCopy(array, offset, _buffer, oldPos, count);
                
                Interlocked.Decrement(ref _pendingBufferCopy);
                Thread.EndCriticalRegion();
                return;
            }

        } while (true);
    } 

#if _ENABLE_STREAM_FACTORING
    public override long Position 
    {
        // Making the getter thread safe is not very useful anyways 
        get { 
            if (!CanSeek) __Error.SeekNotSupported();
            Debug.Assert((_readPos==0 && _readLen==0 && _writePos >= 0) || (_writePos==0 && _readPos <= _readLen), "We're either reading or writing, but not both.");

            // Compensate for buffer that we read from the handle (_readLen) Vs what the user
            // read so far from the internel buffer (_readPos). Of course add any unwrittern  
            // buffered data
            return pos + (_readPos - _readLen + _writePos);
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        set {
            if (value < 0) throw new ArgumentOutOfRangeException("value", Helper.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            
            if (_writePos > 0) FlushWrite(false);
            _readPos = 0;
            _readLen = 0;

            Seek(value, SeekOrigin.Begin);
        }
    }

    [MethodImplAttribute(MethodImplOptions.Synchronized)]
    public override int Read(/*[In, Out]*/ byte[] array, int offset, int count) 
    {
        if (array == null)
            throw new ArgumentNullException("array", Helper.GetResourceString("ArgumentNull_Buffer"));
        if (offset < 0)
            throw new ArgumentOutOfRangeException("offset", Helper.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
        if (count < 0)
            throw new ArgumentOutOfRangeException("count", Helper.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
        if (array.Length - offset < count)
            throw new ArgumentException(Helper.GetResourceString("Argument_InvalidOffLen"));
        
        Debug.Assert((_readPos==0 && _readLen==0 && _writePos >= 0) || (_writePos==0 && _readPos <= _readLen), "We're either reading or writing, but not both.");

        bool isBlocked = false;
        int n = _readLen - _readPos;

        // If the read buffer is empty, read into either user's array or our
        // buffer, depending on number of bytes user asked for and buffer size.
        if (n == 0) {
            if (!CanRead) __Error.ReadNotSupported();
            if (_writePos > 0) FlushWrite(false);
            if (!CanSeek || (count >= bufferSize)) {
                n = ReadCore(array, offset, count);
                // Throw away read buffer.
                _readPos = 0;
                _readLen = 0;
                return n;
            }
            if (_buffer == null) _buffer = new byte[bufferSize];
            n = ReadCore(_buffer, 0, bufferSize);
            if (n == 0) return 0;
            isBlocked = n < bufferSize;
            _readPos = 0;
            _readLen = n;
        }

        // Now copy min of count or numBytesAvailable (ie, near EOF) to array.
        if (n > count) n = count;
        Buffer.BlockCopy(_buffer, _readPos, array, offset, n);
        _readPos += n;

        // We may have read less than the number of bytes the user asked 
        // for, but that is part of the Stream contract.  Reading again for
        // more data may cause us to block if we're using a device with 
        // no clear end of file, such as a serial port or pipe.  If we
        // blocked here & this code was used with redirected pipes for a
        // process's standard output, this can lead to deadlocks involving
        // two processes. But leave this here for files to avoid what would
        // probably be a breaking change.         -- 

        return n;
    }

    [HostProtection(ExternalThreading=true)]
    [MethodImplAttribute(MethodImplOptions.Synchronized)]
    public override IAsyncResult BeginRead(byte[] array, int offset, int numBytes, AsyncCallback userCallback, Object stateObject)
    {
        if (array==null)
            throw new ArgumentNullException("array");
        if (offset < 0)
            throw new ArgumentOutOfRangeException("offset", Helper.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
        if (numBytes < 0)
            throw new ArgumentOutOfRangeException("numBytes", Helper.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
        if (array.Length - offset < numBytes)
            throw new ArgumentException(Helper.GetResourceString("Argument_InvalidOffLen"));

        if (!CanRead) __Error.ReadNotSupported();

        Debug.Assert((_readPos==0 && _readLen==0 && _writePos >= 0) || (_writePos==0 && _readPos <= _readLen), "We're either reading or writing, but not both.");
        
        if (_writePos > 0) FlushWrite(false);
        if (_readPos == _readLen) {
            // I can't see how to handle buffering of async requests when 
            // filling the buffer asynchronously, without a lot of complexity.
            // The problems I see are issuing an async read, we do an async 
            // read to fill the buffer, then someone issues another read 
            // (either synchronously or asynchronously) before the first one 
            // returns.  This would involve some sort of complex buffer locking
            // that we probably don't want to get into, at least not in V1.
            // If we did a sync read to fill the buffer, we could avoid the
            // problem, and any async read less than 64K gets turned into a
            // synchronous read by NT anyways...       -- 

            if (numBytes < bufferSize) {
                if (_buffer == null) _buffer = new byte[bufferSize];
                IAsyncResult bufferRead = BeginReadCore(_buffer, 0, bufferSize, null, null, 0);
                _readLen = EndRead(bufferRead);
                int n = _readLen;
                if (n > numBytes) n = numBytes;
                Buffer.BlockCopy(_buffer, 0, array, offset, n);
                _readPos = n;

                // Fake async
                return BufferedStreamAsyncResult.Complete(n, userCallback, stateObject, false);
            }

            // Here we're making our position pointer inconsistent
            // with our read buffer.  Throw away the read buffer's contents.
            _readPos = 0;
            _readLen = 0;
            return BeginReadCore(array, offset, numBytes, userCallback, stateObject, 0);
        }
        else {
            int n = _readLen - _readPos;
            if (n > numBytes) n = numBytes;
            Buffer.BlockCopy(_buffer, _readPos, array, offset, n);
            _readPos += n;

            if (n >= numBytes) 
                return BufferedStreamAsyncResult.Complete(n, userCallback, stateObject, false);

            // For streams with no clear EOF like serial ports or pipes
            // we cannot read more data without causing an app to block
            // incorrectly.  Pipes don't go down this path 
            // though.  This code needs to be fixed.
            // Throw away read buffer.
            _readPos = 0;
            _readLen = 0;
            
            // WARNING: all state on asyncResult objects must be set before
            // we call ReadFile in BeginReadCore, since the OS can run our
            // callback & the user's callback before ReadFile returns.
            return BeginReadCore(array, offset + n, numBytes - n, userCallback, stateObject, n);
        }
    }

    public unsafe override int EndRead(IAsyncResult asyncResult)
    {
        if (asyncResult == null)
            throw new ArgumentNullException("asyncResult"); 

        BufferedStreamAsyncResult bsar = asyncResult as BufferedStreamAsyncResult;
        if (bsar != null) {
            if (bsar._isWrite)
                __Error.WrongAsyncResult();
            return bsar._numBytes;
        }
        else {
            return EndReadCore(asyncResult);
        }
    }

    [HostProtection(ExternalThreading=true)]
    [MethodImplAttribute(MethodImplOptions.Synchronized)]
    public override IAsyncResult BeginWrite(byte[] array, int offset, int numBytes, AsyncCallback userCallback, Object stateObject)
    {
        if (array==null)
            throw new ArgumentNullException("array");
        if (offset < 0)
            throw new ArgumentOutOfRangeException("offset", Helper.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
        if (numBytes < 0)
            throw new ArgumentOutOfRangeException("numBytes", Helper.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
        if (array.Length - offset < numBytes)
            throw new ArgumentException(Helper.GetResourceString("Argument_InvalidOffLen"));

        if (!CanWrite) __Error.WriteNotSupported();

        Debug.Assert((_readPos==0 && _readLen==0 && _writePos >= 0) || (_writePos==0 && _readPos <= _readLen), "We're either reading or writing, but not both.");

        if (_writePos==0) {
            if (_readPos < _readLen) FlushRead();
            _readPos = 0;
            _readLen = 0;
        }

        int n = bufferSize - _writePos;
        if (numBytes <= n) {
            if (_buffer == null) _buffer = new byte[bufferSize];
            Buffer.BlockCopy(array, offset, _buffer, _writePos, numBytes);
            _writePos += numBytes;

            return BufferedStreamAsyncResult.Complete(numBytes, userCallback, stateObject, true);
        }

        if (_writePos > 0) FlushWrite(false);
        return BeginWriteCore(array, offset, numBytes, userCallback, stateObject);
    }

    public unsafe override void EndWrite(IAsyncResult asyncResult)
    {
        if (asyncResult == null)
            throw new ArgumentNullException("asyncResult"); 

        BufferedStreamAsyncResult bsar = asyncResult as BufferedStreamAsyncResult;
        if (bsar == null) 
            EndWriteCore(asyncResult);
    }

    // Reads a byte from the file stream.  Returns the byte cast to an int
    // or -1 if reading from the end of the stream.
    [MethodImplAttribute(MethodImplOptions.Synchronized)]
    public override int ReadByte() 
    {
        if (_readLen == 0 && !CanRead) 
            __Error.ReadNotSupported();
        
        Debug.Assert((_readPos==0 && _readLen==0 && _writePos >= 0) || (_writePos==0 && _readPos <= _readLen), "We're either reading or writing, but not both.");
        
        if (_readPos == _readLen) {
            if (_writePos > 0) FlushWrite(false);

            Debug.Assert(bufferSize > 0, "bufferSize > 0");
            if (_buffer == null) _buffer = new byte[bufferSize];
            _readLen = ReadCore(_buffer, 0, bufferSize);
            _readPos = 0;
        }

        if (_readPos == _readLen)
            return -1;

        int result = _buffer[_readPos];
        _readPos++;
        return result;
    }

    [MethodImplAttribute(MethodImplOptions.Synchronized)]
    public override void WriteByte(byte value)
    {
        if (_writePos == 0) {
            if (!CanWrite) 
                __Error.WriteNotSupported();
            if (_readPos < _readLen) 
                FlushRead();
            _readPos = 0;
            _readLen = 0;
            Debug.Assert(bufferSize > 0, "bufferSize > 0");
            if (_buffer==null) 
                _buffer = new byte[bufferSize];
        }

        if (_writePos == bufferSize)
            FlushWrite(false);

        _buffer[_writePos] = value;
        _writePos++;
    }


    [MethodImplAttribute(MethodImplOptions.Synchronized)]
    public override long Seek(long offset, SeekOrigin origin) 
    {
        if (origin<SeekOrigin.Begin || origin>SeekOrigin.End)
            throw new ArgumentException(Helper.GetResourceString("Argument_InvalidSeekOrigin"));
        
        if (!CanSeek) __Error.SeekNotSupported();

        Debug.Assert((_readPos==0 && _readLen==0 && _writePos >= 0) || (_writePos==0 && _readPos <= _readLen), "We're either reading or writing, but not both.");

        // If we've got bytes in our buffer to write, write them out.
        // If we've read in and consumed some bytes, we'll have to adjust
        // our seek positions ONLY IF we're seeking relative to the current
        // position in the stream.  This simulates doing a seek to the new
        // position, then a read for the number of bytes we have in our buffer.
        if (_writePos > 0) {
            FlushWrite(false);
        }
        else if (origin == SeekOrigin.Current) {
            // Don't call FlushRead here, which would have caused an infinite
            // loop.  Simply adjust the seek origin.  This isn't necessary
            // if we're seeking relative to the beginning or end of the stream.
            offset -= (_readLen - _readPos);
        }

        long oldPos = pos + (_readPos - _readLen);
        long curPos = SeekCore(offset, origin);

        // We now must update the read buffer.  We can in some cases simply
        // update _readPos within the buffer, copy around the buffer so our 
        // Position property is still correct, and avoid having to do more 
        // reads from the disk.  Otherwise, discard the buffer's contents.
        if (_readLen > 0) {
            // We can optimize the following condition:
            // oldPos - _readPos <= curPos < oldPos + _readLen - _readPos
            if (oldPos == curPos) {
                if (_readPos > 0) {
                    Buffer.BlockCopy(_buffer, _readPos, _buffer, 0, _readLen - _readPos);
                    _readLen -= _readPos;
                    _readPos = 0;
                }
                // If we still have buffered data, we must update the stream's 
                // position so our Position property is correct.
                if (_readLen > 0)
                    SeekCore(_readLen, SeekOrigin.Current);
            }
            else if (oldPos - _readPos < curPos && curPos < oldPos + _readLen - _readPos) {
                int diff = (int)(curPos - oldPos);
                Buffer.BlockCopy(_buffer, _readPos+diff, _buffer, 0, _readLen - (_readPos + diff));
                _readLen -= (_readPos + diff);
                _readPos = 0;
                if (_readLen > 0)
                    SeekCore(_readLen, SeekOrigin.Current);
            }
            else {
                // Lose the read buffer.
                _readPos = 0;
                _readLen = 0;
            }
            Debug.Assert(_readLen >= 0 && _readPos <= _readLen, "_readLen should be nonnegative, and _readPos should be less than or equal _readLen");
            Debug.Assert(curPos == Position, "Seek optimization: curPos != Position!  Buffer math was mangled.");
        }
        return curPos;
    }
#endif //_ENABLE_STREAM_FACTORING

    [MethodImplAttribute(MethodImplOptions.Synchronized)]
    public override void Flush()
    {
        try {
            if (_writePos > 0) 
                FlushWrite(false);
            else if (_readPos < _readLen) 
                FlushRead();
        }
        finally {
            _writePos = 0;
            _readPos = 0;
            _readLen = 0;
        }
    }

    // Reading is done by blocks from the file, but someone could read
    // 1 byte from the buffer then write.  At that point, the OS's file
    // pointer is out of sync with the stream's position.  All write 
    // functions should call this function to preserve the position in the file.
    [MethodImplAttribute(MethodImplOptions.Synchronized)]
    protected void FlushRead() {
#if _ENABLE_STREAM_FACTORING
        Debug.Assert(_writePos == 0, "BufferedStream: Write buffer must be empty in FlushRead!");
        
        if (_readPos - _readLen != 0) {
            Debug.Assert(CanSeek, "BufferedStream will lose buffered read data now.");
            if (CanSeek)
                SeekCore(_readPos - _readLen, SeekOrigin.Current);
        }
        _readPos = 0;
        _readLen = 0;
#endif //_ENABLE_STREAM_FACTORING
    }

    // Writes are buffered.  Anytime the buffer fills up 
    // (_writePos + delta > bufferSize) or the buffer switches to reading
    // and there is left over data (_writePos > 0), this function must be called.
    [MethodImplAttribute(MethodImplOptions.Synchronized)]
    protected void FlushWrite(bool blockForWrite) {
        Debug.Assert(_readPos == 0 && _readLen == 0, "BufferedStream: Read buffer must be empty in FlushWrite!");
        
        if (_writePos > 0) 
            WriteCore(_buffer, 0, _writePos, blockForWrite);
        _writePos = 0;
    }

    protected override void Dispose(bool disposing)
    {
        try {
            // Flush data to disk iff we were writing.  
            if (_writePos > 0) {
                // With our Whidbey async IO & overlapped support for AD unloads,
                // we don't strictly need to block here to release resources 
                // if the underlying IO is overlapped since that support 
                // takes care of the pinning & freeing the 
                // overlapped struct.  We need to do this when called from
                // Close so that the handle is closed when Close returns, but
                // we do't need to call EndWrite from the finalizer.  
                // Additionally, if we do call EndWrite, we block forever 
                // because AD unloads prevent us from running the managed 
                // callback from the IO completion port.  Blocking here when 
                // called from the finalizer during AD unload is clearly wrong, 
                // but we can't use any sort of test for whether the AD is 
                // unloading because if we weren't unloading, an AD unload 
                // could happen on a separate thread before we call EndWrite.
                FlushWrite(disposing);
            }
        }
        finally {
            // Don't set the buffer to null, to avoid a NullReferenceException
            // when users have a race condition in their code (ie, they call
            // Close when calling another method on Stream like Read).
            //_buffer = null;
            _readPos = 0;
            _readLen = 0;
            _writePos = 0;

            base.Dispose(disposing);
        }
    }

    //
    // Helper methods
    //

#if  _ENABLE_STREAM_FACTORING
    protected int BufferedWritePosition 
    {
        // Making the getter thread safe is not very useful anyways 
        get { 
            return _writePos;
        }
        //set {
        //    Interlocked.Exchange(ref _writePos, value);
        //}
    }

    protected int BufferedReadPosition 
    {
        // Making the getter thread safe is not very useful anyways 
        get { 
            return _readPos;
        }
        //set {
        //    Interlocked.Exchange(ref _readPos, value);
        //}
    }
#endif  //_ENABLE_STREAM_FACTORING

    protected long UnderlyingStreamPosition 
    {
        // Making the getter thread safe is not very useful anyways 
        get { 
            return pos;
        }
        set {
            Interlocked.Exchange(ref pos, value);
        }
    }

    protected long AddUnderlyingStreamPosition(long posDelta)
    {
        return Interlocked.Add(ref pos, posDelta);
    }
    
    [MethodImplAttribute(MethodImplOptions.Synchronized)]
    protected internal void DiscardBuffer()
    {
        _readPos = 0;
        _readLen = 0;
        _writePos = 0;
    }

    private void WriteCore(byte[] buffer, int offset, int count, bool blockForWrite) 
    {
        long streamPos;
        WriteCore(buffer, offset, count, blockForWrite, out streamPos);
    }
    protected abstract void WriteCore(byte[] buffer, int offset, int count, bool blockForWrite, out long streamPos);

#if _ENABLE_STREAM_FACTORING
    private int ReadCore(byte[] buffer, int offset, int count) 
    {
        long streamPos;
        return ReadCore(buffer, offset, count, out streamPos);
    }
    
    private int EndReadCore(IAsyncResult asyncResult)
    {
        long streamPos;
        return EndReadCore(asyncResult, out streamPos);
    }

    private void EndWriteCore(IAsyncResult asyncResult)
    {
        long streamPos;
        EndWriteCore(asyncResult, out streamPos);
    }

    // Derived streams should implement the following core methods
    protected abstract int ReadCore(byte[] buffer, int offset, int count, out long streamPos);
    [ResourceExposure(ResourceScope.None)]
    [ResourceConsumption(ResourceScope.AppDomain, ResourceScope.AppDomain)]
    protected abstract IAsyncResult BeginReadCore(byte[] bytes, int offset, int numBytes, AsyncCallback userCallback, Object stateObject, int numBufferedBytesRead);
    protected abstract int EndReadCore(IAsyncResult asyncResult, out long streamPos);
    [ResourceExposure(ResourceScope.None)]
    [ResourceConsumption(ResourceScope.AppDomain, ResourceScope.AppDomain)]
    protected abstract IAsyncResult BeginWriteCore(byte[] bytes, int offset, int numBytes, AsyncCallback userCallback, Object stateObject);
    protected abstract void EndWriteCore(IAsyncResult asyncResult, out long streamPos);
    protected abstract long SeekCore(long offset, SeekOrigin origin);
#endif //_ENABLE_STREAM_FACTORING
}

#if _ENABLE_STREAM_FACTORING
// Fake async result 
unsafe internal sealed class BufferedStreamAsyncResult : IAsyncResult
{
    // User code callback
    internal AsyncCallback _userCallback;
    internal Object _userStateObject;
    internal int _numBytes;     // number of bytes read OR written
    //internal int _errorCode;

    internal bool _isWrite;     // Whether this is a read or a write

    public Object AsyncState
    {
        get { return _userStateObject; }
    }

    public bool IsCompleted
    {
        get { return true; }
    }

    public WaitHandle AsyncWaitHandle
    {
        get { return null; }
    }

    public bool CompletedSynchronously
    {
        get { return true; }
    }

    internal static IAsyncResult Complete(int numBufferedBytes, AsyncCallback userCallback, Object userStateObject, bool isWrite)
    {
        // Fake async
        BufferedStreamAsyncResult asyncResult = new BufferedStreamAsyncResult();
        asyncResult._numBytes = numBufferedBytes;
        asyncResult._userCallback = userCallback;
        asyncResult._userStateObject = userStateObject;
        asyncResult._isWrite = isWrite;
        if (userCallback != null) {
            userCallback(asyncResult);
        }
        return asyncResult;
    }
}
#endif //_ENABLE_STREAM_FACTORING
}
