//
// Mono.Web.Util.MembershipSectionMapper
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2007 Novell, Inc
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
#if NET_2_0
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Configuration;

namespace Mono.Web.Util
{
	internal class MembershipSectionMapper : ISectionSettingsMapper
	{
		public object MapSection (object _section, List <SettingsMappingWhat> whats)
		{
			MembershipSection section = _section as MembershipSection;
			if (section == null)
				return _section;
			
			List <SettingsMappingWhatContents> contents;

			foreach (SettingsMappingWhat what in whats) {
				contents = what.Contents;
				if (contents == null || contents.Count == 0)
					continue;

				foreach (SettingsMappingWhatContents item in contents) {
					switch (item.Operation) {
						case SettingsMappingWhatOperation.Add:
							ProcessAdd (section, item);
							break;

						case SettingsMappingWhatOperation.Clear:
							ProcessClear (section, item);
							break;

						case SettingsMappingWhatOperation.Replace:
							ProcessReplace (section, item);
							break;

						case SettingsMappingWhatOperation.Remove:
							ProcessRemove (section, item);
							break;
					}
				}
			}
				
			return section;
		}

		bool GetCommonAttributes (SettingsMappingWhatContents how, out string name, out string type)
		{
			name = type = null;
			
			Dictionary <string, string> attrs = how.Attributes;
			
			if (attrs == null || attrs.Count == 0)
				return false;

			if (!attrs.TryGetValue ("name", out name))
				return false;

			if (String.IsNullOrEmpty (name))
				return false;

			attrs.TryGetValue ("type", out type);

			return true;
		}

		void SetProviderProperties (SettingsMappingWhatContents how, ProviderSettings prov)
		{
			Dictionary <string, string> attrs = how.Attributes;
			if (attrs == null || attrs.Count == 0)
				return;

			string key;
			foreach (KeyValuePair <string, string> kvp in attrs) {
				key = kvp.Key;
				if (key == "name")
					continue;
				if (key == "type") {
					prov.Type = kvp.Value;
					continue;
				}
				prov.Parameters [key] = kvp.Value;
			}
		}
		
		void ProcessAdd (MembershipSection section, SettingsMappingWhatContents how)
		{
			string name, type;
			if (!GetCommonAttributes (how, out name, out type))
				return;

			ProviderSettingsCollection providers = section.Providers;
			ProviderSettings provider = providers [name];
			if (provider != null)
				return;

			ProviderSettings prov = new ProviderSettings (name, type);
			SetProviderProperties (how, prov);
			
			providers.Add (prov);
		}

		void ProcessRemove (MembershipSection section, SettingsMappingWhatContents how)
		{
			string name, type;
			if (!GetCommonAttributes (how, out name, out type))
				return;

			ProviderSettingsCollection providers = section.Providers;
			ProviderSettings provider = providers [name];
			if (provider != null) {
				if (provider.Type != type)
					return;
				providers.Remove (name);
			}
		}
		
		void ProcessClear (MembershipSection section, SettingsMappingWhatContents how)
		{
			section.Providers.Clear ();
		}

		void ProcessReplace (MembershipSection section, SettingsMappingWhatContents how)
		{
			string name, type;
			if (!GetCommonAttributes (how, out name, out type))
				return;

			ProviderSettings provider = section.Providers [name];
			if (provider != null)
				SetProviderProperties (how, provider);
		}
	}
}
#endif
