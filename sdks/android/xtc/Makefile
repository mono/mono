build:
	nuget restore
	xbuild RuntimeTests.csproj

xtc-submit: build
	mono packages/Xamarin.UITest.2.0.7/tools/test-cloud.exe submit ../bin/AndroidRunner-debug.apk $(API_KEY) --devices 8e3ae326 --series "master" --locale "en_US" --app-name "Mono Runtime Test runner" --user kumpera@xamarin.com --assembly-dir bin/Debug
	