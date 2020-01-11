TOP=$(realpath $(CURDIR)/../../..)
-include $(TOP)/sdks/Make.config

ifndef DISABLE_CLASSIC
ALL_TARGETS += app-simulator app-device.a
endif

ifdef ENABLE_NETCORE
ALL_TARGETS += app-netcore-simulator app-netcore-device.a
endif

all: $(ALL_TARGETS)

CONFIGURATION?=release

XCODE_ROOT=$(shell xcode-select -p)

UNREFERENCED_SYMBOLS_COMMON = \
        _xamarin_log \
        _xamarin_timezone_get_data \
        _xamarin_timezone_get_names

UNREFERENCED_SYMBOLS_CLASSIC = \
        _CloseZStream \
        _CreateZStream \
        _Flush \
        _ReadZStream \
        _WriteZStream

CC = $(XCODE_ROOT)/Toolchains/XcodeDefault.xctoolchain/usr/bin/clang
SIM_SYSROOT = $(XCODE_ROOT)/Platforms/iPhoneSimulator.platform/Developer/SDKs/iPhoneSimulator.sdk
SIM_ARCH = x86_64
SIM_CLASSIC_SDK_DIR = ../../out/ios-sim64-$(CONFIGURATION)
SIM_NETCORE_SDK_DIR = ../../out/ios-netcore_sim64-$(CONFIGURATION)
SIM_CFLAGS = \
	-std=gnu11 \
	-fobjc-arc \
	-isysroot $(SIM_SYSROOT) \
	-mios-simulator-version-min=10.1 \
	-g

SIM_LDFLAGS = \
	-isysroot $(SIM_SYSROOT) \
	-mios-simulator-version-min=10.1 \
	 -framework Foundation \
	 -framework UIKit \
	 -framework GSS \
	-liconv -lz

ifndef DISABLE_CLASSIC
SIM_CLASSIC_LDFLAGS = \
	$(foreach u,$(UNREFERENCED_SYMBOLS_COMMON) $(UNREFERENCED_SYMBOLS_CLASSIC),-u $u)  \
	$(SIM_CLASSIC_SDK_DIR)/lib/libmonosgen-2.0.a \
	-force_load $(SIM_CLASSIC_SDK_DIR)/lib/libmono-native-unified.a \
	-force_load $(SIM_CLASSIC_SDK_DIR)/lib/libMonoPosixHelper.a

sim-%.o: %.m
	$(ENV) $(CC) -arch $(SIM_ARCH) $(SIM_CFLAGS) -I$(SIM_CLASSIC_SDK_DIR)/include/mono-2.0 -c -o $@ $^

app-simulator: sim-main.o sim-runtime.o
	$(ENV) $(CC) -arch $(SIM_ARCH) $(SIM_LDFLAGS) $(SIM_CLASSIC_LDFLAGS) -o $@ $^
endif

ifdef ENABLE_NETCORE
SIM_NETCORE_LDFLAGS = \
	$(foreach u,$(UNREFERENCED_SYMBOLS_COMMON),-u $u)  \
	$(SIM_NETCORE_SDK_DIR)/lib/libmonosgen-2.0.a \
	-force_load $(SIM_NETCORE_SDK_DIR)/lib/System.Native.a \
	-force_load $(SIM_NETCORE_SDK_DIR)/lib/System.IO.Compression.Native.a \
	-force_load $(SIM_NETCORE_SDK_DIR)/lib/System.Security.Cryptography.Native.Apple.a

sim-netcore-%.o: %.m
	$(ENV) $(CC) -arch $(SIM_ARCH) $(SIM_CFLAGS) -I$(SIM_NETCORE_SDK_DIR)/include/mono-2.0 -c -o $@ $^

app-netcore-simulator: sim-netcore-main.o sim-netcore-runtime.o
	$(ENV) $(CC) -arch $(SIM_ARCH) $(SIM_LDFLAGS) $(SIM_NETCORE_LDFLAGS) -o $@ $^
endif


DEV_ARCH = arm64
DEV_CLASSIC_SDK_DIR = ../../out/ios-target64-$(CONFIGURATION)
DEV_NETCORE_SDK_DIR = ../../out/ios-netcore_target64-$(CONFIGURATION)
DEV_SYSROOT = $(XCODE_ROOT)/Platforms/iPhoneOS.platform/Developer/SDKs/iPhoneOS.sdk/
DEV_CFLAGS = \
	-isysroot $(DEV_SYSROOT) \
	-std=gnu11 \
	-fobjc-arc \
	-mios-simulator-version-min=10.1 \
	-g \
	-fPIC \
	-DDEVICE=1

ifndef DISABLE_CLASSIC
dev-%.o: %.m
	$(ENV) $(CC) -arch $(DEV_ARCH) $(DEV_CFLAGS) -I$(DEV_CLASSIC_SDK_DIR)/include/mono-2.0 -c -o $@ $^

app-device.a: dev-main.o dev-runtime.o
	libtool -static -o $@ $^
endif

ifdef ENABLE_NETCORE
dev-netcore-%.o: %.m
	$(ENV) $(CC) -arch $(DEV_ARCH) $(DEV_CFLAGS) -I$(DEV_NETCORE_SDK_DIR)/include/mono-2.0 -DENABLE_NETCORE -c -o $@ $^

app-netcore-device.a: dev-netcore-main.o dev-netcore-runtime.o
	libtool -static -o $@ $^
endif

clean:
	$(RM) -rf app-simulator app-netcore-simulator *.o *.a
