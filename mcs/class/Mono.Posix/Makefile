thisdir = class/Mono.Posix
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Posix.dll
# Don't warn about [Obsolete] members, as there are now *lots* of [Obsolete]
# members, generating volumes of output.
LIB_MCS_FLAGS = /unsafe /r:$(corlib) /r:System.dll /nowarn:0618
TEST_MCS_FLAGS = /r:Mono.Posix.dll /r:System.dll /nowarn:0219,0618

include ../../build/library.make

EXTRA_DISTFILES = Mono.Unix.Native/make-map.cs

all-local: Mono.Unix.Native/make-map.exe 

Mono.Unix.Native/make-map.exe: Mono.Unix.Native/make-map.cs $(the_lib)
	cp $(the_lib) Mono.Unix.Native/
ifneq ($(PLATFORM),win32)
	$(CSCOMPILE) -debug+ -out:Mono.Unix.Native/make-map.exe -r:Mono.Posix.dll Mono.Unix.Native/make-map.cs
else
	$(CSCOMPILE) -debug+ -out:Mono.Unix.Native/make-map.exe -r:Mono.Posix.dll Mono.Unix.Native\\make-map.cs
endif

CLEAN_FILES = Mono.Unix.Native/make-map.exe Mono.Unix.Native/Mono.Posix.dll
