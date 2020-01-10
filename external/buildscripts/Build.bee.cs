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
			Artifacts.Add("7z",
				new Tuple<string, string>(
					"7z/9df1e3b3b120_12ed325f6a47f0e5cebc247dbe9282a5da280d392cce4e6c9ed227d57ff1e2ff.7z",
					"testing"));

			Artifacts.Add("MonoBleedingEdge",
				new Tuple<string, string>(
					"MonoBleedingEdge/9df1e3b3b120_ab6d2f131e6bd4fe2aacafb0f683e8fa4e1ccba35552b6fe89bf359b6ee16215.7z",
					"testing"));

			Artifacts.Add("reference-assemblies",
				new Tuple<string, string>(
					"reference-assemblies/9df1e3b3b120_bbb4750c6bf0a1784bec7d7c04b8ef5881f31f6212136e014694f3864a388886.7z",
					"testing"));
		}

		private static void RegisterWindowsArtifacts()
		{
			Artifacts.Add("android-ndk-r16b-windows",
				new Tuple<string, string>(
					"android-ndk-r16b-windows/9df1e3b3b120_403e0d58eabae03f0d9e8d1d2cea2dbf1d14c380c3d1c7eeb6e8c60ffc15e1b8.7z",
					"testing"));
		}

		private static void RegisterOSXArtifacts()
		{
			Artifacts.Add("android-ndk-r16b-darwin",
				new Tuple<string, string>(
					"android-ndk-r16b-darwin/9df1e3b3b120_c7cda5a221dd72799b7e618597b3f8766df7183d386becb2785631c2d3ac0d75.7z",
					"testing"));

			Artifacts.Add("MacBuildEnvironment",
				new Tuple<string, string>(
					"MacBuildEnvironment/9df1e3b3b120_2fc8e616a2e5dfb7907fc42d9576b427e692223c266dc3bc305de4bf03714e30.7z",
					"testing"));

			Artifacts.Add("mono-build-tools-extra",
				new Tuple<string, string>(
					"mono-build-tools-extra/70f9c4060363d11b3e69f000c8ff2c9ac8112bc2_e66148037ab6371658815c726e27e7ee7bdfdd705fb734708d07cbdfe7d8141e.7z",
					"testing"));
		}

		private static void RegisterLinuxArtifacts()
		{
			Artifacts.Add("android-ndk-r16b-linux",
				new Tuple<string, string>(
					"android-ndk-r16b-linux/9df1e3b3b120_fbabd18208d82cbc810266e8b566bb0ea4e1e438de38d450a92deaa3e23757b6.7z",
					"testing"));

			Artifacts.Add("linux-sdk-20170609",
				new Tuple<string, string>(
					"linux-sdk-20170609/9df1e3b3b120_9a3a0847d5b3767579e908b5a9ce050936617b1b9275a79a8b71bb3229998957.7z",
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

			Artifacts.Add("libgdiplus",
				new Tuple<string, string>(
					"libgdiplus/9df1e3b3b120_4cf7c08770db93922f54f38d2461b9122cddc898db58585864446e70c5ad3057.7z",
					"testing"));
		}
	}
}