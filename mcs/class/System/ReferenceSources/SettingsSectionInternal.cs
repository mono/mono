namespace System.Net.Configuration {
	sealed class SettingsSectionInternal
	{
		static readonly SettingsSectionInternal instance = new SettingsSectionInternal ();

		internal static SettingsSectionInternal Section {
			get {
				return instance;
			}
		}

		internal UnicodeEncodingConformance WebUtilityUnicodeEncodingConformance = UnicodeEncodingConformance.Auto;
		internal UnicodeDecodingConformance WebUtilityUnicodeDecodingConformance = UnicodeDecodingConformance.Auto;
	}
}