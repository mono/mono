# This is just used to copy and install the DLL files that are currently
# being compiled on windows.
#

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
	Mono.Cairo.dll	                		\
	Mono.CSharp.Debugger.dll				\
	Mono.Data.DB2Client.dll					\
	Mono.Data.MySql.dll					\
	Mono.Data.PostgreSqlClient.dll				\
	Mono.Data.SqliteClient.dll				\
	Mono.Data.SybaseClient.dll				\
	Mono.Data.TdsClient.dll					\
	Mono.Data.Tds.dll					\
	Novell.Directory.Ldap.dll					\
	Mono.GetOptions.dll					\
	Mono.Http.dll						\
	Mono.PEToolkit.dll					\
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
	System.Xml.dll						\
	libgdiplus.dll.so

monobins_DATA = mcs.exe mbas.exe monoresgen.exe secutil.exe ilasm.exe cilc.exe xsd.exe wsdl.exe genxs.exe disco.exe sqlsharp.exe signcode.exe chktrust.exe

EXTRA_DIST = $(monobins_DATA) $(assemblies_DATA)

#
# Keep in sync with mono/mono/metadata/Makefile.am
#
assembliesdir = $(libdir)
monobinsdir = $(bindir)

$(assemblies_DATA):
	-cp $(top_srcdir)/../mcs/class/lib/$@ . || cp $(assembliesdir)/$@ . 

$(monobins_DATA):
	-cp $(top_srcdir)/../mcs/mcs/mcs.exe . || cp $(monobinsdir)/mcs.exe . 
	-cp $(top_srcdir)/../mcs/mbas/mbas.exe . || cp $(monobinsdir)/mbas.exe .
	-cp $(top_srcdir)/../mcs/tools/security/secutil.exe . || cp $(monobinsdir)/secutil.exe . 
	-cp $(top_srcdir)/../mcs/monoresgen/monoresgen.exe . || cp $(monobinsdir)/monoresgen.exe . 
	-cp $(top_srcdir)/../mcs/ilasm/ilasm.exe . || cp $(monobinsdir)/ilasm.exe .
	-cp $(top_srcdir)/../mcs/tools/cilc/cilc.exe . || cp $(monobinsdir)/cilc.exe .
	-cp $(top_srcdir)/../mcs/tools/mono-xsd/xsd.exe . || cp $(monobinsdir)/xsd.exe .
	-cp $(top_srcdir)/../mcs/tools/wsdl/wsdl.exe . || cp $(monobinsdir)/wsdl.exe .
	-cp $(top_srcdir)/../mcs/tools/genxs/genxs.exe . || cp $(monobinsdir)/genxs.exe .
	-cp $(top_srcdir)/../mcs/tools/disco/disco.exe . || cp $(monobinsdir)/disco.exe .
	-cp $(top_srcdir)/../mcs/tools/SqlSharp/sqlsharp.exe . || cp $(monobinsdir)/sqlsharp.exe .
	-cp $(top_srcdir)/../mcs/tools/security/chktrust.exe . || cp $(monobinsdir)/chktrust.exe .
	-cp $(top_srcdir)/../mcs/tools/security/signcode.exe . || cp $(monobinsdir)/signcode.exe .
        
dist-hook:
	for i in $(monobins_DATA) $(assemblies_DATA); do	\
		if test ! -s $(srcdir)/$$i; then echo $$i is empty && exit 1; fi	\
	done

copy_dlls:
	cp /nt/mono/mcs/class/*/*.dll .

push_dlls:
	scp -o "Protocol 1" *.dll *.exe snapshot@mono-cvs.ximian.com:dlls
