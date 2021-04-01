thisdir := .

SUBDIRS := build jay mcs class ilasm tools tests errors docs packages

# Resgen is corlib specific tool

build_SUBDIRS := build jay tools/gensources class class/aot-compiler tools
monodroid_SUBDIRS := build class tools
monodroid_tools_SUBDIRS := build class tools
monotouch_SUBDIRS := build class tools
monotouch_watch_SUBDIRS := build class
monotouch_tv_SUBDIRS := build class
monotouch_runtime_SUBDIRS := build class
monotouch_watch_runtime_SUBDIRS := build class
monotouch_tv_runtime_SUBDIRS := build class
monotouch_tools_SUBDIRS := build class tools
xammac_SUBDIRS := build class
testing_aot_full_interp_SUBDIRS := build class
testing_aot_hybrid_SUBDIRS := build class
testing_aot_full_SUBDIRS := build class
binary_reference_assemblies_SUBDIRS := build class
net_4_x_SUBDIRS := build class ilasm tools tests errors docs mcs class/aot-compiler packages
xammac_net_4_5_SUBDIRS := build class
xbuild_12_SUBDIRS := build class tools/xbuild
xbuild_14_SUBDIRS := build class tools/xbuild
winaot_SUBDIRS := build class
testing_winaot_interp_SUBDIRS := build class
orbis_SUBDIRS := build class
unreal_SUBDIRS := build class
wasm_SUBDIRS := build class
wasm_tools_SUBDIRS := build class tools
netcore_SUBDIRS := build class

include build/rules.make

all-recursive $(STD_TARGETS:=-recursive): dir-check platform-check profile-check

.PHONY: all-local $(STD_TARGETS:=-local)
all-local $(STD_TARGETS:=-local):
	@:

dir-check:
	@if [ "$(NO_DIR_CHECK)" = "" ]; then $(MAKE) -C ../runtime; fi

# fun specialty targets

PROFILES = net_4_x binary_reference_assemblies xbuild_12 xbuild_14

.PHONY: all-profiles compiler-test-profiles run-compiler-test-profiles $(STD_TARGETS:=-profiles)
all-profiles compiler-test-profiles run-compiler-test-profiles $(STD_TARGETS:=-profiles): %-profiles: profiles-do--%
	@:

profiles-do--%:
	$(if $(PROFILES),$(MAKE) $(PROFILES:%=profile-do--%--$*))

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
$(_boot_:%=profile-do--monodroid_tools--%):         profile-do--monodroid_tools--%:         profile-do--build--%
$(_boot_:%=profile-do--monotouch--%):         profile-do--monotouch--%:         profile-do--build--%
$(_boot_:%=profile-do--monotouch_watch--%):   profile-do--monotouch_watch--%:   profile-do--build--%
$(_boot_:%=profile-do--monotouch_tv--%):      profile-do--monotouch_tv--%:      profile-do--build--%
$(_boot_:%=profile-do--monotouch_runtime--%):  profile-do--monotouch_runtime--%:  profile-do--build--%
$(_boot_:%=profile-do--monotouch_watch_runtime--%):  profile-do--monotouch_watch_runtime--%:  profile-do--build--%
$(_boot_:%=profile-do--monotouch_tv_runtime--%):     profile-do--monotouch_tv_runtime--%:     profile-do--build--%
$(_boot_:%=profile-do--monotouch_tools--%):         profile-do--monotouch_tools--%:         profile-do--build--%
$(_boot_:%=profile-do--xammac--%):            profile-do--xammac--%:            profile-do--build--%
$(_boot_:%=profile-do--xammac_net_4_5--%):    profile-do--xammac_net_4_5--%:           profile-do--build--%
$(_boot_:%=profile-do--testing_aot_full_interp--%):        profile-do--testing_aot_full_interp--%:        profile-do--build--%
$(_boot_:%=profile-do--testing_aot_hybrid--%):        profile-do--testing_aot_hybrid--%:        profile-do--build--%
$(_boot_:%=profile-do--testing_aot_full--%):          profile-do--testing_aot_full--%:          profile-do--build--%
$(_boot_:%=profile-do--winaot--%):              profile-do--winaot--%:              profile-do--build--%
$(_boot_:%=profile-do--testing_winaot_interp--%):     profile-do--testing_winaot_interp--%:              profile-do--build--%
$(_boot_:%=profile-do--orbis--%):              profile-do--orbis--%:              profile-do--build--%
$(_boot_:%=profile-do--unreal--%):              profile-do--unreal--%:              profile-do--build--%
$(_boot_:%=profile-do--wasm--%):              profile-do--wasm--%:              profile-do--build--%
$(_boot_:%=profile-do--wasm_tools--%):         profile-do--wasm_tools--%:         profile-do--build--%

