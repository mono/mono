//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Dispatcher
{
    using System.Net.Mime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Web;
    using System.Text;

    abstract class MultiplexingFormatMapping
    {
        protected Encoding writeEncoding;
        protected string writeCharset;
        protected WebContentTypeMapper contentTypeMapper;

        abstract public WebMessageFormat MessageFormat { get; }
        abstract public WebContentFormat ContentFormat { get; }
        abstract public string DefaultMediaType { get; }
        abstract protected MessageEncoder Encoder { get; }

        ContentType defaultContentType;

        public ContentType DefaultContentType
        {
            get
            {
                if (defaultContentType == null)
                {
                    defaultContentType = new ContentType(this.DefaultMediaType) { CharSet = this.writeCharset };
                }
                return defaultContentType;
            }
        }

        public MultiplexingFormatMapping(Encoding writeEncoding, WebContentTypeMapper contentTypeMapper)
        {
            if (writeEncoding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writeEncoding");
            }
            this.writeEncoding = writeEncoding;
            this.writeCharset = TextEncoderDefaults.EncodingToCharSet(writeEncoding);
            this.contentTypeMapper = contentTypeMapper;
        }

        public bool CanFormatResponse(ContentType acceptHeaderElement, bool matchCharset, out ContentType contentType)
        {
            if (acceptHeaderElement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("acceptHeaderElement");
            }

            // Scrub the content type so that it is only mediaType and the charset
            string charset = acceptHeaderElement.CharSet;
            contentType = new ContentType(acceptHeaderElement.MediaType);
            contentType.CharSet = this.DefaultContentType.CharSet;
            string contentTypeStr = contentType.ToString();
            
            if (matchCharset &&
                !string.IsNullOrEmpty(charset) &&
                !string.Equals(charset, this.DefaultContentType.CharSet, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (this.contentTypeMapper != null &&
                this.contentTypeMapper.GetMessageFormatForContentType(contentType.MediaType) == this.ContentFormat)
            {
                return true;
            }

            if (this.Encoder.IsContentTypeSupported(contentTypeStr) && 
                (charset == null || contentType.CharSet == this.DefaultContentType.CharSet))
            {
                return true;
            }
            contentType = null;
            return false;
        }
    }

}
