thisdir := .
SUBDIRS := build jay mcs class mbas nunit20 monoresgen ilasm tools tests errors docs
include build/rules.make

all-local: platform-check

install-local: platform-check

test-local: platform-check

clean-local:

dist-local:

# fun specialty targets

testcorlib:
	@cd class/corlib && $(MAKE) test run-test

bootstrap:
	$(MAKE) all MCS='$$(INTERNAL_MCS)'

unbootstrap:
	$(MAKE) all MCS='$$(BOOTSTRAP_MCS)'

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
	README.building		\
	ScalableMonoIcon.svg	\
	winexe.in

dist-pre:
	rm -rf $(package)

dist-post:
	tar cvzf $(package).tar.gz $(package)

dist-tarball: dist-pre dist-default dist-recursive dist-post

dist: dist-tarball
	rm -rf $(package)

# the egrep -v is kind of a hack (to get rid of the makefrags)
# but otherwise we have to make dist then make clean which
# is sort of not kosher. And it breaks with DIST_ONLY_SUBDIRS.

distcheck:
	rm -rf InstallTest Distcheck-MCS ; \
	mkdir InstallTest ; \
	destdir=`cd InstallTest && pwd` ; \
	$(MAKE) dist-tarball || exit 1 ; \
	mv $(package) Distcheck-MCS ; \
	(cd Distcheck-MCS && \
	    make && make test && make install DESTDIR="$$destdir" && \
	    make clean && make dist || exit 1) || exit 1 ; \
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
