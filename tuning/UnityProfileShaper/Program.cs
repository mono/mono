using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Linker;
using Mono.Linker.Steps;
using System.Reflection;

namespace UnityProfileShaper
{
	class Program
	{
		static void Main(string[] args)
		{
		
			var mydir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
			var monoroot = mydir.Parent.Parent.Parent.Parent;
			
			var inputdir = Path.Combine(monoroot.FullName,"builds/monodistribution/lib/mono/unity");
			var outputdir = Path.Combine(monoroot.FullName, "tmp/unity_linkered");
			
			if (Directory.Exists(outputdir))
				Directory.Delete(outputdir, true);
			Directory.CreateDirectory(outputdir);
			
			var assemblies = new List<string>();
			var nonassemblies = new List<string>();
			var files = new List<string>();
			foreach(var file in Directory.GetFiles(inputdir))
			{
				var ext = Path.GetExtension(file);
				var filename = Path.GetFileName(file);
				files.Add(filename);
				if (ext == ".dll" || ext==".exe")
					assemblies.Add(filename);
				else
					nonassemblies.Add(filename);
			}
			
			Console.WriteLine("InputDir: "+inputdir);
			Console.WriteLine("OutputDir: "+outputdir);

			Pipeline p = new Pipeline();

			foreach(var file in assemblies)
			{
				var fullfile = Path.Combine(inputdir,file);
				IStep s = null;
				var action = GetAssemblyAction(file);
				if (action==AssemblyAction.Link)
					s = new MarkPublicApiExceptStep(fullfile);
				else if (action == AssemblyAction.Copy)
					s = new ResolveFromAssemblyStep(fullfile);
				if (s!=null) p.AppendStep(s);
			}
			p.AppendStep(new LoadReferencesStep());
			p.AppendStep(new BlacklistStep());
			p.AppendStep(new TypeMapStep());			
			p.AppendStep(new MarkAllFieldsOfSerializableTypes());
			p.AppendStep(new MarkStep());
			p.AppendStep(new SweepStep());
			p.AppendStep(new CleanStep());
			p.AppendStep(new RegenerateGuidStep());
			p.AppendStep(new OutputStep());
			p.AppendStep(new OutputMarkBacktraceReportStep());
			p.AppendStep(new FailIfWeMissTypesThatWeShippedInAPreviousVersionStep());

			LinkContext context = new LinkContext(p);
			context.CoreAction = AssemblyAction.Link;
			context.OutputDirectory = outputdir;
			context.Resolver.AddSearchDirectory(inputdir);
			p.Process(context);
			
			foreach(var file in files.Where(f => GetAssemblyAction(f)==AssemblyAction.Copy))
			{
				var _from = Path.Combine(inputdir,file);
				var _to = Path.Combine(outputdir,file);
				File.Copy(_from,_to,true);
			}
			WriteDiffReport(inputdir,assemblies, outputdir);
		}

		private static void WriteDiffReport(string inputdir, List<string> inputs, string outputdir)
		{
			foreach(var i in inputs)
			{
				var a = Path.Combine(inputdir, i);
				var source = AssemblyFactory.GetAssembly(a);
				List<TypeDefinition> tunedtypes = new List<TypeDefinition>();
				var assemblyname = Path.GetFileName(a);
				try
				{
					var tuned = AssemblyFactory.GetAssembly(Path.Combine(outputdir, assemblyname));
					foreach (TypeDefinition td in tuned.MainModule.Types)
						tunedtypes.Add(td);
				} catch (FileNotFoundException)
				{
				}
				var sourcetypes = source.MainModule.Types;
				var writer = new StreamWriter(Path.Combine(outputdir, assemblyname+"_stripreport.txt"));
				foreach(TypeDefinition td in sourcetypes)
				{
					if (!tunedtypes.Where(t => t.FullName == td.FullName).Any())
						writer.WriteLine("Stripped: "+td.FullName);
				}
			}
		}
		
		static AssemblyAction GetAssemblyAction(string assembly)
		{
			var link = new List<string>() 
			{ 
				"mscorlib.dll",
				"System.dll",
				"System.Xml.dll",
				"System.Data.dll",
                "System.Core.dll",
				"Mono.Security.dll",
			};
			if (link.Contains(assembly)) return AssemblyAction.Link;
			
			var copy_pattern = new[] {
				"Boo",
				"UnityScript",
				"us.exe",
				"smcs.exe",
				"booc.exe",
				"Mono.CompilerServices.SymbolWriter.dll",
				"Mono.Cecil.dll", // required when referencing UnityEditor.dll for the webplayer target
			};
			
			if (copy_pattern.Where(a => assembly.Contains(a)).Any()) return AssemblyAction.Copy;
			return AssemblyAction.Skip;			
		}
	}
}
