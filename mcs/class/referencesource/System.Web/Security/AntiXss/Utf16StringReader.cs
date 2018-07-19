//------------------------------------------------------------------------------
// <copyright file="Utf16StringReader.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Security.AntiXss {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// Reads individual scalar values from a UTF-16 input string.
    /// </summary>
    /// <remarks>
    /// For performance reasons, this is a mutable struct. Use caution when capturing instances of this type.
    /// </remarks>
    internal struct Utf16StringReader {

        /// <summary>
        /// Starting code point for the UTF-16 leading surrogates.
        /// </summary>
        private const char LeadingSurrogateStart = '\uD800';

        /// <summary>
        /// Starting code point for the UTF-16 trailing surrogates.
        /// </summary>
        private const char TrailingSurrogateStart = '\uDC00';

        /// <summary>
        /// The Unicode replacement character U+FFFD.
        /// </summary>
        /// <remarks>
        /// For more info, see http://www.unicode.org/charts/PDF/UFFF0.pdf.
        /// </remarks>
        private const int UnicodeReplacementCharacterCodePoint = '\uFFFD';

        /// <summary>
        /// The current offset into '_input'.
        /// </summary>
        private int _currentOffset;

        /// <summary>
        /// The input string we're iterating on.
        /// </summary>
        private readonly string _input;

        /// <summary>
        /// Initializes the reader with the given UTF-16 input string.
        /// </summary>
        /// <param name="input">The input string to decompose into scalar values.</param>
        public Utf16StringReader(string input) {
            Debug.Assert(input != null);

            _input = input;
            _currentOffset = 0;
        }

        /// <summary>
        /// Similar to Char.ConvertToUtf32, but slightly faster in tight loops since parameter checks are not done.
        /// </summary>
        /// <param name="leadingSurrogate">The UTF-16 leading surrogate character.</param>
        /// <param name="trailingSurrogate">The UTF-16 trailing surrogate character.</param>
        /// <returns>The scalar value resulting from combining these two surrogate characters.</returns>
        /// <remarks>The caller must ensure that the inputs are valid surrogate characters. If not,
        /// the output of this routine is undefined.</remarks>
        private static int ConvertToUtf32(char leadingSurrogate, char trailingSurrogate) {
            Debug.Assert(Char.IsHighSurrogate(leadingSurrogate), "'leadingSurrogate' was not a high surrogate.");
            Debug.Assert(Char.IsLowSurrogate(trailingSurrogate), "'trailingSurrogate' was not a low surrogate.");

            return (int)((leadingSurrogate - LeadingSurrogateStart) * 0x400 + (trailingSurrogate - TrailingSurrogateStart)) + 0x10000;
        }

        /// <summary>
        /// Determines whether a given code point is a valid Unicode scalar value.
        /// </summary>
        /// <param name="codePoint">The code point whose validity is to be checked.</param>
        /// <returns>True if the input is a valid Unicode scalar value, false otherwise.</returns>
        private static bool IsValidUnicodeScalarValue(int codePoint) {
            // Valid scalar values are U+0000 .. U+D7FF and U+E000 .. U+10FFFF.
            // See: http://www.unicode.org/versions/Unicode6.0.0/ch03.pdf, D76
            return (0 <= codePoint && codePoint <= 0xD7FF)
                || (0xE000 <= codePoint && codePoint <= 0x10FFFF);
        }

        /// <summary>
        /// Reads the next scalar value from the input string.
        /// </summary>
        /// <returns>The next scalar value. If the input string contains invalid UTF-16, the
        /// return value is the Unicode replacement character U+FFFD. If the end of the string
        /// is reached, returns -1.</returns>
        public int ReadNextScalarValue() {
            if (_currentOffset >= _input.Length) {
                return -1; // EOF
            }

            char thisCodeUnit = _input[_currentOffset++];
            int thisCodePoint = thisCodeUnit;

            if (Char.IsHighSurrogate(thisCodeUnit)) {
                if (_currentOffset < _input.Length) {
                    char nextCodeUnit = _input[_currentOffset];
                    if (Char.IsLowSurrogate(nextCodeUnit)) {
                        // We encountered a high (leading) surrogate followed by a low
                        // (trailing) surrogate. Bump '_currentOffset' up by one more
                        // since we're consuming both code units.
                        _currentOffset++;
                        thisCodePoint = ConvertToUtf32(thisCodeUnit, nextCodeUnit);
                    }
                }
            }

            if (IsValidUnicodeScalarValue(thisCodePoint)) {
                return thisCodePoint;
            }
            else {
                // ERROR: This code point was either an unmatched high (leading)
                // surrogate or an unmatched low (trailing) surrogate, neither of
                // which maps to a valid Unicode scalar value. Per the Unicode
                // specification (Ch. 3, C10), we replace with U+FFFD.
                return UnicodeReplacementCharacterCodePoint;
            }
        }

    }
}
