thisdir = class/System.Windows.Forms.DataVisualization
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Windows.Forms.DataVisualization.dll
LIB_MCS_FLAGS = -r:System -r:System.Drawing -r:System.Windows.Forms -r:System.Core -r:System.Data -r:System.Xml
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES =

VALID_PROFILE := $(filter 4, $(FRAMEWORK_VERSION_MAJOR))

ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.Windows.Forms.DataVisualization.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make
