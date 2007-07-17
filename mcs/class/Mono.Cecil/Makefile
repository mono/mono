thisdir = class/Mono.Cecil
include ../../build/rules.make

LIBRARY = Mono.Cecil.dll
LIBRARY_SNK = ../mono.snk

LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll -keyfile:$(LIBRARY_SNK)

NO_TEST = yes

include ../../build/library.make

install-local:
	$(GACUTIL) /i $(the_lib) /f $(gacdir_flag) /root $(GACROOT)
