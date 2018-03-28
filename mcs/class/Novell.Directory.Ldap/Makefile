thisdir = class/Novell.Directory.Ldap
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Novell.Directory.Ldap.dll

LIB_REFS = System Mono.Security
KEYFILE = ../mono.pub
LIB_MCS_FLAGS = -warn:1 -nowarn:612 

RESOURCE_DEFS = ResultCodeMessages,Novell.Directory.Ldap.Utilclass/ResultCodeMessages.txt
include ../../build/library.make

EXTRA_DISTFILES = \
	Novell.Directory.Ldap.Rfc2251/RfcLdapURL.cs \
	Novell.Directory.Ldap.Utilclass/ExceptionMessages.resx \
	Novell.Directory.Ldap.Utilclass/ExceptionMessages.txt \
	Novell.Directory.Ldap.Utilclass/ResultCodeMessages.resx

