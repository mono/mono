///
/// MonoWSDL.cs -- a WSDL to proxy code generator.
///
/// Author: Erik LeBel (eriklebel@yahoo.ca)
/// 		Lluis Sanchez (lluis@novell.com)
///
/// Copyright (C) 2003, Erik LeBel,
///

#if NET_2_0

using System;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Collections;
using System.Collections.Specialized;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Net;
using System.Web.Services.Description;
using System.Web.Services.Discovery;
using System.Web.Services;

using Microsoft.CSharp;

namespace Mono.WebServices
{
	public class Driver
	{
		string ProductId = "Web Services Description Language Utility\nMono Framework v" + Environment.Version;
		const string UsageMessage = 
			"wsdl [options] {path | URL} {path | URL} ...\n\n"
			+ "   -d, -domain:domain           Domain of username for server authentication.\n"
			+ "   -l, -language:language       Language of generated code. Allowed CS (default)\n"
			+ "                                and VB. You can also specify the fully qualified\n"
			+ "                                name of a class that implements the\n"
			+ "                                System.CodeDom.Compiler.CodeDomProvider Class.\n"
			+ "   -n, -namespace:ns            The namespace of the generated code, default\n"
			+ "                                namespace if none.\n"
			+ "   -nologo                      Surpress the startup logo.\n"
			+ "   -o, -out:filename            The target file for generated code.\n"
			+ "   -p, -password:pwd            Password used to contact the server.\n"
			+ "   -protocol:protocol           Protocol to implement. Allowed: Soap (default),\n"
			+ "                                HttpGet or HttpPost.\n"
			+ "   -fields                      Generate fields instead of properties in data\n"
			+ "                                classes.\n"
			+ "   -server                      Generate server instead of client proxy code.\n"
			+ "   -u, -username:username       Username used to contact the server.\n"
			+ "   -proxy:url                   Address of the proxy.\n"
			+ "   -pu, -proxyusername:username Username used to contact the proxy.\n"
			+ "   -pp, -proxypassword:pwd      Password used to contact the proxy.\n"
			+ "   -pd, -proxydomain:domain     Domain of username for proxy authentication.\n"
			+ "   -urlkey, -appsettingurlkey:key Configuration key that contains the default\n"
			+ "                                url for the generated WS proxy.\n"
			+ "   -baseurl, -appsettingbaseurl:url Base url to use when constructing the\n"
			+ "                                service url.\n"
			+ "   -sample:[binding/]operation  Display a sample SOAP request and response.\n"
			+ "   -?                           Display this message\n"
			+ "\n"
			+ "Options can be of the forms  -option, --option or /option\n";
		
		ArrayList descriptions = new ArrayList ();
		ArrayList schemas = new ArrayList ();
		
		bool noLogo;
		bool help;
		string sampleSoap;
		
		string proxyAddress;
		string proxyDomain;
		string proxyPassword;
		string proxyUsername;
		string username;
		string password;
		string domain;
		
		string applicationSignature;
		string appSettingURLKey;
		string appSettingBaseURL;
		string language = "CS";
		string ns;
		string outFilename;
		string protocol = "Soap";
		ServiceDescriptionImportStyle style;
		CodeGenerationOptions options = CodeGenerationOptions.GenerateProperties | CodeGenerationOptions.GenerateNewAsync;
		bool verbose;
		
		StringCollection urls = new StringCollection ();

		///
		/// <summary>
		///	Application entry point.
		/// </summary>
		///
		public static int Main(string[] args)
		{
			Driver d = new Driver();
			return d.Run(args);
		}
		
		Driver()
		{
			applicationSignature = ProductId;
		}
		
