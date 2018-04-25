/*++
Copyright (c) Microsoft Corporation

Module Name:

    _NegoStream.cs

Abstract:
    The class is used to encrypt/decrypt user data based on established
    security context. Presumably the context belongs to SSPI NEGO or NTLM package.

Author:
    Alexei Vopilov    12-Aug-2003

Revision History:
    12-Aug-2003 New design that has obsoleted Authenticator class
    15-Jan-2004 Converted to a partial class, only internal NegotiateStream implementaion goes into this file.

--*/

namespace System.Net.Security {
    using System;
    using System.IO;
    using System.Security;
    using System.Security.Principal;
    using System.Security.Permissions;
    using System.Threading;

    //
    // This is a wrapping stream that does data encryption/decryption based on a successfully authenticated SSPI context.
    //
    public partial class NegotiateStream: AuthenticatedStream
    {
        private static AsyncCallback _WriteCallback = new AsyncCallback(WriteCallback);
        private static AsyncProtocolCallback _ReadCallback  = new AsyncProtocolCallback(ReadCallback);

        private int         _NestedWrite;
        private int         _NestedRead;
        private byte[]      _ReadHeader;

        // never updated directly, special properties are used
        private byte[]      _InternalBuffer;
        private int         _InternalOffset;
        private int         _InternalBufferCount;

        FixedSizeReader     _FrameReader;

        //
        // Private implemenation
        //

        private void InitializeStreamPart()
        {
            _ReadHeader = new byte[4];
            _FrameReader = new FixedSizeReader(InnerStream);
        }

        //
        //
        private byte[] InternalBuffer {
            get {
                return _InternalBuffer;
            }
        }
        //
        //
        private int InternalOffset {
            get {
                return _InternalOffset;
            }
        }
        //
        private int InternalBufferCount {
            get {
                return _InternalBufferCount;
            }
        }
        //
        //
        private void DecrementInternalBufferCount(int decrCount)
        {
            _InternalOffset += decrCount;
            _InternalBufferCount -= decrCount;
        }
        //
        //
        private void EnsureInternalBufferSize(int bytes)
        {
            _InternalBufferCount = bytes;
            _InternalOffset = 0;
            if (InternalBuffer == null || InternalBuffer.Length < bytes)
            {
                _InternalBuffer = new byte[bytes];
            }
        }
        //
        private void AdjustInternalBufferOffsetSize(int bytes, int offset)
        {
            _InternalBufferCount = bytes;
            _InternalOffset = offset;
        }
        //
        // Validates user parameteres for all Read/Write methods
        //
        private void ValidateParameters(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset");

            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            if (count > buffer.Length-offset)
                throw new ArgumentOutOfRangeException("count", SR.GetString(SR.net_offset_plus_count));
        }
        //
        // Combined sync/async write method. For sync requet asyncRequest==null
        //
        private void ProcessWrite(byte[] buffer, int offset, int count, AsyncProtocolRequest asyncRequest)
        {
            ValidateParameters(buffer, offset, count);

            if (Interlocked.Exchange(ref _NestedWrite, 1) == 1)
            {
                throw new NotSupportedException(SR.GetString(SR.net_io_invalidnestedcall, (asyncRequest != null? "BeginWrite":"Write"), "write"));
            }


            bool failed = false;
            try
            {
                StartWriting(buffer, offset, count, asyncRequest);
            }
            catch (Exception e)
            {
                failed = true;
                if (e is IOException) {
                    throw;
                }
                throw new IOException(SR.GetString(SR.net_io_write), e);
            }
            finally
            {
                if (asyncRequest == null || failed)
                {
                    _NestedWrite = 0;
                }
            }
        }
        //
        //
        //
        private void StartWriting(byte[] buffer, int offset, int count, AsyncProtocolRequest asyncRequest)
        {
            // We loop to this method from the callback
            // If the last chunk was just completed from async callback (count < 0), we complete user request
            if (count >= 0 )
            {
                byte[] outBuffer = null;
                do
                {
                    int chunkBytes = Math.Min(count, NegoState.c_MaxWriteDataSize);
                    int encryptedBytes;

                    try {
                        encryptedBytes = _NegoState.EncryptData(buffer, offset, chunkBytes, ref outBuffer);
                    }
                    catch (Exception e) {
                        throw new IOException(SR.GetString(SR.net_io_encrypt), e);
                    }

                    if (asyncRequest != null)
                    {
                        // prepare for the next request
                        asyncRequest.SetNextRequest(buffer, offset+chunkBytes, count-chunkBytes, null);
                        IAsyncResult ar = InnerStream.BeginWrite(outBuffer, 0, encryptedBytes, _WriteCallback, asyncRequest);
                        if (!ar.CompletedSynchronously)
                        {
                            return;
                        }
                        InnerStream.EndWrite(ar);

                    }
                    else
                    {
                        InnerStream.Write(outBuffer, 0, encryptedBytes);
                    }
                    offset += chunkBytes;
                    count  -= chunkBytes;
                } while (count != 0);
            }

            if (asyncRequest != null) {
                asyncRequest.CompleteUser();
            }
        }
        //
        // Combined sync/async read method. For sync requet asyncRequest==null
        // There is a little overheader because we need to pass buffer/offset/count used only in sync.
        // Still the benefit is that we have a common sync/async code path.
        //
        private int ProcessRead(byte[] buffer, int offset, int count, AsyncProtocolRequest asyncRequest)
        {
            ValidateParameters(buffer, offset, count);

            if (Interlocked.Exchange(ref _NestedRead, 1) == 1)
            {
                throw new NotSupportedException(SR.GetString(SR.net_io_invalidnestedcall, (asyncRequest!=null? "BeginRead":"Read"), "read"));
            }

            bool failed = false;
            try
            {
                if (InternalBufferCount != 0)
                {
                    int copyBytes = InternalBufferCount > count? count:InternalBufferCount;
                    if (copyBytes != 0)
                    {
                        Buffer.BlockCopy(InternalBuffer, InternalOffset, buffer, offset, copyBytes);
                        DecrementInternalBufferCount(copyBytes);
                    }
                    if (asyncRequest != null) {
                        asyncRequest.CompleteUser((object) copyBytes);
                    }
                    return copyBytes;
                }
                // going into real IO
                return StartReading(buffer, offset, count, asyncRequest);
            }
            catch (Exception e)
            {
                failed = true;
                if (e is IOException) {
                    throw;
                }
                throw new IOException(SR.GetString(SR.net_io_read), e);
            }
            finally
            {
                // if sync request or exception
                if (asyncRequest == null || failed)
                {
                    _NestedRead = 0;
                }
            }
        }
        //
        // To avoid recursion when decrypted 0 bytes this method will loop until decryption resulted at least in 1 byte.
        //
        private int StartReading(byte[] buffer, int offset, int count, AsyncProtocolRequest asyncRequest)
        {
            int result;
            // When we read -1 bytes means we have decrypted 0 bytes, need looping.
            while ((result = StartFrameHeader(buffer, offset, count, asyncRequest)) == -1) {
                ;
            }
            return result;
        }

