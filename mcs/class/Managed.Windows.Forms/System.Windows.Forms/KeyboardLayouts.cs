// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Jackson Harper (jackson@ximian.com)
//
//

// TODO:
// - Move these tables into unmanaged code (libgdiplus) and access with a pointer
//


using System;

namespace System.Windows.Forms {

	internal class KeyboardLayout {
		public string Comment;
		public int CodePage;
		public string [] Key;
		public short [] Scan;
		public VirtualKeys [] VKey;

		public KeyboardLayout (string comment, int code_page, string [] key, short [] scan, VirtualKeys [] vkey)
		{
			Comment = comment;
			CodePage = code_page;
			Key = key;
			Scan = scan;
			VKey = vkey;
		}
	}
	
	internal class KeyboardLayouts {

		public static readonly int MainLen = 48;
		private static readonly string [] main_key_US = new string []
		{
			"`~","1!","2@","3#","4$","5%","6^","7&","8*","9(","0)","-_","=+",
			"qQ","wW","eE","rR","tT","yY","uU","iI","oO","pP","[{","]}",
			"aA","sS","dD","fF","gG","hH","jJ","kK","lL",";:","'\"","\\|",
			"zZ","xX","cC","vV","bB","nN","mM",",<",".>","/?"
		};

		private static string [] main_key_US_phantom = new string []
		{
			"`~","1!","2@","3#","4$","5%","6^","7&","8*","9(","0)","-_","=+",
			"qQ","wW","eE","rR","tT","yY","uU","iI","oO","pP","[{","]}",
			"aA","sS","dD","fF","gG","hH","jJ","kK","lL",";:","'\"","\\|",
			"zZ","xX","cC","vV","bB","nN","mM",",<",".>","/?",
			"<>" /* the phantom key */
		};

		/*** United States keyboard layout (dvorak version) */
		private static readonly string [] main_key_US_dvorak = new string []
		{
			"`~","1!","2@","3#","4$","5%","6^","7&","8*","9(","0)","[{","]}",
			"'\"",",<",".>","pP","yY","fF","gG","cC","rR","lL","/?","=+",
			"aA","oO","eE","uU","iI","dD","hH","tT","nN","sS","-_","\\|",
			";:","qQ","jJ","kK","xX","bB","mM","wW","vV","zZ"
		};

		/*** British keyboard layout */
		private static readonly string [] main_key_UK = new string []
		{
			"`","1!","2\"","3£","4$","5%","6^","7&","8*","9(","0)","-_","=+",
			"qQ","wW","eE","rR","tT","yY","uU","iI","oO","pP","[{","]}",
			"aA","sS","dD","fF","gG","hH","jJ","kK","lL",";:","'@","#~",
			"zZ","xX","cC","vV","bB","nN","mM",",<",".>","/?",
			"\\|"
		};

		/*** French keyboard layout (contributed by Eric Pouech) */
		private static readonly string [] main_key_FR = new string []
		{
			"²","&1","é2~","\"3#","'4{","(5[","-6|","è7","_8\\","ç9^±","à0@",")°]","=+}",
			"aA","zZ","eE","rR","tT","yY","uU","iI","oO","pP","^¨","$£¤",
			"qQ","sSß","dD","fF","gG","hH","jJ","kK","lL","mM","ù%","*µ",
			"wW","xX","cC","vV","bB","nN",",?",";.",":/","!§",
			"<>"
		};

		/*** Icelandic keyboard layout (contributed by Ríkharður Egilsson) */
		private static readonly string [] main_key_IS = new string []
		{
			"°","1!","2\"","3#","4$","5%","6&","7/{","8([","9)]","0=}","öÖ\\","-_",
			"qQ@","wW","eE","rR","tT","yY","uU","iI","oO","pP","ðÐ","'?~",
			"aA","sS","dD","fF","gG","hH","jJ","kK","lL","æÆ","´^","+*`",
			"zZ","xX","cC","vV","bB","nN","mM",",;",".:","þÞ",
			"<>|"
		};

		/*** German keyboard layout (contributed by Ulrich Weigand) */
		private static readonly string [] main_key_DE = new string []
		{
			"^°","1!","2\"²","3§³","4$","5%","6&","7/{","8([","9)]","0=}","ß?\\","'`",
			"qQ@","wW","eE","rR","tT","zZ","uU","iI","oO","pP","üÜ","+*~",
			"aA","sS","dD","fF","gG","hH","jJ","kK","lL","öÖ","äÄ","#´",
			"yY","xX","cC","vV","bB","nN","mMµ",",;",".:","-_",
			"<>|"
		};

		/*** German keyboard layout without dead keys */
		private static readonly string [] main_key_DE_nodead = new string []
		{
			"^°","1!","2\"","3§","4$","5%","6&","7/{","8([","9)]","0=}","ß?\\","´",
			"qQ","wW","eE","rR","tT","zZ","uU","iI","oO","pP","üÜ","+*~",
			"aA","sS","dD","fF","gG","hH","jJ","kK","lL","öÖ","äÄ","#'",
			"yY","xX","cC","vV","bB","nN","mM",",;",".:","-_",
			"<>"
		};

		/*** Swiss German keyboard layout (contributed by Jonathan Naylor) */
		private static readonly string [] main_key_SG = new string []
		{
			"§°","1+|","2\"@","3*#","4ç","5%","6&¬","7/¦","8(¢","9)","0=","'?´","^`~",
			"qQ","wW","eE","rR","tT","zZ","uU","iI","oO","pP","üè[","¨!]",
			"aA","sS","dD","fF","gG","hH","jJ","kK","lL","öé","äà{","$£}",
			"yY","xX","cC","vV","bB","nN","mM",",;",".:","-_",
			"<>\\"
		};

		/*** Swiss French keyboard layout (contributed by Philippe Froidevaux) */
		private static readonly string [] main_key_SF = new string []
		{
			"§°","1+|","2\"@","3*#","4ç","5%","6&¬","7/¦","8(¢","9)","0=","'?´","^`~",
			"qQ","wW","eE","rR","tT","zZ","uU","iI","oO","pP","èü[","¨!]",
			"aA","sS","dD","fF","gG","hH","jJ","kK","lL","éö","àä{","$£}",
			"yY","xX","cC","vV","bB","nN","mM",",;",".:","-_",
			"<>\\"
		};

