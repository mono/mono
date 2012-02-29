//
// Mono.Data.Tds.Protocol.TdsCollation.cs
//
// Authors:
//   Veerapuram Varadhan  <vvaradhan@novell.com>
//	 Dmitry S. Kataev  <dmitryskey@hotmail.com>
//
// Charset parts - Copyright (C) 2006,2007 Dmitry S. Kataev
// Copyright (C) Novell Inc, 2009
//

//
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

using System;
using System.Collections;
using System.Text;

namespace Mono.Data.Tds {
	internal static class TdsCollation 
	{
		public static int LCID (byte[] collation)
		{
			if (collation == null)
				return -1;
			
			return (collation[0] | (collation[1] << 8) | ((collation[2] & 0x0F) << 16));
		}
		
		public static int CollationFlags (byte[] collation)
		{
			if (collation == null)
				return -1;
			
			return ((collation[2] & 0xF0) | ((collation[3] & 0x0F) << 4));
		}

		public static int Version (byte[] collation)
		{
			if (collation == null)
				return -1;
			
			return (collation[3] & 0xF0);
		}
		
		public static int SortId (byte[] collation)
		{
			if (collation == null)
				return -1;
			
			return (collation[4]);
		}
	}
	
	internal static class TdsCharset
	{
		private static Hashtable lcidCodes = new Hashtable();
		private static Hashtable sortCodes = new Hashtable();
		
