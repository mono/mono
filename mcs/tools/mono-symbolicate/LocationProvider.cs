using System;
using System.IO;
using System.Linq;
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

			public AssemblyLocationProvider (AssemblyDefinition assembly, MonoSymbolFile symbolFile)
			{
				this.assembly = assembly;
				this.symbolFile = symbolFile;
			}

			public bool TryGetLocation (string methodFullName, string[] methodParamsTypes, int ilOffset, out Location location)
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

			assemblies.Add (assemblyPath, new AssemblyLocationProvider (assembly, symbolFile));

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
			if (Directory.Exists (directory))
				throw new ArgumentException ("Directory " + directory + " does not exist.");

			directories.Add (directory);
		}

		public bool TryGetLocation (string methodName, string[] methodParams, int ilOffset, out Location location)
		{
			location = default (Location);
			foreach (var assembly in assemblies.Values) {
				if (assembly.TryGetLocation (methodName, methodParams, ilOffset, out location))
					return true;
			}

			return false;
		}
	}
}

