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
		string assemblyFullPath;
		Logger logger;

		public AssemblyLocationProvider (string assemblyPath, Logger logger)
		{
			assemblyPath = Path.GetFullPath (assemblyPath);
			this.logger = logger;

			if (!File.Exists (assemblyPath))
				throw new ArgumentException ("assemblyPath does not exist: "+ assemblyPath);

			assemblyFullPath = assemblyPath;
		}

		public bool TryResolveLocation (StackFrameData sfData, SeqPointInfo seqPointInfo, out Location location)
		{
			var readerParameters = new ReaderParameters { ReadSymbols = true };
			using (var assembly = AssemblyDefinition.ReadAssembly (assemblyFullPath, readerParameters)) {

				if (!assembly.MainModule.HasSymbols) {
					location = default;
					return false;
				}

				TypeDefinition type = null;
				string[] nested;
				if (sfData.TypeFullName.IndexOf ('/') >= 0)
					nested = sfData.TypeFullName.Split ('/');
				else
					nested = sfData.TypeFullName.Split ('+');

				var types = assembly.MainModule.Types;
				foreach (var ntype in nested) {
					if (type == null) {
						// Use namespace first time.
						type = types.FirstOrDefault (t => t.FullName == ntype);
					} else {
						type = types.FirstOrDefault (t => t.Name == ntype);
					}

					if (type == null) {
						logger.LogWarning ("Could not find type: {0}", ntype);
						location = default;
						return false;
					}

					types = type.NestedTypes;
				}

				var parensStart = sfData.MethodSignature.IndexOf ('(');
				var methodName = sfData.MethodSignature.Substring (0, parensStart).TrimEnd ();
				var methodParameters = sfData.MethodSignature.Substring (parensStart);
				var methods = type.Methods.Where (m => CompareName (m, methodName) && CompareParameters (m.Parameters, methodParameters)).ToArray ();
				if (methods.Length == 0) {
					logger.LogWarning ("Could not find method: {0}", methodName);
					location = default;
					return false;
				}
				if (methods.Length > 1) {
					logger.LogWarning ("Ambiguous match for method: {0}", sfData.MethodSignature);
					location = default;
					return false;
				}
				var method = methods [0];

				int ilOffset;
				if (sfData.IsILOffset) {
					ilOffset = sfData.Offset;
				} else {
					if (seqPointInfo == null) {
						location = default;
						return false;
					}

					ilOffset = seqPointInfo.GetILOffset (method.MetadataToken.ToInt32 (), sfData.MethodIndex, sfData.Offset);
				}

				if (ilOffset < 0) {
					location = default;
					return false;
				}

				if (!method.DebugInformation.HasSequencePoints) {
					var async_method = GetAsyncStateMachine (method);
					if (async_method?.ConstructorArguments?.Count == 1) {
						string state_machine = ((TypeReference)async_method.ConstructorArguments [0].Value).FullName;
						return TryResolveLocation (sfData.Relocate (state_machine, "MoveNext ()"), seqPointInfo, out location);
					}

					location = default;
					return false;
				}

				SequencePoint prev = null;
				foreach (var sp in method.DebugInformation.SequencePoints.OrderBy (l => l.Offset)) {
					if (sp.Offset >= ilOffset) {
						location = new Location (sp.Document.Url, sp.StartLine);
						return true;
					}

					prev = sp;
				}

				if (prev != null) {
					location = new Location (prev.Document.Url, prev.StartLine);
					return true;
				}

				location = default;
				return false;
			}
		}

		static CustomAttribute GetAsyncStateMachine (MethodDefinition method)
		{
			if (!method.HasCustomAttributes)
				return null;

			return method.CustomAttributes.FirstOrDefault (l =>
				l.AttributeType.Name == "AsyncStateMachineAttribute" && l.AttributeType.Namespace == "System.Runtime.CompilerServices");
		}

		static bool CompareName (MethodDefinition candidate, string expected)
		{
			if (candidate.Name == expected)
				return true;

			if (!candidate.HasGenericParameters)
				return false;

			var genStart = expected.IndexOf ('[');
			if (genStart < 0)
				genStart = expected.IndexOf ('<');

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

		static string RemoveGenerics (string expected, char open, char close)
		{
			if (expected.IndexOf (open) < 0)
				return expected;

			var sb = new StringBuilder ();
			for (int i = 0; i < expected.Length;) {
				int start = expected.IndexOf (open, i);
				int end = expected.IndexOf (close, i);
				if (start < 0 || end < 0) {
					sb.Append (expected, i, expected.Length - i);
					break;
				}

				bool is_ginst = false;
				for (int j = start + 1; j < end; ++j) {
					if (expected [j] != ',')
						is_ginst = true;
				}

				if (is_ginst) //discard the the generic args
					sb.Append (expected, i, start - i);
				else //include array arity
					sb.Append (expected, i, end + 1 - i);
				i = end + 1;

			}
			return sb.ToString ();
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
				FormatElementType (pt, builder);

				builder.Append (" ");
				builder.Append (parameter.Name);
			}

			builder.Append (")");

			if (builder.ToString () == RemoveGenerics (expected, '[', ']'))
				return true;

			//now try the compact runtime format.

			builder.Clear ();

			builder.Append ("(");

			for (int i = 0; i < candidate.Count; i++) {
				var parameter = candidate [i];
				if (i > 0)
					builder.Append (",");

				if (parameter.ParameterType.IsSentinel)
					builder.Append ("...,");

				var pt = parameter.ParameterType;

				RuntimeFormatElementType (pt, builder);
			}

			builder.Append (")");

			if (builder.ToString () == RemoveGenerics (expected, '<', '>'))
				return true;
			return false;

		}

		static void RuntimeFormatElementType (TypeReference tr, StringBuilder builder)
		{
			var ts = tr as TypeSpecification;
			if (ts != null) {
				if (ts.IsByReference) {
					RuntimeFormatElementType (ts.ElementType, builder);
					builder.Append ("&");
					return;
				}
			}

			switch (tr.MetadataType) {
			case MetadataType.Void:
				builder.Append ("void");
				break;
			case MetadataType.Boolean:
				builder.Append ("bool");
				break;
			case MetadataType.Char:
				builder.Append ("char");
				break;
			case MetadataType.SByte:
				builder.Append ("sbyte");
				break;
			case MetadataType.Byte:
				builder.Append ("byte");
				break;
			case MetadataType.Int16:
				builder.Append ("int16");
				break;
			case MetadataType.UInt16:
				builder.Append ("uint16");
				break;
			case MetadataType.Int32:
				builder.Append ("int");
				break;
			case MetadataType.UInt32:
				builder.Append ("uint");
				break;
			case MetadataType.Int64:
				builder.Append ("long");
				break;
			case MetadataType.UInt64:
				builder.Append ("ulong");
				break;
			case MetadataType.Single:
				builder.Append ("single");
				break;
			case MetadataType.Double:
				builder.Append ("double");
				break;
			case MetadataType.String:
				builder.Append ("string");
				break;
			case MetadataType.Pointer:
				builder.Append (((TypeSpecification)tr).ElementType);
				builder.Append ("*");
				break;
			case MetadataType.ValueType:
			case MetadataType.Class:
			case MetadataType.GenericInstance: {
				FormatName (tr, builder, '/');
				break;
			}
			case MetadataType.Var:
			case MetadataType.MVar:
				builder.Append (tr.Name);
				builder.Append ("_REF");
				break;
			case MetadataType.Array: {
				var array = (ArrayType)tr;
				RuntimeFormatElementType (array.ElementType, builder);
				builder.Append ("[");

				for (int i = 0; i < array.Rank - 1; ++i)
					builder.Append (",");

				builder.Append ("]");
				break;
			}

			case MetadataType.TypedByReference:
				builder.Append ("typedbyref");
				break;
			case MetadataType.IntPtr:
				builder.Append ("intptr");
				break;
			case MetadataType.UIntPtr:
				builder.Append ("uintptr");
				break;
			case MetadataType.FunctionPointer:
				builder.Append ("*()");
				break;
			case MetadataType.Object:
				builder.Append ("object");
				break;
			default:
				builder.Append ("-unknown-");
				break;
			}
		}

		static void FormatName (TypeReference tr, StringBuilder builder, char sep)
		{
			if (tr.IsNested && !(tr.MetadataType == MetadataType.Var || tr.MetadataType == MetadataType.MVar)) {
				FormatName (tr.DeclaringType, builder, sep);
				builder.Append (sep);
			}
			if (!string.IsNullOrEmpty (tr.Namespace)) {
				builder.Append (tr.Namespace);
				builder.Append (".");
			}

			builder.Append (tr.Name);
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
			FormatName (tr, builder, '+');
		}
	}
}

