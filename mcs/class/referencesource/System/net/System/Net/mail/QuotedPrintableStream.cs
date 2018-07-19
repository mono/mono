//-----------------------------------------------------------------------------
// <copyright file="QuotedPrintableStream.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------

namespace System.Net.Mime
{
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// This stream performs in-place decoding of quoted-printable
    /// encoded streams.  Encoding requires copying into a separate
    /// buffer as the data being encoded will most likely grow.
    /// Encoding and decoding is done transparently to the caller.
    /// 
    /// This stream should only be used for the e-mail content.  
    /// Use QEncodedStream for encoding headers.
    /// </summary>
    internal class QuotedPrintableStream : DelegatedStream, IEncodableStream
    {
        //should we encode CRLF or not?
        bool encodeCRLF;

        //number of bytes needed for a soft CRLF in folding
        const int sizeOfSoftCRLF = 3;

        //each encoded byte occupies three bytes when encoded
        const int sizeOfEncodedChar = 3;

        //it takes six bytes to encode a CRLF character (a CRLF that does not indicate folding)
        const int sizeOfEncodedCRLF = 6;

        //if we aren't encoding CRLF then it occupies two chars
        const int sizeOfNonEncodedCRLF = 2;

        static byte[] hexDecodeMap = new byte[] {// 0   1   2   3   4   5   6   7   8   9   A   B   C   D   E   F
                                                  255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255, // 0
                                                  255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255, // 1
                                                  255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255, // 2
                                                    0,  1,  2,  3,  4,  5,  6,  7,  8,  9,255,255,255,255,255,255, // 3
                                                  255, 10, 11, 12, 13, 14, 15,255,255,255,255,255,255,255,255,255, // 4
                                                  255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255, // 5
                                                  255, 10, 11, 12, 13, 14, 15,255,255,255,255,255,255,255,255,255, // 6
                                                  255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255, // 7
                                                  255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255, // 8
                                                  255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255, // 9
                                                  255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255, // A
                                                  255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255, // B
                                                  255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255, // C
                                                  255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255, // D
                                                  255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255, // E
                                                  255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255, // F
        };

        static byte[] hexEncodeMap = new byte[] {  48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 65, 66, 67, 68, 69, 70};

        int lineLength;
        ReadStateInfo readState;
        WriteStateInfoBase writeState;

      

        /// <summary>
        /// ctor.
        /// </summary>
        /// <param name="stream">Underlying stream</param>
        /// <param name="lineLength">Preferred maximum line-length for writes</param>
        internal QuotedPrintableStream(Stream stream, int lineLength) : base(stream)
        {
            if (lineLength < 0)
                throw new ArgumentOutOfRangeException("lineLength");

            this.lineLength = lineLength;
        }

