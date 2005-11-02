thisdir = class/Managed.Windows.Forms
include ../../build/rules.make

LIBRARY = System.Windows.Forms.dll

# UTF-8
CODEPAGE = 65001

LIB_MCS_FLAGS = /unsafe \
	/r:$(corlib) /r:System.dll /r:System.Xml.dll \
	/r:System.Drawing.dll /r:Accessibility.dll \
	/r:System.Data.dll /r:Mono.Posix.dll \
	@System.Windows.Forms.dll.resources \
	/nowarn:649 /nowarn:169 /nowarn:67 /nowarn:108 /nowarn:114 -debug

RESX_RESOURCES = \
	resources/System.Windows.Forms.resources \
	resources/System.Windows.Forms.en.resources \
	resources/System.Windows.Forms.de.resources

CUR_RESOURCES = \
	resources/SplitterNS.cur \
	resources/SplitterWE.cur \
	resources/DnDNo.cur \
	resources/DnDCopy.cur \
	resources/DnDLink.cur \
	resources/DnDMove.cur

RESOURCES = $(RESX_RESOURCES) $(CUR_RESOURCES)

PREBUILT = $(RESX_RESOURCES:=.prebuilt)

EXTRA_DISTFILES = \
	README System.Windows.Forms.dll.resources \
	$(RESX_RESOURCES:.resources=.resx) $(CUR_RESOURCES) $(PREBUILT) build-csproj

TEST_MCS_FLAGS = /r:System.Drawing.dll -resource:Test/resources/a.cur,a.cur

SWF.csproj: System.Windows.Forms.dll.sources
	./build-csproj

include ../../build/library.make

$(the_lib): $(RESOURCES) SWF.csproj

$(RESX_RESOURCES): %.resources: %.resx
	$(RESGEN) $< || cp $@.prebuilt $@

$(LIBRARY): $(CUR_RESOURCES)

$(PREBUILT): %.prebuilt: %
	cp $* $@

dist-default: $(PREBUILT)
