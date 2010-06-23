//
// ServiceMetadataExtension.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//	Ankit Jain <jankit@novell.com>
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Web;
using System.Web.Services;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Schema;

using WSServiceDescription = System.Web.Services.Description.ServiceDescription;
using WSMessage = System.Web.Services.Description.Message;
using SMMessage = System.ServiceModel.Channels.Message;
using WCFBinding = System.ServiceModel.Channels.Binding;

namespace System.ServiceModel.Description
{
	public class ServiceMetadataExtension : IExtension<ServiceHostBase>
	{
		const string ServiceMetadataBehaviorHttpGetBinding = "ServiceMetadataBehaviorHttpGetBinding";

		MetadataSet metadata;
		ServiceHostBase owner;
		Dictionary<Uri, ChannelDispatcher> dispatchers;
		HttpGetWsdl instance;

		public ServiceMetadataExtension ()
		{
		}

		internal HttpGetWsdl Instance {
			get { return instance; }
		}

		[MonoTODO]
		public MetadataSet Metadata {
			get {
				if (metadata == null) {
					var exporter = new WsdlExporter ();
					exporter.ExportEndpoints (owner.Description.Endpoints, new XmlQualifiedName (owner.Description.Name, owner.Description.Namespace));
					metadata = exporter.GetGeneratedMetadata ();
				}
				return metadata;
			}
		}

		internal ServiceHostBase Owner {
			get { return owner; }
		}

		internal static ServiceMetadataExtension EnsureServiceMetadataExtension (ServiceHostBase serviceHostBase) {
			ServiceMetadataExtension sme = serviceHostBase.Extensions.Find<ServiceMetadataExtension> ();
			if (sme == null) {
				sme = new ServiceMetadataExtension ();
				serviceHostBase.Extensions.Add (sme);
			}
			return sme;
		}

		// FIXME: distinguish HTTP and HTTPS in the Url properties.
		// FIXME: reject such ServiceDescription that has no HTTP(S) binding.
		// FIXME: it should not use the binding that is used in the ServiceEndpoint. For example, WSDL results have to be given as text, not binary.
		// FIXME: if the ServiceDescription has a base address (e.g. http://localhost:8080) and HttpGetUrl is empty, it returns UnknownDestination while it is expected to return the HTTP help page.
		internal void EnsureChannelDispatcher (bool isMex, string scheme, Uri uri, WCFBinding binding)
		{
			if (isMex)
				instance.WsdlUrl = uri;
			else
				instance.HelpUrl = uri;

			if (dispatchers == null)
				dispatchers = new Dictionary<Uri, ChannelDispatcher> ();
			else if (dispatchers.ContainsKey (uri))
				return; // already exists (e.g. reached here for wsdl while help is already filled on the same URI.)

			if (binding == null) {
				switch (scheme) {
				case "http":
					binding = MetadataExchangeBindings.CreateMexHttpBinding ();
					break;
				case "https":
					binding = MetadataExchangeBindings.CreateMexHttpsBinding ();
					break;
				case "net.tcp":
					binding = MetadataExchangeBindings.CreateMexTcpBinding ();
					break;
				case "net.pipe":
					binding = MetadataExchangeBindings.CreateMexNamedPipeBinding ();
					break;
				}
			}

			CustomBinding cb = new CustomBinding (binding) { Name = ServiceMetadataBehaviorHttpGetBinding };
			cb.Elements.Find<MessageEncodingBindingElement> ().MessageVersion = MessageVersion.None;

			ServiceEndpoint se = new ServiceEndpoint (ContractDescription.GetContract (typeof (IHttpGetHelpPageAndMetadataContract)), cb, new EndpointAddress (uri))
			{
				ListenUri = uri,
			};

			var channelDispatcher = new DispatcherBuilder (Owner).BuildChannelDispatcher (owner.Description.ServiceType, se, new BindingParameterCollection ());
			channelDispatcher.MessageVersion = MessageVersion.None; // it has no MessageVersion.
			channelDispatcher.IsMex = true;
			channelDispatcher.Endpoints [0].DispatchRuntime.InstanceContextProvider = new SingletonInstanceContextProvider (new InstanceContext (owner, instance));

			dispatchers.Add (uri, channelDispatcher);
			owner.ChannelDispatchers.Add (channelDispatcher);
		}

