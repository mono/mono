NANT	= ../../nant/NAnt.exe

all: linux

windows: linux

linux: CorCompare.exe

CorCompare.exe: *.cs
	$(NANT) linux

clean:
	rm -f *.exe *.pdb *.dll
