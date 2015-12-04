//------------------------------------------------------------------------------
// <copyright file="_ChunkParser.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net.Configuration;
using System.Threading;

namespace System.Net
{
    // This class is a helper for parsing chunked HTTP responses. Usage is to either call Read() ([....]) or ReadAsync()
    // (async) methods to retrieve the response payload (without chunk metadata).
    // The buffer passed to the .ctor is owned by the ChunkParser until the whole response is read (i.e. Read/
    // ReadAsync return 0 bytes) or an error occurs.
    // ChunkParser requires the whole metadata to be in the buffer, i.e. if only parts of a secion (chunk length/
    // extension/trailer header etc.) fit into the buffer, ChunkParser will create a new, larger buffer until the 
    // data can be parsed or the buffer length limit is reached. The limit can be specified in the constructor.
    internal sealed class ChunkParser
    {
        // 12 = CRLF <4-byte value in hex> CRLF: i.e. read CRLF of previous chunk + length + CRLF
        private const int chunkLengthBuffer = 12;
        private const int noChunkLength = -1;

        private static readonly bool[] tokenChars;

        private enum ReadState
        {
            ChunkLength = 0,
            Extension,
            Payload,
            PayloadEnd, // read CRLF
            Trailer,
            Done,
            Error
        }

        private byte[] buffer;
        private int bufferCurrentPos;
        private int bufferFillLength;
        private int maxBufferLength;
        private byte[] userBuffer;
        private int userBufferOffset;
        private int userBufferCount;
        private LazyAsyncResult userAsyncResult;
        private Stream dataSource;
        private ReadState readState;
        private int totalTrailerHeadersLength;

        private int currentChunkLength;
        private int currentChunkBytesRead; // Bytes read so far for the current chunk.
        private int currentOperationBytesRead; // Bytes read so far for the current read operation.
        private int syncResult;

        private bool IsAsync
        {
            get { return userAsyncResult != null; }
        }

        static ChunkParser()
        {
            // The following was copied from System.Net.Http.HttpRuleParser. Consider combining the two implementations.

            // token = 1*<any CHAR except CTLs or separators>
            // CTL = <any US-ASCII control character (octets 0 - 31) and DEL (127)>

            tokenChars = new bool[128]; // everything is false

            for (int i = 33; i < 127; i++) // skip Space (32) & DEL (127)
            {
                tokenChars[i] = true;
            }

            // remove separators: these are not valid token characters
            tokenChars[(byte)'('] = false;
            tokenChars[(byte)')'] = false;
            tokenChars[(byte)'<'] = false;
            tokenChars[(byte)'>'] = false;
            tokenChars[(byte)'@'] = false;
            tokenChars[(byte)','] = false;
            tokenChars[(byte)';'] = false;
            tokenChars[(byte)':'] = false;
            tokenChars[(byte)'\\'] = false;
            tokenChars[(byte)'"'] = false;
            tokenChars[(byte)'/'] = false;
            tokenChars[(byte)'['] = false;
            tokenChars[(byte)']'] = false;
            tokenChars[(byte)'?'] = false;
            tokenChars[(byte)'='] = false;
            tokenChars[(byte)'{'] = false;
            tokenChars[(byte)'}'] = false;
        }

        public ChunkParser(Stream dataSource, byte[] internalBuffer, int initialBufferOffset, int initialBufferCount,
            int maxBufferLength)
        {
            Contract.Requires(dataSource != null);
            Contract.Requires(internalBuffer != null);
            Contract.Requires(internalBuffer.Length >= chunkLengthBuffer,
                "Buffer must be big enough to hold the chunk length.");
            Contract.Requires((initialBufferCount >= 0) && (initialBufferCount <= internalBuffer.Length));
            Contract.Requires((initialBufferOffset >= 0) &&
                (initialBufferOffset + initialBufferCount <= internalBuffer.Length));

            this.dataSource = dataSource;
            this.buffer = internalBuffer;
            this.bufferCurrentPos = initialBufferOffset;
            this.bufferFillLength = initialBufferOffset + initialBufferCount;
            this.maxBufferLength = maxBufferLength;

            this.currentChunkLength = noChunkLength;
            this.readState = ReadState.ChunkLength;
        }

