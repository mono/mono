git submodule update --init --recursive
cp .yamato/config/Stevedore.conf ~/Stevedore.conf
cd external/buildscripts
./bee
cd ../..
perl external/buildscripts/build_runtime_osx.pl --stevedorebuilddeps=1
mkdir -p incomingbuilds/osx-i386
cp -r builds/* incomingbuilds/osx-i386/