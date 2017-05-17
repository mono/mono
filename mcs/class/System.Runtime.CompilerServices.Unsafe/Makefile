thisdir = class/System.Runtime.CompilerServices.Unsafe
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Runtime.CompilerServices.Unsafe.dll

XTEST_LIB_FLAGS = -unsafe

NO_BUILD = 1

NUGET := https://dotnet.myget.org/F/dotnet-core/api/v2/package/System.Runtime.CompilerServices.Unsafe/4.4.0-preview2-25224-01

include ../../build/library.make

$(build_lib):
	curl $(NUGET) -L -o nuget.zip || wget $(NUGET) --local-encoding=utf-8 -O nuget.zip
	MONO_PATH="$(topdir)/class/lib/$(BUILD_TOOLS_PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) $(topdir)/class/lib/net_4_x/csharp.exe -r:$(topdir)/class/lib/$(PROFILE)/System.IO.Compression.FileSystem.dll -e ' System.IO.Compression.ZipFile.ExtractToDirectory("nuget.zip", "tmp/");' && cp tmp/lib/netstandard2.0/$(LIBRARY) $(topdir)/class/lib/$(PROFILE)/
	rm -f nuget.zip
	rm -rf tmp/


