///
/// MonoWSDL.cs -- a WSDL to proxy code generator.
///
/// Author: Erik LeBel (eriklebel@yahoo.ca)
/// 		Lluis Sanchez (lluis@ximian.com)
///
/// Copyright (C) 2003, Erik LeBel,
///

using System;
using System.Xml;
using System.Xml.Schema;
using System.Collections;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Services.Description;

using Microsoft.CSharp;

namespace Mono.WebServices
{
	///
	/// <summary>
	///	Document retriever.
	///
	///	By extanciating this class, and setting URL and the optional Username, Password and Domain
	///	properties, the Document property can be used to retrieve the document at the specified 
	///	address.
	///
	///	If none of Username, Password or Domain are specified the DocumentRetriever attempts
	///	to fetch the document without providing any authentication credentials. If at least one of 
	///	these values is provided, then the retrieval process is attempted with an authentication
	///	
	/// </summary>
	///	
	internal class DocumentRetriever
	{
		string url	= null;
		string domain	= null;
		string password = null;
		string username = null;
		
		ArrayList readLocations = new ArrayList ();
		
		///
		/// <summary>
		///	Set the URL from which the document will be retrieved.
		/// </summary>
		///
		public string URL
		{
			set 
			{
				if (url != null)
					throw new Exception("Too many document sources");
				
				url = value;
			}
			get
			{
				return url;
			}
		}
		
		///
		/// <summary>
		///	Specify the username to be used.
		/// </summary>
		///
		public string Username
		{
			set { username = value; }
		}
		
		///
		/// <summary/>
		///
		public string Password
		{
			set { password = value; }
		}
		
		///
		/// <summary/>
		///
		public string Domain
		{
			set { domain = value; }
		}
		
		///
		/// <summary>
		///	This property returns the document found at the DocumentRetriever's URL.
		/// </summary>
		///
		public Stream GetStream ()
		{
			return GetStream (url);
		}
		
		public Stream GetStream (string documentUrl)
		{
			WebClient webClient = new WebClient();
			
			if (username != null || password != null || domain != null)
			{
				NetworkCredential credentials = new NetworkCredential();
				
				if (username != null)
					credentials.UserName = username;
				
				if (password != null)
					credentials.Password = password;
				
				if (domain != null)
					credentials.Domain = domain;
				
				webClient.Credentials = credentials;
			}
			
			readLocations.Add (documentUrl);

			try
			{
				Console.WriteLine ("Fetching " + documentUrl);
				return webClient.OpenRead (documentUrl);
			}
			catch (Exception ex)
			{
				throw new Exception ("Could not read document from url " + documentUrl + ". " + ex.Message);
			}
		}
		
		public bool AlreadyDownloaded (string documentUrl)
		{
			return readLocations.Contains (documentUrl);
		}
	}
	
	///
	/// <summary>
	///	Source code generator.
	/// </summary>
	///
	class SourceGenerator
	{
		string applicationSiganture 	= null;
		string appSettingURLKey		= null;
		string appSettingBaseURL	= null;
		string language			= "CS";
		string ns			= null;
		string outFilename		= null;
		string protocol			= "Soap";
		bool   server			= false;
		
		///
		/// <summary/>
		///
		public string Language 
		{
			// FIXME validate
			set { language = value; }
		}
		
		///
		/// <summary/>
		///
		public string Namespace 
		{
			set { ns = value; }
		}
		
		///
		/// <summary>
		///	The file to contain the generated code.
		/// </summary>
		///
		public string Filename
		{
			set { outFilename = value; }
		}
		
		///
		/// <summary/>
		///
		public string Protocol
		{
			// FIXME validate
			set { protocol = value; }
		}
		
		///
		/// <summary/>
		///
		public string ApplicationSignature
		{
			set { applicationSiganture = value; }
		}
		
		///
		/// <summary/>
		///
		public string AppSettingURLKey
		{
			set { appSettingURLKey = value; }
		}
		
		///
		/// <summary/>
		///
		public string AppSettingBaseURL 
		{
			set { appSettingBaseURL  = value; }
		}
		
		///
		/// <summary/>
		///
		public bool Server
		{
			set { server = value; }
		}
		
