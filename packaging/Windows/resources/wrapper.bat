@echo off
"%~dp0\mono.exe" %MONO_OPTIONS% "%~dp0\..\lib\mono\@@RUNTIME@@\@@COMMAND@@" %*
