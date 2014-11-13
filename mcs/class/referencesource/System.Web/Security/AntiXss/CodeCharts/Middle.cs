//------------------------------------------------------------------------------
// <copyright file="Middle.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Security.AntiXss.CodeCharts {
    using System.Collections;

    /// <summary>
    /// Provides safe character positions for the middle section of the UTF code tables.
    /// </summary>
    internal static class Middle {
        /// <summary>
        /// Determines if the specified flag is set.
        /// </summary>
        /// <param name="flags">The value to check.</param>
        /// <param name="flagToCheck">The flag to check for.</param>
        /// <returns>true if the flag is set, otherwise false.</returns>
        public static bool IsFlagSet(MidCodeCharts flags, MidCodeCharts flagToCheck) {
            return (flags & flagToCheck) != 0;
        }

        /// <summary>
        /// Provides the safe characters for the Greek Extended code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable GreekExtended() {
           
            return CodeChartHelper.GetRange(0x1F00, 0x1FFE, 
                i => (i == 0x1F16 ||
                    i == 0x1F17 ||
                    i == 0x1F1E ||
                    i == 0x1F1F ||
                    i == 0x1F46 ||
                    i == 0x1F47 ||
                    i == 0x1F4E ||
                    i == 0x1F4F ||
                    i == 0x1F58 ||
                    i == 0x1F5A ||
                    i == 0x1F5C ||
                    i == 0x1F5E ||
                    i == 0x1F7E ||
                    i == 0x1F7F ||
                    i == 0x1FB5 ||
                    i == 0x1FC5 ||
                    i == 0x1FD4 ||
                    i == 0x1FD5 ||
                    i == 0x1FDC ||
                    i == 0x1FF0 ||
                    i == 0x1FF1 ||
                    i == 0x1FF5));
        }

        /// <summary>
        /// Provides the safe characters for the General Punctuation code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable GeneralPunctuation() {
            return CodeChartHelper.GetRange(0x2000, 0x206F, 
                i => (i >= 0x2065 && i <= 0x2069));
        }

        /// <summary>
        /// Provides the safe characters for the Superscripts and subscripts code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable SuperscriptsAndSubscripts() {
            return CodeChartHelper.GetRange(0x2070, 0x2094, 
                i => (i == 0x2072 ||
                    i == 0x2073 ||
                    i == 0x208F));
        }

        /// <summary>
        /// Provides the safe characters for the Currency Symbols code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable CurrencySymbols() {
            return CodeChartHelper.GetRange(0x20A0, 0x20B8);
        }

        /// <summary>
        /// Provides the safe characters for the Combining Diacritrical Marks for Symbols code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable CombiningDiacriticalMarksForSymbols() {
            return CodeChartHelper.GetRange(0x20D0, 0x20F0);
        }

        /// <summary>
        /// Provides the safe characters for the Letterlike Symbols code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable LetterlikeSymbols() {
            return CodeChartHelper.GetRange(0x2100, 0x214F);
        }

        /// <summary>
        /// Provides the safe characters for the Number Forms code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable NumberForms() {
            return CodeChartHelper.GetRange(0x2150, 0x2189);
        }

        /// <summary>
        /// Provides the safe characters for the Arrows code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Arrows() {
            return CodeChartHelper.GetRange(0x2190, 0x21FF);
        }

        /// <summary>
        /// Provides the safe characters for the Mathematical Operators code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable MathematicalOperators() {
            return CodeChartHelper.GetRange(0x2200, 0x22FF);
        }

        /// <summary>
        /// Provides the safe characters for the Miscellaneous Technical code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable MiscellaneousTechnical() {
            return CodeChartHelper.GetRange(0x2300, 0x23E8);
        }

        /// <summary>
        /// Provides the safe characters for the Control Pictures code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable ControlPictures() {
            return CodeChartHelper.GetRange(0x2400, 0x2426);
        }

        /// <summary>
        /// Provides the safe characters for the OCR code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable OpticalCharacterRecognition() {
            return CodeChartHelper.GetRange(0x2440, 0x244A);
        }

        /// <summary>
        /// Provides the safe characters for the Enclosed Alphanumerics code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable EnclosedAlphanumerics() {
            return CodeChartHelper.GetRange(0x2460, 0x24FF);
        }

        /// <summary>
        /// Provides the safe characters for the Box Drawing code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable BoxDrawing() {
            return CodeChartHelper.GetRange(0x2500, 0x257F);
        }

        /// <summary>
        /// Provides the safe characters for the Block Elements code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable BlockElements() {
            return CodeChartHelper.GetRange(0x2580, 0x259F);
        }

        /// <summary>
        /// Provides the safe characters for the Geometric Shapes code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable GeometricShapes() {
            return CodeChartHelper.GetRange(0x25A0, 0x25FF);
        }

        /// <summary>
        /// Provides the safe characters for the Miscellaneous Symbols code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable MiscellaneousSymbols() {
            return CodeChartHelper.GetRange(0x2600, 0x26FF, 
                i => (i == 0x26CE ||
                    i == 0x26E2 ||
                    (i >= 0x26E4 && i <= 0x26E7)));
        }

        /// <summary>
        /// Provides the safe characters for the Dingbats code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Dingbats() {
            return CodeChartHelper.GetRange(0x2701, 0x27BE, 
                i => (i == 0x2705 ||
                    i == 0x270A ||
                    i == 0x270B ||
                    i == 0x2728 ||
                    i == 0x274C ||
                    i == 0x274E ||
                    i == 0x2753 ||
                    i == 0x2754 ||
                    i == 0x2755 ||
                    i == 0x275F ||
                    i == 0x2760 ||
                    i == 0x2795 ||
                    i == 0x2796 ||
                    i == 0x2797 ||
                    i == 0x27B0));
        }

        /// <summary>
        /// Provides the safe characters for the Miscellaneous Mathematical Symbols A code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable MiscellaneousMathematicalSymbolsA() {
            return CodeChartHelper.GetRange(0x27C0, 0x27EF, 
                i => (i == 0x27CB ||
                    i == 0x27CD ||
                    i == 0x27CE ||
                    i == 0x27CF));
        }

        /// <summary>
        /// Provides the safe characters for the Supplemental Arrows A code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable SupplementalArrowsA() {
            return CodeChartHelper.GetRange(0x27F0, 0x27FF);
        }

        /// <summary>
        /// Provides the safe characters for the Braille Patterns code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable BraillePatterns() {
            return CodeChartHelper.GetRange(0x2800, 0x28FF);
        }

        /// <summary>
        /// Provides the safe characters for the Supplemental Arrows B code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable SupplementalArrowsB() {
            return CodeChartHelper.GetRange(0x2900, 0x297F);
        }

        /// <summary>
        /// Provides the safe characters for the Miscellaneous Mathematical Symbols B code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable MiscellaneousMathematicalSymbolsB() {
            return CodeChartHelper.GetRange(0x2980, 0x29FF);
        }

        /// <summary>
        /// Provides the safe characters for the Supplemental Mathematical Operators code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable SupplementalMathematicalOperators() {
            return CodeChartHelper.GetRange(0x2A00, 0x2AFF);
        }

        /// <summary>
        /// Provides the safe characters for the Miscellaneous Symbols and Arrows code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable MiscellaneousSymbolsAndArrows() {
            return CodeChartHelper.GetRange(0x2B00, 0x2B59, 
                i => (i == 0x2B4D ||
                    i == 0x2B4E ||
                    i == 0x2B4F));
        }

        /// <summary>
        /// Provides the safe characters for the Glagolitic code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Glagolitic() {
            return CodeChartHelper.GetRange(0x2C00, 0x2C5E, 
                i => (i == 0x2C2F));
        }

        /// <summary>
        /// Provides the safe characters for the Latin Extended C code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable LatinExtendedC() {
            return CodeChartHelper.GetRange(0x2C60, 0x2C7F);
        }

        /// <summary>
        /// Provides the safe characters for the Coptic table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Coptic() {
            return CodeChartHelper.GetRange(0x2C80, 0x2CFF, 
                i => (i >= 0x2CF2 && i <= 0x2CF8));
        }

        /// <summary>
        /// Provides the safe characters for the Georgian Supplement code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable GeorgianSupplement() {
            return CodeChartHelper.GetRange(0x2D00, 0x2D25);
        }

        /// <summary>
        /// Provides the safe characters for the Tifinagh code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Tifinagh() {
            return CodeChartHelper.GetRange(0x2D30, 0x2D6F, 
                i => (i >= 0x2D66 && i <= 0x2D6E));
        }

        /// <summary>
        /// Provides the safe characters for the Ethiopic Extended code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable EthiopicExtended() {
            return CodeChartHelper.GetRange(0x2D80, 0x2DDE, 
                i => ((i >= 0x2D97 && i <= 0x2D9F) ||
                    i == 0x2DA7 ||
                    i == 0x2DAF ||
                    i == 0x2DB7 ||
                    i == 0x2DBF ||
                    i == 0x2DC7 ||
                    i == 0x2DCF ||
                    i == 0x2DD7 ||
                    i == 0x2DDF));
        }
    }
}
