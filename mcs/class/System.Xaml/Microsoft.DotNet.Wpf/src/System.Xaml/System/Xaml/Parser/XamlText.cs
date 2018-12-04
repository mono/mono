// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Text;
using System.Diagnostics;
using System.Xaml;
using System.Xaml.MS.Impl;

namespace MS.Internal.Xaml.Parser
{
    [DebuggerDisplay("{Text}")]
    internal class XamlText
    {
        private const Char SPACE = ' ';
        private const Char NEWLINE = '\n';
        private const Char RETURN = '\r';
        private const Char TAB = '\t';
        private const Char OPENCURLIE = '{';
        private const Char CLOSECURLIE = '}';
        private const String ME_ESCAPE = "{}";
        private const string RETURN_STRING = "\r";

        private StringBuilder _sb;
        private readonly bool _isSpacePreserve;   // should space be preserved or collapsed?
        private bool _isWhiteSpaceOnly;           // is the whole thing whitespace.

        public XamlText(bool spacePreserve)
        {
            // The majority of TEXT found in a XamlParse results in a single string call to Paste().
            // A possible perf improvement might be to hold the first string and
            // put off allocating the StringBuilder until a second string is Paste()ed.

            _sb = new StringBuilder();
            _isSpacePreserve = spacePreserve;
            _isWhiteSpaceOnly = true;
        }

        public bool IsEmpty
        {
            get { return _sb.Length==0; }
        }

        public string Text
        {
            get
            {
                return _sb.ToString();
            }
        }

        public string AttributeText
        {
            get
            {
                String text = Text;
                if (text.StartsWith(ME_ESCAPE, false, TypeConverterHelper.InvariantEnglishUS))
                {
                    return text.Remove(0, ME_ESCAPE.Length);
                }
                return text;
            }
        }

        public bool IsSpacePreserved
        {
            get { return _isSpacePreserve; }
        }

        public bool IsWhiteSpaceOnly
        {
            get { return _isWhiteSpaceOnly; }
        }

        public void Paste(string text, bool trimLeadingWhitespace, bool convertCRLFtoLF=true)
        {
            bool newTextIsWhitespace = IsWhitespace(text);

            if (_isSpacePreserve)
            {
                if (convertCRLFtoLF)
                {
                    // Convert CRLF into just LF.  Including attribute text values.
                    // Attribute text is internal set to "preserve" to prevent (other) processing.
                    // We processing attribute values in this way because 3.x did it.
                    // Normally new lines are converted to SPACE by the XML reader.
                    //
                    // Note that the code actually just removes CR.  Thus it affects
                    // attribute values that contain CR without LF, which can
                    // arise by explicit user declaration:  <Element Prop="a&#13;b" />
                    // This is the root cause of Dev11 796882.  I'm leaving it as-is, in
                    // case there are compat issues, but if the intent was to convert
                    // CRLF to LF, it should really say   Replace("\r\n", "\n")
                    //
                    // To fix 796882, we don't do any substitutions if the
                    // property is Glyphs.UnicodeString, which should never be changed
                    // without changing the corresponding Glyphs.Indices property.
                    // See XamlScanner.EnqueueAnotherAttribute for the fixed call.
                    text = text.Replace(RETURN_STRING, "");
                }
                _sb.Append(text);
            }
            else if (newTextIsWhitespace)
            {
                if (IsEmpty && !trimLeadingWhitespace)
                {
                    _sb.Append(SPACE);
                }
            }
            else
            {
                bool textHadLeadingWhitespace = IsWhitespaceChar(text[0]);
                bool textHadTrailingWhitespace = IsWhitespaceChar(text[text.Length - 1]);
                bool existingTextHasTrailingWhitespace = false;
                string trimmed = CollapseWhitespace(text);

                // When Text is split by a Comment (or other non-Element/PropElement) we will paste
                // two pieces of Text together.
                // This ensures that the spacing between them is correct.
                if (_sb.Length > 0)
                {
                    // If the existing text is WS then just strike it!
                    if (_isWhiteSpaceOnly)
                    {
                        _sb = new StringBuilder();
                    }
                    else
                    {
                        // Notice if it ends in WS
                        if (IsWhitespaceChar(_sb[_sb.Length-1]))
                        {
                            existingTextHasTrailingWhitespace = true;
                        }
                    }
                }

                // If the new text item had leading whitespace
                // AND we were not asked to trim it off
                // AND there isn't existing text that ends in whitespace.
                // THEN put a space before the trimmed Text.
                if (textHadLeadingWhitespace && !trimLeadingWhitespace && !existingTextHasTrailingWhitespace)
                {
                    _sb.Append(SPACE);
                }
                _sb.Append(trimmed);

                // Always leave trailing WS, if it was present.
                // Trimming trailing WS can only be figured out from higher level context.
                //
                if (textHadTrailingWhitespace)
                {
                    _sb.Append(SPACE);
                }
            }
            _isWhiteSpaceOnly = _isWhiteSpaceOnly && newTextIsWhitespace;
        }

