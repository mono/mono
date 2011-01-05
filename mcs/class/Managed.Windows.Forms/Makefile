thisdir = class/Managed.Windows.Forms
include ../../build/rules.make

LIBRARY = System.Windows.Forms.dll

LIB_MCS_FLAGS = /unsafe \
	/r:$(corlib) /r:System.dll /r:System.Xml.dll \
	/r:System.Drawing.dll /r:Accessibility.dll \
	/r:System.Data.dll /r:Mono.Posix.dll \
	/r:Mono.WebBrowser \
	@System.Windows.Forms.dll.resources \
	-debug -nowarn:618,612,809

ifneq (1, $(FRAMEWORK_VERSION_MAJOR))
LIB_MCS_FLAGS += /r:System.Configuration.dll
endif

ifeq (true, $(GENERATE_RESOURCES))
LIB_MCS_FLAGS += /define:GENERATING_RESOURCES
endif

RESX_RESOURCES = \
	resources/keyboards.resources

CUR_RESOURCES = resources/NWSE.cur resources/NESW.cur \
	resources/SplitterNS.cur \
	resources/SplitterWE.cur \
	resources/NWSE.cur \
	resources/NESW.cur \
	resources/DnDNo.cur \
	resources/DnDCopy.cur \
	resources/DnDLink.cur \
	resources/DnDMove.cur

RESOURCES = $(RESX_RESOURCES) $(CUR_RESOURCES)

PREBUILT = $(RESX_RESOURCES:=.prebuilt)

IMAGES_RESOURCES = \
	resources/16_computer.png		\
	resources/32_computer.png		\
	resources/computer.png			\
	resources/16_document-open.png		\
	resources/32_document-open.png		\
	resources/document-open.png		\
	resources/16_folder.png			\
	resources/folder-new.png		\
	resources/folder.png			\
	resources/16_folder-remote.png		\
	resources/32_folder-remote.png		\
	resources/folder-remote.png		\
	resources/go-previous.png		\
	resources/go-top.png			\
	resources/22_page-magnifier.png		\
	resources/preferences-system-windows.png\
	resources/16_printer.png		\
	resources/32_printer.png		\
	resources/text-x-generic.png		\
	resources/16_user-desktop.png		\
	resources/32_user-desktop.png		\
	resources/user-desktop.png		\
	resources/16_user-home.png		\
	resources/32_user-home.png		\
	resources/user-home.png		\
	resources/16_dialog-error.png		\
	resources/16_dialog-information.png	\
	resources/16_dialog-warning.png		\
	resources/1-up.png			\
	resources/2-up.png			\
	resources/3-up.png			\
	resources/4-up.png			\
	resources/6-up.png			\
	resources/nav_delete.png		\
	resources/nav_end.png			\
	resources/nav_first.png			\
	resources/nav_previous.png		\
	resources/nav_next.png			\
	resources/nav_plus.png			\
	resources/image-missing.png		\
	resources/image-x-generic.png		\
	resources/propertygrid-alphabetical.png	\
	resources/propertygrid-categorized.png	\
	resources/EventsTab.bmp			\
	resources/PropertiesTab.bmp 		\
	resources/propertygrid-propertypages.png	\
	resources/mono.ico			\
	resources/errorProvider.ico


TEST_DISTFILES = \
	M.gif \
	Test/resources/a.cur \
	Test/resources/32x32.ico \
	Test/System.Resources/compat_1_1.resx \
	Test/System.Resources/compat_2_0.resx \
	Test/System.Windows.Forms/bitmaps/a.png

EXTRA_DISTFILES = \
	README System.Windows.Forms.dll.resources \
	$(RESX_RESOURCES:.resources=.resx) $(CUR_RESOURCES) $(PREBUILT) build-csproj build-csproj2k5 \
	$(IMAGES_RESOURCES) \
	$(TEST_DISTFILES)

TEST_MCS_FLAGS = /r:System.Data.dll /r:System.Drawing.dll /r:Accessibility.dll \
	-resource:Test/resources/a.cur,a.cur \
	-resource:Test/resources/32x32.ico,32x32.ico \
	-nowarn:618,612

include ../../build/library.make

TEST_HARNESS_EXCLUDES = -exclude=Interactive,NotWorking,ValueAdd,CAS,InetAccess
TEST_HARNESS_EXCLUDES_ONDOTNET = -exclude=Interactive,NotDotNet,CAS

$(the_lib): $(RESOURCES)

$(RESX_RESOURCES): %.resources: %.resx
	$(RESGEN) $< || cp $@.prebuilt $@

$(LIBRARY): $(CUR_RESOURCES)

$(PREBUILT): %.prebuilt: %
	cp $* $@

dist-default: $(PREBUILT)
