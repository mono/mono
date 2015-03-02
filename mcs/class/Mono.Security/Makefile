thisdir = class/Mono.Security
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Security.dll
LOCAL_MCS_FLAGS = -lib:$(the_libdir_base)bare
LIB_REFS = System
LIB_MCS_FLAGS = -unsafe -nowarn:1030
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:169,219,618,672

include ../../build/library.make

$(build_lib): $(the_libdir_base)bare/System.dll

$(the_libdir_base)bare/System.dll:
	(cd ../System; $(MAKE) $@)

.NOTPARALLEL: $(the_libdir_base)bare/System.dll