compiler-test:
	$(MAKE) -C tests test
	$(MAKE) -C errors test

run-compiler-test:
	$(MAKE) -C tests run-test
	$(MAKE) -C errors run-test

package := mcs-$(VERSION)

DISTFILES = \
	AUTHORS			\
	COPYING			\
	INSTALL.txt		\
	Makefile		\
	mkinstalldirs		\
	MonoIcon.png		\
	README			\
	ScalableMonoIcon.svg

dist-local: dist-default

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

# Targets for creating API diffs of the framework

MONO_API_SNAPSHOT_PATH := $(topdir)../external/api-snapshot/
GENAPI := $(MONO_API_SNAPSHOT_PATH)tools/genapi/Microsoft.DotNet.GenAPI.exe
MONO_API_SNAPSHOT_PROFILE_PATH := $(MONO_API_SNAPSHOT_PATH)profiles/$(PROFILE)/
MONO_API_ASSEMBLIES_IGNORED := $(addprefix $(topdir)class/lib/$(PROFILE)/, Mono.CSharp.dll SystemWebTestShim.dll standalone-runner-support.dll nunitlite.dll Mono.Profiler.Log.dll Mono.Debugger.Soft.dll)
MONO_API_ASSEMBLIES := $(filter-out $(MONO_API_ASSEMBLIES_IGNORED), $(wildcard $(topdir)class/lib/$(PROFILE)/*.dll)) $(wildcard $(topdir)class/lib/$(PROFILE)/Facades/*.dll)
MONO_API_ASSEMBLIES_CS := $(MONO_API_ASSEMBLIES:$(topdir)class/lib/$(PROFILE)/%.dll=$(MONO_API_SNAPSHOT_PROFILE_PATH)%.cs)

apidiff/ignored-attr.txt:
	@mkdir -p $(dir $@)
	@echo "T:System.Runtime.CompilerServices.CompilerGeneratedAttribute" > $@

$(MONO_API_SNAPSHOT_PROFILE_PATH)%.cs: $(topdir)class/lib/$(PROFILE)/%.dll $(GENAPI) $(MONO_API_SNAPSHOT_PATH)profiles/license-header.txt apidiff/ignored-attr.txt
	@mkdir -p $(dir $@)
	$(Q) MONO_PATH=$(topdir)class/lib/$(BUILD_TOOLS_PROFILE) $(RUNTIME) $(GENAPI) -libPath:$(topdir)class/lib/$(PROFILE),$(topdir)class/lib/$(PROFILE)/Facades -out:$(dir $@) -headerFile:$(MONO_API_SNAPSHOT_PATH)profiles/license-header.txt -assemblyAttributes -typeForwardedTo -assemblyVersion -excludeAttributesList:apidiff/ignored-attr.txt -assembly:$< || echo "Couldn't process assembly." > $@

mono-api-current: $(MONO_API_ASSEMBLIES_CS)

mono-api-current-clean:
	$(Q) find "$(MONO_API_SNAPSHOT_PROFILE_PATH)" -name "*.cs" -type f -delete

mono-api-diff:
	@echo "Regenerating API snapshot..."
	$(Q) $(MAKE) mono-api-current-clean PROFILE=net_4_x
	$(Q) $(MAKE) mono-api-current-clean PROFILE=monotouch
	$(Q) $(MAKE) mono-api-current-clean PROFILE=monodroid
	$(Q) $(MAKE) mono-api-current PROFILE=net_4_x
	$(Q) $(MAKE) mono-api-current PROFILE=monotouch
	$(Q) $(MAKE) mono-api-current PROFILE=monodroid
	@echo "Checking public API differences..."
	$(Q) cd $(MONO_API_SNAPSHOT_PATH); git add -A .
	$(Q) cd $(MONO_API_SNAPSHOT_PATH); git diff --no-renames HEAD > $(abspath $(topdir))/temp.patch
	@mkdir -p apidiff
	$(Q) sed -e "/@diffdata@/r temp.patch" -e "/@diffdata@/d" -e "s/@title@/Public API Diff/g" -e "s/@description@/If the changes are intentional, run <code>make -C mcs mono-api-diff<\/code> locally and commit changes in external\/api-snapshot./g" diff.html.in > apidiff/index.html
	$(Q) if [ -s temp.patch ]; then echo "Error: Found public API differences, see mcs/apidiff/index.html. If the changes are intentional, please go to external/api-snapshot and commit them."; rm -f temp.patch; exit 1; else echo "No differences found."; rm -f temp.patch; fi
