using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Mono.Cecil;

namespace CoreClr.Tools
{
    public class MethodPrivilegePropagationStringReport
    {
        public string PublicApis { get; set; }
        public string InjectionInstructions { get; set; }
    }

    public class MethodPrivilegePropagationReport
    {
	    private readonly IEnumerable<MethodDefinition> _criticalMethods;
	    private readonly ICollection<MethodDefinition> _sscMethods;
	    private readonly ICollection<TypeDefinition> _criticalTypes;

	    public MethodPrivilegePropagationReport(IEnumerable<MethodDefinition> criticalMethods, ICollection<MethodDefinition> sscMethods, ICollection<TypeDefinition> criticalTypes)
	    {
	        _criticalMethods = criticalMethods;
	        _sscMethods = sscMethods;
	        _criticalTypes = criticalTypes;
	    }

        public IEnumerable<MethodDefinition> CriticalMethods
        {
            get { return _criticalMethods; }
        }

        public ICollection<MethodDefinition> SscMethods
        {
            get { return _sscMethods; }
        }

        public ICollection<TypeDefinition> CriticalTypes
        {
            get { return _criticalTypes; }
        }

        public IEnumerable<CecilSecurityAttributeDescriptor> GetInjectionsFor(AssemblyDefinition assembly)
	    {
        	var criticalMethods = MethodsFromAssembly(_criticalMethods, assembly);
        	foreach (var criticalMethod in criticalMethods)
                yield return new CecilSecurityAttributeDescriptor(criticalMethod, SecurityAttributeType.Critical);

            foreach (var safeMethod in MethodsFromAssembly(_sscMethods, assembly).Where(m => !criticalMethods.Contains(m)))
                yield return new CecilSecurityAttributeDescriptor(safeMethod, SecurityAttributeType.SafeCritical);

            foreach (var criticalType in _criticalTypes.Where(t => t.Module.Assembly == assembly))
                yield return new CecilSecurityAttributeDescriptor(criticalType, SecurityAttributeType.Critical);
	    }

	    private IEnumerable<MethodDefinition> MethodsFromAssembly(IEnumerable<MethodDefinition> methodDefinitions, AssemblyDefinition assembly)
	    {
	        return methodDefinitions.Where(m => m.DeclaringType.Module.Assembly == assembly);
	    }
	}

	public class MethodPrivilegePropagationReportBuilder
	{
		private readonly AssemblyDefinition[] _assemblies;
		private readonly ICollection<MethodDefinition> _methodRequiringPrivilegesThemselves;
		private readonly ICollection<MethodDefinition> _canBeSscManual;
		private readonly ICollection<MethodDefinition> _resultingSecurityCriticalMethods;
		private readonly Dictionary<MethodDefinition, List<PropagationReason>> _propagationGraph;
		private readonly ICollection<TypeDefinition> _criticalTypes;

		public MethodPrivilegePropagationReportBuilder(AssemblyDefinition[] assemblies, ICollection<MethodDefinition> methodRequiringPrivilegesThemselves, ICollection<MethodDefinition> canBeSscManual, ICollection<MethodDefinition> resultingSecurityCriticalMethods, Dictionary<MethodDefinition, List<PropagationReason>> propagationGraph, ICollection<TypeDefinition> criticalTypes)
		{
			_assemblies = assemblies;
			_methodRequiringPrivilegesThemselves = methodRequiringPrivilegesThemselves;
			_canBeSscManual = canBeSscManual;
			_resultingSecurityCriticalMethods = resultingSecurityCriticalMethods;
			_propagationGraph = propagationGraph;
			_criticalTypes = criticalTypes;
		}

		public MethodPrivilegePropagationReport Build()
		{
            return new MethodPrivilegePropagationReport(_resultingSecurityCriticalMethods, _canBeSscManual, _criticalTypes);
		}

