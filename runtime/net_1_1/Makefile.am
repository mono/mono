# This is just used to copy and install the DLL files that are currently
# being compiled on windows.
#

mcs_topdir=$(top_srcdir)/../mcs
gacutil=$(srcdir)/../gacutil.exe

PROFILE=default

if JIT_SUPPORTED
mono_runtime = $(top_builddir)/mono/mini/mono
else
mono_runtime = $(top_builddir)/mono/interpreter/mint
endif

gac_assemblies_list = \
	Accessibility					\
	ByteFX.Data.dll					\
	Commons.Xml.Relaxng.dll				\
	Cscompmgd					\
	I18N.CJK.dll					\
	I18N.dll					\
	I18N.MidEast.dll				\
	I18N.Other.dll					\
	I18N.Rare.dll					\
	I18N.West.dll					\
	ICSharpCode.SharpZipLib.dll			\
	Microsoft.VisualBasic.dll			\
	Microsoft.VisualC.dll				\
	Microsoft.Vsa.dll				\
	Microsoft.Web.Services.dll 			\
	Mono.Cairo.dll					\
	Mono.CSharp.Debugger.dll			\
	Mono.Data.SybaseClient.dll			\
	Mono.Data.SqliteClient.dll			\
	Mono.Data.TdsClient.dll				\
	Mono.Data.Tds.dll				\
	Mono.GetOptions.dll				\
	Mono.Http.dll					\
	Mono.Posix.dll					\
	Mono.Security					\
	Mono.Security.Win32				\
	Novell.Directory.Ldap.dll			\
	Npgsql.dll					\
	PEAPI.dll					\
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

non_gac_assemblies = \
	NUnit.Util.dll					\
	NUnit.Framework.dll

corlibdir = $(libdir)
dist_corlib_DATA = mscorlib.dll

EXTRA_DIST = $(gac_assemblies)

#
# Keep in sync with mono/mono/metadata/Makefile.am
#
assembliesdir = $(libdir)
dist_assemblies_DATA = $(non_gac_assemblies)

$(non_gac_assemblies):
	@f=$(mcs_topdir)/class/lib/$(PROFILE)/$@; \
	if test -f $$f; then :; else f=$(assembliesdir)/$@; fi ; \
	echo "cp -f $$f $(srcdir)"; \
	cp -f $$f $(srcdir)

mscorlib.dll $(gac_assemblies):
	cp -f $(mcs_topdir)/class/lib/$(PROFILE)/$@ $(srcdir)

all-local: $(gac_assemblies)

install-data-local:
	@if test -n '$(gac_assemblies)'; then \
	  for i in ''$(gac_assemblies); do \
	    echo "MONO_PATH=$(srcdir) $(mono_runtime) $(gacutil) /i $(srcdir)/$$i.dll /f /prefix $(DESTDIR)$(libdir)/mono/gac" ; \
	    MONO_PATH=$(srcdir) \
	      $(mono_runtime) $(gacutil) /i $(srcdir)/$$i /f /prefix $(DESTDIR)$(libdir)/mono/gac || exit 1 ; \
	  done; fi

uninstall-local:
	@if test -n '$(gac_assemblies_list)'; then \
	  for i in ''$(gac_assemblies_list); do \
	    echo "MONO_PATH=$(srcdir) $(mono_runtime) $(gacutil) /u $$i" ; \
	    MONO_PATH=$(srcdir) \
	      $(mono_runtime) $(gacutil) /u $$i || exit 1 ; \
	  done; fi

copy_dlls:
	cp /nt/mono/mcs/class/*/*.dll .

push_dlls:
	scp -o "Protocol 1" *.dll *.exe snapshot@mono-cvs.ximian.com:dlls
