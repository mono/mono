thisdir = class/Managed.Windows.Forms
include ../../build/rules.make

LIBRARY = System.Windows.Forms.dll
LIB_MCS_FLAGS = /unsafe \
	/r:$(corlib) /r:System.dll /r:System.Xml.dll \
	/r:System.Drawing.dll /r:Accessibility.dll \
	/r:Mono.Posix.dll \
	@System.Windows.Forms.dll.resources \
	/nowarn:649 /nowarn:169 /nowarn:67 /nowarn:108 /nowarn:114

RESOURCES = \
	resources/System.Windows.Forms.resources \
	resources/System.Windows.Forms.en.resources \
	resources/System.Windows.Forms.de.resources

PREBUILT = $(RESOURCES:=.prebuilt)

EXTRA_DISTFILES = \
	README System.Windows.Forms.dll.resources \
	$(RESOURCES:.resources=.resx) $(PREBUILT) build-csproj

TEST_MCS_FLAGS = /r:System.Drawing.dll

SWF.csproj: System.Windows.Forms.dll.sources
	./build-csproj

include ../../build/library.make

$(the_lib): $(RESOURCES) SWF.csproj

$(RESOURCES): %.resources: %.resx
	$(RESGEN) $< || cp $@.prebuilt $@

$(PREBUILT): %.prebuilt: %
	cp $* $@

dist-default: $(PREBUILT)
