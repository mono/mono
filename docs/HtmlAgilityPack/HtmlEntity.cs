// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>
using System;
using System.Collections;
using System.Text;

namespace HtmlAgilityPack
{
    /// <summary>
    /// A utility class to replace special characters by entities and vice-versa.
    /// Follows HTML 4.0 specification found at http://www.w3.org/TR/html4/sgml/entities.html
    /// </summary>
    public class HtmlEntity
    {
        #region Static Members

        private static readonly int _maxEntitySize;
        private static Hashtable _entityName;
        private static Hashtable _entityValue;

        /// <summary>
        /// A collection of entities indexed by name.
        /// </summary>
        public static Hashtable EntityName
        {
            get { return _entityName; }
        }

        /// <summary>
        /// A collection of entities indexed by value.
        /// </summary>
        public static Hashtable EntityValue
        {
            get { return _entityValue; }
        }

        #endregion

        #region Constructors

        static HtmlEntity()
        {
            _entityName = new Hashtable();
            _entityValue = new Hashtable();

            #region Entities Definition

            _entityValue.Add("nbsp", 160); // no-break space = non-breaking space, U+00A0 ISOnum 
            _entityName.Add(160, "nbsp");
            _entityValue.Add("iexcl", 161); // inverted exclamation mark, U+00A1 ISOnum 
            _entityName.Add(161, "iexcl");
            _entityValue.Add("cent", 162); // cent sign, U+00A2 ISOnum 
            _entityName.Add(162, "cent");
            _entityValue.Add("pound", 163); // pound sign, U+00A3 ISOnum 
            _entityName.Add(163, "pound");
            _entityValue.Add("curren", 164); // currency sign, U+00A4 ISOnum 
            _entityName.Add(164, "curren");
            _entityValue.Add("yen", 165); // yen sign = yuan sign, U+00A5 ISOnum 
            _entityName.Add(165, "yen");
            _entityValue.Add("brvbar", 166); // broken bar = broken vertical bar, U+00A6 ISOnum 
            _entityName.Add(166, "brvbar");
            _entityValue.Add("sect", 167); // section sign, U+00A7 ISOnum 
            _entityName.Add(167, "sect");
            _entityValue.Add("uml", 168); // diaeresis = spacing diaeresis, U+00A8 ISOdia 
            _entityName.Add(168, "uml");
            _entityValue.Add("copy", 169); // copyright sign, U+00A9 ISOnum 
            _entityName.Add(169, "copy");
            _entityValue.Add("ordf", 170); // feminine ordinal indicator, U+00AA ISOnum 
            _entityName.Add(170, "ordf");
            _entityValue.Add("laquo", 171);
                // left-pointing double angle quotation mark = left pointing guillemet, U+00AB ISOnum 
            _entityName.Add(171, "laquo");
            _entityValue.Add("not", 172); // not sign, U+00AC ISOnum 
            _entityName.Add(172, "not");
            _entityValue.Add("shy", 173); // soft hyphen = discretionary hyphen, U+00AD ISOnum 
            _entityName.Add(173, "shy");
            _entityValue.Add("reg", 174); // registered sign = registered trade mark sign, U+00AE ISOnum 
            _entityName.Add(174, "reg");
            _entityValue.Add("macr", 175); // macron = spacing macron = overline = APL overbar, U+00AF ISOdia 
            _entityName.Add(175, "macr");
            _entityValue.Add("deg", 176); // degree sign, U+00B0 ISOnum 
            _entityName.Add(176, "deg");
            _entityValue.Add("plusmn", 177); // plus-minus sign = plus-or-minus sign, U+00B1 ISOnum 
            _entityName.Add(177, "plusmn");
            _entityValue.Add("sup2", 178); // superscript two = superscript digit two = squared, U+00B2 ISOnum 
            _entityName.Add(178, "sup2");
            _entityValue.Add("sup3", 179); // superscript three = superscript digit three = cubed, U+00B3 ISOnum 
            _entityName.Add(179, "sup3");
            _entityValue.Add("acute", 180); // acute accent = spacing acute, U+00B4 ISOdia 
            _entityName.Add(180, "acute");
            _entityValue.Add("micro", 181); // micro sign, U+00B5 ISOnum 
            _entityName.Add(181, "micro");
            _entityValue.Add("para", 182); // pilcrow sign = paragraph sign, U+00B6 ISOnum 
            _entityName.Add(182, "para");
            _entityValue.Add("middot", 183); // middle dot = Georgian comma = Greek middle dot, U+00B7 ISOnum 
            _entityName.Add(183, "middot");
            _entityValue.Add("cedil", 184); // cedilla = spacing cedilla, U+00B8 ISOdia 
            _entityName.Add(184, "cedil");
            _entityValue.Add("sup1", 185); // superscript one = superscript digit one, U+00B9 ISOnum 
            _entityName.Add(185, "sup1");
            _entityValue.Add("ordm", 186); // masculine ordinal indicator, U+00BA ISOnum 
            _entityName.Add(186, "ordm");
            _entityValue.Add("raquo", 187);
                // right-pointing double angle quotation mark = right pointing guillemet, U+00BB ISOnum 
            _entityName.Add(187, "raquo");
            _entityValue.Add("frac14", 188); // vulgar fraction one quarter = fraction one quarter, U+00BC ISOnum 
            _entityName.Add(188, "frac14");
            _entityValue.Add("frac12", 189); // vulgar fraction one half = fraction one half, U+00BD ISOnum 
            _entityName.Add(189, "frac12");
            _entityValue.Add("frac34", 190); // vulgar fraction three quarters = fraction three quarters, U+00BE ISOnum 
            _entityName.Add(190, "frac34");
            _entityValue.Add("iquest", 191); // inverted question mark = turned question mark, U+00BF ISOnum 
            _entityName.Add(191, "iquest");
            _entityValue.Add("Agrave", 192);
                // latin capital letter A with grave = latin capital letter A grave, U+00C0 ISOlat1 
            _entityName.Add(192, "Agrave");
            _entityValue.Add("Aacute", 193); // latin capital letter A with acute, U+00C1 ISOlat1 
            _entityName.Add(193, "Aacute");
            _entityValue.Add("Acirc", 194); // latin capital letter A with circumflex, U+00C2 ISOlat1 
            _entityName.Add(194, "Acirc");
            _entityValue.Add("Atilde", 195); // latin capital letter A with tilde, U+00C3 ISOlat1 
            _entityName.Add(195, "Atilde");
            _entityValue.Add("Auml", 196); // latin capital letter A with diaeresis, U+00C4 ISOlat1 
            _entityName.Add(196, "Auml");
            _entityValue.Add("Aring", 197);
                // latin capital letter A with ring above = latin capital letter A ring, U+00C5 ISOlat1 
            _entityName.Add(197, "Aring");
            _entityValue.Add("AElig", 198); // latin capital letter AE = latin capital ligature AE, U+00C6 ISOlat1 
            _entityName.Add(198, "AElig");
            _entityValue.Add("Ccedil", 199); // latin capital letter C with cedilla, U+00C7 ISOlat1 
            _entityName.Add(199, "Ccedil");
            _entityValue.Add("Egrave", 200); // latin capital letter E with grave, U+00C8 ISOlat1 
            _entityName.Add(200, "Egrave");
            _entityValue.Add("Eacute", 201); // latin capital letter E with acute, U+00C9 ISOlat1 
            _entityName.Add(201, "Eacute");
            _entityValue.Add("Ecirc", 202); // latin capital letter E with circumflex, U+00CA ISOlat1 
            _entityName.Add(202, "Ecirc");
            _entityValue.Add("Euml", 203); // latin capital letter E with diaeresis, U+00CB ISOlat1 
            _entityName.Add(203, "Euml");
            _entityValue.Add("Igrave", 204); // latin capital letter I with grave, U+00CC ISOlat1 
            _entityName.Add(204, "Igrave");
            _entityValue.Add("Iacute", 205); // latin capital letter I with acute, U+00CD ISOlat1 
            _entityName.Add(205, "Iacute");
            _entityValue.Add("Icirc", 206); // latin capital letter I with circumflex, U+00CE ISOlat1 
            _entityName.Add(206, "Icirc");
            _entityValue.Add("Iuml", 207); // latin capital letter I with diaeresis, U+00CF ISOlat1 
            _entityName.Add(207, "Iuml");
            _entityValue.Add("ETH", 208); // latin capital letter ETH, U+00D0 ISOlat1 
            _entityName.Add(208, "ETH");
            _entityValue.Add("Ntilde", 209); // latin capital letter N with tilde, U+00D1 ISOlat1 
            _entityName.Add(209, "Ntilde");
            _entityValue.Add("Ograve", 210); // latin capital letter O with grave, U+00D2 ISOlat1 
            _entityName.Add(210, "Ograve");
            _entityValue.Add("Oacute", 211); // latin capital letter O with acute, U+00D3 ISOlat1 
            _entityName.Add(211, "Oacute");
            _entityValue.Add("Ocirc", 212); // latin capital letter O with circumflex, U+00D4 ISOlat1 
            _entityName.Add(212, "Ocirc");
            _entityValue.Add("Otilde", 213); // latin capital letter O with tilde, U+00D5 ISOlat1 
            _entityName.Add(213, "Otilde");
            _entityValue.Add("Ouml", 214); // latin capital letter O with diaeresis, U+00D6 ISOlat1 
            _entityName.Add(214, "Ouml");
            _entityValue.Add("times", 215); // multiplication sign, U+00D7 ISOnum 
            _entityName.Add(215, "times");
            _entityValue.Add("Oslash", 216);
                // latin capital letter O with stroke = latin capital letter O slash, U+00D8 ISOlat1 
            _entityName.Add(216, "Oslash");
            _entityValue.Add("Ugrave", 217); // latin capital letter U with grave, U+00D9 ISOlat1 
            _entityName.Add(217, "Ugrave");
            _entityValue.Add("Uacute", 218); // latin capital letter U with acute, U+00DA ISOlat1 
            _entityName.Add(218, "Uacute");
            _entityValue.Add("Ucirc", 219); // latin capital letter U with circumflex, U+00DB ISOlat1 
            _entityName.Add(219, "Ucirc");
            _entityValue.Add("Uuml", 220); // latin capital letter U with diaeresis, U+00DC ISOlat1 
            _entityName.Add(220, "Uuml");
            _entityValue.Add("Yacute", 221); // latin capital letter Y with acute, U+00DD ISOlat1 
            _entityName.Add(221, "Yacute");
            _entityValue.Add("THORN", 222); // latin capital letter THORN, U+00DE ISOlat1 
            _entityName.Add(222, "THORN");
            _entityValue.Add("szlig", 223); // latin small letter sharp s = ess-zed, U+00DF ISOlat1 
            _entityName.Add(223, "szlig");
            _entityValue.Add("agrave", 224);
                // latin small letter a with grave = latin small letter a grave, U+00E0 ISOlat1 
            _entityName.Add(224, "agrave");
            _entityValue.Add("aacute", 225); // latin small letter a with acute, U+00E1 ISOlat1 
            _entityName.Add(225, "aacute");
            _entityValue.Add("acirc", 226); // latin small letter a with circumflex, U+00E2 ISOlat1 
            _entityName.Add(226, "acirc");
            _entityValue.Add("atilde", 227); // latin small letter a with tilde, U+00E3 ISOlat1 
            _entityName.Add(227, "atilde");
            _entityValue.Add("auml", 228); // latin small letter a with diaeresis, U+00E4 ISOlat1 
            _entityName.Add(228, "auml");
            _entityValue.Add("aring", 229);
                // latin small letter a with ring above = latin small letter a ring, U+00E5 ISOlat1 
            _entityName.Add(229, "aring");
            _entityValue.Add("aelig", 230); // latin small letter ae = latin small ligature ae, U+00E6 ISOlat1 
            _entityName.Add(230, "aelig");
            _entityValue.Add("ccedil", 231); // latin small letter c with cedilla, U+00E7 ISOlat1 
            _entityName.Add(231, "ccedil");
            _entityValue.Add("egrave", 232); // latin small letter e with grave, U+00E8 ISOlat1 
            _entityName.Add(232, "egrave");
            _entityValue.Add("eacute", 233); // latin small letter e with acute, U+00E9 ISOlat1 
            _entityName.Add(233, "eacute");
            _entityValue.Add("ecirc", 234); // latin small letter e with circumflex, U+00EA ISOlat1 
            _entityName.Add(234, "ecirc");
            _entityValue.Add("euml", 235); // latin small letter e with diaeresis, U+00EB ISOlat1 
            _entityName.Add(235, "euml");
            _entityValue.Add("igrave", 236); // latin small letter i with grave, U+00EC ISOlat1 
            _entityName.Add(236, "igrave");
            _entityValue.Add("iacute", 237); // latin small letter i with acute, U+00ED ISOlat1 
            _entityName.Add(237, "iacute");
            _entityValue.Add("icirc", 238); // latin small letter i with circumflex, U+00EE ISOlat1 
            _entityName.Add(238, "icirc");
            _entityValue.Add("iuml", 239); // latin small letter i with diaeresis, U+00EF ISOlat1 
            _entityName.Add(239, "iuml");
            _entityValue.Add("eth", 240); // latin small letter eth, U+00F0 ISOlat1 
            _entityName.Add(240, "eth");
            _entityValue.Add("ntilde", 241); // latin small letter n with tilde, U+00F1 ISOlat1 
            _entityName.Add(241, "ntilde");
            _entityValue.Add("ograve", 242); // latin small letter o with grave, U+00F2 ISOlat1 
            _entityName.Add(242, "ograve");
            _entityValue.Add("oacute", 243); // latin small letter o with acute, U+00F3 ISOlat1 
            _entityName.Add(243, "oacute");
            _entityValue.Add("ocirc", 244); // latin small letter o with circumflex, U+00F4 ISOlat1 
            _entityName.Add(244, "ocirc");
            _entityValue.Add("otilde", 245); // latin small letter o with tilde, U+00F5 ISOlat1 
            _entityName.Add(245, "otilde");
            _entityValue.Add("ouml", 246); // latin small letter o with diaeresis, U+00F6 ISOlat1 
            _entityName.Add(246, "ouml");
            _entityValue.Add("divide", 247); // division sign, U+00F7 ISOnum 
            _entityName.Add(247, "divide");
            _entityValue.Add("oslash", 248);
                // latin small letter o with stroke, = latin small letter o slash, U+00F8 ISOlat1 
            _entityName.Add(248, "oslash");
            _entityValue.Add("ugrave", 249); // latin small letter u with grave, U+00F9 ISOlat1 
            _entityName.Add(249, "ugrave");
            _entityValue.Add("uacute", 250); // latin small letter u with acute, U+00FA ISOlat1 
            _entityName.Add(250, "uacute");
            _entityValue.Add("ucirc", 251); // latin small letter u with circumflex, U+00FB ISOlat1 
            _entityName.Add(251, "ucirc");
            _entityValue.Add("uuml", 252); // latin small letter u with diaeresis, U+00FC ISOlat1 
            _entityName.Add(252, "uuml");
            _entityValue.Add("yacute", 253); // latin small letter y with acute, U+00FD ISOlat1 
            _entityName.Add(253, "yacute");
            _entityValue.Add("thorn", 254); // latin small letter thorn, U+00FE ISOlat1 
            _entityName.Add(254, "thorn");
            _entityValue.Add("yuml", 255); // latin small letter y with diaeresis, U+00FF ISOlat1 
            _entityName.Add(255, "yuml");
            _entityValue.Add("fnof", 402); // latin small f with hook = function = florin, U+0192 ISOtech 
            _entityName.Add(402, "fnof");
            _entityValue.Add("Alpha", 913); // greek capital letter alpha, U+0391 
            _entityName.Add(913, "Alpha");
            _entityValue.Add("Beta", 914); // greek capital letter beta, U+0392 
            _entityName.Add(914, "Beta");
            _entityValue.Add("Gamma", 915); // greek capital letter gamma, U+0393 ISOgrk3 
            _entityName.Add(915, "Gamma");
            _entityValue.Add("Delta", 916); // greek capital letter delta, U+0394 ISOgrk3 
            _entityName.Add(916, "Delta");
            _entityValue.Add("Epsilon", 917); // greek capital letter epsilon, U+0395 
            _entityName.Add(917, "Epsilon");
            _entityValue.Add("Zeta", 918); // greek capital letter zeta, U+0396 
            _entityName.Add(918, "Zeta");
            _entityValue.Add("Eta", 919); // greek capital letter eta, U+0397 
            _entityName.Add(919, "Eta");
            _entityValue.Add("Theta", 920); // greek capital letter theta, U+0398 ISOgrk3 
            _entityName.Add(920, "Theta");
            _entityValue.Add("Iota", 921); // greek capital letter iota, U+0399 
            _entityName.Add(921, "Iota");
            _entityValue.Add("Kappa", 922); // greek capital letter kappa, U+039A 
            _entityName.Add(922, "Kappa");
            _entityValue.Add("Lambda", 923); // greek capital letter lambda, U+039B ISOgrk3 
            _entityName.Add(923, "Lambda");
            _entityValue.Add("Mu", 924); // greek capital letter mu, U+039C 
            _entityName.Add(924, "Mu");
            _entityValue.Add("Nu", 925); // greek capital letter nu, U+039D 
            _entityName.Add(925, "Nu");
            _entityValue.Add("Xi", 926); // greek capital letter xi, U+039E ISOgrk3 
            _entityName.Add(926, "Xi");
            _entityValue.Add("Omicron", 927); // greek capital letter omicron, U+039F 
            _entityName.Add(927, "Omicron");
            _entityValue.Add("Pi", 928); // greek capital letter pi, U+03A0 ISOgrk3 
            _entityName.Add(928, "Pi");
            _entityValue.Add("Rho", 929); // greek capital letter rho, U+03A1 
            _entityName.Add(929, "Rho");
            _entityValue.Add("Sigma", 931); // greek capital letter sigma, U+03A3 ISOgrk3 
            _entityName.Add(931, "Sigma");
            _entityValue.Add("Tau", 932); // greek capital letter tau, U+03A4 
            _entityName.Add(932, "Tau");
            _entityValue.Add("Upsilon", 933); // greek capital letter upsilon, U+03A5 ISOgrk3 
            _entityName.Add(933, "Upsilon");
            _entityValue.Add("Phi", 934); // greek capital letter phi, U+03A6 ISOgrk3 
            _entityName.Add(934, "Phi");
            _entityValue.Add("Chi", 935); // greek capital letter chi, U+03A7 
            _entityName.Add(935, "Chi");
            _entityValue.Add("Psi", 936); // greek capital letter psi, U+03A8 ISOgrk3 
            _entityName.Add(936, "Psi");
            _entityValue.Add("Omega", 937); // greek capital letter omega, U+03A9 ISOgrk3 
            _entityName.Add(937, "Omega");
            _entityValue.Add("alpha", 945); // greek small letter alpha, U+03B1 ISOgrk3 
            _entityName.Add(945, "alpha");
            _entityValue.Add("beta", 946); // greek small letter beta, U+03B2 ISOgrk3 
            _entityName.Add(946, "beta");
            _entityValue.Add("gamma", 947); // greek small letter gamma, U+03B3 ISOgrk3 
            _entityName.Add(947, "gamma");
            _entityValue.Add("delta", 948); // greek small letter delta, U+03B4 ISOgrk3 
            _entityName.Add(948, "delta");
            _entityValue.Add("epsilon", 949); // greek small letter epsilon, U+03B5 ISOgrk3 
            _entityName.Add(949, "epsilon");
            _entityValue.Add("zeta", 950); // greek small letter zeta, U+03B6 ISOgrk3 
            _entityName.Add(950, "zeta");
            _entityValue.Add("eta", 951); // greek small letter eta, U+03B7 ISOgrk3 
            _entityName.Add(951, "eta");
            _entityValue.Add("theta", 952); // greek small letter theta, U+03B8 ISOgrk3 
            _entityName.Add(952, "theta");
            _entityValue.Add("iota", 953); // greek small letter iota, U+03B9 ISOgrk3 
            _entityName.Add(953, "iota");
            _entityValue.Add("kappa", 954); // greek small letter kappa, U+03BA ISOgrk3 
            _entityName.Add(954, "kappa");
            _entityValue.Add("lambda", 955); // greek small letter lambda, U+03BB ISOgrk3 
            _entityName.Add(955, "lambda");
            _entityValue.Add("mu", 956); // greek small letter mu, U+03BC ISOgrk3 
            _entityName.Add(956, "mu");
            _entityValue.Add("nu", 957); // greek small letter nu, U+03BD ISOgrk3 
            _entityName.Add(957, "nu");
            _entityValue.Add("xi", 958); // greek small letter xi, U+03BE ISOgrk3 
            _entityName.Add(958, "xi");
            _entityValue.Add("omicron", 959); // greek small letter omicron, U+03BF NEW 
            _entityName.Add(959, "omicron");
            _entityValue.Add("pi", 960); // greek small letter pi, U+03C0 ISOgrk3 
            _entityName.Add(960, "pi");
            _entityValue.Add("rho", 961); // greek small letter rho, U+03C1 ISOgrk3 
            _entityName.Add(961, "rho");
            _entityValue.Add("sigmaf", 962); // greek small letter final sigma, U+03C2 ISOgrk3 
            _entityName.Add(962, "sigmaf");
            _entityValue.Add("sigma", 963); // greek small letter sigma, U+03C3 ISOgrk3 
            _entityName.Add(963, "sigma");
            _entityValue.Add("tau", 964); // greek small letter tau, U+03C4 ISOgrk3 
            _entityName.Add(964, "tau");
            _entityValue.Add("upsilon", 965); // greek small letter upsilon, U+03C5 ISOgrk3 
            _entityName.Add(965, "upsilon");
            _entityValue.Add("phi", 966); // greek small letter phi, U+03C6 ISOgrk3 
            _entityName.Add(966, "phi");
            _entityValue.Add("chi", 967); // greek small letter chi, U+03C7 ISOgrk3 
            _entityName.Add(967, "chi");
            _entityValue.Add("psi", 968); // greek small letter psi, U+03C8 ISOgrk3 
            _entityName.Add(968, "psi");
            _entityValue.Add("omega", 969); // greek small letter omega, U+03C9 ISOgrk3 
            _entityName.Add(969, "omega");
            _entityValue.Add("thetasym", 977); // greek small letter theta symbol, U+03D1 NEW 
            _entityName.Add(977, "thetasym");
            _entityValue.Add("upsih", 978); // greek upsilon with hook symbol, U+03D2 NEW 
            _entityName.Add(978, "upsih");
            _entityValue.Add("piv", 982); // greek pi symbol, U+03D6 ISOgrk3 
            _entityName.Add(982, "piv");
            _entityValue.Add("bull", 8226); // bullet = black small circle, U+2022 ISOpub 
            _entityName.Add(8226, "bull");
            _entityValue.Add("hellip", 8230); // horizontal ellipsis = three dot leader, U+2026 ISOpub 
            _entityName.Add(8230, "hellip");
            _entityValue.Add("prime", 8242); // prime = minutes = feet, U+2032 ISOtech 
            _entityName.Add(8242, "prime");
            _entityValue.Add("Prime", 8243); // double prime = seconds = inches, U+2033 ISOtech 
            _entityName.Add(8243, "Prime");
            _entityValue.Add("oline", 8254); // overline = spacing overscore, U+203E NEW 
            _entityName.Add(8254, "oline");
            _entityValue.Add("frasl", 8260); // fraction slash, U+2044 NEW 
            _entityName.Add(8260, "frasl");
            _entityValue.Add("weierp", 8472); // script capital P = power set = Weierstrass p, U+2118 ISOamso 
            _entityName.Add(8472, "weierp");
            _entityValue.Add("image", 8465); // blackletter capital I = imaginary part, U+2111 ISOamso 
            _entityName.Add(8465, "image");
            _entityValue.Add("real", 8476); // blackletter capital R = real part symbol, U+211C ISOamso 
            _entityName.Add(8476, "real");
            _entityValue.Add("trade", 8482); // trade mark sign, U+2122 ISOnum 
            _entityName.Add(8482, "trade");
            _entityValue.Add("alefsym", 8501); // alef symbol = first transfinite cardinal, U+2135 NEW 
            _entityName.Add(8501, "alefsym");
            _entityValue.Add("larr", 8592); // leftwards arrow, U+2190 ISOnum 
            _entityName.Add(8592, "larr");
            _entityValue.Add("uarr", 8593); // upwards arrow, U+2191 ISOnum
            _entityName.Add(8593, "uarr");
            _entityValue.Add("rarr", 8594); // rightwards arrow, U+2192 ISOnum 
            _entityName.Add(8594, "rarr");
            _entityValue.Add("darr", 8595); // downwards arrow, U+2193 ISOnum 
            _entityName.Add(8595, "darr");
            _entityValue.Add("harr", 8596); // left right arrow, U+2194 ISOamsa 
            _entityName.Add(8596, "harr");
            _entityValue.Add("crarr", 8629); // downwards arrow with corner leftwards = carriage return, U+21B5 NEW 
            _entityName.Add(8629, "crarr");
            _entityValue.Add("lArr", 8656); // leftwards double arrow, U+21D0 ISOtech 
            _entityName.Add(8656, "lArr");
            _entityValue.Add("uArr", 8657); // upwards double arrow, U+21D1 ISOamsa 
            _entityName.Add(8657, "uArr");
            _entityValue.Add("rArr", 8658); // rightwards double arrow, U+21D2 ISOtech 
            _entityName.Add(8658, "rArr");
            _entityValue.Add("dArr", 8659); // downwards double arrow, U+21D3 ISOamsa 
            _entityName.Add(8659, "dArr");
            _entityValue.Add("hArr", 8660); // left right double arrow, U+21D4 ISOamsa 
            _entityName.Add(8660, "hArr");
            _entityValue.Add("forall", 8704); // for all, U+2200 ISOtech 
            _entityName.Add(8704, "forall");
            _entityValue.Add("part", 8706); // partial differential, U+2202 ISOtech 
            _entityName.Add(8706, "part");
            _entityValue.Add("exist", 8707); // there exists, U+2203 ISOtech 
            _entityName.Add(8707, "exist");
            _entityValue.Add("empty", 8709); // empty set = null set = diameter, U+2205 ISOamso 
            _entityName.Add(8709, "empty");
            _entityValue.Add("nabla", 8711); // nabla = backward difference, U+2207 ISOtech 
            _entityName.Add(8711, "nabla");
            _entityValue.Add("isin", 8712); // element of, U+2208 ISOtech 
            _entityName.Add(8712, "isin");
            _entityValue.Add("notin", 8713); // not an element of, U+2209 ISOtech 
            _entityName.Add(8713, "notin");
            _entityValue.Add("ni", 8715); // contains as member, U+220B ISOtech 
            _entityName.Add(8715, "ni");
            _entityValue.Add("prod", 8719); // n-ary product = product sign, U+220F ISOamsb 
            _entityName.Add(8719, "prod");
            _entityValue.Add("sum", 8721); // n-ary sumation, U+2211 ISOamsb 
            _entityName.Add(8721, "sum");
            _entityValue.Add("minus", 8722); // minus sign, U+2212 ISOtech 
            _entityName.Add(8722, "minus");
            _entityValue.Add("lowast", 8727); // asterisk operator, U+2217 ISOtech 
            _entityName.Add(8727, "lowast");
            _entityValue.Add("radic", 8730); // square root = radical sign, U+221A ISOtech 
            _entityName.Add(8730, "radic");
            _entityValue.Add("prop", 8733); // proportional to, U+221D ISOtech 
            _entityName.Add(8733, "prop");
            _entityValue.Add("infin", 8734); // infinity, U+221E ISOtech 
            _entityName.Add(8734, "infin");
            _entityValue.Add("ang", 8736); // angle, U+2220 ISOamso 
            _entityName.Add(8736, "ang");
            _entityValue.Add("and", 8743); // logical and = wedge, U+2227 ISOtech 
            _entityName.Add(8743, "and");
            _entityValue.Add("or", 8744); // logical or = vee, U+2228 ISOtech 
            _entityName.Add(8744, "or");
            _entityValue.Add("cap", 8745); // intersection = cap, U+2229 ISOtech 
            _entityName.Add(8745, "cap");
            _entityValue.Add("cup", 8746); // union = cup, U+222A ISOtech 
            _entityName.Add(8746, "cup");
            _entityValue.Add("int", 8747); // integral, U+222B ISOtech 
            _entityName.Add(8747, "int");
            _entityValue.Add("there4", 8756); // therefore, U+2234 ISOtech 
            _entityName.Add(8756, "there4");
            _entityValue.Add("sim", 8764); // tilde operator = varies with = similar to, U+223C ISOtech 
            _entityName.Add(8764, "sim");
            _entityValue.Add("cong", 8773); // approximately equal to, U+2245 ISOtech 
            _entityName.Add(8773, "cong");
            _entityValue.Add("asymp", 8776); // almost equal to = asymptotic to, U+2248 ISOamsr 
            _entityName.Add(8776, "asymp");
            _entityValue.Add("ne", 8800); // not equal to, U+2260 ISOtech 
            _entityName.Add(8800, "ne");
            _entityValue.Add("equiv", 8801); // identical to, U+2261 ISOtech 
            _entityName.Add(8801, "equiv");
            _entityValue.Add("le", 8804); // less-than or equal to, U+2264 ISOtech 
            _entityName.Add(8804, "le");
            _entityValue.Add("ge", 8805); // greater-than or equal to, U+2265 ISOtech 
            _entityName.Add(8805, "ge");
            _entityValue.Add("sub", 8834); // subset of, U+2282 ISOtech 
            _entityName.Add(8834, "sub");
            _entityValue.Add("sup", 8835); // superset of, U+2283 ISOtech 
            _entityName.Add(8835, "sup");
            _entityValue.Add("nsub", 8836); // not a subset of, U+2284 ISOamsn 
            _entityName.Add(8836, "nsub");
            _entityValue.Add("sube", 8838); // subset of or equal to, U+2286 ISOtech 
            _entityName.Add(8838, "sube");
            _entityValue.Add("supe", 8839); // superset of or equal to, U+2287 ISOtech 
            _entityName.Add(8839, "supe");
            _entityValue.Add("oplus", 8853); // circled plus = direct sum, U+2295 ISOamsb 
            _entityName.Add(8853, "oplus");
            _entityValue.Add("otimes", 8855); // circled times = vector product, U+2297 ISOamsb 
            _entityName.Add(8855, "otimes");
            _entityValue.Add("perp", 8869); // up tack = orthogonal to = perpendicular, U+22A5 ISOtech 
            _entityName.Add(8869, "perp");
            _entityValue.Add("sdot", 8901); // dot operator, U+22C5 ISOamsb 
            _entityName.Add(8901, "sdot");
            _entityValue.Add("lceil", 8968); // left ceiling = apl upstile, U+2308 ISOamsc 
            _entityName.Add(8968, "lceil");
            _entityValue.Add("rceil", 8969); // right ceiling, U+2309 ISOamsc 
            _entityName.Add(8969, "rceil");
            _entityValue.Add("lfloor", 8970); // left floor = apl downstile, U+230A ISOamsc 
            _entityName.Add(8970, "lfloor");
            _entityValue.Add("rfloor", 8971); // right floor, U+230B ISOamsc 
            _entityName.Add(8971, "rfloor");
            _entityValue.Add("lang", 9001); // left-pointing angle bracket = bra, U+2329 ISOtech 
            _entityName.Add(9001, "lang");
            _entityValue.Add("rang", 9002); // right-pointing angle bracket = ket, U+232A ISOtech 
            _entityName.Add(9002, "rang");
            _entityValue.Add("loz", 9674); // lozenge, U+25CA ISOpub 
            _entityName.Add(9674, "loz");
            _entityValue.Add("spades", 9824); // black spade suit, U+2660 ISOpub 
            _entityName.Add(9824, "spades");
            _entityValue.Add("clubs", 9827); // black club suit = shamrock, U+2663 ISOpub 
            _entityName.Add(9827, "clubs");
            _entityValue.Add("hearts", 9829); // black heart suit = valentine, U+2665 ISOpub 
            _entityName.Add(9829, "hearts");
            _entityValue.Add("diams", 9830); // black diamond suit, U+2666 ISOpub 
            _entityName.Add(9830, "diams");
            _entityValue.Add("quot", 34); // quotation mark = APL quote, U+0022 ISOnum 
            _entityName.Add(34, "quot");
            _entityValue.Add("amp", 38); // ampersand, U+0026 ISOnum 
            _entityName.Add(38, "amp");
            _entityValue.Add("lt", 60); // less-than sign, U+003C ISOnum 
            _entityName.Add(60, "lt");
            _entityValue.Add("gt", 62); // greater-than sign, U+003E ISOnum 
            _entityName.Add(62, "gt");
            _entityValue.Add("OElig", 338); // latin capital ligature OE, U+0152 ISOlat2 
            _entityName.Add(338, "OElig");
            _entityValue.Add("oelig", 339); // latin small ligature oe, U+0153 ISOlat2 
            _entityName.Add(339, "oelig");
            _entityValue.Add("Scaron", 352); // latin capital letter S with caron, U+0160 ISOlat2 
            _entityName.Add(352, "Scaron");
            _entityValue.Add("scaron", 353); // latin small letter s with caron, U+0161 ISOlat2 
            _entityName.Add(353, "scaron");
            _entityValue.Add("Yuml", 376); // latin capital letter Y with diaeresis, U+0178 ISOlat2 
            _entityName.Add(376, "Yuml");
            _entityValue.Add("circ", 710); // modifier letter circumflex accent, U+02C6 ISOpub 
            _entityName.Add(710, "circ");
            _entityValue.Add("tilde", 732); // small tilde, U+02DC ISOdia 
            _entityName.Add(732, "tilde");
            _entityValue.Add("ensp", 8194); // en space, U+2002 ISOpub 
            _entityName.Add(8194, "ensp");
            _entityValue.Add("emsp", 8195); // em space, U+2003 ISOpub 
            _entityName.Add(8195, "emsp");
            _entityValue.Add("thinsp", 8201); // thin space, U+2009 ISOpub 
            _entityName.Add(8201, "thinsp");
            _entityValue.Add("zwnj", 8204); // zero width non-joiner, U+200C NEW RFC 2070 
            _entityName.Add(8204, "zwnj");
            _entityValue.Add("zwj", 8205); // zero width joiner, U+200D NEW RFC 2070 
            _entityName.Add(8205, "zwj");
            _entityValue.Add("lrm", 8206); // left-to-right mark, U+200E NEW RFC 2070 
            _entityName.Add(8206, "lrm");
            _entityValue.Add("rlm", 8207); // right-to-left mark, U+200F NEW RFC 2070 
            _entityName.Add(8207, "rlm");
            _entityValue.Add("ndash", 8211); // en dash, U+2013 ISOpub 
            _entityName.Add(8211, "ndash");
            _entityValue.Add("mdash", 8212); // em dash, U+2014 ISOpub 
            _entityName.Add(8212, "mdash");
            _entityValue.Add("lsquo", 8216); // left single quotation mark, U+2018 ISOnum 
            _entityName.Add(8216, "lsquo");
            _entityValue.Add("rsquo", 8217); // right single quotation mark, U+2019 ISOnum 
            _entityName.Add(8217, "rsquo");
            _entityValue.Add("sbquo", 8218); // single low-9 quotation mark, U+201A NEW 
            _entityName.Add(8218, "sbquo");
            _entityValue.Add("ldquo", 8220); // left double quotation mark, U+201C ISOnum 
            _entityName.Add(8220, "ldquo");
            _entityValue.Add("rdquo", 8221); // right double quotation mark, U+201D ISOnum 
            _entityName.Add(8221, "rdquo");
            _entityValue.Add("bdquo", 8222); // double low-9 quotation mark, U+201E NEW 
            _entityName.Add(8222, "bdquo");
            _entityValue.Add("dagger", 8224); // dagger, U+2020 ISOpub 
            _entityName.Add(8224, "dagger");
            _entityValue.Add("Dagger", 8225); // double dagger, U+2021 ISOpub 
            _entityName.Add(8225, "Dagger");
            _entityValue.Add("permil", 8240); // per mille sign, U+2030 ISOtech 
            _entityName.Add(8240, "permil");
            _entityValue.Add("lsaquo", 8249); // single left-pointing angle quotation mark, U+2039 ISO proposed 
            _entityName.Add(8249, "lsaquo");
            _entityValue.Add("rsaquo", 8250); // single right-pointing angle quotation mark, U+203A ISO proposed 
            _entityName.Add(8250, "rsaquo");
            _entityValue.Add("euro", 8364); // euro sign, U+20AC NEW 
            _entityName.Add(8364, "euro");

            _maxEntitySize = 8 + 1; // we add the # char

            #endregion
        }

