all: NUnitConsole.exe
linux: NUnitConsole.exe
windows: NUnitConsole.exe

NUnitConsole.exe: NUnitCore.dll
	(cd src/NUnitConsole; csc /nologo /target:exe /out:../../NUnitConsole.exe *.cs /r:../../NUnitCore.dll)

NUnitCore.dll:
	(cd src/NUnitCore; csc /nologo /target:library /out:../../NUnitCore.dll *.cs)

clean:
	rm -f NUnitCore.dll NUnitConsole.exe
