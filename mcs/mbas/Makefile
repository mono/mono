thisdir = mbas
SUBDIRS = 
include ../build/rules.make

PROGRAM = mbas.exe
BUILT_SOURCES = mb-parser.cs
HAS_TEST = yes

LOCAL_MCS_FLAGS = /r:System.dll /r:$(topdir)/class/lib/Mono.GetOptions.dll

EXTRA_DISTFILES = mb-parser.jay mbas.csproj mbas.ico mbas.sln
CLEAN_FILES = y.output testmbas/WriteOK.exe

include ../build/executable.make

test-local: mbas.exe

run-test-local: testmbas/WriteOK.exe
	$(TEST_RUNTIME) --debug testmbas/WriteOK.exe

mb-parser.cs: mb-parser.jay
	$(topdir)/jay/jay -ctv <$(topdir)/jay/skeleton.cs $< >$@

verbose: mbas.exe
	$(TEST_RUNTIME) ./mbas.exe --verbosegetoptions --verbose --stacktrace /libpath:../class/lib /r:Mono.GetOptions /r:System.Data,System.Windows.Forms --main WriteOK testmbas/WriteOK.vb testmbas/WriteOK2.mbs

testmbas/WriteOK.exe: mbas.exe testmbas/*.vb testmbas/*.mbs
	$(TEST_RUNTIME) ./mbas.exe --stacktrace --verbosegetoptions /libpath:../class/lib /r:Mono.GetOptions /r:System.Data,System.Windows.Forms --main WriteOK testmbas/WriteOK.vb testmbas/WriteOK2.mbs

test-aspx: mbas.exe testmbas/aspx_temp.vb
	$(TEST_RUNTIME) --debug ./mbas.exe --stacktrace --verbosegetoptions /libpath:../class/lib /target:library /r:"System.dll" /r:"System.Xml.dll" /r:"System.Data.dll" /r:"System.Web.dll" /r:"System.Web.Services.dll" /r:"System.Drawing.dll" -- testmbas/aspx_temp.vb
