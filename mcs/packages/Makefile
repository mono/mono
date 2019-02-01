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
	$(ROSLYN_CSC_DIR)/vbc.exe				\
	$(ROSLYN_CSC_DIR)/vbc.exe.config			\
	$(ROSLYN_CSC_DIR)/vbc.rsp				\
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
	$(ROSLYN_CSC_DIR)/Microsoft.Managed.Core.targets	 \
	$(ROSLYN_CSC_DIR)/Microsoft.VisualBasic.Core.targets

ROSLYN_DIM_FILES = $(topdir)/../external/roslyn-binaries/Prototypes/DefaultInterfaceImplementation/*

DISTFILES = $(ROSLYN_FILES_FOR_MONO) $(ROSLYN_FILES_TO_COPY_FOR_MSBUILD) csi-test.csx

ifeq ($(PROFILE), $(DEFAULT_PROFILE))

TARGET_DIR = $(DESTDIR)$(mono_libdir)/mono/$(FRAMEWORK_VERSION)
MSBUILD_ROSLYN_DIR = $(DESTDIR)$(mono_libdir)/mono/msbuild/Current/bin/Roslyn

install-local: install-prototypes
	$(MKINSTALLDIRS) $(TARGET_DIR)
	$(INSTALL_LIB) $(ROSLYN_FILES_FOR_MONO) $(TARGET_DIR)
	$(MKINSTALLDIRS) $(MSBUILD_ROSLYN_DIR)
	$(INSTALL_LIB) $(ROSLYN_FILES_TO_COPY_FOR_MSBUILD) $(MSBUILD_ROSLYN_DIR)

	(cd $(MSBUILD_ROSLYN_DIR); for asm in $(ROSLYN_FILES_FOR_MONO); do ln -fs ../../../../$(FRAMEWORK_VERSION)/$$(basename $$asm) . ; done)

install-prototypes:
	$(MKINSTALLDIRS) $(TARGET_DIR)/dim
	$(INSTALL_LIB) $(ROSLYN_DIM_FILES) $(TARGET_DIR)/dim

run-test-local: test-csi

test-csi:
	MONO_PATH="$(topdir)/class/lib/$(PROFILE)" $(RUNTIME) $(ROSLYN_CSC_DIR)/csi.exe csi-test.csx > csi-test-output.txt
	cat csi-test-output.txt && grep -q "hello world" csi-test-output.txt
	rm csi-test-output.txt

endif

dist-local: dist-default