		/*** Norwegian keyboard layout (contributed by Ove Kåven) */
		private static readonly string [] main_key_NO = new string []
		{
			"|§","1!","2\"@","3#£","4¤$","5%","6&","7/{","8([","9)]","0=}","+?","\\`´",
			"qQ","wW","eE","rR","tT","yY","uU","iI","oO","pP","åÅ","¨^~",
			"aA","sS","dD","fF","gG","hH","jJ","kK","lL","øØ","æÆ","'*",
			"zZ","xX","cC","vV","bB","nN","mM",",;",".:","-_",
			"<>"
		};

		/*** Danish keyboard layout (contributed by Bertho Stultiens) */
		private static readonly string [] main_key_DA = new string []
		{
			"½§","1!","2\"@","3#£","4¤$","5%","6&","7/{","8([","9)]","0=}","+?","´`|",
			"qQ","wW","eE","rR","tT","yY","uU","iI","oO","pP","åÅ","¨^~",
			"aA","sS","dD","fF","gG","hH","jJ","kK","lL","æÆ","øØ","'*",
			"zZ","xX","cC","vV","bB","nN","mM",",;",".:","-_",
			"<>\\"
		};

		/*** Swedish keyboard layout (contributed by Peter Bortas) */
		private static readonly string [] main_key_SE = new string []
		{
			"§½","1!","2\"@","3#£","4¤$","5%","6&","7/{","8([","9)]","0=}","+?\\","´`",
			"qQ","wW","eE","rR","tT","yY","uU","iI","oO","pP","åÅ","¨^~",
			"aA","sS","dD","fF","gG","hH","jJ","kK","lL","öÖ","äÄ","'*",
			"zZ","xX","cC","vV","bB","nN","mM",",;",".:","-_",
			"<>|"
		};

		/*** Canadian French keyboard layout */
		private static readonly string [] main_key_CF = new string []
		{
			"#|\\","1!±","2\"@","3/£","4$¢","5%¤","6?¬","7&¦","8*²","9(³","0)¼","-_½","=+¾",
			"qQ","wW","eE","rR","tT","yY","uU","iI","oO§","pP¶","^^[","¸¨]",
			"aA","sS","dD","fF","gG","hH","jJ","kK","lL",";:~","``{","<>}",
			"zZ","xX","cC","vV","bB","nN","mM",",'-",".","éÉ",
			"«»°"
		};

		/*** Portuguese keyboard layout */
		private static readonly string [] main_key_PT = new string []
		{
			"\\¦","1!","2\"@","3#£","4$§","5%","6&","7/{","8([","9)]","0=}","'?","«»",
			"qQ",  "wW","eE",  "rR", "tT", "yY", "uU", "iI", "oO", "pP", "+*\\¨","\\'\\`",
			"aA",  "sS","dD",  "fF", "gG", "hH", "jJ", "kK", "lL", "çÇ", "ºª", "\\~\\^",
			"zZ",  "xX","cC",  "vV", "bB", "nN", "mM", ",;", ".:", "-_",
			"<>"
		};

		/*** Italian keyboard layout */
		private static readonly string [] main_key_IT = new string []
		{
			"\\|","1!¹","2\"²","3£³","4$¼","5%½","6&¾","7/{","8([","9)]","0=}","'?`","ì^~",
			"qQ@","wW","eE","rR","tT","yY","uU","iI","oOø","pPþ","èé[","+*]",
			"aA","sSß","dDð","fF","gG","hH","jJ","kK","lL","òç@","à°#","ù§",
			"zZ","xX","cC","vV","bB","nN","mMµ",",;",".:·","-_",
			"<>|"
		};

		/*** Finnish keyboard layout */
		private static readonly string [] main_key_FI = new string []
		{
			"","1!","2\"@","3#","4$","5%","6&","7/{","8([","9)]","0=}","+?\\","\'`",
			"qQ","wW","eE","rR","tT","yY","uU","iI","oO","pP","","\"^~",
			"aA","sS","dD","fF","gG","hH","jJ","kK","lL","","","'*",
			"zZ","xX","cC","vV","bB","nN","mM",",;",".:","-_",
			"<>|"
		};

		/*** Russian keyboard layout (contributed by Pavel Roskin) */
		private static readonly string [] main_key_RU = new string []
		{
			"`~","1!","2@","3#","4$","5%","6^","7&","8*","9(","0)","-_","=+",
			"qQÊê","wWÃã","eEÕõ","rRËë","tTÅå","yYÎî","uUÇç","iIÛû","oOÝý","pPÚú","[{Èè","]}ßÿ",
			"aAÆæ","sSÙù","dD×÷","fFÁá","gGÐð","hHÒò","jJÏï","kKÌì","lLÄä",";:Öö","'\"Üü","\\|",
			"zZÑñ","xXÞþ","cCÓó","vVÍí","bBÉé","nNÔô","mMØø",",<Ââ",".>Àà","/?"
		};

		/*** Russian keyboard layout (phantom key version) */
		private static readonly string [] main_key_RU_phantom = new string []
		{
			"`~","1!","2@","3#","4$","5%","6^","7&","8*","9(","0)","-_","=+",
			"qQÊê","wWÃã","eEÕõ","rRËë","tTÅå","yYÎî","uUÇç","iIÛû","oOÝý","pPÚú","[{Èè","]}ßÿ",
			"aAÆæ","sSÙù","dD×÷","fFÁá","gGÐð","hHÒò","jJÏï","kKÌì","lLÄä",";:Öö","'\"Üü","\\|",
			"zZÑñ","xXÞþ","cCÓó","vVÍí","bBÉé","nNÔô","mMØø",",<Ââ",".>Àà","/?",
			"<>" /* the phantom key */
		};

		/*** Russian keyboard layout KOI8-R */
		private static readonly string [] main_key_RU_koi8r = new string []
		{
			"()","1!","2\"","3/","4$","5:","6,","7.","8;","9?","0%","-_","=+",
			"Êê","Ãã","Õõ","Ëë","Åå","Îî","Çç","Ûû","Ýý","Úú","Èè","ßÿ",
			"Ææ","Ùù","×÷","Áá","Ðð","Òò","Ïï","Ìì","Ää","Öö","Üü","\\|",
			"Ññ","Þþ","Óó","Íí","Éé","Ôô","Øø","Ââ","Àà","/?",
			"<>" /* the phantom key */
		};

