//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.Xml
{
    using System.IO;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Text;
    using System.Threading;

    abstract class XmlStreamNodeWriter : XmlNodeWriter
    {
        Stream stream;
        byte[] buffer;
        int offset;
        bool ownsStream;
        const int bufferLength = 512;
        const int maxEntityLength = 32;
        const int maxBytesPerChar = 3;
        Encoding encoding;
        int hasPendingWrite;
        AsyncEventArgs<object> flushBufferState;
        static UTF8Encoding UTF8Encoding = new UTF8Encoding(false, true);
        static AsyncCallback onFlushBufferComplete;
        static AsyncEventArgsCallback onGetFlushComplete;

        protected XmlStreamNodeWriter()
        {
            this.buffer = new byte[bufferLength];
            encoding = XmlStreamNodeWriter.UTF8Encoding;
        }

        protected void SetOutput(Stream stream, bool ownsStream, Encoding encoding)
        {
            this.stream = stream;
            this.ownsStream = ownsStream;
            this.offset = 0;

            if (encoding != null)
            {
                this.encoding = encoding;
            }
        }

        // Getting/Setting the Stream exists for fragmenting
        public Stream Stream
        {
            get
            {
                return stream;
            }
            set
            {
                stream = value;
            }
        }

        // StreamBuffer/BufferOffset exists only for the BinaryWriter to fix up nodes
        public byte[] StreamBuffer
        {
            get
            {
                return buffer;
            }
        }
        public int BufferOffset
        {
            get
            {
                return offset;
            }
        }

        public int Position
        {
            get
            {
                return (int)stream.Position + offset;
            }
        }

        protected byte[] GetBuffer(int count, out int offset)
        {
            Fx.Assert(count >= 0 && count <= bufferLength, "");
            int bufferOffset = this.offset;
            if (bufferOffset + count <= bufferLength)
            {
                offset = bufferOffset;
            }
            else
            {
                FlushBuffer();
                offset = 0;
            }
#if DEBUG
            Fx.Assert(offset + count <= bufferLength, "");
            for (int i = 0; i < count; i++)
            {
                buffer[offset + i] = (byte)'<';
            }
#endif
            return buffer;
        }

        internal AsyncCompletionResult GetBufferAsync(GetBufferAsyncEventArgs getBufferState)
        {
            Fx.Assert(getBufferState != null, "GetBufferAsyncEventArgs cannot be null.");
            int count = getBufferState.Arguments.Count;
            Fx.Assert(count >= 0 && count <= bufferLength, String.Empty);
            int finalOffset = 0;

            int bufferOffset = this.offset;
            if (bufferOffset + count <= bufferLength)
            {
                finalOffset = bufferOffset;
            }
            else
            {
                if (onGetFlushComplete == null)
                {
                    onGetFlushComplete = new AsyncEventArgsCallback(GetBufferFlushComplete);
                }
                if (flushBufferState == null)
                {
                    this.flushBufferState = new AsyncEventArgs<object>();
                }

                this.flushBufferState.Set(onGetFlushComplete, getBufferState, this);
                if (FlushBufferAsync(this.flushBufferState) == AsyncCompletionResult.Completed)
                {
                    finalOffset = 0;
                    this.flushBufferState.Complete(true);
                }
                else
                {
                    return AsyncCompletionResult.Queued;
                }
            }
#if DEBUG
            Fx.Assert(finalOffset + count <= bufferLength, "");
            for (int i = 0; i < count; i++)
            {
                buffer[finalOffset + i] = (byte)'<';
            }
#endif
            //return the buffer and finalOffset;
            getBufferState.Result = getBufferState.Result ?? new GetBufferEventResult();
            getBufferState.Result.Buffer = this.buffer;
            getBufferState.Result.Offset = finalOffset;
            return AsyncCompletionResult.Completed;
        }

        static void GetBufferFlushComplete(IAsyncEventArgs completionState)
        {
            XmlStreamNodeWriter thisPtr = (XmlStreamNodeWriter)completionState.AsyncState;
            GetBufferAsyncEventArgs getBufferState = (GetBufferAsyncEventArgs)thisPtr.flushBufferState.Arguments;
            getBufferState.Result = getBufferState.Result ?? new GetBufferEventResult();
            getBufferState.Result.Buffer = thisPtr.buffer;
            getBufferState.Result.Offset = 0;
            getBufferState.Complete(false, completionState.Exception);
        }

        AsyncCompletionResult FlushBufferAsync(AsyncEventArgs<object> state)
        {
            if (Interlocked.CompareExchange(ref this.hasPendingWrite, 1, 0) != 0)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.GetString(SR.FlushBufferAlreadyInUse)));
            }

            if (this.offset != 0)
            {
                if (onFlushBufferComplete == null)
                {
                    onFlushBufferComplete = new AsyncCallback(OnFlushBufferCompete);
                }

                IAsyncResult result = stream.BeginWrite(buffer, 0, this.offset, onFlushBufferComplete, this);
                if (!result.CompletedSynchronously)
                {
                    return AsyncCompletionResult.Queued;
                }

                stream.EndWrite(result);
                this.offset = 0;
            }

            if (Interlocked.CompareExchange(ref this.hasPendingWrite, 0, 1) != 1)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.GetString(SR.NoAsyncWritePending)));
            }

            return AsyncCompletionResult.Completed;
        }

        static void OnFlushBufferCompete(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            XmlStreamNodeWriter thisPtr = (XmlStreamNodeWriter)result.AsyncState;
            Exception completionException = null;
            try
            {
                thisPtr.stream.EndWrite(result);
                thisPtr.offset = 0;
                if (Interlocked.CompareExchange(ref thisPtr.hasPendingWrite, 0, 1) != 1)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.GetString(SR.NoAsyncWritePending)));
                }
            }
            catch (Exception ex)
            {
                if (Fx.IsFatal(ex))
                {
                    throw;
                }

                completionException = ex;
            }

            thisPtr.flushBufferState.Complete(false, completionException);
        }

        protected IAsyncResult BeginGetBuffer(int count, AsyncCallback callback, object state)
        {
            Fx.Assert(count >= 0 && count <= bufferLength, "");
            return new GetBufferAsyncResult(count, this, callback, state);
        }

        protected byte[] EndGetBuffer(IAsyncResult result, out int offset)
        {
            return GetBufferAsyncResult.End(result, out offset);
        }

        class GetBufferAsyncResult : AsyncResult
        {
            XmlStreamNodeWriter writer;
            int offset;
            int count;
            static AsyncCompletion onComplete = new AsyncCompletion(OnComplete);

            public GetBufferAsyncResult(int count, XmlStreamNodeWriter writer, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.count = count;
                this.writer = writer;
                int bufferOffset = writer.offset;

                bool completeSelf = false;

                if (bufferOffset + count <= bufferLength)
                {
                    this.offset = bufferOffset;
                    completeSelf = true;
                }
                else
                {
                    IAsyncResult result = writer.BeginFlushBuffer(PrepareAsyncCompletion(onComplete), this);
                    completeSelf = SyncContinue(result);
                }

                if (completeSelf)
                {
                    this.Complete(true);
                }
            }

            static bool OnComplete(IAsyncResult result)
            {
                GetBufferAsyncResult thisPtr = (GetBufferAsyncResult)result.AsyncState;
                return thisPtr.HandleFlushBuffer(result);
            }

            bool HandleFlushBuffer(IAsyncResult result)
            {
                writer.EndFlushBuffer(result);
                this.offset = 0;

#if DEBUG
                Fx.Assert(this.offset + this.count <= bufferLength, "");
                for (int i = 0; i < this.count; i++)
                {
                    writer.buffer[this.offset + i] = (byte)'<';
                }
#endif
                return true;
            }

            public static byte[] End(IAsyncResult result, out int offset)
            {
                GetBufferAsyncResult thisPtr = AsyncResult.End<GetBufferAsyncResult>(result);

                offset = thisPtr.offset;
                return thisPtr.writer.buffer;
            }
        }

        protected void Advance(int count)
        {
            Fx.Assert(offset + count <= bufferLength, "");
            offset += count;
        }

        void EnsureByte()
        {
            if (offset >= bufferLength)
            {
                FlushBuffer();
            }
        }

        protected void WriteByte(byte b)
        {
            EnsureByte();
            buffer[offset++] = b;
        }

        protected void WriteByte(char ch)
        {
            Fx.Assert(ch < 0x80, "");
            WriteByte((byte)ch);
        }

        protected void WriteBytes(byte b1, byte b2)
        {
            byte[] buffer = this.buffer;
            int offset = this.offset;
            if (offset + 1 >= bufferLength)
            {
                FlushBuffer();
                offset = 0;
            }
            buffer[offset + 0] = b1;
            buffer[offset + 1] = b2;
            this.offset += 2;
        }

        protected void WriteBytes(char ch1, char ch2)
        {
            Fx.Assert(ch1 < 0x80 && ch2 < 0x80, "");
            WriteBytes((byte)ch1, (byte)ch2);
        }

        public void WriteBytes(byte[] byteBuffer, int byteOffset, int byteCount)
        {
            if (byteCount < bufferLength)
            {
                int offset;
                byte[] buffer = GetBuffer(byteCount, out offset);
                Buffer.BlockCopy(byteBuffer, byteOffset, buffer, offset, byteCount);
                Advance(byteCount);
            }
            else
            {
                FlushBuffer();
                stream.Write(byteBuffer, byteOffset, byteCount);
            }
        }

        public IAsyncResult BeginWriteBytes(byte[] byteBuffer, int byteOffset, int byteCount, AsyncCallback callback, object state)
        {
            return new WriteBytesAsyncResult(byteBuffer, byteOffset, byteCount, this, callback, state);
        }

        public void EndWriteBytes(IAsyncResult result)
        {
            WriteBytesAsyncResult.End(result);
        }

        class WriteBytesAsyncResult : AsyncResult
        {
            static AsyncCompletion onHandleGetBufferComplete = new AsyncCompletion(OnHandleGetBufferComplete);
            static AsyncCompletion onHandleFlushBufferComplete = new AsyncCompletion(OnHandleFlushBufferComplete);
            static AsyncCompletion onHandleWrite = new AsyncCompletion(OnHandleWrite);

            byte[] byteBuffer;
            int byteOffset;
            int byteCount;
            XmlStreamNodeWriter writer;

            public WriteBytesAsyncResult(byte[] byteBuffer, int byteOffset, int byteCount, XmlStreamNodeWriter writer, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.byteBuffer = byteBuffer;
                this.byteOffset = byteOffset;
                this.byteCount = byteCount;
                this.writer = writer;

                bool completeSelf = false;

                if (byteCount < bufferLength)
                {
                    completeSelf = HandleGetBuffer(null);
                }
                else
                {
                    completeSelf = HandleFlushBuffer(null);
                }

                if (completeSelf)
                {
                    this.Complete(true);
                }
            }

            static bool OnHandleGetBufferComplete(IAsyncResult result)
            {
                WriteBytesAsyncResult thisPtr = (WriteBytesAsyncResult)result.AsyncState;
                return thisPtr.HandleGetBuffer(result);
            }

            static bool OnHandleFlushBufferComplete(IAsyncResult result)
            {
                WriteBytesAsyncResult thisPtr = (WriteBytesAsyncResult)result.AsyncState;
                return thisPtr.HandleFlushBuffer(result);
            }

            static bool OnHandleWrite(IAsyncResult result)
            {
                WriteBytesAsyncResult thisPtr = (WriteBytesAsyncResult)result.AsyncState;
                return thisPtr.HandleWrite(result);
            }

            bool HandleGetBuffer(IAsyncResult result)
            {
                if (result == null)
                {
                    result = writer.BeginGetBuffer(this.byteCount, PrepareAsyncCompletion(onHandleGetBufferComplete), this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                }

                int offset;
                byte[] buffer = writer.EndGetBuffer(result, out offset);

                Buffer.BlockCopy(this.byteBuffer, this.byteOffset, buffer, offset, this.byteCount);
                writer.Advance(this.byteCount);

                return true;
            }

            bool HandleFlushBuffer(IAsyncResult result)
            {
                if (result == null)
                {
                    result = writer.BeginFlushBuffer(PrepareAsyncCompletion(onHandleFlushBufferComplete), this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                }

                writer.EndFlushBuffer(result);
                return HandleWrite(null);
            }

            bool HandleWrite(IAsyncResult result)
            {
                if (result == null)
                {
                    result = writer.stream.BeginWrite(this.byteBuffer, this.byteOffset, this.byteCount, PrepareAsyncCompletion(onHandleWrite), this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                }

                writer.stream.EndWrite(result);
                return true;
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<WriteBytesAsyncResult>(result);
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Contains unsafe code. Caller needs to validate arguments.")]
        [SecurityCritical]
        unsafe protected void UnsafeWriteBytes(byte* bytes, int byteCount)
        {
            FlushBuffer();
            byte[] buffer = this.buffer;
            while (byteCount > bufferLength)
            {
                for (int i = 0; i < bufferLength; i++)
                    buffer[i] = bytes[i];
                stream.Write(buffer, 0, bufferLength);
                bytes += bufferLength;
                byteCount -= bufferLength;
            }

            if (byteCount > 0)
            {
                for (int i = 0; i < byteCount; i++)
                    buffer[i] = bytes[i];
                stream.Write(buffer, 0, byteCount);
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Contains unsafe code.",
            Safe = "Unsafe code is effectively encapsulated, all inputs are validated.")]
        [SecuritySafeCritical]
        unsafe protected void WriteUTF8Char(int ch)
        {
            if (ch < 0x80)
            {
                WriteByte((byte)ch);
            }
            else if (ch <= char.MaxValue)
            {
                char* chars = stackalloc char[1];
                chars[0] = (char)ch;
                UnsafeWriteUTF8Chars(chars, 1);
            }
            else
            {
                SurrogateChar surrogateChar = new SurrogateChar(ch);
                char* chars = stackalloc char[2];
                chars[0] = surrogateChar.HighChar;
                chars[1] = surrogateChar.LowChar;
                UnsafeWriteUTF8Chars(chars, 2);
            }
        }

        protected void WriteUTF8Chars(byte[] chars, int charOffset, int charCount)
        {
            if (charCount < bufferLength)
            {
                int offset;
                byte[] buffer = GetBuffer(charCount, out offset);
                Buffer.BlockCopy(chars, charOffset, buffer, offset, charCount);
                Advance(charCount);
            }
            else
            {
                FlushBuffer();
                stream.Write(chars, charOffset, charCount);
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Contains unsafe code.",
            Safe = "Unsafe code is effectively encapsulated, all inputs are validated.")]
        [SecuritySafeCritical]
        unsafe protected void WriteUTF8Chars(string value)
        {
            int count = value.Length;
            if (count > 0)
            {
                fixed (char* chars = value)
                {
                    UnsafeWriteUTF8Chars(chars, count);
                }
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Contains unsafe code. Caller needs to validate arguments.")]
        [SecurityCritical]
        unsafe protected void UnsafeWriteUTF8Chars(char* chars, int charCount)
        {
            const int charChunkSize = bufferLength / maxBytesPerChar;
            while (charCount > charChunkSize)
            {
                int offset;
                int chunkSize = charChunkSize;
                if ((int)(chars[chunkSize - 1] & 0xFC00) == 0xD800) // This is a high surrogate
                    chunkSize--;
                byte[] buffer = GetBuffer(chunkSize * maxBytesPerChar, out offset);
                Advance(UnsafeGetUTF8Chars(chars, chunkSize, buffer, offset));
                charCount -= chunkSize;
                chars += chunkSize;
            }
            if (charCount > 0)
            {
                int offset;
                byte[] buffer = GetBuffer(charCount * maxBytesPerChar, out offset);
                Advance(UnsafeGetUTF8Chars(chars, charCount, buffer, offset));
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Contains unsafe code. Caller needs to validate arguments.")]
        [SecurityCritical]
        unsafe protected void UnsafeWriteUnicodeChars(char* chars, int charCount)
        {
            const int charChunkSize = bufferLength / 2;
            while (charCount > charChunkSize)
            {
                int offset;
                int chunkSize = charChunkSize;
                if ((int)(chars[chunkSize - 1] & 0xFC00) == 0xD800) // This is a high surrogate
                    chunkSize--;
                byte[] buffer = GetBuffer(chunkSize * 2, out offset);
                Advance(UnsafeGetUnicodeChars(chars, chunkSize, buffer, offset));
                charCount -= chunkSize;
                chars += chunkSize;
            }
            if (charCount > 0)
            {
                int offset;
                byte[] buffer = GetBuffer(charCount * 2, out offset);
                Advance(UnsafeGetUnicodeChars(chars, charCount, buffer, offset));
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Contains unsafe code. Caller needs to validate arguments.")]
        [SecurityCritical]
        unsafe protected int UnsafeGetUnicodeChars(char* chars, int charCount, byte[] buffer, int offset)
        {
            char* charsMax = chars + charCount;
            while (chars < charsMax)
            {
                char value = *chars++;
                buffer[offset++] = (byte)value;
                value >>= 8;
                buffer[offset++] = (byte)value;
            }
            return charCount * 2;
        }

        [Fx.Tag.SecurityNote(Critical = "Contains unsafe code. Caller needs to validate arguments.")]
        [SecurityCritical]
        unsafe protected int UnsafeGetUTF8Length(char* chars, int charCount)
        {
            char* charsMax = chars + charCount;
            while (chars < charsMax)
            {
                if (*chars >= 0x80)
                    break;

                chars++;
            }

            if (chars == charsMax)
                return charCount;

            return (int)(chars - (charsMax - charCount)) + encoding.GetByteCount(chars, (int)(charsMax - chars));
        }

        [Fx.Tag.SecurityNote(Critical = "Contains unsafe code. Caller needs to validate arguments.")]
        [SecurityCritical]
        unsafe protected int UnsafeGetUTF8Chars(char* chars, int charCount, byte[] buffer, int offset)
        {
            if (charCount > 0)
            {
                fixed (byte* _bytes = &buffer[offset])
                {
                    byte* bytes = _bytes;
                    byte* bytesMax = &bytes[buffer.Length - offset];
                    char* charsMax = &chars[charCount];

                    while (true)
                    {
                        while (chars < charsMax && *chars < 0x80)
                        {
                            *bytes = (byte)(*chars);
                            bytes++;
                            chars++;
                        }

                        if (chars >= charsMax)
                            break;

                        char* charsStart = chars;
                        while (chars < charsMax && *chars >= 0x80)
                        {
                            chars++;
                        }

                        bytes += encoding.GetBytes(charsStart, (int)(chars - charsStart), bytes, (int)(bytesMax - bytes));

                        if (chars >= charsMax)
                            break;
                    }

                    return (int)(bytes - _bytes);
                }
            }
            return 0;
        }

        protected virtual void FlushBuffer()
        {
            if (offset != 0)
            {
                stream.Write(buffer, 0, offset);
                offset = 0;
            }
        }

        protected virtual IAsyncResult BeginFlushBuffer(AsyncCallback callback, object state)
        {
            return new FlushBufferAsyncResult(this, callback, state);
        }

        protected virtual void EndFlushBuffer(IAsyncResult result)
        {
            FlushBufferAsyncResult.End(result);
        }

        class FlushBufferAsyncResult : AsyncResult
        {
            static AsyncCompletion onComplete = new AsyncCompletion(OnComplete);
            XmlStreamNodeWriter writer;

            public FlushBufferAsyncResult(XmlStreamNodeWriter writer, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.writer = writer;
                bool completeSelf = true;

                if (writer.offset != 0)
                {
                    completeSelf = HandleFlushBuffer(null);
                }

                if (completeSelf)
                {
                    this.Complete(true);
                }
            }

            static bool OnComplete(IAsyncResult result)
            {
                FlushBufferAsyncResult thisPtr = (FlushBufferAsyncResult)result.AsyncState;
                return thisPtr.HandleFlushBuffer(result);
            }

            bool HandleFlushBuffer(IAsyncResult result)
            {
                if (result == null)
                {
                    result = this.writer.stream.BeginWrite(writer.buffer, 0, writer.offset, PrepareAsyncCompletion(onComplete), this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                }

                this.writer.stream.EndWrite(result);
                this.writer.offset = 0;

                return true;
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<FlushBufferAsyncResult>(result);
            }
        }

        public override void Flush()
        {
            FlushBuffer();
            stream.Flush();
        }

        public override void Close()
        {
            if (stream != null)
            {
                if (ownsStream)
                {
                    stream.Close();
                }
                stream = null;
            }
        }

        internal class GetBufferArgs
        {
            public int Count { get; set; }
        }

        internal class GetBufferEventResult
        {
            internal byte[] Buffer { get; set; }
            internal int Offset { get; set; }
        }

        internal class GetBufferAsyncEventArgs : AsyncEventArgs<GetBufferArgs, GetBufferEventResult>
        {
        }
    }
}
