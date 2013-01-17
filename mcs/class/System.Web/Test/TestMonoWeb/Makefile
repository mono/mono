thisdir = class/System.Web/Test/TestMonoWeb
SUBDIRS = 
include ../../../../build/rules.make

LOCAL_MCS_FLAGS = /nowarn:0168 /nowarn:0162 /unsafe \
 /r:System.dll /r:System.Web.dll \
 /r:System.Drawing.dll /r:System.Xml.dll

all-local install-local uninstall-local doc-update-local csproj-local:

# it doesn't compile for me.
# test-local: TestMonoWeb.exe

test-local:

# ??? What do we run here?

run-test-local run-test-ondotnet-local:

clean-local:
	rm -f *.exe

sources = \
	AsyncHandler.cs		\
	AsyncModule.cs		\
	AsyncOperation.cs	\
	SyncHandler.cs		\
	SyncModule.cs		\
	Test1.cs

DISTFILES = $(sources) README

dist-local: dist-default

# The thingie

TestMonoWeb.exe: $(sources)
	$(CSCOMPILE) /target:exe /out:$@ $(sources)
