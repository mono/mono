thisdir = class/System.ServiceModel.Activation
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.ServiceModel.Activation.dll

LIB_REFS = System.Core System plainservice/System.ServiceModel
LIB_MCS_FLAGS =

TEST_MCS_FLAGS =
TEST_LIB_REFS = System System.Core

servicemodel = $(the_libdir_base)plainservice/System.ServiceModel.dll

include ../../build/library.make

$(the_libdir_base)$(LIBRARY): $(servicemodel)

$(servicemodel): 
	(cd ../System.ServiceModel; $(MAKE) $@)

.NOTPARALLEL: $(servicemodel)
