@"%~dp0cli.bat" %MONO_OPTIONS% --debug "%~dp0..\lib\mono\4.0\nunit-console.exe" %*
exit /b %ERRORLEVEL%