		/*** Ukrainian keyboard layout KOI8-U */
		private static readonly string [] main_key_UA = new string []
		{
			"`~­½","1!1!","2@2\"","3#3'","4$4*","5%5:","6^6,","7&7.","8*8;","9(9(","0)0)","-_-_","=+=+",
			"qQÊê","wWÃã","eEÕõ","rRËë","tTÅå","yYÎî","uUÇç","iIÛû","oOÝý","pPÚú","[{Èè","]}§·",
			"aAÆæ","sS¦¶","dD×÷","fFÁá","gGÐð","hHÒò","jJÏï","kKÌì","lLÄä",";:Öö","'\"¤´","\\|\\|",
			"zZÑñ","xXÞþ","cCÓó","vVÍí","bBÉé","nNÔô","mMØø",",<Ââ",".>Àà","/?/?",
			"<>" /* the phantom key */
		};

		/*** Spanish keyboard layout (contributed by José Marcos López) */
		private static readonly string [] main_key_ES = new string []
		{
			"ºª\\","1!|","2\"@","3·#","4$","5%","6&¬","7/","8(","9)","0=","'?","¡¿",
			"qQ","wW","eE","rR","tT","yY","uU","iI","oO","pP","`^[","+*]",
			"aA","sS","dD","fF","gG","hH","jJ","kK","lL","ñÑ","'¨{","çÇ}",
			"zZ","xX","cC","vV","bB","nN","mM",",;",".:","-_",
			"<>"
		};

		/*** Belgian keyboard layout ***/
		private static readonly string [] main_key_BE = new string []
		{
			"","&1|","é2@","\"3#","'4","(5","§6^","è7","!8","ç9{","à0}",")°","-_",
			"aA","zZ","eE¤","rR","tT","yY","uU","iI","oO","pP","^¨[","$*]",
			"qQ","sSß","dD","fF","gG","hH","jJ","kK","lL","mM","ù%´","µ£`",
			"wW","xX","cC","vV","bB","nN",",?",";.",":/","=+~",
			"<>\\"
		};

		/*** Hungarian keyboard layout (contributed by Zoltán Kovács) */
		private static readonly string [] main_key_HU = new string []
		{
			"0§","1'~","2\"·","3+^","4!¢","5%°","6/²","7=`","8(ÿ","9)´","öÖ½","üÜ¨","óÓ¸",
			"qQ\\","wW|","eE","rR","tT","zZ","uU","iIÍ","oOø","pP","õÕ÷","úÚ×",
			"aA","sSð","dDÐ","fF[","gG]","hH","jJí","kK³","lL£","éÉ$","áÁß","ûÛ¤",
			"yY>","xX#","cC&","vV@","bB{","nN}","mM",",?;",".:·","-_*",
			"íÍ<"
		};

		/*** Polish (programmer's) keyboard layout ***/
		private static readonly string [] main_key_PL = new string []
		{
			"`~","1!","2@","3#","4$","5%","6^","7&§","8*","9(","0)","-_","=+",
			"qQ","wW","eEêÊ","rR","tT","yY","uU","iI","oOóÓ","pP","[{","]}",
			"aA±¡","sS¶¦","dD","fF","gG","hH","jJ","kK","lL³£",";:","'\"","\\|",
			"zZ¿¯","xX¼¬","cCæÆ","vV","bB","nNñÑ","mM",",<",".>","/?",
			"<>|"
		};

		/*** Croatian keyboard layout ***/
		private static readonly string [] main_key_HR = new string []
		{
			"¸¨","1!","2\"·","3#^","4$¢","5%°","6&²","7/`","8(ÿ","9)´","0=½","'?¨","+*¸",
			"qQ\\","wW|","eE","rR","tT","zZ","uU","iI","oO","pP","¹©÷","ðÐ×",
			"aA","sS","dD","fF[","gG]","hH","jJ","kK³","lL£","èÈ","æÆß","¾®¤",
			"yY","xX","cC","vV@","bB{","nN}","mM§",",;",".:","-_/",
			"<>"
		};

		/*** Japanese 106 keyboard layout ***/
		private static readonly string [] main_key_JA_jp106 = new string []
		{
			"1!","2\"","3#","4$","5%","6&","7'","8(","9)","0~","-=","^~","\\|",
			"qQ","wW","eE","rR","tT","yY","uU","iI","oO","pP","@`","[{",
			"aA","sS","dD","fF","gG","hH","jJ","kK","lL",";+",":*","]}",
			"zZ","xX","cC","vV","bB","nN","mM",",<",".>","/?",
			"\\_",
		};

		/*** Japanese pc98x1 keyboard layout ***/
		private static readonly string [] main_key_JA_pc98x1 = new string []
		{
			"1!","2\"","3#","4$","5%","6&","7'","8(","9)","0","-=","^`","\\|",
			"qQ","wW","eE","rR","tT","yY","uU","iI","oO","pP","@~","[{",
			"aA","sS","dD","fF","gG","hH","jJ","kK","lL",";+",":*","]}",
			"zZ","xX","cC","vV","bB","nN","mM",",<",".>","/?",
			"\\_",
		};

		/*** Brazilian ABNT-2 keyboard layout (contributed by Raul Gomes Fernandes) */
		private static readonly string [] main_key_PT_br = new string []
		{
			"'\"","1!","2@","3#","4$","5%","6\"","7&","8*","9(","0)","-_","=+",
			"qQ","wW","eE","rR","tT","yY","uU","iI","oO","pP","'`","[{",
			"aA","sS","dD","fF","gG","hH","jJ","kK","lL","çÇ","~^","]}",
			"zZ","xX","cC","vV","bB","nN","mM",",<",".>","/?"
		};

