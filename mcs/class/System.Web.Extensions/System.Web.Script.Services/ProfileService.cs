//
// ScriptHandlerFactory.cs
//
// Author:
//   Konstantin Triger <kostat@mainsoft.com>
//
// (C) 2007 Mainsoft, Inc.  http://www.mainsoft.com
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

using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Services;
using System.Configuration;
using System.Web.Configuration;
using System.Web.Script.Serialization;
using System.Web.Profile;

namespace System.Web.Script.Services
{
	sealed class ProfileService
	{
		#region ProfileSerializer

		sealed class ProfileSerializer : JavaScriptSerializer.LazyDictionary
		{
			readonly string [] _properties;
			public ProfileSerializer (string [] properties) {
				_properties = properties;
			}
			protected override IEnumerator<KeyValuePair<string, object>> GetEnumerator () {
				if (_properties == null)
					yield break;

				ProfileBase profile = HttpContext.Current.Profile;
				for (int i = 0; i < _properties.Length; i++) {
					string name = _properties [i];
					int dot = name.IndexOf ('.');
					object value = (dot > 0) ? profile.GetProfileGroup (name.Substring (0, dot))
						.GetPropertyValue (name.Substring (dot + 1)) :
						profile.GetPropertyValue (name);
					yield return new KeyValuePair<string, object> (name, value);
				}
			}
		}

		#endregion

		public const string DefaultWebServicePath = "/Profile_JSON_AppService.axd";

		readonly ScriptingProfileServiceSection _section;

		public ProfileService () {
			_section = (ScriptingProfileServiceSection) WebConfigurationManager.GetSection ("system.web.extensions/scripting/webServices/profileService");
		}

		ScriptingProfileServiceSection ScriptingProfileServiceSection {
			get {
				if (_section == null || !_section.Enabled)
					throw new InvalidOperationException ("Profile service is disabled.");

				return _section;
			}
		}

		[WebMethod()]
		public IDictionary<string,object> GetAllPropertiesForCurrentUser () {
			return new ProfileSerializer (ScriptingProfileServiceSection.ReadAccessProperties);
		}

		[WebMethod ()]
		public IDictionary<string, object> GetPropertiesForCurrentUser (string [] properties) {
			if (properties == null)
				return GetAllPropertiesForCurrentUser ();

			string [] raProps = ScriptingProfileServiceSection.ReadAccessPropertiesNoCopy;

			List<string> list = null;
			for (int i = 0; i < properties.Length; i++) {
				string prop = properties [i];
				if (prop == null)
					throw new ArgumentNullException ("properties[" + i + "]");

				if (IsPropertyConfigured(raProps, prop)) {
					if (list != null)
						list.Add(prop);
				}
				else if (list == null) {
					list = new List<string> (properties.Length - 1);
					for (int k = 0; k < i; k++)
						list.Add (properties [k]);
				}
			}

			return new ProfileSerializer (list != null ? list.ToArray () : properties);
		}

		[WebMethod ()]
		public int SetPropertiesForCurrentUser (Dictionary<string, object> values) {
			if (values == null)
				return 0;

			string [] waProps = ScriptingProfileServiceSection.WriteAccessPropertiesNoCopy;
			
			int counter = 0;
			ProfileBase profile = HttpContext.Current.Profile;
			foreach (KeyValuePair<string, object> pair in values) {
				try {
					string name = pair.Key;
					if (!IsPropertyConfigured (waProps, name))
						continue;

					int dot = name.IndexOf ('.');
					if (dot > 0)
						profile.GetProfileGroup (name.Substring (0, dot))
							.SetPropertyValue (name.Substring (dot + 1), pair.Value);
					else
						profile.SetPropertyValue (name, pair.Value);
				}
				catch {
					continue; //MS seems to ignore errors...
				}

				counter++;
			}

			return counter;
		}

		static bool IsPropertyConfigured (string [] configuredProperties, string propertyToCheck) {
			if (configuredProperties == null)
				return false;

			bool found = false;
			for (int i = 0; !found && i < configuredProperties.Length; i++)
				found = configuredProperties [i].Equals (propertyToCheck, StringComparison.OrdinalIgnoreCase);

			return found;
		}
	}
}
