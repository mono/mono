thisdir = tools/nunit-lite/NUnitLite

include ../../../build/rules.make

LIBRARY = nunitlite.dll
KEYFILE = nunit.pub

LOCAL_MCS_FLAGS= /target:library /define:"__MOBILE__;TRACE;DEBUG;NET_4_0;CLR_4_0,NUNITLITE" /warn:4 /delaysign
LIB_REFS = System System.Xml System.Core

NO_TEST = yes

EXTRA_DISTFILES = nunit.pub

include ../../../build/library.make

