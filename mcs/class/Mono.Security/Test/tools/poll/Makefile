thisdir = class/Mono.Security/Test/tools/poll
SUBDIRS = 
include ../../../../../build/rules.make

LOCAL_MCS_FLAGS = /r:System.dll

all-local install-local uninstall-local:

test-local:

# ??? What do we run here?

run-test-local:

clean-local:
	rm -f *.exe *.pdb *.mdb

sources = poll.cs

DISTFILES = $(sources)

dist-local: dist-default

all: poll.exe

poll.exe: poll.cs
	$(CSCOMPILE) /target:exe /out:$@ $^
