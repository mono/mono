#
# This is just used to copy and install the DLL files that are currently
# being compiled on windows.
#

assemblies_DATA =	\
	corlib.dll	\
	System.dll	\
	System.Data.dll	\
	System.Xml.dll	\
	System.Web.dll	\
	System.Drawing.dll

monobins_DATA = mcs.exe

EXTRA_DIST = $(monobins_DATA) $(assemblies_DATA)

#
# Keep in sync with mono/mono/metadata/Makefile.am
#
assembliesdir = $(libdir)
monobinsdir = $(bindir)

$(assemblies_DATA):
	cp $(top_srcdir)/../mcs/class/lib/$@ . || cp $(assembliesdir)/$@ . || touch $@

$(monobins_DATA):
	cp $(top_srcdir)/../mcs/mcs/$@ . || cp $(monobinsdir)/$@ . || touch $@

dist-hook:
	for i in $(monobins_DATA) $(assemblies_DATA); do	\
		if test ! -s $(srcdir)/$$i; then echo $$i is empty && exit 1; fi	\
	done

copy_dlls:
	cp /nt/mono/mcs/class/*/*.dll .

push_dlls:
	scp -o "Protocol 1" *.dll *.exe snapshot@mono-cvs.ximian.com:dlls
