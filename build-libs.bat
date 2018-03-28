@echo off
cd mcs\jay
vcbuild jay.vcxproj
cd msvc\scripts
csc prepare.cs
prepare.exe ..\..\mcs core
msbuild net_4_x.sln
