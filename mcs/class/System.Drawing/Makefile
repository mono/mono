thisdir = class/System.Drawing
include ../../build/rules.make
SUBDIRS = 

# to use JPEG decoder add -r ./cdeclRedirector/cdeclCallback.dll 
# and remove -define:DECLARE_CDECL_DELEGATES
# cdeclCallback.dll should be manually copied to library folder

LIBRARY = System.Drawing.dll
LIB_MCS_FLAGS = /unsafe /define:DECLARE_CDECL_DELEGATES /r:$(corlib) \
		/r:System.dll /nowarn:649 /nowarn:169

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) /r:System.Drawing.dll 

include ../../build/library.make