        //
        // Need read frame size first
        //
        private int StartFrameHeader(byte[] buffer, int offset, int count, AsyncProtocolRequest asyncRequest)
        {
            int readBytes = 0;
            if (asyncRequest != null)
            {
                asyncRequest.SetNextRequest(_ReadHeader, 0, _ReadHeader.Length, _ReadCallback);
                _FrameReader.AsyncReadPacket(asyncRequest);
                if (!asyncRequest.MustCompleteSynchronously)
                {
                    return 0;
                }
                readBytes = asyncRequest.Result;
            }
            else
            {
                readBytes = _FrameReader.ReadPacket(_ReadHeader, 0, _ReadHeader.Length);
            }
            return StartFrameBody(readBytes, buffer, offset, count, asyncRequest);
        }
        //
        //
        //
        private int StartFrameBody(int readBytes, byte[] buffer, int offset, int count, AsyncProtocolRequest asyncRequest)
        {
            if (readBytes == 0)
            {
                //EOF
                if (asyncRequest != null)
                {
                    asyncRequest.CompleteUser((object)0);
                }
                return 0;
            }
            GlobalLog.Assert(readBytes == _ReadHeader.Length, "NegoStream::ProcessHeader()|Frame size must be 4 but received {0} bytes.", readBytes);

            //rpelace readBytes with the body size recovered from the header content
            readBytes =  _ReadHeader[3];
            readBytes = (readBytes<<8) | _ReadHeader[2];
            readBytes = (readBytes<<8) | _ReadHeader[1];
            readBytes = (readBytes<<8) | _ReadHeader[0];

            //
            // The body carries 4 bytes for trailer size slot plus trailer, hence <=4 frame size is always an error.
            // Additionally we'd like to restrice the read frame size to modest 64k
            //
            if (readBytes <= 4 || readBytes > NegoState.c_MaxReadFrameSize)
            {
                throw new IOException(SR.GetString(SR.net_frame_read_size));
            }

            //
            // Always pass InternalBuffer for SSPI "in place" decryption.
            // A user buffer can be shared by many threads in that case decryption/integrity check may fail cause of data corruption.
            //
            EnsureInternalBufferSize(readBytes);
            if (asyncRequest != null) //Async
            {
                asyncRequest.SetNextRequest(InternalBuffer, 0, readBytes, _ReadCallback);

                _FrameReader.AsyncReadPacket(asyncRequest);

                if (!asyncRequest.MustCompleteSynchronously)
                {
                    return 0;
                }
                readBytes = asyncRequest.Result;
            }
            else //Sync
            {
                readBytes = _FrameReader.ReadPacket(InternalBuffer, 0, readBytes);
            }
            return ProcessFrameBody(readBytes, buffer, offset, count, asyncRequest);
        }
        //
        //
        //
        private int ProcessFrameBody(int readBytes, byte[] buffer, int offset, int count, AsyncProtocolRequest asyncRequest)
        {
            if (readBytes == 0)
            {
                // We already checked that the frame body is bigger than 0 bytes
                // Hence, this is an EOF ... fire.
                throw new IOException(SR.GetString(SR.net_io_eof));
            }

            //Decrypt into internal buffer, change "readBytes" to count now _Decrypted Bytes_
            int internalOffset;
            readBytes = _NegoState.DecryptData(InternalBuffer, 0, readBytes, out internalOffset);

            // Decrypted data start from zero offset, the size can be shrinked after decryption
            AdjustInternalBufferOffsetSize(readBytes, internalOffset);

            if (readBytes == 0 && count != 0)
            {
                //Read again
                return -1;
            }

            if (readBytes > count)
            {
                readBytes = count;
            }
            Buffer.BlockCopy(InternalBuffer, InternalOffset, buffer, offset, readBytes);

            // This will adjust both the remaining internal buffer count and the offset
            DecrementInternalBufferCount(readBytes);

            if (asyncRequest != null)
            {
                asyncRequest.CompleteUser((object)readBytes);
            }

            return readBytes;
        }
        //
        //
        //
        private static void WriteCallback(IAsyncResult transportResult)
        {
            if (transportResult.CompletedSynchronously)
            {
                return;
            }
            GlobalLog.Assert(transportResult.AsyncState is AsyncProtocolRequest , "NegotiateSteam::WriteCallback|State type is wrong, expected AsyncProtocolRequest.");

            AsyncProtocolRequest asyncRequest = (AsyncProtocolRequest) transportResult.AsyncState;

            try {
                NegotiateStream negoStream = (NegotiateStream)asyncRequest.AsyncObject;
                negoStream.InnerStream.EndWrite(transportResult);
                if (asyncRequest.Count == 0) {
                    // this was the last chunk
                    asyncRequest.Count = -1;
                }
                negoStream.StartWriting(asyncRequest.Buffer, asyncRequest.Offset, asyncRequest.Count, asyncRequest);

            }
            catch (Exception e) {
                if (asyncRequest.IsUserCompleted) {
                    // This will throw on a worker thread.
                    throw;
                }
                asyncRequest.CompleteWithError(e);
            }
        }
        //
        //
        private static void ReadCallback(AsyncProtocolRequest asyncRequest)
        {
            // Async ONLY completion
            try
            {
                NegotiateStream negoStream = (NegotiateStream)asyncRequest.AsyncObject;
                BufferAsyncResult bufferResult = (BufferAsyncResult) asyncRequest.UserAsyncResult;

                // This is not a hack, just optimization to avoid an additional callback.
                //
                if ((object) asyncRequest.Buffer == (object)negoStream._ReadHeader)
                {
                    negoStream.StartFrameBody(asyncRequest.Result, bufferResult.Buffer, bufferResult.Offset, bufferResult.Count, asyncRequest);
                }
                else
                {
                    if (-1 == negoStream.ProcessFrameBody(asyncRequest.Result, bufferResult.Buffer, bufferResult.Offset, bufferResult.Count, asyncRequest))
                    {
                        // in case we decrypted 0 bytes start another reading.
                        negoStream.StartReading(bufferResult.Buffer, bufferResult.Offset, bufferResult.Count, asyncRequest);

                    }
                }
            }
            catch (Exception e)
            {
                if (asyncRequest.IsUserCompleted) {
                    // This will throw on a worker thread.
                    throw;
                }
                asyncRequest.CompleteWithError(e);
            }
        }
    }
}