		int Run (string[] args)
		{
			try
			{
				// parse command line arguments
				foreach (string argument in args)
					ImportArgument(argument);
				
				if (noLogo == false)
					Console.WriteLine(ProductId);
				
				if (help || urls.Count == 0)
				{
					Console.WriteLine(UsageMessage);
					return 0;
				}
				
				CodeCompileUnit codeUnit = new CodeCompileUnit();
				CodeNamespace proxyCode = GetCodeNamespace();
				codeUnit.Namespaces.Add (proxyCode);
				
				WebReferenceCollection references = new WebReferenceCollection ();

				DiscoveryClientProtocol dcc = CreateClient ();

				foreach (string murl in urls) 
				{

					string url = murl;
					if (!url.StartsWith ("http://") && !url.StartsWith ("https://") && !url.StartsWith ("file://"))
						url = new Uri (Path.GetFullPath (url)).ToString ();

					dcc.DiscoverAny (url);
					dcc.ResolveAll ();
					
				}
				
				WebReference reference = new WebReference (dcc.Documents, proxyCode, protocol, appSettingURLKey, appSettingBaseURL);
				references.Add (reference);
				
				if (sampleSoap != null)
					ConsoleSampleGenerator.Generate (descriptions, schemas, sampleSoap, protocol);
				
				if (sampleSoap != null)
					return 0;
					
				// generate the code
				GenerateCode (references, codeUnit);
				return 0;
			}
			catch (Exception exception)
			{
				Console.WriteLine("Error: {0}", exception.Message);
				
				// Supress this except for when debug is enabled
				Console.WriteLine("Stack:\n {0}", exception.StackTrace);
				return 2;
			}
		}
		
		///
		/// <summary>
		///	Generate code for the specified ServiceDescription.
		/// </summary>
		///
		public bool GenerateCode (WebReferenceCollection references, CodeCompileUnit codeUnit)
		{
			bool hasWarnings = false;
			
			CodeDomProvider provider = GetProvider();
				
			StringCollection validationWarnings;
			WebReferenceOptions opts = new WebReferenceOptions ();
			opts.CodeGenerationOptions = options;
			opts.Style = style;
			opts.Verbose = verbose;
			validationWarnings = ServiceDescriptionImporter.GenerateWebReferences (references, provider, codeUnit, opts);
			
			for (int n=0; n<references.Count; n++)
			{
				WebReference wr  = references [n];
				
				BasicProfileViolationCollection violations = new BasicProfileViolationCollection ();
				if (String.Compare (protocol, "SOAP", StringComparison.OrdinalIgnoreCase) == 0 && !WebServicesInteroperability.CheckConformance (WsiProfiles.BasicProfile1_1, wr, violations)) {
					wr.Warnings |= ServiceDescriptionImportWarnings.WsiConformance;
				}
				
				if (wr.Warnings != 0)
				{
					if (!hasWarnings) {
						WriteText ("", 0, 0);
						WriteText ("There were some warnings while generating the code:", 0, 0);
					}
					
					WriteText ("", 0, 0);
					WriteText (urls[n], 2, 2);
					
					if ((wr.Warnings & ServiceDescriptionImportWarnings.WsiConformance) > 0) {
						WriteText ("- This web reference does not conform to WS-I Basic Profile v1.1", 4, 6); 
						foreach (BasicProfileViolation vio in violations) {
							WriteText (vio.NormativeStatement + ": " + vio.Details, 8, 8);
							foreach (string ele in vio.Elements)
								WriteText ("* " + ele, 10, 12);
						}
					}
					
					if ((wr.Warnings & ServiceDescriptionImportWarnings.NoCodeGenerated) > 0)
						WriteText ("- WARNING: No proxy class was generated", 4, 6); 
					if ((wr.Warnings & ServiceDescriptionImportWarnings.NoMethodsGenerated) > 0)
						WriteText ("- WARNING: The proxy class generated includes no methods", 4, 6);
					if ((wr.Warnings & ServiceDescriptionImportWarnings.OptionalExtensionsIgnored) > 0)
						WriteText ("- WARNING: At least one optional extension has been ignored", 4, 6);
					if ((wr.Warnings & ServiceDescriptionImportWarnings.RequiredExtensionsIgnored) > 0)
						WriteText ("- WARNING: At least one necessary extension has been ignored", 4, 6);
					if ((wr.Warnings & ServiceDescriptionImportWarnings.UnsupportedBindingsIgnored) > 0)
						WriteText ("- WARNING: At least one binding is of an unsupported type and has been ignored", 4, 6);
					if ((wr.Warnings & ServiceDescriptionImportWarnings.UnsupportedOperationsIgnored) > 0)
						WriteText ("- WARNING: At least one operation is of an unsupported type and has been ignored", 4, 6);
						
					hasWarnings = true;
				}
			}
			
			if (hasWarnings) WriteText ("",0,0);
				
			string filename = outFilename;
			bool hasBindings = false;
			
			foreach (object doc in references[0].Documents.Values)
			{
				ServiceDescription desc = doc as ServiceDescription;
				if (desc == null) continue;
				
				if (desc.Services.Count > 0 && filename == null)
					filename = desc.Services[0].Name + "." + provider.FileExtension;
					
				if (desc.Bindings.Count > 0 || desc.Services.Count > 0)
					hasBindings = true;
			}
			
			if (filename == null)
				filename = "output." + provider.FileExtension;
			
			if (hasBindings) {
				WriteText ("Writing file '" + filename + "'", 0, 0);
				StreamWriter writer = new StreamWriter(filename);
				
				CodeGeneratorOptions compilerOptions = new CodeGeneratorOptions();
				provider.GenerateCodeFromCompileUnit (codeUnit, writer, compilerOptions);
				writer.Close();
			}
			
			return hasWarnings;
		}
		
