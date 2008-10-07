SUBDIRS = net_1_1 net_2_0

monodir = $(sysconfdir)/mono

EXTRA_DIST =  	\
	config.in \
	browscap.ini mono.supp mono.d README \
	mono.pc.in mint.pc.in dotnet.pc.in dotnet35.pc.in wcf.pc.in \
	mono-nunit.pc.in mono-cairo.pc.in cecil.pc.in smcs.pc.in

pkgconfigdir = $(libdir)/pkgconfig

if JIT_SUPPORTED
if INTERP_SUPPORTED
pkgconfig_DATA= mono.pc mint.pc dotnet.pc dotnet35.pc wcf.pc mono-nunit.pc mono-cairo.pc cecil.pc smcs.pc
else
pkgconfig_DATA= mono.pc dotnet.pc dotnet35.pc wcf.pc mono-cairo.pc cecil.pc smcs.pc
endif
else
pkgconfig_DATA= mint.pc mono-cairo.pc cecil.pc smcs.pc
endif

DISTCLEANFILES= mono.pc mint.pc dotnet.pc dotnet35.pc wcf.pc mono-nunit.pc mono-cairo.pc cecil.pc smcs.pc

mono_DATA =  config \
	     browscap.ini

