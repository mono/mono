using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CoreClr.Tools;
using Mono.Cecil;

namespace DetectMethodPrivileges
{
    public static class Program
	{
		public static class Paths
		{
			public static string Combine(string p1, params string[] prest)
			{
				foreach (var p in prest)
					p1 = Path.Combine(p1, p);
				return p1;
			}
		}
		public static class Files
		{
			public static string AuditedSafeMethodsFileFor(string assemblyName, string profileDirectory)
			{
				return Path.Combine(profileDirectory, assemblyName + ".audit");
			}

			public static string ReviewedPublicApisFileFor(string assemblyName, string profileDirectory)
			{
				return Path.Combine(profileDirectory, assemblyName + ".reviewedPublicApis");
			}

			public static string CriticalTypesFileFor(string assemblyName, string profileDirectory)
			{
				return Path.Combine(profileDirectory, assemblyName + ".criticalTypes");
			}

			public static string PublicApisInfoFor(string assemblyName, string outputDirectory)
			{
				return Path.Combine(outputDirectory, assemblyName + ".publicApis.html");
			}
		}

		public static int Main()
		{
			var monoroot = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.Parent.Parent.Parent.Parent.Parent.FullName;
			var inputdir = Paths.Combine(monoroot, "tmp","unity_linkered");
			//var inputdir = Paths.Combine(monoroot, "mcs","class","lib","unity");
			var outputdir = Paths.Combine(monoroot, "builds","monodistribution","lib","mono","unity_web");
			var profileDirectory = Paths.Combine(monoroot, "tuning","TuningInput","Security");
			Console.WriteLine("Outputdir: "+outputdir);
			Console.WriteLine("Inputdir: " + inputdir);
			if (Directory.Exists(outputdir))
				Directory.Delete(outputdir, true);
			Directory.CreateDirectory(outputdir);

			var assemblypaths = new List<string>();

			var inject = new[] { "mscorlib.dll", "System.dll", "System.Core.dll", "System.Xml.dll", "Mono.Security.dll" };
			foreach(var f in Directory.GetFiles(inputdir))
			{
				var name = Path.GetFileName(f);
				if (inject.Where(name.Contains).Any() && Path.GetExtension(name)==".dll")
				{
					assemblypaths.Add(f);
					continue;
				}
				if (f.ToLower().Contains("report")) continue;
				if (f.ToLower().Contains("I18N")) continue;
				File.Copy(f,Path.Combine(outputdir,name));
			}
			
			var assemblies = TimeFunction("Assembly loading", () => LoadAssembliesAndSetUpAssemblyResolver(assemblypaths, profileDirectory));
			var criticalTypes = CriticalTypes(assemblies, profileDirectory).ToList();
			EnsureCriticalTypesEnheritanceRulesAreCompliedWith(assemblies, criticalTypes);
            
			MethodPrivilegeDetector.CriticalTypes = criticalTypes.Cast<TypeReference>().ToList();
			var methodsRequiringPrivilegesThemselvesWithComments = TimeFunction("Privilege detection", ()=>assemblies.SelectMany(assembly => MethodPrivilegeDetector.MethodsRequiringPrivilegesThemselvesOn(assembly)).ToList());
			var methodsRequiringPrivilegesThemselvesManual = TimeFunction("Read manually provided methods requiring privileges",() =>ManuallyProvidedMethodsRequiringPrivilegesThemselves(profileDirectory, assemblies));

			methodsRequiringPrivilegesThemselvesWithComments.AddRange(
				methodsRequiringPrivilegesThemselvesManual.Select(
					n => new KeyValuePair<MethodDefinition, string>(n, "Manually added in .manual file")));

			var methodsRequiringPrivilegesThemselves = methodsRequiringPrivilegesThemselvesWithComments.Select(g => g.Key).ToList();
			
			var auditedSafeMethods = TimeFunction("Loading audit files", ()=>AuditedSafeMethods(profileDirectory, assemblies));
			var ignoredCalls = TimeFunction("Loading ignored call files", () => IgnoredCalls(profileDirectory, assemblies)).ToList();
			var privilegePropagation = TimeFunction("Privilege propagation", ()=>new MethodPrivilegePropagation(assemblies, methodsRequiringPrivilegesThemselves, auditedSafeMethods, criticalTypes, ignoredCalls));

		    var report = privilegePropagation.CreateReportBuilder().Build();
            foreach(var assembly in assemblies)
            {
                var injector = new Injector(assembly) { UI = UI, OutputDirectory = outputdir};
                injector.InjectAll(report.GetInjectionsFor(assembly));
            }
		    //var allCriticalMethods = privilegePropagation.ResultingCriticalMethods;
			//TimeAction("Attribute injection", ()=>InjectSecurityAttributes(assemblies, auditedSafeMethods, allCriticalMethods, outputdir));

			var rapportdir = Path.Combine(Path.Combine(monoroot, "tuning"),"GeneratedSecurityReports");
			TimeAction("Report generation", ()=>GenerateReportsFor(assemblies, methodsRequiringPrivilegesThemselvesWithComments, privilegePropagation, profileDirectory, rapportdir));

			int count = FindUnreviewedUnavailablePublicAPISInReport(rapportdir);

			//return 0;
			return count;
		}

