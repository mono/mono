#
# This directory is used to AOT the C# compiler to speed up the rest of the class libs build
# It should be run after the assemblies the compiler depends on have been compiled, since
# the AOT image has a dependency on the exact assembly versions used to produce it.
#

thisdir = class/aot-compiler

include ../../build/rules.make

the_libdir = $(topdir)/class/lib/$(PROFILE_DIRECTORY)/
CSC_DIR = $(dir $(CSC_LOCATION))
# The directory where the AOT images are stored
images_dir = $(the_libdir)

# mcs.exe is only in the build profile, but the aot image should be compiled against the current
# profile
mcs_exe = $(the_libdir)/mcs.exe
mcs_aot_image = $(the_libdir)/mcs.exe$(PLATFORM_AOT_SUFFIX)
csc_exe = $(CSC_LOCATION)
csc_aot_image = $(images_dir)/csc.exe$(PLATFORM_AOT_SUFFIX)
csc_MCS_dll = $(CSC_DIR)/Microsoft.CodeAnalysis.CSharp.dll
csc_MCS_image = $(images_dir)/Microsoft.CodeAnalysis.CSharp.dll$(PLATFORM_AOT_SUFFIX)
csc_MC_dll = $(CSC_DIR)/Microsoft.CodeAnalysis.dll
csc_MC_image = $(images_dir)/Microsoft.CodeAnalysis.dll$(PLATFORM_AOT_SUFFIX)
csc_SRM_dll = $(CSC_DIR)/System.Reflection.Metadata.dll
csc_SRM_image = $(images_dir)/System.Reflection.Metadata.dll$(PLATFORM_AOT_SUFFIX)
csc_SCI_dll = $(CSC_DIR)/System.Collections.Immutable.dll
csc_SCI_image = $(images_dir)/System.Collections.Immutable.dll$(PLATFORM_AOT_SUFFIX)

mscorlib_dll = $(the_libdir)/mscorlib.dll
mscorlib_aot_image = $(mscorlib_dll)$(PLATFORM_AOT_SUFFIX)

# The $(dir $(RUNTIME)) is necessary to get path to the mono binary in case when we cross-compile
# or just compile from a different directory than the top source dir
runtime_dep = $(dir $(RUNTIME))/../mono/mini/mono

PROGRAM_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)
LIBRARY_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)

LOG_FILE = $(PROFILE_DIRECTORY)_aot.log

ifndef SKIP_AOT

profile_file:=$(wildcard $(topdir)/class/lib/build/csc.*.aotprofile)
ifneq ($(profile_file),)
comma:=,
space:=
space+=
profile_arg:=$(subst $(space)$(comma),$(comma),$(foreach pf,$(profile_file),$(comma)profile=$(strip $(pf))))
endif

ifdef PLATFORM_AOT_SUFFIX
$(mcs_aot_image): $(mcs_exe) $(mscorlib_dll) $(runtime_dep)
	$(Q_AOT) MONO_PATH='$(the_libdir)' > $(LOG_FILE) 2>&1 $(RUNTIME) --aot=bind-to-runtime-version$(profile_arg),outfile=$(mcs_aot_image) --debug $(mcs_exe) || cat $(LOG_FILE) || (cat $(LOG_FILE); exit 1)

$(csc_aot_image): $(csc_exe) $(mscorlib_dll) $(runtime_dep)
	$(Q_AOT) MONO_PATH='$(the_libdir)' > $(LOG_FILE) 2>&1 $(RUNTIME) --aot=bind-to-runtime-version$(profile_arg),outfile=$(csc_aot_image) --debug $(csc_exe) || cat $(LOG_FILE) || (cat $(LOG_FILE); exit 1)

$(mscorlib_aot_image): $(mscorlib_dll) $(runtime_dep)
	$(Q_AOT) MONO_PATH='$(the_libdir)' > $(LOG_FILE) 2>&1 $(RUNTIME) --aot=bind-to-runtime-version$(profile_arg) --debug $(mscorlib_dll) || (cat $(LOG_FILE); exit 1)

$(csc_MC_image): $(csc_MC_dll) $(runtime_dep)
	$(Q_AOT) MONO_PATH='$(the_libdir)' > $(LOG_FILE) 2>&1 $(RUNTIME) --aot=bind-to-runtime-version$(profile_arg),outfile=$(csc_MC_image) --debug $(csc_MC_dll) || (cat $(LOG_FILE); exit 1)

$(csc_MCS_image): $(csc_MCS_dll) $(runtime_dep)
	$(Q_AOT) MONO_PATH='$(the_libdir)' > $(LOG_FILE) 2>&1 $(RUNTIME) --aot=bind-to-runtime-version$(profile_arg),outfile=$(csc_MCS_image) --debug $(csc_MCS_dll) || (cat $(LOG_FILE); exit 1)

$(csc_SRM_image): $(csc_SRM_dll) $(runtime_dep)
	$(Q_AOT) MONO_PATH='$(the_libdir)' > $(LOG_FILE) 2>&1 $(RUNTIME) --aot=bind-to-runtime-version$(profile_arg),outfile=$(csc_SRM_image) --debug --apply-bindings=$(csc_exe).config $(csc_SRM_dll) || (cat $(LOG_FILE); exit 1)

$(csc_SCI_image): $(csc_SCI_dll) $(runtime_dep)
	$(Q_AOT) MONO_PATH='$(the_libdir)' > $(LOG_FILE) 2>&1 $(RUNTIME) --aot=bind-to-runtime-version$(profile_arg),outfile=$(csc_SCI_image) --debug $(csc_SCI_dll) || (cat $(LOG_FILE); exit 1)

ifdef ENABLE_AOT

CSC_IMAGES = $(csc_aot_image) $(csc_SRM_image) $(csc_SCI_image) $(csc_MC_image) $(csc_MCS_image)

clean-local:
	-rm -f $(mscorlib_aot_image) $(mcs_aot_image) $(CSC_IMAGES) $(LOG_FILE)

# AOT build profile to speed up build
ifeq ($(PROFILE),build)

IMAGES = $(mscorlib_aot_image)

ifdef MCS_MODE
IMAGES += $(mcs_aot_image)
else
IMAGES += $(CSC_IMAGES)
endif

all-local: $(IMAGES)
install-local:

endif

ifeq ($(PROFILE), $(DEFAULT_PROFILE))

IMAGES = $(mscorlib_aot_image) $(mcs_aot_image) $(CSC_IMAGES)

all-local: $(IMAGES)
install-local:
	$(MKINSTALLDIRS) $(DESTDIR)$(LIBRARY_INSTALL_DIR)
	$(INSTALL_LIB) $(IMAGES) $(DESTDIR)$(LIBRARY_INSTALL_DIR)

endif

endif

endif

endif

dist-local: dist-default
