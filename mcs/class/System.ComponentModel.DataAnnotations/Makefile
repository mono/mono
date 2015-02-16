thisdir = class/System.ComponentModel.DataAnnotations
SUBDIRS =
include ../../build/rules.make

LIBRARY = System.ComponentModel.DataAnnotations.dll
LIB_MCS_FLAGS = \
                /r:System.dll \
                /r:System.Core.dll \
                /r:System.Data.dll \
                /r:System.Xml.dll \

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

# This is a .NET 3.5+ assembly
VALID_PROFILE := $(filter net_4_5 monotouch monodroid xammac mobile mobile_static, $(PROFILE))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.ComponentModel.DataAnnotations.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make
