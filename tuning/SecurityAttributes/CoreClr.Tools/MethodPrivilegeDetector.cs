using System;
using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace CoreClr.Tools
{
	/// <summary>
	/// Detect code that needs to be [SecurityCritical] in order to be executable 
	/// under the CoreCLR security model. The ouput includes comments as to why 
	/// the attribute is needed.
	/// 
	/// Note: Results are not always 100% certain so a comment is added wrt the
	/// condition that was used to report it as a candidate.
	/// </summary>
	public static class MethodPrivilegeDetector
	{
		static Dictionary<MethodDefinition, string> methods = new Dictionary<MethodDefinition, string>();
		public static ICollection<TypeReference> CriticalTypes { get; set; }

		public static IEnumerable<KeyValuePair<MethodDefinition, string>> MethodsRequiringPrivilegesThemselvesOn(AssemblyDefinition assembly)
		{
			methods.Clear();
			foreach (ModuleDefinition module in assembly.Modules)
				foreach (TypeDefinition type in module.Types)
					ProcessType(type);

			return new Dictionary<MethodDefinition, string>(methods);
		}

		public static string ReportOfMethodsRequiringPrivilegesThemselves(AssemblyDefinition assembly)
		{
			var output = new StringWriter();
			WriteMethodsRequiringPrivilegesThemselves(assembly, output);
			return output.ToString().Trim();
		}

		static void WriteMethodsRequiringPrivilegesThemselves(AssemblyDefinition assembly, TextWriter sw)
		{
			WriteMethodsRequiringPrivilegesThemselves(sw, MethodsRequiringPrivilegesThemselvesOn(assembly));
		}

		static void WriteMethodsRequiringPrivilegesThemselves(TextWriter sw, IEnumerable<KeyValuePair<MethodDefinition, string>> methodsWithComments)
		{
			sw.WriteLine("# This file has a list of methods that were automatically detected to require privileges ([sc] or [ssc])");
			sw.WriteLine("# in order to not generate an exception at runtime.");
			sw.WriteLine();
			foreach (KeyValuePair<MethodDefinition, string> kvp in methodsWithComments)
			{
				sw.WriteLine("# {0}", kvp.Value);
				sw.WriteLine(SignatureFor(kvp.Key));
				sw.WriteLine();
			}
		}

		// note: pointers don't *need* to be SecurityCritical because they can't be
		// used without a "unsafe" or "fixed" context that transparent code won't support
		static bool CheckTypeIsSafe(TypeReference type)
		{
			string fullname = type.FullName;

			// pointers can only be used by fixed/unsafe code
			if (fullname.EndsWith("*")) return false;
			if (CriticalTypes.Contains(type)) return false;
			return true;
		}

		static string CheckVerifiability(MethodDefinition method)
		{
			if (!method.HasBody)
				return String.Empty;

			foreach (Instruction ins in method.Body.Instructions)
			{
				switch (ins.OpCode.Code)
				{
					case Code.No:		// ecma 335, part III, 2.2
					case Code.Calli:	// Lidin p260
					case Code.Cpblk:	// ecma 335, part III, 3.30
					case Code.Initblk:	// ecma 335, part III, 3.36
					case Code.Jmp:		// ecma 335, part III, 3.37 / Lidin p259
					case Code.Localloc:	// ecma 335, part III, 3.47
						return ins.OpCode.Name;
					case Code.Arglist:	// lack test case
					case Code.Cpobj:
						return ins.OpCode.Name;
					case Code.Mkrefany:	// not 100% certain
						return ins.OpCode.Name;
				}
			}
			return String.Empty;
		}

		static void ProcessMethod(MethodDefinition method)
		{
			string comment = null;

			// All p/invoke methods needs to be [SecurityCritical] to be executed
			bool sc = method.IsPInvokeImpl;
			if (sc)
			{
				comment = "p/invoke declaration";
			}

			if (!sc)
			{
				comment = CheckVerifiability(method);
				sc = !String.IsNullOrEmpty(comment);
			}

			if (!sc)
			{
				sc = !IsMethodSignatureSafe(method, ref comment);
			}

			// check if this method implements an interface where the corresponding member
			// is [SecurityCritical]
			TypeDefinition type = method.DeclaringType;
			if (!sc && !method.IsConstructor && type.HasInterfaces)
			{
				foreach (TypeReference intf in type.Interfaces)
				{
					TypeDefinition td = intf.Resolve();
					if (td == null || !td.HasMethods)
						continue;
					foreach (MethodDefinition im in td.Methods)
					{
						if (IsSecurityCritical(im))
						{
							if (MethodDefinitionComparator.Compare(method, im))
							{
								sc = true;
								comment = String.Format("implements '{0}'.", im);
							}
						}
					}
				}
			}

			
			if (!method.IsVirtual)
			{
				// note: we don't want to break the override rules above (resulting in TypeLoadException)
				// an icall that is NOT part of the visible API is considered as critical (like a p/invoke)
				if (method.IsInternalCall)
				{
					sc = true;
					comment = "internal call";
				}
			}

			if (sc && !methods.ContainsKey(method))
			{
				// note: add a warning on visible API since adding [SecurityCritical]
				// on "new" visible API would introduce incompatibility (so this needs
				// to be reviewed).
				methods.Add(method, comment);
			}
		}

		public static bool IsMethodSignatureSafe(MethodDefinition method)
		{
			string s="";
			return IsMethodSignatureSafe(method, ref s);
		}
		public static bool IsMethodSignatureSafe(MethodDefinition method, ref string comment)
		{
			if (method.HasParameters)
			{
				// compilers will add public stuff like: System.Action`1::.ctor(System.Object,System.IntPtr)
				var declaringType = (method.DeclaringType as TypeDefinition);
				if (declaringType == null || declaringType.BaseType == null ||
				    declaringType.BaseType.FullName != "System.MulticastDelegate")
				{
					foreach (ParameterDefinition p in method.Parameters)
					{
						if (!CheckTypeIsSafe(p.ParameterType))
						{
							comment = String.Format("using '{0}' as a parameter type", p.ParameterType.FullName);
							return false;
						}
					}
				}
			}

			if (!method.IsConstructor)
			{
				TypeReference rtype = method.ReturnType.ReturnType;
				if (!CheckTypeIsSafe(rtype))
				{
					comment = String.Format("using '{0}' as return type", rtype.FullName);
					return false;
				}
			}
			return true;
		}

		private static string SignatureFor(MethodDefinition method)
		{
			return MethodSignatureProvider.SignatureFor(method);
		}

		private static bool IsVisible(MethodDefinition im)
		{
			throw new NotImplementedException();
		}

		private static bool IsSecurityCritical(MethodDefinition m)
		{
			return methods.ContainsKey(m);
		}

		static void ProcessType(TypeDefinition type)
		{
			if (CriticalTypes.Contains(type))
				return;
			if (type.HasConstructors)
				foreach (MethodDefinition ctor in type.Constructors)
					ProcessMethod(ctor);
			if (type.HasMethods)
				foreach (MethodDefinition method in type.Methods)
					ProcessMethod(method);
		}
	}
}
