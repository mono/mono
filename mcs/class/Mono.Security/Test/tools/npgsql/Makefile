thisdir = class/Mono.Security/Test/tools/npdbtest
SUBDIRS = 
include ../../../../../build/rules.make

LOCAL_MCS_FLAGS = /r:System.Data.dll /r:Npgsql.dll

all-local install-local uninstall-local:

test-local:

# ??? What do we run here?

run-test-local:

clean-local:
	rm -f *.exe *.mdb *.pdb

sources = npdbtest.cs

DISTFILES = $(sources)

dist-local: dist-default

all: npdbtest.exe

npdbtest.exe: $(sources)
	$(CSCOMPILE) /target:exe /out:$@ $(sources)
