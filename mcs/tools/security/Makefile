thisdir = tools/security
SUBDIRS = 
DIST_ONLY_SUBDIRS = certview
include ../../build/rules.make

LOCAL_MCS_FLAGS = /lib:$(topdir)/class/lib/$(PROFILE) /r:Mono.Security.dll

SECURITY_PROGRAMS = secutil.exe cert2spc.exe sn.exe MakeCert.exe chktrust.exe signcode.exe setreg.exe certmgr.exe
SECURITY_SOURCES = AssemblyInfo.cs StrongNameManager.cs $(SECURITY_PROGRAMS:.exe=.cs)

PROGRAM_INSTALL_DIR = $(prefix)/lib/mono/$(FRAMEWORK_VERSION)

DISTFILES = README $(SECURITY_SOURCES)

ifeq (net_1_1_bootstrap, $(PROFILE))
all-local: $(topdir)/class/lib/$(PROFILE)/sn.exe
else
all-local: $(SECURITY_PROGRAMS)
endif

install-local: $(SECURITY_PROGRAMS)
	$(MKINSTALLDIRS) $(DESTDIR)$(PROGRAM_INSTALL_DIR)
	for p in $(SECURITY_PROGRAMS) ; do \
	    $(INSTALL_BIN) $$p $(DESTDIR)$(PROGRAM_INSTALL_DIR) ; \
	    $(INSTALL_BIN) $$p.mdb $(DESTDIR)$(PROGRAM_INSTALL_DIR) ; \
	done

uninstall-local:
	for p in $(SECURITY_PROGRAMS) ; do \
	    rm -f $(DESTDIR)$(PROGRAM_INSTALL_DIR)/$$p* ; \
	done

test-local:

run-test-local:

clean-local:
	rm -f *.exe *.mdb *.pdb
	rm -f $(topdir)/class/lib/$(PROFILE)/sn.exe*

dist-local: dist-default

sn_sources = sn.cs StrongNameManager.cs AssemblyInfo.cs
sn.exe $(topdir)/class/lib/$(PROFILE)/sn.exe: $(sn_sources)
	$(CSCOMPILE) /out:$@ $(sn_sources)

%.exe: %.cs AssemblyInfo.cs
	$(CSCOMPILE) $^
