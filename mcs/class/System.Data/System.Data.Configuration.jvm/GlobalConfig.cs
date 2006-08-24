// 
// System.Data.Configuration.GlobaConfig.cs
//
// Authors:
//	Konstantin Triger <kostat@mainsoft.com>
//	
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

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

		const string SwitchesSection = "Mainsoft.Data.Configuration/switches";
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