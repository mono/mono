# This is just used to copy and install the DLL files that are currently
# being compiled on windows.
#

mcs_topdir=$(top_srcdir)/../mcs
gacutil=$(srcdir)/../gacutil.exe

PROFILE=net_2_0

if JIT_SUPPORTED
mono_runtime = $(top_builddir)/mono/mini/mono
else
mono_runtime = $(top_builddir)/mono/interpreter/mint
endif

gac_assemblies_list = \
	Accessibility					\
	ByteFX.Data					\
	Commons.Xml.Relaxng				\
	Cscompmgd					\
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
	Mono.CSharp.Debugger				\
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
	NUnit.Util					\
	NUnit.Framework					\
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

corlibdir = $(libdir)/net_2_0
dist_corlib_DATA = mscorlib.dll

EXTRA_DIST = $(gac_assemblies)
CLEANFILES = $(EXTRA_DIST)

#
# Keep in sync with mono/mono/metadata/Makefile.am
#
assembliesdir = $(libdir)

mscorlib.dll $(gac_assemblies):
	cp -f $(mcs_topdir)/class/lib/$(PROFILE)/$@ $(srcdir)

all-local: $(gac_assemblies)

install-data-local:
	@if test -n '$(gac_assemblies)'; then \
	  for i in ''$(gac_assemblies); do \
	    echo "MONO_PATH=$(srcdir)/../net_1_1 $(mono_runtime) $(gacutil) -i $(srcdir)/$$i /f /root $(DESTDIR)$(libdir)" ; \
	    MONO_PATH=$(srcdir)/../net_1_1 \
	      $(mono_runtime) $(gacutil) -i $(srcdir)/$$i /f /root $(DESTDIR)$(libdir) || exit 1 ; \
	  done; fi

uninstall-local:
	@if test -n '$(gac_assemblies_list)'; then \
	  for i in ''$(gac_assemblies_list); do \
	    echo "MONO_PATH=$(srcdir)/../net_1_1 $(mono_runtime) $(gacutil) /u $$i" ; \
	    MONO_PATH=$(srcdir)/../net_1_1 \
	      $(mono_runtime) $(gacutil) /u $$i || exit 1 ; \
	  done; fi

copy_dlls:
	cp /nt/mono/mcs/class/*/*.dll .

push_dlls:
	scp -o "Protocol 1" *.dll *.exe snapshot@mono-cvs.ximian.com:dlls
