namespace System.Net.Mail
{
    using System;
    using System.IO;
    using System.Text;
    using System.Collections;

    //Streams created are read only and return 0 once a full server reply has been read
    //To get the next server reply, call GetNextReplyReader
    class SmtpReplyReaderFactory
    {
        enum ReadState
        {
            Status0,
            Status1,
            Status2,
            ContinueFlag,
            ContinueCR,
            ContinueLF,
            LastCR,
            LastLF,
            Done
        }


        BufferedReadStream bufferedStream;
        byte[] byteBuffer;
        SmtpReplyReader currentReader;
        const int DefaultBufferSize = 256;
        ReadState readState = ReadState.Status0;
        SmtpStatusCode statusCode;

        internal SmtpReplyReaderFactory(Stream stream)
        {
            bufferedStream = new BufferedReadStream(stream);
        }

        internal SmtpReplyReader CurrentReader
        {
            get
            {
                return currentReader;
            }
        }

        internal SmtpStatusCode StatusCode
        {
            get
            {
                return statusCode;
            }
        }

        internal IAsyncResult BeginReadLines(SmtpReplyReader caller, AsyncCallback callback, object state)
        {
            ReadLinesAsyncResult result =  new ReadLinesAsyncResult(this, callback, state);
            result.Read(caller);
            return result;
        }

        internal IAsyncResult BeginReadLine(SmtpReplyReader caller, AsyncCallback callback, object state)
        {
            ReadLinesAsyncResult result =  new ReadLinesAsyncResult(this, callback, state, true);
            result.Read(caller);
            return result;
        }
        
        
        internal void Close(SmtpReplyReader caller)
        {
            if (currentReader == caller)
            {
                if (readState != ReadState.Done)
                {
                    if (byteBuffer == null)
                    {
                        byteBuffer = new byte[SmtpReplyReaderFactory.DefaultBufferSize];
                    }

                    while (0 != Read(caller, byteBuffer, 0, byteBuffer.Length));
                }

                currentReader = null;
            }
        }

        internal LineInfo[] EndReadLines(IAsyncResult result)
        {
            return ReadLinesAsyncResult.End(result);
        }

        internal LineInfo EndReadLine(IAsyncResult result)
        {
            LineInfo[] info = ReadLinesAsyncResult.End(result);
            if(info != null && info.Length >0){
                return info[0];
            }
            return new LineInfo();
        }

        internal SmtpReplyReader GetNextReplyReader()
        {
            if (currentReader != null)
            {
                currentReader.Close();
            }

            readState = ReadState.Status0;
            currentReader = new SmtpReplyReader(this);
            return currentReader;
        }