        public IAsyncResult ReadAsync(object caller, byte[] userBuffer, int userBufferOffset,
            int userBufferCount, AsyncCallback callback, object state)
        {
            SetReadParameters(userBuffer, userBufferOffset, userBufferCount);

            userAsyncResult = new LazyAsyncResult(caller, state, callback);

            // Store to local var to handle inline completions that would reset 'userAsyncResult'.
            IAsyncResult localResult = userAsyncResult;

            try
            {
                ProcessResponse();
            }
            catch (Exception e)
            {
                CompleteUserRead(e);
            }

            return localResult;
        }

        public int Read(byte[] userBuffer, int userBufferOffset, int userBufferCount)
        {
            SetReadParameters(userBuffer, userBufferOffset, userBufferCount);

            try
            {
                // By not setting userAsyncResult we indicate that this is a synchronous operation.
                ProcessResponse();
            }
            catch (Exception)
            {
                TransitionToErrorState();
                throw;
            }

            return syncResult;
        }

        private void SetReadParameters(byte[] userBuffer, int userBufferOffset, int userBufferCount)
        {
            Contract.Requires(userBuffer != null);
            Contract.Requires((userBufferCount > 0) && (userBufferCount <= userBuffer.Length));
            Contract.Requires((userBufferOffset >= 0) && (userBufferOffset + userBufferCount <= userBuffer.Length));

            if (Interlocked.CompareExchange(ref this.userBuffer, userBuffer, null) != null)
            {
                // Overlapped read operations are not supported.
                throw new InvalidOperationException(SR.GetString(SR.net_inasync));
            }

            Contract.Assert(userAsyncResult == null, "Overlapped read operations are not allowed.");
            Contract.Assert((readState == ReadState.ChunkLength) || (readState == ReadState.PayloadEnd) ||
                ((readState == ReadState.Payload) && (currentChunkBytesRead < currentChunkLength)),
                "Only one outstanding read operation at a time supported.");

            this.userBufferCount = userBufferCount;
            this.userBufferOffset = userBufferOffset;
        }

        public bool TryGetLeftoverBytes(out byte[] buffer, out int leftoverBufferOffset, out int leftoverBufferSize)
        {
            leftoverBufferOffset = 0;
            leftoverBufferSize = 0;
            buffer = null;

            if (readState != ReadState.Done)
            {
                // The ConnectStream was closed before we completed reading the response (e.g. when closing the 
                // stream without consuming it). 
                return false;
            }

            Contract.Assert(userAsyncResult == null, 
                "If we're in the 'done' state we should not have pending operations.");

            if (bufferCurrentPos == bufferFillLength)
            {
                // We consumed the whole buffer. No leftover bytes.
                return false;
            }

            leftoverBufferOffset = bufferCurrentPos;
            leftoverBufferSize = bufferFillLength - bufferCurrentPos;
            buffer = this.buffer;

            return true;
        }

        private void ProcessResponse()
        {
            Contract.Assert(readState < ReadState.Done, "We're already done. No need to process state.");

            DataParseStatus result;

            while (readState < ReadState.Done)
            {
                switch (readState)
                {
                    case ReadState.ChunkLength:
                        result = ParseChunkLength();
                        break;

                    case ReadState.Extension:
                        result = ParseExtension();
                        break;

                    case ReadState.Payload:
                        result = HandlePayload();
                        break;

                    case ReadState.PayloadEnd:
                        result = ParsePayloadEnd();
                        break;

                    case ReadState.Trailer:
                        result = ParseTrailer();
                        break;

                    default:
                        Contract.Assert(false, "Unknown state");
                        throw new InternalException(); ;
                }

                switch (result)
                {
                    case DataParseStatus.ContinueParsing:
                        // Continue with next loop iteration. Parsing was successful and we'll process the next state.
                        break;

                    case DataParseStatus.Done:
                        // Parsing was successful and we should return. We either have a result or we have a pending
                        // operation and will continue once the operation completes.
                        return;

                    case DataParseStatus.Invalid:
                    case DataParseStatus.DataTooBig:
                        CompleteUserRead(new IOException(
                            SR.GetString(SR.net_io_readfailure, SR.GetString(SR.net_io_connectionclosed))));
                        return;

                    case DataParseStatus.NeedMoreData:
                        if (!TryGetMoreData())
                        {
                            // Read operation didn't complete synchronously. Just return. The read completion
                            // callback will continue.
                            return;
                        }
                        // We already got more data, continue the loop.
                        break;

                    default:
                        Contract.Assert(false, "Unknown state");
                        throw new InternalException(); ;
                }
            }
        }

