SUBDIRS = net_1_1 net_2_0

if INSTALL_4_0
SUBDIRS += net_4_0
endif

monodir = $(sysconfdir)/mono

EXTRA_DIST =  	\
	config.in \
	browscap.ini mono.supp mono.d README \
	mono.pc.in mint.pc.in dotnet.pc.in dotnet35.pc.in wcf.pc.in monodoc.pc.in \
	mono-nunit.pc.in mono-cairo.pc.in mono-options.pc.in cecil.pc.in \
	mono-lineeditor.pc.in system.web.extensions_1.0.pc.in system.web.extensions.design_1.0.pc.in\
	dtrace-prelink.sh mono.web.pc.in system.web.mvc.pc.in \
	gdb/mono-gdb.py

pkgconfigdir = $(libdir)/pkgconfig

if JIT_SUPPORTED
if INTERP_SUPPORTED
pkgconfig_DATA= mono.pc mint.pc dotnet.pc dotnet35.pc wcf.pc mono-nunit.pc mono-cairo.pc mono-options.pc cecil.pc monodoc.pc mono-lineeditor.pc system.web.extensions_1.0.pc \
		system.web.extensions.design_1.0.pc mono.web.pc system.web.mvc.pc
else
pkgconfig_DATA= mono.pc dotnet.pc dotnet35.pc wcf.pc mono-nunit.pc mono-cairo.pc mono-options.pc cecil.pc monodoc.pc mono-lineeditor.pc system.web.extensions_1.0.pc \
		system.web.extensions.design_1.0.pc mono.web.pc system.web.mvc.pc
endif
else
pkgconfig_DATA= mint.pc mono-nunit.pc mono-cairo.pc mono-options.pc cecil.pc monodoc.pc mono-lineeditor.pc
endif

DISTCLEANFILES= mono.pc mint.pc dotnet.pc dotnet35.pc wcf.pc mono-nunit.pc mono-cairo.pc mono-options.pc cecil.pc monodoc.pc mono-lineeditor.pc system.web.extensions_1.0.pc \
		system.web.extensions.design_1.0.pc mono.web.pc system.web.mvc.pc

mono_DATA =  config \
	     browscap.ini

gdbdir = $(bindir)

gdb_DATA = gdb/mono-gdb.py
