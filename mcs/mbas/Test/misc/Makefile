thisdir = mbas/Test/misc

include ../../../build/rules.make

WRITEOK_SOURCES = WriteOK.vb WriteOK2.mbs RedimPreserve.vb AssemblyInfo.vb

test-local: WriteOK.exe aspx_temp.dll ctest.exe

run-test-local: WriteOK.exe
	$(RUNTIME) $(RUNTIME_FLAGS) --debug WriteOK.exe --sayho /about -say:this,that,those /say:what?

WriteOK.exe: $(WRITEOK_SOURCES) Makefile ../../mbas.exe
	$(BASCOMPILE) --stacktrace --reference=Mono.GetOptions -r:System.Data,System.Messaging --main WriteOK $(WRITEOK_SOURCES) &> verbose.log
	tail -n 1 verbose.log

verbose:
	rm -f WriteOK.exe
	$(MAKE) LOCAL_MBAS_FLAGS='--verbosegetoptions --verbose --stacktrace' WriteOK.exe

aspx_temp.dll: aspx_temp.vb Makefile ../../mbas.exe
	$(BASCOMPILE) --stacktrace /target:library /r:"System.dll" /r:"System.Xml.dll" /r:"System.Data.dll" /r:"System.Web.dll" /r:"System.Web.Services.dll" /r:"System.Drawing.dll" -- aspx_temp.vb  &> verbose.aspx.log
	tail -n 1 verbose.aspx.log

ctest.exe: ctest.vb Makefile ../../mbas.exe
	$(BASCOMPILE) -r:System.Windows.Forms -r:System.Drawing ctest.vb &> verbose.ctest.log
	tail -n 1 verbose.ctest.log

profile: WRITEOK_SOURCES Makefile
	rm -f WriteOK.exe
	$(MAKE) RUNTIME_FLAGS='--profile' WriteOK.exe > profile

clean-local:
	-rm -f *.exe *.exe.mdb aspx_temp.dll verbose*.log 

all-local install-local uninstall-local:
	@:

DISTFILES = $(wildcard *.vb) $(wildcard *.mbs)

dist-local: dist-default