		static TdsCharset ()
		{
			lcidCodes[0x401] = 1256; //ar-SA, Arabic
			lcidCodes[0x401] = 1256; //ar-SA, Arabic (Saudi Arabia)
			lcidCodes[0x402] = 1251; //bg-BG, Bulgarian
			lcidCodes[0x402] = 1251; //bg-BG, Bulgarian (Bulgaria)
			lcidCodes[0x403] = 1252; //ca-ES, Catalan
			lcidCodes[0x403] = 1252; //ca-ES, Catalan (Catalan)
			lcidCodes[0x404] = 950; //zh-TW, Chinese (Taiwan)
			lcidCodes[0x404] = 950; //zh-TW, Chinese (Traditional)
			lcidCodes[0x405] = 1250; //cs-CZ, Czech
			lcidCodes[0x405] = 1250; //cs-CZ, Czech (Czech Republic)
			lcidCodes[0x406] = 1252; //da-DK, Danish
			lcidCodes[0x406] = 1252; //da-DK, Danish (Denmark)
			lcidCodes[0x407] = 1252; //de-DE, German
			lcidCodes[0x407] = 1252; //de-DE, German (Germany)
			lcidCodes[0x408] = 1253; //el-GR, Greek
			lcidCodes[0x408] = 1253; //el-GR, Greek (Greece)
			lcidCodes[0x409] = 1252; //en-US, English
			lcidCodes[0x409] = 1252; //en-US, English (United States)
			lcidCodes[0x40a] = 1252; //es-ES, Spanish
			lcidCodes[0x40b] = 1252; //fi-FI, Finnish
			lcidCodes[0x40b] = 1252; //fi-FI, Finnish (Finland)
			lcidCodes[0x40c] = 1252; //fr-FR, French
			lcidCodes[0x40c] = 1252; //fr-FR, French (France)
			lcidCodes[0x40d] = 1255; //he-IL, Hebrew
			lcidCodes[0x40d] = 1255; //he-IL, Hebrew (Israel)
			lcidCodes[0x40e] = 1250; //hu-HU, Hungarian
			lcidCodes[0x40e] = 1250; //hu-HU, Hungarian (Hungary)
			lcidCodes[0x40f] = 1252; //is-IS, Icelandic
			lcidCodes[0x40f] = 1252; //is-IS, Icelandic (Iceland)
			lcidCodes[0x410] = 1252; //it-IT, Italian
			lcidCodes[0x410] = 1252; //it-IT, Italian (Italy)
			lcidCodes[0x411] = 932; //ja-JP, Japanese
			lcidCodes[0x411] = 932; //ja-JP, Japanese (Japan)
			lcidCodes[0x412] = 949; //ko-KR, Korean
			lcidCodes[0x412] = 949; //ko-KR, Korean (Korea)
			lcidCodes[0x413] = 1252; //nl-NL, Dutch
			lcidCodes[0x413] = 1252; //nl-NL, Dutch (Netherlands)
			lcidCodes[0x414] = 1252; //nb-NO, Norwegian
			lcidCodes[0x414] = 1252; //nb-NO, Norwegian, Bokmal (Norway)
			lcidCodes[0x415] = 1250; //pl-PL, Polish
			lcidCodes[0x415] = 1250; //pl-PL, Polish (Poland)
			lcidCodes[0x416] = 1252; //pt-BR, Portuguese
			lcidCodes[0x416] = 1252; //pt-BR, Portuguese (Brazil)
			lcidCodes[0x417] = 1252; //rm-CH, Romansh (Switzerland)
			lcidCodes[0x418] = 1250; //ro-RO, Romanian
			lcidCodes[0x418] = 1250; //ro-RO, Romanian (Romania)
			lcidCodes[0x419] = 1251; //ru-RU, Russian
			lcidCodes[0x419] = 1251; //ru-RU, Russian (Russia)
			lcidCodes[0x41a] = 1250; //hr-HR, Croatian
			lcidCodes[0x41a] = 1250; //hr-HR, Croatian (Croatia)
			lcidCodes[0x41b] = 1250; //sk-SK, Slovak
			lcidCodes[0x41b] = 1250; //sk-SK, Slovak (Slovakia)
			lcidCodes[0x41c] = 1250; //sq-AL, Albanian
			lcidCodes[0x41c] = 1250; //sq-AL, Albanian (Albania)
			lcidCodes[0x41d] = 1252; //sv-SE, Swedish
			lcidCodes[0x41d] = 1252; //sv-SE, Swedish (Sweden)
			lcidCodes[0x41e] = 874; //th-TH, Thai
			lcidCodes[0x41e] = 874; //th-TH, Thai (Thailand)
			lcidCodes[0x41f] = 1254; //tr-TR, Turkish
			lcidCodes[0x41f] = 1254; //tr-TR, Turkish (Turkey)
			lcidCodes[0x420] = 1256; //ur-PK, Urdu
			lcidCodes[0x420] = 1256; //ur-PK, Urdu (Islamic Republic of Pakistan)
			lcidCodes[0x421] = 1252; //id-ID, Indonesian
			lcidCodes[0x421] = 1252; //id-ID, Indonesian (Indonesia)
			lcidCodes[0x422] = 1251; //uk-UA, Ukrainian
			lcidCodes[0x422] = 1251; //uk-UA, Ukrainian (Ukraine)
			lcidCodes[0x423] = 1251; //be-BY, Belarusian
			lcidCodes[0x423] = 1251; //be-BY, Belarusian (Belarus)
			lcidCodes[0x424] = 1250; //sl-SI, Slovenian
			lcidCodes[0x424] = 1250; //sl-SI, Slovenian (Slovenia)
			lcidCodes[0x425] = 1257; //et-EE, Estonian
			lcidCodes[0x425] = 1257; //et-EE, Estonian (Estonia)
			lcidCodes[0x426] = 1257; //lv-LV, Latvian
			lcidCodes[0x426] = 1257; //lv-LV, Latvian (Latvia)
			lcidCodes[0x427] = 1257; //lt-LT, Lithuanian
			lcidCodes[0x427] = 1257; //lt-LT, Lithuanian (Lithuania)
			lcidCodes[0x429] = 1256; //fa-IR, Persian
			lcidCodes[0x429] = 1256; //fa-IR, Persian (Iran)
			lcidCodes[0x42a] = 1258; //vi-VN, Vietnamese
			lcidCodes[0x42a] = 1258; //vi-VN, Vietnamese (Vietnam)
			lcidCodes[0x42b] = 0; //hy-AM, Armenian
			lcidCodes[0x42b] = 0; //hy-AM, Armenian (Armenia)
			lcidCodes[0x42c] = 1254; //az-Latn-AZ, Azeri
			lcidCodes[0x42c] = 1254; //az-Latn-AZ, Azeri (Latin, Azerbaijan)
			lcidCodes[0x42d] = 1252; //eu-ES, Basque
			lcidCodes[0x42d] = 1252; //eu-ES, Basque (Basque)
			lcidCodes[0x42f] = 1251; //mk-MK, Macedonian
			lcidCodes[0x42f] = 1251; //mk-MK, Macedonian (Former Yugoslav Republic of Macedonia)
			lcidCodes[0x432] = 1252; //tn-ZA, Tswana (South Africa)
			lcidCodes[0x434] = 1252; //xh-ZA, Xhosa (South Africa)
			lcidCodes[0x435] = 1252; //zu-ZA, Zulu (South Africa)
			lcidCodes[0x436] = 1252; //af-ZA, Afrikaans
			lcidCodes[0x436] = 1252; //af-ZA, Afrikaans (South Africa)
			lcidCodes[0x437] = 0; //ka-GE, Georgian
			lcidCodes[0x437] = 0; //ka-GE, Georgian (Georgia)
			lcidCodes[0x438] = 1252; //fo-FO, Faroese
			lcidCodes[0x438] = 1252; //fo-FO, Faroese (Faroe Islands)
			lcidCodes[0x439] = 0; //hi-IN, Hindi
			lcidCodes[0x439] = 0; //hi-IN, Hindi (India)
			lcidCodes[0x43a] = 0; //mt-MT, Maltese (Malta)
			lcidCodes[0x43b] = 1252; //se-NO, Sami (Northern) (Norway)
			lcidCodes[0x43e] = 1252; //ms-MY, Malay
			lcidCodes[0x43e] = 1252; //ms-MY, Malay (Malaysia)
			lcidCodes[0x43f] = 1251; //kk-KZ, Kazakh
			lcidCodes[0x43f] = 1251; //kk-KZ, Kazakh (Kazakhstan)
			lcidCodes[0x440] = 1251; //ky-KG, Kyrgyz
			lcidCodes[0x440] = 1251; //ky-KG, Kyrgyz (Kyrgyzstan)
			lcidCodes[0x441] = 1252; //sw-KE, Kiswahili
			lcidCodes[0x441] = 1252; //sw-KE, Kiswahili (Kenya)
			lcidCodes[0x443] = 1254; //uz-Latn-UZ, Uzbek
			lcidCodes[0x443] = 1254; //uz-Latn-UZ, Uzbek (Latin, Uzbekistan)
			lcidCodes[0x444] = 1251; //tt-RU, Tatar
			lcidCodes[0x444] = 1251; //tt-RU, Tatar (Russia)
			lcidCodes[0x446] = 0; //pa-IN, Punjabi
			lcidCodes[0x446] = 0; //pa-IN, Punjabi (India)
			lcidCodes[0x447] = 0; //gu-IN, Gujarati
			lcidCodes[0x447] = 0; //gu-IN, Gujarati (India)
			lcidCodes[0x449] = 0; //ta-IN, Tamil
			lcidCodes[0x449] = 0; //ta-IN, Tamil (India)
			lcidCodes[0x44a] = 0; //te-IN, Telugu
			lcidCodes[0x44a] = 0; //te-IN, Telugu (India)
			lcidCodes[0x44b] = 0; //kn-IN, Kannada
			lcidCodes[0x44b] = 0; //kn-IN, Kannada (India)
			lcidCodes[0x44e] = 0; //mr-IN, Marathi
			lcidCodes[0x44e] = 0; //mr-IN, Marathi (India)
			lcidCodes[0x44f] = 0; //sa-IN, Sanskrit
			lcidCodes[0x44f] = 0; //sa-IN, Sanskrit (India)
			lcidCodes[0x450] = 1251; //mn-MN, Mongolian
			lcidCodes[0x450] = 1251; //mn-MN, Mongolian (Cyrillic, Mongolia)
			lcidCodes[0x452] = 1252; //cy-GB, Welsh (United Kingdom)
			lcidCodes[0x456] = 1252; //gl-ES, Galician
			lcidCodes[0x456] = 1252; //gl-ES, Galician (Galician)
			lcidCodes[0x457] = 0; //kok-IN, Konkani
			lcidCodes[0x457] = 0; //kok-IN, Konkani (India)
			lcidCodes[0x45a] = 0; //syr-SY, Syriac
			lcidCodes[0x45a] = 0; //syr-SY, Syriac (Syria)
			lcidCodes[0x462] = 1252; //fy-NL, Frisian (Netherlands)
			lcidCodes[0x464] = 1252; //fil-PH, Filipino (Philippines)
			lcidCodes[0x465] = 0; //dv-MV, Divehi
			lcidCodes[0x465] = 0; //dv-MV, Divehi (Maldives)
			lcidCodes[0x46b] = 1252; //quz-BO, Quechua (Bolivia)
			lcidCodes[0x46c] = 1252; //ns-ZA, Northern Sotho (South Africa)
			lcidCodes[0x46e] = 1252; //lb-LU, Luxembourgish (Luxembourg)
			lcidCodes[0x47a] = 1252; //arn-CL, Mapudungun (Chile)
			lcidCodes[0x47c] = 1252; //moh-CA, Mohawk (Canada)
			lcidCodes[0x481] = 0; //mi-NZ, Maori (New Zealand)
			lcidCodes[0x801] = 1256; //ar-IQ, Arabic (Iraq)
			lcidCodes[0x804] = 936; //zh-CN, Chinese (Simplified)
			lcidCodes[0x804] = 936; //zh-CN, Chinese (People's Republic of China)
			lcidCodes[0x807] = 1252; //de-CH, German (Switzerland)
			lcidCodes[0x809] = 1252; //en-GB, English (United Kingdom)
			lcidCodes[0x80a] = 1252; //es-MX, Spanish (Mexico)
			lcidCodes[0x80c] = 1252; //fr-BE, French (Belgium)
			lcidCodes[0x810] = 1252; //it-CH, Italian (Switzerland)
			lcidCodes[0x813] = 1252; //nl-BE, Dutch (Belgium)
			lcidCodes[0x814] = 1252; //nn-NO, Norwegian, Nynorsk (Norway)
			lcidCodes[0x816] = 1252; //pt-PT, Portuguese (Portugal)
			lcidCodes[0x81a] = 1250; //sr-Latn-CS, Serbian (Latin, Serbia)
			lcidCodes[0x81a] = 1251; //sr-Latn-CS, Serbian
			lcidCodes[0x81d] = 1252; //sv-FI, Swedish (Finland)
			lcidCodes[0x82c] = 1251; //az-Cyrl-AZ, Azeri (Cyrillic, Azerbaijan)
			lcidCodes[0x83b] = 1252; //se-SE, Sami (Northern) (Sweden)
			lcidCodes[0x83c] = 1252; //ga-IE, Irish (Ireland)
			lcidCodes[0x83e] = 1252; //ms-BN, Malay (Brunei Darussalam)
			lcidCodes[0x843] = 1251; //uz-Cyrl-UZ, Uzbek (Cyrillic, Uzbekistan)
			lcidCodes[0x85d] = 1252; //iu-Latn-CA, Inuktitut (Latin) (Canada)
			lcidCodes[0x86b] = 1252; //quz-EC, Quechua (Ecuador)
			lcidCodes[0xc01] = 1256; //ar-EG, Arabic (Egypt)
			lcidCodes[0xc04] = 950; //zh-HK, Chinese (Hong Kong S.A.R.)
			lcidCodes[0xc07] = 1252; //de-AT, German (Austria)
			lcidCodes[0xc09] = 1252; //en-AU, English (Australia)
			lcidCodes[0xc0a] = 1252; //es-ES, Spanish (Spain)
			lcidCodes[0xc0c] = 1252; //fr-CA, French (Canada)
			lcidCodes[0xc1a] = 1251; //sr-Cyrl-CS, Serbian (Cyrillic, Serbia)
			lcidCodes[0xc3b] = 1252; //se-FI, Sami (Northern) (Finland)
			lcidCodes[0xc6b] = 1252; //quz-PE, Quechua (Peru)
			lcidCodes[0x1001] = 1256; //ar-LY, Arabic (Libya)
			lcidCodes[0x1004] = 936; //zh-SG, Chinese (Singapore)
			lcidCodes[0x1007] = 1252; //de-LU, German (Luxembourg)
			lcidCodes[0x1009] = 1252; //en-CA, English (Canada)
			lcidCodes[0x100a] = 1252; //es-GT, Spanish (Guatemala)
			lcidCodes[0x100c] = 1252; //fr-CH, French (Switzerland)
			lcidCodes[0x101a] = 1250; //hr-BA, Croatian (Bosnia and Herzegovina)
			lcidCodes[0x103b] = 1252; //smj-NO, Sami (Lule) (Norway)
			lcidCodes[0x1401] = 1256; //ar-DZ, Arabic (Algeria)
			lcidCodes[0x1404] = 950; //zh-MO, Chinese (Macao S.A.R.)
			lcidCodes[0x1407] = 1252; //de-LI, German (Liechtenstein)
			lcidCodes[0x1409] = 1252; //en-NZ, English (New Zealand)
			lcidCodes[0x140a] = 1252; //es-CR, Spanish (Costa Rica)
			lcidCodes[0x140c] = 1252; //fr-LU, French (Luxembourg)
			lcidCodes[0x141a] = 1250; //bs-Latn-BA, Bosnian (Bosnia and Herzegovina)
			lcidCodes[0x143b] = 1252; //smj-SE, Sami (Lule) (Sweden)
			lcidCodes[0x1801] = 1256; //ar-MA, Arabic (Morocco)
			lcidCodes[0x1809] = 1252; //en-IE, English (Ireland)
			lcidCodes[0x180a] = 1252; //es-PA, Spanish (Panama)
			lcidCodes[0x180c] = 1252; //fr-MC, French (Principality of Monaco)
			lcidCodes[0x181a] = 1250; //sr-Latn-BA, Serbian (Latin) (Bosnia and Herzegovina)
			lcidCodes[0x183b] = 1252; //sma-NO, Sami (Southern) (Norway)
			lcidCodes[0x1c01] = 1256; //ar-TN, Arabic (Tunisia)
			lcidCodes[0x1c09] = 1252; //en-ZA, English (South Africa)
			lcidCodes[0x1c0a] = 1252; //es-DO, Spanish (Dominican Republic)
			lcidCodes[0x1c1a] = 1251; //sr-Cyrl-BA, Serbian (Cyrillic) (Bosnia and Herzegovina)
			lcidCodes[0x1c3b] = 1252; //sma-SE, Sami (Southern) (Sweden)
			lcidCodes[0x2001] = 1256; //ar-OM, Arabic (Oman)
			lcidCodes[0x2009] = 1252; //en-JM, English (Jamaica)
			lcidCodes[0x200a] = 1252; //es-VE, Spanish (Venezuela)
			lcidCodes[0x201a] = 1250; //bs-Cyrl-BA, Bosnian (Cyrillic) (Bosnia and Herzegovina)
			lcidCodes[0x203b] = 1252; //sms-FI, Sami (Skolt) (Finland)
			lcidCodes[0x2401] = 1256; //ar-YE, Arabic (Yemen)
			lcidCodes[0x2409] = 1252; //en-029, English (Caribbean)
			lcidCodes[0x240a] = 1252; //es-CO, Spanish (Colombia)
			lcidCodes[0x243b] = 1252; //smn-FI, Sami (Inari) (Finland)
			lcidCodes[0x2801] = 1256; //ar-SY, Arabic (Syria)
			lcidCodes[0x2809] = 1252; //en-BZ, English (Belize)
			lcidCodes[0x280a] = 1252; //es-PE, Spanish (Peru)
			lcidCodes[0x2c01] = 1256; //ar-JO, Arabic (Jordan)
			lcidCodes[0x2c09] = 1252; //en-TT, English (Trinidad and Tobago)
			lcidCodes[0x2c0a] = 1252; //es-AR, Spanish (Argentina)
			lcidCodes[0x3001] = 1256; //ar-LB, Arabic (Lebanon)
			lcidCodes[0x3009] = 1252; //en-ZW, English (Zimbabwe)
			lcidCodes[0x300a] = 1252; //es-EC, Spanish (Ecuador)
			lcidCodes[0x3401] = 1256; //ar-KW, Arabic (Kuwait)
			lcidCodes[0x3409] = 1252; //en-PH, English (Republic of the Philippines)
			lcidCodes[0x340a] = 1252; //es-CL, Spanish (Chile)
			lcidCodes[0x3801] = 1256; //ar-AE, Arabic (U.A.E.)
			lcidCodes[0x380a] = 1252; //es-UY, Spanish (Uruguay)
			lcidCodes[0x3c01] = 1256; //ar-BH, Arabic (Bahrain)
			lcidCodes[0x3c0a] = 1252; //es-PY, Spanish (Paraguay)
			lcidCodes[0x4001] = 1256; //ar-QA, Arabic (Qatar)
			lcidCodes[0x400a] = 1252; //es-BO, Spanish (Bolivia)
			lcidCodes[0x440a] = 1252; //es-SV, Spanish (El Salvador)
			lcidCodes[0x480a] = 1252; //es-HN, Spanish (Honduras)
			lcidCodes[0x4c0a] = 1252; //es-NI, Spanish (Nicaragua)
			lcidCodes[0x500a] = 1252; //es-PR, Spanish (Puerto Rico)
							
			sortCodes[30] = 437;
			sortCodes[31] = 437;
			sortCodes[32] = 437;
			sortCodes[33] = 437;
			sortCodes[34] = 437;
			sortCodes[40] = 850;
			sortCodes[41] = 850;
			sortCodes[42] = 850;
			sortCodes[43] = 850;
			sortCodes[44] = 850;
			sortCodes[49] = 850;
			sortCodes[50] = 1252;
			sortCodes[51] = 1252;
			sortCodes[52] = 1252;
			sortCodes[53] = 1252;
			sortCodes[54] = 1252;
			sortCodes[55] = 850;
			sortCodes[56] = 850;
			sortCodes[57] = 850;
			sortCodes[58] = 850;
			sortCodes[59] = 850;
			sortCodes[60] = 850;
			sortCodes[61] = 850;
			sortCodes[71] = 1252;
			sortCodes[72] = 1252;
			sortCodes[73] = 1252;
			sortCodes[74] = 1252;
			sortCodes[75] = 1252;
			sortCodes[80] = 1250;
			sortCodes[81] = 1250;
			sortCodes[82] = 1250;
			sortCodes[83] = 1250;
			sortCodes[84] = 1250;
			sortCodes[85] = 1250;
			sortCodes[86] = 1250;
			sortCodes[87] = 1250;
			sortCodes[88] = 1250;
			sortCodes[89] = 1250;
			sortCodes[90] = 1250;
			sortCodes[91] = 1250;
			sortCodes[92] = 1250;
			sortCodes[93] = 1250;
			sortCodes[94] = 1250;
			sortCodes[95] = 1250;
			sortCodes[96] = 1250;
			sortCodes[97] = 1250;
			sortCodes[98] = 1250;
			sortCodes[104] = 1251;
			sortCodes[105] = 1251;
			sortCodes[106] = 1251;
			sortCodes[107] = 1251;
			sortCodes[108] = 1251;
			sortCodes[112] = 1253;
			sortCodes[113] = 1253;
			sortCodes[114] = 1253;
			sortCodes[120] = 1253;
			sortCodes[121] = 1253;
			sortCodes[124] = 1253;
			sortCodes[128] = 1254;
			sortCodes[129] = 1254;
			sortCodes[130] = 1254;
			sortCodes[136] = 1255;
			sortCodes[137] = 1255;
			sortCodes[138] = 1255;
			sortCodes[144] = 1256;
			sortCodes[145] = 1256;
			sortCodes[146] = 1256;
			sortCodes[152] = 1257;
			sortCodes[153] = 1257;
			sortCodes[154] = 1257;
			sortCodes[155] = 1257;
			sortCodes[156] = 1257;
			sortCodes[157] = 1257;
			sortCodes[158] = 1257;
			sortCodes[159] = 1257;
			sortCodes[160] = 1257;
			sortCodes[183] = 1252;
			sortCodes[184] = 1252;
			sortCodes[185] = 1252;
			sortCodes[186] = 1252;
			sortCodes[192] = 932;
			sortCodes[193] = 932;
			sortCodes[194] = 949;
			sortCodes[195] = 949;
			sortCodes[196] = 950;
			sortCodes[197] = 950;
			sortCodes[198] = 936;
			sortCodes[199] = 936;
			sortCodes[200] = 932;
			sortCodes[201] = 949;
			sortCodes[202] = 950;
			sortCodes[203] = 936;
			sortCodes[204] = 874;
			sortCodes[205] = 874;
			sortCodes[206] = 874;
		}
		
		public static Encoding GetEncoding (byte[] collation) 
		{
			if (TdsCollation.SortId (collation) != 0)
				return GetEncodingFromSortOrder (collation);
			else
				return GetEncodingFromLCID (collation);
		}
		
		public static Encoding GetEncodingFromLCID (byte[] collation)
		{
			int lcid = TdsCollation.LCID (collation);
			return GetEncodingFromLCID (lcid);
		}
		
		public static Encoding GetEncodingFromLCID (int lcid)
		{
			if (lcidCodes[lcid] != null)
				return Encoding.GetEncoding ((int)lcidCodes[lcid]);
			else
				return null;			
		}
		
		public static Encoding GetEncodingFromSortOrder(byte[] collation)
		{
			int sortId = TdsCollation.SortId (collation);
			return GetEncodingFromSortOrder (sortId);
		}

		public static Encoding GetEncodingFromSortOrder (int sortId)
		{
			if (sortCodes[sortId] != null)
				return Encoding.GetEncoding ((int)sortCodes[sortId]);
			else
				return null;			
		}
	} 	
}
