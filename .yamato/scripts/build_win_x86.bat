@echo off
git submodule update --init --recursive

perl external/buildscripts/build_runtime_win.pl --stevedorebuilddeps=1
if NOT %errorlevel% == 0 (
 echo "mono build script failed"
 EXIT /B %errorlevel%
)
echo "mono build script ran successfully"

md incomingbuilds\win32
xcopy /s /e /h /y builds\* incomingbuilds\win32