//
// System.Globalization.CultureInfo
//
// Miguel de Icaza (miguel@ximian.com)
// Dick Porter (dick@ximian.com)
//
// (C) 2001, 2002, 2003 Ximian, Inc. (http://www.ximian.com)
//

using System.Collections;
using System.Threading;
using System.Runtime.CompilerServices;

namespace System.Globalization
{
	[Serializable]
	public class CultureInfo : ICloneable, IFormatProvider
	{
		static CultureInfo invariant_culture_info;
		bool is_read_only;
		int  lcid;
		bool use_user_override;
		NumberFormatInfo number_format;
		DateTimeFormatInfo datetime_format;
		TextInfo textinfo;

		private string name;
		private string displayname;
		private string englishname;
		private string nativename;
		private string iso3lang;
		private string iso2lang;
		private string icu_name;
		private string win3lang;
		private CompareInfo compareinfo;
		
		private static readonly string MSG_READONLY = "This instance is read only";
		
		private sealed class CultureMap : IEnumerable
		{
			private static Hashtable CultureID;
			private static Hashtable CultureNames;

			/* This is used so that foreach can enumerate
			 * the ID map hash without having to
			 * instantiate an instance itself
			 */
			public static CultureMap lcids;
			
			private string name;
			private string icu_name;
			private string win3lang;
			private int lcid;
			private int specific_lcid;
			private int parent_lcid;

