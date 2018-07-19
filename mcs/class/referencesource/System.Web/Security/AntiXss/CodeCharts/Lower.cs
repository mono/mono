//------------------------------------------------------------------------------
// <copyright file="Lower.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Security.AntiXss.CodeCharts {
    using System.Collections;

    /// <summary>
    /// Provides safe character positions for the lower section of the UTF code tables.
    /// </summary>
    internal static class Lower {
        /// <summary>
        /// Determines if the specified flag is set.
        /// </summary>
        /// <param name="flags">The value to check.</param>
        /// <param name="flagToCheck">The flag to check for.</param>
        /// <returns>true if the flag is set, otherwise false.</returns>
        public static bool IsFlagSet(LowerCodeCharts flags, LowerCodeCharts flagToCheck) {
            return (flags & flagToCheck) != 0;
        }

        /// <summary>
        /// Provides the safe characters for the Basic Latin code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable BasicLatin() {
            return CodeChartHelper.GetRange(0x0020, 0x007E);
        }

        /// <summary>
        /// Provides the safe characters for the Latin 1 Supplement code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Latin1Supplement() {
            return CodeChartHelper.GetRange(0x00A1, 0x00FF, 
                i => (i == 0x00AD));
        }

        /// <summary>
        /// Provides the safe characters for the Latin Extended A code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable LatinExtendedA() {
            return CodeChartHelper.GetRange(0x0100, 0x17F);
        }

        /// <summary>
        /// Provides the safe characters for the Latin Extended B code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable LatinExtendedB() {
            return CodeChartHelper.GetRange(0x0180, 0x024F);
        }

        /// <summary>
        /// Provides the safe characters for the IPA Extensions code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable IpaExtensions() {
            return CodeChartHelper.GetRange(0x0250, 0x2AF);
        }

        /// <summary>
        /// Provides the safe characters for the Spacing Modifiers code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable SpacingModifierLetters() {
            return CodeChartHelper.GetRange(0x02B0, 0x2FF);
        }

        /// <summary>
        /// Provides the safe characters for the Combining Diacritical Marks code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable CombiningDiacriticalMarks() {
            return CodeChartHelper.GetRange(0x0300, 0x36F);
        }

        /// <summary>
        /// Provides the safe characters for the Greek and Coptic code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable GreekAndCoptic() {
            return CodeChartHelper.GetRange(0x0370, 0x03FF, 
                i => (i == 0x378 ||
                    i == 0x379 ||
                    (i >= 0x37F && i <= 0x383) ||
                    i == 0x38B ||
                    i == 0x38D ||
                    i == 0x3A2));
        }

        /// <summary>
        /// Provides the safe characters for the Cyrillic code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Cyrillic() {
            return CodeChartHelper.GetRange(0x0400, 0x04FF);
        }

        /// <summary>
        /// Provides the safe characters for the Cyrillic Supplement code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable CyrillicSupplement() {
            return CodeChartHelper.GetRange(0x0500, 0x0525);
        }

        /// <summary>
        /// Provides the safe characters for the Armenian code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>        
        public static IEnumerable Armenian() {
            return CodeChartHelper.GetRange(0x0531, 0x058A, 
                i => (i == 0x0557 ||
                    i == 0x0558 ||
                    i == 0x0560 ||
                    i == 0x0588));
        }

        /// <summary>
        /// Provides the safe characters for the Hebrew code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Hebrew() {
            return CodeChartHelper.GetRange(0x0591, 0x05F4, 
                i => ((i >= 0x05C8 && i <= 0x05CF) ||
                    (i >= 0x05EB && i <= 0x05EF)));
        }

        /// <summary>
        /// Provides the safe characters for the Arabic code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Arabic() {
            return CodeChartHelper.GetRange(0x0600, 0x06FF, 
                i => (i == 0x0604 ||
                    i == 0x0605 ||
                    i == 0x061C ||
                    i == 0x061d ||
                    i == 0x0620 ||
                    i == 0x065F));
        }

        /// <summary>
        /// Provides the safe characters for the Syriac code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Syriac() {
            return CodeChartHelper.GetRange(0x0700, 0x074F, 
                i => (i == 0x070E ||
                    i == 0x074B ||
                    i == 0x074C));
        }

        /// <summary>
        /// Provides the safe characters for the Arabic Supplement code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable ArabicSupplement() {
            return CodeChartHelper.GetRange(0x0750, 0x077F);
        }

        /// <summary>
        /// Provides the safe characters for the Thaana code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Thaana() {
            return CodeChartHelper.GetRange(0x0780, 0x07B1);
        }

        /// <summary>
        /// Provides the safe characters for the Nko code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Nko() {
            return CodeChartHelper.GetRange(0x07C0, 0x07FA);
        }

        /// <summary>
        /// Provides the safe characters for the Samaritan code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Samaritan() {
            return CodeChartHelper.GetRange(0x0800, 0x083E, 
                i => (i == 0x082E ||
                    i == 0x082F));
        }

        /// <summary>
        /// Provides the safe characters for the Devenagari code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Devanagari() {
            return CodeChartHelper.GetRange(0x0900, 0x097F, 
                i => (i == 0x093A ||
                    i == 0x093B ||
                    i == 0x094F ||
                    i == 0x0956 ||
                    i == 0x0957 ||
                    (i >= 0x0973 && i <= 0x0978)));
        }

        /// <summary>
        /// Provides the safe characters for the Bengali code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Bengali() {
            return CodeChartHelper.GetRange(0x0981, 0x09FB, 
                i => (i == 0x0984 ||
                    i == 0x098D ||
                    i == 0x098E ||
                    i == 0x0991 ||
                    i == 0x0992 ||
                    i == 0x09A9 ||
                    i == 0x09B1 ||
                    i == 0x09B3 ||
                    i == 0x09B4 ||
                    i == 0x09B5 ||
                    i == 0x09BA ||
                    i == 0x09BB ||
                    i == 0x09C5 ||
                    i == 0x09C6 ||
                    i == 0x09C9 ||
                    i == 0x09CA ||
                    (i >= 0x09CF && i <= 0x09D6) ||
                    (i >= 0x09D8 && i <= 0x09DB) ||
                    i == 0x09DE ||
                    i == 0x09E4 ||
                    i == 0x09E5));
        }

        /// <summary>
        /// Provides the safe characters for the Gurmukhi code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Gurmukhi() {
            return CodeChartHelper.GetRange(0x0A01, 0x0A75, 
                i => (i == 0x0A04 ||
                    (i >= 0x0A0B && i <= 0x0A0E) ||
                    i == 0x0A11 ||
                    i == 0x0A12 ||
                    i == 0x0A29 ||
                    i == 0x0A31 ||
                    i == 0x0A34 ||
                    i == 0x0A37 ||
                    i == 0x0A3A ||
                    i == 0x0A3B ||
                    i == 0x0A3D ||
                    (i >= 0x0A43 && i <= 0x0A46) ||
                    i == 0x0A49 ||
                    i == 0x0A4A ||
                    (i >= 0x0A4E && i <= 0x0A50) ||
                    (i >= 0x0A52 && i <= 0x0A58) ||
                    i == 0x0A5D ||
                    (i >= 0x0A5F && i <= 0x0A65)));
        }

        /// <summary>
        /// Provides the safe characters for the Gujarati code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Gujarati() {
            return CodeChartHelper.GetRange(0x0A81, 0x0AF1, 
                i => (i == 0x0A84 ||
                    i == 0x0A8E ||
                    i == 0x0A92 ||
                    i == 0x0AA9 ||
                    i == 0x0AB1 ||
                    i == 0x0AB4 ||
                    i == 0x0ABA ||
                    i == 0x0ABB ||
                    i == 0x0AC6 ||
                    i == 0x0ACA ||
                    i == 0x0ACE ||
                    i == 0x0ACF ||
                    (i >= 0xAD1 && i <= 0x0ADF) ||
                    i == 0x0AE4 ||
                    i == 0x0AE5 ||
                    i == 0x0AF0));
        }

        /// <summary>
        /// Provides the safe characters for the Oriya code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Oriya() {
            return CodeChartHelper.GetRange(0x0B01, 0x0B71, 
                i => (i == 0x0B04 ||
                    i == 0x0B0D ||
                    i == 0x0B0E ||
                    i == 0x0B11 ||
                    i == 0x0B12 ||
                    i == 0x0B29 ||
                    i == 0x0B31 ||
                    i == 0x0B34 ||
                    i == 0x0B3A ||
                    i == 0x0B3B ||
                    i == 0x0B45 ||
                    i == 0x0B46 ||
                    i == 0x0B49 ||
                    i == 0x0B4A ||
                    (i >= 0x0B4E && i <= 0x0B55) ||
                    (i >= 0x0B58 && i <= 0x0B5B) ||
                    i == 0x0B5E ||
                    i == 0x0B64 ||
                    i == 0x0B65));
        }

        /// <summary>
        /// Provides the safe characters for the Tamil code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Tamil() {
            return CodeChartHelper.GetRange(0x0B82, 0x0BFA, 
                i => (i == 0x0B84 ||
                    i == 0x0B8B ||
                    i == 0x0B8C ||
                    i == 0x0B8D ||
                    i == 0x0B91 ||
                    i == 0x0B96 ||
                    i == 0x0B97 ||
                    i == 0x0B98 ||
                    i == 0x0B9B ||
                    i == 0x0B9D ||
                    i == 0x0BA0 ||
                    i == 0x0BA1 ||
                    i == 0x0BA2 ||
                    i == 0x0BA5 ||
                    i == 0x0BA6 ||
                    i == 0x0BA7 ||
                    i == 0x0BAB ||
                    i == 0x0BAC ||
                    i == 0x0BAD ||
                    (i >= 0x0BBA && i <= 0x0BBD) ||
                    i == 0x0BC3 ||
                    i == 0x0BC4 ||
                    i == 0x0BC5 ||
                    i == 0x0BC9 ||
                    i == 0x0BCE ||
                    i == 0x0BCF ||
                    (i >= 0x0BD1 && i <= 0x0BD6) ||
                    (i >= 0x0BD8 && i <= 0x0BE5)));
        }

        /// <summary>
        /// Provides the safe characters for the Telugu code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Telugu() {
            return CodeChartHelper.GetRange(0x0C01, 0x0C7F, 
                i => (i == 0x0C04 ||
                    i == 0x0C0D ||
                    i == 0x0C11 ||
                    i == 0x0C29 ||
                    i == 0x0C34 ||
                    i == 0x0C3A ||
                    i == 0x0C3B ||
                    i == 0x0C3C ||
                    i == 0x0C45 ||
                    i == 0x0C49 ||
                    (i >= 0x0C4E && i <= 0x0C54) ||
                    i == 0x0C57 ||
                    (i >= 0x0C5A && i <= 0x0C5F) ||
                    i == 0x0C64 ||
                    i == 0x0C65 ||
                    (i >= 0x0C70 && i <= 0x0C77)));
        }

        /// <summary>
        /// Provides the safe characters for the Kannada code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>        
        public static IEnumerable Kannada() {
            return CodeChartHelper.GetRange(0x0C82, 0x0CF2, 
                i => (i == 0x0C84 ||
                    i == 0x0C8D ||
                    i == 0x0C91 ||
                    i == 0x0CA9 ||
                    i == 0x0CB4 ||
                    i == 0x0CBA ||
                    i == 0x0CBB ||
                    i == 0x0CC5 ||
                    i == 0x0CC9 ||
                    (i >= 0x0CCE && i <= 0x0CD4) ||
                    (i >= 0x0CD7 && i <= 0x0CDD) ||
                    i == 0x0CDF ||
                    i == 0x0CE4 ||
                    i == 0x0CE5 ||
                    i == 0x0CF0));
        }

        /// <summary>
        /// Provides the safe characters for the Malayalam code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Malayalam() {
            return CodeChartHelper.GetRange(0x0D02, 0x0D7F, 
                i => (i == 0x0D04 ||
                    i == 0x0D0D ||
                    i == 0x0D11 ||
                    i == 0x0D29 ||
                    i == 0x0D3A ||
                    i == 0x0D3B ||
                    i == 0x0D3C ||
                    i == 0x0D45 ||
                    i == 0x0D49 ||
                    (i >= 0x0D4E && i <= 0x0D56) ||
                    (i >= 0x0D58 && i <= 0x0D5F) ||
                    i == 0x0D64 ||
                    i == 0x0D65 ||
                    i == 0x0D76 ||
                    i == 0x0D77 ||
                    i == 0x0D78));
        }

        /// <summary>
        /// Provides the safe characters for the Sinhala code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Sinhala() {
            return CodeChartHelper.GetRange(0x0D82, 0x0DF4, 
                i => (i == 0x0D84 ||
                    i == 0x0D97 ||
                    i == 0X0D98 ||
                    i == 0x0D99 ||
                    i == 0x0DB2 ||
                    i == 0x0DBC ||
                    i == 0x0DBE ||
                    i == 0x0DBF ||
                    i == 0x0DC7 ||
                    i == 0x0DC8 ||
                    i == 0x0DC9 ||
                    (i >= 0x0DCB && i <= 0x0DCE) ||
                    i == 0x0DD5 ||
                    i == 0x0DD7 ||
                    (i >= 0x0DE0 && i <= 0x0DF1)));
        }

        /// <summary>
        /// Provides the safe characters for the Thai code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Thai() {
            return CodeChartHelper.GetRange(0x0E01, 0x0E5B, 
                i => (i >= 0x0E3B && i <= 0x0E3E));
        }

        /// <summary>
        /// Provides the safe characters for the Lao code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Lao() {
            return CodeChartHelper.GetRange(0x0E81, 0x0EDD, 
                i => (i == 0x0E83 ||
                    i == 0x0E85 ||
                    i == 0x0E86 ||
                    i == 0x0E89 ||
                    i == 0x0E8B ||
                    i == 0x0E8C ||
                    (i >= 0x0E8E && i <= 0x0E93) ||
                    i == 0x0E98 ||
                    i == 0x0EA0 ||
                    i == 0x0EA4 ||
                    i == 0x0EA6 ||
                    i == 0x0EA8 ||
                    i == 0x0EA9 ||
                    i == 0x0EAC ||
                    i == 0x0EBA ||
                    i == 0x0EBE ||
                    i == 0x0EBF ||
                    i == 0x0EC5 ||
                    i == 0x0EC7 ||
                    i == 0x0ECE ||
                    i == 0x0ECF ||
                    i == 0x0EDA ||
                    i == 0x0EDB));
        }

        /// <summary>
        /// Provides the safe characters for the Tibetan code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Tibetan() {
            return CodeChartHelper.GetRange(0x0F00, 0x0FD8, 
                i => (i == 0x0F48 ||
                    (i >= 0x0F6D && i <= 0x0F70) ||
                    (i >= 0x0F8C && i <= 0x0F8F) ||
                    i == 0x0F98 ||
                    i == 0x0FBD ||
                    i == 0x0FCD));
        }
    }
}
