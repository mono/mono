using System;
using System.Collections.Generic;
using Bee.Core;
using Bee.Core.Stevedore;

namespace BuildProgram
{
    public class BuildProgram
    {
        private static readonly List<StevedoreArtifact> StevedoreArtifacts = new List<StevedoreArtifact>();

        internal static void Main()
        {
            var manifest = "manifest.stevedore";

            Backend.Current.StevedoreSettings = new StevedoreSettings
            {
                Manifest = { manifest }
            };

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

            foreach (var artifact in StevedoreArtifacts)
            {
                Console.WriteLine($">>> Registering artifact {artifact.ArtifactName}");
                artifact.UnpackToUnusualLocation($"artifacts/Stevedore/{artifact.ArtifactName}");
            }
        }

        private static void RegisterCommonArtifacts()
        {
            StevedoreArtifacts.Add(new StevedoreArtifact("MonoBleedingEdge"));
            StevedoreArtifacts.Add(new StevedoreArtifact("reference-assemblies"));
        }

        private static void RegisterWindowsArtifacts()
        {
            StevedoreArtifacts.Add(new StevedoreArtifact("android-ndk-win"));
        }

        private static void RegisterOSXArtifacts()
        {
            StevedoreArtifacts.Add(new StevedoreArtifact("android-ndk-mac"));
            StevedoreArtifacts.Add(new StevedoreArtifact("mac-toolchain-11_0"));
            StevedoreArtifacts.Add(new StevedoreArtifact("mono-build-tools-extra"));
        }

        private static void RegisterLinuxArtifacts()
        {
            StevedoreArtifacts.Add(new StevedoreArtifact("android-ndk-linux"));
            StevedoreArtifacts.Add(new StevedoreArtifact("sysroot-gcc-glibc-x64"));
            StevedoreArtifacts.Add(new StevedoreArtifact("toolchain-llvm-centos"));
            StevedoreArtifacts.Add(new StevedoreArtifact("cmake-linux-x64"));
        }

        private static void RegisterCommonNonWindowsArtifacts()
        {
            StevedoreArtifacts.Add(new StevedoreArtifact("libtool-src"));
            StevedoreArtifacts.Add(new StevedoreArtifact("texinfo-src"));
            StevedoreArtifacts.Add(new StevedoreArtifact("automake-src"));
            StevedoreArtifacts.Add(new StevedoreArtifact("autoconf-src"));
            StevedoreArtifacts.Add(new StevedoreArtifact("libgdiplus-mac"));
        }
    }
}