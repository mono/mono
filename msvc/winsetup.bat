@echo off
cd ..
if exist config.h if not exist cygconfig.h copy config.h cygconfig.h
if exist eglib\config.h if not exist eglib\cygconfig.h copy eglib\config.h eglib\cygconfig.h
copy winconfig.h config.h
copy eglib\winconfig.h eglib\config.h
goto end
:error
echo fatal error: the VSDepenancies directory was not found in the "mono" directory
echo error: you must download and unzip that file
exit /b 100
goto end
:ok
echo OK
:end
exit /b 0
