using System.Globalization;

namespace System.Globalization {

	[Serializable]
	public class RegionInfo {
		int NLS_id;

		public RegionInfo (int culture) {

			if (CultureInfo.IsIDNeutralCulture (culture))
				throw new ArgumentException ("Culture ID " + culture
							 + " (0x" + culture.ToString ("X4")
							 + ") is a neutral culture. A region can not be created from it.");

			switch (culture) {
			case 0x0401: // ar-SA Arabic (Saudi Arabia)
				NLS_id = 682;
				break;
			case 0x0801: // ar-IQ Arabic (Iraq)
				NLS_id = 368;
				break;
			case 0x0C01: // ar-EG Arabic (Egypt)
				NLS_id = 818;
				break;
			case 0x1001: // ar-LY Arabic (Libya)
				NLS_id = 434;
				break;
			case 0x1401: // ar-DZ Arabic (Algeria)
				NLS_id = 12;
				break;
			case 0x1801: // ar-MA Arabic (Morocco)
				NLS_id = 504;
				break;
			case 0x1C01: // ar-TN Arabic (Tunisia)
				NLS_id = 788;
				break;
			case 0x2001: // ar-OM Arabic (Oman)
				NLS_id = 512;
				break;
			case 0x2401: // ar-YE Arabic (Yemen)
				NLS_id = 887;
				break;
			case 0x2801: // ar-SY Arabic (Syria)
				NLS_id = 760;
				break;
			case 0x2C01: // ar-JO Arabic (Jordan)
				NLS_id = 400;
				break;
			case 0x3001: // ar-LB Arabic (Lebanon)
				NLS_id = 422;
				break;
			case 0x3401: // ar-KW Arabic (Kuwait)
				NLS_id = 414;
				break;
			case 0x3801: // ar-AE Arabic (U.A.E.)
				NLS_id = 784;
				break;
			case 0x3C01: // ar-BH Arabic (Bahrain)
				NLS_id = 48;
				break;
			case 0x4001: // ar-QA Arabic (Qatar)
				NLS_id = 634;
				break;
			case 0x0402: // bg-BG Bulgarian (Bulgaria)
				NLS_id = 100;
				break;
			case 0x0403: // ca-ES Catalan (Spain)
				NLS_id = 724;
				break;
			case 0x0404: // zh-TW Chinese (Taiwan)
				NLS_id = 158;
				break;
			case 0x0804: // zh-CN Chinese (People's Republic of China)
				NLS_id = 156;
				break;
			case 0x0C04: // zh-HK Chinese (Hong Kong S.A.R.)
				NLS_id = 344;
				break;
			case 0x1004: // zh-SG Chinese (Singapore)
				NLS_id = 702;
				break;
			case 0x1404: // zh-MO Chinese (Macau S.A.R.)
				NLS_id = 446;
				break;
			case 0x0405: // cs-CZ Czech (Czech Republic)
				NLS_id = 203;
				break;
			case 0x0406: // da-DK Danish (Denmark)
				NLS_id = 208;
				break;
			case 0x0407: // de-DE German (Germany)
				NLS_id = 276;
				break;
			case 0x0807: // de-CH German (Switzerland)
				NLS_id = 756;
				break;
			case 0x0C07: // de-AT German (Austria)
				NLS_id = 40;
				break;
			case 0x1007: // de-LU German (Luxembourg)
				NLS_id = 442;
				break;
			case 0x1407: // de-LI German (Liechtenstein)
				NLS_id = 438;
				break;
			case 0x0408: // el-GR Greek (Greece)
				NLS_id = 300;
				break;
			case 0x0409: // en-US English (United States)
				NLS_id = 840;
				break;
			case 0x0809: // en-GB English (United Kingdom)
				NLS_id = 826;
				break;
			case 0x0C09: // en-AU English (Australia)
				NLS_id = 36;
				break;
			case 0x1009: // en-CA English (Canada)
				NLS_id = 124;
				break;
			case 0x1409: // en-NZ English (New Zealand)
				NLS_id = 554;
				break;
			case 0x1809: // en-IE English (Ireland)
				NLS_id = 372;
				break;
			case 0x1C09: // en-ZA English (South Africa)
				NLS_id = 710;
				break;
			case 0x2009: // en-JM English (Jamaica)
				NLS_id = 388;
				break;
			case 0x2809: // en-BZ English (Belize)
				NLS_id = 84;
				break;
			case 0x2C09: // en-TT English (Trinidad and Tobago)
				NLS_id = 780;
				break;
			case 0x3009: // en-ZW English (Zimbabwe)
				NLS_id = 716;
				break;
			case 0x3409: // en-PH English (Republic of the Philippines)
				NLS_id = 608;
				break;
			case 0x080A: // es-MX Spanish (Mexico)
				NLS_id = 484;
				break;
			case 0x0C0A: // es-ES Spanish (Spain)
				NLS_id = 724;
				break;
			case 0x100A: // es-GT Spanish (Guatemala)
				NLS_id = 320;
				break;
			case 0x140A: // es-CR Spanish (Costa Rica)
				NLS_id = 188;
				break;
			case 0x180A: // es-PA Spanish (Panama)
				NLS_id = 591;
				break;
			case 0x1C0A: // es-DO Spanish (Dominican Republic)
				NLS_id = 214;
				break;
			case 0x200A: // es-VE Spanish (Venezuela)
				NLS_id = 862;
				break;
			case 0x240A: // es-CO Spanish (Colombia)
				NLS_id = 170;
				break;
			case 0x280A: // es-PE Spanish (Peru)
				NLS_id = 604;
				break;
			case 0x2C0A: // es-AR Spanish (Argentina)
				NLS_id = 32;
				break;
			case 0x300A: // es-EC Spanish (Ecuador)
				NLS_id = 218;
				break;
			case 0x340A: // es-CL Spanish (Chile)
				NLS_id = 152;
				break;
			case 0x380A: // es-UY Spanish (Uruguay)
				NLS_id = 858;
				break;
			case 0x3C0A: // es-PY Spanish (Paraguay)
				NLS_id = 600;
				break;
			case 0x400A: // es-BO Spanish (Bolivia)
				NLS_id = 68;
				break;
			case 0x440A: // es-SV Spanish (El Salvador)
				NLS_id = 222;
				break;
			case 0x480A: // es-HN Spanish (Honduras)
				NLS_id = 340;
				break;
			case 0x4C0A: // es-NI Spanish (Nicaragua)
				NLS_id = 558;
				break;
			case 0x500A: // es-PR Spanish (Puerto Rico)
				NLS_id = 630;
				break;
			case 0x040B: // fi-FI Finnish (Finland)
				NLS_id = 246;
				break;
			case 0x040C: // fr-FR French (France)
				NLS_id = 250;
				break;
			case 0x080C: // fr-BE French (Belgium)
				NLS_id = 56;
				break;
			case 0x0C0C: // fr-CA French (Canada)
				NLS_id = 124;
				break;
			case 0x100C: // fr-CH French (Switzerland)
				NLS_id = 756;
				break;
			case 0x140C: // fr-LU French (Luxembourg)
				NLS_id = 442;
				break;
			case 0x180C: // fr-MC French (Principality of Monaco)
				NLS_id = 492;
				break;
			case 0x040D: // he-IL Hebrew (Israel)
				NLS_id = 376;
				break;
			case 0x040E: // hu-HU Hungarian (Hungary)
				NLS_id = 348;
				break;
			case 0x040F: // is-IS Icelandic (Iceland)
				NLS_id = 352;
				break;
			case 0x0410: // it-IT Italian (Italy)
				NLS_id = 380;
				break;
			case 0x0810: // it-CH Italian (Switzerland)
				NLS_id = 756;
				break;
			case 0x0411: // ja-JP Japanese (Japan)
				NLS_id = 392;
				break;
			case 0x0412: // ko-KR Korean (Korea)
				NLS_id = 410;
				break;
			case 0x0413: // nl-NL Dutch (Netherlands)
				NLS_id = 528;
				break;
			case 0x0813: // nl-BE Dutch (Belgium)
				NLS_id = 56;
				break;
			case 0x0414: // nb-NO Norwegian (Bokm†l) (Norway)
				NLS_id = 578;
				break;
			case 0x0814: // nn-NO Norwegian (Nynorsk) (Norway)
				NLS_id = 578;
				break;
			case 0x0415: // pl-PL Polish (Poland)
				NLS_id = 616;
				break;
			case 0x0416: // pt-BR Portuguese (Brazil)
				NLS_id = 76;
				break;
			case 0x0816: // pt-PT Portuguese (Portugal)
				NLS_id = 620;
				break;
			case 0x0418: // ro-RO Romanian (Romania)
				NLS_id = 642;
				break;
			case 0x0419: // ru-RU Russian (Russia)
				NLS_id = 643;
				break;
			case 0x041A: // hr-HR Croatian (Croatia)
				NLS_id = 191;
				break;
			case 0x041B: // sk-SK Slovak (Slovakia)
				NLS_id = 703;
				break;
			case 0x041C: // sq-AL Albanian (Albania)
				NLS_id = 8;
				break;
			case 0x041D: // sv-SE Swedish (Sweden)
				NLS_id = 752;
				break;
			case 0x081D: // sv-FI Swedish (Finland)
				NLS_id = 246;
				break;
			case 0x041E: // th-TH Thai (Thailand)
				NLS_id = 764;
				break;
			case 0x041F: // tr-TR Turkish (Turkey)
				NLS_id = 792;
				break;
			case 0x0420: // ur-PK Urdu (Islamic Republic of Pakistan)
				NLS_id = 586;
				break;
			case 0x0421: // id-ID Indonesian (Indonesia)
				NLS_id = 360;
				break;
			case 0x0422: // uk-UA Ukrainian (Ukraine)
				NLS_id = 804;
				break;
			case 0x0423: // be-BY Belarusian (Belarus)
				NLS_id = 112;
				break;
			case 0x0424: // sl-SI Slovenian (Slovenia)
				NLS_id = 705;
				break;
			case 0x0425: // et-EE Estonian (Estonia)
				NLS_id = 233;
				break;
			case 0x0426: // lv-LV Latvian (Latvia)
				NLS_id = 428;
				break;
			case 0x0427: // lt-LT Lithuanian (Lithuania)
				NLS_id = 440;
				break;
			case 0x0429: // fa-IR Farsi (Iran)
				NLS_id = 364;
				break;
			case 0x042A: // vi-VN Vietnamese (Viet Nam)
				NLS_id = 704;
				break;
			case 0x042B: // hy-AM Armenian (Armenia)
				NLS_id = 51;
				break;
			case 0x042C: // Lt-az-AZ Azeri (Latin) (Azerbaijan)
				NLS_id = 31;
				break;
			case 0x082C: // Cy-az-AZ Azeri (Cyrillic) (Azerbaijan)
				NLS_id = 31;
				break;
			case 0x042D: // eu-ES Basque (Spain)
				NLS_id = 724;
				break;
			case 0x042F: // mk-MK FYRO Macedonian (Former Yugoslav Republic of Macedonia)
				NLS_id = 807;
				break;
			case 0x0436: // af-ZA Afrikaans (South Africa)
				NLS_id = 710;
				break;
			case 0x0437: // ka-GE Georgian (Georgia)
				NLS_id = 268;
				break;
			case 0x0438: // fo-FO Faeroese (Faeroe Islands)
				NLS_id = 234;
				break;
			case 0x0439: // hi-IN Hindi (India)
				NLS_id = 356;
				break;
			case 0x043E: // ms-MY Malay (Malaysia)
				NLS_id = 458;
				break;
			case 0x083E: // ms-BN Malay (Brunei Darussalam)
				NLS_id = 96;
				break;
			case 0x043F: // kk-KZ Kazakh (Kazakhstan)
				NLS_id = 398;
				break;
			case 0x0440: // ky-KZ Kyrgyz (Kyrgyzstan)
				NLS_id = 398;
				break;
			case 0x0441: // sw-KE Swahili (Kenya)
				NLS_id = 404;
				break;
			case 0x0443: // Lt-uz-UZ Uzbek (Latin) (Uzbekistan)
				NLS_id = 860;
				break;
			case 0x0843: // Cy-uz-UZ Uzbek (Cyrillic) (Uzbekistan)
				NLS_id = 860;
				break;
			case 0x0446: // pa-IN Punjabi (India)
				NLS_id = 356;
				break;
			case 0x0447: // gu-IN Gujarati (India)
				NLS_id = 356;
				break;
			case 0x0449: // ta-IN Tamil (India)
				NLS_id = 356;
				break;
			case 0x044A: // te-IN Telugu (India)
				NLS_id = 356;
				break;
			case 0x044B: // kn-IN Kannada (India)
				NLS_id = 356;
				break;
			case 0x044E: // mr-IN Marathi (India)
				NLS_id = 356;
				break;
			case 0x044F: // sa-IN Sanskrit (India)
				NLS_id = 356;
				break;
			case 0x0450: // mn-MN Mongolian (Mongolia)
				NLS_id = 496;
				break;
			case 0x0456: // gl-ES Galician (Spain)
				NLS_id = 724;
				break;
			case 0x0457: // kok-IN Konkani (India)
				NLS_id = 356;
				break;
			case 0x045A: // syr-SY Syriac (Syria)
				NLS_id = 760;
				break;
			case 0x0465: // div-MV Divehi (Maldives)
				NLS_id = 462;
				break;
			case 0x007F: //  Invariant Language (Invariant Country)
				throw new ArgumentException ("There is no region associated with the Invariant Culture (Culture ID: 0x7F).");
			default:
				throw new ArgumentException ("Culture ID " + culture + " (0x" + culture.ToString ("X4")
							 + ") is not a supported culture.");
			}
		}