		///
		/// <summary>
		///	Generate code for the specified ServiceDescription.
		/// </summary>
		///
		public bool GenerateCode (ArrayList descriptions, ArrayList schemas)
		{
			// FIXME iterate over each serviceDescription.Services?
			CodeNamespace codeNamespace = GetCodeNamespace();
			CodeCompileUnit codeUnit = new CodeCompileUnit();
			bool hasWarnings = false;
			
			codeUnit.Namespaces.Add(codeNamespace);

			ServiceDescriptionImporter importer = new ServiceDescriptionImporter();
			importer.ProtocolName = protocol;
			if (server)
				importer.Style = ServiceDescriptionImportStyle.Server;
			
			foreach (ServiceDescription sd in descriptions)
				importer.AddServiceDescription(sd, appSettingURLKey, appSettingBaseURL);
				
			foreach (XmlSchema sc in schemas)
				importer.Schemas.Add (sc);
			
			ServiceDescriptionImportWarnings warnings = importer.Import(codeNamespace, codeUnit);
			if (warnings != 0)
			{
				if ((warnings & ServiceDescriptionImportWarnings.NoCodeGenerated) > 0)
					Console.WriteLine ("WARNING: No proxy class was generated"); 
				if ((warnings & ServiceDescriptionImportWarnings.NoMethodsGenerated) > 0)
					Console.WriteLine ("WARNING: The proxy class generated includes no methods");
				if ((warnings & ServiceDescriptionImportWarnings.OptionalExtensionsIgnored) > 0)
					Console.WriteLine ("WARNING: At least one optional extension has been ignored");
				if ((warnings & ServiceDescriptionImportWarnings.RequiredExtensionsIgnored) > 0)
					Console.WriteLine ("WARNING: At least one necessary extension has been ignored");
				if ((warnings & ServiceDescriptionImportWarnings.UnsupportedBindingsIgnored) > 0)
					Console.WriteLine ("WARNING: At least one binding is of an unsupported type and has been ignored");
				if ((warnings & ServiceDescriptionImportWarnings.UnsupportedOperationsIgnored) > 0)
					Console.WriteLine ("WARNING: At least one operation is of an unsupported type and has been ignored");
				hasWarnings = true;
			}
			
			string serviceName = ((ServiceDescription)descriptions[0]).Services[0].Name;
			WriteCodeUnit(codeUnit, serviceName);
			
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
			
			if (applicationSiganture != null)
			{
				codeNamespace.Comments.Add(new CodeCommentStatement("\n This source code was auto-generated by " + applicationSiganture + "\n"));
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
			// FIXME these should be loaded dynamically using reflection
			CodeDomProvider provider;
			
			switch (language.ToUpper())
			{
			    case "CS":
				    provider = new CSharpCodeProvider();
				    break;
			    
				case "VB":
					provider = new Microsoft.VisualBasic.VBCodeProvider();
					break;
					
			    default:
				    throw new Exception("Unknow language");
			}

			return provider;
		}
	}
	
	///
	/// <summary>
	///	monoWSDL's main application driver. Reads the command-line arguments and dispatch the
	///	appropriate handlers.
	/// </summary>
	///
	public class Driver
	{
		const string ProductId = "Mono Web Services Description Language Utility";
		const string UsageMessage = 
			"wsdl [options] {path | URL} \n\n"
			+ "   -appsettingurlkey:key        (short -urlkey)\n"
			+ "   -appsettingbaseurl:baseurl   (short -baseurl)\n"
			+ "   -domain:domain (short -d)    Domain of username for server authentication\n"
			+ "   -language:language           Language of generated code. Allowed CS\n"
			+ "                                (default) (short -l)\n"
			+ "   -namespace:ns                The namespace of the generated code, default\n"
			+ "                                NS if none (short -n)\n"
			+ "   -nologo                      Surpress the startup logo\n"
			+ "   -out:filename                The target file for generated code \n"
			+ "                                (short -o)\n"
			+ "   -password:pwd                Password used to contact server (short -p)\n"
			+ "   -protocol:protocol           Protocol to implement. Allowed: Soap \n"
			+ "                                (default), HttpGet, HttpPost\n"
			+ "   -server                      Generate server instead of client proxy code.\n"
			+ "   -username:username           Username used to contact server (short -u)\n"
			+ "   -?                           Display this message\n"
			+ "\n"
			+ "Options can be of the forms  -option, --option or /option\n";
		
