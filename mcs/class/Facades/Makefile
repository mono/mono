MCS_BUILD_DIR = ../../build

thisdir = class/Facades

include subdirs.make

#OVERRIDE_TARGET_ALL = yes

# 
# The output directory for the facade assemblies
#
target_dir = $(topdir)/class/lib/$(PROFILE)/Facades

#
# Contains the expanded version of the target dll for each profile
#
target_libraries = $(patsubst %,$(target_dir)/%.dll,$(SUBDIRS))

#
# To build each target, go into the subdirectory
#
$(target_dir)/%.dll: 
	(cd `basename $@ .dll`; $(MAKE))

#all.override: $(target_libraries)

include $(MCS_BUILD_DIR)/rules.make

dist-local: dist-default

DIST_SUBDIRS = $(net_4_x_PARALLEL_SUBDIRS) $(net_4_x_SUBDIRS) $(mobile_only_SUBDIRS)
DISTFILES=subdirs.make

doc-update-local:
	@echo "not doing docs"

doc-update-recursive:
	@echo "do not recurse the Facades folder"

System System.Core System.ComponentModel.DataAnnotations System.Numerics System.Numerics.Vectors System.Runtime.Serialization System.Transactions System.Xml \
System.ComponentModel.Composition System.ServiceModel System.Xml.Linq System.Data System.Data.DataSetExtensions System.IO.Compression.FileSystem System.Runtime.InteropServices.RuntimeInformation \
System.ServiceProcess System.Security System.Net.Http.WebRequest System.Net.Http System.ServiceProcess System.IO.Compression System.IdentityModel System.Web \
Facades/System.Drawing.Primitives Facades/System.Drawing.Common System.Drawing System.Web.Services:

all-local-aot:

