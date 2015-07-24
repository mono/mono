thisdir = class/Novell.Directory.Ldap
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Novell.Directory.Ldap.dll

LIB_REFS = System Mono.Security
LIB_MCS_FLAGS = \
	-warn:1 -nowarn:612 \
	-r:$(corlib)				\
	-r:System.dll			 	\
	-r:Mono.Security.dll			\
	$(RESX_RES:%=/res:%)

include ../../build/library.make

RESULTCODE_MESSAGES = Novell.Directory.Ldap.Utilclass/ResultCodeMessages.resources

RESX_RES = $(RESULTCODE_MESSAGES)

$(the_lib): $(RESULTCODE_MESSAGES)

$(RESULTCODE_MESSAGES): Novell.Directory.Ldap.Utilclass/ResultCodeMessages.txt
	$(RESGEN) $< $@

EXTRA_DISTFILES = \
	Novell.Directory.Ldap.Rfc2251/RfcLdapURL.cs \
	Novell.Directory.Ldap.Utilclass/ExceptionMessages.resx \
	Novell.Directory.Ldap.Utilclass/ExceptionMessages.txt \
	Novell.Directory.Ldap.Utilclass/ResultCodeMessages.resx \
	Novell.Directory.Ldap.Utilclass/ResultCodeMessages.txt 

