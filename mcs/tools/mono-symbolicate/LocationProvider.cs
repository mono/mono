using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace Symbolicate
{
	class LocationProvider {
		class AssemblyLocationProvider {
			AssemblyDefinition assembly;
			string seqPointDataPath;

			public AssemblyLocationProvider (AssemblyDefinition assembly, string seqPointDataPath)
			{
				this.assembly = assembly;
				this.seqPointDataPath = seqPointDataPath;
			}

			public SequencePoint TryGetLocation (string typeFullName, string methodSignature, int offset, bool isOffsetIL, uint methodIndex)
			{
				if (!assembly.MainModule.HasSymbols)
					return null;

				TypeDefinition type = null;
				var nested = typeFullName.Split ('+');
				var types = assembly.MainModule.Types;
				foreach (var ntype in nested) {
					type = types.FirstOrDefault (t => t.Name == ntype);
					if (type == null)
						return null;

					types = type.NestedTypes;
				}

				var parensStart = methodSignature.IndexOf ('(');
				var methodName = methodSignature.Substring (0, parensStart).TrimEnd ();
				var methodParameters = methodSignature.Substring (parensStart);
				var method = type.Methods.FirstOrDefault (m => CompareName (m, methodName) && CompareParameters (m.Parameters, methodParameters));
				if (method == null)
					return null;

				int ilOffset = isOffsetIL ? offset : GetILOffsetFromFile (method.MetadataToken.ToInt32 (), methodIndex, offset);
				if (ilOffset < 0)
					return null;

				SequencePoint sp = null;
				foreach (var instr in method.Body.Instructions) {
					if (instr.SequencePoint != null)
						sp = instr.SequencePoint;
					
					if (instr.Offset >= ilOffset) {
						return sp;
					}
				}

				return null;
			}

			SeqPointInfo seqPointInfo;
			private int GetILOffsetFromFile (int methodToken, uint methodIndex, int nativeOffset)
			{
				if (seqPointInfo == null)
					seqPointInfo = SeqPointInfo.Read (seqPointDataPath);

				return seqPointInfo.GetILOffset (methodToken, methodIndex, nativeOffset);
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

		Dictionary<string, AssemblyLocationProvider> assemblies;
		HashSet<string> directories;

		public LocationProvider () {
			assemblies = new Dictionary<string, AssemblyLocationProvider> ();
			directories = new HashSet<string> ();
		}

		public void AddAssembly (string assemblyPath)
		{
			assemblyPath = Path.GetFullPath (assemblyPath);
			if (assemblies.ContainsKey (assemblyPath))
				return;

			if (!File.Exists (assemblyPath))
				throw new ArgumentException ("assemblyPath does not exist: "+ assemblyPath);

			var readerParameters = new ReaderParameters { ReadSymbols = true };
			var assembly = AssemblyDefinition.ReadAssembly (assemblyPath, readerParameters);

			var seqPointDataPath = assemblyPath + ".msym";
			if (!File.Exists (seqPointDataPath))
				seqPointDataPath = null;

			assemblies.Add (assemblyPath, new AssemblyLocationProvider (assembly, seqPointDataPath));

			// TODO: Should use AssemblyName with .net unification rules
			directories.Add (Path.GetDirectoryName (assemblyPath));

			foreach (var assemblyRef in assembly.MainModule.AssemblyReferences) {
				string refPath = null;
				foreach (var dir in directories) {
					refPath = Path.Combine (dir, assemblyRef.Name);
					if (File.Exists (refPath))
						break;
					refPath = Path.Combine (dir, assemblyRef.Name + ".dll");
					if (File.Exists (refPath))
						break;
					refPath = Path.Combine (dir, assemblyRef.Name + ".exe");
					if (File.Exists (refPath))
						break;
					refPath = null;
				}
				if (refPath != null)
					AddAssembly (refPath);
			}
		}

		public void AddDirectory (string directory)
		{
			directory = Path.GetFullPath (directory);
			if (!Directory.Exists (directory)) {
				Console.Error.WriteLine ("Directory " + directory + " does not exist.");
				return;
			}

			directories.Add (directory);
		}

		public SequencePoint TryGetLocation (string typeFullName, string methodSignature, int offset, bool isOffsetIL, uint methodIndex)
		{
			foreach (var assembly in assemblies.Values) {
				var loc = assembly.TryGetLocation (typeFullName, methodSignature, offset, isOffsetIL, methodIndex);
				if (loc != null)
					return loc;
			}

			return null;
		}
	}
}

