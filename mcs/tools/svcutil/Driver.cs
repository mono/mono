using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Collections.ObjectModel;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Web.Services;
using System.Web.Services.Discovery;

using System.Collections.Generic;
using System.Runtime.Serialization;

using WSServiceDescrition = System.Web.Services.Description.ServiceDescription;

namespace Mono.ServiceContractTool
{
	public class Driver
	{
		public static void Main (string [] args)
		{
			new Driver ().Run (args);
		}

		CommandLineOptions co = new CommandLineOptions ();
		ServiceContractGenerator generator;
		CodeDomProvider code_provider;

		void Run (string [] args)
		{
			co.ProcessArgs (args);
			if (co.Usage) {
				co.DoUsage ();
				return;
			}

			if (co.Version) {
				co.DoVersion ();
				return;
			}

			if (co.Help || co.RemainingArguments.Count == 0) {
				co.DoHelp ();
				return;
			}
			if (!co.NoLogo)
				co.ShowBanner ();

			CodeCompileUnit ccu = new CodeCompileUnit ();
			CodeNamespace cns = new CodeNamespace (co.Namespace);
			ccu.Namespaces.Add (cns);

			generator = new ServiceContractGenerator (ccu);
			generator.Options = GetGenerationOption ();
			generator.Options |=ServiceContractGenerationOptions.ChannelInterface;

			code_provider = GetCodeProvider ();
			MetadataSet metadata = null;

			// For now only assemblyPath is supported.
			foreach (string arg in co.RemainingArguments) {
				Uri uri = null;
				if (Uri.TryCreate (arg, UriKind.Absolute, out uri)) {
					metadata = ResolveWithDisco (arg);
					if (metadata == null)
						metadata = ResolveWithWSMex (arg);

					continue;
				}

				FileInfo fi = new FileInfo (arg);
				if (!fi.Exists)
				switch (fi.Extension) {
				case ".exe":
				case ".dll":
					GenerateContractType (fi.FullName);
					break;
				default:
					throw new NotSupportedException ("Not supported file extension: " + fi.Extension);
				}
			}

			if (metadata == null)
				return;
			
			List<IWsdlImportExtension> list = new List<IWsdlImportExtension> ();
			list.Add (new TransportBindingElementImporter ());
			//list.Add (new DataContractSerializerMessageContractImporter ());
			list.Add (new XmlSerializerMessageContractImporter ());

			//WsdlImporter importer = new WsdlImporter (metadata, null, list);
			WsdlImporter importer = new WsdlImporter (metadata);
			ServiceEndpointCollection endpoints = importer.ImportAllEndpoints ();
			Collection<ContractDescription> contracts = new Collection<ContractDescription> ((from se in endpoints select se.Contract).ToArray ());

			Console.WriteLine ("Generating files..");

			// FIXME: could better become IWsdlExportExtension
			foreach (ContractDescription cd in contracts) {
				if (co.GenerateMoonlightProxy) {
					var moonctx = new MoonlightChannelBaseContext ();
					cd.Behaviors.Add (new MoonlightChannelBaseContractExtension (moonctx, co.GenerateMonoTouchProxy));
					foreach (var od in cd.Operations)
						od.Behaviors.Add (new MoonlightChannelBaseOperationExtension (moonctx, co.GenerateMonoTouchProxy));
					generator.GenerateServiceContractType (cd);
					moonctx.Fixup ();
				}
				else
					generator.GenerateServiceContractType (cd);
			}

			/*if (cns.Types.Count == 0) {
				Console.Error.WriteLine ("Argument assemblies have no types.");
				Environment.Exit (1);
			}*/

			//FIXME: Generate .config 

			Console.WriteLine (GetOutputFilename ());
			using (TextWriter w = File.CreateText (GetOutputFilename ())) {
				code_provider.GenerateCodeFromCompileUnit (ccu, w, null);
			}
		}

