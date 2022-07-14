#!/bin/sh

#	show ubuntu version
#lsb_release -a

#No LSB modules are available.
#Distributor ID: Ubuntu
#Description:    Ubuntu 18.04.5 LTS
#Release:        18.04
#Codename:       bionic

#	git clone mono
#git clone https://github.com/mono/mono

#	Go to mono-dir
#cd mono

#	run ./autogen.sh
./autogen.sh

#	download mono-monolite-latest to build from "mcs"-folder
make get-monolite-latest

#	make install /mono/ with mono-monolite-latest"
make install

#	"external/bdwgc" was been compiled automatically, after run ./autogen.sh, but need to recompile this again, for mono/mono
echo "/mono/external/bdwgc/: make "
cd external/bdwgc
make

#	make "mono/mono" and "mono/mono/mini"
cd mono
make

#	make "mono/runtime"
cd ../../runtime
make

echo ""
echo ""
echo "check out binary files there: \mono\mcs\class\lib\net_4_x-linux\"