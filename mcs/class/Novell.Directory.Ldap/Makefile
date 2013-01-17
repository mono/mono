thisdir = class/Novell.Directory.Ldap
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Novell.Directory.Ldap.dll

ifeq (net_1_1_java, $(PROFILE))
LIB_MCS_FLAGS = \
	/nowarn:649 /nowarn:169 /nowarn:219 /nowarn:168 \
	-r:$(corlib)				\
	-r:System.dll			 	\
	-r:rt.dll					\
	-r:J2SE.Helpers.dll			\
	$(RESX_RES:%=/res:%)
else
LIB_MCS_FLAGS = \
	-warn:1 -nowarn:612 \
	-r:$(corlib)				\
	-r:System.dll			 	\
	-r:Mono.Security.dll			\
	$(RESX_RES:%=/res:%)
endif

include ../../build/library.make

RESULTCODE_MESSAGES = Novell.Directory.Ldap.Utilclass/ResultCodeMessages.resources

RESX_RES = $(RESULTCODE_MESSAGES)

$(the_lib): $(RESULTCODE_MESSAGES)

$(RESULTCODE_MESSAGES): Novell.Directory.Ldap.Utilclass/ResultCodeMessages.txt
	$(RESGEN) $< $@

EXTRA_DISTFILES = \
	net_1_1_java_Novell.Directory.Ldap.dll.sources \
	Novell.Directory.Ldap.Rfc2251/RfcLdapURL.cs \
	Novell.Directory.Ldap.Security.jvm/AuthenticationCallbackHandler.cs \
	Novell.Directory.Ldap.Security.jvm/ChangeLog \
	Novell.Directory.Ldap.Security.jvm/CreateContextPrivilegedAction.cs \
	Novell.Directory.Ldap.Security.jvm/ExchangeTokenPrivilegedAction.cs \
	Novell.Directory.Ldap.Security.jvm/Krb5Helper.cs \
	Novell.Directory.Ldap.Security.jvm/SecureStream.cs \
	Novell.Directory.Ldap.Security.jvm/UnwrapPrivilegedAction.cs \
	Novell.Directory.Ldap.Security.jvm/WrapPrivilegedAction.cs \
	Novell.Directory.Ldap.Utilclass/ExceptionMessages.resx \
	Novell.Directory.Ldap.Utilclass/ExceptionMessages.txt \
	Novell.Directory.Ldap.Utilclass/ResultCodeMessages.resx \
	Novell.Directory.Ldap.Utilclass/ResultCodeMessages.txt 

