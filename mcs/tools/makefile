ROOT=/cygdrive/$(subst \,/,$(subst :\,/,$(SYSTEMROOT)))
CSC=$(ROOT)/microsoft.net/framework/v1.0.2914/csc.exe
CSCFLAGS=/nologo /debug+ /debug:full 

windows: verifier.exe CorCompare.exe EnumCheck.exe todo.exe

linux: verifier.exe CorCompare.exe EnumCheck.exe todo.exe

verifier.exe: verifier.cs
	$(CSC) $(CSCFLAGS) verifier.cs

todo.exe:
	$(CSC) $(CSCFLAGS) /out:todo.exe todo.cs

CorCompare.exe: CorCompare.cs
	$(CSC) $(CSCFLAGS) CorCompare.cs
	./CorCompare.exe ../class/lib/corlib_cmp.dll > ../../mono/status/cormissing.xml

EnumCheck.exe:
	$(CSC) $(CSCFLAGS) /out:EnumCheck.exe EnumCheck.cs

clean:
	rm -f *.exe *.pdb
	rm -f ../../mono/status/cormissing.xml
