using System;
using System.IO;
using System.Linq;
using System.Text;
using IKVM.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using Mono.CompilerServices.SymbolWriter;
using System.Runtime.InteropServices;

namespace Symbolicate
{
	struct Location {
		public string FileName;
		public int Line;
	}

	class LocationProvider {
		class AssemblyLocationProvider {
			Assembly assembly;
			MonoSymbolFile symbolFile;
			string seqPointDataPath;

			public AssemblyLocationProvider (Assembly assembly, MonoSymbolFile symbolFile, string seqPointDataPath)
			{
				this.assembly = assembly;
				this.symbolFile = symbolFile;
				this.seqPointDataPath = seqPointDataPath;
			}

			public bool TryGetLocation (string methodStr, string typeFullName, int offset, bool isOffsetIL, uint methodIndex, out Location location)
			{
				location = default (Location);
				if (symbolFile == null)
					return false;

				var type = assembly.GetTypes().FirstOrDefault (t => t.FullName == typeFullName);
				if (type == null)
					return false;

				var bindingflags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
				var method = type.GetMethods(bindingflags).FirstOrDefault (m => GetMethodFullName (m) == methodStr);
				if (method == null)
					return false;

				int ilOffset = (isOffsetIL)? offset : GetILOffsetFromFile (method.MetadataToken, methodIndex, offset);
				if (ilOffset < 0)
					return false;

				var methodSymbol = symbolFile.Methods [(method.MetadataToken & 0x00ffffff) - 1];

				var lineNumbers = methodSymbol.GetLineNumberTable ().LineNumbers;
				var lineNumber = lineNumbers.FirstOrDefault (l => l.Offset >= ilOffset) ?? lineNumbers.Last ();

				location.FileName = symbolFile.Sources [lineNumber.File-1].FileName;
				location.Line = lineNumber.Row;
				return true;
			}

			SeqPointInfo seqPointInfo;
			private int GetILOffsetFromFile (int methodToken, uint methodIndex, int nativeOffset)
			{
				if (seqPointInfo == null)
					seqPointInfo = SeqPointInfo.Read (seqPointDataPath);

				return seqPointInfo.GetILOffset (methodToken, methodIndex, nativeOffset);
			}

			private string GetMethodFullName (MethodBase m)
			{
				StringBuilder sb = new StringBuilder ();

				StackTraceHelper.GetFullNameForStackTrace (sb, m);

				return sb.ToString ();
			}
		}

		static readonly Universe ikvm_reflection = new Universe ();
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

			var assembly = ikvm_reflection.LoadFile (assemblyPath);
			MonoSymbolFile symbolFile = null;

			var symbolPath = assemblyPath + ".mdb";
			if (!File.Exists (symbolPath))
				Debug.WriteLine (".mdb file was not found for " + assemblyPath);
			else
				symbolFile = MonoSymbolFile.ReadSymbolFile (symbolPath);

			var seqPointDataPath = assemblyPath + ".msym";
			if (!File.Exists (seqPointDataPath))
				seqPointDataPath = null;

			assemblies.Add (assemblyPath, new AssemblyLocationProvider (assembly, symbolFile, seqPointDataPath));

			directories.Add (Path.GetDirectoryName (assemblyPath));

			foreach (var assemblyRef in assembly.GetReferencedAssemblies ()) {
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

		public bool TryGetLocation (string method, string typeFullName, int offset, bool isOffsetIL, uint methodIndex, out Location location)
		{
			location = default (Location);
			foreach (var assembly in assemblies.Values) {
				if (assembly.TryGetLocation (method, typeFullName, offset, isOffsetIL, methodIndex, out location))
					return true;
			}

			return false;
		}
	}
}

