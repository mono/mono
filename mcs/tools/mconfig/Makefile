thisdir = tools/mconfig
SUBDIRS = 
include ../../build/rules.make

LOCAL_MCS_FLAGS = -r:System.Xml.dll
PROGRAM = mconfig.exe

BUILT_SOURCES=Mono.MonoConfig/consts.cs

Mono.MonoConfig/consts.cs: Mono.MonoConfig/consts.cs.in
	sed -e "s;@MONO_SYSCONFDIR@;$(sysconfdir);g" < $< > $@

install-local: install-local-data

install-local-data:
	$(MKINSTALLDIRS) $(DESTDIR)$(sysconfdir)/mono/mconfig/
	$(INSTALL_DATA) data/config.xml $(DESTDIR)$(sysconfdir)/mono/mconfig

EXTRA_DISTFILES = TODO data/config.xml Mono.MonoConfig/consts.cs.in

include ../../build/executable.make