		public RegionInfo (string name) {
			switch (name.ToUpper ()) {
			case "AF": // Afghanistan
				NLS_id = 004;
				break;
			case "AL": // Albania
				NLS_id = 008;
				break;
			case "DZ": // Algeria
				NLS_id = 012;
				break;
			case "AS": // American Samoa
				NLS_id = 016;
				break;
			case "AD": // Andorra
				NLS_id = 020;
				break;
			case "AO": // Angola
				NLS_id = 024;
				break;
			case "AI": // Anguilla
				NLS_id = 660;
				break;
			case "AQ": // Antarctica
				NLS_id = 010;
				break;
			case "AG": // Antigua and Barbuda
				NLS_id = 028;
				break;
			case "AR": // Argentina
				NLS_id = 032;
				break;
			case "AM": // Armenia
				NLS_id = 051;
				break;
			case "AW": // Aruba
				NLS_id = 533;
				break;
			case "AU": // Australia
				NLS_id = 036;
				break;
			case "AT": // Austria
				NLS_id = 040;
				break;
			case "AZ": // Azerbaijan
				NLS_id = 031;
				break;
			case "BS": // Bahamas
				NLS_id = 044;
				break;
			case "BH": // Bahrain
				NLS_id = 048;
				break;
			case "BD": // Bangladesh
				NLS_id = 050;
				break;
			case "BB": // Barbados
				NLS_id = 052;
				break;
			case "BY": // Belarus
				NLS_id = 112;
				break;
			case "BE": // Belgium
				NLS_id = 056;
				break;
			case "BZ": // Belize
				NLS_id = 084;
				break;
			case "BJ": // Benin
				NLS_id = 204;
				break;
			case "BM": // Bermuda
				NLS_id = 060;
				break;
			case "BT": // Bhutan
				NLS_id = 064;
				break;
			case "BO": // Bolivia
				NLS_id = 068;
				break;
			case "BA": // Bosnia and Herzegowina
				NLS_id = 070;
				break;
			case "BW": // Botswana
				NLS_id = 072;
				break;
			case "BV": // Bouvet Island
				NLS_id = 074;
				break;
			case "BR": // Brazil
				NLS_id = 076;
				break;
			case "IO": // British Indian Ocean Territory
				NLS_id = 086;
				break;
			case "BN": // Brunei Darussalam
				NLS_id = 096;
				break;
			case "BG": // Bulgaria
				NLS_id = 100;
				break;
			case "BF": // Burkina Faso
				NLS_id = 854;
				break;
			case "BI": // Burundi
				NLS_id = 108;
				break;
			case "KH": // Cambodia
				NLS_id = 116;
				break;
			case "CM": // Cameroon
				NLS_id = 120;
				break;
			case "CA": // Canada
				NLS_id = 124;
				break;
			case "CV": // Cape Verde
				NLS_id = 132;
				break;
			case "KY": // Cayman Islands
				NLS_id = 136;
				break;
			case "CF": // Central African Republic
				NLS_id = 140;
				break;
			case "TD": // Chad
				NLS_id = 148;
				break;
			case "CL": // Chile
				NLS_id = 152;
				break;
			case "CN": // China
				NLS_id = 156;
				break;
			case "CX": // Christmas Island
				NLS_id = 162;
				break;
			case "CC": // Cocos (Keeling) Islands
				NLS_id = 166;
				break;
			case "CO": // Colombia
				NLS_id = 170;
				break;
			case "KM": // Comoros
				NLS_id = 174;
				break;
			case "CG": // Congo
				NLS_id = 178;
				break;
			case "CK": // Cook Islands
				NLS_id = 184;
				break;
			case "CR": // Costa Rica
				NLS_id = 188;
				break;
			case "CI": // Cote D'Ivoire
				NLS_id = 384;
				break;
			case "HR": // Croatia (Local Name: Hrvatska)
				NLS_id = 191;
				break;
			case "CU": // Cuba
				NLS_id = 192;
				break;
			case "CY": // Cyprus
				NLS_id = 196;
				break;
			case "CZ": // Czech Republic
				NLS_id = 203;
				break;
			case "DK": // Denmark
				NLS_id = 208;
				break;
			case "DJ": // Djibouti
				NLS_id = 262;
				break;
			case "DM": // Dominica
				NLS_id = 212;
				break;
			case "DO": // Dominican Republic
				NLS_id = 214;
				break;
			case "TP": // East Timor
				NLS_id = 626;
				break;
			case "EC": // Ecuador
				NLS_id = 218;
				break;
			case "EG": // Egypt
				NLS_id = 818;
				break;
			case "SV": // El Salvador
				NLS_id = 222;
				break;
			case "GQ": // Equatorial Guinea
				NLS_id = 226;
				break;
			case "ER": // Eritrea
				NLS_id = 232;
				break;
			case "EE": // Estonia
				NLS_id = 233;
				break;
			case "ET": // Ethiopia
				NLS_id = 231;
				break;
			case "FK": // Falkland Islands (Malvinas)
				NLS_id = 238;
				break;
			case "FO": // Faroe Islands
				NLS_id = 234;
				break;
			case "FJ": // Fiji
				NLS_id = 242;
				break;
			case "FI": // Finland
				NLS_id = 246;
				break;
			case "FR": // France
				NLS_id = 250;
				break;
			case "FX": // France, Metropolitan
				NLS_id = 249;
				break;
			case "GF": // French Guiana
				NLS_id = 254;
				break;
			case "PF": // French Polynesia
				NLS_id = 258;
				break;
			case "TF": // French Southern Territories
				NLS_id = 260;
				break;
			case "GA": // Gabon
				NLS_id = 266;
				break;
			case "GM": // Gambia
				NLS_id = 270;
				break;
			case "GE": // Georgia
				NLS_id = 268;
				break;
			case "DE": // Germany
				NLS_id = 276;
				break;
			case "GH": // Ghana
				NLS_id = 288;
				break;
			case "GI": // Gibraltar
				NLS_id = 292;
				break;
			case "GR": // Greece
				NLS_id = 300;
				break;
			case "GL": // Greenland
				NLS_id = 304;
				break;
			case "GD": // Grenada
				NLS_id = 308;
				break;
			case "GP": // Guadeloupe
				NLS_id = 312;
				break;
			case "GU": // Guam
				NLS_id = 316;
				break;
			case "GT": // Guatemala
				NLS_id = 320;
				break;
			case "GN": // Guinea
				NLS_id = 324;
				break;
			case "GW": // Guinea-Bissau
				NLS_id = 624;
				break;
			case "GY": // Guyana
				NLS_id = 328;
				break;
			case "HT": // Haiti
				NLS_id = 332;
				break;
			case "HM": // Heard and Mc Donald Islands
				NLS_id = 334;
				break;
			case "VA": // Holy See (Vatican City State)
				NLS_id = 336;
				break;
			case "HN": // Honduras
				NLS_id = 340;
				break;
			case "HK": // Hong Kong
				NLS_id = 344;
				break;
			case "HU": // Hungary
				NLS_id = 348;
				break;
			case "IS": // Iceland
				NLS_id = 352;
				break;
			case "IN": // India
				NLS_id = 356;
				break;
			case "ID": // Indonesia
				NLS_id = 360;
				break;
			case "IR": // Iran (Islamic Republic of)
				NLS_id = 364;
				break;
			case "IQ": // Iraq
				NLS_id = 368;
				break;
			case "IE": // Ireland
				NLS_id = 372;
				break;
			case "IL": // Israel
				NLS_id = 376;
				break;
			case "IT": // Italy
				NLS_id = 380;
				break;
			case "JM": // Jamaica
				NLS_id = 388;
				break;
			case "JP": // Japan
				NLS_id = 392;
				break;
			case "JO": // Jordan
				NLS_id = 400;
				break;
			case "KZ": // Kazakhstan
				NLS_id = 398;
				break;
			case "KE": // Kenya
				NLS_id = 404;
				break;
			case "KI": // Kiribati
				NLS_id = 296;
				break;
			case "KP": // Korea, Democratic People's Republic of
				NLS_id = 408;
				break;
			case "KR": // Korea, Republic of
				NLS_id = 410;
				break;
			case "KW": // Kuwait
				NLS_id = 414;
				break;
			case "KG": // Kyrgyzstan
				NLS_id = 417;
				break;
			case "LA": // Lao People's Democratic Republic
				NLS_id = 418;
				break;
			case "LV": // Latvia
				NLS_id = 428;
				break;
			case "LB": // Lebanon
				NLS_id = 422;
				break;
			case "LS": // Lesotho
				NLS_id = 426;
				break;
			case "LR": // Liberia
				NLS_id = 430;
				break;
			case "LY": // Libyan Arab Jamahiriya
				NLS_id = 434;
				break;
			case "LI": // Liechtenstein
				NLS_id = 438;
				break;
			case "LT": // Lithuania
				NLS_id = 440;
				break;
			case "LU": // Luxembourg
				NLS_id = 442;
				break;
			case "MO": // Macau
				NLS_id = 446;
				break;
			case "MK": // Macedonia, The Former Yugoslav Republic of
				NLS_id = 807;
				break;
			case "MG": // Madagascar
				NLS_id = 450;
				break;
			case "MW": // Malawi
				NLS_id = 454;
				break;
			case "MY": // Malaysia
				NLS_id = 458;
				break;
			case "MV": // Maldives
				NLS_id = 462;
				break;
			case "ML": // Mali
				NLS_id = 466;
				break;
			case "MT": // Malta
				NLS_id = 470;
				break;
			case "MH": // Marshall Islands
				NLS_id = 584;
				break;
			case "MQ": // Martinique
				NLS_id = 474;
				break;
			case "MR": // Mauritania
				NLS_id = 478;
				break;
			case "MU": // Mauritius
				NLS_id = 480;
				break;
			case "YT": // Mayotte
				NLS_id = 175;
				break;
			case "MX": // Mexico
				NLS_id = 484;
				break;
			case "FM": // Micronesia, Federated States of
				NLS_id = 583;
				break;
			case "MD": // Moldova, Republic of
				NLS_id = 498;
				break;
			case "MC": // Monaco
				NLS_id = 492;
				break;
			case "MN": // Mongolia
				NLS_id = 496;
				break;
			case "MS": // Montserrat
				NLS_id = 500;
				break;
			case "MA": // Morocco
				NLS_id = 504;
				break;
			case "MZ": // Mozambique
				NLS_id = 508;
				break;
			case "MM": // Myanmar
				NLS_id = 104;
				break;
			case "NA": // Namibia
				NLS_id = 516;
				break;
			case "NR": // Nauru
				NLS_id = 520;
				break;
			case "NP": // Nepal
				NLS_id = 524;
				break;
			case "NL": // Netherlands
				NLS_id = 528;
				break;
			case "AN": // Netherlands Antilles
				NLS_id = 530;
				break;
			case "NC": // New Caledonia
				NLS_id = 540;
				break;
			case "NZ": // New Zealand
				NLS_id = 554;
				break;
			case "NI": // Nicaragua
				NLS_id = 558;
				break;
			case "NE": // Niger
				NLS_id = 562;
				break;
			case "NG": // Nigeria
				NLS_id = 566;
				break;
			case "NU": // Niue
				NLS_id = 570;
				break;
			case "NF": // Norfolk Island
				NLS_id = 574;
				break;
			case "MP": // Northern Mariana Islands
				NLS_id = 580;
				break;
			case "NO": // Norway
				NLS_id = 578;
				break;
			case "OM": // Oman
				NLS_id = 512;
				break;
			case "PK": // Pakistan
				NLS_id = 586;
				break;
			case "PW": // Palau
				NLS_id = 585;
				break;
			case "PA": // Panama
				NLS_id = 591;
				break;
			case "PG": // Papua New Guinea
				NLS_id = 598;
				break;
			case "PY": // Paraguay
				NLS_id = 600;
				break;
			case "PE": // Peru
				NLS_id = 604;
				break;
			case "PH": // Philippines
				NLS_id = 608;
				break;
			case "PN": // Pitcairn
				NLS_id = 612;
				break;
			case "PL": // Poland
				NLS_id = 616;
				break;
			case "PT": // Portugal
				NLS_id = 620;
				break;
			case "PR": // Puerto Rico
				NLS_id = 630;
				break;
			case "QA": // Qatar
				NLS_id = 634;
				break;
			case "RE": // Reunion
				NLS_id = 638;
				break;
			case "RO": // Romania
				NLS_id = 642;
				break;
			case "RU": // Russian Federation
				NLS_id = 643;
				break;
			case "RW": // Rwanda
				NLS_id = 646;
				break;
			case "KN": // Saint Kitts and Nevis
				NLS_id = 659;
				break;
			case "LC": // Saint Lucia
				NLS_id = 662;
				break;
			case "VC": // Saint Vincent and The Grenadines
				NLS_id = 670;
				break;
			case "WS": // Samoa
				NLS_id = 882;
				break;
			case "SM": // San Marino
				NLS_id = 674;
				break;
			case "ST": // Sao Tome and Principe
				NLS_id = 678;
				break;
			case "SA": // Saudi Arabia
				NLS_id = 682;
				break;
			case "SN": // Senegal
				NLS_id = 686;
				break;
			case "SC": // Seychelles
				NLS_id = 690;
				break;
			case "SL": // Sierra Leone
				NLS_id = 694;
				break;
			case "SG": // Singapore
				NLS_id = 702;
				break;
			case "SK": // Slovakia (Slovak Republic)
				NLS_id = 703;
				break;
			case "SI": // Slovenia
				NLS_id = 705;
				break;
			case "SB": // Solomon Islands
				NLS_id = 090;
				break;
			case "SO": // Somalia
				NLS_id = 706;
				break;
			case "ZA": // South Africa
				NLS_id = 710;
				break;
			case "GS": // South Georgia and The South Sandwich Islands
				NLS_id = 239;
				break;
			case "ES": // Spain
				NLS_id = 724;
				break;
			case "LK": // Sri Lanka
				NLS_id = 144;
				break;
			case "SH": // St. Helena
				NLS_id = 654;
				break;
			case "PM": // St. Pierre and Miquelon
				NLS_id = 666;
				break;
			case "SD": // Sudan
				NLS_id = 736;
				break;
			case "SR": // Suriname
				NLS_id = 740;
				break;
			case "SJ": // Svalbard and Jan Mayen Islands
				NLS_id = 744;
				break;
			case "SZ": // Swaziland
				NLS_id = 748;
				break;
			case "SE": // Sweden
				NLS_id = 752;
				break;
			case "CH": // Switzerland
				NLS_id = 756;
				break;
			case "SY": // Syrian Arab Republic
				NLS_id = 760;
				break;
			case "TW": // Taiwan, Province of China
				NLS_id = 158;
				break;
			case "TJ": // Tajikistan
				NLS_id = 762;
				break;
			case "TZ": // Tanzania, United Republic of
				NLS_id = 834;
				break;
			case "TH": // Thailand
				NLS_id = 764;
				break;
			case "TG": // Togo
				NLS_id = 768;
				break;
			case "TK": // Tokelau
				NLS_id = 772;
				break;
			case "TO": // Tonga
				NLS_id = 776;
				break;
			case "TT": // Trinidad and Tobago
				NLS_id = 780;
				break;
			case "TN": // Tunisia
				NLS_id = 788;
				break;
			case "TR": // Turkey
				NLS_id = 792;
				break;
			case "TM": // Turkmenistan
				NLS_id = 795;
				break;
			case "TC": // Turks and Caicos Islands
				NLS_id = 796;
				break;
			case "TV": // Tuvalu
				NLS_id = 798;
				break;
			case "UG": // Uganda
				NLS_id = 800;
				break;
			case "UA": // Ukraine
				NLS_id = 804;
				break;
			case "AE": // United Arab Emirates
				NLS_id = 784;
				break;
			case "GB": // United Kingdom
				NLS_id = 826;
				break;
			case "US": // United States
				NLS_id = 840;
				break;
			case "UM": // United States Minor Outlying Islands
				NLS_id = 581;
				break;
			case "UY": // Uruguay
				NLS_id = 858;
				break;
			case "UZ": // Uzbekistan
				NLS_id = 860;
				break;
			case "VU": // Vanuatu
				NLS_id = 548;
				break;
			case "VE": // Venezuela
				NLS_id = 862;
				break;
			case "VN": // Viet Nam
				NLS_id = 704;
				break;
			case "VG": // Virgin Islands (British)
				NLS_id = 092;
				break;
			case "VI": // Virgin Islands (U.S.)
				NLS_id = 850;
				break;
			case "WF": // Wallis and Futuna Islands
				NLS_id = 876;
				break;
			case "EH": // Western Sahara
				NLS_id = 732;
				break;
			case "YE": // Yemen
				NLS_id = 887;
				break;
			case "YU": // Yugoslavia
				NLS_id = 891;
				break;
			case "ZR": // Zaire
				NLS_id = 180;
				break;
			case "ZM": // Zambia
				NLS_id = 894;
				break;
			case "ZW": // Zimbabwe
				NLS_id = 716;
				break;
			default:
				throw new ArgumentException ("Region name " + name + " is not supported.");
			}
		}

