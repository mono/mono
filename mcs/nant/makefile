all: nant.exe
linux: nant.exe
	-cp NAnt.exe nant.exe
windows: nant.exe

nant.exe: NAnt.exe

NAnt.exe: makefile src/*.cs src/Attributes/*.cs src/Tasks/*.cs src/Util/*.cs
	csc /out:NAnt.exe /recurse:*.cs 

NAnt_mono.exe: makefile
	csc /out:NAnt_mono.exe /nostdlib /noconfig /recurse:*.cs /lib:../class/lib /r:corlib.dll /r:System.Xml.dll /r:System.dll /nowarn:1595

clean:
	rm -f NAnt_mono.exe
