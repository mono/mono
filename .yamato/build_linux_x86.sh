sudo dpkg --add-architecture i386
sudo DEBIAN_FRONTEND=noninteractive apt-get -qy update
sudo apt-get install -y schroot
sudo apt-get install -y binutils debootstrap
sudo apt-get install -y m4
sudo apt-get install -y libc6-i386
sudo apt-get install -y libc6-dev-i386
sudo apt-get install -y zlib1g:i386
sudo apt-get install -y zlib1g-dev:i386
sudo apt-get install -y libstdc++6:i386
sudo apt-get install -y libncurses5:i386
sudo apt-get install -y libncurses5-dev:i386
git submodule update --init --recursive
export UNITY_THISISABUILDMACHINE=1
# try again in case previous update failed
git submodule update --init --recursive
cd external/buildscripts
./bee
cd ../..

perl external/buildscripts/build_runtime_linux.pl -build64=0 --stevedorebuilddeps=1
if [ $? -eq 0 ]
then
  echo "mono build script ran successfully"
else
  echo "mono build script failed" >&2
  exit 1
fi

echo "Making directory incomingbuilds/linux32"
mkdir -p incomingbuilds/linux32
ls -al incomingbuilds/linux32
echo "Copying builds to incomingbuilds"
cp -r -v builds/* incomingbuilds/linux32/
ls -al incomingbuilds/linux32