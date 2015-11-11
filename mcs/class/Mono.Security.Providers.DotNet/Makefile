thisdir = class/Mono.Security.Providers.DotNet
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Security.Providers.DotNet.dll
LIB_REFS = System Mono.Security
LIB_MCS_FLAGS = -unsafe -nowarn:1030 -keyfile:../mono.pub -delaysign

EXTRA_DISTFILES = README.md

include ../../build/library.make

