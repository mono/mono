thisdir = class/PlayScript.Core
include ../../build/rules.make

LIBRARY = PlayScript.Core.dll
LIBRARY_SNK = ../mono.snk
LIBRARY_PACKAGE = none

LIB_MCS_FLAGS = -d:DYNAMIC_SUPPORT -keyfile:$(LIBRARY_SNK) -delaysign -r:System.dll -r:System.Core.dll -r:System.Xml.dll -r:Microsoft.CSharp.dll -r:Mono.CSharp.dll

NO_TEST = yes

include ../../build/library.make
