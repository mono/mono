all: test-runner

UNREFERENCED_SYMBOLS =                                                  \
        _xamarin_log                                                            \
        _xamarin_timezone_get_data                                      \
        _xamarin_timezone_get_names                                     \
        _CloseZStream                                                           \
        _CreateZStream                                                          \
        _Flush                                                                          \
        _ReadZStream                                                            \
        _WriteZStream

CC = /Applications/Xcode.app/Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/bin/clang
ARCH = x86_64
SDK_DIR = ../../out/ios-sim64
CFLAGS = \
	-std=gnu11 \
	-fobjc-arc \
	-isysroot /Applications/Xcode.app/Contents/Developer/Platforms/iPhoneSimulator.platform/Developer/SDKs/iPhoneSimulator.sdk \
	-mios-simulator-version-min=10.1 \
	-g \
	-I$(SDK_DIR)/include/mono-2.0

LDFLAGS = \
	-isysroot /Applications/Xcode.app/Contents/Developer/Platforms/iPhoneSimulator.platform/Developer/SDKs/iPhoneSimulator.sdk \
	-mios-simulator-version-min=10.1 \
	 -framework Foundation \
	 -framework UIKit \
	$(foreach u,$(UNREFERENCED_SYMBOLS),-u $u)  \
	$(SDK_DIR)/lib/libmonosgen-2.0.a \
	$(SDK_DIR)/lib/libMonoPosixHelper.a \
	-liconv -lz

%.o: %.m
	$(ENV) $(CC) -arch $(ARCH) $(CFLAGS) -c -o $@ $^

test-runner: main.o runtime.o AppDelegate.o ViewController.o
	$(ENV) $(CC) -arch $(ARCH) $(LDFLAGS) -o $@ $^

clean:
	$(RM) -rf test-runner *.o
