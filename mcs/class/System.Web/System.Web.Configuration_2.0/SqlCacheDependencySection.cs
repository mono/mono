//
// System.Web.Configuration.SqlCacheDependencySection
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Configuration;

#if NET_2_0

namespace System.Web.Configuration {

	public sealed class SqlCacheDependencySection : ConfigurationSection
	{
		static ConfigurationProperty databasesProp;
		static ConfigurationProperty enabledProp;
		static ConfigurationProperty pollTimeProp;
		static ConfigurationPropertyCollection properties;

		static ConfigurationElementProperty elementProperty;

		static SqlCacheDependencySection ()
		{
			databasesProp = new ConfigurationProperty ("databases", typeof (SqlCacheDependencyDatabaseCollection), null,
								   null, null, ConfigurationPropertyOptions.None);
			enabledProp = new ConfigurationProperty ("enabled", typeof (bool), true);
			pollTimeProp = new ConfigurationProperty ("pollTime", typeof (int), 60000);
			properties = new ConfigurationPropertyCollection ();

			properties.Add (databasesProp);
			properties.Add (enabledProp);
			properties.Add (pollTimeProp);

			elementProperty = new ConfigurationElementProperty (new CallbackValidator (typeof (SqlCacheDependencySection), ValidateElement));
		}

		static void ValidateElement (object o)
		{
			/* XXX do some sort of element validation here? */
		}

		protected internal override ConfigurationElementProperty ElementProperty {
			get { return elementProperty; }
		}

		protected override void PostDeserialize ()
		{
			base.PostDeserialize ();
		}

		[ConfigurationProperty ("databases")]
		public SqlCacheDependencyDatabaseCollection Databases {
			get { return (SqlCacheDependencyDatabaseCollection) base [databasesProp];}
		}

		[ConfigurationProperty ("enabled", DefaultValue = "True")]
		public bool Enabled {
			get { return (bool) base [enabledProp];}
			set { base[enabledProp] = value; }
		}

		[ConfigurationProperty ("pollTime", DefaultValue = "60000")]
		public int PollTime {
			get { return (int) base [pollTimeProp];}
			set { base[pollTimeProp] = value; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

	}

}

#endif
