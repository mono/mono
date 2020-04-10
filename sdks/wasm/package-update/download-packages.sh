#!/bin/bash
while (("$#"));
do 
    key="${1}"
    
    case ${key} in
    -r|--runtime)
        RUNTIME="$2"
        shift 2
        ;;
    -f|--filepath)
        FILEPATH="$2"
        shift 2
        ;;
    -u|--url)
        URL="$2"
        shift 2
        ;;
    *)
        shift
        ;;
    esac
done

PACKAGE_PATH=$HOME/.nuget/packages/microsoft.aspnetcore.components.webassembly.runtime/$RUNTIME/tools/dotnetwasm

if [ -z "$FILEPATH" ]
then
    wget -O wasm-package.zip $URL
    unzip wasm-package.zip -d wasm-package/
    FILEPATH=wasm-package/
fi

rm -r $PACKAGE_PATH/bcl
cp $FILEPATH/wasm-bcl $PACKAGE_PATH/bcl

rm -r $PACKAGE_PATH/framework
cp $FILEPATH/framework $PACKAGE_PATH/framework

rm "$PACKAGE_PATH"/wasm/*
cp $FILEPATH/builds/release/dotnet.$RUNTIME.js $PACKAGE_PATH/wasm/
cp $FILEPATH/builds/release/dotnet.wasm $PACKAGE_PATH/wasm/

rm wasm-package.zip
rm -rf wasm-package