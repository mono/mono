CSC=csc.exe
CSCRIPT=$(WINDIR)/system32/cscript.exe
CSCFLAGS=/nologo /debug+ /debug:full 

windows: linux

linux: monostyle.exe verifier.exe GenerateDelegate.exe EnumCheck.exe IFaceDisco.exe ./type-reflector/type-reflector.exe ./corcompare/CorCompare.exe ./SqlSharp/sqlsharp.exe secutil.exe Cert2Spc.exe
#linux: verifier.exe GenerateDelegate.exe EnumCheck.exe IFaceDisco.exe ./corcompare/CorCompare.exe update

monostyle.exe: monostyle.cs
	$(CSC) $(CSCFLAGS) monostyle.cs

GenerateDelegate.exe: GenerateDelegate.cs
	$(CSC) $(CSCFLAGS) /out:$@ $<

verifier.exe: verifier.cs
	$(CSC) $(CSCFLAGS) verifier.cs

./type-reflector/type-reflector.exe: dummy
	(cd type-reflector; make CSC=$(CSC))

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

Cert2Spc.exe: cert2spc.cs ASN1.cs
	$(CSC) $(CSCFLAGS) /out:Cert2Spc.exe cert2spc.cs ASN1.cs

clean:
	(cd corcompare; make clean)
	(cd type-reflector; make clean)
	(cd SqlSharp; make clean)
	rm -f *.exe *.pdb *.dbg *.dll
	rm -f cormissing.xml
	rm -f ../../mono/doc/pending-classes.in

dummy:
