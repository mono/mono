topdir=..
MCS = ../mcs/mcs.exe
MCS_FLAGS = /target:exe $(MCS_DEFINES)
INSTALL = /usr/bin/install
prefix = /usr
RUNTIME=mono
MONO_PATH_PREFIX=$(topdir)/class/lib:

all: nant.exe

nant.exe: makefile.gnu src/*.cs src/Attributes/*.cs src/Tasks/*.cs src/Util/*.cs
	MONO_PATH=$(MONO_PATH_PREFIX)$(MONO_PATH) $(RUNTIME) $(MCS) $(MCSFLAGS) /out:nant.exe /recurse:*.cs

install: all
	mkdir -p $(prefix)/bin/
	$(INSTALL) -m 755 nant.exe $(prefix)/bin/

clean:
	rm -f nant.exe
