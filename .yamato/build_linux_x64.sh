sudo apt-get install -y schroot
sudo apt-get install -y binutils debootstrap
git submodule update --init --recursive
# try again in case previous update failed
git submodule update --init --recursive
cd external/buildscripts
./bee
cd ../..
perl external/buildscripts/build_runtime_linux.pl -build64=1 --stevedorebuilddeps=1
echo "Making directory incomingbuilds/linux64"
mkdir -p incomingbuilds/linux64
ls -al incomingbuilds/linux64
echo "Copying builds to incomingbuilds"
cp -r builds/* incomingbuilds/linux64/
ls -al incomingbuilds/linux64