using System.IO;
using System.Net.Mime;
using System.Text;
using System.Runtime.Versioning;

namespace System.Net.Mail
{
    public abstract class AttachmentBase : IDisposable
    {
        internal bool disposed = false;
        MimePart part = new MimePart();

        internal AttachmentBase(){
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        protected AttachmentBase(string fileName) {
            SetContentFromFile(fileName, String.Empty);
        }
    
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        protected AttachmentBase(string fileName, string mediaType) {
            SetContentFromFile(fileName, mediaType);
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        protected AttachmentBase(string fileName, ContentType contentType) {
            SetContentFromFile(fileName, contentType);
        }

        protected AttachmentBase(Stream contentStream) {
            part.SetContent(contentStream);
        }

        protected AttachmentBase(Stream contentStream, string mediaType) {
            part.SetContent(contentStream, null, mediaType);
        }
        
        internal AttachmentBase(Stream contentStream, string name, string mediaType) {
            part.SetContent(contentStream, name, mediaType);
        }

        protected AttachmentBase(Stream contentStream, ContentType contentType) {
            part.SetContent(contentStream, contentType);
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
                part.Dispose();
            }
        }

        internal static string ShortNameFromFile(string fileName) {
            string name;
            int start = fileName.LastIndexOfAny(new char[] { '\\', ':' }, fileName.Length - 1, fileName.Length);

            if (start > 0) {
                name = fileName.Substring(start + 1, fileName.Length - start - 1);
            }
            else {
                name = fileName;
            }
            return name;
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal void SetContentFromFile(string fileName, ContentType contentType) {
            if (fileName == null) {
                throw new ArgumentNullException("fileName");
            }
        
            if (fileName == String.Empty)
            {
                throw new ArgumentException(SR.GetString(SR.net_emptystringcall,"fileName"), "fileName");
            }
            
            Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            part.SetContent(stream, contentType);
        }


        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal void SetContentFromFile(string fileName, string mediaType) {
            if (fileName == null) {
                throw new ArgumentNullException("fileName");
            }
        
            if (fileName == String.Empty)
            {
                throw new ArgumentException(SR.GetString(SR.net_emptystringcall,"fileName"), "fileName");
            }
            
            Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            part.SetContent(stream,null,mediaType);
        }

        internal void SetContentFromString(string contentString, ContentType contentType) {
            if (contentString == null) {
                throw new ArgumentNullException("content");
            }

            if (part.Stream != null) {
                part.Stream.Close();
            }

            Encoding encoding;

            if (contentType != null && contentType.CharSet != null) {
                encoding = Text.Encoding.GetEncoding(contentType.CharSet);
            }
            else{
                if (MimeBasePart.IsAscii(contentString,false)) {
                    encoding = Text.Encoding.ASCII;
                }
                else {
                    encoding = Text.Encoding.GetEncoding(MimeBasePart.defaultCharSet);
                }
            }
            byte[] buffer = encoding.GetBytes(contentString);
            part.SetContent(new MemoryStream(buffer),contentType);

            
            if (MimeBasePart.ShouldUseBase64Encoding(encoding)){
                part.TransferEncoding = TransferEncoding.Base64;
            }
            else{
                part.TransferEncoding = TransferEncoding.QuotedPrintable;
            }
        }

        internal void SetContentFromString(string contentString, Encoding encoding, string mediaType) {
            if (contentString == null) {
                throw new ArgumentNullException("content");
            }
            
            if (part.Stream != null) {
                part.Stream.Close();
            }

            if (mediaType == null || mediaType == string.Empty) {
                mediaType = MediaTypeNames.Text.Plain;
            }

            //validate the mediaType
            int offset = 0;
            try{
                string value = MailBnfHelper.ReadToken(mediaType, ref offset, null);
                if (value.Length == 0 || offset >= mediaType.Length || mediaType[offset++] != '/')
                   throw new ArgumentException(SR.GetString(SR.MediaTypeInvalid), "mediaType");
                value = MailBnfHelper.ReadToken(mediaType, ref offset, null);
                if(value.Length == 0 || offset < mediaType.Length){
                    throw new ArgumentException(SR.GetString(SR.MediaTypeInvalid), "mediaType");
                }
            }
            catch(FormatException){
                throw new ArgumentException(SR.GetString(SR.MediaTypeInvalid), "mediaType");
            }


            ContentType contentType = new ContentType(mediaType);

            if (encoding == null){
                if (MimeBasePart.IsAscii(contentString,false)) {
                    encoding = Text.Encoding.ASCII;
                }
                else {
                    encoding = Text.Encoding.GetEncoding(MimeBasePart.defaultCharSet);
                }
            }

            contentType.CharSet = encoding.BodyName;
            byte[] buffer = encoding.GetBytes(contentString);
            part.SetContent(new MemoryStream(buffer),contentType);

            if (MimeBasePart.ShouldUseBase64Encoding(encoding)){
                part.TransferEncoding = TransferEncoding.Base64;
            }
            else{
                part.TransferEncoding = TransferEncoding.QuotedPrintable;
            }
        }


        internal virtual void PrepareForSending(bool allowUnicode){
            part.ResetStream();
        }
               
        public Stream ContentStream {
            get {
                if (disposed) {
                    throw new ObjectDisposedException(this.GetType().FullName);
                }

                return part.Stream;
            }
        }

        public string ContentId
        {
            get
            {
                string cid = part.ContentID;
                if (string.IsNullOrEmpty(cid))
                {
                    cid = Guid.NewGuid().ToString();
                    ContentId = cid;
                    return cid;
                }
                if (cid.Length >= 2 && cid[0] == '<' && cid[cid.Length - 1] == '>')
                {
                    return cid.Substring(1, cid.Length - 2);
                }
                return cid;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    part.ContentID = null;
                }
                else
                {
                    if(value.IndexOfAny(new char[] { '<', '>' }) != -1)
                    {
                        throw new ArgumentException(SR.GetString(SR.MailHeaderInvalidCID), "value");
                    }

                    part.ContentID = "<" + value + ">";
                }
            }
        }

        public ContentType ContentType {
            get {
                return part.ContentType;
            }

            set {
                part.ContentType = value;
            }
        }

        public TransferEncoding TransferEncoding {
            get {
                return part.TransferEncoding;
            }
            set {
                part.TransferEncoding = value;
            }
        }

        internal Uri ContentLocation
        {
            get
            {
                Uri uri;
                if (!Uri.TryCreate(part.ContentLocation, UriKind.RelativeOrAbsolute, out uri))
                {
                    return null;
                }
                return uri;
            }

            set
            {
                part.ContentLocation = value == null ? null : value.IsAbsoluteUri ? value.AbsoluteUri : value.OriginalString;
            }
        }

        internal MimePart MimePart {
            get {
                return part;
            }
        }
    }

