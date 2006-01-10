thisdir = nunit20/nunit-console
SUBDIRS = 
include ../../build/rules.make

PROGRAM = $(topdir)/class/lib/$(PROFILE)/nunit-console.exe
LOCAL_MCS_FLAGS = /r:nunit.framework.dll /r:nunit.util.dll /r:nunit.core.dll
DISTFILES = app.config App.ico nunit-console.csproj nunit-console.exe.net_2_0.config

ifeq (1.0, $(FRAMEWORK_VERSION))

include ../../build/executable.make

else

base_prog = nunit-console.exe
$(PROGRAM): $(topdir)/class/lib/default/$(base_prog)
	cp $< $@
	-cp -f $<.mdb $@.mdb

$(PROGRAM).config: $(base_prog).$(PROFILE).config
	cp $< $@

all-local: $(PROGRAM) $(PROGRAM).config
clean-local:
	rm -f $(PROGRAM) $(PROGRAM).config $(PROGRAM:.exe=.mdb) $(PROGRAM:.exe=.pdb)

test-local run-test-local run-test-ondotnet-local: all-local

ifndef PROGRAM_INSTALL_DIR
PROGRAM_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)
endif

install-local: all-local
	$(MKINSTALLDIRS) $(DESTDIR)$(PROGRAM_INSTALL_DIR)
	$(INSTALL_BIN) $(PROGRAM) $(DESTDIR)$(PROGRAM_INSTALL_DIR)
	test ! -f $(PROGRAM).mdb || $(INSTALL_BIN) $(PROGRAM).mdb $(DESTDIR)$(PROGRAM_INSTALL_DIR)
	$(INSTALL_DATA) $(PROGRAM).config $(DESTDIR)$(PROGRAM_INSTALL_DIR)

uninstall-local:
	-rm -f $(DESTDIR)$(PROGRAM_INSTALL_DIR)/$(base_prog) $(DESTDIR)$(PROGRAM_INSTALL_DIR)/$(base_prog).mdb $(DESTDIR)$(PROGRAM_INSTALL_DIR)/$(base_prog).config

endif
