CSC=mcs


all: Mono.Globalization.dll

Mono.Globalization.dll: write-source.exe
	mcs src/*.cs /t:library /out:Mono.Globalization.dll

write-source: write-source.cs
	mcs write-source.cs

write-source.exe: write-source
	mono write-source.exe