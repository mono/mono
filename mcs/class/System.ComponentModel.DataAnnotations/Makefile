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

RESOURCE_STRINGS = ../../../external/referencesource/System.ComponentModel.DataAnnotations/System.ComponentModel.DataAnnotations.txt

include ../../build/library.make
