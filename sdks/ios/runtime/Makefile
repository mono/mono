all: runtime libmonoios.a

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
SIM_SYSROOT = /Applications/Xcode.app/Contents/Developer/Platforms/iPhoneSimulator.platform/Developer/SDKs/iPhoneSimulator.sdk
SIM_ARCH = x86_64
SIM_SDK_DIR = ../../out/ios-sim64
SIM_CFLAGS = \
	-std=gnu11 \
	-fobjc-arc \
	-isysroot $(SIM_SYSROOT) \
	-mios-simulator-version-min=10.1 \
	-g \
	-I$(SIM_SDK_DIR)/include/mono-2.0

SIM_LDFLAGS = \
	-isysroot $(SIM_SYSROOT) \
	-mios-simulator-version-min=10.1 \
	 -framework Foundation \
	 -framework UIKit \
	$(foreach u,$(UNREFERENCED_SYMBOLS),-u $u)  \
	$(SIM_SDK_DIR)/lib/libmonosgen-2.0.a \
	$(SIM_SDK_DIR)/lib/libMonoPosixHelper.a \
	-liconv -lz

sim-%.o: %.m
	$(ENV) $(CC) -arch $(SIM_ARCH) $(SIM_CFLAGS) -c -o $@ $^

runtime: sim-main.o sim-runtime.o sim-AppDelegate.o sim-ViewController.o
	$(ENV) $(CC) -arch $(SIM_ARCH) $(SIM_LDFLAGS) -o $@ $^

DEV_ARCH = arm64
DEV_SDK_DIR = ../../out/ios-target64
DEV_SYSROOT = /Applications/Xcode.app/Contents/Developer/Platforms/iPhoneOS.platform/Developer/SDKs/iPhoneOS.sdk/
DEV_CFLAGS = \
	-isysroot $(DEV_SYSROOT) \
	-std=gnu11 \
	-fobjc-arc \
	-mios-simulator-version-min=10.1 \
	-g \
	-fPIC \
	-DDEVICE=1 \
	-I$(DEV_SDK_DIR)/include/mono-2.0

DEV_LDFLAGS = \
	-isysroot $(DEV_SYSROOT) \
	-mios-simulator-version-min=10.1 \
	 -framework Foundation \
	 -framework UIKit \
	-Wl,-pie \
	$(foreach u,$(UNREFERENCED_SYMBOLS),-u $u)  \
	$(DEV_SDK_DIR)/lib/libmonosgen-2.0.a \
	$(DEV_SDK_DIR)/lib/libMonoPosixHelper.a \
	-liconv -lz

dev-%.o: %.m
	$(ENV) $(CC) -arch $(DEV_ARCH) $(DEV_CFLAGS) -c -o $@ $^

libmonoios.a: dev-main.o dev-runtime.o dev-AppDelegate.o dev-ViewController.o
	libtool -static -o $@ $^

clean:
	$(RM) -rf test-runner *.o
