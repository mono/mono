using System;
using System.IO;
using System.Collections;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Web.Services.Discovery;
using System.Web.Services.Description;
using System.Net;

public class Driver
{
	static bool foundErrors = false;
	static string basePath;
	static int runningCount;
	
	static ServiceCollection services;
	
	public static void Main (string[] args)
	{
		if (args.Length == 0)
		{
			Console.WriteLine ();
			Console.WriteLine ("Usage: wstest [options]");
			Console.WriteLine ();
			Console.WriteLine ("Options:");
			Console.WriteLine ("  gp: Generate proxies");
			Console.WriteLine ("  gc <url>: Generate test client class");
			Console.WriteLine ("  ur <url>: Update service references from DISCO or WSDL document");
			Console.WriteLine ();
			return;
		}
		
		basePath = ".";
		
		XmlSerializer ser = new XmlSerializer (typeof(ServiceCollection));
		
		string servicesFile = Path.Combine (basePath, "services.xml");
		if (!File.Exists (servicesFile)) 
			services = new ServiceCollection ();
		else
		{
			StreamReader sr = new StreamReader (servicesFile);
			services = (ServiceCollection) ser.Deserialize (sr);
			sr.Close ();
		}
		
		if (args[0] == "gp")
		{
			BuildProxies (GetArg (args,1) == "all");
		}
		else if (args[0] == "gc")
		{
			BuildClients (GetArg (args,1));
		}
		else if (args[0] == "ur")
		{
			UpdateReferences (GetArg (args,1), GetArg (args,2));
		}
		else if (args[0] == "stat")
		{
			ShowStatus ();
		}
		else if (args[0] == "clean")
		{
			Clean ();
		}
		
		StreamWriter sw = new StreamWriter (servicesFile);
		ser.Serialize (sw, services);
		sw.Close ();
		
		if (foundErrors)
			Console.WriteLine ("Please check error log at " + Path.Combine (GetErrorPath(), "error.log"));
	}
	
	static string GetArg (string[] args, int n)
	{
		if (n >= args.Length) return null;
		else return args[n];
	}
	
	static void Cleanup ()
	{
		string pp = GetProxyPath ();
		DirectoryInfo di = new DirectoryInfo (pp);
		if (di.Exists) di.Delete (true);
		di.Create ();
	}
	
	static void Clean ()
	{
		Hashtable clientHash = new Hashtable ();
		
		foreach (ServiceData sd in services.services)
		{
			if (sd.ClientTest)
				clientHash [GetClientFile (sd)] = sd;
				
			foreach (string prot in sd.Protocols)
				clientHash [GetProxyFile (sd, prot)] = sd;
		}
			
		Clean (clientHash, GetClientPath ());
		Clean (clientHash, GetProxyPath ());
	}
	
	static void Clean (Hashtable clientHash, string path)
	{
		if (Path.GetFileName (path) == "CVS") return;
		
		string[] files = Directory.GetFiles (path);

		foreach (string file in files)
		{
			ServiceData sd = clientHash [file] as ServiceData;
			if (sd != null) continue;
			
			File.Delete (file);
			Console.WriteLine ("Deleted file '" + file + "'");
		}
		
		string [] dirs = Directory.GetDirectories (path);
		foreach (string dir in dirs)
			Clean (clientHash, dir);
			
		int count = Directory.GetFiles (path).Length + Directory.GetDirectories (path).Length;
		if (count == 0 && path != GetClientPath () && path != GetProxyPath ())
		{
			Directory.Delete (path);
			Console.WriteLine ("Deleted directory '" + path + "'");
		}
	}
	