		/*** US international keyboard layout (contributed by Gustavo Noronha (kov@debian.org)) */
		private static readonly string [] main_key_US_intl = new string []
		{
			"`~", "1!", "2@", "3#", "4$", "5%", "6^", "7&", "8*", "9(", "0)", "-_", "=+", "\\|",
			"qQ", "wW", "eE", "rR", "tT", "yY", "uU", "iI", "oO", "pP", "[{", "]}",
			"aA", "sS", "dD", "fF", "gG", "hH", "jJ", "kK", "lL", ";:", "'\"",
			"zZ", "xX", "cC", "vV", "bB", "nN", "mM", ",<", ".>", "/?"
		};

		/*** Slovak keyboard layout (see cssk_ibm(sk_qwerty) in xkbsel)
		     - dead_abovering replaced with degree - no symbol in iso8859-2
		     - brokenbar replaced with bar					*/
		private static readonly string [] main_key_SK = new string []
		{
			";°`'","+1","µ2","¹3","è4","»5","¾6","ý7","á8","í9","é0)","=%","",
			"qQ\\","wW|","eE","rR","tT","yY","uU","iI","oO","pP","ú/÷","ä(×",
			"aA","sSð","dDÐ","fF[","gG]","hH","jJ","kK³","lL£","ô\"$","§!ß","ò)¤",
			"zZ>","xX#","cC&","vV@","bB{","nN}","mM",",?<",".:>","-_*",
			"<>\\|"
		};

		/*** Slovak and Czech (programmer's) keyboard layout (see cssk_dual(cs_sk_ucw)) */
		private static readonly string [] main_key_SK_prog = new string []
		{
			"`~","1!","2@","3#","4$","5%","6^","7&","8*","9(","0)","-_","=+",
			"qQäÄ","wWìÌ","eEéÉ","rRøØ","tT»«","yYýÝ","uUùÙ","iIíÍ","oOóÓ","pPöÖ","[{","]}",
			"aAáÁ","sS¹©","dDïÏ","fFëË","gGàÀ","hHúÚ","jJüÜ","kKôÔ","lLµ¥",";:","'\"","\\|",
			"zZ¾®","xX¤","cCèÈ","vVçÇ","bB","nNòÒ","mMåÅ",",<",".>","/?",
			"<>"
		};

		/*** Czech keyboard layout (see cssk_ibm(cs_qwerty) in xkbsel) */
		private static readonly string [] main_key_CS = new string []
		{
			";","+1","ì2","¹3","è4","ø5","¾6","ý7","á8","í9","é0½)","=%","",
			"qQ\\","wW|","eE","rR","tT","yY","uU","iI","oO","pP","ú/[{",")(]}",
			"aA","sSð","dDÐ","fF[","gG]","hH","jJ","kK³","lL£","ù\"$","§!ß","¨'",
			"zZ>","xX#","cC&","vV@","bB{","nN}","mM",",?<",".:>","-_*",
			"<>\\|"
		};

		/*** Latin American keyboard layout (contributed by Gabriel Orlando Garcia) */
		private static readonly string [] main_key_LA = new string []
		{
			"|°¬","1!","2\"","3#","4$","5%","6&","7/","8(","9)","0=","'?\\","¡¿",
			"qQ@","wW","eE","rR","tT","yY","uU","iI","oO","pP","´¨","+*~",
			"aA","sS","dD","fF","gG","hH","jJ","kK","lL","ñÑ","{[^","}]`",
			"zZ","xX","cC","vV","bB","nN","mM",",;",".:","-_",
			"<>"
		};

		/*** Lithuanian (Baltic) keyboard layout (contributed by Nerijus Baliûnas) */
		private static readonly string [] main_key_LT_B = new string []
		{
			"`~","àÀ","èÈ","æÆ","ëË","áÁ","ðÐ","øØ","ûÛ","((","))","-_","þÞ",
			"qQ","wW","eE","rR","tT","yY","uU","iI","oO","pP","[{","]}",
			"aA","sS","dD","fF","gG","hH","jJ","kK","lL",";:","'\"","\\|",
			"zZ","xX","cC","vV","bB","nN","mM",",<",".>","/?"
		};

		/*** Turkish keyboard Layout */
		private static readonly string [] main_key_TK = new string []
		{
			"\"é","1!","2'","3^#","4+$","5%","6&","7/{","8([","9)]","0=}","*?\\","-_",
			"qQ@","wW","eE","rR","tT","yY","uU","ýIî","oO","pP","ðÐ","üÜ~",
			"aAæ","sSß","dD","fF","gG","hH","jJ","kK","lL","þÞ","iÝ",",;`",
			"zZ","xX","cC","vV","bB","nN","mM","öÖ","çÇ",".:"
		};

		private static readonly string [] main_key_vnc = new string []
		{
			"1!","2@","3#","4$","5%","6^","7&","8*","9(","0)","-_","=+","[{","]}",";:","'\"","`~",",<",".>","/?","\\|",
			"aA","bB","cC","dD","eE","fF","gG","hH","iI","jJ","kK","lL","mM","nN","oO","pP","qQ","rR","sS","tT","uU","vV","wW","xX","yY","zZ"
		};

		/*** VNC keyboard layout */
		private static readonly short [] main_key_scan_vnc = new short []
		{
			0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,0x0A,0x0B,0x0C,0x0D,0x1A,0x1B,0x27,0x28,0x29,0x33,0x34,0x35,0x2B,
			0x1E,0x30,0x2E,0x20,0x12,0x21,0x22,0x23,0x17,0x24,0x25,0x26,0x32,0x31,0x18,0x19,0x10,0x13,0x1F,0x14,0x16,0x2F,0x11,0x2D,0x15,0x2C,
			0x56
		};

