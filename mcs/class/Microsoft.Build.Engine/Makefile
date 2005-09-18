thisdir = class/Microsoft.Build.Engine
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Microsoft.Build.Engine.dll
LIB_MCS_FLAGS = \
	/r:$(corlib)				\
	/r:System.dll				\
	/r:System.Xml.dll			\
	/r:Microsoft.Build.Framework.dll	\
	/r:Microsoft.Build.Utilities.dll

include ../../build/library.make
