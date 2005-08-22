thisdir = build
SUBDIRS = 
include ../build/rules.make

all-local install-local test-local run-test-local run-test-ondotnet-local uninstall-local:
	@:

clean-local:
	-rm -f deps/*

PLATFORMS = linux win32
PROFILES = \
	basic net_1_1_bootstrap default net_2_0_bootstrap net_2_0 \
	atomic net_1_0

COMMON_SRCS = \
	Consts.cs			\
	Locale.cs			\
	MonoTODOAttribute.cs		\
	MonoVersion.cs			\
	TestConfiguration.cs

DISTFILES = \
	README.makefiles		\
	README.platforms		\
	README.configury		\
	config-default.make		\
	executable.make			\
	library.make			\
	rules.make			\
	$(COMMON_SRCS:%=common/%)	\
	$(PLATFORMS:%=platforms/%.make) $(PROFILES:%=profiles/%.make)

dist-local: dist-default
	$(MKINSTALLDIRS) $(distdir)/deps
