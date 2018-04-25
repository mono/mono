//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using System.Xml;

    abstract class CanonicalFormWriter
    {
        internal static readonly UTF8Encoding Utf8WithoutPreamble = new UTF8Encoding(false);

        protected static void Base64EncodeAndWrite(Stream stream, byte[] workBuffer, char[] base64WorkBuffer, byte[] data)
        {
            if ((data.Length / 3) * 4 + 4 > base64WorkBuffer.Length)
            {
                EncodeAndWrite(stream, Convert.ToBase64String(data));
                return;
            }

            int encodedLength = Convert.ToBase64CharArray(data, 0, data.Length, base64WorkBuffer, 0, Base64FormattingOptions.None);
            EncodeAndWrite(stream, workBuffer, base64WorkBuffer, encodedLength);
        }

        protected static void EncodeAndWrite(Stream stream, byte[] workBuffer, string s)
        {
            if (s.Length > workBuffer.Length)
            {
                EncodeAndWrite(stream, s);
                return;
            }

            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c < 127)
                {
                    workBuffer[i] = (byte) c;
                }
                else
                {
                    EncodeAndWrite(stream, s);
                    return;
                }
            }

            stream.Write(workBuffer, 0, s.Length);
        }

        protected static void EncodeAndWrite(Stream stream, byte[] workBuffer, char[] chars)
        {
            EncodeAndWrite(stream, workBuffer, chars, chars.Length);
        }

        protected static void EncodeAndWrite(Stream stream, byte[] workBuffer, char[] chars, int count)
        {
            if (count > workBuffer.Length)
            {
                EncodeAndWrite(stream, chars, count);
                return;
            }

            for (int i = 0; i < count; i++)
            {
                char c = chars[i];
                if (c < 127)
                {
                    workBuffer[i] = (byte) c;
                }
                else
                {
                    EncodeAndWrite(stream, chars, count);
                    return;
                }
            }

            stream.Write(workBuffer, 0, count);
        }

        static void EncodeAndWrite(Stream stream, string s)
        {
            byte[] buffer = CanonicalFormWriter.Utf8WithoutPreamble.GetBytes(s);
            stream.Write(buffer, 0, buffer.Length);
        }

        static void EncodeAndWrite(Stream stream, char[] chars, int count)
        {
            byte[] buffer = CanonicalFormWriter.Utf8WithoutPreamble.GetBytes(chars, 0, count);
            stream.Write(buffer, 0, buffer.Length);
        }
    }
}
