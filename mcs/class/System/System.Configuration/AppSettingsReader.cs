//
// System.Configuration.AppSettingsReader
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System.Reflection;
using System.Collections.Specialized;

namespace System.Configuration
{
	public class AppSettingsReader
	{
		NameValueCollection appSettings;

		public AppSettingsReader ()
		{
			appSettings = ConfigurationSettings.AppSettings;
		}

		public object GetValue (string key, Type type)
		{
			if (key == null)
				throw new ArgumentNullException ("key");

			if (type == null)
				throw new ArgumentNullException ("type");

			string value = appSettings [key];
			if (value == null)
				throw new InvalidOperationException ("'" + key + "' could not be found.");

			if (type == typeof (string))
				return value;
			
			MethodInfo parse = type.GetMethod ("Parse", new Type [] {typeof (string)});
			if (parse == null)
				throw new InvalidOperationException ("Type " + type + " does not have a Parse method");

			object result = null;
			try {
				result = parse.Invoke (null, new object [] {value});
			} catch (Exception e) {
				throw new InvalidOperationException ("Parse error.", e);
			}

			return result;
		}
	}
}

