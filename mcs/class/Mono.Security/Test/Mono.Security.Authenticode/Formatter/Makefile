MONO_PATH ?= /inst/m6/lib

.NOISY:

all: writefiles hashfile.exe

clean:
	rm -f writefiles hashfile.exe

test check run: all
	./writefiles
	set -e
	set -x
	for a in *.dll ; do \
	 	echo mono ./hashfile.exe $$a; \
	 	mono ./hashfile.exe $$a; \
	 done

hashfile.exe: hashfile.cs
	mcs hashfile.cs /r:$(MONO_PATH)/mono/4.7.1-api/Mono.Security.dll
