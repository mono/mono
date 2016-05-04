using System.Net.Mime;
using System.Text;

namespace System.Net.Mail
{
    public enum MailPriority {
        Normal = 0,  
        Low = 1,
        High = 2
    }
    
    internal class Message
    {
        #region Fields

        MailAddress from;
        MailAddress sender;
        MailAddressCollection replyToList;
        MailAddress replyTo;
        MailAddressCollection to;
        MailAddressCollection cc;
        MailAddressCollection bcc;
        MimeBasePart content;
        HeaderCollection headers;
        HeaderCollection envelopeHeaders;
        string subject;
        Encoding subjectEncoding;
        Encoding headersEncoding;
        MailPriority priority = (MailPriority)(-1);

        #endregion Fields

        #region Constructors

        internal Message() {
        }

        internal Message(string from, string to):this() {
            if (from == null) 
                throw new ArgumentNullException("from");

            if (to == null)
                throw new ArgumentNullException("to");

            if (from == String.Empty)
                throw new ArgumentException(SR.GetString(SR.net_emptystringcall,"from"), "from");

            if (to == String.Empty)
                throw new ArgumentException(SR.GetString(SR.net_emptystringcall,"to"), "to");

            this.from = new MailAddress(from);
            MailAddressCollection collection = new MailAddressCollection();
            collection.Add(to);
            this.to = collection;
        }


        internal Message(MailAddress from, MailAddress to):this() {
            this.from = from;
            this.To.Add(to);
        }
        
        #endregion Constructors

        #region Properties

        public MailPriority Priority{
            get {
                return (((int)priority == -1)?MailPriority.Normal:priority);
            }
            set{
                priority = value;
            }
        }

        internal MailAddress From {
            get {
                return from;
            }
            set {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }
                from = value;
            }
        }
        
        
        internal MailAddress Sender {
            get {
                return sender;
            }
            set {
                sender = value;
            }
        }
        
        
        internal MailAddress ReplyTo {
            get {
                return replyTo;
            }
            set {
                replyTo = value;
            }
        }

        internal MailAddressCollection ReplyToList {
            get {
                if (replyToList == null)
                    replyToList = new MailAddressCollection();

                return replyToList;
            }
        }

        internal MailAddressCollection To {
            get {
                if (to == null)
                    to = new MailAddressCollection();

                return to;
            }
        }
        
        internal MailAddressCollection Bcc {
            get {
                if (bcc == null)
                    bcc = new MailAddressCollection();

                return bcc;
            }
        }

        internal MailAddressCollection CC {
            get {
                if (cc == null)
                    cc = new MailAddressCollection();

                return cc;
            }
        }


        internal string Subject {
            get {
                return subject;
            }
            set {
                Encoding inputEncoding = null;
                try { 
                    // extract the encoding from =?encoding?BorQ?blablalba?=
                    inputEncoding = MimeBasePart.DecodeEncoding(value);
                }
                catch (ArgumentException) { };

                if (inputEncoding != null && value != null) {
                    try {
                        // Store the decoded value, we'll re-encode before sending
                        value = MimeBasePart.DecodeHeaderValue(value);
                        subjectEncoding = subjectEncoding ?? inputEncoding;
                    }
                    // Failed to decode, just pass it through as ascii (legacy)
                    catch (FormatException) { }
                }

                if (value != null && MailBnfHelper.HasCROrLF(value)) {
                    throw new ArgumentException(SR.GetString(SR.MailSubjectInvalidFormat));
                }
                subject = value;

                if (subject != null) {
                    subject = subject.Normalize(NormalizationForm.FormC);   
                    if (subjectEncoding == null && !MimeBasePart.IsAscii(subject, false)) {
                        subjectEncoding = Encoding.GetEncoding(MimeBasePart.defaultCharSet);
                    }
                }
            }
        }

        internal Encoding SubjectEncoding {
            get {
                return subjectEncoding;
            }
            set {
                subjectEncoding = value;
            }
        }

        internal HeaderCollection Headers {
            get {
                if (headers == null) {
                    headers = new HeaderCollection();
                    if(Logging.On)Logging.Associate(Logging.Web, this, headers);
                }

                return headers;
            }
        }

        internal Encoding HeadersEncoding {
            get {
                return headersEncoding;
            }
            set {
                headersEncoding = value;
            }
        }

        internal HeaderCollection EnvelopeHeaders {
            get {
                if (envelopeHeaders == null) {
                    envelopeHeaders = new HeaderCollection();
                    if(Logging.On)Logging.Associate(Logging.Web, this, envelopeHeaders);
                }

                return envelopeHeaders;
            }
        }


