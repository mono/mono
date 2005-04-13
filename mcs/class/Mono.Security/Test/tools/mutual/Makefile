thisdir = class/Mono.Security/Test/tools/mutual
SUBDIRS = 
include ../../../../../build/rules.make

LOCAL_MCS_FLAGS = /r:System.dll /r:Mono.Security.dll

all-local install-local uninstall-local:

test-local:

# ??? What do we run here?

run-test-local:

clean-local:
	rm -f *.exe *.pdb *.mdb

sources = mutual.cs

DISTFILES = $(sources) ca.pem client.p12 client.pem server_cert.pem \
	server_key.pem server.sh README

dist-local: dist-default

all: mutual.exe

mutual.exe: mutual.cs
	$(CSCOMPILE) /target:exe /out:$@ $^