        private HtmlEntity()
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Replace known entities by characters.
        /// </summary>
        /// <param name="text">The source text.</param>
        /// <returns>The result text.</returns>
        public static string DeEntitize(string text)
        {
            if (text == null)
                return null;

            if (text.Length == 0)
                return text;

            StringBuilder sb = new StringBuilder(text.Length);
            ParseState state = ParseState.Text;
            StringBuilder entity = new StringBuilder(10);

            for (int i = 0; i < text.Length; i++)
            {
                switch (state)
                {
                    case ParseState.Text:
                        switch (text[i])
                        {
                            case '&':
                                state = ParseState.EntityStart;
                                break;

                            default:
                                sb.Append(text[i]);
                                break;
                        }
                        break;

                    case ParseState.EntityStart:
                        switch (text[i])
                        {
                            case ';':
                                if (entity.Length == 0)
                                {
                                    sb.Append("&;");
                                }
                                else
                                {
                                    if (entity[0] == '#')
                                    {
                                        string e = entity.ToString();
                                        try
                                        {
                                            int code = Convert.ToInt32(e.Substring(1, e.Length - 1));
                                            sb.Append(Convert.ToChar(code));
                                        }
                                        catch
                                        {
                                            sb.Append("&#" + e + ";");
                                        }
                                    }
                                    else
                                    {
                                        // named entity?
                                        int code;
                                        object o = _entityValue[entity.ToString()];
                                        if (o == null)
                                        {
                                            // nope
                                            sb.Append("&" + entity + ";");
                                        }
                                        else
                                        {
                                            // we found one
                                            code = (int) o;
                                            sb.Append(Convert.ToChar(code));
                                        }
                                    }
                                    entity.Remove(0, entity.Length);
                                }
                                state = ParseState.Text;
                                break;

                            case '&':
                                // new entity start without end, it was not an entity...
                                sb.Append("&" + entity);
                                entity.Remove(0, entity.Length);
                                break;

                            default:
                                entity.Append(text[i]);
                                if (entity.Length > _maxEntitySize)
                                {
                                    // unknown stuff, just don't touch it
                                    state = ParseState.Text;
                                    sb.Append("&" + entity);
                                    entity.Remove(0, entity.Length);
                                }
                                break;
                        }
                        break;
                }
            }

            // finish the work
            if (state == ParseState.EntityStart)
            {
                sb.Append("&" + entity);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Clone and entitize an HtmlNode. This will affect attribute values and nodes' text. It will also entitize all child nodes.
        /// </summary>
        /// <param name="node">The node to entitize.</param>
        /// <returns>An entitized cloned node.</returns>
        public static HtmlNode Entitize(HtmlNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            HtmlNode result = node.CloneNode(true);
            if (result.HasAttributes)
                Entitize(result.Attributes);

            if (result.HasChildNodes)
            {
                Entitize(result.ChildNodes);
            }
            else
            {
                if (result.NodeType == HtmlNodeType.Text)
                {
                    ((HtmlTextNode) result).Text = Entitize(((HtmlTextNode) result).Text, true, true);
                }
            }
            return result;
        }


        /// <summary>
        /// Replace characters above 127 by entities.
        /// </summary>
        /// <param name="text">The source text.</param>
        /// <returns>The result text.</returns>
        public static string Entitize(string text)
        {
            return Entitize(text, true);
        }

        /// <summary>
        /// Replace characters above 127 by entities.
        /// </summary>
        /// <param name="text">The source text.</param>
        /// <param name="useNames">If set to false, the function will not use known entities name. Default is true.</param>
        /// <returns>The result text.</returns>
        public static string Entitize(string text, bool useNames)
        {
            return Entitize(text, useNames, false);
        }

        /// <summary>
        /// Replace characters above 127 by entities.
        /// </summary>
        /// <param name="text">The source text.</param>
        /// <param name="useNames">If set to false, the function will not use known entities name. Default is true.</param>
        /// <param name="entitizeQuotAmpAndLtGt">If set to true, the [quote], [ampersand], [lower than] and [greather than] characters will be entitized.</param>
        /// <returns>The result text</returns>
        public static string Entitize(string text, bool useNames, bool entitizeQuotAmpAndLtGt)
//		_entityValue.Add("quot", 34);	// quotation mark = APL quote, U+0022 ISOnum 
//		_entityName.Add(34, "quot");
//		_entityValue.Add("amp", 38);	// ampersand, U+0026 ISOnum 
//		_entityName.Add(38, "amp");
//		_entityValue.Add("lt", 60);	// less-than sign, U+003C ISOnum 
//		_entityName.Add(60, "lt");
//		_entityValue.Add("gt", 62);	// greater-than sign, U+003E ISOnum 
//		_entityName.Add(62, "gt");
        {
            if (text == null)
                return null;

            if (text.Length == 0)
                return text;

            StringBuilder sb = new StringBuilder(text.Length);
            for (int i = 0; i < text.Length; i++)
            {
                int code = text[i];
                if ((code > 127) ||
                    (entitizeQuotAmpAndLtGt && ((code == 34) || (code == 38) || (code == 60) || (code == 62))))
                {
                    string entity = _entityName[code] as string;
                    if ((entity == null) || (!useNames))
                    {
                        sb.Append("&#" + code + ";");
                    }
                    else
                    {
                        sb.Append("&" + entity + ";");
                    }
                }
                else
                {
                    sb.Append(text[i]);
                }
            }

            return sb.ToString();
        }

        #endregion

        #region Private Methods

        private static void Entitize(HtmlAttributeCollection collection)
        {
            foreach (HtmlAttribute at in collection)
            {
                at.Value = Entitize(at.Value);
            }
        }

        private static void Entitize(HtmlNodeCollection collection)
        {
            foreach (HtmlNode node in collection)
            {
                if (node.HasAttributes)
                    Entitize(node.Attributes);

                if (node.HasChildNodes)
                {
                    Entitize(node.ChildNodes);
                }
                else
                {
                    if (node.NodeType == HtmlNodeType.Text)
                    {
                        ((HtmlTextNode) node).Text = Entitize(((HtmlTextNode) node).Text, true, true);
                    }
                }
            }
        }

        #endregion

        #region Nested type: ParseState

        private enum ParseState
        {
            Text,
            EntityStart
        }

        #endregion
    }
}