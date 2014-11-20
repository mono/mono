thisdir = class/System.Configuration
SUBDIRS =

include ../../build/rules.make

LIBRARY = System.Configuration.dll

LIB_MCS_FLAGS = -r:$(bare_libdir)/System.Xml.dll -r:$(secxml_libdir)/System.dll
LOCAL_MCS_FLAGS = -r:$(corlib) -r:$(the_libdir_base)System.Security.dll -nowarn:618 
TEST_MCS_FLAGS = -r:System.dll -r:System.Xml.dll

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
