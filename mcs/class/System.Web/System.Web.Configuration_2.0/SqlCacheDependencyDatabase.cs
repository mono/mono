//
// System.Web.Configuration.SqlCacheDependencyDatabaseCollection
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
using System.ComponentModel;
using System.Configuration;

#if NET_2_0

namespace System.Web.Configuration {

	public sealed class SqlCacheDependencyDatabase : ConfigurationElement
	{
		static ConfigurationProperty connectionStringNameProp;
		static ConfigurationProperty nameProp;
		static ConfigurationProperty pollTimeProp;
		static ConfigurationPropertyCollection properties;

		static ConfigurationElementProperty elementProperty;

		static SqlCacheDependencyDatabase ()
		{
			connectionStringNameProp = new ConfigurationProperty ("connectionStringName", typeof (string), null,
									      TypeDescriptor.GetConverter (typeof (string)),
									      PropertyHelper.NonEmptyStringValidator,
									      ConfigurationPropertyOptions.IsRequired);
			nameProp = new ConfigurationProperty ("name", typeof (string), null,
							      TypeDescriptor.GetConverter (typeof (string)),
							      PropertyHelper.NonEmptyStringValidator,
							      ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
			pollTimeProp = new ConfigurationProperty ("pollTime", typeof (int), 60000);
			properties = new ConfigurationPropertyCollection ();

			properties.Add (connectionStringNameProp);
			properties.Add (nameProp);
			properties.Add (pollTimeProp);

			elementProperty = new ConfigurationElementProperty (new CallbackValidator (typeof (SqlCacheDependencyDatabase), ValidateElement));
		}

		internal SqlCacheDependencyDatabase ()
		{
		}

		public SqlCacheDependencyDatabase (string name, string connectionStringName)
		{
			this.Name = name;
			this.ConnectionStringName = name;
		}

		public SqlCacheDependencyDatabase (string name, string connectionStringName, int pollTime)
		{
			this.Name = name;
			this.ConnectionStringName = name;
			this.PollTime = pollTime;
		}

		static void ValidateElement (object o)
		{
			/* XXX do some sort of element validation here? */
		}

		protected internal override ConfigurationElementProperty ElementProperty {
			get { return elementProperty; }
		}

		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("connectionStringName", Options = ConfigurationPropertyOptions.IsRequired)]
		public string ConnectionStringName {
			get { return (string) base [connectionStringNameProp];}
			set { base[connectionStringNameProp] = value; }
		}

		[StringValidator (MinLength = 1)]
		[ConfigurationProperty ("name", Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		public string Name {
			get { return (string) base [nameProp];}
			set { base[nameProp] = value; }
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

