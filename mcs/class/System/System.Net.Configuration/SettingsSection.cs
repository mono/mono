//
// System.Net.Configuration.SettingsSection.cs
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

#if NET_2_0

using System.Configuration;

namespace System.Net.Configuration 
{
	public sealed class SettingsSection : ConfigurationSection
	{
		#region Fields

		ConfigurationPropertyCollection properties;
		static ConfigurationProperty httpWebRequest = new ConfigurationProperty ("HttpWebRequest", typeof (HttpWebRequestElement), new HttpWebRequestElement ());
		static ConfigurationProperty ipv6 = new ConfigurationProperty ("Ipv6", typeof (Ipv6Element), new Ipv6Element ());
		static ConfigurationProperty servicePointManager = new ConfigurationProperty ("ServicePointManager", typeof (ServicePointManagerElement), new ServicePointManagerElement ());
		static ConfigurationProperty socket = new ConfigurationProperty ("Socket", typeof (SocketElement), new SocketElement ());

		#endregion // Fields

		#region Constructors

		public SettingsSection ()
		{
			properties = new ConfigurationPropertyCollection ();
			properties.Add (httpWebRequest);
			properties.Add (ipv6);
			properties.Add (servicePointManager);
			properties.Add (socket);
		}

		#endregion // Constructors

		#region Properties

		public HttpWebRequestElement HttpWebRequest {
			get { return (HttpWebRequestElement) base [httpWebRequest]; }
		}

		public Ipv6Element Ipv6 {
			get { return (Ipv6Element) base [ipv6]; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		public ServicePointManagerElement ServicePointManager {
			get { return (ServicePointManagerElement) base [servicePointManager]; }
		}

		public SocketElement Socket {
			get { return (SocketElement) base [socket]; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		protected internal override object GetRuntimeObject ()
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}

#endif
