#if XMLCHARTYPE_GEN_RESOURCE
#undef XMLCHARTYPE_USE_RESOURCE
#endif

//------------------------------------------------------------------------------
// <copyright file="XmlCharType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

//#define XMLCHARTYPE_USE_RESOURCE    // load the character properties from resources (XmlCharType.bin must be linked to System.Xml.dll)
//#define XMLCHARTYPE_GEN_RESOURCE    // generate the character properties into XmlCharType.bin

#if XMLCHARTYPE_GEN_RESOURCE || XMLCHARTYPE_USE_RESOURCE
using System.IO;
using System.Reflection;
#endif
using System.Threading;
using System.Diagnostics;

namespace System.Xml {

    /// <include file='doc\XmlCharType.uex' path='docs/doc[@for="XmlCharType"]/*' />
    /// <internalonly/>
    /// <devdoc>
    ///  The XmlCharType class is used for quick character type recognition
    ///  which is optimized for the first 127 ascii characters.
    /// </devdoc>
#if SILVERLIGHT    
#if !SILVERLIGHT_XPATH
    [System.Runtime.CompilerServices.FriendAccessAllowed] // Used by System.ServiceModel.dll and System.Runtime.Serialization.dll
#endif
#endif
#if XMLCHARTYPE_USE_RESOURCE
    unsafe internal struct XmlCharType {
#else
    internal struct XmlCharType {
#endif
        // Surrogate constants
        internal const int SurHighStart = 0xd800;    // 1101 10xx
        internal const int SurHighEnd = 0xdbff;
        internal const int SurLowStart = 0xdc00;    // 1101 11xx
        internal const int SurLowEnd = 0xdfff;
        internal const int SurMask = 0xfc00;    // 1111 11xx

#if XML10_FIFTH_EDITION
        // Characters defined in the XML 1.0 Fifth Edition
        // Whitespace chars           -- Section 2.3 [3]
        // Star NCName characters     -- Section 2.3 [4]  (NameStartChar characters without ':')
        // NCName characters          -- Section 2.3 [4a] (NameChar characters without ':')
        // Character data characters  -- Section 2.2 [2]
        // Public ID characters       -- Section 2.3 [13]

        // Characters defined in the XML 1.0 Fourth Edition
        // NCNameCharacters           -- Appending B: Characters Classes in XML 1.0 4th edition and earlier - minus the ':' char per the Namespaces in XML spec
        // Letter characters          -- Appending B: Characters Classes in XML 1.0 4th edition and earlier
        //                               This appendix has been deprecated in XML 1.0 5th edition, but we still need to use
        //                               the Letter and NCName definitions from the 4th edition in some places because of backwards compatibility
        internal const int fWhitespace    = 1;
        internal const int fLetter        = 2;
        internal const int fNCStartNameSC = 4;  // SC = Single Char
        internal const int fNCNameSC      = 8;  // SC = Single Char
        internal const int fCharData      = 16;
        internal const int fNCNameXml4e   = 32; // NCName char according to the XML 1.0 4th edition
        internal const int fText          = 64;
        internal const int fAttrValue     = 128;

        // name surrogate constants
        private const int s_NCNameSurCombinedStart = 0x10000;
        private const int s_NCNameSurCombinedEnd   = 0xEFFFF;
        private const int s_NCNameSurHighStart     = SurHighStart + ((s_NCNameSurCombinedStart - 0x10000) / 1024);
        private const int s_NCNameSurHighEnd       = SurHighStart + ((s_NCNameSurCombinedEnd   - 0x10000) / 1024);
        private const int s_NCNameSurLowStart      = SurLowStart  + ((s_NCNameSurCombinedStart - 0x10000) % 1024);
        private const int s_NCNameSurLowEnd        = SurLowStart  + ((s_NCNameSurCombinedEnd   - 0x10000) % 1024);
#else
        // Characters defined in the XML 1.0 Fourth Edition
        // Whitespace chars -- Section 2.3 [3]
        // Letters -- Appendix B [84]
        // Starting NCName characters -- Section 2.3 [5] (Starting Name characters without ':')
        // NCName characters -- Section 2.3 [4]          (Name characters without ':')
        // Character data characters -- Section 2.2 [2]
        // PubidChar ::=  #x20 | #xD | #xA | [a-zA-Z0-9] | [-'()+,./:=?;!*#@$_%] Section 2.3 of spec
        internal const int fWhitespace = 1;
        internal const int fLetter = 2;
        internal const int fNCStartNameSC = 4;
        internal const int fNCNameSC = 8;
        internal const int fCharData = 16;
        internal const int fNCNameXml4e = 32;
        internal const int fText = 64;
        internal const int fAttrValue = 128;
#endif

        // bitmap for public ID characters - 1 bit per character 0x0 - 0x80; no character > 0x80 is a PUBLIC ID char
        private const string s_PublicIdBitmap = "\u2400\u0000\uffbb\uafff\uffff\u87ff\ufffe\u07ff";

        // size of XmlCharType table
        private const uint CharPropertiesSize = (uint)char.MaxValue + 1;

#if !XMLCHARTYPE_USE_RESOURCE || XMLCHARTYPE_GEN_RESOURCE
        internal const string s_Whitespace =
            "\u0009\u000a\u000d\u000d\u0020\u0020";
        
#if XML10_FIFTH_EDITION
        // StartNameChar without ':' -- see Section 2.3 production [4]
        const string s_NCStartName =
            "\u0041\u005a\u005f\u005f\u0061\u007a\u00c0\u00d6" +
            "\u00d8\u00f6\u00f8\u02ff\u0370\u037d\u037f\u1fff" +
            "\u200c\u200d\u2070\u218f\u2c00\u2fef\u3001\ud7ff" +
            "\uf900\ufdcf\ufdf0\ufffd";
        
