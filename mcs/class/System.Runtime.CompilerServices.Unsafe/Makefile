thisdir = class/System.Runtime.CompilerServices.Unsafe
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Runtime.CompilerServices.Unsafe.dll

XTEST_LIB_FLAGS = -unsafe

NO_BUILD = 1

include ../../build/library.make

$(build_lib):
	wget https://dotnet.myget.org/F/dotnet-core/api/v2/package/System.Runtime.CompilerServices.Unsafe/4.4.0-preview2-25224-01 -O nuget.zip
	mono $(topdir)/class/lib/net_4_x/csharp.exe -r:$(topdir)/class/lib/$(PROFILE)/System.IO.Compression.FileSystem.dll -e ' System.IO.Compression.ZipFile.ExtractToDirectory("nuget.zip", "tmp/");'	
	rm -f nuget.zip
	cp tmp/lib/netstandard2.0/System.Runtime.CompilerServices.Unsafe.dll $(topdir)/class/lib/$(PROFILE)/
	rm -rf tmp/

