ROOT=//$(subst \,/,$(subst :\,/,$(SYSTEMROOT)))
CSC=$(ROOT)/microsoft.net/framework/v1.0.2914/csc.exe
CSCFLAGS=/nologo /debug+ /debug:full 

windows: verifier.exe

verifier.exe: verifier.cs
	$(CSC) $(CSCFLAGS) verifier.cs