		///
		/// <summary>
		///	Create the CodeNamespace with the generator's signature commented in.
		/// </summary>
		///
		CodeNamespace GetCodeNamespace()
		{
			CodeNamespace codeNamespace = new CodeNamespace(ns);
			
			if (applicationSignature != null)
			{
				codeNamespace.Comments.Add(new CodeCommentStatement("\n This source code was auto-generated by " + applicationSignature + "\n"));
			}
			
			return codeNamespace;
		}
		
		///
		/// <summary/>
		///
		void WriteCodeUnit(CodeCompileUnit codeUnit, string serviceName)
		{
			CodeDomProvider provider = GetProvider();
			ICodeGenerator generator = provider.CreateGenerator();
			CodeGeneratorOptions options = new CodeGeneratorOptions();
			
			string filename;
			if (outFilename != null)
				filename = outFilename;
			else
				filename = serviceName	+ "." + provider.FileExtension;
			
			Console.WriteLine ("Writing file '{0}'", filename);
			StreamWriter writer = new StreamWriter(filename);
			generator.GenerateCodeFromCompileUnit(codeUnit, writer, options);
			writer.Close();
		}
		
		///
		/// <summary>
		///	Fetch the Code Provider for the language specified by the 'language' members.
		/// </summary>
		///
		private CodeDomProvider GetProvider()
		{
			CodeDomProvider provider;
			Type type;
			
			switch (language.ToUpper ()) {
			case "CS":
				provider = new CSharpCodeProvider ();
				break;
			case "VB":
				provider = new Microsoft.VisualBasic.VBCodeProvider ();
				break;
			case "BOO":
				type = Type.GetType("Boo.Lang.CodeDom.BooCodeProvider, Boo.Lang.CodeDom, Version=1.0.0.0, Culture=neutral, PublicKeyToken=32c39770e9a21a67");
				if (type != null){
					return (CodeDomProvider) Activator.CreateInstance (type);
				}
				throw new Exception ("Boo.Lang.CodeDom.BooCodeProvider not available");
				
			default:
				type = Type.GetType(language);
				if (type != null) {
					return (CodeDomProvider) Activator.CreateInstance (type);
				}	
				throw new Exception ("Unknown language");
			}
			return provider;
		}
		


