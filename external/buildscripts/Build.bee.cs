using System;
using System.IO;
using Bee.Core;
using Bee.Stevedore;
using NiceIO;
using Unity.BuildTools;
using System.Collections.Generic;
using System.Text;

namespace BuildProgram
{
	public class BuildProgram
	{
		internal static void Main()
		{
			if (IsRunningOnBuildMachine())
				Console.WriteLine("\n>>> Running on build machine");

			var monoRoot = GetMonoRootDir();
			Console.WriteLine(">>> Mono root directory: " + monoRoot);

			var buildScriptsRoot = monoRoot.Combine("external").Combine("buildscripts");
			Console.WriteLine(">>> Build scripts directory: " + buildScriptsRoot);

			var monoBuildDeps = monoRoot.Parent.Parent.Combine("mono-build-deps").Combine("build");
			Console.WriteLine(">>> Mono build dependecies directory: " + monoBuildDeps);

			var buildDependenciesConfigFile = buildScriptsRoot.Combine("buildDependencies.txt");
			Console.WriteLine(">>> Mono build dependecies stevedore version config file: " + buildDependenciesConfigFile);

			var stevedoreArtifactsDir = buildScriptsRoot.Combine("artifacts").Combine("Stevedore");
			Console.WriteLine(">>> Stevedore artifacts directory: " + stevedoreArtifactsDir + "\n");

			if (buildDependenciesConfigFile.Exists())
			{
				if (!monoBuildDeps.DirectoryExists())
				{
					Console.WriteLine(">>> " + monoBuildDeps + " does not exist. Creating it ...");
					monoBuildDeps.CreateDirectory();
				}

				var artifactNameIdFilesDictionary = ParseBuildDependenciesConfigFile(buildDependenciesConfigFile.ToString());

				foreach (var item in artifactNameIdFilesDictionary)
				{
					var artifactName = item.Key.Key;
					var artifactId = item.Key.Value;
					var artifactFiles = item.Value;
					DownloadAndCopyArtifact(artifactId, artifactName, artifactFiles, monoBuildDeps, stevedoreArtifactsDir);
				}
			}
			else
			{
				throw new Exception($"{buildDependenciesConfigFile} does not exist");
			}
		}

		private static void DownloadAndCopyArtifact(string artifactId, string artifactName, IEnumerable<NPath> artifacts, NPath monoBuildDeps, NPath stevedoreArtifactsDir)
		{
			var artifact = StevedoreArtifact.Testing(artifactId);
			Backend.Current.Register(artifact);

			var inputs = new List<NPath>();
			var targetFiles = new List<NPath>();
			foreach (var item in artifacts)
			{
				inputs.Add(stevedoreArtifactsDir.Combine(artifactName).Combine(item));
				targetFiles.Add(monoBuildDeps.Combine(artifactName).Combine(item));
			}

			var targetDir = monoBuildDeps;
			if (HostPlatform.IsWindows)
			{
				targetDir = monoBuildDeps.Combine(artifactName);
			}

			Backend.Current.AddAction(
				actionName: "CopyArtifact",
				targetFiles: targetFiles.ToArray(),
				inputs: inputs.ToArray(),
				executableStringFor: ExecutableStringForDirectoryCopy(stevedoreArtifactsDir.Combine(artifactName), targetDir),
				commandLineArguments: new string[] { },
				allowUnwrittenOutputFiles: true
			);
		}

		private static void ExecuteBuildScript(NPath[] inputFiles, NPath buildScript, NPath buildRoot)
		{
			Backend.Current.AddAction(
				actionName: "ExecuteBuildScript",
				targetFiles: new[] { buildRoot},
				inputs: inputFiles,
				executableStringFor: $"perl {buildScript}",
				commandLineArguments: new string[] { },
				allowUnwrittenOutputFiles: true
			);
		}
		private static NPath GetMonoRootDir()
		{
			var exePath = new NPath(System.Reflection.Assembly.GetEntryAssembly().Location);
			var monoRoot = exePath;

			//Assume "external" directory exists under monoRoot. 
			while (monoRoot.ToString().Contains("external"))
				monoRoot = monoRoot.Parent;

			return monoRoot;
		}

		private static string ExecutableStringForDirectoryCopy(NPath from, NPath target)
		{
			return HostPlatform.IsWindows
				? $"xcopy {from.InQuotes(SlashMode.Native)} {target.InQuotes(SlashMode.Native)} /s /e /d /Y"
				: $"cp -r -v {from.InQuotes(SlashMode.Native)} {target.InQuotes(SlashMode.Native)}";
		}

		private static bool IsRunningOnBuildMachine()
		{
			var buildMachine = Environment.GetEnvironmentVariable("UNITY_THISISABUILDMACHINE");
			return buildMachine != null && buildMachine == "1";
		}

		//Sample config file format:
		/*
			# Dependencoes to pull down from Stevedore. Please follow the following format:
			# name : <stevedore artifact name>
			# id : <stevedore artifact id>
			# files : <folder and/or comma-separated list of files downloaded and unpacked> 

			name: 7z
			id: 7z/9df1e3b3b120_12ed325f6a47f0e5cebc247dbe9282a5da280d392cce4e6c9ed227d57ff1e2ff.7z
			files : 7z

			name: libgdiplus
			id : libgdiplus/9df1e3b3b120_4cf7c08770db93922f54f38d2461b9122cddc898db58585864446e70c5ad3057.7z
			files : libgdiplus,lib2
		*/
		private static Dictionary<KeyValuePair<string, string>, List<NPath>> ParseBuildDependenciesConfigFile(string buildDependenciesConfigFile)
		{
			var artifactNameIdFilesDictionary = new Dictionary<KeyValuePair<string, string>, List<NPath>>();

			var fileStream = new FileStream(buildDependenciesConfigFile, FileMode.Open, FileAccess.Read);
			using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
			{
				string line;
				while ((line = streamReader.ReadLine()) != null)
				{
					//Check if line contains a comment
					if (!string.IsNullOrEmpty(line) && !line.Contains("#"))
					{
						if (line.Contains("name :") || line.Contains("name:"))
						{
							var name = "";
							var id = "";
							var files = "";

							//read name
							name = line.Split(':')[1].Trim();

							//read id
							if ((line = streamReader.ReadLine()) != null)
								id = line.Split(':')[1].Trim();
							else
								throw new Exception($">>> Invalid {buildDependenciesConfigFile}");

							//read comma separated folder/files list
							if ((line = streamReader.ReadLine()) != null)
								files = line.Split(':')[1].Trim();
							else
								throw new Exception($">>> Invalid {buildDependenciesConfigFile}");

							var filesList = new List<NPath>();
							if (!string.IsNullOrEmpty(files))
							{
								if (files.Contains(","))
									files.Split(',').ForEach(f => { filesList.Add(new NPath(f.Trim())); });
								else
									filesList.Add(new NPath(files.Trim()));
							}
							else
							{
								throw new Exception($">>> Invalid {buildDependenciesConfigFile}");
							}
							artifactNameIdFilesDictionary.Add(new KeyValuePair<string, string>(name, id), filesList);
						}
					}
				}
			}
			return artifactNameIdFilesDictionary;
		}
	}
}
