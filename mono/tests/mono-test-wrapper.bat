set MONO_PATH=%~dp0\..\..\monodistribution\lib\mono\2.0
set MONO_SHARED_DIR=%~dp0\..\..\runtime
set MONO_CFG_DIR=%~dp0\..\..\runtime\etc
set PATH=%~dp0\..\..\builds\embedruntimes\win32;%~dp0\..\..\msvc\Win32_Release_eglib\bin;%PATH%
%*