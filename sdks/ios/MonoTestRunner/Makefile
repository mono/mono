TOP=../../

MT_BCL=$(TOP)/out/bcl/monotouch

logcat:
	tail -f  ~/Library/Logs/CoreSimulator/B0326406-244C-4E12-9B3A-2752B336053D/system.log 


prep-managed:
	mkdir -p managed
	cp $(MT_BCL)/mscorlib.dll managed/
	cp $(MT_BCL)/System.dll managed/
	cp $(MT_BCL)/System.Xml.dll managed/
	cp $(MT_BCL)/Mono.Security.dll managed/
	cp $(MT_BCL)/System.Core.dll managed/
	cp $(MT_BCL)/nunitlite.dll managed/
	cp mini_tests.dll managed/
	cp monotouch_corlib_test.dll managed/
	mcs /nostdlib main.cs -r:$(MT_BCL)/mscorlib.dll -r:$(MT_BCL)/nunitlite.dll -r:$(MT_BCL)/System.Core.dll -out:managed/main.exe

build-sim32: prep-managed
	xcodebuild -configuration Debug -arch i386 -sdk iphonesimulator

build-sim64: prep-managed
	xcodebuild -configuration Debug -arch x86_64 -sdk iphonesimulator

install:
	xcrun simctl install booted  build/Debug-iphonesimulator/MonoTestRunner.app

launch:
	xcrun simctl  launch booted runtime.MonoTestRunner

boot:
	nohup /Applications/Xcode.app/Contents/Developer/Applications/Simulator.app/Contents/MacOS/Simulator -CurrentDeviceUDID B0326406-244C-4E12-9B3A-2752B336053D &

shutdown:
	xcrun simctl shutdown all

kill:
	xcrun simctl  terminate booted runtime.MonoTestRunner

