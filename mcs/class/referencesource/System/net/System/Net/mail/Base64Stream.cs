//-----------------------------------------------------------------------------
// <copyright file="Base64Stream.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------

namespace System.Net
{
    using System;
    using System.IO;
    using System.Net.Mime;
    using System.Text;
    using System.Diagnostics;

    internal class Base64Stream : DelegatedStream, IEncodableStream
    {
        static byte[] base64DecodeMap = new byte[] {
            //0   1   2   3   4   5   6   7   8   9   A   B   C   D   E   F
            255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255, // 0
            255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255, // 1
            255,255,255,255,255,255,255,255,255,255,255, 62,255,255,255, 63, // 2
             52, 53, 54, 55, 56, 57, 58, 59, 60, 61,255,255,255,255,255,255, // 3
            255,  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14, // 4
             15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25,255,255,255,255,255, // 5
            255, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, // 6
             41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51,255,255,255,255,255, // 7
            255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255, // 8
            255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255, // 9
            255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255, // A
            255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255, // B
            255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255, // C
            255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255, // D
            255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255, // E
            255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255, // F
        };

        static byte[] base64EncodeMap = new byte[] {
             65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80,
             81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 97, 98, 99,100,101,102,
            103,104,105,106,107,108,109,110,111,112,113,114,115,116,117,118,
            119,120,121,122, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 43, 47,
            61
            };

        int lineLength;
        ReadStateInfo readState;        
        Base64WriteStateInfo writeState;

        //the number of bytes needed to encode three bytes (see algorithm description in Encode method below)
        const int sizeOfBase64EncodedChar = 4;

        //bytes with this value in the decode map are invalid
        const byte invalidBase64Value = 255;

        internal Base64Stream(Stream stream, Base64WriteStateInfo writeStateInfo)
            : base(stream)
        {
            this.writeState = new Base64WriteStateInfo();
            this.lineLength = writeStateInfo.MaxLineLength;
        }

        internal Base64Stream(Stream stream, int lineLength)
            : base(stream)
        {
            this.lineLength = lineLength;
            this.writeState = new Base64WriteStateInfo();            
        }

        internal Base64Stream(Base64WriteStateInfo writeStateInfo)
        {
            this.lineLength = writeStateInfo.MaxLineLength;
            this.writeState = writeStateInfo;
        }

        public override bool CanWrite
        {
            get
            {
                return base.CanWrite;
            }
        }

        ReadStateInfo ReadState
        {
            get
            {
                if (this.readState == null)
                    this.readState = new ReadStateInfo();
                return this.readState;
            }
        }

