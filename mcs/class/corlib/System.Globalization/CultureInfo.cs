//
// System.Globalization.CultureInfo
//
// Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc. 2001 (http://www.ximian.com)
//

using System.Threading;

namespace System.Globalization
{
	public class CultureInfo
	{
		static CultureInfo invariant_culture_info;
		bool is_read_only;
		int  lcid;
		bool use_user_override;
		NumberFormatInfo number_format;
		DateTimeFormatInfo datetime_format;

		private static readonly string MSG_READONLY = "This instance is read only";
		
		// <summary>
		//   Returns the Invariant Culture Information ("iv")
		// </summary>
		static public CultureInfo InvariantCulture {
			get {
				if (invariant_culture_info != null)
					return invariant_culture_info;
				
				invariant_culture_info = new CultureInfo (0x07f, false);
				invariant_culture_info.is_read_only = true;
				
				return invariant_culture_info;
			}
		}

		//
		// Initializes the CultureInfo object for the specific culture_id
		//
		void InitializeByID (int culture_id, bool use_user_override)
		{
			switch (culture_id){
			case 0x0001: // ar Arabic
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
			case 0x0002: // bg Bulgarian
			case 0x0402: // bg-BG Bulgarian (Bulgaria)
			case 0x0003: // ca Catalan
			case 0x0403: // ca-ES Catalan (Spain)
			case 0x0004: // zh-CHS Chinese (Simplified)
			case 0x0404: // zh-TW Chinese (Taiwan)
			case 0x0804: // zh-CN Chinese (People's Republic of China)
			case 0x0C04: // zh-HK Chinese (Hong Kong S.A.R.)
			case 0x1004: // zh-SG Chinese (Singapore)
			case 0x1404: // zh-MO Chinese (Macau S.A.R.)
			case 0x7C04: // zh-CHT Chinese (Traditional)
			case 0x0005: // cs Czech
			case 0x0405: // cs-CZ Czech (Czech Republic)
			case 0x0006: // da Danish
			case 0x0406: // da-DK Danish (Denmark)
			case 0x0007: // de German
			case 0x0407: // de-DE German (Germany)
			case 0x0807: // de-CH German (Switzerland)
			case 0x0C07: // de-AT German (Austria)
			case 0x1007: // de-LU German (Luxembourg)
			case 0x1407: // de-LI German (Liechtenstein)
			case 0x0008: // el Greek
			case 0x0408: // el-GR Greek (Greece)
			case 0x0009: // en English
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
			case 0x000A: // es Spanish
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
			case 0x000B: // fi Finnish
			case 0x040B: // fi-FI Finnish (Finland)
			case 0x000C: // fr French
			case 0x040C: // fr-FR French (France)
			case 0x080C: // fr-BE French (Belgium)
			case 0x0C0C: // fr-CA French (Canada)
			case 0x100C: // fr-CH French (Switzerland)
			case 0x140C: // fr-LU French (Luxembourg)
			case 0x180C: // fr-MC French (Principality of Monaco)
			case 0x000D: // he Hebrew
			case 0x040D: // he-IL Hebrew (Israel)
			case 0x000E: // hu Hungarian
			case 0x040E: // hu-HU Hungarian (Hungary)
			case 0x000F: // is Icelandic
			case 0x040F: // is-IS Icelandic (Iceland)
			case 0x0010: // it Italian
			case 0x0410: // it-IT Italian (Italy)
			case 0x0810: // it-CH Italian (Switzerland)
			case 0x0011: // ja Japanese
			case 0x0411: // ja-JP Japanese (Japan)
			case 0x0012: // ko Korean
			case 0x0412: // ko-KR Korean (Korea)
			case 0x0013: // nl Dutch
			case 0x0413: // nl-NL Dutch (Netherlands)
			case 0x0813: // nl-BE Dutch (Belgium)
			case 0x0014: // no Norwegian
			case 0x0414: // nb-NO Norwegian (Bokmål) (Norway)
			case 0x0814: // nn-NO Norwegian (Nynorsk) (Norway)
			case 0x0015: // pl Polish
			case 0x0415: // pl-PL Polish (Poland)
			case 0x0016: // pt Portuguese
			case 0x0416: // pt-BR Portuguese (Brazil)
			case 0x0816: // pt-PT Portuguese (Portugal)
			case 0x0018: // ro Romanian
			case 0x0418: // ro-RO Romanian (Romania)
			case 0x0019: // ru Russian
			case 0x0419: // ru-RU Russian (Russia)
			case 0x001A: // hr Croatian
			case 0x041A: // hr-HR Croatian (Croatia)
			case 0x081A: // Lt-sr-SP Serbian (Latin) (Serbia)
			case 0x0C1A: // Cy-sr-SP Serbian (Cyrillic) (Serbia)
			case 0x001B: // sk Slovak
			case 0x041B: // sk-SK Slovak (Slovakia)
			case 0x001C: // sq Albanian
			case 0x041C: // sq-AL Albanian (Albania)
			case 0x001D: // sv Swedish
			case 0x041D: // sv-SE Swedish (Sweden)
			case 0x081D: // sv-FI Swedish (Finland)
			case 0x001E: // th Thai
			case 0x041E: // th-TH Thai (Thailand)
			case 0x001F: // tr Turkish
			case 0x041F: // tr-TR Turkish (Turkey)
			case 0x0020: // ur Urdu
			case 0x0420: // ur-PK Urdu (Islamic Republic of Pakistan)
			case 0x0021: // id Indonesian
			case 0x0421: // id-ID Indonesian (Indonesia)
			case 0x0022: // uk Ukrainian
			case 0x0422: // uk-UA Ukrainian (Ukraine)
			case 0x0023: // be Belarusian
			case 0x0423: // be-BY Belarusian (Belarus)
			case 0x0024: // sl Slovenian
			case 0x0424: // sl-SI Slovenian (Slovenia)
			case 0x0025: // et Estonian
			case 0x0425: // et-EE Estonian (Estonia)
			case 0x0026: // lv Latvian
			case 0x0426: // lv-LV Latvian (Latvia)
			case 0x0027: // lt Lithuanian
			case 0x0427: // lt-LT Lithuanian (Lithuania)
			case 0x0029: // fa Farsi
			case 0x0429: // fa-IR Farsi (Iran)
			case 0x002A: // vi Vietnamese
			case 0x042A: // vi-VN Vietnamese (Viet Nam)
			case 0x002B: // hy Armenian
			case 0x042B: // hy-AM Armenian (Armenia)
			case 0x002C: // az Azeri
			case 0x042C: // Lt-az-AZ Azeri (Latin) (Azerbaijan)
			case 0x082C: // Cy-az-AZ Azeri (Cyrillic) (Azerbaijan)
			case 0x002D: // eu Basque
			case 0x042D: // eu-ES Basque (Spain)
			case 0x002F: // mk FYRO Macedonian
			case 0x042F: // mk-MK FYRO Macedonian (Former Yugoslav Republic of Macedonia)
			case 0x0036: // af Afrikaans
			case 0x0436: // af-ZA Afrikaans (South Africa)
			case 0x0037: // ka Georgian
			case 0x0437: // ka-GE Georgian (Georgia)
			case 0x0038: // fo Faeroese
			case 0x0438: // fo-FO Faeroese (Faeroe Islands)
			case 0x0039: // hi Hindi
			case 0x0439: // hi-IN Hindi (India)
			case 0x003E: // ms Malay
			case 0x043E: // ms-MY Malay (Malaysia)
			case 0x083E: // ms-BN Malay (Brunei Darussalam)
			case 0x003F: // kk Kazakh
			case 0x043F: // kk-KZ Kazakh (Kazakhstan)
			case 0x0040: // ky Kyrgyz
			case 0x0440: // ky-KZ Kyrgyz (Kyrgyzstan)
			case 0x0041: // sw Swahili
			case 0x0441: // sw-KE Swahili (Kenya)
			case 0x0043: // uz Uzbek
			case 0x0443: // Lt-uz-UZ Uzbek (Latin) (Uzbekistan)
			case 0x0843: // Cy-uz-UZ Uzbek (Cyrillic) (Uzbekistan)
			case 0x0044: // tt Tatar
			case 0x0444: // tt-TA Tatar (Tatarstan)
			case 0x0046: // pa Punjabi
			case 0x0446: // pa-IN Punjabi (India)
			case 0x0047: // gu Gujarati
			case 0x0447: // gu-IN Gujarati (India)
			case 0x0049: // ta Tamil
			case 0x0449: // ta-IN Tamil (India)
			case 0x004A: // te Telugu
			case 0x044A: // te-IN Telugu (India)
			case 0x004B: // kn Kannada
			case 0x044B: // kn-IN Kannada (India)
			case 0x004E: // mr Marathi
			case 0x044E: // mr-IN Marathi (India)
			case 0x004F: // sa Sanskrit
			case 0x044F: // sa-IN Sanskrit (India)
			case 0x0050: // mn Mongolian
			case 0x0450: // mn-MN Mongolian (Mongolia)
			case 0x0056: // gl Galician
			case 0x0456: // gl-ES Galician (Spain)
			case 0x0057: // kok Konkani
			case 0x0457: // kok-IN Konkani (India)
			case 0x005A: // syr Syriac
			case 0x045A: // syr-SY Syriac (Syria)
			case 0x0065: // div Divehi
			case 0x0465: // div-MV Divehi (Maldives)
			case 0x007F: //  Invariant Language (Invariant Country)
				break;

			default:
				throw new ArgumentException ("CultureInfoCode");
			}
			lcid = culture_id;
			this.use_user_override = use_user_override;
		}