	static void UpdateReferences (string url, string ignoreFile)
	{	
		Console.WriteLine ();
		Console.WriteLine ("Updating service references");
		Console.WriteLine ("---------------------------");
		
		ArrayList ignoreList = new ArrayList ();
		
		if (ignoreFile != null)
		{
			StreamReader sr = new StreamReader (ignoreFile);
			string line;
			while ((line = sr.ReadLine ()) != null)
			{
				int i = line.IndexOfAny (new char[] {' ','\t'});
				if (i != -1) line = line.Substring (0,i);
				ignoreList.Add (line);
			}
		}
		
		DiscoveryClientProtocol client = new DiscoveryClientProtocol ();
		client.DiscoverAny (url);
		
		ArrayList list = new ArrayList (client.References.Values);
		foreach (DiscoveryReference re in list)
		{
			bool ignore = ignoreList.Contains (re.Url);
			ServiceData sd = FindService (re.Url);
			if (sd != null)
			{
				if (ignore) RemoveService (re.Url);
				continue;
			}
			
			if (ignore) continue;
			
			Console.Write ("Resolving " + re.Url + " ");
			try
			{
				re.Resolve ();
				Console.WriteLine ("OK");
				
				ServiceDescription doc = client.Documents [re.Url] as ServiceDescription;
				if (doc != null)
					services.services.Add (CreateServiceData (re, doc));
			}
			catch (Exception ex)
			{
				Console.WriteLine ("FAILED");
				ReportError ("Error resolving: " + re.Url, ex.ToString ());
			}
		}
	}
	
	public static ServiceData CreateServiceData (DiscoveryReference dref, ServiceDescription doc)
	{
		ServiceData sd = new ServiceData ();
		
		string name = GetServiceName (dref);
		sd.Name = name;
		int nc = 2;
		while (FindServiceByName (sd.Name) != null)
		{
			sd.Name = name + nc;
			nc++;
		}
		
		sd.Wsdl = dref.Url;
		
		string loc = GetLocation (doc);
		if (loc != null)
		{
			WebResponse res = null;
			try
			{
				res = WebRequest.Create (loc).GetResponse ();
			}
			catch (Exception ex)
			{
				WebException wex = ex as WebException;
				if (wex != null) res = wex.Response;
			}
			if (res != null)
			{
				sd.ServerType = res.Headers ["Server"] + " # " + res.Headers ["X-Powered-By"];
			}
		}
		
		Console.WriteLine (loc + " - " + sd.ServerType);
		
		ArrayList bins = GetBindingTypes (doc);
		sd.Protocols = (string[]) bins.ToArray(typeof(string));
		return sd;
	}

	static ArrayList GetBindingTypes (ServiceDescription doc)
	{
		ServiceDescriptionCollection col = new ServiceDescriptionCollection ();
		col.Add (doc);
		
		ArrayList list = new ArrayList ();
		foreach (Service s in doc.Services)
		{
			foreach (Port p in s.Ports)
			{
				Binding bin = col.GetBinding (p.Binding);
				if (bin.Extensions.Find (typeof (System.Web.Services.Description.SoapBinding)) != null)
					if (!list.Contains ("Soap")) list.Add ("Soap");
					
				HttpBinding ext = (HttpBinding) bin.Extensions.Find (typeof (HttpBinding));
				if (ext != null)
				{
					if (ext.Verb == "POST") list.Add ("HttpPost");
					else list.Add ("HttpGet");
				}
			}
		}
		return list;
	}
	
	static string GetLocation (ServiceDescription doc)
	{
		foreach (Service s in doc.Services)
		{
			foreach (Port p in s.Ports)
			{
				SoapAddressBinding loc = (SoapAddressBinding) p.Extensions.Find (typeof (System.Web.Services.Description.SoapAddressBinding));
				if (loc != null)
					return loc.Location;
			}
		}
		return null;
	}
	
	static string GetServiceName (DiscoveryReference dref)
	{
		string name = dref.DefaultFilename;
		
		if (name.EndsWith (".wsdl")) name = name.Substring (0,name.Length-5);
		if (name == "wsd")
		{
			int i = dref.Url.IndexOf ("service=");
			if (i == -1) name = dref.Url.Substring (7);
			else name = dref.Url.Substring (i+8);
		}
		
		name = name + "Test";
		if (Char.IsDigit (name,0)) name = "Ws" + name;
		return Normalize (name);
	}
	
	static string Normalize (string s)
	{
		System.Text.StringBuilder sb = new System.Text.StringBuilder ();
		for (int n=0; n<s.Length; n++)
			if (Char.IsLetterOrDigit (s[n]) || s[n] == '_') sb.Append (s[n]);
			
		return sb.ToString ();
	}
	
/*	static string GetFileName (DiscoveryReference dref)
	{
		return Path.Combine (classPath, GetServiceName (dref) + ".cs");
	}
*/

	static ServiceData FindService (string url)
	{
		foreach (ServiceData sd in services.services)
			if (sd.Wsdl == url) return sd;

		return null;
	}