        // NameChar without ':' -- see Section 2.3 production [4a] 
        const string s_NCName =
            "\u002d\u002e\u0030\u0039\u0041\u005a\u005f\u005f" +
            "\u0061\u007a\u00b7\u00b7\u00c0\u00d6\u00d8\u00f6" +
            "\u00f8\u037d\u037f\u1fff\u200c\u200d\u203f\u2040" +
            "\u2070\u218f\u2c00\u2fef\u3001\ud7ff\uf900\ufdcf" +
            "\ufdf0\ufffd";
#else
        const string s_NCStartName =
            "\u0041\u005a\u005f\u005f\u0061\u007a" +
            "\u00c0\u00d6\u00d8\u00f6\u00f8\u0131\u0134\u013e" +
            "\u0141\u0148\u014a\u017e\u0180\u01c3\u01cd\u01f0" +
            "\u01f4\u01f5\u01fa\u0217\u0250\u02a8\u02bb\u02c1" +
            "\u0386\u0386\u0388\u038a\u038c\u038c\u038e\u03a1" +
            "\u03a3\u03ce\u03d0\u03d6\u03da\u03da\u03dc\u03dc" +
            "\u03de\u03de\u03e0\u03e0\u03e2\u03f3\u0401\u040c" +
            "\u040e\u044f\u0451\u045c\u045e\u0481\u0490\u04c4" +
            "\u04c7\u04c8\u04cb\u04cc\u04d0\u04eb\u04ee\u04f5" +
            "\u04f8\u04f9\u0531\u0556\u0559\u0559\u0561\u0586" +
            "\u05d0\u05ea\u05f0\u05f2\u0621\u063a\u0641\u064a" +
            "\u0671\u06b7\u06ba\u06be\u06c0\u06ce\u06d0\u06d3" +
            "\u06d5\u06d5\u06e5\u06e6\u0905\u0939\u093d\u093d" +
            "\u0958\u0961\u0985\u098c\u098f\u0990\u0993\u09a8" +
            "\u09aa\u09b0\u09b2\u09b2\u09b6\u09b9\u09dc\u09dd" +
            "\u09df\u09e1\u09f0\u09f1\u0a05\u0a0a\u0a0f\u0a10" +
            "\u0a13\u0a28\u0a2a\u0a30\u0a32\u0a33\u0a35\u0a36" +
            "\u0a38\u0a39\u0a59\u0a5c\u0a5e\u0a5e\u0a72\u0a74" +
            "\u0a85\u0a8b\u0a8d\u0a8d\u0a8f\u0a91\u0a93\u0aa8" +
            "\u0aaa\u0ab0\u0ab2\u0ab3\u0ab5\u0ab9\u0abd\u0abd" +
            "\u0ae0\u0ae0\u0b05\u0b0c\u0b0f\u0b10\u0b13\u0b28" +
            "\u0b2a\u0b30\u0b32\u0b33\u0b36\u0b39\u0b3d\u0b3d" +
            "\u0b5c\u0b5d\u0b5f\u0b61\u0b85\u0b8a\u0b8e\u0b90" +
            "\u0b92\u0b95\u0b99\u0b9a\u0b9c\u0b9c\u0b9e\u0b9f" +
            "\u0ba3\u0ba4\u0ba8\u0baa\u0bae\u0bb5\u0bb7\u0bb9" +
            "\u0c05\u0c0c\u0c0e\u0c10\u0c12\u0c28\u0c2a\u0c33" +
            "\u0c35\u0c39\u0c60\u0c61\u0c85\u0c8c\u0c8e\u0c90" +
            "\u0c92\u0ca8\u0caa\u0cb3\u0cb5\u0cb9\u0cde\u0cde" +
            "\u0ce0\u0ce1\u0d05\u0d0c\u0d0e\u0d10\u0d12\u0d28" +
            "\u0d2a\u0d39\u0d60\u0d61\u0e01\u0e2e\u0e30\u0e30" +
            "\u0e32\u0e33\u0e40\u0e45\u0e81\u0e82\u0e84\u0e84" +
            "\u0e87\u0e88\u0e8a\u0e8a\u0e8d\u0e8d\u0e94\u0e97" +
            "\u0e99\u0e9f\u0ea1\u0ea3\u0ea5\u0ea5\u0ea7\u0ea7" +
            "\u0eaa\u0eab\u0ead\u0eae\u0eb0\u0eb0\u0eb2\u0eb3" +
            "\u0ebd\u0ebd\u0ec0\u0ec4\u0f40\u0f47\u0f49\u0f69" +
            "\u10a0\u10c5\u10d0\u10f6\u1100\u1100\u1102\u1103" +
            "\u1105\u1107\u1109\u1109\u110b\u110c\u110e\u1112" +
            "\u113c\u113c\u113e\u113e\u1140\u1140\u114c\u114c" +
            "\u114e\u114e\u1150\u1150\u1154\u1155\u1159\u1159" +
            "\u115f\u1161\u1163\u1163\u1165\u1165\u1167\u1167" +
            "\u1169\u1169\u116d\u116e\u1172\u1173\u1175\u1175" +
            "\u119e\u119e\u11a8\u11a8\u11ab\u11ab\u11ae\u11af" +
            "\u11b7\u11b8\u11ba\u11ba\u11bc\u11c2\u11eb\u11eb" +
            "\u11f0\u11f0\u11f9\u11f9\u1e00\u1e9b\u1ea0\u1ef9" +
            "\u1f00\u1f15\u1f18\u1f1d\u1f20\u1f45\u1f48\u1f4d" +
            "\u1f50\u1f57\u1f59\u1f59\u1f5b\u1f5b\u1f5d\u1f5d" +
            "\u1f5f\u1f7d\u1f80\u1fb4\u1fb6\u1fbc\u1fbe\u1fbe" +
            "\u1fc2\u1fc4\u1fc6\u1fcc\u1fd0\u1fd3\u1fd6\u1fdb" +
            "\u1fe0\u1fec\u1ff2\u1ff4\u1ff6\u1ffc\u2126\u2126" +
            "\u212a\u212b\u212e\u212e\u2180\u2182\u3007\u3007" +
            "\u3021\u3029\u3041\u3094\u30a1\u30fa\u3105\u312c" +
            "\u4e00\u9fa5\uac00\ud7a3";

