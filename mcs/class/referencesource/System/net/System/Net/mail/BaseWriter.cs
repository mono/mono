using System.IO;
using System.Collections.Specialized;
using System.Net.Mail;

namespace System.Net.Mime
{
    internal abstract class BaseWriter
    {
        #region Fields

        // This is the maximum default line length that can actually be written.  When encoding 
        // headers, the line length is more conservative to account for things like folding.
        // In MailWriter, all encoding has already been done so this will only fold lines
        // that are NOT encoded already, which means being less conservative is ok.
        private static int DefaultLineLength = 76;
        private static AsyncCallback onWrite = new AsyncCallback(OnWrite);
        protected static byte[] CRLF = new byte[] { (byte)'\r', (byte)'\n' };

        protected BufferBuilder bufferBuilder;
        protected Stream contentStream;
        protected bool isInContent;
        protected Stream stream;
        private int lineLength;
        private EventHandler onCloseHandler;
        private bool shouldEncodeLeadingDots;

        #endregion Fields

        protected BaseWriter(Stream stream, bool shouldEncodeLeadingDots)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            
            this.stream = stream;
            this.shouldEncodeLeadingDots = shouldEncodeLeadingDots;
            this.onCloseHandler = new EventHandler(OnClose);
            this.bufferBuilder = new BufferBuilder();
            this.lineLength = DefaultLineLength;
        }

        #region Headers
        
        internal abstract void WriteHeaders(NameValueCollection headers, bool allowUnicode);

        internal void WriteHeader(string name, string value, bool allowUnicode)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            if (value == null)
                throw new ArgumentNullException("value");

            if (this.isInContent)
                throw new InvalidOperationException(SR.GetString(SR.MailWriterIsInContent));

            CheckBoundary();
            this.bufferBuilder.Append(name);
            this.bufferBuilder.Append(": ");
            WriteAndFold(value, name.Length + 2, allowUnicode);
            this.bufferBuilder.Append(CRLF);
        }

        private void WriteAndFold(string value, int charsAlreadyOnLine, bool allowUnicode)
        {
            int lastSpace = 0, startOfLine = 0;
            for (int index = 0; index < value.Length; index++)
            {
                // When we find a FWS (CRLF) copy it as is.
                if (MailBnfHelper.IsFWSAt(value, index)) // At the first char of "\r\n " or "\r\n\t"
                {
                    index += 2; // Skip the FWS
                    this.bufferBuilder.Append(value, startOfLine, index - startOfLine, allowUnicode);
                    // Reset for the next line
                    startOfLine = index;
                    lastSpace = index;
                    charsAlreadyOnLine = 0;
                }
                // When we pass the line length limit, and know where there was a space to fold at, fold there
                else if (((index - startOfLine) > (this.lineLength - charsAlreadyOnLine))
                    && lastSpace != startOfLine)
                {
                    this.bufferBuilder.Append(value, startOfLine, lastSpace - startOfLine, allowUnicode);
                    this.bufferBuilder.Append(CRLF);
                    startOfLine = lastSpace;
                    charsAlreadyOnLine = 0;
                }
                // Mark a foldable space.  If we go over the line length limit, fold here.
                else if (value[index] == MailBnfHelper.Space || value[index] == MailBnfHelper.Tab)
                {
                    lastSpace = index;
                }
            }
            // Write any remaining data to the buffer.
            if (value.Length - startOfLine > 0)
            {
                this.bufferBuilder.Append(value, startOfLine, value.Length - startOfLine, allowUnicode);
            }
        }

        #endregion Headers

        #region Content

        internal Stream GetContentStream()
        {
            return GetContentStream(null);
        }

        private Stream GetContentStream(MultiAsyncResult multiResult)
        {
            if (this.isInContent)
                throw new InvalidOperationException(SR.GetString(SR.MailWriterIsInContent));

            this.isInContent = true;

            CheckBoundary();

            this.bufferBuilder.Append(CRLF);
            Flush(multiResult);

            Stream tempStream = new EightBitStream(this.stream, shouldEncodeLeadingDots);
            ClosableStream cs = new ClosableStream(tempStream, this.onCloseHandler);
            this.contentStream = cs;
            return cs;
        }

        internal IAsyncResult BeginGetContentStream(AsyncCallback callback, object state)
        {
            MultiAsyncResult multiResult = new MultiAsyncResult(this, callback, state);

            Stream s = GetContentStream(multiResult);

            if (!(multiResult.Result is Exception))
                multiResult.Result = s;

            multiResult.CompleteSequence();

            return multiResult;
        }

        internal Stream EndGetContentStream(IAsyncResult result)
        {
            object o = MultiAsyncResult.End(result);
            if (o is Exception)
            {
                throw (Exception)o;
            }
            return (Stream)o;
        }

        #endregion Content

        #region Cleanup

        protected void Flush(MultiAsyncResult multiResult)
        {
            if (this.bufferBuilder.Length > 0)
            {
                if (multiResult != null)
                {
                    multiResult.Enter();
                    IAsyncResult result = this.stream.BeginWrite(this.bufferBuilder.GetBuffer(), 0,
                        this.bufferBuilder.Length, onWrite, multiResult);
                    if (result.CompletedSynchronously)
                    {
                        this.stream.EndWrite(result);
                        multiResult.Leave();
                    }
                }
                else
                {
                    this.stream.Write(this.bufferBuilder.GetBuffer(), 0, this.bufferBuilder.Length);
                }
                this.bufferBuilder.Reset();
            }
        }
        
        protected static void OnWrite(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                MultiAsyncResult multiResult = (MultiAsyncResult)result.AsyncState;
                BaseWriter thisPtr = (BaseWriter)multiResult.Context;
                try
                {
                    thisPtr.stream.EndWrite(result);
                    multiResult.Leave();
                }
                catch (Exception e)
                {
                    multiResult.Leave(e);
                }
            }
        }

        internal abstract void Close();
        
        protected abstract void OnClose(object sender, EventArgs args);

        #endregion Cleanup

        protected virtual void CheckBoundary() { }
    }
}