        private void CompleteUserRead(object result)
        {
            bool error = result is Exception;

            // Reset user buffer information.
            this.userBuffer = null;
            this.userBufferCount = 0;
            this.userBufferOffset = 0;

            if (error)
            {
                TransitionToErrorState();
            }

            if (IsAsync)
            {
                LazyAsyncResult localResult = userAsyncResult;
                userAsyncResult = null;

                localResult.InvokeCallback(result);
            }
            else
            {
                if (error)
                {
                    throw result as Exception;
                }

                Contract.Assert(result is int);
                syncResult = (int)result;
            }
        }

        private void TransitionToErrorState()
        {
            readState = ReadState.Error;
        }

        private bool TryGetMoreData()
        {
            PrepareBufferForMoreData();

            int readSize = buffer.Length - bufferFillLength;
            if (readState == ReadState.ChunkLength)
            {
                // When reading the chunk length we want to consume as few bytes from the network as possible (since
                // the length information is just a few bytes). This will avoid copying large amounts of data to the
                // user's buffer.
                readSize = Math.Min(chunkLengthBuffer, readSize);
            }

            int bytesRead = 0;

            if (IsAsync)
            {
                IAsyncResult ar = dataSource.BeginRead(buffer, bufferFillLength, readSize, ReadCallback, null);
                CheckAsyncResult(ar);

                if (!ar.CompletedSynchronously)
                {
                    return false;
                }

                // The read operation already completed. Read the number of bytes read and continue processing.
                bytesRead = dataSource.EndRead(ar);
            }
            else
            {
                bytesRead = dataSource.Read(buffer, bufferFillLength, readSize);
            }

            CompleteMetaDataReadOperation(bytesRead);

            return true;
        }

        private void PrepareBufferForMoreData()
        {
            // If we have data left in the buffer, move it to the beginning of the buffer. If the whole buffer is
            // filled with unconsumed data, increase the buffer to accommodate more data.

            Contract.Assert(bufferCurrentPos <= bufferFillLength);

            int currentPos = bufferCurrentPos;
            bufferCurrentPos = 0;

            if (currentPos == bufferFillLength)
            {
                // We have consumed all the data in the buffer (same scenario as having an empty buffer).
                bufferFillLength = 0;
                return;
            }

            if ((currentPos > 0) || (bufferFillLength < buffer.Length))
            {
                // We have consumed some data from the buffer. However, we need more data to process data left in 
                // the buffer. So move left data to the beginning of the buffer and fill the rest of the buffer.

                if (currentPos > 0)
                {
                    int count = bufferFillLength - currentPos;
                    Buffer.BlockCopy(buffer, currentPos, buffer, 0, count);
                    bufferFillLength = count;
                }

                return;
            }

            // The buffer is entirely filled and we haven't consumed a single byte. However, we need more data 
            // to be able to process it (e.g. if the whole buffer contains just part of a trailer header).

            Contract.Assert(currentPos == 0);
            Contract.Assert(bufferFillLength == buffer.Length);

            if (buffer.Length == maxBufferLength)
            {
                throw new IOException(SR.GetString(SR.net_io_readfailure, SR.GetString(SR.net_io_connectionclosed)));
            }

            int newBufferLength = Math.Min(maxBufferLength, buffer.Length * 2);

            byte[] newBuffer = new byte[newBufferLength];
            Buffer.BlockCopy(buffer, 0, newBuffer, 0, buffer.Length);
            buffer = newBuffer;
        }

