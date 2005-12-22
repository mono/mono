thisdir = class/Compat.ICSharpCode.SharpZipLib
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Compat.ICSharpCode.SharpZipLib.dll
LIBRARY_NAME = ICSharpCode.SharpZipLib.dll
LIBRARY_COMPAT = yes
LIBRARY_SNK = SharpZipLib.key

LIB_MCS_FLAGS = -r:$(corlib) -r:System.dll -r:System.Xml.dll
NO_TEST = yes
EXTRA_DISTFILES = SharpZipLib.pub $(LIBRARY_SNK)

include ../../build/library.make
