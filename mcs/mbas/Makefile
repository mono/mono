thisdir = mbas
SUBDIRS = 
include ../build/rules.make

PROGRAM = mbas.exe
BUILT_SOURCES = mb-parser.cs

LOCAL_MCS_FLAGS = /r:System.dll /r:$(topdir)/class/lib/$(PROFILE)/Mono.GetOptions.dll

EXTRA_DISTFILES = mb-parser.jay mbas.csproj mbas.ico mbas.sln
CLEAN_FILES = y.output testmbas/WriteOK.exe x.exe

PROGRAM_INSTALL_DIR = $(prefix)/lib/mono/1.0

include ../build/executable.make

run-test-local: run-mbas-test

run-mbas-test: testmbas/WriteOK.exe
	$(TEST_RUNTIME) --debug testmbas/WriteOK.exe --sayho /about -say:this,that,those /say:what?

mb-parser.cs: mb-parser.jay
	$(topdir)/jay/jay -ctv <$(topdir)/jay/skeleton.cs $< >$@

verbose: mbas.exe
	$(TEST_RUNTIME) ./mbas.exe --verbosegetoptions --verbose --stacktrace /libpath:../class/lib/$(PROFILE) /r:Mono.GetOptions /r:System.Data,System.Windows.Forms --main WriteOK testmbas/WriteOK.vb testmbas/WriteOK2.mbs

testmbas/WriteOK.exe: Makefile mbas.exe testmbas/*.vb testmbas/*.mbs
	$(TEST_RUNTIME) ./mbas.exe --verbosegetoptions --stacktrace /libpath:../class/lib/$(PROFILE) --reference=Mono.GetOptions -r:System.Data,System.Messaging --main WriteOK testmbas/WriteOK.vb testmbas/WriteOK2.mbs

test-aspx: mbas.exe testmbas/aspx_temp.vb
	$(TEST_RUNTIME) ./mbas.exe --stacktrace --verbosegetoptions /libpath:../class/lib/$(PROFILE) /target:library /r:"System.dll" /r:"System.Xml.dll" /r:"System.Data.dll" /r:"System.Web.dll" /r:"System.Web.Services.dll" /r:"System.Drawing.dll" -- testmbas/aspx_temp.vb

profile: mbas.exe testmbas/*.vb testmbas/*.mbs Makefile
	$(TEST_RUNTIME) --profile ./mbas.exe /out:x.exe /target:exe -r:System.Windows.Forms -r:System.Web /r:Mono.GetOptions /m:WriteOK testmbas/*.vb testmbas/*.mbs > profile