		DocumentRetriever retriever = null;
		SourceGenerator generator = null;
		
		ArrayList descriptions = new ArrayList ();
		ArrayList schemas = new ArrayList ();
		
		bool noLogo = false;
		bool help = false;
		bool hasURL = false;
		
		// FIXME implement these options
		// (are they are usable by the System.Net.WebProxy class???)
		string proxy = null;
		string proxyDomain = null;
		string proxyPassword = null;
		string proxyUsername = null;

		///
		/// <summary>
		///	Initialize the document retrieval component and the source code generator.
		/// </summary>
		///
		Driver()
		{
			retriever = new DocumentRetriever();
			generator = new SourceGenerator();
			generator.ApplicationSignature = ProductId;
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
				hasURL = true;
				retriever.URL = argument;
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
				    generator.AppSettingURLKey = value;
				    break;

				case "appsettingbaseurl":
				case "baseurl":
				    generator.AppSettingBaseURL = value;
				    break;

				case "d":
				case "domain":
				    retriever.Domain = value;
				    break;

				case "l":
				case "language":
				    generator.Language = value;
				    break;

				case "n":
				case "namespace":
				    generator.Namespace = value;
				    break;

				case "nologo":
				    noLogo = true;
				    break;

				case "o":
				case "out":
				    generator.Filename = value;
				    break;

				case "p":
				case "password":
				    retriever.Password = value;
				    break;

				case "protocol":
				    generator.Protocol = value;
				    break;

				case "proxy":
				    proxy = value;
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
				    generator.Server = true;
				    break;

				case "u":
				case "username":
				    retriever.Username = value;
				    break;

				case "?":
				    help = true;
				    break;

				default:
				    throw new Exception("Unknown option " + option);
			}
		}

		///
		/// <summary>
		///	Driver's main control flow:
		///	 - parse arguments
		///	 - report required messages
		///	 - terminate if no input
		///	 - report errors
		/// </summary>
		///
		int Run(string[] args)
		{
			try
			{
				// parse command line arguments
				foreach (string argument in args)
				{
					ImportArgument(argument);
				}
				
				if (noLogo == false)
					Console.WriteLine(ProductId);
				
				if (help || !hasURL)
				{
					Console.WriteLine(UsageMessage);
					return 0;
				}
				
				// fetch the document
				using (Stream stream = retriever.GetStream ())
				{
					// import the document as a ServiceDescription
					XmlTextReader xtr = new XmlTextReader (stream);
					xtr.MoveToContent ();
					if (xtr.LocalName != "definitions") throw new Exception ("The document at '" + retriever.URL + "' is not a valid WSDL document");
					ServiceDescription serviceDescription = ServiceDescription.Read (xtr);
					xtr.Close ();
					ReadDocuments (serviceDescription);
				}
				
				// generate the code
				if (generator.GenerateCode (descriptions, schemas))
					return 1;
				else
					return 0;
			}
			catch (Exception exception)
			{
				Console.WriteLine("Error: {0}", exception.Message);
				// FIXME: surpress this except for when debug is enabled
				//Console.WriteLine("Stack:\n {0}", exception.StackTrace);
				return 2;
			}
		}
		
		void ReadDocuments (ServiceDescription serviceDescription)
		{
			descriptions.Add (serviceDescription);
			
			foreach (Import import in serviceDescription.Imports)
			{
				if (retriever.AlreadyDownloaded (import.Location)) continue;
				using (Stream stream = retriever.GetStream (import.Location))
				{
					XmlTextReader reader = new XmlTextReader (stream);
					reader.MoveToContent ();
					
					if (reader.LocalName == "definitions")
					{
						ServiceDescription desc = ServiceDescription.Read (reader);
						ReadDocuments (desc);
					}
					else
					{
						XmlSchema schema = XmlSchema.Read (reader, null);
						schemas.Add (schema);
					}
				}
			}
		}
		
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
	}
}
