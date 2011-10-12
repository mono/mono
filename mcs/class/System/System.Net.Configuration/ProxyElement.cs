//
// System.Net.Configuration.ProxyElement.cs
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

using System;
using System.Configuration;

namespace System.Net.Configuration 
{
	public sealed class ProxyElement : ConfigurationElement
	{
		#region Fields

		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty autoDetectProp;
		static ConfigurationProperty bypassOnLocalProp;
		static ConfigurationProperty proxyAddressProp;
		static ConfigurationProperty scriptLocationProp;
		static ConfigurationProperty useSystemDefaultProp;

		#endregion // Fields

		#region Constructors

		static ProxyElement ()
		{
			autoDetectProp = new ConfigurationProperty ("autoDetect", typeof (AutoDetectValues), AutoDetectValues.Unspecified);
			bypassOnLocalProp = new ConfigurationProperty ("bypassonlocal", typeof (BypassOnLocalValues), BypassOnLocalValues.Unspecified);
			proxyAddressProp = new ConfigurationProperty ("proxyaddress", typeof (Uri), null);
			scriptLocationProp = new ConfigurationProperty ("scriptLocation", typeof (Uri), null);
			useSystemDefaultProp = new ConfigurationProperty ("usesystemdefault", typeof (UseSystemDefaultValues), UseSystemDefaultValues.Unspecified);

			properties = new ConfigurationPropertyCollection ();
								    
			properties.Add (bypassOnLocalProp);
			properties.Add (proxyAddressProp);
			properties.Add (scriptLocationProp);
			properties.Add (useSystemDefaultProp);
		}

		public ProxyElement ()
		{
		}

		#endregion // Constructors

		#region Properties

		[ConfigurationProperty ("autoDetect", DefaultValue = "Unspecified")]
		public AutoDetectValues AutoDetect {
			get { return (AutoDetectValues) base [autoDetectProp]; }
			set { base [autoDetectProp] = value; }
		}

		[ConfigurationProperty ("bypassonlocal", DefaultValue = "Unspecified")]
		public BypassOnLocalValues BypassOnLocal {
			get { return (BypassOnLocalValues) base [bypassOnLocalProp]; }
			set { base [bypassOnLocalProp] = value; }
		}

		[ConfigurationProperty ("proxyaddress")]
		public Uri ProxyAddress {
			get { return (Uri) base [proxyAddressProp]; }
			set { base [proxyAddressProp] = value; }
		}

		[ConfigurationProperty ("scriptLocation")]
		public Uri ScriptLocation {
			get { return (Uri) base [scriptLocationProp]; }
			set { base [scriptLocationProp] = value; }
		}

		[ConfigurationProperty ("usesystemdefault", DefaultValue = "Unspecified")]
		public UseSystemDefaultValues UseSystemDefault {
			get { return (UseSystemDefaultValues) base [useSystemDefaultProp]; }
			set { base [useSystemDefaultProp] = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		#endregion // Properties

		public enum BypassOnLocalValues
		{
			Unspecified = -1,
			True = 1,
			False = 0
		}

		public enum UseSystemDefaultValues
		{
			Unspecified = -1,
			True = 1,
			False = 0
		}

		public enum AutoDetectValues
		{
			Unspecified = -1,
			True = 1,
			False = 0
		}
	}
}

#endif
