CSC=mcs
CSCRIPT=$(WINDIR)/system32/cscript.exe
CSCFLAGS=/nologo /debug+ /debug:full 
INSTALL = /usr/bin/install

MONO_TOOLS = monostyle.exe GenerateDelegate.exe EnumCheck.exe IFaceDisco.exe ./SqlSharp/sqlsharp.exe secutil.exe cert2spc.exe

DIRS = 

# tools commented here because they were unable to build under linux
#MONO_TOOLS = monostyle.exe verifier.exe GenerateDelegate.exe EnumCheck.exe IFaceDisco.exe ./type-reflector/type-reflector.exe ./corcompare/CorCompare.exe ./SqlSharp/SqlSharpCli.exe

all: tools
	for i in $(DIRS) ; do \
		$(MAKE) -C $$i -f makefile.gnu $@ || exit 1; \
	done

linx: $(MONO_TOOLS)

tools: $(MONO_TOOLS)

windows: $(MONO_TOOLS)

install: all
	if test x$$prefix = x; then \
		echo Usage is: make -f makefile.gnu install prefix=YOURPREFIX; \
		exit 1; \
	fi;
	mkdir -p $(prefix)/bin/
	for i in $(MONO_TOOLS) ; do \
		($(INSTALL) -m 755 $$i $(prefix)/bin/) || exit 1; \
	done
	for i in $(DIRS) ; do \
		$(MAKE) -C $$i -f makefile.gnu $@ || exit 1; \
	done

monostyle.exe: monostyle.cs
	$(CSC) $(CSCFLAGS) monostyle.cs

GenerateDelegate.exe: GenerateDelegate.cs
	$(CSC) $(CSCFLAGS) /out:$@ $<

verifier.exe: verifier.cs
	$(CSC) $(CSCFLAGS) verifier.cs

./SqlSharp/sqlsharp.exe: dummy
	(cd SqlSharp; make CSC=$(CSC))

./corcompare/CorCompare.exe: dummy
	(cd corcompare; make CorCompare.exe)

update: ../../mono/doc/pending-classes

cormissing.xml: ./corcompare/CorCompare.exe ../class/lib/corlib_cmp.dll
	./corcompare/CorCompare.exe -x cormissing.xml -f corlib -ms mscorlib ../class/lib/corlib_cmp.dll

../../mono/doc/pending-classes: ./corcompare/cormissing.xsl cormissing.xml
	$(CSCRIPT) /nologo ./corcompare/transform.js cormissing.xml ./corcompare/cormissing.xsl > ../../mono/doc/pending-classes


EnumCheck: EnumCheck.exe

EnumCheck.exe: EnumCheck.cs EnumCheckAssemblyCollection.cs
	$(CSC) $(CSCFLAGS) /out:EnumCheck.exe EnumCheck.cs EnumCheckAssemblyCollection.cs

IFaceDisco.exe: IFaceDisco.cs XMLUtil.cs
	$(CSC) $(CSCFLAGS) /out:IFaceDisco.exe IFaceDisco.cs XMLUtil.cs

secutil.exe: secutil.cs
	$(CSC) $(CSCFLAGS) secutil.cs

cert2spc.exe: cert2spc.cs ASN1.cs
	$(CSC) $(CSCFLAGS) /out:cert2spc.exe cert2spc.cs ASN1.cs

clean:
	(cd corcompare; make clean)
	(cd SqlSharp; make clean)
	rm -f *.exe *.pdb *.dbg *.dll
	rm -f cormissing.xml
	rm -f ../../mono/doc/pending-classes.in
	for i in $(DIRS) ; do \
		$(MAKE) -C $$i -f makefile.gnu $@ || exit 1; \
	done

dummy:

test:
