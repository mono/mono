thisdir = tools/nunit-lite/NUnitLite

include ../../../build/rules.make

LIBRARY = nunitlite.dll
LIBRARY_SNK = ../../../../external/nunit-lite/NUnitLite-1.0.0/src/framework/nunit.snk

LOCAL_MCS_FLAGS= /target:library /define:"__MOBILE__;TRACE;DEBUG;NET_4_0;CLR_4_0,NUNITLITE" /warn:4 /keyfile:$(LIBRARY_SNK) /publicsign
LIB_REFS = System System.Xml System.Core

NO_TEST = yes

EXTRA_DISTFILES =

include ../../../build/library.make

