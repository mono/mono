using System;
using System.IO;
using System.Collections.Specialized;
using System.Net.Mime;
using System.Text;

namespace System.Net.Mail
{
    /// <summary>
    /// Summary description for MailMessage.
    /// </summary>


 


    //rfc3461
    [Flags]
    public enum DeliveryNotificationOptions  {
        None = 0, OnSuccess = 1, OnFailure = 2, Delay = 4, Never = (int)0x08000000
    }
     

    public class MailMessage:IDisposable
    {
        private AlternateViewCollection views;
        AttachmentCollection attachments;
        AlternateView bodyView = null;
        string body = String.Empty;
        Encoding bodyEncoding;
        TransferEncoding bodyTransferEncoding = TransferEncoding.Unknown;
        bool isBodyHtml = false;
        bool disposed = false;
        Message message;
        DeliveryNotificationOptions deliveryStatusNotification = DeliveryNotificationOptions.None;

        public MailMessage() {
            message = new Message();
            if(Logging.On)Logging.Associate(Logging.Web, this, message);
            string from = SmtpClient.MailConfiguration.Smtp.From;
            
            if (from != null && from.Length > 0) {
                message.From = new MailAddress(from);
            }
        }

        public MailMessage(string from, string to) {
            if (from == null) 
                throw new ArgumentNullException("from");

            if (to == null)
                throw new ArgumentNullException("to");

            if (from == String.Empty)
                throw new ArgumentException(SR.GetString(SR.net_emptystringcall,"from"), "from");

            if (to == String.Empty)
                throw new ArgumentException(SR.GetString(SR.net_emptystringcall,"to"), "to");

            message = new Message(from,to);
            if(Logging.On)Logging.Associate(Logging.Web, this, message);
        }


        public MailMessage(string from, string to, string subject, string body):this(from,to) {
            Subject = subject;
            Body = body;
        }

        
        public MailMessage(MailAddress from, MailAddress to) {
            if (from == null) 
                throw new ArgumentNullException("from");

            if (to == null)
                throw new ArgumentNullException("to");
        
            message = new Message(from,to);
        }

        
        public MailAddress From {
            get {
                return message.From;
            }
            set {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }
                message.From = value;
            }
        }

        public MailAddress Sender {
            get {
                return message.Sender;
            }
            set {
                message.Sender = value;
            }
        }
        
        [Obsolete("ReplyTo is obsoleted for this type.  Please use ReplyToList instead which can accept multiple addresses. http://go.microsoft.com/fwlink/?linkid=14202")]
        public MailAddress ReplyTo {
            get {
                return message.ReplyTo;
            }
            set {
                message.ReplyTo = value;
            }
        }

        public MailAddressCollection ReplyToList {
            get {
                return message.ReplyToList;
            }
        }

        public MailAddressCollection To {
            get {
                return message.To;
            }
        }
        
        public MailAddressCollection Bcc {
            get {
                return message.Bcc;
            }
        }

        public MailAddressCollection CC {
            get {
                return message.CC;
            }
        }

        public MailPriority Priority{
            get {
                return message.Priority;
            }
            set{
                message.Priority = value;
            }
        }

        public DeliveryNotificationOptions  DeliveryNotificationOptions {
            get{
                return deliveryStatusNotification;
            }
            set{
                if (7 < (uint)value && value != DeliveryNotificationOptions.Never) {
                    throw new ArgumentOutOfRangeException("value");
                }
                deliveryStatusNotification = value;
            }
        }

        public string Subject {
            get {
                return (message.Subject != null ? message.Subject : String.Empty);
            }
            set {
               message.Subject = value;
            }
        }

        public Encoding SubjectEncoding {
            get {
                return message.SubjectEncoding;
            }
            set {
                message.SubjectEncoding = value;
            }
        }

        public NameValueCollection Headers {
            get {
                return message.Headers;
            }
        }

        public Encoding HeadersEncoding {
            get {
                return message.HeadersEncoding;
            }
            set {
                message.HeadersEncoding = value;
            }
        }

        public string Body {
            get {
                return (body != null ? body : String.Empty);
            }

            set{
                body = value;

                if (bodyEncoding == null && body != null) {
                    if (MimeBasePart.IsAscii(body,true)) {
                        bodyEncoding = Text.Encoding.ASCII;
                    }
                    else {
                        bodyEncoding = Text.Encoding.GetEncoding(MimeBasePart.defaultCharSet);
                    }
                }
            }
        }

        public Encoding BodyEncoding {
            get {
                return bodyEncoding;
            }
            set {
                bodyEncoding = value;
            }
        }

        public TransferEncoding BodyTransferEncoding {
            get {
                return bodyTransferEncoding;
            }
            set {
                bodyTransferEncoding = value;
            }
        }


        public bool IsBodyHtml{
            get{
                return isBodyHtml;
            }
            set{
                isBodyHtml = value;
            }
        }


        public AttachmentCollection Attachments {
            get {
                if (disposed) {
                    throw new ObjectDisposedException(this.GetType().FullName);
                }

                if (attachments == null) {
                    attachments = new AttachmentCollection();
                }
                return attachments;
            }
        }
        public AlternateViewCollection AlternateViews {
            get {
                if (disposed) {
                    throw new ObjectDisposedException(this.GetType().FullName);
                }
                
                if (views == null) {
                    views = new AlternateViewCollection();
                }

                return views;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !disposed)
            {
                disposed = true;

                if(views != null){
                    views.Dispose();
                }
                if(attachments != null){
                    attachments.Dispose();
                }
                if(bodyView != null){
                    bodyView.Dispose();
                }
            }
        }


