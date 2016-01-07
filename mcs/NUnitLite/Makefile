thisdir = NUnitLite
SUBDIRS = nunit-lite-console
include ../build/rules.make

LIBRARY = nunitlite.dll
LIBRARY_SNK = $(topdir)/NUnitLite/src/framework/nunit.snk

LOCAL_MCS_FLAGS= /target:library /define:"__MOBILE__;TRACE;DEBUG;NET_4_0,;CLR_4_0,NUNITLITE" /nostdlib -r:System.dll -r:System.Xml.dll -r:System.Core.dll -r:mscorlib.dll /warn:4 /noconfig /debug:full /debug+ /keyfile:src/framework/nunit.snk /optimize

NO_TEST = yes

EXTRA_DISTFILES =

include ../build/library.make

# src/framework/obj/Debug/net-3.0/.NETFramework,Version=v4.0.AssemblyAttribute.cs

