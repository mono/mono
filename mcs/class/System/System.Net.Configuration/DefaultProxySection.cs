//
// System.Net.Configuration.DefaultProxySection.cs
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

#if CONFIGURATION_DEP

using System.Configuration;

namespace System.Net.Configuration 
{
	public sealed class DefaultProxySection : ConfigurationSection
	{
		#region Fields

		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty bypassListProp;
		static ConfigurationProperty enabledProp;
		static ConfigurationProperty moduleProp;
		static ConfigurationProperty proxyProp;
		static ConfigurationProperty useDefaultCredentialsProp;

		#endregion // Fields

		#region Constructors

		static DefaultProxySection ()
		{
			bypassListProp = new ConfigurationProperty ("bypasslist", typeof (BypassElementCollection), null);
			enabledProp = new ConfigurationProperty ("enabled", typeof (bool), true);
			moduleProp = new ConfigurationProperty ("module", typeof (ModuleElement), null);
			proxyProp = new ConfigurationProperty ("proxy", typeof (ProxyElement), null);
			useDefaultCredentialsProp = new ConfigurationProperty ("useDefaultCredentials", typeof (bool), false);
			properties = new ConfigurationPropertyCollection ();

			properties.Add (bypassListProp);
			properties.Add (moduleProp);
			properties.Add (proxyProp);
		}

		public DefaultProxySection ()
		{
		}

		#endregion // Constructors

		#region Properties

		[ConfigurationProperty ("bypasslist")]
		public BypassElementCollection BypassList {
			get { return (BypassElementCollection) base [bypassListProp]; }
		}

		[ConfigurationProperty ("enabled", DefaultValue = "True")]
		public bool Enabled {
			get { return (bool) base [enabledProp]; }
			set { base [enabledProp] = value; }
		}

		[ConfigurationProperty ("module")]
		public ModuleElement Module {
			get { return (ModuleElement) base [moduleProp]; }
		}

		[ConfigurationProperty ("proxy")]
		public ProxyElement Proxy {
			get { return (ProxyElement) base [proxyProp]; }
		}

		[ConfigurationProperty ("useDefaultCredentials", DefaultValue = "False")]
		public bool UseDefaultCredentials {
			get { return (bool) base [useDefaultCredentialsProp]; }
			set { base [useDefaultCredentialsProp] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		protected override void PostDeserialize ()
		{
		}

		[MonoTODO]
		protected override void Reset (ConfigurationElement parentElement)
		{
		}

		#endregion // Methods
	}
}

#endif
