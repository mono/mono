
##
# Parameters
#  $(1): version
#  $(2): target
#  $(3): src
define LLVMProvisionTemplate
_$(1)-$(2)_HASH = $$(shell git -C $(3) rev-parse HEAD)
_$(1)-$(2)_PACKAGE = $(1)-$(2)-$$(_$(1)-$(2)_HASH)-$$(UNAME).tar.gz
_$(1)-$(2)_URL = "http://xamjenkinsartifact.blob.core.windows.net/mono-sdks/$$(_$(1)-$(2)_PACKAGE)"

$$(TOP)/sdks/out/$(1)-$(2)/.stamp-download:
	curl --location --silent --show-error $$(_$(1)-$(2)_URL) | tar -xvzf - -C $$(dir $$@)
	touch $$@

.PHONY: download-$(1)-$(2)
download-$(1)-$(2): | $(3) setup-$(1)-$(2)
	-$$(MAKE) $$(TOP)/sdks/out/$(1)-$(2)/.stamp-download

.PHONY: provision-$(1)-$(2)
provision-$(1)-$(2): | $(3) download-$(1)-$(2)
	$$(if $$(wildcard $$(TOP)/sdks/out/$(1)-$(2)/.stamp-download),,$$(MAKE) package-$(1)-$(2))

.PHONY: archive-$(1)-$(2)
archive-$(1)-$(2): package-$(1)-$(2)
	tar -cvzf $$(TOP)/$$(_$(1)-$(2)_PACKAGE) -C $$(TOP)/sdks/out/$(1)-$(2) .
endef

$(eval $(call LLVMProvisionTemplate,llvm,llvm64,$(TOP)/external/llvm-project/llvm))
$(eval $(call LLVMProvisionTemplate,llvm,llvmwin64,$(TOP)/external/llvm-project/llvm))
ifeq ($(UNAME),Windows)
$(eval $(call LLVMProvisionTemplate,llvm,llvmwin64-msvc,$(TOP)/external/llvm-project/llvm))
endif

##
# Parameters
#  $(1): target
define LLVMTemplate

_llvm-$(1)_CMAKE_ARGS = \
	$$(llvm-$(1)_CMAKE_ARGS)

.PHONY: setup-llvm-$(1)
setup-llvm-$(1):
	mkdir -p $$(TOP)/sdks/out/llvm-$(1)

.PHONY: package-llvm-$(1)
package-llvm-$(1): setup-llvm-$(1)
	$$(MAKE) -C $$(TOP)/llvm -f build.mk install-llvm \
		LLVM_BUILD="$$(TOP)/sdks/builds/llvm-$(1)" \
		LLVM_PREFIX="$$(TOP)/sdks/out/llvm-$(1)" \
		LLVM_CMAKE_ARGS="$$(_llvm-$(1)_CMAKE_ARGS)"

.PHONY: clean-llvm-$(1)
clean-llvm-$(1)::
	$$(MAKE) -C $$(TOP)/llvm -f build.mk clean-llvm \
		LLVM_BUILD="$$(TOP)/sdks/builds/llvm-$(1)" \
		LLVM_PREFIX="$$(TOP)/sdks/out/llvm-$(1)"

endef

##
# Parameters:
#  $(1): target
define LLVMTemplateStub

.PHONY: setup-llvm-$(1)
setup-llvm-$(1):
	@echo "TODO: setup-llvm-$(1) on $(NAME)"

.PHONY: package-llvm-$(1)
package-llvm-$(1):
	@echo "TODO: package-llvm-$(1) on $(UNAME)"

.PHONY: clean-llvm-$(1)
clean-llvm-$(1)::
	@echo "TODO: clean-llvm-$(1) on $(UNAME)"

endef

$(eval $(call LLVMTemplate,llvm64))

##
# Parameters
#  $(1): target
#  $(2): arch
#  $(3): mxe
define LLVMMxeTemplate