    	private static void EnsureCriticalTypesEnheritanceRulesAreCompliedWith(AssemblyDefinition[] assemblies, List<TypeDefinition> criticalTypes)
    	{
			foreach(var ass in assemblies)
			{
				foreach (TypeDefinition t in ass.MainModule.Types)
				{
					foreach (var basetype in GetBaseTypes(t))
					{
						if (criticalTypes.Contains(basetype) && !criticalTypes.Contains(t))
						{
							throw new ApplicationException(basetype + " is marked as a critical type, but it gets inherited by " + t +
							                               " which is not. this is illegal");
						}
					}
				}
			}
	   	}

    	private static IEnumerable<TypeDefinition> GetBaseTypes(TypeDefinition typeDefinition)
    	{
    		if (typeDefinition.BaseType==null) yield break;
    		var basetype = typeDefinition.BaseType.Resolve();
    		yield return basetype;
			foreach(var base2 in GetBaseTypes(basetype))
				yield return base2;
    	}

    	private static int FindUnreviewedUnavailablePublicAPISInReport(string rapportdir)
    	{
    		int found = 0;
    		foreach(var file in Directory.GetFiles(rapportdir,"*publicApis.html"))
    		{
    			var txt = File.ReadAllText(file);
				int i = CountStringOccurences("#unavailable_notreviewed", txt);
				if (i>0)
					Console.WriteLine("Foudn unavailable unreviewed public apis in: "+file);
    			found += i;
    		}
    		return found;
    	}

    	static int CountStringOccurences(string needle, string haystack)
		{
			return (haystack.Length - haystack.Replace(needle, "").Length) / needle.Length;
		}

    	private static IEnumerable<TypeDefinition> CriticalTypes(IEnumerable<AssemblyDefinition> assembies, string profileDirectory)
		{
			return assembies.SelectMany((a) => CriticalTypesFor(a, profileDirectory));
		}

		private static IEnumerable<TypeDefinition> CriticalTypesFor(AssemblyDefinition assemblyDefinition, string profileDirectory)
		{
			string assemblyName = assemblyDefinition.Name.Name;
			var cdf = new CecilDefinitionFinder(assemblyDefinition);
			
			var file = Files.CriticalTypesFileFor(assemblyName, profileDirectory);
			if (!File.Exists(file)) return new TypeDefinition[0];
			return File.ReadAllLines(file)
				.Select(s => cdf.GetType(s)).Where(t => t!=null);
		}

		private static AssemblyDefinition[] LoadAssembliesAndSetUpAssemblyResolver(IEnumerable<string> assembyNames, string profileDirectory)
		{
		    var assemblies = assembyNames.Select(a => AssemblyFactory.GetAssembly(Path.Combine(profileDirectory, a))).ToArray();
            AssemblySetResolver.SetUp(assemblies);
		    return assemblies;
		}

