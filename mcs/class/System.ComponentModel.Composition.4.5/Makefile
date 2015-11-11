thisdir = class/System.ComponentModel.Composition.4.5
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.ComponentModel.Composition.dll
LIB_REFS = System System.Core
LIB_MCS_FLAGS = -d:CLR40 -resource:$(STRING_MESSAGES) -d:USE_ECMA_KEY,FEATURE_REFLECTIONCONTEXT,FEATURE_REFLECTIONFILEIO,FEATURE_SERIALIZATION,FEATURE_SLIMLOCK -nowarn:219,414

STRING_MESSAGES = Microsoft.Internal.Strings.resources

CLEAN_FILES += $(STRING_MESSAGES)

EXTRA_DISTFILES = \
	src/ComponentModel/Strings.resx
	
include ../../build/library.make

$(the_lib): $(STRING_MESSAGES)

$(STRING_MESSAGES): src/ComponentModel/Strings.resx
	$(RESGEN) $< $@
