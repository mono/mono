# This is just used to copy and install the DLL files that are currently
# being compiled on windows.
#

assemblies_DATA =	\
	Accessibility.dll					\
	ByteFX.Data.dll						\
	corlib.dll						\
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
	Mono.CSharp.Debugger.dll				\
	Mono.Data.DB2Client.dll					\
	Mono.Data.MySql.dll					\
	Mono.Data.PostgreSqlClient.dll				\
	Mono.Data.SqliteClient.dll				\
	Mono.Data.SybaseClient.dll				\
	Mono.Data.TdsClient.dll					\
	Mono.Data.Tds.dll					\
	Mono.GetOptions.dll					\
	Mono.PEToolkit.dll					\
	Mono.Posix.dll						\
	Npgsql.dll						\
	NUnit.Util.dll						\
	NUnit.Framework.dll					\
	PEAPI.dll						\
	System.Configuration.Install.dll			\
	System.Data.dll						\
	System.Data.OracleClient.dll				\
	System.dll						\
	System.Design.dll					\
	System.Drawing.dll					\
	System.EnterpriseServices.dll				\
	System.Runtime.Remoting.dll				\
	System.Runtime.Serialization.Formatters.Soap.dll	\
	System.Security.dll					\
	System.Web.dll						\
	System.Web.Services.dll					\
	System.Windows.Forms.dll				\
	System.Xml.dll

monobins_DATA = mcs.exe mbas.exe monoresgen.exe secutil.exe ilasm.exe cilc.exe

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

dist-hook:
	for i in $(monobins_DATA) $(assemblies_DATA); do	\
		if test ! -s $(srcdir)/$$i; then echo $$i is empty && exit 1; fi	\
	done

copy_dlls:
	cp /nt/mono/mcs/class/*/*.dll .

push_dlls:
	scp -o "Protocol 1" *.dll *.exe snapshot@mono-cvs.ximian.com:dlls
