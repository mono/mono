# This is just used to copy and install the DLL files that are currently
# being compiled on windows.
#

SUBDIRS = . net_1_1 net_2_0

mcs_topdir=$(top_srcdir)/../mcs

monobins_DATA = \
	monoresgen.exe				\
	monoresgen.exe.mdb			\
	secutil.exe				\
	secutil.exe.mdb			\
	ilasm.exe				\
	ilasm.exe.mdb				\
	cilc.exe				\
	cilc.exe.mdb				\
	xsd.exe					\
	xsd.exe.mdb				\
	wsdl.exe				\
	wsdl.exe.mdb				\
	wsdl2.exe				\
	wsdl2.exe.mdb				\
	genxs.exe				\
	genxs.exe.mdb				\
	al.exe					\
	al.exe.mdb				\
	disco.exe				\
	disco.exe.mdb				\
	sqlsharp.exe				\
	sqlsharp.exe.mdb			\
	signcode.exe				\
	signcode.exe.mdb			\
	chktrust.exe				\
	chktrust.exe.mdb			\
	soapsuds.exe				\
	soapsuds.exe.mdb			\
	monop.exe				\
	monop.exe.mdb				\
	mono-find-provides.exe			\
	mono-find-provides.exe.mdb		\
	mono-find-requires.exe		\
	mono-find-requires.exe.mdb		\
	setreg.exe				\
	setreg.exe.mdb			\
	cert2spc.exe				\
	cert2spc.exe.mdb			\
	certmgr.exe				\
	certmgr.exe.mdb			\
	MakeCert.exe				\
	MakeCert.exe.mdb			\
	gacutil.exe				\
	gacutil.exe.mdb			\
	sn.exe					\
	sn.exe.mdb

monoone_DATA = \
	mcs.exe				\
	mcs.exe.mdb				\
	mcs.exe.config			\
	mbas.exe

if ! PLATFORM_WIN32
gmcs_exe = gmcs.exe gmcs.exe.config gmcs.exe.mdb
endif
monotwo_DATA = $(gmcs_exe)

EXTRA_DIST= $(monobins_DATA) $(monoone_DATA) $(monotwo_DATA)

MAINTAINERCLEANFILES = $(EXTRA_DIST)

#
# Keep in sync with mono/mono/metadata/Makefile.am
#
if PLATFORM_WIN32
monobinsdir = $(libdir)
monoonedir = $(libdir)
monotwodir = $(libdir)
else
monobinsdir = $(bindir)
monoonedir = $(libdir)/mono/1.0/
monotwodir = $(libdir)/mono/2.0/
endif

$(monoone_DATA) $(monotwo_DATA) $(monobins_DATA):
	@name=`echo "$@" | sed 's,\.exe.*$$,,'`; \
	case $$name in \
	mcs | mbas | monoresgen | gmcs | ilasm) 	d=$$name 					 ;; \
	cilc | genxs | al | disco | soapsuds | monop | browsercaps-updater | gacutil)	d=tools/$$name 	 ;; \
	chktrust | signcode | MakeCert | cert2spc | certmgr | secutil | setreg | sn)	d=tools/security ;; \
	mono-find-provides | mono-find-requires)	d=tools/mono-rpm-helpers/$$name 		 ;; \
	xsd)      					d=tools/mono-xsd 				 ;; \
	wsdl*)    					d=tools/wsdl					 ;; \
	sqlsharp) 					d=tools/SqlSharp 				 ;; \
	esac; \
	echo "test -f $(top_srcdir)/../mcs/$$d/$@" ; \
	test -f $(top_srcdir)/../mcs/$$d/$@ || exit 1 ; \
	echo "rm -f $(srcdir)/$@" ; \
	rm -f $(srcdir)/$@ ; \
	echo "cd $(srcdir) && $(LN_S) ../../mcs/$$d/$@ $@" ; \
	cd $(srcdir) && $(LN_S) ../../mcs/$$d/$@ $@

copy_dlls:
	cp /nt/mono/mcs/class/*/*.dll .

push_dlls:
	scp -o "Protocol 1" *.dll *.exe snapshot@mono-cvs.ximian.com:dlls

cleanassemblies:
	find -name \*.dll -exec rm -vf {} \;
	find -name \*.exe -exec rm -vf {} \;
