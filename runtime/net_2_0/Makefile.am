# This is just used to copy and install the DLL files that are currently
# being compiled on windows.
#

gacutil=$(srcdir)/../gacutil.exe
framework_version = 2.0

if USE_JIT
mono_runtime = $(top_builddir)/mono/mini/mono
else
mono_runtime = $(top_builddir)/mono/interpreter/mint
endif

if PLATFORM_WIN32
MONO_CSHARP_DEBUGGER =
else
MONO_CSHARP_DEBUGGER = Mono.CompilerServices.SymbolWriter
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

if PLATFORM_WIN32
gac_assemblies =
mdb_files =
local_mscorlib =
else
gac_assemblies = $(gac_assemblies_list:=.dll)
mdb_files = $(gac_assemblies:=.mdb)
local_mscorlib = mscorlib.dll
endif

corlibdir = $(libdir)/mono/$(framework_version)
corlib_DATA = $(local_mscorlib)

EXTRA_DIST = $(gac_assemblies) $(corlib_DATA) $(mdb_files)
MAINTAINERCLEANFILES = $(EXTRA_DIST)

mscorlib.dll $(gac_assemblies) $(mdb_files):
	test -f $(top_srcdir)/../mcs/class/lib/net_2_0/$@
	-rm -f $(srcdir)/$@
	cd $(srcdir) && $(LN_S) ../../../mcs/class/lib/net_2_0/$@ $@

all-local: $(gac_assemblies)

if INSTALL_2_0
install-data-local:
	@if test -f $(srcdir)/mscorlib.dll.mdb; then \
	  echo $(INSTALL_DATA) $(srcdir)/mscorlib.dll.mdb $(DESTDIR)$(corlibdir)/mscorlib.dll.mdb; \
	  $(INSTALL_DATA) $(srcdir)/mscorlib.dll.mdb $(DESTDIR)$(corlibdir)/mscorlib.dll.mdb; \
	fi
	@if test -n '$(gac_assemblies)'; then \
	  for i in ''$(gac_assemblies); do \
	    echo "MONO_PATH=$(srcdir)/../net_1_1 $(mono_runtime) --config ../../data/config $(gacutil) /i $(srcdir)/$$i /f /package $(framework_version) /root $(DESTDIR)$(libdir)" ; \
	    MONO_PATH=$(srcdir)/../net_1_1 \
	      $(LIBTOOL) --mode=execute $(mono_runtime)	 --config ../../data/config  $(gacutil) /i $(srcdir)/$$i /f /package $(framework_version) /root $(DESTDIR)$(libdir) || exit 1 ; \
	  done; fi

uninstall-local:
	@if test -n '$(gac_assemblies_list)'; then \
	  for i in ''$(gac_assemblies_list); do \
	    echo "MONO_PATH=$(srcdir)/../net_1_1 $(mono_runtime) $(gacutil) /u $$i /root $(DESTDIR)$(libdir)" ; \
	    MONO_PATH=$(srcdir)/../net_1_1 \
	      $(LIBTOOL) --mode=execute $(mono_runtime) $(gacutil) /u $$i /root $(DESTDIR)$(libdir) || true ; \
	  done; fi
else
install-data-local uninstall-local:
	@:
endif

copy_dlls:
	cp /nt/mono/mcs/class/*/*.dll .

push_dlls:
	scp -o "Protocol 1" *.dll *.exe snapshot@mono-cvs.ximian.com:dlls
