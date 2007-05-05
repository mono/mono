// 
// disco.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) 2003 Ximian, Inc.
//

using System;
using System.Net;
using System.Web.Services.Discovery;
using System.IO;

public class Driver
{
	static bool save = true;
	static bool logo = true;
	static string url;
	static string directory = ".";
	static DiscoveryClientProtocol prot;
	
	static void Main (string[] args)
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
			
			if (url == null) throw new Exception ("URL to discover not provided");
			
			prot.DiscoverAny (url);
			prot.ResolveAll ();
			
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
			}
			else
				Console.WriteLine ("Disco didn't find any document at the specified URL");
			
			if (save)
			{
				DiscoveryClientResultCollection col = prot.WriteAll (directory, "results.discomap");
				Console.WriteLine ();
				Console.WriteLine ("The following files hold the content found at the corresponding URLs:");
				foreach (DiscoveryClientResult res in col)
					Console.WriteLine ("- " + res.Filename + " <- " + res.Url);
				Console.WriteLine ();
				Console.WriteLine ("The file " + Path.Combine (directory,"results.discomap") + " holds links to each of there files");
				Console.WriteLine ();
			}				
		}
		catch (Exception ex)
		{
			Console.WriteLine ("ERROR: " + ex.Message);
			Console.WriteLine (ex);
		}
	}

	static void WriteLogo ()
	{
		Console.WriteLine ("Mono Web Service Discovery Tool " + Consts.MonoVersion);
		Console.WriteLine ();
	}
	
	static void WriteHelp ()
	{
		Console.WriteLine ("Usage: disco [options] url");
		Console.WriteLine ();
		Console.WriteLine ("Options:");
		Console.WriteLine ("   -nologo               Supress the startup logo");
		Console.WriteLine ("   -nosave               Do not save the discovered documents to disk.");
		Console.WriteLine ("                         The default is to save the documents.");
		Console.WriteLine ("   -o -out:directory     The directory where to save the discovered documents.");
		Console.WriteLine ("                         By default, documents are saved in the current");
		Console.WriteLine ("                         directory.");
		Console.WriteLine ("   -u -username:username  ");
		Console.WriteLine ("   -p -password:password  ");
		Console.WriteLine ("   -d -domain:domain     The credentials to use when connecting to the server.");
		Console.WriteLine ("   -proxy:url            The url of the proxy server to use for http requests.");
		Console.WriteLine ("   -proxyusername:name");
		Console.WriteLine ("   -proxypassword:pwd");
		Console.WriteLine ("   -proxydomin:domain    The credentials to use when connection to the proxy.");
		Console.WriteLine ();
	}
	
	static void ReadParameters (string[] args)
	{
		prot = new DiscoveryClientProtocol ();
		NetworkCredential cred = new NetworkCredential ();
		NetworkCredential proxyCred = new NetworkCredential ();
		WebProxy proxy = new WebProxy ();
		url = null;
		
		foreach (string arg in args)
		{
			if (arg.StartsWith ("/") || arg.StartsWith ("-"))
			{
				string parg = arg.Substring (1);
				int i = parg.IndexOf (":");
				string param = null;
				if (i != -1) {
					param = parg.Substring (i+1);
					parg = parg.Substring (0,i);
				}
				
				switch (parg)
				{
					case "nologo":
						logo = false;
						break;
						
					case "nosave":
						save = false;
						break;
						
					case "out":	case "o":
						directory = param;
						break;
						
					case "username": case "u":
						cred.UserName = param;
						break;
						
					case "password": case "p":
						cred.Password = param;
						break;
						
					case "domain": case "d":
						cred.Domain = param;
						break;
						
					case "proxy":
						proxy.Address = new Uri (param);
						break;
						
					case "proxyusername":
						proxyCred.UserName = param;
						break;
						
					case "proxypassword":
						proxyCred.Password = param;
						break;
						
					case "proxydomain":
						proxyCred.Domain = param;
						break;
				}
				
				if (cred.UserName != null)
					prot.Credentials = cred;
					
				if (proxyCred.UserName != null)
					proxy.Credentials = proxyCred;
					
				if (proxy.Address != null)
					prot.Proxy = proxy;
			}
			else
			{
				url = arg;
			}
		}
	}
}
