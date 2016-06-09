using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace Mono
{
	class AssemblyLocationProvider
	{
		AssemblyDefinition assembly;

		public AssemblyLocationProvider (string assemblyPath)
		{
			assemblyPath = Path.GetFullPath (assemblyPath);

			if (!File.Exists (assemblyPath))
				throw new ArgumentException ("assemblyPath does not exist: "+ assemblyPath);

			var readerParameters = new ReaderParameters { ReadSymbols = true };
			assembly = AssemblyDefinition.ReadAssembly (assemblyPath, readerParameters);
		}

		public bool TryResolveLocation (StackFrameData sfData, SeqPointInfo seqPointInfo)
		{
			if (!assembly.MainModule.HasSymbols)
				return false;

			TypeDefinition type = null;
			var nested = sfData.TypeFullName.Split ('+');
			var types = assembly.MainModule.Types;
			foreach (var ntype in nested) {
				type = types.FirstOrDefault (t => t.Name == ntype);
				if (type == null)
					return false;

				types = type.NestedTypes;
			}

			var parensStart = sfData.MethodSignature.IndexOf ('(');
			var methodName = sfData.MethodSignature.Substring (0, parensStart).TrimEnd ();
			var methodParameters = sfData.MethodSignature.Substring (parensStart);
			var method = type.Methods.FirstOrDefault (m => CompareName (m, methodName) && CompareParameters (m.Parameters, methodParameters));
			if (method == null)
				return false;

			int ilOffset = sfData.IsILOffset ? sfData.Offset : seqPointInfo.GetILOffset (method.MetadataToken.ToInt32 (), sfData.MethodIndex, sfData.Offset);
			if (ilOffset < 0)
				return false;

			SequencePoint sp = null;
			foreach (var instr in method.Body.Instructions) {
				if (instr.SequencePoint != null)
					sp = instr.SequencePoint;
				
				if (instr.Offset >= ilOffset) {
					sfData.SetLocation (sp.Document.Url, sp.StartLine);
					return true;
				}
			}

			return false;
		}

		static bool CompareName (MethodDefinition candidate, string expected)
		{
			if (candidate.Name == expected)
				return true;

			if (!candidate.HasGenericParameters)
				return false;
			
			var genStart = expected.IndexOf ('[');
			if (genStart < 0)
				return false;

			if (candidate.Name != expected.Substring (0, genStart))
				return false;

			int arity = 1;
			for (int pos = genStart; pos < expected.Length; ++pos) {
				if (expected [pos] == ',')
					++arity;
			}

			return candidate.GenericParameters.Count == arity;
		}

		static bool CompareParameters (Collection<ParameterDefinition> candidate, string expected)
		{
			var builder = new StringBuilder ();
			builder.Append ("(");

			for (int i = 0; i < candidate.Count; i++) {
				var parameter = candidate [i];
				if (i > 0)
					builder.Append (", ");

				if (parameter.ParameterType.IsSentinel)
					builder.Append ("...,");

				var pt = parameter.ParameterType;
				if (!string.IsNullOrEmpty (pt.Namespace)) {
					builder.Append (pt.Namespace);
					builder.Append (".");
				}

				FormatElementType (pt, builder);

				builder.Append (" ");
				builder.Append (parameter.Name);
			}

			builder.Append (")");

			return builder.ToString () == expected;
		}

		static void FormatElementType (TypeReference tr, StringBuilder builder)
		{
			var ts = tr as TypeSpecification;
			if (ts != null) {
				if (ts.IsByReference) {
					FormatElementType (ts.ElementType, builder);
					builder.Append ("&");
					return;
				}

				var array = ts as ArrayType;
				if (array != null) {
					FormatElementType (ts.ElementType, builder);
					builder.Append ("[");

					for (int ii = 0; ii < array.Rank - 1; ++ii) {
						builder.Append (",");
					}

					builder.Append ("]");
					return;
				}
			}

			builder.Append (tr.Name);
		}
	}
}

