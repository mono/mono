using System.Net.Security;
using System.Net.Sockets;

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

		internal readonly bool HttpListenerUnescapeRequestUrl = true;
		internal readonly IPProtectionLevel IPProtectionLevel = IPProtectionLevel.Unspecified;

		internal bool UseNagleAlgorithm { get; set; }
		internal bool Expect100Continue { get; set; }
		internal bool CheckCertificateName { get; private set; }
		internal int DnsRefreshTimeout { get; set; }
		internal bool EnableDnsRoundRobin { get; set; }
		internal bool CheckCertificateRevocationList { get; set; }
		internal EncryptionPolicy EncryptionPolicy { get; private set; }

		internal bool Ipv6Enabled {
			get {
#if CONFIGURATION_DEP && !MOBILE
				try {
					var config = (SettingsSection) System.Configuration.ConfigurationManager.GetSection ("system.net/settings");
					if (config != null)
						return config.Ipv6.Enabled;
				} catch {
				}
#endif

				return true;
			}
		}
	}
}