		private static readonly VirtualKeys [] main_key_vkey_vnc = new VirtualKeys []
		{
			VirtualKeys.VK_1, VirtualKeys.VK_2, VirtualKeys.VK_3, VirtualKeys.VK_4, VirtualKeys.VK_5, VirtualKeys.VK_6, 
			VirtualKeys.VK_7, VirtualKeys.VK_8, VirtualKeys.VK_9, VirtualKeys.VK_0, VirtualKeys.VK_OEM_MINUS, 
			VirtualKeys.VK_OEM_PLUS, VirtualKeys.VK_OEM_4, VirtualKeys.VK_OEM_6, VirtualKeys.VK_OEM_1, 
			VirtualKeys.VK_OEM_7, VirtualKeys.VK_OEM_3, VirtualKeys.VK_OEM_COMMA, VirtualKeys.VK_OEM_PERIOD, 
			VirtualKeys.VK_OEM_2, VirtualKeys.VK_OEM_5, VirtualKeys.VK_A, VirtualKeys.VK_B, VirtualKeys.VK_C, 
			VirtualKeys.VK_D, VirtualKeys.VK_E, VirtualKeys.VK_F, VirtualKeys.VK_G, VirtualKeys.VK_H, 
			VirtualKeys.VK_I, VirtualKeys.VK_J, VirtualKeys.VK_K, VirtualKeys.VK_L, VirtualKeys.VK_M, 
			VirtualKeys.VK_N, VirtualKeys.VK_O, VirtualKeys.VK_P, VirtualKeys.VK_Q, VirtualKeys.VK_R, 
			VirtualKeys.VK_S, VirtualKeys.VK_T, VirtualKeys.VK_U, VirtualKeys.VK_V, VirtualKeys.VK_W, 
			VirtualKeys.VK_X, VirtualKeys.VK_Y, VirtualKeys.VK_Z, VirtualKeys.VK_OEM_102
		};

		private static readonly short [] main_key_scan_qwerty = new short []
		{
			/* this is my (102-key) keyboard layout, sorry if it doesn't quite match yours */
			/* `	1    2	  3    4    5	 6    7	   8	9    0	  -    = */
			0x29,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,0x0A,0x0B,0x0C,0x0D,
			/* q	w    e	  r    t    y	 u    i	   o	p    [	  ] */
			0x10,0x11,0x12,0x13,0x14,0x15,0x16,0x17,0x18,0x19,0x1A,0x1B,
			/* a	s    d	  f    g    h	 j    k	   l	;    '	  \ */
			0x1E,0x1F,0x20,0x21,0x22,0x23,0x24,0x25,0x26,0x27,0x28,0x2B,
			/* z	x    c	  v    b    n	 m    ,	   .	/ */
			0x2C,0x2D,0x2E,0x2F,0x30,0x31,0x32,0x33,0x34,0x35,
			0x56 /* the 102nd key (actually to the right of l-shift) */
		};

		private static readonly short [] main_key_scan_dvorak = new short []
		{
			/* `	1    2	  3    4    5	 6    7	   8	9    0	  [    ] */
			0x29,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,0x0A,0x0B,0x1A,0x1B,
			/* '	,    .	  p    y    f	 g    c	   r	l    /	  = */
			0x28,0x33,0x34,0x19,0x15,0x21,0x22,0x2E,0x13,0x26,0x35,0x0D,
			/* a	o    e	  u    i    d	 h    t	   n	s    -	  \ */
			0x1E,0x18,0x12,0x16,0x17,0x20,0x23,0x14,0x31,0x1F,0x0C,0x2B,
			/* ;	q    j	  k    x    b	 m    w	   v	z */
			0x27,0x10,0x24,0x25,0x2D,0x30,0x32,0x11,0x2F,0x2C,
			0x56 /* the 102nd key (actually to the right of l-shift) */
		};

		private static readonly VirtualKeys [] main_key_vkey_qwerty = new VirtualKeys []
		{
			// NOTE: this layout must concur with the scan codes layout above
			VirtualKeys.VK_OEM_3, VirtualKeys.VK_1, VirtualKeys.VK_2, VirtualKeys.VK_3, VirtualKeys.VK_4, 
			VirtualKeys.VK_5, VirtualKeys.VK_6, VirtualKeys.VK_7, VirtualKeys.VK_8, VirtualKeys.VK_9, 
			VirtualKeys.VK_0, VirtualKeys.VK_OEM_MINUS, VirtualKeys.VK_OEM_PLUS,  VirtualKeys.VK_Q, 
			VirtualKeys.VK_W, VirtualKeys.VK_E, VirtualKeys.VK_R, VirtualKeys.VK_T, VirtualKeys.VK_Y, 
			VirtualKeys.VK_U, VirtualKeys.VK_I, VirtualKeys.VK_O, VirtualKeys.VK_P, VirtualKeys.VK_OEM_4, 
			VirtualKeys.VK_OEM_6, VirtualKeys.VK_A, VirtualKeys.VK_S, VirtualKeys.VK_D, VirtualKeys.VK_F, 
			VirtualKeys.VK_G, VirtualKeys.VK_H, VirtualKeys.VK_J, VirtualKeys.VK_K, VirtualKeys.VK_L, 
			VirtualKeys.VK_OEM_1, VirtualKeys.VK_OEM_7, VirtualKeys.VK_OEM_5, VirtualKeys.VK_Z, 
			VirtualKeys.VK_X, VirtualKeys.VK_C, VirtualKeys.VK_V, VirtualKeys.VK_B, VirtualKeys.VK_N, 
			VirtualKeys.VK_M, VirtualKeys.VK_OEM_COMMA, VirtualKeys.VK_OEM_PERIOD, VirtualKeys.VK_OEM_2, 
			VirtualKeys.VK_OEM_102 // the 102nd key (actually to the right of l-shift)
		};

		private static readonly VirtualKeys [] main_key_vkey_dvorak = new VirtualKeys []
		{
			// NOTE: this layout must concur with the scan codes layout above
			VirtualKeys.VK_OEM_3, VirtualKeys.VK_1, VirtualKeys.VK_2, VirtualKeys.VK_3, VirtualKeys.VK_4,
			VirtualKeys.VK_5, VirtualKeys.VK_6, VirtualKeys.VK_7, VirtualKeys.VK_8, VirtualKeys.VK_9,
			VirtualKeys.VK_0, VirtualKeys.VK_OEM_4, VirtualKeys.VK_OEM_6, VirtualKeys.VK_OEM_7,
			VirtualKeys.VK_OEM_COMMA, VirtualKeys.VK_OEM_PERIOD, VirtualKeys.VK_P, VirtualKeys.VK_Y,
			VirtualKeys.VK_F, VirtualKeys.VK_G, VirtualKeys.VK_C, VirtualKeys.VK_R, VirtualKeys.VK_L,
			VirtualKeys.VK_OEM_2, VirtualKeys.VK_OEM_PLUS, VirtualKeys.VK_A, VirtualKeys.VK_O,
			VirtualKeys.VK_E, VirtualKeys.VK_U, VirtualKeys.VK_I, VirtualKeys.VK_D, VirtualKeys.VK_H,
			VirtualKeys.VK_T, VirtualKeys.VK_N, VirtualKeys.VK_S, VirtualKeys.VK_OEM_MINUS, VirtualKeys.VK_OEM_5, 
			VirtualKeys.VK_OEM_1, VirtualKeys.VK_Q, VirtualKeys.VK_J, VirtualKeys.VK_K, VirtualKeys.VK_X,
			VirtualKeys.VK_B, VirtualKeys.VK_M, VirtualKeys.VK_W, VirtualKeys.VK_V, VirtualKeys.VK_Z,
			VirtualKeys.VK_OEM_102 // the 102nd key (actually to the right of l-shift)
		};

