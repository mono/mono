// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  AsyncStreamReader
**
** Purpose: For reading text from streams using a particular 
** encoding in an asychronous manner used by the process class
**
**
===========================================================*/


namespace System.Diagnostics {    
    using System;
    using System.IO;
    using System.Text;
    using System.Runtime.InteropServices;
    using System.Threading;    
    using System.Collections;    
        
    internal delegate void UserCallBack(String data);
 
    internal class AsyncStreamReader : IDisposable
    {
        internal const int DefaultBufferSize = 1024;  // Byte buffer size
        private const int MinBufferSize = 128;
    
        private Stream stream;
        private Encoding encoding;
        private Decoder decoder;
        private byte[] byteBuffer;
        private char[] charBuffer;
        // Record the number of valid bytes in the byteBuffer, for a few checks.

        // This is the maximum number of chars we can get from one call to 
        // ReadBuffer.  Used so ReadBuffer can tell when to copy data into
        // a user's char[] directly, instead of our internal char[].
        private int _maxCharsPerBuffer;

        // Store a backpointer to the process class, to check for user callbacks
        private Process process;

        // Delegate to call user function.
        private UserCallBack userCallBack;

        // Internal Cancel operation
        private bool cancelOperation;       
        private ManualResetEvent eofEvent;
        private Queue messageQueue;
        private StringBuilder sb;
        private bool bLastCarriageReturn;
		
        internal AsyncStreamReader(Process process, Stream stream, UserCallBack callback, Encoding encoding) 
            : this(process, stream, callback, encoding, DefaultBufferSize) {
        }
        

        // Creates a new AsyncStreamReader for the given stream.  The 
        // character encoding is set by encoding and the buffer size, 
        // in number of 16-bit characters, is set by bufferSize.  
        // 
        internal AsyncStreamReader(Process process, Stream stream, UserCallBack callback,  Encoding encoding, int bufferSize)
        {
            Debug.Assert (process != null && stream !=null && encoding !=null && callback != null, "Invalid arguments!");
            Debug.Assert(stream.CanRead, "Stream must be readable!");
            Debug.Assert(bufferSize > 0, "Invalid buffer size!");

            Init(process, stream, callback, encoding, bufferSize);
            messageQueue = new Queue();
        }
            
        private void Init(Process process, Stream stream, UserCallBack callback, Encoding encoding, int bufferSize) {
            this.process = process;
            this.stream = stream;
            this.encoding = encoding;
            this.userCallBack = callback;
            decoder = encoding.GetDecoder();
            if (bufferSize < MinBufferSize) bufferSize = MinBufferSize;
            byteBuffer = new byte[bufferSize];
            _maxCharsPerBuffer = encoding.GetMaxCharCount(bufferSize);
            charBuffer = new char[_maxCharsPerBuffer];
            cancelOperation = false;
            eofEvent = new ManualResetEvent(false);
            sb = null;
            this.bLastCarriageReturn = false;
        }

        public virtual void Close()
        {
            Dispose(true);
        }
        
        void IDisposable.Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) {
                if (stream != null)
                    stream.Close();
            }
            if (stream != null) {
                stream = null;
                encoding = null;
                decoder = null;
                byteBuffer = null;
                charBuffer = null;
            }
            
