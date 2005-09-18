thisdir = class/Microsoft.Build.Tasks
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Microsoft.Build.Tasks.dll
LIB_MCS_FLAGS = \
	/r:$(corlib)				\
	/r:System.dll				\
	/r:System.Windows.Forms.dll		\
	/r:Microsoft.Build.Utilities.dll	\
	/r:Microsoft.Build.Framework.dll

include ../../build/library.make
