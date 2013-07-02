SUBDIRS = net_2_0

if INSTALL_4_0
SUBDIRS += net_4_0 net_4_5
endif

monodir = $(sysconfdir)/mono

EXTRA_DIST =  	\
	config.in \
	browscap.ini mono.supp mono.d README ChangeLog \
	mono.pc.in mono-2.pc.in monosgen-2.pc.in mint.pc.in dotnet.pc.in dotnet35.pc.in wcf.pc.in monodoc.pc.in \
	mono-nunit.pc.in mono-cairo.pc.in mono-options.pc.in cecil.pc.in \
	mono-lineeditor.pc.in system.web.extensions_1.0.pc.in system.web.extensions.design_1.0.pc.in\
	dtrace-prelink.sh mono.web.pc.in system.web.mvc.pc.in system.web.mvc2.pc.in system.web.mvc3.pc.in aspnetwebstack.pc.in reactive.pc.in \
	net_1_1/machine.config \
	gdb/mono-gdb.py \
	gdb/gdb-python.diff

pkgconfigdir = $(libdir)/pkgconfig

if SUPPORT_SGEN
SGENPCFILE=monosgen-2.pc
else
SGENPCFILE=
endif

if JIT_SUPPORTED
if INTERP_SUPPORTED
pkgconfig_DATA= mono.pc mono-2.pc mint.pc dotnet.pc dotnet35.pc wcf.pc mono-nunit.pc mono-cairo.pc mono-options.pc cecil.pc monodoc.pc mono-lineeditor.pc system.web.extensions_1.0.pc \
		system.web.extensions.design_1.0.pc mono.web.pc system.web.mvc.pc system.web.mvc2.pc system.web.mvc3.pc aspnetwebstack.pc reactive.pc $(SGENPCFILE)
else
pkgconfig_DATA= mono.pc mono-2.pc dotnet.pc dotnet35.pc wcf.pc mono-nunit.pc mono-cairo.pc mono-options.pc cecil.pc monodoc.pc mono-lineeditor.pc system.web.extensions_1.0.pc \
		system.web.extensions.design_1.0.pc mono.web.pc system.web.mvc.pc system.web.mvc2.pc system.web.mvc3.pc aspnetwebstack.pc reactive.pc $(SGENPCFILE)
endif
else
pkgconfig_DATA= mint.pc mono-nunit.pc mono-cairo.pc mono-options.pc cecil.pc monodoc.pc mono-lineeditor.pc
endif

DISTCLEANFILES= mono-2.pc mono.pc mint.pc dotnet.pc dotnet35.pc wcf.pc mono-nunit.pc mono-cairo.pc mono-options.pc cecil.pc monodoc.pc mono-lineeditor.pc system.web.extensions_1.0.pc \
		system.web.extensions.design_1.0.pc mono.web.pc system.web.mvc.pc system.web.mvc2.pc system.web.mvc3.pc aspnetwebstack.pc reactive.pc $(SGENPCFILE) mono-sgen-gdb.py

mono_DATA =  config \
	     browscap.ini

gdbdir = $(bindir)

mono-sgen-gdb.py: gdb/mono-gdb.py
	cp $< $@

gdb_DATA = gdb/mono-gdb.py mono-sgen-gdb.py

