@echo off
rem This script will convert Unix-style line endings into Windows format.

for %%P in (*.ds?) do call :fixline %%P
goto :eof

:fixline
@echo on
if exist "%~1.new" del "%~1.new"
for /f %%S in (%1) do (
    echo %%S>>"%~1.new"
)
ren %1 "%~1.bak"
ren "%~1.new" %1
goto :eof
