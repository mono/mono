thisdir = class/System.Windows.Forms
include ../../build/rules.make

LIBRARY = System.Windows.Forms.dll

LIB_REFS = System System.Xml System.Drawing Accessibility System.Data Mono.Posix Mono.WebBrowser System.Configuration System.Runtime.Serialization.Formatters.Soap
KEYFILE = ../ecma.pub
LIB_MCS_FLAGS = /unsafe \
	@System.Windows.Forms.dll.resources \
	-nowarn:618,612,809

ifeq (true, $(GENERATE_RESOURCES))
LIB_MCS_FLAGS += /define:GENERATING_RESOURCES
endif

RESX_RESOURCES = \
	resources/keyboards.resources \
        resources/System.Windows.Forms.resources

CLEAN_FILES += $(RESX_RESOURCES)

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

TEST_RESOURCE_FILES = \
	Test/resources/M.gif \
	Test/resources/a.cur \
	Test/resources/32x32.ico \
	Test/System.Resources/compat_1_1.resx \
	Test/System.Resources/compat_2_0.resx \
	Test/System.Windows.Forms/bitmaps/a.png

DUMMY_ASSEMBLY_SRC = \
	Test/DummyAssembly/AnotherSerializable.cs \
	Test/DummyAssembly/Convertable.cs \
	Test/DummyAssembly/Properties/AssemblyInfo.cs \

EXTRA_DISTFILES = \
	README System.Windows.Forms.dll.resources \
	$(RESX_RESOURCES:.resources=.resx) $(CUR_RESOURCES) $(PREBUILT) \
	$(IMAGES_RESOURCES) \
	$(TEST_RESOURCE_FILES) \
	$(DUMMY_ASSEMBLY_SRC) \
	Test/simple/Program.cs

TEST_LIB_REFS = System.Data System.Drawing Accessibility System System.Xml System.Runtime.Serialization.Formatters.Soap
TEST_MCS_FLAGS = \
	$(foreach r, $(TEST_RESOURCE_FILES), -resource:$(r),$(r)) \
	-nowarn:618,612

$(topdir)/class/lib/$(PROFILE)/tests/DummyAssembly.dll: $(DUMMY_ASSEMBLY_SRC)
	$(CSCOMPILE) /target:library /out:$@ $^ -r:$(topdir)/class/lib/$(PROFILE)/mscorlib.dll -r:$(topdir)/class/lib/$(PROFILE)/System.dll

test-local: $(topdir)/class/lib/$(PROFILE)/tests/DummyAssembly.dll

include ../../build/library.make

TEST_HARNESS_EXCLUDES += Interactive

$(the_lib): $(RESOURCES)

$(RESX_RESOURCES): %.resources: %.resx
	$(RESGEN) $< || cp $@.prebuilt $@

$(LIBRARY): $(CUR_RESOURCES)

$(PREBUILT): %.prebuilt: %
	cp $* $@

dist-default: $(PREBUILT)

simple-test.exe: Test/simple/Program.cs $(topdir)/class/lib/$(PROFILE)/System.Windows.Forms.dll $(topdir)/class/lib/$(PROFILE)/mscorlib.dll
	$(CSCOMPILE) -out:$@ Test/simple/Program.cs -r:$(topdir)/class/lib/$(PROFILE)/System.Windows.Forms.dll -r:$(topdir)/class/lib/$(PROFILE)/mscorlib.dll

test-simple: simple-test.exe
	$(TEST_RUNTIME) $(TEST_RUNTIME_FLAGS) simple-test.exe

notepad.exe: samples/notepad.cs $(topdir)/class/lib/$(PROFILE)/System.Windows.Forms.dll $(topdir)/class/lib/$(PROFILE)/System.Drawing.dll $(topdir)/class/lib/$(PROFILE)/System.dll $(topdir)/class/lib/$(PROFILE)/mscorlib.dll
	$(CSCOMPILE) -out:$@ samples/notepad.cs -r:$(topdir)/class/lib/$(PROFILE)/System.Windows.Forms.dll -r:$(topdir)/class/lib/$(PROFILE)/System.Drawing.dll -r:$(topdir)/class/lib/$(PROFILE)/System.dll -r:$(topdir)/class/lib/$(PROFILE)/mscorlib.dll

test-notepad: notepad.exe
	$(TEST_RUNTIME) $(TEST_RUNTIME_FLAGS) notepad.exe

issue%.exe: samples/issue%.cs $(topdir)/class/lib/$(PROFILE)/System.Windows.Forms.dll $(topdir)/class/lib/$(PROFILE)/System.Drawing.dll $(topdir)/class/lib/$(PROFILE)/System.dll $(topdir)/class/lib/$(PROFILE)/mscorlib.dll
	$(CSCOMPILE) -out:$@ $< -r:$(topdir)/class/lib/$(PROFILE)/System.Windows.Forms.dll -r:$(topdir)/class/lib/$(PROFILE)/System.Drawing.dll -r:$(topdir)/class/lib/$(PROFILE)/System.dll -r:$(topdir)/class/lib/$(PROFILE)/mscorlib.dll

test-issue%: issue%.exe
	$(TEST_RUNTIME) $(TEST_RUNTIME_FLAGS) $<
