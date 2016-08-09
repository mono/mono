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
	public class SymbolManager
	{
		string msymDir;
		Logger logger;

		public SymbolManager (string msymDir, Logger logger) {
			this.msymDir = msymDir;
			this.logger = logger;
		}

		internal bool TryResolveLocation (StackFrameData sfData)
		{
			if (sfData.Mvid == null)
				return false;

			var assemblyLocProvider = GetOrCreateAssemblyLocationProvider (sfData.Mvid);
			if (assemblyLocProvider == null)
				return false;

			SeqPointInfo seqPointInfo = null;
			if (!sfData.IsILOffset && sfData.Aotid != null)
				seqPointInfo = GetOrCreateSeqPointInfo (sfData.Aotid);

			return assemblyLocProvider.TryResolveLocation (sfData, seqPointInfo);
		}

		Dictionary<string, AssemblyLocationProvider> assemblies = new Dictionary<string, AssemblyLocationProvider> ();

		private AssemblyLocationProvider GetOrCreateAssemblyLocationProvider (string mvid)
		{
			if (assemblies.ContainsKey (mvid))
				return assemblies[mvid];

			var mvidDir = Path.Combine (msymDir, mvid);
			if (!Directory.Exists (mvidDir)) {
				logger.LogWarning ("MVID directory does not exist: {0}", mvidDir);
				return  null;
			}

			string assemblyPath = null;
			var exeFiles = Directory.GetFiles (mvidDir, "*.exe");
			var dllFiles = Directory.GetFiles (mvidDir, "*.dll");

			if (exeFiles.Length + dllFiles.Length != 1) {
				logger.LogError ("MVID directory should include one assembly: {0}", mvidDir);
				return null;
			}

			assemblyPath = (exeFiles.Length > 0)? exeFiles[0] : dllFiles[0];

			var locProvider = new AssemblyLocationProvider (assemblyPath, logger);

			assemblies.Add (mvid, locProvider);

			return locProvider;
		}

		Dictionary<string, SeqPointInfo> seqPointInfos = new Dictionary<string, SeqPointInfo> ();

		private SeqPointInfo GetOrCreateSeqPointInfo (string aotid)
		{
			if (seqPointInfos.ContainsKey (aotid))
				return seqPointInfos[aotid];

			var aotidDir = Path.Combine (msymDir, aotid);
			if (!Directory.Exists (aotidDir)) {
				logger.LogError ("AOTID directory does not exist: {0}", aotidDir);
				return null;
			}

			string msymFile = null;
			var msymFiles = Directory.GetFiles(aotidDir, "*.msym");
			msymFile = msymFiles[0];

			var seqPointInfo = SeqPointInfo.Read (msymFile);

			seqPointInfos.Add (aotid, seqPointInfo);

			return seqPointInfo;
		}

		public void StoreSymbols (params string[] lookupDirs)
		{
			foreach (var dir in lookupDirs) {
				var exeFiles = Directory.GetFiles (dir, "*.exe");
				var dllFiles = Directory.GetFiles (dir, "*.dll");
				var assemblies = exeFiles.Concat (dllFiles);
				foreach (var assemblyPath in assemblies) {
					var mdbPath = assemblyPath + ".mdb";
					if (!File.Exists (mdbPath)) {
						logger.LogWarning ("Directory {0} contains {1} but no mdb {2}.", dir, Path.GetFileName (assemblyPath), Path.GetFileName (mdbPath));
						// assemblies without mdb files are useless
						continue;
					}

					var assembly = AssemblyDefinition.ReadAssembly (assemblyPath);

					var mvid = assembly.MainModule.Mvid.ToString ("N");
					var mvidDir = Path.Combine (msymDir, mvid);

					if (Directory.Exists (mvidDir)) {
						try {
							Directory.Delete (mvidDir, true);
						} catch (DirectoryNotFoundException e) {}
					}

					Directory.CreateDirectory (mvidDir);

					var mvidAssemblyPath = Path.Combine (mvidDir, Path.GetFileName (assemblyPath));
					File.Copy (assemblyPath, mvidAssemblyPath);

					var mvidMdbPath = Path.Combine (mvidDir, Path.GetFileName (mdbPath));
					File.Copy (mdbPath, mvidMdbPath);

					// TODO create MVID dir for non main modules with links to main module MVID
				}
			}
		}
	}
}
