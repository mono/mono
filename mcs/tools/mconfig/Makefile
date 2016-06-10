thisdir = tools/mconfig
SUBDIRS = 
include ../../build/rules.make

LOCAL_MCS_FLAGS =
LIB_REFS = System.Xml System
PROGRAM = mconfig.exe

install-local: install-local-data

install-local-data:
	$(MKINSTALLDIRS) $(DESTDIR)$(sysconfdir)/mono/mconfig/
	$(INSTALL_DATA) data/config.xml $(DESTDIR)$(sysconfdir)/mono/mconfig

EXTRA_DISTFILES = TODO data/config.xml 

include ../../build/executable.make
