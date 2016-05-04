//------------------------------------------------------------------------------
// <copyright file="Upper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Security.AntiXss.CodeCharts {
    using System.Collections;
    using System.Linq;

    /// <summary>
    /// Provides safe character positions for the upper section of the UTF code tables.
    /// </summary>
    internal static class Upper {
        /// <summary>
        /// Determines if the specified flag is set.
        /// </summary>
        /// <param name="flags">The value to check.</param>
        /// <param name="flagToCheck">The flag to check for.</param>
        /// <returns>true if the flag is set, otherwise false.</returns>
        public static bool IsFlagSet(UpperCodeCharts flags, UpperCodeCharts flagToCheck) {
            return (flags & flagToCheck) != 0;
        }

        /// <summary>
        /// Provides the safe characters for the Devanagari Extended code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable DevanagariExtended() {
            return CodeChartHelper.GetRange(0xA8E0, 0xA8FB);
        }

        /// <summary>
        /// Provides the safe characters for the Kayah Li code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable KayahLi() {
            return CodeChartHelper.GetRange(0xA900, 0xA92F);
        }

        /// <summary>
        /// Provides the safe characters for the Rejang code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Rejang() {
            return CodeChartHelper.GetRange(0xA930, 0xA953).Concat(
                                    new[] { 0xA95F });
        }

        /// <summary>
        /// Provides the safe characters for the Hangul Jamo Extended A code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable HangulJamoExtendedA() {
            return CodeChartHelper.GetRange(0xA960, 0xA97C);
        }

        /// <summary>
        /// Provides the safe characters for the Javanese code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Javanese() {
            return CodeChartHelper.GetRange(0xA980, 0xA9DF, 
                i => (i == 0xA9CE ||
                    (i >= 0xA9DA && i <= 0xA9DD)));
        }

        /// <summary>
        /// Provides the safe characters for the Cham code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Cham() {
            return CodeChartHelper.GetRange(0xAA00, 0xAA5F, 
                i => ((i >= 0xAA37 && i <= 0xAA3F) ||
                    i == 0xAA4E ||
                    i == 0xAA4F ||
                    i == 0xAA5A ||
                    i == 0xAA5B));
        }

        /// <summary>
        /// Provides the safe characters for the Myanmar Extended A code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable MyanmarExtendedA() {
            return CodeChartHelper.GetRange(0xAA60, 0xAA7B);
        }

        /// <summary>
        /// Provides the safe characters for the Myanmar Extended A code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable TaiViet() {
            return CodeChartHelper.GetRange(0xAA80, 0xAAC2).Concat(
                   CodeChartHelper.GetRange(0xAADB, 0xAADF));
        }

        /// <summary>
        /// Provides the safe characters for the Meetei Mayek code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable MeeteiMayek() {
            return CodeChartHelper.GetRange(0xABC0, 0xABF9, 
                i => (i == 0xABEE ||
                    i == 0xABEF));
        }

        /// <summary>
        /// Provides the safe characters for the Hangul Syllables code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable HangulSyllables() {
            return CodeChartHelper.GetRange(0xAC00, 0xD7A3);
        }

        /// <summary>
        /// Provides the safe characters for the Hangul Jamo Extended B code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable HangulJamoExtendedB() {
            return CodeChartHelper.GetRange(0xD7B0, 0xD7FB, 
                i => (i == 0xD7C7 ||
                    i == 0xD7C8 ||
                    i == 0xD7C9 ||
                    i == 0xD7CA));
        }

        /// <summary>
        /// Provides the safe characters for the CJK Compatibility Ideographs code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable CjkCompatibilityIdeographs() {
            return CodeChartHelper.GetRange(0xF900, 0xFAD9, 
                i => (i == 0xFA2E ||
                    i == 0xFA2F ||
                    i == 0xFA6E ||
                    i == 0xFA6F));
        }

        /// <summary>
        /// Provides the safe characters for the Alphabetic Presentation Forms code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable AlphabeticPresentationForms() {
            return CodeChartHelper.GetRange(0xFB00, 0xFB4F, 
                i => ((i >= 0xFB07 && i <= 0xFB12) ||
                    (i >= 0xFB18 && i <= 0xFB1C) ||
                    i == 0xFB37 ||
                    i == 0xFB3D ||
                    i == 0xFB3F ||
                    i == 0xFB42 ||
                    i == 0xFB45));
        }

        /// <summary>
        /// Provides the safe characters for the Arabic Presentation Forms A code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable ArabicPresentationFormsA() {
            return CodeChartHelper.GetRange(0xFB50, 0xFDFD, 
                i => ((i >= 0xFBB2 && i <= 0xFBD2) ||
                    (i >= 0xFD40 && i <= 0xFD4F) ||
                    i == 0xFD90 ||
                    i == 0xFD91 ||
                    (i >= 0xFDC8 && i <= 0xFDEF)));
        }

        /// <summary>
        /// Provides the safe characters for the Variation Selectors code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable VariationSelectors() {
            return CodeChartHelper.GetRange(0xFE00, 0xFE0F);
        }

        /// <summary>
        /// Provides the safe characters for the Vertical Forms code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable VerticalForms() {
            return CodeChartHelper.GetRange(0xFE10, 0xFE19);
        }

        /// <summary>
        /// Provides the safe characters for the Combining Half Marks code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable CombiningHalfMarks() {
            return CodeChartHelper.GetRange(0xFE20, 0xFE26);
        }

        /// <summary>
        /// Provides the safe characters for the CJK Compatibility Forms code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable CjkCompatibilityForms() {
            return CodeChartHelper.GetRange(0xFE30, 0xFE4F);
        }

        /// <summary>
        /// Provides the safe characters for the Small Form Variants code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable SmallFormVariants() {
            return CodeChartHelper.GetRange(0xFE50, 0xFE6B, 
                i => (i == 0xFE53 || i == 0xFE67));
        }

        /// <summary>
        /// Provides the safe characters for the Arabic Presentation Forms B code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable ArabicPresentationFormsB() {
            return CodeChartHelper.GetRange(0xFE70, 0xFEFC, 
                i => (i == 0xFE75));
        }

        /// <summary>
        /// Provides the safe characters for the Half Width and Full Width Forms code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable HalfWidthAndFullWidthForms() {
            return CodeChartHelper.GetRange(0xFF01, 0xFFEE, 
                i => (i == 0xFFBF ||
                    i == 0xFFC0 ||
                    i == 0xFFC1 ||
                    i == 0xFFC8 ||
                    i == 0xFFC9 ||
                    i == 0xFFD0 ||
                    i == 0xFFD1 ||
                    i == 0xFFD8 ||
                    i == 0xFFD9 ||
                    i == 0xFFDD ||
                    i == 0xFFDE ||
                    i == 0xFFDF ||
                    i == 0xFFE7));
        }

        /// <summary>
        /// Provides the safe characters for the Specials code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Specials() {
            return CodeChartHelper.GetRange(0xFFF9, 0xFFFD);
        }
    }
}
