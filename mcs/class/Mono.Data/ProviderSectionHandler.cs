//
// Mono.Data.ProviderSectionHandler
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com) 
//  
//
// Copyright (C) Brian Ritchie, 2002
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
using System.Xml;
using System.Configuration;

namespace Mono.Data
{
#if NET_2_0
	[Obsolete("ProviderFactory in assembly Mono.Data has been made obsolete by DbProviderFactories in assembly System.Data.")]
#endif
	public class ProviderSectionHandler : IConfigurationSectionHandler
	{
		public virtual object Create (object parent, object configContext, XmlNode section)
		{
			if (section == null)
				throw new System.ArgumentNullException ("section");

			ProviderCollection providers = new ProviderCollection ();
			
			XmlNodeList ProviderList = section.SelectNodes ("./provider");

			foreach (XmlNode ProviderNode in ProviderList) {
				Provider provider = new Provider(
					GetStringValue (ProviderNode, "name", true),
					GetStringValue (ProviderNode, "connection", true),
					GetStringValue (ProviderNode, "adapter", true),
					GetStringValue (ProviderNode, "command", true),
					GetStringValue (ProviderNode, "assembly", true),
					GetStringValue (ProviderNode, "description", false),
					GetStringValue (ProviderNode, "parameterprefix", false),
					GetStringValue (ProviderNode, "commandbuilder", false));
				providers.Add (provider);
			}
			return providers;
		}

		private string GetStringValue(XmlNode _node, string _attribute, bool required)
		{
			XmlNode a = _node.Attributes.RemoveNamedItem(_attribute);
			if (a == null) {
				if (required)
					throw new ConfigurationException("Attribute required: " + _attribute);
				else
					return null;
			}
			return a.Value;		
		}
	}
}

