thisdir := .

SUBDIRS := build jay mcs monoresgen class mbas nunit20 ilasm tools tests errors docs
DIST_ONLY_SUBDIRS := gmcs

basic_SUBDIRS := jay mcs class
net_1_1_bootstrap_SUBDIRS := jay mcs class
net_2_0_bootstrap_SUBDIRS := class
net_2_0_SUBDIRS := jay gmcs class nunit20 tests errors tools

ifdef TEST_SUBDIRS
$(PROFILE)_SUBDIRS := $(TEST_SUBDIRS)
endif

OVERRIDE_TARGET_ALL = yes

include build/rules.make

all-recursive $(STD_TARGETS:=-recursive): platform-check profile-check

# Used when OVERRIDE_TARGET_ALL is defined
all.override:
ifndef NO_SIGN_ASSEMBLY
	$(MAKE) NO_SIGN_ASSEMBLY=yes all.real
endif
	$(MAKE) all.real

.PHONY: all-local $(STD_TARGETS:=-local)
all-local $(STD_TARGETS:=-local):
	@:

# fun specialty targets

PROFILES = default net_2_0

.PHONY: all-profiles $(STD_TARGETS:=-profiles)
all-profiles $(STD_TARGETS:=-profiles): %-profiles: profiles-do--%
	@:

profiles-do--%:
	$(MAKE) $(PROFILES:%=profile-do--%--$*)

# The % below looks like profile-name--target-name
profile-do--%:
	$(MAKE) PROFILE=$(subst --, ,$*)

profiles-do--run-test:
	ret=:; \
	$(MAKE) PROFILE=default run-test || ret=false; \
	$(MAKE) PROFILE=net_2_0 run-test && $$ret

# Orchestrate the bootstrap here.
profiles-do--all: profile-do--net_2_0--all
	@:

profile-do--net_2_0--all: profile-do--net_2_0_bootstrap--all
profile-do--net_2_0_bootstrap--all: profile-do--default--all

ifeq (linux, $(PLATFORM))
profile-do--default--all: profile-do--net_1_1_bootstrap--all
profile-do--net_1_1_bootstrap--all: profile-do--basic--all
endif

testcorlib:
	@cd class/corlib && $(MAKE) test run-test

compiler-tests:
	$(MAKE) TEST_SUBDIRS="tests errors" run-test-profiles

test-installed-compiler:
	$(MAKE) TEST_SUBDIRS="tests errors" PROFILE=default TEST_RUNTIME=mono MCS=mcs run-test
	$(MAKE) TEST_SUBDIRS="tests errors" PROFILE=net_2_0 TEST_RUNTIME=mono MCS=gmcs run-test

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
	Makefile		\
	mkinstalldirs		\
	MIT.X11			\
	MonoIcon.png		\
	README			\
	ScalableMonoIcon.svg	\
	winexe.in

dist-local: dist-default

dist-pre:
	rm -rf $(package)
	mkdir $(package)

dist-tarball: dist-pre
	$(MAKE) distdir='$(package)' dist-recursive
	tar cvzf $(package).tar.gz $(package)

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
	tar tzf $(package)/$(package).tar.gz |sed -e 's,/$$,,' |sort >distdist.list ; \
	rm $(package)/$(package).tar.gz ; \
	tar tzf $(package).tar.gz |sed -e 's,/$$,,' |sort >before.list ; \
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
	tar cvzf "$$chargedir".tgz "$$chargedir" ; \
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
	tar cvzf "$$chargedir".tgz "$$chargedir" ; \
	rm -rf "$$chargedir"