		///
		/// <summary>
		///	Interperet the command-line arguments and configure the relavent components.
		/// </summary>
		///		
		void ImportArgument(string argument)
		{
			string optionValuePair;
			
			if (argument.StartsWith("--"))
			{
				optionValuePair = argument.Substring(2);
			}
			else if (argument.StartsWith("/") || argument.StartsWith("-"))
			{
				optionValuePair = argument.Substring(1);
			}
			else
			{
				urls.Add (argument);
				return;
			}
			
			string option;
			string value;
			
			int indexOfEquals = optionValuePair.IndexOf(':');
			if (indexOfEquals > 0)
			{
				option = optionValuePair.Substring(0, indexOfEquals);
				value = optionValuePair.Substring(indexOfEquals + 1);
			}
			else
			{
				option = optionValuePair;
				value = null;
			}
			
			switch (option)
			{
				case "appsettingurlkey":
				case "urlkey":
				    appSettingURLKey = value;
				    break;

				case "appsettingbaseurl":
				case "baseurl":
				    appSettingBaseURL = value;
				    break;

				case "d":
				case "domain":
				    domain = value;
				    break;

				case "l":
				case "language":
				    language = value;
				    break;

				case "n":
				case "namespace":
				    ns = value;
				    break;

				case "nologo":
				    noLogo = true;
				    break;

				case "o":
				case "out":
				    outFilename = value;
				    break;

				case "p":
				case "password":
				    password = value;
				    break;

				case "protocol":
				    protocol = value;
				    break;

				case "proxy":
				    proxyAddress = value;
				    break;

				case "proxydomain":
				case "pd":
				    proxyDomain = value;
				    break;

				case "proxypassword":
				case "pp":
				    proxyPassword = value;
				    break;

				case "proxyusername":
				case "pu":
				    proxyUsername = value;
				    break;

				case "server":
				    style = ServiceDescriptionImportStyle.Server;
				    break;

				case "u":
				case "username":
				    username = value;
				    break;
					
				case "verbose":
					verbose = true;
					break;
					
				case "fields":
					options &= ~CodeGenerationOptions.GenerateProperties;
					break;
					
				case "sample":
					sampleSoap = value;
					break;

				case "?":
				    help = true;
				    break;

				default:
					if (argument.StartsWith ("/") && argument.IndexOfAny (Path.InvalidPathChars) == -1) {
						urls.Add (argument);
						break;
					}
					else
					    throw new Exception("Unknown option " + option);
			}
		}
		
		DiscoveryClientProtocol CreateClient ()
		{
			DiscoveryClientProtocol dcc = new DiscoveryClientProtocol ();
			
			if (username != null || password != null || domain != null)
			{
				NetworkCredential credentials = new NetworkCredential();
				
				if (username != null)
					credentials.UserName = username;
				
				if (password != null)
					credentials.Password = password;
				
				if (domain != null)
					credentials.Domain = domain;
				
				dcc.Credentials = credentials;
			}
			
			if (proxyAddress != null)
			{
				WebProxy proxy = new WebProxy (proxyAddress);
				if (proxyUsername != null || proxyPassword != null || proxyDomain != null)
				{
					NetworkCredential credentials = new NetworkCredential();
					
					if (proxyUsername != null)
						credentials.UserName = proxyUsername;
					
					if (proxyPassword != null)
						credentials.Password = proxyPassword;
					
					if (proxyDomain != null)
						credentials.Domain = proxyDomain;
					
					proxy.Credentials = credentials;
				}
			}			
			
			return dcc;
		}
		
		static void WriteText (string text, int initialLeftMargin, int leftMargin)
		{
			int n = 0;
			int margin = initialLeftMargin;
			int maxCols = 80;
			
			if (text == "") {
				Console.WriteLine ();
				return;
			}
			
			while (n < text.Length)
			{
				int col = margin;
				int lastWhite = -1;
				int sn = n;
				while (col < maxCols && n < text.Length) {
					if (char.IsWhiteSpace (text[n]))
						lastWhite = n;
					col++;
					n++;
				}
				
				if (lastWhite == -1 || col < maxCols)
					lastWhite = n;
				else if (col >= maxCols)
					n = lastWhite + 1;
				
				Console.WriteLine (new String (' ', margin) + text.Substring (sn, lastWhite - sn));
				margin = leftMargin;
			}
		}
	}
}

#endif