        private static void GenerateReportsFor(IEnumerable<AssemblyDefinition> assemblies, IEnumerable<KeyValuePair<MethodDefinition, string>> methodsRequiringPrivilegesThemselvesWithComments, MethodPrivilegePropagation privilegePropagation, string profileDirectory, string outputDirectory)
		{
			EnsureDirectoryIsEmpty(outputDirectory);

			var reportBuilder =	privilegePropagation.CreateReportBuilder();
			foreach (var assembly in assemblies)
			{
				GenerateMethodsRequiringPrivilegesThemselvesInfo(assembly, methodsRequiringPrivilegesThemselvesWithComments, outputDirectory);
				GeneratePublicApisInfo(assembly, reportBuilder, profileDirectory, outputDirectory);
			}
		}

		private static void EnsureDirectoryIsEmpty(string outputDirectory)
		{
			if (Directory.Exists(outputDirectory))
				Directory.Delete(outputDirectory, true);
			Directory.CreateDirectory(outputDirectory);
			Console.WriteLine("Created directory: "+outputDirectory);
			if (!Directory.Exists(outputDirectory))
				throw new ApplicationException("Unable to create directory: "+outputDirectory);
		}

		private static void GeneratePublicApisInfo(AssemblyDefinition assembly, MethodPrivilegePropagationReportBuilder reportBuilder, string profileDirectory, string outputDirectory)
		{
			var outputFile = Files.PublicApisInfoFor(assembly.SimpleName(), outputDirectory);
			ReportGenerationOf(outputFile);

			var reviewedMethods = ReadRegexesFromFile(Files.ReviewedPublicApisFileFor(assembly.SimpleName(), profileDirectory));
			var candidateMethods = assembly.AllMethodDefinitions();
			File.WriteAllText(outputFile, reportBuilder.BuildPublicApisReport(candidateMethods, reviewedMethods, new HtmlWriter()));
		}

    	private static List<Regex> ReadRegexesFromFile(string reviewedPublicApisFileFor)
    	{
    		var result = new List<Regex>();
			if (!File.Exists(reviewedPublicApisFileFor)) return result;
			foreach (var line in File.ReadAllLines(reviewedPublicApisFileFor))
    		{
    			if (line.StartsWith("#")) continue;
				if (line.Trim().Length==0) continue;
    			result.Add(new Regex(line));
    		}
    		return result;
    	}

    	private static void GenerateMethodsRequiringPrivilegesThemselvesInfo(AssemblyDefinition assembly, IEnumerable<KeyValuePair<MethodDefinition, string>> methodsRequiringPrivilegesThemselvesWithComments, string outputDirectory)
		{
			var outputFile = Path.Combine(outputDirectory, assembly.SimpleName()) + ".methodsRequiringPrivilegesThemselves.info";
			ReportGenerationOf(outputFile);
			using (var writer = new StreamWriter(outputFile))
				foreach (var kvp in methodsRequiringPrivilegesThemselvesWithComments.Where(g => g.Key.DeclaringType.Module.Assembly == assembly))
					writer.WriteLine(kvp.Key + "   #because: " + kvp.Value);
		}

		private static void ReportGenerationOf(string outputFile)
		{
			Info("Generating {0}...", outputFile);
		}


		private static bool InjectSecurityAttributeOn(ILookup<AssemblyDefinition, IGrouping<AssemblyDefinition, MethodDefinition>> methodsPerAssembly, Injector injector, string attributeType)
		{
			var assembly = injector.Assembly;
			var methods = methodsPerAssembly[assembly].SingleOrDefault();
			if (methods == null)
				return false;

			foreach (var m in methods)
				injector.InjectAttributeOn(m, attributeType);
			return true;
		}

		private static ILookup<AssemblyDefinition, IGrouping<AssemblyDefinition, MethodDefinition>> ToAssemblyLookup(IEnumerable<MethodDefinition> methods)
		{
			return methods.GroupBy(m => m.DeclaringType.Module.Assembly).ToLookup(g => g.Key);
		}

