//------------------------------------------------------------------------------
// <copyright file="Utf16StringValidator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Util {
    using System;

    // This class contains utility methods for dealing with security contexts when crossing AppDomain boundaries.

    internal static class Utf16StringValidator {

        private const char UNICODE_NULL_CHAR = '\0';
        private const char UNICODE_REPLACEMENT_CHAR = '\uFFFD';

        private static readonly bool _skipUtf16Validation = AppSettings.AllowRelaxedUnicodeDecoding;

        public static string ValidateString(string input) {
            return ValidateString(input, _skipUtf16Validation);
        }

        // only internal for unit testing
        internal static string ValidateString(string input, bool skipUtf16Validation) {
            if (skipUtf16Validation || String.IsNullOrEmpty(input)) {
                return input;
            }

            // locate the first surrogate character
            int idxOfFirstSurrogate = -1;
            for (int i = 0; i < input.Length; i++) {
                if (Char.IsSurrogate(input[i])) {
                    idxOfFirstSurrogate = i;
                    break;
                }
            }

            // fast case: no surrogates = return input string
            if (idxOfFirstSurrogate < 0) {
                return input;
            }

            // slow case: surrogates exist, so we need to validate them
            char[] chars = input.ToCharArray();
            for (int i = idxOfFirstSurrogate; i < chars.Length; i++) {
                char thisChar = chars[i];

                // If this character is a low surrogate, then it was not preceded by
                // a high surrogate, so we'll replace it.
                if (Char.IsLowSurrogate(thisChar)) {
                    chars[i] = UNICODE_REPLACEMENT_CHAR;
                    continue;
                }

                if (Char.IsHighSurrogate(thisChar)) {
                    // If this character is a high surrogate and it is followed by a
                    // low surrogate, allow both to remain.
                    if (i + 1 < chars.Length && Char.IsLowSurrogate(chars[i + 1])) {
                        i++; // skip the low surrogate also
                        continue;
                    }

                    // If this character is a high surrogate and it is not followed
                    // by a low surrogate, replace it.
                    chars[i] = UNICODE_REPLACEMENT_CHAR;
                    continue;
                }

                // Otherwise, this is a non-surrogate character and just move to the
                // next character.
            }
            return new String(chars);
        }

    }
}
