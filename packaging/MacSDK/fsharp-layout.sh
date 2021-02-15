#!/bin/bash

set -euxo pipefail

if [[ $# -ne 2 ]]; then
  echo "Usage: ./fsharp_layout.sh [F# repo folder path] [Mono prefix folder path]"
  exit 2
fi
FSHARP_REPO_DIR=$1
ARTIFACTS_DIR="$1/artifacts"
PREFIX_DIR=$2
MONO_BIN_DIR="$2/bin"
MONO_LIB_DIR="$2/lib/mono"
MONO_FSHARP_LIB_DIR="$MONO_LIB_DIR/fsharp"

mkdir -p "$MONO_FSHARP_LIB_DIR"
cp "$ARTIFACTS_DIR/bin/fsc/Release/net472/fsc.exe" "$MONO_FSHARP_LIB_DIR/fsc.exe"
cp "$ARTIFACTS_DIR/bin/fsc/Release/net472/fsc.exe.config" "$MONO_FSHARP_LIB_DIR/fsc.exe.config"
cp "$ARTIFACTS_DIR/bin/fsc/Release/net472/FSharp.Build.dll" "$MONO_FSHARP_LIB_DIR/FSharp.Build.dll"
cp "$ARTIFACTS_DIR/bin/fsc/Release/net472/FSharp.Build.xml" "$MONO_FSHARP_LIB_DIR/FSharp.Build.xml"
cp "$ARTIFACTS_DIR/bin/fsc/Release/net472/FSharp.Compiler.Private.dll" "$MONO_FSHARP_LIB_DIR/FSharp.Compiler.Private.dll"
cp "$ARTIFACTS_DIR/bin/fsc/Release/net472/FSharp.Compiler.Private.xml" "$MONO_FSHARP_LIB_DIR/FSharp.Compiler.Private.xml"
cp "$ARTIFACTS_DIR/bin/fsc/Release/net472/FSharp.Core.dll" "$MONO_FSHARP_LIB_DIR/FSharp.Core.dll"
cp "$ARTIFACTS_DIR/bin/fsc/Release/net472/FSharp.Core.xml" "$MONO_FSHARP_LIB_DIR/FSharp.Core.xml"
cp "$ARTIFACTS_DIR/bin/fsc/Release/net472/Microsoft.FSharp.Targets" "$MONO_FSHARP_LIB_DIR/Microsoft.FSharp.Targets"
cp "$ARTIFACTS_DIR/bin/fsc/Release/net472/Microsoft.Portable.FSharp.Targets" "$MONO_FSHARP_LIB_DIR/Microsoft.Portable.FSharp.Targets"
cp "$ARTIFACTS_DIR/bin/fsc/Release/net472/Microsoft.Build.dll" "$MONO_FSHARP_LIB_DIR/Microsoft.Build.dll"
cp "$ARTIFACTS_DIR/bin/fsc/Release/net472/Microsoft.Build.Framework.dll" "$MONO_FSHARP_LIB_DIR/Microsoft.Build.Framework.dll"
cp "$ARTIFACTS_DIR/bin/fsc/Release/net472/Microsoft.Build.Tasks.Core.dll" "$MONO_FSHARP_LIB_DIR/Microsoft.Build.Tasks.Core.dll"
cp "$ARTIFACTS_DIR/bin/fsc/Release/net472/Microsoft.Build.Utilities.Core.dll" "$MONO_FSHARP_LIB_DIR/Microsoft.Build.Utilities.Core.dll"
cp "$ARTIFACTS_DIR/bin/fsc/Release/net472/System.Buffers.dll" "$MONO_FSHARP_LIB_DIR/System.Buffers.dll"
cp "$ARTIFACTS_DIR/bin/fsc/Release/net472/System.Collections.Immutable.dll" "$MONO_FSHARP_LIB_DIR/System.Collections.Immutable.dll"
cp "$ARTIFACTS_DIR/bin/fsc/Release/net472/System.Memory.dll" "$MONO_FSHARP_LIB_DIR/System.Memory.dll"
cp "$ARTIFACTS_DIR/bin/fsc/Release/net472/System.Numerics.Vectors.dll" "$MONO_FSHARP_LIB_DIR/System.Numerics.Vectors.dll"
cp "$ARTIFACTS_DIR/bin/fsc/Release/net472/System.Reflection.Metadata.dll" "$MONO_FSHARP_LIB_DIR/System.Reflection.Metadata.dll"
cp "$ARTIFACTS_DIR/bin/fsc/Release/net472/System.Reflection.TypeExtensions.dll" "$MONO_FSHARP_LIB_DIR/System.Reflection.TypeExtensions.dll"
cp "$ARTIFACTS_DIR/bin/fsc/Release/net472/System.Resources.Extensions.dll" "$MONO_FSHARP_LIB_DIR/System.Resources.Extensions.dll"
cp "$ARTIFACTS_DIR/bin/fsc/Release/net472/System.Runtime.CompilerServices.Unsafe.dll" "$MONO_FSHARP_LIB_DIR/System.Runtime.CompilerServices.Unsafe.dll"
cp "$ARTIFACTS_DIR/bin/fsc/Release/net472/System.Threading.Tasks.Dataflow.dll" "$MONO_FSHARP_LIB_DIR/System.Threading.Tasks.Dataflow.dll"
cp "$ARTIFACTS_DIR/bin/fsi/Release/net472/fsi.exe" "$MONO_FSHARP_LIB_DIR/fsi.exe"
cp "$ARTIFACTS_DIR/bin/fsi/Release/net472/fsi.exe.config" "$MONO_FSHARP_LIB_DIR/fsi.exe.config"
cp "$ARTIFACTS_DIR/bin/fsiAnyCpu/Release/net472/fsiAnyCpu.exe" "$MONO_FSHARP_LIB_DIR/fsiAnyCpu.exe"
cp "$ARTIFACTS_DIR/bin/fsiAnyCpu/Release/net472/fsiAnyCpu.exe.config" "$MONO_FSHARP_LIB_DIR/fsiAnyCpu.exe.config"
cp "$ARTIFACTS_DIR/bin/fsi/Release/net472/FSharp.Compiler.Interactive.Settings.dll" "$MONO_FSHARP_LIB_DIR/FSharp.Compiler.Interactive.Settings.dll"
cp "$ARTIFACTS_DIR/bin/fsi/Release/net472/FSharp.Compiler.Interactive.Settings.xml" "$MONO_FSHARP_LIB_DIR/FSharp.Compiler.Interactive.Settings.xml"
cp "$ARTIFACTS_DIR/bin/fsi/Release/net472/FSharp.Compiler.Server.Shared.dll" "$MONO_FSHARP_LIB_DIR/FSharp.Compiler.Server.Shared.dll"
cp "$ARTIFACTS_DIR/bin/fsi/Release/net472/FSharp.Compiler.Server.Shared.xml" "$MONO_FSHARP_LIB_DIR/FSharp.Compiler.Server.Shared.xml"
cp "$ARTIFACTS_DIR/bin/fsi/Release/net472/FSharp.DependencyManager.Nuget.dll" "$MONO_FSHARP_LIB_DIR/FSharp.DependencyManager.Nuget.dll"
cp "$ARTIFACTS_DIR/bin/fsi/Release/net472/FSharp.DependencyManager.Nuget.xml" "$MONO_FSHARP_LIB_DIR/FSharp.DependencyManager.Nuget.xml"
cp "$ARTIFACTS_DIR/bin/fsi/Release/net472/Microsoft.DotNet.DependencyManager.dll" "$MONO_FSHARP_LIB_DIR/Microsoft.DotNet.DependencyManager.dll"
cp "$ARTIFACTS_DIR/bin/fsi/Release/net472/Microsoft.DotNet.DependencyManager.xml" "$MONO_FSHARP_LIB_DIR/Microsoft.DotNet.DependencyManager.xml"

function copy_fsharp_api_files {
  mkdir -p "$2"
  cp "$1/FSharp.Core.dll" "$2/FSharp.Core.dll"
  cp "$1/FSharp.Core.xml" "$2/FSharp.Core.xml"
  cp "$1/FSharp.Core.sigdata" "$2/FSharp.Core.sigdata"
  cp "$1/FSharp.Core.optdata" "$2/FSharp.Core.optdata"
}

copy_fsharp_api_files "$FSHARP_REPO_DIR/fsharp-nugets/microsoft.visualfsharp.core.redist/1.0.0/content/.NETFramework/v4.0/4.3.0.0" "$MONO_FSHARP_LIB_DIR/api/.NETFramework/v4.0/4.3.0.0"
copy_fsharp_api_files "$FSHARP_REPO_DIR/fsharp-nugets/microsoft.visualfsharp.core.redist/1.0.0/content/.NETFramework/v4.0/4.3.1.0" "$MONO_FSHARP_LIB_DIR/api/.NETFramework/v4.0/4.3.1.0"
copy_fsharp_api_files "$FSHARP_REPO_DIR/fsharp-nugets/microsoft.visualfsharp.core.redist/1.0.0/content/.NETFramework/v4.0/4.4.0.0" "$MONO_FSHARP_LIB_DIR/api/.NETFramework/v4.0/4.4.0.0"
copy_fsharp_api_files "$FSHARP_REPO_DIR/fsharp-nugets/microsoft.portable.fsharp.core/10.1.0/lib/versions/4.4.1.0" "$MONO_FSHARP_LIB_DIR/api/.NETFramework/v4.0/4.4.1.0"
copy_fsharp_api_files "$FSHARP_REPO_DIR/fsharp-nugets/fsharp.core/4.3.4/lib/net45" "$MONO_FSHARP_LIB_DIR/api/.NETFramework/v4.0/4.4.3.0"
copy_fsharp_api_files "$FSHARP_REPO_DIR/fsharp-nugets/fsharp.core/4.3.4/lib/net45" "$MONO_FSHARP_LIB_DIR/api/.NETFramework/v4.0/4.4.5.0"
copy_fsharp_api_files "$FSHARP_REPO_DIR/fsharp-nugets/microsoft.visualfsharp.core.redist/1.0.0/content/.NETCore/3.3.1.0" "$MONO_FSHARP_LIB_DIR/api/.NETCore/3.3.1.0"
copy_fsharp_api_files "$FSHARP_REPO_DIR/fsharp-nugets/microsoft.visualfsharp.core.redist/1.0.0/content/.NETCore/3.7.4.0" "$MONO_FSHARP_LIB_DIR/api/.NETCore/3.7.4.0"
copy_fsharp_api_files "$FSHARP_REPO_DIR/fsharp-nugets/microsoft.visualfsharp.core.redist/1.0.0/content/.NETCore/3.78.3.1" "$MONO_FSHARP_LIB_DIR/api/.NETCore/3.78.3.1"
copy_fsharp_api_files "$FSHARP_REPO_DIR/fsharp-nugets/microsoft.visualfsharp.core.redist/1.0.0/content/.NETCore/3.78.4.0" "$MONO_FSHARP_LIB_DIR/api/.NETCore/3.78.4.0"
copy_fsharp_api_files "$FSHARP_REPO_DIR/fsharp-nugets/microsoft.visualfsharp.core.redist/1.0.0/content/.NETCore/3.259.4.0" "$MONO_FSHARP_LIB_DIR/api/.NETCore/3.259.4.0"
copy_fsharp_api_files "$FSHARP_REPO_DIR/fsharp-nugets/microsoft.portable.fsharp.core/10.1.0/lib/profiles/portable-net45+netcore45" "$MONO_FSHARP_LIB_DIR/api/.NETCore/3.7.41.0"
copy_fsharp_api_files "$FSHARP_REPO_DIR/fsharp-nugets/microsoft.portable.fsharp.core/10.1.0/lib/profiles/portable-net45+netcore45+wp8" "$MONO_FSHARP_LIB_DIR/api/.NETCore/3.78.41.0"
copy_fsharp_api_files "$FSHARP_REPO_DIR/fsharp-nugets/microsoft.portable.fsharp.core/10.1.0/lib/profiles/portable-net45+netcore45+wpa81+wp8" "$MONO_FSHARP_LIB_DIR/api/.NETCore/3.259.41.0"
copy_fsharp_api_files "$FSHARP_REPO_DIR/fsharp-nugets/microsoft.visualfsharp.core.redist/1.0.0/content/.NETPortable/2.3.5.0" "$MONO_FSHARP_LIB_DIR/api/.NETPortable/2.3.5.0"
copy_fsharp_api_files "$FSHARP_REPO_DIR/fsharp-nugets/microsoft.visualfsharp.core.redist/1.0.0/content/.NETPortable/2.3.5.1" "$MONO_FSHARP_LIB_DIR/api/.NETPortable/2.3.5.1"
copy_fsharp_api_files "$FSHARP_REPO_DIR/fsharp-nugets/microsoft.visualfsharp.core.redist/1.0.0/content/.NETPortable/3.47.4.0" "$MONO_FSHARP_LIB_DIR/api/.NETPortable/3.47.4.0"
copy_fsharp_api_files "$FSHARP_REPO_DIR/fsharp-nugets/microsoft.portable.fsharp.core/10.1.0/lib/profiles/portable-net45+sl5+netcore45" "$MONO_FSHARP_LIB_DIR/api/.NETPortable/3.47.41.0"

function copy_fsharp_netsdk_files {
  mkdir -p "$2"
  cp "$1/Microsoft.FSharp.NetSdk.props" "$2/Microsoft.FSharp.NetSdk.props"
  cp "$1/Microsoft.FSharp.NetSdk.targets" "$2/Microsoft.FSharp.NetSdk.targets"
  cp "$1/Microsoft.FSharp.Overrides.NetSdk.targets" "$2/Microsoft.FSharp.Overrides.NetSdk.targets"
}

copy_fsharp_netsdk_files "$ARTIFACTS_DIR/bin/fsc/Release/net472" "$MONO_LIB_DIR/xbuild/Microsoft/VisualStudio/v/FSharp"
copy_fsharp_netsdk_files "$ARTIFACTS_DIR/bin/fsc/Release/net472" "$MONO_LIB_DIR/xbuild/Microsoft/VisualStudio/v11.0/FSharp"
copy_fsharp_netsdk_files "$ARTIFACTS_DIR/bin/fsc/Release/net472" "$MONO_LIB_DIR/xbuild/Microsoft/VisualStudio/v12.0/FSharp"
copy_fsharp_netsdk_files "$ARTIFACTS_DIR/bin/fsc/Release/net472" "$MONO_LIB_DIR/xbuild/Microsoft/VisualStudio/v14.0/FSharp"
copy_fsharp_netsdk_files "$ARTIFACTS_DIR/bin/fsc/Release/net472" "$MONO_LIB_DIR/xbuild/Microsoft/VisualStudio/v15.0/FSharp"
copy_fsharp_netsdk_files "$ARTIFACTS_DIR/bin/fsc/Release/net472" "$MONO_LIB_DIR/xbuild/Microsoft/VisualStudio/v16.0/FSharp"

function write_fsharp_targets_files {
  mkdir -p "$1"
  cat >"$1/Microsoft.FSharp.Targets" <<EOL
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
    <Import Project="$MONO_FSHARP_LIB_DIR/Microsoft.FSharp.Targets" />
</Project>
EOL
  cat >"$1/Microsoft.Portable.FSharp.Targets" <<EOL
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
    <Import Project="$MONO_FSHARP_LIB_DIR/Microsoft.Portable.FSharp.Targets" />
</Project>
EOL
}

write_fsharp_targets_files "$MONO_LIB_DIR/Microsoft F#/v4.0"
write_fsharp_targets_files "$MONO_LIB_DIR/Microsoft SDKs/F#/3.0/Framework/v4.0"
write_fsharp_targets_files "$MONO_LIB_DIR/Microsoft SDKs/F#/3.1/Framework/v4.0"
write_fsharp_targets_files "$MONO_LIB_DIR/Microsoft SDKs/F#/4.0/Framework/v4.0"
write_fsharp_targets_files "$MONO_LIB_DIR/Microsoft SDKs/F#/4.1/Framework/v4.0"
write_fsharp_targets_files "$MONO_LIB_DIR/xbuild/Microsoft/VisualStudio/v/FSharp"
write_fsharp_targets_files "$MONO_LIB_DIR/xbuild/Microsoft/VisualStudio/v11.0/FSharp"
write_fsharp_targets_files "$MONO_LIB_DIR/xbuild/Microsoft/VisualStudio/v12.0/FSharp"
write_fsharp_targets_files "$MONO_LIB_DIR/xbuild/Microsoft/VisualStudio/v14.0/FSharp"
write_fsharp_targets_files "$MONO_LIB_DIR/xbuild/Microsoft/VisualStudio/v15.0/FSharp"
write_fsharp_targets_files "$MONO_LIB_DIR/xbuild/Microsoft/VisualStudio/v16.0/FSharp"

function generate_script {
  mkdir -p "$MONO_BIN_DIR"
  cat >"$MONO_BIN_DIR/$1" <<EOL
#!/bin/sh
EXEC="exec "

if test x"\$1" = x--debug; then
   DEBUG=--debug
   shift
fi

if test x"\$1" = x--gdb; then
   shift
   EXEC="gdb --eval-command=run --args "
fi

if test x"\$1" = x--valgrind; then
  shift
  EXEC="valgrind \$VALGRIND_OPTIONS"
fi

\$EXEC "$MONO_BIN_DIR/mono" \$DEBUG \$MONO_OPTIONS "$MONO_FSHARP_LIB_DIR/$2" --exename:\$(basename "\$0") "\$@"
EOL
chmod +x "$MONO_BIN_DIR/$1"
}

generate_script "fsharpc" "fsc.exe"
generate_script "fsharpi" "fsi.exe"
generate_script "fsharpiAnyCpu" "fsiAnyCpu.exe"
