thisdir = class/Mono.Security
SUBDIRS = 
include ../../build/rules.make

ASSEMBLY = Mono.Security.dll

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:169,219,618,672

include ../../build/msbuild.make

EXTRA_DISTFILES = Mono.Security.Interface/README.md
