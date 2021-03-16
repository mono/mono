thisdir = build
SUBDIRS = 
include ../build/rules.make

BUILT_FILES = common/Consts.cs $(topdir)/class/lib/$(PROFILE_DIRECTORY)/Consts.cs common/sourcelink.json

all-local install-local test-local run-test-local run-test-ondotnet-local uninstall-local doc-update-local: $(BUILT_FILES)
	@:

clean-local:
	-rm -f $(BUILT_FILES) deps/*

$(topdir)/class/lib/$(PROFILE_DIRECTORY):
	mkdir -p $@

common/Consts.cs $(topdir)/class/lib/$(PROFILE_DIRECTORY)/Consts.cs: common/Consts.cs.in $(wildcard config.make) | $(topdir)/class/lib/$(PROFILE_DIRECTORY)
	test -n '$(MONO_VERSION)'
	test -n '$(MONO_CORLIB_VERSION)'
	sed -e 's,@''MONO_VERSION@,$(MONO_VERSION),' -e 's,@''MONO_CORLIB_VERSION@,$(MONO_CORLIB_VERSION),' $< > $@

common/sourcelink.json: common/sourcelink.json.in $(wildcard config.make) | $(topdir)/class/lib/$(PROFILE_DIRECTORY)
	sed -e 's,@mono_topdir@,$(shell cd $(topdir)/.. && pwd),' -e 's,@mono_commit_hash@,$(shell cd $(topdir)/.. && git rev-parse HEAD),' -e 's,@corefx_commit_hash@,$(shell cd $(topdir)/../external/corefx && git rev-parse HEAD),' -e 's,@corert_commit_hash@,$(shell cd $(topdir)/../external/corert && git rev-parse HEAD),' -e 's,@cecil_commit_hash@,$(shell cd $(topdir)/../external/cecil && git rev-parse HEAD),' $< > $@

PLATFORMS = macos linux win32 unix
PROFILES = \
	build \
	binary_reference_assemblies \
	net_4_x \
	xbuild_12 \
	xbuild_14

COMMON_SRCS = \
	Consts.cs.in			\
	sourcelink.json.in	\
	Locale.cs			\
	MonoTODOAttribute.cs \
	basic-profile-check.cs		\
	SR.cs	\
	AssemblyRef.cs

DISTFILES = \
	README.makefiles.md		\
	README.platforms		\
	README.configury		\
	config-default.make		\
	corcompare.make			\
	corcompare-api.xsl		\
	executable.make			\
	library.make			\
	rules.make			\
	tests.make			\
	start-compiler-server.sh	\
	$(COMMON_SRCS:%=common/%)	\
	$(PLATFORMS:%=platforms/%.make) $(PROFILES:%=profiles/%.make)

dist-local: dist-default