        private void SetContent(bool allowUnicode) {

            //the attachments may have changed, so we need to reset the message
            if(bodyView != null){
                bodyView.Dispose();
                bodyView = null;
            }

            if (AlternateViews.Count == 0 && Attachments.Count == 0) {
                if (!string.IsNullOrEmpty(body)) {
                    bodyView = AlternateView.CreateAlternateViewFromString(body, bodyEncoding, (isBodyHtml?MediaTypeNames.Text.Html:null));
                    message.Content = bodyView.MimePart;
                }
            }
            else if (AlternateViews.Count == 0 && Attachments.Count > 0){
                MimeMultiPart part = new MimeMultiPart(MimeMultiPartType.Mixed);

                if (!string.IsNullOrEmpty(body)) {
                    bodyView = AlternateView.CreateAlternateViewFromString(body, bodyEncoding, (isBodyHtml?MediaTypeNames.Text.Html:null));
                }
                else{
                    bodyView = AlternateView.CreateAlternateViewFromString(string.Empty);
                }

                part.Parts.Add(bodyView.MimePart);
                
                foreach (Attachment attachment in Attachments) {
                    if(attachment != null){
                        //ensure we can read from the stream.
                        attachment.PrepareForSending(allowUnicode);
                        part.Parts.Add(attachment.MimePart);
                    }
                }
                message.Content = part;
            }
            else{
                //
                // DTS issue 610361 - we should not unnecessarily use Multipart/Mixed
                // When there is no attachement and all the alternative views are of "Alternative" types.
                //
                MimeMultiPart part = null; 
                MimeMultiPart viewsPart = new MimeMultiPart(MimeMultiPartType.Alternative);

                if (!string.IsNullOrEmpty(body)) {
                    bodyView = AlternateView.CreateAlternateViewFromString(body, bodyEncoding, null);
                    viewsPart.Parts.Add(bodyView.MimePart);
                }

                foreach (AlternateView view in AlternateViews)
                {
                    //ensure we can read from the stream.
                    if (view != null) {
                        view.PrepareForSending(allowUnicode);
                        if (view.LinkedResources.Count > 0)
                        {
                            MimeMultiPart wholeView = new MimeMultiPart(MimeMultiPartType.Related);
                            wholeView.ContentType.Parameters["type"] = view.ContentType.MediaType;
                            wholeView.ContentLocation = view.MimePart.ContentLocation;
                            wholeView.Parts.Add(view.MimePart);

                            foreach (LinkedResource resource in view.LinkedResources)
                            {
                                //ensure we can read from the stream.
                                resource.PrepareForSending(allowUnicode);

                                wholeView.Parts.Add(resource.MimePart);
                            }
                            viewsPart.Parts.Add(wholeView);
                        }
                        else
                        {
                            viewsPart.Parts.Add(view.MimePart);
                        }
                    }
                }

                if (Attachments.Count > 0)
                {
                    part = new MimeMultiPart(MimeMultiPartType.Mixed);
                    part.Parts.Add(viewsPart);

                    MimeMultiPart attachmentsPart = new MimeMultiPart(MimeMultiPartType.Mixed);
                    foreach (Attachment attachment in Attachments)
                    {
                        if (attachment != null)
                        {
                            //ensure we can read from the stream.
                            attachment.PrepareForSending(allowUnicode);
                            attachmentsPart.Parts.Add(attachment.MimePart);
                        }
                    }
                    part.Parts.Add(attachmentsPart);
                    message.Content = part;
                } // If there is no Attachement, AND only "1" Alternate View AND !!no body!!
                  // then in fact, this is NOT a multipart region.
                else if (viewsPart.Parts.Count == 1 && string.IsNullOrEmpty(body))
                {
                    message.Content = viewsPart.Parts[0];
                }
                else
                {
                    message.Content = viewsPart;
                }
            }
            
            if (bodyView != null && bodyTransferEncoding != TransferEncoding.Unknown)
            {
                bodyView.TransferEncoding = bodyTransferEncoding;
            }
        }

        internal void Send(BaseWriter writer, bool sendEnvelope, bool allowUnicode) {
            SetContent(allowUnicode);
            message.Send(writer, sendEnvelope, allowUnicode);
        }

        internal IAsyncResult BeginSend(BaseWriter writer, bool sendEnvelope, bool allowUnicode, 
            AsyncCallback callback, object state) {
            SetContent(allowUnicode);
            return message.BeginSend(writer, sendEnvelope, allowUnicode, callback, state);
        }

        internal void EndSend(IAsyncResult asyncResult) {
            message.EndSend(asyncResult);
        }

        internal string BuildDeliveryStatusNotificationString(){
            if(deliveryStatusNotification != DeliveryNotificationOptions.None){
                StringBuilder s = new StringBuilder(" NOTIFY=");

                bool oneSet = false;

                //none
                if(deliveryStatusNotification == DeliveryNotificationOptions.Never){
                    s.Append("NEVER");
                    return s.ToString();
                }

                if((((int)deliveryStatusNotification) & (int)DeliveryNotificationOptions.OnSuccess) > 0){
                    s.Append("SUCCESS");
                    oneSet = true; 
                }
                if((((int)deliveryStatusNotification) & (int)DeliveryNotificationOptions.OnFailure) > 0){
                    if(oneSet){
                        s.Append(",");
                    }
                    s.Append("FAILURE");
                    oneSet = true; 
                }
                if((((int)deliveryStatusNotification) & (int)DeliveryNotificationOptions.Delay) > 0){
                    if(oneSet){
                        s.Append(",");
                    }
                    s.Append("DELAY");
                }
                return s.ToString();
            }
            return String.Empty;
        }
    }
}
