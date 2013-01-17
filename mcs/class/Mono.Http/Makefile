thisdir = class/Mono.Http
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Http.dll
LIB_MCS_FLAGS = -r:$(corlib) \
		-r:System.dll \
		-r:System.Xml.dll \
		-r:System.Web.dll \
		-r:ICSharpCode.SharpZipLib.dll \
		-r:Mono.Security.dll \
		-nowarn:618

NO_TEST = yes

include ../../build/library.make