	static void RemoveService (string url)
	{
		for (int n=0; n<services.services.Count; n++)
		{
			ServiceData sd  = (ServiceData) services.services [n];
			if (sd.Wsdl == url) { services.services.RemoveAt (n); return; }
		}
	}

	static ServiceData FindServiceByName (string name)
	{
		foreach (ServiceData sd in services.services)
			if (sd.Name == name) return sd;

		return null;
	}
	
	static void ShowStatus ()
	{
		int total = 0;
		int soap = 0;
		int post = 0;
		int get = 0;
		int tests = 0;
		
		Hashtable servers = new Hashtable ();
		
		foreach (ServiceData sd in services.services)
		{
			if (Array.IndexOf(sd.Protocols, "Soap") != -1) soap++;
			if (Array.IndexOf(sd.Protocols, "HttpPost") != -1) post++;
			if (Array.IndexOf(sd.Protocols, "HttpGet") != -1) get++;
			if (sd.ClientTest) tests++;
			
			string st = sd.ServerType;
			if (st == null) st = "Unknown";
			object on = servers [st];
			
			if (on == null)
				servers [st] = 1;
			else
				servers [st] = ((int)on)+1;
		}
		
		Console.WriteLine ("Total services: " + services.services.Count);
		Console.WriteLine ("Soap Protocol: " + soap);
		Console.WriteLine ("HttpPost Protocol: " + post);
		Console.WriteLine ("HttpGet Protocol:  " + get);
		Console.WriteLine ("Total proxies: " + (soap + post + get));
		Console.WriteLine ("Nunit Tests: " + tests);
		Console.WriteLine ();
		Console.WriteLine ("Server Types:");
		
		string[] serverNames = new string[servers.Count];
		int[] serverCounts = new int[servers.Count];
		int n=0;
		
		foreach (DictionaryEntry ent in servers)
		{
			serverNames [n] = (string) ent.Key;
			serverCounts [n++] = (int) ent.Value;
		}
		
		Array.Sort (serverCounts, serverNames);
		for (n=serverNames.Length-1; n >=0; n--)
			Console.WriteLine ("{0,-3} {1}", serverCounts[n], serverNames[n]);
	}

	static void BuildProxies (bool buildAll)
	{
		Console.WriteLine ();
		Console.WriteLine ("Generating proxies");
		Console.WriteLine ("------------------");
		
		ArrayList proxies = new ArrayList ();
		
		foreach (ServiceData fd in services.services)
			BuildProxy (fd, buildAll, proxies);
		
		StreamWriter sw = new StreamWriter (Path.Combine (basePath, "proxies.sources"));
		foreach (string f in proxies)
			sw.WriteLine (f);
		sw.Close ();
	}
	
	static void BuildProxy (ServiceData fd, bool rebuild, ArrayList proxies)
	{
		string wsdl = fd.Wsdl;
		
		if (fd.Protocols == null)
		{
			ReportError ("Client test '" + fd.Name + "': no protocols declared", null);
			return;
		}
		
		foreach (string prot in fd.Protocols)
		{
			string ns = fd.Namespace;
			ns = CodeIdentifier.MakeValid (ns) + "." + prot;
		
			string pfile = GetProxyFile (fd, prot);
			if (File.Exists (pfile) && !rebuild) { proxies.Add (pfile); continue; }
			
			CreateFolderForFile	(pfile);

			Console.Write (prot + " proxy for " + wsdl + "... ");
			Process proc = new Process ();
			proc.StartInfo.RedirectStandardOutput = true;
			proc.StartInfo.RedirectStandardError = true;
			proc.StartInfo.FileName = "wsdl";
			proc.StartInfo.Arguments = "-out:" + pfile + " -nologo -namespace:" + ns + " -protocol:" + prot + " " + wsdl;
			proc.Start();
			proc.WaitForExit ();
			
			if (proc.ExitCode != 0)
			{
				Console.WriteLine ("FAIL");
				
				string err = proc.StandardOutput.ReadToEnd ();
				err += "\n" + proc.StandardError.ReadToEnd ();
				
				if (proc.ExitCode == 1) {
					string fn = fd.Name + prot + "Proxy.cs";
					fn = Path.Combine (GetErrorPath(), fn);
					CreateFolderForFile (fn);
					File.Move (pfile, fn);
					
					StreamWriter sw = new StreamWriter (fn, true);
					sw.WriteLine ();
					sw.WriteLine ("// " + fd.Wsdl);
					sw.WriteLine ();
					sw.Close ();
					
				}
				else
					File.Delete (pfile);
					
				ReportError ("Errors found while generating " + prot + " proxy for WSDL: " + wsdl, err);
			}
			else
			{
				Console.WriteLine ("OK");
				proxies.Add (pfile);
			}
		}
	}
	
