//
// System.Globalization.CultureInfo
//
// Miguel de Icaza (miguel@ximian.com)
// Dick Porter (dick@ximian.com)
//
// (C) 2001, 2002 Ximian, Inc. (http://www.ximian.com)
//

using System.Collections;
using System.Threading;

namespace System.Globalization
{
	[Serializable]
	public class CultureInfo : ICloneable, IFormatProvider
	{
		static CultureInfo invariant_culture_info;
		static Hashtable nameToID;
		bool is_read_only;
		int  lcid;
		bool use_user_override;
		NumberFormatInfo number_format;
		DateTimeFormatInfo datetime_format;
		TextInfo textinfo;

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
				throw new ArgumentException ("CultureInfoCode " + culture_id);
			}
			lcid = culture_id;
			this.use_user_override = use_user_override;
		}

		static void InitNameToID ()
		{
			nameToID = new Hashtable (CaseInsensitiveHashCodeProvider.Default,
						  CaseInsensitiveComparer.Default);

			nameToID.Add ("ar", 0x0001);
			nameToID.Add ("ar-SA", 0x0401);
			nameToID.Add ("ar-IQ", 0x0801);
			nameToID.Add ("ar-EG", 0x0C01);
			nameToID.Add ("ar-LY", 0x1001);
			nameToID.Add ("ar-DZ", 0x1401);
			nameToID.Add ("ar-MA", 0x1801);
			nameToID.Add ("ar-TN", 0x1C01);
			nameToID.Add ("ar-OM", 0x2001);
			nameToID.Add ("ar-YE", 0x2401);
			nameToID.Add ("ar-SY", 0x2801);
			nameToID.Add ("ar-JO", 0x2C01);
			nameToID.Add ("ar-LB", 0x3001);
			nameToID.Add ("ar-KW", 0x3401);
			nameToID.Add ("ar-AE", 0x3801);
			nameToID.Add ("ar-BH", 0x3C01);
			nameToID.Add ("ar-QA", 0x4001);
			nameToID.Add ("bg", 0x0002);
			nameToID.Add ("bg-BG", 0x0402);
			nameToID.Add ("ca", 0x0003);
			nameToID.Add ("ca-ES", 0x0403);
			nameToID.Add ("zh-CHS", 0x0004);
			nameToID.Add ("zh-TW", 0x0404);
			nameToID.Add ("zh-CN", 0x0804);
			nameToID.Add ("zh-HK", 0x0C04);
			nameToID.Add ("zh-SG", 0x1004);
			nameToID.Add ("zh-MO", 0x1404);
			nameToID.Add ("zh-CHT", 0x7C04);
			nameToID.Add ("cs", 0x0005);
			nameToID.Add ("cs-CZ", 0x0405);
			nameToID.Add ("da", 0x0006);
			nameToID.Add ("da-DK", 0x0406);
			nameToID.Add ("de", 0x0007);
			nameToID.Add ("de-DE", 0x0407);
			nameToID.Add ("de-CH", 0x0807);
			nameToID.Add ("de-AT", 0x0C07);
			nameToID.Add ("de-LU", 0x1007);
			nameToID.Add ("de-LI", 0x1407);
			nameToID.Add ("el", 0x0008);
			nameToID.Add ("el-GR", 0x0408);
			nameToID.Add ("en", 0x0009);
			nameToID.Add ("en-US", 0x0409);
			nameToID.Add ("en-GB", 0x0809);
			nameToID.Add ("en-AU", 0x0C09);
			nameToID.Add ("en-CA", 0x1009);
			nameToID.Add ("en-NZ", 0x1409);
			nameToID.Add ("en-IE", 0x1809);
			nameToID.Add ("en-ZA", 0x1C09);
			nameToID.Add ("en-JM", 0x2009);
			nameToID.Add ("en-CB", 0x2409);
			nameToID.Add ("en-BZ", 0x2809);
			nameToID.Add ("en-TT", 0x2C09);
			nameToID.Add ("en-ZW", 0x3009);
			nameToID.Add ("en-PH", 0x3409);
			nameToID.Add ("es", 0x000A);
			nameToID.Add ("es-MX", 0x080A);
			nameToID.Add ("es-ES", 0x0C0A);
			nameToID.Add ("es-GT", 0x100A);
			nameToID.Add ("es-CR", 0x140A);
			nameToID.Add ("es-PA", 0x180A);
			nameToID.Add ("es-DO", 0x1C0A);
			nameToID.Add ("es-VE", 0x200A);
			nameToID.Add ("es-CO", 0x240A);
			nameToID.Add ("es-PE", 0x280A);
			nameToID.Add ("es-AR", 0x2C0A);
			nameToID.Add ("es-EC", 0x300A);
			nameToID.Add ("es-CL", 0x340A);
			nameToID.Add ("es-UY", 0x380A);
			nameToID.Add ("es-PY", 0x3C0A);
			nameToID.Add ("es-BO", 0x400A);
			nameToID.Add ("es-SV", 0x440A);
			nameToID.Add ("es-HN", 0x480A);
			nameToID.Add ("es-NI", 0x4C0A);
			nameToID.Add ("es-PR", 0x500A);
			nameToID.Add ("fi", 0x000B);
			nameToID.Add ("fi-FI", 0x040B);
			nameToID.Add ("fr", 0x000C);
			nameToID.Add ("fr-FR", 0x040C);
			nameToID.Add ("fr-BE", 0x080C);
			nameToID.Add ("fr-CA", 0x0C0C);
			nameToID.Add ("fr-CH", 0x100C);
			nameToID.Add ("fr-LU", 0x140C);
			nameToID.Add ("fr-MC", 0x180C);
			nameToID.Add ("he", 0x000D);
			nameToID.Add ("he-IL", 0x040D);
			nameToID.Add ("hu", 0x000E);
			nameToID.Add ("hu-HU", 0x040E);
			nameToID.Add ("is", 0x000F);
			nameToID.Add ("is-IS", 0x040F);
			nameToID.Add ("it", 0x0010);
			nameToID.Add ("it-IT", 0x0410);
			nameToID.Add ("it-CH", 0x0810);
			nameToID.Add ("ja", 0x0011);
			nameToID.Add ("ja-JP", 0x0411);
			nameToID.Add ("ko", 0x0012);
			nameToID.Add ("ko-KR", 0x0412);
			nameToID.Add ("nl", 0x0013);
			nameToID.Add ("nl-NL", 0x0413);
			nameToID.Add ("nl-BE", 0x0813);
			nameToID.Add ("no", 0x0014);
			nameToID.Add ("nb-NO", 0x0414);
			nameToID.Add ("nn-NO", 0x0814);
			nameToID.Add ("pl", 0x0015);
			nameToID.Add ("pl-PL", 0x0415);
			nameToID.Add ("pt", 0x0016);
			nameToID.Add ("pt-BR", 0x0416);
			nameToID.Add ("pt-PT", 0x0816);
			nameToID.Add ("ro", 0x0018);
			nameToID.Add ("ro-RO", 0x0418);
			nameToID.Add ("ru", 0x0019);
			nameToID.Add ("ru-RU", 0x0419);
			nameToID.Add ("hr", 0x001A);
			nameToID.Add ("hr-HR", 0x041A);
			nameToID.Add ("Lt-sr-SP", 0x081A);
			nameToID.Add ("Cy-sr-SP", 0x0C1A);
			nameToID.Add ("sk", 0x001B);
			nameToID.Add ("sk-SK", 0x041B);
			nameToID.Add ("sq", 0x001C);
			nameToID.Add ("sq-AL", 0x041C);
			nameToID.Add ("sv", 0x001D);
			nameToID.Add ("sv-SE", 0x041D);
			nameToID.Add ("sv-FI", 0x081D);
			nameToID.Add ("th", 0x001E);
			nameToID.Add ("th-TH", 0x041E);
			nameToID.Add ("tr", 0x001F);
			nameToID.Add ("tr-TR", 0x041F);
			nameToID.Add ("ur", 0x0020);
			nameToID.Add ("ur-PK", 0x0420);
			nameToID.Add ("id", 0x0021);
			nameToID.Add ("id-ID", 0x0421);
			nameToID.Add ("uk", 0x0022);
			nameToID.Add ("uk-UA", 0x0422);
			nameToID.Add ("be", 0x0023);
			nameToID.Add ("be-BY", 0x0423);
			nameToID.Add ("sl", 0x0024);
			nameToID.Add ("sl-SI", 0x0424);
			nameToID.Add ("et", 0x0025);
			nameToID.Add ("et-EE", 0x0425);
			nameToID.Add ("lv", 0x0026);
			nameToID.Add ("lv-LV", 0x0426);
			nameToID.Add ("lt", 0x0027);
			nameToID.Add ("lt-LT", 0x0427);
			nameToID.Add ("fa", 0x0029);
			nameToID.Add ("fa-IR", 0x0429);
			nameToID.Add ("vi", 0x002A);
			nameToID.Add ("vi-VN", 0x042A);
			nameToID.Add ("hy", 0x002B);
			nameToID.Add ("hy-AM", 0x042B);
			nameToID.Add ("az", 0x002C);
			nameToID.Add ("Lt-az-AZ", 0x042C);
			nameToID.Add ("Cy-az-AZ", 0x082C);
			nameToID.Add ("eu", 0x002D);
			nameToID.Add ("eu-ES", 0x042D);
			nameToID.Add ("mk", 0x002F);
			nameToID.Add ("mk-MK", 0x042F);
			nameToID.Add ("af", 0x0036);
			nameToID.Add ("af-ZA", 0x0436);
			nameToID.Add ("ka", 0x0037);
			nameToID.Add ("ka-GE", 0x0437);
			nameToID.Add ("fo", 0x0038);
			nameToID.Add ("fo-FO", 0x0438);
			nameToID.Add ("hi", 0x0039);
			nameToID.Add ("hi-IN", 0x0439);
			nameToID.Add ("ms", 0x003E);
			nameToID.Add ("ms-MY", 0x043E);
			nameToID.Add ("ms-BN", 0x083E);
			nameToID.Add ("kk", 0x003F);
			nameToID.Add ("kk-KZ", 0x043F);
			nameToID.Add ("ky", 0x0040);
			nameToID.Add ("ky-KZ", 0x0440);
			nameToID.Add ("sw", 0x0041);
			nameToID.Add ("sw-KE", 0x0441);
			nameToID.Add ("uz", 0x0043);
			nameToID.Add ("Lt-uz-UZ", 0x0443);
			nameToID.Add ("Cy-uz-UZ", 0x0843);
			nameToID.Add ("tt", 0x0044);
			nameToID.Add ("tt-TA", 0x0444);
			nameToID.Add ("pa", 0x0046);
			nameToID.Add ("pa-IN", 0x0446);
			nameToID.Add ("gu", 0x0047);
			nameToID.Add ("gu-IN", 0x0447);
			nameToID.Add ("ta", 0x0049);
			nameToID.Add ("ta-IN", 0x0449);
			nameToID.Add ("te", 0x004A);
			nameToID.Add ("te-IN", 0x044A);
			nameToID.Add ("kn", 0x004B);
			nameToID.Add ("kn-IN", 0x044B);
			nameToID.Add ("mr", 0x004E);
			nameToID.Add ("mr-IN", 0x044E);
			nameToID.Add ("sa", 0x004F);
			nameToID.Add ("sa-IN", 0x044F);
			nameToID.Add ("mn", 0x0050);
			nameToID.Add ("mn-MN", 0x0450);
			nameToID.Add ("gl", 0x0056);
			nameToID.Add ("gl-ES", 0x0456);
			nameToID.Add ("kok", 0x0057);
			nameToID.Add ("kok-IN", 0x0457);
			nameToID.Add ("syr", 0x005A);
			nameToID.Add ("syr-SY", 0x045A);
			nameToID.Add ("div", 0x0065);
			nameToID.Add ("div-MV", 0x0465);
			nameToID.Add ("", 0x007F);
		}

		//
		// Maps a name to a culture id
		//
		static int NameToID (string name)
		{
			if (nameToID == null) {
				lock (typeof (CultureInfo)) {
					if (nameToID == null)
						InitNameToID ();
				}
			}

			if (!nameToID.ContainsKey (name))
				return -1;

			return (int) nameToID [name];
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
			
			/* FIXME: the set method isnt listed in the spec */
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
			
			/* FIXME: the set method isnt listed in the spec */
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
				/* FIXME: This long switch should be
				 * moved into a hash, initialised in
				 * the static class constructor
				 */
				switch (lcid){
				case 0x007f:
					return "iv";
				case 0x0036:
					return "af";
				case 0x0436:
					return "af-ZA";
				case 0x001c:
					return "sq";
				case 0x041c:
					return "sq-AL";
				case 0x0001:
					return "ar";
				case 0x1401:
					return "ar-DZ";
				case 0x3c01:
					return "ar-BH";
				case 0x0c01:
					return "ar-EG";
				case 0x0801:
					return "ar-IQ";
				case 0x2c01:
					return "ar-JO";
				case 0x3401:
					return "ar-KW";
				case 0x3001:
					return "ar-LB";
				case 0x1001:
					return "ar-LY";
				case 0x1801:
					return "ar-MA";
				case 0x2001:
					return "ar-OM";
				case 0x4001:
					return "ar-QA";
				case 0x0401:
					return "ar-SA";
				case 0x2801:
					return "ar-SY";
				case 0x1c01:
					return "ar-TN";
				case 0x3801:
					return "ar-AE";
				case 0x2401:
					return "ar-YE";
				case 0x002b:
					return "hy";
				case 0x042b:
					return "hy-AM";
				case 0x002c:
					return "az";
				case 0x082c:
					return "Cy-az-AZ";
				case 0x042c:
					return "Lt-az-AZ";
				case 0x002d:
					return "eu";
				case 0x042d:
					return "eu-ES";
				case 0x0023:
					return "be";
				case 0x0423:
					return "be-BY";
				case 0x0002:
					return "bg";
				case 0x0402:
					return "bg-BG";
				case 0x0003:
					return "ca";
				case 0x0403:
					return "ca-ES";
				case 0x0c04:
					return "zh-HK";
				case 0x1404:
					return "zh-MO";
				case 0x0804:
					return "zh-CN";
				case 0x0004:
					return "zh-CHS";
				case 0x1004:
					return "zh-SG";
				case 0x0404:
					return "zh-TW";
				case 0x7c04:
					return "zh-CHT";
				case 0x001a:
					return "hr";
				case 0x041a:
					return "hr-HR";
				case 0x0005:
					return "cs";
				case 0x0405:
					return "cs-CZ";
				case 0x0006:
					return "da";
				case 0x0406:
					return "da-DK";
				case 0x0065:
					return "div";
				case 0x0465:
					return "div-MV";
				case 0x0013:
					return "nl";
				case 0x0813:
					return "nl-BE";
				case 0x0413:
					return "nl-NL";
				case 0x0009:
					return "en";
				case 0x0c09:
					return "en-AU";
				case 0x2809:
					return "en-BZ";
				case 0x1009:
					return "en-CA";
				case 0x2409:
					return "en-CB";
				case 0x1809:
					return "en-IE";
				case 0x2009:
					return "en-JM";
				case 0x1409:
					return "en-NZ";
				case 0x3409:
					return "en-PH";
				case 0x1c09:
					return "en-ZA";
				case 0x2c09:
					return "en-TT";
				case 0x0809:
					return "en-GB";
				case 0x0409:
					return "en-US";
				case 0x3009:
					return "en-ZW";
				case 0x0025:
					return "et";
				case 0x0425:
					return "et-EE";
				case 0x0038:
					return "fo";
				case 0x0438:
					return "fo-FO";
				case 0x0029:
					return "fa";
				case 0x0429:
					return "fa-IR";
				case 0x000b:
					return "fi";
				case 0x040b:
					return "fi-FI";
				case 0x000c:
					return "fr";
				case 0x080c:
					return "fr-BE";
				case 0x0c0c:
					return "fr-CA";
				case 0x040c:
					return "fr-FR";
				case 0x140c:
					return "fr-LU";
				case 0x180c:
					return "fr-MC";
				case 0x100c:
					return "fr-CH";
				case 0x0056:
					return "gl";
				case 0x0456:
					return "gl-ES";
				case 0x0037:
					return "ka";
				case 0x0437:
					return "ka-GE";
				case 0x0007:
					return "de";
				case 0x0c07:
					return "de-AT";
				case 0x0407:
					return "de-DE";
				case 0x1407:
					return "de-LI";
				case 0x1007:
					return "de-LU";
				case 0x0807:
					return "de-CH";
				case 0x0008:
					return "el";
				case 0x0408:
					return "el-GR";
				case 0x0047:
					return "gu";
				case 0x0447:
					return "gu-IN";
				case 0x000d:
					return "he";
				case 0x040d:
					return "he-IL";
				case 0x0039:
					return "hi";
				case 0x0439:
					return "hi-IN";
				case 0x000e:
					return "hu";
				case 0x040e:
					return "hu-HU";
				case 0x000f:
					return "is";
				case 0x040f:
					return "is-IS";
				case 0x0021:
					return "id";
				case 0x0421:
					return "id-ID";
				case 0x0010:
					return "it";
				case 0x0410:
					return "it-IT";
				case 0x0810:
					return "it-CH";
				case 0x0011:
					return "ja";
				case 0x0411:
					return "ja-JP";
				case 0x004b:
					return "kn";
				case 0x044b:
					return "kn-IN";
				case 0x003f:
					return "kk";
				case 0x043f:
					return "kk-KZ";
				case 0x0057:
					return "kok";
				case 0x0457:
					return "kok-IN";
				case 0x0012:
					return "ko";
				case 0x0412:
					return "ko-KR";
				case 0x0040:
					return "ky";
				case 0x0440:
					return "ky-KZ";
				case 0x0026:
					return "lv";
				case 0x0426:
					return "lv-LV";
				case 0x0027:
					return "lt";
				case 0x0427:
					return "lt-LT";
				case 0x002f:
					return "mk";
				case 0x042f:
					return "mk-MK";
				case 0x003e:
					return "ms";
				case 0x083e:
					return "ms-BN";
				case 0x043e:
					return "ms-MY";
				case 0x004e:
					return "mr";
				case 0x044e:
					return "mr-IN";
				case 0x0050:
					return "mn";
				case 0x0450:
					return "mn-MN";
				case 0x0014:
					return "no";
				case 0x0414:
					return "nb-NO";
				case 0x0814:
					return "nn-NO";
				case 0x0015:
					return "pl";
				case 0x0415:
					return "pl-PL";
				case 0x0016:
					return "pt";
				case 0x0416:
					return "pt-BR";
				case 0x0816:
					return "pt-PT";
				case 0x0046:
					return "pa";
				case 0x0446:
					return "pa-IN";
				case 0x0018:
					return "ro";
				case 0x0418:
					return "ro-RO";
				case 0x0019:
					return "ru";
				case 0x0419:
					return "ru-RU";
				case 0x004f:
					return "sa";
				case 0x044f:
					return "sa-IN";
				case 0x0c1a:
					return "Cy-sr-SP";
				case 0x081a:
					return "Lt-sr-SP";
				case 0x001b:
					return "sk";
				case 0x041b:
					return "sk-SK";
				case 0x0024:
					return "sl";
				case 0x0424:
					return "sl-SI";
				case 0x000a:
					return "es";
				case 0x2c0a:
					return "es-AR";
				case 0x400a:
					return "es-BO";
				case 0x340a:
					return "es-CL";
				case 0x240a:
					return "es-CO";
				case 0x140a:
					return "es-CR";
				case 0x1c0a:
					return "es-DO";
				case 0x300a:
					return "es-EC";
				case 0x440a:
					return "es-SV";
				case 0x100a:
					return "es-GT";
				case 0x480a:
					return "es-HN";
				case 0x080a:
					return "es-MX";
				case 0x4c0a:
					return "es-NI";
				case 0x180a:
					return "es-PA";
				case 0x3c0a:
					return "es-PY";
				case 0x280a:
					return "es-PE";
				case 0x500a:
					return "es-PR";
				case 0x0c0a:
					return "es-ES";
				case 0x380a:
					return "es-UY";
				case 0x200a:
					return "es-VE";
				case 0x0041:
					return "sw";
				case 0x0441:
					return "sw-KE";
				case 0x001d:
					return "sv";
				case 0x081d:
					return "sv-FI";
				case 0x041d:
					return "sv-SE";
				case 0x005a:
					return "syr";
				case 0x045a:
					return "syr-SY";
				case 0x0049:
					return "ta";
				case 0x0449:
					return "ta-IN";
				case 0x0044:
					return "tt";
				case 0x0444:
					return "tt-RU";
				case 0x004a:
					return "te";
				case 0x044a:
					return "te-IN";
				case 0x001e:
					return "th";
				case 0x041e:
					return "th-TH";
				case 0x001f:
					return "tr";
				case 0x041f:
					return "tr-TR";
				case 0x0022:
					return "uk";
				case 0x0422:
					return "uk-UA";
				case 0x0020:
					return "ur";
				case 0x0420:
					return "ur-PK";
				case 0x0043:
					return "uz";
				case 0x0843:
					return "Cy-uz-UZ";
				case 0x0443:
					return "Lt-uz-UZ";
				case 0x002a:
					return "vi";
				case 0x042a:
					return "vi-VN";
				}
				throw new Exception ("Miss constructed object for LCID: " + lcid);
			}
		}

		[MonoTODO]
		public virtual string NativeName
		{
			get {
				return("");
			}
		}
		

		[MonoTODO]
		public virtual Calendar Calendar
		{
			get { return null; }
		}

		[MonoTODO]
		public virtual Calendar[] OptionalCalendars
		{
			get {
				return(null);
			}
		}

		[MonoTODO]
		public virtual CultureInfo Parent
		{
			get {
				return(CultureInfo.InvariantCulture);
			}
		}

		[MonoTODO]
		public virtual TextInfo TextInfo
		{
			get {
				if (textinfo == null) 
					textinfo = new TextInfo ();
				return textinfo;
			}
		}

		[MonoTODO]
		public virtual string ThreeLetterISOLanguageName
		{
			get {
				return("");
			}
		}

		[MonoTODO]
		public virtual string ThreeLetterWindowsLanguageName
		{
			get {
				return("");
			}
		}

		[MonoTODO]
		public virtual string TwoLetterISOLanguageName
		{
			get {
				return("");
			}
		}

		public bool UseUserOverride
		{
			get {
				return use_user_override;
			}
		}

		[MonoTODO]
		public void ClearCachedData()
		{
		}

		[MonoTODO]
		public virtual object Clone()
		{
			return(null);
		}

		public override bool Equals (object value)
		{
			CultureInfo b = value as CultureInfo;
			if (b != null)
				return b.lcid == lcid;
			return false;
		}

		[MonoTODO]
		public static CultureInfo[] GetCultures(CultureTypes types)
		{
			return(null);
		}

		public override int GetHashCode()
		{
			return lcid;
		}

		[MonoTODO]
		public static CultureInfo ReadOnly(CultureInfo ci)
		{
			if(ci==null) {
				throw new ArgumentNullException("CultureInfo is null");
			}
			
			return(null);
		}

		public override string ToString()
		{
			return(this.Name);
		}
		
		
		[MonoTODO]
		public virtual CompareInfo CompareInfo
		{
			get {
				return null;
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

		[MonoTODO]
		public virtual string DisplayName
		{
			get {
				return("");
			}
		}

		[MonoTODO]
		public virtual string EnglishName
		{
			get {
				return("");
			}
		}

		[MonoTODO]
		public static CultureInfo InstalledUICulture
		{
			get {
				return(null);
			}
		}

		public bool IsReadOnly 
		{
			get {
				return(is_read_only);
			}
		}
		

		// 
		// IFormatProvider implementation
		//
		public virtual object GetFormat( Type formatType )
		{
			object format = null;

			if ( formatType == typeof(NumberFormatInfo) )
				format = NumberFormat;
			else if ( formatType == typeof(DateTimeFormatInfo) )
				format = DateTimeFormat;
			
			return format;
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