        const string s_NCName =
            "\u002d\u002e\u0030\u0039\u0041\u005a\u005f\u005f" +
            "\u0061\u007a\u00b7\u00b7\u00c0\u00d6\u00d8\u00f6" +
            "\u00f8\u0131\u0134\u013e\u0141\u0148\u014a\u017e" +
            "\u0180\u01c3\u01cd\u01f0\u01f4\u01f5\u01fa\u0217" +
            "\u0250\u02a8\u02bb\u02c1\u02d0\u02d1\u0300\u0345" +
            "\u0360\u0361\u0386\u038a\u038c\u038c\u038e\u03a1" +
            "\u03a3\u03ce\u03d0\u03d6\u03da\u03da\u03dc\u03dc" +
            "\u03de\u03de\u03e0\u03e0\u03e2\u03f3\u0401\u040c" +
            "\u040e\u044f\u0451\u045c\u045e\u0481\u0483\u0486" +   
            "\u0490\u04c4\u04c7\u04c8\u04cb\u04cc\u04d0\u04eb" +  
            "\u04ee\u04f5\u04f8\u04f9\u0531\u0556\u0559\u0559" +
            "\u0561\u0586\u0591\u05a1\u05a3\u05b9\u05bb\u05bd" +
            "\u05bf\u05bf\u05c1\u05c2\u05c4\u05c4\u05d0\u05ea" +
            "\u05f0\u05f2\u0621\u063a\u0640\u0652\u0660\u0669" +
            "\u0670\u06b7\u06ba\u06be\u06c0\u06ce\u06d0\u06d3" +
            "\u06d5\u06e8\u06ea\u06ed\u06f0\u06f9\u0901\u0903" +
            "\u0905\u0939\u093c\u094d\u0951\u0954\u0958\u0963" +
            "\u0966\u096f\u0981\u0983\u0985\u098c\u098f\u0990" +
            "\u0993\u09a8\u09aa\u09b0\u09b2\u09b2\u09b6\u09b9" +
            "\u09bc\u09bc\u09be\u09c4\u09c7\u09c8\u09cb\u09cd" +
            "\u09d7\u09d7\u09dc\u09dd\u09df\u09e3\u09e6\u09f1" +
            "\u0a02\u0a02\u0a05\u0a0a\u0a0f\u0a10\u0a13\u0a28" +
            "\u0a2a\u0a30\u0a32\u0a33\u0a35\u0a36\u0a38\u0a39" +
            "\u0a3c\u0a3c\u0a3e\u0a42\u0a47\u0a48\u0a4b\u0a4d" +
            "\u0a59\u0a5c\u0a5e\u0a5e\u0a66\u0a74\u0a81\u0a83" +
            "\u0a85\u0a8b\u0a8d\u0a8d\u0a8f\u0a91\u0a93\u0aa8" +
            "\u0aaa\u0ab0\u0ab2\u0ab3\u0ab5\u0ab9\u0abc\u0ac5" +
            "\u0ac7\u0ac9\u0acb\u0acd\u0ae0\u0ae0\u0ae6\u0aef" +
            "\u0b01\u0b03\u0b05\u0b0c\u0b0f\u0b10\u0b13\u0b28" +
            "\u0b2a\u0b30\u0b32\u0b33\u0b36\u0b39\u0b3c\u0b43" +
            "\u0b47\u0b48\u0b4b\u0b4d\u0b56\u0b57\u0b5c\u0b5d" +
            "\u0b5f\u0b61\u0b66\u0b6f\u0b82\u0b83\u0b85\u0b8a" +
            "\u0b8e\u0b90\u0b92\u0b95\u0b99\u0b9a\u0b9c\u0b9c" +
            "\u0b9e\u0b9f\u0ba3\u0ba4\u0ba8\u0baa\u0bae\u0bb5" +
            "\u0bb7\u0bb9\u0bbe\u0bc2\u0bc6\u0bc8\u0bca\u0bcd" +
            "\u0bd7\u0bd7\u0be7\u0bef\u0c01\u0c03\u0c05\u0c0c" +
            "\u0c0e\u0c10\u0c12\u0c28\u0c2a\u0c33\u0c35\u0c39" +
            "\u0c3e\u0c44\u0c46\u0c48\u0c4a\u0c4d\u0c55\u0c56" +
            "\u0c60\u0c61\u0c66\u0c6f\u0c82\u0c83\u0c85\u0c8c" +
            "\u0c8e\u0c90\u0c92\u0ca8\u0caa\u0cb3\u0cb5\u0cb9" +
            "\u0cbe\u0cc4\u0cc6\u0cc8\u0cca\u0ccd\u0cd5\u0cd6" +
            "\u0cde\u0cde\u0ce0\u0ce1\u0ce6\u0cef\u0d02\u0d03" +
            "\u0d05\u0d0c\u0d0e\u0d10\u0d12\u0d28\u0d2a\u0d39" +
            "\u0d3e\u0d43\u0d46\u0d48\u0d4a\u0d4d\u0d57\u0d57" +
            "\u0d60\u0d61\u0d66\u0d6f\u0e01\u0e2e\u0e30\u0e3a" +
            "\u0e40\u0e4e\u0e50\u0e59\u0e81\u0e82\u0e84\u0e84" +
            "\u0e87\u0e88\u0e8a\u0e8a\u0e8d\u0e8d\u0e94\u0e97" +
            "\u0e99\u0e9f\u0ea1\u0ea3\u0ea5\u0ea5\u0ea7\u0ea7" +
            "\u0eaa\u0eab\u0ead\u0eae\u0eb0\u0eb9\u0ebb\u0ebd" +
            "\u0ec0\u0ec4\u0ec6\u0ec6\u0ec8\u0ecd\u0ed0\u0ed9" +
            "\u0f18\u0f19\u0f20\u0f29\u0f35\u0f35\u0f37\u0f37" +
            "\u0f39\u0f39\u0f3e\u0f47\u0f49\u0f69\u0f71\u0f84" +
            "\u0f86\u0f8b\u0f90\u0f95\u0f97\u0f97\u0f99\u0fad" +
            "\u0fb1\u0fb7\u0fb9\u0fb9\u10a0\u10c5\u10d0\u10f6" +
            "\u1100\u1100\u1102\u1103\u1105\u1107\u1109\u1109" +
            "\u110b\u110c\u110e\u1112\u113c\u113c\u113e\u113e" +
            "\u1140\u1140\u114c\u114c\u114e\u114e\u1150\u1150" +
            "\u1154\u1155\u1159\u1159\u115f\u1161\u1163\u1163" +
            "\u1165\u1165\u1167\u1167\u1169\u1169\u116d\u116e" +
            "\u1172\u1173\u1175\u1175\u119e\u119e\u11a8\u11a8" +
            "\u11ab\u11ab\u11ae\u11af\u11b7\u11b8\u11ba\u11ba" +
            "\u11bc\u11c2\u11eb\u11eb\u11f0\u11f0\u11f9\u11f9" +
            "\u1e00\u1e9b\u1ea0\u1ef9\u1f00\u1f15\u1f18\u1f1d" +
            "\u1f20\u1f45\u1f48\u1f4d\u1f50\u1f57\u1f59\u1f59" +
            "\u1f5b\u1f5b\u1f5d\u1f5d\u1f5f\u1f7d\u1f80\u1fb4" +
            "\u1fb6\u1fbc\u1fbe\u1fbe\u1fc2\u1fc4\u1fc6\u1fcc" +
            "\u1fd0\u1fd3\u1fd6\u1fdb\u1fe0\u1fec\u1ff2\u1ff4" +
            "\u1ff6\u1ffc\u20d0\u20dc\u20e1\u20e1\u2126\u2126" +
            "\u212a\u212b\u212e\u212e\u2180\u2182\u3005\u3005" +
            "\u3007\u3007\u3021\u302f\u3031\u3035\u3041\u3094" +
            "\u3099\u309a\u309d\u309e\u30a1\u30fa\u30fc\u30fe" +
            "\u3105\u312c\u4e00\u9fa5\uac00\ud7a3";
#endif

