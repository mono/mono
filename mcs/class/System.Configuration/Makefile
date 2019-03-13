thisdir = class/System.Configuration
SUBDIRS =

include ../../build/rules.make

LIBRARY = System.Configuration.dll

OTHER_RES= \
		System.Configuration.resources

LOCAL_MCS_FLAGS = -d:FEATURE_PAL -d:CODEDOM \
	$(OTHER_RES:%=/resource:%)
LIB_REFS = System.Security System System.Xml
KEYFILE = ../msfinal.pub
LIB_MCS_FLAGS = -nowarn:618
TEST_MCS_FLAGS = -d:FEATURE_PAL -d:CODEDOM
TEST_LIB_REFS = System.Xml System
TEST_NUNITLITE_APP_CONFIG_GLOBAL=Test/test-config-file
LIBRARY_WARN_AS_ERROR = yes

RESX_RESOURCE_STRING=../../../external/corefx/src/System.Configuration.ConfigurationManager/src/Resources/Strings.resx

include ../../build/library.make

$(build_lib): $(OTHER_RES)


test-local:
	cp -f Test/appSettings.config $(dir $(NUNITLITE_CONFIG_FILE))/System.Configuration-appSettings.config

run-test:
	$(MAKE) -C Test/standalone
