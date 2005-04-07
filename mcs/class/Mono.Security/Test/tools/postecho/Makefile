thisdir = class/Mono.Security/Test/tools/postecho
SUBDIRS = 
include ../../../../../build/rules.make

LOCAL_MCS_FLAGS = /r:System.dll /r:Mono.Security.dll

all-local install-local uninstall-local:

test-local:

# ??? What do we run here?

run-test-local:

clean-local:
	rm -f *.exe *.mdb *.pdb

sources = postecho.cs postmulti.cs

DISTFILES = $(sources)

dist-local: dist-default

all: postecho.exe postmulti.exe

postecho.exe: postecho.cs
	$(CSCOMPILE) /target:exe /out:$@ $^

postmulti.exe: postmulti.cs
	$(CSCOMPILE) /target:exe /out:$@ $^
