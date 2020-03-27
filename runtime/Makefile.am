
tmpinst = _tmpinst

noinst_SCRIPTS = mono-wrapper monodis-wrapper

EXTRA_DIST = bin/mono-hang-watchdog.in

etctmp = etc
symlinks = etc/mono/1.0/machine.config etc/mono/2.0/machine.config etc/mono/2.0/web.config etc/mono/browscap.ini etc/mono/2.0/Browsers/Compat.browser

if INSTALL_4_x
symlinks += etc/mono/4.0/machine.config etc/mono/4.0/web.config etc/mono/4.5/web.config etc/mono/4.0/Browsers/Compat.browser etc/mono/4.5/Browsers/Compat.browser
endif

# This is needed at least to build configure --with-runtime-preset=fullaot.
symlinks += etc/mono/4.5/machine.config

etc/mono/2.0/machine.config: $(top_srcdir)/data/net_2_0/machine.config
etc/mono/2.0/web.config: $(top_srcdir)/data/net_2_0/web.config
etc/mono/browscap.ini: $(top_srcdir)/data/browscap.ini
etc/mono/2.0/Browsers/Compat.browser: $(top_srcdir)/data/Browsers/Compat.browser
etc/mono/4.0/Browsers/Compat.browser: $(top_srcdir)/data/Browsers/Compat.browser
etc/mono/4.5/Browsers/Compat.browser: $(top_srcdir)/data/Browsers/Compat.browser
etc/mono/4.0/machine.config: $(top_srcdir)/data/net_4_0/machine.config
etc/mono/4.0/web.config: $(top_srcdir)/data/net_4_0/web.config
etc/mono/4.5/machine.config: $(top_srcdir)/data/net_4_5/machine.config
etc/mono/4.5/web.config: $(top_srcdir)/data/net_4_5/web.config

$(symlinks):
	cd $(top_builddir) && $(SHELL) ./config.status $(subdir)/$@

SUPPORT_FILES = $(symlinks) mono-wrapper etc/mono/config

build_profiles = 

if INSTALL_4_x
build_profiles += binary_reference_assemblies net_4_x xbuild_12 xbuild_14
net_profile = net_4_x
endif

if INSTALL_MONODROID
build_profiles += monodroid monodroid_tools
endif

if INSTALL_MONOTOUCH
build_profiles += monotouch monotouch_runtime
endif

if INSTALL_MONOTOUCH_WATCH
build_profiles += monotouch_watch monotouch_watch_runtime
endif

if INSTALL_MONOTOUCH_TV
build_profiles += monotouch_tv monotouch_tv_runtime
endif

if INSTALL_TESTING_AOT_FULL_INTERP
build_profiles += testing_aot_full_interp
endif

if INSTALL_TESTING_AOT_HYBRID
build_profiles += testing_aot_hybrid
endif

if INSTALL_TESTING_AOT_FULL
build_profiles += testing_aot_full
endif

if INSTALL_WINAOT
build_profiles += winaot
endif

if INSTALL_TESTING_WINAOT_INTERP
build_profiles += testing_winaot_interp
endif

if INSTALL_XAMMAC
build_profiles += xammac xammac_net_4_5
endif

if INSTALL_ORBIS
build_profiles += orbis
endif

if INSTALL_UNREAL
build_profiles += unreal
endif

if INSTALL_WASM
build_profiles += wasm wasm_tools
endif

if ENABLE_NETCORE
build_profiles += netcore
endif

test_profiles = $(filter-out binary_reference_assemblies monodroid_tools wasm_tools,$(build_profiles))

MAKE_FLAGS=$(if $(V),,--no-print-directory -s)

# The write check is to foil 'make distcheck'
all-mcs: $(SUPPORT_FILES) $(TEST_SUPPORT_FILES)
	if test -w $(mcs_topdir); then :; else chmod -R +w $(mcs_topdir); fi
	cd $(mcs_topdir) && $(MAKE) $(MAKE_FLAGS) NO_DIR_CHECK=1 PROFILES='$(build_profiles)' CC='$(CC)' all-profiles

install-mcs: install-mcs-exec install-mcs-data

# override automake
install-mcs-exec: $(SUPPORT_FILES) $(TEST_SUPPORT_FILES)
	cd $(mcs_topdir) && $(MAKE) NO_DIR_CHECK=1 PROFILES='$(build_profiles)' RUNTIME_HAS_CONSISTENT_GACDIR=yes prefix=$(prefix) install-profiles

