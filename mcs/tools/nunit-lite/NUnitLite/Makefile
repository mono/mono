thisdir = tools/nunit-lite/NUnitLite

include ../../../build/rules.make

LIBRARY = nunitlite.dll
LIBRARY_SNK = ../../../../external/nunit-lite/NUnitLite-1.0.0/src/framework/nunit.snk

LOCAL_MCS_FLAGS= /target:library /define:"__MOBILE__;TRACE;DEBUG;NET_4_0;CLR_4_0,NUNITLITE" /nostdlib -r:System.dll -r:System.Xml.dll -r:System.Core.dll -r:mscorlib.dll /warn:4 /noconfig /debug:full /debug+ /keyfile:$(LIBRARY_SNK) /optimize

NO_TEST = yes

EXTRA_DISTFILES =

include ../../../build/library.make

