//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System.Collections;
    using System.Runtime;

    internal static class XPathCharTypes
    {
        static byte[] charProperties;

        // Types of characters identified by this class
        private const byte None = 0x00;
        private const byte Letter = 0x01;
        private const byte Combining = 0x02;
        private const byte Digit = 0x04;
        private const byte Extender = 0x08;
        private const byte Whitespace = 0x10;
        private const byte NCName = 0x20;
        private const byte NCNameStart = 0x40;

        #region Data Tables
        // The character tables below hold unicode characters that are paired to represent character ranges.
        // The first is the first character in the range, the second is the last character in the range.

        // The definition of NCName is taken from "Namespaces in XML" (http://www.w3.org/TR/REC-xml-names/) Section 2
        // The definition of Whitespace is taken from: "XML 1.0 (Second Edition)" (http://www.w3.org/TR/REC-xml) Section 2.3
        // All classes of characters contain references to their definitions

        // BaseChars
        // Taken from: "XML 1.0 (Second Edition)" (http://www.w3.org/TR/REC-xml) Appendix B
        // [#x0041-#x005A] | [#x0061-#x007A] | [#x00C0-#x00D6] | [#x00D8-#x00F6] | [#x00F8-#x00FF] | [#x0100-#x0131] | [#x0134-#x013E] | [#x0141-#x0148] | [#x014A-#x017E] | [#x0180-#x01C3] | [#x01CD-#x01F0] | [#x01F4-#x01F5] | [#x01FA-#x0217] | [#x0250-#x02A8] | [#x02BB-#x02C1] | #x0386 | [#x0388-#x038A] | #x038C | [#x038E-#x03A1] | [#x03A3-#x03CE] | [#x03D0-#x03D6] | #x03DA | #x03DC | #x03DE | #x03E0 | [#x03E2-#x03F3] | [#x0401-#x040C] | [#x040E-#x044F] | [#x0451-#x045C] | [#x045E-#x0481] | [#x0490-#x04C4] | [#x04C7-#x04C8] | [#x04CB-#x04CC] | [#x04D0-#x04EB] | [#x04EE-#x04F5] | [#x04F8-#x04F9] | [#x0531-#x0556] | #x0559 | [#x0561-#x0586] | [#x05D0-#x05EA] | [#x05F0-#x05F2] | [#x0621-#x063A] | [#x0641-#x064A] | [#x0671-#x06B7] | [#x06BA-#x06BE] | [#x06C0-#x06CE] | [#x06D0-#x06D3] | #x06D5 | [#x06E5-#x06E6] | [#x0905-#x0939] | #x093D | [#x0958-#x0961] | [#x0985-#x098C] | [#x098F-#x0990] | [#x0993-#x09A8] | [#x09AA-#x09B0] | #x09B2 | [#x09B6-#x09B9] | [#x09DC-#x09DD] | [#x09DF-#x09E1] | [#x09F0-#x09F1] | [#x0A05-#x0A0A] | [#x0A0F-#x0A10] | [#x0A13-#x0A28] | [#x0A2A-#x0A30] | [#x0A32-#x0A33] | [#x0A35-#x0A36] | [#x0A38-#x0A39] | [#x0A59-#x0A5C] | #x0A5E | [#x0A72-#x0A74] | [#x0A85-#x0A8B] | #x0A8D | [#x0A8F-#x0A91] | [#x0A93-#x0AA8] | [#x0AAA-#x0AB0] | [#x0AB2-#x0AB3] | [#x0AB5-#x0AB9] | #x0ABD | #x0AE0 | [#x0B05-#x0B0C] | [#x0B0F-#x0B10] | [#x0B13-#x0B28] | [#x0B2A-#x0B30] | [#x0B32-#x0B33] | [#x0B36-#x0B39] | #x0B3D | [#x0B5C-#x0B5D] | [#x0B5F-#x0B61] | [#x0B85-#x0B8A] | [#x0B8E-#x0B90] | [#x0B92-#x0B95] | [#x0B99-#x0B9A] | #x0B9C | [#x0B9E-#x0B9F] | [#x0BA3-#x0BA4] | [#x0BA8-#x0BAA] | [#x0BAE-#x0BB5] | [#x0BB7-#x0BB9] | [#x0C05-#x0C0C] | [#x0C0E-#x0C10] | [#x0C12-#x0C28] | [#x0C2A-#x0C33] | [#x0C35-#x0C39] | [#x0C60-#x0C61] | [#x0C85-#x0C8C] | [#x0C8E-#x0C90] | [#x0C92-#x0CA8] | [#x0CAA-#x0CB3] | [#x0CB5-#x0CB9] | #x0CDE | [#x0CE0-#x0CE1] | [#x0D05-#x0D0C] | [#x0D0E-#x0D10] | [#x0D12-#x0D28] | [#x0D2A-#x0D39] | [#x0D60-#x0D61] | [#x0E01-#x0E2E] | #x0E30 | [#x0E32-#x0E33] | [#x0E40-#x0E45] | 
        // [#x0E81-#x0E82] | #x0E84 | [#x0E87-#x0E88] | #x0E8A | #x0E8D | [#x0E94-#x0E97] | [#x0E99-#x0E9F] | [#x0EA1-#x0EA3] | #x0EA5 | #x0EA7 | [#x0EAA-#x0EAB] | [#x0EAD-#x0EAE] | #x0EB0 | [#x0EB2-#x0EB3] | #x0EBD | [#x0EC0-#x0EC4] | [#x0F40-#x0F47] | [#x0F49-#x0F69] | [#x10A0-#x10C5] | [#x10D0-#x10F6] | #x1100 | [#x1102-#x1103] | [#x1105-#x1107] | #x1109 | [#x110B-#x110C] | [#x110E-#x1112] | #x113C | #x113E | #x1140 | #x114C | #x114E | #x1150 | [#x1154-#x1155] | #x1159 | [#x115F-#x1161] | #x1163 | #x1165 | #x1167 | #x1169 | [#x116D-#x116E] | [#x1172-#x1173] | #x1175 | #x119E | #x11A8 | #x11AB | [#x11AE-#x11AF] | [#x11B7-#x11B8] | #x11BA | [#x11BC-#x11C2] | #x11EB | #x11F0 | #x11F9 | [#x1E00-#x1E9B] | [#x1EA0-#x1EF9] | [#x1F00-#x1F15] | [#x1F18-#x1F1D] | [#x1F20-#x1F45] | [#x1F48-#x1F4D] | [#x1F50-#x1F57] | #x1F59 | #x1F5B | #x1F5D | [#x1F5F-#x1F7D] | [#x1F80-#x1FB4] | [#x1FB6-#x1FBC] | #x1FBE | [#x1FC2-#x1FC4] | [#x1FC6-#x1FCC] | [#x1FD0-#x1FD3] | [#x1FD6-#x1FDB] | [#x1FE0-#x1FEC] | [#x1FF2-#x1FF4] | [#x1FF6-#x1FFC] | #x2126 | [#x212A-#x212B] | #x212E | [#x2180-#x2182] | [#x3041-#x3094] | [#x30A1-#x30FA] | [#x3105-#x312C] | [#xAC00-#xD7A3]
        const string BaseChars =
            "\u0041\u005A\u0061\u007A\u00C0\u00D6\u00D8\u00F6" +
            "\u00F8\u00FF\u0100\u0131\u0134\u013E\u0141\u0148" +
            "\u014A\u017E\u0180\u01C3\u01CD\u01F0\u01F4\u01F5" +
            "\u01FA\u0217\u0250\u02A8\u02BB\u02C1\u0386\u0386" +
            "\u0388\u038A\u038C\u038C\u038E\u03A1\u03A3\u03CE" +
            "\u03D0\u03D6\u03DA\u03DA\u03DC\u03DC\u03DE\u03DE" +
            "\u03E0\u03E0\u03E2\u03F3\u0401\u040C\u040E\u044F" +
            "\u0451\u045C\u045E\u0481\u0490\u04C4\u04C7\u04C8" +
            "\u04CB\u04CC\u04D0\u04EB\u04EE\u04F5\u04F8\u04F9" +
            "\u0531\u0556\u0559\u0559\u0561\u0586\u05D0\u05EA" +
            "\u05F0\u05F2\u0621\u063A\u0641\u064A\u0671\u06B7" +
            "\u06BA\u06BE\u06C0\u06CE\u06D0\u06D3\u06D5\u06D5" +
            "\u06E5\u06E6\u0905\u0939\u093D\u093D\u0958\u0961" +
            "\u0985\u098C\u098F\u0990\u0993\u09A8\u09AA\u09B0" +
            "\u09B2\u09B2\u09B6\u09B9\u09DC\u09DD\u09DF\u09E1" +
            "\u09F0\u09F1\u0A05\u0A0A\u0A0F\u0A10\u0A13\u0A28" +
            "\u0A2A\u0A30\u0A32\u0A33\u0A35\u0A36\u0A38\u0A39" +
            "\u0A59\u0A5C\u0A5E\u0A5E\u0A72\u0A74\u0A85\u0A8B" +
            "\u0A8D\u0A8D\u0A8F\u0A91\u0A93\u0AA8\u0AAA\u0AB0" +
            "\u0AB2\u0AB3\u0AB5\u0AB9\u0ABD\u0ABD\u0AE0\u0AE0" +
            "\u0B05\u0B0C\u0B0F\u0B10\u0B13\u0B28\u0B2A\u0B30" +
            "\u0B32\u0B33\u0B36\u0B39\u0B3D\u0B3D\u0B5C\u0B5D" +
            "\u0B5F\u0B61\u0B85\u0B8A\u0B8E\u0B90\u0B92\u0B95" +
            "\u0B99\u0B9A\u0B9C\u0B9C\u0B9E\u0B9F\u0BA3\u0BA4" +
            "\u0BA8\u0BAA\u0BAE\u0BB5\u0BB7\u0BB9\u0C05\u0C0C" +
            "\u0C0E\u0C10\u0C12\u0C28\u0C2A\u0C33\u0C35\u0C39" +
            "\u0C60\u0C61\u0C85\u0C8C\u0C8E\u0C90\u0C92\u0CA8" +
            "\u0CAA\u0CB3\u0CB5\u0CB9\u0CDE\u0CDE\u0CE0\u0CE1" +
            "\u0D05\u0D0C\u0D0E\u0D10\u0D12\u0D28\u0D2A\u0D39" +
            "\u0D60\u0D61\u0E01\u0E2E\u0E30\u0E30\u0E32\u0E33" +
            "\u0E40\u0E45\u0E81\u0E82\u0E84\u0E84\u0E87\u0E88" +
            "\u0E8A\u0E8A\u0E8D\u0E8D\u0E94\u0E97\u0E99\u0E9F" +
            "\u0EA1\u0EA3\u0EA5\u0EA5\u0EA7\u0EA7\u0EAA\u0EAB" +
            "\u0EAD\u0EAE\u0EB0\u0EB0\u0EB2\u0EB3\u0EBD\u0EBD" +
            "\u0EC0\u0EC4\u0F40\u0F47\u0F49\u0F69\u10A0\u10C5" +
            "\u10D0\u10F6\u1100\u1100\u1102\u1103\u1105\u1107" +
            "\u1109\u1109\u110B\u110C\u110E\u1112\u113C\u113C" +
            "\u113E\u113E\u1140\u1140\u114C\u114C\u114E\u114E" +
            "\u1150\u1150\u1154\u1155\u1159\u1159\u115F\u1161" +
            "\u1163\u1163\u1165\u1165\u1167\u1167\u1169\u1169" +
            "\u116D\u116E\u1172\u1173\u1175\u1175\u119E\u119E" +
            "\u11A8\u11A8\u11AB\u11AB\u11AE\u11AF\u11B7\u11B8" +
            "\u11BA\u11BA\u11BC\u11C2\u11EB\u11EB\u11F0\u11F0" +
            "\u11F9\u11F9\u1E00\u1E9B\u1EA0\u1EF9\u1F00\u1F15" +
            "\u1F18\u1F1D\u1F20\u1F45\u1F48\u1F4D\u1F50\u1F57" +
            "\u1F59\u1F59\u1F5B\u1F5B\u1F5D\u1F5D\u1F5F\u1F7D" +
            "\u1F80\u1FB4\u1FB6\u1FBC\u1FBE\u1FBE\u1FC2\u1FC4" +
            "\u1FC6\u1FCC\u1FD0\u1FD3\u1FD6\u1FDB\u1FE0\u1FEC" +
            "\u1FF2\u1FF4\u1FF6\u1FFC\u2126\u2126\u212A\u212B" +
            "\u212E\u212E\u2180\u2182\u3041\u3094\u30A1\u30FA" +
            "\u3105\u312C\uAC00\uD7A3";

        // IdeogramicChars
        // Taken from: "XML 1.0 (Second Edition)" (http://www.w3.org/TR/REC-xml) Appendix B
        // [#x4E00-#x9FA5] | #x3007 | [#x3021-#x3029]
        const string IdeogramicChars =
            "\u4E00\u9FA5\u3007\u3007\u3021\u3029";

        // CombiningChars
        // Taken from: "XML 1.0 (Second Edition)" (http://www.w3.org/TR/REC-xml) Appendix B
        // [#x0300-#x0345] | [#x0360-#x0361] | [#x0483-#x0486] | [#x0591-#x05A1] | [#x05A3-#x05B9] | [#x05BB-#x05BD] | #x05BF | [#x05C1-#x05C2] | #x05C4 | [#x064B-#x0652] | #x0670 | [#x06D6-#x06DC] | [#x06DD-#x06DF] | [#x06E0-#x06E4] | [#x06E7-#x06E8] | [#x06EA-#x06ED] | [#x0901-#x0903] | #x093C | [#x093E-#x094C] | #x094D | [#x0951-#x0954] | [#x0962-#x0963] | [#x0981-#x0983] | #x09BC | #x09BE | #x09BF | [#x09C0-#x09C4] | [#x09C7-#x09C8] | [#x09CB-#x09CD] | #x09D7 | [#x09E2-#x09E3] | #x0A02 | #x0A3C | #x0A3E | #x0A3F | [#x0A40-#x0A42] | [#x0A47-#x0A48] | [#x0A4B-#x0A4D] | [#x0A70-#x0A71] | [#x0A81-#x0A83] | #x0ABC | [#x0ABE-#x0AC5] | [#x0AC7-#x0AC9] | [#x0ACB-#x0ACD] | [#x0B01-#x0B03] | #x0B3C | [#x0B3E-#x0B43] | [#x0B47-#x0B48] | [#x0B4B-#x0B4D] | [#x0B56-#x0B57] | [#x0B82-#x0B83] | [#x0BBE-#x0BC2] | [#x0BC6-#x0BC8] | [#x0BCA-#x0BCD] | #x0BD7 | [#x0C01-#x0C03] | [#x0C3E-#x0C44] | [#x0C46-#x0C48] | [#x0C4A-#x0C4D] | [#x0C55-#x0C56] | [#x0C82-#x0C83] | [#x0CBE-#x0CC4] | [#x0CC6-#x0CC8] | [#x0CCA-#x0CCD] | [#x0CD5-#x0CD6] | [#x0D02-#x0D03] | [#x0D3E-#x0D43] | [#x0D46-#x0D48] | [#x0D4A-#x0D4D] | #x0D57 | #x0E31 | [#x0E34-#x0E3A] | [#x0E47-#x0E4E] | #x0EB1 | [#x0EB4-#x0EB9] | [#x0EBB-#x0EBC] | [#x0EC8-#x0ECD] | [#x0F18-#x0F19] | #x0F35 | #x0F37 | #x0F39 | #x0F3E | #x0F3F | [#x0F71-#x0F84] | [#x0F86-#x0F8B] | [#x0F90-#x0F95] | #x0F97 | [#x0F99-#x0FAD] | [#x0FB1-#x0FB7] | #x0FB9 | [#x20D0-#x20DC] | #x20E1 | [#x302A-#x302F] | #x3099 | #x309A
        const string CombiningChars =
            "\u0300\u0345\u0360\u0361\u0483\u0486\u0591\u05A1" +
            "\u05A3\u05B9\u05BB\u05BD\u05BF\u05BF\u05C1\u05C2" +
            "\u05C4\u05C4\u064B\u0652\u0670\u0670\u06D6\u06DC" +
            "\u06DD\u06DF\u06E0\u06E4\u06E7\u06E8\u06EA\u06ED" +
            "\u0901\u0903\u093C\u093C\u093E\u094C\u094D\u094D" +
            "\u0951\u0954\u0962\u0963\u0981\u0983\u09BC\u09BC" +
            "\u09BE\u09BE\u09BF\u09BF\u09C0\u09C4\u09C7\u09C8" +
            "\u09CB\u09CD\u09D7\u09D7\u09E2\u09E3\u0A02\u0A02" +
            "\u0A3C\u0A3C\u0A3E\u0A3E\u0A3F\u0A3F\u0A40\u0A42" +
            "\u0A47\u0A48\u0A4B\u0A4D\u0A70\u0A71\u0A81\u0A83" +
            "\u0ABC\u0ABC\u0ABE\u0AC5\u0AC7\u0AC9\u0ACB\u0ACD" +
            "\u0B01\u0B03\u0B3C\u0B3C\u0B3E\u0B43\u0B47\u0B48" +
            "\u0B4B\u0B4D\u0B56\u0B57\u0B82\u0B83\u0BBE\u0BC2" +
            "\u0BC6\u0BC8\u0BCA\u0BCD\u0BD7\u0BD7\u0C01\u0C03" +
            "\u0C3E\u0C44\u0C46\u0C48\u0C4A\u0C4D\u0C55\u0C56" +
            "\u0C82\u0C83\u0CBE\u0CC4\u0CC6\u0CC8\u0CCA\u0CCD" +
            "\u0CD5\u0CD6\u0D02\u0D03\u0D3E\u0D43\u0D46\u0D48" +
            "\u0D4A\u0D4D\u0D57\u0D57\u0E31\u0E31\u0E34\u0E3A" +
            "\u0E47\u0E4E\u0EB1\u0EB1\u0EB4\u0EB9\u0EBB\u0EBC" +
            "\u0EC8\u0ECD\u0F18\u0F19\u0F35\u0F35\u0F37\u0F37" +
            "\u0F39\u0F39\u0F3E\u0F3E\u0F3F\u0F3F\u0F71\u0F84" +
            "\u0F86\u0F8B\u0F90\u0F95\u0F97\u0F97\u0F99\u0FAD" +
            "\u0FB1\u0FB7\u0FB9\u0FB9\u20D0\u20DC\u20E1\u20E1" +
            "\u302A\u302F\u3099\u3099\u309A\u309A";

        // DigitChars
        // Taken from: "XML 1.0 (Second Edition)" (http://www.w3.org/TR/REC-xml) Appendix B
        // [#x0030-#x0039] | [#x0660-#x0669] | [#x06F0-#x06F9] | [#x0966-#x096F] | [#x09E6-#x09EF] | [#x0A66-#x0A6F] | [#x0AE6-#x0AEF] | [#x0B66-#x0B6F] | [#x0BE7-#x0BEF] | [#x0C66-#x0C6F] | [#x0CE6-#x0CEF] | [#x0D66-#x0D6F] | [#x0E50-#x0E59] | [#x0ED0-#x0ED9] | [#x0F20-#x0F29]
        const string DigitChars =
            "\u0030\u0039\u0660\u0669\u06F0\u06F9\u0966\u096F" +
            "\u09E6\u09EF\u0A66\u0A6F\u0AE6\u0AEF\u0B66\u0B6F" +
            "\u0BE7\u0BEF\u0C66\u0C6F\u0CE6\u0CEF\u0D66\u0D6F" +
            "\u0E50\u0E59\u0ED0\u0ED9\u0F20\u0F29";

        // ExtenderChars
        // Taken from: "XML 1.0 (Second Edition)" (http://www.w3.org/TR/REC-xml) Appendix B
        // #x00B7 | #x02D0 | #x02D1 | #x0387 | #x0640 | #x0E46 | #x0EC6 | #x3005 | [#x3031-#x3035] | [#x309D-#x309E] | [#x30FC-#x30FE]
        const string ExtenderChars =
            "\u00B7\u00B7\u02D0\u02D0\u02D1\u02D1\u0387\u0387" +
            "\u0640\u0640\u0E46\u0E46\u0EC6\u0EC6\u3005\u3005" +
            "\u3031\u3035\u309D\u309E\u30FC\u30FE";

        // WhitespaceChars
        // Taken from: "XML 1.0 (Second Edition)" (http://www.w3.org/TR/REC-xml) Section 2.3
        // #x0020 | #x0009 | #x000D | #x000A
        const string WhitespaceChars =
            "\u0020\u0020\u0009\u0009\u000D\u000D\u000A\u000A";

        // Other NCName start chars
        // Taken from: "Namespaces in XML" (http://www.w3.org/TR/REC-xml-names/) Section 2
        const string OtherNCNameStartChars =
            "__";

        // Other NCName chars
        // Taken from: "Namespaces in XML" (http://www.w3.org/TR/REC-xml-names/) Section 2
        const string OtherNCNameChars =
            "..--__";
        #endregion

        // Static Constructor
        // Initializes the table of unicode -> type mappings
        static XPathCharTypes()
        {
            if (charProperties != null)
            {
                return;
            }

            charProperties = new byte[char.MaxValue];

            // PERF: precompute the classes of each character set so SetProperties only needs to be run once per set.

            // Letter = BaseChar + Ideogramic
            SetProperties(BaseChars, Letter);
            SetProperties(IdeogramicChars, Letter);

            // Combining
            SetProperties(CombiningChars, Combining);

            // Digit
            SetProperties(DigitChars, Digit);

            // Extender
            SetProperties(ExtenderChars, Extender);

            // Whitespace
            SetProperties(WhitespaceChars, Whitespace);

            // NCNameStart = Base + Ideogramic + Other
            SetProperties(BaseChars, NCNameStart);
            SetProperties(IdeogramicChars, NCNameStart);
            SetProperties(OtherNCNameStartChars, NCNameStart);

            // NCName = NCNameStart + Combining + Extender
            SetProperties(BaseChars, NCName);
            SetProperties(IdeogramicChars, NCName);
            SetProperties(DigitChars, NCName);
            SetProperties(CombiningChars, NCName);
            SetProperties(ExtenderChars, NCName);
            SetProperties(OtherNCNameChars, NCName);
        }

        // Identify all described characters as belonging to a particular type
        private static void SetProperties(string ranges, byte value)
        {
            // Iterate over all characters in the table
            for (int p = 0; p < ranges.Length; p += 2)
            {
                // Iterate over all characters in a range
                for (int i = ranges[p], last = ranges[p + 1]; i <= last; i++)
                {
                    // Add the code to the character
                    charProperties[i] |= value;
                }
            }
        }

        // Get a character's code
        // This is done as a separate function for clarity and flexability
        // The compiler should just inline it
        private static byte GetCode(char c)
        {
            return charProperties[c];
        }


        #region Classifying functions

        // These are functions that simply test whether a character is of a particular type

#if NO
        internal static bool IsLetter(char c)
        {
            return ((GetCode(c) & Letter) != 0);
        }

        internal static bool IsCombining(char c)
        {
            return ((GetCode(c) & Combining) != 0);
        }
#endif
        internal static bool IsDigit(char c)
        {
            return ((GetCode(c) & Digit) != 0);
        }

#if NO
        internal static bool IsExtender(char c)
        {
            return ((GetCode(c) & Extender) != 0);
        }
#endif
        internal static bool IsWhitespace(char c)
        {
            return ((GetCode(c) & Whitespace) != 0);
        }

        internal static bool IsNCName(char c)
        {
            return ((GetCode(c) & NCName) != 0);
        }

        internal static bool IsNCNameStart(char c)
        {
            return ((GetCode(c) & NCNameStart) != 0);
        }
        #endregion
    }


    internal enum XPathTokenID
    {
        Unknown = 0x00000000,
        Terminal = 0x10000000,
        NameTest = 0x20000000,
        NodeType = 0x40000000,
        Operator = 0x01000000,
        NamedOperator = 0x02000000,
        Function = 0x04000000,
        Axis = 0x08000000,
        Literal = 0x00100000,
        Number = 0x00200000,
        Variable = 0x00400000,
        TypeMask = 0x7f400000,
        // terminals
        LParen = 0x00000001 | XPathTokenID.Terminal,
        RParen = 0x00000002 | XPathTokenID.Terminal,
        LBracket = 0x00000003 | XPathTokenID.Terminal,
        RBracket = 0x00000004 | XPathTokenID.Terminal,
        Period = 0x00000005 | XPathTokenID.Terminal,
        DblPeriod = 0x00000006 | XPathTokenID.Terminal,
        AtSign = 0x00000007 | XPathTokenID.Terminal,
        Comma = 0x00000008 | XPathTokenID.Terminal,
        DblColon = 0x00000009 | XPathTokenID.Terminal,
        Whitespace = 0x0000000A | XPathTokenID.Terminal,
        // operators
        Eq = 0x0000000B | XPathTokenID.Operator,
        Neq = 0x0000000C | XPathTokenID.Operator,
        Gt = 0x0000000D | XPathTokenID.Operator,
        Gte = 0x0000000E | XPathTokenID.Operator,
        Lt = 0x0000000F | XPathTokenID.Operator,
        Lte = 0x00000010 | XPathTokenID.Operator,
        Plus = 0x00000012 | XPathTokenID.Operator,
        Minus = 0x00000013 | XPathTokenID.Operator,
        Slash = 0x00000014 | XPathTokenID.Operator,
        Multiply = 0x00000015 | XPathTokenID.Operator,
        Pipe = 0x00000016 | XPathTokenID.Operator,
        DblSlash = 0x00000017 | XPathTokenID.Operator,
        Mod = 0x00000018 | XPathTokenID.NamedOperator,
        And = 0x00000019 | XPathTokenID.NamedOperator,
        Or = 0x0000001A | XPathTokenID.NamedOperator,
        Div = 0x0000001B | XPathTokenID.NamedOperator,
        // Literals
        Integer = 0x0000001C | XPathTokenID.Number,
        Decimal = 0x0000001D | XPathTokenID.Number,
        String = 0x0000001E | XPathTokenID.Literal,
        //
        Comment = 0x0000001F | XPathTokenID.NodeType,
        Text = 0x00000020 | XPathTokenID.NodeType,
        Processing = 0x00000021 | XPathTokenID.NodeType,
        Node = 0x00000022 | XPathTokenID.NodeType,
        Wildcard = 0x00000023 | XPathTokenID.NameTest,
        NameWildcard = 0x00000024 | XPathTokenID.NameTest,
        //QName = 0x00000025 | XPathTokenID.NameTest,
        // Keywords
        Ancestor = 0x00000027 | XPathTokenID.Axis,
        AncestorOrSelf = 0x00000028 | XPathTokenID.Axis,
        Attribute = 0x00000029 | XPathTokenID.Axis,
        Child = 0x0000002A | XPathTokenID.Axis,
        Descendant = 0x0000002B | XPathTokenID.Axis,
        DescendantOrSelf = 0x0000002C | XPathTokenID.Axis,
        Following = 0x0000002D | XPathTokenID.Axis,
        FollowingSibling = 0x0000002E | XPathTokenID.Axis,
        Namespace = 0x0000002F | XPathTokenID.Axis,
        Parent = 0x00000030 | XPathTokenID.Axis,
        Preceding = 0x00000031 | XPathTokenID.Axis,
        PrecedingSibling = 0x00000032 | XPathTokenID.Axis,
        Self = 0x00000033 | XPathTokenID.Axis
    }

    // Represents a single token of an XPath expression
    internal class XPathToken
    {
        string name;
        double number;
        string prefix;
        XPathTokenID tokenID;

        internal XPathToken()
        {
            this.tokenID = XPathTokenID.Unknown;
        }

        internal string Name
        {
            get
            {
                return this.name;
            }
        }

        internal double Number
        {
            get
            {
                return this.number;
            }
        }

        internal string Prefix
        {
            get
            {
                return this.prefix;
            }
        }

        internal XPathTokenID TokenID
        {
            get
            {
                return this.tokenID;
            }
        }

        internal void Clear()
        {
            this.number = double.NaN;
            this.prefix = string.Empty;
            this.name = string.Empty;
            this.tokenID = XPathTokenID.Unknown;
        }

        internal void Set(XPathTokenID id)
        {
            this.Clear();
            this.tokenID = id;
        }

        internal void Set(XPathTokenID id, double number)
        {
            this.Set(id);
            this.number = number;
        }

        internal void Set(XPathTokenID id, string name)
        {
            Fx.Assert(null != name, "");

            this.Clear();
            this.tokenID = id;
            this.name = name;
        }

        internal void Set(XPathTokenID id, XPathParser.QName qname)
        {
            this.Set(id, qname.Name);
            this.prefix = qname.Prefix;
        }
    }


    internal class XPathLexer
    {
        static Hashtable namedTypes;  // Mapping from named types to token IDs

        XPathTokenID previousID;
        string xpath;
        int tokenStart;
        int currChar;
        int xpathLength;
        char ch;
        XPathToken token;
        bool resolveKeywords;

        // Static Constructor
        // Set up the mapping of named types
        static XPathLexer()
        {
            namedTypes = new Hashtable();

            // Named operators
            namedTypes.Add("and", XPathTokenID.And);
            namedTypes.Add("or", XPathTokenID.Or);
            namedTypes.Add("mod", XPathTokenID.Mod);
            namedTypes.Add("div", XPathTokenID.Div);

            // Axes
            namedTypes.Add("ancestor", XPathTokenID.Ancestor);
            namedTypes.Add("ancestor-or-self", XPathTokenID.AncestorOrSelf);
            namedTypes.Add("attribute", XPathTokenID.Attribute);
            namedTypes.Add("child", XPathTokenID.Child);
            namedTypes.Add("descendant", XPathTokenID.Descendant);
            namedTypes.Add("descendant-or-self", XPathTokenID.DescendantOrSelf);
            namedTypes.Add("following", XPathTokenID.Following);
            namedTypes.Add("following-sibling", XPathTokenID.FollowingSibling);
            namedTypes.Add("namespace", XPathTokenID.Namespace);
            namedTypes.Add("parent", XPathTokenID.Parent);
            namedTypes.Add("preceding", XPathTokenID.Preceding);
            namedTypes.Add("preceding-sibling", XPathTokenID.PrecedingSibling);
            namedTypes.Add("self", XPathTokenID.Self);

            // Node types
            namedTypes.Add("comment", XPathTokenID.Comment);
            namedTypes.Add("text", XPathTokenID.Text);
            namedTypes.Add("processing-instruction", XPathTokenID.Processing);
            namedTypes.Add("node", XPathTokenID.Node);
        }

        internal XPathLexer(string xpath)
            : this(xpath, true)
        {
        }

        internal XPathLexer(string xpath, bool resolveKeywords)
        {
            this.resolveKeywords = resolveKeywords;

            // Hold on to a copy of the string so it can't be changed out from under us
            this.xpath = string.Copy(xpath);
            this.xpathLength = this.xpath.Length;

            // Start at the beginning
            this.tokenStart = 0;
            this.currChar = 0;

            this.ch = char.MinValue;
            this.previousID = XPathTokenID.Unknown;

            // We will not create new tokens, we will simply change the old one.
            // This will be the only XPathToken instance created by the lexer
            // The 'next token' data can be more quickly communicated to the parser if they both hold a reference to the data.
            this.token = new XPathToken();

            // Strip leading whitespace
            ConsumeWhitespace();
        }

        internal int FirstTokenChar
        {
            get
            {
                return this.tokenStart;
            }
        }

        internal XPathToken Token
        {
            get
            {
                // Return the lexer's token instance.
                return this.token;
            }
        }

        // Try to advance to the next character in the xpath string
        private bool AdvanceChar()
        {
            if (this.currChar < this.xpathLength)
            {
                // Advance to the next character
                this.ch = this.xpath[this.currChar];
                this.currChar++;
                return true;
            }
            else if (this.currChar == this.xpathLength)
            {
                // Signal that we're at the end of the string
                this.currChar++;
                this.ch = char.MinValue;
            }
            return false;
        }

        // Advance the 'start of token' marker to the current location in the string
        private void ConsumeToken()
        {
            this.tokenStart = this.currChar;
        }

        // Query for the portion of the xpath expression already consumed.
        internal string ConsumedSubstring()
        {
            return this.xpath.Substring(0, this.tokenStart);
        }

        // Query for the portion of the xpath expression currently being consumed as a token.
        private string CurrentSubstring()
        {
            return this.xpath.Substring(this.tokenStart, this.currChar - this.tokenStart);
        }

        private char PeekChar()
        {
            return PeekChar(1);
        }

        private char PeekChar(int offset)
        {
            int peekChar = this.currChar + offset - 1;
            if (peekChar < this.xpathLength)
            {
                return this.xpath[peekChar];
            }
            return char.MinValue;
        }

        private void PutbackChar()
        {
            if (this.currChar > this.tokenStart)
            {
                --this.currChar;
            }
        }

        // Move to the next token
        // This updates the values in the token instance and returns true if successful.
        internal bool MoveNext()
        {
            // Hold onto the ID of the last token.
            // It will be needed by some of the special cases.
            this.previousID = this.token.TokenID;

            // If there are no more characters, we can't get another token.
            if (!AdvanceChar())
            {
                return false;
            }

            if (XPathCharTypes.IsNCNameStart(this.ch))
            {
                // Extract a QName if we've got the start of an NCName
                TokenizeQName();
            }
            else if (XPathCharTypes.IsDigit(this.ch))
            {
                // Extract a number
                TokenizeNumber();
            }
            else
            {
                // Everything else is a single/double character token, or a variable.
                switch (this.ch)
                {
                    case '(':
                        token.Set(XPathTokenID.LParen);
                        break;

                    case ')':
                        token.Set(XPathTokenID.RParen);
                        break;

                    case '[':
                        token.Set(XPathTokenID.LBracket);
                        break;

                    case ']':
                        token.Set(XPathTokenID.RBracket);
                        break;

                    case '.':
                        // Watch for a double period
                        if (PeekChar() == '.')
                        {
                            AdvanceChar();
                            token.Set(XPathTokenID.DblPeriod);
                        }
                        else
                        {
                            // Check if the period is the start of a number
                            if (XPathCharTypes.IsDigit(PeekChar()))
                            {
                                TokenizeNumber();
                            }
                            else
                            {
                                token.Set(XPathTokenID.Period);
                            }
                        }
                        break;

                    case '@':
                        token.Set(XPathTokenID.AtSign);
                        break;

                    case ',':
                        token.Set(XPathTokenID.Comma);
                        break;

                    case ':':
                        // Only a double colon is permitted.
                        // The single colon part of the QName is consumed in TokenizeQName if it is valid
                        if (PeekChar() == ':')
                        {
                            AdvanceChar();
                            token.Set(XPathTokenID.DblColon);
                        }
                        else
                        {
                            ThrowError(QueryCompileError.UnexpectedToken, CurrentSubstring());
                        }
                        break;



                    case '/':
                        // Check for a double slash
                        if (PeekChar() == '/')
                        {
                            AdvanceChar();
                            token.Set(XPathTokenID.DblSlash);
                        }
                        else
                        {
                            token.Set(XPathTokenID.Slash);
                        }
                        break;

                    case '|':
                        token.Set(XPathTokenID.Pipe);
                        break;

                    case '+':
                        token.Set(XPathTokenID.Plus);
                        break;

                    case '-':
                        token.Set(XPathTokenID.Minus);
                        break;

                    case '=':
                        token.Set(XPathTokenID.Eq);
                        break;

                    case '!':
                        // This can only be the start of a '!='
                        // 'not' is a negation in XPath
                        if (PeekChar() == '=')
                        {
                            AdvanceChar();
                            token.Set(XPathTokenID.Neq);
                        }
                        else
                        {
                            ThrowError(QueryCompileError.UnsupportedOperator, CurrentSubstring());
                        }
                        break;

                    case '<':
                        // Watch for '<='
                        if (PeekChar() == '=')
                        {
                            AdvanceChar();
                            token.Set(XPathTokenID.Lte);
                        }
                        else
                        {
                            token.Set(XPathTokenID.Lt);
                        }
                        break;

                    case '>':
                        // Watch for '>='
                        if (PeekChar() == '=')
                        {
                            AdvanceChar();
                            token.Set(XPathTokenID.Gte);
                        }
                        else
                        {
                            token.Set(XPathTokenID.Gt);
                        }
                        break;


                    case '*':
                        // Check if we're supposed to parse a '*' as a multiply
                        if (IsSpecialPrev())
                        {
                            token.Set(XPathTokenID.Multiply);
                        }
                        else
                        {
                            token.Set(XPathTokenID.Wildcard, new XPathParser.QName(string.Empty, QueryDataModel.Wildcard));
                        }
                        break;

                    case '$':
                        // Make sure '$' was followed by something that counts as a variable name
                        XPathParser.QName qname = GetQName();
                        if (qname.Prefix.Length == 0 && qname.Name.Length == 0)
                        {
                            AdvanceChar();
                            ThrowError(QueryCompileError.InvalidVariable, this.ch == char.MinValue ? string.Empty : CurrentSubstring());
                        }
                        token.Set(XPathTokenID.Variable, qname);
                        break;

                    case '\"':
                        TokenizeLiteral('\"');
                        break;

                    case '\'':
                        TokenizeLiteral('\'');
                        break;

                    default:
                        // Unrecognized character
                        token.Set(XPathTokenID.Unknown);
                        break;
                }
            }

            // Whitespace can mark the end of a token, but is not part of the XPath syntax
            ConsumeWhitespace();

            return true;
        }

        private void ConsumeWhitespace()
        {
            // Advance over all whitespace characters and consume the all recently read characters
            for (; XPathCharTypes.IsWhitespace(PeekChar()); AdvanceChar());
            ConsumeToken();
        }

        private void TokenizeQName()
        {
            for (; XPathCharTypes.IsNCName(PeekChar()); AdvanceChar());

            string name1 = this.CurrentSubstring();
            XPathTokenID id = XPathTokenID.Unknown;
            XPathParser.QName qname = new XPathParser.QName("", "");

            if (PeekChar() == ':' && PeekChar(2) != ':')
            {
                AdvanceChar();
                ConsumeToken();
                AdvanceChar();
                if (XPathCharTypes.IsNCNameStart(this.ch))
                {
                    // It's a full QName
                    for (; XPathCharTypes.IsNCName(PeekChar()); AdvanceChar());
                    id = XPathTokenID.NameTest;
                    qname = new XPathParser.QName(name1, this.CurrentSubstring());
                }
                else if (this.ch == '*')
                {
                    // We've got a wildcard
                    id = XPathTokenID.NameWildcard;
                    qname = new XPathParser.QName(name1, QueryDataModel.Wildcard);
                }
                else
                {
                    ThrowError(QueryCompileError.InvalidNCName, this.ch == char.MinValue ? "" : CurrentSubstring());
                }
            }
            else
            {
                // It's a name test without a prefix
                id = XPathTokenID.NameTest;
                qname = new XPathParser.QName(string.Empty, name1);
            }

            // Handle special cases
            ConsumeWhitespace();
            if (IsSpecialPrev())
            {
                // If we're in the the first special case of the lexer, a qname MUST
                // be a NamedOperator
                token.Set(GetNamedOperator(qname));
                return;
            }
            else if (qname.Prefix.Length == 0)
            {
                if (this.PeekChar() == '(')
                {
                    // An NCName followed by a '(' MUST be eiter a node type or function name
                    id = GetNodeTypeOrFunction(qname);
                    if (id != XPathTokenID.Function)
                    {
                        token.Set(id);
                    }
                    else
                    {
                        token.Set(id, qname);
                    }
                }
                else if (this.PeekChar() == ':' && this.PeekChar(2) == ':')
                {
                    // An NCName followed by a '::' MUST be an axis
                    token.Set(GetAxisName(qname));
                }
                else
                {
                    token.Set(id, qname);
                }
            }
            else
            {
                if (this.PeekChar() == '(')
                {
                    id = XPathTokenID.Function;
                }
                token.Set(id, qname);
            }
        }

        private XPathParser.QName GetQName()
        {
            string name1 = GetNCName();

            // Return an empty QName if we can't read one
            if (name1 == null)
            {
                return new XPathParser.QName(string.Empty, string.Empty);
            }

            // Pull the '$' off a variable
            if (name1[0] == '$')
            {
                name1 = name1.Substring(1);
            }

            // See if there's a second part to the QName
            if (PeekChar() == ':' && XPathCharTypes.IsNCNameStart(PeekChar(2)))
            {
                AdvanceChar();
                ConsumeToken();
                return new XPathParser.QName(name1, GetNCName());
            }
            else
            {
                return new XPathParser.QName(string.Empty, name1);
            }
        }

        private string GetNCName()
        {
            // Make sure we're starting an NCName
            if (XPathCharTypes.IsNCNameStart(PeekChar()))
            {
                AdvanceChar();

                // Read all the NCName characters
                for (; XPathCharTypes.IsNCName(PeekChar()); AdvanceChar());

                // Extract, consume, and return the NCName
                string name = CurrentSubstring();
                ConsumeToken();
                return name;
            }
            else
            {
                return null;
            }
        }

        private void TokenizeNumber()
        {
            XPathTokenID id = XPathTokenID.Integer;

            // Read all the digits
            for (; XPathCharTypes.IsDigit(this.ch); AdvanceChar());
            if (this.ch == '.')
            {
                AdvanceChar();
                if (XPathCharTypes.IsDigit(this.ch))
                {
                    id = XPathTokenID.Decimal;
                    // Read all the digits after the decimal point
                    for (; XPathCharTypes.IsDigit(this.ch); AdvanceChar());
                }
            }
            PutbackChar();

            // The converted double
            double d = QueryValueModel.Double(CurrentSubstring());

            // flip the sign if we're negative
            token.Set(id, d);
        }

        private void TokenizeLiteral(char c)
        {
            // Consume the opening quote
            ConsumeToken();

            // Advance over all characters that are not the closing quote
            AdvanceChar();
            while (this.ch != c)
            {
                // Watch for an unclosed literal
                if (this.ch == char.MinValue)
                {
                    PutbackChar();
                    ThrowError(QueryCompileError.InvalidLiteral, CurrentSubstring());
                }
                AdvanceChar();
            }
            // Put back the closing quote
            PutbackChar();

            // Grab the literal string
            token.Set(XPathTokenID.Literal, CurrentSubstring());

            // Read the closing quote
            AdvanceChar();
        }

        private bool IsSpecialPrev()
        {
            // The first lexer special case is when there was a previous token and it
            // wasn't '@', '::', '(', '[', ',', an operator, or a named operator
            return (this.previousID != XPathTokenID.Unknown) &&
                (this.previousID != XPathTokenID.AtSign) &&
                (this.previousID != XPathTokenID.DblColon) &&
                (this.previousID != XPathTokenID.LParen) &&
                (this.previousID != XPathTokenID.LBracket) &&
                (this.previousID != XPathTokenID.Comma) &&
                (this.previousID & XPathTokenID.Operator) == 0 &&
                (this.previousID & XPathTokenID.NamedOperator) == 0;
        }

        private XPathTokenID GetNamedOperator(XPathParser.QName qname)
        {
            // Named operators can't have prefixes
            if (qname.Prefix.Length != 0)
            {
                ThrowError(QueryCompileError.InvalidOperatorName, qname.Prefix + ":" + qname.Name);
            }

            // Make sure the type is 'NamedOperator'
            XPathTokenID id = GetNamedType(qname.Name);
            if (this.resolveKeywords && (id & XPathTokenID.NamedOperator) == 0)
            {
                ThrowError(QueryCompileError.UnsupportedOperator, this.previousID.ToString() + "->" + qname.Name);
            }

            return id;
        }

        private XPathTokenID GetAxisName(XPathParser.QName qname)
        {
            // Axes can't have prefixes
            if (qname.Prefix.Length != 0)
            {
                ThrowError(QueryCompileError.InvalidAxisSpecifier, qname.Prefix + ":" + qname.Name);
            }

            // Make sure the type is 'Axis'
            XPathTokenID id = GetNamedType(qname.Name);
            if (this.resolveKeywords && (id & XPathTokenID.Axis) == 0)
            {
                ThrowError(QueryCompileError.UnsupportedAxis, qname.Name);
            }

            return id;
        }

        private XPathTokenID GetNodeTypeOrFunction(XPathParser.QName qname)
        {
            XPathTokenID id = GetNamedType(qname.Name);

            // If it's not a node type, it's lexed as a function
            if ((id & XPathTokenID.NodeType) == 0)
            {
                id = XPathTokenID.Function;
            }
            else if (qname.Prefix.Length > 0)
            {
                // Node types don't have prefixes
                ThrowError(QueryCompileError.InvalidNodeType, qname.Prefix + ":" + qname.Name);
            }

            return id;
        }

        private XPathTokenID GetNamedType(string name)
        {
            // Get the named type if one exists
            if (this.resolveKeywords && namedTypes.ContainsKey(name))
            {
                return (XPathTokenID)namedTypes[name];
            }
            else
            {
                return XPathTokenID.Unknown;
            }
        }

        private void ThrowError(QueryCompileError err, string msg)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryCompileException(err, msg));
        }
    }
}
