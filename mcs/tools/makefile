CSC=csc.exe
CSCFLAGS=/nologo /debug+ /debug:full 

windows: verifier.exe CorCompare.exe EnumCheck.exe IFaceDisco.exe

linux: verifier.exe CorCompare.exe EnumCheck.exe IFaceDisco.exe

verifier.exe: verifier.cs
	$(CSC) $(CSCFLAGS) verifier.cs

CorCompare.exe: CorCompare.cs
	$(CSC) $(CSCFLAGS) CorCompare.cs
#	./CorCompare.exe ../class/lib/corlib_cmp.dll > ../../mono/status/cormissing.xml

EnumCheck.exe:
	$(CSC) $(CSCFLAGS) /out:EnumCheck.exe EnumCheck.cs

IFaceDisco.exe: IFaceDisco.cs XMLUtil.cs
	$(CSC) $(CSCFLAGS) /out:IFaceDisco.exe IFaceDisco.cs XMLUtil.cs

clean:
	rm -f *.exe *.pdb
	rm -f ../../mono/status/cormissing.xml
