sudo apt-get install -qy zip unzip
sudo apt-get install -qy p7zip-full p7zip-rar
perl external/buildscripts/collect_allbuilds.pl
pwd
ls -al