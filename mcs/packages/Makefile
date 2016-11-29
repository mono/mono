thisdir = packages
include ../build/rules.make

ROSLYN_CSC_DIR = $(dir $(CSC_LOCATION))

ifeq ($(PROFILE), $(DEFAULT_PROFILE))

TARGET_DIR = $(DESTDIR)$(mono_libdir)/mono/$(FRAMEWORK_VERSION)

install-local:
	$(INSTALL_LIB) $(ROSLYN_CSC_DIR)/csc.exe $(TARGET_DIR)
	$(INSTALL_LIB) $(ROSLYN_CSC_DIR)/csc.rsp $(TARGET_DIR)
	$(INSTALL_LIB) $(ROSLYN_CSC_DIR)/csc.exe.config $(TARGET_DIR)
	$(INSTALL_LIB) $(ROSLYN_CSC_DIR)/Microsoft.CodeAnalysis.CSharp.dll $(TARGET_DIR)
	$(INSTALL_LIB) $(ROSLYN_CSC_DIR)/Microsoft.CodeAnalysis.dll $(TARGET_DIR)
	$(INSTALL_LIB) $(ROSLYN_CSC_DIR)/System.Collections.Immutable.dll $(TARGET_DIR)
	$(INSTALL_LIB) $(ROSLYN_CSC_DIR)/System.Reflection.Metadata.dll $(TARGET_DIR)

	$(INSTALL_LIB) $(ROSLYN_CSC_DIR)/csc.exe$(PLATFORM_AOT_SUFFIX) $(TARGET_DIR)
	$(INSTALL_LIB) $(ROSLYN_CSC_DIR)/System.Reflection.Metadata.dll$(PLATFORM_AOT_SUFFIX) $(TARGET_DIR)
endif


dist-local: dist-default

