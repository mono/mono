CLEAN_FILES = y.output WriteOK.exe x.exe

run-test: WriteOK.exe
	mono --debug WriteOK.exe --sayho /about -say:this,that,those /say:what?

WriteOK.exe: Makefile *.vb *.mbs
	mbas --verbosegetoptions --stacktrace --reference=Mono.GetOptions -r:System.Data,System.Messaging --main WriteOK WriteOK.vb WriteOK2.mbs AssemblyInfo.vb

verbose: 
	 mbas --verbosegetoptions --verbose --stacktrace /r:Mono.GetOptions /r:System.Data,System.Windows.Forms --main WriteOK WriteOK.vb WriteOK2.mbs AssemblyInfo.vb 

test-aspx: aspx_temp.vb
	mbas --stacktrace --verbosegetoptions /target:library /r:"System.dll" /r:"System.Xml.dll" /r:"System.Data.dll" /r:"System.Web.dll" /r:"System.Web.Services.dll" /r:"System.Drawing.dll" -- aspx_temp.vb

profile: *.vb *.mbs Makefile
	mono --profile ../../mbas.exe /libpath:../../../class/lib/default /out:x.exe /target:exe -r:System.Windows.Forms -r:System.Web /r:Mono.GetOptions /m:WriteOK *.vb *.mbs > profile

clean:
	@rm -f WriteOK.exe aspx_temp.dll x.exe

all test install run-test-ondotnet uninstall:
	@:
	@: