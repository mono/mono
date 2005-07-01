thisdir = class/Mono.Security/Test/tools/server
SUBDIRS = 
include ../../../../../build/rules.make

LOCAL_MCS_FLAGS = -r:System.dll -r:Mono.Security.dll

all-local install-local uninstall-local:

test-local:

# ??? What do we run here?

run-test-local:

clean-local:
	rm -f *.exe *.pdb *.mdb ssl.cer ssl.pvk index.html*

sources = msslserver.cs

DISTFILES = $(sources) mutual.pem README Makefile

dist-local: dist-default

all: msslserver.exe ssl.pvk

msslserver.exe: msslserver.cs
	$(CSCOMPILE) /target:exe /out:$@ $^

ssl.pvk:
	makecert -r -eku 1.3.6.1.5.5.7.3.1 -n "CN=$(HOSTNAME)" -sv ssl.pvk ssl.cer
