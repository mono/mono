using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Globalization;
using System.Net.Mail;

namespace System.Net.Mime
{
    /// <summary>
    /// Summary description for MimePart.
    /// </summary>
    internal class MimePart: MimeBasePart,IDisposable
    {
        Stream stream = null;
        bool streamSet = false;
        bool streamUsedOnce = false;
        AsyncCallback readCallback;
        AsyncCallback writeCallback;
        const int maxBufferSize = 0x4400;  //seems optimal for send based on perf analysis
        
        internal MimePart()
        {
        }

        public void Dispose(){
            if (stream != null) {
                stream.Close();
            }
        }

        internal Stream Stream {
            get {
                return stream;
            }
        }


        internal ContentDisposition ContentDisposition{
            get{
                return contentDisposition;
            }
            set{
                contentDisposition = value;
                if(value == null){
                    ((HeaderCollection)Headers).InternalRemove(MailHeaderInfo.GetString(MailHeaderID.ContentDisposition));
                }
                else{
                    contentDisposition.PersistIfNeeded((HeaderCollection)Headers,true);
                }
            }
        }

        internal TransferEncoding TransferEncoding {
            get {
                string value = Headers[MailHeaderInfo.GetString(MailHeaderID.ContentTransferEncoding)];
                if (value.Equals("base64", StringComparison.OrdinalIgnoreCase))
                    return TransferEncoding.Base64;
                else if (value.Equals("quoted-printable", StringComparison.OrdinalIgnoreCase))
                    return TransferEncoding.QuotedPrintable;
                else if (value.Equals("7bit", StringComparison.OrdinalIgnoreCase))
                    return TransferEncoding.SevenBit;
                else if (value.Equals("8bit", StringComparison.OrdinalIgnoreCase))
                    return TransferEncoding.EightBit;
                else
                    return TransferEncoding.Unknown;
            }
            set {
                //QFE 4554
                if (value == TransferEncoding.Base64) {
                    Headers[MailHeaderInfo.GetString(MailHeaderID.ContentTransferEncoding)] = "base64";
                }
                else if (value == TransferEncoding.QuotedPrintable) {
                    Headers[MailHeaderInfo.GetString(MailHeaderID.ContentTransferEncoding)] = "quoted-printable";
                }
                else if (value == TransferEncoding.SevenBit) {
                    Headers[MailHeaderInfo.GetString(MailHeaderID.ContentTransferEncoding)] = "7bit";
                }
                else if (value == TransferEncoding.EightBit) {
                    Headers[MailHeaderInfo.GetString(MailHeaderID.ContentTransferEncoding)] = "8bit";
                }
                else {
                    throw new NotSupportedException(SR.GetString(SR.MimeTransferEncodingNotSupported, value));
                }
            }
        }

        internal void SetContent(Stream stream){
            if (stream == null) {
                throw new ArgumentNullException("stream");
            }
            
            if (streamSet) {
                this.stream.Close();
                this.stream = null;
                streamSet = false;
            }

            this.stream = stream;
            streamSet = true;
            streamUsedOnce = false;
            TransferEncoding = TransferEncoding.Base64;
        }

        internal void SetContent(Stream stream, string name, string mimeType) {
            if (stream == null) {
                throw new ArgumentNullException("stream");
            }

            if (mimeType != null && mimeType != string.Empty) {
                contentType = new ContentType(mimeType);
            }
            if (name != null && name != string.Empty) {
                ContentType.Name = name;
            }
            SetContent(stream);
        }


        internal void SetContent(Stream stream, ContentType contentType) {
            if (stream == null) {
                throw new ArgumentNullException("stream");
            }
            this.contentType = contentType;
            SetContent(stream);
        }


        internal void Complete(IAsyncResult result, Exception e){
            //if we already completed and we got called again,
            //it mean's that there was an exception in the callback and we
            //should just rethrow it.
            
            MimePartContext context = (MimePartContext)result.AsyncState;
            if (context.completed) {
                throw e;
            }

            try{
                if(context.outputStream != null){
                    context.outputStream.Close();
                }
            }
            catch(Exception ex){
                if (e == null) {
                    e = ex;
                }
            }
            context.completed = true;
            context.result.InvokeCallback(e);
        }


        internal void ReadCallback(IAsyncResult result)
        {
            if (result.CompletedSynchronously ) {
                return;
            }

            ((MimePartContext)result.AsyncState).completedSynchronously = false;
            
            try {
                ReadCallbackHandler(result);
            }
            catch(Exception e){
                Complete(result,e);
            }
        }

        
        internal void ReadCallbackHandler(IAsyncResult result){
            MimePartContext context = (MimePartContext)result.AsyncState;
            context.bytesLeft = Stream.EndRead(result);
            if (context.bytesLeft > 0) {
                IAsyncResult writeResult = context.outputStream.BeginWrite(context.buffer, 0, context.bytesLeft, writeCallback, context);
                if (writeResult.CompletedSynchronously) {
                    WriteCallbackHandler(writeResult);
                }
            }
            else {
                Complete(result,null);
            }
        }


