MCS = csc
MCS_FLAGS = /target:exe $(MCS_DEFINES)
INSTALL = /usr/bin/install
prefix = /usr
RUNTIME=

SOURCES=monoresgen.cs

all:
	if test x$(OS) = xWindows_NT; then make linux; else make -f makefile.gnu; fi

linux: monoresgen.exe

windows: monoresgen.exe

monoresgen.exe: $(SOURCES)
	$(RUNTIME) $(MCS) $(MCS_FLAGS) /out:$@ $(SOURCES)

install: all
	mkdir -p $(prefix)/bin/
	$(INSTALL) -m 755 monoresgen.exe $(prefix)/bin/

test:

clean:
	rm -f monoresgen.exe
