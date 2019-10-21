@echo off
git submodule update --init --recursive

perl external/buildscripts/build_unityscript_bareminimum_win.pl
if NOT %errorlevel% == 0 (
 echo "mono build script failed"
 EXIT /B %errorlevel%
)
echo "mono build script ran successfully"

md incomingbuilds\bareminimum
xcopy /s /e /h /y builds\* incomingbuilds\bareminimum