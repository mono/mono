//
// MetadataProvider.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2012 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#if !MOBILE && !XAMMAC_4_5
using System;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Configuration;
using WS = System.Web.Services.Description;

namespace MonoTests.System.ServiceModel.MetadataTests {

	public static class MetadataSamples  {

		internal const string HttpUri = "http://tempuri.org/TestHttp/";
		internal const string HttpsUri = "https://tempuri.org/TestHttps/";
		internal const string NetTcpUri = "net-tcp://tempuri.org:8000/TestNetTcp/";
		internal const string CustomUri = "custom://tempuri.org:8000/Test/";

		[MetadataSample]
		public static MetadataSet BasicHttp ()
		{
			var exporter = new WsdlExporter ();
			
			var cd = new ContractDescription ("MyContract");
			
			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, new BasicHttpBinding (), new EndpointAddress (HttpUri)));
			
			return exporter.GetGeneratedMetadata ();
		}

		[MetadataSample]
		public static MetadataSet BasicHttp_TransportSecurity ()
		{
			var exporter = new WsdlExporter ();
			
			var cd = new ContractDescription ("MyContract");
			
			var binding = new BasicHttpBinding ();
			binding.Security.Mode = BasicHttpSecurityMode.Transport;
			
			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, binding, new EndpointAddress (HttpUri)));
			
			return exporter.GetGeneratedMetadata ();
		}

		[MetadataSample]
		public static MetadataSet BasicHttp_MessageSecurity ()
		{
			var exporter = new WsdlExporter ();
			
			var cd = new ContractDescription ("MyContract");
			
			var binding = new BasicHttpBinding ();
			binding.Security.Mode = BasicHttpSecurityMode.Message;
			binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.Certificate;
			
			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, binding, new EndpointAddress (HttpUri)));
			
			return exporter.GetGeneratedMetadata ();
		}
		
		[MetadataSample]
		public static MetadataSet BasicHttp_TransportWithMessageCredential ()
		{
			var exporter = new WsdlExporter ();
			
			var cd = new ContractDescription ("MyContract");
			
			var binding = new BasicHttpBinding ();
			binding.Security.Mode = BasicHttpSecurityMode.TransportWithMessageCredential;
			
			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, binding, new EndpointAddress (HttpUri)));
			
			return exporter.GetGeneratedMetadata ();
		}
		
		[MetadataSample]
		public static MetadataSet BasicHttp_Mtom ()
		{
			var exporter = new WsdlExporter ();
			
			var cd = new ContractDescription ("MyContract");
			
			var binding = new BasicHttpBinding ();
			binding.MessageEncoding = WSMessageEncoding.Mtom;
			
			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, binding, new EndpointAddress (HttpUri)));
			
			return exporter.GetGeneratedMetadata ();
		}

		[MetadataSample]
		public static MetadataSet BasicHttp_NtlmAuth ()
		{
			var exporter = new WsdlExporter ();
			
			var cd = new ContractDescription ("MyContract");
			
			var binding = new BasicHttpBinding (BasicHttpSecurityMode.TransportCredentialOnly);
			binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;
			
			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, binding, new EndpointAddress (HttpUri)));
			
			return exporter.GetGeneratedMetadata ();
		}
		
		[MetadataSample]
		public static MetadataSet BasicHttps ()
		{
			var exporter = new WsdlExporter ();
			
			var cd = new ContractDescription ("MyContract");
			
			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, new BasicHttpsBinding (), new EndpointAddress (HttpsUri)));
			
			return exporter.GetGeneratedMetadata ();
		}
		
		[MetadataSample]
		public static MetadataSet BasicHttps_NtlmAuth ()
		{
			var exporter = new WsdlExporter ();
			
			var cd = new ContractDescription ("MyContract");
			
			var binding = new BasicHttpsBinding ();
			
			binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;
			
			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, binding, new EndpointAddress (HttpsUri)));
			
			return exporter.GetGeneratedMetadata ();
		}
		
		[MetadataSample]
		public static MetadataSet BasicHttps_Certificate ()
		{
			var exporter = new WsdlExporter ();
			
			var cd = new ContractDescription ("MyContract");
			
			var binding = new BasicHttpsBinding ();
			binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;
			
			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, binding, new EndpointAddress (HttpsUri)));
			
			return exporter.GetGeneratedMetadata ();
		}
		
		[MetadataSample]
		public static MetadataSet BasicHttps_TransportWithMessageCredential ()
		{
			var exporter = new WsdlExporter ();
			
			var cd = new ContractDescription ("MyContract");
			
			var binding = new BasicHttpsBinding (BasicHttpsSecurityMode.TransportWithMessageCredential);
			
			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, binding, new EndpointAddress (HttpsUri)));
			
			return exporter.GetGeneratedMetadata ();
		}
		
		[MetadataSample]
		public static MetadataSet NetTcp ()
		{
			var exporter = new WsdlExporter ();
			
			var cd = new ContractDescription ("MyContract");
			
			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, new NetTcpBinding (SecurityMode.None, false),
				new EndpointAddress (NetTcpUri)));
			
			return exporter.GetGeneratedMetadata ();
		}
		
		[MetadataSample]
		public static MetadataSet NetTcp_TransportSecurity ()
		{
			var exporter = new WsdlExporter ();
			
			var cd = new ContractDescription ("MyContract");
			
			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, new NetTcpBinding (SecurityMode.Transport, false),
				new EndpointAddress (NetTcpUri)));
			
			return exporter.GetGeneratedMetadata ();
		}
		
		[MetadataSample]
		public static MetadataSet NetTcp_MessageSecurity ()
		{
			var exporter = new WsdlExporter ();
			
			var cd = new ContractDescription ("MyContract");
			
			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, new NetTcpBinding (SecurityMode.Message, false),
				new EndpointAddress (NetTcpUri)));
			
			return exporter.GetGeneratedMetadata ();
		}
		
		[MetadataSample]
		public static MetadataSet NetTcp_TransportWithMessageCredential ()
		{
			var exporter = new WsdlExporter ();
			
			var cd = new ContractDescription ("MyContract");
			
			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, new NetTcpBinding (SecurityMode.TransportWithMessageCredential, false),
				new EndpointAddress (NetTcpUri)));
			
			return exporter.GetGeneratedMetadata ();
		}

		[MetadataSample]
		public static MetadataSet NetTcp_ReliableSession ()
		{
			var exporter = new WsdlExporter ();
			
			var cd = new ContractDescription ("MyContract");
			
			var binding = new NetTcpBinding (SecurityMode.None, true);
			
			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, binding, new EndpointAddress (NetTcpUri)));
			
			return exporter.GetGeneratedMetadata ();
		}
		
		[MetadataSample]
		public static MetadataSet NetTcp_TransferMode ()
		{
			var exporter = new WsdlExporter ();
			
			var cd = new ContractDescription ("MyContract");
			
			var binding = new NetTcpBinding (SecurityMode.None, false);
			binding.TransferMode = TransferMode.Streamed;

			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, binding, new EndpointAddress (NetTcpUri)));
			
			return exporter.GetGeneratedMetadata ();
		}

		[ServiceContract]
		public interface IMyContract {
			[OperationContract]
			void Hello ();
		}

		[MetadataSample]
		public static MetadataSet BasicHttp_Operation ()
		{
			var exporter = new WsdlExporter ();

			var cd = ContractDescription.GetContract (typeof (IMyContract));

			var binding = new BasicHttpBinding ();

			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, binding, new EndpointAddress (HttpUri)));

			return exporter.GetGeneratedMetadata ();
		}

		[MetadataSample]
		public static MetadataSet NetTcp_Operation ()
		{
			var exporter = new WsdlExporter ();
			
			var cd = ContractDescription.GetContract (typeof (IMyContract));
			
			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, new NetTcpBinding (SecurityMode.None, false),
				new EndpointAddress (NetTcpUri)));
			
			return exporter.GetGeneratedMetadata ();
		}

		[MetadataSample (CreateConfig = true)]
		public static MetadataSet BasicHttp_Config ()
		{
			var exporter = new WsdlExporter ();
			
			var cd = ContractDescription.GetContract (typeof (IMyContract));
			
			var binding = new BasicHttpBinding ();
			
			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, binding, new EndpointAddress (HttpUri)));
			
			return exporter.GetGeneratedMetadata ();
		}

		[MetadataSample (CreateConfig = true)]
		public static MetadataSet BasicHttp_Config2 ()
		{
			var exporter = new WsdlExporter ();
			
			var cd = ContractDescription.GetContract (typeof (IMyContract));
			
			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, new BasicHttpBinding (),
				new EndpointAddress (HttpUri)));
			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, new NetTcpBinding (SecurityMode.None, false),
				new EndpointAddress (NetTcpUri)));
			
			return exporter.GetGeneratedMetadata ();
		}

		#region Helper API

		public static void Export (string outputDir)
		{
			if (!Directory.Exists (outputDir))
				Directory.CreateDirectory (outputDir);

			var bf = BindingFlags.Public | BindingFlags.Static;
			foreach (var method in typeof (MetadataSamples).GetMethods (bf)) {
				MetadataSampleAttribute sampleAttr = null;
				foreach (var obj in method.GetCustomAttributes (false)) {
					var cattr = obj as MetadataSampleAttribute;
					if (cattr != null) {
						sampleAttr = cattr;
						break;
					}
				}

				if (sampleAttr == null)
					continue;

				var name = sampleAttr.Name ?? method.Name;
				var metadata = (MetadataSet)method.Invoke (null, null);

				var xmlFilename = Path.Combine (outputDir, name + ".xml");
				TestContext.SaveMetadata (xmlFilename, metadata);

				if (!sampleAttr.CreateConfig)
					continue;

				var configFilename = Path.Combine (outputDir, name + ".config");
				TestContext.GenerateConfig (configFilename, metadata);
			}
		}

		public static MetadataSet GetMetadataByName (string name)
		{
			if (name.EndsWith (".xml"))
				name = name.Substring (name.Length - 4);

			var bf = BindingFlags.Public | BindingFlags.Static;
			foreach (var method in typeof (MetadataSamples).GetMethods (bf)) {
				MetadataSampleAttribute sampleAttr = null;
				foreach (var obj in method.GetCustomAttributes (false)) {
					var cattr = obj as MetadataSampleAttribute;
					if (cattr != null) {
						sampleAttr = cattr;
						break;
					}
				}
				
				if (sampleAttr == null)
					continue;
				
				if (!name.Equals (sampleAttr.Name ?? method.Name))
					continue;

				return (MetadataSet)method.Invoke (null, null);
			}

			throw new InvalidOperationException (string.Format (
				"No such metadata sample: '{0}'", name));
		}

		public class MetadataSampleAttribute : Attribute {
			
			public MetadataSampleAttribute ()
			{
			}
			
			public MetadataSampleAttribute (string name)
			{
				Name = name;
			}
			
			public string Name {
				get; set;
			}

			public bool CreateConfig {
				get; set;
			}
			
		}

		#endregion
	}
}
#endif
