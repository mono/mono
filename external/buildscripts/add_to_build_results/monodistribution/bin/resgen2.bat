@"%~dp0cli.bat" %MONO_OPTIONS% "%~dp0..\lib\mono\4.5\resgen.exe" %*
exit /b %ERRORLEVEL%
