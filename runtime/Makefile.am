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
	@case "$@" in \
	mcs.exe)		 d=mcs				;; \
	mcs.exe.mdb)		 d=mcs				;; \
	mcs.exe.config)		 d=mcs				;; \
	mbas.exe)		 d=mbas				;; \
	mbas.exe.mdb)		 d=mbas				;; \
	monoresgen.exe)          d=monoresgen                   ;; \
	monoresgen.exe.mdb)          d=monoresgen                   ;; \
	ilasm.exe)               d=ilasm                        ;; \
	ilasm.exe.mdb)               d=ilasm                        ;; \
	cilc.exe)                d=tools/cilc                   ;; \
	cilc.exe.mdb)                d=tools/cilc                   ;; \
	xsd.exe)                 d=tools/mono-xsd               ;; \
	xsd.exe.mdb)                 d=tools/mono-xsd               ;; \
	wsdl.exe)                d=tools/wsdl                   ;; \
	wsdl.exe.mdb)                d=tools/wsdl                   ;; \
	genxs.exe)               d=tools/genxs                  ;; \
	genxs.exe.mdb)               d=tools/genxs                  ;; \
	al.exe)                  d=tools/al                     ;; \
	al.exe.mdb)                  d=tools/al                     ;; \
	disco.exe)               d=tools/disco                  ;; \
	disco.exe.mdb)               d=tools/disco                  ;; \
	soapsuds.exe)            d=tools/soapsuds               ;; \
	soapsuds.exe.mdb)            d=tools/soapsuds               ;; \
	sqlsharp.exe)            d=tools/SqlSharp               ;; \
	sqlsharp.exe.mdb)            d=tools/SqlSharp               ;; \
	chktrust.exe)            d=tools/security               ;; \
	chktrust.exe.mdb)            d=tools/security               ;; \
	signcode.exe)            d=tools/security               ;; \
	signcode.exe.mdb)            d=tools/security               ;; \
	MakeCert.exe)            d=tools/security               ;; \
	MakeCert.exe.mdb)            d=tools/security               ;; \
	cert2spc.exe)            d=tools/security               ;; \
	cert2spc.exe.mdb)            d=tools/security               ;; \
	certmgr.exe)             d=tools/security               ;; \
	certmgr.exe.mdb)             d=tools/security               ;; \
	secutil.exe)             d=tools/security               ;; \
	secutil.exe.mdb)             d=tools/security               ;; \
	setreg.exe)              d=tools/security               ;; \
	setreg.exe.mdb)              d=tools/security               ;; \
	sn.exe)                  d=tools/security               ;; \
	sn.exe.mdb)                  d=tools/security               ;; \
	monop.exe)               d=tools/monop                  ;; \
	monop.exe.mdb)               d=tools/monop                  ;; \
	mono-find-provides.exe)	 d=tools/mono-rpm-helpers/mono-find-provides	;; \
	mono-find.mdb-provides.exe)	 d=tools/mono-rpm-helpers/mono-find-provides	;; \
	mono-find-requires.exe)	 d=tools/mono-rpm-helpers/mono-find-requires	;; \
	mono-find.mdb-requires.exe)	 d=tools/mono-rpm-helpers/mono-find-requires	;; \
	browsercaps-updater.exe) d=tools/browsercaps-updater    ;; \
	browsercaps-updater.mdb.exe) d=tools/browsercaps-updater    ;; \
	gacutil.exe)		 d=tools/gacutil		;; \
	gacutil.exe.mdb)		 d=tools/gacutil		;; \
	gmcs.exe)                d=gmcs				;; \
	gmcs.exe.mdb)                d=gmcs				;; \
	gmcs.exe.config)         d=gmcs				;; \
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
