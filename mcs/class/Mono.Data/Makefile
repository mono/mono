thisdir = class/Mono.Data
SUBDIRS =
include ../../build/rules.make

LIBRARY = Mono.Data.dll
LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll /r:System.Xml.dll \
    /r:System.Data.dll
NO_TEST = yes

include ../../build/library.make
