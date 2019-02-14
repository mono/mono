git submodule update --init --recursive
cd external/buildscripts
./bee
cd ../..
perl external/buildscripts/build_classlibs_osx.pl --stevedorebuilddeps=1
mkdir -p incomingbuilds/classlibs
cp -r ZippedClasslibs.tar.gz incomingbuilds/classlibs/
cd incomingbuilds/classlibs
tar -pzxf ZippedClasslibs.tar.gz
rm -f ZippedClasslibs.tar.gz
cd ../..