// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.Globalization
{
    using System;
    using System.Text;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Security;
    using System.Threading;
    using System.Diagnostics.Contracts;

    internal static partial class EncodingTable
    {
        static int GetNumEncodingItems ()
        {
            return encodingDataPtr.Length;
        }

#region "from coreclr/src/classlibnative/nls/encodingdata.cpp"
// as of d921298

    static InternalEncodingDataItem ENC (string name, ushort cp) { return new InternalEncodingDataItem () { webName = name, codePage = cp }; }

        internal static InternalEncodingDataItem [] encodingDataPtr = new InternalEncodingDataItem [] {
#if FEATURE_CORECLR
    // encoding name, codepage.
    ENC ("ANSI_X3.4-1968", 20127 ),
    ENC ("ANSI_X3.4-1986", 20127 ),
    ENC ("ascii", 20127 ),
    ENC ("cp367", 20127 ),
    ENC ("cp819", 28591 ),
    ENC ("csASCII", 20127 ),
    ENC ("csISOLatin1", 28591 ),
    ENC ("csUnicode11UTF7", 65000 ),
    ENC ("IBM367", 20127 ),
    ENC ("ibm819", 28591 ),
    ENC ("ISO-10646-UCS-2", 1200 ),
    ENC ("iso-8859-1", 28591 ),
    ENC ("iso-ir-100", 28591 ),
    ENC ("iso-ir-6", 20127 ),
    ENC ("ISO646-US", 20127 ),
    ENC ("iso8859-1", 28591 ),
    ENC ("ISO_646.irv:1991", 20127 ),
    ENC ("iso_8859-1", 28591 ),
    ENC ("iso_8859-1:1987", 28591 ),
    ENC ("l1", 28591 ),
    ENC ("latin1", 28591 ),
    ENC ("ucs-2", 1200 ),
    ENC ("unicode", 1200), 
    ENC ("unicode-1-1-utf-7", 65000 ),
    ENC ("unicode-1-1-utf-8", 65001 ),
    ENC ("unicode-2-0-utf-7", 65000 ),
    ENC ("unicode-2-0-utf-8", 65001 ),
    // People get confused about the FFFE here.  We can't change this because it'd break existing apps.
    // This has been this way for a long time, including in Mlang.
    ENC ("unicodeFFFE", 1201),             // Big Endian, BOM seems backwards, think of the BOM in little endian order.
    ENC ("us", 20127 ),
    ENC ("us-ascii", 20127 ),
    ENC ("utf-16", 1200 ),
    ENC ("UTF-16BE", 1201), 
    ENC ("UTF-16LE", 1200),        
    ENC ("utf-32", 12000 ),
    ENC ("UTF-32BE", 12001 ),
    ENC ("UTF-32LE", 12000 ),
    ENC ("utf-7", 65000 ),
    ENC ("utf-8", 65001 ),
    ENC ("x-unicode-1-1-utf-7", 65000 ),
    ENC ("x-unicode-1-1-utf-8", 65001 ),
    ENC ("x-unicode-2-0-utf-7", 65000 ),
    ENC ("x-unicode-2-0-utf-8", 65001 ),
#else
 // Total Items: 455
// encoding name, codepage.
ENC ("437", 437), 
ENC ("ANSI_X3.4-1968", 20127), 
ENC ("ANSI_X3.4-1986", 20127), 
// ENC (L"_autodetect", 50932), 
// ENC (L"_autodetect_all", 50001), 
// ENC (L"_autodetect_kr", 50949), 
ENC ("arabic", 28596), 
ENC ("ascii", 20127), 
ENC ("ASMO-708", 708), 
ENC ("Big5", 950), 
ENC ("Big5-HKSCS", 950), 
ENC ("CCSID00858", 858), 
ENC ("CCSID00924", 20924), 
ENC ("CCSID01140", 1140), 
ENC ("CCSID01141", 1141), 
ENC ("CCSID01142", 1142), 
ENC ("CCSID01143", 1143), 
ENC ("CCSID01144", 1144), 
ENC ("CCSID01145", 1145), 
ENC ("CCSID01146", 1146), 
ENC ("CCSID01147", 1147), 
ENC ("CCSID01148", 1148), 
ENC ("CCSID01149", 1149), 
ENC ("chinese", 936), 
ENC ("cn-big5", 950), 
ENC ("CN-GB", 936), 
ENC ("CP00858", 858), 
ENC ("CP00924", 20924), 
ENC ("CP01140", 1140), 
ENC ("CP01141", 1141), 
ENC ("CP01142", 1142), 
ENC ("CP01143", 1143), 
ENC ("CP01144", 1144), 
ENC ("CP01145", 1145), 
ENC ("CP01146", 1146), 
ENC ("CP01147", 1147), 
ENC ("CP01148", 1148), 
ENC ("CP01149", 1149), 
ENC ("cp037", 37), 
ENC ("cp1025", 21025), 
ENC ("CP1026", 1026), 
ENC ("cp1256", 1256), 
ENC ("CP273", 20273), 
ENC ("CP278", 20278), 
ENC ("CP280", 20280), 
ENC ("CP284", 20284), 
ENC ("CP285", 20285), 
ENC ("cp290", 20290), 
ENC ("cp297", 20297), 
ENC ("cp367", 20127), 
ENC ("cp420", 20420), 
ENC ("cp423", 20423), 
ENC ("cp424", 20424), 
ENC ("cp437", 437), 
ENC ("CP500", 500), 
ENC ("cp50227", 50227), 
    //ENC (L"cp50229", 50229), 
ENC ("cp819", 28591), 
ENC ("cp850", 850), 
ENC ("cp852", 852), 
ENC ("cp855", 855), 
ENC ("cp857", 857), 
ENC ("cp858", 858), 
ENC ("cp860", 860), 
ENC ("cp861", 861), 
ENC ("cp862", 862), 
ENC ("cp863", 863), 
ENC ("cp864", 864), 
ENC ("cp865", 865), 
ENC ("cp866", 866), 
ENC ("cp869", 869), 
ENC ("CP870", 870), 
ENC ("CP871", 20871), 
ENC ("cp875", 875), 
ENC ("cp880", 20880), 
ENC ("CP905", 20905), 
//ENC (L"cp930", 50930), 
//ENC (L"cp933", 50933), 
//ENC (L"cp935", 50935), 
//ENC (L"cp937", 50937), 
//ENC (L"cp939", 50939), 
ENC ("csASCII", 20127), 
ENC ("csbig5", 950), 
ENC ("csEUCKR", 51949), 
ENC ("csEUCPkdFmtJapanese", 51932), 
ENC ("csGB2312", 936), 
ENC ("csGB231280", 936), 
ENC ("csIBM037", 37), 
ENC ("csIBM1026", 1026), 
ENC ("csIBM273", 20273), 
ENC ("csIBM277", 20277), 
ENC ("csIBM278", 20278), 
ENC ("csIBM280", 20280), 
ENC ("csIBM284", 20284), 
ENC ("csIBM285", 20285), 
ENC ("csIBM290", 20290), 
ENC ("csIBM297", 20297), 
ENC ("csIBM420", 20420), 
ENC ("csIBM423", 20423), 
ENC ("csIBM424", 20424), 
ENC ("csIBM500", 500), 
ENC ("csIBM870", 870), 
ENC ("csIBM871", 20871), 
ENC ("csIBM880", 20880), 
ENC ("csIBM905", 20905), 
ENC ("csIBMThai", 20838), 
ENC ("csISO2022JP", 50221), 
ENC ("csISO2022KR", 50225), 
ENC ("csISO58GB231280", 936), 
ENC ("csISOLatin1", 28591), 
ENC ("csISOLatin2", 28592), 
ENC ("csISOLatin3", 28593), 
ENC ("csISOLatin4", 28594), 
ENC ("csISOLatin5", 28599), 
ENC ("csISOLatin9", 28605), 
ENC ("csISOLatinArabic", 28596), 
ENC ("csISOLatinCyrillic", 28595), 
ENC ("csISOLatinGreek", 28597), 
ENC ("csISOLatinHebrew", 28598), 
ENC ("csKOI8R", 20866), 
ENC ("csKSC56011987", 949), 
ENC ("csPC8CodePage437", 437), 
ENC ("csShiftJIS", 932), 
ENC ("csUnicode11UTF7", 65000), 
ENC ("csWindows31J", 932), 
ENC ("cyrillic", 28595), 
ENC ("DIN_66003", 20106), 
ENC ("DOS-720", 720), 
ENC ("DOS-862", 862), 
ENC ("DOS-874", 874), 
ENC ("ebcdic-cp-ar1", 20420), 
ENC ("ebcdic-cp-be", 500), 
ENC ("ebcdic-cp-ca", 37), 
ENC ("ebcdic-cp-ch", 500), 
ENC ("EBCDIC-CP-DK", 20277), 
ENC ("ebcdic-cp-es", 20284), 
ENC ("ebcdic-cp-fi", 20278), 
ENC ("ebcdic-cp-fr", 20297), 
ENC ("ebcdic-cp-gb", 20285), 
ENC ("ebcdic-cp-gr", 20423), 
ENC ("ebcdic-cp-he", 20424), 
ENC ("ebcdic-cp-is", 20871), 
ENC ("ebcdic-cp-it", 20280), 
ENC ("ebcdic-cp-nl", 37), 
ENC ("EBCDIC-CP-NO", 20277), 
ENC ("ebcdic-cp-roece", 870), 
ENC ("ebcdic-cp-se", 20278), 
ENC ("ebcdic-cp-tr", 20905), 
ENC ("ebcdic-cp-us", 37), 
ENC ("ebcdic-cp-wt", 37), 
ENC ("ebcdic-cp-yu", 870), 
ENC ("EBCDIC-Cyrillic", 20880), 
ENC ("ebcdic-de-273+euro", 1141), 
ENC ("ebcdic-dk-277+euro", 1142), 
ENC ("ebcdic-es-284+euro", 1145), 
ENC ("ebcdic-fi-278+euro", 1143), 
ENC ("ebcdic-fr-297+euro", 1147), 
ENC ("ebcdic-gb-285+euro", 1146), 
ENC ("ebcdic-international-500+euro", 1148), 
ENC ("ebcdic-is-871+euro", 1149), 
ENC ("ebcdic-it-280+euro", 1144), 
ENC ("EBCDIC-JP-kana", 20290), 
ENC ("ebcdic-Latin9--euro", 20924), 
ENC ("ebcdic-no-277+euro", 1142), 
ENC ("ebcdic-se-278+euro", 1143), 
ENC ("ebcdic-us-37+euro", 1140), 
ENC ("ECMA-114", 28596), 
ENC ("ECMA-118", 28597), 
ENC ("ELOT_928", 28597), 
ENC ("euc-cn", 51936), 
ENC ("euc-jp", 51932), 
ENC ("euc-kr", 51949), 
ENC ("Extended_UNIX_Code_Packed_Format_for_Japanese", 51932), 
ENC ("GB18030", 54936), 
ENC ("GB2312", 936), 
ENC ("GB2312-80", 936), 
ENC ("GB231280", 936), 
ENC ("GBK", 936), 
ENC ("GB_2312-80", 936), 
ENC ("German", 20106), 
ENC ("greek", 28597), 
ENC ("greek8", 28597), 
ENC ("hebrew", 28598), 
ENC ("hz-gb-2312", 52936), 
ENC ("IBM-Thai", 20838), 
ENC ("IBM00858", 858), 
ENC ("IBM00924", 20924), 
ENC ("IBM01047", 1047), 
ENC ("IBM01140", 1140), 
ENC ("IBM01141", 1141), 
ENC ("IBM01142", 1142), 
ENC ("IBM01143", 1143), 
ENC ("IBM01144", 1144), 
ENC ("IBM01145", 1145), 
ENC ("IBM01146", 1146), 
ENC ("IBM01147", 1147), 
ENC ("IBM01148", 1148), 
ENC ("IBM01149", 1149), 
ENC ("IBM037", 37), 
ENC ("IBM1026", 1026), 
ENC ("IBM273", 20273), 
ENC ("IBM277", 20277), 
ENC ("IBM278", 20278), 
ENC ("IBM280", 20280), 
ENC ("IBM284", 20284), 
ENC ("IBM285", 20285), 
ENC ("IBM290", 20290), 
ENC ("IBM297", 20297), 
ENC ("IBM367", 20127), 
ENC ("IBM420", 20420), 
ENC ("IBM423", 20423), 
ENC ("IBM424", 20424), 
ENC ("IBM437", 437), 
ENC ("IBM500", 500), 
ENC ("ibm737", 737), 
ENC ("ibm775", 775), 
ENC ("ibm819", 28591), 
ENC ("IBM850", 850), 
ENC ("IBM852", 852), 
ENC ("IBM855", 855), 
ENC ("IBM857", 857), 
ENC ("IBM860", 860), 
ENC ("IBM861", 861), 
ENC ("IBM862", 862), 
ENC ("IBM863", 863), 
ENC ("IBM864", 864), 
ENC ("IBM865", 865), 
ENC ("IBM866", 866), 
ENC ("IBM869", 869), 
ENC ("IBM870", 870), 
ENC ("IBM871", 20871), 
ENC ("IBM880", 20880), 
ENC ("IBM905", 20905), 
ENC ("irv", 20105), 
ENC ("ISO-10646-UCS-2", 1200), 
ENC ("iso-2022-jp", 50220), 
ENC ("iso-2022-jpeuc", 51932), 
ENC ("iso-2022-kr", 50225), 
ENC ("iso-2022-kr-7", 50225), 
ENC ("iso-2022-kr-7bit", 50225), 
ENC ("iso-2022-kr-8", 51949), 
ENC ("iso-2022-kr-8bit", 51949), 
ENC ("iso-8859-1", 28591), 
ENC ("iso-8859-11", 874), 
ENC ("iso-8859-13", 28603), 
ENC ("iso-8859-15", 28605), 
ENC ("iso-8859-2", 28592), 
ENC ("iso-8859-3", 28593), 
ENC ("iso-8859-4", 28594), 
ENC ("iso-8859-5", 28595), 
ENC ("iso-8859-6", 28596), 
ENC ("iso-8859-7", 28597), 
ENC ("iso-8859-8", 28598), 
ENC ("ISO-8859-8 Visual", 28598), 
ENC ("iso-8859-8-i", 38598), 
ENC ("iso-8859-9", 28599), 
ENC ("iso-ir-100", 28591), 
ENC ("iso-ir-101", 28592), 
ENC ("iso-ir-109", 28593), 
ENC ("iso-ir-110", 28594), 
ENC ("iso-ir-126", 28597), 
ENC ("iso-ir-127", 28596), 
ENC ("iso-ir-138", 28598), 
ENC ("iso-ir-144", 28595), 
ENC ("iso-ir-148", 28599), 
ENC ("iso-ir-149", 949), 
ENC ("iso-ir-58", 936), 
ENC ("iso-ir-6", 20127), 
ENC ("ISO646-US", 20127), 
ENC ("iso8859-1", 28591), 
ENC ("iso8859-2", 28592), 
ENC ("ISO_646.irv:1991", 20127), 
ENC ("iso_8859-1", 28591), 
ENC ("ISO_8859-15", 28605), 
ENC ("iso_8859-1:1987", 28591), 
ENC ("iso_8859-2", 28592), 
ENC ("iso_8859-2:1987", 28592), 
ENC ("ISO_8859-3", 28593), 
ENC ("ISO_8859-3:1988", 28593), 
ENC ("ISO_8859-4", 28594), 
ENC ("ISO_8859-4:1988", 28594), 
ENC ("ISO_8859-5", 28595), 
ENC ("ISO_8859-5:1988", 28595), 
ENC ("ISO_8859-6", 28596), 
ENC ("ISO_8859-6:1987", 28596), 
ENC ("ISO_8859-7", 28597), 
ENC ("ISO_8859-7:1987", 28597), 
ENC ("ISO_8859-8", 28598), 
ENC ("ISO_8859-8:1988", 28598), 
ENC ("ISO_8859-9", 28599), 
ENC ("ISO_8859-9:1989", 28599), 
ENC ("Johab", 1361), 
ENC ("koi", 20866), 
ENC ("koi8", 20866), 
ENC ("koi8-r", 20866), 
ENC ("koi8-ru", 21866), 
ENC ("koi8-u", 21866), 
ENC ("koi8r", 20866), 
ENC ("korean", 949), 
ENC ("ks-c-5601", 949), 
ENC ("ks-c5601", 949), 
ENC ("KSC5601", 949), 
ENC ("KSC_5601", 949), 
ENC ("ks_c_5601", 949), 
ENC ("ks_c_5601-1987", 949), 
ENC ("ks_c_5601-1989", 949), 
ENC ("ks_c_5601_1987", 949), 
ENC ("l1", 28591), 
ENC ("l2", 28592), 
ENC ("l3", 28593), 
ENC ("l4", 28594), 
ENC ("l5", 28599), 
ENC ("l9", 28605), 
ENC ("latin1", 28591), 
ENC ("latin2", 28592), 
ENC ("latin3", 28593), 
ENC ("latin4", 28594), 
ENC ("latin5", 28599), 
ENC ("latin9", 28605), 
ENC ("logical", 28598), 
ENC ("macintosh", 10000), 
ENC ("ms_Kanji", 932), 
ENC ("Norwegian", 20108), 
ENC ("NS_4551-1", 20108), 
ENC ("PC-Multilingual-850+euro", 858), 
ENC ("SEN_850200_B", 20107), 
ENC ("shift-jis", 932), 
ENC ("shift_jis", 932), 
ENC ("sjis", 932), 
ENC ("Swedish", 20107), 
ENC ("TIS-620", 874), 
ENC ("ucs-2", 1200), 
ENC ("unicode", 1200), 
ENC ("unicode-1-1-utf-7", 65000), 
ENC ("unicode-1-1-utf-8", 65001), 
ENC ("unicode-2-0-utf-7", 65000), 
ENC ("unicode-2-0-utf-8", 65001), 
// People get confused about the FFFE here.  We can't change this because it'd break existing apps.
// This has been this way for a long time, including in Mlang.
ENC ("unicodeFFFE", 1201),             // Big Endian, BOM seems backwards, think of the BOM in little endian order.
ENC ("us", 20127), 
ENC ("us-ascii", 20127), 
ENC ("utf-16", 1200), 
ENC ("UTF-16BE", 1201), 
ENC ("UTF-16LE", 1200),
ENC ("utf-32", 12000),
ENC ("UTF-32BE", 12001),
ENC ("UTF-32LE", 12000),
ENC ("utf-7", 65000), 
ENC ("utf-8", 65001),
ENC ("visual", 28598), 
ENC ("windows-1250", 1250), 
ENC ("windows-1251", 1251), 
ENC ("windows-1252", 1252), 
ENC ("windows-1253", 1253), 
ENC ("Windows-1254", 1254), 
ENC ("windows-1255", 1255), 
ENC ("windows-1256", 1256), 
ENC ("windows-1257", 1257), 
ENC ("windows-1258", 1258), 
ENC ("windows-874", 874), 
ENC ("x-ansi", 1252), 
ENC ("x-Chinese-CNS", 20000), 
ENC ("x-Chinese-Eten", 20002), 
ENC ("x-cp1250", 1250), 
ENC ("x-cp1251", 1251), 
ENC ("x-cp20001", 20001), 
ENC ("x-cp20003", 20003), 
ENC ("x-cp20004", 20004), 
ENC ("x-cp20005", 20005), 
ENC ("x-cp20261", 20261), 
ENC ("x-cp20269", 20269), 
ENC ("x-cp20936", 20936), 
ENC ("x-cp20949", 20949),
ENC ("x-cp50227", 50227), 
//ENC (L"x-cp50229", 50229), 
//ENC (L"X-EBCDIC-JapaneseAndUSCanada", 50931), 
ENC ("X-EBCDIC-KoreanExtended", 20833), 
ENC ("x-euc", 51932), 
ENC ("x-euc-cn", 51936), 
ENC ("x-euc-jp", 51932), 
ENC ("x-Europa", 29001), 
ENC ("x-IA5", 20105), 
ENC ("x-IA5-German", 20106), 
ENC ("x-IA5-Norwegian", 20108), 
ENC ("x-IA5-Swedish", 20107), 
ENC ("x-iscii-as", 57006), 
ENC ("x-iscii-be", 57003), 
ENC ("x-iscii-de", 57002), 
ENC ("x-iscii-gu", 57010), 
ENC ("x-iscii-ka", 57008), 
ENC ("x-iscii-ma", 57009), 
ENC ("x-iscii-or", 57007), 
ENC ("x-iscii-pa", 57011), 
ENC ("x-iscii-ta", 57004), 
ENC ("x-iscii-te", 57005), 
ENC ("x-mac-arabic", 10004), 
ENC ("x-mac-ce", 10029), 
ENC ("x-mac-chinesesimp", 10008), 
ENC ("x-mac-chinesetrad", 10002), 
ENC ("x-mac-croatian", 10082), 
ENC ("x-mac-cyrillic", 10007), 
ENC ("x-mac-greek", 10006), 
ENC ("x-mac-hebrew", 10005), 
ENC ("x-mac-icelandic", 10079), 
ENC ("x-mac-japanese", 10001), 
ENC ("x-mac-korean", 10003), 
ENC ("x-mac-romanian", 10010), 
ENC ("x-mac-thai", 10021), 
ENC ("x-mac-turkish", 10081), 
ENC ("x-mac-ukrainian", 10017), 
ENC ("x-ms-cp932", 932),
ENC ("x-sjis", 932), 
ENC ("x-unicode-1-1-utf-7", 65000), 
ENC ("x-unicode-1-1-utf-8", 65001), 
ENC ("x-unicode-2-0-utf-7", 65000), 
ENC ("x-unicode-2-0-utf-8", 65001), 
ENC ("x-x-big5", 950), 

#endif // FEATURE_CORECLR
    
        };

// Working set optimization: 
// 1. code page, family code page stored as unsigned short
// 2. if web/header/body names are the same, only web name is stored; otherwise, we store "|webname|headername|bodyname"
// 3. Move flags before names to fill gap on 64-bit platforms

    static InternalCodePageDataItem MapCodePageDataItem (UInt16 cp, UInt16 fcp, string names, uint flags) { return new InternalCodePageDataItem () { codePage = cp, uiFamilyCodePage = fcp, flags = flags, Names = names }; }
//
// Information about codepages.
//
    internal static InternalCodePageDataItem [] codePageDataPtr = new InternalCodePageDataItem [] {
#if FEATURE_CORECLR

// Total Items: 
// code page, family code page, web name, header name, body name, flags

    MapCodePageDataItem(  1200,  1200, "utf-16",      MIMECONTF_SAVABLE_BROWSER), // "Unicode"
    MapCodePageDataItem(  1201,  1200, "utf-16BE",    0), // Big Endian, old FFFE BOM seems backwards, think of the BOM in little endian order.
    MapCodePageDataItem(  12000, 1200, "utf-32", 0), // "Unicode (UTF-32)"
    MapCodePageDataItem(  12001, 1200, "utf-32BE", 0), // "Unicode (UTF-32 Big Endian)"
    MapCodePageDataItem(  20127, 1252, "us-ascii", MIMECONTF_MAILNEWS | MIMECONTF_SAVABLE_MAILNEWS), // "US-ASCII"
    MapCodePageDataItem(  28591,  1252, "iso-8859-1",  MIMECONTF_MAILNEWS | MIMECONTF_BROWSER | MIMECONTF_SAVABLE_MAILNEWS | MIMECONTF_SAVABLE_BROWSER), // "Western European (ISO)"
    MapCodePageDataItem(  65000, 1200, "utf-7", MIMECONTF_MAILNEWS | MIMECONTF_SAVABLE_MAILNEWS), // "Unicode (UTF-7)"
    MapCodePageDataItem(  65001, 1200, "utf-8", MIMECONTF_MAILNEWS | MIMECONTF_BROWSER | MIMECONTF_SAVABLE_MAILNEWS | MIMECONTF_SAVABLE_BROWSER), // "Unicode (UTF-8)"

#else //FEATURE_CORECLR

// Total Items: 146
// code page, family code page, web name, header name, body name, flags


    MapCodePageDataItem(    37,  1252, "IBM037",      0), // "IBM EBCDIC (US-Canada)"
    MapCodePageDataItem(   437,  1252, "IBM437",      0), // "OEM United States"
    MapCodePageDataItem(   500,  1252, "IBM500",      0), // "IBM EBCDIC (International)"
    MapCodePageDataItem(   708,  1256, "ASMO-708",    MIMECONTF_BROWSER | MIMECONTF_SAVABLE_BROWSER), // "Arabic (ASMO 708)"
//    MapCodePageDataItem(   720,  1256, "DOS-720",     MIMECONTF_BROWSER | MIMECONTF_SAVABLE_BROWSER), // "Arabic (DOS)"
    MapCodePageDataItem(   737,  1253, "ibm737",      0), // "Greek (DOS)"
    MapCodePageDataItem(   775,  1257, "ibm775",      0), // "Baltic (DOS)"
    MapCodePageDataItem(   850,  1252, "ibm850",      0), // "Western European (DOS)"
    MapCodePageDataItem(   852,  1250, "ibm852",      MIMECONTF_BROWSER | MIMECONTF_SAVABLE_BROWSER), // "Central European (DOS)"
    MapCodePageDataItem(   855,  1252, "IBM855",      0), // "OEM Cyrillic"
    MapCodePageDataItem(   857,  1254, "ibm857",      0), // "Turkish (DOS)"
    MapCodePageDataItem(   858,  1252, "IBM00858",    0), // "OEM Multilingual Latin I"
    MapCodePageDataItem(   860,  1252, "IBM860",      0), // "Portuguese (DOS)"
    MapCodePageDataItem(   861,  1252, "ibm861",      0), // "Icelandic (DOS)"
    MapCodePageDataItem(   862,  1255, "DOS-862",     MIMECONTF_BROWSER | MIMECONTF_SAVABLE_BROWSER), // "Hebrew (DOS)"
    MapCodePageDataItem(   863,  1252, "IBM863",      0), // "French Canadian (DOS)"
    MapCodePageDataItem(   864,  1256, "IBM864",      0), // "Arabic (864)"
    MapCodePageDataItem(   865,  1252, "IBM865",      0), // "Nordic (DOS)"
    MapCodePageDataItem(   866,  1251, "cp866",       MIMECONTF_BROWSER | MIMECONTF_SAVABLE_BROWSER), // "Cyrillic (DOS)"
    MapCodePageDataItem(   869,  1253, "ibm869",      0), // "Greek, Modern (DOS)"
    MapCodePageDataItem(   870,  1250, "IBM870",      0), // "IBM EBCDIC (Multilingual Latin-2)"
    MapCodePageDataItem(   874,   874, "windows-874", MIMECONTF_MAILNEWS | MIMECONTF_BROWSER | MIMECONTF_SAVABLE_MAILNEWS | MIMECONTF_SAVABLE_BROWSER), // "Thai (Windows)"
    MapCodePageDataItem(   875,  1253, "cp875",       0), // "IBM EBCDIC (Greek Modern)"
    MapCodePageDataItem(   932,   932, "|shift_jis|iso-2022-jp|iso-2022-jp",   MIMECONTF_MAILNEWS | MIMECONTF_BROWSER | MIMECONTF_SAVABLE_MAILNEWS | MIMECONTF_SAVABLE_BROWSER), // "Japanese (Shift-JIS)"
    MapCodePageDataItem(   936,   936, "gb2312",      MIMECONTF_MAILNEWS | MIMECONTF_BROWSER | MIMECONTF_SAVABLE_MAILNEWS | MIMECONTF_SAVABLE_BROWSER), // "Chinese Simplified (GB2312)"
    MapCodePageDataItem(   949,   949, "ks_c_5601-1987", MIMECONTF_MAILNEWS | MIMECONTF_BROWSER | MIMECONTF_SAVABLE_MAILNEWS | MIMECONTF_SAVABLE_BROWSER), // "Korean"
    MapCodePageDataItem(   950,   950, "big5",        MIMECONTF_MAILNEWS | MIMECONTF_BROWSER | MIMECONTF_SAVABLE_MAILNEWS | MIMECONTF_SAVABLE_BROWSER), // "Chinese Traditional (Big5)"
    MapCodePageDataItem(  1026,  1254, "IBM1026",     0), // "IBM EBCDIC (Turkish Latin-5)"
    MapCodePageDataItem(  1047,  1252, "IBM01047",    0), // "IBM Latin-1"
    MapCodePageDataItem(  1140,  1252, "IBM01140",    0), // "IBM EBCDIC (US-Canada-Euro)"
    MapCodePageDataItem(  1141,  1252, "IBM01141",    0), // "IBM EBCDIC (Germany-Euro)"
    MapCodePageDataItem(  1142,  1252, "IBM01142",    0), // "IBM EBCDIC (Denmark-Norway-Euro)"
    MapCodePageDataItem(  1143,  1252, "IBM01143",    0), // "IBM EBCDIC (Finland-Sweden-Euro)"
    MapCodePageDataItem(  1144,  1252, "IBM01144",    0), // "IBM EBCDIC (Italy-Euro)"
    MapCodePageDataItem(  1145,  1252, "IBM01145",    0), // "IBM EBCDIC (Spain-Euro)"
    MapCodePageDataItem(  1146,  1252, "IBM01146",    0), // "IBM EBCDIC (UK-Euro)"
    MapCodePageDataItem(  1147,  1252, "IBM01147",    0), // "IBM EBCDIC (France-Euro)"
    MapCodePageDataItem(  1148,  1252, "IBM01148",    0), // "IBM EBCDIC (International-Euro)"
    MapCodePageDataItem(  1149,  1252, "IBM01149",    0), // "IBM EBCDIC (Icelandic-Euro)"
    MapCodePageDataItem(  1200,  1200, "utf-16",      MIMECONTF_SAVABLE_BROWSER), // "Unicode"
    MapCodePageDataItem(  1201,  1200, "utf-16BE",    0), // Big Endian, old FFFE BOM seems backwards, think of the BOM in little endian order.
    MapCodePageDataItem(  1250,  1250, "|windows-1250|windows-1250|iso-8859-2", MIMECONTF_MAILNEWS | MIMECONTF_BROWSER | MIMECONTF_SAVABLE_MAILNEWS | MIMECONTF_SAVABLE_BROWSER), // "Central European (Windows)"
    MapCodePageDataItem(  1251,  1251, "|windows-1251|windows-1251|koi8-r", MIMECONTF_MAILNEWS | MIMECONTF_BROWSER | MIMECONTF_SAVABLE_MAILNEWS | MIMECONTF_SAVABLE_BROWSER), // "Cyrillic (Windows)"
    MapCodePageDataItem(  1252,  1252, "|Windows-1252|Windows-1252|iso-8859-1", MIMECONTF_MAILNEWS | MIMECONTF_BROWSER | MIMECONTF_SAVABLE_MAILNEWS | MIMECONTF_SAVABLE_BROWSER), // "Western European (Windows)"
    MapCodePageDataItem(  1253,  1253, "|windows-1253|windows-1253|iso-8859-7", MIMECONTF_MAILNEWS | MIMECONTF_BROWSER | MIMECONTF_SAVABLE_MAILNEWS | MIMECONTF_SAVABLE_BROWSER), // "Greek (Windows)"
    MapCodePageDataItem(  1254,  1254, "|windows-1254|windows-1254|iso-8859-9", MIMECONTF_MAILNEWS | MIMECONTF_BROWSER | MIMECONTF_SAVABLE_MAILNEWS | MIMECONTF_SAVABLE_BROWSER), // "Turkish (Windows)"
    MapCodePageDataItem(  1255,  1255, "windows-1255", MIMECONTF_MAILNEWS | MIMECONTF_BROWSER | MIMECONTF_SAVABLE_MAILNEWS | MIMECONTF_SAVABLE_BROWSER), // "Hebrew (Windows)"
    MapCodePageDataItem(  1256,  1256, "windows-1256", MIMECONTF_MAILNEWS | MIMECONTF_BROWSER | MIMECONTF_SAVABLE_MAILNEWS | MIMECONTF_SAVABLE_BROWSER), // "Arabic (Windows)"
    MapCodePageDataItem(  1257,  1257, "windows-1257", MIMECONTF_MAILNEWS | MIMECONTF_BROWSER | MIMECONTF_SAVABLE_MAILNEWS | MIMECONTF_SAVABLE_BROWSER), // "Baltic (Windows)"
    MapCodePageDataItem(  1258,  1258, "windows-1258", MIMECONTF_MAILNEWS | MIMECONTF_BROWSER | MIMECONTF_SAVABLE_MAILNEWS | MIMECONTF_SAVABLE_BROWSER), // "Vietnamese (Windows)"
//    MapCodePageDataItem(  1361,   949, "Johab",        0), // "Korean (Johab)"
    MapCodePageDataItem( 10000,  1252, "macintosh",    0), // "Western European (Mac)"
/*
    MapCodePageDataItem( 10001,   932, "x-mac-japanese", 0), // "Japanese (Mac)"
    MapCodePageDataItem( 10002,   950, "x-mac-chinesetrad",   0), // "Chinese Traditional (Mac)"
    MapCodePageDataItem( 10003,   949, "x-mac-korean",        0), // "Korean (Mac)"
    MapCodePageDataItem( 10004,  1256, "x-mac-arabic",        0), // "Arabic (Mac)"
    MapCodePageDataItem( 10005,  1255, "x-mac-hebrew",        0), // "Hebrew (Mac)"
    MapCodePageDataItem( 10006,  1253, "x-mac-greek",         0), // "Greek (Mac)"
    MapCodePageDataItem( 10007,  1251, "x-mac-cyrillic",      0), // "Cyrillic (Mac)"
    MapCodePageDataItem( 10008,   936, "x-mac-chinesesimp",   0), // "Chinese Simplified (Mac)"
    MapCodePageDataItem( 10010,  1250, "x-mac-romanian",      0), // "Romanian (Mac)"
    MapCodePageDataItem( 10017,  1251, "x-mac-ukrainian",     0), // "Ukrainian (Mac)"
    MapCodePageDataItem( 10021,   874, "x-mac-thai",          0), // "Thai (Mac)"
    MapCodePageDataItem( 10029,  1250, "x-mac-ce",            0), // "Central European (Mac)"
*/
    MapCodePageDataItem( 10079,  1252, "x-mac-icelandic",     0), // "Icelandic (Mac)"
//    MapCodePageDataItem( 10081,  1254, "x-mac-turkish",       0), // "Turkish (Mac)"
//    MapCodePageDataItem( 10082,  1250, "x-mac-croatian",      0), // "Croatian (Mac)"
    MapCodePageDataItem( 12000,  1200, "utf-32",              0), // "Unicode (UTF-32)"
    MapCodePageDataItem( 12001,  1200, "utf-32BE",            0), // "Unicode (UTF-32 Big Endian)"
/*
    MapCodePageDataItem( 20000,   950, "x-Chinese-CNS",       0), // "Chinese Traditional (CNS)"
    MapCodePageDataItem( 20001,   950, "x-cp20001",           0), // "TCA Taiwan"
    MapCodePageDataItem( 20002,   950, "x-Chinese-Eten",      0), // "Chinese Traditional (Eten)"
    MapCodePageDataItem( 20003,   950, "x-cp20003",           0), // "IBM5550 Taiwan"
    MapCodePageDataItem( 20004,   950, "x-cp20004",           0), // "TeleText Taiwan"
    MapCodePageDataItem( 20005,   950, "x-cp20005",           0), // "Wang Taiwan"
    MapCodePageDataItem( 20105,  1252, "x-IA5",               0), // "Western European (IA5)"
    MapCodePageDataItem( 20106,  1252, "x-IA5-German",        0), // "German (IA5)"
    MapCodePageDataItem( 20107,  1252, "x-IA5-Swedish",       0), // "Swedish (IA5)"
    MapCodePageDataItem( 20108,  1252, "x-IA5-Norwegian",     0), // "Norwegian (IA5)"
*/
    MapCodePageDataItem( 20127,  1252, "us-ascii",            MIMECONTF_MAILNEWS | MIMECONTF_SAVABLE_MAILNEWS), // "US-ASCII"
//    MapCodePageDataItem( 20261,  1252, "x-cp20261",           0), // "T.61"
//    MapCodePageDataItem( 20269,  1252, "x-cp20269",           0), // "ISO-6937"
    MapCodePageDataItem( 20273,  1252, "IBM273",              0), // "IBM EBCDIC (Germany)"
    MapCodePageDataItem( 20277,  1252, "IBM277",              0), // "IBM EBCDIC (Denmark-Norway)"
    MapCodePageDataItem( 20278,  1252, "IBM278",              0), // "IBM EBCDIC (Finland-Sweden)"
    MapCodePageDataItem( 20280,  1252, "IBM280",              0), // "IBM EBCDIC (Italy)"
    MapCodePageDataItem( 20284,  1252, "IBM284",              0), // "IBM EBCDIC (Spain)"
    MapCodePageDataItem( 20285,  1252, "IBM285",              0), // "IBM EBCDIC (UK)"
    MapCodePageDataItem( 20290,   932, "IBM290",              0), // "IBM EBCDIC (Japanese katakana)"
    MapCodePageDataItem( 20297,  1252, "IBM297",              0), // "IBM EBCDIC (France)"
    MapCodePageDataItem( 20420,  1256, "IBM420",              0), // "IBM EBCDIC (Arabic)"
//    MapCodePageDataItem( 20423,  1253, "IBM423",              0), // "IBM EBCDIC (Greek)"
    MapCodePageDataItem( 20424,  1255, "IBM424",              0), // "IBM EBCDIC (Hebrew)"
//    MapCodePageDataItem( 20833,   949, "x-EBCDIC-KoreanExtended", 0), // "IBM EBCDIC (Korean Extended)"
//    MapCodePageDataItem( 20838,   874, "IBM-Thai",            0), // "IBM EBCDIC (Thai)"
    MapCodePageDataItem( 20866,  1251, "koi8-r",              MIMECONTF_MAILNEWS | MIMECONTF_BROWSER | MIMECONTF_SAVABLE_MAILNEWS | MIMECONTF_SAVABLE_BROWSER), // "Cyrillic (KOI8-R)"
    MapCodePageDataItem( 20871,  1252, "IBM871",              0), // "IBM EBCDIC (Icelandic)"
/*
    MapCodePageDataItem( 20880,  1251, "IBM880",              0), // "IBM EBCDIC (Cyrillic Russian)"
    MapCodePageDataItem( 20905,  1254, "IBM905",              0), // "IBM EBCDIC (Turkish)"
    MapCodePageDataItem( 20924,  1252, "IBM00924",            0), // "IBM Latin-1"
    MapCodePageDataItem( 20932,   932, "EUC-JP",              0), // "Japanese (JIS 0208-1990 and 0212-1990)"
    MapCodePageDataItem( 20936,   936, "x-cp20936",           0), // "Chinese Simplified (GB2312-80)"
    MapCodePageDataItem( 20949,   949, "x-cp20949",           0), // "Korean Wansung"
*/
    MapCodePageDataItem( 21025,  1251, "cp1025",              0), // "IBM EBCDIC (Cyrillic Serbian-Bulgarian)"
    MapCodePageDataItem( 21866,  1251, "koi8-u",              MIMECONTF_MAILNEWS | MIMECONTF_BROWSER | MIMECONTF_SAVABLE_MAILNEWS | MIMECONTF_SAVABLE_BROWSER), // "Cyrillic (KOI8-U)"
    MapCodePageDataItem( 28591,  1252, "iso-8859-1",          MIMECONTF_MAILNEWS | MIMECONTF_BROWSER | MIMECONTF_SAVABLE_MAILNEWS | MIMECONTF_SAVABLE_BROWSER), // "Western European (ISO)"
    MapCodePageDataItem( 28592,  1250, "iso-8859-2",          MIMECONTF_MAILNEWS | MIMECONTF_BROWSER | MIMECONTF_SAVABLE_MAILNEWS | MIMECONTF_SAVABLE_BROWSER), // "Central European (ISO)"
    MapCodePageDataItem( 28593,  1254, "iso-8859-3",          MIMECONTF_MAILNEWS | MIMECONTF_SAVABLE_MAILNEWS), // "Latin 3 (ISO)"
    MapCodePageDataItem( 28594,  1257, "iso-8859-4",          MIMECONTF_MAILNEWS | MIMECONTF_BROWSER | MIMECONTF_SAVABLE_MAILNEWS | MIMECONTF_SAVABLE_BROWSER), // "Baltic (ISO)"
    MapCodePageDataItem( 28595,  1251, "iso-8859-5",          MIMECONTF_MAILNEWS | MIMECONTF_BROWSER | MIMECONTF_SAVABLE_MAILNEWS | MIMECONTF_SAVABLE_BROWSER), // "Cyrillic (ISO)"
    MapCodePageDataItem( 28596,  1256, "iso-8859-6",          MIMECONTF_MAILNEWS | MIMECONTF_BROWSER | MIMECONTF_SAVABLE_MAILNEWS | MIMECONTF_SAVABLE_BROWSER), // "Arabic (ISO)"
    MapCodePageDataItem( 28597,  1253, "iso-8859-7",          MIMECONTF_MAILNEWS | MIMECONTF_BROWSER | MIMECONTF_SAVABLE_MAILNEWS | MIMECONTF_SAVABLE_BROWSER), // "Greek (ISO)"
    MapCodePageDataItem( 28598,  1255, "iso-8859-8",          MIMECONTF_BROWSER | MIMECONTF_SAVABLE_BROWSER), // "Hebrew (ISO-Visual)"
    MapCodePageDataItem( 28599,  1254, "iso-8859-9",          MIMECONTF_MAILNEWS | MIMECONTF_BROWSER | MIMECONTF_SAVABLE_MAILNEWS | MIMECONTF_SAVABLE_BROWSER), // "Turkish (ISO)"
//    MapCodePageDataItem( 28603,  1257, "iso-8859-13",         MIMECONTF_MAILNEWS | MIMECONTF_SAVABLE_MAILNEWS), // "Estonian (ISO)"
    MapCodePageDataItem( 28605,  1252, "iso-8859-15",         MIMECONTF_MAILNEWS | MIMECONTF_SAVABLE_MAILNEWS | MIMECONTF_SAVABLE_BROWSER), // "Latin 9 (ISO)"
//    MapCodePageDataItem( 29001,  1252, "x-Europa",            0), // "Europa"
    MapCodePageDataItem( 38598,  1255, "iso-8859-8-i",        MIMECONTF_MAILNEWS | MIMECONTF_BROWSER | MIMECONTF_SAVABLE_MAILNEWS | MIMECONTF_SAVABLE_BROWSER), // "Hebrew (ISO-Logical)"
    MapCodePageDataItem( 50220,   932, "iso-2022-jp",         MIMECONTF_MAILNEWS | MIMECONTF_SAVABLE_MAILNEWS), // "Japanese (JIS)"
    MapCodePageDataItem( 50221,   932, "|csISO2022JP|iso-2022-jp|iso-2022-jp", MIMECONTF_MAILNEWS | MIMECONTF_SAVABLE_MAILNEWS | MIMECONTF_SAVABLE_BROWSER), // "Japanese (JIS-Allow 1 byte Kana)"
    MapCodePageDataItem( 50222,   932, "iso-2022-jp",         0), // "Japanese (JIS-Allow 1 byte Kana - SO/SI)"
//    MapCodePageDataItem( 50225,   949, "|iso-2022-kr|euc-kr|iso-2022-kr", MIMECONTF_MAILNEWS), // "Korean (ISO)"
//    MapCodePageDataItem( 50227,   936, "x-cp50227",           0), // "Chinese Simplified (ISO-2022)"
//MapCodePageDataItem( 50229,   950, L"x-cp50229", L"x-cp50229", L"x-cp50229", 0}, // "Chinese Traditional (ISO-2022)"
//MapCodePageDataItem( 50930,   932, L"cp930", L"cp930", L"cp930", 0}, // "IBM EBCDIC (Japanese and Japanese Katakana)"
//MapCodePageDataItem( 50931,   932, L"x-EBCDIC-JapaneseAndUSCanada", L"x-EBCDIC-JapaneseAndUSCanada", L"x-EBCDIC-JapaneseAndUSCanada", 0}, // "IBM EBCDIC (Japanese and US-Canada)"
//MapCodePageDataItem( 50933,   949, L"cp933", L"cp933", L"cp933", 0}, // "IBM EBCDIC (Korean and Korean Extended)"
//MapCodePageDataItem( 50935,   936, L"cp935", L"cp935", L"cp935", 0}, // "IBM EBCDIC (Simplified Chinese)"
//MapCodePageDataItem( 50937,   950, L"cp937", L"cp937", L"cp937", 0}, // "IBM EBCDIC (Traditional Chinese)"
//MapCodePageDataItem( 50939,   932, L"cp939", L"cp939", L"cp939", 0}, // "IBM EBCDIC (Japanese and Japanese-Latin)"
    MapCodePageDataItem( 51932,   932, "euc-jp",              MIMECONTF_MAILNEWS | MIMECONTF_BROWSER | MIMECONTF_SAVABLE_MAILNEWS | MIMECONTF_SAVABLE_BROWSER), // "Japanese (EUC)"
//    MapCodePageDataItem( 51936,   936, "EUC-CN",              0), // "Chinese Simplified (EUC)"
    MapCodePageDataItem( 51949,   949, "euc-kr",              MIMECONTF_MAILNEWS | MIMECONTF_SAVABLE_MAILNEWS), // "Korean (EUC)"
//    MapCodePageDataItem( 52936,   936, "hz-gb-2312",          MIMECONTF_MAILNEWS | MIMECONTF_BROWSER | MIMECONTF_SAVABLE_MAILNEWS | MIMECONTF_SAVABLE_BROWSER), // "Chinese Simplified (HZ)"
    MapCodePageDataItem( 54936,   936, "GB18030",             MIMECONTF_MAILNEWS | MIMECONTF_BROWSER | MIMECONTF_SAVABLE_MAILNEWS | MIMECONTF_SAVABLE_BROWSER), // "Chinese Simplified (GB18030)"
    MapCodePageDataItem( 57002, 57002, "x-iscii-de",          0), // "ISCII Devanagari"
    MapCodePageDataItem( 57003, 57003, "x-iscii-be",          0), // "ISCII Bengali"
    MapCodePageDataItem( 57004, 57004, "x-iscii-ta",          0), // "ISCII Tamil"
    MapCodePageDataItem( 57005, 57005, "x-iscii-te",          0), // "ISCII Telugu"
    MapCodePageDataItem( 57006, 57006, "x-iscii-as",          0), // "ISCII Assamese"
    MapCodePageDataItem( 57007, 57007, "x-iscii-or",          0), // "ISCII Oriya"
    MapCodePageDataItem( 57008, 57008, "x-iscii-ka",          0), // "ISCII Kannada"
    MapCodePageDataItem( 57009, 57009, "x-iscii-ma",          0), // "ISCII Malayalam"
    MapCodePageDataItem( 57010, 57010, "x-iscii-gu",          0), // "ISCII Gujarati"
    MapCodePageDataItem( 57011, 57011, "x-iscii-pa",          0), // "ISCII Punjabi"
    MapCodePageDataItem( 65000,  1200, "utf-7",               MIMECONTF_MAILNEWS | MIMECONTF_SAVABLE_MAILNEWS), // "Unicode (UTF-7)"
    MapCodePageDataItem( 65001,  1200, "utf-8",               MIMECONTF_MAILNEWS | MIMECONTF_BROWSER | MIMECONTF_SAVABLE_MAILNEWS | MIMECONTF_SAVABLE_BROWSER), // "Unicode (UTF-8)"
#endif // FEATURE_CORECLR

    // End of data.
    MapCodePageDataItem( 0, 0, null, 0),

};

#endregion

#region "from coreclr/src/pal/inc/rt/palrt.h"
// modified

    const int
//typedef 
//enum tagMIMECONTF {
    MIMECONTF_MAILNEWS  = 0x1,
    MIMECONTF_BROWSER   = 0x2,
    MIMECONTF_MINIMAL   = 0x4,
    MIMECONTF_IMPORT    = 0x8,
    MIMECONTF_SAVABLE_MAILNEWS  = 0x100,
    MIMECONTF_SAVABLE_BROWSER   = 0x200,
    MIMECONTF_EXPORT    = 0x400,
    MIMECONTF_PRIVCONVERTER = 0x10000,
    MIMECONTF_VALID = 0x20000,
    MIMECONTF_VALID_NLS = 0x40000,
    MIMECONTF_MIME_IE4  = 0x10000000,
    MIMECONTF_MIME_LATEST   = 0x20000000,
    MIMECONTF_MIME_REGISTRY = 0x40000000
//    }   MIMECONTF;
    ;
#endregion
}

#region "from referencesource/mscorlib/system/globalization/encodingtable.cs"
    //
    // Data table for encoding classes.  Used by System.Text.Encoding.
    // This class contains two hashtables to allow System.Text.Encoding
    // to retrieve the data item either by codepage value or by webName.
    //
    
    // Only statics, does not need to be marked with the serializable attribute
    internal static partial class EncodingTable
    {

        //This number is the size of the table in native.  The value is retrieved by
        //calling the native GetNumEncodingItems().
        private static int lastEncodingItem = GetNumEncodingItems() - 1;

        //This number is the size of the code page table.  Its generated when we walk the table the first time.
        private static volatile int lastCodePageItem;

/*        
        //
        // This points to a native data table which maps an encoding name to the correct code page.        
        //
        [SecurityCritical]
        unsafe internal static InternalEncodingDataItem* encodingDataPtr = GetEncodingData();
        //
        // This points to a native data table which stores the properties for the code page, and
        // the table is indexed by code page.
        //
        [SecurityCritical]
        unsafe internal static InternalCodePageDataItem* codePageDataPtr = GetCodePageData();
*/

        //
        // This caches the mapping of an encoding name to a code page.
        //
        private static Dictionary<string, int> hashByName = new Dictionary<string, int> (StringComparer.OrdinalIgnoreCase);
        //
        // THe caches the data item which is indexed by the code page value.
        //
        private static Dictionary<int, CodePageDataItem> hashByCodePage = new Dictionary<int, CodePageDataItem> ();

        [System.Security.SecuritySafeCritical] // static constructors should be safe to call
        static EncodingTable()
        { 
        }

        // Find the data item by binary searching the table that we have in native.
        // nativeCompareOrdinalWC is an internal-only function.
        [System.Security.SecuritySafeCritical]  // auto-generated
        unsafe private static int internalGetCodePageFromName(String name) {
            int left  = 0;
            int right = lastEncodingItem;
            int index;
            int result;
    
            //Binary search the array until we have only a couple of elements left and then
            //just walk those elements.
            while ((right - left)>3) {
                index = ((right - left)/2) + left;
                
                result = String.Compare (name, encodingDataPtr[index].webName, StringComparison.OrdinalIgnoreCase);
    
                if (result == 0) {
                    //We found the item, return the associated codepage.
                    return (encodingDataPtr[index].codePage);
                } else if (result<0) {
                    //The name that we're looking for is less than our current index.
                    right = index;
                } else {
                    //The name that we're looking for is greater than our current index
                    left = index;
                }
            }
    
            //Walk the remaining elements (it'll be 3 or fewer).
            for (; left<=right; left++) {
                if (String.Compare(name, encodingDataPtr[left].webName, StringComparison.OrdinalIgnoreCase) == 0) {
                    return (encodingDataPtr[left].codePage);
                }
            }
            // The encoding name is not valid.
            throw new ArgumentException(
                String.Format(
                    CultureInfo.CurrentCulture,
                    Environment.GetResourceString("Argument_EncodingNotSupported"), name), "name");
        }

        // Return a list of all EncodingInfo objects describing all of our encodings
        [System.Security.SecuritySafeCritical]  // auto-generated
        internal static unsafe EncodingInfo[] GetEncodings()
        {
            if (lastCodePageItem == 0)
            {
                int count;
                for (count = 0; codePageDataPtr[count].codePage != 0; count++)
                {
                    // Count them
                }
                lastCodePageItem = count;
            }

            EncodingInfo[] arrayEncodingInfo = new EncodingInfo[lastCodePageItem];

            int i;
            for (i = 0; i < lastCodePageItem; i++)
            {
                arrayEncodingInfo[i] = new EncodingInfo(codePageDataPtr[i].codePage, CodePageDataItem.CreateString(codePageDataPtr[i].Names, 0),
                    Environment.GetResourceString("Globalization.cp_" + codePageDataPtr[i].codePage));
            }
            
            return arrayEncodingInfo;
        }        
    
        /*=================================GetCodePageFromName==========================
        **Action: Given a encoding name, return the correct code page number for this encoding.
        **Returns: The code page for the encoding.
        **Arguments:
        **  name    the name of the encoding
        **Exceptions:
        **  ArgumentNullException if name is null.
        **  internalGetCodePageFromName will throw ArgumentException if name is not a valid encoding name.
        ============================================================================*/
        
        internal static int GetCodePageFromName(String name)
        {   
            if (name==null) {
                throw new ArgumentNullException("name");
            }
            Contract.EndContractBlock();

            //
            // The name is case-insensitive, but ToLower isn't free.  Check for
            // the code page in the given capitalization first.
            //
           ICollection ic = hashByName;
           lock (ic.SyncRoot) {
                int codePage;

                if (hashByName.TryGetValue (name, out codePage))
                    return codePage;

                //Okay, we didn't find it in the hash table, try looking it up in the
                //unmanaged data.
                codePage = internalGetCodePageFromName(name);

                hashByName[name] = codePage;

                return codePage;
            }
        }
    
        [System.Security.SecuritySafeCritical]  // auto-generated
        unsafe internal static CodePageDataItem GetCodePageDataItem(int codepage) {
            CodePageDataItem dataItem;

            // We synchronize around dictionary gets/sets. There's still a possibility that two threads
            // will create a CodePageDataItem and the second will clobber the first in the dictionary. 
            // However, that's acceptable because the contents are correct and we make no guarantees
            // other than that. 

            //Look up the item in the hashtable.
            ICollection ic = hashByCodePage;
            lock (ic.SyncRoot) {
                if (hashByCodePage.TryGetValue (codepage, out dataItem))
                    return dataItem;

                //If we didn't find it, try looking it up now.
                //If we find it, add it to the hashtable.
                //This is a linear search, but we probably won't be doing it very often.
                //
                int i = 0;
                int data;
                while ((data = codePageDataPtr[i].codePage) != 0) {
                    if (data == codepage) {
                        dataItem = new CodePageDataItem(i);
                        hashByCodePage[codepage] = dataItem;
                        return (dataItem);
                    }
                    i++;
                }
            }

            //Nope, we didn't find it.
            return null;
        }

/*
        [System.Security.SecurityCritical]  // auto-generated
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private unsafe static extern InternalEncodingDataItem *GetEncodingData();
        
        //
        // Return the number of encoding data items.
        //
        [System.Security.SecurityCritical]  // auto-generated
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern int GetNumEncodingItems();

        [System.Security.SecurityCritical]  // auto-generated
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private unsafe static extern InternalCodePageDataItem* GetCodePageData();

        [System.Security.SecurityCritical]  // auto-generated
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal unsafe static extern byte* nativeCreateOpenFileMapping(
            String inSectionName, int inBytesToAllocate, out IntPtr mappedFileHandle);   
*/
    }
    
    /*=================================InternalEncodingDataItem==========================
    **Action: This is used to map a encoding name to a correct code page number. By doing this,
    ** we can get the properties of this encoding via the InternalCodePageDataItem.
    **
    ** We use this structure to access native data exposed by the native side.
    ============================================================================*/
    
    [System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
    internal struct InternalEncodingDataItem {
        [SecurityCritical]
        internal string webName;
        internal UInt16   codePage;
    }

    /*=================================InternalCodePageDataItem==========================
    **Action: This is used to access the properties related to a code page.
    ** We use this structure to access native data exposed by the native side.
    ============================================================================*/

    [System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
    internal struct InternalCodePageDataItem {
        internal UInt16   codePage;
        internal UInt16   uiFamilyCodePage;
        internal uint     flags;
        [SecurityCritical]
        internal string Names;
    }
#endregion
}
