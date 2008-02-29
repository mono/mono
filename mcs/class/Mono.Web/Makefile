thisdir = class/Mono.Web
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Web.dll
LIB_MCS_FLAGS = -r:$(corlib) \
		-r:System.dll \
		-r:System.Xml.dll \
		-r:System.Web.dll \
		-r:System.Configuration.dll

NO_TEST = yes

include ../../build/library.make
