//
// AddressHeaderCollectionElement.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.IdentityModel.Tokens;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Diagnostics;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.MsmqIntegration;
using System.ServiceModel.PeerResolvers;
using System.ServiceModel.Security;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace System.ServiceModel.Configuration
{
	public sealed class AddressHeaderCollectionElement
		 : ConfigurationElement
	{
		AddressHeaderCollection _headers;

		// Properties

		[ConfigurationProperty ("headers",
			 DefaultValue = null,
			 Options = ConfigurationPropertyOptions.None)]
		public AddressHeaderCollection Headers {
			get { return _headers; }
			set { _headers = value; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return base.Properties; }
		}
		
		protected override void DeserializeElement (
			XmlReader reader, bool serializeCollectionKey) {
			reader.MoveToContent ();
			_headers = new AddressHeaderCollection (DeserializeAddressHeaders (reader));
		}

		static IEnumerable<AddressHeader> DeserializeAddressHeaders (XmlReader reader) {
			while (true) {
				do {
					reader.Read ();
				} while (reader.NodeType == XmlNodeType.Whitespace);

				if (reader.NodeType == XmlNodeType.EndElement)
					yield break;

				if (reader.NodeType != XmlNodeType.Element)
					throw new ConfigurationErrorsException ("invalid node type.");

				yield return new ConfiguredAddressHeader (reader.LocalName, reader.NamespaceURI, reader.ReadOuterXml ());
			}
		}

		[MonoTODO]
		protected override bool SerializeToXmlElement (
			XmlWriter writer, string elementName)
		{
			return true;
			throw new NotImplementedException ();
		}

		class ConfiguredAddressHeader : AddressHeader {

			readonly string _name;
			readonly string _namespace;
			readonly string _outerXml;

			public ConfiguredAddressHeader (string name, string @namespace, string outerXml) {
				_name = name;
				_namespace = @namespace;
				_outerXml = outerXml;
			}

			[MonoTODO]
			protected override void OnWriteAddressHeaderContents (XmlDictionaryWriter writer) {
				throw new NotImplementedException ();
			}

			public override string Name {
				get { return _name; }
			}

			public override string Namespace {
				get { return _namespace; }
			}
		}
	}

}