			static CultureMap ()
			{
				CultureID=new Hashtable ();
				CultureNames=new Hashtable ();

				CultureMap map;
				
				/* Invariant */
				map=new CultureMap ("", "en_US_POSIX", "IVL", 0x007f, 0x007f, 0x007f);
				CultureID.Add (0x007f, map);
				CultureNames.Add ("", map);

				map=new CultureMap ("af", "af", "AFK", 0x0036, 0x0436, 0x007f);
				CultureID.Add (0x0036, map);
				CultureNames.Add ("af", map);

				map=new CultureMap ("af-ZA", "af_ZA", "AFK", 0x0436, 0x0436, 0x0036);
				CultureID.Add (0x0436, map);
				CultureNames.Add ("af-ZA", map);

				map=new CultureMap("sq", "sq", "SQI", 0x001c, 0x041c, 0x007f);
				CultureID.Add (0x001c, map);
				CultureNames.Add ("sq", map);
				
				map=new CultureMap("sq-AL", "sq_AL", "SQI", 0x041c, 0x041c, 0x001c);
				CultureID.Add (0x041c, map);
				CultureNames.Add ("sq-AL", map);
				
				map=new CultureMap("ar", "ar", "ARA", 0x0001, 0x0401, 0x007f);
				CultureID.Add (0x0001, map);
				CultureNames.Add ("ar", map);

				map=new CultureMap("ar-DZ", "ar_DZ", "ARG", 0x1401, 0x1401, 0x0001);
				CultureID.Add (0x1401, map);
				CultureNames.Add ("ar-DZ", map);

				map=new CultureMap("ar-BH", "ar_BH", "ARH", 0x3c01, 0x3c01, 0x0001);
				CultureID.Add (0x3c01, map);
				CultureNames.Add ("ar-BH", map);

				map=new CultureMap("ar-EG", "ar_EG", "ARE", 0x0c01, 0x0c01, 0x0001);
				CultureID.Add (0x0c01, map);
				CultureNames.Add ("ar-EG", map);

				map=new CultureMap("ar-IQ", "ar_IQ", "ARI", 0x0801, 0x0801, 0x0001);
				CultureID.Add (0x0801, map);
				CultureNames.Add ("ar-IQ", map);

				map=new CultureMap("ar-JO", "ar_JO", "ARJ", 0x2c01, 0x2c01, 0x0001);
				CultureID.Add (0x2c01, map);
				CultureNames.Add ("ar-JO", map);

				map=new CultureMap("ar-KW", "ar_KW", "ARK", 0x3401, 0x3401, 0x0001);
				CultureID.Add (0x3401, map);
				CultureNames.Add ("ar-KW", map);

				map=new CultureMap("ar-LB", "ar_LB", "ARB", 0x3001, 0x3001, 0x0001);
				CultureID.Add (0x3001, map);
				CultureNames.Add ("ar-LB", map);

				map=new CultureMap("ar-LY", "ar_LY", "ARL", 0x1001, 0x1001, 0x0001);
				CultureID.Add (0x1001, map);
				CultureNames.Add ("ar-LY", map);

				map=new CultureMap("ar-MA", "ar_MA", "ARM", 0x1801, 0x1801, 0x0001);
				CultureID.Add (0x1801, map);
				CultureNames.Add ("ar-MA", map);

				map=new CultureMap("ar-OM", "ar_OM", "ARO", 0x2001, 0x2001, 0x0001);
				CultureID.Add (0x2001, map);
				CultureNames.Add ("ar-OM", map);

				map=new CultureMap("ar-QA", "ar_QA", "ARQ", 0x4001, 0x4001, 0x0001);
				CultureID.Add (0x4001, map);
				CultureNames.Add ("ar-QA", map);

				map=new CultureMap("ar-SA", "ar_SA", "ARA", 0x0401, 0x0401, 0x0001);
				CultureID.Add (0x0401, map);
				CultureNames.Add ("ar-SA", map);

				map=new CultureMap("ar-SY", "ar_SY", "ARS", 0x2801, 0x2801, 0x0001);
				CultureID.Add (0x2801, map);
				CultureNames.Add ("ar-SY", map);

				map=new CultureMap("ar-TN", "ar_TN", "ART", 0x1c01, 0x1c01, 0x0001);
				CultureID.Add (0x1c01, map);
				CultureNames.Add ("ar-TN", map);

				map=new CultureMap("ar-AE", "ar_AE", "ARU", 0x3801, 0x3801, 0x0001);
				CultureID.Add (0x3801, map);
				CultureNames.Add ("ar-AE", map);

				map=new CultureMap("ar-YE", "ar_YE", "ARY", 0x2401, 0x2401, 0x0001);
				CultureID.Add (0x2401, map);
				CultureNames.Add ("ar-YE", map);

				map=new CultureMap("hy", "hy", "HYE", 0x002b, 0x042b, 0x007f);
				CultureID.Add (0x002b, map);
				CultureNames.Add ("hy", map);

				/* theres a _REVISED version of this one too */
				map=new CultureMap("hy-AM", "hy_AM", "HYE", 0x042b, 0x042b, 0x002b);
				CultureID.Add (0x042b, map);
				CultureNames.Add ("hy-AM", map);

				/* Azeri not supported
				   map=new CultureMap("az", "", "AZE", 0x002c, 0x042c, 0x007f);
				   CultureID.Add (0x002c, map);
				   CultureNames.Add ("az", map);

				   map=new CultureMap("Cy-az-AZ", "", "AZE", 0x082c, 0x082c, 0x002c);
				   CultureID.Add (0x082c, map);
				   CultureNames.Add ("Cy-az-AZ", map);

				   map=new CultureMap("Lt-az-AZ", "", "AZE", 0x042c, 0x042c);
				   CultureID.Add (0x042c, map);
				   CultureNames.Add ("Lt-az-AZ", map);*/

				map=new CultureMap("eu", "eu", "EUQ", 0x002d, 0x042d, 0x007f);
				CultureID.Add (0x002d, map);
				CultureNames.Add ("eu", map);

				map=new CultureMap("eu-ES", "eu_ES", "EUQ", 0x042d, 0x042d, 0x002d);
				CultureID.Add (0x042d, map);
				CultureNames.Add ("eu-ES", map);

				map=new CultureMap("be", "be", "BEL", 0x0023, 0x0423, 0x007f);
				CultureID.Add (0x0023, map);
				CultureNames.Add ("be", map);

				map=new CultureMap("be-BY", "be_BY", "BEL", 0x0423, 0x0423, 0x0023);
				CultureID.Add (0x0423, map);
				CultureNames.Add ("be-BY", map);

				map=new CultureMap("bg", "bg", "BGR", 0x0002, 0x0402, 0x007f);
				CultureID.Add (0x0002, map);
				CultureNames.Add ("bg", map);

				map=new CultureMap("bg-BG", "bg_BG", "BGR", 0x0402, 0x0402, 0x0002);
				CultureID.Add (0x0402, map);
				CultureNames.Add ("bg-BG", map);

				map=new CultureMap("ca", "ca", "CAT", 0x0003, 0x0403, 0x007f);
				CultureID.Add (0x0003, map);
				CultureNames.Add ("ca", map);

				map=new CultureMap("ca-ES", "ca_ES", "CAT", 0x0403, 0x0403, 0x0003);
				CultureID.Add (0x0403, map);
				CultureNames.Add ("ca-ES", map);

				map=new CultureMap("zh-HK", "zh_HK", "ZHH", 0x0c04, 0x0c04, 0x7c04);
				CultureID.Add (0x0c04, map);
				CultureNames.Add ("zh-HK", map);

				map=new CultureMap("zh-MO", "zh_MO", "ZHM", 0x1404, 0x1404, 0x0004);
				CultureID.Add (0x1404, map);
				CultureNames.Add ("zh-MO", map);

				map=new CultureMap("zh-CN", "zh_CN", "CHS", 0x0804, 0x0804, 0x0004);
				CultureID.Add (0x0804, map);
				CultureNames.Add ("zh-CN", map);

				/* zh-CHS (Chinese simplified). 'zh' is LCID 0x0004 in ICU */
				map=new CultureMap("zh-CHS", "zh", "CHS", 0x0004, 0x0000, 0x007f);
				CultureID.Add (0x0004, map);
				CultureNames.Add ("zh-CHS", map);

				map=new CultureMap("zh-SG", "zh_SG", "ZHI", 0x1004, 0x1004, 0x0004);
				CultureID.Add (0x1004, map);
				CultureNames.Add ("zh-SG", map);
				
				map=new CultureMap("zh-TW", "zh_TW", "CHT", 0x0404, 0x0404, 0x7c04);
				CultureID.Add (0x0404, map);
				CultureNames.Add ("zh-TW", map);

				/* zh-CHT (Chinese traditional).  Maybe set this to zh_TW? (politics, politics...) */
				map=new CultureMap("zh-CHT", "zh_TW", "CHT", 0x7c04, 0x0000, 0x007f);
				CultureID.Add (0x7c04, map);
				CultureNames.Add ("zh-CHT", map);

				map=new CultureMap("hr", "hr", "HRV", 0x001a, 0x041a, 0x007f);
				CultureID.Add (0x001a, map);
				CultureNames.Add ("hr", map);

				map=new CultureMap("hr-HR", "hr_HR", "HRV", 0x041a, 0x041a, 0x001a);
				CultureID.Add (0x041a, map);
				CultureNames.Add ("hr-HR", map);

				map=new CultureMap("cs", "cs", "CSY", 0x0005, 0x0405, 0x007f);
				CultureID.Add (0x0005, map);
				CultureNames.Add ("cs", map);

				map=new CultureMap("cs-CZ", "cs_CZ", "CSY", 0x0405, 0x0405, 0x0005);
				CultureID.Add (0x0405, map);
				CultureNames.Add ("cs-CZ", map);

				map=new CultureMap("da", "da", "DAN", 0x0006, 0x0406, 0x007f);
				CultureID.Add (0x0006, map);
				CultureNames.Add ("da", map);

				map=new CultureMap("da-DK", "da_DK", "DAN", 0x0406, 0x0406, 0x0006);
				CultureID.Add (0x0406, map);
				CultureNames.Add ("da-DK", map);

				/* Dhivehi not supported
				   map=new CultureMap("div", "", "DIV", 0x0065, 0x0465, 0x007f);
				   CultureID.Add (0x0065, map);
				   CultureNames.Add ("div", map);

				   map=new CultureMap("div-MV", "", "DIV", 0x0465, 0x0465, 0x0065);
				   CultureID.Add (0x0465, map);
				   CultureNames.Add ("div-MV", map);*/

				map=new CultureMap("nl", "nl", "NLD", 0x0013, 0x0413, 0x007f);
				CultureID.Add (0x0013, map);
				CultureNames.Add ("nl", map);

				map=new CultureMap("nl-BE", "nl_BE", "NLB", 0x0813, 0x0813, 0x0013);
				CultureID.Add (0x0813, map);
				CultureNames.Add ("nl-BE", map);

				map=new CultureMap("nl-NL", "nl_NL", "NLD", 0x0413, 0x0413, 0x0013);
				CultureID.Add (0x0413, map);
				CultureNames.Add ("nl-NL", map);

				map=new CultureMap("en", "en", "ENU", 0x0009, 0x0409, 0x007f);
				CultureID.Add (0x0009, map);
				CultureNames.Add ("en", map);

				map=new CultureMap("en-AU", "en_AU", "ENA", 0x0c09, 0x0c09, 0x0009);
				CultureID.Add (0x0c09, map);
				CultureNames.Add ("en-AU", map);

				/* Falls back to 'en' currently */
				map=new CultureMap("en-BZ", "en_BZ", "ENL", 0x2809, 0x2809, 0x0009);
				CultureID.Add (0x2809, map);
				CultureNames.Add ("en-BZ", map);

				map=new CultureMap("en-CA", "en_CA", "ENC", 0x1009, 0x1009, 0x0009);
				CultureID.Add (0x1009, map);
				CultureNames.Add ("en-CA", map);

				/* ms calls this en-CB but ICU has this ID as Virgin Islands */
				map=new CultureMap("en-CB", "en_VI", "ENB", 0x2409, 0x2409, 0x0009);
				CultureID.Add (0x2409, map);
				CultureNames.Add ("en-CB", map);

				map=new CultureMap("en-IE", "en_IE", "ENI", 0x1809, 0x1809, 0x0009);
				CultureID.Add (0x1809, map);
				CultureNames.Add ("en-IE", map);

				/* Falls back to 'en' currently */
				map=new CultureMap("en-JM", "en_JM", "ENJ", 0x2009, 0x2009, 0x0009);
				CultureID.Add (0x2009, map);
				CultureNames.Add ("en-JM", map);

				map=new CultureMap("en-NZ", "en_NZ", "ENZ", 0x1409, 0x1409, 0x0009);
				CultureID.Add (0x1409, map);
				CultureNames.Add ("en-NZ", map);

				map=new CultureMap("en-PH", "en_PH", "ENP", 0x3409, 0x3409, 0x0009);
				CultureID.Add (0x3409, map);
				CultureNames.Add ("en-PH", map);

				map=new CultureMap("en-ZA", "en_ZA", "ENS", 0x1c09, 0x1c09, 0x0009);
				CultureID.Add (0x1c09, map);
				CultureNames.Add ("en-ZA", map);

				/* Falls back to 'en' currently */
				map=new CultureMap("en-TT", "en_TT", "ENT", 0x2c09, 0x2c09, 0x0009);
				CultureID.Add (0x2c09, map);
				CultureNames.Add ("en-TT", map);

				map=new CultureMap("en-GB", "en_GB", "ENG", 0x0809, 0x0809, 0x0009);
				CultureID.Add (0x0809, map);
				CultureNames.Add ("en-GB", map);

				map=new CultureMap("en-US", "en_US", "ENU", 0x0409, 0x0409, 0x0009);
				CultureID.Add (0x0409, map);
				CultureNames.Add ("en-US", map);

				map=new CultureMap("en-ZW", "en_ZW", "ENW", 0x3009, 0x3009, 0x0009);
				CultureID.Add (0x3009, map);
				CultureNames.Add ("en-ZW", map);

				map=new CultureMap("et", "et", "ETI", 0x0025, 0x0425, 0x007f);
				CultureID.Add (0x0025, map);
				CultureNames.Add ("et", map);

				map=new CultureMap("et-EE", "et_EE", "ETI", 0x0425, 0x0425, 0x0025);
				CultureID.Add (0x0425, map);
				CultureNames.Add ("et-EE", map);

				map=new CultureMap("fo", "fo", "FOS", 0x0038, 0x0438, 0x007f);
				CultureID.Add (0x0038, map);
				CultureNames.Add ("fo", map);

				map=new CultureMap("fo-FO", "fo_FO", "FOS", 0x0438, 0x0438, 0x0038);
				CultureID.Add (0x0438, map);
				CultureNames.Add ("fo-FO", map);

				map=new CultureMap("fa", "fa", "FAR", 0x0029, 0x0429, 0x007f);
				CultureID.Add (0x0029, map);
				CultureNames.Add ("fa", map);

				map=new CultureMap("fa-IR", "fa_IR", "FAR", 0x0429, 0x0429, 0x0029);
				CultureID.Add (0x0429, map);
				CultureNames.Add ("fa-IR", map);

				map=new CultureMap("fi", "fi", "FIN", 0x000b, 0x040b, 0x007f);
				CultureID.Add (0x000b, map);
				CultureNames.Add ("fi", map);

				map=new CultureMap("fi-FI", "fi_FI", "FIN", 0x040b, 0x040b, 0x000b);
				CultureID.Add (0x040b, map);
				CultureNames.Add ("fi-FI", map);

				map=new CultureMap("fr", "fr", "FRA", 0x000c, 0x040c, 0x007f);
				CultureID.Add (0x000c, map);
				CultureNames.Add ("fr", map);

				map=new CultureMap("fr-BE", "fr_BE", "FRB", 0x080c, 0x080c, 0x000c);
				CultureID.Add (0x080c, map);
				CultureNames.Add ("fr-BE", map);

				map=new CultureMap("fr-CA", "fr_CA", "FRC", 0x0c0c, 0x0c0c, 0x000c);
				CultureID.Add (0x0c0c, map);
				CultureNames.Add ("fr-CA", map);

				map=new CultureMap("fr-FR", "fr_FR", "FRA", 0x040c, 0x040c, 0x000c);
				CultureID.Add (0x040c, map);
				CultureNames.Add ("fr-FR", map);

				map=new CultureMap("fr-LU", "fr_LU", "FRL", 0x140c, 0x140c, 0x000c);
				CultureID.Add (0x140c, map);
				CultureNames.Add ("fr-LU", map);

				/* Falls back to 'fr' currently */
				map=new CultureMap("fr-MC", "fr_MC", "FRM", 0x180c, 0x180c, 0x000c);
				CultureID.Add (0x180c, map);
				CultureNames.Add ("fr-MC", map);

				map=new CultureMap("fr-CH", "fr_CH", "FRS", 0x100c, 0x100c, 0x000c);
				CultureID.Add (0x100c, map);
				CultureNames.Add ("fr-CH", map);

				map=new CultureMap("gl", "gl", "GLC", 0x0056, 0x0456, 0x007f);
				CultureID.Add (0x0056, map);
				CultureNames.Add ("gl", map);

				map=new CultureMap("gl-ES", "gl_ES", "GLC", 0x0456, 0x0456, 0x0056);
				CultureID.Add (0x0456, map);
				CultureNames.Add ("gl-ES", map);

				/* Georgian not supported
				   map=new CultureMap("ka", "", "KAT", 0x0037, 0x0437, 0x007f);
				   CultureID.Add (0x0037, map);
				   CultureNames.Add ("ka", map);

				   map=new CultureMap("ka-GE", "", "KAT", 0x0437, 0x0437, 0x0037);
				   CultureID.Add (0x0437, map);
				   CultureNames.Add ("ka-GE", map);*/

				map=new CultureMap("de", "de", "DEU", 0x0007, 0x0407, 0x007f);
				CultureID.Add (0x0007, map);
				CultureNames.Add ("de", map);

				map=new CultureMap("de-AT", "de_AT", "DEA", 0x0c07, 0x0c07, 0x0007);
				CultureID.Add (0x0c07, map);
				CultureNames.Add ("de-AT", map);

				map=new CultureMap("de-DE", "de_DE", "DEU", 0x0407, 0x0407, 0x0007);
				CultureID.Add (0x0407, map);
				CultureNames.Add ("de-DE", map);

				/* Falls back to 'de' currently */
				map=new CultureMap("de-LI", "de_LI", "DEC", 0x1407, 0x1407, 0x0007);
				CultureID.Add (0x1407, map);
				CultureNames.Add ("de-LI", map);

				map=new CultureMap("de-LU", "de_LU", "DEL", 0x1007, 0x1007, 0x0007);
				CultureID.Add (0x1007, map);
				CultureNames.Add ("de-LU", map);

				map=new CultureMap("de-CH", "de_CH", "DES", 0x0807, 0x0807, 0x0007);
				CultureID.Add (0x0807, map);
				CultureNames.Add ("de-CH", map);

				map=new CultureMap("el", "el", "ELL", 0x0008, 0x0408, 0x007f);
				CultureID.Add (0x0008, map);
				CultureNames.Add ("el", map);

				map=new CultureMap("el-GR", "el_GR", "ELL", 0x0408, 0x0408, 0x0008);
				CultureID.Add (0x0408, map);
				CultureNames.Add ("el-GR", map);

				map=new CultureMap("gu", "gu", "GUJ", 0x0047, 0x0447, 0x007f);
				CultureID.Add (0x0047, map);
				CultureNames.Add ("gu", map);

				map=new CultureMap("gu-IN", "gu_IN", "GUJ", 0x0447, 0x0447, 0x0047);
				CultureID.Add (0x0447, map);
				CultureNames.Add ("gu-IN", map);

				map=new CultureMap("he", "he", "HEB", 0x000d, 0x040d, 0x007f);
				CultureID.Add (0x000d, map);
				CultureNames.Add ("he", map);

				map=new CultureMap("he-IL", "he_IL", "HEB", 0x040d, 0x040d, 0x000d);
				CultureID.Add (0x040d, map);
				CultureNames.Add ("he-IL", map);

				map=new CultureMap("hi", "hi", "HIN", 0x0039, 0x0439, 0x007f);
				CultureID.Add (0x0039, map);
				CultureNames.Add ("hi", map);

				map=new CultureMap("hi-IN", "hi_IN", "HIN", 0x0439, 0x0439, 0x0039);
				CultureID.Add (0x0439, map);
				CultureNames.Add ("hi-IN", map);

				map=new CultureMap("hu", "hu", "HUN", 0x000e, 0x040e, 0x007f);
				CultureID.Add (0x000e, map);
				CultureNames.Add ("hu", map);

				map=new CultureMap("hu-HU", "hu_HU", "HUN", 0x040e, 0x040e, 0x000e);
				CultureID.Add (0x040e, map);
				CultureNames.Add ("hu-HU", map);

				map=new CultureMap("is", "is", "ISL", 0x000f, 0x040f, 0x007f);
				CultureID.Add (0x000f, map);
				CultureNames.Add ("is", map);

				map=new CultureMap("is-IS", "is_IS", "ISL", 0x040f, 0x040f, 0x000f);
				CultureID.Add (0x040f, map);
				CultureNames.Add ("is-IS", map);

				map=new CultureMap("id", "id", "IND", 0x0021, 0x0421, 0x007f);
				CultureID.Add (0x0021, map);
				CultureNames.Add ("id", map);

				map=new CultureMap("id-ID", "id_ID", "IND", 0x0421, 0x0421, 0x0021);
				CultureID.Add (0x0421, map);
				CultureNames.Add ("id-ID", map);

				map=new CultureMap("it", "it", "ITA", 0x0010, 0x0410, 0x007f);
				CultureID.Add (0x0010, map);
				CultureNames.Add ("it", map);

				map=new CultureMap("it-IT", "it_IT", "ITA", 0x0410, 0x0410, 0x0010);
				CultureID.Add (0x0410, map);
				CultureNames.Add ("it-IT", map);

				map=new CultureMap("it-CH", "it_CH", "ITS", 0x0810, 0x0810, 0x0010);
				CultureID.Add (0x0810, map);
				CultureNames.Add ("it-CH", map);

				map=new CultureMap("ja", "ja", "JPN", 0x0011, 0x0411, 0x007f);
				CultureID.Add (0x0011, map);
				CultureNames.Add ("ja", map);

				map=new CultureMap("ja-JP", "ja_JP", "JPN", 0x0411, 0x0411, 0x0011);
				CultureID.Add (0x0411, map);
				CultureNames.Add ("ja-JP", map);

				map=new CultureMap("kn", "kn", "KAN", 0x004b, 0x044b, 0x007f);
				CultureID.Add (0x004b, map);
				CultureNames.Add ("kn", map);

				map=new CultureMap("kn-IN", "kn_IN", "KAN", 0x044b, 0x044b, 0x004b);
				CultureID.Add (0x044b, map);
				CultureNames.Add ("kn-IN", map);

				/* Kazakh not supported
				   map=new CultureMap("kk", "kk", "KKZ", 0x003f, 0x043f, 0x007f);
				   CultureID.Add (0x003f, map);
				   CultureNames.Add ("kk", map);

				   map=new CultureMap("kk-KZ", "kk-KZ", "KKZ", 0x043f, 0x043f, 0x003f);
				   CultureID.Add (0x043f, map);
				   CultureNames.Add ("kk-KZ", map);*/

				map=new CultureMap("kok", "kok", "KNK", 0x0057, 0x0457, 0x007f);
				CultureID.Add (0x0057, map);
				CultureNames.Add ("kok", map);

				map=new CultureMap("kok-IN", "kok_IN", "KNK", 0x0457, 0x0457, 0x0057);
				CultureID.Add (0x0457, map);
				CultureNames.Add ("kok-IN", map);

				map=new CultureMap("ko", "ko", "KOR", 0x0012, 0x0412, 0x007f);
				CultureID.Add (0x0012, map);
				CultureNames.Add ("ko", map);

				map=new CultureMap("ko-KR", "ko_KR", "KOR", 0x0412, 0x0412, 0x0012);
				CultureID.Add (0x0412, map);
				CultureNames.Add ("ko-KR", map);

				/* Kyrgyz not supported
				   map=new CultureMap("ky", "ky", "KYR", 0x0040, 0x0440, 0x007f);
				   CultureID.Add (0x0040, map);
				   CultureNames.Add ("ky", map);

				   map=new CultureMap("ky-KZ", "ky-KZ", "KYR", 0x0440, 0x0440, 0x0040);
				   CultureID.Add (0x0440, map);
				   CultureNames.Add ("ky-KZ", map);*/

				map=new CultureMap("lv", "lv", "LVI", 0x0026, 0x0426, 0x007f);
				CultureID.Add (0x0026, map);
				CultureNames.Add ("lv", map);

				map=new CultureMap("lv-LV", "lv_LV", "LVI", 0x0426, 0x0426, 0x0026);
				CultureID.Add (0x0426, map);
				CultureNames.Add ("lv-LV", map);

				map=new CultureMap("lt", "lt", "LTH", 0x0027, 0x0427, 0x007f);
				CultureID.Add (0x0027, map);
				CultureNames.Add ("lt", map);

				map=new CultureMap("lt-LT", "lt_LT", "LTH", 0x0427, 0x0427, 0x0027);
				CultureID.Add (0x0427, map);
				CultureNames.Add ("lt-LT", map);

				map=new CultureMap("mk", "mk", "MKI", 0x002f, 0x042f, 0x007f);
				CultureID.Add (0x002f, map);
				CultureNames.Add ("mk", map);

				map=new CultureMap("mk-MK", "mk_MK", "MKI", 0x042f, 0x042f, 0x002f);
				CultureID.Add (0x042f, map);
				CultureNames.Add ("mk-MK", map);

				/* Malay not supported
				   map=new CultureMap("ms", "ms", "MSL", 0x003e, 0x043e, 0x007f);
				   CultureID.Add (0x003e, map);
				   CultureNames.Add ("ms", map);

				   map=new CultureMap("ms-BN", "ms-BN", "MSB", 0x083e, 0x083e, 0x003e);
				   CultureID.Add (0x083e, map);
				   CultureNames.Add ("ms-BN", map);

				   map=new CultureMap("ms-MY", "ms-MY", "MSL", 0x043e, 0x043e, 0x003e);
				   CultureID.Add (0x043e, map);
				   CultureNames.Add ("ms-MY", map);*/

				map=new CultureMap("mr", "mr", "MAR", 0x004e, 0x044e, 0x007f);
				CultureID.Add (0x004e, map);
				CultureNames.Add ("mr", map);

				map=new CultureMap("mr-IN", "mr_IN", "MAR", 0x044e, 0x044e, 0x004e);
				CultureID.Add (0x044e, map);
				CultureNames.Add ("mr-IN", map);

				/* Mongolian not supported
				   map=new CultureMap("mn", "mn", "MON", 0x0050, 0x0450, 0x007f);
				   CultureID.Add (0x0050, map);
				   CultureNames.Add ("mn", map);

				   map=new CultureMap("mn-MN", "mn-MN", "MON", 0x0450, 0x0450, 0x0050);
				   CultureID.Add (0x0450, map);
				   CultureNames.Add ("mn-MN", map);*/

				map=new CultureMap("no", "no", "NOR", 0x0014, 0x0414, 0x007f);
				CultureID.Add (0x0014, map);
				CultureNames.Add ("no", map);

				map=new CultureMap("nb-NO", "nb_NO", "NOR", 0x0414, 0x0414, 0x0014);
				CultureID.Add (0x0414, map);
				CultureNames.Add ("nb-NO", map);

				map=new CultureMap("nn-NO", "nn_NO", "NON", 0x0814, 0x0814, 0x0014);
				CultureID.Add (0x0814, map);
				CultureNames.Add ("nn-NO", map);

				map=new CultureMap("pl", "pl", "PLK", 0x0015, 0x0415, 0x007f);
				CultureID.Add (0x0015, map);
				CultureNames.Add ("pl", map);

				map=new CultureMap("pl-PL", "pl_PL", "PLK", 0x0415, 0x0415, 0x0015);
				CultureID.Add (0x0415, map);
				CultureNames.Add ("pl-PL", map);

				map=new CultureMap("pt", "pt", "PTB", 0x0016, 0x0416, 0x007f);
				CultureID.Add (0x0016, map);
				CultureNames.Add ("pt", map);

				map=new CultureMap("pt-BR", "pt_BR", "PTB", 0x0416, 0x0416, 0x0016);
				CultureID.Add (0x0416, map);
				CultureNames.Add ("pt-BR", map);
				
				map=new CultureMap("pt-PT", "pt_PT", "PTG", 0x0816, 0x0816, 0x0016);
				CultureID.Add (0x0816, map);
				CultureNames.Add ("pt-PT", map);

				/* Punjabi not supported
				   map=new CultureMap("pa", "pa", "PAN", 0x0046, 0x0446, 0x007f);
				   CultureID.Add (0x0046, map);
				   CultureNames.Add ("pa", map);

				   map=new CultureMap("pa-IN", "pa-IN", "PAN", 0x0446, 0x0446, 0x0046);
				   CultureID.Add (0x0446, map);
				   CultureNames.Add ("pa-IN", map);*/

				map=new CultureMap("ro", "ro", "ROM", 0x0018, 0x0418, 0x007f);
				CultureID.Add (0x0018, map);
				CultureNames.Add ("ro", map);

				map=new CultureMap("ro-RO", "ro_RO", "ROM", 0x0418, 0x0418, 0x0018);
				CultureID.Add (0x0418, map);
				CultureNames.Add ("ro-RO", map);

				map=new CultureMap("ru", "ru", "RUS", 0x0019, 0x0419, 0x007f);
				CultureID.Add (0x0019, map);
				CultureNames.Add ("ru", map);

				map=new CultureMap("ru-RU", "ru_RU", "RUS", 0x0419, 0x0419, 0x0019);
				CultureID.Add (0x0419, map);
				CultureNames.Add ("ru-RU", map);

				/* Sanskrit not supported
				   map=new CultureMap("sa", "sa", "SAN", 0x004f, 0x044f, 0x007f);
				   CultureID.Add (0x004f, map);
				   CultureNames.Add ("sa", map);

				   map=new CultureMap("sa-IN", "sa-IN", "SAN", 0x044f, 0x044f, 0x004f);
				   CultureID.Add (0x044f, map);
				   CultureNames.Add ("sa-IN", map);*/

				map=new CultureMap("Cy-sr-SP", "sr", "SRB", 0x0c1a, 0x0c1a, 0x001a);
				CultureID.Add (0x0c1a, map);
				CultureNames.Add ("Cy-sr-SP", map);

				map=new CultureMap("Lt-sr-SP", "sh", "SRL", 0x081a, 0x081a, 0x001a);
				CultureID.Add (0x081a, map);
				CultureNames.Add ("Lt-sr-SP", map);

				map=new CultureMap("sk", "sk", "SKY", 0x001b, 0x041b, 0x007f);
				CultureID.Add (0x001b, map);
				CultureNames.Add ("sk", map);

				map=new CultureMap("sk-SK", "sk_SK", "SKY", 0x041b, 0x041b, 0x001b);
				CultureID.Add (0x041b, map);
				CultureNames.Add ("sk-SK", map);

				map=new CultureMap("sl", "sl", "SLV", 0x0024, 0x0424, 0x007f);
				CultureID.Add (0x0024, map);
				CultureNames.Add ("sl", map);

				map=new CultureMap("sl-SI", "sl_SI", "SLV", 0x0424, 0x0424, 0x0024);
				CultureID.Add (0x0424, map);
				CultureNames.Add ("sl-SI", map);

				map=new CultureMap("es", "es", "ESP", 0x000a, 0x0c0a, 0x007f);
				CultureID.Add (0x000a, map);
				CultureNames.Add ("es", map);

				map=new CultureMap("es-AR", "es_AR", "ESS", 0x2c0a, 0x2c0a, 0x000a);
				CultureID.Add (0x2c0a, map);
				CultureNames.Add ("es-AR", map);

				map=new CultureMap("es-BO", "es_BO", "ESB", 0x400a, 0x400a, 0x000a);
				CultureID.Add (0x400a, map);
				CultureNames.Add ("es-BO", map);

				map=new CultureMap("es-CL", "es_CL", "ESL", 0x340a, 0x340a, 0x000a);
				CultureID.Add (0x340a, map);
				CultureNames.Add ("es-CL", map);

				map=new CultureMap("es-CO", "es_CO", "ESO", 0x240a, 0x240a, 0x000a);
				CultureID.Add (0x240a, map);
				CultureNames.Add ("es-CO", map);

				map=new CultureMap("es-CR", "es_CR", "ESC", 0x140a, 0x140a, 0x000a);
				CultureID.Add (0x140a, map);
				CultureNames.Add ("es-CR", map);

				map=new CultureMap("es-DO", "es_DO", "ESD", 0x1c0a, 0x1c0a, 0x000a);
				CultureID.Add (0x1c0a, map);
				CultureNames.Add ("es-DO", map);

				map=new CultureMap("es-EC", "es_EC", "ESF", 0x300a, 0x300a, 0x000a);
				CultureID.Add (0x300a, map);
				CultureNames.Add ("es-EC", map);

				map=new CultureMap("es-SV", "es_SV", "ESE", 0x440a, 0x440a, 0x000a);
				CultureID.Add (0x440a, map);
				CultureNames.Add ("es-SV", map);

				map=new CultureMap("es-GT", "es_GT", "ESG", 0x100a, 0x100a, 0x000a);
				CultureID.Add (0x100a, map);
				CultureNames.Add ("es-GT", map);

				map=new CultureMap("es-HN", "es_HN", "ESH", 0x480a, 0x480a, 0x000a);
				CultureID.Add (0x480a, map);
				CultureNames.Add ("es-HN", map);
				
				map=new CultureMap("es-MX", "es_MX", "ESM", 0x080a, 0x080a, 0x000a);
				CultureID.Add (0x080a, map);
				CultureNames.Add ("es-MX", map);

				map=new CultureMap("es-NI", "es_NI", "ESI", 0x4c0a, 0x4c0a, 0x000a);
				CultureID.Add (0x4c0a, map);
				CultureNames.Add ("es-NI", map);

				map=new CultureMap("es-PA", "es_PA", "ESA", 0x180a, 0x180a, 0x000a);
				CultureID.Add (0x180a, map);
				CultureNames.Add ("es-PA", map);

				map=new CultureMap("es-PY", "es_PY", "ESZ", 0x3c0a, 0x3c0a, 0x000a);
				CultureID.Add (0x3c0a, map);
				CultureNames.Add ("es-PY", map);

				map=new CultureMap("es-PE", "es_PE", "ESR", 0x280a, 0x280a, 0x000a);
				CultureID.Add (0x280a, map);
				CultureNames.Add ("es-PE", map);

				map=new CultureMap("es-PR", "es_PR", "ESU", 0x500a, 0x500a, 0x000a);
				CultureID.Add (0x500a, map);
				CultureNames.Add ("es-PR", map);

				map=new CultureMap("es-ES", "es_ES", "ESN", 0x0c0a, 0x0c0a, 0x000a);
				CultureID.Add (0x0c0a, map);
				CultureNames.Add ("es-ES", map);

				map=new CultureMap("es-UY", "es_UY", "ESY", 0x380a, 0x380a, 0x000a);
				CultureID.Add (0x380a, map);
				CultureNames.Add ("es-UY", map);

				map=new CultureMap("es-VE", "es_VE", "ESV", 0x200a, 0x200a, 0x000a);
				CultureID.Add (0x200a, map);
				CultureNames.Add ("es-VE", map);

				map=new CultureMap("sw", "sw", "SWK", 0x0041, 0x0441, 0x007f);
				CultureID.Add (0x0041, map);
				CultureNames.Add ("sw", map);

				map=new CultureMap("sw-KE", "sw_KE", "SWK", 0x0441, 0x0441, 0x0041);
				CultureID.Add (0x0441, map);
				CultureNames.Add ("sw-KE", map);

				map=new CultureMap("sv", "sv", "SVE", 0x001d, 0x041d, 0x007f);
				CultureID.Add (0x001d, map);
				CultureNames.Add ("sv", map);

				map=new CultureMap("sv-FI", "sv_FI", "SVF", 0x081d, 0x081d, 0x001d);
				CultureID.Add (0x081d, map);
				CultureNames.Add ("sv-FI", map);

				map=new CultureMap("sv-SE", "sv_SE", "SVE", 0x041d, 0x041d, 0x001d);
				CultureID.Add (0x041d, map);
				CultureNames.Add ("sv-SE", map);

				/* Syriac not supported
				   map=new CultureMap("syr", "syr", "SYR", 0x005a, 0x045a, 0x007f);
				   CultureID.Add (0x005a, map);
				   CultureNames.Add ("syr", map);

				   map=new CultureMap("syr-SY", "syr-SY", "SYR", 0x045a, 0x045a, 0x005a);
				   CultureID.Add (0x045a, map);
				   CultureNames.Add ("syr-SY", map);*/

				map=new CultureMap("ta", "ta", "TAM", 0x0049, 0x0449, 0x007f);
				CultureID.Add (0x0049, map);
				CultureNames.Add ("ta", map);

				map=new CultureMap("ta-IN", "ta_IN", "TAM", 0x0449, 0x0449, 0x0049);
				CultureID.Add (0x0449, map);
				CultureNames.Add ("ta-IN", map);

				/* Tatar not supported
				   map=new CultureMap("tt", "tt", "TTT", 0x0044, 0x0444, 0x007f);
				   CultureID.Add (0x0044, map);
				   CultureNames.Add ("tt", map);

				   map=new CultureMap("tt-RU", "tt-RU", "TTT", 0x0444, 0x0444, 0x0044);
				   CultureID.Add (0x0444, map);
				   CultureNames.Add ("tt-RU", map);*/

				map=new CultureMap("te", "te", "TEL", 0x004a, 0x044a, 0x007f);
				CultureID.Add (0x004a, map);
				CultureNames.Add ("te", map);

				map=new CultureMap("te-IN", "te_IN", "TEL", 0x044a, 0x044a, 0x004a);
				CultureID.Add (0x044a, map);
				CultureNames.Add ("te-IN", map);

				map=new CultureMap("th", "th", "THA", 0x001e, 0x041e, 0x007f);
				CultureID.Add (0x001e, map);
				CultureNames.Add ("th", map);

				/* _TRADITIONAL variant too */
				map=new CultureMap("th-TH", "th_TH", "THA", 0x041e, 0x041e, 0x001e);
				CultureID.Add (0x041e, map);
				CultureNames.Add ("th-TH", map);

				map=new CultureMap("tr", "tr", "TRK", 0x001f, 0x041f, 0x007f);
				CultureID.Add (0x001f, map);
				CultureNames.Add ("tr", map);

				map=new CultureMap("tr-TR", "tr_TR", "TRK", 0x041f, 0x041f, 0x001f);
				CultureID.Add (0x041f, map);
				CultureNames.Add ("tr-TR", map);

				map=new CultureMap("uk", "uk", "UKR", 0x0022, 0x0422, 0x007f);
				CultureID.Add (0x0022, map);
				CultureNames.Add ("uk", map);

				map=new CultureMap("uk-UA", "uk_UA", "UKR", 0x0422, 0x0422, 0x0022);
				CultureID.Add (0x0422, map);
				CultureNames.Add ("uk-UA", map);

				/* Urdu not supported
				   map=new CultureMap("ur", "ur", "URD", 0x0020, 0x0420, 0x007f);
				   CultureID.Add (0x0020, map);
				   CultureNames.Add ("ur", map);

				   map=new CultureMap("ur-PK", "ur-PK", "URD", 0x0420, 0x0420, 0x0020);
				   CultureID.Add (0x0420, map);
				   CultureNames.Add ("ur-PK", map);*/

				/* Uzbek not supported
				   map=new CultureMap("uz", "uz", "UZB", 0x0043, 0x0443, 0x007f);
				   CultureID.Add (0x0043, map);
				   CultureNames.Add ("uz", map);

				   map=new CultureMap("Cy-uz-UZ", "Cy-uz-UZ", "UZB", 0x0843, 0x0843, 0x0043);
				   CultureID.Add (0x0843, map);
				   CultureNames.Add ("Cy-uz-UZ", map);

				   map=new CultureMap("Lt-uz-UZ", "Lt-uz-UZ", "UZB", 0x0443, 0x0443, 0x0043);
				   CultureID.Add (0x0443, map);
				   CultureNames.Add ("Lt-uz-UZ", map);*/

				map=new CultureMap("vi", "vi", "VIT", 0x002a, 0x042a, 0x007f);
				CultureID.Add (0x002a, map);
				CultureNames.Add ("vi", map);

				map=new CultureMap("vi-VN", "vi_VN", "VIT", 0x042a, 0x042a, 0x002a);
				CultureID.Add (0x042a, map);
				CultureNames.Add ("vi-VN", map);

				/* Extras not listed in the docs... */
				map=new CultureMap ("de-DE-PHONEBOOK", "de__PHONEBOOK", "DEU", 0x10407, 0x0407, 0x0007);
				CultureID.Add (0x10407, map);
				CultureNames.Add ("de-DE-PHONEBOOK",map);

				map=new CultureMap ("es-ES-Ts", "es__TRADITIONAL", "ESN", 0x040a, 0x0c0a, 0x000a);
				CultureID.Add (0x040a, map);
				CultureNames.Add ("es-ES-Ts", map);

				/* Others not supported by ms */

				map=new CultureMap ("bn", "dn", "", 0x0045, 0x0445, 0x007f);
				CultureID.Add (0x0045, map);
				CultureNames.Add ("bn", map);

				map=new CultureMap ("bn-IN", "bn_IN", "", 0x0445, 0x0445, 0x0045);
				CultureID.Add (0x0445,map);
				CultureNames.Add ("bn-IN", map);

				map=new CultureMap ("mt", "mt", "", 0x003a, 0x043a, 0x007f);
				CultureID.Add (0x003a, map);
				CultureNames.Add ("mt", map);

				map=new CultureMap ("mt-MT", "mt_MT", "", 0x043a, 0x043a, 0x003a);
				CultureID.Add (0x043a, map);
				CultureNames.Add ("mt-MT", map);

				map=new CultureMap ("zh-TW-STROKE", "zh_TW_STROKE", "", 0x20404, 0x20404, 0x7c04);
				CultureID.Add (0x20404, map);
				CultureNames.Add ("zh-TW-STROKE", map);

				/* Just need an instance to call
				 * GetEnumerator() on
				 */
				lcids=map;
			}
			
