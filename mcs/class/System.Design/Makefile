thisdir = class/System.Design
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Design.dll

LIB_REFS = System System.Xml System.Windows.Forms System.Drawing Accessibility System.Data System.Configuration
LIB_MCS_FLAGS =

TEST_LIB_REFS = System System.Drawing System.Windows.Forms
TEST_MCS_FLAGS =

include ../../build/library.make

ifeq (plaindesign/,$(intermediate))
LIB_REFS += System.Web
else
csproj-local:
	$(MAKE) csproj-local intermediate=plaindesign/

LIB_REFS += plainweb/System.Web
$(the_libdir_base)plaindesign/System.Design.dll:
	$(MAKE) intermediate=plaindesign/ $(the_libdir_base)plaindesign/System.Design.dll
endif

