# This is just used to copy and install the DLL files that are currently
# being compiled on windows.
#

tmpinst = _tmpinst

noinst_SCRIPTS = mono-wrapper

symlinks = etc/mono/1.0/machine.config etc/mono/2.0/machine.config

etc/mono/1.0/machine.config: $(top_srcdir)/data/net_1_1/machine.config
etc/mono/2.0/machine.config: $(top_srcdir)/data/net_2_0/machine.config

$(symlinks):
	cd $(top_builddir) && $(SHELL) ./config.status $(subdir)/$@

all-local: $(symlinks) mono-wrapper
	d=`pwd`; cd $(mcs_topdir) && $(MAKE) RUNTIME=$$d/mono-wrapper all-profiles

if INSTALL_2_0
install_profiles = default net_2_0
else
install_profiles = default
endif

install-exec-local:
	d=`pwd`; cd $(mcs_topdir) && $(MAKE) RUNTIME=$$d/mono-wrapper prefix=$(exec_prefix) PROFILES='$(install_profiles)' install-profiles

clean-local:
	-rm -fr $(symlinks)
	d=`pwd`; cd $(mcs_topdir) && $(MAKE) PROFILES='basic net_1_1_bootstrap default net_2_0_bootstrap net_2_0' clean-profiles
	-rm -fr $(tmpinst)

TEST_SUPPORT_FILES = $(tmpinst)/bin/mcs $(tmpinst)/bin/mbas $(tmpinst)/bin/ilasm $(tmpinst)/bin/gmcs
check-local:
	$(mkdir_p) $(tmpinst)/bin
	$(MAKE) $(TEST_SUPPORT_FILES)
	d=`pwd`; PATH=$$d/$(tmpinst)/bin:$$PATH ; export PATH ; \
	( cd $(mcs_topdir) && $(MAKE) RUNTIME=$$d/mono-wrapper run-test-profiles ) || ret=false ; \
	rm -f $(tmpinst) ; $$ret

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