			public CultureMap (string name, string icu_name,
					   string win3lang, int lcid,
					   int specific_lcid, int parent_lcid)
			{
				this.name=name;
				this.icu_name=icu_name;
				this.win3lang=win3lang;
				this.lcid=lcid;
				this.specific_lcid=specific_lcid;
				this.parent_lcid=parent_lcid;
			}

			public IEnumerator GetEnumerator ()
			{
				return(new CultureEnumerator (CultureID));
			}
		
			public static int name_to_lcid (string name)
			{
				CultureMap map=CultureNames[name] as CultureMap;
				
				if(map==null) {
					throw new ArgumentException ("Culture name "+name+" is not supported.");
				}
				
				return(map.lcid);
			}
		
			public static int name_to_specific_lcid (string name)
			{
				CultureMap map=CultureNames[name] as CultureMap;
				
				if(map==null) {
					throw new ArgumentException ("Culture name "+name+" is not supported.");
				}
				
				return(map.specific_lcid);
			}

			public static string lcid_to_name (int lcid)
			{
				CultureMap map=CultureID[lcid] as CultureMap;
				
				if(map==null) {
					throw new ArgumentException ("Culture ID "+lcid+" (0x"+lcid.ToString ("x4")+") is not supported.");
				}
				
				return(map.name);
			}
					