        const string s_CharData =
            "\u0009\u000a\u000d\u000d\u0020\ud7ff\ue000\ufffd";

        const string s_PublicID =
            "\u000a\u000a\u000d\u000d\u0020\u0021\u0023\u0025" +
            "\u0027\u003b\u003d\u003d\u003f\u005a\u005f\u005f" +
            "\u0061\u007a";

        const string s_Text = // TextChar = CharData - { 0xA | 0xD | '<' | '&' | 0x9 | ']' | 0xDC00 - 0xDFFF }
            "\u0020\u0025\u0027\u003b\u003d\u005c\u005e\ud7ff\ue000\ufffd";

        const string s_AttrValue = // AttrValueChar = CharData - { 0xA | 0xD | 0x9 | '<' | '>' | '&' | '\'' | '"' | 0xDC00 - 0xDFFF }
            "\u0020\u0021\u0023\u0025\u0028\u003b\u003d\u003d\u003f\ud7ff\ue000\ufffd";

        //
        // XML 1.0 Fourth Edition definitions for name characters 
        //
        const string s_LetterXml4e =
            "\u0041\u005a\u0061\u007a\u00c0\u00d6\u00d8\u00f6" +
            "\u00f8\u0131\u0134\u013e\u0141\u0148\u014a\u017e" +
            "\u0180\u01c3\u01cd\u01f0\u01f4\u01f5\u01fa\u0217" +
            "\u0250\u02a8\u02bb\u02c1\u0386\u0386\u0388\u038a" +
            "\u038c\u038c\u038e\u03a1\u03a3\u03ce\u03d0\u03d6" +
            "\u03da\u03da\u03dc\u03dc\u03de\u03de\u03e0\u03e0" +
            "\u03e2\u03f3\u0401\u040c\u040e\u044f\u0451\u045c" +
            "\u045e\u0481\u0490\u04c4\u04c7\u04c8\u04cb\u04cc" +
            "\u04d0\u04eb\u04ee\u04f5\u04f8\u04f9\u0531\u0556" +
            "\u0559\u0559\u0561\u0586\u05d0\u05ea\u05f0\u05f2" +
            "\u0621\u063a\u0641\u064a\u0671\u06b7\u06ba\u06be" +
            "\u06c0\u06ce\u06d0\u06d3\u06d5\u06d5\u06e5\u06e6" +
            "\u0905\u0939\u093d\u093d\u0958\u0961\u0985\u098c" +
            "\u098f\u0990\u0993\u09a8\u09aa\u09b0\u09b2\u09b2" +
            "\u09b6\u09b9\u09dc\u09dd\u09df\u09e1\u09f0\u09f1" +
            "\u0a05\u0a0a\u0a0f\u0a10\u0a13\u0a28\u0a2a\u0a30" +
            "\u0a32\u0a33\u0a35\u0a36\u0a38\u0a39\u0a59\u0a5c" +
            "\u0a5e\u0a5e\u0a72\u0a74\u0a85\u0a8b\u0a8d\u0a8d" +
            "\u0a8f\u0a91\u0a93\u0aa8\u0aaa\u0ab0\u0ab2\u0ab3" +
            "\u0ab5\u0ab9\u0abd\u0abd\u0ae0\u0ae0\u0b05\u0b0c" +
            "\u0b0f\u0b10\u0b13\u0b28\u0b2a\u0b30\u0b32\u0b33" +
            "\u0b36\u0b39\u0b3d\u0b3d\u0b5c\u0b5d\u0b5f\u0b61" +
            "\u0b85\u0b8a\u0b8e\u0b90\u0b92\u0b95\u0b99\u0b9a" +
            "\u0b9c\u0b9c\u0b9e\u0b9f\u0ba3\u0ba4\u0ba8\u0baa" +
            "\u0bae\u0bb5\u0bb7\u0bb9\u0c05\u0c0c\u0c0e\u0c10" +
            "\u0c12\u0c28\u0c2a\u0c33\u0c35\u0c39\u0c60\u0c61" +
            "\u0c85\u0c8c\u0c8e\u0c90\u0c92\u0ca8\u0caa\u0cb3" +
            "\u0cb5\u0cb9\u0cde\u0cde\u0ce0\u0ce1\u0d05\u0d0c" +
            "\u0d0e\u0d10\u0d12\u0d28\u0d2a\u0d39\u0d60\u0d61" +
            "\u0e01\u0e2e\u0e30\u0e30\u0e32\u0e33\u0e40\u0e45" +
            "\u0e81\u0e82\u0e84\u0e84\u0e87\u0e88\u0e8a\u0e8a" +
            "\u0e8d\u0e8d\u0e94\u0e97\u0e99\u0e9f\u0ea1\u0ea3" +
            "\u0ea5\u0ea5\u0ea7\u0ea7\u0eaa\u0eab\u0ead\u0eae" +
            "\u0eb0\u0eb0\u0eb2\u0eb3\u0ebd\u0ebd\u0ec0\u0ec4" +
            "\u0f40\u0f47\u0f49\u0f69\u10a0\u10c5\u10d0\u10f6" +
            "\u1100\u1100\u1102\u1103\u1105\u1107\u1109\u1109" +
            "\u110b\u110c\u110e\u1112\u113c\u113c\u113e\u113e" +
            "\u1140\u1140\u114c\u114c\u114e\u114e\u1150\u1150" +
            "\u1154\u1155\u1159\u1159\u115f\u1161\u1163\u1163" +
            "\u1165\u1165\u1167\u1167\u1169\u1169\u116d\u116e" +
            "\u1172\u1173\u1175\u1175\u119e\u119e\u11a8\u11a8" +
            "\u11ab\u11ab\u11ae\u11af\u11b7\u11b8\u11ba\u11ba" +
            "\u11bc\u11c2\u11eb\u11eb\u11f0\u11f0\u11f9\u11f9" +
            "\u1e00\u1e9b\u1ea0\u1ef9\u1f00\u1f15\u1f18\u1f1d" +
            "\u1f20\u1f45\u1f48\u1f4d\u1f50\u1f57\u1f59\u1f59" +
            "\u1f5b\u1f5b\u1f5d\u1f5d\u1f5f\u1f7d\u1f80\u1fb4" +
            "\u1fb6\u1fbc\u1fbe\u1fbe\u1fc2\u1fc4\u1fc6\u1fcc" +
            "\u1fd0\u1fd3\u1fd6\u1fdb\u1fe0\u1fec\u1ff2\u1ff4" +
            "\u1ff6\u1ffc\u2126\u2126\u212a\u212b\u212e\u212e" +
            "\u2180\u2182\u3007\u3007\u3021\u3029\u3041\u3094" +
            "\u30a1\u30fa\u3105\u312c\u4e00\u9fa5\uac00\ud7a3";

