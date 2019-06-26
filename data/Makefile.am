SUBDIRS = net_2_0 net_4_0 net_4_5

monodir = $(sysconfdir)/mono
monolldbdir = $(libdir)/mono/lldb

EXTRA_DIST =  	\
	config.in \
	browscap.ini mono.supp mono.d README \
	mono.pc.in mono-2.pc.in monosgen-2.pc.in mint.pc.in dotnet.pc.in dotnet35.pc.in wcf.pc.in monodoc.pc.in \
	mono-cairo.pc.in mono-options.pc.in cecil.pc.in \
	mono-lineeditor.pc.in system.web.extensions_1.0.pc.in system.web.extensions.design_1.0.pc.in\
	dtrace-prelink.sh system.web.mvc.pc.in system.web.mvc2.pc.in system.web.mvc3.pc.in aspnetwebstack.pc.in \
	reactive.pc.in xbuild12.pc \
	net_1_1/machine.config \
	gdb/mono-gdb.py \
	gdb/gdb-python.diff	\
	lldb/mono.py \
	lldb/monobt.py

pkgconfigdir = $(libdir)/pkgconfig

if SUPPORT_SGEN
SGENPCFILE=monosgen-2.pc
else
SGENPCFILE=
endif

pkgconfig_DATA= mono.pc mono-2.pc dotnet.pc dotnet35.pc wcf.pc mono-cairo.pc mono-options.pc cecil.pc monodoc.pc mono-lineeditor.pc system.web.extensions_1.0.pc \
		system.web.extensions.design_1.0.pc system.web.mvc.pc system.web.mvc2.pc system.web.mvc3.pc aspnetwebstack.pc reactive.pc xbuild12.pc $(SGENPCFILE)

DISTCLEANFILES= mono-2.pc mono.pc mint.pc dotnet.pc dotnet35.pc wcf.pc mono-cairo.pc mono-options.pc cecil.pc monodoc.pc mono-lineeditor.pc system.web.extensions_1.0.pc \
		system.web.extensions.design_1.0.pc system.web.mvc.pc system.web.mvc2.pc system.web.mvc3.pc aspnetwebstack.pc reactive.pc $(SGENPCFILE) mono-sgen-gdb.py

mono_DATA =  config \
	     browscap.ini

monolldb_DATA = lldb/mono.py
gdbdir = $(bindir)

mono-sgen-gdb.py: gdb/mono-gdb.py
	cp $< $@

gdb_DATA = gdb/mono-gdb.py mono-sgen-gdb.py

