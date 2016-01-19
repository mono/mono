thisdir = class/Mono.Security.Providers.NewTls
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Security.Providers.NewTls.dll
LIB_MCS_FLAGS = -unsafe -nowarn:1030 -keyfile:../mono.pub -delaysign -r:System.dll \
	-r:NewSystemSource=Mono.Security.Providers.NewSystemSource.dll -r:Mono.Security.dll

include ../../build/library.make

$(the_lib): ../Mono.Security/Makefile

