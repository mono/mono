//
// System.Net.Configuration.ConnectionManagementElement.cs
//
// Authors:
//	Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2004
// (c) 2004 Novell, Inc. (http://www.novell.com)
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

#if NET_2_0 && XML_DEP

using System.Configuration;

namespace System.Net.Configuration 
{
	public sealed class ConnectionManagementElement : ConfigurationElement
	{
		#region Fields

		ConfigurationPropertyCollection properties;
		static ConfigurationProperty address = new ConfigurationProperty ("Address", typeof (string), null);
		static ConfigurationProperty maxConnection = new ConfigurationProperty ("MaxConnection", typeof (int), 1);

		#endregion // Fields

		#region Constructors

		public ConnectionManagementElement ()
		{
			properties = new ConfigurationPropertyCollection ();
		}

		public ConnectionManagementElement (string address, int maxConnection)
			: this ()
		{
			Address = address;
			MaxConnection = maxConnection;
		}

		#endregion // Constructors

		#region Properties

		public string Address {
			get { return (string) base [address]; }
			set { base [address] = value; }
		}

		public int MaxConnection {
			get { return (int) base [maxConnection]; }
			set { base [maxConnection] = value; } 
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		#endregion // Properties
	}
}

#endif
