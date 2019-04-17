thisdir = class/System.Private.CoreLib
SUBDIRS =
#include ../../build/rules.make

dirs := $(dir $(wildcard */*))
files := $(wildcard */*.cs)

all-local:
	dotnet build @CORETARGETS@ -p:BuildArch=@COREARCH@ -p:OutputPath=bin/@COREARCH@ -p:FeaturePortableTimer=true System.Private.CoreLib.csproj

dist-recursive:
	$(mkdir_p) $(distdir)
	cp -a Makefile.am $(distdir)
	cp -a Makefile.in $(distdir)
	cp -a *.csproj $(distdir)
	for i in $(dirs); do $(mkdir_p) $(distdir)/$$i; done
	for i in $(files); do cp -a $$i $(distdir)/$$i; done
