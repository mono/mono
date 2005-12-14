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

RESCODEMESS = Novell.Directory.Ldap.Utilclass/Novell.Directory.Ldap.Utilclass.ResultCodeMessages.resources
EXCEPTMESS = Novell.Directory.Ldap.Utilclass/Novell.Directory.Ldap.Utilclass.ExceptionMessages.resources

RESX_RES = \
	${RESCODEMESS} \
	${EXCEPTMESS}

LIB_MCS_FLAGS +=	\
	-r:rt.dll	\
	-r:J2SE.Helpers.dll \
	$(RESX_RES:%=/res:%)

endif

NO_TEST = yes

include ../../build/library.make

ifeq (net_1_1_java, $(PROFILE))

$(the_lib) : $(RESX_RES)

$(RESCODEMESS) : Novell.Directory.Ldap.Utilclass/ResultCodeMessages.resx
	$(RESGEN) $< $@
$(EXCEPTMESS) : Novell.Directory.Ldap.Utilclass/ExceptionMessages.resx
	$(RESGEN) $< $@

endif

