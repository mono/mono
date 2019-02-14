sudo apt-get install -y schroot
sudo apt-get install -y binutils debootstrap
sudo apt-get install -y m4
sudo apt-get install -y libc6-i386
sudo apt-get install -y libc6-dev-i386
sudo apt-get install -y libncurses5-i386
git submodule update --init --recursive
# try again in case previous update failed
git submodule update --init --recursive
cd external/buildscripts
./bee
cd ../..
perl external/buildscripts/build_runtime_linux.pl -build64=0 --stevedorebuilddeps=1
echo "Making directory incomingbuilds/linux32"
mkdir -p incomingbuilds/linux32
ls -al incomingbuilds/linux32
echo "Copying builds to incomingbuilds"
cp -r -v builds/* incomingbuilds/linux32/
ls -al incomingbuilds/linux32