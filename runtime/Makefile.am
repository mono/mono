#
# This is just used to copy and install the DLL files that are currently
# being compiled on windows.
#

assemblies_DATA =	\
	Accessibility.dll					\
	corlib.dll						\
	Cscompmgd.dll						\
	I18N.CJK.dll						\
	I18N.dll						\
	I18N.MidEast.dll					\
	I18N.Other.dll						\
	I18N.Rare.dll						\
	I18N.West.dll						\
	Microsoft.VisualBasic.dll				\
	Mono.CSharp.Debugger.dll				\
	Mono.Data.MySql.dll					\
	Mono.Data.PostgreSqlClient.dll				\
	Mono.Data.SqliteClient.dll				\
	Mono.Data.SybaseClient.dll				\
	Mono.Data.TdsClient.dll					\
	Mono.Data.Tds.dll					\
	Mono.Data.DB2Client.dll					\
	Mono.GetOptions.dll					\
	Mono.PEToolkit.dll					\
	NUnitCore_mono.dll					\
	System.Configuration.Install.dll			\
	System.Data.dll						\
	System.dll						\
	System.Drawing.dll					\
	System.EnterpriseServices.dll				\
	System.Runtime.Remoting.dll				\
	System.Runtime.Serialization.Formatters.Soap.dll	\
	System.Web.dll						\
	System.Web.Services.dll					\
	System.Windows.Forms.dll				\
	System.Xml.dll

monobins_DATA = mcs.exe mbas.exe monoresgen.exe secutil.exe sqlsharp.exe

EXTRA_DIST = $(monobins_DATA) $(assemblies_DATA)

#
# Keep in sync with mono/mono/metadata/Makefile.am
#
assembliesdir = $(libdir)
monobinsdir = $(bindir)

$(assemblies_DATA):
	-cp $(top_srcdir)/../mcs/class/lib/$@ . || cp $(assembliesdir)/$@ . 

$(monobins_DATA):
	-cp $(top_srcdir)/../mcs/mcs/mcs.exe . || cp $(monobinsdir)/$@ . 
	-cp $(top_srcdir)/../mcs/mbas/mbas.exe . || cp $(monobinsdir)/$@ .
	-cp $(top_srcdir)/../mcs/tools/secutil.exe . || cp $(monobinsdir)/$@ . 
	-cp $(top_srcdir)/../mcs/tools/SqlSharp/sqlsharp.exe . || cp $(monobinsdir)/$@ . 
	-cp $(top_srcdir)/../mcs/monoresgen/monoresgen.exe . || cp $(monobinsdir)/$@ . 

dist-hook:
	for i in $(monobins_DATA) $(assemblies_DATA); do	\
		if test ! -s $(srcdir)/$$i; then echo $$i is empty && exit 1; fi	\
	done

copy_dlls:
	cp /nt/mono/mcs/class/*/*.dll .

push_dlls:
	scp -o "Protocol 1" *.dll *.exe snapshot@mono-cvs.ximian.com:dlls
