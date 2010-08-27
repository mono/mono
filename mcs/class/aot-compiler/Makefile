#
# This directory is used to AOT the C# compiler to speed up the rest of the class libs build
# It should be run after the assemblies the compiler depends on have been compiled, since
# the AOT image has a dependency on the exact assembly versions used to produce it.
#

thisdir = class/aot-compiler

include ../../build/rules.make

the_libdir = $(topdir)/class/lib/$(PROFILE)/

# The 2.0 profile uses mcs/gmcs.exe instead of lib/net_2_0/gmcs.exe
ifeq (net_2_0, $(PROFILE))
mcs_exe = $(topdir)/mcs/gmcs.exe
else ifeq (net_4_0, $(PROFILE))
mcs_exe = $(topdir)/class/lib/$(PROFILE)/dmcs.exe
endif

mcs_aot_image = $(mcs_exe)$(PLATFORM_AOT_SUFFIX)

ifdef PLATFORM_AOT_SUFFIX
Q_AOT=$(if $(V),,@echo "AOT [$(PROFILE)] $(notdir $(@))";)
$(mcs_aot_image): $(mcs_exe)
	$(Q_AOT) MONO_PATH='$(the_libdir)' > $(PROFILE)_aot.log 2>&1 $(RUNTIME) --aot=bind-to-runtime-version --debug $(mcs_exe)

ifeq (net_2_0, $(PROFILE))
all-local: $(mcs_aot_image)
endif

ifeq (net_4_0, $(PROFILE))
all-local: $(mcs_aot_image)
endif

endif
