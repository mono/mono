namespace System.Net.Mail
{
    using System;
    using System.IO;


    //streams are read only; return of 0 means end of server's reply
    class SmtpReplyReader
    {
        SmtpReplyReaderFactory reader;

        internal SmtpReplyReader(SmtpReplyReaderFactory reader)
        {
            this.reader = reader;
        }

        internal IAsyncResult BeginReadLines(AsyncCallback callback, object state)
        {
            return reader.BeginReadLines(this, callback, state);
        }

        internal IAsyncResult BeginReadLine(AsyncCallback callback, object state)
        {
            return reader.BeginReadLine(this, callback, state);
        }


        public void Close()
        {
            reader.Close(this);
        }

        
        internal LineInfo[] EndReadLines(IAsyncResult result)
        {
            return reader.EndReadLines(result);
        }

        internal LineInfo EndReadLine(IAsyncResult result)
        {
            return reader.EndReadLine(result);
        }
        
        internal LineInfo[] ReadLines()
        {
            return reader.ReadLines(this);
        }

        internal LineInfo ReadLine()
        {
            return reader.ReadLine(this);
        }
    }
}


