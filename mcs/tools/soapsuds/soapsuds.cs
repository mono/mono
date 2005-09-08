// 
// soapsuds.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) 2003 Ximian, Inc.
//

using System;
using System.Reflection;
using System.Collections;
using System.Runtime.Remoting.MetadataServices;
using System.Net;
using System.IO;

public class Driver
{
	static void Main (string[] args)
	{
		Runner run = new Runner (args);
		AppDomain domain = AppDomain.CreateDomain ("runner", null, Directory.GetCurrentDirectory(), "", false);
		domain.DoCallBack (new CrossAppDomainDelegate (run.Main));
	}
}

[Serializable]
class Runner
{
	static bool logo = true;
	static string inputUrl = null;
	static string inputTypes = null;
	static string inputSchema = null;
	static string inputAssembly = null;
	static string inputDirectory = null;

	static string serviceEndpoint = null;
	static string outputSchema = null;
	static string outputDirectory = null;
	static string outputAssembly = null;
	static bool outputCode = false;

	static bool wrappedProxy = true;
	static string proxyNamespace = null;
	static string strongNameFile = null;

	static string userName = null;
	static string password = null;
	static string domain = null;
	static string httpProxyName = null;
	static string httpProxyPort = null;
	
	string[] args;
	
	public Runner (string[] args)
	{
		this.args = args;
	}
	
	public void Main ()
	{
		try
		{
			ReadParameters (args);
			
			if (logo) 
				WriteLogo ();
			
			if (args.Length == 0 || args[0] == "--help")
			{
				WriteHelp ();
				return;
			}
			
			ArrayList types = new ArrayList ();
			Assembly assembly = null;
			
			if (inputAssembly != null)
			{
				assembly = Assembly.LoadFile (inputAssembly);
				foreach (Type t in assembly.GetTypes ())
					types.Add (new ServiceType (t, serviceEndpoint));
			}
			
			if (inputTypes != null)
			{
				string[] ts = inputTypes.Split (';');
				foreach (string type in ts)
				{
					Type t = null;
					string url = null;

					string[] typeParts = type.Split (',');
					if (typeParts.Length == 1)
						throw new Exception ("Type assembly not specified");

					if (typeParts.Length >= 2)
					{
						t = Type.GetType (typeParts[0] + ", " + typeParts [1]);
						if (typeParts.Length > 2)
							url = typeParts [2];
					}
					types.Add (new ServiceType (t, url));
				}
			}
			
			ArrayList writtenFiles = new ArrayList ();
			MemoryStream schemaStream = null;
			
			if (types.Count > 0)
			{
				schemaStream = new MemoryStream ();
				MetaData.ConvertTypesToSchemaToStream ((ServiceType[]) types.ToArray (typeof(ServiceType)), SdlType.Wsdl, schemaStream);
			}
			
			if (inputUrl != null)
			{
				if (schemaStream != null) throw new Exception ("Only one type source can be specified");
				schemaStream = new MemoryStream ();
				MetaData.RetrieveSchemaFromUrlToStream (inputUrl, schemaStream);
			}
			
			if (inputSchema != null)
			{
				if (schemaStream != null) throw new Exception ("Only one type source can be specified");
				schemaStream = new MemoryStream ();
				
				FileStream fs = new FileStream (inputSchema, FileMode.Open, FileAccess.Read);
				byte[] buffer = new byte [1024*5];
				int nr = 0;
				while ((nr = fs.Read (buffer, 0, buffer.Length)) > 0)
					schemaStream.Write (buffer, 0, nr);
			}
			
			if (outputSchema != null)
			{
				if (schemaStream == null) throw new Exception ("No input schema or assembly has been specified");
				
				schemaStream.Position = 0;
				MetaData.SaveStreamToFile (schemaStream, outputSchema);
				Console.WriteLine ("Written file " + outputSchema);
			}
			
			if (outputCode)
			{
				if (schemaStream == null) throw new Exception ("No input schema or assembly has been specified");
				
				schemaStream.Position = 0;
				MetaData.ConvertSchemaStreamToCodeSourceStream (wrappedProxy, outputDirectory, schemaStream, writtenFiles, null, null);
			}
			
			if (outputAssembly != null)
			{
				if (schemaStream == null) throw new Exception ("No input schema or assembly has been specified");
				
				schemaStream.Position = 0;
				if (outputCode)
					MetaData.ConvertCodeSourceStreamToAssemblyFile (writtenFiles, outputAssembly, strongNameFile);
				else
				{
					MetaData.ConvertSchemaStreamToCodeSourceStream (wrappedProxy, outputDirectory, schemaStream, writtenFiles, null, null);
					MetaData.ConvertCodeSourceStreamToAssemblyFile (writtenFiles, outputAssembly, strongNameFile);
					foreach (string file in writtenFiles)
						File.Delete (file);
					writtenFiles.Clear ();
				}
				writtenFiles.Add (outputAssembly);
			}
			
			foreach (string fn in writtenFiles)
				Console.WriteLine ("Written file " + fn);
		}
		catch (Exception ex)
		{
			Console.WriteLine ("ERROR: " + ex.Message);
			if (ex.GetType() != typeof(Exception))
				Console.WriteLine (ex);
		}
		Console.WriteLine ();
	}

