thisdir = class/Moonlight.Build.Tasks
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Moonlight.Build.Tasks.dll
LIBRARY_NAME = Moonlight.Build.Tasks.dll

ifeq (3.5, $(FRAMEWORK_VERSION))
NAME_SUFFIX = .v3.5
else
ifeq (4, $(FRAMEWORK_VERSION_MAJOR))
NAME_SUFFIX = .v4.0
endif
endif

NO_INSTALL = yes
NO_TEST = yes
NO_SIGN_ASSEMBLY = yes

REFERENCES = \
	/r:$(corlib)				\
	/r:System.dll				\
	/r:System.Xml.dll			\
	/r:Microsoft.Build.Engine.dll	\
	/r:Microsoft.Build.Framework.dll	\
	/r:Microsoft.Build.Utilities$(NAME_SUFFIX).dll \
	/r:Microsoft.Build.Tasks$(NAME_SUFFIX).dll \
	/r:ICSharpCode.SharpZipLib.dll

LIB_MCS_FLAGS = \
	$(REFERENCES) \
	$(RESOURCE_FILES:%=-resource:%)

RESOURCE_FILES = Moonlight.Build.Tasks/PreviewTemplate.html

EXTRA_DISTFILES = $(RESOURCE_FILES)

XBUILD_DIR=../../tools/xbuild
include $(XBUILD_DIR)/xbuild_targets.make

include ../../build/library.make

SILVERLIGHT_DIR = $(DESTDIR)$(mono_libdir)/mono/xbuild/Microsoft/Silverlight

install-local: install-lib

install-lib: $(the_lib)
	-$(MKINSTALLDIRS) $(SILVERLIGHT_DIR)/v2.0
	-$(MKINSTALLDIRS) $(SILVERLIGHT_DIR)/v3.0
	$(INSTALL_DATA) $(topdir)/class/lib/$(PROFILE)/$(LIBRARY_NAME) $(SILVERLIGHT_DIR)/v2.0
	$(INSTALL_DATA) $(topdir)/class/lib/$(PROFILE)/$(LIBRARY_NAME) $(SILVERLIGHT_DIR)/v3.0

uninstall-local: uninstall-lib

uninstall-lib:
	-rm -f $(SILVERLIGHT_DIR)/v2.0/$(LIBRARY_NAME)
	-rm -f $(SILVERLIGHT_DIR)/v3.0/$(LIBRARY_NAME)
