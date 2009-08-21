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
		Dictionary<Uri, ChannelDispatcherBase> _serviceMetadataChanelDispatchers;
		
		[MonoTODO]
		public ServiceMetadataExtension ()
		{
		}

		[MonoTODO]
		public MetadataSet Metadata {
			get {
				if (metadata == null) {
					MetadataExporter exporter = new WsdlExporter ();
					foreach (ServiceEndpoint ep in owner.Description.Endpoints) {
						if (ep.Contract.Name == ServiceMetadataBehavior.MexContractName)
							continue;

						exporter.ExportEndpoint (ep);
					}
					metadata = exporter.GetGeneratedMetadata ();
				}
				return metadata;
			}
		}

		internal ServiceHostBase Owner {
			get { return owner; }
		}

		internal static ServiceMetadataExtension EnsureServiceMetadataExtension (ServiceDescription description, ServiceHostBase serviceHostBase) {
			ServiceMetadataExtension sme = serviceHostBase.Extensions.Find<ServiceMetadataExtension> ();
			if (sme == null) {
				sme = new ServiceMetadataExtension ();
				serviceHostBase.Extensions.Add (sme);
			}
			return sme;
		}

		internal static void EnsureServiceMetadataHttpChanelDispatcher (ServiceDescription description, ServiceHostBase serviceHostBase, ServiceMetadataExtension sme, Uri uri, WCFBinding binding)
		{
			EnsureServiceMetadataDispatcher (description, serviceHostBase, sme, uri, binding ?? MetadataExchangeBindings.CreateMexHttpBinding ());
		}

		internal static void EnsureServiceMetadataHttpsChanelDispatcher (ServiceDescription description, ServiceHostBase serviceHostBase, ServiceMetadataExtension sme, Uri uri, WCFBinding binding)
		{
			// same as http now.
			EnsureServiceMetadataDispatcher (description, serviceHostBase, sme, uri, binding ?? MetadataExchangeBindings.CreateMexHttpsBinding ());
		}

		static void EnsureServiceMetadataDispatcher (ServiceDescription description, ServiceHostBase serviceHostBase, ServiceMetadataExtension sme, Uri uri, WCFBinding binding)
		{
			if (sme._serviceMetadataChanelDispatchers == null)
				sme._serviceMetadataChanelDispatchers = new Dictionary<Uri, ChannelDispatcherBase> ();
			else if (sme._serviceMetadataChanelDispatchers.ContainsKey (uri))
				return;

			CustomBinding cb = new CustomBinding (binding)
			{
				Name = ServiceMetadataBehaviorHttpGetBinding,
			};
			cb.Elements.Find<MessageEncodingBindingElement> ().MessageVersion = MessageVersion.None;

			ServiceEndpoint se = new ServiceEndpoint (ContractDescription.GetContract (typeof (IHttpGetHelpPageAndMetadataContract)), cb, new EndpointAddress (uri))
			{
				ListenUri = uri,
			};

			ChannelDispatcher channelDispatcher = serviceHostBase.BuildChannelDispatcher (se);

			channelDispatcher.Endpoints [0].DispatchRuntime.InstanceContextProvider = new SingletonInstanceContextProvider (new InstanceContext (serviceHostBase, new HttpGetWsdl (sme, uri)));

			sme._serviceMetadataChanelDispatchers.Add (uri, channelDispatcher);
			serviceHostBase.ChannelDispatchers.Add (channelDispatcher);
		}

		void IExtension<ServiceHostBase>.Attach (ServiceHostBase owner)
		{
			this.owner = owner;
		}

		[MonoTODO]
		void IExtension<ServiceHostBase>.Detach (ServiceHostBase owner)
		{
			throw new NotImplementedException ();
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
		ServiceMetadataExtension metadata_extn;
		Uri base_uri;

		Dictionary <string,WSServiceDescription> wsdl_documents = 
			new Dictionary<string, WSServiceDescription> ();
		Dictionary <string, XmlSchema> schemas = 
			new Dictionary<string, XmlSchema> ();

		public HttpGetWsdl (ServiceMetadataExtension metadata_extn, Uri base_uri)
		{
			this.metadata_extn = metadata_extn;
			this.base_uri = base_uri;
			GetMetadata (metadata_extn.Owner);
		}
		
		public SMMessage Get (SMMessage req)
		{
			HttpRequestMessageProperty prop = (HttpRequestMessageProperty) req.Properties [HttpRequestMessageProperty.Name];

			NameValueCollection query_string = CreateQueryString (prop.QueryString);
			if (query_string == null || query_string.AllKeys.Length != 1)
				return CreateHelpPage (req);

			if (query_string [null] == "wsdl") {
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

		SMMessage CreateHelpPage (SMMessage request)
		{
			//FIXME Check for ServiceDebugBehavior.HttpHelpPage
			//else do what? Check
			throw new NotImplementedException ();
		}

		SMMessage CreateWsdlMessage (WSServiceDescription wsdl)
		{
			MemoryStream ms = new MemoryStream ();
			XmlWriter xw = XmlWriter.Create (ms);

			WSServiceDescription.Serializer.Serialize (xw, wsdl);
			ms.Seek (0, SeekOrigin.Begin);
			return SMMessage.CreateMessage (MessageVersion.None, "", XmlReader.Create (ms));
		}

		void GetMetadata (ServiceHostBase host)
		{
			MetadataSet metadata = metadata_extn.Metadata;
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
			
			string base_url = base_uri.ToString ();
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
			WSServiceDescription wsdl;
			wsdl_documents.TryGetValue (which, out wsdl);
			return wsdl;
		}
		
		XmlSchema GetXmlSchema (string which)
		{
			XmlSchema schema;
			schemas.TryGetValue (which, out schema);
			return schema;
		}

	}

}