# -DCROSS_TOOLCHAIN_FLAGS_NATIVE is needed to compile the native tools (tlbgen) using the host compilers
# -DLLVM_ENABLE_THREADS=0 is needed because mxe doesn't define std::mutex etc.
# -DLLVM_BUILD_EXECUTION_ENGINE=Off is needed because it depends on threads
# -DCMAKE_EXE_LINKER_FLAGS=-static is needed so that we don't dynamically link with any of the mingw gcc support libs.
_llvm-$(1)_CMAKE_ARGS = \
	-DCMAKE_EXE_LINKER_FLAGS=\"-static\" \
	-DCROSS_TOOLCHAIN_FLAGS_NATIVE=-DCMAKE_TOOLCHAIN_FILE=$$(TOP)/external/llvm-project/llvm/cmake/modules/NATIVE.cmake \
	-DCMAKE_TOOLCHAIN_FILE=$$(TOP)/external/llvm-project/llvm/cmake/modules/$(3).cmake \
	-DLLVM_ENABLE_THREADS=Off \
	-DLLVM_BUILD_EXECUTION_ENGINE=Off \
	$$(llvm-$(1)_CMAKE_ARGS)

ifeq ($(UNAME),Darwin)
_llvm-$(1)_CMAKE_ARGS += \
	-DZLIB_ROOT=$$(MXE_PREFIX)/opt/mingw-zlib/usr/$(2)-w64-mingw32 -DZLIB_LIBRARY=$$(MXE_PREFIX)/opt/mingw-zlib/usr/$(2)-w64-mingw32/lib/libz.a -DZLIB_INCLUDE_DIR=$$(MXE_PREFIX)/opt/mingw-zlib/usr/$(2)-w64-mingw32/include
endif

$$(TOP)/external/llvm-project/llvm/cmake/modules/$(3).cmake: $(3).cmake.in
	sed -e 's,@MXE_PATH@,$$(MXE_PREFIX),' < $$< > $$@

.PHONY: setup-llvm-$(1)
setup-llvm-$(1):
	mkdir -p $$(TOP)/sdks/out/llvm-$(1)

.PHONY: package-llvm-$(1)
package-llvm-$(1): $$(TOP)/external/llvm-project/llvm/cmake/modules/$(3).cmake setup-llvm-$(1)
	$$(MAKE) -C $$(TOP)/llvm -f build.mk install-llvm \
		LLVM_BUILD="$$(TOP)/sdks/builds/llvm-$(1)" \
		LLVM_PREFIX="$$(TOP)/sdks/out/llvm-$(1)" \
		LLVM_CMAKE_ARGS="$$(_llvm-$(1)_CMAKE_ARGS)"

.PHONY: clean-llvm-$(1)
clean-llvm-$(1)::
	$$(MAKE) -C $$(TOP)/llvm -f build.mk clean-llvm \
		LLVM_BUILD="$$(TOP)/sdks/builds/llvm-$(1)" \
		LLVM_PREFIX="$$(TOP)/sdks/out/llvm-$(1)"

endef

ifneq ($(MXE_PREFIX),)
$(eval $(call LLVMMxeTemplate,llvmwin64,x86_64,mxe-Win64))
endif

##
# Parameters
#  $(1): target
#  $(2): arch
define LLVMMsvcTemplate

.PHONY: setup-llvm-$(1)
setup-llvm-$(1):
	mkdir -p $$(TOP)/sdks/out/llvm-$(1)

.PHONY: package-llvm-$(1)
package-llvm-$(1): setup-llvm-$(1)
	$$(TOP)/llvm/build_llvm_msbuild.sh "build" "$(2)" "release" "$$(TOP)/msvc/" "$$(TOP)/sdks/builds/llvm-$(1)" "$$(TOP)/sdks/out/llvm-$(1)"

.PHONY: clean-llvm-$(1)
clean-llvm-$(1):
	$$(TOP)/llvm/build_llvm_msbuild.sh "clean" "$(2)" "release" "$$(TOP)/msvc/" "$$(TOP)/sdks/builds/llvm-$(1)" "$$(TOP)/sdks/out/llvm-$(1)"

endef

ifeq ($(UNAME),Windows)
$(eval $(call LLVMMsvcTemplate,llvmwin64-msvc,x86_64))
endif
