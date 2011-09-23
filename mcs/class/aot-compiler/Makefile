#
# This directory is used to AOT the C# compiler to speed up the rest of the class libs build
# It should be run after the assemblies the compiler depends on have been compiled, since
# the AOT image has a dependency on the exact assembly versions used to produce it.
#

thisdir = class/aot-compiler

include ../../build/rules.make

the_libdir = $(topdir)/class/lib/$(PROFILE)/

# mcs.exe is only in the build profile, but the aot image should be compiled against the current
# profile
mcs_exe = $(topdir)/class/lib/build/mcs.exe
mcs_aot_image = $(the_libdir)/mcs.exe$(PLATFORM_AOT_SUFFIX)

mscorlib_dll = $(the_libdir)/mscorlib.dll
mscorlib_aot_image = $(mscorlib_dll)$(PLATFORM_AOT_SUFFIX)

# The $(dir $(RUNTIME)) is necessary to get path to the mono binary in case when we cross-compile
# or just compile from a different directory than the top source dir
runtime_dep = $(dir $(RUNTIME))/../mono/mini/mono

PROGRAM_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)
LIBRARY_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)

ifndef SKIP_AOT

ifdef PLATFORM_AOT_SUFFIX
Q_AOT=$(if $(V),,@echo "AOT [$(PROFILE)] $(notdir $(@))";)
$(mcs_aot_image): $(mcs_exe) $(mscorlib_dll) $(runtime_dep)
	$(Q_AOT) MONO_PATH='$(the_libdir)' > $(PROFILE)_aot.log 2>&1 $(RUNTIME) --aot=bind-to-runtime-version,outfile=$(mcs_aot_image) --debug $(mcs_exe) || cat $(PROFILE)_aot.log || (cat $(PROFILE)_aot.log; exit 1)

$(mscorlib_aot_image): $(mscorlib_dll) $(runtime_dep)
	$(Q_AOT) MONO_PATH='$(the_libdir)' > $(PROFILE)_aot.log 2>&1 $(RUNTIME) --aot=bind-to-runtime-version --debug $(mscorlib_dll) || (cat $(PROFILE)_aot.log; exit 1)

ifdef ENABLE_AOT

clean-local:
	-rm -f $(mscorlib_aot_image) $(mcs_aot_image) $(PROFILE)_aot.log

# AOT build profile mcs to speed up build
ifeq ($(PROFILE),build)
all-local: $(mscorlib_aot_image) $(mcs_aot_image)
install-local:
endif

ifeq ($(PROFILE),net_4_0)
all-local: $(mscorlib_aot_image) $(mcs_aot_image)
install-local:
	$(MKINSTALLDIRS) $(DESTDIR)$(LIBRARY_INSTALL_DIR)
	$(INSTALL_LIB) $(mscorlib_aot_image) $(DESTDIR)$(LIBRARY_INSTALL_DIR)
	$(INSTALL_LIB) $(mcs_aot_image) $(DESTDIR)$(PROGRAM_INSTALL_DIR)
endif

# No mcs in net 2.0 or net 4.5
MSCORLIB_PROFILE := $(filter net_2_0 net_4_5, $(PROFILE))
ifdef MSCORLIB_PROFILE
all-local: $(mscorlib_aot_image)
install-local:
	$(MKINSTALLDIRS) $(DESTDIR)$(LIBRARY_INSTALL_DIR)
	$(INSTALL_LIB) $(mscorlib_aot_image) $(DESTDIR)$(LIBRARY_INSTALL_DIR)
endif

endif

endif

endif

dist-local: dist-default
