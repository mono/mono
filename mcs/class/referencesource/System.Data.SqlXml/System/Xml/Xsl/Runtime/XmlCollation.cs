//------------------------------------------------------------------------------
// <copyright file="XmlCollation.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------

using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.IO;

namespace System.Xml.Xsl.Runtime {
    using Res = System.Xml.Utils.Res;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class XmlCollation {
        // lgid support for sort
        private const int deDE = 0x0407;
        private const int huHU = 0x040E;
        private const int jaJP = 0x0411;
        private const int kaGE = 0x0437;
        private const int koKR = 0x0412;
        private const int zhTW = 0x0404;
        private const int zhCN = 0x0804;
        private const int zhHK = 0x0C04;
        private const int zhSG = 0x1004;
        private const int zhMO = 0x1404;
        private const int zhTWbopo = 0x030404;
        private const int deDEphon = 0x010407;
        private const int huHUtech = 0x01040e;
        private const int kaGEmode = 0x010437;

        // Invariant: compops == (options & Options.mask)
        private CultureInfo cultInfo;
        private Options options;
        private CompareOptions compops;


        /// <summary>
        /// Extends System.Globalization.CompareOptions with additional flags.
        /// </summary>
        private struct Options {
            public const int FlagUpperFirst      = 0x1000;
            public const int FlagEmptyGreatest   = 0x2000;
            public const int FlagDescendingOrder = 0x4000;

            private const int Mask = FlagUpperFirst | FlagEmptyGreatest | FlagDescendingOrder;

            private int value;

            public Options(int value) {
                this.value = value;
            }

            public bool GetFlag(int flag) {
                return (this.value & flag) != 0;
            }

            public void SetFlag(int flag, bool value) {
                if (value)
                    this.value |= flag;
                else
                    this.value &= ~flag;
            }

            public bool UpperFirst {
                get { return GetFlag(FlagUpperFirst); }
                set { SetFlag(FlagUpperFirst, value); }
            }

            public bool EmptyGreatest {
                get { return GetFlag(FlagEmptyGreatest); }
            }

            public bool DescendingOrder {
                get { return GetFlag(FlagDescendingOrder); }
            }

            public bool IgnoreCase {
                get { return GetFlag((int)CompareOptions.IgnoreCase); }
            }

            public bool Ordinal {
                get { return GetFlag((int)CompareOptions.Ordinal); }
            }

            public CompareOptions CompareOptions {
                get {
                    return (CompareOptions)(value & ~Mask);
                }
                set {
                    Debug.Assert(((int)value & Mask) == 0);
                    this.value = (this.value & Mask) | (int)value;
                }
            }

            public static implicit operator int(Options options) {
                return options.value;
            }
        }


        //-----------------------------------------------
        // Constructors
        //-----------------------------------------------

        /// <summary>
        /// Construct a collation that uses the specified culture and compare options.
        /// </summary>
        private XmlCollation(CultureInfo cultureInfo, Options options) {
            this.cultInfo = cultureInfo;
            this.options = options;
            this.compops = options.CompareOptions;
        }


        //-----------------------------------------------
        // Create
        //-----------------------------------------------

        /// <summary>
        /// Singleton collation that sorts according to Unicode code points.
        /// </summary>
        private static XmlCollation cp = new XmlCollation(CultureInfo.InvariantCulture, new Options((int)CompareOptions.Ordinal));

        internal static XmlCollation CodePointCollation {
            get { return cp; }
        }

        internal static XmlCollation Create(string collationLiteral) {
            return Create(collationLiteral, /*throw:*/true);
        }
        // This function is used in both parser and F&O library, so just strictly map valid literals to XmlCollation.
        // Set compare options one by one:
        //     0, false: no effect; 1, true: yes
        // Disregard unrecognized options.
        internal static XmlCollation Create(string collationLiteral, bool throwOnError) {
            Debug.Assert(collationLiteral != null, "collation literal should not be null");
            
            if (collationLiteral == XmlReservedNs.NsCollCodePoint) {
                return CodePointCollation;
            }

            Uri collationUri;
            CultureInfo cultInfo = null;
            Options options = new Options();

            if (throwOnError) {
                collationUri = new Uri(collationLiteral);
            } else {
                if (!Uri.TryCreate(collationLiteral, UriKind.Absolute, out collationUri)) {
                    return null;
                }
            }
            string authority = collationUri.GetLeftPart(UriPartial.Authority);
            if (authority == XmlReservedNs.NsCollationBase) {
                // Language
                // at least a '/' will be returned for Uri.LocalPath
                string lang = collationUri.LocalPath.Substring(1);
                if (lang.Length == 0) {
                    // Use default culture of current thread (cultinfo = null)
                } else {
                    // Create culture from RFC 1766 string
                    try {
                        cultInfo = new CultureInfo(lang);
                    }
                    catch (ArgumentException) {
                        if (!throwOnError) return null;
                        throw new XslTransformException(Res.Coll_UnsupportedLanguage, lang);
                    }
                }
            } else if (collationUri.IsBaseOf(new Uri(XmlReservedNs.NsCollCodePoint))) {
                // language with codepoint collation is not allowed
                options.CompareOptions = CompareOptions.Ordinal;
            } else {
                // Unrecognized collation
                if (!throwOnError) return null;
                throw new XslTransformException(Res.Coll_Unsupported, collationLiteral);
            }

            // Sort & Compare option
            // at least a '?' will be returned for Uri.Query if not empty
            string query = collationUri.Query;
            string sort = null;

            if (query.Length != 0) {
                foreach (string option in query.Substring(1).Split('&')) {
                    string[] pair = option.Split('=');

                    if (pair.Length != 2) {
                        if (!throwOnError) return null;
                        throw new XslTransformException(Res.Coll_BadOptFormat, option);
                    }

                    string optionName = pair[0].ToUpper(CultureInfo.InvariantCulture);
                    string optionValue = pair[1].ToUpper(CultureInfo.InvariantCulture);

                    if (optionName == "SORT") {
                        sort = optionValue;
                    }
                    else {
                        int flag;

                        switch (optionName) {
                        case "IGNORECASE":        flag = (int)CompareOptions.IgnoreCase;     break;
                        case "IGNORENONSPACE":    flag = (int)CompareOptions.IgnoreNonSpace; break;
                        case "IGNORESYMBOLS":     flag = (int)CompareOptions.IgnoreSymbols;  break;
                        case "IGNOREKANATYPE":    flag = (int)CompareOptions.IgnoreKanaType; break;
                        case "IGNOREWIDTH":       flag = (int)CompareOptions.IgnoreWidth;    break;
                        case "UPPERFIRST":        flag = Options.FlagUpperFirst;        break;
                        case "EMPTYGREATEST":     flag = Options.FlagEmptyGreatest;     break;
                        case "DESCENDINGORDER":   flag = Options.FlagDescendingOrder;   break;
                        default:
                            if (!throwOnError) return null;
                            throw new XslTransformException(Res.Coll_UnsupportedOpt, pair[0]);
                        }

                        switch (optionValue) {
                        case "0": case "FALSE": options.SetFlag(flag, false); break;
                        case "1": case "TRUE" : options.SetFlag(flag, true ); break;
                        default:
                            if (!throwOnError) return null;
                            throw new XslTransformException(Res.Coll_UnsupportedOptVal, pair[0], pair[1]);
                        }
                    }
                }
            }

            // upperfirst option is only meaningful when not ignore case
            if (options.UpperFirst && options.IgnoreCase)
                options.UpperFirst = false;

            // other CompareOptions are only meaningful if Ordinal comparison is not being used
            if (options.Ordinal) {
                options.CompareOptions = CompareOptions.Ordinal;
                options.UpperFirst = false;
            }

            // new cultureinfo based on alternate sorting option
            if (sort != null && cultInfo != null) {
                int lgid = GetLangID(cultInfo.LCID);
                switch (sort) {
                case "bopo":
                    if (lgid == zhTW) {
                        cultInfo = new CultureInfo(zhTWbopo);
                    }
                    break;
                case "strk":
                    if (lgid == zhCN || lgid == zhHK || lgid == zhSG || lgid == zhMO) {
                        cultInfo = new CultureInfo(MakeLCID(cultInfo.LCID, /*Stroke*/ 0x02));
                    }
                    break;
                case "uni":
                    if (lgid == jaJP || lgid == koKR) {
                        cultInfo = new CultureInfo(MakeLCID(cultInfo.LCID, /*Unicode*/ 0x01));
                    }
                    break;
                case "phn":
                    if (lgid == deDE) {
                        cultInfo = new CultureInfo(deDEphon);
                    }
                    break;
                case "tech":
                    if (lgid == huHU) {
                        cultInfo = new CultureInfo(huHUtech);
                    }
                    break;
                case "mod":
                    // ka-GE(Georgian - Georgia) Modern Sort: 0x00010437
                    if (lgid == kaGE) {
                        cultInfo = new CultureInfo(kaGEmode);
                    }
                    break;
                case "pron": case "dict": case "trad":
                    // es-ES(Spanish - Spain) Traditional: 0x0000040A
                    // They are removing 0x040a (Spanish Traditional sort) in NLS+.
                    // So if you create 0x040a, it's just like 0x0c0a (Spanish International sort).
                    // Thus I don't handle it differently.
                    break;
                default:
                    if (!throwOnError) return null;
                    throw new XslTransformException(Res.Coll_UnsupportedSortOpt, sort);
                }
            }
            return new XmlCollation(cultInfo, options);
        }


        //-----------------------------------------------
        // Collection Support
        //-----------------------------------------------

        // Redefine Equals and GetHashCode methods, they are needed for UniqueList<XmlCollation>
        public override bool Equals(object obj) {
            if (this == obj) {
                return true;
            }

            XmlCollation that = obj as XmlCollation;
            return that != null &&
                this.options == that.options &&
                object.Equals(this.cultInfo, that.cultInfo);
        }

        public override int GetHashCode() {
            int hashCode = this.options;
            if (this.cultInfo != null) {
                hashCode ^= this.cultInfo.GetHashCode();
            }
            return hashCode;
        }


        //-----------------------------------------------
        // Serialization Support
        //-----------------------------------------------

        // Denotes the current thread locale
        private const int LOCALE_CURRENT = -1;

        internal void GetObjectData(BinaryWriter writer) {
            // NOTE: For CultureInfo we serialize only LCID. It seems to suffice for our purposes.
            Debug.Assert(this.cultInfo == null || this.cultInfo.Equals(new CultureInfo(this.cultInfo.LCID)),
                "Cannot serialize CultureInfo correctly");
            writer.Write(this.cultInfo != null ? this.cultInfo.LCID : LOCALE_CURRENT);
            writer.Write(this.options);
        }

        internal XmlCollation(BinaryReader reader) {
            int lcid = reader.ReadInt32();
            this.cultInfo = (lcid != LOCALE_CURRENT) ? new CultureInfo(lcid) : null;
            this.options = new Options(reader.ReadInt32());
            this.compops = options.CompareOptions;
        }

        //-----------------------------------------------
        // Compare Properties
        //-----------------------------------------------

        internal bool UpperFirst {
            get { return this.options.UpperFirst; }
        }

        internal bool EmptyGreatest {
            get { return this.options.EmptyGreatest; }
        }

        internal bool DescendingOrder {
            get { return this.options.DescendingOrder; }
        }

        internal CultureInfo Culture {
            get {
                // Use default thread culture if this.cultinfo = null
                if (this.cultInfo == null)
                    return CultureInfo.CurrentCulture;

                return this.cultInfo;
            }
        }


        //-----------------------------------------------
        //
        //-----------------------------------------------

        /// <summary>
        /// Create a sort key that can be compared quickly with other keys.
        /// </summary>
        internal XmlSortKey CreateSortKey(string s) {
            SortKey sortKey;
            byte[] bytesKey;
            int idx;

            // 


            sortKey = Culture.CompareInfo.GetSortKey(s, this.compops);

            // Create an XmlStringSortKey using the SortKey if possible
        #if DEBUG
            // In debug-only code, test other code path more frequently
            if (!UpperFirst && DescendingOrder)
                return new XmlStringSortKey(sortKey, DescendingOrder);
        #else
            if (!UpperFirst)
                return new XmlStringSortKey(sortKey, DescendingOrder);
        #endif

            // Get byte buffer from SortKey and modify it
            bytesKey = sortKey.KeyData;
            if (UpperFirst && bytesKey.Length != 0) {
                // By default lower-case is always sorted first for any locale (verified by empirical testing).
                // In order to place upper-case first, invert the case weights in the generated sort key.
                // Skip to case weight section (3rd weight section)
                idx = 0;
                while (bytesKey[idx] != 1)
                    idx++;

                do {
                    idx++;
                }
                while (bytesKey[idx] != 1);

                // Invert all case weights (including terminating 0x1)
                do {
                    idx++;
                    bytesKey[idx] ^= 0xff;
                }
                while (bytesKey[idx] != 0xfe);
            }

            return new XmlStringSortKey(bytesKey, DescendingOrder);
        }

#if not_used
        /// <summary>
        /// Compare two strings with each other.  Return <0 if str1 sorts before str2, 0 if they're equal, and >0
        /// if str1 sorts after str2.
        /// </summary>
        internal int Compare(string str1, string str2) {
            CultureInfo cultinfo = Culture;
            int result;

            if (this.options.Ordinal) {
                result = string.CompareOrdinal(str1, str2);
                if (result < 0) result = -1;
                else if (result > 0) result = 1;
            }
            else if (UpperFirst) {
                // First compare case-insensitive, then break ties by considering case
                result = cultinfo.CompareInfo.Compare(str1, str2, this.compops | CompareOptions.IgnoreCase);
                if (result == 0)
                    result = -cultinfo.CompareInfo.Compare(str1, str2, this.compops);
            }
            else {
                result = cultinfo.CompareInfo.Compare(str1, str2, this.compops);
            }

            if (DescendingOrder)
                result = -result;

            return result;
        }

        /// <summary>
        /// Return the index of str1 in str2, or -1 if str1 is not a substring of str2.
        /// </summary>
        internal int IndexOf(string str1, string str2) {
            return Culture.CompareInfo.IndexOf(str1, str2, this.compops);
        }

        /// <summary>
        /// Return true if str1 ends with str2.
        /// </summary>
        internal bool IsSuffix(string str1, string str2) {
            if (this.options.Ordinal){
                if (str1.Length < str2.Length) {
                    return false;
                } else {
                    return String.CompareOrdinal(str1, str1.Length - str2.Length, str2, 0, str2.Length) == 0;
                }
            }
            return Culture.CompareInfo.IsSuffix (str1, str2, this.compops);
        }

        /// <summary>
        /// Return true if str1 starts with str2.
        /// </summary>
        internal bool IsPrefix(string str1, string str2) {
            if (this.options.Ordinal) {
                if (str1.Length < str2.Length) {
                    return false;
                } else {
                    return String.CompareOrdinal(str1, 0, str2, 0, str2.Length) == 0;
                }
            }
            return Culture.CompareInfo.IsPrefix (str1, str2, this.compops);
        }
#endif


        //-----------------------------------------------
        // Helper Functions
        //-----------------------------------------------

        private static int MakeLCID(int langid, int sortid) {
            return (langid & 0xffff) | ((sortid & 0xf) << 16);
        }

        private static int GetLangID(int lcid) {
            return (lcid & 0xffff);
        }
    }
}