		//
		// Maps a name to a culture id
		//
		static int NameToID (string name)
		{
			switch (name){
			case "ar":
				return 0x0001;
			case "ar-SA":
				return 0x0401;
			case "ar-IQ":
				return 0x0801;
			case "ar-EG":
				return 0x0C01;
			case "ar-LY":
				return 0x1001;
			case "ar-DZ":
				return 0x1401;
			case "ar-MA":
				return 0x1801;
			case "ar-TN":
				return 0x1C01;
			case "ar-OM":
				return 0x2001;
			case "ar-YE":
				return 0x2401;
			case "ar-SY":
				return 0x2801;
			case "ar-JO":
				return 0x2C01;
			case "ar-LB":
				return 0x3001;
			case "ar-KW":
				return 0x3401;
			case "ar-AE":
				return 0x3801;
			case "ar-BH":
				return 0x3C01;
			case "ar-QA":
				return 0x4001;
			case "bg":
				return 0x0002;
			case "bg-BG":
				return 0x0402;
			case "ca":
				return 0x0003;
			case "ca-ES":
				return 0x0403;
			case "zh-CHS":
				return 0x0004;
			case "zh-TW":
				return 0x0404;
			case "zh-CN":
				return 0x0804;
			case "zh-HK":
				return 0x0C04;
			case "zh-SG":
				return 0x1004;
			case "zh-MO":
				return 0x1404;
			case "zh-CHT":
				return 0x7C04;
			case "cs":
				return 0x0005;
			case "cs-CZ":
				return 0x0405;
			case "da":
				return 0x0006;
			case "da-DK":
				return 0x0406;
			case "de":
				return 0x0007;
			case "de-DE":
				return 0x0407;
			case "de-CH":
				return 0x0807;
			case "de-AT":
				return 0x0C07;
			case "de-LU":
				return 0x1007;
			case "de-LI":
				return 0x1407;
			case "el":
				return 0x0008;
			case "el-GR":
				return 0x0408;
			case "en":
				return 0x0009;
			case "en-US":
				return 0x0409;
			case "en-GB":
				return 0x0809;
			case "en-AU":
				return 0x0C09;
			case "en-CA":
				return 0x1009;
			case "en-NZ":
				return 0x1409;
			case "en-IE":
				return 0x1809;
			case "en-ZA":
				return 0x1C09;
			case "en-JM":
				return 0x2009;
			case "en-CB":
				return 0x2409;
			case "en-BZ":
				return 0x2809;
			case "en-TT":
				return 0x2C09;
			case "en-ZW":
				return 0x3009;
			case "en-PH":
				return 0x3409;
			case "es":
				return 0x000A;
			case "es-MX":
				return 0x080A;
			case "es-ES":
				return 0x0C0A;
			case "es-GT":
				return 0x100A;
			case "es-CR":
				return 0x140A;
			case "es-PA":
				return 0x180A;
			case "es-DO":
				return 0x1C0A;
			case "es-VE":
				return 0x200A;
			case "es-CO":
				return 0x240A;
			case "es-PE":
				return 0x280A;
			case "es-AR":
				return 0x2C0A;
			case "es-EC":
				return 0x300A;
			case "es-CL":
				return 0x340A;
			case "es-UY":
				return 0x380A;
			case "es-PY":
				return 0x3C0A;
			case "es-BO":
				return 0x400A;
			case "es-SV":
				return 0x440A;
			case "es-HN":
				return 0x480A;
			case "es-NI":
				return 0x4C0A;
			case "es-PR":
				return 0x500A;
			case "fi":
				return 0x000B;
			case "fi-FI":
				return 0x040B;
			case "fr":
				return 0x000C;
			case "fr-FR":
				return 0x040C;
			case "fr-BE":
				return 0x080C;
			case "fr-CA":
				return 0x0C0C;
			case "fr-CH":
				return 0x100C;
			case "fr-LU":
				return 0x140C;
			case "fr-MC":
				return 0x180C;
			case "he":
				return 0x000D;
			case "he-IL":
				return 0x040D;
			case "hu":
				return 0x000E;
			case "hu-HU":
				return 0x040E;
			case "is":
				return 0x000F;
			case "is-IS":
				return 0x040F;
			case "it":
				return 0x0010;
			case "it-IT":
				return 0x0410;
			case "it-CH":
				return 0x0810;
			case "ja":
				return 0x0011;
			case "ja-JP":
				return 0x0411;
			case "ko":
				return 0x0012;
			case "ko-KR":
				return 0x0412;
			case "nl":
				return 0x0013;
			case "nl-NL":
				return 0x0413;
			case "nl-BE":
				return 0x0813;
			case "no":
				return 0x0014;
			case "nb-NO":
				return 0x0414;
			case "nn-NO":
				return 0x0814;
			case "pl":
				return 0x0015;
			case "pl-PL":
				return 0x0415;
			case "pt":
				return 0x0016;
			case "pt-BR":
				return 0x0416;
			case "pt-PT":
				return 0x0816;
			case "ro":
				return 0x0018;
			case "ro-RO":
				return 0x0418;
			case "ru":
				return 0x0019;
			case "ru-RU":
				return 0x0419;
			case "hr":
				return 0x001A;
			case "hr-HR":
				return 0x041A;
			case "Lt-sr-SP":
				return 0x081A;
			case "Cy-sr-SP":
				return 0x0C1A;
			case "sk":
				return 0x001B;
			case "sk-SK":
				return 0x041B;
			case "sq":
				return 0x001C;
			case "sq-AL":
				return 0x041C;
			case "sv":
				return 0x001D;
			case "sv-SE":
				return 0x041D;
			case "sv-FI":
				return 0x081D;
			case "th":
				return 0x001E;
			case "th-TH":
				return 0x041E;
			case "tr":
				return 0x001F;
			case "tr-TR":
				return 0x041F;
			case "ur":
				return 0x0020;
			case "ur-PK":
				return 0x0420;
			case "id":
				return 0x0021;
			case "id-ID":
				return 0x0421;
			case "uk":
				return 0x0022;
			case "uk-UA":
				return 0x0422;
			case "be":
				return 0x0023;
			case "be-BY":
				return 0x0423;
			case "sl":
				return 0x0024;
			case "sl-SI":
				return 0x0424;
			case "et":
				return 0x0025;
			case "et-EE":
				return 0x0425;
			case "lv":
				return 0x0026;
			case "lv-LV":
				return 0x0426;
			case "lt":
				return 0x0027;
			case "lt-LT":
				return 0x0427;
			case "fa":
				return 0x0029;
			case "fa-IR":
				return 0x0429;
			case "vi":
				return 0x002A;
			case "vi-VN":
				return 0x042A;
			case "hy":
				return 0x002B;
			case "hy-AM":
				return 0x042B;
			case "az":
				return 0x002C;
			case "Lt-az-AZ":
				return 0x042C;
			case "Cy-az-AZ":
				return 0x082C;
			case "eu":
				return 0x002D;
			case "eu-ES":
				return 0x042D;
			case "mk":
				return 0x002F;
			case "mk-MK":
				return 0x042F;
			case "af":
				return 0x0036;
			case "af-ZA":
				return 0x0436;
			case "ka":
				return 0x0037;
			case "ka-GE":
				return 0x0437;
			case "fo":
				return 0x0038;
			case "fo-FO":
				return 0x0438;
			case "hi":
				return 0x0039;
			case "hi-IN":
				return 0x0439;
			case "ms":
				return 0x003E;
			case "ms-MY":
				return 0x043E;
			case "ms-BN":
				return 0x083E;
			case "kk":
				return 0x003F;
			case "kk-KZ":
				return 0x043F;
			case "ky":
				return 0x0040;
			case "ky-KZ":
				return 0x0440;
			case "sw":
				return 0x0041;
			case "sw-KE":
				return 0x0441;
			case "uz":
				return 0x0043;
			case "Lt-uz-UZ":
				return 0x0443;
			case "Cy-uz-UZ":
				return 0x0843;
			case "tt":
				return 0x0044;
			case "tt-TA":
				return 0x0444;
			case "pa":
				return 0x0046;
			case "pa-IN":
				return 0x0446;
			case "gu":
				return 0x0047;
			case "gu-IN":
				return 0x0447;
			case "ta":
				return 0x0049;
			case "ta-IN":
				return 0x0449;
			case "te":
				return 0x004A;
			case "te-IN":
				return 0x044A;
			case "kn":
				return 0x004B;
			case "kn-IN":
				return 0x044B;
			case "mr":
				return 0x004E;
			case "mr-IN":
				return 0x044E;
			case "sa":
				return 0x004F;
			case "sa-IN":
				return 0x044F;
			case "mn":
				return 0x0050;
			case "mn-MN":
				return 0x0450;
			case "gl":
				return 0x0056;
			case "gl-ES":
				return 0x0456;
			case "kok":
				return 0x0057;
			case "kok-IN":
				return 0x0457;
			case "syr":
				return 0x005A;
			case "syr-SY":
				return 0x045A;
			case "div":
				return 0x0065;
			case "div-MV":
				return 0x0465;
			case "":
				return 0x007F;
			}
			return -1;
		}
		
