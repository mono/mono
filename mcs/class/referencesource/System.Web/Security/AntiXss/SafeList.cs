//------------------------------------------------------------------------------
// <copyright file="SafeList.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Security.AntiXss {
    using System.Collections;
    using System.Globalization;

    /// <summary>
    /// Provides safe list utility functions.
    /// </summary>
    internal static class SafeList {
        /// <summary>
        /// Generates a safe character array representing the specified value.
        /// </summary>
        /// <returns>A safe character array representing the specified value.</returns>
        /// <param name="value">The value to generate a safe representation for.</param>
        internal delegate char[] GenerateSafeValue(int value);

        /// <summary>
        /// Generates a new safe list of the specified size, using the specified function to produce safe values.
        /// </summary>
        /// <param name="length">The length of the safe list to generate.</param>
        /// <param name="generateSafeValue">The <see cref="GenerateSafeValue"/> function to use.</param>
        /// <returns>A new safe list.</returns>
        internal static char[][] Generate(int length, GenerateSafeValue generateSafeValue) {
            char[][] allCharacters = new char[length + 1][];
            for (int i = 0; i <= length; i++) {
                allCharacters[i] = generateSafeValue(i);
            }

            return allCharacters;
        }

        /// <summary>
        /// Marks characters from the specified languages as safe.
        /// </summary>
        /// <param name="safeList">The safe list to punch holes in.</param>
        /// <param name="lowerCodeCharts">The combination of lower code charts to use.</param>
        /// <param name="lowerMidCodeCharts">The combination of lower mid code charts to use.</param>
        /// <param name="midCodeCharts">The combination of mid code charts to use.</param>
        /// <param name="upperMidCodeCharts">The combination of upper mid code charts to use.</param>
        /// <param name="upperCodeCharts">The combination of upper code charts to use.</param>
        internal static void PunchUnicodeThrough(
            ref char[][] safeList,
            LowerCodeCharts lowerCodeCharts,
            LowerMidCodeCharts lowerMidCodeCharts,
            MidCodeCharts midCodeCharts,
            UpperMidCodeCharts upperMidCodeCharts,
            UpperCodeCharts upperCodeCharts) {
            if (lowerCodeCharts != LowerCodeCharts.None) {
                PunchCodeCharts(ref safeList, lowerCodeCharts);
            }

            if (lowerMidCodeCharts != LowerMidCodeCharts.None) {
                PunchCodeCharts(ref safeList, lowerMidCodeCharts);
            }

            if (midCodeCharts != MidCodeCharts.None) {
                PunchCodeCharts(ref safeList, midCodeCharts);
            }

            if (upperMidCodeCharts != UpperMidCodeCharts.None) {
                PunchCodeCharts(ref safeList, upperMidCodeCharts);
            }

            if (upperCodeCharts != UpperCodeCharts.None) {
                PunchCodeCharts(ref safeList, upperCodeCharts);
            }
        }

        /// <summary>
        /// Punches holes as necessary.
        /// </summary>
        /// <param name="safeList">The safe list to punch through.</param>
        /// <param name="----edCharacters">The list of character positions to punch.</param>
        internal static void PunchSafeList(ref char[][] safeList, IEnumerable whiteListedCharacters) {
            PunchHolesIfNeeded(ref safeList, true, whiteListedCharacters);
        }

        /// <summary>
        /// Generates a hash prefixed character array representing the specified value.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <returns>A character array representing the specified value.</returns>
        /// <remarks>
        /// Example inputs and encoded outputs:
        /// <list type="table">
        /// <item><term>1</term><description>#1</description></item>
        /// <item><term>10</term><description>#10</description></item>
        /// <item><term>100</term><description>#100</description></item>
        /// </list>
        /// </remarks>
        internal static char[] HashThenValueGenerator(int value) {
            return StringToCharArrayWithHashPrefix(value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Generates a hash prefixed character array representing the specified value in hexadecimal.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <returns>A character array representing the specified value.</returns>
        /// <remarks>
        /// Example inputs and encoded outputs:
        /// <list type="table">
        /// <item><term>1</term><description>#1</description></item>
        /// <item><term>10</term><description>#0a</description></item>
        /// <item><term>100</term><description>#64</description></item>
        /// </list>
        /// </remarks>
        internal static char[] HashThenHexValueGenerator(int value) {
            return StringToCharArrayWithHashPrefix(value.ToString("x2", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Generates a percent prefixed character array representing the specified value in hexadecimal.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <returns>A character array representing the specified value.</returns>
        /// <remarks>
        /// Example inputs and encoded outputs:
        /// <list type="table">
        /// <item><term>1</term><description>%01</description></item>
        /// <item><term>10</term><description>%0A</description></item>
        /// <item><term>100</term><description>%64</description></item>
        /// </list>
        /// </remarks>
        internal static char[] PercentThenHexValueGenerator(int value) {
            return StringToCharArrayWithPercentPrefix(value.ToString("X2", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Generates a slash prefixed character array representing the specified value in hexadecimal.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <returns>A character array representing the specified value.</returns>
        /// <remarks>
        /// Example inputs and encoded outputs:
        /// <list type="table">
        /// <item><term>1</term><description>\01</description></item>
        /// <item><term>10</term><description>\0a</description></item>
        /// <item><term>100</term><description>\64</description></item>
        /// </list>
        /// </remarks>
        internal static char[] SlashThenHexValueGenerator(int value) {
            return StringToCharArrayWithSlashPrefix(value.ToString("x2", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Generates a slash prefixed character array representing the specified value in hexadecimal.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <returns>A character array representing the specified value.</returns>
        /// <remarks>
        /// Example inputs and encoded outputs:
        /// <list type="table">
        /// <item><term>1</term><description>\000001</description></item>
        /// <item><term>10</term><description>\000000A</description></item>
        /// <item><term>100</term><description>\000064</description></item>
        /// </list>
        /// </remarks>
        internal static char[] SlashThenSixDigitHexValueGenerator(long value) {
            return StringToCharArrayWithSlashPrefix(value.ToString("X6", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Generates a slash prefixed character array representing the specified value in hexadecimal.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <returns>A character array representing the specified value.</returns>
        /// <remarks>
        /// Example inputs and encoded outputs:
        /// <list type="table">
        /// <item><term>1</term><description>\000001</description></item>
        /// <item><term>10</term><description>\000000A</description></item>
        /// <item><term>100</term><description>\000064</description></item>
        /// </list>
        /// </remarks>
        internal static char[] SlashThenSixDigitHexValueGenerator(int value) {
            return StringToCharArrayWithSlashPrefix(value.ToString("X6", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Generates a hash prefixed character array representing the specified value.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <returns>A character array representing the specified value.</returns>
        /// <remarks>
        /// Example inputs and encoded outputs:
        /// <list type="table">
        /// <item><term>1</term><description>#1</description></item>
        /// <item><term>10</term><description>#10</description></item>
        /// <item><term>100</term><description>#100</description></item>
        /// </list>
        /// </remarks>
        internal static char[] HashThenValueGenerator(long value) {
            return StringToCharArrayWithHashPrefix(value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Generates a hash prefixed character array from the specified string.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <returns>A character array representing the specified value.</returns>
        /// <remarks>
        /// Example inputs and encoded outputs:
        /// <list type="table">
        /// <item><term>1</term><description>#1</description></item>
        /// <item><term>10</term><description>#10</description></item>
        /// <item><term>100</term><description>#100</description></item>
        /// </list>
        /// </remarks>
        private static char[] StringToCharArrayWithHashPrefix(string value) {
            return StringToCharArrayWithPrefix(value, '#');
        }

        /// <summary>
        /// Generates a percent prefixed character array from the specified string.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <returns>A character array representing the specified value.</returns>
        /// <remarks>
        /// Example inputs and encoded outputs:
        /// <list type="table">
        /// <item><term>1</term><description>%1</description></item>
        /// <item><term>10</term><description>%10</description></item>
        /// <item><term>100</term><description>%100</description></item>
        /// </list>
        /// </remarks>
        private static char[] StringToCharArrayWithPercentPrefix(string value) {
            return StringToCharArrayWithPrefix(value, '%');
        }

        /// <summary>
        /// Generates a slash prefixed character array from the specified string.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <returns>A character array representing the specified value.</returns>
        /// <remarks>
        /// Example inputs and encoded outputs:
        /// <list type="table">
        /// <item><term>1</term><description>\1</description></item>
        /// <item><term>10</term><description>\10</description></item>
        /// <item><term>100</term><description>\100</description></item>
        /// </list>
        /// </remarks>
        private static char[] StringToCharArrayWithSlashPrefix(string value) {
            return StringToCharArrayWithPrefix(value, '\\');
        }

        /// <summary>
        /// Generates a prefixed character array from the specified string and prefix.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <param name="prefix">The prefix to use.</param>
        /// <returns>A prefixed character array representing the specified value.</returns>
        private static char[] StringToCharArrayWithPrefix(string value, char prefix) {
            int valueAsStringLength = value.Length;

            char[] valueAsCharArray = new char[valueAsStringLength + 1];
            valueAsCharArray[0] = prefix;
            for (int j = 0; j < valueAsStringLength; j++) {
                valueAsCharArray[j + 1] = value[j];
            }

            return valueAsCharArray;
        }

        /// <summary>
        /// Punch appropriate holes for the selected code charts.
        /// </summary>
        /// <param name="safeList">The safe list to punch through.</param>
        /// <param name="codeCharts">The code charts to punch.</param>
        private static void PunchCodeCharts(ref char[][] safeList, LowerCodeCharts codeCharts) {
            PunchHolesIfNeeded(ref safeList, CodeCharts.Lower.IsFlagSet(codeCharts, LowerCodeCharts.BasicLatin), CodeCharts.Lower.BasicLatin());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Lower.IsFlagSet(codeCharts, LowerCodeCharts.C1ControlsAndLatin1Supplement), CodeCharts.Lower.Latin1Supplement());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Lower.IsFlagSet(codeCharts, LowerCodeCharts.LatinExtendedA), CodeCharts.Lower.LatinExtendedA());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Lower.IsFlagSet(codeCharts, LowerCodeCharts.LatinExtendedB), CodeCharts.Lower.LatinExtendedB());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Lower.IsFlagSet(codeCharts, LowerCodeCharts.IpaExtensions), CodeCharts.Lower.IpaExtensions());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Lower.IsFlagSet(codeCharts, LowerCodeCharts.SpacingModifierLetters), CodeCharts.Lower.SpacingModifierLetters());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Lower.IsFlagSet(codeCharts, LowerCodeCharts.CombiningDiacriticalMarks), CodeCharts.Lower.CombiningDiacriticalMarks());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Lower.IsFlagSet(codeCharts, LowerCodeCharts.GreekAndCoptic), CodeCharts.Lower.GreekAndCoptic());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Lower.IsFlagSet(codeCharts, LowerCodeCharts.Cyrillic), CodeCharts.Lower.Cyrillic());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Lower.IsFlagSet(codeCharts, LowerCodeCharts.CyrillicSupplement), CodeCharts.Lower.CyrillicSupplement());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Lower.IsFlagSet(codeCharts, LowerCodeCharts.Armenian), CodeCharts.Lower.Armenian());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Lower.IsFlagSet(codeCharts, LowerCodeCharts.Hebrew), CodeCharts.Lower.Hebrew());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Lower.IsFlagSet(codeCharts, LowerCodeCharts.Arabic), CodeCharts.Lower.Arabic());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Lower.IsFlagSet(codeCharts, LowerCodeCharts.Syriac), CodeCharts.Lower.Syriac());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Lower.IsFlagSet(codeCharts, LowerCodeCharts.ArabicSupplement), CodeCharts.Lower.ArabicSupplement());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Lower.IsFlagSet(codeCharts, LowerCodeCharts.Thaana), CodeCharts.Lower.Thaana());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Lower.IsFlagSet(codeCharts, LowerCodeCharts.Nko), CodeCharts.Lower.Nko());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Lower.IsFlagSet(codeCharts, LowerCodeCharts.Samaritan), CodeCharts.Lower.Samaritan());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Lower.IsFlagSet(codeCharts, LowerCodeCharts.Devanagari), CodeCharts.Lower.Devanagari());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Lower.IsFlagSet(codeCharts, LowerCodeCharts.Bengali), CodeCharts.Lower.Bengali());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Lower.IsFlagSet(codeCharts, LowerCodeCharts.Gurmukhi), CodeCharts.Lower.Gurmukhi());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Lower.IsFlagSet(codeCharts, LowerCodeCharts.Gujarati), CodeCharts.Lower.Gujarati());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Lower.IsFlagSet(codeCharts, LowerCodeCharts.Oriya), CodeCharts.Lower.Oriya());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Lower.IsFlagSet(codeCharts, LowerCodeCharts.Tamil), CodeCharts.Lower.Tamil());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Lower.IsFlagSet(codeCharts, LowerCodeCharts.Telugu), CodeCharts.Lower.Telugu());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Lower.IsFlagSet(codeCharts, LowerCodeCharts.Kannada), CodeCharts.Lower.Kannada());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Lower.IsFlagSet(codeCharts, LowerCodeCharts.Malayalam), CodeCharts.Lower.Malayalam());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Lower.IsFlagSet(codeCharts, LowerCodeCharts.Sinhala), CodeCharts.Lower.Sinhala());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Lower.IsFlagSet(codeCharts, LowerCodeCharts.Thai), CodeCharts.Lower.Thai());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Lower.IsFlagSet(codeCharts, LowerCodeCharts.Lao), CodeCharts.Lower.Lao());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Lower.IsFlagSet(codeCharts, LowerCodeCharts.Tibetan), CodeCharts.Lower.Tibetan());
        }

        /// <summary>
        /// Punch appropriate holes for the selected code charts.
        /// </summary>
        /// <param name="safeList">The safe list to punch through.</param>
        /// <param name="codeCharts">The code charts to punch.</param>
        private static void PunchCodeCharts(ref char[][] safeList, LowerMidCodeCharts codeCharts) {
            PunchHolesIfNeeded(ref safeList, CodeCharts.LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.Myanmar), CodeCharts.LowerMiddle.Myanmar());
            PunchHolesIfNeeded(ref safeList, CodeCharts.LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.Georgian), CodeCharts.LowerMiddle.Georgian());
            PunchHolesIfNeeded(ref safeList, CodeCharts.LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.HangulJamo), CodeCharts.LowerMiddle.HangulJamo());
            PunchHolesIfNeeded(ref safeList, CodeCharts.LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.Ethiopic), CodeCharts.LowerMiddle.Ethiopic());
            PunchHolesIfNeeded(ref safeList, CodeCharts.LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.EthiopicSupplement), CodeCharts.LowerMiddle.EthiopicSupplement());
            PunchHolesIfNeeded(ref safeList, CodeCharts.LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.Cherokee), CodeCharts.LowerMiddle.Cherokee());
            PunchHolesIfNeeded(ref safeList, CodeCharts.LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.UnifiedCanadianAboriginalSyllabics), CodeCharts.LowerMiddle.UnifiedCanadianAboriginalSyllabics());
            PunchHolesIfNeeded(ref safeList, CodeCharts.LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.Ogham), CodeCharts.LowerMiddle.Ogham());
            PunchHolesIfNeeded(ref safeList, CodeCharts.LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.Runic), CodeCharts.LowerMiddle.Runic());
            PunchHolesIfNeeded(ref safeList, CodeCharts.LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.Tagalog), CodeCharts.LowerMiddle.Tagalog());
            PunchHolesIfNeeded(ref safeList, CodeCharts.LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.Hanunoo), CodeCharts.LowerMiddle.Hanunoo());
            PunchHolesIfNeeded(ref safeList, CodeCharts.LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.Buhid), CodeCharts.LowerMiddle.Buhid());
            PunchHolesIfNeeded(ref safeList, CodeCharts.LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.Tagbanwa), CodeCharts.LowerMiddle.Tagbanwa());
            PunchHolesIfNeeded(ref safeList, CodeCharts.LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.Khmer), CodeCharts.LowerMiddle.Khmer());
            PunchHolesIfNeeded(ref safeList, CodeCharts.LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.Mongolian), CodeCharts.LowerMiddle.Mongolian());
            PunchHolesIfNeeded(ref safeList, CodeCharts.LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.UnifiedCanadianAboriginalSyllabicsExtended), CodeCharts.LowerMiddle.UnifiedCanadianAboriginalSyllabicsExtended());
            PunchHolesIfNeeded(ref safeList, CodeCharts.LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.Limbu), CodeCharts.LowerMiddle.Limbu());
            PunchHolesIfNeeded(ref safeList, CodeCharts.LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.TaiLe), CodeCharts.LowerMiddle.TaiLe());
            PunchHolesIfNeeded(ref safeList, CodeCharts.LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.NewTaiLue), CodeCharts.LowerMiddle.NewTaiLue());
            PunchHolesIfNeeded(ref safeList, CodeCharts.LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.KhmerSymbols), CodeCharts.LowerMiddle.KhmerSymbols());
            PunchHolesIfNeeded(ref safeList, CodeCharts.LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.Buginese), CodeCharts.LowerMiddle.Buginese());
            PunchHolesIfNeeded(ref safeList, CodeCharts.LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.TaiTham), CodeCharts.LowerMiddle.TaiTham());
            PunchHolesIfNeeded(ref safeList, CodeCharts.LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.Balinese), CodeCharts.LowerMiddle.Balinese());
            PunchHolesIfNeeded(ref safeList, CodeCharts.LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.Sudanese), CodeCharts.LowerMiddle.Sudanese());
            PunchHolesIfNeeded(ref safeList, CodeCharts.LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.Lepcha), CodeCharts.LowerMiddle.Lepcha());
            PunchHolesIfNeeded(ref safeList, CodeCharts.LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.OlChiki), CodeCharts.LowerMiddle.OlChiki());
            PunchHolesIfNeeded(ref safeList, CodeCharts.LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.VedicExtensions), CodeCharts.LowerMiddle.VedicExtensions());
            PunchHolesIfNeeded(ref safeList, CodeCharts.LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.PhoneticExtensions), CodeCharts.LowerMiddle.PhoneticExtensions());
            PunchHolesIfNeeded(ref safeList, CodeCharts.LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.PhoneticExtensionsSupplement), CodeCharts.LowerMiddle.PhoneticExtensionsSupplement());
            PunchHolesIfNeeded(ref safeList, CodeCharts.LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.CombiningDiacriticalMarksSupplement), CodeCharts.LowerMiddle.CombiningDiacriticalMarksSupplement());
            PunchHolesIfNeeded(ref safeList, CodeCharts.LowerMiddle.IsFlagSet(codeCharts, LowerMidCodeCharts.LatinExtendedAdditional), CodeCharts.LowerMiddle.LatinExtendedAdditional());
        }

        /// <summary>
        /// Punch appropriate holes for the selected code charts.
        /// </summary>
        /// <param name="safeList">The safe list to punch through.</param>
        /// <param name="codeCharts">The code charts to punch.</param>
        private static void PunchCodeCharts(ref char[][] safeList, MidCodeCharts codeCharts) {
            PunchHolesIfNeeded(ref safeList, CodeCharts.Middle.IsFlagSet(codeCharts, MidCodeCharts.GreekExtended), CodeCharts.Middle.GreekExtended());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Middle.IsFlagSet(codeCharts, MidCodeCharts.GeneralPunctuation), CodeCharts.Middle.GeneralPunctuation());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Middle.IsFlagSet(codeCharts, MidCodeCharts.SuperscriptsAndSubscripts), CodeCharts.Middle.SuperscriptsAndSubscripts());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Middle.IsFlagSet(codeCharts, MidCodeCharts.CurrencySymbols), CodeCharts.Middle.CurrencySymbols());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Middle.IsFlagSet(codeCharts, MidCodeCharts.CombiningDiacriticalMarksForSymbols), CodeCharts.Middle.CombiningDiacriticalMarksForSymbols());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Middle.IsFlagSet(codeCharts, MidCodeCharts.LetterlikeSymbols), CodeCharts.Middle.LetterlikeSymbols());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Middle.IsFlagSet(codeCharts, MidCodeCharts.NumberForms), CodeCharts.Middle.NumberForms());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Middle.IsFlagSet(codeCharts, MidCodeCharts.Arrows), CodeCharts.Middle.Arrows());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Middle.IsFlagSet(codeCharts, MidCodeCharts.MathematicalOperators), CodeCharts.Middle.MathematicalOperators());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Middle.IsFlagSet(codeCharts, MidCodeCharts.MiscellaneousTechnical), CodeCharts.Middle.MiscellaneousTechnical());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Middle.IsFlagSet(codeCharts, MidCodeCharts.ControlPictures), CodeCharts.Middle.ControlPictures());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Middle.IsFlagSet(codeCharts, MidCodeCharts.OpticalCharacterRecognition), CodeCharts.Middle.OpticalCharacterRecognition());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Middle.IsFlagSet(codeCharts, MidCodeCharts.EnclosedAlphanumerics), CodeCharts.Middle.EnclosedAlphanumerics());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Middle.IsFlagSet(codeCharts, MidCodeCharts.BoxDrawing), CodeCharts.Middle.BoxDrawing());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Middle.IsFlagSet(codeCharts, MidCodeCharts.BlockElements), CodeCharts.Middle.BlockElements());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Middle.IsFlagSet(codeCharts, MidCodeCharts.GeometricShapes), CodeCharts.Middle.GeometricShapes());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Middle.IsFlagSet(codeCharts, MidCodeCharts.MiscellaneousSymbols), CodeCharts.Middle.MiscellaneousSymbols());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Middle.IsFlagSet(codeCharts, MidCodeCharts.Dingbats), CodeCharts.Middle.Dingbats());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Middle.IsFlagSet(codeCharts, MidCodeCharts.MiscellaneousMathematicalSymbolsA), CodeCharts.Middle.MiscellaneousMathematicalSymbolsA());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Middle.IsFlagSet(codeCharts, MidCodeCharts.SupplementalArrowsA), CodeCharts.Middle.SupplementalArrowsA());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Middle.IsFlagSet(codeCharts, MidCodeCharts.BraillePatterns), CodeCharts.Middle.BraillePatterns());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Middle.IsFlagSet(codeCharts, MidCodeCharts.SupplementalArrowsB), CodeCharts.Middle.SupplementalArrowsB());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Middle.IsFlagSet(codeCharts, MidCodeCharts.MiscellaneousMathematicalSymbolsB), CodeCharts.Middle.MiscellaneousMathematicalSymbolsB());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Middle.IsFlagSet(codeCharts, MidCodeCharts.SupplementalMathematicalOperators), CodeCharts.Middle.SupplementalMathematicalOperators());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Middle.IsFlagSet(codeCharts, MidCodeCharts.MiscellaneousSymbolsAndArrows), CodeCharts.Middle.MiscellaneousSymbolsAndArrows());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Middle.IsFlagSet(codeCharts, MidCodeCharts.Glagolitic), CodeCharts.Middle.Glagolitic());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Middle.IsFlagSet(codeCharts, MidCodeCharts.LatinExtendedC), CodeCharts.Middle.LatinExtendedC());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Middle.IsFlagSet(codeCharts, MidCodeCharts.Coptic), CodeCharts.Middle.Coptic());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Middle.IsFlagSet(codeCharts, MidCodeCharts.GeorgianSupplement), CodeCharts.Middle.GeorgianSupplement());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Middle.IsFlagSet(codeCharts, MidCodeCharts.Tifinagh), CodeCharts.Middle.Tifinagh());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Middle.IsFlagSet(codeCharts, MidCodeCharts.EthiopicExtended), CodeCharts.Middle.EthiopicExtended());
        }

        /// <summary>
        /// Punch appropriate holes for the selected code charts.
        /// </summary>
        /// <param name="safeList">The safe list to punch through.</param>
        /// <param name="codeCharts">The code charts to punch.</param>
        private static void PunchCodeCharts(ref char[][] safeList, UpperMidCodeCharts codeCharts) {
            PunchHolesIfNeeded(ref safeList, CodeCharts.UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.CyrillicExtendedA), CodeCharts.UpperMiddle.CyrillicExtendedA());
            PunchHolesIfNeeded(ref safeList, CodeCharts.UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.SupplementalPunctuation), CodeCharts.UpperMiddle.SupplementalPunctuation());
            PunchHolesIfNeeded(ref safeList, CodeCharts.UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.CjkRadicalsSupplement), CodeCharts.UpperMiddle.CjkRadicalsSupplement());
            PunchHolesIfNeeded(ref safeList, CodeCharts.UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.KangxiRadicals), CodeCharts.UpperMiddle.KangxiRadicals());
            PunchHolesIfNeeded(ref safeList, CodeCharts.UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.IdeographicDescriptionCharacters), CodeCharts.UpperMiddle.IdeographicDescriptionCharacters());
            PunchHolesIfNeeded(ref safeList, CodeCharts.UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.CjkSymbolsAndPunctuation), CodeCharts.UpperMiddle.CjkSymbolsAndPunctuation());
            PunchHolesIfNeeded(ref safeList, CodeCharts.UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.Hiragana), CodeCharts.UpperMiddle.Hiragana());
            PunchHolesIfNeeded(ref safeList, CodeCharts.UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.Katakana), CodeCharts.UpperMiddle.Katakana());
            PunchHolesIfNeeded(ref safeList, CodeCharts.UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.Bopomofo), CodeCharts.UpperMiddle.Bopomofo());
            PunchHolesIfNeeded(ref safeList, CodeCharts.UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.HangulCompatibilityJamo), CodeCharts.UpperMiddle.HangulCompatibilityJamo());
            PunchHolesIfNeeded(ref safeList, CodeCharts.UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.Kanbun), CodeCharts.UpperMiddle.Kanbun());
            PunchHolesIfNeeded(ref safeList, CodeCharts.UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.BopomofoExtended), CodeCharts.UpperMiddle.BopomofoExtended());
            PunchHolesIfNeeded(ref safeList, CodeCharts.UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.CjkStrokes), CodeCharts.UpperMiddle.CjkStrokes());
            PunchHolesIfNeeded(ref safeList, CodeCharts.UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.KatakanaPhoneticExtensions), CodeCharts.UpperMiddle.KatakanaPhoneticExtensions());
            PunchHolesIfNeeded(ref safeList, CodeCharts.UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.EnclosedCjkLettersAndMonths), CodeCharts.UpperMiddle.EnclosedCjkLettersAndMonths());
            PunchHolesIfNeeded(ref safeList, CodeCharts.UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.CjkCompatibility), CodeCharts.UpperMiddle.CjkCompatibility());
            PunchHolesIfNeeded(ref safeList, CodeCharts.UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.CjkUnifiedIdeographsExtensionA), CodeCharts.UpperMiddle.CjkUnifiedIdeographsExtensionA());
            PunchHolesIfNeeded(ref safeList, CodeCharts.UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.YijingHexagramSymbols), CodeCharts.UpperMiddle.YijingHexagramSymbols());
            PunchHolesIfNeeded(ref safeList, CodeCharts.UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.CjkUnifiedIdeographs), CodeCharts.UpperMiddle.CjkUnifiedIdeographs());
            PunchHolesIfNeeded(ref safeList, CodeCharts.UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.YiSyllables), CodeCharts.UpperMiddle.YiSyllables());
            PunchHolesIfNeeded(ref safeList, CodeCharts.UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.YiRadicals), CodeCharts.UpperMiddle.YiRadicals());
            PunchHolesIfNeeded(ref safeList, CodeCharts.UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.Lisu), CodeCharts.UpperMiddle.Lisu());
            PunchHolesIfNeeded(ref safeList, CodeCharts.UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.Vai), CodeCharts.UpperMiddle.Vai());
            PunchHolesIfNeeded(ref safeList, CodeCharts.UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.CyrillicExtendedB), CodeCharts.UpperMiddle.CyrillicExtendedB());
            PunchHolesIfNeeded(ref safeList, CodeCharts.UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.Bamum), CodeCharts.UpperMiddle.Bamum());
            PunchHolesIfNeeded(ref safeList, CodeCharts.UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.ModifierToneLetters), CodeCharts.UpperMiddle.ModifierToneLetters());
            PunchHolesIfNeeded(ref safeList, CodeCharts.UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.LatinExtendedD), CodeCharts.UpperMiddle.LatinExtendedD());
            PunchHolesIfNeeded(ref safeList, CodeCharts.UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.SylotiNagri), CodeCharts.UpperMiddle.SylotiNagri());
            PunchHolesIfNeeded(ref safeList, CodeCharts.UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.CommonIndicNumberForms), CodeCharts.UpperMiddle.CommonIndicNumberForms());
            PunchHolesIfNeeded(ref safeList, CodeCharts.UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.Phagspa), CodeCharts.UpperMiddle.Phagspa());
            PunchHolesIfNeeded(ref safeList, CodeCharts.UpperMiddle.IsFlagSet(codeCharts, UpperMidCodeCharts.Saurashtra), CodeCharts.UpperMiddle.Saurashtra());
        }

        /// <summary>
        /// Punch appropriate holes for the selected code charts.
        /// </summary>
        /// <param name="safeList">The safe list to punch through.</param>
        /// <param name="codeCharts">The code charts to punch.</param>
        private static void PunchCodeCharts(ref char[][] safeList, UpperCodeCharts codeCharts) {
            PunchHolesIfNeeded(ref safeList, CodeCharts.Upper.IsFlagSet(codeCharts, UpperCodeCharts.DevanagariExtended), CodeCharts.Upper.DevanagariExtended());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Upper.IsFlagSet(codeCharts, UpperCodeCharts.KayahLi), CodeCharts.Upper.KayahLi());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Upper.IsFlagSet(codeCharts, UpperCodeCharts.Rejang), CodeCharts.Upper.Rejang());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Upper.IsFlagSet(codeCharts, UpperCodeCharts.HangulJamoExtendedA), CodeCharts.Upper.HangulJamoExtendedA());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Upper.IsFlagSet(codeCharts, UpperCodeCharts.Javanese), CodeCharts.Upper.Javanese());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Upper.IsFlagSet(codeCharts, UpperCodeCharts.Cham), CodeCharts.Upper.Cham());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Upper.IsFlagSet(codeCharts, UpperCodeCharts.MyanmarExtendedA), CodeCharts.Upper.MyanmarExtendedA());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Upper.IsFlagSet(codeCharts, UpperCodeCharts.TaiViet), CodeCharts.Upper.TaiViet());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Upper.IsFlagSet(codeCharts, UpperCodeCharts.MeeteiMayek), CodeCharts.Upper.MeeteiMayek());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Upper.IsFlagSet(codeCharts, UpperCodeCharts.HangulSyllables), CodeCharts.Upper.HangulSyllables());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Upper.IsFlagSet(codeCharts, UpperCodeCharts.HangulJamoExtendedB), CodeCharts.Upper.HangulJamoExtendedB());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Upper.IsFlagSet(codeCharts, UpperCodeCharts.CjkCompatibilityIdeographs), CodeCharts.Upper.CjkCompatibilityIdeographs());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Upper.IsFlagSet(codeCharts, UpperCodeCharts.AlphabeticPresentationForms), CodeCharts.Upper.AlphabeticPresentationForms());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Upper.IsFlagSet(codeCharts, UpperCodeCharts.ArabicPresentationFormsA), CodeCharts.Upper.ArabicPresentationFormsA());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Upper.IsFlagSet(codeCharts, UpperCodeCharts.VariationSelectors), CodeCharts.Upper.VariationSelectors());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Upper.IsFlagSet(codeCharts, UpperCodeCharts.VerticalForms), CodeCharts.Upper.VerticalForms());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Upper.IsFlagSet(codeCharts, UpperCodeCharts.CombiningHalfMarks), CodeCharts.Upper.CombiningHalfMarks());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Upper.IsFlagSet(codeCharts, UpperCodeCharts.CjkCompatibilityForms), CodeCharts.Upper.CjkCompatibilityForms());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Upper.IsFlagSet(codeCharts, UpperCodeCharts.SmallFormVariants), CodeCharts.Upper.SmallFormVariants());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Upper.IsFlagSet(codeCharts, UpperCodeCharts.ArabicPresentationFormsB), CodeCharts.Upper.ArabicPresentationFormsB());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Upper.IsFlagSet(codeCharts, UpperCodeCharts.HalfWidthAndFullWidthForms), CodeCharts.Upper.HalfWidthAndFullWidthForms());
            PunchHolesIfNeeded(ref safeList, CodeCharts.Upper.IsFlagSet(codeCharts, UpperCodeCharts.Specials), CodeCharts.Upper.Specials());
        }

        /// <summary>
        /// Punches holes as necessary.
        /// </summary>
        /// <param name="safeList">The safe list to punch through.</param>
        /// <param name="isNeeded">Value indicating whether the holes should be punched.</param>
        /// <param name="----edCharacters">The list of character positions to punch.</param>
        private static void PunchHolesIfNeeded(ref char[][] safeList, bool isNeeded, IEnumerable whiteListedCharacters) {
            if (!isNeeded) {
                return;
            }

            foreach (int offset in whiteListedCharacters) {
                safeList[offset] = null;
            }
        }
    }
}
