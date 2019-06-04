#!/bin/sh

# https://github.com/dotnet/standard
STANDARD_REPO=/path/to/standard

# https://github.com/dotnet/arcade
ARCARDE_REPO=/path/to/arcade

APICOMPAT=$ARCARDE_REPO/artifacts/bin/Microsoft.DotNet.ApiCompat/Debug/net472/Microsoft.DotNet.ApiCompat.exe
NSAPI=$STANDARD_REPO/artifacts/bin/ref/netstandard/Debug/netstandard.dll

# uncomment to run only for net_4_x:
# mono $APICOMPAT $NSAPI --impl-dirs "../../lib/net_4_x/Facades/,../../lib/net_4_x/" --exclude-non-browsable > ns21_diff_net_4_x.txt

# run ApiCompat for all profiles (runtime should be built with `--runtime-preset=all` flag)
for profile in net_4_x monodroid monotouch monotouch_watch testing_aot_full_interp testing_aot_full testing_aot_hybrid unreal wasm winaot xammac xammac_net_4_5; do
    profile_path=../../lib/$profile
    if [ -d "$profile_path" ]; then
        echo "checking $(basename $profile_path) ..."
        mono $APICOMPAT $NSAPI --impl-dirs "$profile_path/Facades/,$profile_path/" --exclude-non-browsable > ns21_diff_$(basename $profile_path).txt
    fi
done