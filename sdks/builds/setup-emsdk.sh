
OUTDIR=$1

echo Setting up emcc in $OUTDIR ...

git clone https://github.com/juj/emsdk.git $OUTDIR || exit 1

#
# WE lock the emscripten version to 1.37.19 cuz later versions suffer from a bug that makes them not work on Sierra
#
pushd $OUTDIR
./emsdk install emscripten-1.37.19 sdk-1.37.19-64bit || exit 1
./emsdk activate --embedded emscripten-1.37.19 sdk-1.37.19-64bit || exit 1
popd