        internal void WriteCallback(IAsyncResult result)
        {
            if (result.CompletedSynchronously ) {
                return;
            }

            ((MimePartContext)result.AsyncState).completedSynchronously = false;
            
            try {
                WriteCallbackHandler(result);
            }
            catch (Exception e) {
                Complete(result,e);
            }
        }



        internal void WriteCallbackHandler(IAsyncResult result){
            MimePartContext context = (MimePartContext)result.AsyncState;
            context.outputStream.EndWrite(result);
            IAsyncResult readResult = Stream.BeginRead(context.buffer, 0, context.buffer.Length, readCallback, context);
            if (readResult.CompletedSynchronously) {
                ReadCallbackHandler(readResult);
            }
        }


        internal Stream GetEncodedStream(Stream stream){
            Stream outputStream = stream;                                            

            if (TransferEncoding == TransferEncoding.Base64) {
                outputStream = new Base64Stream(outputStream, new Base64WriteStateInfo());
            }
            else if (TransferEncoding == TransferEncoding.QuotedPrintable) {
                outputStream = new QuotedPrintableStream(outputStream,true);
            }
            else if (TransferEncoding == TransferEncoding.SevenBit || TransferEncoding == TransferEncoding.EightBit) {
                outputStream = new EightBitStream(outputStream);
            }

            return outputStream;
        }
    
        internal void ContentStreamCallbackHandler(IAsyncResult result){

            MimePartContext context = (MimePartContext)result.AsyncState;
            Stream outputStream = context.writer.EndGetContentStream(result);
            context.outputStream = GetEncodedStream(outputStream);

            readCallback = new AsyncCallback(ReadCallback);
            writeCallback = new AsyncCallback(WriteCallback);
            IAsyncResult readResult = Stream.BeginRead(context.buffer, 0, context.buffer.Length,readCallback, context);
            if (readResult.CompletedSynchronously) {
                ReadCallbackHandler(readResult);
            }
        }

        
        internal void ContentStreamCallback(IAsyncResult result)
        {
            if (result.CompletedSynchronously ) {
                return;
            }

            ((MimePartContext)result.AsyncState).completedSynchronously = false;
            
            try{
                ContentStreamCallbackHandler(result);
            }
            catch (Exception e) {
                Complete(result,e);
            }
        }


        internal class MimePartContext
        {
            internal MimePartContext(BaseWriter writer, LazyAsyncResult result)
            {
                this.writer = writer;
                this.result = result;
                buffer = new byte[maxBufferSize];
            }
            
            internal Stream outputStream;
            internal LazyAsyncResult result;
            internal int bytesLeft;
            internal BaseWriter writer;
            internal byte[] buffer;
            internal bool completed;
            internal bool completedSynchronously = true;
        }


        internal override IAsyncResult BeginSend(BaseWriter writer, AsyncCallback callback, bool allowUnicode, 
            object state)
        {
            PrepareHeaders(allowUnicode);
            writer.WriteHeaders(Headers, allowUnicode);
            MimePartAsyncResult result = new MimePartAsyncResult(this, state, callback);
            MimePartContext context = new MimePartContext(writer, result);
            
            ResetStream();
            streamUsedOnce = true;
            IAsyncResult contentResult = writer.BeginGetContentStream(new AsyncCallback(ContentStreamCallback),context);
            if (contentResult.CompletedSynchronously) {
                ContentStreamCallbackHandler(contentResult);
            }
            return result;
        }


        internal override void Send(BaseWriter writer, bool allowUnicode) {
            if (Stream != null) {
                byte[] buffer = new byte[maxBufferSize];
    
                PrepareHeaders(allowUnicode);
                writer.WriteHeaders(Headers, allowUnicode);
    
                Stream outputStream = writer.GetContentStream();
                outputStream = GetEncodedStream(outputStream);
                
                int read;
                
                ResetStream();
                streamUsedOnce = true;
   
                while ((read = Stream.Read(buffer, 0, maxBufferSize)) > 0) {
                    outputStream.Write(buffer, 0, read);
                }
                outputStream.Close();
            }
        }


        //Ensures that if we've used the stream once, we will either reset it to the origin, or throw.
        internal void ResetStream(){
            if (streamUsedOnce) {
                if (Stream.CanSeek) {
                    Stream.Seek(0,SeekOrigin.Begin);
                    streamUsedOnce = false;
                }
                else{
                    throw new InvalidOperationException(SR.GetString(SR.MimePartCantResetStream));
                }
            }
        }
    }
}
