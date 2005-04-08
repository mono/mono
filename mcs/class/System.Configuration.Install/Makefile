thisdir = class/System.Configuration.Install
SUBDIRS =
include ../../build/rules.make

LIBRARY = System.Configuration.Install.dll
LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll /r:System.Xml.dll /r:System.Runtime.Serialization.Formatters.Soap.dll
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES = Test/ChangeLog Test/System.Configuration.Install/InstallerAssembly.cs

include ../../build/library.make

test-local run-test-local: Test/System.Configuration.Install/InstallerAssembly.dll

Test/System.Configuration.Install/InstallerAssembly.dll: Test/System.Configuration.Install/InstallerAssembly.cs
	$(CSCOMPILE) /t:library /r:System.Configuration.Install.dll /out:$@ $<
