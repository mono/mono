thisdir = class/Mono.Data.TdsClient
SUBDIRS =
include ../../build/rules.make

LIBRARY = Mono.Data.TdsClient.dll
LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll /r:System.Xml.dll \
	/r:System.EnterpriseServices.dll /r:System.Data.dll \
	/r:Mono.Data.Tds.dll
NO_TEST = yes

include ../../build/library.make
