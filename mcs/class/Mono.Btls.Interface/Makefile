thisdir = class/Mono.Btls.Interface
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Btls.Interface.dll
LIB_REFS = System Mono.Security
LIB_MCS_FLAGS = -unsafe -nowarn:1030 -keyfile:../mono.pub -delaysign -d:SECURITY_DEP

ifndef HAVE_BTLS
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
NO_BUILD = yes
endif

include ../../build/library.make

$(the_lib): ../Mono.Security/Makefile

