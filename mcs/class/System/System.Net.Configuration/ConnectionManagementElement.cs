//
// System.Net.Configuration.ConnectionManagementElement.cs
//
// Authors:
//	Tim Coleman (tim@timcoleman.com)
//	Chris Toshok (toshok@ximian.com)
//
// Copyright (C) Tim Coleman, 2004
// (C) 2004,2005 Novell, Inc. (http://www.novell.com)
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

#if NET_2_0 && CONFIGURATION_DEP

using System.Configuration;

namespace System.Net.Configuration 
{
	public sealed class ConnectionManagementElement : ConfigurationElement
	{
		#region Fields

		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty addressProp;
		static ConfigurationProperty maxConnectionProp;

		#endregion // Fields

		#region Constructors

		static ConnectionManagementElement ()
		{
			addressProp = new ConfigurationProperty ("address", typeof (string),
								 null, ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
			maxConnectionProp = new ConfigurationProperty ("maxconnection", typeof (int),
								       1, ConfigurationPropertyOptions.IsRequired);
			properties = new ConfigurationPropertyCollection ();

			properties.Add (addressProp);
			properties.Add (maxConnectionProp);
		}

		public ConnectionManagementElement ()
		{
		}

		public ConnectionManagementElement (string address, int maxConnection)
		{
			Address = address;
			MaxConnection = maxConnection;
		}

		#endregion // Constructors

		#region Properties

		[ConfigurationProperty ("address", Options = ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey)]
		public string Address {
			get { return (string) base [addressProp]; }
			set { base [addressProp] = value; }
		}

		[ConfigurationProperty ("maxconnection", DefaultValue = "6", Options = ConfigurationPropertyOptions.IsRequired)]
		public int MaxConnection {
			get { return (int) base [maxConnectionProp]; }
			set { base [maxConnectionProp] = value; } 
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		#endregion // Properties
	}
}

#endif
