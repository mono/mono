RUNTIME=mono
CSC = $(RUNTIME) ../../mcs/mcs.exe
CSCFLAGS=/nologo /debug+ /debug:full /out:$@
SECFLAGS=/r:../../class/lib/Mono.Security.dll

windows: secutil.exe cert2spc.exe sn.exe makecert.exe

linux: secutil.exe

cert2spc.exe: cert2spc.cs AssemblyInfo.cs
	$(CSC) $(CSCFLAGS) $(SECFLAGS) cert2spc.cs AssemblyInfo.cs

chktrust.exe: chktrust.cs AssemblyInfo.cs
	$(CSC) $(CSCFLAGS) $(SECFLAGS) chktrust.cs AssemblyInfo.cs 

makecert.exe: MakeCert.cs AssemblyInfo.cs
	$(CSC) $(CSCFLAGS) $(SECFLAGS) MakeCert.cs AssemblyInfo.cs 

secutil.exe: secutil.cs AssemblyInfo.cs
	$(CSC) $(CSCFLAGS) secutil.cs AssemblyInfo.cs 

signcode.exe: signcode.cs AssemblyInfo.cs
	$(CSC) $(CSCFLAGS) $(SECFLAGS) signcode.cs AssemblyInfo.cs 

sn.exe: sn.cs AssemblyInfo.cs
	$(CSC) $(CSCFLAGS) $(SECFLAGS) sn.cs AssemblyInfo.cs

clean:
	rm -f *.exe *.pdb *.dbg *.dll

dummy:
