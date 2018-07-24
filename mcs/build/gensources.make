ifndef GENSOURCES_MAKE_INCLUDED
export GENSOURCES_MAKE_INCLUDED = 1

GENSOURCES_LIBDIR = $(topdir)/class/lib/$(BUILD_TOOLS_PROFILE)

gensources = $(topdir)/build/gensources.exe
$(gensources): $(topdir)/build/gensources.cs
	echo $(BOOTSTRAP_MCS) -lib:$(GENSOURCES_LIBDIR) -noconfig -debug:portable -r:mscorlib.dll -r:System.dll -r:System.Core.dll -out:$(gensources) $(topdir)/build/gensources.cs
	$(BOOTSTRAP_MCS) -lib:$(GENSOURCES_LIBDIR) -noconfig -debug:portable -r:mscorlib.dll -r:System.dll -r:System.Core.dll -out:$(gensources) $(topdir)/build/gensources.cs

endif
