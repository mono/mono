thisdir := .

SUBDIRS := build jay mcs class nunit24 ilasm tools tests errors docs

basic_SUBDIRS := build jay mcs class tools
net_2_0_SUBDIRS := build class nunit24 ilasm tools tests errors
moonlight_raw_SUBDIRS := build class tools
moonlight_SUBDIRS := tools
monodroid_SUBDIRS := build class
monotouch_SUBDIRS := build class
net_3_5_SUBDIRS := build class tools/xbuild
net_4_0_SUBDIRS := build class nunit24 ilasm tools tests errors docs

# List of test subdirs that should pass 100%
centum_tests := \
	class/corlib						\
	class/System						\
	class/System.XML					\
	class/Commons.Xml.Relaxng				\
	class/Cscompmgd						\
	class/Microsoft.JScript					\
	class/Mono.Posix					\
	class/Mono.Security					\
	class/System.Design					\
	class/System.DirectoryServices		\
	class/System.Drawing				\
	class/System.Runtime.Remoting				\
	class/System.Runtime.Serialization.Formatters.Soap	\
	class/System.Security					\
	class/System.ServiceProcess				\
	class/System.Web						\
	class/System.Web.Services				\
	tests							\
	errors

default_centum_tests := \
	$(centum_tests) 	\
	class/System.Data

net_2_0_centum_tests := \
	$(centum_tests)				\
	class/System.Core			\
	class/System.Configuration		\
	class/System.Data.DataSetExtensions	\
	class/System.Xml.Linq			\
	class/System.Transactions		\
	class/Microsoft.Build.Framework		\
	class/Microsoft.Build.Utilities		\
	class/Microsoft.Build.Engine		\
	class/System.Runtime.Serialization	\
	class/System.IdentityModel		\
	class/System.ServiceModel		\
	class/System.ServiceModel.Web		\
	class/Mono.C5

#	class/System.Web.Extensions
#	class/Microsoft.Build.Tasks	

# note that System.Xml.Linq is here to prevent this from being empty
net_3_5_centum_tests := \
	class/System.Xml.Linq

ifdef ONLY_CENTUM_TESTS
TEST_SUBDIRS := $($(PROFILE)_centum_tests)
endif

ifdef TEST_SUBDIRS
$(PROFILE)_SUBDIRS := $(TEST_SUBDIRS)
endif

include build/rules.make

all-recursive $(STD_TARGETS:=-recursive): dir-check platform-check profile-check

.PHONY: all-local $(STD_TARGETS:=-local)
all-local $(STD_TARGETS:=-local):
	@:

dir-check:
	@if [ "$(NO_DIR_CHECK)" = "" -a "$(PROFILE)" != "basic" ]; then echo "Running make from the mcs directory is not supported. Run make from the parent directory."; exit 1; fi

# fun specialty targets

PROFILES = net_2_0 net_3_5 net_4_0

.PHONY: all-profiles $(STD_TARGETS:=-profiles)
all-profiles $(STD_TARGETS:=-profiles): %-profiles: profiles-do--%
	@:

profiles-do--%:
	$(MAKE) $(PROFILES:%=profile-do--%--$*)

# The % below looks like profile-name--target-name
profile-do--%:
	$(MAKE) PROFILE=$(subst --, ,$*)

# We don't want to run the tests in parallel.  We want behaviour like -k.
profiles-do--run-test:
	ret=:; $(foreach p,$(PROFILES), { $(MAKE) PROFILE=$(p) run-test || ret=false; }; ) $$ret

# Orchestrate the bootstrap here.
_boot_ = all clean install
$(_boot_:%=profile-do--net_4_0--%):           profile-do--net_4_0--%:           profile-do--basic--%
$(_boot_:%=profile-do--net_3_5--%):           profile-do--net_3_5--%:           profile-do--net_2_0--%
$(_boot_:%=profile-do--moonlight--%):         profile-do--moonlight--%:         profile-do--moonlight_raw--%
$(_boot_:%=profile-do--monodroid--%):         profile-do--monodroid--%:         profile-do--basic--%
$(_boot_:%=profile-do--monotouch--%):         profile-do--monotouch--%:         profile-do--basic--%
$(_boot_:%=profile-do--moonlight_raw--%):     profile-do--moonlight_raw--%:     profile-do--basic--%
$(_boot_:%=profile-do--net_2_0--%):           profile-do--net_2_0--%:           profile-do--basic--%

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
