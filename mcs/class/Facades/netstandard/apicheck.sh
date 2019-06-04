#!/bin/sh

# Cloned and Build location of https://github.com/dotnet/standard
STANDARD=/Users/cloned-location-of/dotnet/standard

# Microsoft.DotNet.ApiCompat.exe can be built in https://github.com/dotnet/arcade
APICOMPAT=Microsoft.DotNet.ApiCompat.exe
NSAPI=$(STANDARD)/artifacts/bin/ref/netstandard/Debug/netstandard.dll

dotnet $APICOMPAT $NSAPI --impl-dirs "../../lib/net_4_x/Facades/,../../lib/net_4_x/" --exclude-non-browsable
# --exclude-non-browsable ignores `EditorBrowsableAttribute` issues