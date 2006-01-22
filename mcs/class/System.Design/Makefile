thisdir = class/System.Design
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Design.dll
LIB_MCS_FLAGS = -r:$(corlib) -r:System.dll -r:System.Web.dll \
	-r:System.Windows.Forms.dll -r:System.Drawing.dll \
	-r:Accessibility.dll -r:System.Data.dll

ifeq (net_2_0, $(PROFILE))
LIB_MCS_FLAGS += -r:System.Configuration.dll
endif

NO_TEST = yes

include ../../build/library.make
