thisdir = packages
include ../build/rules.make

ROSLYN_CSC_DIR = $(dir $(CSC_LOCATION))

ROSLYN_FILES = \
	$(ROSLYN_CSC_DIR)/csc.exe				\
	$(ROSLYN_CSC_DIR)/csc.rsp				\
	$(ROSLYN_CSC_DIR)/csc.exe.config			\
	$(ROSLYN_CSC_DIR)/Microsoft.CodeAnalysis.CSharp.dll	\
	$(ROSLYN_CSC_DIR)/Microsoft.CodeAnalysis.dll		\
	$(ROSLYN_CSC_DIR)/System.Collections.Immutable.dll	\
	$(ROSLYN_CSC_DIR)/System.Reflection.Metadata.dll

ROSLYN_AOT_FILES = \
	$(ROSLYN_CSC_DIR)/csc.exe$(PLATFORM_AOT_SUFFIX)		\
	$(ROSLYN_CSC_DIR)/System.Reflection.Metadata.dll$(PLATFORM_AOT_SUFFIX)

DISTFILES = $(ROSLYN_FILES)

ifeq ($(PROFILE), $(DEFAULT_PROFILE))

TARGET_DIR = $(DESTDIR)$(mono_libdir)/mono/$(FRAMEWORK_VERSION)

install-local: $(ROSLYN_FILES) $(ROSLYN_AOT_FILES)
	$(MKINSTALLDIRS) $(TARGET_DIR)
	$(INSTALL_LIB) $(ROSLYN_FILES) $(TARGET_DIR)
	$(INSTALL_LIB) $(ROSLYN_AOT_FILES) $(TARGET_DIR)

endif

dist-local: dist-default
