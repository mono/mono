CSC=mcs
MONO=mono

all: Mono.Globalization.dll

Mono.Globalization.dll: write-source.exe
	$(CSC) /t:library MonoCultures.cs /out:Mono.Globalization.dll

write-source.exe: write-source
	$(MONO) --debug write-source.exe

write-source: write-source.cs
	$(CSC) /debug write-source.cs