            if( eofEvent != null) {
                eofEvent.Close();
                eofEvent = null;
            }
        }
        
        public virtual Encoding CurrentEncoding {
            get { return encoding; }
        }
        
        public virtual Stream BaseStream {
            get { return stream; }
        }

        // User calls BeginRead to start the asynchronous read
        internal void BeginReadLine() {
            if( cancelOperation) {
                cancelOperation = false;
            }
            
            if( sb == null ) {
                sb = new StringBuilder(DefaultBufferSize);
                stream.BeginRead(byteBuffer, 0 , byteBuffer.Length,  new AsyncCallback(ReadBuffer), null);
            }
            else {
                FlushMessageQueue();
            }
        }

        internal void CancelOperation() {
            cancelOperation = true;
        }

        // This is the async callback function. Only one thread could/should call this.
        private void ReadBuffer(IAsyncResult ar) {
            
            int byteLen;
            
            try {
                byteLen = stream.EndRead(ar);
            }
            catch (IOException ) {
                // We should ideally consume errors from operations getting cancelled
                // so that we don't crash the unsuspecting parent with an unhandled exc. 
                // This seems to come in 2 forms of exceptions (depending on platform and scenario), 
                // namely OperationCanceledException and IOException (for errorcode that we don't 
                // map explicitly).   
                byteLen = 0; // Treat this as EOF
            }
            catch (OperationCanceledException ) {
                // We should consume any OperationCanceledException from child read here  
                // so that we don't crash the parent with an unhandled exc
                byteLen = 0; // Treat this as EOF
            }
                
            if (byteLen == 0) { 
                // We're at EOF, we won't call this function again from here on.
                lock(messageQueue) {
                    if( sb.Length != 0) {
                        messageQueue.Enqueue(sb.ToString());
                        sb.Length = 0;
                    }
                    messageQueue.Enqueue(null);
                }

                try {
                    // UserCallback could throw, we should still set the eofEvent 
                    FlushMessageQueue();
                }
                finally {
                    eofEvent.Set();
                }
            } else {
                int charLen = decoder.GetChars(byteBuffer, 0, byteLen, charBuffer, 0);
                sb.Append(charBuffer, 0, charLen);
                GetLinesFromStringBuilder();
                stream.BeginRead(byteBuffer, 0 , byteBuffer.Length,  new AsyncCallback(ReadBuffer), null);
            }
        }
        

        // Read lines stored in StringBuilder and the buffer we just read into. 
        // A line is defined as a sequence of characters followed by
        // a carriage return ('\r'), a line feed ('\n'), or a carriage return
        // immediately followed by a line feed. The resulting string does not
        // contain the terminating carriage return and/or line feed. The returned
        // value is null if the end of the input stream has been reached.
        //

        private void GetLinesFromStringBuilder() {
            int i = 0;
            int lineStart = 0;
            int len = sb.Length;

            // skip a beginning '\n' character of new block if last block ended 
            // with '\r'
            if (bLastCarriageReturn && (len > 0) && sb[0] == '\n')
            {            
                i = 1;
                lineStart = 1;
                bLastCarriageReturn = false;
            }
		 
            while(i < len) {
                char ch = sb[i];
                // Note the following common line feed chars:
                // \n - UNIX   \r\n - DOS   \r - Mac
                if (ch == '\r' || ch == '\n') {
                    string s = sb.ToString(lineStart, i - lineStart);
                    lineStart = i + 1;
                    // skip the "\n" character following "\r" character
                    if ((ch == '\r') && (lineStart < len) && (sb[lineStart] == '\n'))
                    {
                        lineStart++;
                        i++;                
                    }
                                        
                    lock(messageQueue) {
                        messageQueue.Enqueue(s);
                    }
                }
                i++;
            }             
            if (sb[len - 1] == '\r') {
                bLastCarriageReturn = true;
            } 
            // Keep the rest characaters which can't form a new line in string builder.
            if( lineStart < len) {
                sb.Remove(0, lineStart);
            }
            else {
                sb.Length = 0;
            }

            FlushMessageQueue();
        }

        private void FlushMessageQueue() {            
            while(true) {
               
                // When we call BeginReadLine, we also need to flush the queue
                // So there could be a ---- between the ReadBuffer and BeginReadLine
                // We need to take lock before DeQueue.
                if( messageQueue.Count > 0) {
                    lock(messageQueue) {
                        if( messageQueue.Count > 0) {
                            string s = (string)messageQueue.Dequeue();
                            // skip if the read is the read is cancelled
                            // this might happen inside UserCallBack
                            // However, continue to drain the queue
                            if (!cancelOperation)
                            {
                                userCallBack(s);
                            }							
                        }
                    }
                }
                else {
                    break;
                }
            }
        }
        
        // Wait until we hit EOF. This is called from Process.WaitForExit
        // We will lose some information if we don't do this.
        internal void WaitUtilEOF() {
            if( eofEvent != null) {
                eofEvent.WaitOne();
                eofEvent.Close();
                eofEvent = null;
            }            
        }
    }
}
