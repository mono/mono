//
// System.Configuration.ConnectionStringSettings.cs
//
// Authors:
//   Sureshkumar T <tsureshkumar@novell.com>
//   Chris Toshok <toshok@ximian.com>
//
//
// Copyright (C) 2004,2005 Novell, Inc (http://www.novell.com)
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

namespace System.Configuration
{
	public sealed class ConnectionStringSettings : ConfigurationElement
	{
		private static ConfigurationPropertyCollection _properties;
	
		private static readonly ConfigurationProperty _propConnectionString;
		private static readonly ConfigurationProperty _propName;
		private static readonly ConfigurationProperty _propProviderName;

		static ConnectionStringSettings ()
		{
			_properties	= new ConfigurationPropertyCollection ();
			_propName = new ConfigurationProperty ("name", typeof(string), null,
							       TypeDescriptor.GetConverter (typeof (string)),
							       null,
							       ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);

			_propProviderName = new ConfigurationProperty ("providerName", typeof (string), "",
								       ConfigurationPropertyOptions.None);

			_propConnectionString = new ConfigurationProperty ("connectionString", typeof (string), "",
									   ConfigurationPropertyOptions.IsRequired);

			_properties.Add (_propName);
			_properties.Add (_propProviderName);
			_properties.Add (_propConnectionString);
		}

		public ConnectionStringSettings ()
		{
		}

		public ConnectionStringSettings (string name, string connectionString)
			: this (name, connectionString, "")
		{
		}

		public ConnectionStringSettings (string name, string connectionString, string providerName)
		{
			Name = name;
			ConnectionString = connectionString;
			ProviderName = providerName;
		}

		protected internal override ConfigurationPropertyCollection Properties
		{
			get { return _properties; }
		}

		[ConfigurationProperty ("name", DefaultValue = "", Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		public string Name
		{
			get { return (string) base [_propName];}
			set { base [_propName] = value; }
		}

		[ConfigurationProperty ("providerName", DefaultValue = "System.Data.SqlClient")]
		public string ProviderName
		{
			get { return (string) base [_propProviderName]; }
			set { base [_propProviderName] = value; }
		}

		[ConfigurationProperty ("connectionString", DefaultValue = "", Options = ConfigurationPropertyOptions.IsRequired)]
		public string ConnectionString
		{
			get { return (string) base [_propConnectionString]; }
			set { base [_propConnectionString] = value; }
		}
		
		public override string ToString ()
		{
			return ConnectionString;
		}
	}
}

