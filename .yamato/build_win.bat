@echo off
git submodule update --init --recursive
perl external/buildscripts/build_runtime_win64.pl --stevedorebuilddeps=1
mkdir -p incomingbuilds\win64
xcopy /s /e /h /y builds\* incomingbuilds\win64
