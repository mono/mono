# This is just used to copy and install the DLL files that are currently
# being compiled on windows.
#

mcs_topdir=$(top_srcdir)/../mcs
gacutil=$(mcs_topdir)/tools/gacutil/gacutil.exe

gac_assemblies_list = \
	Accessibility.dll					\
	Cscompmgd.dll						\
	Mono.Security.dll					\
	Mono.Security.Win32.dll					\
	System.Configuration.Install.dll			\
	System.Data.dll						\
	System.Data.OracleClient.dll				\
	System.dll						\
	System.Design.dll					\
	System.DirectoryServices.dll				\
	System.Drawing.Design.dll				\
	System.Drawing.dll					\
	System.EnterpriseServices.dll				\
	System.Management.dll					\
	System.Messaging.dll					\
	System.Runtime.Remoting.dll				\
	System.Runtime.Serialization.Formatters.Soap.dll	\
	System.Security.dll					\
	System.ServiceProcess.dll				\
	System.Web.dll						\
	System.Web.Services.dll					\
	System.Windows.Forms.dll				\
	System.Xml.dll

assemblies_DATA =	\
	ByteFX.Data.dll						\
	Commons.Xml.Relaxng.dll					\
	mscorlib.dll						\
	I18N.CJK.dll						\
	I18N.dll						\
	I18N.MidEast.dll					\
	I18N.Other.dll						\
	I18N.Rare.dll						\
	I18N.West.dll						\
	ICSharpCode.SharpZipLib.dll				\
	Microsoft.VisualBasic.dll				\
	Microsoft.VisualC.dll					\
	Microsoft.Vsa.dll					\
	Mono.Cairo.dll						\
	Mono.CSharp.Debugger.dll				\
	Mono.Data.SqliteClient.dll				\
	Mono.Data.SybaseClient.dll				\
	Mono.Data.TdsClient.dll					\
	Mono.Data.Tds.dll					\
	Novell.Directory.Ldap.dll				\
	Mono.GetOptions.dll					\
	Mono.Http.dll						\
	Mono.Posix.dll						\
	Npgsql.dll						\
	NUnit.Util.dll						\
	NUnit.Framework.dll					\
	PEAPI.dll						


EXTRA_DIST = $(assemblies_DATA)

#
# Keep in sync with mono/mono/metadata/Makefile.am
#
assembliesdir = $(libdir)

$(assemblies_DATA):
	@f=$(mcs_topdir)/class/lib/net_2_0/$@; \
	if test -f $$f; then :; else f=$(assembliesdir)/$@; fi ; \
	echo "cp -f $$f $(srcdir)"; \
	cp -f $$f $(srcdir)

$(gac_assemblies_list):
	cp -f $(mcs_topdir)/class/lib/net_2_0/$@ $(srcdir)

all-local: $(gac_assemblies_list)

install-data-local:
	@if test -n '$(gac_assemblies_list)'; then \
	  for i in ''$(gac_assemblies_list); do	\
		echo "$(top_builddir)/mono/mini/mono $(gacutil) /i $(srcdir)/$$i /f" ; \
		$(top_builddir)/mono/mini/mono $(gacutil) /i $(srcdir)/$$i /f || exit 1 ; \
	  done; fi

uninstall-local:
	@if test -n '$(gac_assemblies_list)'; then \
	  for i in ''$(gac_assemblies_list); do \
		echo "$(top_builddir)/mono/mini/mono $(gacutil) /u $(srcdir)/$$i" ; \
		$(top_builddir)/mono/mini/mono $(gacutil) /u $(srcdir)/$$i || exit 1 ; \
	  done; fi

dist-hook: $(gac_assemblies_list)
	@for i in $(gac_assemblies_list); do \
		cp $(srcdir)/$$i $(distdir)/. ; \
	done

copy_dlls:
	cp /nt/mono/mcs/class/*/*.dll .

push_dlls:
	scp -o "Protocol 1" *.dll *.exe snapshot@mono-cvs.ximian.com:dlls
