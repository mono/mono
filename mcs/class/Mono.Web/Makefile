thisdir = class/Mono.Web
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Web.dll

NO_TEST = yes

# This is a .NET 2.0+ only assembly
VALID_PROFILE := $(filter net_2_0 net_4_0 net_4_5, $(PROFILE))
ifdef VALID_PROFILE

LIB_MCS_FLAGS = -r:$(corlib) \
		-r:System.dll \
		-r:System.Xml.dll \
		-r:System.Web.dll \
		-r:System.Configuration.dll

else
LIBRARY_NAME = dummy-Mono.Web.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make
