LLVM_SRC?=$(TOP)/sdks/builds/toolchains/llvm

$(TOP)/sdks/builds/toolchains/llvm:
	git clone -b $(LLVM_BRANCH) https://github.com/mono/llvm.git $@
	cd $@ && git checkout $(LLVM_HASH)

$(LLVM_SRC)/configure: | $(LLVM_SRC)

# Compile only a subset of tools to speed up the build and avoid building tools which use threads since they don't build on mxe
llvm_base_CMAKE_FLAGS = \
	-DCMAKE_BUILD_TYPE=Release \
	-DLLVM_TARGETS_TO_BUILD="X86;ARM;AArch64" \
	-DLLVM_BUILD_TESTS=Off -DLLVM_INCLUDE_TESTS=Off \
	-DLLVM_BUILD_EXAMPLES=Off -DLLVM_INCLUDE_EXAMPLES=Off \
	-DLLVM_TOOLS_TO_BUILD="opt;llc;llvm-config;llvm-dis" \
	$(if $(NINJA),-G Ninja,)

##
# Parameters
#  $(1): target
#  $(2): arch
define LLVMTemplate

_llvm_$(1)_CXXFLAGS= \
	$$(if $$(filter $$(UNAME),Darwin),-mmacosx-version-min=10.9 -stdlib=libc++)

_llvm_$(1)_LDFLAGS= \
	$$(if $$(filter $$(UNAME),Darwin),-mmacosx-version-min=10.9)

_llvm_$(1)_CONFIGURE_ENVIRONMENT= \
	CXXFLAGS="$$(_llvm_$(1)_CXXFLAGS)" \
	LDFLAGS="$$(_llvm_$(1)_LDFLAGS)"

_llvm_$(1)_CMAKE_FLAGS = \
	$(llvm_base_CMAKE_FLAGS) \
	-DCMAKE_INSTALL_PREFIX=$$(TOP)/sdks/out/llvm-$(1) \
	$$(llvm_$(1)_CMAKE_FLAGS) \
	$(LLVM_SRC)

.PHONY: package-llvm-$(1)

build-custom-llvm-$(1):
	cmake --build $$(TOP)/sdks/builds/llvm-$(1)

package-llvm-$(1):
	$$(MAKE) -C $$(TOP)/llvm -f Makefile.am install-llvm \
		LLVM_PATH="$(LLVM_SRC)" \
		LLVM_BUILD="$$(TOP)/sdks/builds/llvm-$(1)" \
		LLVM_PREFIX="$$(TOP)/sdks/out/llvm-$(1)" \
		$$(if $$(llvm-$(1)_CMAKE_ARGS),LLVM_CMAKE_ARGS="$$(llvm-$(1)_CMAKE_ARGS)")

.PHONY: clean-llvm-$(1)
clean-llvm-$(1):
	$$(MAKE) clean -C $$(TOP)/llvm

TARGETS += llvm-$(1)

endef

llvm_llvm64_CMAKE_FLAGS =
# We only use this for the cross compiler so it needs no architectures/tools
# Some products might only build llvm32 so disable this for now
#llvm_llvm32_CMAKE_FLAGS = -DLLVM_BUILD_32_BITS=On -DLLVM_TARGETS_TO_BUILD="" -DLLVM_BUILD_TOOLS=Off -DLLVM_BUILD_UTILS=Off
llvm_llvm32_CMAKE_FLAGS = -DLLVM_BUILD_32_BITS=On

$(eval $(call LLVMTemplate,llvm32,i386))
$(eval $(call LLVMTemplate,llvm64,x86_64))

##
# Parameters
#  $(1): target
#  $(2): arch
#
# Flags
#  llvm_$(1)_CONFIGURE_ENVIRONMENT
define LLVMMxeTemplate

_llvm_$(1)_CXXFLAGS=

_llvm_$(1)_LDFLAGS=

_llvm_$(1)_CMAKE = $(2)-w64-mingw32$$(if $$(filter $$(UNAME),Darwin),.static)

# -DLLVM_ENABLE_THREADS=0 is needed because mxe doesn't define std::mutex etc.
# -DLLVM_BUILD_EXECUTION_ENGINE=Off is needed because it depends on threads
# -DCROSS_TOOLCHAIN_FLAGS_NATIVE is needed to compile the native tools (tlbgen) using the host compilers
_llvm_$(1)_CMAKE_FLAGS = \
	$(llvm_base_CMAKE_FLAGS) \
	-DCMAKE_INSTALL_PREFIX=$$(TOP)/sdks/out/llvm-$(1) \
	-DLLVM_ENABLE_THREADS=OFF \
	-DCROSS_TOOLCHAIN_FLAGS_NATIVE=-DCMAKE_TOOLCHAIN_FILE=$(LLVM_SRC)/cmake/modules/NATIVE.cmake \
	-DLLVM_BUILD_EXECUTION_ENGINE=Off \
	$$(llvm_$(1)_CMAKE_FLAGS) \
	$(LLVM_SRC)

.stamp-llvm-$(1)-toolchain: | $$(LLVM_SRC)
	touch $$@

.stamp-llvm-$(1)-configure:
	mkdir -p $$(TOP)/sdks/builds/llvm-$(1)
	cd $$(TOP)/sdks/builds/llvm-$(1) && $$(_llvm_$(1)_CMAKE) $$(_llvm_$(1)_CMAKE_FLAGS)
	touch $$@

build-custom-llvm-$(1):
	$(_llvm_$(1)_CMAKE) --build $$(TOP)/sdks/builds/llvm-$(1)

package-llvm-$(1):
	$(_llvm_$(1)_CMAKE) --build $$(TOP)/sdks/builds/llvm-$(1) --target install

.PHONY: clean-llvm-$(1)
clean-llvm-$(1):
	rm -rf .stamp-llvm-$(1)-toolchain .stamp-llvm-$(1)-configure $$(TOP)/sdks/builds/llvm-$(1) $$(TOP)/sdks/builds/llvm-$(1).config.cache $$(TOP)/sdks/out/llvm-$(1)

TARGETS += llvm-$(1)

endef

llvm_llvmwin64_CMAKE_FLAGS =
llvm_llvmwin32_CMAKE_FLAGS = -DLLVM_BUILD_32_BITS=On

ifneq ($(MXE_PREFIX),)
$(eval $(call LLVMMxeTemplate,llvmwin32,i686))
$(eval $(call LLVMMxeTemplate,llvmwin64,x86_64))
endif
