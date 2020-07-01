git submodule update --init --recursive
cp .yamato/config/Stevedore.conf ~/Stevedore.conf
cd external/buildscripts
./bee
cd ../..

perl external/buildscripts/build_runtime_osx.pl --stevedorebuilddeps=1
if [ $? -eq 0 ]
then
  echo "mono build script ran successfully"
else
  echo "mono build script failed" >&2
  exit 1
fi

mkdir -p incomingbuilds/osx-i386
cp -r builds/* incomingbuilds/osx-i386/