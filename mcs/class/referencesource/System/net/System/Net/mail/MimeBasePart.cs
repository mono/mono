using System.Collections.Specialized;
using System.Text;
using System.Net.Mail;

namespace System.Net.Mime
{

    internal class MimeBasePart
    {
        protected ContentType contentType;
        protected ContentDisposition contentDisposition;
        HeaderCollection headers;
        internal const string defaultCharSet = "utf-8";//"iso-8859-1";

        internal MimeBasePart()
        {
        }

        internal static bool ShouldUseBase64Encoding(Encoding encoding){
            if (encoding == Encoding.Unicode || encoding == Encoding.UTF8 || encoding == Encoding.UTF32  || encoding == Encoding.BigEndianUnicode) {
                return true;
            }
            return false;
        }

        //use when the length of the header is not known or if there is no header
        internal static string EncodeHeaderValue(string value, Encoding encoding, bool base64Encoding){
            return MimeBasePart.EncodeHeaderValue(value, encoding, base64Encoding, 0);
        }

        //used when the length of the header name itself is known (i.e. Subject : )
        internal static string EncodeHeaderValue(string value, Encoding encoding, bool base64Encoding, int headerLength) {
            StringBuilder newString = new StringBuilder();
            
            //no need to encode if it's pure ascii
            if (IsAscii(value, false)) {
                return value;
            }

            if (encoding == null) {
                encoding = Encoding.GetEncoding(MimeBasePart.defaultCharSet);
            }

            EncodedStreamFactory factory = new EncodedStreamFactory();
            IEncodableStream stream = factory.GetEncoderForHeader(encoding, base64Encoding, headerLength);
            
            byte[] buffer = encoding.GetBytes(value);
            stream.EncodeBytes(buffer, 0, buffer.Length);
            return stream.GetEncodedString();
        }

        internal static string DecodeHeaderValue(string value) {
            if(value == null || value.Length == 0){
                return String.Empty;
            }

            string newValue = String.Empty;

            //split strings, they may be folded.  If they are, decode one at a time and append the results
            string[] substringsToDecode = value.Split(new char[] { '\r', '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string foldedSubString in substringsToDecode) {
                //an encoded string has as specific format in that it must start and end with an
                //'=' char and contains five parts, separated by '?' chars.
                //the first and last part are therefore '=', the second part is the byte encoding (B or Q)
                //the third is the unicode encoding type, and the fourth is encoded message itself.  '?' is not valid inside of
                //an encoded string other than as a separator for these five parts.
                //If this check fails, the string is either not encoded or cannot be decoded by this method
                string[] subStrings = foldedSubString.Split('?');
                if ((subStrings.Length != 5 || subStrings[0] != "=" || subStrings[4] != "=")) {
                    return value;
                }

                string charSet = subStrings[1];
                bool base64Encoding = (subStrings[2] == "B");
                byte[] buffer = ASCIIEncoding.ASCII.GetBytes(subStrings[3]);
                int newLength;

                EncodedStreamFactory encoderFactory = new EncodedStreamFactory();
                IEncodableStream s = encoderFactory.GetEncoderForHeader(Encoding.GetEncoding(charSet), base64Encoding, 0);

                newLength = s.DecodeBytes(buffer, 0, buffer.Length);

                Encoding encoding = Encoding.GetEncoding(charSet);
                newValue += encoding.GetString(buffer, 0, newLength);
            }
            return newValue;
        }

        // Detect the encoding: "=?encoding?BorQ?content?="
        // "=?utf-8?B?RmlsZU5hbWVf55CG0Y3Qq9C60I5jw4TRicKq0YIM0Y1hSsSeTNCy0Klh?="; // 3.5
        // With the addition of folding in 4.0, there may be multiple lines with encoding, only detect the first:
        // "=?utf-8?B?RmlsZU5hbWVf55CG0Y3Qq9C60I5jw4TRicKq0YIM0Y1hSsSeTNCy0Klh?=\r\n =?utf-8?B??=";
        internal static Encoding DecodeEncoding(string value) {
            if (value == null || value.Length == 0){
                return null;
            }

            string[] subStrings = value.Split('?', '\r', '\n');
            if ((subStrings.Length < 5 || subStrings[0] != "=" || subStrings[4] != "=")) {
                return null;
            }
            string charSet = subStrings[1];
            return Encoding.GetEncoding(charSet);
        }

        internal static bool IsAscii(string value, bool permitCROrLF) {
            if (value == null)
                throw new ArgumentNullException("value");
            
            foreach (char c in value) {
                if ((int)c > 0x7f) {
                    return false;
                }
                if (!permitCROrLF && (c=='\r' || c=='\n')) {
                    return false;
                }
            }
            return true;
        }

        internal static bool IsAnsi(string value, bool permitCROrLF) {
            if (value == null)
                throw new ArgumentNullException("value");
            
            foreach (char c in value) {
                if ((int)c > 0xff) {
                    return false;
                }
                if (!permitCROrLF && (c=='\r' || c=='\n')) {
                    return false;
                }
            }
            return true;
        }
        
        internal string ContentID {
            get {
                return Headers[MailHeaderInfo.GetString(MailHeaderID.ContentID)];
            }
            set {
                if (string.IsNullOrEmpty(value))
                {
                    Headers.Remove(MailHeaderInfo.GetString(MailHeaderID.ContentID));
                }
                else
                {
                    Headers[MailHeaderInfo.GetString(MailHeaderID.ContentID)] = value;
                }
            }
        }

        internal string ContentLocation
        {
            get
            {
                return Headers[MailHeaderInfo.GetString(MailHeaderID.ContentLocation)];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Headers.Remove(MailHeaderInfo.GetString(MailHeaderID.ContentLocation));
                }
                else
                {
                    Headers[MailHeaderInfo.GetString(MailHeaderID.ContentLocation)] = value;
                }
            }
        }

