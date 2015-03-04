thisdir = class/Mono.Http
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Http.dll
LIB_REFS = System System.Xml System.Web ICSharpCode.SharpZipLib Mono.Security
LIB_MCS_FLAGS = -r:$(corlib) \
		-nowarn:618

NO_TEST = yes

include ../../build/library.make