			public static string lcid_to_icuname (int lcid)
			{
				CultureMap map=CultureID[lcid] as CultureMap;
				
				if(map==null) {
					throw new ArgumentException ("Culture ID "+lcid+" (0x"+lcid.ToString ("x4")+") is not supported.");
				}
				
				return(map.icu_name);
			}

			public static string lcid_to_win3lang (int lcid)
			{
				CultureMap map=CultureID[lcid] as CultureMap;
				
				if(map==null) {
					throw new ArgumentException ("Culture ID "+lcid+" (0x"+lcid.ToString ("x4")+") is not supported.");
				}
				
				return(map.win3lang);
			}

			public static int lcid_to_specific_lcid (int lcid)
			{
				CultureMap map=CultureID[lcid] as CultureMap;
				
				if(map==null) {
					throw new ArgumentException ("Culture ID "+lcid+" (0x"+lcid.ToString ("x4")+") is not supported.");
				}
				
				return(map.specific_lcid);
			}

			public static int lcid_to_parent_lcid (int lcid)
			{
				CultureMap map=CultureID[lcid] as CultureMap;
				
				if(map==null) {
					throw new ArgumentException ("Culture ID "+lcid+" (0x"+lcid.ToString ("x4")+") is not supported.");
				}
				
				return(map.parent_lcid);
			}

