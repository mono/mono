thisdir = build
SUBDIRS = 
include ../build/rules.make

all-local install-local test-local run-test-local run-test-ondotnet-local clean-local uninstall-local:

DISTFILES = \
	README.makefiles		\
	README.platforms		\
	README.configury		\
	config-default.make		\
	executable.make			\
	library.make			\
	rules.make			\
	platforms/linux.make		\
	platforms/win32.make		\
	profiles/default.make		\
	profiles/atomic.make		\
	profiles/net_1_0.make

dist-local: dist-default
	$(MKINSTALLDIRS) $(distdir)/deps