		void IExtension<ServiceHostBase>.Attach (ServiceHostBase owner)
		{
			this.owner = owner;
			instance = new HttpGetWsdl (this);
		}

		void IExtension<ServiceHostBase>.Detach (ServiceHostBase owner)
		{
			this.owner = null;
		}
	}

	[ServiceContract (Namespace = "http://schemas.microsoft.com/2006/04/http/metadata")]
	interface IHttpGetHelpPageAndMetadataContract
	{
		[OperationContract (Action = "*", ReplyAction = "*")]
		SMMessage Get (SMMessage req);
	}

	class HttpGetWsdl : IHttpGetHelpPageAndMetadataContract
	{
		ServiceMetadataExtension ext;
		bool initialized;

		Dictionary <string,WSServiceDescription> wsdl_documents = 
			new Dictionary<string, WSServiceDescription> ();
		Dictionary <string, XmlSchema> schemas = 
			new Dictionary<string, XmlSchema> ();

		public HttpGetWsdl (ServiceMetadataExtension ext)
		{
			this.ext = ext;
		}

		public Uri HelpUrl { get; set; }
		public Uri WsdlUrl { get; set; }

		void EnsureMetadata ()
		{
			if (!initialized) {
				GetMetadata ();
				initialized = true;
			}
		}

		public SMMessage Get (SMMessage req)
		{
			EnsureMetadata ();

			HttpRequestMessageProperty prop = (HttpRequestMessageProperty) req.Properties [HttpRequestMessageProperty.Name];

			NameValueCollection query_string = CreateQueryString (prop.QueryString);
			if (query_string == null || query_string.AllKeys.Length != 1) {
				if (HelpUrl != null && Uri.Compare (req.Headers.To, HelpUrl, UriComponents.HttpRequestUrl ^ UriComponents.Query, UriFormat.UriEscaped, StringComparison.Ordinal) == 0)
					return CreateHelpPage (req);
				WSServiceDescription w = GetWsdl ("wsdl");
				if (w != null)
					return CreateWsdlMessage (w);
			}

			if (String.Compare (query_string [null], "wsdl", StringComparison.OrdinalIgnoreCase) == 0) {
				WSServiceDescription wsdl = GetWsdl ("wsdl");
				if (wsdl != null)
					return CreateWsdlMessage (wsdl);
			} else if (query_string ["wsdl"] != null) {
				WSServiceDescription wsdl = GetWsdl (query_string ["wsdl"]);
				if (wsdl != null)
					return CreateWsdlMessage (wsdl);
			} else if (query_string ["xsd"] != null) {
				XmlSchema schema = GetXmlSchema (query_string ["xsd"]);
				if (schema != null) {
					//FIXME: Is this the correct way?
					MemoryStream ms = new MemoryStream ();

					schema.Write (ms);
					ms.Seek (0, SeekOrigin.Begin);
					SMMessage ret = SMMessage.CreateMessage (MessageVersion.None, "", XmlReader.Create (ms));

					return ret;
				}
			}

			return CreateHelpPage (req);
		}

		/* Code from HttpListenerRequest */
		NameValueCollection CreateQueryString (string query)
		{
			NameValueCollection query_string = new NameValueCollection ();
			if (query == null || query.Length == 0)
				return null;

			string [] components = query.Split ('&');
			foreach (string kv in components) {
				int pos = kv.IndexOf ('=');
				if (pos == -1) {
					query_string.Add (null, HttpUtility.UrlDecode (kv));
				} else {
					string key = HttpUtility.UrlDecode (kv.Substring (0, pos));
					string val = HttpUtility.UrlDecode (kv.Substring (pos + 1));

					query_string.Add (key, val);
				}
			}

			return query_string;
		}

		// It is returned for ServiceDebugBehavior.Http(s)HelpPageUrl.
		// They may be empty, and for such case the help page URL is
		// simply the service endpoint URL (foobar.svc).
		//
		// Note that if there is also ServiceMetadataBehavior that
		// lacks Http(s)GetUrl, then it is also mapped to the same
		// URL, but it requires "?wsdl" parameter and .NET somehow
		// differentiates those requests.
		//
		// If both Http(s)HelpPageUrl and Http(s)GetUrl exist, then
		// requests to the service endpoint URL (foobar.svc) results
		// in an xml output with empty string (non-WF XML error).