        const string s_NCNameXml4e =
            "\u002d\u002e\u0030\u0039\u0041\u005a\u005f\u005f" +
            "\u0061\u007a\u00b7\u00b7\u00c0\u00d6\u00d8\u00f6" +
            "\u00f8\u0131\u0134\u013e\u0141\u0148\u014a\u017e" +
            "\u0180\u01c3\u01cd\u01f0\u01f4\u01f5\u01fa\u0217" +
            "\u0250\u02a8\u02bb\u02c1\u02d0\u02d1\u0300\u0345" +
            "\u0360\u0361\u0386\u038a\u038c\u038c\u038e\u03a1" +
            "\u03a3\u03ce\u03d0\u03d6\u03da\u03da\u03dc\u03dc" +
            "\u03de\u03de\u03e0\u03e0\u03e2\u03f3\u0401\u040c" +
            "\u040e\u044f\u0451\u045c\u045e\u0481\u0483\u0486" +   
            "\u0490\u04c4\u04c7\u04c8\u04cb\u04cc\u04d0\u04eb" +  
            "\u04ee\u04f5\u04f8\u04f9\u0531\u0556\u0559\u0559" +
            "\u0561\u0586\u0591\u05a1\u05a3\u05b9\u05bb\u05bd" +
            "\u05bf\u05bf\u05c1\u05c2\u05c4\u05c4\u05d0\u05ea" +
            "\u05f0\u05f2\u0621\u063a\u0640\u0652\u0660\u0669" +
            "\u0670\u06b7\u06ba\u06be\u06c0\u06ce\u06d0\u06d3" +
            "\u06d5\u06e8\u06ea\u06ed\u06f0\u06f9\u0901\u0903" +
            "\u0905\u0939\u093c\u094d\u0951\u0954\u0958\u0963" +
            "\u0966\u096f\u0981\u0983\u0985\u098c\u098f\u0990" +
            "\u0993\u09a8\u09aa\u09b0\u09b2\u09b2\u09b6\u09b9" +
            "\u09bc\u09bc\u09be\u09c4\u09c7\u09c8\u09cb\u09cd" +
            "\u09d7\u09d7\u09dc\u09dd\u09df\u09e3\u09e6\u09f1" +
            "\u0a02\u0a02\u0a05\u0a0a\u0a0f\u0a10\u0a13\u0a28" +
            "\u0a2a\u0a30\u0a32\u0a33\u0a35\u0a36\u0a38\u0a39" +
            "\u0a3c\u0a3c\u0a3e\u0a42\u0a47\u0a48\u0a4b\u0a4d" +
            "\u0a59\u0a5c\u0a5e\u0a5e\u0a66\u0a74\u0a81\u0a83" +
            "\u0a85\u0a8b\u0a8d\u0a8d\u0a8f\u0a91\u0a93\u0aa8" +
            "\u0aaa\u0ab0\u0ab2\u0ab3\u0ab5\u0ab9\u0abc\u0ac5" +
            "\u0ac7\u0ac9\u0acb\u0acd\u0ae0\u0ae0\u0ae6\u0aef" +
            "\u0b01\u0b03\u0b05\u0b0c\u0b0f\u0b10\u0b13\u0b28" +
            "\u0b2a\u0b30\u0b32\u0b33\u0b36\u0b39\u0b3c\u0b43" +
            "\u0b47\u0b48\u0b4b\u0b4d\u0b56\u0b57\u0b5c\u0b5d" +
            "\u0b5f\u0b61\u0b66\u0b6f\u0b82\u0b83\u0b85\u0b8a" +
            "\u0b8e\u0b90\u0b92\u0b95\u0b99\u0b9a\u0b9c\u0b9c" +
            "\u0b9e\u0b9f\u0ba3\u0ba4\u0ba8\u0baa\u0bae\u0bb5" +
            "\u0bb7\u0bb9\u0bbe\u0bc2\u0bc6\u0bc8\u0bca\u0bcd" +
            "\u0bd7\u0bd7\u0be7\u0bef\u0c01\u0c03\u0c05\u0c0c" +
            "\u0c0e\u0c10\u0c12\u0c28\u0c2a\u0c33\u0c35\u0c39" +
            "\u0c3e\u0c44\u0c46\u0c48\u0c4a\u0c4d\u0c55\u0c56" +
            "\u0c60\u0c61\u0c66\u0c6f\u0c82\u0c83\u0c85\u0c8c" +
            "\u0c8e\u0c90\u0c92\u0ca8\u0caa\u0cb3\u0cb5\u0cb9" +
            "\u0cbe\u0cc4\u0cc6\u0cc8\u0cca\u0ccd\u0cd5\u0cd6" +
            "\u0cde\u0cde\u0ce0\u0ce1\u0ce6\u0cef\u0d02\u0d03" +
            "\u0d05\u0d0c\u0d0e\u0d10\u0d12\u0d28\u0d2a\u0d39" +
            "\u0d3e\u0d43\u0d46\u0d48\u0d4a\u0d4d\u0d57\u0d57" +
            "\u0d60\u0d61\u0d66\u0d6f\u0e01\u0e2e\u0e30\u0e3a" +
            "\u0e40\u0e4e\u0e50\u0e59\u0e81\u0e82\u0e84\u0e84" +
            "\u0e87\u0e88\u0e8a\u0e8a\u0e8d\u0e8d\u0e94\u0e97" +
            "\u0e99\u0e9f\u0ea1\u0ea3\u0ea5\u0ea5\u0ea7\u0ea7" +
            "\u0eaa\u0eab\u0ead\u0eae\u0eb0\u0eb9\u0ebb\u0ebd" +
            "\u0ec0\u0ec4\u0ec6\u0ec6\u0ec8\u0ecd\u0ed0\u0ed9" +
            "\u0f18\u0f19\u0f20\u0f29\u0f35\u0f35\u0f37\u0f37" +
            "\u0f39\u0f39\u0f3e\u0f47\u0f49\u0f69\u0f71\u0f84" +
            "\u0f86\u0f8b\u0f90\u0f95\u0f97\u0f97\u0f99\u0fad" +
            "\u0fb1\u0fb7\u0fb9\u0fb9\u10a0\u10c5\u10d0\u10f6" +
            "\u1100\u1100\u1102\u1103\u1105\u1107\u1109\u1109" +
            "\u110b\u110c\u110e\u1112\u113c\u113c\u113e\u113e" +
            "\u1140\u1140\u114c\u114c\u114e\u114e\u1150\u1150" +
            "\u1154\u1155\u1159\u1159\u115f\u1161\u1163\u1163" +
            "\u1165\u1165\u1167\u1167\u1169\u1169\u116d\u116e" +
            "\u1172\u1173\u1175\u1175\u119e\u119e\u11a8\u11a8" +
            "\u11ab\u11ab\u11ae\u11af\u11b7\u11b8\u11ba\u11ba" +
            "\u11bc\u11c2\u11eb\u11eb\u11f0\u11f0\u11f9\u11f9" +
            "\u1e00\u1e9b\u1ea0\u1ef9\u1f00\u1f15\u1f18\u1f1d" +
            "\u1f20\u1f45\u1f48\u1f4d\u1f50\u1f57\u1f59\u1f59" +
            "\u1f5b\u1f5b\u1f5d\u1f5d\u1f5f\u1f7d\u1f80\u1fb4" +
            "\u1fb6\u1fbc\u1fbe\u1fbe\u1fc2\u1fc4\u1fc6\u1fcc" +
            "\u1fd0\u1fd3\u1fd6\u1fdb\u1fe0\u1fec\u1ff2\u1ff4" +
            "\u1ff6\u1ffc\u20d0\u20dc\u20e1\u20e1\u2126\u2126" +
            "\u212a\u212b\u212e\u212e\u2180\u2182\u3005\u3005" +
            "\u3007\u3007\u3021\u302f\u3031\u3035\u3041\u3094" +
            "\u3099\u309a\u309d\u309e\u30a1\u30fa\u30fc\u30fe" +
            "\u3105\u312c\u4e00\u9fa5\uac00\ud7a3";
#endif

