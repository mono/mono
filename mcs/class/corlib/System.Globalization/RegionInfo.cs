using System.Globalization;

namespace System.Globalization {

	public class RegionInfo {
		int NLS_id;

		public RegionInfo (int culture) {

			if (CultureInfo.IsIDNeutralCulture (culture))
				throw new ArgumentException ("Culture ID " + culture
							 + " (0x" + culture.ToString ("X4")
							 + ") is a neutral culture. A region can not be created from it.");

			switch (culture) {
			case 0x0401: // ar-SA Arabic (Saudi Arabia)
			case 0x0801: // ar-IQ Arabic (Iraq)
			case 0x0C01: // ar-EG Arabic (Egypt)
			case 0x1001: // ar-LY Arabic (Libya)
			case 0x1401: // ar-DZ Arabic (Algeria)
			case 0x1801: // ar-MA Arabic (Morocco)
			case 0x1C01: // ar-TN Arabic (Tunisia)
			case 0x2001: // ar-OM Arabic (Oman)
			case 0x2401: // ar-YE Arabic (Yemen)
			case 0x2801: // ar-SY Arabic (Syria)
			case 0x2C01: // ar-JO Arabic (Jordan)
			case 0x3001: // ar-LB Arabic (Lebanon)
			case 0x3401: // ar-KW Arabic (Kuwait)
			case 0x3801: // ar-AE Arabic (U.A.E.)
			case 0x3C01: // ar-BH Arabic (Bahrain)
			case 0x4001: // ar-QA Arabic (Qatar)
			case 0x0402: // bg-BG Bulgarian (Bulgaria)
			case 0x0403: // ca-ES Catalan (Spain)
			case 0x0004: // zh-CHS Chinese (Simplified)
			case 0x0404: // zh-TW Chinese (Taiwan)
			case 0x0804: // zh-CN Chinese (People's Republic of China)
			case 0x0C04: // zh-HK Chinese (Hong Kong S.A.R.)
			case 0x1004: // zh-SG Chinese (Singapore)
			case 0x1404: // zh-MO Chinese (Macau S.A.R.)
			case 0x7C04: // zh-CHT Chinese (Traditional)
			case 0x0405: // cs-CZ Czech (Czech Republic)
			case 0x0406: // da-DK Danish (Denmark)
			case 0x0407: // de-DE German (Germany)
			case 0x0807: // de-CH German (Switzerland)
			case 0x0C07: // de-AT German (Austria)
			case 0x1007: // de-LU German (Luxembourg)
			case 0x1407: // de-LI German (Liechtenstein)
			case 0x0408: // el-GR Greek (Greece)
			case 0x0409: // en-US English (United States)
			case 0x0809: // en-GB English (United Kingdom)
			case 0x0C09: // en-AU English (Australia)
			case 0x1009: // en-CA English (Canada)
			case 0x1409: // en-NZ English (New Zealand)
			case 0x1809: // en-IE English (Ireland)
			case 0x1C09: // en-ZA English (South Africa)
			case 0x2009: // en-JM English (Jamaica)
			case 0x2409: // en-CB English (Caribbean)
			case 0x2809: // en-BZ English (Belize)
			case 0x2C09: // en-TT English (Trinidad and Tobago)
			case 0x3009: // en-ZW English (Zimbabwe)
			case 0x3409: // en-PH English (Republic of the Philippines)
			case 0x080A: // es-MX Spanish (Mexico)
			case 0x0C0A: // es-ES Spanish (Spain)
			case 0x100A: // es-GT Spanish (Guatemala)
			case 0x140A: // es-CR Spanish (Costa Rica)
			case 0x180A: // es-PA Spanish (Panama)
			case 0x1C0A: // es-DO Spanish (Dominican Republic)
			case 0x200A: // es-VE Spanish (Venezuela)
			case 0x240A: // es-CO Spanish (Colombia)
			case 0x280A: // es-PE Spanish (Peru)
			case 0x2C0A: // es-AR Spanish (Argentina)
			case 0x300A: // es-EC Spanish (Ecuador)
			case 0x340A: // es-CL Spanish (Chile)
			case 0x380A: // es-UY Spanish (Uruguay)
			case 0x3C0A: // es-PY Spanish (Paraguay)
			case 0x400A: // es-BO Spanish (Bolivia)
			case 0x440A: // es-SV Spanish (El Salvador)
			case 0x480A: // es-HN Spanish (Honduras)
			case 0x4C0A: // es-NI Spanish (Nicaragua)
			case 0x500A: // es-PR Spanish (Puerto Rico)
			case 0x040B: // fi-FI Finnish (Finland)
			case 0x040C: // fr-FR French (France)
			case 0x080C: // fr-BE French (Belgium)
			case 0x0C0C: // fr-CA French (Canada)
			case 0x100C: // fr-CH French (Switzerland)
			case 0x140C: // fr-LU French (Luxembourg)
			case 0x180C: // fr-MC French (Principality of Monaco)
			case 0x040D: // he-IL Hebrew (Israel)
			case 0x040E: // hu-HU Hungarian (Hungary)
			case 0x040F: // is-IS Icelandic (Iceland)
			case 0x0410: // it-IT Italian (Italy)
			case 0x0810: // it-CH Italian (Switzerland)
			case 0x0411: // ja-JP Japanese (Japan)
			case 0x0412: // ko-KR Korean (Korea)
			case 0x0413: // nl-NL Dutch (Netherlands)
			case 0x0813: // nl-BE Dutch (Belgium)
			case 0x0414: // nb-NO Norwegian (Bokmål) (Norway)
			case 0x0814: // nn-NO Norwegian (Nynorsk) (Norway)
			case 0x0415: // pl-PL Polish (Poland)
			case 0x0016: // pt Portuguese
			case 0x0416: // pt-BR Portuguese (Brazil)
			case 0x0816: // pt-PT Portuguese (Portugal)
			case 0x0418: // ro-RO Romanian (Romania)
			case 0x0419: // ru-RU Russian (Russia)
			case 0x041A: // hr-HR Croatian (Croatia)
			case 0x081A: // Lt-sr-SP Serbian (Latin) (Serbia)
			case 0x0C1A: // Cy-sr-SP Serbian (Cyrillic) (Serbia)
			case 0x041B: // sk-SK Slovak (Slovakia)
			case 0x041C: // sq-AL Albanian (Albania)
			case 0x041D: // sv-SE Swedish (Sweden)
			case 0x081D: // sv-FI Swedish (Finland)
			case 0x041E: // th-TH Thai (Thailand)
			case 0x041F: // tr-TR Turkish (Turkey)
			case 0x0420: // ur-PK Urdu (Islamic Republic of Pakistan)
			case 0x0421: // id-ID Indonesian (Indonesia)
			case 0x0422: // uk-UA Ukrainian (Ukraine)
			case 0x0423: // be-BY Belarusian (Belarus)
			case 0x0424: // sl-SI Slovenian (Slovenia)
			case 0x0425: // et-EE Estonian (Estonia)
			case 0x0426: // lv-LV Latvian (Latvia)
			case 0x0427: // lt-LT Lithuanian (Lithuania)
			case 0x0429: // fa-IR Farsi (Iran)
			case 0x042A: // vi-VN Vietnamese (Viet Nam)
			case 0x042B: // hy-AM Armenian (Armenia)
			case 0x042C: // Lt-az-AZ Azeri (Latin) (Azerbaijan)
			case 0x082C: // Cy-az-AZ Azeri (Cyrillic) (Azerbaijan)
			case 0x042D: // eu-ES Basque (Spain)
			case 0x042F: // mk-MK FYRO Macedonian (Former Yugoslav Republic of Macedonia)
			case 0x0436: // af-ZA Afrikaans (South Africa)
			case 0x0437: // ka-GE Georgian (Georgia)
			case 0x0438: // fo-FO Faeroese (Faeroe Islands)
			case 0x0439: // hi-IN Hindi (India)
			case 0x043E: // ms-MY Malay (Malaysia)
			case 0x083E: // ms-BN Malay (Brunei Darussalam)
			case 0x043F: // kk-KZ Kazakh (Kazakhstan)
			case 0x0440: // ky-KZ Kyrgyz (Kyrgyzstan)
			case 0x0441: // sw-KE Swahili (Kenya)
			case 0x0443: // Lt-uz-UZ Uzbek (Latin) (Uzbekistan)
			case 0x0843: // Cy-uz-UZ Uzbek (Cyrillic) (Uzbekistan)
			case 0x0444: // tt-TA Tatar (Tatarstan)
			case 0x0446: // pa-IN Punjabi (India)
			case 0x0447: // gu-IN Gujarati (India)
			case 0x0449: // ta-IN Tamil (India)
			case 0x044A: // te-IN Telugu (India)
			case 0x044B: // kn-IN Kannada (India)
			case 0x044E: // mr-IN Marathi (India)
			case 0x044F: // sa-IN Sanskrit (India)
			case 0x0450: // mn-MN Mongolian (Mongolia)
			case 0x0456: // gl-ES Galician (Spain)
			case 0x0457: // kok-IN Konkani (India)
			case 0x045A: // syr-SY Syriac (Syria)
			case 0x0465: // div-MV Divehi (Maldives)
			case 0x007F: //  Invariant Language (Invariant Country)
				throw new ArgumentException ("There is no region associated with the Invariant Culture (Culture ID: 0x7F).");
			default:
				throw new ArgumentException ("Culture ID " + culture + " (0x" + culture.ToString ("X4")
							 + ") is not a supported culture.");
			}
		}

		public RegionInfo (string name) {
		}

                public virtual string CurrencySymbol {
			get {
				switch (NLS_id) {
				default:
					throw new Exception ();
				}
			}
		}

                public override bool Equals(object value) {
			return value == this;
		}

                public override int GetHashCode () {
			return NLS_id.GetHashCode ();
		}
	}

}
