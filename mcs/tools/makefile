CSC=csc.exe
CSCRIPT=$(WINDIR)/system32/cscript.exe
CSCFLAGS=/nologo /debug+ /debug:full 

windows: linux

linux: verifier.exe EnumCheck.exe IFaceDisco.exe CorCompare.exe update

verifier.exe: verifier.cs
	$(CSC) $(CSCFLAGS) verifier.cs

CorCompare.exe:
	(cd corcompare; make CorCompare.exe)

update: CorCompare.exe
	./corcompare/CorCompare.exe -x cormissing.xml ../class/lib/corlib_cmp.dll
	./corcompare/CorCompare.exe -t ../class/lib/corlib_cmp.dll > ../../mono/doc/pending-classes.in
	$(CSCRIPT) /nologo ./corcompare/transform.js ./corcompare/cormissing.xml ./corcompare/cormissing.xsl > ../../mono/doc/pending-classes

EnumCheck: EnumCheck.exe

EnumCheck.exe: EnumCheck.cs EnumCheckAssemblyCollection.cs
	$(CSC) $(CSCFLAGS) /out:EnumCheck.exe EnumCheck.cs EnumCheckAssemblyCollection.cs

IFaceDisco.exe: IFaceDisco.cs XMLUtil.cs
	$(CSC) $(CSCFLAGS) /out:IFaceDisco.exe IFaceDisco.cs XMLUtil.cs

clean:
	(cd corcompare; make clean)
	rm -f *.exe *.pdb *.dll
	rm -f cormissing.xml
	rm -f ../../mono/doc/pending-classes.in
