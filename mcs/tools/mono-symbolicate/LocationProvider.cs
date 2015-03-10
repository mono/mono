using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.CompilerServices.SymbolWriter;

namespace Symbolicate
{
	struct Location {
		public string FileName;
		public int Line;
	}

	class LocationProvider {
		class AssemblyLocationProvider {
			AssemblyDefinition assembly;
			MonoSymbolFile symbolFile;
			string seqPointDataPath;

			public AssemblyLocationProvider (AssemblyDefinition assembly, MonoSymbolFile symbolFile, string seqPointDataPath)
			{
				this.assembly = assembly;
				this.symbolFile = symbolFile;
				this.seqPointDataPath = seqPointDataPath;
			}

			public bool TryGetLocation (string methodFullName, string[] methodParamsTypes, int offset, bool isOffsetIL, out Location location)
			{
				location = default (Location);
				if (symbolFile == null)
					return false;

				var typeNameEnd = methodFullName.LastIndexOf (".");
				var typeName = methodFullName.Substring (0, typeNameEnd);
				var methodName = methodFullName.Substring (typeNameEnd + 1, methodFullName.Length - typeNameEnd - 1);

				var type = assembly.MainModule.Types.FirstOrDefault (t => t.FullName == typeName);
				if (type == null)
					return false;

				var method = type.Methods.FirstOrDefault (m => {
					if (m.Name != methodName)
						return false;

					if (m.Parameters.Count != methodParamsTypes.Length)
						return false;

					for (var i = 0; i < methodParamsTypes.Length; i++) {
						var paramType = m.Parameters[i].ParameterType;
						if (paramType.Name != methodParamsTypes[i])
							return false;
					}

					return true;
				});

				if (method == null)
					return false;

				int ilOffset = (isOffsetIL)? offset : GetILOffsetFromFile (method.MetadataToken.ToInt32 (), offset);
				if (ilOffset < 0)
					return false;

				var methodSymbol = symbolFile.Methods [method.MetadataToken.RID-1];

				foreach (var lineNumber in methodSymbol.GetLineNumberTable ().LineNumbers) {
					if (lineNumber.Offset < ilOffset)
						continue;

					location.FileName = symbolFile.Sources [lineNumber.File-1].FileName;
					location.Line = lineNumber.Row;
					return true;
				}

				return false;
			}

			static MethodInfo methodGetIL;
			private int GetILOffsetFromFile (int methodToken, int nativeOffset)
			{
				if (string.IsNullOrEmpty (seqPointDataPath))
					return -1;

				if (methodGetIL == null)
					methodGetIL = typeof (StackFrame).GetMethod ("GetILOffsetFromFile", BindingFlags.NonPublic | BindingFlags.Static);

				if (methodGetIL == null)
					throw new Exception ("System.Diagnostics.StackFrame.GetILOffsetFromFile could not be found, make sure you have an updated mono installed.");

				return (int) methodGetIL.Invoke (null, new object[] {seqPointDataPath, methodToken, nativeOffset});
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

			var assembly = AssemblyDefinition.ReadAssembly (assemblyPath);
			MonoSymbolFile symbolFile = null;

			var symbolPath = assemblyPath + ".mdb";
			if (!File.Exists (symbolPath))
				Debug.WriteLine (".mdb file was not found for " + assemblyPath);
			else
				symbolFile = MonoSymbolFile.ReadSymbolFile (assemblyPath + ".mdb");

			var seqPointDataPath = assemblyPath + ".msym";
			if (!File.Exists (seqPointDataPath))
				seqPointDataPath = null;

			assemblies.Add (assemblyPath, new AssemblyLocationProvider (assembly, symbolFile, seqPointDataPath));

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

		public bool TryGetLocation (string methodName, string[] methodParams, int offset, bool isOffsetIL, out Location location)
		{
			location = default (Location);
			foreach (var assembly in assemblies.Values) {
				if (assembly.TryGetLocation (methodName, methodParams, offset, isOffsetIL, out location))
					return true;
			}

			return false;
		}
	}
}

