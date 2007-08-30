SUBDIRS = net_1_1 net_2_0

monodir = $(sysconfdir)/mono

EXTRA_DIST =  	\
	config.in \
	browscap.ini mono.supp README \
	mono.pc.in mint.pc.in dotnet.pc.in mono-cairo.pc.in cecil.pc.in

pkgconfigdir = $(libdir)/pkgconfig

if JIT_SUPPORTED
if INTERP_SUPPORTED
pkgconfig_DATA= mono.pc mint.pc dotnet.pc mono-cairo.pc cecil.pc
else
pkgconfig_DATA= mono.pc dotnet.pc mono-cairo.pc cecil.pc
endif
else
pkgconfig_DATA= mint.pc mono-cairo.pc cecil.pc
endif

DISTCLEANFILES= mono.pc mint.pc dotnet.pc mono-cairo.pc cecil.pc

mono_DATA =  config \
	     browscap.ini