	    public MethodPrivilegePropagationStringReport BuildStringReport()
        {
            var report = Build();

            return new MethodPrivilegePropagationStringReport
                       {
                           InjectionInstructions = BuildInjectionInstructionsStringReportFor(report),
                           PublicApis = BuildPublicApisReport(AllMethodDefinitions())
                       };
		}

	    private string BuildInjectionInstructionsStringReportFor(MethodPrivilegePropagationReport report)
	    {
	        var writer = new StringWriter();
	        WriteInjectionInstructionsFor(report.CriticalMethods, "SC-M", writer);
	        WriteInjectionInstructionsFor(report.SscMethods, "SSC-M", writer);
	        WriteInjectionInstructionsFor(report.CriticalTypes, "SC-T", writer);
            return writer.ToString();
	    }

	    private string BuildPublicApisReport(IEnumerable<MethodDefinition> candidates)
		{
			return BuildPublicApisReport(candidates, new List<Regex>() , PlainText);
		}

		public string BuildPublicApisReport(IEnumerable<MethodDefinition> candidates, List<Regex> reviewedMethods, ReportWriter format)
		{
			var report = SelectVisibleEntryPoints(candidates)
				.Select(m => new
				             	{
				             		Method = m,
				             		Comment = CommentFor(m, format, reviewedMethods)
				             	}).GroupBy(row => row.Comment)
				.OrderByDescending(g => g.Key)
				.SelectMany(g => g.OrderBy(row => row.Method.DeclaringType.FullName + row.Method.Name));
			
			var writer = new StringWriter();
			format.BeginReport(writer);
			foreach (var row in report)
				format.PublicMethod(writer, row.Method, row.Comment);
			format.EndReport(writer);

			return writer.ToString();
		}

		public abstract class ReportWriter
		{
			public virtual void BeginReport(TextWriter writer)
			{	
			}

			public abstract void PublicMethod(TextWriter writer, MethodDefinition method, string comment);

			public virtual void EndReport(TextWriter writer)
			{	
			}

			public virtual string PropagationGraphStringFor(IEnumerable<PropagationReason> stack)
			{
				string result = stack.Aggregate("",
					(s, m) => m.MethodThatTaintedMe == null
						? s + m.Explanation
						: s + string.Format("{1} {0} (ML:{2}) which ", MethodSignatureProvider.SignatureFor(m.MethodThatTaintedMe), m.Explanation, Moonlight.GetSecurityStatusFor(m.MethodThatTaintedMe)));
				return result;
			}
		}

		public static readonly ReportWriter PlainText = new PlainTextWriter();

		class PlainTextWriter : ReportWriter
		{
			override public void PublicMethod(TextWriter writer, MethodDefinition method, string comment)
			{
				writer.WriteLine("{0}    {1}", MethodSignatureProvider.SignatureFor(method), comment);
			}
		}

		private string CommentFor(MethodDefinition m, ReportWriter format, List<Regex> reviewedMethods)
		{
			if (!MethodPrivilegeDetector.IsMethodSignatureSafe(m))
				return "#methodsignature_notsafe";

			string unavailablereason = null;
			bool criticaltype = false;
			if (_criticalTypes.Contains(m.DeclaringType))
			{
				criticaltype = true;
				unavailablereason = format.PropagationGraphStringFor(new[] { new PropagationReasonIsInCriticalType(m) });
			}
			else if (_canBeSscManual.Contains(m))
			{
				return "#available_manualSSC";
			}
			else if (_resultingSecurityCriticalMethods.Contains(m))
			{
				if (_methodRequiringPrivilegesThemselves.Contains(m))
					unavailablereason= "method itself requires privileges";
				else
					unavailablereason = format.PropagationGraphStringFor(PropagationStackFor(m));
			}
			if (unavailablereason!=null)
			{
				string prefix = "#unavailable_notreviewed ";
				if (criticaltype || reviewedMethods.Any(r => r.Match(m.ToString()).Success))
					prefix = "#unavailable_butreviewed ";

				return prefix + " (ML: " + Moonlight.GetSecurityStatusFor(m) + ") " + unavailablereason;
			}
			return "#available";
		}