		private static readonly VirtualKeys [] main_key_vkey_azerty = new VirtualKeys []
		{
			// NOTE: this layout must concur with the scan codes layout above
			VirtualKeys.VK_OEM_7, VirtualKeys.VK_1, VirtualKeys.VK_2, VirtualKeys.VK_3, VirtualKeys.VK_4,
			VirtualKeys.VK_5, VirtualKeys.VK_6, VirtualKeys.VK_7, VirtualKeys.VK_8, VirtualKeys.VK_9,
			VirtualKeys.VK_0, VirtualKeys.VK_OEM_4, VirtualKeys.VK_OEM_PLUS, VirtualKeys.VK_A, VirtualKeys.VK_Z,
			VirtualKeys.VK_E, VirtualKeys.VK_R, VirtualKeys.VK_T, VirtualKeys.VK_Y, VirtualKeys.VK_U,
			VirtualKeys.VK_I, VirtualKeys.VK_O, VirtualKeys.VK_P, VirtualKeys.VK_OEM_6, VirtualKeys.VK_OEM_1, 
			VirtualKeys.VK_Q, VirtualKeys.VK_S, VirtualKeys.VK_D, VirtualKeys.VK_F, VirtualKeys.VK_G,
			VirtualKeys.VK_H, VirtualKeys.VK_J, VirtualKeys.VK_K, VirtualKeys.VK_L, VirtualKeys.VK_M,
			VirtualKeys.VK_OEM_3, VirtualKeys.VK_OEM_5, VirtualKeys.VK_W, VirtualKeys.VK_X, VirtualKeys.VK_C,
			VirtualKeys.VK_V, VirtualKeys.VK_B, VirtualKeys.VK_N, VirtualKeys.VK_OEM_COMMA, VirtualKeys.VK_OEM_PERIOD,
			VirtualKeys.VK_OEM_2, VirtualKeys.VK_OEM_8, 
			VirtualKeys.VK_OEM_102 // the 102nd key (actually to the right of l-shift)
		};

		public static int [] nonchar_key_vkey = new int []
		{
			/* unused */
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FF00 */
			/* special keys */
			(int) VirtualKeys.VK_BACK, (int) VirtualKeys.VK_TAB, 0, (int) VirtualKeys.VK_CLEAR, 0, (int) VirtualKeys.VK_RETURN, 0, 0,	    /* FF08 */
			0, 0, 0, (int) VirtualKeys.VK_PAUSE, (int) VirtualKeys.VK_SCROLL_LOCK, 0, 0, 0,			     /* FF10 */
			0, 0, 0, (int) VirtualKeys.VK_ESCAPE, 0, 0, 0, 0,			      /* FF18 */
			/* unused */
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FF20 */
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FF28 */
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FF30 */
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FF38 */
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FF40 */
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FF48 */
			/* cursor keys */
			(int) VirtualKeys.VK_HOME, (int) VirtualKeys.VK_LEFT, (int) VirtualKeys.VK_UP, (int) VirtualKeys.VK_RIGHT,			    /* FF50 */
			(int) VirtualKeys.VK_DOWN, (int) VirtualKeys.VK_PRIOR, (int) VirtualKeys.VK_NEXT, (int) VirtualKeys.VK_END,
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FF58 */
			/* misc keys */
			(int) VirtualKeys.VK_SELECT, (int) VirtualKeys.VK_SNAPSHOT, (int) VirtualKeys.VK_EXECUTE, (int) VirtualKeys.VK_INSERT, 0, 0, 0, 0,  /* FF60 */
			(int) VirtualKeys.VK_CANCEL, (int) VirtualKeys.VK_HELP, (int) VirtualKeys.VK_CANCEL, (int) VirtualKeys.VK_CANCEL, 0, 0, 0, 0,	    /* FF68 */
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FF70 */
			/* keypad keys */
			0, 0, 0, 0, 0, 0, 0, (int) VirtualKeys.VK_NUMLOCK,			      /* FF78 */
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FF80 */
			0, 0, 0, 0, 0, (int) VirtualKeys.VK_RETURN, 0, 0,			      /* FF88 */
			0, 0, 0, 0, 0, (int) VirtualKeys.VK_HOME, (int) VirtualKeys.VK_LEFT, (int) VirtualKeys.VK_UP,			  /* FF90 */
			(int) VirtualKeys.VK_RIGHT, (int) VirtualKeys.VK_DOWN, (int) VirtualKeys.VK_PRIOR, (int) VirtualKeys.VK_NEXT,			    /* FF98 */
			(int) VirtualKeys.VK_END, 0, (int) VirtualKeys.VK_INSERT, (int) VirtualKeys.VK_DELETE,
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FFA0 */
			0, 0, (int) VirtualKeys.VK_MULTIPLY, (int) VirtualKeys.VK_ADD,					/* FFA8 */
			(int) VirtualKeys.VK_SEPARATOR, (int) VirtualKeys.VK_SUBTRACT, (int) VirtualKeys.VK_DECIMAL, (int) VirtualKeys.VK_DIVIDE,
			(int) VirtualKeys.VK_NUMPAD0, (int) VirtualKeys.VK_NUMPAD1, (int) VirtualKeys.VK_NUMPAD2, (int) VirtualKeys.VK_NUMPAD3,		    /* FFB0 */
			(int) VirtualKeys.VK_NUMPAD4, (int) VirtualKeys.VK_NUMPAD5, (int) VirtualKeys.VK_NUMPAD6, (int) VirtualKeys.VK_NUMPAD7,
			(int) VirtualKeys.VK_NUMPAD8, (int) VirtualKeys.VK_NUMPAD9, 0, 0, 0, 0,				/* FFB8 */
			/* function keys */
			(int) VirtualKeys.VK_F1, (int) VirtualKeys.VK_F2,
			(int) VirtualKeys.VK_F3, (int) VirtualKeys.VK_F4, (int) VirtualKeys.VK_F5, (int) VirtualKeys.VK_F6, (int) VirtualKeys.VK_F7, (int) VirtualKeys.VK_F8, (int) VirtualKeys.VK_F9, (int) VirtualKeys.VK_F10,    /* FFC0 */
			(int) VirtualKeys.VK_F11, (int) VirtualKeys.VK_F12, (int) VirtualKeys.VK_F13, (int) VirtualKeys.VK_F14, (int) VirtualKeys.VK_F15, (int) VirtualKeys.VK_F16, 0, 0,	/* FFC8 */
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FFD0 */
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FFD8 */
			/* modifier keys */
			0, (int) VirtualKeys.VK_SHIFT, (int) VirtualKeys.VK_SHIFT, (int) VirtualKeys.VK_CONTROL,			  /* FFE0 */
			(int) VirtualKeys.VK_CONTROL, (int) VirtualKeys.VK_CAPITAL, 0, (int) VirtualKeys.VK_MENU,
			(int) VirtualKeys.VK_MENU, (int) VirtualKeys.VK_MENU, (int) VirtualKeys.VK_MENU, 0, 0, 0, 0, 0,			  /* FFE8 */
			0, 0, 0, 0, 0, 0, 0, 0,					    /* FFF0 */
			0, 0, 0, 0, 0, 0, 0, (int) VirtualKeys.VK_DELETE			      /* FFF8 */
		};

