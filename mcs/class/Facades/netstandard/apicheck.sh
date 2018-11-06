#!/bin/sh

# Cloned and Build location of https://github.com/dotnet/standard
STANDARD=/Users/cloned-location-of/dotnet/standard

APICOMPAT=$STANDARD/Tools/ApiCompat.exe
NSAPI=$STANDARD/bin/ref/netstandard/2.0.0.0/netstandard.dll

dotnet $APICOMPAT $NSAPI -implDirs:../../lib/net_4_x/Facades/,../../lib/net_4_x/