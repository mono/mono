@echo off
SETLOCAL
set MONO_PREFIX=%~dp0..
set MONO=%MONO_PREFIX%\bin\mono
"%MONO%" %*
exit /b %ERRORLEVEL%
ENDLOCAL
