/*
 * Handlers.cs - Implementation of the "I18N.Common.Handlers" class.
 *
 * Copyright (c) 2002  Southern Storm Software, Pty Ltd
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included
 * in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
 * OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR
 * OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
 * ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 */

namespace I18N.Common
{

using System;
using System.Collections.Generic;

// This class provides an internal list of handlers, for runtime
// engines that do not implement the altered "GetFile" semantics.
// The list must be kept up to date manually.

public sealed class Handlers
{
    public static readonly String[] List = {
        "I18N.CJK.CP932",
        "I18N.CJK.CP936",
        "I18N.CJK.CP949",
        "I18N.CJK.CP950",
        "I18N.CJK.CP50220",
        "I18N.CJK.CP50221",
        "I18N.CJK.CP50222",
        "I18N.CJK.CP51932",
        "I18N.CJK.CP51949",
        "I18N.CJK.CP54936",
        "I18N.CJK.ENCbig5",
        "I18N.CJK.ENCgb2312",
        "I18N.CJK.ENCshift_jis",
        "I18N.CJK.ENCiso_2022_jp",
        "I18N.CJK.ENCeuc_jp",
        "I18N.CJK.ENCeuc_kr",
        "I18N.CJK.ENCuhc",
        "I18N.CJK.ENCgb18030",
        "I18N.MidEast.CP1254",
        "I18N.MidEast.ENCwindows_1254",
        "I18N.MidEast.CP1255",
        "I18N.MidEast.ENCwindows_1255",
        "I18N.MidEast.CP1256",
        "I18N.MidEast.ENCwindows_1256",
        "I18N.MidEast.CP28596",
        "I18N.MidEast.ENCiso_8859_6",
        "I18N.MidEast.CP28598",
        "I18N.MidEast.ENCiso_8859_8",
        "I18N.MidEast.CP28599",
        "I18N.MidEast.ENCiso_8859_9",
        "I18N.MidEast.CP38598",
        "I18N.MidEast.ENCwindows_38598",
        "I18N.Other.CP1251",
        "I18N.Other.ENCwindows_1251",
        "I18N.Other.CP1257",
        "I18N.Other.ENCwindows_1257",
        "I18N.Other.CP1258",
        "I18N.Other.ENCwindows_1258",
        "I18N.Other.CP20866",
        "I18N.Other.ENCkoi8_r",
        "I18N.Other.CP21866",
        "I18N.Other.ENCkoi8_u",
        "I18N.Other.CP28594",
        "I18N.Other.ENCiso_8859_4",
        "I18N.Other.CP28595",
        "I18N.Other.ENCiso_8859_5",
        "I18N.Other.ISCIIEncoding",
        "I18N.Other.CP57002",
        "I18N.Other.CP57003",
        "I18N.Other.CP57004",
        "I18N.Other.CP57005",
        "I18N.Other.CP57006",
        "I18N.Other.CP57007",
        "I18N.Other.CP57008",
        "I18N.Other.CP57009",
        "I18N.Other.CP57010",
        "I18N.Other.CP57011",
        "I18N.Other.ENCx_iscii_de",
        "I18N.Other.ENCx_iscii_be",
        "I18N.Other.ENCx_iscii_ta",
        "I18N.Other.ENCx_iscii_te",
        "I18N.Other.ENCx_iscii_as",
        "I18N.Other.ENCx_iscii_or",
        "I18N.Other.ENCx_iscii_ka",
        "I18N.Other.ENCx_iscii_ma",
        "I18N.Other.ENCx_iscii_gu",
        "I18N.Other.ENCx_iscii_pa",
        "I18N.Other.CP874",
        "I18N.Other.ENCwindows_874",
        "I18N.Rare.CP1026",
        "I18N.Rare.ENCibm1026",
        "I18N.Rare.CP1047",
        "I18N.Rare.ENCibm1047",
        "I18N.Rare.CP1140",
        "I18N.Rare.ENCibm01140",
        "I18N.Rare.CP1141",
        "I18N.Rare.ENCibm01141",
        "I18N.Rare.CP1142",
        "I18N.Rare.ENCibm01142",
        "I18N.Rare.CP1143",
        "I18N.Rare.ENCibm01143",
        "I18N.Rare.CP1144",
        "I18N.Rare.ENCibm1144",
        "I18N.Rare.CP1145",
        "I18N.Rare.ENCibm1145",
        "I18N.Rare.CP1146",
        "I18N.Rare.ENCibm1146",
        "I18N.Rare.CP1147",
        "I18N.Rare.ENCibm1147",
        "I18N.Rare.CP1148",
        "I18N.Rare.ENCibm1148",
        "I18N.Rare.CP1149",
        "I18N.Rare.ENCibm1149",
        "I18N.Rare.CP20273",
        "I18N.Rare.ENCibm273",
        "I18N.Rare.CP20277",
        "I18N.Rare.ENCibm277",
        "I18N.Rare.CP20278",
        "I18N.Rare.ENCibm278",
        "I18N.Rare.CP20280",
        "I18N.Rare.ENCibm280",
        "I18N.Rare.CP20284",
        "I18N.Rare.ENCibm284",
        "I18N.Rare.CP20285",
        "I18N.Rare.ENCibm285",
        "I18N.Rare.CP20290",
        "I18N.Rare.ENCibm290",
        "I18N.Rare.CP20297",
        "I18N.Rare.ENCibm297",
        "I18N.Rare.CP20420",
        "I18N.Rare.ENCibm420",
        "I18N.Rare.CP20424",
        "I18N.Rare.ENCibm424",
        "I18N.Rare.CP20871",
        "I18N.Rare.ENCibm871",
        "I18N.Rare.CP21025",
        "I18N.Rare.ENCibm1025",
        "I18N.Rare.CP37",
        "I18N.Rare.ENCibm037",
        "I18N.Rare.CP500",
        "I18N.Rare.ENCibm500",
        "I18N.Rare.CP708",
        "I18N.Rare.ENCasmo_708",
        "I18N.Rare.CP852",
        "I18N.Rare.ENCibm852",
        "I18N.Rare.CP855",
        "I18N.Rare.ENCibm855",
        "I18N.Rare.CP857",
        "I18N.Rare.ENCibm857",
        "I18N.Rare.CP858",
        "I18N.Rare.ENCibm00858",
        "I18N.Rare.CP862",
        "I18N.Rare.ENCibm862",
        "I18N.Rare.CP864",
        "I18N.Rare.ENCibm864",
        "I18N.Rare.CP866",
        "I18N.Rare.ENCibm866",
        "I18N.Rare.CP869",
        "I18N.Rare.ENCibm869",
        "I18N.Rare.CP870",
        "I18N.Rare.ENCibm870",
        "I18N.Rare.CP875",
        "I18N.Rare.ENCibm875",
        "I18N.West.CP10000",
        "I18N.West.ENCmacintosh",
        "I18N.West.CP10079",
        "I18N.West.ENCx_mac_icelandic",
        "I18N.West.CP1250",
        "I18N.West.ENCwindows_1250",
        "I18N.West.CP1252",
        "I18N.West.ENCwindows_1252",
        "I18N.West.CP1253",
        "I18N.West.ENCwindows_1253",
        "I18N.West.CP28592",
        "I18N.West.ENCiso_8859_2",
        "I18N.West.CP28593",
        "I18N.West.ENCiso_8859_3",
        "I18N.West.CP28597",
        "I18N.West.ENCiso_8859_7",
        "I18N.West.CP28605",
        "I18N.West.ENCiso_8859_15",
        "I18N.West.CP437",
        "I18N.West.ENCibm437",
        "I18N.West.CP850",
        "I18N.West.ENCibm850",
        "I18N.West.CP860",
        "I18N.West.ENCibm860",
        "I18N.West.CP861",
        "I18N.West.ENCibm861",
        "I18N.West.CP863",
        "I18N.West.ENCibm863",
        "I18N.West.CP865",
        "I18N.West.ENCibm865"
    };
	
