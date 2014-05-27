thisdir = class/System.Configuration
SUBDIRS =

include ../../build/rules.make

LIBRARY = System.Configuration.dll

LOCAL_MCS_FLAGS = -lib:$(secxml_libdir) -lib:$(bare_libdir)
test_remove = $(LOCAL_MCS_FLAGS)
LIB_MCS_FLAGS = -r:$(corlib) -r:System.dll -r:System.Xml.dll -r:System.Security.dll -nowarn:618 
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) 

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
