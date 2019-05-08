using System;
using System.IO;
using Bee.Core;
using Bee.Stevedore;
using NiceIO;
using Unity.BuildTools;
using System.Collections.Generic;
using System.Text;
using Bee.Stevedore.Program;

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

			var buildDependenciesConfigFile = buildScriptsRoot.Combine("buildDependencies.txt");
			Console.WriteLine(">>> Mono build dependecies stevedore version config file: " + buildDependenciesConfigFile);

			var stevedoreArtifactsDir = buildScriptsRoot.Combine("artifacts").Combine("Stevedore");
			Console.WriteLine(">>> Stevedore artifacts directory: " + stevedoreArtifactsDir + "\n");

			if (buildDependenciesConfigFile.Exists())
			{
				var artifactList = ParseBuildDependenciesConfigFile(buildDependenciesConfigFile.ToString());

				foreach (var item in artifactList)
				{
					var artifactName = item.Item1;
					var artifactId = item.Item2;
					var repoName = new RepoName(item.Item3);
					DownloadArtifact(artifactId, artifactName, repoName);
				}
			}
			else
			{
				throw new Exception($"{buildDependenciesConfigFile} does not exist");
			}
		}

		private static void DownloadArtifact(string artifactId, string artifactName, RepoName repoName)
		{
			Console.WriteLine($">>> Registering artifact {artifactName}");
			var artifact = new StevedoreArtifact(repoName, new ArtifactId(artifactId));
			Backend.Current.Register(artifact);
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
			# repo : <stevedore repo name (can be testing/public/unityinternal)> 

			name: 7z
			id: 7z/9df1e3b3b120_12ed325f6a47f0e5cebc247dbe9282a5da280d392cce4e6c9ed227d57ff1e2ff.7z
			repo: testing

			name: libgdiplus
			id : libgdiplus/9df1e3b3b120_4cf7c08770db93922f54f38d2461b9122cddc898db58585864446e70c5ad3057.7z
			repo: public
		*/
		private static List<Tuple<string, string, string>> ParseBuildDependenciesConfigFile(string buildDependenciesConfigFile)
		{
			var artifactNameIdFilesDictionary = new List<Tuple<string, string, string>>();

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
							var repoName = "";

							//read name
							name = line.Split(':')[1].Trim();

							//read id
							if ((line = streamReader.ReadLine()) != null)
								id = line.Split(':')[1].Trim();
							else
								throw new Exception($">>> Invalid {buildDependenciesConfigFile}, id name does not exist");

							//read repo name
							if ((line = streamReader.ReadLine()) != null)
								repoName = line.Split(':')[1].Trim();
							else
								throw new Exception($">>> Invalid {buildDependenciesConfigFile}, repo name does not exist");

							artifactNameIdFilesDictionary.Add(new Tuple<string, string, string>(name, id, repoName));
						}
					}
				}
			}
			return artifactNameIdFilesDictionary;
		}
	}
}
