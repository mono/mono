git submodule update --init --recursive
cd external/buildscripts
./bee
cd ../..
perl external/buildscripts/build_runtime_android.pl --stevedorebuilddeps=1
mkdir -p incomingbuilds/android
cp -r builds/* incomingbuilds/android/