        public bool LooksLikeAMarkupExtension
        {
            get
            {
                int length = _sb.Length;
                if (length > 0 && _sb[0] == OPENCURLIE)
                {
                    if (length > 1 && _sb[1] == CLOSECURLIE)
                        return false;
                    return true;
                }
                return false;
            }
        }

        // ===========================================================

        static bool IsWhitespace(string text)
        {
            for (int i=0; i<text.Length; i++)
            {
                if (!IsWhitespaceChar(text[i]))
                    return false;
            }
            return true;
        }

        static bool IsWhitespaceChar(Char ch)
        {
            return (ch == SPACE || ch == TAB || ch == NEWLINE || ch == RETURN);
        }

        // This removes all leading and trailing whitespace, and it
        // collapses any internal runs of whitespace to a single space.
        //
        static string CollapseWhitespace(string text)
        {
            StringBuilder sb = new StringBuilder(text.Length);
            int firstIdx=0;
            while (firstIdx < text.Length)
            {
                // If it is not whitespace copy it to the destination.
                char ch = text[firstIdx];
                if (!IsWhitespaceChar(ch))
                {
                    sb.Append(ch);
                    ++firstIdx;
                    continue;
                }

                // Skip any runs of whitespace.
                int advancingIdx = firstIdx;
                while (++advancingIdx < text.Length)
                {
                    if (!IsWhitespaceChar(text[advancingIdx]))
                        break;
                }

                // If the spacing is in the middle.  (not at the ends)
                if (firstIdx != 0 && advancingIdx != text.Length)
                {
                    bool skipSpace = false;
                    // check some easy things before digging deep
                    // for Asian chars.  (only process a single newline)
                    if (ch == NEWLINE)
                    {
                        if ((advancingIdx - firstIdx == 2) && text[firstIdx - 1] >= 0x1100)
                        {
                            if (HasSurroundingEastAsianChars(firstIdx, advancingIdx, text))
                            {
                                skipSpace = true;
                            }
                        }
                    }
                    if (!skipSpace)
                    {
                        sb.Append(SPACE);
                    }
                }
                firstIdx = advancingIdx;
            }
            return sb.ToString();
        }

        public static string TrimLeadingWhitespace(string source)
        {
            string result = source.TrimStart(SPACE, TAB, NEWLINE);
            return result;
        }

        public static string TrimTrailingWhitespace(string source)
        {
            string result = source.TrimEnd(SPACE, TAB, NEWLINE);
            return result;
        }

        // ---- Asian newline suppression code ------

