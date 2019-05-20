thisdir = class/System.Private.CoreLib
SUBDIRS =
include ../../build/rules.make

include ../../../netcore/roslyn.make
ROSLYN_PROPS_FILE := ../../../netcore/roslyn/packages/microsoft.net.compilers.toolset/$(ROSLYN_VERSION)/build/Microsoft.Net.Compilers.Toolset.props

dirs := $(dir $(wildcard */*))
files := $(wildcard */*.cs)

all-local: $(ROSLYN_PROPS_FILE)
	dotnet build $(CORETARGETS) $(CORLIB_BUILD_FLAGS) -p:BuildArch=$(COREARCH) -p:OutputPath=bin/$(COREARCH) -p:RoslynPropsFile="$(ROSLYN_PROPS_FILE)" System.Private.CoreLib.csproj

$(ROSLYN_PROPS_FILE):
	make -C ../../../netcore update-roslyn

dist-local:
	mkdir -p $(distdir)
	cp -a Makefile $(distdir)
	cp -a *.csproj $(distdir)
	for i in $(dirs); do mkdir -p $(distdir)/$$i; done
	for i in $(files); do cp -a $$i $(distdir)/$$i; done
