thisdir = class/Mono.Posix
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Posix.dll
LIB_MCS_FLAGS = /unsafe /r:$(corlib) /r:System.dll
NO_TEST = yes

include ../../build/library.make

all-local: Mono.Unix/make-map.exe 

Mono.Unix/make-map.exe: Mono.Unix/make-map.cs $(the_lib)
	cp $(the_lib) Mono.Unix/
ifneq ($(PLATFORM),win32)
	$(CSCOMPILE)  -out:Mono.Unix/make-map.exe -r:Mono.Posix.dll Mono.Unix/make-map.cs
else
	$(CSCOMPILE)  -out:Mono.Unix/make-map.exe -r:Mono.Posix.dll Mono.Unix\\make-map.cs
endif

