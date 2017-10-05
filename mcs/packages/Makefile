thisdir = packages
include ../build/rules.make

ROSLYN_CSC_DIR = $(dir $(CSC_LOCATION))

ROSLYN_FILES_FOR_MONO = \
	$(ROSLYN_CSC_DIR)/csc.exe				\
	$(ROSLYN_CSC_DIR)/csc.rsp				\
	$(ROSLYN_CSC_DIR)/csc.exe.config			\
	$(ROSLYN_CSC_DIR)/csi.exe				\
	$(ROSLYN_CSC_DIR)/csi.exe.config			\
	$(ROSLYN_CSC_DIR)/csi.rsp				\
	$(ROSLYN_CSC_DIR)/Microsoft.CodeAnalysis.CSharp.dll	\
	$(ROSLYN_CSC_DIR)/Microsoft.CodeAnalysis.CSharp.Scripting.dll \
	$(ROSLYN_CSC_DIR)/Microsoft.CodeAnalysis.VisualBasic.dll \
	$(ROSLYN_CSC_DIR)/Microsoft.CodeAnalysis.dll		\
	$(ROSLYN_CSC_DIR)/Microsoft.CodeAnalysis.Scripting.dll \
	$(ROSLYN_CSC_DIR)/System.Collections.Immutable.dll	\
	$(ROSLYN_CSC_DIR)/System.Reflection.Metadata.dll	\
	$(ROSLYN_CSC_DIR)/VBCSCompiler.exe			\
	$(ROSLYN_CSC_DIR)/VBCSCompiler.exe.config

ROSLYN_FILES_TO_COPY_FOR_MSBUILD = \
	$(ROSLYN_CSC_DIR)/Microsoft.Build.Tasks.CodeAnalysis.dll \
	$(ROSLYN_CSC_DIR)/Microsoft.CSharp.Core.targets 	 \
	$(ROSLYN_CSC_DIR)/Microsoft.VisualBasic.Core.targets

ROSLYN_FILES_TO_LINK_FOR_MSBUILD = \
	csi.exe						\
	csi.exe.config					\
	csi.rsp						\
	Microsoft.CodeAnalysis.dll			\
	Microsoft.CodeAnalysis.CSharp.dll		\
	Microsoft.CodeAnalysis.CSharp.Scripting.dll 	\
	Microsoft.CodeAnalysis.Scripting.dll 		\
	System.Collections.Immutable.dll		\
	System.Reflection.Metadata.dll			\
	VBCSCompiler.exe				\
	VBCSCompiler.exe.config

DISTFILES = $(ROSLYN_FILES_FOR_MONO) $(ROSLYN_FILES_TO_COPY_FOR_MSBUILD)

ifeq ($(PROFILE), $(DEFAULT_PROFILE))

TARGET_DIR = $(DESTDIR)$(mono_libdir)/mono/$(FRAMEWORK_VERSION)
MSBUILD_ROSLYN_DIR = $(DESTDIR)$(mono_libdir)/mono/msbuild/15.0/bin/Roslyn

install-local:
	$(MKINSTALLDIRS) $(TARGET_DIR)
	$(INSTALL_LIB) $(ROSLYN_FILES_FOR_MONO) $(TARGET_DIR)
	$(MKINSTALLDIRS) $(MSBUILD_ROSLYN_DIR)
	$(INSTALL_LIB) $(ROSLYN_FILES_TO_COPY_FOR_MSBUILD) $(MSBUILD_ROSLYN_DIR)

	(cd $(MSBUILD_ROSLYN_DIR); for asm in $(ROSLYN_FILES_TO_LINK_FOR_MSBUILD); do ln -fs ../../../../$(FRAMEWORK_VERSION)/$$asm . ; done)

endif

dist-local: dist-default
