thisdir = class/Microsoft.VisualBasic
SUBDIRS =

include ../../build/rules.make

LIBRARY = Microsoft.VisualBasic.dll
LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll /r:System.Windows.Forms.dll @Microsoft.VisualBasic.dll.resources -codepage:65001
TEST_MCS_FLAGS = -codepage:65001

LIB_MCS_FLAGS_RESOURCES = $(LIB_MCS_FLAGS) 

TXT_RES = Microsoft.VisualBasic/VBUtils.resources
#RESX_RES = 

EXTRA_DISTFILES = Microsoft.VisualBasic.dll.resources $(TXT_RES:.resources=.txt) #$(RESX_RES:.resources=.resx)
CLEAN_FILES = $(TXT_RES) #$(RESX_RES)

include ../../build/library.make

$(the_lib): $(TXT_RES) #$(RESX_RES)

$(TXT_RES): %.resources: %.txt
	$(RESGEN) `echo $< | $(PLATFORM_CHANGE_SEPARATOR_CMD)`

#$(RESX_RES): %.resources: %.resx
#	$(RESGEN) `echo $< | $(PLATFORM_CHANGE_SEPARATOR_CMD)`
