CSC=csc.exe
CSCFLAGS=/nologo /debug+ /debug:full 

windows: verifier.exe CorCompare.exe EnumCheck.exe IFaceDisco.exe

linux: verifier.exe CorCompare.exe EnumCheck.exe IFaceDisco.exe update

verifier.exe: verifier.cs
	$(CSC) $(CSCFLAGS) verifier.cs

CorCompare.exe: CorCompare.cs
	$(CSC) $(CSCFLAGS) CorCompare.cs XMLUtil.cs

update: CorCompare.exe
	./CorCompare.exe ../class/lib/corlib_cmp.dll > ../../mono/status/cormissing.xml
	./CorCompare.exe -t ../class/lib/corlib_cmp.dll > ../../mono/doc/pending-classes.in

EnumCheck.exe:
	$(CSC) $(CSCFLAGS) /out:EnumCheck.exe EnumCheck.cs

IFaceDisco.exe: IFaceDisco.cs XMLUtil.cs
	$(CSC) $(CSCFLAGS) /out:IFaceDisco.exe IFaceDisco.cs XMLUtil.cs

clean:
	rm -f *.exe *.pdb
	rm -f ../../mono/status/cormissing.xml
