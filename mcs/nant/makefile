all: nant.exe
linux: nant.exe
windows: nant.exe

nant.exe: NAnt.exe
	cp NAnt.exe nant.exe

NAnt.exe: makefile
	csc /out:NAnt.exe /recurse:*.cs 