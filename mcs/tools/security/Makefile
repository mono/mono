thisdir = tools/security
SUBDIRS = 
DIST_ONLY_SUBDIRS = certview
include ../../build/rules.make

LOCAL_MCS_FLAGS = /lib:$(topdir)/class/lib/$(PROFILE) -r:Mono.Security.dll

SECURITY_PROGRAMS = secutil.exe cert2spc.exe sn.exe makecert.exe chktrust.exe crlupdate.exe \
	signcode.exe setreg.exe certmgr.exe caspol.exe permview.exe mozroots.exe
SECURITY_PROGRAMS_2_0 = httpcfg.exe

HELPER_SOURCES = AssemblyInfo.cs $(topdir)/build/common/Consts.cs
SN_SOURCES = sn.cs StrongNameManager.cs $(HELPER_SOURCES)

SECURITY_SOURCES = $(HELPER_SOURCES) $(SN_SOURCES) $(SECURITY_PROGRAMS:.exe=.cs) $(SECURITY_PROGRAMS_2_0:.exe=.cs)

ifeq (net_4_0, $(PROFILE))
SECURITY_TARGETS = $(SECURITY_PROGRAMS) $(SECURITY_PROGRAMS_2_0)
endif

PROGRAM_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)

DISTFILES = README TESTS $(SECURITY_SOURCES)

ifeq (build, $(PROFILE))
all-local: $(topdir)/class/lib/$(PROFILE)/sn.exe
else
all-local: $(SECURITY_TARGETS)
endif

doc-update-local:

install-local: all-local
uninstall-local:

ifndef NO_INSTALL
install-local: $(SECURITY_TARGETS)
	$(MKINSTALLDIRS) $(DESTDIR)$(PROGRAM_INSTALL_DIR)
	for p in $(SECURITY_TARGETS) ; do \
	    $(INSTALL_BIN) $$p $(DESTDIR)$(PROGRAM_INSTALL_DIR) ; \
	    test ! -f $$p.mdb || $(INSTALL_BIN) $$p.mdb $(DESTDIR)$(PROGRAM_INSTALL_DIR) ; \
	done

uninstall-local:
	for p in $(SECURITY_TARGETS) ; do \
	    rm -f $(DESTDIR)$(PROGRAM_INSTALL_DIR)/$$p* ; \
	done
endif

test-local:

run-test-local run-test-ondotnet-local csproj-local:

clean-local:
	rm -f *.exe *.mdb *.pdb
	rm -f $(topdir)/class/lib/$(PROFILE)/sn.exe*

dist-local: dist-default

sn.exe $(topdir)/class/lib/$(PROFILE)/sn.exe: $(SN_SOURCES)
	$(CSCOMPILE) /out:$@ $(SN_SOURCES)

permview.exe: permview.cs
	$(CSCOMPILE) $^ $(HELPER_SOURCES) -r:Mono.Cecil.dll

%.exe: %.cs $(HELPER_SOURCES)
	$(CSCOMPILE) $^
