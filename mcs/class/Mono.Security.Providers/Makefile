MCS_BUILD_DIR = ../../build

thisdir = class/Mono.Security.Providers

PROVIDERS = DotNet NewSystemSource

mobile_static_SUBDIRS = $(PROVIDERS)

net_4_x_SUBDIRS = $(PROVIDERS)

monodroid_SUBDIRS = $(PROVIDERS)

monotouch_SUBDIRS = $(PROVIDERS)

monotouch_watch_SUBDIRS := $(PROVIDERS)
monotouch_tv_SUBDIRS := $(PROVIDERS)

xammac_SUBDIRS = $(PROVIDERS)

xammac_net_4_5_SUBDIRS = $(PROVIDERS)

PROFILE_SUBDIRS = $(PROVIDERS)

#OVERRIDE_TARGET_ALL = yes

# 
# The output directory for the facade assemblies
#
target_dir = $(topdir)/class/lib/$(PROFILE)/Mono.Security.Providers

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

SUBDIRS = $(PROFILE)

doc-update-local:
	@echo "not doing docs"

doc-update-recursive:
	@echo "not doing docs"
	
