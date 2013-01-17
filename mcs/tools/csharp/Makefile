thisdir = tools/csharp
SUBDIRS = 
mono_sourcelibs_DIR  = $(DESTDIR)$(mono_libdir)/mono-source-libs

include ../../build/rules.make

// 3021: CLS attribute not needed since assembly is not CLS compliant
NOWARNS = -nowarn:3021
LOCAL_MCS_FLAGS = -r:$(topdir)/class/lib/$(PROFILE)/Mono.CSharp.dll -r:$(topdir)/class/lib/$(PROFILE)/Mono.Posix.dll -r:Mono.Management.dll -unsafe $(NOWARNS)

PROGRAM = csharp.exe

DISTFILES = repl.txt

CLEAN_FILES = csharp.exe *.mdb

include ../../build/executable.make

install-local: install-source

install-source:
	-$(MKINSTALLDIRS) $(mono_sourcelibs_DIR)
	$(INSTALL) -m 644 getline.cs $(mono_sourcelibs_DIR)

