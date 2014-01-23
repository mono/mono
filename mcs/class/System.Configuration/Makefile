thisdir = class/System.Configuration
SUBDIRS =
include ../../build/rules.make

LIBRARY = System.Configuration.dll
LIBRARY_USE_INTERMEDIATE_FILE = yes

LIB_MCS_FLAGS = -r:$(corlib) -r:System.dll -r:System.Xml.dll -r:System.Security.dll -nowarn:618
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

include ../../build/library.make
