using System;
using System.Collections.Generic;
using System.Text;

namespace Ionic.Encoding
{
    /// <summary>
    /// Provides a text encoder for the iso-8859-1 encoding, aka Latin1 encoding,
    /// for platforms that do not support it, for example on Silverlight or some
    /// Compact Framework platforms.
    /// </summary>
    internal class Iso8859Dash1Encoding : System.Text.Encoding
    {
        /// <summary>
        /// Gets the name registered with the
        /// Internet Assigned Numbers Authority (IANA) for the current encoding.
        /// </summary>
        /// <returns>
        /// Always returns "iso-8859-1".
        /// </returns>
        public override string WebName
        {
            get { return "iso-8859-1"; }
        }

        /// <summary>
        /// Encodes a set of characters from a character array into
        /// a byte array.
        /// </summary>
        /// <returns>
        /// The actual number of bytes written into <paramref name="bytes"/>.
        /// </returns>
        /// <param name="chars">The character array containing the set of characters to encode.
        /// </param><param name="start">The index of the first character to encode.
        /// </param><param name="count">The number of characters to encode.
        /// </param><param name="bytes">The byte array to contain the resulting sequence of bytes.
        /// </param><param name="byteIndex">The index at which to start writing the resulting sequence of bytes.
        /// </param>
        public override int GetBytes(char[] chars, int start, int count, byte[] bytes, int byteIndex)
        {
            if (chars == null)
                throw new ArgumentNullException("chars", "null array");

            if (bytes == null)
                throw new ArgumentNullException("bytes", "null array");

            if (start < 0)
                throw new ArgumentOutOfRangeException("start");
            if (count < 0)
                throw new ArgumentOutOfRangeException("charCount");

            if ((chars.Length - start) < count)
                throw new ArgumentOutOfRangeException("chars");

            if ((byteIndex < 0) || (byteIndex > bytes.Length))
                throw new ArgumentOutOfRangeException("byteIndex");

            // iso-8859-1 is special in that it was adopted as the first page of
            // UCS - ISO's Universal Coding Standard, described in ISO 10646,
            // which is the same as Unicode. This means that a a Unicode
            // character in the range of 0 to FF maps to the iso-8859-1 character
            // with the same value. Because of that the encoding and decoding is
            // trivial.
            for (int i=0; i < count; i++)
            {
                char c = chars[start+i]; // get the unicode char

                if (c >= '\x00FF') // out of range?
                    bytes[byteIndex+i] = (byte) '?';
                else
                    bytes[byteIndex+i] = (byte) c;
            }
            return count;
        }


        /// <summary>
        /// Decodes a sequence of bytes from the specified byte array into the specified character array.
        /// </summary>
        /// <returns>
        /// The actual number of characters written into <paramref name="chars"/>.
        /// </returns>
        /// <param name="bytes">The byte array containing the sequence of bytes to decode.
        /// </param><param name="start">The index of the first byte to decode.
        /// </param><param name="count">The number of bytes to decode.
        /// </param><param name="chars">The character array to contain the resulting set of characters.
        /// </param><param name="charIndex">The index at which to start writing the resulting set of characters.
        /// </param>
        public override int GetChars(byte[] bytes, int start, int count, char[] chars, int charIndex)
        {
            if (chars == null)
                throw new ArgumentNullException("chars", "null array");

            if (bytes == null)
                throw new ArgumentNullException("bytes", "null array");

            if (start < 0)
                throw new ArgumentOutOfRangeException("start");
            if (count < 0)
                throw new ArgumentOutOfRangeException("charCount");

            if ((bytes.Length - start) < count)
                throw new ArgumentOutOfRangeException("bytes");

            if ((charIndex < 0) || (charIndex > chars.Length))
                throw new ArgumentOutOfRangeException("charIndex");

            // In the range 00 to FF, the Unicode characters are the same as the
            // iso-8859-1 characters; because of that, decoding is trivial.
            for (int i = 0; i < count; i++)
                chars[charIndex + i] = (char) bytes[i + start];

            return count;
        }


        /// <summary>
        /// Calculates the number of bytes produced by encoding a set of characters
        /// from the specified character array.
        /// </summary>
        /// <returns>
        /// The number of bytes produced by encoding the specified characters. This class
        /// alwas returns the value of <paramref name="count"/>.
        /// </returns>
        public override int GetByteCount(char[] chars, int index, int count)
        {
            return count;
        }


        /// <summary>
        /// Calculates the number of characters produced by decoding a sequence
        /// of bytes from the specified byte array.
        /// </summary>
        /// <returns>
        /// The number of characters produced by decoding the specified sequence of bytes. This class
        /// alwas returns the value of <paramref name="count"/>.
        /// </returns>
        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return count;
        }


        /// <summary>
        /// Calculates the maximum number of bytes produced by encoding the specified number of characters.
        /// </summary>
        /// <returns>
        /// The maximum number of bytes produced by encoding the specified number of characters. This
        /// class alwas returns the value of <paramref name="charCount"/>.
        /// </returns>
        /// <param name="charCount">The number of characters to encode.
        /// </param>
        public override int GetMaxByteCount(int charCount)
        {
            return charCount;
        }

        /// <summary>
        /// Calculates the maximum number of characters produced by decoding the specified number of bytes.
        /// </summary>
        /// <returns>
        /// The maximum number of characters produced by decoding the specified number of bytes. This class
        /// alwas returns the value of <paramref name="byteCount"/>.
        /// </returns>
        /// <param name="byteCount">The number of bytes to decode.</param>
        public override int GetMaxCharCount(int byteCount)
        {
            return byteCount;
        }

        /// <summary>
        /// Gets the number of characters that are supported by this encoding.
        /// This property returns a maximum value of 256, as the encoding class
        /// only supports single byte encodings (1 byte == 256 possible values).
        /// </summary>
        internal static int CharacterCount
        {
            get { return 256; }
        }

    }
}
