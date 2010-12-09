thisdir = class/System.Web.Extensions.Design
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Web.Extensions.Design.dll

LIB_MCS_FLAGS = \
	-define:NET_3_5			\
	-r:System.dll -r:System.Design.dll -r:System.Drawing.dll \
	$(OTHER_LIB_MCS_FLAGS)

include ../../build/library.make

