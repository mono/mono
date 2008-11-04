SUBDIRS = net_1_1 net_2_0

monodir = $(sysconfdir)/mono

EXTRA_DIST =  	\
	config.in \
	browscap.ini mono.supp mono.d README \
	mono.pc.in mint.pc.in dotnet.pc.in dotnet35.pc.in wcf.pc.in monodoc.pc.in \
	mono-nunit.pc.in mono-cairo.pc.in mono-options.pc.in cecil.pc.in smcs.pc.in \
	mono-lineeditor.pc.in

pkgconfigdir = $(libdir)/pkgconfig

if JIT_SUPPORTED
if INTERP_SUPPORTED
pkgconfig_DATA= mono.pc mint.pc dotnet.pc dotnet35.pc wcf.pc mono-nunit.pc mono-cairo.pc mono-options.pc cecil.pc smcs.pc monodoc.pc mono-lineeditor.pc
else
pkgconfig_DATA= mono.pc dotnet.pc dotnet35.pc wcf.pc mono-nunit.pc mono-cairo.pc mono-options.pc cecil.pc smcs.pc monodoc.pc mono-lineeditor.pc
endif
else
pkgconfig_DATA= mint.pc mono-nunit.pc mono-cairo.pc mono-options.pc cecil.pc smcs.pc monodoc.pc mono-lineeditor.pc
endif

DISTCLEANFILES= mono.pc mint.pc dotnet.pc dotnet35.pc wcf.pc mono-nunit.pc mono-cairo.pc mono-options.pc cecil.pc smcs.pc monodoc.pc mono-lineeditor.pc

mono_DATA =  config \
	     browscap.ini