        // this code was modeled from the 3.5 XamlReaderHelper.cs
        static bool HasSurroundingEastAsianChars(int start, int end, string text)
        {
            Debug.Assert(start > 0);
            Debug.Assert(end < text.Length);

            int beforeValue;
            if (start - 2 < 0)
            {
                beforeValue = text[0];
            }
            else
            {
                beforeValue = ComputeUnicodeScalarValue(start - 1, start - 2, text);
            }

            if (IsEastAsianCodePoint(beforeValue))
            {
                int afterValue;
                if (end + 1 >= text.Length)
                {
                    afterValue = text[end];
                }
                else
                {
                    afterValue = ComputeUnicodeScalarValue(end, end, text);
                }

                if (IsEastAsianCodePoint(afterValue))
                {
                    return true;
                }
            }
            return false;
        }

        static int ComputeUnicodeScalarValue(int takeOneIdx, int takeTwoIdx, string text)
        {
            int unicodeScalarValue=0;
            bool isSurrogate = false;

            Char highChar = text[takeTwoIdx];
            if (Char.IsHighSurrogate(highChar))
            {
                Char lowChar = text[takeTwoIdx + 1];
                if (Char.IsLowSurrogate(lowChar))
                {
                    isSurrogate = true;
                    unicodeScalarValue = (((highChar & 0x03FF) << 10) | (lowChar & 0x3FF)) + 0x1000;
                }
            }

            if (!isSurrogate)
            {
                unicodeScalarValue = text[takeOneIdx];
            }
            return unicodeScalarValue;
        }

        struct CodePointRange
        {
            public readonly int Min;
            public readonly int Max;
            public CodePointRange(int min, int max)
            {
                Min = min;
                Max = max;
            }
        }

        static CodePointRange[] EastAsianCodePointRanges = new CodePointRange[]
        {
            new CodePointRange ( 0x1100, 0x11FF ),     // Hangul
            new CodePointRange ( 0x2E80, 0x2FD5 ),     // CJK and KangXi Radicals
            new CodePointRange ( 0x2FF0, 0x2FFB ),     // Ideographic Description
#if NICE_COMMENTED_RANGES
            new CodePointRange ( 0x3040, 0x309F ),     // Hiragana
            new CodePointRange ( 0x30A0, 0x30FF ),     // Katakana
            new CodePointRange ( 0x3100, 0x312F ),     // Bopomofo
            new CodePointRange ( 0x3130, 0x318F ),     // Hangul Compatibility Jamo
            new CodePointRange ( 0x3190, 0x319F ),     // Kanbun

            new CodePointRange ( 0x31F0, 0x31FF ),     // Katakana Phonetic Extensions
            new CodePointRange ( 0x3400, 0x4DFF ),     // CJK Unified Ideographs Extension A
            new CodePointRange ( 0x4E00, 0x9FFF ),     // CJK Unified Ideographs
            new CodePointRange ( 0xA000, 0xA4CF ),     // Yi
#else
            new CodePointRange ( 0x3040, 0x319F ),
            new CodePointRange ( 0x31F0, 0xA4CF ),
#endif
            new CodePointRange ( 0xAC00, 0xD7A3 ),     // Hangul Syllables
            new CodePointRange ( 0xF900, 0xFAFF ),     // CJK Compatibility
            new CodePointRange ( 0xFF00, 0xFFEF ),     // Halfwidth and Fullwidth forms
                // surrogates
            new CodePointRange ( 0x20000, 0x2a6d6 ),   // CJK Unified Ext. B
            new CodePointRange ( 0x2F800, 0x2FA1D ),   // CJK Compatibility Supplement
        };

        // taken directly from WPF 3.5
        static bool IsEastAsianCodePoint(int unicodeScalarValue)
        {
            for (int i = 0; i < EastAsianCodePointRanges.Length; i++)
            {
                if (unicodeScalarValue >= EastAsianCodePointRanges[i].Min)
                {
                    if (unicodeScalarValue <= EastAsianCodePointRanges[i].Max)
                        return true;
                }
            }
            return false;
        }
    }
}
