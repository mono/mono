thisdir = class/System.Design
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Design.dll


LIB_MCS_FLAGS = -r:$(corlib) -r:System.dll -r:System.Xml.dll -r:System.Web.dll \
	-r:System.Windows.Forms.dll -r:System.Drawing.dll \
	-r:Accessibility.dll -r:System.Data.dll \
	-r:System.Configuration.dll

TEST_MCS_FLAGS = /r:System.dll -r:System.Drawing.dll -r:System.Windows.Forms

include ../../build/library.make