	static void BuildClients (string wsdl)
	{
		StreamWriter sw = new StreamWriter (Path.Combine (basePath, "client.sources"));
		
		foreach (ServiceData fd in services.services)
		{
			if (wsdl != null && fd.Wsdl == wsdl) fd.ClientTest = true;
			
			if (!fd.ClientTest) continue;
			
			BuildClient (fd);
			sw.WriteLine (GetClientFile (fd));
		}
		
		sw.Close ();
	}
	
	public static void BuildClient (ServiceData sd)
	{
		string file = GetClientFile (sd);
		
		if (File.Exists (file)) return;

		CreateFolderForFile (file);
		
		StreamWriter sw = new StreamWriter (file);
		sw.WriteLine ("// Web service test for WSDL document:");
		sw.WriteLine ("// " + sd.Wsdl);
		
		sw.WriteLine ();
		sw.WriteLine ("using System;");
		sw.WriteLine ("using NUnit.Framework;");
		
		foreach (string prot in sd.Protocols)
			sw.WriteLine ("using " + sd.Namespace + "." + prot + ";");
		
		sw.WriteLine ();
		sw.WriteLine ("namespace " + sd.Namespace);
		sw.WriteLine ("{");
		sw.WriteLine ("\tpublic class " + sd.Name + ": WebServiceTest");
		sw.WriteLine ("\t{");
		sw.WriteLine ("\t\t[Test]");
		sw.WriteLine ("\t\tpublic void TestService ()");
		sw.WriteLine ("\t\t{");
		sw.WriteLine ("\t\t}");
		sw.WriteLine ("\t}");
		sw.WriteLine ("}");
		sw.Close ();
		
		Console.WriteLine ("Written file '" + file + "'");
	}
	
	static string GetProxyFile (ServiceData fd, string protocol)
	{
		string fn = Path.Combine (new Uri (fd.Wsdl).Host, fd.Name + protocol + "Proxy.cs");
		return Path.Combine (GetProxyPath(), fn);
	}
	
	static string GetClientFile (ServiceData sd)
	{
		return Path.Combine (GetClientPath(), sd.TestFile);
	}
	
	static void ReportError (string error, string detail)
	{
		string fn = Path.Combine (GetErrorPath(), "error.log");
		CreateFolderForFile (fn);

		StreamWriter sw = new StreamWriter (fn, true);
		sw.WriteLine ("*** " + error);
		sw.WriteLine ("   " + detail.Replace ("\n","\n   "));
		sw.WriteLine ();
		sw.Close ();
		foundErrors = true;
	}
	
	static string GetProxyPath ()
	{
		return Path.Combine (basePath, "proxies");
	}
	
	static string GetClientPath ()
	{
		return Path.Combine (basePath, "client");
	}
	
	static string GetErrorPath ()
	{
		return Path.Combine (basePath, "error");
	}
	
	static void CreateFolderForFile (string file)
	{
		string dir = Path.GetDirectoryName (file);
		DirectoryInfo di = new DirectoryInfo (dir);
		if (!di.Exists) di.Create ();
	}
}

[XmlType("services")]
public class ServiceCollection
{
	[XmlElement("service", typeof(ServiceData))]
	public ArrayList services = new ArrayList ();
}

[XmlType("service")]
public class ServiceData
{
	[XmlElement("wsdl")]
	public string Wsdl;
	
	[XmlElement("name")]
	public string Name;
	
	[XmlElement("serverType")]
	public string ServerType;
	
	[XmlArray("protocols")]
	[XmlArrayItem("protocol")]
	public string[] Protocols;
	
	[XmlElement("clientTest")]
	public bool ClientTest;
	
	[XmlIgnore]
	public string Namespace
	{
		get { return Name + "s"; }
	}
	
	[XmlIgnore]
	public string TestFile
	{
		get 
		{ 
			string dir = new Uri (Wsdl).Host;
			return Path.Combine (dir, Name + ".cs");
		}
	}
}