        private void CheckAsyncResult(IAsyncResult ar)
        {
            // A null return indicates that the connection was closed underneath us.
            if (ar == null)
            {
                throw new WebException(NetRes.GetWebStatusString("net_requestaborted",
                    WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled);
            }
        }

        private void CompleteMetaDataReadOperation(int bytesRead)
        {
            if (bytesRead == 0)
            {
                // We don't expect a g----ful connection close from the server while we're in the middle of reading 
                // chunk metadata (chunk length, extension, trailer, etc.).
                throw new IOException(SR.GetString(SR.net_io_readfailure, SR.GetString(SR.net_io_connectionclosed)));
            }

            bufferFillLength += bytesRead;
        }

        public void ReadCallback(IAsyncResult ar)
        {
            if (ar.CompletedSynchronously)
            {
                return;
            }

            try
            {
                int bytesRead = dataSource.EndRead(ar);

                if (readState == ReadState.Payload)
                {
                    // We received payload data and stored it in the user buffer. Validate the result and complete
                    // the IAsyncResult we returned to the caller. Next time ReadAsync() gets called we'll
                    // continue where we left off (i.e. read more data from the current chunk or if we already read
                    // the whole chunk, process the terminating CRLF and the next chunk/trailer).
                    CompletePayloadReadOperation(bytesRead);
                    return;
                }

                CompleteMetaDataReadOperation(bytesRead);

                // We received data for our internal buffer. Process received data.
                ProcessResponse();
            }
            catch (Exception e)
            {
                CompleteUserRead(e);
            }
        }

        private DataParseStatus HandlePayload()
        {
            // Try to fill the user buffer with data from the internal buffer first.
            if (bufferCurrentPos < bufferFillLength)
            {
                Contract.Assert(currentOperationBytesRead == 0,
                    "We only read chunk data from the buffer once per read operation.");

                // We have chunk body data in our internal buffer. Copy it to the user buffer.
                int bufferedBytesToRead = Math.Min(Math.Min(userBufferCount, bufferFillLength - bufferCurrentPos),
                    currentChunkLength - currentChunkBytesRead);

                Buffer.BlockCopy(buffer, bufferCurrentPos, userBuffer, userBufferOffset, bufferedBytesToRead);

                bufferCurrentPos += bufferedBytesToRead;

                if ((currentChunkBytesRead + bufferedBytesToRead == currentChunkLength) ||
                    (bufferedBytesToRead == userBufferCount))
                {
                    // We read the whole chunk or filled the user buffer entirely. Complete the operation.
                    CompletePayloadReadOperation(bufferedBytesToRead);
                    return DataParseStatus.Done;
                }

                // Remember how many bytes we copied from our internal buffer to the user buffer: We need to add this
                // value to the amount of data read from the socket below.
                currentOperationBytesRead += bufferedBytesToRead;
                currentChunkBytesRead += bufferedBytesToRead;
            }

            Contract.Assert(bufferCurrentPos == bufferFillLength,
                "We still have data buffered even though the user buffer is not filled yet.");
            Contract.Assert(currentOperationBytesRead < userBufferCount);

            // If we get here we either didn't have any chunk data buffered, or we had less bytes buffered than the
            // user requested. Post a receive on the socket to retrieve more chunk data.
            int bytesToRead = Math.Min(userBufferCount - currentOperationBytesRead,
                currentChunkLength - currentChunkBytesRead);

            if (IsAsync)
            {
                IAsyncResult ar = dataSource.BeginRead(userBuffer, userBufferOffset + currentOperationBytesRead,
                        bytesToRead, ReadCallback, null);
                CheckAsyncResult(ar);

                if (ar.CompletedSynchronously)
                {
                    CompletePayloadReadOperation(dataSource.EndRead(ar));
                }
            }
            else
            {
                int bytesRead = dataSource.Read(userBuffer, userBufferOffset + currentOperationBytesRead, bytesToRead);
                CompletePayloadReadOperation(bytesRead);
            }

            return DataParseStatus.Done;
        }

        private void CompletePayloadReadOperation(int bytesRead)
        {
            Contract.Requires(bytesRead >= 0);
            Contract.Assert(readState == ReadState.Payload,
                "Chunk payload read completion must only be invoked when we're processing payload data.");

            // Getting EOF in the middle of a chunk is a failure.
            if (bytesRead == 0)
            {
                throw new WebException(NetRes.GetWebStatusString("net_requestaborted",
                    WebExceptionStatus.ConnectionClosed), WebExceptionStatus.ConnectionClosed);
            }

            currentChunkBytesRead += bytesRead;
            Contract.Assert(currentChunkBytesRead <= currentChunkLength,
                "Read more bytes than available in the current chunk.");

            int totalBytesRead = currentOperationBytesRead + bytesRead;

            if (currentChunkBytesRead == currentChunkLength)
            {
                // We're done reading this chunk.
                readState = ReadState.PayloadEnd;
            }

            currentOperationBytesRead = 0;
            CompleteUserRead(totalBytesRead);
        }

        private DataParseStatus ParseChunkLength()
        {
            Contract.Assert(currentChunkLength == noChunkLength);

            int chunkLength = noChunkLength;

            for (int i = bufferCurrentPos; i < bufferFillLength; i++)
            {
                byte c = buffer[i];

                if (((c < '0') || (c > '9')) && ((c < 'A') || (c > 'F')) && ((c < 'a') || (c > 'f')))
                {
                    // Not a hex number. Check if we had at least one hex digit. If not, then this is an invalid chunk.
                    if (chunkLength == noChunkLength)
                    {
                        return DataParseStatus.Invalid;
                    }

                    // Point to the first character after the chunk length that is not part of the length value.
                    bufferCurrentPos = i;
                    currentChunkLength = chunkLength;

                    readState = ReadState.Extension;
                    return DataParseStatus.ContinueParsing;
                }

                byte currentDigit = (byte)((c < (byte)'A') ? (c - (byte)'0') :
                    10 + ((c < (byte)'a') ? (c - (byte)'A') : (c - (byte)'a')));

                if (chunkLength == noChunkLength)
                {
                    chunkLength = currentDigit;
                }
                else
                {
                    if (chunkLength >= 0x8000000)
                    {
                        // Shifting the value by an order of magnitude (hex) would result in a value > Int32.MaxValue.
                        // Currently only chunks up to 2GB are supported. The provided chunk length is too large.
                        return DataParseStatus.Invalid;
                    }

                    // Multiply current chunk length by 16 and add the current digit.
                    chunkLength = (chunkLength << 4) + currentDigit;
                }
            }

            // The current buffer didn't include the whole chunk length information followed by a non-hex digit char.
            return DataParseStatus.NeedMoreData;
        }

        private DataParseStatus ParseExtension()
        {
            int currentPos = bufferCurrentPos;

            // After the chunk length we can only have <space> or <tab> chars. A LWS with CRLF would be ambiguous since
            // CRLF also delimits the chunk length from chunk data.
            DataParseStatus result = ParseWhitespaces(ref currentPos);
            if (result != DataParseStatus.ContinueParsing)
            {
                return result;
            }

            result = ParseExtensionNameValuePairs(ref currentPos);
            if (result != DataParseStatus.ContinueParsing)
            {
                return result;
            }

            result = ParseCRLF(ref currentPos);
            if (result != DataParseStatus.ContinueParsing)
            {
                return result;
            }

            bufferCurrentPos = currentPos;

            if (currentChunkLength == 0)
            {
                // zero-chunk read. We're done with the response. Consume trailer and complete.
                readState = ReadState.Trailer;
            }
            else
            {
                readState = ReadState.Payload;
            }

            return DataParseStatus.ContinueParsing;
        }

        private DataParseStatus ParsePayloadEnd()
        {
            Contract.Assert(currentChunkBytesRead == currentChunkLength);

            DataParseStatus crlfResult = ParseCRLF(ref bufferCurrentPos);

            if (crlfResult != DataParseStatus.ContinueParsing)
            {
                return crlfResult;
            }

            currentChunkLength = noChunkLength;
            currentChunkBytesRead = 0;

            readState = ReadState.ChunkLength;

            return DataParseStatus.ContinueParsing;
        }

        private DataParseStatus ParseTrailer()
        {
            if (ParseWhitespaces(ref bufferCurrentPos) == DataParseStatus.NeedMoreData)
            {
                return DataParseStatus.NeedMoreData;
            }

            int currentPos = bufferCurrentPos;

            // Leverage WebHeaderCollection to parse the trailer.
            DataParseStatus result;
            WebParseError error;
            error.Section = WebParseErrorSection.Generic;
            error.Code = WebParseErrorCode.Generic;
            WebHeaderCollection trailer = new WebHeaderCollection();
            if (SettingsSectionInternal.Section.UseUnsafeHeaderParsing)
            {
                result = trailer.ParseHeaders(buffer, bufferFillLength, ref currentPos, ref totalTrailerHeadersLength,
                    maxBufferLength, ref error);
            }
            else
            {
                result = trailer.ParseHeadersStrict(buffer, bufferFillLength, ref currentPos, 
                    ref totalTrailerHeadersLength, maxBufferLength, ref error);
            }

            Contract.Assert(result != DataParseStatus.ContinueParsing,
                "ContinueParsing should never be returned by WebHeaderCollection.ParseHeaders*().");

            if ((result == DataParseStatus.NeedMoreData) || (result == DataParseStatus.Done))
            {
                bufferCurrentPos = currentPos;
            }

            if (result != DataParseStatus.Done)
            {
                return result;
            }

            readState = ReadState.Done;

            // We're done reading the whole response. Invoke the user callback with 0 bytes to indicate "end of stream".
            CompleteUserRead(0);

            return DataParseStatus.Done;
        }

        private DataParseStatus ParseCRLF(ref int pos)
        {
            Contract.Ensures((Contract.Result<DataParseStatus>() != DataParseStatus.Done) ||
                (Contract.Result<DataParseStatus>() != DataParseStatus.DataTooBig));
            
            const int crlfLength = 2;

            if (pos + crlfLength > bufferFillLength)
            {
                return DataParseStatus.NeedMoreData;
            }

            if ((buffer[pos] != '\r') || (buffer[pos + 1] != '\n'))
            {
                return DataParseStatus.Invalid;
            }

            pos += crlfLength;
            return DataParseStatus.ContinueParsing;
        }

        private DataParseStatus ParseWhitespaces(ref int pos)
        {
            Contract.Ensures((Contract.Result<DataParseStatus>() == DataParseStatus.ContinueParsing) ||
                (Contract.Result<DataParseStatus>() == DataParseStatus.NeedMoreData) ||
                (Contract.Result<DataParseStatus>() != DataParseStatus.DataTooBig));

            int currentPos = pos;
            
            while (currentPos < bufferFillLength)
            {
                byte c = buffer[currentPos];

                if (!IsWhiteSpace(c))
                {
                    // Point to the first character that is not a SP (space) or HT (horizontal tab).
                    pos = currentPos;
                    return DataParseStatus.ContinueParsing;
                }
                
                currentPos++;
            }

            // We only had whitespaces until the end of the buffer. Request more data to continue.
            return DataParseStatus.NeedMoreData;
        }

        private static bool IsWhiteSpace(byte c)
        {
            return (c == ' ') || (c == '\t');
        }

        private DataParseStatus ParseExtensionNameValuePairs(ref int pos)
        {
            Contract.Ensures((Contract.Result<DataParseStatus>() != DataParseStatus.Done) ||
                (Contract.Result<DataParseStatus>() != DataParseStatus.DataTooBig));

            // chunk-extension= *( ";" chunk-ext-name [ "=" chunk-ext-val ] )
            // chunk-ext-name = token
            // chunk-ext-val  = token | quoted-string

            DataParseStatus result;
            int currentPos = pos;

            while (buffer[currentPos] == ';')
            {
                currentPos++;

                result = ParseWhitespaces(ref currentPos);
                if (result != DataParseStatus.ContinueParsing)
                {
                    return result;
                }

                result = ParseToken(ref currentPos);
                if (result != DataParseStatus.ContinueParsing)
                {
                    return result;
                }

                result = ParseWhitespaces(ref currentPos);
                if (result != DataParseStatus.ContinueParsing)
                {
                    return result;
                }

                Contract.Assert(currentPos < bufferFillLength, 
                    "After skipping white spaces we should have at least one character.");
            
                if (buffer[currentPos] == '=')
                {
                    currentPos++;

                    result = ParseWhitespaces(ref currentPos);
                    if (result != DataParseStatus.ContinueParsing)
                    {
                        return result;
                    }

                    result = ParseToken(ref currentPos);
                    if (result == DataParseStatus.Invalid)
                    {
                        result = ParseQuotedString(ref currentPos);
                    }

                    if (result != DataParseStatus.ContinueParsing)
                    {
                        return result;
                    }

                    result = ParseWhitespaces(ref currentPos);
                    if (result != DataParseStatus.ContinueParsing)
                    {
                        return result;
                    }
                }
            }

            pos = currentPos;

            return DataParseStatus.ContinueParsing;
        }

        private DataParseStatus ParseQuotedString(ref int pos)
        {
            Contract.Ensures((Contract.Result<DataParseStatus>() != DataParseStatus.Done) ||
                (Contract.Result<DataParseStatus>() != DataParseStatus.DataTooBig));

            if (pos == bufferFillLength)
            {
                return DataParseStatus.NeedMoreData;
            }

            if (buffer[pos] != '"')
            {
                return DataParseStatus.Invalid;
            }

            int currentPos = pos + 1;

            while (currentPos < bufferFillLength)
            {
                if ((buffer[currentPos] == '"'))
                {
                    pos = currentPos + 1; // return index pointing to char after closing quote char.
                    return DataParseStatus.ContinueParsing;
                }

                // Note that for extensions we can't support backslash before the terminating quote: E.g. if we see
                // \"\r\n we don't know if we have an escaped " followed by a LWS or if we're at the end of the quoted
                // string followed by the extension-terminating CRLF. I.e. as soon as we see \" we interpret it as
                // quoted pair.
                if (buffer[currentPos] == '\\')
                { 
                    // We have a quoted pair. Make sure we have at least one more char in the buffer.
                    currentPos++;
                    if (currentPos == bufferFillLength)
                    {
                        return DataParseStatus.NeedMoreData;
                    }

                    // Only 0-127 values are allowed in a quoted pair. If the char after \ is > 127 then \ is not part
                    // of a quoted pair but a regular char in the quoted string.
                    if (buffer[currentPos] <= 0x7F)
                    {
                        currentPos++; // skip quoted pair
                        continue;
                    }
                }

                currentPos++;
            }

            return DataParseStatus.NeedMoreData;
        }

        private DataParseStatus ParseToken(ref int pos)
        {
            Contract.Ensures((Contract.Result<DataParseStatus>() != DataParseStatus.Done) ||
                (Contract.Result<DataParseStatus>() != DataParseStatus.DataTooBig));

            for (int currentPos = pos; currentPos < bufferFillLength; currentPos++)
            {
                if (!IsTokenChar(buffer[currentPos]))
                {
                    // If we found at least one token character, we have a token. If not, indicate failure since
                    // we were supposed to parse a token but we didn't find one.
                    if (currentPos > pos)
                    {
                        pos = currentPos;
                        return DataParseStatus.ContinueParsing;
                    }
                    else
                    {
                        return DataParseStatus.Invalid;
                    }
                }
            }

            return DataParseStatus.NeedMoreData;
        }
        
        private static bool IsTokenChar(byte character)
        {
            // Must be between 'space' (32) and 'DEL' (127)
            if (character > 127)
            {
                return false;
            }

            return tokenChars[character];
        }    
    }
}
