thisdir = class/Mono.Data.SybaseClient
SUBDIRS =
include ../../build/rules.make

LIBRARY = Mono.Data.SybaseClient.dll
LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll /r:System.Xml.dll \
	/r:System.EnterpriseServices.dll /r:System.Data.dll \
	/r:Mono.Data.Tds.dll
NO_TEST = yes

include ../../build/library.make
