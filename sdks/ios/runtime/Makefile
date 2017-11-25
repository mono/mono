all: test-runner

CC = /Applications/Xcode.app/Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/bin/clang
ARCH = x86_64
SDK_DIR = ../../out/ios-sim64
CFLAGS = \
	-std=gnu11 \
	-fobjc-arc \
	-isysroot /Applications/Xcode.app/Contents/Developer/Platforms/iPhoneSimulator.platform/Developer/SDKs/iPhoneSimulator11.1.sdk \
	-mios-simulator-version-min=11.1 \
	-g \
	-I$(SDK_DIR)/include/mono-2.0

LDFLAGS = \
	-isysroot /Applications/Xcode.app/Contents/Developer/Platforms/iPhoneSimulator.platform/Developer/SDKs/iPhoneSimulator11.1.sdk \
	-mios-simulator-version-min=11.1 \
	 -framework Foundation \
	 -framework UIKit \
	$(SDK_DIR)/lib/libmonosgen-2.0.a \
	-liconv

%.o: %.m
	$(ENV) $(CC) -arch $(ARCH) $(CFLAGS) -c -o $@ $^

test-runner: main.o runtime.o AppDelegate.o ViewController.o
	$(ENV) $(CC) -arch $(ARCH) $(LDFLAGS) -o $@ $^

clean:
	$(RM) -rf test-runner *.o
