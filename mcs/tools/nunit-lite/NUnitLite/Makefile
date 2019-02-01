thisdir = tools/nunit-lite/NUnitLite

include ../../../build/rules.make

LIBRARY = nunitlite.dll
KEYFILE = nunit.pub
NO_SIGN_ASSEMBLY = 1

ifdef MCS_MODE
KEYFILE = $(topdir)/class/mono.snk
NO_SIGN_ASSEMBLY =
endif

LOCAL_MCS_FLAGS= /target:library /define:"__MOBILE__;TRACE;DEBUG;NET_4_0;CLR_4_0,NUNITLITE" /warn:4 /publicsign
LIB_REFS = System System.Xml System.Core

NO_TEST = yes

EXTRA_DISTFILES =

include ../../../build/library.make

