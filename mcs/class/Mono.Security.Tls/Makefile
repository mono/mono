OVERRIDE_BARE_TARGETS = yes
thisdir = class/Mono.Security.Tls
LIBRARY = System.Security.Tls.dll
NO_TEST = yes
include ../../build/rules.make

all:
	@cp $(LIBRARY) ../../class/lib

install:
	$(MKINSTALLDIRS) $(DESTDIR)$(prefix)/lib
	$(INSTALL_LIB) $(LIBRARY) $(DESTDIR)$(prefix)/lib

test:

run-test:

clean:

include ../../build/library.make
