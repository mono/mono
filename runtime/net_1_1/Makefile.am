# This is just used to copy and install the DLL files that are currently
# being compiled on windows.
#

gacutil=$(srcdir)/../gacutil.exe
framework_version = 1.0

if USE_JIT
mono_runtime = $(top_builddir)/mono/mini/mono
else
mono_runtime = $(top_builddir)/mono/interpreter/mint
endif

MONO_CSHARP_DEBUGGER = Mono.CompilerServices.SymbolWriter
if PLATFORM_WIN32
GAC_DIR = `cygpath -w $(libdir)`
GAC_ROOT_DIR = `cygpath -w $(DESTDIR)$(libdir)`
else
GAC_DIR = $(libdir)
GAC_ROOT_DIR = $(DESTDIR)$(libdir)
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

corlibdir = $(libdir)/mono/$(framework_version)
corlib_DATA = mscorlib.dll

EXTRA_DIST = $(gac_assemblies) $(corlib_DATA)
MAINTAINERCLEANFILES = $(EXTRA_DIST)

mscorlib.dll $(gac_assemblies):
	test -f $(top_srcdir)/../mcs/class/lib/default/$@
	-rm -f $(srcdir)/$@
	cd $(srcdir) && $(LN_S) ../../../mcs/class/lib/default/$@ $@

all-local: $(gac_assemblies)

install-data-local:
	@if test -f $(srcdir)/mscorlib.dll.mdb; then \
	  echo $(INSTALL_DATA) $(srcdir)/mscorlib.dll.mdb $(corlibdir)/mscorlib.dll.mdb; \
	  $(INSTALL_DATA) $(srcdir)/mscorlib.dll.mdb $(corlibdir)/mscorlib.dll.mdb; \
	fi
	@if test -n '$(gac_assemblies)'; then \
	  for i in ''$(gac_assemblies); do \
	    echo "MONO_PATH=$(srcdir) $(mono_runtime) --config ../../data/config $(gacutil) /i $(srcdir)/$$i /f /package $(framework_version) /gacdir $(GAC_DIR) /root $(GAC_ROOT_DIR)" ; \
	    MONO_PATH=$(srcdir) \
	      $(LIBTOOL) --mode=execute $(mono_runtime) --config ../../data/config $(gacutil) /i $(srcdir)/$$i /f /package $(framework_version) /gacdir $(GAC_DIR) /root $(GAC_ROOT_DIR) || exit 1 ; \
	  done; fi

uninstall-local:
	@if test -n '$(gac_assemblies_list)'; then \
	  for i in ''$(gac_assemblies_list); do \
	    echo "MONO_PATH=$(srcdir) $(mono_runtime) $(gacutil) /u $$i /gacdir $(GAC_DIR) /root $(GAC_ROOT_DIR)" ; \
	    MONO_PATH=$(srcdir) \
	      $(LIBTOOL) --mode=execute $(mono_runtime) --config ../../data/config $(gacutil) /u $$i /gacdir $(GAC_DIR) /root $(GAC_ROOT_DIR) || true ; \
	  done; fi

copy_dlls:
	cp /nt/mono/mcs/class/*/*.dll .

push_dlls:
	scp -o "Protocol 1" *.dll *.exe snapshot@mono-cvs.ximian.com:dlls
