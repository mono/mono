thisdir = class/Mono.Globalization
SUBDIRS = 
include ../../build/rules.make

LOCAL_MCS_FLAGS = /r:$(corlib) /r:System.dll
thelib = $(topdir)/class/lib/Mono.Globalization.dll

all-local: $(thelib)

install-local:
	$(MKINSTALLDIRS) $(DESTDIR)$(prefix)/lib
	$(INSTALL_LIB) $(thelib) $(DESTDIR)$(prefix)/lib

clean-local:
	rm -rf $(thelib) MonoCultures.cs write-source.exe

DISTFILES = \
	README			\
	TODO			\
	write-source.cs 	\
	$(wildcard Data/*.xml) 	\
	Data/CultureInfo.xsd

dist-local: dist-default

test-local run-test-local:

$(thelib): MonoCultures.cs
	$(CSCOMPILE) /target:library /out:$@ $^

RUNTIME_FLAGS = --debug

MonoCultures.cs: write-source.exe $(wildcard Data/*.xml)
	$(RUNTIME) $<

write-source.exe: write-source.cs
	$(CSCOMPILE) /target:exe /out:$@ $^
