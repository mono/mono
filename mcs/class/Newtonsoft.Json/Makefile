thisdir = class/Newtonsoft.Json
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Newtonsoft.Json.dll

LIB_MCS_FLAGS = -r:System.Core.dll -r:System.dll -r:System.Runtime.Serialization.dll -r:System.Xml.dll -r:System.Xml.Linq.dll -r:System.Data.dll \
		-d:SIGNED -keyfile:../mono.pub -delaysign

include ../../build/library.make