	static Dictionary<string, string> aliases;
	public static string GetAlias (string name)
	{
		if (aliases == null)
			BuildHash ();

		string v;
		aliases.TryGetValue (name, out v);
		return v;
	}

	static void BuildHash ()
	{
		aliases = new Dictionary<string, string> (StringComparer.OrdinalIgnoreCase);

		aliases.Add ("arabic", "iso_8859_6");
		aliases.Add ("csISOLatinArabic", "iso_8859_6");
		aliases.Add ("ECMA_114", "iso_8859_6");
		aliases.Add ("ISO_8859_6:1987", "iso_8859_6");
		aliases.Add ("iso_ir_127", "iso_8859_6");

		aliases.Add ("cp1256" ,"windows_1256");

		aliases.Add ("csISOLatin4", "iso_8859_4");
		aliases.Add ("ISO_8859_4:1988", "iso_8859_4");
		aliases.Add ("iso_ir_110", "iso_8859_4");
		aliases.Add ("l4", "iso_8859_4");
		aliases.Add ("latin4", "iso_8859_4");

		aliases.Add ("cp852" ,"ibm852");

		aliases.Add ("csISOLatin2", "iso_8859_2");
		aliases.Add ("iso_8859_2:1987", "iso_8859_2");
		aliases.Add ("iso8859_2", "iso_8859_2");
		aliases.Add ("iso_ir_101", "iso_8859_2");
		aliases.Add ("l2", "iso_8859_2");
		aliases.Add ("latin2", "iso_8859_2");

		aliases.Add ("x-cp1250", "windows_1250");

		aliases.Add ("chinese", "gb2312");
		aliases.Add ("CN-GB", "gb2312");
		aliases.Add ("csGB2312", "gb2312");
		aliases.Add ("csGB231280", "gb2312");
		aliases.Add ("csISO58GB231280", "gb2312");
		aliases.Add ("GB_2312_80", "gb2312");
		aliases.Add ("GB231280", "gb2312");
		aliases.Add ("GB2312_80", "gb2312");
		aliases.Add ("GBK", "gb2312");
		aliases.Add ("iso_ir_58", "gb2312");

		aliases.Add ("cn-big5", "big5");
		aliases.Add ("csbig5", "big5");
		aliases.Add ("x-x-big5", "big5");

		aliases.Add ("cp866", "ibm866");

		aliases.Add ("csISOLatin5", "iso_8859_5");
		aliases.Add ("csISOLatinCyrillic", "iso_8859_5");
		aliases.Add ("cyrillic", "iso_8859_5");
		aliases.Add ("ISO_8859_5:1988", "iso_8859_5");
		aliases.Add ("iso_ir_144", "iso_8859_5");
		aliases.Add ("l5", "iso_8859_5");

		aliases.Add ("csKOI8R", "koi8_r");
		aliases.Add ("koi", "koi8_r");
		aliases.Add ("koi8", "koi8_r");
		aliases.Add ("koi8r", "koi8_r");

		aliases.Add ("koi8ru", "koi8_u");

		aliases.Add ("x-cp1251", "windows_1251");

		aliases.Add ("csISOLatinGreek", "iso_8859_7");
		aliases.Add ("ECMA_118", "iso_8859_7");
		aliases.Add ("ELOT_928", "iso_8859_7");
		aliases.Add ("greek", "iso_8859_7");
		aliases.Add ("greek8", "iso_8859_7");
		aliases.Add ("ISO_8859_7:1987", "iso_8859_7");
		aliases.Add ("iso_ir_126", "iso_8859_7");

		aliases.Add ("csISOLatinHebrew", "iso_8859_8");
		aliases.Add ("hebrew", "iso_8859_8");
		aliases.Add ("ISO_8859_8:1988", "iso_8859_8");
		aliases.Add ("iso_ir_138", "iso_8859_8");

		aliases.Add ("csShiftJIS", "shift_jis");
		aliases.Add ("csWindows31J", "shift_jis");
		aliases.Add ("ms_Kanji", "shift_jis");
		aliases.Add ("shift-jis", "shift_jis");
		aliases.Add ("x-ms-cp932", "shift_jis");
		aliases.Add ("x-sjis", "shift_jis");

		aliases.Add ("csISOLatin3", "iso_8859_3");
		aliases.Add ("ISO_8859_3:1988", "iso_8859_3");
		aliases.Add ("iso_ir_109", "iso_8859_3");
		aliases.Add ("l3", "iso_8859_3");
		aliases.Add ("latin3", "iso_8859_3");

		aliases.Add ("csISOLatin9", "iso_8859_15");
		aliases.Add ("l9", "iso_8859_15");
		aliases.Add ("latin9", "iso_8859_15");

		aliases.Add ("cp437", "ibm437");
		aliases.Add ("csPC8", "ibm437");
		aliases.Add ("CodePage437", "ibm437");

		aliases.Add ("DOS_874", "windows_874");
		aliases.Add ("iso_8859_11", "windows_874");
		aliases.Add ("TIS_620", "windows_874");
	}

	
}; // class Handlers

}; // namespace I18N.Common
