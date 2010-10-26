using System;
using System.Collections.Generic;
using GuiCompare;
using System.Threading;
using System.IO;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace ProfileComparer
{
	class MainClass
	{
		static string monoroot;
		static string monolibmono;

		public static void Main (string[] args)
		{
			string myassembly = Assembly.GetExecutingAssembly().Location;
			monoroot = new FileInfo(myassembly).Directory.Parent.Parent.Parent.Parent.FullName;
			monolibmono =  monoroot+"/../Mono/builds/monodistribution/lib/mono/";

			string originalprofiledirectory = UnityProfilesUtils.DirectoryNameFromProfile (UnityProfiles.None);
			Dictionary<UnityProfiles,UnityProfilesDocumentation> docs = new Dictionary<UnityProfiles, UnityProfilesDocumentation> ();
			UnityProfilesDocumentation mergedDoc = new UnityProfilesDocumentation ();
			
			Dictionary<string,MasterAssembly> masterInfos = new Dictionary<string, MasterAssembly> ();
			foreach(var file in Directory.GetFiles(monolibmono+originalprofiledirectory,"*.dll"))
			{
				if (!file.Contains("mscorlib") && !file.Contains("System.")) continue;
				// FIXME: for some reason, mono-api-info.exe fails on these assemblies (Cecil failure).
				if (file.Contains("System.Web.DynamicData.dll")) continue;
				if (file.Contains("System.Web.Extensions.dll")) continue;
				if (file.Contains("System.Web.Routing.dll")) continue;
				if (file.Contains("System.Web.dll")) continue;
				
				string assemblyname = Path.GetFileName(file);
				Console.WriteLine("Reading reference assembly {0}", assemblyname);
				
				string originalxml = GenerateAPIInfo(file);
				MasterAssembly reference = new MasterAssembly(originalxml);
				masterInfos [assemblyname] = reference;
			}
			
			foreach (UnityProfiles profile in UnityProfilesUtils.ListProfiles ()) {
				var newprofiledirectory = profile.DirectoryNameFromProfile ();
				
				Console.WriteLine (" *** Working on profile {0} (directory {1})", profile, newprofiledirectory);
				
				foreach(var file in Directory.GetFiles(monolibmono+newprofiledirectory,"*.dll"))
				{
					if (!file.Contains("mscorlib") && !file.Contains("System.")) continue;
					// FIXME: for some reason, mono-api-info.exe fails on these assemblies (Cecil failure).
					if (file.Contains("System.Web.DynamicData.dll")) continue;
					if (file.Contains("System.Web.Extensions.dll")) continue;
					if (file.Contains("System.Web.Routing.dll")) continue;
					if (file.Contains("System.Web.dll")) continue;
					
					string assemblyname = Path.GetFileName(file);
					Console.WriteLine("Working on assembly {0}", assemblyname);
					
					if (! masterInfos.ContainsKey (assemblyname)) {
						Console.WriteLine ("Assembly {0} from profile {1} not found in main master infos, skipping", assemblyname, profile);
						continue;
					}
					
					Console.WriteLine ("Reading assembly {0} in profile {1}", assemblyname, profile);
					string newxml = GenerateAPIInfo(file);
					GuiCompare.MasterAssembly target = new MasterAssembly(newxml);

					GuiCompare.MasterAssembly reference = masterInfos [assemblyname];
					UnityProfilesDocumentation currentProfileDocumentation = new UnityProfilesDocumentation ();
					docs [profile] = currentProfileDocumentation;
					
					CompareContext comparer = new CompareContext(() => reference,() => target, profile, currentProfileDocumentation);
					//CompareContext comparer = new CompareContext(() => reference,() => target, profile, doc);
					
					bool finished = false;
					comparer.Finished += (a,b) => finished = true;
					comparer.ProgressChanged += (a,b) => Console.WriteLine(b.ToString());
					comparer.Compare();
					while (!finished)
					{
					        Console.WriteLine("Waiting for comparison to finish...");
					        Thread.Sleep(2000);
					}
					
					// Handle errors here...
				}
			}
			
			// Merge profiles
			foreach (UnityProfiles profile in UnityProfilesUtils.ListProfiles ()) {
				Console.WriteLine (" *** Merging profile {0}", profile);
				UnityProfilesDocumentation currentDoc = docs [profile];
				foreach (DocumentedNamespace ns in currentDoc.Namespaces) {
					DocumentedNamespace referenceNs = mergedDoc.AddReferenceNamespace (ns.Name);
					foreach (DocumentedClass c in ns.Classes) {
						DocumentedClass referenceClass = referenceNs.AddReferenceClass (c.Name);
						referenceClass.AddSupportedProfile (profile);
						foreach (DocumentedMember member in c.Members) {
							DocumentedMember referenceMember;
							if (member is DocumentedField) {
								referenceMember = referenceClass.AddReferenceField (member.Name);
							} else if (member is DocumentedProperty) {
								referenceMember = referenceClass.AddReferenceProperty (member.Name);
							} else if (member is DocumentedMethod) {
								referenceMember = referenceClass.AddReferenceMethod (member.Name);
							} else {
								referenceMember = null;
							}
							referenceMember.AddSupportedProfile (profile);
						}
					}
				}
			}
			
			// Generate report here...
			Console.WriteLine (" *** Dumping merged docs");
			//mergedDoc.DebugDump ();
		}

		static string GenerateAPIInfo(string assembly)
		{
			var domain = AppDomain.CreateDomain("otherDomain");
			var monoapiinfo = monolibmono+"/2.0/mono-api-info.exe";
			
			var p = new Process();
			p.StartInfo.FileName = monoapiinfo;
			p.StartInfo.Arguments = assembly;
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.UseShellExecute=false;
			p.Start();
			var sb = new StringBuilder();
			while(!p.HasExited)
			{
				var s = p.StandardOutput.ReadToEnd();
				sb.Append(s);
				Thread.Sleep(1000);
			}
			sb.Append(p.StandardOutput.ReadToEnd());

			var tmp = Path.GetTempFileName();
			File.WriteAllText(tmp,sb.ToString());
			return tmp;
		}
	}
}

