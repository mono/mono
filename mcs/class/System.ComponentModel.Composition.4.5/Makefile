thisdir = class/System.ComponentModel.Composition
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.ComponentModel.Composition.dll
LIB_MCS_FLAGS = -r:System.dll -r:System.Core.dll -d:CLR40 -resource:$(STRING_MESSAGES) -d:USE_ECMA_KEY,FEATURE_REFLECTIONCONTEXT,FEATURE_REFLECTIONFILEIO,FEATURE_SERIALIZATION,FEATURE_SLIMLOCK -nowarn:219,414

STRING_MESSAGES = Microsoft.Internal.Strings.resources

CLEAN_FILES += $(STRING_MESSAGES)

EXTRA_DISTFILES = \
	src/ComponentModel/Strings.resx
	
VALID_PROFILE := $(filter 4, $(FRAMEWORK_VERSION_MAJOR))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.ComponentModel.Composition.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make

$(the_lib): $(STRING_MESSAGES)

$(STRING_MESSAGES): src/ComponentModel/Strings.resx
	$(RESGEN) $< $@