		// <summary>
		//   Creates a CultureInfo for a specific ID
		// </summary>
		public static CultureInfo CreateSpecificCulture (string name)
		{
			if (name == null)
				throw new ArgumentNullException ();
			
			int id = NameToID (name);

			if (id == -1)
				throw new ArgumentException ("name");

			return new CultureInfo (id, false);
		}

		/// <summary>
		/// CultureInfo instance that represents the culture used by the current thread
		/// </summary>
		public static CultureInfo CurrentCulture 
		{
			get 
			{
				return Thread.CurrentThread.CurrentCulture;
			}
			
			set 
			{
				Thread.CurrentThread.CurrentCulture = value;
			}
		}

		/// <summary>
		/// CultureInfo instance that represents the current culture used by the ResourceManager to look up culture-specific resources at run time
		/// </summary>
		public static CultureInfo CurrentUICulture 
		{
			get 
			{
				return Thread.CurrentThread.CurrentUICulture;
			}
			
			set 
			{
				Thread.CurrentThread.CurrentUICulture =	value;
			}
		}


		public virtual int LCID {
			get {
				return lcid;
			}
		}

		// <summary>
		//   Gets the string-encoded name of the culture
		// </summary>
		public virtual string Name {
			get {
				switch (lcid){
				case 0x007f:
					return "iv";
				}
				throw new Exception ("Miss constructed object for LCID: " + lcid);
			}
		}

