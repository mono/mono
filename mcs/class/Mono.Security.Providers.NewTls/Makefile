thisdir = class/Mono.Security.Providers.NewTls
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Security.Providers.NewTls.dll
LIB_REFS = System NewSystemSource=Mono.Security.Providers.NewSystemSource Mono.Security
LIB_MCS_FLAGS = -unsafe -nowarn:1030 -keyfile:../mono.pub -delaysign

include ../../build/library.make

$(the_lib): ../Mono.Security/Makefile