# override automake
install-mcs-data:
	@:

# override automake
uninstall-mcs:
	cd $(mcs_topdir) && $(MAKE) NO_DIR_CHECK=1 PROFILES='$(build_profiles)' RUNTIME_HAS_CONSISTENT_GACDIR=yes prefix=$(prefix) uninstall-profiles

clean-mcs:
	cd $(mcs_topdir) && $(MAKE) NO_DIR_CHECK=1 PROFILES='$(build_profiles)' clean-profiles
	-rm -fr $(etctmp) $(tmpinst)

if BUILD_MCS

all-local: all-mcs

# override automake
install: install-mcs

# override automake
install-exec: install-mcs-exec

# override automake
install-data: install-mcs-data

# override automake
uninstall: uninstall-mcs

clean-local: clean-mcs

endif BUILD_MCS

TEST_SUPPORT_FILES = $(tmpinst)/bin/mono $(tmpinst)/bin/ilasm $(tmpinst)/bin/csc $(tmpinst)/bin/mcs $(tmpinst)/bin/al

mcs-do-test-profiles:
	cd $(mcs_topdir) && $(MAKE) NO_DIR_CHECK=1 PROFILES='$(test_profiles)' test-profiles
	cd $(mcs_topdir) && $(MAKE) NO_DIR_CHECK=1 PROFILES='$(test_profiles)' compiler-test-profiles

mcs-do-xunit-test-profiles:
	cd $(mcs_topdir) && $(MAKE) NO_DIR_CHECK=1 PROFILES='$(test_profiles)' xunit-test-profiles

mcs-do-run-test-profiles: test-support-files
	cd $(mcs_topdir) && $(MAKE) NO_DIR_CHECK=1 PROFILES='$(test_profiles)' run-test-profiles
	cd $(mcs_topdir) && $(MAKE) NO_DIR_CHECK=1 PROFILES='$(test_profiles)' run-compiler-test-profiles

mcs-do-xunit-run-test-profiles: test-support-files
	cd $(mcs_topdir) && $(MAKE) NO_DIR_CHECK=1 PROFILES='$(test_profiles)' run-xunit-test-profiles

