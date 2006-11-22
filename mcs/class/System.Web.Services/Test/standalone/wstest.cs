using System;
using System.IO;
using System.Collections;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
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
	static ServiceStatusCollection serviceStatus;
	
	public static void Main (string[] args)
	{
		if (args.Length == 0)
		{
			Console.WriteLine ();
			Console.WriteLine ("Usage: wstest [options]");
			Console.WriteLine ();
			Console.WriteLine ("Options:");
			Console.WriteLine ("  ur <url>: Update service references from DISCO or WSDL document");
			Console.WriteLine ("  dw: download wsdl documents for registered services");
			Console.WriteLine ("  gp: Generate proxies");
			Console.WriteLine ("  gc <url>: Generate test client class");
			Console.WriteLine ();
			return;
		}
		
		basePath = ".";
		
		LoadInfo ();
		
		if (args[0] == "gp")
		{
			BuildProxies (GetArg (args,1));
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
		else if (args[0] == "dw")
		{
			DownloadWsdls (GetArg (args,1));
		}
		else if (args[0] == "clean")
		{
			Clean ();
		}
		else if (args[0] == "checkdiff")
		{
			CheckDiff (GetArg (args,1));
		}
		else if (args[0] == "msfix")
		{
			MsFix (GetArg (args,1));
		}
		else if (args[0] == "genfiles")
		{
			GenerateFiles (GetArg (args,1));
		}
		
		SaveInfo ();
		
		if (foundErrors)
			Console.WriteLine ("Please check error log at " + Path.Combine (GetErrorPath(), "error.log"));
	}
	
	static void LoadInfo ()
	{
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
		
		ser = new XmlSerializer (typeof(ServiceStatusCollection));
		
		servicesFile = Path.Combine (basePath, "serviceStatus.xml");
		if (!File.Exists (servicesFile)) 
			serviceStatus = new ServiceStatusCollection ();
		else
		{
			StreamReader sr = new StreamReader (servicesFile);
			serviceStatus = (ServiceStatusCollection) ser.Deserialize (sr);
			sr.Close ();
		}
	}
	
	static void SaveInfo ()
	{
		XmlSerializer ser = new XmlSerializer (typeof(ServiceCollection));
		string servicesFile = Path.Combine (basePath, "services.xml");
		StreamWriter sw = new StreamWriter (servicesFile);
		ser.Serialize (sw, services);
		sw.Close ();
		
		ser = new XmlSerializer (typeof(ServiceStatusCollection));
		servicesFile = Path.Combine (basePath, "serviceStatus.xml");
		sw = new StreamWriter (servicesFile);
		ser.Serialize (sw, serviceStatus);
		sw.Close ();
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
				
			if (sd.Protocols != null)
			{
				foreach (string prot in sd.Protocols)
					clientHash [GetProxyFile (sd, prot)] = sd;
			}
			clientHash [GetWsdlFile (sd)] = sd;
		}
			
		Clean (clientHash, GetClientPath ());
		Clean (clientHash, GetProxyPath ());
		Clean (clientHash, GetWsdlPath ());
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
			if (!(re is ContractReference)) continue;
			
			bool ignore = ignoreList.Contains (re.Url);
			
			ServiceData sd = FindService (re.Url);
			
			if (ignore)
			{
				if (sd != null) {
					RemoveService (re.Url);
					Console.WriteLine ("Removed " + re.Url);
				}
				continue;
			}
			
			if (sd == null)
			{
				sd = CreateServiceData (re);
				Console.WriteLine ("Added " + re.Url);
				services.services.Add (sd);
			}
		}
		Console.WriteLine ("Done");
		Console.WriteLine ();
	}
	
	static void DownloadWsdls (string host)
	{
		Console.WriteLine ();
		Console.WriteLine ("Downloading WSDL documents");
		Console.WriteLine ("---------------------------");
		
		foreach (ServiceData sd in services.services) {
			if (host != null && host != new Uri (sd.Wsdl).Host) continue;
			if (!File.Exists (GetWsdlFile(sd)))
				Resolve (sd);
		}
				
		Console.WriteLine ("Done");
		Console.WriteLine ();
	}
	
	static void Resolve (ServiceData sd)
	{
		Console.Write ("Resolving " + sd.Wsdl + " ");
		try
		{
			DiscoveryClientProtocol contract = new DiscoveryClientProtocol ();
			contract.DiscoverAny (sd.Wsdl);
			
			if (sd.Protocols == null || sd.Protocols.Length==0) 
				RetrieveServiceData (sd, contract);
				
			string wsdlFile = GetWsdlFile (sd);
			CreateFolderForFile (wsdlFile);
			
			ServiceDescription doc = (ServiceDescription) contract.Documents [sd.Wsdl];
			doc.Write (wsdlFile);
			
			Console.WriteLine ("OK");
			CleanFailures (sd);
		}
		catch (Exception ex)
		{
			Console.WriteLine ("FAILED");
			ReportError ("Error resolving: " + sd.Wsdl, ex.ToString ());
			RegisterFailure (sd);
		}
	}
	
	public static ServiceData CreateServiceData (DiscoveryReference dref)
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
		return sd;
	}
	
	public static void RetrieveServiceData (ServiceData sd, DiscoveryClientProtocol contract)
	{
		ServiceDescriptionCollection col = new ServiceDescriptionCollection ();
		foreach (object doc in contract.Documents.Values)
			if (doc is ServiceDescription) col.Add ((ServiceDescription)doc);
		
		string loc = GetLocation (col[0]);
		if (loc != null)
		{
			WebResponse res = null;
			try
			{
				WebRequest req = (WebRequest)WebRequest.Create (loc);
				req.Timeout = 15000;
				res = req.GetResponse ();
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
		
		ArrayList bins = GetBindingTypes (col);
		sd.Protocols = (string[]) bins.ToArray(typeof(string));
	}
	
	static ArrayList GetBindingTypes (ServiceDescriptionCollection col)
	{
		ServiceDescription doc = col [0];
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
			if (sd.Protocols != null)
			{
				if (Array.IndexOf(sd.Protocols, "Soap") != -1) soap++;
				if (Array.IndexOf(sd.Protocols, "HttpPost") != -1) post++;
				if (Array.IndexOf(sd.Protocols, "HttpGet") != -1) get++;
			}
			
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

	static void BuildProxies (string host)
	{
		Console.WriteLine ();
		Console.WriteLine ("Generating proxies");
		Console.WriteLine ("------------------");
		
		XmlDocument doc = new XmlDocument ();
		XmlElement ele = doc.CreateElement ("errors");
		doc.AppendChild (ele);
		
		bool buildAll = host == "*";
		if (buildAll) host = null;
		
		ArrayList proxies = new ArrayList ();
		
		foreach (ServiceData fd in services.services) {
			if (host != null && host != new Uri (fd.Wsdl).Host) continue;
			BuildProxy (fd, buildAll, proxies, ele);
		}
		
		StreamWriter sw = new StreamWriter (Path.Combine (basePath, "proxies.sources"));
		foreach (string f in proxies)
			sw.WriteLine (f);
		sw.Close ();
		
		doc.Save ("proxy-gen-error.xml");		
		Console.WriteLine ("Done");
		Console.WriteLine ();
	}
	
	static void BuildProxy (ServiceData fd, bool rebuild, ArrayList proxies, XmlElement errdoc)
	{
		string wsdl = GetWsdlFile (fd);
		if (!File.Exists (wsdl)) 
			return;
		
		if (fd.Protocols == null)
		{
			ReportError ("Client test '" + fd.Name + "': no protocols declared", "");
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
			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.RedirectStandardOutput = true;
			proc.StartInfo.RedirectStandardError = true;
			proc.StartInfo.FileName = "wsdl";
			proc.StartInfo.Arguments = "/out:" + pfile + " /nologo /namespace:" + ns + " /protocol:" + prot + " " + wsdl;
			proc.Start();
			
			if (!proc.WaitForExit (30000))
			{
				try {
					proc.Kill ();
				} catch {}
				
				Console.WriteLine ("FAIL (timeout)");
				if (File.Exists (pfile)) File.Delete (pfile);
				WriteError (errdoc, ns, "Errors found while generating " + prot + " proxy for WSDL: " + wsdl, "wsdl.exe timeout");
			}
			else if (proc.ExitCode != 0)
			{
				Console.WriteLine ("FAIL " + proc.ExitCode);
				
				string err = proc.StandardOutput.ReadToEnd ();
				err += "\n" + proc.StandardError.ReadToEnd ();

				if (File.Exists (pfile))
				{
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
				}
				
				WriteError (errdoc, ns, "Errors found while generating " + prot + " proxy for WSDL: " + wsdl, err);
			}
			else
			{
				if (File.Exists (pfile)) {
					Console.WriteLine ("OK");
					proxies.Add (pfile);
				}
				else {
					Console.WriteLine ("FAIL");
					string err = proc.StandardOutput.ReadToEnd ();
					err += "\n" + proc.StandardError.ReadToEnd ();
					WriteError (errdoc, ns, "Errors found while generating " + prot + " proxy for WSDL: " + wsdl, err);
				}
			}
		}
	}
	
	static void WriteError (XmlElement err, string ns, string error, string detail)
	{
		XmlElement ele = err.OwnerDocument.CreateElement ("error");
		ele.SetAttribute ("ns",ns);
		XmlText text = err.OwnerDocument.CreateTextNode (error + "\n" + detail);
		ele.AppendChild (text);
		err.AppendChild (ele);
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
	
	public static void CheckDiff (string diffFile)
	{
		int suc=0, tot=0, ign=0;
		XmlDocument doc = new XmlDocument ();
		doc.Load (diffFile);
		
		XmlDocument errdoc = new XmlDocument ();
		errdoc.Load ("proxy-gen-error.xml");
		
		XmlDocument result = new XmlDocument ();
		XmlElement res = result.CreateElement ("test-results");
		res.SetAttribute ("name", "wsdl-tests");
		res.SetAttribute ("date", DateTime.Now.ToShortDateString ());
		res.SetAttribute ("time", DateTime.Now.ToShortTimeString ());
		result.AppendChild (res);
		
		XmlElement ts = result.CreateElement ("test-suite");
		ts.SetAttribute ("name", "wsdl");
		ts.SetAttribute ("asserts", "0");
		ts.SetAttribute ("time", "0");
		res.AppendChild (ts);

		XmlElement tsres = result.CreateElement ("results");
		ts.AppendChild (tsres);
		
		XslTransform xsl = new XslTransform ();
		xsl.Load ("cormissing.xsl");
		
		foreach (ServiceData sd in services.services)
		{
			if (sd.Protocols == null) continue;

			foreach (string prot in sd.Protocols)
			{
				string ns = sd.Namespace + "." + prot;
				
				XmlElement elem = doc.SelectSingleNode ("assemblies/assembly/namespaces/namespace[@name='" + ns + "']") as XmlElement;
				if (elem == null) {
					ign++;
					continue;
				}
				
				if (!File.Exists (GetWsdlFile(sd))) {
					Console.WriteLine ("WARNING: wsdl file not found: " + GetWsdlFile(sd));
					ign++;
					continue;
				}
				
				tot++;

				bool extrans = elem.GetAttribute ("presence") == "extra";
				
				if ((elem.GetAttribute ("error") != "" || elem.GetAttribute ("missing") != "" || elem.GetAttribute ("extra") != "") && !extrans)
				{
					XmlElement errelem = errdoc.SelectSingleNode ("/errors/error[@ns='" + ns + "']") as XmlElement;			
					if (errelem != null) {
						WriteResult (tsres, ns, false, sd.Wsdl + "\n" + errelem.InnerText);
					}
					else {
						StringWriter str = new StringWriter ();
						xsl.Transform (elem, null, str, null);
						WriteResult (tsres, ns, false, sd.Wsdl + "\n" + str.ToString ());
					}
				}
				else 
				{
					if (extrans) Console.WriteLine ("BONUS CLASS: " + ns);
					suc++;
					WriteResult (tsres, ns, true, sd.Wsdl);
				}
				
			}
		}
		ts.SetAttribute ("success", (suc < tot).ToString());
		
		res.SetAttribute ("total", tot.ToString());
		res.SetAttribute ("failures", (tot-suc).ToString());
		res.SetAttribute ("not-run", "0");

		result.Save ("WsdlTestResult.xml");
		Console.WriteLine ("Compared:" + tot + " Ignored:" + ign + " Sucess:" + suc + " Fail:" + (tot - suc));
	}
	
	static void WriteResult (XmlElement res, string name, bool success, string msg)
	{
		XmlDocument doc = res.OwnerDocument;
		XmlElement test = doc.CreateElement ("test-case");
		test.SetAttribute ("name", name);
		test.SetAttribute ("executed", "True");
		test.SetAttribute ("success", success.ToString());
		test.SetAttribute ("time", "0");
		test.SetAttribute ("asserts", "0");
		
		if (!success) {
			XmlElement fail = doc.CreateElement ("failure");
			test.AppendChild (fail);
			XmlElement xmsg = doc.CreateElement ("message");
			fail.AppendChild (xmsg);
			xmsg.AppendChild (doc.CreateCDataSection (msg));
			fail.AppendChild (doc.CreateElement ("stack-trace"));
		}
		res.AppendChild (test);
	}
	
	static void MsFix (string fileName)
	{
		StreamReader sr = new StreamReader ("msfix");
		XmlDocument doc = new XmlDocument ();
		doc.Load (fileName);
		
		string line;
		while ((line = sr.ReadLine ()) != null)
		{
			string[] ss = line.Split ('/');
			XmlElement elem = doc.SelectSingleNode ("assemblies/assembly/namespaces/namespace[@name='" + ss[0] + "']/classes/class[@name='" + ss[1] + "']") as XmlElement;
			if (elem != null) elem.ParentNode.RemoveChild (elem);
		}
		doc.Save (fileName);
		sr.Close ();
	}
	
	static void GenerateFiles (string templateFile)
	{
		XmlDocument doc = new XmlDocument ();
		doc.Load (templateFile);
		
		XmlNodeList targets = doc.SelectNodes ("fileGeneration/targets/target");
		XmlElement gvarset = (XmlElement) doc.SelectSingleNode ("fileGeneration/vars[@name='_global']");
		
		foreach (XmlElement node in targets) {
			string tid = node.GetAttribute ("template");
			string file = node.GetAttribute ("file");
			XmlElement xtemp = (XmlElement) doc.SelectSingleNode ("fileGeneration/templates/template[@id='" + tid + "']");
			if (xtemp == null) throw new Exception ("Template " + tid + " not found");
			string tempFile = xtemp.GetAttribute ("file");
			
			StreamReader sr = new StreamReader (tempFile);
			string template = sr.ReadToEnd ();
			sr.Close ();
			
			string content = ReplaceVars (template, node);
			content = ReplaceVars (content, gvarset);
			
			StreamWriter sw = new StreamWriter (file);
			sw.Write (content);
			sw.Close ();
			Console.WriteLine ("Writen " + file);
		}
	}
	
	static string ReplaceVars (string content, XmlElement root)
	{
		XmlNodeList nodes = root.SelectNodes ("var");
		foreach (XmlElement elem in nodes) {
			string include = elem.GetAttribute ("include");
			if (include != "") {
				XmlElement varset = (XmlElement) root.OwnerDocument.SelectSingleNode ("fileGeneration/vars[@name='" + include + "']");
				if (varset == null) throw new Exception ("varset '" + include + "' not found");
				content = ReplaceVars (content, varset);
			}
			else {
				content = content.Replace ("$" + elem.GetAttribute ("name") + "$", elem.InnerText);
			}
		}
		
		return content;
	}
	
	static void RegisterFailure (ServiceData sd)
	{
		ServiceStatus status = null;
		foreach (ServiceStatus ss in serviceStatus.services)
		{
			if (ss.Name == sd.Name)
			{
				status = ss;
				break;
			}
		}
		
		if (status == null)
		{
			status = new ServiceStatus ();
			status.Name = sd.Name;
			status.Retries = 1;
			status.LastTestDate = DateTime.Now;
			serviceStatus.services.Add (status);
		}
		else
		{
			if ((DateTime.Now - status.LastTestDate).TotalHours >= 24)
				status.Retries++;
		}
	}

	static void CleanFailures (ServiceData sd)
	{
		ServiceStatus status = null;
		foreach (ServiceStatus ss in serviceStatus.services)
		{
			if (ss.Name == sd.Name)
			{
				serviceStatus.services.Remove (ss);
				break;
			}
		}
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
	
	static string GetWsdlFile (ServiceData fd)
	{
		string fn = Path.Combine (new Uri (fd.Wsdl).Host, fd.Name + ".wsdl");
		return Path.Combine (GetWsdlPath(), fn);
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
	
	static string GetWsdlPath ()
	{
		return Path.Combine (basePath, "wsdlcache");
	}
	
	static string GetTempWsdlPath ()
	{
		return Path.Combine (basePath, "wsdlcache");
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
			if (dir != "localhost") dir = "external";
			return Path.Combine (dir, Name + ".cs");
		}
	}
}

[XmlType("serviceStatus")]
public class ServiceStatusCollection
{
	[XmlElement("service", typeof(ServiceStatus))]
	public ArrayList services = new ArrayList ();
}

public class ServiceStatus
{
	[XmlElement("name")]
	public string Name;
	
	[XmlElement("lastTestDate")]
	public DateTime LastTestDate;
	
	[XmlElement("retries")]
	public int Retries;
}

