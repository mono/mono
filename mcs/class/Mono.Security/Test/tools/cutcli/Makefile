thisdir = class/Mono.Security/Test/tools/cutcli
SUBDIRS = 
include ../../../../../build/rules.make

LOCAL_MCS_FLAGS = /r:System.dll /r:Mono.Security.dll

all-local install-local uninstall-local:

test-local:

# ??? What do we run here?

run-test-local:

clean-local:
	rm -f *.exe *.pdb

sources = cutcli.cs ControlledNetworkStream.cs

DISTFILES = $(sources)

dist-local: dist-default

all: cutcli.exe

cutcli.exe: $(sources)
	$(CSCOMPILE) /target:exe /out:$@ $(sources)
