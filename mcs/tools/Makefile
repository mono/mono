thisdir = tools
SUBDIRS = cilc corcompare ictool mono-xsd security
DIST_ONLY_SUBDIRS = SqlSharp
include ../build/rules.make

MONO_TOOLS = \
	monostyle.exe 		\
	GenerateDelegate.exe 	\
	EnumCheck.exe 		\
	IFaceDisco.exe

DISTFILES = \
	assemblies.xml			\
	DumpCultureInfo.cs		\
	EnumCheckAssemblyCollection.cs	\
	EnumCheck.cs			\
	GenerateDelegate.cs		\
	IFaceDisco.cs			\
	monostyle.cs			\
	mono-win32-setup-dark.bmp	\
	mono-win32-setup-light.bmp	\
	mono-win32-setup.nsi		\
	sample_cast_const.cs		\
	scan-tests.pl			\
	serialize.cs			\
	verifier.cs			\
	XMLUtil.cs			\
	tinderbox/smtp.c		\
	tinderbox/tinderbox.sh

all-local: $(MONO_TOOLS)

install-local: all-local
	$(MKINSTALLDIRS) $(DESTDIR)$(prefix)/bin
	for t in $(MONO_TOOLS) ; do \
	   $(INSTALL_BIN) $$t $(DESTDIR)$(prefix)/bin ; \
	done

test-local run-test-local:

clean-local:
	rm -f *.exe *.dll *.pdb cormissing.xml

dist-local: dist-default

%.exe: %.cs
	$(CSCOMPILE) /target:exe /out:$@ $^

EnumCheck.exe: EnumCheck.cs EnumCheckAssemblyCollection.cs
	$(CSCOMPILE) /target:exe /out:$@ $^

IFaceDisco.exe: IFaceDisco.cs XMLUtil.cs
	$(CSCOMPILE) /target:exe /out:$@ $^

