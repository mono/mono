using System.Net.Security;

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


		internal bool UseNagleAlgorithm { get; set; }
		internal bool Expect100Continue { get; set; }
		internal bool CheckCertificateName { get; private set; }
		internal int DnsRefreshTimeout { get; set; }
		internal bool EnableDnsRoundRobin { get; set; }
		internal bool CheckCertificateRevocationList { get; set; }
		internal EncryptionPolicy EncryptionPolicy { get; private set; }
	}
}
