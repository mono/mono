//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Diagnostics
{
    using System.Xml;
    using System.Text;
    using System.IO;

    class EncodingFallbackAwareXmlTextWriter : XmlTextWriter
    {
        Encoding encoding;

        internal EncodingFallbackAwareXmlTextWriter(TextWriter writer)
            : base(writer)
        {
            this.encoding = writer.Encoding;
        }

        public override void WriteString(string value)
        {
            if (!string.IsNullOrEmpty(value) && 
                ContainsInvalidXmlChar(value))
            {
                byte[] blob = encoding.GetBytes(value);
                value = encoding.GetString(blob);
            }
            base.WriteString(value);
        }

        bool ContainsInvalidXmlChar(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            int i = 0;
            int len = value.Length;

            while (i < len)
            {
                if (XmlConvert.IsXmlChar(value[i]))
                {
                    i++;
                    continue;
                }
                
                if (i + 1 < len &&
                    XmlConvert.IsXmlSurrogatePair(value[i + 1], value[i]))
                {
                    i += 2;
                    continue;
                }

                return true;
            }

            return false;
        }
    }
}