        int ProcessRead(byte[] buffer, int offset, int read, bool readLine)
        {
            // if 0 bytes were read,there was a failure
            if (read == 0)
            {
                throw new IOException(SR.GetString(SR.net_io_readfailure, SR.net_io_connectionclosed));
            }

            unsafe
            {
                fixed (byte* pBuffer = buffer)
                {
                    byte* start = pBuffer + offset;
                    byte* ptr = start;
                    byte* end = ptr + read;

                    switch (readState)
                    {
                        case ReadState.Status0:
                        {
                            if (ptr < end)
                            {
                                byte b = *ptr++;
                                if (b < '0' && b > '9')
                                {
                                    throw new FormatException(SR.GetString(SR.SmtpInvalidResponse));
                                }

                                statusCode = (SmtpStatusCode)(100*(b - '0'));

                                goto case ReadState.Status1;
                            }
                            readState = ReadState.Status0;
                            break;
                        }
                        case ReadState.Status1:
                        {
                            if (ptr < end)
                            {
                                byte b = *ptr++;
                                if (b < '0' && b > '9')
                                {
                                    throw new FormatException(SR.GetString(SR.SmtpInvalidResponse));
                                }

                                statusCode += 10*(b - '0');

                                goto case ReadState.Status2;
                            }
                            readState = ReadState.Status1;
                            break;
                        }
                        case ReadState.Status2:
                        {
                            if (ptr < end)
                            {
                                byte b = *ptr++;
                                if (b < '0' && b > '9')
                                {
                                    throw new FormatException(SR.GetString(SR.SmtpInvalidResponse));
                                }

                                statusCode += b - '0';

                                goto case ReadState.ContinueFlag;
                            }
                            readState = ReadState.Status2;
                            break;
                        }
                        case ReadState.ContinueFlag:
                        {
                            if (ptr < end)
                            {
                                byte b = *ptr++;
                                if (b == ' ')       // last line
                                {
                                    goto case ReadState.LastCR;
                                }
                                else if (b == '-')  // more lines coming
                                {
                                    goto case ReadState.ContinueCR;
                                }
                                else                // error
                                {
                                    throw new FormatException(SR.GetString(SR.SmtpInvalidResponse));
                                }
                            }
                            readState = ReadState.ContinueFlag;
                            break;
                        }
                        case ReadState.ContinueCR:
                        {
                            while (ptr < end)
                            {
                                if (*ptr++ == '\r')
                                {
                                    goto case ReadState.ContinueLF;
                                }
                            }
                            readState = ReadState.ContinueCR;
                            break;
                        }
                        case ReadState.ContinueLF:
                        {
                            if (ptr < end)
                            {
                                if (*ptr++ != '\n')
                                {
                                    throw new FormatException(SR.GetString(SR.SmtpInvalidResponse));
                                }
                                if (readLine)
                                {
                                    readState = ReadState.Status0;
                                    return (int)(ptr - start);
                                }
                                goto case ReadState.Status0;
                            }
                            readState = ReadState.ContinueLF;
                            break;
                        }
                        case ReadState.LastCR:
                        {
                            while (ptr < end)
                            {
                                if (*ptr++ == '\r')
                                {
                                    goto case ReadState.LastLF;
                                }
                            }
                            readState = ReadState.LastCR;
                            break;
                        }
                        case ReadState.LastLF:
                        {
                            if (ptr < end)
                            {
                                if (*ptr++ != '\n')
                                {
                                    throw new FormatException(SR.GetString(SR.SmtpInvalidResponse));
                                }
                                goto case ReadState.Done;
                            }
                            readState = ReadState.LastLF;
                            break;
                        }
                        case ReadState.Done:
                        {
                            int actual = (int)(ptr - start);
                            readState = ReadState.Done;
                            return actual;
                        }
                    }
                    return (int)(ptr - start);
                }
            }
        }
               
        internal int Read(SmtpReplyReader caller, byte[] buffer, int offset, int count)
        {
            // if we've already found the delimitter, then return 0 indicating
            // end of stream.
            if (count == 0 || currentReader != caller || readState == ReadState.Done)
            {
                return 0;
            }

            int read = bufferedStream.Read(buffer, offset, count);
            int actual = ProcessRead(buffer, offset, read, false);
            if (actual < read)
            {
                bufferedStream.Push(buffer, offset + actual, read - actual);
            }

            return actual;
        }
              

        internal LineInfo ReadLine(SmtpReplyReader caller)
        {
           LineInfo[] info = ReadLines(caller,true);
           if(info != null && info.Length >0){
               return info[0];
           }
           return new LineInfo();
        }

        internal LineInfo[] ReadLines(SmtpReplyReader caller)
        {
            return ReadLines(caller,false);
        }


        internal LineInfo[] ReadLines(SmtpReplyReader caller, bool oneLine)
        {
            if (caller != currentReader || readState == ReadState.Done)
            {
                return new LineInfo[0];
            }

            if (byteBuffer == null)
            {
                byteBuffer = new byte[SmtpReplyReaderFactory.DefaultBufferSize];
            }

            System.Diagnostics.Debug.Assert(readState == ReadState.Status0);

            StringBuilder builder = new StringBuilder();
            ArrayList lines = new ArrayList();
            int statusRead = 0;

            for(int start = 0, read = 0; ; )
            {
                if (start == read)
                {
                    read = bufferedStream.Read(byteBuffer, 0, byteBuffer.Length);
                    start = 0;
                }

                int actual = ProcessRead(byteBuffer, start, read - start, true);

                if (statusRead < 4)
                {
                    int left = Math.Min(4-statusRead, actual);
                    statusRead += left;
                    start += left;
                    actual -= left;
                    if (actual == 0)
                    {
                        continue;
                    }
                }

                builder.Append(Encoding.UTF8.GetString(byteBuffer, start, actual));
                start += actual;

                if (readState == ReadState.Status0)
                {
                    statusRead = 0;
                    lines.Add(new LineInfo(statusCode, builder.ToString(0, builder.Length - 2))); // return everything except CRLF
                    
                    if(oneLine){
                        bufferedStream.Push(byteBuffer, start, read - start);
                        return (LineInfo[])lines.ToArray(typeof(LineInfo));
                    }
                    builder = new StringBuilder();
                }
                else if (readState == ReadState.Done)
                {
                    lines.Add(new LineInfo(statusCode, builder.ToString(0, builder.Length - 2))); // return everything except CRLF
                    bufferedStream.Push(byteBuffer, start, read - start);
                    return (LineInfo[])lines.ToArray(typeof(LineInfo));
                }
            }
        }

