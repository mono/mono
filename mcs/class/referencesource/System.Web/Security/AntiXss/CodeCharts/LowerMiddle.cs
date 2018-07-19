//------------------------------------------------------------------------------
// <copyright file="LowerMiddle.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Security.AntiXss.CodeCharts {
    using System.Collections;

    /// <summary>
    /// Provides safe character positions for the lower middle section of the UTF code tables.
    /// </summary>
    internal static class LowerMiddle {
        /// <summary>
        /// Determines if the specified flag is set.
        /// </summary>
        /// <param name="flags">The value to check.</param>
        /// <param name="flagToCheck">The flag to check for.</param>
        /// <returns>true if the flag is set, otherwise false.</returns>
        public static bool IsFlagSet(LowerMidCodeCharts flags, LowerMidCodeCharts flagToCheck) {
            return (flags & flagToCheck) != 0;
        }

        /// <summary>
        /// Provides the safe characters for the Myanmar code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>
        public static IEnumerable Myanmar() {
            return CodeChartHelper.GetRange(0x1000, 0x109F);
        }

        /// <summary>
        /// Provides the safe characters for the Georgian code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>        
        public static IEnumerable Georgian() {
            return CodeChartHelper.GetRange(0x10A0, 0x10FC, 
                i => (i >= 0x10C6 && i <= 0x10CF));
        }

        /// <summary>
        /// Provides the safe characters for the Hangul Jamo code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>   
        public static IEnumerable HangulJamo() {
            return CodeChartHelper.GetRange(0x1100, 0x11FF);
        }

        /// <summary>
        /// Provides the safe characters for the Ethiopic code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>   
        public static IEnumerable Ethiopic() {
            return CodeChartHelper.GetRange(0x1200, 0x137C, 
                i => (i == 0x1249 ||
                    i == 0x124E ||
                    i == 0x124F ||
                    i == 0x1257 ||
                    i == 0x1259 ||
                    i == 0x125E ||
                    i == 0x125F ||
                    i == 0x1289 ||
                    i == 0x128E ||
                    i == 0x128F ||
                    i == 0x12B1 ||
                    i == 0x12B6 ||
                    i == 0x12B7 ||
                    i == 0x12BF ||
                    i == 0x12C1 ||
                    i == 0x12C6 ||
                    i == 0x12C7 ||
                    i == 0x12D7 ||
                    i == 0x1311 ||
                    i == 0x1316 ||
                    i == 0x1317 ||
                    (i >= 0x135B && i <= 0x135E)));
        }

        /// <summary>
        /// Provides the safe characters for the Ethiopic Supplement code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>   
        public static IEnumerable EthiopicSupplement() {
            return CodeChartHelper.GetRange(0x1380, 0x1399);
        }

        /// <summary>
        /// Provides the safe characters for the Cherokee code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>   
        public static IEnumerable Cherokee() {
            return CodeChartHelper.GetRange(0x13A0, 0x13F4);
        }

        /// <summary>
        /// Provides the safe characters for the Unified Canadian Aboriginal Syllabic code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>  
        public static IEnumerable UnifiedCanadianAboriginalSyllabics() {
            return CodeChartHelper.GetRange(0x1400, 0x167F);
        }

        /// <summary>
        /// Provides the safe characters for the Ogham code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns> 
        public static IEnumerable Ogham() {
            return CodeChartHelper.GetRange(0x1680, 0x169C);
        }

        /// <summary>
        /// Provides the safe characters for the Runic code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns> 
        public static IEnumerable Runic() {
            return CodeChartHelper.GetRange(0x16A0, 0x16F0);
        }

        /// <summary>
        /// Provides the safe characters for the Tagalog code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns> 
        public static IEnumerable Tagalog() {
            return CodeChartHelper.GetRange(0x1700, 0x1714, 
                i => (i == 0x170D));
        }

        /// <summary>
        /// Provides the safe characters for the Hanunoo code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns> 
        public static IEnumerable Hanunoo() {
            return CodeChartHelper.GetRange(0x1720, 0x1736);
        }

        /// <summary>
        /// Provides the safe characters for the Buhid code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns> 
        public static IEnumerable Buhid() {
            return CodeChartHelper.GetRange(0x1740, 0x1753);
        }

        /// <summary>
        /// Provides the safe characters for the Tagbanwa code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns> 
        public static IEnumerable Tagbanwa() {
            return CodeChartHelper.GetRange(0x1760, 0x1773, 
                i => (i == 0x176D ||
                    i == 0x1771));
        }

        /// <summary>
        /// Provides the safe characters for the Khmer code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns> 
        public static IEnumerable Khmer() {
            return CodeChartHelper.GetRange(0x1780, 0x17F9, 
                i => (i == 0x17DE ||
                    i == 0x17DF ||
                    (i >= 0x17EA && i <= 0x17EF)));
        }

        /// <summary>
        /// Provides the safe characters for the Mongolian code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns> 
        public static IEnumerable Mongolian() {
            return CodeChartHelper.GetRange(0x1800, 0x18AA, 
                i => (i == 0x180F ||
                    (i >= 0x181A && i <= 0x181F) ||
                    (i >= 0x1878 && i <= 0x187F)));
        }

        /// <summary>
        /// Provides the safe characters for the Unified Canadian Aboriginal Syllabic Extended code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>  
        public static IEnumerable UnifiedCanadianAboriginalSyllabicsExtended() {
            return CodeChartHelper.GetRange(0x18B0, 0x18F5);
        }

        /// <summary>
        /// Provides the safe characters for the Limbu code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>  
        public static IEnumerable Limbu() {
            return CodeChartHelper.GetRange(0x1900, 0x194F, 
                i => (i == 0x191D ||
                    i == 0x191E ||
                    i == 0x191F ||
                    (i >= 0x192C && i <= 0x192F) ||
                    (i >= 0x193C && i <= 0x193F) ||
                    i == 0x1941 ||
                    i == 0x1942 ||
                    i == 0x1943));
        }

        /// <summary>
        /// Provides the safe characters for the Tai Le code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>  
        public static IEnumerable TaiLe() {
            return CodeChartHelper.GetRange(0x1950, 0x1974, 
                i => (i == 0x196E ||
                    i == 0x196F));
        }

        /// <summary>
        /// Provides the safe characters for the New Tai Lue code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>          
        public static IEnumerable NewTaiLue() {
            return CodeChartHelper.GetRange(0x1980, 0x19DF, 
                i => ((i >= 0x19AC && i <= 0x19AF) ||
                    (i >= 0x19CA && i <= 0x19CF) ||
                    (i >= 0x19DB && i <= 0x19DD)));
        }

        /// <summary>
        /// Provides the safe characters for the Khmer Symbols code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>   
        public static IEnumerable KhmerSymbols() {
            return CodeChartHelper.GetRange(0x19E0, 0x19FF);
        }

        /// <summary>
        /// Provides the safe characters for the Khmer Symbols code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>   
        public static IEnumerable Buginese() {
            return CodeChartHelper.GetRange(0x1A00, 0x1A1F, 
                i => (i == 0x1A1C ||
                    i == 0x1A1D));
        }

        /// <summary>
        /// Provides the safe characters for the Tai Tham code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>   
        public static IEnumerable TaiTham() {
            return CodeChartHelper.GetRange(0x1A20, 0x1AAD, 
                i => (i == 0x1A5F ||
                    i == 0x1A7D ||
                    i == 0x1A7E ||
                    (i >= 0x1A8A && i <= 0x1A8F) ||
                    (i >= 0x1A9A && i <= 0x1A9F)));
        }

        /// <summary>
        /// Provides the safe characters for the Balinese code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>   
        public static IEnumerable Balinese() {
            return CodeChartHelper.GetRange(0x1B00, 0x1B7C, 
                i => (i >= 0x1B4C && i <= 0x1B4F));
        }

        /// <summary>
        /// Provides the safe characters for the Sudanese code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>   
        public static IEnumerable Sudanese() {
            return CodeChartHelper.GetRange(0x1B80, 0x1BB9, 
                i => (i >= 0x1BAB && i <= 0x1BAD));
        }

        /// <summary>
        /// Provides the safe characters for the Lepcha code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>   
        public static IEnumerable Lepcha() {
            return CodeChartHelper.GetRange(0x1C00, 0x1C4F, 
                i => ((i >= 0x1C38 && i <= 0x1C3A) ||
                    (i >= 0x1C4A && i <= 0x1C4C)));
        }

        /// <summary>
        /// Provides the safe characters for the Ol Chiki code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>  
        public static IEnumerable OlChiki() {
            return CodeChartHelper.GetRange(0x1C50, 0x1C7F);
        }

        /// <summary>
        /// Provides the safe characters for the Vedic Extensions code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>  
        public static IEnumerable VedicExtensions() {
            return CodeChartHelper.GetRange(0x1CD0, 0x1CF2);
        }

        /// <summary>
        /// Provides the safe characters for the Phonetic Extensions code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>  
        public static IEnumerable PhoneticExtensions() {
            return CodeChartHelper.GetRange(0x1D00, 0x1D7F);
        }

        /// <summary>
        /// Provides the safe characters for the Phonetic Extensions Supplement code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>  
        public static IEnumerable PhoneticExtensionsSupplement() {
            return CodeChartHelper.GetRange(0x1D80, 0x1DBF);
        }

        /// <summary>
        /// Provides the safe characters for the Combining Diacritical Marks Supplement code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>  
        public static IEnumerable CombiningDiacriticalMarksSupplement() {
            return CodeChartHelper.GetRange(0x1DC0, 0x1DFF, 
                i => (i >= 0x1DE7 && i <= 0x1DFC));
        }

        /// <summary>
        /// Provides the safe characters for the Latin Extended Addition code table.
        /// </summary>
        /// <returns>The safe characters for the code table.</returns>  
        public static IEnumerable LatinExtendedAdditional() {
            return CodeChartHelper.GetRange(0x1E00, 0x1EFF);
        }
    }
}
