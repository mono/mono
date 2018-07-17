GENSOURCES_LIBDIR = $(topdir)/class/lib/$(BUILD_TOOLS_PROFILE)

gensources = $(topdir)/build/gensources.exe
$(gensources): $(topdir)/build/gensources.cs
	echo $(BOOTSTRAP_MCS) -noconfig -lib:$(GENSOURCES_LIBDIR) -debug:portable -r:mscorlib.dll -r:System.dll -r:System.Core.dll -out:$(gensources) $(topdir)/build/gensources.cs
	$(BOOTSTRAP_MCS) -lib:$(GENSOURCES_LIBDIR) -noconfig -debug:portable -r:mscorlib.dll -r:System.dll -r:System.Core.dll -out:$(gensources) $(topdir)/build/gensources.cs

ifdef PROFILE_RUNTIME
GENSOURCES_RUNTIME = $(PROFILE_RUNTIME)
else
GENSOURCES_RUNTIME = MONO_PATH="$(GENSOURCES_LIBDIR)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME)
endif