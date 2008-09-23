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

		public IDictionary <string, object> GetProfileDictionary (string[] properties)
		{
			var ret = new Dictionary <string, object> ();

			int len = properties != null ? properties.Length : 0;
			if (len <= 0)
				return ret;

			ProfileBase profile = HttpContext.Current.Profile;
			string name;
			int dot;
			object value;
			
			for (int i = 0; i < len; i++) {
				name = properties [i];
				dot = name.IndexOf ('.');
				value = (dot > 0) ? profile.GetProfileGroup (name.Substring (0, dot)).GetPropertyValue (name.Substring (dot + 1)) : profile.GetPropertyValue (name);
				ret.Add (name, value);
			}

			return ret;
		}
		
		[WebMethod()]
		public IDictionary<string, object> GetAllPropertiesForCurrentUser (bool authenticatedUserOnly) {
			return GetProfileDictionary (ScriptingProfileServiceSection.ReadAccessProperties);
		}

		[WebMethod ()]
		public IDictionary<string, object> GetPropertiesForCurrentUser (string [] properties, bool authenticatedUserOnly) {
			if (properties == null)
				return GetAllPropertiesForCurrentUser (authenticatedUserOnly);

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

			return GetProfileDictionary (list != null ? list.ToArray () : properties);
		}

		[WebMethod ()]
		public string [] SetPropertiesForCurrentUser (Dictionary<string, object> values, bool authenticatedUserOnly) {
			if (values == null)
				return new string [] { };

			string [] waProps = ScriptingProfileServiceSection.WriteAccessPropertiesNoCopy;

			List<string> list = new List<string> ();
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
					list.Add (pair.Key);
				}
			}

			return list.ToArray ();
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
