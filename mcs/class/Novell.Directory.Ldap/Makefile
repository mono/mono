thisdir = class/Novell.Directory.Ldap
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Novell.Directory.Ldap.dll
LIB_MCS_FLAGS = \
	/nowarn:649 /nowarn:169 /nowarn:219 /nowarn:168 \
	-r:$(corlib)				\
	-r:System.dll			 	\
	-r:Mono.Security.dll

ifeq (net_1_1_java, $(PROFILE))
LIB_MCS_FLAGS +=	\
	-r:rt.dll				\
	-r:J2SE.Helpers.dll
endif

NO_TEST = yes

include ../../build/library.make
