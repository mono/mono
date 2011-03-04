rm -r builds

perl ../native_client_sdk/prepare.pl
export NACL_NATIVE_CLIENT="$PWD/../native_client_sdk/builds/"

./autogen.sh
make distclean

cd nacl
./nacl-runtime-mono.sh
export TARGET_BIT_SIZE=64
./nacl-runtime-mono.sh
cd ..

mkdir -p builds/embedruntimes/nacl
cp nacl/runtime/lib/libmono-2.0.a builds/embedruntimes/nacl/libmono.a

mkdir -p builds/embedruntimes/nacl64
cp nacl/runtime64/lib/libmono-2.0.a builds/embedruntimes/nacl64/libmono.a

mkdir -p builds/monodistribution/lib/mono/2.0
cp nacl/runtime/lib/mono/2.0/mscorlib.dll* builds/monodistribution/lib/mono/2.0
cp nacl/runtime/lib/mono/2.0/System.dll* builds/monodistribution/lib/mono/2.0
cp nacl/runtime/lib/mono/2.0/System.Core.dll* builds/monodistribution/lib/mono/2.0
cp nacl/runtime/lib/mono/2.0/Mono.Security.dll* builds/monodistribution/lib/mono/2.0

cd builds
zip builds.zip -r *