		private static IEnumerable<MethodDefinition> AuditedSafeMethods(string profileDirectory, IEnumerable<AssemblyDefinition> assemblies)
		{	
			return assemblies.SelectMany(a => ReadMethodsFromMoonlightFile(a, Files.AuditedSafeMethodsFileFor(a.SimpleName(), profileDirectory))).ToList();
		}
		private static IEnumerable<MethodToMethodCall> IgnoredCalls(string profileDirectory,IEnumerable<AssemblyDefinition> assemblies)
		{
			return ReadMethodToMethodCallsFromFile(assemblies, Path.Combine(profileDirectory,"ignoreTheseMethodToMethodCalls.txt"));
		}
		private static IEnumerable<MethodDefinition> ManuallyProvidedMethodsRequiringPrivilegesThemselves(string profileDirectory, IEnumerable<AssemblyDefinition> assemblies)
		{
			return assemblies.SelectMany(a => ReadMethodsFromMoonlightFile(a, Path.Combine(profileDirectory, a.SimpleName() + ".manualCriticalMethods"))).ToList();
		}

    	private static IEnumerable<MethodToMethodCall> ReadMethodToMethodCallsFromFile(IEnumerable<AssemblyDefinition> assemblies, string file)
    	{
			if (!File.Exists(file)) yield break;
			foreach(var line in File.ReadAllLines(file))
			{
				if (line.StartsWith("#")) continue;
				if (line.Trim().Length==0) continue;
				var splitter = "=>";
				int i = line.IndexOf(splitter);
				if (i == -1) throw new ApplicationException("illegal line found in " + file + " was: " + line);
				var caller = line.Substring(0, i).Trim();
				var callee = line.Substring(i + splitter.Length).Trim();

				var cecilDefinitionFinder = new CecilDefinitionFinder(assemblies);
				var mcaller = cecilDefinitionFinder.FindMethod(caller);
				if (mcaller == null) throw new ApplicationException("couldnt find method while parsing " + file + " signarture: " + caller);
				var mcallee = cecilDefinitionFinder.FindMethod(callee);
				if (mcallee == null) throw new ApplicationException("couldnt find method while parsing " + file + " signarture: " + callee);
				yield return new MethodToMethodCall(mcaller,mcallee);
			}
    	}

    	private static IEnumerable<MethodDefinition> ReadMethodsFromMoonlightFile(AssemblyDefinition assembly, string auditFile)
		{	
			if (!File.Exists(auditFile))
			{
				Warning("File '{0}' not found. No safe methods will be considered for assembly '{1}'.", auditFile, assembly);
				return new MethodDefinition[0];
			}
			return new CecilDefinitionFinder(assembly).FindMethods(Compatibility.ParseMoonlightAuditFormat(File.ReadAllText(auditFile)).NonEmptyLines());
		}

		private static IEnumerable<MethodDefinition> ReadMethodsFromFile(AssemblyDefinition assembly, string directory, string extension)
		{
			var file = Path.Combine(directory, assembly.SimpleName() + extension);
			return ReadMethodsFromFile(assembly, file);
		}

		private static IEnumerable<MethodDefinition> ReadMethodsFromFile(AssemblyDefinition assembly, string file)
		{
			if (!File.Exists(file))
			{
				Warning("File '{0}' not found. No {1} file will be processed for assembly '{2}'.", file, Path.GetExtension(file), assembly);
				return new MethodDefinition[0];
			}

			var lines = File.ReadAllText(file).NonEmptyLines();
			var preprocessed =lines.Select(l => l.Split('#')[0].Trim());
			return new CecilDefinitionFinder(assembly).FindMethods(preprocessed);
		}

		static void Warning(string format, params Object[] args)
		{
			UI.Warning(format, args);
		}

		static void Info(string format, params Object[] args)
		{
			UI.Info(format, args);
		}

		private static TResult TimeFunction<TResult>(string label, Func<TResult> action)
		{
			TResult result = default(TResult);
			TimeAction(label, () => result = action());
			return result;
		}

		private static void TimeAction(string label, Action action)
		{
			var watch = Stopwatch.StartNew();
			action();
			watch.Stop();
			Info("{0} took {1}ms.", label, watch.ElapsedMilliseconds);
		}

		// allow plugging external GUI
		private static UserInterface _ui = new ConsoleUserInterface();

		class ConsoleUserInterface : UserInterface
		{
			public void Info(string format, params object[] args)
			{
				Console.WriteLine(format, args);
			}

			public void Warning(string format, params object[] args)
			{
				Console.Error.WriteLine(format, args);
			}
		}

		public static UserInterface UI
		{
			get { return _ui; }
			set { _ui = value; }
		}
	}
}
