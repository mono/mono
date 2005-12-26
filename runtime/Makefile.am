# hack to prevent 'check' from depending on 'all'
AUTOMAKE_OPTIONS = cygnus

tmpinst = _tmpinst

noinst_SCRIPTS = mono-wrapper monodis-wrapper semdel-wrapper

etctmp = etc
symlinks = etc/mono/1.0/machine.config etc/mono/2.0/machine.config

etc/mono/1.0/machine.config: $(top_srcdir)/data/net_1_1/machine.config
etc/mono/2.0/machine.config: $(top_srcdir)/data/net_2_0/machine.config

$(symlinks):
	cd $(top_builddir) && $(SHELL) ./config.status $(subdir)/$@

SUPPORT_FILES = $(symlinks) mono-wrapper wrapper-config

if INSTALL_2_0
build_profiles = default net_2_0
else
build_profiles = default
endif

if BUILD_MCS

# The write check is to foil 'make distcheck'
all-local: $(SUPPORT_FILES)
	if test -w $(mcs_topdir); then :; else chmod -R +w $(mcs_topdir); fi
	cd $(mcs_topdir) && $(MAKE) PROFILES='$(build_profiles)' CC=$(CC) all-profiles

# override automake
install: install-exec install-data

# override automake
install-exec: $(SUPPORT_FILES)
	cd $(mcs_topdir) && $(MAKE) PROFILES='$(build_profiles)' RUNTIME_HAS_CONSISTENT_GACDIR=yes prefix=$(prefix) install-profiles

# override automake
install-data:
	@:

# override automake
uninstall:
	cd $(mcs_topdir) && $(MAKE) PROFILES='$(build_profiles)' RUNTIME_HAS_CONSISTENT_GACDIR=yes prefix=$(prefix) uninstall-profiles

## semdel-wrapper will probably not delete the semaphore if someone is crazy enough to do a 'make -j distclean' :-)
clean-local:
	cd $(mcs_topdir) && $(MAKE) PROFILES='$(build_profiles)' clean-profiles
	-./semdel-wrapper
	-rm -fr $(etctmp) $(tmpinst) .wapi

endif BUILD_MCS

if INSTALL_2_0
TEST_SUPPORT_FILES = $(tmpinst)/bin/mono $(tmpinst)/bin/mcs $(tmpinst)/bin/mbas $(tmpinst)/bin/ilasm $(tmpinst)/bin/gmcs
else
TEST_SUPPORT_FILES = $(tmpinst)/bin/mono $(tmpinst)/bin/mcs $(tmpinst)/bin/mbas $(tmpinst)/bin/ilasm
endif

# now a misnomer, but it'll go away soon enough.
if ENABLE_NUNIT_TESTS
test_select =
else
test_select = ONLY_CENTUM_TESTS=yes
endif

mcs-do-test-profiles:
	cd $(mcs_topdir) && $(MAKE) PROFILES='$(build_profiles)' test-profiles

mcs-do-run-test-profiles: test-support-files
	d=`cd $(tmpinst) && pwd`; PATH=$$d/bin:$$PATH ; export PATH ; \
	cd $(mcs_topdir) && $(MAKE) PROFILES='$(build_profiles)' run-test-profiles

if PLATFORM_WIN32
cur_dir_cmd = cygpath -w -a .
PLATFORM_PATH_SEPARATOR = ;
else
cur_dir_cmd = pwd
PLATFORM_PATH_SEPARATOR = :
endif

# Use --compile-all as a poor man's PEVerify to detect invalid IL
mcs-compileall: mono-wrapper wrapper-config
	save_MONO_PATH=$$MONO_PATH; mcs_topdir=`cd $(mcs_topdir) && $(cur_dir_cmd)`; ok=:; \
	for profile in $(build_profiles); do \
	  MONO_PATH="$$mcs_topdir/class/lib/$$profile$(PLATFORM_PATH_SEPARATOR)$$save_MONO_PATH"; export MONO_PATH; \
	  for i in $(mcs_topdir)/class/lib/$$profile/*.dll $(mcs_topdir)/class/lib/$$profile/*.exe $(mcs_topdir)/class/Microsoft.VisualBasic/fixup/$$profile/*.dll; do \
	    if ./mono-wrapper --compile-all $$i; then \
	      echo $$i verified OK; \
	    else \
	      echo $$i verification failed; ok=false; \
	    fi; done; done; \
	$$ok

check-local: mcs-compileall mcs-do-test-profiles
	$(MAKE) $(test_select) mcs-do-run-test-profiles

CLEANFILES = wrapper-config

wrapper-config: ../data/config Makefile
	d=`cd ../support && pwd`; \
	sed 's,target="libMonoPosixHelper[^"]*",target="'$$d/libMonoPosixHelper.la'",' ../data/config > $@
	if test -z "$(libgdiplus_loc)"; then :; else \
	  mv $@ $@t; \
	  sed 's,<configuration>,& <dllmap dll="gdiplus.dll" target="$(libgdiplus_loc)" />,' $@t > $@; \
	  rm -f $@t; \
	fi

$(tmpinst)/bin/mono: mono-wrapper wrapper-config
	cp mono-wrapper $@

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
MYDISTFILES = $(DIST_COMMON)
distdir: $(MYDISTFILES)
	rm -fr $(distdir)
	mkdir $(distdir)
	test -z '$(MYDISTFILES)' || for file in ''$(MYDISTFILES); do \
	  cp -p $$file $(distdir) ; done
	find $(distdir) -type f -exec chmod a+r {} ';'
