thisdir = class/System.Drawing
SUBDIRS = 
include ../../build/rules.make

# to use JPEG decoder add -r ./cdeclRedirector/cdeclCallback.dll 
# and remove -define:DECLARE_CDECL_DELEGATES
# cdeclCallback.dll should be manually copied to library folder

LIBRARY = System.Drawing.dll
LIB_MCS_FLAGS = /unsafe /define:DECLARE_CDECL_DELEGATES /r:$(corlib) \
		/r:System.dll /nowarn:649 /nowarn:169
NO_TEST = yes

include ../../build/library.make
