#!/usr/bin/env bash
set -e 
set -u

ASP_REMOTE_NAME="upstream"
MONO_REMOTE_NAME="upstream"
ASP_BRANCH_NAME="blazor-wasm"
MONO_BRANCH_NAME="master"
FILEPATH=""
URL=""

while (("$#"));
do 
    key="${1}"
    
    case ${key} in
    -r|--runtime)
        RUNTIME_VER="$2"
        shift 2
        ;;
    -u|--url)
        URL="$2"
        shift 2
        ;;
    -a|--aspnetcore_working_dir)
        ASPNETCORE="$2"
        shift 2
        ;;
    -m|--mono_working_dir)
        MONO="$2"
        shift 2
        ;;
    --asp_remote_name)
        ASP_REMOTE_NAME="$2"
        shift 2
        ;;
     --mono_remote_name)
        MONO_REMOTE_NAME="$2"
        shift 2
        ;;
    *)
        shift
        ;;
    esac
done

if [ -z "$RUNTIME_VER" ]; then
	echo "Error: runtime version required. Use -r"
	exit 1
fi

if [ -z "$ASPNETCORE" ]; then
    echo "Error: aspnetcore working directory required. Use -a"
    exit 1
fi

if [ -z "$MONO" ]; then
    echo "Error: mono working directory required. Use -m"
    exit 1
fi

NUGET_HOME=${NUGET_HOME:-"$HOME/.nuget"}
PACKAGE_PATH="$NUGET_HOME/packages/microsoft.aspnetcore.components.webassembly.runtime/$RUNTIME_VER/tools/dotnetwasm"
PROXY_PACKAGE_PATH="$NUGET_HOME/packages/microsoft.aspnetcore.components.webassembly.devserver/$RUNTIME_VER/tools/BlazorDebugProxy"
ASP_PROXY_PATH="$ASPNETCORE/src/Components/WebAssembly/DebugProxy/src"
MONO_PROXY_PATH="$MONO/sdks/wasm/BrowserDebugProxy"
TMP_DIR=`mktemp -d`
TMP_PKG_DIR=$TMP_DIR/wasm-package
mkdir $TMP_PKG_DIR

if [ ! -d "$NUGET_HOME" ]; then
	echo "NUGET_HOME envar = $NUGET_HOME does not exist."
	exit 1
fi

if [ ! -z "$URL" ]; then
    wget -O "$TMP_DIR/wasm-package.zip" $URL
    unzip "$TMP_DIR/wasm-package.zip" -d $TMP_PKG_DIR
else
    cp -r "$MONO/sdks/out/wasm-bcl" $TMP_PKG_DIR
    cp -r "$MONO/sdks/wasm/framework" $TMP_PKG_DIR
    cp -r "$MONO/sdks/wasm/builds" $TMP_PKG_DIR
fi

echo "pkg: $TMP_PKG_DIR"

rm -r "$PACKAGE_PATH/bcl"
cp -r "$TMP_PKG_DIR/wasm-bcl/wasm/" "$PACKAGE_PATH/bcl"

rm -r "$PACKAGE_PATH/framework"
cp -r "$TMP_PKG_DIR/framework" "$PACKAGE_PATH/framework"

rm "$PACKAGE_PATH"/wasm/*
cp "$TMP_PKG_DIR/builds/release/dotnet.js" "$PACKAGE_PATH/wasm/dotnet.${RUNTIME_VER}.js"
cp "$TMP_PKG_DIR/builds/release/dotnet.wasm" "$PACKAGE_PATH/wasm/"

echo "Replacing DebuggerProxy..."

cd $MONO
git checkout $MONO_BRANCH_NAME
# git reset --hard
# git fetch $MONO_REMOTE_NAME
# git pull $MONO_REMOTE_NAME $MONO_BRANCH_NAME 

cd $ASPNETCORE 
git checkout $ASP_BRANCH_NAME
git fetch $ASP_REMOTE_NAME
git reset --hard $ASP_REMOTE_NAME/$ASP_BRANCH_NAME

./clean.sh || true

cp $MONO_PROXY_PATH/*.cs "$ASP_PROXY_PATH/MonoDebugProxy/ws-proxy/"
rm "$ASP_PROXY_PATH/MonoDebugProxy/ws-proxy/AssemblyInfo.cs"
./build.sh || true

cp "$ASP_PROXY_PATH/bin/Debug/netcoreapp3.1/Microsoft.AspNetCore.Components.WebAssembly.DebugProxy.dll" $PROXY_PACKAGE_PATH

echo "Replacement finished"
