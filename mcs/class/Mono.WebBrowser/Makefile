thisdir = class/Mono.WebBrowser
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.WebBrowser.dll
LIB_MCS_FLAGS = -r:System.dll -warn:1

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES = README build-csproj2k5

Mono.WebBrowser2K5.csproj: Mono.WebBrowser.dll.sources build-csproj2k5
	./build-csproj2k5

include ../../build/library.make

$(the_lib): Mono.WebBrowser2K5.csproj
