thisdir = class/System.Windows.Forms/SWFTest
SUBDIRS = 
include ../../../build/rules.make

all-local:

clean-local:
	rm -f *.exe

test-local: SWFTest.exe

# Fixme: does the program run itself and exit automatically?
# We want run-test to work without supervision.

run-test-local install-local:

DISTFILES = App.ico AssemblyInfo.cs changelog Form1.cs \
    Form1.resx SWFTest.csproj SWFTest.csproj.user

dist-local: dist-default

SWFTest.exe: App.ico AssemblyInfo.cs Form1.cs Form1.resx
	$(CSCOMPILE) /r:System.Windows.Forms.dll /r:System.Drawing.dll /resource:App.ico \
	    /resource:Form1.resx /r:System.Data.dll /target:exe /out:$@ AssemblyInfo.cs Form1.cs
