thisdir = build
SUBDIRS = 
include ../build/rules.make

BUILT_FILES = common/Consts.cs

all-local install-local test-local run-test-local csproj-local run-test-ondotnet-local uninstall-local doc-update-local: $(BUILT_FILES)
	@:

clean-local:
	-rm -f $(BUILT_FILES) deps/*

common/Consts.cs: common/Consts.cs.in $(wildcard config.make)
	test -n '$(MONO_VERSION)'
	sed -e 's,@''MONO_VERSION@,$(MONO_VERSION),' $< > $@

PLATFORMS = darwin linux win32
PROFILES = \
	basic \
	build \
	net_2_0 \
	moonlight_raw \
	net_3_5	\
	net_4_0	

COMMON_SRCS = \
	Consts.cs.in			\
	Locale.cs			\
	MonoTODOAttribute.cs \
	basic-profile-check.cs

DISTFILES = \
	README.makefiles		\
	README.platforms		\
	README.configury		\
	config-default.make		\
	corcompare.make			\
	corcompare-api.xsl		\
	executable.make			\
	gensources.sh			\
	library.make			\
	nunit-summary.xsl		\
	rules.make			\
	tests.make			\
	$(COMMON_SRCS:%=common/%)	\
	$(PLATFORMS:%=platforms/%.make) $(PROFILES:%=profiles/%.make)

dist-local: dist-default

