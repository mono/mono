#
# This directory is used to AOT the C# compiler to speed up the rest of the class libs build
# It should be run after the assemblies the compiler depends on have been compiled, since
# the AOT image has a dependency on the exact assembly versions used to produce it.
#

thisdir = class/aot-compiler

include ../../build/rules.make

the_libdir = $(topdir)/class/lib/$(PROFILE)/

mscorlib_dll = $(the_libdir)/mscorlib.dll
mscorlib_aot_image = $(mscorlib_dll)$(PLATFORM_AOT_SUFFIX)

# mcs.exe is only in the build profile, but the aot image should be compiled against the current
# profile
ifdef MCS_MODE
mcs_exe = $(the_libdir)/mcs.exe
mcs_aot_image = $(the_libdir)/mcs.exe$(PLATFORM_AOT_SUFFIX)
else
csc_exe = $(CSC_LOCATION)
csc_aot_image = $(the_libdir)/csc.exe$(PLATFORM_AOT_SUFFIX)
csc_MCS_dll = $(dir $(CSC_LOCATION))/Microsoft.CodeAnalysis.CSharp.dll
csc_MCS_image = $(the_libdir)/Microsoft.CodeAnalysis.CSharp.dll$(PLATFORM_AOT_SUFFIX)
csc_MC_dll = $(dir $(CSC_LOCATION))/Microsoft.CodeAnalysis.dll
csc_MC_image = $(the_libdir)/Microsoft.CodeAnalysis.dll$(PLATFORM_AOT_SUFFIX)
csc_SRM_dll = $(dir $(CSC_LOCATION))/System.Reflection.Metadata.dll
csc_SRM_image = $(the_libdir)/System.Reflection.Metadata.dll$(PLATFORM_AOT_SUFFIX)
csc_SCI_dll = $(dir $(CSC_LOCATION))/System.Collections.Immutable.dll
csc_SCI_image = $(the_libdir)/System.Collections.Immutable.dll$(PLATFORM_AOT_SUFFIX)
endif

# The $(dir $(RUNTIME)) is necessary to get path to the mono binary in case when we cross-compile
# or just compile from a different directory than the top source dir
runtime_dep = $(dir $(RUNTIME))/../mono/mini/mono

PROGRAM_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)
LIBRARY_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)

ifndef SKIP_AOT

profile_file:=$(wildcard $(topdir)/class/lib/build/csc.*.aotprofile)
ifneq ($(profile_file),)
comma:=,
space:=
space+=
profile_arg:=$(subst $(space)$(comma),$(comma),$(foreach pf,$(profile_file),$(comma)profile=$(strip $(pf))))
endif

ifdef PLATFORM_AOT_SUFFIX

IMAGES = $(mscorlib_aot_image)
$(mscorlib_aot_image): $(mscorlib_dll) $(runtime_dep)
	$(Q_AOT) MONO_PATH='$(the_libdir)' > $(PROFILE)_aot.log 2>&1 $(RUNTIME) --aot=bind-to-runtime-version$(profile_arg) --debug $(mscorlib_dll) || (cat $(PROFILE)_aot.log; exit 1)

ifdef MCS_MODE

IMAGES += $(mcs_aot_image)
$(mcs_aot_image): $(mcs_exe) $(mscorlib_dll) $(runtime_dep)
	$(Q_AOT) MONO_PATH='$(the_libdir)' > $(PROFILE)_aot.log 2>&1 $(RUNTIME) --aot=bind-to-runtime-version$(profile_arg),outfile=$(mcs_aot_image) --debug $(mcs_exe) || cat $(PROFILE)_aot.log || (cat $(PROFILE)_aot.log; exit 1)

else

IMAGES += $(csc_aot_image)
$(csc_aot_image): $(csc_exe) $(mscorlib_dll) $(runtime_dep)
	$(Q_AOT) MONO_PATH='$(the_libdir)' > $(PROFILE)_aot.log 2>&1 $(RUNTIME) --aot=bind-to-runtime-version$(profile_arg),outfile=$(csc_aot_image) --debug $(csc_exe) || cat $(PROFILE)_aot.log || (cat $(PROFILE)_aot.log; exit 1)

IMAGES += $(csc_MC_image)
$(csc_MC_image): $(csc_MC_dll) $(runtime_dep)
	$(Q_AOT) MONO_PATH='$(the_libdir)' > $(PROFILE)_aot.log 2>&1 $(RUNTIME) --aot=bind-to-runtime-version$(profile_arg),outfile=$(csc_MC_image) --debug $(csc_MC_dll) || (cat $(PROFILE)_aot.log; exit 1)

IMAGES += $(csc_MCS_image)
$(csc_MCS_image): $(csc_MCS_dll) $(runtime_dep)
	$(Q_AOT) MONO_PATH='$(the_libdir)' > $(PROFILE)_aot.log 2>&1 $(RUNTIME) --aot=bind-to-runtime-version$(profile_arg),outfile=$(csc_MCS_image) --debug $(csc_MCS_dll) || (cat $(PROFILE)_aot.log; exit 1)

IMAGES += $(csc_SRM_image)
$(csc_SRM_image): $(csc_SRM_dll) $(runtime_dep)
	$(Q_AOT) MONO_PATH='$(the_libdir)' > $(PROFILE)_aot.log 2>&1 $(RUNTIME) --aot=bind-to-runtime-version$(profile_arg),outfile=$(csc_SRM_image) --debug --apply-bindings=$(csc_exe).config $(csc_SRM_dll) || (cat $(PROFILE)_aot.log; exit 1)

IMAGES += $(csc_SCI_image)
$(csc_SCI_image): $(csc_SCI_dll) $(runtime_dep)
	$(Q_AOT) MONO_PATH='$(the_libdir)' > $(PROFILE)_aot.log 2>&1 $(RUNTIME) --aot=bind-to-runtime-version$(profile_arg),outfile=$(csc_SCI_image) --debug $(csc_SCI_dll) || (cat $(PROFILE)_aot.log; exit 1)

endif

ifdef ENABLE_AOT

# AOT build profile to speed up build
ifeq ($(PROFILE),build)

clean-local:
	-rm -f $(IMAGES) $(PROFILE)_aot.log

all-local: $(IMAGES)

install-local:

endif

ifeq ($(PROFILE), $(DEFAULT_PROFILE))

all-local: $(IMAGES)

install-local:
	$(MKINSTALLDIRS) $(DESTDIR)$(LIBRARY_INSTALL_DIR)
	$(INSTALL_LIB) $(IMAGES) $(DESTDIR)$(LIBRARY_INSTALL_DIR)

endif

endif

endif

endif

dist-local: dist-default
