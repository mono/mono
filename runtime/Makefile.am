# This is just used to copy and install the DLL files that are currently
# being compiled on windows.
#

SUBDIRS = . net_1_1 net_2_0

mcs_topdir=$(top_srcdir)/../mcs

monobins_DATA = mcs.exe mbas.exe monoresgen.exe secutil.exe ilasm.exe cilc.exe xsd.exe wsdl.exe genxs.exe al.exe disco.exe sqlsharp.exe signcode.exe chktrust.exe soapsuds.exe monop.exe setreg.exe cert2spc.exe certmgr.exe MakeCert.exe gacutil.exe gmcs.exe

EXTRA_DIST = $(monobins_DATA)

#
# Keep in sync with mono/mono/metadata/Makefile.am
#
assembliesdir = $(libdir)
if PLATFORM_WIN32
monobinsdir = $(libdir)
else
monobinsdir = $(bindir)
endif

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
	gacutil.exe)		 d=tools/gacutil		;; \
	gmcs.exe)                d=gmcs				;; \
	sn.exe)                  d=tools/sn                     ;; \
	esac; \
	f=$(mcs_topdir)/$$d/$@; \
	if test -f $$f; then :; else f=$(monobinsdir)/$@; fi ; \
	echo "cp -f $$f $(srcdir)"; \
	cp -f $$f $(srcdir)

dist-hook:
	for i in $(monobins_DATA) $(assemblies_DATA); do        \
		if test ! -s $(srcdir)/$$i; then echo $$i is empty && exit 1; fi	\
	done

copy_dlls:
	cp /nt/mono/mcs/class/*/*.dll .

push_dlls:
	scp -o "Protocol 1" *.dll *.exe snapshot@mono-cvs.ximian.com:dlls
