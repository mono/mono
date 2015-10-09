thisdir = class/Mono.Security.Providers/DotNet
SUBDIRS = 
include ../../../build/rules.make

LIBRARY = Mono.Security.Providers.DotNet.dll
LIB_MCS_FLAGS = -r:System.dll -r:Mono.Security -unsafe -nowarn:1030 -keyfile:../../mono.pub -delaysign
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:169,219,618,672

include ../../../build/library.make

