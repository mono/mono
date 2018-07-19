//------------------------------------------------------------------------------
// <copyright file="UnicodeCharacterEncoder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Security.AntiXss {
    using System;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Provides HTML encoding methods.
    /// </summary>
    internal static class UnicodeCharacterEncoder {
        /// <summary>
        /// A lock object to use when performing safe listing.
        /// </summary>
        private static readonly ReaderWriterLockSlim SyncLock = new ReaderWriterLockSlim();

        /// <summary>
        /// The HTML escaped value for a space, used in attribute encoding.
        /// </summary>
        private static readonly char[] UnicodeSpace = "&#32;".ToCharArray();

        /// <summary>
        /// The XML named entity for an apostrophe, used in XML encoding.
        /// </summary>
        private static readonly char[] XmlApostrophe = "&apos;".ToCharArray();

        /// <summary>
        /// The current lower code chart settings.
        /// </summary>
        private static LowerCodeCharts currentLowerCodeChartSettings = LowerCodeCharts.None;

        /// <summary>
        /// The current lower middle code chart settings.
        /// </summary>
        private static LowerMidCodeCharts currentLowerMidCodeChartSettings = LowerMidCodeCharts.None;

        /// <summary>
        /// The current middle code chart settings.
        /// </summary>
        private static MidCodeCharts currentMidCodeChartSettings = MidCodeCharts.None;

        /// <summary>
        /// The current upper middle code chart settings.
        /// </summary>
        private static UpperMidCodeCharts currentUpperMidCodeChartSettings = UpperMidCodeCharts.None;

        /// <summary>
        /// The current upper code chart settings.
        /// </summary>
        private static UpperCodeCharts currentUpperCodeChartSettings = UpperCodeCharts.None;

        /// <summary>
        /// The values to output for each character.
        /// </summary>
        private static char[][] characterValues;

        /// <summary>
        /// The values to output for HTML named entities.
        /// </summary>
        private static Lazy<char[][]> namedEntitiesLazy = new Lazy<char[][]>(InitialiseNamedEntityList);

        /// <summary>
        /// Provides method specific encoding of characters.
        /// </summary>
        /// <param name="input">The character to encode</param>
        /// <param name="output">The encoded character, if it has been encoded.</param>
        /// <returns>True if the character has been encoded, otherwise false.</returns>
        private delegate bool MethodSpecificEncoder(char input, out char[] output);

        /// <summary>
        /// Marks characters from the specified languages as safe.
        /// </summary>
        /// <param name="lowerCodeCharts">The combination of lower code charts to use.</param>
        /// <param name="lowerMidCodeCharts">The combination of lower mid code charts to use.</param>
        /// <param name="midCodeCharts">The combination of mid code charts to use.</param>
        /// <param name="upperMidCodeCharts">The combination of upper mid code charts to use.</param>
        /// <param name="upperCodeCharts">The combination of upper code charts to use.</param>
        /// <remarks>The safe list affects all HTML and XML encoding functions.</remarks>
        public static void MarkAsSafe(
            LowerCodeCharts lowerCodeCharts,
            LowerMidCodeCharts lowerMidCodeCharts,
            MidCodeCharts midCodeCharts,
            UpperMidCodeCharts upperMidCodeCharts,
            UpperCodeCharts upperCodeCharts) {
            if (lowerCodeCharts == currentLowerCodeChartSettings &&
                lowerMidCodeCharts == currentLowerMidCodeChartSettings &&
                midCodeCharts == currentMidCodeChartSettings &&
                upperMidCodeCharts == currentUpperMidCodeChartSettings &&
                upperCodeCharts == currentUpperCodeChartSettings) {
                return;
            }

            SyncLock.EnterWriteLock();
            try {
                if (characterValues == null) {
                    characterValues = SafeList.Generate(65536, SafeList.HashThenValueGenerator);
                }

                SafeList.PunchUnicodeThrough(
                    ref characterValues,
                    lowerCodeCharts,
                    lowerMidCodeCharts,
                    midCodeCharts,
                    upperMidCodeCharts,
                    upperCodeCharts);

                ApplyHtmlSpecificValues();

                currentLowerCodeChartSettings = lowerCodeCharts;
                currentLowerMidCodeChartSettings = lowerMidCodeCharts;
                currentMidCodeChartSettings = midCodeCharts;
                currentUpperMidCodeChartSettings = upperMidCodeCharts;
                currentUpperCodeChartSettings = upperCodeCharts;
            }
            finally {
                SyncLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Encodes input strings for use in XML.
        /// </summary>
        /// <param name="input">String to be encoded</param>
        /// <returns>
        /// Encoded string for use in XML.
        /// </returns>
        internal static string XmlEncode(string input) {
            return HtmlEncode(input, false, XmlTweak);
        }

        /// <summary>
        /// Encodes input strings for use in XML.
        /// </summary>
        /// <param name="input">String to be encoded</param>
        /// <returns>
        /// Encoded string for use in XML.
        /// </returns>
        internal static string XmlAttributeEncode(string input) {
            return HtmlEncode(input, false, XmlAttributeTweak);
        }

        /// <summary>
        /// Encodes input strings for use in HTML attributes.
        /// </summary>
        /// <param name="input">String to be encoded</param>
        /// <returns>
        /// Encoded string for use in HTML attributes.
        /// </returns>
        internal static string HtmlAttributeEncode(string input) {
            return HtmlEncode(input, false, HtmlAttributeTweak);
        }

        /// <summary>
        /// Encodes input strings for use in HTML.
        /// </summary>
        /// <param name="input">String to be encoded</param>
        /// <param name="useNamedEntities">Value indicating if the HTML 4.0 named entities should be used.</param>
        /// <returns>
        /// Encoded string for use in HTML.
        /// </returns>
        internal static string HtmlEncode(string input, bool useNamedEntities) {
            return HtmlEncode(input, useNamedEntities, null);
        }

        /// <summary>
        /// HTML Attribute Encoding specific tweaks.
        /// </summary>
        /// <param name="input">The character to potentially encode.</param>
        /// <param name="output">The encoded character, if any.</param>
        /// <returns>True if encoding took place, otherwise false.</returns>
        private static bool HtmlAttributeTweak(char input, out char[] output) {
            if (input == ' ') {
                output = UnicodeSpace;
                return true;
            }

            output = null;
            return false;
        }

        /// <summary>
        /// XML specific tweaks.
        /// </summary>
        /// <param name="input">The character to potentially encode.</param>
        /// <param name="output">The encoded character, if any.</param>
        /// <returns>True if encoding took place, otherwise false.</returns>
        private static bool XmlTweak(char input, out char[] output) {
            if (input == '\'') {
                output = XmlApostrophe;
                return true;
            }

            output = null;
            return false;
        }

        /// <summary>
        /// XML Attribute Encoding specific tweaks.
        /// </summary>
        /// <param name="input">The character to potentially encode.</param>
        /// <param name="output">The encoded character, if any.</param>
        /// <returns>True if encoding took place, otherwise false.</returns>
        private static bool XmlAttributeTweak(char input, out char[] output) {
            if (input == '\'') {
                output = XmlApostrophe;
                return true;
            }

            if (input == ' ') {
                output = UnicodeSpace;
                return true;
            }

            output = null;
            return false;
        }

        /// <summary>
        /// Encodes input strings for use in HTML.
        /// </summary>
        /// <param name="input">String to be encoded</param>
        /// <param name="useNamedEntities">Value indicating if the HTML 4.0 named entities should be used.</param>
        /// <param name="encoderTweak">A <see cref="MethodSpecificEncoder"/> function, if needed.</param>
        /// <returns>
        /// Encoded string for use in HTML.
        /// </returns>
        private static string HtmlEncode(string input, bool useNamedEntities, MethodSpecificEncoder encoderTweak) {
            if (string.IsNullOrEmpty(input)) {
                return input;
            }

            if (characterValues == null) {
                InitialiseSafeList();
            }

            char[][] namedEntities = null;
            if (useNamedEntities) {
                namedEntities = namedEntitiesLazy.Value;
            }

            // Setup a new StringBuilder for output.
            // Worse case scenario - the longest entity name, thetasym is 10 characters, including the & and ;.
            StringBuilder builder = EncoderUtil.GetOutputStringBuilder(input.Length, 10 /* worstCaseOutputCharsPerInputChar */);

            SyncLock.EnterReadLock();
            try {
                Utf16StringReader stringReader = new Utf16StringReader(input);
                while (true) {
                    int currentCodePoint = stringReader.ReadNextScalarValue();
                    if (currentCodePoint < 0) {
                        break; // EOF
                    }

                    if (currentCodePoint > Char.MaxValue) {
                        // We don't have a pre-generated mapping of characters beyond the Basic Multilingual
                        // Plane (BMP), so we need to generate these encodings on-the-fly. We should encode
                        // the code point rather than the surrogate code units that make up this code point.
                        // See: http://www.w3.org/International/questions/qa-escapes#bytheway

                        char[] encodedCharacter = SafeList.HashThenValueGenerator(currentCodePoint);
                        builder.Append('&');
                        builder.Append(encodedCharacter);
                        builder.Append(';');
                    }
                    else {
                        // If we reached this point, the code point is within the BMP.
                        char currentCharacter = (char)currentCodePoint;
                        char[] tweekedValue;

                        if (encoderTweak != null && encoderTweak(currentCharacter, out tweekedValue)) {
                            builder.Append(tweekedValue);
                        }
                        else if (useNamedEntities && namedEntities[currentCodePoint] != null) {
                            char[] encodedCharacter = namedEntities[currentCodePoint];
                            builder.Append('&');
                            builder.Append(encodedCharacter);
                            builder.Append(';');
                        }
                        else if (characterValues[currentCodePoint] != null) {
                            // character needs to be encoded
                            char[] encodedCharacter = characterValues[currentCodePoint];
                            builder.Append('&');
                            builder.Append(encodedCharacter);
                            builder.Append(';');
                        }
                        else {
                            // character does not need encoding
                            builder.Append(currentCharacter);
                        }
                    }
                }
            }
            finally {
                SyncLock.ExitReadLock();
            }

            return builder.ToString();
        }

        /// <summary>
        /// Initializes the HTML safe list.
        /// </summary>
        private static void InitialiseSafeList() {
            SyncLock.EnterWriteLock();
            try {
                if (characterValues == null) {
                    characterValues = SafeList.Generate(0xFFFF, SafeList.HashThenValueGenerator);
                    SafeList.PunchUnicodeThrough(
                        ref characterValues,
                        LowerCodeCharts.Default,
                        LowerMidCodeCharts.None,
                        MidCodeCharts.None,
                        UpperMidCodeCharts.None,
                        UpperCodeCharts.None);
                    ApplyHtmlSpecificValues();
                }
            }
            finally {
                SyncLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Applies Html specific values to the internal value list.
        /// </summary>
        /// <remarks>
        /// ASP.NET 4 and Razor introduced a new syntax &lt;%: %&gt; and @ which are used to HTML-encode values.
        /// For example, &lt;%: foo %&gt; is shorthand for &lt;%= HttpUtility.HtmlEncode(foo) %&gt;. Since these could
        /// occur inside an attribute, e.g. &lt;a href="@Foo"&gt;, ASP.NET mandates that HtmlEncode also encode
        /// characters that are meaningful inside HTML attributes, like the single quote. Encoding spaces
        /// isn't mandatory since it's expected that users will surround such variables with quotes.
        /// </remarks>
        private static void ApplyHtmlSpecificValues() {
            characterValues['<'] = "lt".ToCharArray();
            characterValues['>'] = "gt".ToCharArray();
            characterValues['&'] = "amp".ToCharArray();
            characterValues['"'] = "quot".ToCharArray();
            characterValues['\''] = "#39".ToCharArray();
        }

        /// <summary>
        /// Initialises the HTML named entities list.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Maintainability",
            "CA1505:AvoidUnmaintainableCode",
            Justification = "Splitting or initialising via lookups has too large a performance increase.")]
        private static char[][] InitialiseNamedEntityList() {
            char[][] namedEntities = new char[65536][];
            namedEntities[160] = "nbsp".ToCharArray();
            namedEntities[161] = "iexcl".ToCharArray();
            namedEntities[162] = "cent".ToCharArray();
            namedEntities[163] = "pound".ToCharArray();
            namedEntities[164] = "curren".ToCharArray();
            namedEntities[165] = "yen".ToCharArray();
            namedEntities[166] = "brvbar".ToCharArray();
            namedEntities[167] = "sect".ToCharArray();
            namedEntities[168] = "uml".ToCharArray();
            namedEntities[169] = "copy".ToCharArray();
            namedEntities[170] = "ordf".ToCharArray();
            namedEntities[171] = "laquo".ToCharArray();
            namedEntities[172] = "not".ToCharArray();
            namedEntities[173] = "shy".ToCharArray();
            namedEntities[174] = "reg".ToCharArray();
            namedEntities[175] = "macr".ToCharArray();
            namedEntities[176] = "deg".ToCharArray();
            namedEntities[177] = "plusmn".ToCharArray();
            namedEntities[178] = "sup2".ToCharArray();
            namedEntities[179] = "sup3".ToCharArray();
            namedEntities[180] = "acute".ToCharArray();
            namedEntities[181] = "micro".ToCharArray();
            namedEntities[182] = "para".ToCharArray();
            namedEntities[183] = "middot".ToCharArray();
            namedEntities[184] = "cedil".ToCharArray();
            namedEntities[185] = "sup1".ToCharArray();
            namedEntities[186] = "ordm".ToCharArray();
            namedEntities[187] = "raquo".ToCharArray();
            namedEntities[188] = "frac14".ToCharArray();
            namedEntities[189] = "frac12".ToCharArray();
            namedEntities[190] = "frac34".ToCharArray();
            namedEntities[191] = "iquest".ToCharArray();
            namedEntities[192] = "Agrave".ToCharArray();
            namedEntities[193] = "Aacute".ToCharArray();
            namedEntities[194] = "Acirc".ToCharArray();
            namedEntities[195] = "Atilde".ToCharArray();
            namedEntities[196] = "Auml".ToCharArray();
            namedEntities[197] = "Aring".ToCharArray();
            namedEntities[198] = "AElig".ToCharArray();
            namedEntities[199] = "Ccedil".ToCharArray();
            namedEntities[200] = "Egrave".ToCharArray();
            namedEntities[201] = "Eacute".ToCharArray();
            namedEntities[202] = "Ecirc".ToCharArray();
            namedEntities[203] = "Euml".ToCharArray();
            namedEntities[204] = "Igrave".ToCharArray();
            namedEntities[205] = "Iacute".ToCharArray();
            namedEntities[206] = "Icirc".ToCharArray();
            namedEntities[207] = "Iuml".ToCharArray();
            namedEntities[208] = "ETH".ToCharArray();
            namedEntities[209] = "Ntilde".ToCharArray();
            namedEntities[210] = "Ograve".ToCharArray();
            namedEntities[211] = "Oacute".ToCharArray();
            namedEntities[212] = "Ocirc".ToCharArray();
            namedEntities[213] = "Otilde".ToCharArray();
            namedEntities[214] = "Ouml".ToCharArray();
            namedEntities[215] = "times".ToCharArray();
            namedEntities[216] = "Oslash".ToCharArray();
            namedEntities[217] = "Ugrave".ToCharArray();
            namedEntities[218] = "Uacute".ToCharArray();
            namedEntities[219] = "Ucirc".ToCharArray();
            namedEntities[220] = "Uuml".ToCharArray();
            namedEntities[221] = "Yacute".ToCharArray();
            namedEntities[222] = "THORN".ToCharArray();
            namedEntities[223] = "szlig".ToCharArray();
            namedEntities[224] = "agrave".ToCharArray();
            namedEntities[225] = "aacute".ToCharArray();
            namedEntities[226] = "acirc".ToCharArray();
            namedEntities[227] = "atilde".ToCharArray();
            namedEntities[228] = "auml".ToCharArray();
            namedEntities[229] = "aring".ToCharArray();
            namedEntities[230] = "aelig".ToCharArray();
            namedEntities[231] = "ccedil".ToCharArray();
            namedEntities[232] = "egrave".ToCharArray();
            namedEntities[233] = "eacute".ToCharArray();
            namedEntities[234] = "ecirc".ToCharArray();
            namedEntities[235] = "euml".ToCharArray();
            namedEntities[236] = "igrave".ToCharArray();
            namedEntities[237] = "iacute".ToCharArray();
            namedEntities[238] = "icirc".ToCharArray();
            namedEntities[239] = "iuml".ToCharArray();
            namedEntities[240] = "eth".ToCharArray();
            namedEntities[241] = "ntilde".ToCharArray();
            namedEntities[242] = "ograve".ToCharArray();
            namedEntities[243] = "oacute".ToCharArray();
            namedEntities[244] = "ocirc".ToCharArray();
            namedEntities[245] = "otilde".ToCharArray();
            namedEntities[246] = "ouml".ToCharArray();
            namedEntities[247] = "divide".ToCharArray();
            namedEntities[248] = "oslash".ToCharArray();
            namedEntities[249] = "ugrave".ToCharArray();
            namedEntities[250] = "uacute".ToCharArray();
            namedEntities[251] = "ucirc".ToCharArray();
            namedEntities[252] = "uuml".ToCharArray();
            namedEntities[253] = "yacute".ToCharArray();
            namedEntities[254] = "thorn".ToCharArray();
            namedEntities[255] = "yuml".ToCharArray();

            namedEntities[338] = "OElig".ToCharArray();
            namedEntities[339] = "oelig".ToCharArray();
            namedEntities[352] = "Scaron".ToCharArray();
            namedEntities[353] = "scaron".ToCharArray();
            namedEntities[376] = "Yuml".ToCharArray();
            namedEntities[402] = "fnof".ToCharArray();
            namedEntities[710] = "circ".ToCharArray();
            namedEntities[732] = "tilde".ToCharArray();

            namedEntities[913] = "Alpha".ToCharArray();
            namedEntities[914] = "Beta".ToCharArray();
            namedEntities[915] = "Gamma".ToCharArray();
            namedEntities[916] = "Delta".ToCharArray();
            namedEntities[917] = "Epsilon".ToCharArray();
            namedEntities[918] = "Zeta".ToCharArray();
            namedEntities[919] = "Eta".ToCharArray();
            namedEntities[920] = "Theta".ToCharArray();
            namedEntities[921] = "Iota".ToCharArray();
            namedEntities[922] = "Kappa".ToCharArray();
            namedEntities[923] = "Lambda".ToCharArray();
            namedEntities[924] = "Mu".ToCharArray();
            namedEntities[925] = "Nu".ToCharArray();
            namedEntities[926] = "Xi".ToCharArray();
            namedEntities[927] = "Omicron".ToCharArray();
            namedEntities[928] = "Pi".ToCharArray();
            namedEntities[929] = "Rho".ToCharArray();
            namedEntities[931] = "Sigma".ToCharArray();
            namedEntities[932] = "Tau".ToCharArray();
            namedEntities[933] = "Upsilon".ToCharArray();
            namedEntities[934] = "Phi".ToCharArray();
            namedEntities[935] = "Chi".ToCharArray();
            namedEntities[936] = "Psi".ToCharArray();
            namedEntities[937] = "Omega".ToCharArray();
            namedEntities[945] = "alpha".ToCharArray();
            namedEntities[946] = "beta".ToCharArray();
            namedEntities[947] = "gamma".ToCharArray();
            namedEntities[948] = "delta".ToCharArray();
            namedEntities[949] = "epsilon".ToCharArray();
            namedEntities[950] = "zeta".ToCharArray();
            namedEntities[951] = "eta".ToCharArray();
            namedEntities[952] = "theta".ToCharArray();
            namedEntities[953] = "iota".ToCharArray();
            namedEntities[954] = "kappa".ToCharArray();
            namedEntities[955] = "lambda".ToCharArray();
            namedEntities[956] = "mu".ToCharArray();
            namedEntities[957] = "nu".ToCharArray();
            namedEntities[958] = "xi".ToCharArray();
            namedEntities[959] = "omicron".ToCharArray();
            namedEntities[960] = "pi".ToCharArray();
            namedEntities[961] = "rho".ToCharArray();
            namedEntities[962] = "sigmaf".ToCharArray();
            namedEntities[963] = "sigma".ToCharArray();
            namedEntities[964] = "tau".ToCharArray();
            namedEntities[965] = "upsilon".ToCharArray();
            namedEntities[966] = "phi".ToCharArray();
            namedEntities[967] = "chi".ToCharArray();
            namedEntities[968] = "psi".ToCharArray();
            namedEntities[969] = "omega".ToCharArray();
            namedEntities[977] = "thetasym".ToCharArray();
            namedEntities[978] = "upsih".ToCharArray();
            namedEntities[982] = "piv".ToCharArray();

            namedEntities[0x2002] = "ensp".ToCharArray();
            namedEntities[0x2003] = "emsp".ToCharArray();
            namedEntities[0x2009] = "thinsp".ToCharArray();
            namedEntities[0x200C] = "zwnj".ToCharArray();
            namedEntities[0x200D] = "zwj".ToCharArray();
            namedEntities[0x200E] = "lrm".ToCharArray();
            namedEntities[0x200F] = "rlm".ToCharArray();
            namedEntities[0x2013] = "ndash".ToCharArray();
            namedEntities[0x2014] = "mdash".ToCharArray();
            namedEntities[0x2018] = "lsquo".ToCharArray();
            namedEntities[0x2019] = "rsquo".ToCharArray();
            namedEntities[0x201A] = "sbquo".ToCharArray();
            namedEntities[0x201C] = "ldquo".ToCharArray();
            namedEntities[0x201D] = "rdquo".ToCharArray();
            namedEntities[0x201E] = "bdquo".ToCharArray();
            namedEntities[0x2020] = "dagger".ToCharArray();
            namedEntities[0x2021] = "Dagger".ToCharArray();
            namedEntities[0x2022] = "bull".ToCharArray();
            namedEntities[0x2026] = "hellip".ToCharArray();
            namedEntities[0x2030] = "permil".ToCharArray();
            namedEntities[0x2032] = "prime".ToCharArray();
            namedEntities[0x2033] = "Prime".ToCharArray();
            namedEntities[0x2039] = "lsaquo".ToCharArray();
            namedEntities[0x203A] = "rsaquo".ToCharArray();
            namedEntities[0x203E] = "oline".ToCharArray();
            namedEntities[0x2044] = "frasl".ToCharArray();
            namedEntities[0x20AC] = "euro".ToCharArray();
            namedEntities[0x2111] = "image".ToCharArray();
            namedEntities[0x2118] = "weierp".ToCharArray();
            namedEntities[0x211C] = "real".ToCharArray();
            namedEntities[0x2122] = "trade".ToCharArray();
            namedEntities[0x2135] = "alefsym".ToCharArray();
            namedEntities[0x2190] = "larr".ToCharArray();
            namedEntities[0x2191] = "uarr".ToCharArray();
            namedEntities[0x2192] = "rarr".ToCharArray();
            namedEntities[0x2193] = "darr".ToCharArray();
            namedEntities[0x2194] = "harr".ToCharArray();
            namedEntities[0x21B5] = "crarr".ToCharArray();
            namedEntities[0x21D0] = "lArr".ToCharArray();
            namedEntities[0x21D1] = "uArr".ToCharArray();
            namedEntities[0x21D2] = "rArr".ToCharArray();
            namedEntities[0x21D3] = "dArr".ToCharArray();
            namedEntities[0x21D4] = "hArr".ToCharArray();
            namedEntities[0x2200] = "forall".ToCharArray();
            namedEntities[0x2202] = "part".ToCharArray();
            namedEntities[0x2203] = "exist".ToCharArray();
            namedEntities[0x2205] = "empty".ToCharArray();
            namedEntities[0x2207] = "nabla".ToCharArray();
            namedEntities[0x2208] = "isin".ToCharArray();
            namedEntities[0x2209] = "notin".ToCharArray();
            namedEntities[0x220B] = "ni".ToCharArray();
            namedEntities[0x220F] = "prod".ToCharArray();
            namedEntities[0x2211] = "sum".ToCharArray();
            namedEntities[0x2212] = "minus".ToCharArray();
            namedEntities[0x2217] = "lowast".ToCharArray();
            namedEntities[0x221A] = "radic".ToCharArray();
            namedEntities[0x221D] = "prop".ToCharArray();
            namedEntities[0x221E] = "infin".ToCharArray();
            namedEntities[0x2220] = "ang".ToCharArray();
            namedEntities[0x2227] = "and".ToCharArray();
            namedEntities[0x2228] = "or".ToCharArray();
            namedEntities[0x2229] = "cap".ToCharArray();
            namedEntities[0x222A] = "cup".ToCharArray();
            namedEntities[0x222B] = "int".ToCharArray();
            namedEntities[0x2234] = "there4".ToCharArray();
            namedEntities[0x223C] = "sim".ToCharArray();
            namedEntities[0x2245] = "cong".ToCharArray();
            namedEntities[0x2248] = "asymp".ToCharArray();
            namedEntities[0x2260] = "ne".ToCharArray();
            namedEntities[0x2261] = "equiv".ToCharArray();
            namedEntities[0x2264] = "le".ToCharArray();
            namedEntities[0x2265] = "ge".ToCharArray();
            namedEntities[0x2282] = "sub".ToCharArray();
            namedEntities[0x2283] = "sup".ToCharArray();
            namedEntities[0x2284] = "nsub".ToCharArray();
            namedEntities[0x2286] = "sube".ToCharArray();
            namedEntities[0x2287] = "supe".ToCharArray();
            namedEntities[0x2295] = "oplus".ToCharArray();
            namedEntities[0x2297] = "otimes".ToCharArray();
            namedEntities[0x22A5] = "perp".ToCharArray();
            namedEntities[0x22C5] = "sdot".ToCharArray();
            namedEntities[0x2308] = "lceil".ToCharArray();
            namedEntities[0x2309] = "rceil".ToCharArray();
            namedEntities[0x230A] = "lfloor".ToCharArray();
            namedEntities[0x230B] = "rfloor".ToCharArray();
            namedEntities[0x2329] = "lang".ToCharArray();
            namedEntities[0x232A] = "rang".ToCharArray();
            namedEntities[0x25CA] = "loz".ToCharArray();
            namedEntities[0x2660] = "spades".ToCharArray();
            namedEntities[0x2663] = "clubs".ToCharArray();
            namedEntities[0x2665] = "hearts".ToCharArray();
            namedEntities[0x2666] = "diams".ToCharArray();

            return namedEntities;
        }
    }
}
