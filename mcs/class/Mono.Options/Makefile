thisdir = class/Mono.Options
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Options.dll
LIBRARY_PACKAGE = none

LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll
TEST_MCS_FLAGS = /r:Mono.Posix.dll /r:System.dll

gacdir   = $(GACROOT)/mono/gac/Mono.Options/0.2.1.0__0738eb9f132ed756
gac_DATA = Mono.Options/Options.cs

include ../../build/library.make

do-install:
	$(MAKE) install-local
	$(INSTALL) -m 644 $(gac_DATA) $(gacdir)/