        class ReadLinesAsyncResult : LazyAsyncResult
        {
            StringBuilder builder;
            ArrayList lines;
            SmtpReplyReaderFactory parent;
            static AsyncCallback readCallback = new AsyncCallback(ReadCallback);
            int read;
            int statusRead;
            bool oneLine;

            internal ReadLinesAsyncResult(SmtpReplyReaderFactory parent, AsyncCallback callback, object state) : base(null, state, callback)
            {
                this.parent = parent;
            }

            internal ReadLinesAsyncResult(SmtpReplyReaderFactory parent, AsyncCallback callback, object state, bool oneLine) : base(null, state, callback)
            {
                this.oneLine = oneLine;
                this.parent = parent;
            }

            internal void Read(SmtpReplyReader caller){

                // if we've already found the delimitter, then return 0 indicating
                // end of stream.
                if (parent.currentReader != caller || parent.readState == ReadState.Done)
                {
                    InvokeCallback();
                    return;
                }

                if (parent.byteBuffer == null)
                {
                    parent.byteBuffer = new byte[SmtpReplyReaderFactory.DefaultBufferSize];
                }

                System.Diagnostics.Debug.Assert(parent.readState == ReadState.Status0);

                builder = new StringBuilder();
                lines = new ArrayList();

                Read();
            }

            internal static LineInfo[] End(IAsyncResult result)
            {
                ReadLinesAsyncResult thisPtr = (ReadLinesAsyncResult)result;
                thisPtr.InternalWaitForCompletion();
                return (LineInfo[])thisPtr.lines.ToArray(typeof(LineInfo));
            }

            void Read()
            {
                do
                {
                    IAsyncResult result = parent.bufferedStream.BeginRead(parent.byteBuffer, 0, parent.byteBuffer.Length, readCallback, this);
                    if (!result.CompletedSynchronously)
                    {
                        return;
                    }
                    read = parent.bufferedStream.EndRead(result);
                } while(ProcessRead());
            }

            static void ReadCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    Exception exception = null;
                    ReadLinesAsyncResult thisPtr = (ReadLinesAsyncResult)result.AsyncState;
                    try
                    {
                        thisPtr.read = thisPtr.parent.bufferedStream.EndRead(result);
                        if (thisPtr.ProcessRead())
                        {
                            thisPtr.Read();
                        }
                    }
                    catch (Exception e)
                    {   exception = e;
                    }

                    if(exception != null){
                        thisPtr.InvokeCallback(exception);
                    }
                }
            }

            bool ProcessRead()
            {
                if (read == 0)
                {
                    throw new IOException(SR.GetString(SR.net_io_readfailure, SR.net_io_connectionclosed));
                }

                for(int start = 0; start != read; )
                {
                    int actual = parent.ProcessRead(parent.byteBuffer, start, read - start, true);

                    if (statusRead < 4)
                    {
                        int left = Math.Min(4-statusRead, actual);
                        statusRead += left;
                        start += left;
                        actual -= left;
                        if (actual == 0)
                        {
                            continue;
                        }
                    }

                    builder.Append(Encoding.UTF8.GetString(parent.byteBuffer, start, actual));
                    start += actual;

                    if (parent.readState == ReadState.Status0)
                    {
                        lines.Add(new LineInfo(parent.statusCode, builder.ToString(0, builder.Length - 2))); // return everything except CRLF
                        builder = new StringBuilder();
                        statusRead = 0;

                        if (oneLine) {
                            parent.bufferedStream.Push(parent.byteBuffer, start, read - start);
                            InvokeCallback();
                            return false;
                        }
                    }
                    else if (parent.readState == ReadState.Done)
                    {
                        lines.Add(new LineInfo(parent.statusCode, builder.ToString(0, builder.Length - 2))); // return everything except CRLF
                        parent.bufferedStream.Push(parent.byteBuffer, start, read - start);
                        InvokeCallback();
                        return false;
                    }
                }
                return true;
            }

        }
    }
}