		MetadataSet ResolveWithDisco (string url)
		{
			DiscoveryClientProtocol prot = null;
			Console.WriteLine ("\nAttempting to download metadata from '{0}' using DISCO..", url);
			try { 
				prot = new DiscoveryClientProtocol ();
				prot.DiscoverAny (url);
				prot.ResolveAll ();
			} catch (Exception e) {
				Console.WriteLine ("Disco failed for the url '{0}' with exception :\n {1}", url, e.Message);
				return null;
			}

			if (prot.References.Count > 0)
			{
				Console.WriteLine ("Disco found documents at the following URLs:");
				foreach (DiscoveryReference refe in prot.References.Values)
				{
					if (refe is ContractReference) Console.Write ("- WSDL document at  ");
					else if (refe is DiscoveryDocumentReference) Console.Write ("- DISCO document at ");
					else Console.Write ("- Xml Schema at    ");
					Console.WriteLine (refe.Url);
				}
			} else {
				Console.WriteLine ("Disco didn't find any document at the specified URL");
				return null;
			}

			MetadataSet metadata = new MetadataSet ();
			foreach (object o in prot.Documents.Values) {
				if (o is WSServiceDescrition) {
					metadata.MetadataSections.Add (
						new MetadataSection (MetadataSection.ServiceDescriptionDialect, "", (WSServiceDescrition) o));
				}
				if (o is XmlSchema) {
					metadata.MetadataSections.Add (
						new MetadataSection (MetadataSection.XmlSchemaDialect, "", (XmlSchema) o));
				}
			}

			return metadata;
		}

		MetadataSet ResolveWithWSMex (string url)
		{
			MetadataSet metadata = null;
			try {
				MetadataExchangeClient client = new MetadataExchangeClient (new EndpointAddress (url));

				Console.WriteLine ("\nAttempting to download metadata from {0} using WS-MetadataExchange..", url);
				metadata = client.GetMetadata ();
			} catch (InvalidOperationException e) {
				//MetadataExchangeClient wraps exceptions, thrown while
				//fetching the metadata, in an InvalidOperationException
				string msg;
				if (e.InnerException == null)
					msg = e.Message;
				else
					msg = e.InnerException.ToString ();

				Console.WriteLine ("WS-MetadataExchange query failed for the url '{0}' with exception :\n {1}",
					url, msg);
			}

			return metadata;
		}

		CodeDomProvider GetCodeProvider ()
		{
			switch (co.Language) {
			case "csharp":
			case "cs":
				return new Microsoft.CSharp.CSharpCodeProvider ();
			case "vb":
				return new Microsoft.VisualBasic.VBCodeProvider ();
			default:
				throw new NotSupportedException ();
			}
		}

		void GenerateContractType (string file)
		{
			Assembly ass = Assembly.LoadFile (file);
			foreach (Module m in ass.GetModules ())
				foreach (Type t in m.GetTypes ())
					ProcessType (t);
		}

		void ProcessType (Type type)
		{
			object [] a = type.GetCustomAttributes (
				typeof (ServiceContractAttribute), true);
			if (a.Length > 0)
				generator.GenerateServiceContractType (
					ContractDescription.GetContract (type));
		}

		ServiceContractGenerationOptions GetGenerationOption ()
		{
			ServiceContractGenerationOptions go =
				ServiceContractGenerationOptions.ClientClass;
			if (co.GenerateAsync)
				go |= ServiceContractGenerationOptions.AsynchronousMethods;
			if (co.ChannelInterface)
				go |= ServiceContractGenerationOptions.ChannelInterface;
			if (co.GenerateTypesAsInternal)
				go |= ServiceContractGenerationOptions.InternalTypes;
			if (co.GenerateProxy)
				go |= ServiceContractGenerationOptions.ClientClass;
			if (co.GenerateTypedMessages)
				go |= ServiceContractGenerationOptions.TypedMessages;
			if ((co.TargetClientVersion35 && co.GenerateAsync) || co.GenerateMoonlightProxy)
				go |= ServiceContractGenerationOptions.EventBasedAsynchronousMethods;

			return go;
		}

		string GetOutputFilename ()
		{
			if (co.OutputFilename != null)
				return co.OutputFilename;
			return "output." + code_provider.FileExtension;
		}
	}
}
