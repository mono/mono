# This is just used to copy and install the DLL files that are currently
# being compiled on windows.
#

# hack to prevent 'check' from depending on 'all'
AUTOMAKE_OPTIONS = cygnus

tmpinst = _tmpinst

noinst_SCRIPTS = mono-wrapper

symlinks = etc/mono/1.0/machine.config etc/mono/2.0/machine.config

etc/mono/1.0/machine.config: $(top_srcdir)/data/net_1_1/machine.config
etc/mono/2.0/machine.config: $(top_srcdir)/data/net_2_0/machine.config

$(symlinks):
	cd $(top_builddir) && $(SHELL) ./config.status $(subdir)/$@

all-local: $(symlinks) mono-wrapper
	d=`pwd`; cd $(mcs_topdir) && $(MAKE) RUNTIME=$$d/mono-wrapper PROFILES='$(build_profiles)' all-profiles

if INSTALL_2_0
build_profiles = default net_2_0
clean_profiles = basic net_1_1_bootstrap default net_2_0_bootstrap net_2_0
else
build_profiles = default
clean_profiles = basic net_1_1_bootstrap default
endif

# override automake
install: install-exec install-data

# override automake
install-exec:
	d=`pwd`; cd $(mcs_topdir) && $(MAKE) RUNTIME=$$d/mono-wrapper prefix=$(exec_prefix) PROFILES='$(build_profiles)' install-profiles

# override automake
install-data:
	@:

clean-local:
	-rm -fr $(symlinks)
	d=`pwd`; cd $(mcs_topdir) && $(MAKE) PROFILES='$(clean_profiles)' clean-profiles
	-rm -fr $(tmpinst)

if INSTALL_2_0
TEST_SUPPORT_FILES = $(tmpinst)/bin/mcs $(tmpinst)/bin/mbas $(tmpinst)/bin/ilasm $(tmpinst)/bin/gmcs
else
TEST_SUPPORT_FILES = $(tmpinst)/bin/mcs $(tmpinst)/bin/mbas $(tmpinst)/bin/ilasm
endif

check-local:
	$(mkinstalldirs) $(tmpinst)/bin
	$(MAKE) $(TEST_SUPPORT_FILES)
	d=`pwd`; PATH=$$d/$(tmpinst)/bin:$$PATH ; export PATH ; \
	( cd $(mcs_topdir) && $(MAKE) RUNTIME=$$d/mono-wrapper PROFILES='$(build_profiles)' run-test-profiles ) || ret=false ; \
	rm -fr $(tmpinst) ; $$ret

$(tmpinst)/bin/mcs:
	$(MAKE) test-support-file target=$@ file=class/lib/default/mcs.exe

$(tmpinst)/bin/mbas:
	$(MAKE) test-support-file target=$@ file=mbas/mbas.exe

$(tmpinst)/bin/gmcs:
	$(MAKE) test-support-file target=$@ file=gmcs/gmcs.exe

$(tmpinst)/bin/ilasm:
	$(MAKE) test-support-file target=$@ file=ilasm/ilasm.exe

test-support-file:
	echo '#! /bin/sh' > $(target)
	r=`pwd`; m=`cd $(mcs_topdir) && pwd`; echo 'exec "'"$$r/mono-wrapper"'" "'"$$m/$(file)"'" "$$@"' >> $(target)
	chmod +x $(target)

$(tmpinst)/bin/pedump: $(srcdir)/Makefile.am
	$(mkdir_p) $(@D)
	(b=`pwd`; echo '#! /bin/sh'; echo 'exec "'"$$b/libtool"'" --mode=execute "'"$$b/mono/metadata/pedump"'" "$$@"') > $@
	chmod +x $@