        internal QuotedPrintableStream(Stream stream, bool encodeCRLF)
            : this(stream, EncodedStreamFactory.DefaultMaxLineLength) {
            this.encodeCRLF = encodeCRLF;
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

        internal WriteStateInfoBase WriteState
        {
            get
            {
                if (this.writeState == null)
                    this.writeState = new WriteStateInfoBase(1024, null, null, lineLength);
                return this.writeState;
            }
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
            FlushInternal();
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

                    // if the last read ended in a partially decoded
                    // sequence, pick up where we left off.
                    if (ReadState.IsEscaped)
                    {
                        // this will be -1 if the previous read ended
                        // with an escape character.
                        if (ReadState.Byte == -1)
                        {
                            // if we only read one byte from the underlying
                            // stream, we'll need to save the byte and
                            // ask for more.
                            if (count == 1)
                            {
                                ReadState.Byte = *source;
                                return 0;
                            }
                            
                            // '=\r\n' means a soft (aka. invisible) CRLF sequence...
                            if (source[0] != '\r' || source[1] != '\n')
                            {
                                byte b1 = hexDecodeMap[source[0]];
                                byte b2 = hexDecodeMap[source[1]];
                                if (b1 == 255)
                                    throw new FormatException(SR.GetString(SR.InvalidHexDigit, b1));
                                if (b2 == 255)
                                    throw new FormatException(SR.GetString(SR.InvalidHexDigit, b2));

                                *dest++ = (byte)((b1 << 4) + b2);
                            }

                            source += 2;
                        }
                        else
                        {
                            // '=\r\n' means a soft (aka. invisible) CRLF sequence...
                            if (ReadState.Byte != '\r' || *source != '\n')
                            {
                                byte b1 = hexDecodeMap[ReadState.Byte];
                                byte b2 = hexDecodeMap[*source];
                                if (b1 == 255)
                                    throw new FormatException(SR.GetString(SR.InvalidHexDigit, b1));
                                if (b2 == 255)
                                    throw new FormatException(SR.GetString(SR.InvalidHexDigit, b2));
                                *dest++ = (byte)((b1 << 4) + b2);
                            }
                            source++;
                        }
                        // reset state for next read.
                        ReadState.IsEscaped = false;
                        ReadState.Byte = -1;
                    }

                    // Here's where most of the decoding takes place.
                    // We'll loop around until we've inspected all the
                    // bytes read.
                    while (source < end)
                    {
                        // if the source is not an escape character, then
                        // just copy as-is.
                        if (*source != '=')
                        {
                            *dest++ = *source++;
                        }
                        else
                        {
                            // determine where we are relative to the end
                            // of the data.  If we don't have enough data to 
                            // decode the escape sequence, save off what we
                            // have and continue the decoding in the next
                            // read.  Otherwise, decode the data and copy
                            // into dest.
                            switch (end - source)
                            {
                                case 2:
                                    ReadState.Byte = source[1];
                                    goto case 1;
                                case 1:
                                    ReadState.IsEscaped = true;
                                    goto EndWhile;
                                default:
                                    if (source[1] != '\r' || source[2] != '\n')
                                    {
                                        byte b1 = hexDecodeMap[source[1]];
                                        byte b2 = hexDecodeMap[source[2]];
                                        if (b1 == 255)
                                            throw new FormatException(SR.GetString(SR.InvalidHexDigit, b1));
                                        if (b2 == 255)
                                            throw new FormatException(SR.GetString(SR.InvalidHexDigit, b2));

                                        *dest++ = (byte)((b1 << 4) + b2);
                                    }
                                    source += 3;
                                    break;
                            }
                        }
                    }
                EndWhile:
                    count = (int)(dest - start);
                }
            }
            return count;
        }


