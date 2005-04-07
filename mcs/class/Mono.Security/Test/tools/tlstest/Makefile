thisdir = class/Mono.Security/Test/tools/tlstest
SUBDIRS = 
include ../../../../../build/rules.make

LOCAL_MCS_FLAGS = /r:System.dll /r:Mono.Security.dll

all-local install-local uninstall-local:

test-local:

# ??? What do we run here?

run-test-local:

clean-local:
	rm -f *.exe *.pdb

sources = tlstest.cs tlsmulti.cs

DISTFILES = $(sources)

dist-local: dist-default

all: tlstest.exe tlsmulti.exe

tlstest.exe: tlstest.cs
	$(CSCOMPILE) /target:exe /out:$@ $^

tlsmulti.exe: tlsmulti.cs
	$(CSCOMPILE) /target:exe /out:$@ $^