    public class Attachment : AttachmentBase
    {


        string name;
        Encoding nameEncoding;

        internal Attachment(){
            MimePart.ContentDisposition = new ContentDisposition();
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public Attachment(string fileName) :base(fileName)
        { 
            Name = ShortNameFromFile(fileName);
            MimePart.ContentDisposition = new ContentDisposition();
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public Attachment(string fileName, string mediaType) :
            base(fileName, mediaType)
        {
            Name = ShortNameFromFile(fileName);
            MimePart.ContentDisposition = new ContentDisposition();
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public Attachment(string fileName, ContentType contentType) :
            base(fileName, contentType)
        { 
            if (contentType.Name == null || contentType.Name == String.Empty) {
                Name = ShortNameFromFile(fileName);
            }
            else{
                Name = contentType.Name;
            }
            MimePart.ContentDisposition = new ContentDisposition();
        }

        public Attachment(Stream contentStream, string name) :
            base(contentStream, null, null)
        { 
            Name = name;
            MimePart.ContentDisposition = new ContentDisposition();
        }

        public Attachment(Stream contentStream, string name, string mediaType) :
            base(contentStream, null, mediaType)
        { 
            Name = name;
            MimePart.ContentDisposition = new ContentDisposition();
        }

        public Attachment(Stream contentStream, ContentType contentType) :
            base(contentStream, contentType)
        { 
            Name = contentType.Name;
            MimePart.ContentDisposition = new ContentDisposition();
        }
            
        internal void SetContentTypeName(bool allowUnicode){
            if (!allowUnicode && name != null && name.Length != 0 && !MimeBasePart.IsAscii(name, false)) {
                Encoding encoding = NameEncoding;
                if(encoding == null){
                    encoding = Encoding.GetEncoding(MimeBasePart.defaultCharSet);
                }
                MimePart.ContentType.Name = MimeBasePart.EncodeHeaderValue(name, encoding ,MimeBasePart.ShouldUseBase64Encoding(encoding));
            }
            else{
                MimePart.ContentType.Name = name;
            }
        }

        public string Name {
            get {
                return name;
            }
            set {
                Encoding nameEncoding = MimeBasePart.DecodeEncoding(value);
                if(nameEncoding != null){
                    this.nameEncoding = nameEncoding;
                    this.name = MimeBasePart.DecodeHeaderValue(value);
                    MimePart.ContentType.Name = value;
                }
                else{
                    this.name = value;
                    SetContentTypeName(true);
                    // This keeps ContentType.Name up to date for user viewability, but isn't necessary.
                    // SetContentTypeName is called again by PrepareForSending()
                }
            }
        }


        public Encoding NameEncoding {
            get {
                return nameEncoding;
            }
            set {
                nameEncoding = value;
                if(name != null && name != String.Empty){
                    SetContentTypeName(true);
                }
            }
        }



        public ContentDisposition ContentDisposition
        {
            get
            {
                return MimePart.ContentDisposition;
            }
        }    


        internal override void PrepareForSending(bool allowUnicode){
            if(name != null && name != String.Empty){
                SetContentTypeName(allowUnicode);
            }
            base.PrepareForSending(allowUnicode);
        }

        public static Attachment CreateAttachmentFromString(string content, string name){
            Attachment a = new Attachment();
            a.SetContentFromString(content,null, String.Empty);
            a.Name = name;
            return a;
        }

        public static Attachment CreateAttachmentFromString(string content, string name, Encoding contentEncoding, string mediaType){
            Attachment a = new Attachment();
            a.SetContentFromString(content, contentEncoding, mediaType);
            a.Name = name;
            return a;
        }

        public static Attachment CreateAttachmentFromString(string content, ContentType contentType){
            Attachment a = new Attachment();
            a.SetContentFromString(content, contentType);
            a.Name = contentType.Name;
            return a;
        }
    }
}