        public int EncodeBytes(byte[] buffer, int offset, int count)
        {
            int cur = offset;
            for (; cur < count + offset; cur++)
            {
                //only fold if we're before a whitespace or if we're at the line limit
                //add two to the encoded Byte Length to be conservative so that we guarantee that the line length is acceptable                
                if ((lineLength != -1 && WriteState.CurrentLineLength + sizeOfEncodedChar + 2 >= this.lineLength && (buffer[cur] == ' ' ||
                    buffer[cur] == '\t' || buffer[cur] == '\r' || buffer[cur] == '\n')) || 
                    writeState.CurrentLineLength + sizeOfEncodedChar + 2 >= EncodedStreamFactory.DefaultMaxLineLength)
                {
                    if (WriteState.Buffer.Length - WriteState.Length < sizeOfSoftCRLF)
                        return cur - offset;  //ok because folding happens externally

                    WriteState.Append((byte)'=');
                    WriteState.AppendCRLF(false);
                }

                // We don't need to worry about RFC 2821 4.5.2 (encoding first dot on a line),
                // it is done by the underlying 7BitStream

                //detect a CRLF in the input and encode it.
                if (buffer[cur] == '\r' && cur + 1 < count + offset && buffer[cur+1] == '\n')
                {
                    if (WriteState.Buffer.Length - WriteState.Length < (encodeCRLF ? sizeOfEncodedCRLF : sizeOfNonEncodedCRLF))
                        return cur - offset;
                    cur++;
                    
                    if(encodeCRLF){
                        // The encoding for CRLF is =0D=0A
                        WriteState.Append((byte)'=', (byte)'0', (byte)'D', (byte)'=', (byte)'0', (byte)'A');
                    }
                    else{
                        WriteState.AppendCRLF(false);
                    }
                }
                //ascii chars less than 32 (control chars) and greater than 126 (non-ascii) are not allowed so we have to encode
                else if ((buffer[cur] < 32 && buffer[cur] != '\t') ||                    
                    buffer[cur] == '=' ||
                    buffer[cur] > 126) {
                    if (WriteState.Buffer.Length - WriteState.Length < sizeOfSoftCRLF)
                        return cur - offset;

                    //append an = to indicate an encoded character
                    WriteState.Append((byte)'=');
                    //shift 4 to get the first four bytes only and look up the hex digit
                    WriteState.Append(hexEncodeMap[buffer[cur] >> 4]);
                    //clear the first four bytes to get the last four and look up the hex digit
                    WriteState.Append(hexEncodeMap[buffer[cur] & 0xF]);
                }
                else
                {
                    if (WriteState.Buffer.Length - WriteState.Length < 1)
                        return cur - offset;

                    //detect special case:  is whitespace at end of line?  we must encode it if it is
                    if ((buffer[cur] == (byte)'\t' || buffer[cur] == (byte)' ') &&
                        (cur + 1 >= count + offset)) {

                        if (WriteState.Buffer.Length - WriteState.Length < sizeOfEncodedChar)
                            return cur - offset;

                        //append an = to indicate an encoded character
                        WriteState.Append((byte)'=');
                        //shift 4 to get the first four bytes only and look up the hex digit
                        WriteState.Append(hexEncodeMap[buffer[cur] >> 4]);
                        //clear the first four bytes to get the last four and look up the hex digit
                        WriteState.Append(hexEncodeMap[buffer[cur] & 0xF]);
                    }
                    else {
                        WriteState.Append(buffer[cur]);
                    }
                }
            }
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

        public override void EndWrite(IAsyncResult asyncResult)
        {
            WriteAsyncResult.End(asyncResult);
        }

        public override void Flush()
        {
            FlushInternal();
            base.Flush();
        }

        void FlushInternal()
        {
            if (this.writeState != null && this.writeState.Length > 0)
            {
                base.Write(WriteState.Buffer, 0, WriteState.Length);
                WriteState.BufferFlushed();
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
            for (;;)
            {
                written += EncodeBytes(buffer, offset + written, count - written);
                if (written < count)
                    FlushInternal();
                else
                    break;
            }
        }

        class ReadStateInfo
        {
            bool isEscaped = false;
            short b1 = -1;

            internal bool IsEscaped
            {
                get { return this.isEscaped; }
                set { this.isEscaped = value; }
            }

            internal short Byte
            {
                get { return this.b1; }
                set { this.b1 = value; }
            }
        }

        class WriteAsyncResult : LazyAsyncResult
        {
            QuotedPrintableStream parent;
            byte[] buffer;
            int offset;
            int count;
            static AsyncCallback onWrite = new AsyncCallback(OnWrite);
            int written;

            internal WriteAsyncResult(QuotedPrintableStream parent, byte[] buffer, int offset, int count, AsyncCallback callback, object state) : base(null, state, callback)
            {
                this.parent = parent;
                this.buffer = buffer;
                this.offset = offset;
                this.count = count;
            }

            void CompleteWrite(IAsyncResult result)
            {
                this.parent.BaseStream.EndWrite(result);
                this.parent.WriteState.BufferFlushed();
            }

            internal static void End(IAsyncResult result)
            {
                WriteAsyncResult thisPtr = (WriteAsyncResult)result;
                thisPtr.InternalWaitForCompletion();
                System.Diagnostics.Debug.Assert(thisPtr.written == thisPtr.count);
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
                        thisPtr.InvokeCallback(e);
                    }
                }
            }

            internal void Write()
            {
                for (;;)
                {
                    this.written += this.parent.EncodeBytes(this.buffer, this.offset + this.written, this.count - this.written);
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
        }
    }
}
