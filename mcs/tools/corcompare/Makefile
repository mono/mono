all: linux

windows: linux

linux: CorCompare.exe

CorCompare.exe: *.cs
	../../nant/NAnt.exe linux

clean:
	rm -f *.exe *.pdb *.dll