        // static lock for XmlCharType class
        private static object s_Lock;

        private static object StaticLock {
            get {
                if ( s_Lock == null ) {
                    object o = new object();
                    Interlocked.CompareExchange<object>( ref s_Lock, o, null );
                }
                return s_Lock;
            }
        }

#if XMLCHARTYPE_USE_RESOURCE

#if SILVERLIGHT && !SILVERLIGHT_DISABLE_SECURITY
        [System.Security.SecurityCritical]
#endif
        private static volatile byte*   s_CharProperties;

#if SILVERLIGHT && !SILVERLIGHT_DISABLE_SECURITY
        [System.Security.SecurityCritical]
#endif
        internal byte*                  charProperties;

#if SILVERLIGHT && !SILVERLIGHT_DISABLE_SECURITY
        [System.Security.SecurityCritical]
#endif
        static void InitInstance() {
            lock ( StaticLock ) {
                if ( s_CharProperties != null ) {
                    return;
                }

                UnmanagedMemoryStream memStream = (UnmanagedMemoryStream)Assembly.GetExecutingAssembly().GetManifestResourceStream( "XmlCharType.bin" );
                Debug.Assert( memStream.Length == CharPropertiesSize );

                byte* chProps    = memStream.PositionPointer;
                Thread.MemoryBarrier();  // For weak memory models (IA64)
                s_CharProperties = chProps;
            }
        }

#else // !XMLCHARTYPE_USE_RESOURCE
        private static volatile byte [] s_CharProperties;
        internal byte []                charProperties;

        static void InitInstance() {
            lock ( StaticLock ) {
                if ( s_CharProperties != null ) {
                    return;
                }

                byte[] chProps = new byte[CharPropertiesSize];

                SetProperties( chProps, s_Whitespace,  fWhitespace );
                SetProperties( chProps, s_LetterXml4e, fLetter );
                SetProperties( chProps, s_NCStartName, fNCStartNameSC );
                SetProperties( chProps, s_NCName,      fNCNameSC );
                SetProperties( chProps, s_CharData,    fCharData );
                SetProperties( chProps, s_NCNameXml4e, fNCNameXml4e );
                SetProperties( chProps, s_Text,        fText );
                SetProperties( chProps, s_AttrValue,   fAttrValue );

                Thread.MemoryBarrier();  // For weak memory models (IA64)
                s_CharProperties = chProps;
            }
        }