		internal static bool IsIDNeutralCulture (int lcid) {
			return (lcid & 0xff00) == 0;
		}

		// <summary>
		//   Returns whether the current culture is neutral (neutral cultures
		//   only specify a language, not a country.
		// </summary>
		public virtual bool IsNeutralCulture {
			get {
				return IsIDNeutralCulture (lcid);
			}
		}
		// <summary>
		//   Returns the NumberFormat for the current lcid
		// </summary>
		public virtual NumberFormatInfo NumberFormat {
			get {
				if (number_format == null){
					lock (this){
						if (number_format == null)
							number_format = new NumberFormatInfo (lcid);
					}
				}

				return number_format;
			}

			set {
				if (is_read_only) throw new InvalidOperationException(MSG_READONLY);

				if (value == null)
					throw new ArgumentNullException ("NumberFormat");
				
				number_format = value;
			}
		}

		public virtual DateTimeFormatInfo DateTimeFormat
		{
			get 
			{
				if (datetime_format == null)
				{
					lock (this)
					{
						if (datetime_format == null)
							datetime_format = new DateTimeFormatInfo(); 
					}
				}

				return datetime_format;
			}

			set 
			{
				if (is_read_only) throw new InvalidOperationException(MSG_READONLY);

				if (value == null)
					throw new ArgumentNullException ("DateTimeFormat");
				
				datetime_format = value;
			}
		}

		//
		// Constructors
		//
		public CultureInfo (int culture, bool use_user_override)
		{
			if (culture < 0)
				throw new ArgumentOutOfRangeException ();
			
			InitializeByID (culture, use_user_override);
		}

		public CultureInfo (int culture) : this (culture, false)
		{
		}
		
		public CultureInfo (string name, bool use_user_override)
		{
			if (name == null)
				throw new ArgumentNullException ();
			
			InitializeByID (NameToID (name), use_user_override);
		}

		public CultureInfo (string name) : this (name, false) {} 
	}
}
