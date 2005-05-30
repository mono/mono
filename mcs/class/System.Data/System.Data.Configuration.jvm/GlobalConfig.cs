using System.Collections;
using System.Configuration;
using System.Xml;
using System.Collections.Specialized;

namespace System.Data.Configuration {

	internal enum BooleanSetting {
		False,
		True,
		NotSet
	}

	internal sealed class Switches {

		private Switches() {}

		const string SwitchesSection = "system.data/switches";
		const string PrefetchSchemaConfigName = "JDBC.PrefetchSchema";
		static readonly string AppDomainPrefetchSchemaConfigName = String.Concat(SwitchesSection, "/", PrefetchSchemaConfigName);

		internal static BooleanSetting PrefetchSchema {
			get {

				object value = AppDomain.CurrentDomain.GetData(AppDomainPrefetchSchemaConfigName);
				if (value != null)
					return (BooleanSetting)value;

				BooleanSetting setting = BooleanSetting.NotSet;

				NameValueCollection switches = (NameValueCollection)ConfigurationSettings.GetConfig(SwitchesSection);
				if (switches != null) {
					string strVal = (string)switches[PrefetchSchemaConfigName];
					if (strVal != null) {
						try {
							setting = Boolean.Parse(strVal) ? BooleanSetting.True : BooleanSetting.False;
						}
						catch (Exception e) {
							throw new ConfigurationException(e.Message, e);
						}
					}
				}

				//lock(AppDomainPrefetchSchemaConfigName)
				AppDomain.CurrentDomain.SetData(AppDomainPrefetchSchemaConfigName, setting);

				return setting;
			}
		}
	}
}