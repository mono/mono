# This is just used to copy and install the DLL files that are currently
# being compiled on windows.
#

mcs_topdir=$(top_srcdir)/../mcs

assemblies_DATA =	\
	Accessibility.dll					\
	ByteFX.Data.dll						\
	Commons.Xml.Relaxng.dll					\
	mscorlib.dll						\
	Cscompmgd.dll						\
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
	Mono.Cairo.dll			\
	Mono.CSharp.Debugger.dll				\
	Mono.Data.SqliteClient.dll				\
	Mono.Data.SybaseClient.dll				\
	Mono.Data.TdsClient.dll					\
	Mono.Data.Tds.dll					\
	Novell.Directory.Ldap.dll					\
	Mono.GetOptions.dll					\
	Mono.Http.dll						\
	Mono.Posix.dll						\
	Mono.Security.dll					\
	Mono.Security.Win32.dll					\
	Npgsql.dll						\
	NUnit.Util.dll						\
	NUnit.Framework.dll					\
	PEAPI.dll						\
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

monobins_DATA = mcs.exe mbas.exe monoresgen.exe secutil.exe ilasm.exe cilc.exe xsd.exe wsdl.exe genxs.exe al.exe disco.exe sqlsharp.exe signcode.exe chktrust.exe soapsuds.exe monop.exe setreg.exe cert2spc.exe certmgr.exe MakeCert.exe

EXTRA_DIST = $(monobins_DATA) $(assemblies_DATA)

#
# Keep in sync with mono/mono/metadata/Makefile.am
#
assembliesdir = $(libdir)
if PLATFORM_WIN32
monobinsdir = $(libdir)
else
monobinsdir = $(bindir)
endif

$(assemblies_DATA):
	@f=$(mcs_topdir)/class/lib/$@; \
	if test -f $$f; then :; else f=$(assembliesdir)/$@; fi ; \
	echo "cp -f $$f $(srcdir)"; \
	cp -f $$f $(srcdir)

$(monobins_DATA):
	@case "$@" in \
	mcs.exe)                 d=mcs                          ;; \
	mbas.exe)                d=mbas                         ;; \
	secutil.exe)             d=tools/security               ;; \
	monoresgen.exe)          d=monoresgen                   ;; \
	ilasm.exe)               d=ilasm                        ;; \
	cilc.exe)                d=tools/cilc                   ;; \
	xsd.exe)                 d=tools/mono-xsd               ;; \
	wsdl.exe)                d=tools/wsdl                   ;; \
	genxs.exe)               d=tools/genxs                  ;; \
	al.exe)                  d=tools/al                     ;; \
	disco.exe)               d=tools/disco                  ;; \
	soapsuds.exe)            d=tools/soapsuds               ;; \
	sqlsharp.exe)            d=tools/SqlSharp               ;; \
	chktrust.exe)            d=tools/security               ;; \
	signcode.exe)            d=tools/security               ;; \
	MakeCert.exe)            d=tools/security               ;; \
	cert2spc.exe)            d=tools/security               ;; \
	certmgr.exe)             d=tools/security               ;; \
	setreg.exe)              d=tools/security               ;; \
	monop.exe)               d=tools/monop                  ;; \
	browsercaps-updater.exe) d=tools/browsercaps-updater    ;; \
	esac; \
	f=$(mcs_topdir)/$$d/$@; \
	if test -f $$f; then :; else f=$(monobinsdir)/$@; fi ; \
	echo "cp -f $$f $(srcdir)"; \
	cp -f $$f $(srcdir)

dist-hook:
	for i in $(monobins_DATA) $(assemblies_DATA); do	\
		if test ! -s $(srcdir)/$$i; then echo $$i is empty && exit 1; fi	\
	done

copy_dlls:
	cp /nt/mono/mcs/class/*/*.dll .

push_dlls:
	scp -o "Protocol 1" *.dll *.exe snapshot@mono-cvs.ximian.com:dlls