        internal virtual MimeBasePart Content {
            get {
                return content;
            }
            set {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }

                content = value;
            }
        }
        
        #endregion Properties

        #region Sending

        internal void EmptySendCallback(IAsyncResult result)
        {
            Exception e = null;

            if(result.CompletedSynchronously){
                return;
            }

            EmptySendContext context = (EmptySendContext)result.AsyncState;
            try{
                context.writer.EndGetContentStream(result).Close();
            }
            catch(Exception ex){
                e = ex;
            }
            context.result.InvokeCallback(e);
        }

        internal class EmptySendContext {
            internal EmptySendContext(BaseWriter writer, LazyAsyncResult result) {
                this.writer = writer;
                this.result = result;
            }
        
            internal LazyAsyncResult result;
            internal BaseWriter writer;
        }

        internal virtual IAsyncResult BeginSend(BaseWriter writer, bool sendEnvelope, bool allowUnicode, 
            AsyncCallback callback, object state) {
                       
            PrepareHeaders(sendEnvelope, allowUnicode);
            writer.WriteHeaders(Headers, allowUnicode);

            if (Content != null) {
                return Content.BeginSend(writer, callback, allowUnicode, state);
            }
            else{
                LazyAsyncResult result = new LazyAsyncResult(this,state,callback);
                IAsyncResult newResult = writer.BeginGetContentStream(EmptySendCallback, new EmptySendContext(writer,result));
                if(newResult.CompletedSynchronously){
                    writer.EndGetContentStream(newResult).Close();
                }
                return result;
            }
        }

        internal virtual void EndSend(IAsyncResult asyncResult){
            if (asyncResult == null) {
                throw new ArgumentNullException("asyncResult");
            }

            if (Content != null) {
                Content.EndSend(asyncResult);
            }
            else{
                LazyAsyncResult castedAsyncResult = asyncResult as LazyAsyncResult;

                if (castedAsyncResult == null || castedAsyncResult.AsyncObject != this) {
                    throw new ArgumentException(SR.GetString(SR.net_io_invalidasyncresult));
                }

                if (castedAsyncResult.EndCalled) {
                    throw new InvalidOperationException(SR.GetString(SR.net_io_invalidendcall, "EndSend"));
                }

                castedAsyncResult.InternalWaitForCompletion();
                castedAsyncResult.EndCalled = true;
                if (castedAsyncResult.Result is Exception) {
                    throw (Exception)castedAsyncResult.Result;
                }
            }
        }

        internal virtual void Send(BaseWriter writer, bool sendEnvelope, bool allowUnicode) {

            if (sendEnvelope) {
                PrepareEnvelopeHeaders(sendEnvelope, allowUnicode);
                writer.WriteHeaders(EnvelopeHeaders, allowUnicode);
            }

            PrepareHeaders(sendEnvelope, allowUnicode);
            writer.WriteHeaders(Headers, allowUnicode);

            if (Content != null) {
                Content.Send(writer, allowUnicode);
            }
            else{
                writer.GetContentStream().Close();
            }
        }

        internal void PrepareEnvelopeHeaders(bool sendEnvelope, bool allowUnicode) {
            
            if (this.headersEncoding == null) {
                this.headersEncoding = Encoding.GetEncoding(MimeBasePart.defaultCharSet);
            }

            EncodeHeaders(this.EnvelopeHeaders, allowUnicode);

            // Dev10 #430372: only add X-Sender header if it wasn't already set by the user
            string xSenderHeader = MailHeaderInfo.GetString(MailHeaderID.XSender);
            if (!IsHeaderSet(xSenderHeader)) {
                MailAddress sender = Sender ?? From;
                EnvelopeHeaders.InternalSet(xSenderHeader, sender.Encode(xSenderHeader.Length, allowUnicode));
            }

            string headerName = MailHeaderInfo.GetString(MailHeaderID.XReceiver);
            EnvelopeHeaders.Remove(headerName);

            foreach (MailAddress address in To) {
                EnvelopeHeaders.InternalAdd(headerName, address.Encode(headerName.Length, allowUnicode));
            }
            foreach (MailAddress address in CC) {
                EnvelopeHeaders.InternalAdd(headerName, address.Encode(headerName.Length, allowUnicode));
            }
            foreach (MailAddress address in Bcc) {
                EnvelopeHeaders.InternalAdd(headerName, address.Encode(headerName.Length, allowUnicode));
            }
        }

        internal void PrepareHeaders(bool sendEnvelope, bool allowUnicode) {
            
            string headerName;

            if (this.headersEncoding == null) {
                this.headersEncoding = Encoding.GetEncoding(MimeBasePart.defaultCharSet);
            }

            //ContentType is written directly to the stream so remove potential user duplicate
            Headers.Remove(MailHeaderInfo.GetString(MailHeaderID.ContentType));

            Headers[MailHeaderInfo.GetString(MailHeaderID.MimeVersion)] = "1.0";
            
            // add sender to headers first so that it is written first to allow the IIS smtp svc to
            // send MAIL FROM with the sender if both sender and from are present
            headerName = MailHeaderInfo.GetString(MailHeaderID.Sender);
            if(Sender != null) {
                Headers.InternalAdd(headerName, Sender.Encode(headerName.Length, allowUnicode));
            }
            else {
                Headers.Remove(headerName);
            }

            headerName = MailHeaderInfo.GetString(MailHeaderID.From);
            Headers.InternalAdd(headerName, From.Encode(headerName.Length, allowUnicode));

            headerName = MailHeaderInfo.GetString(MailHeaderID.To);
            if (To.Count > 0) {
                Headers.InternalAdd(headerName, To.Encode(headerName.Length, allowUnicode));
            }
            else {
                Headers.Remove(headerName);
            }

            headerName = MailHeaderInfo.GetString(MailHeaderID.Cc);
            if (CC.Count > 0) {
                Headers.InternalAdd(headerName, CC.Encode(headerName.Length, allowUnicode));
            }
            else {
                Headers.Remove(headerName);
            }

            headerName = MailHeaderInfo.GetString(MailHeaderID.ReplyTo);
            if (ReplyTo != null) {
                Headers.InternalAdd(headerName, ReplyTo.Encode(headerName.Length, allowUnicode));
            } 
            else if (ReplyToList.Count > 0) {
                Headers.InternalAdd(headerName, ReplyToList.Encode(headerName.Length, allowUnicode));
            } 
            else {
                Headers.Remove(headerName);
            }

            Headers.Remove(MailHeaderInfo.GetString(MailHeaderID.Bcc));

            if (priority == MailPriority.High){
                Headers[MailHeaderInfo.GetString(MailHeaderID.XPriority)] = "1";
                Headers[MailHeaderInfo.GetString(MailHeaderID.Priority)] = "urgent";
                Headers[MailHeaderInfo.GetString(MailHeaderID.Importance)] = "high";
            }
            else if (priority == MailPriority.Low){
                Headers[MailHeaderInfo.GetString(MailHeaderID.XPriority)] = "5";
                Headers[MailHeaderInfo.GetString(MailHeaderID.Priority)] = "non-urgent";
                Headers[MailHeaderInfo.GetString(MailHeaderID.Importance)] = "low";
               }
            //if the priority was never set, allow the app to set the headers directly.
            else if (((int)priority) != -1){
                Headers.Remove(MailHeaderInfo.GetString(MailHeaderID.XPriority));
                Headers.Remove(MailHeaderInfo.GetString(MailHeaderID.Priority));
                Headers.Remove(MailHeaderInfo.GetString(MailHeaderID.Importance));
            }

            Headers.InternalAdd(MailHeaderInfo.GetString(MailHeaderID.Date), 
                MailBnfHelper.GetDateTimeString(DateTime.Now, null));

            headerName = MailHeaderInfo.GetString(MailHeaderID.Subject);
            if (!string.IsNullOrEmpty(subject)){
                if (allowUnicode) {
                    Headers.InternalAdd(headerName, subject);
                }
                else {
                    Headers.InternalAdd(headerName,
                        MimeBasePart.EncodeHeaderValue(subject, subjectEncoding,
                        MimeBasePart.ShouldUseBase64Encoding(subjectEncoding),
                        headerName.Length));
                }
            }
            else{
                Headers.Remove(headerName);
            }

            EncodeHeaders(this.headers, allowUnicode);
        }

        internal void EncodeHeaders(HeaderCollection headers, bool allowUnicode) {
            
            if (this.headersEncoding == null) {
                this.headersEncoding = Encoding.GetEncoding(MimeBasePart.defaultCharSet);
            }

            System.Diagnostics.Debug.Assert(this.headersEncoding != null);
            
            for (int i = 0; i < headers.Count; i++) {
                string headerName = headers.GetKey(i);

                //certain well-known values are encoded by PrepareHeaders and PrepareEnvelopeHeaders
                //so we can ignore them because either we encoded them already or there is no 
                //way for the user to have set them.  If a header is well known and user settable then
                //we should encode it here, otherwise we have already encoded it if necessary
                if (!MailHeaderInfo.IsUserSettable(headerName)) {
                    continue;
                }

                string[] values = headers.GetValues(headerName);
                string encodedValue = String.Empty;
                for (int j = 0; j < values.Length; j++) {
                    //encode if we need to
                    if (MimeBasePart.IsAscii(values[j], false)
                         || (allowUnicode && MailHeaderInfo.AllowsUnicode(headerName) // EAI
                            && !MailBnfHelper.HasCROrLF(values[j]))) { 
                        encodedValue = values[j];
                    } 
                    else {
                        encodedValue = MimeBasePart.EncodeHeaderValue(values[j],
                                                        this.headersEncoding,
                                                        MimeBasePart.ShouldUseBase64Encoding(this.headersEncoding),
                                                        headerName.Length);
                    }

                    //potentially there are multiple values per key
                    if (j == 0) {
                        //if it's the first or only value, set will overwrite all the values assigned to that key
                        //which is fine since we have them stored in values[]
                        headers.Set(headerName, encodedValue);
                    } 
                    else {
                        //this is a subsequent key, so we must Add it since the first key will have overwritten the
                        //other values
                        headers.Add(headerName, encodedValue);
                    }
                    
                }
            }
        }

        private bool IsHeaderSet(string headerName)
        {
            for (int i = 0; i < Headers.Count; i++)
            {
                if (string.Compare(Headers.GetKey(i), headerName, 
                    StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion Sending
    }
}
