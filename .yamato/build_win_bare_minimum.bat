@echo off
git submodule update --init --recursive
perl external/buildscripts/build_unityscript_bareminimum_win.pl
mkdir incomingbuilds\bareminimum
xcopy /s /e /h /y builds\* incomingbuilds\bareminimum