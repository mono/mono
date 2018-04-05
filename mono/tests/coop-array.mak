

CSC=csc
ilasm=ilasm

all: array-coop-bigvt.exe.dylib array-coop-smallvt.exe.dylib array-coop-int.exe.dylib

clean:
	-rm array-coop-bigvt.exe.dylib array-coop-bigvt.exe array-coop-int.exe.dylib array-coop-int.exe array-coop-smallvt.exe.dylib array-coop-smallvt.exe 

array-coop-bigvt.exe.dylib: array-coop-bigvt.exe
	$(MONO) --aot=full array-coop-bigvt.exe

array-coop-smallvt.exe.dylib: array-coop-smallvt.exe
	$(MONO) --aot=full array-coop-smallvt.exe

array-coop-int.exe.dylib: array-coop-int.exe
	$(MONO) --aot=full array-coop-int.exe

array-coop-bigvt.exe: array-coop-bigvt.cs
	MONO_PATH= csc array-coop-bigvt.cs

array-coop-smallvt.exe: array-coop-smallvt.cs
	MONO_PATH= csc array-coop-smallvt.cs

array-coop-int.exe: array-coop-int.cs
	MONO_PATH= csc array-coop-int.cs
