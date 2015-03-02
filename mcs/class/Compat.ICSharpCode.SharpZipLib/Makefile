thisdir = class/Compat.ICSharpCode.SharpZipLib
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Compat.ICSharpCode.SharpZipLib.dll
LIBRARY_NAME = ICSharpCode.SharpZipLib.dll
LIBRARY_COMPAT = yes
LIBRARY_SNK = SharpZipLib.key

LIB_REFS = System System.Xml
LIB_MCS_FLAGS = -r:$(corlib)
NO_TEST = yes
EXTRA_DISTFILES = SharpZipLib.pub $(LIBRARY_SNK)

include ../../build/library.make
