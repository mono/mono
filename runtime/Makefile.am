# This is just used to copy and install the DLL files that are currently
# being compiled on windows.
#

SUBDIRS = . net_1_1 net_2_0

mcs_topdir=$(top_srcdir)/../mcs

monobins_DATA = \
	monoresgen.exe				\
	secutil.exe				\
	ilasm.exe				\
	cilc.exe				\
	xsd.exe					\
	wsdl.exe				\
	genxs.exe				\
	al.exe					\
	disco.exe				\
	sqlsharp.exe				\
	signcode.exe				\
	chktrust.exe				\
	soapsuds.exe				\
	monop.exe				\
	mono-find-provides.exe			\
	mono-find-requires.exe 			\
	setreg.exe				\
	cert2spc.exe				\
	certmgr.exe				\
	MakeCert.exe				\
	gacutil.exe				\
	sn.exe

monoone_DATA = \
	mcs.exe mcs.exe.config			\
	mbas.exe

if ! PLATFORM_WIN32
gmcs_exe = gmcs.exe gmcs.exe.config
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
	mcs.exe.config)		 d=mcs				;; \
	mbas.exe)		 d=mbas				;; \
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
	secutil.exe)             d=tools/security               ;; \
	setreg.exe)              d=tools/security               ;; \
	sn.exe)                  d=tools/security               ;; \
	monop.exe)               d=tools/monop                  ;; \
	mono-find-provides.exe)	 d=tools/mono-rpm-helpers/mono-find-provides	;; \
	mono-find-requires.exe)	 d=tools/mono-rpm-helpers/mono-find-requires	;; \
	browsercaps-updater.exe) d=tools/browsercaps-updater    ;; \
	gacutil.exe)		 d=tools/gacutil		;; \
	gmcs.exe)                d=gmcs				;; \
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
