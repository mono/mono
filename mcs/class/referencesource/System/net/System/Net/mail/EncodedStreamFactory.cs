namespace System.Net.Mime
{
    using System;
    using System.IO;
    using System.Text;

    internal class EncodedStreamFactory
    {
        //RFC 2822: no encoded-word line should be longer than 76 characters not including the soft CRLF
        //since the header length is unknown (if there even is one) we're going to be slightly more conservative
        //and cut off at 70.  This will also prevent any other folding behavior from being triggered anywhere
        //in the code
        private const int defaultMaxLineLength = 70;
        
        //default buffer size for encoder
        private const int initialBufferSize = 1024;

        internal static int DefaultMaxLineLength
        {
            get
            {
                return defaultMaxLineLength;
            }
        }
        
        //get a raw encoder, not for use with header encoding
        internal IEncodableStream GetEncoder(TransferEncoding encoding, Stream stream)
        {
            //raw encoder
            if (encoding == TransferEncoding.Base64)
                return new Base64Stream(stream, new Base64WriteStateInfo());

            //return a QuotedPrintable stream because this is not being used for header encoding
            if (encoding == TransferEncoding.QuotedPrintable)
                return new QuotedPrintableStream(stream, true);

            if (encoding == TransferEncoding.SevenBit || encoding == TransferEncoding.EightBit)
                return new EightBitStream(stream);

            throw new NotSupportedException("Encoding Stream");
        }
        
        //use for encoding headers
        internal IEncodableStream GetEncoderForHeader(Encoding encoding, bool useBase64Encoding, int headerTextLength)
        {
            WriteStateInfoBase writeState;
            byte[] header = CreateHeader(encoding, useBase64Encoding);
            byte[] footer = CreateFooter();

            if (useBase64Encoding)
            {
                writeState = new Base64WriteStateInfo(initialBufferSize, header, footer, DefaultMaxLineLength, headerTextLength);
                return new Base64Stream((Base64WriteStateInfo)writeState);
            }

            writeState = new WriteStateInfoBase(initialBufferSize, header, footer, DefaultMaxLineLength, headerTextLength);
            
            return new QEncodedStream(writeState);
        }

        //Create the header for what type of byte encoding is going to be used
        //based on the encoding type and if base64 encoding should be forced
        //sample header: =?utf-8?B? 
        protected byte[] CreateHeader(Encoding encoding, bool useBase64Encoding)
        {
            //create encoded work header
            string header = String.Format("=?{0}?{1}?", encoding.HeaderName, useBase64Encoding ? "B" : "Q");            
            return Encoding.ASCII.GetBytes(header);
        }

        //creates the footer that marks the end of a quoted string of some sort
        protected byte[] CreateFooter()
        {
            byte[] footer = {(byte)'?', (byte)'='};
            return footer;
        }
    }
}
