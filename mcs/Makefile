thisdir := .
SUBDIRS := build jay mcs monoresgen class mbas nunit20 ilasm tools tests errors docs
OVERRIDE_TARGET_ALL = yes
include build/rules.make

all-recursive $(STD_TARGETS:=-recursive): platform-check profile-check

all.override:
	$(MAKE) NO_SIGN_ASSEMBLY=yes all.real
	$(MAKE) all.real

all-local $(STD_TARGETS:=-local):
	@:

# fun specialty targets

all-profiles:
	$(MAKE) PROFILE=default all
	$(MAKE) PROFILE=net_2_0 all

clean-profiles:
	$(MAKE) PROFILE=default clean
	$(MAKE) PROFILE=net_2_0 clean

testcorlib:
	@cd class/corlib && $(MAKE) test run-test

test-compiler:
	$(MAKE) -C tests test-everything
	$(MAKE) -C errors test-everything

# Disting. We need to override $(distdir) here.

package := mcs-$(VERSION)
top_distdir = $(dots)/$(package)
distdir = $(top_distdir)
export package

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

dist-pre:
	rm -rf $(package)

dist-post:
	tar cvzf $(package).tar.gz $(package)

dist-local: dist-default

dist-tarball: dist-pre dist-recursive dist-post

dist: dist-tarball
	rm -rf $(package)

# the egrep -v is kind of a hack (to get rid of the makefrags)
# but otherwise we have to make dist then make clean which
# is sort of not kosher. And it breaks with DIST_ONLY_SUBDIRS.
#
# We need to set prefix on make so class/System/Makefile can find
# the installed System.Xml to build properly

distcheck:
	rm -rf InstallTest Distcheck-MCS ; \
	mkdir InstallTest ; \
	destdir=`cd InstallTest && pwd` ; \
	$(MAKE) dist-tarball || exit 1 ; \
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
