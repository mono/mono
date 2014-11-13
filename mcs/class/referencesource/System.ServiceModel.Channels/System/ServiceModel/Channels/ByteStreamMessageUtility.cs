//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Xml;

    static class ByteStreamMessageUtility
    {
        public const string StreamElementName = "Binary";
        public const string XmlNamespace = "http://www.w3.org/XML/1998/namespace";
        public const string XmlNamespaceNamespace = "http://www.w3.org/2000/xmlns/";

        // used when doing message tracing
        internal const string EncoderName = "ByteStreamMessageEncoder";

        internal static void EnsureByteBoundaries(byte[] buffer, int index, int count, bool isRead)
        {
            if (buffer == null)
            {
                throw FxTrace.Exception.ArgumentNull("buffer");
            }
            if (index < 0)
            {
                throw FxTrace.Exception.ArgumentOutOfRange("index", index, SR.ArgumentOutOfMinRange(0));
            }
            // we explicitly allow the case for index = 0, buffer.Length = 0 and count = 0 when it is write
            // Note that we rely on the last check of count > buffer.Length - index to cover count > 0 && index == buffer.Length case 
            if (index > buffer.Length || (isRead && index == buffer.Length))
            {
                throw FxTrace.Exception.ArgumentOutOfRange("index", index, SR.OffsetExceedsBufferSize(buffer.Length));
            }
            if (count < 0)
            {
                throw FxTrace.Exception.ArgumentOutOfRange("count", count, SR.ArgumentOutOfMinRange(0));
            }
            if (count > buffer.Length - index)
            {
                throw FxTrace.Exception.ArgumentOutOfRange("count", count, SR.SizeExceedsRemainingBufferSpace(buffer.Length - index));
            }
        }

        internal static XmlDictionaryReaderQuotas EnsureQuotas(XmlDictionaryReaderQuotas quotas)
        {
            return quotas ?? EncoderDefaults.ReaderQuotas;
        }
    }
}
