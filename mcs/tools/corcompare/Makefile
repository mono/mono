all: linux

windows: linux

linux: CorCompare.exe

CorCompare.exe:
	../../nant/NAnt.exe linux
	./CorCompare.exe -x cormissing.xml ../../class/lib/corlib_cmp.dll

clean:
	rm -f *.exe *.pdb *.dll
	rm -f cormissing.xml