		public static readonly int [] nonchar_key_scan = new int []
		{
			/* unused */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FF00 */
			/* special keys */
			0x0E, 0x0F, 0x00, /*?*/ 0, 0x00, 0x1C, 0x00, 0x00,	     /* FF08 */
			0x00, 0x00, 0x00, 0x45, 0x46, 0x00, 0x00, 0x00,		     /* FF10 */
			0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00,		     /* FF18 */
			/* unused */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FF20 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FF28 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FF30 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FF38 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FF40 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FF48 */
			/* cursor keys */
			0x147, 0x14B, 0x148, 0x14D, 0x150, 0x149, 0x151, 0x14F,	     /* FF50 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FF58 */
			/* misc keys */
			/*?*/ 0, 0x137, /*?*/ 0, 0x152, 0x00, 0x00, 0x00, 0x00,	     /* FF60 */
			/*?*/ 0, /*?*/ 0, 0x38, 0x146, 0x00, 0x00, 0x00, 0x00,	     /* FF68 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FF70 */
			/* keypad keys */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x138, 0x145,	     /* FF78 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FF80 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x11C, 0x00, 0x00,	     /* FF88 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x47, 0x4B, 0x48,		     /* FF90 */
			0x4D, 0x50, 0x49, 0x51, 0x4F, 0x4C, 0x52, 0x53,		     /* FF98 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FFA0 */
			0x00, 0x00, 0x37, 0x4E, /*?*/ 0, 0x4A, 0x53, 0x135,	     /* FFA8 */
			0x52, 0x4F, 0x50, 0x51, 0x4B, 0x4C, 0x4D, 0x47,		     /* FFB0 */
			0x48, 0x49, 0x00, 0x00, 0x00, 0x00,			     /* FFB8 */
			/* function keys */
			0x3B, 0x3C,
			0x3D, 0x3E, 0x3F, 0x40, 0x41, 0x42, 0x43, 0x44,		     /* FFC0 */
			0x57, 0x58, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FFC8 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FFD0 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FFD8 */
			/* modifier keys */
			0x00, 0x2A, 0x36, 0x1D, 0x11D, 0x3A, 0x00, 0x38,	     /* FFE0 */
			0x138, 0x38, 0x138, 0x00, 0x00, 0x00, 0x00, 0x00,	     /* FFE8 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,		     /* FFF0 */
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x153		     /* FFF8 */
		};

