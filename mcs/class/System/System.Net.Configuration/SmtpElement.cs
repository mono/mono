//
// System.Net.Configuration.SmtpElement.cs
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
	public sealed class SmtpElement : ConfigurationElement
	{
		#region Fields

		ConfigurationPropertyCollection properties;
		static ConfigurationProperty defaultCredentials = new ConfigurationProperty ("DefaultCredentials", typeof (bool), false);
		static ConfigurationProperty host = new ConfigurationProperty ("Host", typeof (string), null);
		static ConfigurationProperty password = new ConfigurationProperty ("Password", typeof (string), null);
		static ConfigurationProperty port = new ConfigurationProperty ("Port", typeof (int), 25);
		static ConfigurationProperty userName = new ConfigurationProperty ("UserName", typeof (string), null);

		#endregion // Fields

		#region Constructors

		public SmtpElement ()
		{
			properties = new ConfigurationPropertyCollection ();
		}

		#endregion // Constructors

		#region Properties

		public bool DefaultCredentials {
			get { return (bool) base [defaultCredentials]; }
			set { base [defaultCredentials] = value; }
		}

		public string Host {
			get { return (string) base [host]; }
			set { base [host] = value; }
		}

		public string Password {
			get { return (string) base [password]; }
			set { base [password] = value; }
		}

		public int Port {
			get { return (int) base [port]; }
			set { base [port] = value; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		public string UserName {
			get { return (string) base [userName]; }
			set { base [userName] = value; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		protected override void ValidateRequiredProperties (ConfigurationPropertyCollection props, bool serializeCollectionKey)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}

#endif