			private sealed class CultureEnumerator : IEnumerator 
			{
				IEnumerator hash_enum;
				
				public CultureEnumerator (Hashtable hash)
				{
					hash_enum=hash.GetEnumerator ();
				}

				public int Current
				{
					get {
						DictionaryEntry ent=(DictionaryEntry)hash_enum.Current;
						CultureMap map=ent.Value as CultureMap;
						return(map.lcid);
					}
				}
				
				object IEnumerator.Current 
				{
					get {
						return Current;
					}
				}

				public bool MoveNext () 
				{
					return(hash_enum.MoveNext ());
				}

				public void Reset ()
				{
					hash_enum.Reset ();
				}
			}
		}
		
		static public CultureInfo InvariantCulture {
			get {
				if (invariant_culture_info == null) {
					lock (typeof (CultureInfo)) {
						if (invariant_culture_info == null) {
							invariant_culture_info = new CultureInfo (0x7f, false);
							invariant_culture_info.is_read_only = true;
						}
					}
				}
				
				return(invariant_culture_info);
			}
		}

		public static CultureInfo CreateSpecificCulture (string name)
		{
			if (name == null) {
				throw new ArgumentNullException ("name");
			}
			
			CultureInfo culture=new CultureInfo (name);
			if(culture.IsNeutralCulture==false) {
				return(culture);
			}

			return(new CultureInfo (CultureMap.name_to_specific_lcid (name)));
		}

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

		public virtual string Name {
			get {
				return(name);
			}
		}

		public virtual string NativeName
		{
			get {
				return(nativename);
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

		public virtual CultureInfo Parent
		{
			get {
				return(new CultureInfo (CultureMap.lcid_to_parent_lcid (lcid)));
			}
		}

		public virtual TextInfo TextInfo
		{
			get {
				if (textinfo == null) {
					lock (this) {
						if(textinfo == null) {
							textinfo = new TextInfo (lcid);
						}
					}
				}
				
				return(textinfo);
			}
		}

		public virtual string ThreeLetterISOLanguageName
		{
			get {
				return(iso3lang);
			}
		}

		public virtual string ThreeLetterWindowsLanguageName
		{
			get {
				return(win3lang);
			}
		}

		public virtual string TwoLetterISOLanguageName
		{
			get {
				return(iso2lang);
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

		public virtual object Clone()
		{
			CultureInfo ci=(CultureInfo)MemberwiseClone ();
			ci.is_read_only=false;
			return(ci);
		}

		public override bool Equals (object value)
		{
			CultureInfo b = value as CultureInfo;
			if (b != null)
				return b.lcid == lcid;
			return false;
		}

		public static CultureInfo[] GetCultures(CultureTypes types)
		{
			bool neutral=((types & CultureTypes.NeutralCultures)!=0);
			bool specific=((types & CultureTypes.SpecificCultures)!=0);
			bool installed=((types & CultureTypes.InstalledWin32Cultures)!=0);  // TODO

			ArrayList arr=new ArrayList ();
			
			foreach (int id in CultureMap.lcids) {
				bool is_neutral=IsIDNeutralCulture (id);

				if((neutral && is_neutral) ||
				   (specific && !is_neutral)) {
					arr.Add (new CultureInfo (id));
				}
			}

			CultureInfo[] cultures=new CultureInfo[arr.Count];
			arr.CopyTo (cultures, 0);

			return(cultures);
		}

		public override int GetHashCode()
		{
			return lcid;
		}

		public static CultureInfo ReadOnly(CultureInfo ci)
		{
			if(ci==null) {
				throw new ArgumentNullException("ci");
			}

			if(ci.is_read_only) {
				return(ci);
			} else {
				CultureInfo new_ci=(CultureInfo)ci.Clone ();
				new_ci.is_read_only=true;
				return(new_ci);
			}
		}

		public override string ToString()
		{
			return(name);
		}
		
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static void construct_compareinfo (object compareinfo, string locale);
		
		public virtual CompareInfo CompareInfo
		{
			get {
				if(compareinfo==null) {
					lock (this) {
						if(compareinfo==null) {
							compareinfo=new CompareInfo (lcid);
							construct_compareinfo (compareinfo, icu_name);
						}
					}
				}
				
				return(compareinfo);
			}
		}

		/* RegionInfo wants to call this method */
		internal static bool IsIDNeutralCulture (int lcid) 
		{
			return((lcid & 0xff00)==0 ||
			       /* Chinese zh-CHS and zh-CHT are
				* treated as neutral too
				*/
			       CultureMap.lcid_to_specific_lcid (lcid)==0);
		}
		
		public virtual bool IsNeutralCulture {
			get {
				return(IsIDNeutralCulture (lcid));
			}
		}

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

		public virtual string DisplayName
		{
			get {
				return(displayname);
			}
		}

		public virtual string EnglishName
		{
			get {
				return(englishname);
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
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern void construct_internal_locale (string locale);
		
		private void Construct (int lcid, string name,
					bool use_user_override)
		{
			/* This will throw ArgumentException if the
			 * locale isnt known
			 */
			this.name=name;
			this.icu_name=name;
			this.use_user_override=use_user_override;
			this.is_read_only=false;
			this.lcid=lcid;

			/* This will throw ArgumentException if
			 * CultureMap doesn't know the culture
			 */
			this.win3lang=CultureMap.lcid_to_win3lang (lcid);
			
			/* This will throw ArgumentException if ICU
			 * doesn't know the culture
			 */
			construct_internal_locale (CultureMap.lcid_to_icuname (lcid));
		}

		/* Do _NOT_ use CultureMap in this method, we don't
		 * want to initialise the hashtables unless someone
		 * asks for a real CultureInfo.
		 */
		private void ConstructInvariant (bool use_user_override)
		{
			is_read_only=false;
			lcid=0x7f;
			this.use_user_override=use_user_override;

			/* NumberFormatInfo defaults to the invariant data */
			number_format=new NumberFormatInfo ();
			
			/* DateTimeFormatInfo defaults to the invariant data */
			datetime_format=new DateTimeFormatInfo ();

			textinfo=new TextInfo ();

			name="";
			displayname="Invariant Language (Invariant Country)";
			englishname="Invariant Language (Invariant Country)";
			nativename="Invariant Language (Invariant Country)";
			iso3lang="IVL";
			iso2lang="iv";
			icu_name="en_US_POSIX";
			win3lang="IVL";
		}
		
		public CultureInfo (int culture, bool use_user_override)
		{
			if (culture < 0)
				throw new ArgumentOutOfRangeException ("culture");

			if(culture==0x007f) {
				/* Short circuit the invariant culture */
				ConstructInvariant (use_user_override);
			} else {
				Construct (culture,
					   CultureMap.lcid_to_name (culture),
					   use_user_override);
			}
		}

		public CultureInfo (int culture) : this (culture, false) {}
		
		public CultureInfo (string name, bool use_user_override)
		{
			if (name == null)
				throw new ArgumentNullException ();

			if(name=="") {
				/* Short circuit the invariant culture */
				ConstructInvariant (use_user_override);
			} else {
				Construct (CultureMap.name_to_lcid (name),
					   name,
					   use_user_override);
			}
		}

		public CultureInfo (string name) : this (name, false) {} 
	}
}
