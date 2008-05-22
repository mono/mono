thisdir = class/Mono.WebBrowser
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Mozilla.dll
LIB_MCS_FLAGS = -r:System.dll -debug+ -nowarn:108

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES = README build-csproj2k5

Mono.Mozilla2K5.csproj: Mono.Mozilla.dll.sources build-csproj2k5
	./build-csproj2k5

include ../../build/library.make

$(the_lib): Mono.Mozilla2K5.csproj
