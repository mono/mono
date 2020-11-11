import platform
from distutils.version import StrictVersion

class FsharpPackage(GitHubTarballPackage):
    def __init__(self):
        GitHubTarballPackage.__init__(self,
            'dotnet', 'fsharp',
            '5.0.0',
            '03283e07f6bd5717797acb288cf6044cedca2202',
            configure='',
            override_properties={ 'make': 'version= ./build.sh -c Release && version= ./.dotnet/dotnet restore setup/Swix/Microsoft.FSharp.SDK/Microsoft.FSharp.SDK.csproj --packages fsharp-nugets' })

        self.sources.extend(['patches/fsharp-netfx-multitarget.patch',
                             'patches/fsharp-portable-pdb.patch'])

    def prep(self):
        Package.prep(self)

        for p in range(1, len(self.sources)):
            self.sh('patch -p1 < "%{local_sources[' + str(p) + ']}"')

        # .NET Core 3.1 officially only supports macOS 10.13+ but we can get it to work on 10.12 by compiling our own System.Native.dylib
        if StrictVersion(platform.mac_ver()[0]) < StrictVersion("10.13.0"):
            self.sh('git clone --branch release/3.1 git://github.com/dotnet/corefx && git -C corefx checkout 1b5b5f0bf030bf7250c3258c140fa9e4214325c8')
            self.sh('echo \'\' > corefx/src/Native/Unix/System.Security.Cryptography.Native/CMakeLists.txt')
            self.sh('env -i HOME="$HOME" PATH="$PATH" USER="$USER" bash -c \'cd corefx && MACOSX_DEPLOYMENT_TARGET=10.12 src/Native/build-native.sh x64 Debug OSX outconfig netcoreapp-OSX-Debug-x64 -portable\'')
            self.sh('bash -c \'source eng/common/tools.sh && InitializeDotNetCli true\'')
            self.sh('cp corefx/artifacts/bin/native/netcoreapp-OSX-Debug-x64/System.Native.dylib .dotnet/shared/Microsoft.NETCore.App/3.1.6/System.Native.dylib')

    def build(self):
        Package.make(self)

    def install(self):
        fsharp_files = [
            "artifacts/bin/fsc/Release/net472/fsc.exe",
            "artifacts/bin/fsc/Release/net472/fsc.exe.config",
            "artifacts/bin/fsc/Release/net472/FSharp.Build.dll",
            "artifacts/bin/fsc/Release/net472/FSharp.Build.xml",
            "artifacts/bin/fsc/Release/net472/FSharp.Compiler.Private.dll",
            "artifacts/bin/fsc/Release/net472/FSharp.Compiler.Private.xml",
            "artifacts/bin/fsc/Release/net472/FSharp.Core.dll",
            "artifacts/bin/fsc/Release/net472/FSharp.Core.xml",
            "artifacts/bin/fsc/Release/net472/Microsoft.FSharp.Targets",
            "artifacts/bin/fsc/Release/net472/Microsoft.Portable.FSharp.Targets",
            "artifacts/bin/fsc/Release/net472/Microsoft.Build.dll",
            "artifacts/bin/fsc/Release/net472/Microsoft.Build.Framework.dll",
            "artifacts/bin/fsc/Release/net472/Microsoft.Build.Tasks.Core.dll",
            "artifacts/bin/fsc/Release/net472/Microsoft.Build.Utilities.Core.dll",
            "artifacts/bin/fsc/Release/net472/System.Buffers.dll",
            "artifacts/bin/fsc/Release/net472/System.Collections.Immutable.dll",
            "artifacts/bin/fsc/Release/net472/System.Memory.dll",
            "artifacts/bin/fsc/Release/net472/System.Numerics.Vectors.dll",
            "artifacts/bin/fsc/Release/net472/System.Reflection.Metadata.dll",
            "artifacts/bin/fsc/Release/net472/System.Reflection.TypeExtensions.dll",
            "artifacts/bin/fsc/Release/net472/System.Resources.Extensions.dll",
            "artifacts/bin/fsc/Release/net472/System.Runtime.CompilerServices.Unsafe.dll",
            "artifacts/bin/fsc/Release/net472/System.Threading.Tasks.Dataflow.dll",
            "artifacts/bin/fsi/Release/net472/fsi.exe",
            "artifacts/bin/fsi/Release/net472/fsi.exe.config",
            "artifacts/bin/fsiAnyCpu/Release/net472/fsiAnyCpu.exe",
            "artifacts/bin/fsiAnyCpu/Release/net472/fsiAnyCpu.exe.config",
            "artifacts/bin/fsi/Release/net472/FSharp.Compiler.Interactive.Settings.dll",
            "artifacts/bin/fsi/Release/net472/FSharp.Compiler.Interactive.Settings.xml",
            "artifacts/bin/fsi/Release/net472/FSharp.Compiler.Server.Shared.dll",
            "artifacts/bin/fsi/Release/net472/FSharp.Compiler.Server.Shared.xml",
            "artifacts/bin/fsi/Release/net472/FSharp.DependencyManager.Nuget.dll",
            "artifacts/bin/fsi/Release/net472/FSharp.DependencyManager.Nuget.xml",
            "artifacts/bin/fsi/Release/net472/Microsoft.DotNet.DependencyManager.dll",
            "artifacts/bin/fsi/Release/net472/Microsoft.DotNet.DependencyManager.xml"
        ]

        self.copy_files_to_dir(fsharp_files, os.path.join(self.staged_prefix, 'lib/mono/fsharp'))

        self.copy_api_files("fsharp-nugets/microsoft.visualfsharp.core.redist/1.0.0/content/.NETFramework/v4.0/4.3.0.0", os.path.join(self.staged_prefix, 'lib/mono/fsharp/api/.NETFramework/v4.0/4.3.0.0'))
        self.copy_api_files("fsharp-nugets/microsoft.visualfsharp.core.redist/1.0.0/content/.NETFramework/v4.0/4.3.1.0", os.path.join(self.staged_prefix, 'lib/mono/fsharp/api/.NETFramework/v4.0/4.3.1.0'))
        self.copy_api_files("fsharp-nugets/microsoft.visualfsharp.core.redist/1.0.0/content/.NETFramework/v4.0/4.4.0.0", os.path.join(self.staged_prefix, 'lib/mono/fsharp/api/.NETFramework/v4.0/4.4.0.0'))
        self.copy_api_files("fsharp-nugets/microsoft.portable.fsharp.core/10.1.0/lib/versions/4.4.1.0", os.path.join(self.staged_prefix, 'lib/mono/fsharp/api/.NETFramework/v4.0/4.4.1.0'))
        self.copy_api_files("fsharp-nugets/fsharp.core/4.3.4/lib/net45", os.path.join(self.staged_prefix, 'lib/mono/fsharp/api/.NETFramework/v4.0/4.4.3.0'))
        self.copy_api_files("fsharp-nugets/fsharp.core/4.3.4/lib/net45", os.path.join(self.staged_prefix, 'lib/mono/fsharp/api/.NETFramework/v4.0/4.4.5.0'))

        self.copy_api_files("fsharp-nugets/microsoft.visualfsharp.core.redist/1.0.0/content/.NETCore/3.3.1.0", os.path.join(self.staged_prefix, 'lib/mono/fsharp/api/.NETCore/3.3.1.0'))
        self.copy_api_files("fsharp-nugets/microsoft.visualfsharp.core.redist/1.0.0/content/.NETCore/3.7.4.0", os.path.join(self.staged_prefix, 'lib/mono/fsharp/api/.NETCore/3.7.4.0'))
        self.copy_api_files("fsharp-nugets/microsoft.visualfsharp.core.redist/1.0.0/content/.NETCore/3.78.3.1", os.path.join(self.staged_prefix, 'lib/mono/fsharp/api/.NETCore/3.78.3.1'))
        self.copy_api_files("fsharp-nugets/microsoft.visualfsharp.core.redist/1.0.0/content/.NETCore/3.78.4.0", os.path.join(self.staged_prefix, 'lib/mono/fsharp/api/.NETCore/3.78.4.0'))
        self.copy_api_files("fsharp-nugets/microsoft.visualfsharp.core.redist/1.0.0/content/.NETCore/3.259.4.0", os.path.join(self.staged_prefix, 'lib/mono/fsharp/api/.NETCore/3.259.4.0'))
        self.copy_api_files("fsharp-nugets/microsoft.portable.fsharp.core/10.1.0/lib/profiles/portable-net45+netcore45", os.path.join(self.staged_prefix, 'lib/mono/fsharp/api/.NETCore/3.7.41.0'))
        self.copy_api_files("fsharp-nugets/microsoft.portable.fsharp.core/10.1.0/lib/profiles/portable-net45+netcore45+wp8", os.path.join(self.staged_prefix, 'lib/mono/fsharp/api/.NETCore/3.78.41.0'))
        self.copy_api_files("fsharp-nugets/microsoft.portable.fsharp.core/10.1.0/lib/profiles/portable-net45+netcore45+wpa81+wp8", os.path.join(self.staged_prefix, 'lib/mono/fsharp/api/.NETCore/3.259.41.0'))

        self.copy_api_files("fsharp-nugets/microsoft.visualfsharp.core.redist/1.0.0/content/.NETPortable/2.3.5.0", os.path.join(self.staged_prefix, 'lib/mono/fsharp/api/.NETPortable/2.3.5.0'))
        self.copy_api_files("fsharp-nugets/microsoft.visualfsharp.core.redist/1.0.0/content/.NETPortable/2.3.5.1", os.path.join(self.staged_prefix, 'lib/mono/fsharp/api/.NETPortable/2.3.5.1'))
        self.copy_api_files("fsharp-nugets/microsoft.visualfsharp.core.redist/1.0.0/content/.NETPortable/3.47.4.0", os.path.join(self.staged_prefix, 'lib/mono/fsharp/api/.NETPortable/3.47.4.0'))
        self.copy_api_files("fsharp-nugets/microsoft.portable.fsharp.core/10.1.0/lib/profiles/portable-net45+sl5+netcore45", os.path.join(self.staged_prefix, 'lib/mono/fsharp/api/.NETPortable/3.47.41.0'))

        self.copy_netsdk_files("artifacts/bin/fsc/Release/net472", os.path.join(self.staged_prefix, "lib/mono/xbuild/Microsoft/VisualStudio/v/FSharp"))
        self.copy_netsdk_files("artifacts/bin/fsc/Release/net472", os.path.join(self.staged_prefix, "lib/mono/xbuild/Microsoft/VisualStudio/v11.0/FSharp"))
        self.copy_netsdk_files("artifacts/bin/fsc/Release/net472", os.path.join(self.staged_prefix, "lib/mono/xbuild/Microsoft/VisualStudio/v12.0/FSharp"))
        self.copy_netsdk_files("artifacts/bin/fsc/Release/net472", os.path.join(self.staged_prefix, "lib/mono/xbuild/Microsoft/VisualStudio/v14.0/FSharp"))
        self.copy_netsdk_files("artifacts/bin/fsc/Release/net472", os.path.join(self.staged_prefix, "lib/mono/xbuild/Microsoft/VisualStudio/v15.0/FSharp"))
        self.copy_netsdk_files("artifacts/bin/fsc/Release/net472", os.path.join(self.staged_prefix, "lib/mono/xbuild/Microsoft/VisualStudio/v16.0/FSharp"))

        self.write_targets_files(os.path.join(self.staged_prefix, "lib/mono/Microsoft F#/v4.0"))
        self.write_targets_files(os.path.join(self.staged_prefix, "lib/mono/Microsoft SDKs/F#/3.0/Framework/v4.0"))
        self.write_targets_files(os.path.join(self.staged_prefix, "lib/mono/Microsoft SDKs/F#/3.1/Framework/v4.0"))
        self.write_targets_files(os.path.join(self.staged_prefix, "lib/mono/Microsoft SDKs/F#/4.0/Framework/v4.0"))
        self.write_targets_files(os.path.join(self.staged_prefix, "lib/mono/Microsoft SDKs/F#/4.1/Framework/v4.0"))
        self.write_targets_files(os.path.join(self.staged_prefix, "lib/mono/xbuild/Microsoft/VisualStudio/v/FSharp"))
        self.write_targets_files(os.path.join(self.staged_prefix, "lib/mono/xbuild/Microsoft/VisualStudio/v11.0/FSharp"))
        self.write_targets_files(os.path.join(self.staged_prefix, "lib/mono/xbuild/Microsoft/VisualStudio/v12.0/FSharp"))
        self.write_targets_files(os.path.join(self.staged_prefix, "lib/mono/xbuild/Microsoft/VisualStudio/v14.0/FSharp"))
        self.write_targets_files(os.path.join(self.staged_prefix, "lib/mono/xbuild/Microsoft/VisualStudio/v15.0/FSharp"))
        self.write_targets_files(os.path.join(self.staged_prefix, "lib/mono/xbuild/Microsoft/VisualStudio/v16.0/FSharp"))

        self.write_script(os.path.join(self.staged_prefix, "bin"), "fsharpc", "fsc.exe")
        self.write_script(os.path.join(self.staged_prefix, "bin"), "fsharpi", "fsi.exe")
        self.write_script(os.path.join(self.staged_prefix, "bin"), "fsharpiAnyCpu", "fsiAnyCpu.exe")

    def copy_api_files(self, source_dir, target_dir):
        fsharpcore_files = [
            os.path.join(source_dir, "FSharp.Core.dll"),
            os.path.join(source_dir, "FSharp.Core.xml"),
            os.path.join(source_dir, "FSharp.Core.sigdata"),
            os.path.join(source_dir, "FSharp.Core.optdata")
        ]
        self.copy_files_to_dir(fsharpcore_files, target_dir)

    def copy_netsdk_files(self, source_dir, target_dir):
        netsdk_files = [
            os.path.join(source_dir, "Microsoft.FSharp.NetSdk.props"),
            os.path.join(source_dir, "Microsoft.FSharp.NetSdk.targets"),
            os.path.join(source_dir, "Microsoft.FSharp.Overrides.NetSdk.targets")
        ]
        self.copy_files_to_dir(netsdk_files, target_dir)

    def write_targets_files(self, target_dir):
        ensure_dir(target_dir)
        with open(os.path.join(target_dir, "Microsoft.FSharp.Targets"), "w") as output:
            output.write('<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">\n')
            output.write('    <Import Project="' + self.package_prefix + '/lib/mono/fsharp/Microsoft.FSharp.Targets" />\n')
            output.write('</Project>\n')
        with open(os.path.join(target_dir, "Microsoft.Portable.FSharp.Targets"), "w") as output:
            output.write('<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">\n')
            output.write('    <Import Project="' + self.package_prefix + '/lib/mono/fsharp/Microsoft.Portable.FSharp.Targets" />\n')
            output.write('</Project>\n')

    def write_script(self, target_dir, script_name, exe_name):
        ensure_dir(target_dir)
        wrapper = os.path.join(target_dir, script_name)
        with open(wrapper, "w") as output:
            output.write('#!/bin/sh\n')
            output.write('EXEC="exec "\n')
            output.write('\n')
            output.write('if test x"$1" = x--debug; then\n')
            output.write('   DEBUG=--debug\n')
            output.write('   shift\n')
            output.write('fi\n')
            output.write('\n')
            output.write('if test x"$1" = x--gdb; then\n')
            output.write('   shift\n')
            output.write('   EXEC="gdb --eval-command=run --args "\n')
            output.write('fi\n')
            output.write('\n')
            output.write('if test x"$1" = x--valgrind; then\n')
            output.write('  shift\n')
            output.write('  EXEC="valgrind $VALGRIND_OPTIONS"   \n')
            output.write('fi\n')
            output.write('\n')
            output.write('$EXEC ' + self.package_prefix + '/bin/mono $DEBUG $MONO_OPTIONS ' + self.package_prefix + '/lib/mono/fsharp/' + exe_name + ' --exename:$(basename "$0") "$@"\n')
        os.chmod(wrapper, 0o755)

    def copy_files_to_dir(self, files, target_dir):
        ensure_dir(target_dir)

        for f in files:
            source = os.path.join(self.workspace, f)
            target = os.path.join(target_dir, os.path.basename(source))
            shutil.copy(source, target)

FsharpPackage()