        internal Base64WriteStateInfo WriteState
        {
            get
            {
                Debug.Assert(writeState != null, "writeState was null");
                return this.writeState;
            }
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (offset < 0 || offset > buffer.Length)
                throw new ArgumentOutOfRangeException("offset");

            if (offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException("count");

            ReadAsyncResult result = new ReadAsyncResult(this, buffer, offset, count, callback, state);
            result.Read();
            return result;
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (offset < 0 || offset > buffer.Length)
                throw new ArgumentOutOfRangeException("offset");

            if (offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException("count");

            WriteAsyncResult result = new WriteAsyncResult(this, buffer, offset, count, callback, state);
            result.Write();
            return result;
        }


        public override void Close()
        {
            if (this.writeState != null && WriteState.Length > 0)
            {
                switch (WriteState.Padding)
                {
                    case 2:
                        WriteState.Append(base64EncodeMap[WriteState.LastBits], base64EncodeMap[64], 
                            base64EncodeMap[64]);
                        break;
                    case 1:
                        WriteState.Append(base64EncodeMap[WriteState.LastBits], base64EncodeMap[64]);
                        break;
                }
                WriteState.Padding = 0;
                FlushInternal();
            }
            base.Close();
        }

        public int DecodeBytes(byte[] buffer, int offset, int count)
        {
            unsafe
            {
                fixed (byte* pBuffer = buffer)
                {
                    byte* start = pBuffer + offset;
                    byte* source = start;
                    byte* dest = start;
                    byte* end = start + count;

                    while (source < end)
                    {
                        //space and tab are ok because folding must include a whitespace char.
                        if (*source == '\r' || *source == '\n' || *source == '=' || *source == ' ' || *source == '\t')
                        {
                            source++;
                            continue;
                        }

                        byte s = base64DecodeMap[*source];

                        if (s == invalidBase64Value)
                            throw new FormatException(SR.GetString(SR.MailBase64InvalidCharacter));

                        switch (ReadState.Pos)
                        {
                            case 0:
                                ReadState.Val = (byte)(s << 2);
                                ReadState.Pos++;
                                break;
                            case 1:
                                *dest++ = (byte)(ReadState.Val + (s >> 4));
                                ReadState.Val = (byte)(s << 4);
                                ReadState.Pos++;
                                break;
                            case 2:
                                *dest++ = (byte)(ReadState.Val + (s >> 2));
                                ReadState.Val = (byte)(s << 6);
                                ReadState.Pos++;
                                break;
                            case 3:
                                *dest++ = (byte)(ReadState.Val + s);
                                ReadState.Pos = 0;
                                break;
                        }
                        source++;
                    }

                    count = (int)(dest - start);
                }
            }
            return count;
        }

        public int EncodeBytes(byte[] buffer, int offset, int count)
        {
            return this.EncodeBytes(buffer, offset, count, true, true);
        }

        internal int EncodeBytes(byte[] buffer, int offset, int count, 
            bool dontDeferFinalBytes, bool shouldAppendSpaceToCRLF)
        {           
            int cur = offset;
            Debug.Assert(buffer != null, "buffer was null");
            Debug.Assert(this.writeState != null, "writestate was null");
            Debug.Assert(this.writeState.Buffer != null, "writestate.buffer was null");

            // Add Encoding header, if any. e.g. =?encoding?b?
            WriteState.AppendHeader();

            switch (WriteState.Padding)
            {
                case 2:
                    WriteState.Append(base64EncodeMap[WriteState.LastBits | ((buffer[cur]&0xf0)>>4)]);
                    if (count == 1)
                    {
                        WriteState.LastBits = (byte)((buffer[cur]&0x0f)<<2);
                        WriteState.Padding = 1;
                        return cur - offset;
                    }
                    WriteState.Append(base64EncodeMap[((buffer[cur]&0x0f)<<2) | ((buffer[cur+1]&0xc0)>>6)]);
                    WriteState.Append(base64EncodeMap[(buffer[cur+1]&0x3f)]);
                    cur+=2;
                    count-=2;
                    WriteState.Padding = 0;
                    break;
                case 1:
                    WriteState.Append(base64EncodeMap[WriteState.LastBits | ((buffer[cur]&0xc0)>>6)]);
                    WriteState.Append(base64EncodeMap[(buffer[cur]&0x3f)]);
                    cur++;
                    count--;
                    WriteState.Padding = 0;
                    break;
            }

            int calcLength = cur + (count - (count%3));

            // Convert three bytes at a time to base64 notation.  This will output 4 chars.
            for (; cur < calcLength; cur+=3)
            {
                if ((lineLength != -1)
                    && (WriteState.CurrentLineLength + sizeOfBase64EncodedChar + writeState.FooterLength > lineLength))
                {
                    WriteState.AppendCRLF(shouldAppendSpaceToCRLF);
                }

                //how we actually encode: get three bytes in the
                //buffer to be encoded.  Then, extract six bits at a time and encode each six bit chunk as a base-64 character.
                //this means that three bytes of data will be encoded as four base64 characters.  It also means that to encode
                //a character, we must have three bytes to encode so if the number of bytes is not divisible by three, we 
                //must pad the buffer (this happens below)
                WriteState.Append(base64EncodeMap[(buffer[cur]&0xfc)>>2]);
                WriteState.Append(base64EncodeMap[((buffer[cur]&0x03)<<4) | ((buffer[cur+1]&0xf0)>>4)]);
                WriteState.Append(base64EncodeMap[((buffer[cur+1]&0x0f)<<2) | ((buffer[cur+2]&0xc0)>>6)]);
                WriteState.Append(base64EncodeMap[(buffer[cur+2]&0x3f)]);
            }

            cur = calcLength; //Where we left off before

            // See if we need to fold before writing the last section (with possible padding)
            if ((count % 3 != 0) && (lineLength != -1)
                && (WriteState.CurrentLineLength + sizeOfBase64EncodedChar + writeState.FooterLength >= lineLength))
            {
                WriteState.AppendCRLF(shouldAppendSpaceToCRLF);
            }
            
            //now pad this thing if we need to.  Since it must be a number of bytes that is evenly divisble by 3, 
            //if there are extra bytes, pad with '=' until we have a number of bytes divisible by 3
            switch(count%3)
            {
                case 2: //One character padding needed
                    WriteState.Append(base64EncodeMap[(buffer[cur]&0xFC)>>2]);
                    WriteState.Append(base64EncodeMap[((buffer[cur]&0x03)<<4)|((buffer[cur+1]&0xf0)>>4)]);
                    if (dontDeferFinalBytes) {
                        WriteState.Append(base64EncodeMap[((buffer[cur+1]&0x0f)<<2)]);
                        WriteState.Append(base64EncodeMap[64]);
                        WriteState.Padding = 0;
                    }
                    else{
                        WriteState.LastBits = (byte)((buffer[cur+1]&0x0F)<<2);
                        WriteState.Padding = 1;
                    }
                    cur += 2;
                    break;

                case 1: // Two character padding needed
                    WriteState.Append(base64EncodeMap[(buffer[cur]&0xFC)>>2]);
                    if (dontDeferFinalBytes) {
                        WriteState.Append(base64EncodeMap[(byte)((buffer[cur]&0x03)<<4)]);
                        WriteState.Append(base64EncodeMap[64]);
                        WriteState.Append(base64EncodeMap[64]);
                        WriteState.Padding = 0;
                    }
                    else{
                        WriteState.LastBits = (byte)((buffer[cur]&0x03)<<4);
                        WriteState.Padding = 2;
                    }
                    cur++;
                    break;
            }

            // Write out the last footer, if any.  e.g. ?=
            WriteState.AppendFooter();
            return cur - offset;
        }

        public Stream GetStream()
        {
            return this;
        }

        public string GetEncodedString()
        {
            return ASCIIEncoding.ASCII.GetString(this.WriteState.Buffer, 0, this.WriteState.Length);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
                throw new ArgumentNullException("asyncResult");

            int read = ReadAsyncResult.End(asyncResult);
            return read;
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
                throw new ArgumentNullException("asyncResult");

            WriteAsyncResult.End(asyncResult);
        }

        public override void Flush()
        {
            if (this.writeState != null && WriteState.Length > 0)
            {
                FlushInternal();
            }
            base.Flush();
        }

        private void FlushInternal()
        {
            base.Write(WriteState.Buffer, 0, WriteState.Length);
            WriteState.Reset();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (offset < 0 || offset > buffer.Length)
                throw new ArgumentOutOfRangeException("offset");

            if (offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException("count");

            for (;;)
            {
                // read data from the underlying stream
                int read = base.Read(buffer, offset, count);

                // if the underlying stream returns 0 then there
                // is no more data - ust return 0.
                if (read == 0)
                    return 0;

                // while decoding, we may end up not having
                // any bytes to return pending additional data
                // from the underlying stream.
                read = DecodeBytes(buffer, offset, read);
                if (read > 0)
                    return read;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (offset < 0 || offset > buffer.Length)
                throw new ArgumentOutOfRangeException("offset");

            if (offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException("count");

            int written = 0;

            // do not append a space when writing from a stream since this means 
            // it's writing the email body
            for (;;)
            {
                written += EncodeBytes(buffer, offset + written, count - written, false, false);
                if (written < count)
                    FlushInternal();
                else
                    break;
            }
        }

        class ReadAsyncResult : LazyAsyncResult
        {
            Base64Stream parent;
            byte[] buffer;
            int offset;
            int count;
            int read;

            static AsyncCallback onRead = new AsyncCallback(OnRead);

            internal ReadAsyncResult(Base64Stream parent, byte[] buffer, int offset, int count, AsyncCallback callback, object state) : base(null,state,callback)
            {
                this.parent = parent;
                this.buffer = buffer;
                this.offset = offset;
                this.count = count;
            }

            bool CompleteRead(IAsyncResult result)
            {
                this.read = this.parent.BaseStream.EndRead(result);

                // if the underlying stream returns 0 then there
                // is no more data - ust return 0.
                if (read == 0)
                {
                    InvokeCallback();
                    return true;
                }

                // while decoding, we may end up not having
                // any bytes to return pending additional data
                // from the underlying stream.
                this.read = this.parent.DecodeBytes(this.buffer, this.offset, this.read);
                if (this.read > 0)
                {
                    InvokeCallback();
                    return true;
                }

                return false;
            }

            internal void Read()
            {
                for (;;)
                {
                    IAsyncResult result = this.parent.BaseStream.BeginRead(this.buffer, this.offset, this.count, onRead, this);
                    if (!result.CompletedSynchronously || CompleteRead(result))
                        break;
                }
            }

            static void OnRead(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ReadAsyncResult thisPtr = (ReadAsyncResult)result.AsyncState;
                    try
                    {
                        if (!thisPtr.CompleteRead(result))
                            thisPtr.Read();
                    }
                    catch (Exception e)
                    {
                        if (thisPtr.IsCompleted)
                            throw;
                        thisPtr.InvokeCallback(e);
                    }
                }
            }

            internal static int End(IAsyncResult result)
            {
                ReadAsyncResult thisPtr = (ReadAsyncResult)result;
                thisPtr.InternalWaitForCompletion();
                return thisPtr.read;
            }
        }

        class WriteAsyncResult : LazyAsyncResult
        {
            Base64Stream parent;
            byte[] buffer;
            int offset;
            int count;
            static AsyncCallback onWrite = new AsyncCallback(OnWrite);
            int written;

            internal WriteAsyncResult(Base64Stream parent, byte[] buffer, int offset, int count, AsyncCallback callback, object state) : base(null, state, callback)
            {
                this.parent = parent;
                this.buffer = buffer;
                this.offset = offset;
                this.count = count;
            }

            internal void Write()
            {
                for (;;)
                {
                    // do not append a space when writing from a stream since this means 
                    // it's writing the email body
                    this.written += this.parent.EncodeBytes(this.buffer, this.offset + this.written, 
                        this.count - this.written, false, false);
                    if (this.written < this.count)
                    {
                        IAsyncResult result = this.parent.BaseStream.BeginWrite(this.parent.WriteState.Buffer, 0, this.parent.WriteState.Length, onWrite, this);
                        if (!result.CompletedSynchronously)
                            break;
                        CompleteWrite(result);
                    }
                    else
                    {
                        InvokeCallback();
                        break;
                    }
                }
            }

            void CompleteWrite(IAsyncResult result)
            {
                this.parent.BaseStream.EndWrite(result);
                this.parent.WriteState.Reset();
            }

            static void OnWrite(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    WriteAsyncResult thisPtr = (WriteAsyncResult)result.AsyncState;
                    try
                    {
                        thisPtr.CompleteWrite(result);
                        thisPtr.Write();
                    }
                    catch (Exception e)
                    {
                        if (thisPtr.IsCompleted)
                            throw;
                        thisPtr.InvokeCallback(e);
                    }
                }
            }

            internal static void End(IAsyncResult result)
            {
                WriteAsyncResult thisPtr = (WriteAsyncResult)result;
                thisPtr.InternalWaitForCompletion();
                Debug.Assert(thisPtr.written == thisPtr.count);
            }
        }

        class ReadStateInfo
        {
            byte val;
            byte pos;

            internal byte Val
            {
                get { return this.val; }
                set { this.val = value; }
            }

            internal byte Pos
            {
                get { return this.pos; }
                set { this.pos = value; }
            }
        }       
    }
}
