git submodule update --init --recursive
cp .yamato/config/Stevedore.conf ~/Stevedore.conf
cd external/buildscripts
./bee
cd ../..

perl external/buildscripts/build_classlibs_osx.pl --stevedorebuilddeps=1
if [ $? -eq 0 ]
then
  echo "mono build script ran successfully"
else
  echo "mono build script failed" >&2
  exit 1
fi

mkdir -p incomingbuilds/classlibs
cp -r ZippedClasslibs.tar.gz incomingbuilds/classlibs/
cd incomingbuilds/classlibs
tar -pzxf ZippedClasslibs.tar.gz
rm -f ZippedClasslibs.tar.gz
cd ../..