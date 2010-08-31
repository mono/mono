#
# This directory is used to AOT the C# compiler to speed up the rest of the class libs build
# It should be run after the assemblies the compiler depends on have been compiled, since
# the AOT image has a dependency on the exact assembly versions used to produce it.
#

thisdir = class/aot-compiler

include ../../build/rules.make

the_libdir = $(topdir)/class/lib/$(PROFILE)/

ifeq (net_2_0, $(PROFILE))
mcs_exe = $(the_libdir)/gmcs.exe
else ifeq (net_4_0, $(PROFILE))
mcs_exe = $(the_libdir)/dmcs.exe
endif

mcs_aot_image = $(mcs_exe)$(PLATFORM_AOT_SUFFIX)

mscorlib_dll = $(the_libdir)/mscorlib.dll
mscorlib_aot_image = $(mscorlib_dll)$(PLATFORM_AOT_SUFFIX)

PROGRAM_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)
LIBRARY_INSTALL_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)

ifdef PLATFORM_AOT_SUFFIX
Q_AOT=$(if $(V),,@echo "AOT [$(PROFILE)] $(notdir $(@))";)
$(mcs_aot_image): $(mcs_exe)
	$(Q_AOT) MONO_PATH='$(the_libdir)' > $(PROFILE)_aot.log 2>&1 $(RUNTIME) --aot=bind-to-runtime-version --debug $(mcs_exe)

$(mscorlib_aot_image): $(mscorlib_dll)
	$(Q_AOT) MONO_PATH='$(the_libdir)' > $(PROFILE)_aot.log 2>&1 $(RUNTIME) --aot=bind-to-runtime-version --debug $(mscorlib_dll)

ifeq (net_2_0, $(PROFILE))
all-local: $(mscorlib_aot_image) $(mcs_aot_image)
endif

ifeq (net_4_0, $(PROFILE))
all-local: $(mscorlib_aot_image) $(mcs_aot_image)
endif

clean-local:
	-rm -f $(mscorlib_aot_image) $(mcs_aot_image)

install-local:
	$(MKINSTALLDIRS) $(DESTDIR)$(LIBRARY_INSTALL_DIR)
	$(INSTALL_LIB) $(mscorlib_aot_image) $(DESTDIR)$(LIBRARY_INSTALL_DIR)
	$(INSTALL_LIB) $(mcs_aot_image) $(DESTDIR)$(PROGRAM_INSTALL_DIR)

endif

dist-local:
