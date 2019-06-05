thisdir = class/System.Private.CoreLib
SUBDIRS =
include ../../build/rules.make

dirs := $(dir $(wildcard */*))
files := $(wildcard */*.cs)

all-local:
	make -C ../../../netcore bcl

dist-local:
	mkdir -p $(distdir)
	cp -a Makefile $(distdir)
	cp -a *.csproj $(distdir)
	for i in $(dirs); do mkdir -p $(distdir)/$$i; done
	for i in $(files); do cp -a $$i $(distdir)/$$i; done
