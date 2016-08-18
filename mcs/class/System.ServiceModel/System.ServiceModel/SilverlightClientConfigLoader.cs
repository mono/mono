//
// SilverlightClientConfigLoader.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc.  http://www.novell.com
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
#if MOBILE || XAMMAC_4_5

using System;
using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;

namespace System.ServiceModel
{
	// This class is to read ServiceReference.ClientConfig which is
	// used to give default settings for ClientBase<T> configuration.
	// It is only used in Silverlight application.
	//
	// Since System.Configuration is not supported in SL, this config
	// loader has to be created without depending on it.

	internal class SilverlightClientConfigLoader
	{
		public SilverlightClientConfiguration Load (XmlReader reader)
		{
			var ret = new SilverlightClientConfiguration ();
			ret.Bindings = new BindingsConfiguration ();
			ret.Client = new ClientConfiguration ();

			reader.MoveToContent ();
			if (reader.IsEmptyElement)
				return ret;
			reader.ReadStartElement ("configuration");

			for (reader.MoveToContent (); reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ()) {
				if (reader.NodeType != XmlNodeType.Element ||
				    reader.LocalName != "system.serviceModel" ||
				    reader.IsEmptyElement) {
					reader.Skip ();
					continue;
				}
				// in <system.serviceModel>
				reader.ReadStartElement ();
				for (reader.MoveToContent (); reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ()) {
					if (reader.NodeType != XmlNodeType.Element ||
					    reader.IsEmptyElement) {
						reader.Skip ();
						continue;
					}
					switch (reader.LocalName) {
					case "bindings":
						reader.ReadStartElement ();
						for (reader.MoveToContent (); reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ()) {
							if (reader.NodeType != XmlNodeType.Element ||
							    reader.IsEmptyElement) {
								reader.Skip ();
								continue;
							}
							switch (reader.LocalName) {
							case "customBinding":
							reader.ReadStartElement ();
							for (reader.MoveToContent (); reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ()) {
								if (reader.NodeType != XmlNodeType.Element ||
								    reader.LocalName != "binding") {
									reader.Skip ();
									continue;
								}
								ret.Bindings.CustomBinding.Add (ReadCustomBinding (reader));
							}
							reader.ReadEndElement ();
							break;
							case "basicHttpBinding":
							reader.ReadStartElement ();
							for (reader.MoveToContent (); reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ()) {
								if (reader.NodeType != XmlNodeType.Element ||
								    reader.LocalName != "binding") {
									reader.Skip ();
									continue;
								}
								ret.Bindings.BasicHttpBinding.Add (ReadBasicHttpBinding (reader));
							}
							reader.ReadEndElement ();
							break;
							}
						}
						reader.ReadEndElement ();
						break;
					case "client":
						reader.ReadStartElement ();
						for (reader.MoveToContent (); reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ()) {
							if (reader.NodeType != XmlNodeType.Element ||
							    reader.LocalName != "endpoint") {
								reader.Skip ();
								continue;
							}
							ret.Client.Endpoints.Add (ReadEndpoint (reader));
						}
						reader.ReadEndElement ();
						break;
					}
				}
				reader.ReadEndElement ();
				// out <system.serviceModel>
			}
			reader.ReadEndElement ();
			// out <configuration>

			return ret;
		}

		CustomBindingConfiguration ReadCustomBinding (XmlReader reader)
		{
			string a;
			var b = new CustomBindingConfiguration ();
			if ((a = reader.GetAttribute ("name")) != null)
				b.Name = a;

			if (!reader.IsEmptyElement) {
				reader.ReadStartElement ();
				for (reader.MoveToContent (); reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ()) {
					if (reader.NodeType != XmlNodeType.Element) {
						reader.Skip ();
						continue;
					}
					switch (reader.LocalName) {
					case "binaryMessageEncoding":
						b.MessageEncoding = new BinaryMessageEncodingBindingElement ();
						reader.Skip ();
						break;
					case "textMessageEncoding":
						b.MessageEncoding = new TextMessageEncodingBindingElement ();
						reader.Skip ();
						break;
					case "httpTransport":
						b.Transport = new HttpTransportBindingElement ();
						reader.Skip ();
						break;
					case "httpsTransport":
						b.Transport = new HttpsTransportBindingElement ();
						reader.Skip ();
						break;
					default:
						throw new XmlException (String.Format ("Unexpected configuration element '{0}'", reader.LocalName));
					}
				}
				reader.ReadEndElement ();
			}
			
			return b;
		}

		BasicHttpBindingConfiguration ReadBasicHttpBinding (XmlReader reader)
		{
			string a;
			var b = new BasicHttpBindingConfiguration ();
			
			if ((a = reader.GetAttribute ("name")) != null)
				b.Name = a;
			if ((a = reader.GetAttribute ("maxBufferPoolSize")) != null)
				b.MaxBufferPoolSize = XmlConvert.ToInt32 (a);
			if ((a = reader.GetAttribute ("maxBufferSize")) != null)
				b.MaxBufferSize = XmlConvert.ToInt32 (a);
			if ((a = reader.GetAttribute ("maxReceivedMessageSize")) != null)
				b.MaxReceivedMessageSize = XmlConvert.ToInt32 (a);
			if ((a = reader.GetAttribute ("textEncoding")) != null)
				b.TextEncoding = Encoding.GetEncoding (a);

			if (!reader.IsEmptyElement) {
				reader.ReadStartElement ();
				for (reader.MoveToContent (); reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ()) {
					if (reader.NodeType != XmlNodeType.Element ||
					    reader.LocalName != "security") {
						reader.Skip ();
						continue;
					}
					if ((a = reader.GetAttribute ("mode")) != null)
						b.Security.Mode = (BasicHttpSecurityMode) Enum.Parse (typeof (BasicHttpSecurityMode), a, false);
					reader.Skip ();
				}
				reader.ReadEndElement ();
			}
			else
				reader.Read ();
			
			return b;
		}

		EndpointConfiguration ReadEndpoint (XmlReader reader)
		{
			string a;
			var e = new EndpointConfiguration ();

			if ((a = reader.GetAttribute ("name")) != null)
				e.Name = a;
			if ((a = reader.GetAttribute ("address")) != null)
				e.Address = new Uri (a);
			if ((a = reader.GetAttribute ("bindingConfiguration")) != null)
				e.BindingConfiguration = a;
			if ((a = reader.GetAttribute ("contract")) != null)
				e.Contract = a;
			reader.Skip ();

			return e;
		}

		public class SilverlightClientConfiguration
		{
			public SilverlightClientConfiguration ()
			{
			}

			public BindingsConfiguration Bindings { get; set; }
			public ClientConfiguration Client { get; set; }

			public ServiceEndpointConfiguration GetServiceEndpointConfiguration (string name)
			{
				var s = new ServiceEndpointConfiguration ();
				s.Name = name;
				EndpointConfiguration e = GetEndpointConfiguration (name);
				s.Address = new EndpointAddress (e.Address);
				s.Binding = GetConfiguredHttpBinding (e).CreateBinding ();
				return s;
			}

			EndpointConfiguration GetEndpointConfiguration (string name)
			{
				foreach (var e in Client.Endpoints)
					if (e.Name == name || name == "*")
						return e;
				return Client.Endpoints [0];
			}

			BindingConfiguration GetConfiguredHttpBinding (EndpointConfiguration endpoint)
			{
				foreach (var b in Bindings.All ())
					if (b.Name == endpoint.BindingConfiguration)
						return b;
				return Bindings.BasicHttpBinding [0];
			}
		}

		public class ServiceEndpointConfiguration
		{
			public string Name { get; set; }
			public EndpointAddress Address { get; set; }
			public Binding Binding { get; set; }
		}

		public class BindingsConfiguration
		{
			public BindingsConfiguration ()
			{
				BasicHttpBinding = new List<BasicHttpBindingConfiguration> ();
				CustomBinding = new List<CustomBindingConfiguration> ();
			}

			public IList<BasicHttpBindingConfiguration> BasicHttpBinding { get; private set; }
			public IList<CustomBindingConfiguration> CustomBinding { get; private set; }

			public IEnumerable<BindingConfiguration> All ()
			{
				foreach (var b in BasicHttpBinding)
					yield return b;
				foreach (var b in CustomBinding)
					yield return b;
			}
		}

		public abstract class BindingConfiguration
		{
			public string Name { get; set; }

			public abstract Binding CreateBinding ();
		}

		public class BasicHttpBindingConfiguration : BindingConfiguration
		{
			public BasicHttpBindingConfiguration ()
			{
				Binding = new BasicHttpBinding ();
				Security = new BasicHttpSecurityConfiguration (Binding.Security);
			}

			BasicHttpBinding Binding { get; set; }

			public override Binding CreateBinding ()
			{
				return Binding;
			}

			public BasicHttpSecurityConfiguration Security { get; private set; }

			public long MaxBufferPoolSize {
				get { return Binding.MaxBufferPoolSize; }
				set { Binding.MaxBufferPoolSize = value; }
			}
			public int MaxBufferSize {
				get { return Binding.MaxBufferSize; }
				set { Binding.MaxBufferSize = value; }
			}
			public long MaxReceivedMessageSize {
				get { return Binding.MaxReceivedMessageSize; }
				set { Binding.MaxReceivedMessageSize = value; }
			}

			public Encoding TextEncoding {
				get { return Binding.TextEncoding; }
				set { Binding.TextEncoding = value; }
			}

			// public bool AllowCookies { get; set; }
			// public bool BypassProxyOnLocal { get; set; }
			// public HostNameComparisonMode HostNameComparisonMode { get; set; }
			// public WSMessageEncoding MessageEncoding { get; set; }
			// public Uri ProxyAddress { get; set; }
			// public XmlDictionaryReaderQuotasElement ReaderQuotas { get; }
			// public TransferMode TransferMode { get; set; }
			// public bool UseDefaultWebProxy { get; set; }
		}

		public class BasicHttpSecurityConfiguration
		{
			public BasicHttpSecurityConfiguration (BasicHttpSecurity security)
			{
				Security = security;
			}

			public BasicHttpSecurity Security { get; private set; }

			public BasicHttpSecurityMode Mode {
				get { return Security.Mode; }
				set { Security.Mode = value; }
			}
		}

		public class CustomBindingConfiguration : BindingConfiguration
		{
			public override Binding CreateBinding ()
			{
				return new CustomBinding (MessageEncoding, Transport);
			}

			public MessageEncodingBindingElement MessageEncoding { get; set; }
			public TransportBindingElement Transport { get; set; }
		}

		public class ClientConfiguration
		{
			public ClientConfiguration ()
			{
				Endpoints = new List<EndpointConfiguration> ();
			}

			public IList<EndpointConfiguration> Endpoints { get; private set; }

			// (should be) no metadata element support unlike full WCF.
		}

		public class EndpointConfiguration
		{
			public EndpointConfiguration ()
			{
			}

			public string Name { get; set; }
			public Uri Address { get; set; }
			public string BindingConfiguration { get; set; }
			// public string BehaviorConfiguration { get; set; }
			public string Contract { get; set; }
			// public AddressHeaderCollection Headers { get; set; }
			// public EndpointIdentity Identity { get; set; }
		}
	}
}
#endif
