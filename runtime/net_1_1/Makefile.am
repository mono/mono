# This is just used to copy and install the DLL files that are currently
# being compiled on windows.
#

gacutil=$(srcdir)/../gacutil.exe

if USE_JIT
mono_runtime = $(top_builddir)/mono/mini/mono
else
mono_runtime = $(top_builddir)/mono/interpreter/mint
endif

if PLATFORM_WIN32
MONO_CSHARP_DEBUGGER =
GAC_DIR = `cygpath -w $(DESTDIR)$(libdir)`
else
MONO_CSHARP_DEBUGGER = Mono.CSharp.Debugger
GAC_DIR = $(DESTDIR)$(libdir)
endif

gac_assemblies_list = \
	Accessibility					\
	ByteFX.Data					\
	Commons.Xml.Relaxng				\
	cscompmgd					\
	I18N.CJK					\
	I18N						\
	I18N.MidEast					\
	I18N.Other					\
	I18N.Rare					\
	I18N.West					\
	IBM.Data.DB2					\
	ICSharpCode.SharpZipLib				\
	Microsoft.VisualBasic				\
	Microsoft.VisualC				\
	Microsoft.Vsa					\
	Mono.Cairo					\
	$(MONO_CSHARP_DEBUGGER)				\
	Mono.Data.SybaseClient				\
	Mono.Data.SqliteClient				\
	Mono.Data.TdsClient				\
	Mono.Data.Tds					\
	Mono.GetOptions					\
	Mono.Http					\
	Mono.Posix					\
	Mono.Security					\
	Mono.Security.Win32				\
	Novell.Directory.Ldap				\
	Npgsql						\
	nunit.util					\
	nunit.core					\
	nunit.framework					\
	PEAPI						\
	System.Configuration.Install			\
	System.Data					\
	System.Data.OracleClient			\
	System						\
	System.Design					\
	System.DirectoryServices			\
	System.Drawing.Design				\
	System.Drawing					\
	System.EnterpriseServices			\
	System.Management				\
	System.Messaging				\
	System.Runtime.Remoting				\
	System.Runtime.Serialization.Formatters.Soap	\
	System.Security					\
	System.ServiceProcess				\
	System.Web					\
	System.Web.Services				\
	System.Windows.Forms				\
	System.Xml

gac_assemblies = $(gac_assemblies_list:=.dll)

corlibdir = $(libdir)
corlib_DATA = mscorlib.dll

EXTRA_DIST = $(gac_assemblies) $(corlib_DATA)
MAINTAINERCLEANFILES = $(EXTRA_DIST)

mscorlib.dll $(gac_assemblies):
	test -f $(top_srcdir)/../mcs/class/lib/default/$@
	-rm -f $(srcdir)/$@
	cd $(srcdir) && $(LN_S) ../../../mcs/class/lib/default/$@ $@

all-local: $(gac_assemblies)

install-data-local:
	@if test -n '$(gac_assemblies)'; then \
	  for i in ''$(gac_assemblies); do \
	    echo "MONO_PATH=$(srcdir) $(DESTDIR)$(bindir)/mono $(gacutil) /i $(srcdir)/$$i /f /package 1.0 /gacdir $(GAC_DIR) /root $(GAC_DIR)" ; \
	    MONO_PATH=$(srcdir) \
	      $(LIBTOOL) --mode=execute $(DESTDIR)$(bindir)/mono $(gacutil) /i $(srcdir)/$$i /f /package 1.0 /gacdir $(GAC_DIR) /root $(GAC_DIR) || exit 1 ; \
	  done; fi

uninstall-local:
	@if test -n '$(gac_assemblies_list)'; then \
	  for i in ''$(gac_assemblies_list); do \
	    echo "MONO_PATH=$(srcdir) $(mono_runtime) $(gacutil) /u $$i /gacdir $(GAC_DIR) /root $(GAC_DIR)" ; \
	    MONO_PATH=$(srcdir) \
	      $(LIBTOOL) --mode=execute $(DESTDIR)$(bindir)/mono $(gacutil) /u $$i /gacdir $(GAC_DIR) /root $(GAC_DIR) || true ; \
	  done; fi

copy_dlls:
	cp /nt/mono/mcs/class/*/*.dll .

push_dlls:
	scp -o "Protocol 1" *.dll *.exe snapshot@mono-cvs.ximian.com:dlls