		private static IEnumerable<MethodDefinition> SelectVisibleEntryPoints(IEnumerable<MethodDefinition> methods)
		{
            return methods.Where(m => m.IsVisible());
		}
              
		private IEnumerable<MethodDefinition> AllMethodDefinitions()
		{
			return _assemblies.SelectMany(a => a.AllMethodDefinitions());
		}

		private IEnumerable<PropagationReason> PropagationStackFor(MethodDefinition m)
		{
			return PropagationStack.Create(_propagationGraph, m);
		}

	    private void WriteInjectionInstructionsFor(ICollection<TypeDefinition> types, string instruction, StringWriter writer)
	    {
            foreach (var t in types)
            {
                var signature = t.FullName;
                WriteSingleInjectionInstruction(writer, instruction, signature);
            }
	    }

	    private static void WriteInjectionInstructionsFor(IEnumerable<MethodDefinition> methods, string instruction, TextWriter writer)
		{
			foreach (var m in methods)
			{
			    var signature = MethodSignatureProvider.SignatureFor(m);
			    WriteSingleInjectionInstruction(writer, instruction, signature);
			}
		}

	    private static void WriteSingleInjectionInstruction(TextWriter writer, string instruction, string signature)
	    {
	        writer.Write(instruction);
	        writer.Write(": ");
	        writer.WriteLine(signature);
	    }
	}

	public class Moonlight
	{
		public static CecilDefinitionFinder _moonlightDefinitionFinder;

		public static Dictionary<MethodDefinition,string> cache = new Dictionary<MethodDefinition, string>();
		public static string GetSecurityStatusFor(MethodDefinition methodDefinition)
		{
			if (cache.ContainsKey(methodDefinition)) return cache[methodDefinition];

			var result = GetSecurityStatusForInternal(methodDefinition);
			cache[methodDefinition] = result;
			return result;
		}

		private static string GetSecurityStatusForInternal(MethodDefinition methodDefinition)
		{
			if (_moonlightDefinitionFinder == null)
			{
				var securityroot = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.Parent.Parent.Parent.FullName;
				var dir = securityroot + "/MoonlightAssemblies";
				var assemblies = new List<AssemblyDefinition>();
				foreach(var file in Directory.GetFiles(dir,"*.dll"))
				{
					assemblies.Add(AssemblyFactory.GetAssembly(file));
				}
				
				_moonlightDefinitionFinder = new CecilDefinitionFinder(assemblies);
			}

			var moonlightmethod = _moonlightDefinitionFinder.FindMethod(methodDefinition.ToString());
			if (moonlightmethod == null)
			{
				if (_moonlightDefinitionFinder.FindType(methodDefinition.DeclaringType.ToString()) == null)
					return "NotFound (Type also not found)";
				return "NotFound (Type does exist: " + GetSecurityStatusFor(methodDefinition.DeclaringType.Resolve())+")";
			}
			string result = FindSecurityAttribute(moonlightmethod.CustomAttributes);
			if (result!=null) return result;

			return GetSecurityStatusFor(moonlightmethod.DeclaringType);
		}

		private static string GetSecurityStatusFor(TypeDefinition typeDefinition)
		{
			string result = FindSecurityAttribute(typeDefinition.CustomAttributes);
			if (result != null) return result + "Type";
			return "Transparent";
		}

		private static string FindSecurityAttribute(CustomAttributeCollection attributes)
		{
			foreach (CustomAttribute attr in attributes)
			{
				var t = attr.Constructor.DeclaringType.ToString();
				if (t == "System.Security.SecurityCriticalAttribute")
					return "SecurityCritical";
				if (t == "System.Security.SecuritySafeCriticalAttribute")
					return "SecuritySafeCritical";
			}
			return null;
		}
	}
}