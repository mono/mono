NONO_PATH ?= /inst/m6/lib

all: writefiles hashfile.exe

clean:
	-rm writefiles hashfile.exe

writefiles: writefiles.cpp
	g++ writefiles.cpp -o $@

hashfile.exe: hashfile.cs
	mcs hashfile.cs /r:$(NONO_PATH)/mono/4.7.1-api/Mono.Security.dll
