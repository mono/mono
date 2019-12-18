thisdir = class/System.Net.Http/FunctionalTests
SUBDIRS = 
include ../../../build/rules.make

LIBRARY = System.Net.Http.FunctionalTests.dll
NO_BUILD = yes
NO_INSTALL = yes

XTEST_LIB_REFS = System System.Core Facades/System.Threading.Tasks System.Net.Http
USE_XTEST_REMOTE_EXECUTOR = YES

XTEST_LIB_FLAGS = /resource:../../System/Test/TestData/testservereku.contoso.com.pfx
include ../../../build/library.make

TEST_RUNTIME := MONO_URI_DOTNETRELATIVEORABSOLUTE=true $(TEST_RUNTIME)

clean-local:
	-rm -f $(tests_CLEAN_FILES) $(CLEAN_FILES)

dist-local: dist-default
