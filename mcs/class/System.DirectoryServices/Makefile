thisdir = class/System.DirectoryServices
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.DirectoryServices.dll
LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll /r:Novell.Directory.Ldap.dll
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:0618 -nowarn:219 -nowarn:169

include ../../build/library.make
