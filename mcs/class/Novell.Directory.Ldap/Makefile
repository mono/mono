thisdir = class/Novell.Directory.Ldap
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Novell.Directory.Ldap.dll
LIB_MCS_FLAGS = \
	/nowarn:649 /nowarn:169 /nowarn:219 /nowarn:168 \
	-r:$(corlib)				\
	-r:System.dll			 	\
	-r:Mono.Security.dll			\
	$(RESX_RES:%=/res:%)

ifeq (net_1_1_java, $(PROFILE))
LIB_MCS_FLAGS +=	\
	-r:rt.dll				\
	-r:J2SE.Helpers.dll
endif

NO_TEST = yes

include ../../build/library.make

RESULTCODE_MESSAGES = Novell.Directory.Ldap.Utilclass/ResultCodeMessages.resources

RESX_RES = $(RESULTCODE_MESSAGES)

$(the_lib): $(RESULTCODE_MESSAGES)

$(RESULTCODE_MESSAGES): Novell.Directory.Ldap.Utilclass/ResultCodeMessages.txt
	$(RESGEN) $< $@