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

# The write check is to foil 'make distcheck'
all-local: $(symlinks) mono-wrapper
	if test -w $(mcs_topdir); then :; else chmod -R +w $(mcs_topdir); fi
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
	d=`pwd`; \
	cd $(mcs_topdir) && $(MAKE) prefix='$(exec_prefix)' PROFILES='$(build_profiles)' \
				    RUNTIME=$$d/mono-wrapper RUNTIME_HAS_CONSISTENT_GACDIR=yes install-profiles

# override automake
install-data:
	@:

# override automake
uninstall:
	d=`pwd`; \
	cd $(mcs_topdir) && $(MAKE) prefix='$(exec_prefix)' PROFILES='$(build_profiles)' \
				    RUNTIME=$$d/mono-wrapper RUNTIME_HAS_CONSISTENT_GACDIR=yes uninstall-profiles

clean-local:
	d=`pwd`; cd $(mcs_topdir) && $(MAKE) PROFILES='$(clean_profiles)' clean-profiles
	-rm -fr $(symlinks) $(tmpinst) .wapi

if INSTALL_2_0
TEST_SUPPORT_FILES = $(tmpinst)/config $(tmpinst)/bin/mono $(tmpinst)/bin/mcs $(tmpinst)/bin/mbas $(tmpinst)/bin/ilasm $(tmpinst)/bin/gmcs
else
TEST_SUPPORT_FILES = $(tmpinst)/config $(tmpinst)/bin/mono $(tmpinst)/bin/mcs $(tmpinst)/bin/mbas $(tmpinst)/bin/ilasm
endif

# List of NUnit testcases that should be 100% pass
centum_nunit_tests = \
	class/Commons.Xml.Relaxng \
	class/Cscompmgd \
	class/Microsoft.JScript \
	class/Mono.Posix \
	class/Mono.Security \
	class/System \
	class/System.Configuration.Install \
	class/System.Runtime.Remoting \
	class/System.Runtime.Serialization.Formatters.Soap \
	class/System.Web.Services

# now a misnomer, but it'll go away soon enough.
if ENABLE_NUNIT_TESTS
test_select =
else
test_select = TEST_SUBDIRS='$(centum_nunit_tests) tests errors'
endif

mcs-do-test-profiles:
	d=`pwd`; cd $(mcs_topdir) && $(MAKE) RUNTIME=$$d/mono-wrapper PROFILES='$(build_profiles)' test-profiles

mcs-do-run-test-profiles: test-support-files
	d=`pwd`; PATH=$$d/$(tmpinst)/bin:$$PATH ; export PATH ; \
	( cd $(mcs_topdir) && $(MAKE) RUNTIME=$$d/$(tmpinst)/bin/mono PROFILES='$(build_profiles)' run-test-profiles ) || ret=false ; \
	rm -fr $(tmpinst); $$ret

# Use --compile-all as a poor man's PEVerify to detect invalid IL
# This doesn't yet work on the 2.0 profile
verify_profiles = $(filter-out net_2_0, $(build_profiles))
#verify_profiles = $(build_profiles)
mcs-compileall: test-support-files
	save_MONO_PATH=$$MONO_PATH; mcs_topdir=`cd $(mcs_topdir) && pwd`; ret=:; \
	for profile in $(verify_profiles); do \
	  MONO_PATH="$$mcs_topdir/class/lib/$$profile:$$save_MONO_PATH"; export MONO_PATH; \
	  for i in $(mcs_topdir)/class/lib/$$profile/*.dll $(mcs_topdir)/class/lib/$$profile/*.exe; do \
	    if ./$(tmpinst)/bin/mono --compile-all $$i; then \
	      echo $$i verified OK; \
	    else \
	      echo $$i verification failed; ret=false; \
	    fi; done; done; \
	rm -fr $(tmpinst); $$ret

check-local: mcs-compileall mcs-do-test-profiles
	$(MAKE) $(test_select) mcs-do-run-test-profiles

$(tmpinst)/config: ../data/config
	d=`cd ../support && pwd`; \
	sed 's,target="libMonoPosixHelper[^"]*",target="'$$d/libMonoPosixHelper.la'",' ../data/config > $@

$(tmpinst)/bin/mono: mono-wrapper $(tmpinst)/config
	sed 's,/data/config",/runtime/$(tmpinst)/config",' mono-wrapper > $@
	chmod +x $@

$(tmpinst)/bin/mcs:
	$(MAKE) test-support-file target=$@ file=class/lib/default/mcs.exe

$(tmpinst)/bin/mbas:
	$(MAKE) test-support-file target=$@ file=mbas/mbas.exe

$(tmpinst)/bin/gmcs:
	$(MAKE) test-support-file target=$@ file=gmcs/gmcs.exe

$(tmpinst)/bin/ilasm:
	$(MAKE) test-support-file target=$@ file=ilasm/ilasm.exe

test-support-files:
	$(mkinstalldirs) $(tmpinst)/bin
	$(MAKE) $(TEST_SUPPORT_FILES)

test-support-file:
	echo '#! /bin/sh' > $(target)
	r=`pwd`; m=`cd $(mcs_topdir) && pwd`; echo 'exec "'"$$r/mono-wrapper"'" "'"$$m/$(file)"'" "$$@"' >> $(target)
	chmod +x $(target)

$(tmpinst)/bin/pedump: $(srcdir)/Makefile.am
	$(mkdir_p) $(@D)
	(b=`pwd`; echo '#! /bin/sh'; echo 'exec "'"$$b/libtool"'" --mode=execute "'"$$b/mono/metadata/pedump"'" "$$@"') > $@
	chmod +x $@

# the 'cygnus' option also disables the default 'distdir:' target, which we _do_ want
MYDISTFILES = Makefile.am Makefile.in mono-wrapper.in
distdir: $(MYDISTFILES)
	rm -fr $(distdir)
	mkdir $(distdir)
	test -z '$(MYDISTFILES)' || for file in ''$(MYDISTFILES); do \
	  if test -f $$file; then d=.; else d=$(srcdir); fi; \
	  cp -p $$d/$$file $(distdir) ; done
	find $(distdir) -type f -exec chmod a+r {} ';'
