thisdir = class/System.Configuration
SUBDIRS =

include ../../build/rules.make

ASSEMBLY = System.Configuration.dll

TEST_MCS_FLAGS =
TEST_LIB_REFS = System.Xml System
TEST_NUNITLITE_APP_CONFIG_GLOBAL=Test/test-config-file

include ../../build/msbuild.make

test-local:
	cp Test/appSettings.config $(dir $(NUNITLITE_CONFIG_FILE))/Test-appSettings.config

run-test:
	$(MAKE) -C Test/standalone
