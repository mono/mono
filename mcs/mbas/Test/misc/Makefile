thisdir = mbas/Test/misc

include ../../../build/rules.make

LOCAL_MBAS_FLAGS = --verbosegetoptions --stacktrace

test-local: WriteOK.exe

run-test-local: WriteOK.exe
	$(RUNTIME) --debug WriteOK.exe --sayho /about -say:this,that,those /say:what?

run-test-ondotnet-local: WriteOK.exe
	./WriteOK.exe --sayho /about -say:this,that,those /say:what?

WriteOK.exe: Makefile *.vb *.mbs ../../mbas.exe
	$(BASCOMPILE) --reference=Mono.GetOptions -r:System.Data,System.Messaging --main WriteOK WriteOK.vb WriteOK2.mbs RedimPreserve.vb AssemblyInfo.vb

verbose:
	$(MAKE) LOCAL_MBAS_FLAGS='--verbosegetoptions --verbose --stacktrace' WriteOK.exe

test-aspx: aspx_temp.dll

aspx_temp.dll: aspx_temp.vb
	$(BASCOMPILE) --stacktrace --verbosegetoptions /target:library /r:"System.dll" /r:"System.Xml.dll" /r:"System.Data.dll" /r:"System.Web.dll" /r:"System.Web.Services.dll" /r:"System.Drawing.dll" -- aspx_temp.vb

profile: *.vb *.mbs Makefile
	rm -f WriteOK.exe
	$(MAKE) RUNTIME='$(RUNTIME) --profile' WriteOK.exe > profile

clean-local:
	-rm -f WriteOK.exe aspx_temp.dll x.exe

all-local install-local uninstall-local:
	@:

DISTFILES = $(wildcard *.vb) $(wildcard *.mbs)

dist-local: dist-default
