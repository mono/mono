# makes monodoc.dll

include ../Makefile.defs

CFLAGS  := $(CFLAGS) -target:library
SOURCES  = archiver.cs emember.cs member.cs param.cs parser.cs seealso.cs type.cs

all: monodoc.dll

monodoc.dll: $(SOURCES)
	$(MONO) $(MCS) $(CFLAGS) $(SOURCES) -o $@

clean:
	-rm monodoc.dll

install: monodoc.dll
	$(MAKE) uninstall
	install -o root -g root -m 0644 monodoc.dll $(PREFIX)/$(LIBDIR)/monodoc.dll

uninstall:
	-rm -rf $(PREFIX)/$(LIBDIR)/monodoc.dll

.PHONY: all clean install uninstall
