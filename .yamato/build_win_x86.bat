@echo off
git submodule update --init --recursive
perl external/buildscripts/build_runtime_win.pl --stevedorebuilddeps=1
mkdir -p incomingbuilds\win32
xcopy /s /e /h /y builds\* incomingbuilds\win32