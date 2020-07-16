using System;
using System.Collections.Generic;
using Bee.Core;
using Bee.Stevedore;
using Bee.Stevedore.Program;
using Unity.BuildSystem.NativeProgramSupport;

namespace BuildProgram
{
	public class BuildProgram
	{
		private static readonly Dictionary<string, Tuple<string, string>> Artifacts = new Dictionary<string, Tuple<string, string>>();

		internal static void Main()
		{
			RegisterCommonArtifacts();

			if (Platform.HostPlatform is WindowsPlatform)
			{
				RegisterWindowsArtifacts();
			}
			else
			{
				RegisterCommonNonWindowsArtifacts();

				if (Platform.HostPlatform is MacOSXPlatform)
				{
					RegisterOSXArtifacts();
				}
				else if (Platform.HostPlatform is LinuxPlatform)
				{
					RegisterLinuxArtifacts();
				}
			}

			foreach (var artifact in Artifacts)
			{
				var name = artifact.Key;
				var id = artifact.Value.Item1;
				var repo = new RepoName(artifact.Value.Item2);

				Console.WriteLine($">>> Registering artifact {name}");
				var stevedoreArtifact = new StevedoreArtifact(repo, new ArtifactId(id));
				Backend.Current.Register(stevedoreArtifact);
			}
		}

		private static void RegisterCommonArtifacts()
		{
			Artifacts.Add("MonoBleedingEdge",
				new Tuple<string, string>(
					"MonoBleedingEdge/fd0d97a7a35_5d627f842afebea942027a7fe8a590effb76deaf44736482b8bbcfae58316d42.7z",
					"unity-internal"));

			Artifacts.Add("reference-assemblies",
				new Tuple<string, string>(
					"reference-assemblies/1.0_fc1889ab066ec621a44e51c666d750590b0496d8284b4420e1119c26ce0c7462.7z",
					"unity-internal"));
		}

		private static void RegisterWindowsArtifacts()
		{
			Artifacts.Add("android-ndk-win",
				new Tuple<string, string>(
					"android-ndk-win/r19-unity_799f451638695b9da797fcd509f9f2a8e59e35603e78a344acfd7fa2ba5f0ce1.7z",
					"unity-internal"));
		}

		private static void RegisterOSXArtifacts()
		{
			Artifacts.Add("android-ndk-mac",
				new Tuple<string, string>(
					"android-ndk-mac/r19-unity_8b169ff2a8234c85e0c5ba3c776aa94273cd3c15fdc96d213154970d87938589.7z",
					"unity-internal"));

			Artifacts.Add("MacBuildEnvironment",
				new Tuple<string, string>(
					"mac-toolchain-11_0/12.0-12A8158a_2b346a6c93e9c82250a1d88c02f24a5dea9f0bd1aa2ef44109896a44800e8c68.zip",
					"unity-internal"));

			Artifacts.Add("mono-build-tools-extra",
				new Tuple<string, string>(
					"mono-build-tools-extra/9de3c42ef81ec4f79b53e7db32d390227d8c43c4_fa9931c37b7a4ca636eb9e0e48252c4cb591caaa9b77c41b75795037868c1256.7z",
					"unity-internal"));
		}

		private static void RegisterLinuxArtifacts()
		{
			Artifacts.Add("android-ndk-linux",
				new Tuple<string, string>(
					"android-ndk-linux/r19-unity_c81ed9864399ec6f2d28181a58cc0588481499dddd4970e7e9f4bfbb64e8114e.7z",
					"unity-internal"));

			Artifacts.Add("sysroot-gcc-glibc-x64",
				new Tuple<string, string>(
					"sysroot-gcc-glibc-x64/9.1.0-2.17-v0_608efc24a3b402ec57809211b16a6d32d519f891d4038e1fc8509fe300c395b2.7z",
					"testing"));

			Artifacts.Add("toolchain-llvm-centos",
				new Tuple<string, string>(
					"toolchain-llvm-centos/9.0.1-7.7.1908-v0_9c1c26d5205c7ead3cba5a545796579d753c459ac5e16d0690d1b087efa7835d.7z",
					"testing"));
		}

		private static void RegisterCommonNonWindowsArtifacts()
		{
			Artifacts.Add("libtool-src",
				new Tuple<string, string>(
					"libtool-src/2.4.6_49a0ed204b3b24572e044400cd05513f611bcca6ced0d0816a57ac3b17376257.7z",
					"public"));

			Artifacts.Add("texinfo-src",
				new Tuple<string, string>(
					"texinfo-src/4.8_975b9657ebef8a4fe3897047ca450b757a0a956b05399dc813f63e84829bac6a.7z",
					"public"));

			Artifacts.Add("automake-src",
				new Tuple<string, string>(
					"automake-src/1.16.1_d281b950e26265f55f0a63188a8c6388e638b354b7ed80d186690119cbc4f953.7z",
					"public"));

			Artifacts.Add("autoconf-src",
				new Tuple<string, string>(
					"autoconf-src/2.69_0e4ba7a0363c68ad08a7d138b228596aecdaea68e1d8b8eefc645e6ac8fc85c7.7z",
					"public"));

			Artifacts.Add("libgdiplus-mac",
				new Tuple<string, string>(
					"libgdiplus-mac/9df1e3b3b120_4cf7c08770db93922f54f38d2461b9122cddc898db58585864446e70c5ad3057.7z",
					"unity-internal"));
		}
	}
}