	static void WriteLogo ()
	{
		Console.WriteLine ("Mono SOAPSUDS Tool");
		Console.WriteLine ();
	}
	
	static void WriteHelp ()
	{
		Console.WriteLine ("Usage: soapsuds [inputs] [outputs] [options]");
		Console.WriteLine ();
		Console.WriteLine ("Inputs:");
		Console.WriteLine ("   -url urltoschema:url             Url from which to retrieve the schema");
		Console.WriteLine ("   -types:type1,assembly[,serviceEndpoint][;type2,assembly,...] ");
		Console.WriteLine ("                                    List of types from which to generate");
		Console.WriteLine ("                                    a schema or proxy");
		Console.WriteLine ("   -ia -inputassemblyfile:assembly  Assembly that contains the types to export");
		Console.WriteLine ("   -is -inputschemafile:schemafile  Schema from which to generate proxy classes");
		Console.WriteLine ();
		Console.WriteLine ("Input Options:");
		Console.WriteLine ("   -id -inputdirectory:directory    Directory where DLLs are located");
		Console.WriteLine ("   -se -serviceendpoint:url         Url of the service to be placed in the");
		Console.WriteLine ("                                    WSDL document");
		Console.WriteLine ();
		Console.WriteLine ("Outputs:");
		Console.WriteLine ("   -oa -outputassemblyfile:assembly Generate an assembly");
		Console.WriteLine ("   -os -outputschemafile:file       Generate a schema");
		Console.WriteLine ("   -gc -generatecode                Generate proxy source code");
		Console.WriteLine ();
		Console.WriteLine ("Output Options:");
		Console.WriteLine ("   -od -outputdirectory:directory   Directory where output will be generated");
		Console.WriteLine ("   -pn -proxynamespace:namespace    Namespace of the generated proxy");
		Console.WriteLine ("   -nowp -nowrappedproxy            Generate a wrapped proxy");
		Console.WriteLine ("   -wp -wrappedproxy                Generate a wrapped proxy");
		Console.WriteLine ("   -sn -strongnamefile:snfile       Strong name file");
		Console.WriteLine ();
		Console.WriteLine ("General Options:");
		Console.WriteLine ("   -u -username:name                User name for server authentication");
		Console.WriteLine ("   -p -password:pwd                 Password for server authentication");
		Console.WriteLine ("   -d -domain:domain                Domain of the server");
		Console.WriteLine ("   -hpn -httpProxyName:name         Name of http proxy");
		Console.WriteLine ("   -hpp -httpProxyPort:port         Port of http proxy");
		Console.WriteLine ("   -nologo                          Supress the startup logo");
		Console.WriteLine ();
	}
	
	static void ReadParameters (string[] args)
	{
		NetworkCredential cred = new NetworkCredential ();
		NetworkCredential proxyCred = new NetworkCredential ();
		WebProxy proxy = new WebProxy ();

		
		
		foreach (string arg in args)
		{
			if (!arg.StartsWith ("/") && !arg.StartsWith ("-"))
				continue;
			
			string parg = arg.Substring (1);
			int i = parg.IndexOf (":");
			string param = null;
			if (i != -1) {
				param = parg.Substring (i+1);
				parg = parg.Substring (0,i);
			}
			
			switch (parg.ToLower ())
			{
				case "nologo":
					logo = false;
					break;
					
				case "urltoschema": case "url":
					inputUrl = param;
					break;
					
				case "types":
					inputTypes = param;
					break;
					
				case "inputassemblyfile": case "ia":
					inputAssembly = param;
					break;
					
				case "outputassemblyfile": case "oa":
					outputAssembly = param;
					break;
					
				case "inputdirectory": case "id":
					inputDirectory = param;
					break;
					
				case "outputdirectory": case "od":
					outputDirectory = param;
					break;
					
				case "inputschemafile": case "is":
					inputSchema = param;
					break;
					
				case "outputschemafile": case "os":
					outputSchema = param;
					break;
					
				case "proxynamespace": case "pn":
					proxyNamespace = param;
					break;
					
				case "serviceendpoint": case "se":
					serviceEndpoint = param;
					break;
					
				case "strongnamefile": case "sn":
					strongNameFile = param;
					break;

				case "nowrappedproxy": case "nowp":
					wrappedProxy = false;
					break;
					
				case "wrappedproxy": case "wp":
					wrappedProxy = true;
					break;
					
				case "generatecode": case "gc":
					outputCode = true;
					break;
					
				case "username": case "u":
					userName = param;
					break;
					
				case "password": case "p":
					password = param;
					break;
					
				case "domain": case "d":
					domain = param;
					break;
					
				case "httpProxyName": case "hpn":
					httpProxyName = param;
					break;
					
				case "httpProxyPort": case "hpp":
					httpProxyPort = param;
					break;
			}
		}
	}
}
