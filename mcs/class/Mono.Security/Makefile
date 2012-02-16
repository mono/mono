thisdir = class/Mono.Security
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Security.dll
LIB_MCS_FLAGS = -r:System.dll -unsafe -nowarn:1030
LIBRARY_USE_INTERMEDIATE_FILE = yes
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:169,219,618,672

include ../../build/library.make