        internal NameValueCollection Headers
        {
            get {
                //persist existing info before returning
                if (headers == null)
                    headers = new HeaderCollection();

                if (contentType == null){
                    contentType = new ContentType();
                }
                contentType.PersistIfNeeded(headers,false);

                if (contentDisposition != null)
                    contentDisposition.PersistIfNeeded(headers,false);
                return headers;
            }
        }
        
        internal ContentType ContentType{
            get{
                if (contentType == null){
                    contentType = new ContentType();
                }
                return contentType;
            }
            set {
                if (value == null)
                    throw new ArgumentNullException("value");

                contentType = value;
                contentType.PersistIfNeeded((HeaderCollection)Headers,true);
            }
        }

        internal void PrepareHeaders(bool allowUnicode) {
            contentType.PersistIfNeeded((HeaderCollection)Headers, false);
            headers.InternalSet(MailHeaderInfo.GetString(MailHeaderID.ContentType), contentType.Encode(allowUnicode));

            if (contentDisposition != null) {
                contentDisposition.PersistIfNeeded((HeaderCollection)Headers, false);
                headers.InternalSet(MailHeaderInfo.GetString(MailHeaderID.ContentDisposition), 
                    contentDisposition.Encode(allowUnicode));
            }
        }

        internal virtual void Send(BaseWriter writer, bool allowUnicode) { 
            throw new NotImplementedException(); 
        }
        
        internal virtual IAsyncResult BeginSend(BaseWriter writer, AsyncCallback callback, 
            bool allowUnicode, object state) {
            throw new NotImplementedException(); 
        }
 
        internal void EndSend(IAsyncResult asyncResult) {

            if (asyncResult == null) {
                throw new ArgumentNullException("asyncResult");
            }

            LazyAsyncResult castedAsyncResult = asyncResult as MimePartAsyncResult;

            if (castedAsyncResult == null || castedAsyncResult.AsyncObject != this) {
                throw new ArgumentException(SR.GetString(SR.net_io_invalidasyncresult), "asyncResult");
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
     
        internal class MimePartAsyncResult: LazyAsyncResult {
            internal MimePartAsyncResult(MimeBasePart part, object state, AsyncCallback callback):base(part,state,callback) {
            }
        }
    }
}