                public virtual string CurrencySymbol {
			get {
				switch (NLS_id) {
				default:
					throw new Exception ("Dunno what is currency symbol for " + NLS_id + " Region. FIXME.");
				}
			}
		}

		[MonoTODO]
		public static RegionInfo CurrentRegion {
			get {
				return null;
			}
		}

		public virtual string DisplayName {
			get {
				switch (NLS_id) {
				case 203: // Czech republic
					return "Èeská republika";
				case 840: // United States
					return "United States";
				default:
					throw new Exception ("FIXME. Please add your region name in language used in this region.");
				}
			}
		}

		public virtual string EnglishName {
			get {
				switch (NLS_id) {
				case 004:
					return "Afghanistan";
				case 008:
					return "Albania";
				case 012:
					return "Algeria";
				case 016:
					return "American Samoa";
				case 020:
					return "Andorra";
				case 024:
					return "Angola";
				case 660:
					return "Anguilla";
				case 010:
					return "Antarctica";
				case 028:
					return "Antigua and Barbuda";
				case 032:
					return "Argentina";
				case 051:
					return "Armenia";
				case 533:
					return "Aruba";
				case 036:
					return "Australia";
				case 040:
					return "Austria";
				case 031:
					return "Azerbaijan";
				case 044:
					return "Bahamas";
				case 048:
					return "Bahrain";
				case 050:
					return "Bangladesh";
				case 052:
					return "Barbados";
				case 112:
					return "Belarus";
				case 056:
					return "Belgium";
				case 084:
					return "Belize";
				case 204:
					return "Benin";
				case 060:
					return "Bermuda";
				case 064:
					return "Bhutan";
				case 068:
					return "Bolivia";
				case 070:
					return "Bosnia and Herzegowina";
				case 072:
					return "Botswana";
				case 074:
					return "Bouvet Island";
				case 076:
					return "Brazil";
				case 086:
					return "British Indian Ocean Territory";
				case 096:
					return "Brunei Darussalam";
				case 100:
					return "Bulgaria";
				case 854:
					return "Burkina Faso";
				case 108:
					return "Burundi";
				case 116:
					return "Cambodia";
				case 120:
					return "Cameroon";
				case 124:
					return "Canada";
				case 132:
					return "Cape Verde";
				case 136:
					return "Cayman Islands";
				case 140:
					return "Central African Republic";
				case 148:
					return "Chad";
				case 152:
					return "Chile";
				case 156:
					return "China";
				case 162:
					return "Christmas Island";
				case 166:
					return "Cocos (Keeling) Islands";
				case 170:
					return "Colombia";
				case 174:
					return "Comoros";
				case 178:
					return "Congo";
				case 184:
					return "Cook Islands";
				case 188:
					return "Costa Rica";
				case 384:
					return "Cote D'Ivoire";
				case 191:
					return "Croatia (Local Name: Hrvatska)";
				case 192:
					return "Cuba";
				case 196:
					return "Cyprus";
				case 203:
					return "Czech Republic";
				case 208:
					return "Denmark";
				case 262:
					return "Djibouti";
				case 212:
					return "Dominica";
				case 214:
					return "Dominican Republic";
				case 626:
					return "East Timor";
				case 218:
					return "Ecuador";
				case 818:
					return "Egypt";
				case 222:
					return "El Salvador";
				case 226:
					return "Equatorial Guinea";
				case 232:
					return "Eritrea";
				case 233:
					return "Estonia";
				case 231:
					return "Ethiopia";
				case 238:
					return "Falkland Islands (Malvinas)";
				case 234:
					return "Faroe Islands";
				case 242:
					return "Fiji";
				case 246:
					return "Finland";
				case 250:
					return "France";
				case 249:
					return "France, Metropolitan";
				case 254:
					return "French Guiana";
				case 258:
					return "French Polynesia";
				case 260:
					return "French Southern Territories";
				case 266:
					return "Gabon";
				case 270:
					return "Gambia";
				case 268:
					return "Georgia";
				case 276:
					return "Germany";
				case 288:
					return "Ghana";
				case 292:
					return "Gibraltar";
				case 300:
					return "Greece";
				case 304:
					return "Greenland";
				case 308:
					return "Grenada";
				case 312:
					return "Guadeloupe";
				case 316:
					return "Guam";
				case 320:
					return "Guatemala";
				case 324:
					return "Guinea";
				case 624:
					return "Guinea-Bissau";
				case 328:
					return "Guyana";
				case 332:
					return "Haiti";
				case 334:
					return "Heard and Mc Donald Islands";
				case 336:
					return "Holy See (Vatican City State)";
				case 340:
					return "Honduras";
				case 344:
					return "Hong Kong";
				case 348:
					return "Hungary";
				case 352:
					return "Iceland";
				case 356:
					return "India";
				case 360:
					return "Indonesia";
				case 364:
					return "Iran (Islamic Republic of)";
				case 368:
					return "Iraq";
				case 372:
					return "Ireland";
				case 376:
					return "Israel";
				case 380:
					return "Italy";
				case 388:
					return "Jamaica";
				case 392:
					return "Japan";
				case 400:
					return "Jordan";
				case 398:
					return "Kazakhstan";
				case 404:
					return "Kenya";
				case 296:
					return "Kiribati";
				case 408:
					return "Korea, Democratic People's Republic of";
				case 410:
					return "Korea, Republic of";
				case 414:
					return "Kuwait";
				case 417:
					return "Kyrgyzstan";
				case 418:
					return "Lao People's Democratic Republic";
				case 428:
					return "Latvia";
				case 422:
					return "Lebanon";
				case 426:
					return "Lesotho";
				case 430:
					return "Liberia";
				case 434:
					return "Libyan Arab Jamahiriya";
				case 438:
					return "Liechtenstein";
				case 440:
					return "Lithuania";
				case 442:
					return "Luxembourg";
				case 446:
					return "Macau";
				case 807:
					return "Macedonia, The Former Yugoslav Republic of";
				case 450:
					return "Madagascar";
				case 454:
					return "Malawi";
				case 458:
					return "Malaysia";
				case 462:
					return "Maldives";
				case 466:
					return "Mali";
				case 470:
					return "Malta";
				case 584:
					return "Marshall Islands";
				case 474:
					return "Martinique";
				case 478:
					return "Mauritania";
				case 480:
					return "Mauritius";
				case 175:
					return "Mayotte";
				case 484:
					return "Mexico";
				case 583:
					return "Micronesia, Federated States of";
				case 498:
					return "Moldova, Republic of";
				case 492:
					return "Monaco";
				case 496:
					return "Mongolia";
				case 500:
					return "Montserrat";
				case 504:
					return "Morocco";
				case 508:
					return "Mozambique";
				case 104:
					return "Myanmar";
				case 516:
					return "Namibia";
				case 520:
					return "Nauru";
				case 524:
					return "Nepal";
				case 528:
					return "Netherlands";
				case 530:
					return "Netherlands Antilles";
				case 540:
					return "New Caledonia";
				case 554:
					return "New Zealand";
				case 558:
					return "Nicaragua";
				case 562:
					return "Niger";
				case 566:
					return "Nigeria";
				case 570:
					return "Niue";
				case 574:
					return "Norfolk Island";
				case 580:
					return "Northern Mariana Islands";
				case 578:
					return "Norway";
				case 512:
					return "Oman";
				case 586:
					return "Pakistan";
				case 585:
					return "Palau";
				case 591:
					return "Panama";
				case 598:
					return "Papua New Guinea";
				case 600:
					return "Paraguay";
				case 604:
					return "Peru";
				case 608:
					return "Philippines";
				case 612:
					return "Pitcairn";
				case 616:
					return "Poland";
				case 620:
					return "Portugal";
				case 630:
					return "Puerto Rico";
				case 634:
					return "Qatar";
				case 638:
					return "Reunion";
				case 642:
					return "Romania";
				case 643:
					return "Russian Federation";
				case 646:
					return "Rwanda";
				case 659:
					return "Saint Kitts and Nevis";
				case 662:
					return "Saint Lucia";
				case 670:
					return "Saint Vincent and The Grenadines";
				case 882:
					return "Samoa";
				case 674:
					return "San Marino";
				case 678:
					return "Sao Tome and Principe";
				case 682:
					return "Saudi Arabia";
				case 686:
					return "Senegal";
				case 690:
					return "Seychelles";
				case 694:
					return "Sierra Leone";
				case 702:
					return "Singapore";
				case 703:
					return "Slovakia (Slovak Republic)";
				case 705:
					return "Slovenia";
				case 090:
					return "Solomon Islands";
				case 706:
					return "Somalia";
				case 710:
					return "South Africa";
				case 239:
					return "South Georgia and The South Sandwich Islands";
				case 724:
					return "Spain";
				case 144:
					return "Sri Lanka";
				case 654:
					return "St. Helena";
				case 666:
					return "St. Pierre and Miquelon";
				case 736:
					return "Sudan";
				case 740:
					return "Suriname";
				case 744:
					return "Svalbard and Jan Mayen Islands";
				case 748:
					return "Swaziland";
				case 752:
					return "Sweden";
				case 756:
					return "Switzerland";
				case 760:
					return "Syrian Arab Republic";
				case 158:
					return "Taiwan, Province of China";
				case 762:
					return "Tajikistan";
				case 834:
					return "Tanzania, United Republic of";
				case 764:
					return "Thailand";
				case 768:
					return "Togo";
				case 772:
					return "Tokelau";
				case 776:
					return "Tonga";
				case 780:
					return "Trinidad and Tobago";
				case 788:
					return "Tunisia";
				case 792:
					return "Turkey";
				case 795:
					return "Turkmenistan";
				case 796:
					return "Turks and Caicos Islands";
				case 798:
					return "Tuvalu";
				case 800:
					return "Uganda";
				case 804:
					return "Ukraine";
				case 784:
					return "United Arab Emirates";
				case 826:
					return "United Kingdom";
				case 840:
					return "United States";
				case 581:
					return "United States Minor Outlying Islands";
				case 858:
					return "Uruguay";
				case 860:
					return "Uzbekistan";
				case 548:
					return "Vanuatu";
				case 862:
					return "Venezuela";
				case 704:
					return "Viet Nam";
				case 092:
					return "Virgin Islands (British)";
				case 850:
					return "Virgin Islands (U.S.)";
				case 876:
					return "Wallis and Futuna Islands";
				case 732:
					return "Western Sahara";
				case 887:
					return "Yemen";
				case 891:
					return "Yugoslavia";
				case 180:
					return "Zaire";
				case 894:
					return "Zambia";
				case 716:
					return "Zimbabwe";
				default:
					throw new Exception ("This code should not be reached.");
				}
			}
		}

		public virtual bool IsMetric {
			get {
				switch (NLS_id) {
				case 203: // Czech Republic
					return true;
				case 840: // United States
					return false;
				default:
					throw new Exception ("FIXME. Please define.");
				}
			}
		}

		public virtual string ISOCurrencySymbol {
			get {
				switch (NLS_id) {
				default:
					throw new Exception ("This code should not be reached.");
				}
			}
		}

		[MonoTODO]
		public virtual string Name
		{
			get {
				throw new NotImplementedException();
			}
		}

		[MonoTODO]
		public virtual string ThreeLetterISORegionName
		{
			get {
				throw new NotImplementedException();
			}
		}

		[MonoTODO]
		public virtual string ThreeLetterWindowsRegionName
		{
			get {
				throw new NotImplementedException();
			}
		}
		
		[MonoTODO]
		public virtual string TwoLetterISORegionName
		{
			get {
				throw new NotImplementedException();
			}
		}

		//
		// methods

                public override bool Equals(object value) {
			return value == this;
		}

                public override int GetHashCode () {
			return NLS_id.GetHashCode ();
		}

		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException();
		}
		
	}

}
