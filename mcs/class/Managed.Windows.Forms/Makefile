thisdir = class/Managed.Windows.Forms
include ../../build/rules.make

LIBRARY = System.Windows.Forms.dll
LIB_MCS_FLAGS = /unsafe \
	/r:$(corlib) /r:System.dll /r:System.Xml.dll \
	/r:System.Drawing.dll /r:Accessibility.dll \
	/r:Mono.Posix.dll \
	/nowarn:649 /nowarn:169 /nowarn:67 /nowarn:108 /nowarn:114

EXTRA_DISTFILES = \
	README

TEST_MCS_FLAGS = /r:System.Drawing.dll

all.local: SWF.csproj

SWF.csproj: System.Windows.Forms.dll.sources
	./build-csproj

include ../../build/library.make
