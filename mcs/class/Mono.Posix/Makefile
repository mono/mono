thisdir = class/Mono.Posix
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Posix.dll
LIB_MCS_FLAGS = /unsafe /r:$(corlib) /r:System.dll
NO_TEST = yes
MonoPosixHelper = $(topdir)/class/lib/libMonoPosixHelper.dll.so

include ../../build/library.make

all-local: Mono.Posix/map.h ../lib/libMonoPosixHelper.so

Mono.Posix/map.h Mono.Posix/map.c: Mono.Posix/make-map.exe ../lib/Mono.Posix.dll
	cp ../lib/Mono.Posix.dll Mono.Posix/
ifneq ($(PLATFORM),win32)
	MONO_PATH=../lib/ $(PLATFORM_RUNTIME) Mono.Posix/make-map.exe ../lib/Mono.Posix.dll Mono.Posix/map
else
	Mono.Posix/make-map.exe ..\\lib\\Mono.Posix.dll Mono.Posix/map
endif

Mono.Posix/make-map.exe: Mono.Posix/make-map.cs ../lib/Mono.Posix.dll
	cp ../lib/Mono.Posix.dll Mono.Posix/
ifneq ($(PLATFORM),win32)
	$(CSCOMPILE)  -out:Mono.Posix/make-map.exe -r:../lib/Mono.Posix.dll Mono.Posix/make-map.cs
else
	$(CSCOMPILE)  -out:Mono.Posix/make-map.exe -r:../lib/Mono.Posix.dll Mono.Posix\\make-map.cs
endif

local_sources = \
	Mono.Posix/map.c 	\
	Mono.Posix/macros.c

local_objs = 	\
	Mono.Posix/map.o	\
	Mono.Posix/macros.o

%.o: %.c
	$(CCOMPILE) -fPIC -c -o $@ $^

../lib/libMonoPosixHelper.so: $(local_objs)
	gcc -shared -Wl,-soname,libMonoPosixHelper.dll.so -o $(MonoPosixHelper) $(local_objs) $(LOCAL_LDFLAGS)
