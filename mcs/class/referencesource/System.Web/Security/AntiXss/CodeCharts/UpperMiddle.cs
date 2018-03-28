//------------------------------------------------------------------------------
// <copyright file="UpperMiddle.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Security.AntiXss.CodeCharts {
    using System.Collections;
    using System.Linq;

    /// <summary>
    /// Provides safe character positions for the upper middle section of the UTF code tables.
    /// </summary>
    internal static class UpperMiddle {
        /// <summary>
        /// Determines if the specified flag is set.
        /// </summary>
        /// <param name="flags">The value to check.</param>
        /// <param name="flagToCheck">The flag to check for.</param>
        /// <returns>true if the flag is set, otherwise false.</returns>
        public static bool IsFlagSet(UpperMidCodeCharts flags, UpperMidCodeCharts flagToCheck) {
            return (flags & flagToCheck) != 0;
        }

        /// <summary>
        /// Provides the safe characters for the Cyrillic Extended A code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable CyrillicExtendedA() {
            return CodeChartHelper.GetRange(0x2DE0, 0x2DFF);
        }

        /// <summary>
        /// Provides the safe characters for the Cyrillic Extended A code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable SupplementalPunctuation() {
            return CodeChartHelper.GetRange(0x2E00, 0x2E31);
        }

        /// <summary>
        /// Provides the safe characters for the CJK Radicals Supplement code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable CjkRadicalsSupplement() {
            return CodeChartHelper.GetRange(0x2E80, 0x2EF3, 
                i => (i == 0x2E9A));
        }

        /// <summary>
        /// Provides the safe characters for the Kangxi Radicals code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable KangxiRadicals() {
            return CodeChartHelper.GetRange(0x2F00, 0x2FD5);
        }

        /// <summary>
        /// Provides the safe characters for the Ideographic Description Characters code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable IdeographicDescriptionCharacters() {
            return CodeChartHelper.GetRange(0x2FF0, 0x2FFB);
        }

        /// <summary>
        /// Provides the safe characters for the CJK Symbols and Punctuation code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable CjkSymbolsAndPunctuation() {
            return CodeChartHelper.GetRange(0x3000, 0x303F);
        }

        /// <summary>
        /// Provides the safe characters for the Hiragana code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Hiragana() {
            return CodeChartHelper.GetRange(0x3041, 0x309F, 
                i => (i == 0x3097 ||
                    i == 0x3098));
        }

        /// <summary>
        /// Provides the safe characters for the Hiragana code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Katakana() {
            return CodeChartHelper.GetRange(0x30A0, 0x30FF);
        }

        /// <summary>
        /// Provides the safe characters for the Bopomofo code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Bopomofo() {
            return CodeChartHelper.GetRange(0x3105, 0x312D);
        }

        /// <summary>
        /// Provides the safe characters for the Hangul Compatibility Jamo code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable HangulCompatibilityJamo() {
            return CodeChartHelper.GetRange(0x3131, 0x318E);
        }

        /// <summary>
        /// Provides the safe characters for the Kanbun code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Kanbun() {
            return CodeChartHelper.GetRange(0x3190, 0x319F);
        }

        /// <summary>
        /// Provides the safe characters for the Bopomofo Extended code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable BopomofoExtended() {
            return CodeChartHelper.GetRange(0x31A0, 0x31B7);
        }

        /// <summary>
        /// Provides the safe characters for the CJK Strokes code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable CjkStrokes() {
            return CodeChartHelper.GetRange(0x31C0, 0x31E3);
        }

        /// <summary>
        /// Provides the safe characters for the Katakana Phonetic Extensions code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable KatakanaPhoneticExtensions() {
            return CodeChartHelper.GetRange(0x31F0, 0x31FF);
        }

        /// <summary>
        /// Provides the safe characters for the Enclosed CJK Letters and Months code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable EnclosedCjkLettersAndMonths() {
            return CodeChartHelper.GetRange(0x3200, 0x32FE, 
                i => (i == 0x321F));
        }

        /// <summary>
        /// Provides the safe characters for the CJK Compatibility code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable CjkCompatibility() {
            return CodeChartHelper.GetRange(0x3300, 0x33FF);
        }

        /// <summary>
        /// Provides the safe characters for the CJK Unified Ideographs Extension A code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable CjkUnifiedIdeographsExtensionA() {
            return CodeChartHelper.GetRange(0x3400, 0x4DB5);
        }

        /// <summary>
        /// Provides the safe characters for the Yijing Hexagram Symbols code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable YijingHexagramSymbols() {
            return CodeChartHelper.GetRange(0x4DC0, 0x4DFF);
        }

        /// <summary>
        /// Provides the safe characters for the CJK Unified Ideographs code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable CjkUnifiedIdeographs() {
            return CodeChartHelper.GetRange(0x4E00, 0x9FCB);
        }

        /// <summary>
        /// Provides the safe characters for the Yi Syllables code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable YiSyllables() {
            return CodeChartHelper.GetRange(0xA000, 0xA48C);
        }

        /// <summary>
        /// Provides the safe characters for the Yi Radicals code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable YiRadicals() {
            return CodeChartHelper.GetRange(0xA490, 0xA4C6);
        }

        /// <summary>
        /// Provides the safe characters for the Lisu code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Lisu() {
            return CodeChartHelper.GetRange(0xA4D0, 0xA4FF);
        }

        /// <summary>
        /// Provides the safe characters for the Vai code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Vai() {
            return CodeChartHelper.GetRange(0xA500, 0xA62B);
        }

        /// <summary>
        /// Provides the safe characters for the Cyrillic Extended B code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable CyrillicExtendedB() {
            return CodeChartHelper.GetRange(0xA640, 0xA697, 
                i => (i == 0xA660 ||
                    i == 0xA661 ||
                    (i >= 0xA674 && i <= 0xA67b)));
        }

        /// <summary>
        /// Provides the safe characters for the Bamum code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Bamum() {
            return CodeChartHelper.GetRange(0xA6A0, 0xA6F7);
        }

        /// <summary>
        /// Provides the safe characters for the Modifier Tone Letters code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable ModifierToneLetters() {
            return CodeChartHelper.GetRange(0xA700, 0xA71F);
        }

        /// <summary>
        /// Provides the safe characters for the Latin Extended D code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable LatinExtendedD() {
            return CodeChartHelper.GetRange(0xA720, 0xA78C).Concat(
                   CodeChartHelper.GetRange(0xA7FB, 0xA7FF));
        }

        /// <summary>
        /// Provides the safe characters for the Syloti Nagri code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable SylotiNagri() {
            return CodeChartHelper.GetRange(0xA800, 0xA82B);
        }

        /// <summary>
        /// Provides the safe characters for the Common Indic Number Forms code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable CommonIndicNumberForms() {
            return CodeChartHelper.GetRange(0xA830, 0xA839);
        }

        /// <summary>
        /// Provides the safe characters for the Phags-pa code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Phagspa() {
            return CodeChartHelper.GetRange(0xA840, 0xA877);
        }

        /// <summary>
        /// Provides the safe characters for the Saurashtra code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Saurashtra() {
            return CodeChartHelper.GetRange(0xA880, 0xA8D9, 
                i => (i >= 0xA8C5 && i <= 0xA8CD));
        }
    }
}
