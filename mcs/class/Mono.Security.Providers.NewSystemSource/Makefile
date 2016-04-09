thisdir = class/Mono.Security.Providers.NewSystemSource
SUBDIRS = 
include ../../build/rules.make

LIB_REFS = System

LIBRARY = Mono.Security.Providers.NewSystemSource.dll

LIB_MCS_FLAGS = -d:SECURITY_DEP \
	-unsafe -nowarn:1030 -keyfile:../mono.pub -delaysign -d:MONO -d:FEATURE_PAL -d:MONO_FEATURE_NEW_TLS \
	-d:MONO_FEATURE_NEW_SYSTEM_SOURCE -d:DISABLE_CAS_USE

ifdef MOBILE_PROFILE
LIB_REFS += Mono.Security
else
LIB_REFS += MonoSecurity=Mono.Security
LIB_MCS_FLAGS += -d:MONO_SECURITY_ALIAS
endif

EXTRA_DISTFILES = README.md

include ../../build/library.make

$(the_lib): ../Mono.Security/Makefile