		public static readonly KeyboardLayout US = new KeyboardLayout ("United States keyboard layout", 28591,
				main_key_US, main_key_scan_qwerty, main_key_vkey_qwerty);
		public static readonly KeyboardLayout US_phantom = new KeyboardLayout ("United States keyboard layout (phantom key version)", 28591,
				main_key_US_phantom, main_key_scan_qwerty, main_key_vkey_qwerty);
		public static readonly KeyboardLayout US_dvorak = new KeyboardLayout ("United States keyboard layout (dvorak)", 28591,
				main_key_US_dvorak, main_key_scan_dvorak, main_key_vkey_dvorak);
		public static readonly KeyboardLayout UK = new KeyboardLayout ("British keyboard layout", 28591,
				main_key_UK, main_key_scan_qwerty, main_key_vkey_qwerty);
		public static readonly KeyboardLayout German = new KeyboardLayout ("German keyboard layout", 28591,
				main_key_DE, main_key_scan_qwerty, main_key_vkey_qwerty);
		public static readonly KeyboardLayout German_nodead = new KeyboardLayout ("German keyboard layout without dead keys", 28591,
				main_key_DE_nodead, main_key_scan_qwerty, main_key_vkey_qwerty);
		public static readonly KeyboardLayout SwissGerman = new KeyboardLayout ("Swiss German keyboard layout", 28591,
				main_key_SG, main_key_scan_qwerty, main_key_vkey_qwerty);
		public static readonly KeyboardLayout Se = new KeyboardLayout ("Swedish keyboard layout", 28591,
				main_key_SE, main_key_scan_qwerty, main_key_vkey_qwerty);
		public static readonly KeyboardLayout No = new KeyboardLayout ("Norwegian keyboard layout", 28591,
				main_key_NO, main_key_scan_qwerty, main_key_vkey_qwerty);
		public static readonly KeyboardLayout Da = new KeyboardLayout ("Danish keyboard layout", 28591,
				main_key_DA, main_key_scan_qwerty, main_key_vkey_qwerty);
		public static readonly KeyboardLayout Fr = new KeyboardLayout ("French keyboard layout", 28591,
				main_key_FR, main_key_scan_qwerty, main_key_vkey_azerty);
		public static readonly KeyboardLayout CF = new KeyboardLayout ("Canadian French keyboard layout", 28591,
				main_key_CF, main_key_scan_qwerty, main_key_vkey_qwerty);
		public static readonly KeyboardLayout Be = new KeyboardLayout ("Belgian keyboard layout", 28591,
				main_key_BE, main_key_scan_qwerty, main_key_vkey_azerty);
		public static readonly KeyboardLayout SF = new KeyboardLayout ("Swiss French keyboard layout", 28591,
				main_key_SF, main_key_scan_qwerty, main_key_vkey_qwerty);
		public static readonly KeyboardLayout Pt = new KeyboardLayout ("Portuguese keyboard layout", 28591,
				main_key_PT, main_key_scan_qwerty, main_key_vkey_qwerty);
		public static readonly KeyboardLayout Pt_br = new KeyboardLayout ("Brazilian ABNT-2 keyboard layout", 28591,
				main_key_PT_br, main_key_scan_qwerty, main_key_vkey_qwerty);
		public static readonly KeyboardLayout US_intl = new KeyboardLayout ("United States International keyboard layout", 28591,
				main_key_US_intl, main_key_scan_qwerty, main_key_vkey_qwerty);
		public static readonly KeyboardLayout Fi = new KeyboardLayout ("Finnish keyboard layout", 28591,
				main_key_FI, main_key_scan_qwerty, main_key_vkey_qwerty);
		public static readonly KeyboardLayout Ru = new KeyboardLayout ("Russian keyboard layout", 20866,
				main_key_RU, main_key_scan_qwerty, main_key_vkey_qwerty);
		public static readonly KeyboardLayout Ru_phantom = new KeyboardLayout ("Russian keyboard layout (phantom key version)", 20866,
				main_key_RU_phantom, main_key_scan_qwerty, main_key_vkey_qwerty);
		public static readonly KeyboardLayout Ru_koi8r = new KeyboardLayout ("Russian keyboard layout KOI8-R", 20866,
				main_key_RU_koi8r, main_key_scan_qwerty, main_key_vkey_qwerty);
		public static readonly KeyboardLayout Ua = new KeyboardLayout ("Ukrainian keyboard layout KOI8-U", 20866,
				main_key_UA, main_key_scan_qwerty, main_key_vkey_qwerty);
		public static readonly KeyboardLayout Es = new KeyboardLayout ("Spanish keyboard layout", 28591,
				main_key_ES, main_key_scan_qwerty, main_key_vkey_qwerty);
		public static readonly KeyboardLayout It = new KeyboardLayout ("Italian keyboard layout", 28591,
				main_key_IT, main_key_scan_qwerty, main_key_vkey_qwerty);
		public static readonly KeyboardLayout Is = new KeyboardLayout ("Icelandic keyboard layout", 28591,
				main_key_IS, main_key_scan_qwerty, main_key_vkey_qwerty);
		public static readonly KeyboardLayout Hu = new KeyboardLayout ("Hungarian keyboard layout", 28592,
				main_key_HU, main_key_scan_qwerty, main_key_vkey_qwerty);
		public static readonly KeyboardLayout Pl = new KeyboardLayout ("Polish (programmer's) keyboard layout", 28592,
				main_key_PL, main_key_scan_qwerty, main_key_vkey_qwerty);
		public static readonly KeyboardLayout Hr = new KeyboardLayout ("Croatian keyboard layout", 28592,
				main_key_HR, main_key_scan_qwerty, main_key_vkey_qwerty);
		public static readonly KeyboardLayout Ja_jp106 = new KeyboardLayout ("Japanese 106 keyboard layout", 932,
				main_key_JA_jp106, main_key_scan_qwerty, main_key_vkey_qwerty);
		public static readonly KeyboardLayout Ja_pc98x1 = new KeyboardLayout ("Japanese pc98x1 keyboard layout", 932,
				main_key_JA_pc98x1, main_key_scan_qwerty, main_key_vkey_qwerty);
		public static readonly KeyboardLayout Sk = new KeyboardLayout ("Slovak keyboard layout", 28592,
				main_key_SK, main_key_scan_qwerty, main_key_vkey_qwerty);
		public static readonly KeyboardLayout Sk_prog = new KeyboardLayout ("Slovak and Czech keyboard layout without dead keys", 28592,
				main_key_SK_prog, main_key_scan_qwerty, main_key_vkey_qwerty);
		public static readonly KeyboardLayout Cz = new KeyboardLayout ("Czech keyboard layout", 28592,
				main_key_CS, main_key_scan_qwerty, main_key_vkey_qwerty);
		public static readonly KeyboardLayout LA = new KeyboardLayout ("Latin American keyboard layout", 28591,
				main_key_LA, main_key_scan_qwerty, main_key_vkey_qwerty);
		public static readonly KeyboardLayout LT_B = new KeyboardLayout ("Lithuanian (Baltic) keyboard layout", 28603,
				main_key_LT_B, main_key_scan_qwerty, main_key_vkey_qwerty);
		public static readonly KeyboardLayout Tk = new KeyboardLayout ("Turkish keyboard layout", 28599,
				main_key_TK, main_key_scan_qwerty, main_key_vkey_qwerty);
		public static readonly KeyboardLayout Vnc = new KeyboardLayout ("VNC keyboard layout", 28591,
				main_key_vnc, main_key_scan_vnc, main_key_vkey_vnc);
		
		
		public static readonly KeyboardLayout [] layouts = new KeyboardLayout []
		{
			US, US_phantom, US_dvorak, UK, German, German_nodead, SwissGerman, Se, No, Da, Fr, CF, Be, SF, Pt,
			Pt_br, US_intl, Fi, Ru, Ru_phantom, Ru_koi8r, Ua, Es, It, Is, Hu, Pl, Hr, Ja_jp106, Ja_pc98x1, Sk,
			Sk_prog, Cz, LA, LT_B, Tk, Vnc
		};
		
		public static KeyboardLayout [] Layouts {
			get {
				return layouts;
			}
		}
	}
}

