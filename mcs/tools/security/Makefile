thisdir = tools/security
SUBDIRS = 
DIST_ONLY_SUBDIRS = certview
include ../../build/rules.make

LOCAL_MCS_FLAGS = /lib:../class/lib/default /r:Mono.Security.dll

SECURITY_PROGRAMS = secutil.exe cert2spc.exe sn.exe MakeCert.exe chktrust.exe signcode.exe setreg.exe certmgr.exe
PROGRAM_INSTALL_DIR = $(prefix)/bin

DISTFILES = \
	AssemblyInfo.cs	\
	cert2spc.cs	\
	MakeCert.cs	\
	README		\
	secutil.cs	\
	sn.cs		\
	chktrust.cs	\
	signcode.cs	\
	setreg.cs	\
	certmgr.cs	\
	StrongNameManager.cs

all-local: $(SECURITY_PROGRAMS)


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
	rm -f *.exe *.pdb

dist-local: dist-default

sn.exe: sn.cs StrongNameManager.cs AssemblyInfo.cs
	$(CSCOMPILE) $^

%.exe: %.cs AssemblyInfo.cs
	$(CSCOMPILE) $^