        private static void SetProperties(byte[] chProps, string ranges, byte value ) {
            for ( int p = 0; p < ranges.Length; p += 2 ) {
                for ( int i = ranges[p], last = ranges[p + 1]; i <= last; i++ ) {
                    chProps[i] |= value;
                }
            }
        }
#endif

#if XMLCHARTYPE_USE_RESOURCE
#if SILVERLIGHT && !SILVERLIGHT_DISABLE_SECURITY
        [System.Security.SecurityCritical]
#endif
        private XmlCharType( byte* charProperties ) {
#else
        private XmlCharType( byte[] charProperties ) {
#endif
            Debug.Assert( s_CharProperties != null );
            this.charProperties = charProperties;
        }

        public static XmlCharType Instance {
#if SILVERLIGHT && !SILVERLIGHT_DISABLE_SECURITY && XMLCHARTYPE_USE_RESOURCE
            [System.Security.SecuritySafeCritical]
#endif
            get {
                if ( s_CharProperties == null ) {
                    InitInstance();
                }
                return new XmlCharType( s_CharProperties );
            }
        }

        // NOTE: This method will not be inlined (because it uses byte* charProperties)
#if SILVERLIGHT && !SILVERLIGHT_DISABLE_SECURITY && XMLCHARTYPE_USE_RESOURCE
        [System.Security.SecuritySafeCritical]
#endif
        public bool IsWhiteSpace( char ch ) {
            return ( charProperties[ch] & fWhitespace ) != 0;
        }

#if !SILVERLIGHT
        public bool IsExtender( char ch ) {
            return ( ch == 0xb7 );
        }
#endif

        // NOTE: This method will not be inlined (because it uses byte* charProperties)
#if SILVERLIGHT && !SILVERLIGHT_DISABLE_SECURITY && XMLCHARTYPE_USE_RESOURCE
        [System.Security.SecuritySafeCritical]
#endif
        public bool IsNCNameSingleChar(char ch) {
            return ( charProperties[ch] & fNCNameSC ) != 0;
        }

#if XML10_FIFTH_EDITION
        public bool IsNCNameSurrogateChar( string str, int index ) {
            if ( index + 1 >= str.Length ) {
                return false;
            }
            return InRange( str[index], s_NCNameSurHighStart, s_NCNameSurHighEnd ) &&
                   InRange( str[index + 1], s_NCNameSurLowStart, s_NCNameSurLowEnd );
        }

        // Surrogate characters for names are the same for NameChar and StartNameChar, 
        // so this method can be used for both
        public bool IsNCNameSurrogateChar(char lowChar, char highChar) {
            return InRange( highChar, s_NCNameSurHighStart, s_NCNameSurHighEnd ) &&
                   InRange( lowChar, s_NCNameSurLowStart, s_NCNameSurLowEnd );
        }

        public bool IsNCNameHighSurrogateChar( char highChar ) {
            return InRange( highChar, s_NCNameSurHighStart, s_NCNameSurHighEnd );
        }

        public bool IsNCNameLowSurrogateChar( char lowChar ) {
            return InRange( lowChar, s_NCNameSurLowStart, s_NCNameSurLowEnd );
        }
#endif

        // NOTE: This method will not be inlined (because it uses byte* charProperties)
#if SILVERLIGHT && !SILVERLIGHT_DISABLE_SECURITY && XMLCHARTYPE_USE_RESOURCE
        [System.Security.SecuritySafeCritical]
#endif
        public bool IsStartNCNameSingleChar(char ch) {
            return ( charProperties[ch] & fNCStartNameSC ) != 0;
        }

#if XML10_FIFTH_EDITION
        // !!! NOTE: These is no IsStartNCNameSurrogateChar, use IsNCNameSurrogateChar instead.
        // Surrogate ranges for start name charaters are the same as for name characters.
#endif

        public bool IsNameSingleChar(char ch) {
            return IsNCNameSingleChar(ch) || ch == ':';
        }

#if XML10_FIFTH_EDITION
        public bool IsNameSurrogateChar(char lowChar, char highChar) {
            return IsNCNameSurrogateChar(lowChar, highChar);
        }
#endif

        public bool IsStartNameSingleChar(char ch) {
            return IsStartNCNameSingleChar(ch) || ch == ':';
        }

        // NOTE: This method will not be inlined (because it uses byte* charProperties)
#if SILVERLIGHT && !SILVERLIGHT_DISABLE_SECURITY && XMLCHARTYPE_USE_RESOURCE
        [System.Security.SecuritySafeCritical]
#endif
        public bool IsCharData( char ch ) {
            return ( charProperties[ch] & fCharData ) != 0;
        }

        // [13] PubidChar ::=  #x20 | #xD | #xA | [a-zA-Z0-9] | [-'()+,./:=?;!*#@$_%] Section 2.3 of spec
#if SILVERLIGHT && !SILVERLIGHT_DISABLE_SECURITY && XMLCHARTYPE_USE_RESOURCE
        [System.Security.SecuritySafeCritical]
#endif
        public bool IsPubidChar( char ch ) {
            if ( ch < (char)0x80 ) {
                return ( s_PublicIdBitmap[ch >> 4] & ( 1 << ( ch & 0xF ) ) ) != 0;
            }
            return false;
        }

        // TextChar = CharData - { 0xA, 0xD, '<', '&', ']' }
        // NOTE: This method will not be inlined (because it uses byte* charProperties)
#if SILVERLIGHT && !SILVERLIGHT_DISABLE_SECURITY && XMLCHARTYPE_USE_RESOURCE
        [System.Security.SecuritySafeCritical]
#endif
        internal bool IsTextChar( char ch ) {
            return ( charProperties[ch] & fText ) != 0;
        }

        // AttrValueChar = CharData - { 0xA, 0xD, 0x9, '<', '>', '&', '\'', '"' }
        // NOTE: This method will not be inlined (because it uses byte* charProperties)
#if SILVERLIGHT && !SILVERLIGHT_DISABLE_SECURITY && XMLCHARTYPE_USE_RESOURCE
        [System.Security.SecuritySafeCritical]
#endif
        internal bool IsAttributeValueChar( char ch ) {
            return ( charProperties[ch] & fAttrValue ) != 0;
        }

        // XML 1.0 Fourth Edition definitions
        //
        // NOTE: This method will not be inlined (because it uses byte* charProperties)
#if SILVERLIGHT && !SILVERLIGHT_DISABLE_SECURITY && XMLCHARTYPE_USE_RESOURCE
        [System.Security.SecuritySafeCritical]
#endif
        public bool IsLetter( char ch ) {
            return ( charProperties[ch] & fLetter ) != 0;
        }

        // NOTE: This method will not be inlined (because it uses byte* charProperties)
        // This method uses the XML 4th edition name character ranges
#if SILVERLIGHT && !SILVERLIGHT_DISABLE_SECURITY && XMLCHARTYPE_USE_RESOURCE
        [System.Security.SecuritySafeCritical]
#endif
        public bool IsNCNameCharXml4e( char ch ) {
            return ( charProperties[ch] & fNCNameXml4e  ) != 0;
        }

        // This method uses the XML 4th edition name character ranges
        public bool IsStartNCNameCharXml4e( char ch ) {
            return IsLetter( ch ) || ch == '_';
        }

        // This method uses the XML 4th edition name character ranges
        public bool IsNameCharXml4e( char ch ) {
            return IsNCNameCharXml4e( ch ) || ch == ':';
        }

        // This method uses the XML 4th edition name character ranges
        public bool IsStartNameCharXml4e( char ch ) {
            return IsStartNCNameCharXml4e( ch ) || ch == ':';
        }

        // Digit methods
        public static bool IsDigit( char ch ) {
            return InRange( ch, 0x30, 0x39 );
        }

#if !SILVERLIGHT
        public static bool IsHexDigit(char ch) {
            return InRange( ch, 0x30, 0x39 ) || InRange( ch, 'a', 'f' ) || InRange( ch, 'A', 'F' );
        }
#endif

        // Surrogate methods
        internal static bool IsHighSurrogate( int ch ) {
            return InRange( ch, SurHighStart, SurHighEnd );
        }

        internal static bool IsLowSurrogate( int ch ) {
            return InRange( ch, SurLowStart, SurLowEnd );
        }

        internal static bool IsSurrogate( int ch ) {
            return InRange( ch, SurHighStart, SurLowEnd );
        }

        internal static int CombineSurrogateChar( int lowChar, int highChar ) {
            return ( lowChar - SurLowStart ) | ( ( highChar - SurHighStart ) << 10 ) + 0x10000;
        }

        internal static void SplitSurrogateChar( int combinedChar, out char lowChar, out char highChar ) {
            int v = combinedChar - 0x10000;
            lowChar = (char)( SurLowStart + v % 1024 );
            highChar = (char)( SurHighStart + v / 1024 );
        }

        internal bool IsOnlyWhitespace( string str ) {
            return IsOnlyWhitespaceWithPos( str ) == -1;
        }

        // Character checking on strings
#if SILVERLIGHT && !SILVERLIGHT_DISABLE_SECURITY && XMLCHARTYPE_USE_RESOURCE
        [System.Security.SecuritySafeCritical]
#endif
        internal int IsOnlyWhitespaceWithPos( string str ) {
            if ( str != null ) {
                for ( int i = 0; i < str.Length; i++ ) {
                    if ( ( charProperties[str[i]] & fWhitespace ) == 0 ) {
                        return i;
                    }
                }
            }
            return -1;
        }

#if SILVERLIGHT && !SILVERLIGHT_DISABLE_SECURITY && XMLCHARTYPE_USE_RESOURCE
        [System.Security.SecuritySafeCritical]
#endif
        internal int IsOnlyCharData( string str ) {
            if ( str != null ) {
                for ( int i = 0; i < str.Length; i++ ) {
                    if ( ( charProperties[str[i]] & fCharData ) == 0 ) {
                        if ( i + 1 >= str.Length || !(XmlCharType.IsHighSurrogate(str[i]) && XmlCharType.IsLowSurrogate(str[i+1]))) {
                            return i;
                        }
                        else {
                            i++;
                        }
                    }
                }
            }
            return -1;
        }

        static internal bool IsOnlyDigits(string str, int startPos, int len) {
            Debug.Assert(str != null);
            Debug.Assert(startPos + len <= str.Length);
            Debug.Assert(startPos <= str.Length);

            for (int i = startPos; i < startPos + len; i++) {
                if (!IsDigit(str[i])) {
                    return false;
                }
            }
            return true;
        }

        static internal bool IsOnlyDigits( char[] chars, int startPos, int len ) {
            Debug.Assert( chars != null );
            Debug.Assert( startPos + len <= chars.Length );
            Debug.Assert( startPos <= chars.Length );
                
            for ( int i = startPos; i < startPos + len; i++ ) {
                if ( !IsDigit( chars[i] ) ) {
                    return false;
                }
            }
            return true;
        }

        internal int IsPublicId( string str ) {
            if ( str != null ) {
                for ( int i = 0; i < str.Length; i++ ) {
                    if ( !IsPubidChar(str[i]) ) {
                        return i;
                    }
                }
            }
            return -1;
        }

        // This method tests whether a value is in a given range with just one test; start and end should be constants
        private static bool InRange(int value, int start, int end) {
            Debug.Assert(start <= end);
            return (uint)(value - start) <= (uint)(end - start);            
        }

#if XMLCHARTYPE_GEN_RESOURCE
        //
        // Code for generating XmlCharType.bin table and s_PublicIdBitmap
        //
        // build command line:  csc XmlCharType.cs /d:XMLCHARTYPE_GEN_RESOURCE
        //
        public static void Main( string[] args ) {
            try {
                InitInstance();

                // generate PublicId bitmap
                ushort[] bitmap = new ushort[0x80 >> 4];
                for (int i = 0; i < s_PublicID.Length; i += 2) {
                    for (int j = s_PublicID[i], last = s_PublicID[i + 1]; j <= last; j++) {
                        bitmap[j >> 4] |= (ushort)(1 << (j & 0xF));
                    }
                }

                Console.Write("private const string s_PublicIdBitmap = \"");
                for (int i = 0; i < bitmap.Length; i++) {
                    Console.Write("\\u{0:x4}", bitmap[i]);
                }
                Console.WriteLine("\";");
                Console.WriteLine();

                string fileName = ( args.Length == 0 ) ? "XmlCharType.bin" : args[0];
                Console.Write( "Writing XmlCharType character properties to {0}...", fileName );

                FileStream fs = new FileStream( fileName, FileMode.Create );
                for ( int i = 0; i < CharPropertiesSize; i += 4096 ) {
                    fs.Write( s_CharProperties, i, 4096 );
                }
                fs.Close();
                Console.WriteLine( "done." );
            }
            catch ( Exception e ) {
                Console.WriteLine();
                Console.WriteLine( "Exception: {0}", e.Message );
            }
        }
#endif
    }
}
