@echo off
cd mcs\jay
vcbuild jay.vcxproj
msbuild net_4_x.sln
