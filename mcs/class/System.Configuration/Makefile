thisdir = class/System.Configuration
SUBDIRS =

include ../../build/rules.make

LIBRARY = System.Configuration.dll

LOCAL_MCS_FLAGS = 
LIB_REFS = secxml/System bare/System.Xml System.Security
LIB_MCS_FLAGS = -nowarn:618
TEST_MCS_FLAGS =
TEST_LIB_REFS = System.Xml System

include ../../build/library.make

configuration_library_deps = \
	$(secxml_libdir)/System.dll 	\
	$(the_libdir_base)System.Security.dll 	\
	$(bare_libdir)/System.Xml.dll

$(build_lib): $(configuration_library_deps)

.NOTPARALLEL: $(configuration_library_deps)

$(secxml_libdir)/System.dll:
	@echo System.Configuration: GETTING: $@
	(cd ../System; $(MAKE) $@)

$(the_libdir_base)System.Security.dll:
	(cd ../System.Security; $(MAKE) $@)

$(bare_libdir)/System.Xml.dll:
	(cd ../System.XML; $(MAKE) $@)

run-test:
	$(MAKE) -C Test/standalone
