thisdir = class/System.Data.Db2Client
SUBDIRS = 
include ../../build/rules.make

# I have *no idea* why I need to do this.

ifeq ($(PROFILE),atomic)
system = System.dll
else
system = $(topdir)/class/lib/System.dll
endif

LIBRARY = System.Data.Db2Client.dll
LIB_MCS_FLAGS = \
	/unsafe /nowarn:649 /nowarn:169 /nowarn:219 /nowarn:168 \
	/r:$(corlib)				\
	/r:$(system)			 	\
	/r:System.Xml.dll 			\
	/r:System.Data.dll

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

include ../../build/library.make
