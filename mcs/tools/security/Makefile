thisdir = tools/security
SUBDIRS = 
DIST_ONLY_SUBDIRS = certview
include ../../build/rules.make

LOCAL_MCS_FLAGS = /r:$(topdir)/class/lib/Mono.Security.dll

SECURITY_PROGRAMS = secutil.exe cert2spc.exe sn.exe MakeCert.exe

DISTFILES = \
	AssemblyInfo.cs	\
	cert2spc.cs	\
	MakeCert.cs	\
	README		\
	secutil.cs	\
	sn.cs

all-local: $(SECURITY_PROGRAMS)

install-local: $(SECURITY_PROGRAMS)
	$(MKINSTALLDIRS) $(DESTDIR)$(prefix)/bin
	for p in $(SECURITY_PROGRAMS) ; do \
	    $(INSTALL_BIN) $$p $(DESTDIR)$(prefix)/bin ; \
	done

test-local:

run-test-local:

clean-local:
	rm -f *.exe *.pdb

dist-local: dist-default

%.exe: %.cs AssemblyInfo.cs
	$(CSCOMPILE) $^
