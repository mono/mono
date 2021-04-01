thisdir = class/System.Net.Http/UnitTests
SUBDIRS = 
include ../../../build/rules.make

LIBRARY = System.Net.Http.UnitTests.dll
NO_BUILD = yes
NO_INSTALL = yes

XTEST_LIB_REFS = System System.Core Facades/System.Threading.Tasks
USE_XTEST_REMOTE_EXECUTOR = YES
XTEST_MCS_FLAGS = -nowarn:436

include ../../../build/library.make

clean-local:
	-rm -f $(tests_CLEAN_FILES) $(CLEAN_FILES)

dist-local: dist-default
