thisdir = class/Mono.Security.Providers.NewSystemSource
SUBDIRS = 
include ../../build/rules.make

ifdef MOBILE_PROFILE
EXTERN_ALIAS_FLAGS = -r:Mono.Security
else
EXTERN_ALIAS_FLAGS = -d:MONO_SECURITY_ALIAS -r:MonoSecurity=Mono.Security
endif

LIBRARY = Mono.Security.Providers.NewSystemSource.dll
LIB_REFS = System
LIB_MCS_FLAGS = -d:SECURITY_DEP $(EXTERN_ALIAS_FLAGS) \
	-unsafe -nowarn:1030 -keyfile:../mono.pub -delaysign -d:MONO -d:FEATURE_PAL -d:MONO_FEATURE_NEW_TLS \
	-d:MONO_FEATURE_NEW_SYSTEM_SOURCE -d:DISABLE_CAS_USE

EXTRA_DISTFILES = README.md

include ../../build/library.make

