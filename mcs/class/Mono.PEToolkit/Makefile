thisdir = class/Mono.PEToolkit
SUBDIRS = build
include ../../build/rules.make

LIBRARY = Mono.PEToolkit.dll
LIB_MCS_FLAGS = /unsafe /r:$(corlib) /r:System.Xml.dll
NO_TEST = yes

EXTRA_DISTFILES = README

mddump.exe:
	@echo $@ now lives in build/. Email 'peter@newton.cx'
	@echo if this change is a problem.

include ../../build/library.make
