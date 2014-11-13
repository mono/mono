//------------------------------------------------------------------------------
// <copyright file="CodeCharts.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Security.AntiXss {
    using System;

    /// <summary>
    /// Values for the lowest section of the UTF8 Unicode code tables, from U0000 to U0FFF.
    /// </summary>
    [Flags]
    public enum LowerCodeCharts : long {
        /// <summary>
        /// No code charts from the lower region of the Unicode tables are safe-listed.
        /// </summary>
        None = 0,

        /// <summary>
        /// The Basic Latin code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0000.pdf</remarks>
        BasicLatin = 1 << 0x00,

        /// <summary>
        /// The C1 Controls and Latin-1 Supplement code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0080.pdf</remarks>
        C1ControlsAndLatin1Supplement = 1 << 0x01,

        /// <summary>
        /// The Latin Extended-A code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0100.pdf</remarks>
        LatinExtendedA = 1 << 0x02,

        /// <summary>
        /// The Latin Extended-B code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0180.pdf</remarks>
        LatinExtendedB = 1 << 0x03,

        /// <summary>
        /// The IPA Extensions code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0250.pdf</remarks>
        IpaExtensions = 1 << 0x04,

        /// <summary>
        /// The Spacing Modifier Letters code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U02B0.pdf</remarks>
        SpacingModifierLetters = 1 << 0x05,

        /// <summary>
        /// The Combining Diacritical Marks code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0300.pdf</remarks>
        CombiningDiacriticalMarks = 1 << 0x06,

        /// <summary>
        /// The Greek and Coptic code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0370.pdf</remarks>
        GreekAndCoptic = 1 << 0x07,

        /// <summary>
        /// The Cyrillic code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0400.pdf</remarks>
        Cyrillic = 1 << 0x08,

        /// <summary>
        /// The Cyrillic Supplement code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0500.pdf</remarks>
        CyrillicSupplement = 1 << 0x09,

        /// <summary>
        /// The Armenian code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0530.pdf</remarks>
        Armenian = 1 << 0x0A,

        /// <summary>
        /// The Hebrew code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0590.pdf</remarks>
        Hebrew = 1 << 0x0B,

        /// <summary>
        /// The Arabic code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0600.pdf</remarks>
        Arabic = 1 << 0x0C,

        /// <summary>
        /// The Syriac code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0700.pdf</remarks>
        Syriac = 1 << 0x0D,

        /// <summary>
        /// The Arabic Supplement code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0750.pdf</remarks>
        ArabicSupplement = 1 << 0x0E,

        /// <summary>
        /// The Thaana code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0780.pdf</remarks>
        Thaana = 1 << 0x0F,

        /// <summary>
        /// The Nko code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U07C0.pdf</remarks>
        Nko = 1 << 0x10,

        /// <summary>
        /// The Samaritan code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0800.pdf</remarks>
        Samaritan = 1 << 0x11,

        /// <summary>
        /// The Devanagari code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0900.pdf</remarks>
        Devanagari = 1 << 0x12,

        /// <summary>
        /// The Bengali code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0980.pdf</remarks>
        Bengali = 1 << 0x13,

        /// <summary>
        /// The Gurmukhi code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0A00.pdf</remarks>
        Gurmukhi = 1 << 0x14,

        /// <summary>
        /// The Gujarati code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0A80.pdf</remarks>
        Gujarati = 1 << 0x15,

        /// <summary>
        /// The Oriya code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0B00.pdf</remarks>
        Oriya = 1 << 0x16,

        /// <summary>
        /// The Tamil code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0B80.pdf</remarks>
        Tamil = 1 << 0x17,

        /// <summary>
        /// The Telugu code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0C00.pdf</remarks>
        Telugu = 1 << 0x18,

        /// <summary>
        /// The Kannada code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0C80.pdf</remarks>
        Kannada = 1 << 0x19,

        /// <summary>
        /// The Malayalam code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0D00.pdf</remarks>
        Malayalam = 1 << 0x1A,

        /// <summary>
        /// The Sinhala code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0D80.pdf</remarks>
        Sinhala = 1 << 0x1B,

        /// <summary>
        /// The Thai code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0E00.pdf</remarks>
        Thai = 1 << 0x1C,

        /// <summary>
        /// The Lao code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0E80.pdf</remarks>
        Lao = 1 << 0x1D,

        /// <summary>
        /// The Tibetan code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U0F00.pdf</remarks>
        Tibetan = 1 << 0x1E,

        /// <summary>
        /// The default code tables marked as safe on initialisation.
        /// </summary>
        Default = BasicLatin | C1ControlsAndLatin1Supplement | LatinExtendedA | LatinExtendedB | SpacingModifierLetters | IpaExtensions | CombiningDiacriticalMarks
    }

    /// <summary>
    /// Values for the lower-mid section of the UTF8 Unicode code tables, from U1000 to U1EFF.
    /// </summary>
    [Flags]
    public enum LowerMidCodeCharts : long {
        /// <summary>
        /// No code charts from the lower-mid region of the Unicode tables are safe-listed.
        /// </summary>
        None = 0,

        /// <summary>
        /// The Myanmar code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1000.pdf</remarks>
        Myanmar = 1 << 0x00,

        /// <summary>
        /// The Georgian code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U10A0.pdf</remarks>
        Georgian = 1 << 0x01,

        /// <summary>
        /// The Hangul Jamo code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1100.pdf</remarks>
        HangulJamo = 1 << 0x02,

        /// <summary>
        /// The Ethiopic code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1200.pdf</remarks>
        Ethiopic = 1 << 0x03,

        /// <summary>
        /// The Ethiopic supplement code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1380.pdf</remarks>
        EthiopicSupplement = 1 << 0x04,

        /// <summary>
        /// The Cherokee code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U13A0.pdf</remarks>
        Cherokee = 1 << 0x05,

        /// <summary>
        /// The Unified Canadian Aboriginal Syllabics code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1400.pdf</remarks>
        UnifiedCanadianAboriginalSyllabics = 1 << 0x06,

        /// <summary>
        /// The Ogham code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1680.pdf</remarks>
        Ogham = 1 << 0x07,

        /// <summary>
        /// The Runic code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U16A0.pdf</remarks>
        Runic = 1 << 0x08,

        /// <summary>
        /// The Tagalog code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1700.pdf</remarks>
        Tagalog = 1 << 0x09,

        /// <summary>
        /// The Hanunoo code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1720.pdf</remarks>
        Hanunoo = 1 << 0x0A,

        /// <summary>
        /// The Buhid code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1740.pdf</remarks>
        Buhid = 1 << 0x0B,

        /// <summary>
        /// The Tagbanwa code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1760.pdf</remarks>
        Tagbanwa = 1 << 0x0C,

        /// <summary>
        /// The Khmer code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1780.pdf</remarks>
        Khmer = 1 << 0x0D,

        /// <summary>
        /// The Mongolian code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1800.pdf</remarks>
        Mongolian = 1 << 0x0E,

        /// <summary>
        /// The Unified Canadian Aboriginal Syllabics Extended code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U18B0.pdf</remarks>
        UnifiedCanadianAboriginalSyllabicsExtended = 1 << 0x0F,

        /// <summary>
        /// The Limbu code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1900.pdf</remarks>
        Limbu = 1 << 0x10,

        /// <summary>
        /// The Tai Le code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1950.pdf</remarks>
        TaiLe = 1 << 0x11,

        /// <summary>
        /// The New Tai Lue code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1980.pdf</remarks>
        NewTaiLue = 1 << 0x12,

        /// <summary>
        /// The Khmer Symbols code table
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U19E0.pdf</remarks>
        KhmerSymbols = 1 << 0x13,

        /// <summary>
        /// The Buginese code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1A00.pdf</remarks>
        Buginese = 1 << 0x14,

        /// <summary>
        /// The Tai Tham code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1A20.pdf</remarks>
        TaiTham = 1 << 0x15,

        /// <summary>
        /// The Balinese code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1B00.pdf</remarks>
        Balinese = 1 << 0x16,

        /// <summary>
        /// The Sudanese code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1B80.pdf</remarks>
        Sudanese = 1 << 0x17,

        /// <summary>
        /// The Lepcha code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1C00.pdf</remarks>
        Lepcha = 1 << 0x18,

        /// <summary>
        /// The Ol Chiki code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1C50.pdf</remarks>
        OlChiki = 1 << 0x19,

        /// <summary>
        /// The Vedic Extensions code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1CD0.pdf</remarks>
        VedicExtensions = 1 << 0x1A,

        /// <summary>
        /// The Phonetic Extensions code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1D00.pdf</remarks>
        PhoneticExtensions = 1 << 0x1B,

        /// <summary>
        /// The Phonetic Extensions Supplement code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1D80.pdf</remarks>
        PhoneticExtensionsSupplement = 1 << 0x1C,

        /// <summary>
        /// The Combining Diacritical Marks Supplement code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1DC0.pdf</remarks>        
        CombiningDiacriticalMarksSupplement = 1 << 0x1D,

        /// <summary>
        /// The Latin Extended Additional code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1E00.pdf</remarks>
        LatinExtendedAdditional = 1 << 0x1E
    }

    /// <summary>
    /// Values for the middle section of the UTF8 Unicode code tables, from U1F00 to U2DDF
    /// </summary>
    [Flags]
    public enum MidCodeCharts : long {
        /// <summary>
        /// No code charts from the lower region of the Unicode tables are safe-listed.
        /// </summary>
        None = 0,

        /// <summary>
        /// The Greek Extended code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U1F00.pdf</remarks>
        GreekExtended = 1 << 0x00,

        /// <summary>
        /// The General Punctuation code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2000.pdf</remarks>
        GeneralPunctuation = 1 << 0x01,

        /// <summary>
        /// The Superscripts and Subscripts code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2070.pdf</remarks>
        SuperscriptsAndSubscripts = 1 << 0x02,

        /// <summary>
        /// The Currency Symbols code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U20A0.pdf</remarks>
        CurrencySymbols = 1 << 0x03,

        /// <summary>
        /// The Combining Diacritical Marks for Symbols code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U20D0.pdf</remarks>
        CombiningDiacriticalMarksForSymbols = 1 << 0x04,

        /// <summary>
        /// The Letterlike Symbols code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2100.pdf</remarks>
        LetterlikeSymbols = 1 << 0x05,

        /// <summary>
        /// The Number Forms code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2150.pdf</remarks>
        NumberForms = 1 << 0x06,

        /// <summary>
        /// The Arrows code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2190.pdf</remarks>
        Arrows = 1 << 0x07,

        /// <summary>
        /// The Mathematical Operators code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2200.pdf</remarks>
        MathematicalOperators = 1 << 0x08,

        /// <summary>
        /// The Miscellaneous Technical code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2300.pdf</remarks>
        MiscellaneousTechnical = 1 << 0x09,

        /// <summary>
        /// The Control Pictures code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2400.pdf</remarks>
        ControlPictures = 1 << 0x0A,

        /// <summary>
        /// The Optical Character Recognition table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2440.pdf</remarks>
        OpticalCharacterRecognition = 1 << 0x0B,

        /// <summary>
        /// The Enclosed Alphanumeric code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2460.pdf</remarks>
        EnclosedAlphanumerics = 1 << 0x0C,

        /// <summary>
        /// The Box Drawing code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2500.pdf</remarks>
        BoxDrawing = 1 << 0x0D,

        /// <summary>
        /// The Block Elements code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2580.pdf</remarks>
        BlockElements = 1 << 0x0E,

        /// <summary>
        /// The Geometric Shapes code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U25A0.pdf</remarks>
        GeometricShapes = 1 << 0x0F,

        /// <summary>
        /// The Miscellaneous Symbols code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2600.pdf</remarks>
        MiscellaneousSymbols = 1 << 0x10,

        /// <summary>
        /// The Dingbats code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2700.pdf</remarks>
        Dingbats = 1 << 0x11,

        /// <summary>
        /// The Miscellaneous Mathematical Symbols-A code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U27C0.pdf</remarks>
        MiscellaneousMathematicalSymbolsA = 1 << 0x12,

        /// <summary>
        /// The Supplemental Arrows-A code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U27F0.pdf</remarks>
        SupplementalArrowsA = 1 << 0x13,

        /// <summary>
        /// The Braille Patterns code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2800.pdf</remarks>
        BraillePatterns = 1 << 0x14,

        /// <summary>
        /// The Supplemental Arrows-B code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2900.pdf</remarks>
        SupplementalArrowsB = 1 << 0x15,

        /// <summary>
        /// The Miscellaneous Mathematical Symbols-B code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2980.pdf</remarks>                
        MiscellaneousMathematicalSymbolsB = 1 << 0x16,

        /// <summary>
        /// The Supplemental Mathematical Operators code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2A00.pdf</remarks>
        SupplementalMathematicalOperators = 1 << 0x17,

        /// <summary>
        /// The Miscellaneous Symbols and Arrows code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2B00.pdf</remarks>        
        MiscellaneousSymbolsAndArrows = 1 << 0x18,

        /// <summary>
        /// The Glagolitic code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2C00.pdf</remarks>
        Glagolitic = 1 << 0x19,

        /// <summary>
        /// The Latin Extended-C code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2C60.pdf</remarks>        
        LatinExtendedC = 1 << 0x1A,

        /// <summary>
        /// The Coptic code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2C80.pdf</remarks>
        Coptic = 1 << 0x1B,

        /// <summary>
        /// The Georgian Supplement code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2D00.pdf</remarks>
        GeorgianSupplement = 1 << 0x1C,

        /// <summary>
        /// The Tifinagh code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2D30.pdf</remarks>
        Tifinagh = 1 << 0x1D,

        /// <summary>
        /// The Ethiopic Extended code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2D80.pdf</remarks>
        EthiopicExtended = 1 << 0x0E,
    }

    /// <summary>
    /// Values for the upper middle section of the UTF8 Unicode code tables, from U2DE0 to UA8DF
    /// </summary>
    [Flags]
    public enum UpperMidCodeCharts : long {
        /// <summary>
        /// No code charts from the lower region of the Unicode tables are safe-listed.
        /// </summary>
        None = 0,

        /// <summary>
        /// The Cyrillic Extended-A code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2DE0.pdf</remarks>
        CyrillicExtendedA = 1 << 0x00,

        /// <summary>
        /// The Supplemental Punctuation code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2E00.pdf</remarks>
        SupplementalPunctuation = 1 << 0x01,

        /// <summary>
        /// The CJK Radicials Supplement code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2E80.pdf</remarks>
        CjkRadicalsSupplement = 1 << 0x02,

        /// <summary>
        /// The Kangxi Radicials code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2F00.pdf</remarks>
        KangxiRadicals = 1 << 0x03,

        /// <summary>
        /// The Ideographic Description Characters code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U2FF0.pdf</remarks>
        IdeographicDescriptionCharacters = 1 << 0x04,

        /// <summary>
        /// The CJK Symbols and Punctuation code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U3000.pdf</remarks>
        CjkSymbolsAndPunctuation = 1 << 0x05,

        /// <summary>
        /// The Hiragana code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U3040.pdf</remarks>
        Hiragana = 1 << 0x06,

        /// <summary>
        /// The Katakana code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U30A0.pdf</remarks>
        Katakana = 1 << 0x07,

        /// <summary>
        /// The Bopomofo code table.
        /// <seealso cref="BopomofoExtended"/>
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U3100.pdf</remarks>
        Bopomofo = 1 << 0x08,

        /// <summary>
        /// The Hangul Compatbility Jamo code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U3130.pdf</remarks>
        HangulCompatibilityJamo = 1 << 0x09,

        /// <summary>
        /// The Kanbun code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U3190.pdf</remarks>
        Kanbun = 1 << 0x0A,

        /// <summary>
        /// The Bopomofu Extended code table.
        /// <seealso cref="Bopomofo"/>
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U31A0.pdf</remarks>
        BopomofoExtended = 1 << 0x0B,

        /// <summary>
        /// The CJK Strokes code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U31C0.pdf</remarks>
        CjkStrokes = 1 << 0x0C,

        /// <summary>
        /// The Katakana Phonetic Extensoins code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U31F0.pdf</remarks>
        KatakanaPhoneticExtensions = 1 << 0x0D,

        /// <summary>
        /// The Enclosed CJK Letters and Months code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U3200.pdf</remarks>
        EnclosedCjkLettersAndMonths = 1 << 0x0E,

        /// <summary>
        /// The CJK Compatibility code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U3300.pdf</remarks>
        CjkCompatibility = 1 << 0x0F,

        /// <summary>
        /// The CJK Unified Ideographs Extension A code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U3400.pdf</remarks>
        CjkUnifiedIdeographsExtensionA = 1 << 0x10,

        /// <summary>
        /// The Yijing Hexagram Symbols code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U4DC0.pdf</remarks>
        YijingHexagramSymbols = 1 << 0x11,

        /// <summary>
        /// The CJK Unified Ideographs code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/U4E00.pdf</remarks>
        CjkUnifiedIdeographs = 1 << 0x12,

        /// <summary>
        /// The Yi Syllables code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UA000.pdf</remarks>
        YiSyllables = 1 << 0x13,

        /// <summary>
        /// The Yi Radicals code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UA490.pdf</remarks>
        YiRadicals = 1 << 0x14,

        /// <summary>
        /// The Lisu code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UA4D0.pdf</remarks>        
        Lisu = 1 << 0x15,

        /// <summary>
        /// The Vai code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UA500.pdf</remarks>
        Vai = 1 << 0x16,

        /// <summary>
        /// The Cyrillic Extended-B code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UA640.pdf</remarks>
        CyrillicExtendedB = 1 << 0x17,

        /// <summary>
        /// The Bamum code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UA6A0.pdf</remarks>
        Bamum = 1 << 0x18,

        /// <summary>
        /// The Modifier Tone Letters code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UA700.pdf</remarks>
        ModifierToneLetters = 1 << 0x19,

        /// <summary>
        /// The Latin Extended-D code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UA720.pdf</remarks>
        LatinExtendedD = 1 << 0x1A,

        /// <summary>
        /// The Syloti Nagri code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UA800.pdf</remarks>
        SylotiNagri = 1 << 0x1B,

        /// <summary>
        /// The Common Indic Number Forms code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UA830.pdf</remarks>
        CommonIndicNumberForms = 1 << 0x1C,

        /// <summary>
        /// The Phags-pa code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UA840.pdf</remarks>
        Phagspa = 1 << 0x1D,

        /// <summary>
        /// The Saurashtra code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UA880.pdf</remarks>
        Saurashtra = 1 << 0x1E,
    }

    /// <summary>
    /// Values for the upper section of the UTF8 Unicode code tables, from UA8E0 to UFFFD
    /// </summary>
    [Flags]
    public enum UpperCodeCharts {
        /// <summary>
        /// No code charts from the upper region of the Unicode tables are safe-listed.
        /// </summary>
        None = 0,

        /// <summary>
        /// The Devanagari Extended code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UA8E0.pdf</remarks>
        DevanagariExtended = 1 << 0x00,

        /// <summary>
        /// The Kayah Li code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UA900.pdf</remarks>
        KayahLi = 1 << 0x01,

        /// <summary>
        /// The Rejang code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UA930.pdf</remarks>
        Rejang = 1 << 0x02,

        /// <summary>
        /// The Hangul Jamo Extended-A code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UA960.pdf</remarks>
        HangulJamoExtendedA = 1 << 0x03,

        /// <summary>
        /// The Javanese code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UA980.pdf</remarks>
        Javanese = 1 << 0x04,

        /// <summary>
        /// The Cham code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UAA00.pdf</remarks>
        Cham = 1 << 0x05,

        /// <summary>
        /// The Myanmar Extended-A code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UAA60.pdf</remarks>
        MyanmarExtendedA = 1 << 0x06,

        /// <summary>
        /// The Tai Viet code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UAA80.pdf</remarks>
        TaiViet = 1 << 0x07,

        /// <summary>
        /// The Meetei Mayek code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UABC0.pdf</remarks>
        MeeteiMayek = 1 << 0x08,

        /// <summary>
        /// The Hangul Syllables code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UAC00.pdf</remarks>
        HangulSyllables = 1 << 0x09,

        /// <summary>
        /// The Hangul Jamo Extended-B code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UD7B0.pdf</remarks>
        HangulJamoExtendedB = 1 << 0x0A,

        /// <summary>
        /// The CJK Compatibility Ideographs code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UF900.pdf</remarks>
        CjkCompatibilityIdeographs = 1 << 0x0B,

        /// <summary>
        /// The Alphabetic Presentation Forms code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UFB00.pdf</remarks>
        AlphabeticPresentationForms = 1 << 0x0C,

        /// <summary>
        /// The Arabic Presentation Forms-A code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UFB50.pdf</remarks>
        ArabicPresentationFormsA = 1 << 0x0D,

        /// <summary>
        /// The Variation Selectors code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UFE00.pdf</remarks>
        VariationSelectors = 1 << 0x0E,

        /// <summary>
        /// The Vertical Forms code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UFE10.pdf</remarks>
        VerticalForms = 1 << 0x0F,

        /// <summary>
        /// The Combining Half Marks code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UFE20.pdf</remarks>
        CombiningHalfMarks = 1 << 0x10,

        /// <summary>
        /// The CJK Compatibility Forms code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UFE30.pdf</remarks>
        CjkCompatibilityForms = 1 << 0x11,

        /// <summary>
        /// The Small Form Variants code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UFE50.pdf</remarks>
        SmallFormVariants = 1 << 0x12,

        /// <summary>
        /// The Arabic Presentation Forms-B code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UFE70.pdf</remarks>
        ArabicPresentationFormsB = 1 << 0x13,

        /// <summary>
        /// The half width and full width Forms code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UFF00.pdf</remarks>
        HalfWidthAndFullWidthForms = 1 << 0x14,

        /// <summary>
        /// The Specials code table.
        /// </summary>
        /// <remarks>http://www.unicode.org/charts/PDF/UFFF0.pdf</remarks>
        Specials = 1 << 0x15,
    }
}
