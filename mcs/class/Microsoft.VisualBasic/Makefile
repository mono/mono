thisdir = class/Microsoft.VisualBasic
SUBDIRS =

include ../../build/rules.make

LIBRARY = Microsoft.VisualBasic.dll
LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll /r:System.Windows.Forms.dll @Microsoft.VisualBasic.dll.resources 

LIB_MCS_FLAGS_RESOURCES = $(LIB_MCS_FLAGS) 

TXT_RESOURCES = Microsoft.VisualBasic/VBUtils.resources
RESX_RESOURCES = 

EXTRA_DISTSFILES = $(TXT_RESOURCES:.resources=.txt) $(RESX_RESOURCES:.resources=.resx)
CLEAN_FILES = $(TXT_RESOURCES) $(RESX_RESOURCES)

include ../../build/library.make

$(the_lib): $(TXT_RESOURCES) $(RESX_RESOURCES)

$(TXT_RESOURCES): %.resources: %.txt
	$(RESGEN) $<

$(RESX_RESOURCES): %.resources: %.resx
	$(RESGEN) $<
