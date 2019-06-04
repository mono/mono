thisdir = class/System.Configuration
SUBDIRS =

include ../../build/rules.make

LIBRARY = System.Configuration.dll

LOCAL_MCS_FLAGS = 
LIB_REFS = System.Security System System.Xml
KEYFILE = ../msfinal.pub
LIB_MCS_FLAGS = -nowarn:618
TEST_MCS_FLAGS =
TEST_LIB_REFS = System.Xml System
TEST_NUNITLITE_APP_CONFIG_GLOBAL=Test/test-config-file
LIBRARY_WARN_AS_ERROR = yes

include ../../build/library.make

test-local:
	cp -f Test/appSettings.config $(dir $(NUNITLITE_CONFIG_FILE))/System.Configuration-appSettings.config

run-test:
	$(MAKE) -C Test/standalone
