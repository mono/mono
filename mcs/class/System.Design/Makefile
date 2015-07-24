thisdir = class/System.Design
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Design.dll

LIB_REFS = System System.Xml System.Web System.Windows.Forms System.Drawing Accessibility System.Data System.Configuration
LIB_MCS_FLAGS = -r:$(corlib)

plainweb_dir = $(the_libdir_base)plainweb
LOCAL_MCS_FLAGS = -lib:$(plainweb_dir)

TEST_MCS_FLAGS = /r:System.dll -r:System.Drawing.dll -r:System.Windows.Forms

include ../../build/library.make

ifneq (plaindesign/,$(intermediate))
$(the_libdir_base)plaindesign/System.Design.dll:
	$(MAKE) intermediate=plaindesign/ $(the_libdir_base)plaindesign/System.Design.dll
endif
