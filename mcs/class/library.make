MCS = mono $(topdir)/mcs/mcs.exe
MCS_FLAGS = --target library --noconfig
INSTALL = /usr/bin/install
prefix = /usr

SOURCES_CMD=find . \
	  \( -false $(SOURCES_INCLUDE:%=-o -path '%') \) -a	 \
	! \( -false $(SOURCES_EXCLUDE:%=-o -path '%') \)

all: .makefrag $(LIBRARY)

clean:
	-rm -rf $(LIBRARY) .response .makefrag

.PHONY: .makefrag
.makefrag:
	@echo -n "SOURCES=" >$@
	@$(SOURCES_CMD) | tee .response | sed -e 's/$$/ \\/' >>$@

-include .makefrag

$(LIBRARY): $(SOURCES) $(topdir)/class/library.make
	MONO_PATH=$(MONO_PATH_PREFIX)$(MONO_PATH) $(MCS) $(MCS_FLAGS) -o $(LIBRARY) $(LIB_FLAGS) @.response

install: all
	mkdir -p $(prefix)/lib/
	$(INSTALL) -m 644 $(LIBRARY) $(prefix)/lib/

