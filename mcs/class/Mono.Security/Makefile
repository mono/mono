thisdir = class/Mono.Security
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Security.dll
LIB_MCS_FLAGS = /r:System.dll /unsafe
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) /nowarn:0618 /nowarn:0672

include ../../build/library.make
