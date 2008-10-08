thisdir = class/Mono.Security/Test/tools/sockethell
SUBDIRS = 
include ../../../../../build/rules.make

LOCAL_MCS_FLAGS = -r:System.dll -r:Mono.Security.dll -r:../../../../lib/net_1_1/nunit.framework.dll

all-local install-local uninstall-local:

test-local:

run-test-local: SocketHell.dll
	mono ../../../../lib/net_1_1/nunit-console.exe SocketHell.dll

clean-local:
	rm -f *.dll *.mdb *.pdb TestResult.xml

sources = SocketHell.cs

DISTFILES = $(sources)

dist-local: dist-default

all: SocketHell.dll

SocketHell.dll: SocketHell.cs
	$(CSCOMPILE) -target:library -out:$@ $^
