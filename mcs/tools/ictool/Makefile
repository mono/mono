CSC=csc.exe
CSCFLAGS=/nologo /debug+ /debug:full 

all: ictool.exe

ictool.exe: ictool.cs depgraph.cs peer.cs
	$(CSC) $(CSCFLAGS) /out:ictool.exe ictool.cs depgraph.cs peer.cs

clean:
	rm -f *.exe *.pdb *.dll
