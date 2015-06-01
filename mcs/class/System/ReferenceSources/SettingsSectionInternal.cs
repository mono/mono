namespace System.Net.Configuration {
	sealed class SettingsSectionInternal
	{
		static readonly SettingsSectionInternal instance = new SettingsSectionInternal ();

		internal static SettingsSectionInternal Section {
			get {
				return instance;
			}
		}

#if !MOBILE
		internal UnicodeEncodingConformance WebUtilityUnicodeEncodingConformance = UnicodeEncodingConformance.Auto;
		internal UnicodeDecodingConformance WebUtilityUnicodeDecodingConformance = UnicodeDecodingConformance.Auto;
#endif

		internal bool HttpListenerUnescapeRequestUrl = true;
	}
}
