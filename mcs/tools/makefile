CSC=csc.exe
CSCFLAGS=/nologo /debug+ /debug:full 

windows: verifier.exe CorCompare.exe EnumCheck IFaceDisco.exe

linux: verifier.exe CorCompare.exe EnumCheck.exe IFaceDisco.exe update

verifier.exe: verifier.cs
	$(CSC) $(CSCFLAGS) verifier.cs

CorCompare.exe: CorCompare.cs
	$(CSC) $(CSCFLAGS) CorCompare.cs XMLUtil.cs

update: CorCompare.exe
	./CorCompare.exe -x cormissing.xml ../class/lib/corlib_cmp.dll
	./CorCompare.exe -t ../class/lib/corlib_cmp.dll > ../../mono/doc/pending-classes.in

EnumCheck: EnumCheck.exe

EnumCheck.exe: EnumCheck.cs EnumCheckAssemblyCollection.cs
	$(CSC) $(CSCFLAGS) /out:EnumCheck.exe EnumCheck.cs EnumCheckAssemblyCollection.cs

IFaceDisco.exe: IFaceDisco.cs XMLUtil.cs
	$(CSC) $(CSCFLAGS) /out:IFaceDisco.exe IFaceDisco.cs XMLUtil.cs

clean:
	rm -f *.exe *.pdb *.dll
	rm -f cormissing.xml
	rm -f ../../mono/doc/pending-classes.in