		SMMessage CreateHelpPage (SMMessage request)
		{
			var helpBody = ext.Owner.Description.Behaviors.Find<ServiceMetadataBehavior> () != null ?
				String.Format (@"
<p>To create client proxy source, run:</p>
<p><code>svcutil <a href='{0}'>{0}</a></code></p>
<!-- FIXME: add client proxy usage (that required decent ServiceContractGenerator implementation, so I leave it yet.) -->
", new Uri (WsdlUrl.ToString () + "?wsdl")) : // this Uri.ctor() is nasty, but there is no other way to add "?wsdl" (!!)
				String.Format (@"
<p>Service metadata publishing for {0} is not enabled. Service administrators can enable it by adding &lt;serviceMetadata&gt; element in the host configuration (web.config in ASP.NET), or ServiceMetadataBehavior object to the Behaviors collection of the service host's ServiceDescription.</p>", ext.Owner.Description.Name);

			var html = String.Format (@"
<html>
<head>
<title>Service {0}</title>
</head>
<body>
{1}
</body>
</html>", ext.Owner.Description.Name, helpBody);

			var m = SMMessage.CreateMessage (MessageVersion.None, "", XmlReader.Create (new StringReader (html)));
			var rp = new HttpResponseMessageProperty ();
			rp.Headers ["Content-Type"] = "text/html";
			m.Properties.Add (HttpResponseMessageProperty.Name, rp);
			return m;
		}

		SMMessage CreateWsdlMessage (WSServiceDescription wsdl)
		{
			MemoryStream ms = new MemoryStream ();
			XmlWriter xw = XmlWriter.Create (ms);

			WSServiceDescription.Serializer.Serialize (xw, wsdl);
			ms.Seek (0, SeekOrigin.Begin);
			return SMMessage.CreateMessage (MessageVersion.None, "", XmlReader.Create (ms));
		}

		void GetMetadata ()
		{
			if (WsdlUrl == null)
				return;

			MetadataSet metadata = ext.Metadata;
			int xs_i = 0, wsdl_i = 0;

			//Dictionary keyed by namespace
			StringDictionary wsdl_strings = new StringDictionary ();
			StringDictionary xsd_strings = new StringDictionary ();

			foreach (MetadataSection section in metadata.MetadataSections) {
				string key;

				XmlSchema xs = section.Metadata as XmlSchema;
				if (xs != null) {
					key = String.Format ("xsd{0}", xs_i ++);
					schemas [key] = xs;
					xsd_strings [xs.TargetNamespace] = key;
					continue;
				}

				WSServiceDescription wsdl = section.Metadata as WSServiceDescription;
				if (wsdl == null)
					continue;

				//if (wsdl.TargetNamespace == "http://tempuri.org/")
				if (wsdl.Services.Count > 0)
					key = "wsdl";
				else
					key = String.Format ("wsdl{0}", wsdl_i ++);

				wsdl_documents [key] = wsdl;
				wsdl_strings [wsdl.TargetNamespace] = key;
			}
			
			string base_url = WsdlUrl.ToString ();
			foreach (WSServiceDescription wsdl in wsdl_documents.Values) {
				foreach (Import import in wsdl.Imports) {
					if (!String.IsNullOrEmpty (import.Location))
						continue;

					import.Location = String.Format ("{0}?wsdl={1}", base_url, wsdl_strings [import.Namespace]);
				}

				foreach (XmlSchema schema in wsdl.Types.Schemas) {
					foreach (XmlSchemaObject obj in schema.Includes) {
						XmlSchemaImport imp = obj as XmlSchemaImport;
						if (imp == null || imp.SchemaLocation != null)
							continue;

						imp.SchemaLocation = String.Format ("{0}?xsd={1}", base_url, xsd_strings [imp.Namespace]);
					}
				}
			}

		}
		
		WSServiceDescription GetWsdl (string which)
		{
			EnsureMetadata ();
			WSServiceDescription wsdl;
			wsdl_documents.TryGetValue (which, out wsdl);
			return wsdl;
		}
		
		XmlSchema GetXmlSchema (string which)
		{
			EnsureMetadata ();
			XmlSchema schema;
			schemas.TryGetValue (which, out schema);
			return schema;
		}

	}

}