test-bundle-local:
	mkdir -p $(TEST_BUNDLE_PATH)/_tmpinst/bin
	cp -L mono-test.sh $(TEST_BUNDLE_PATH)
	cp -L -R etc $(TEST_BUNDLE_PATH)/_tmpinst
	rm $(TEST_BUNDLE_PATH)/_tmpinst/etc/mono/config
	sed -e 's,$$mono_libdir,$$test_root_dir,g' -e 's,target="$(libgdiplus_install_loc)",target="$$test_root_dir/mono-libgdiplus$(libsuffix)",g' $(top_builddir)/data/config > $(TEST_BUNDLE_PATH)/_tmpinst/etc/mono/config.tmpl
	echo '#! /bin/sh' > $(TEST_BUNDLE_PATH)/_tmpinst/bin/al
	echo '#! /bin/sh' > $(TEST_BUNDLE_PATH)/_tmpinst/bin/mcs
	echo '#! /bin/sh' > $(TEST_BUNDLE_PATH)/_tmpinst/bin/ilasm
	echo '"$$MONO_EXECUTABLE" $$(dirname "$$MONO_EXECUTABLE")/net_4_x/al.exe "$$@"' >> $(TEST_BUNDLE_PATH)/_tmpinst/bin/al
	echo '"$$MONO_EXECUTABLE" $$(dirname "$$MONO_EXECUTABLE")/net_4_x/mcs.exe "$$@"' >> $(TEST_BUNDLE_PATH)/_tmpinst/bin/mcs
	echo '"$$MONO_EXECUTABLE" $$(dirname "$$MONO_EXECUTABLE")/net_4_x/ilasm.exe "$$@"' >> $(TEST_BUNDLE_PATH)/_tmpinst/bin/ilasm
	chmod +x $(TEST_BUNDLE_PATH)/_tmpinst/bin/al
	chmod +x $(TEST_BUNDLE_PATH)/_tmpinst/bin/mcs
	chmod +x $(TEST_BUNDLE_PATH)/_tmpinst/bin/ilasm
	for profile in $(test_profiles); do \
		cp -L -R $(mcs_topdir)/class/lib/$$profile $(TEST_BUNDLE_PATH); \
	done
	cp -L $(top_srcdir)/external/xunit-binaries/*.dll $(top_srcdir)/external/xunit-binaries/*.exe $(top_srcdir)/external/xunit-binaries/*.config $(TEST_BUNDLE_PATH)/net_4_x/
	$(MAKE) -C $(mcs_topdir)/tests test-bundle
	$(MAKE) -C $(mcs_topdir)/errors test-bundle
	$(MAKE) -C $(mcs_topdir)/packages test-bundle
	$(MAKE) -C $(mcs_topdir)/tools/mono-symbolicate test-bundle

if HOST_WIN32
if CROSS_COMPILING
cur_dir_cmd = pwd
PLATFORM_PATH_SEPARATOR = :
else
cur_dir_cmd = cygpath -w -a .
PLATFORM_PATH_SEPARATOR = ;
endif
else
cur_dir_cmd = pwd
PLATFORM_PATH_SEPARATOR = :
endif

# assemblies which are excluded from testing in mcs-compileall below
VERIFY_TESTS_FILTER = System.Runtime.CompilerServices.Unsafe.dll nunitlite.dll

if HOST_WIN32
# Mono.WebBrowser.dll fails to verify on Windows
VERIFY_TESTS_FILTER += Mono.WebBrowser.dll
endif

# fails to verify with "VTable setup of type Xunit.NetCore.Extensions.SkippedTestCase failed", not sure why
VERIFY_TESTS_FILTER += Xunit.NetCore.Extensions.dll

if FULL_AOT_TESTS
#  ILASM.exe has features which a testing_aot_full runtime will not support.
#  It is invoked with an external mono when used in the runtime.
#  We skip it here because otherwise it will fail to verify.
VERIFY_TESTS_FILTER += ilasm.exe
endif

# Compile all assemblies with the verifier turned on. Code must be valid but not verifiable.
# TODO it would be nice to split assemblies without unsafe code to use the verifier with verifiable mode.
mcs-compileall: mono-wrapper etc/mono/config
	save_MONO_PATH=$$MONO_PATH; mcs_topdir=`cd $(mcs_topdir) && $(cur_dir_cmd)`; ok=:; \
	for profile in $(test_profiles); do \
	  if [ "xbuild_12" = "$$profile" ]; then \
		  MONO_PATH="$$mcs_topdir/class/lib/$$profile$(PLATFORM_PATH_SEPARATOR)$$mcs_topdir/class/lib/net_4_x$(PLATFORM_PATH_SEPARATOR)$$save_MONO_PATH"; \
	  elif [ "xbuild_14" = "$$profile" ]; then \
		  MONO_PATH="$$mcs_topdir/class/lib/$$profile$(PLATFORM_PATH_SEPARATOR)$$mcs_topdir/class/lib/net_4_x$(PLATFORM_PATH_SEPARATOR)$$save_MONO_PATH"; \
	  else \
		  MONO_PATH="$$mcs_topdir/class/lib/$$profile$(PLATFORM_PATH_SEPARATOR)$$save_MONO_PATH"; \
	  fi; \
	  export verifiable_files=`ls "$(mcs_topdir)/class/lib/$$profile/" | grep -E '\.(dll|exe)$$' | grep -v '\.dll\.dll$$' | grep -v '\.exe\.dll$$' $(foreach asm,$(VERIFY_TESTS_FILTER), | grep -v $(asm))` ; \
	  export MONO_PATH; \
	  for stub in $$verifiable_files; do \
	  i=$(mcs_topdir)/class/lib/$$profile/$$stub ; \
	  echo $$i ; \
		if [ ! -f $$i ] ; then \
			continue ; \
		fi ;  \
	    if ./mono-wrapper --compile-all --verify-all --security=verifiable $$i; then \
	      echo $$i verified OK; \
	    else \
	      echo $$i verification failed; ok=false; \
	    fi; done; done; \
	if [ "$$ok" = "false" ]; then echo "<?xml version='1.0' encoding='utf-8'?><test-results failures='1' total='1' not-run='0' name='verify' date='$$(date +%F)' time='$$(date +%T)'><test-suite name='mcs-compileall' success='False' time='0'><results><test-case name='fail' executed='True' success='False' time='0'><failure><message>Verifying framework assemblies failed. Check the log for more details.</message><stack-trace></stack-trace></failure></test-case></results></test-suite></test-results>" > TestResult-verify.xml; fi; \
	$$ok

check-local: mcs-compileall mcs-do-test-profiles mcs-do-xunit-test-profiles
	$(MAKE) mcs-do-run-test-profiles mcs-do-run-xunit-test-profiles

# Compile all mcs tests
test: mcs-do-test-profiles
xunit-test: mcs-do-xunit-test-profiles

CLEANFILES = etc/mono/config

# depend on $(symlinks) to ensure 'etc/mono' directory exists
etc/mono/config: ../data/config Makefile $(symlinks)
	cp $< $@
	if test -d "$(abs_top_builddir)/support"; then \
		sed 's,target="$$mono_libdir/libMonoPosixHelper$(libsuffix)",target="$(abs_top_builddir)/support/libMonoPosixHelper.la",' $@ > $@.tmp \
		 && mv $@.tmp $@ \
		 ; \
	else \
		echo "Unknown directory '$(abs_top_builddir)/support'" && false; \
	fi
	if test -d "$(abs_top_builddir)/mono/native"; then \
		sed 's,target="$$mono_libdir/$(MONO_NATIVE_LIBRARY_NAME)$(libsuffix)",target="$(abs_top_builddir)/mono/native/$(MONO_NATIVE_LIBRARY_NAME).la",' $@ > $@.tmp \
		 && mv $@.tmp $@ \
		 ; \
	else \
		echo "Unknown directory '$(abs_top_builddir)/mono/native'" && false; \
	fi
	if test -d "$(abs_top_builddir)/mono/btls/build-shared"; then \
		sed 's,target="$$mono_libdir/libmono-btls-shared$(libsuffix)",target="$(abs_top_builddir)/mono/btls/build-shared/libmono-btls-shared$(libsuffix)",' $@ > $@.tmp \
		 && mv $@.tmp $@ \
		 ; \
	else \
		echo "Unknown directory '$(abs_top_builddir)/mono/btls/build-shared'"; \
	fi
	if test -n "$(libgdiplus_loc)"; then \
		sed 's,target="$(libgdiplus_install_loc)",target="$(libgdiplus_loc)",' $@ > $@.tmp \
		 && mv $@.tmp $@ \
		 ; \
	fi
	cat $@

$(tmpinst)/bin/mono: mono-wrapper etc/mono/config
	$(mkinstalldirs) $(tmpinst)/bin
	cp mono-wrapper $@

$(tmpinst)/bin/csc: $(tmpinst)/bin/mono Makefile
	echo '#! /bin/sh' > $@ ; \
	r=`pwd`; m=`cd $(mcs_topdir) && pwd`; \
	echo 'exec "'"$$r/$(tmpinst)/bin/mono"'" "'"$(CSC)"'" "$$@"' >> $@ ; \
	chmod +x $@

$(tmpinst)/bin/mcs: $(tmpinst)/bin/mono Makefile
	echo '#! /bin/sh' > $@ ; \
	r=`pwd`; m=`cd $(mcs_topdir) && pwd`; \
	echo 'exec "'"$$r/$(tmpinst)/bin/mono"'" "'"$$m/class/lib/$(net_profile)/mcs.exe"'" "$$@"' >> $@ ; \
	chmod +x $@

$(tmpinst)/bin/ilasm: $(tmpinst)/bin/mono Makefile
	echo '#! /bin/sh' > $@ ; \
	r=`pwd`; m=`cd $(mcs_topdir) && pwd`; \
	echo 'exec "'"$$r/$(tmpinst)/bin/mono"'" "'"$$m/ilasm/ilasm.exe"'" "$$@"' >> $@ ; \
	chmod +x $@

$(tmpinst)/bin/al: $(tmpinst)/bin/mono Makefile
	echo '#! /bin/sh' > $@ ; \
	r=`pwd`; m=`cd $(mcs_topdir) && pwd`; \
	echo 'exec "'"$$r/$(tmpinst)/bin/mono"'" "'"$$m/class/lib/$(net_profile)/al.exe"'" "$$@"' >> $@ ; \
	chmod +x $@

test-support-files: $(TEST_SUPPORT_FILES)
	@:
