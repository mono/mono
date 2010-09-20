//
// Author: Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
#if NET_4_0
using System;
using System.ComponentModel;
using System.Configuration;
using System.ServiceModel.Configuration;
using System.Xml;
using System.Xml.Linq;

namespace System.ServiceModel.Discovery.Configuration
{
	public sealed class EndpointDiscoveryElement : BehaviorExtensionElement
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty types, enabled, extensions, scopes;
		
		static EndpointDiscoveryElement ()
		{
			types = new ConfigurationProperty ("types", typeof (ContractTypeNameElementCollection), null, null, null, ConfigurationPropertyOptions.None);
			enabled = new ConfigurationProperty ("enabled", typeof (bool), null, null, null, ConfigurationPropertyOptions.None);
			extensions = new ConfigurationProperty ("extensions", typeof (XmlElementElementCollection), null, null, null, ConfigurationPropertyOptions.None);
			scopes = new ConfigurationProperty ("scopes", typeof (ScopeElementCollection), null, null, null, ConfigurationPropertyOptions.None);
			properties = new ConfigurationPropertyCollection ();
			properties.Add (types);
			properties.Add (enabled);
			properties.Add (extensions);
			properties.Add (scopes);
		}

		public EndpointDiscoveryElement ()
		{
		}
		
		public override Type BehaviorType {
			get { return typeof (EndpointDiscoveryBehavior); }
		}

		[ConfigurationProperty ("types")]
		public ContractTypeNameElementCollection ContractTypeNames {
			get { return (ContractTypeNameElementCollection) base [types]; }
		}
		
		[ConfigurationPropertyAttribute("enabled", DefaultValue = true)]
		public bool Enabled {
			get { return (bool) base [enabled]; }
			set { base [enabled] = value; }
		}
		
		[ConfigurationPropertyAttribute("extensions")]
		public XmlElementElementCollection Extensions {
			get { return (XmlElementElementCollection) base [extensions]; }
		}
		
		[ConfigurationPropertyAttribute("scopes")]
		public ScopeElementCollection Scopes {
			get { return (ScopeElementCollection) base [scopes]; }
		}
		
		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}
		
		protected override object CreateBehavior ()
		{
			var ret = new EndpointDiscoveryBehavior () { Enabled = this.Enabled };
			foreach (ContractTypeNameElement ctn in ContractTypeNames)
				ret.ContractTypeNames.Add (new XmlQualifiedName (ctn.Name, ctn.Namespace));
			foreach (XmlElementElement xee in Extensions)
				ret.Extensions.Add (XElement.Load (new XmlNodeReader (xee.XmlElement)));
			foreach (ScopeElement se in Scopes)
				ret.Scopes.Add (se.Scope);
			return ret;
		}
	}
}

#endif
