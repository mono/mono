thisdir = class/Mono.Management
SUBDIRS =
include ../../build/rules.make

LIBRARY = Mono.Management.dll
LIBRARY_USE_INTERMEDIATE_FILE = yes

LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll /r:Mono.Posix.dll
NO_TEST = yes

include ../../build/library.make
