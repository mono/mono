thisdir = class/Microsoft.VisualBasic
SUBDIRS = fixup

include ../../build/rules.make

# Build current directory before subdirs
all-recursive: all-local
$(STD_TARGETS:%=%-recursive): %-recursive: %-local

# The actual library will be built and signed in the 'fixup' directory
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes

LIBRARY = Microsoft.VisualBasic.dll
LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll /r:System.Windows.Forms.dll @Microsoft.VisualBasic.dll.resources -codepage:65001
TEST_MCS_FLAGS = -codepage:65001 -nowarn:0618 -nowarn:219 -nowarn:169

LIB_MCS_FLAGS_RESOURCES = $(LIB_MCS_FLAGS) 

TXT_RES = Microsoft.VisualBasic/VBUtils.resources
#RESX_RES = 

EXTRA_DISTFILES = Microsoft.VisualBasic.dll.resources $(TXT_RES:.resources=.txt) #$(RESX_RES:.resources=.resx)
CLEAN_FILES = $(TXT_RES) #$(RESX_RES)

# Testcases are run against the fixed-up library
test_against = fixup/$(PROFILE)/$(LIBRARY)
test_dep = $(test_against)

include ../../build/library.make

$(the_lib): $(TXT_RES) #$(RESX_RES)

$(TXT_RES): %.resources: %.txt
	$(RESGEN) `echo $< | $(PLATFORM_CHANGE_SEPARATOR_CMD)`

#$(RESX_RES): %.resources: %.resx
#	$(RESGEN) `echo $< | $(PLATFORM_CHANGE_SEPARATOR_CMD)`

$(test_dep): $(the_lib)
	cd fixup && $(MAKE)
