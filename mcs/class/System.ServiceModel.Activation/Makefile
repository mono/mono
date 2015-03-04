thisdir = class/System.ServiceModel.Activation
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.ServiceModel.Activation.dll

LIB_REFS = System.Core System System.ServiceModel
LIB_MCS_FLAGS =

TEST_MCS_FLAGS = -r:System.dll -r:System.Core.dll

plainservicemodel_dir = $(the_libdir_base)plainservice
servicemodel = $(plainservicemodel_dir)/System.ServiceModel.dll
LOCAL_MCS_FLAGS = -lib:$(plainservicemodel_dir)

include ../../build/library.make

$(the_libdir_base)$(LIBRARY): $(servicemodel)

$(servicemodel): 
	(cd ../System.ServiceModel; $(MAKE) $@)

.NOTPARALLEL: $(servicemodel)
