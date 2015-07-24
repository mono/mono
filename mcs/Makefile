thisdir := .

SUBDIRS := build jay mcs class nunit24 ilasm tools tests errors docs

# Resgen is corlib specific tool

basic_SUBDIRS := build jay mcs class
build_SUBDIRS := build class mcs class/aot-compiler tools
monodroid_SUBDIRS := build class
monotouch_SUBDIRS := build class
monotouch_watch_SUBDIRS := build class
monotouch_runtime_SUBDIRS := build class
xammac_SUBDIRS := build class
mobile_SUBDIRS := build class
mobile_static_SUBDIRS := build class
binary_reference_assemblies_SUBDIRS := build class
net_4_x_SUBDIRS := build mcs class nunit24 ilasm tools tests errors docs
xammac_net_4_5_SUBDIRS := build class
xbuild_12_SUBDIRS := build class tools/xbuild
xbuild_14_SUBDIRS := build class tools/xbuild

include build/rules.make

all-recursive $(STD_TARGETS:=-recursive): dir-check platform-check profile-check

.PHONY: all-local $(STD_TARGETS:=-local)
all-local $(STD_TARGETS:=-local):
	@:

dir-check:
	@if [ "$(NO_DIR_CHECK)" = "" -a "$(PROFILE)" != "basic" ]; then $(MAKE) -C ../runtime; fi

# fun specialty targets

PROFILES = net_4_x binary_reference_assemblies xbuild_12 xbuild_14

.PHONY: all-profiles $(STD_TARGETS:=-profiles)
all-profiles $(STD_TARGETS:=-profiles): %-profiles: profiles-do--%
	@:

profiles-do--%:
	$(MAKE) $(PROFILES:%=profile-do--%--$*)

# The % below looks like profile-name--target-name
profile-do--%:
	$(MAKE) PROFILE=$(subst --, ,$*)

# xbuild_12 and xbuild_14 will try to install the same files, so they need
# to be ordered
profile-do--xbuild_14--install: profile-do--xbuild_12--install

# We don't want to run the tests in parallel.  We want behaviour like -k.
profiles-do--run-test:
	ret=:; $(foreach p,$(PROFILES), { $(MAKE) PROFILE=$(p) run-test || ret=false; }; ) $$ret

# Orchestrate the bootstrap here.
_boot_ = all clean install
$(_boot_:%=profile-do--xbuild_14--%):         profile-do--xbuild_14--%:         profile-do--net_4_x--%
$(_boot_:%=profile-do--xbuild_12--%):         profile-do--xbuild_12--%:         profile-do--net_4_x--%
$(_boot_:%=profile-do--binary_reference_assemblies--%):           profile-do--binary_reference_assemblies--%:           profile-do--build--%
$(_boot_:%=profile-do--net_4_x--%):           profile-do--net_4_x--%:           profile-do--build--%
$(_boot_:%=profile-do--monodroid--%):         profile-do--monodroid--%:         profile-do--build--%
$(_boot_:%=profile-do--monotouch--%):         profile-do--monotouch--%:         profile-do--build--%
$(_boot_:%=profile-do--monotouch_watch--%):   profile-do--monotouch_watch--%:   profile-do--build--%
$(_boot_:%=profile-do--monotouch_runtime--%):  profile-do--monotouch_runtime--%:  profile-do--build--%
$(_boot_:%=profile-do--xammac--%):            profile-do--xammac--%:            profile-do--build--%
$(_boot_:%=profile-do--xammac_net_4_5--%):    profile-do--xammac_net_4_5--%:           profile-do--build--%
$(_boot_:%=profile-do--mobile--%):            profile-do--mobile--%:         profile-do--build--%
$(_boot_:%=profile-do--mobile_static--%):     profile-do--mobile_static--%:     profile-do--build--%
$(_boot_:%=profile-do--build--%):             profile-do--build--%:             profile-do--basic--%

testcorlib:
	@cd class/corlib && $(MAKE) test run-test

compiler-tests:
	$(MAKE) TEST_SUBDIRS="tests errors" run-test-profiles

package := mcs-$(VERSION)

DISTFILES = \
	AUTHORS			\
	ChangeLog		\
	COPYING			\
	COPYING.LIB		\
	INSTALL.txt		\
	LICENSE			\
	LICENSE.GPL		\
	LICENSE.LGPL		\
	LICENSE.MPL		\
	Makefile		\
	mkinstalldirs		\
	MIT.X11			\
	MonoIcon.png		\
	README			\
	ScalableMonoIcon.svg	\
	winexe.in

dist-local: dist-default

csproj-local:

dist-pre:
	rm -rf $(package)
	mkdir $(package)

dist-tarball: dist-pre
	$(MAKE) distdir='$(package)' dist-recursive
	tar cvjf $(package).tar.bz2 $(package)

dist: dist-tarball
	rm -rf $(package)

# the egrep -v is kind of a hack (to get rid of the makefrags)
# but otherwise we have to make dist then make clean which
# is sort of not kosher. And it breaks with DIST_ONLY_SUBDIRS.
#
# We need to set prefix on make so class/System/Makefile can find
# the installed System.Xml to build properly

distcheck: dist-tarball
	rm -rf InstallTest Distcheck-MCS ; \
	mkdir InstallTest ; \
	destdir=`cd InstallTest && pwd` ; \
	mv $(package) Distcheck-MCS ; \
	(cd Distcheck-MCS && \
	    $(MAKE) prefix=$(prefix) && $(MAKE) test && $(MAKE) install DESTDIR="$$destdir" && \
	    $(MAKE) clean && $(MAKE) dist || exit 1) || exit 1 ; \
	mv Distcheck-MCS $(package) ; \
	tar tjf $(package)/$(package).tar.bz2 |sed -e 's,/$$,,' |sort >distdist.list ; \
	rm $(package)/$(package).tar.bz2 ; \
	tar tjf $(package).tar.bz2 |sed -e 's,/$$,,' |sort >before.list ; \
	find $(package) |egrep -v '(makefrag|response)' |sed -e 's,/$$,,' |sort >after.list ; \
	cmp before.list after.list || exit 1 ; \
	cmp before.list distdist.list || exit 1 ; \
	rm -f before.list after.list distdist.list ; \
	rm -rf $(package) InstallTest

monocharge:
	chargedir=monocharge-`date -u +%Y%m%d` ; \
	mkdir "$$chargedir" ; \
	DESTDIR=`cd "$$chargedir" && pwd` ; \
	$(MAKE) install DESTDIR="$$DESTDIR" || exit 1 ; \
	tar cvjf "$$chargedir".tar.bz2 "$$chargedir" ; \
	rm -rf "$$chargedir"

# A bare-bones monocharge.

monocharge-lite:
	chargedir=monocharge-lite-`date -u +%Y%m%d` ; \
	mkdir "$$chargedir" ; \
	DESTDIR=`cd "$$chargedir" && pwd` ; \
	$(MAKE) -C mcs install DESTDIR="$$DESTDIR" || exit 1; \
	$(MAKE) -C class/corlib install DESTDIR="$$DESTDIR" || exit 1; \
	$(MAKE) -C class/System install DESTDIR="$$DESTDIR" || exit 1; \
	$(MAKE) -C class/System.XML install DESTDIR="$$DESTDIR" || exit 1; \
	$(MAKE) -C class/Mono.CSharp.Debugger install DESTDIR="$$DESTDIR" || exit 1; \
	tar cvjf "$$chargedir".tar.bz2 "$$chargedir" ; \
	rm -rf "$$chargedir"
