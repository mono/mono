using System;
using System.Collections;
using System.IO;
using System.Net.Mail;

namespace System.Net.Mime
{
    /// <summary>
    /// Summary description for EmbeddedMessagePart.
    /// </summary>
    ///

    /*
    // Consider removing.
    internal class MimeEmbeddedMessagePart:MimePart
    {
        Message message;
        internal MimeEmbeddedMessagePart(Message message, bool inline)
        {
            if (message == null) {
                throw new ArgumentNullException("message");
            }
            this.message = message;
            this.Inline = inline;
            this.ContentType = new ContentType("message/rfc822");
        }

        internal Message Message {
            get {
                return message;
            }
        }


        void MessageSentCallback(IAsyncResult result) {
            MimePartContext context = (MimePartContext)result.AsyncState;

            try {
                message.EndSend(result);
            }
            catch (Exception e) {
                context.result.InvokeCallback(e);
                return;
            }
            catch {
                context.result.InvokeCallback(new Exception(SR.GetString(SR.net_nonClsCompliantException)));
                return;
            }
            context.result.InvokeCallback();
        }

        new void ContentStreamCallback(IAsyncResult result) {
            MimePartContext context = (MimePartContext)result.AsyncState;

            try {
                context.outputStream = context.writer.EndGetContentStream(result);
                context.writer = new MimeWriter(context.outputStream, ContentType.Boundary);
                message.BeginSend((MimeWriter)context.writer, false, new AsyncCallback(MessageSentCallback), context);
            }
            catch (Exception e) {
                context.result.InvokeCallback(e);
                return;
            }
            catch {
                context.result.InvokeCallback(new Exception(SR.GetString(SR.net_nonClsCompliantException)));
                return;
            }
            context.result.InvokeCallback();
        }

        internal override IAsyncResult BeginSend(BaseWriter writer, AsyncCallback callback, object state)
        {
            writer.WriteHeaders(Headers);
            MimePartAsyncResult result = new MimePartAsyncResult(this, state, callback);
            MimePartContext context = new MimePartContext(writer, result);
            writer.BeginGetContentStream(new AsyncCallback(ContentStreamCallback), context);
            return result;
        }


        internal override void Send(BaseWriter writer) {
            writer.WriteHeaders(Headers);
            Stream outputStream = writer.GetContentStream();
            MimeWriter mimeWriter = new MimeWriter(outputStream, ContentType.Boundary);
            message.Send(mimeWriter,false);
            mimeWriter.Close();
            outputStream.Close();
        }
    }
    */
}
