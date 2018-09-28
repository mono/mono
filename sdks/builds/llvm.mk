
LLVM_SRC?=$(TOP)/sdks/builds/toolchains/llvm

$(TOP)/sdks/builds/toolchains/llvm:
	git clone -b $(LLVM_BRANCH) https://github.com/mono/llvm.git $@
	cd $@ && git checkout $(LLVM_HASH)

$(LLVM_SRC)/CMakeLists.txt: | $(LLVM_SRC)

LLVM36_SRC?=$(TOP)/sdks/builds/toolchains/llvm36

$(TOP)/sdks/builds/toolchains/llvm36:
	git clone -b $(LLVM36_BRANCH) https://github.com/mono/llvm.git $@
	cd $@ && git checkout $(LLVM36_HASH)

$(LLVM36_SRC)/configure: | $(LLVM36_SRC)

# Compile only a subset of tools to speed up the build and avoid building tools which use threads since they don't build on mxe
llvm_CMAKE_FLAGS = \
	-DCMAKE_BUILD_TYPE=Release \
	-DLLVM_TARGETS_TO_BUILD="X86;ARM;AArch64" \
	-DLLVM_BUILD_TESTS=Off -DLLVM_INCLUDE_TESTS=Off \
	-DLLVM_BUILD_EXAMPLES=Off -DLLVM_INCLUDE_EXAMPLES=Off \
	-DLLVM_TOOLS_TO_BUILD="opt;llc;llvm-config;llvm-dis" \
	$(if $(NINJA),-G Ninja,)

##
# Parameters
#  $(1): version
#  $(2): target
#  $(3): configure script
define LLVMProvisionTemplate
_$(1)-$(2)_HASH = $$(shell git -C $$(dir $(3)) rev-parse HEAD)
_$(1)-$(2)_PACKAGE = $(1)-$(2)-$$(_$(1)-$(2)_HASH)-$$(UNAME).tar.gz
_$(1)-$(2)_URL = "http://xamjenkinsartifact.blob.core.windows.net/mono-sdks/$$(_$(1)-$(2)_PACKAGE)"

$$(TOP)/sdks/out/$(1)-$(2)/.stamp-download:
	curl --location --silent --show-error $$(_$(1)-$(2)_URL) | tar -xvzf - -C $$(dir $$@)
	touch $$@

.PHONY: download-$(1)-$(2)
download-$(1)-$(2): | setup-$(1)-$(2)
	-$$(MAKE) $$(TOP)/sdks/out/$(1)-$(2)/.stamp-download

.PHONY: provision-$(1)-$(2)
provision-$(1)-$(2): $(3) | download-$(1)-$(2)
	$$(if $$(wildcard $$(TOP)/sdks/out/$(1)-$(2)/.stamp-download),,$$(MAKE) package-$(1)-$(2))

.PHONY: archive-$(1)-$(2)
archive-$(1)-$(2): package-$(1)-$(2)
	tar -cvzf $$(TOP)/$$(_$(1)-$(2)_PACKAGE) -C $$(TOP)/sdks/out/$(1)-$(2) .
endef

$(eval $(call LLVMProvisionTemplate,llvm,llvm32,$(LLVM_SRC)/CMakeLists.txt))
$(eval $(call LLVMProvisionTemplate,llvm,llvm64,$(LLVM_SRC)/CMakeLists.txt))
$(eval $(call LLVMProvisionTemplate,llvm,llvmwin32,$(LLVM_SRC)/CMakeLists.txt))
$(eval $(call LLVMProvisionTemplate,llvm,llvmwin64,$(LLVM_SRC)/CMakeLists.txt))
ifeq ($(UNAME),Darwin)
$(eval $(call LLVMProvisionTemplate,llvm36,llvm32,$(LLVM36_SRC)/configure))
endif

##
# Parameters
#  $(1): target
define LLVMTemplate

_llvm-$(1)_CMAKE_FLAGS = \
	$$(llvm_CMAKE_FLAGS) \
	-DCMAKE_INSTALL_PREFIX=$$(TOP)/sdks/out/llvm-$(1) \
	$$(llvm-$(1)_CMAKE_FLAGS)

.stamp-llvm-$(1)-configure: $$(LLVM_SRC)/CMakeLists.txt
	mkdir -p $$(TOP)/sdks/builds/llvm-$(1)
	cd $$(TOP)/sdks/builds/llvm-$(1) && cmake $$(_llvm-$(1)_CMAKE_FLAGS) $$(LLVM_SRC)
	touch $$@

.PHONY: setup-llvm-$(1)
setup-llvm-$(1):
	mkdir -p $$(TOP)/sdks/out/llvm-$(1)

.PHONY: build-llvm-$(1)
build-llvm-$(1): .stamp-llvm-$(1)-configure
	cmake --build $$(TOP)/sdks/builds/llvm-$(1)

.PHONY: package-llvm-$(1)
package-llvm-$(1): setup-llvm-$(1) build-llvm-$(1)
	cmake --build $$(TOP)/sdks/builds/llvm-$(1) --target install

.PHONY: clean-llvm-$(1)
clean-llvm-$(1)::
	rm -rf .stamp-llvm-$(1)-configure $$(TOP)/sdks/builds/llvm-$(1) $$(TOP)/sdks/builds/llvm-$(1).config.cache $$(TOP)/sdks/out/llvm-$(1)

endef

llvm-llvm32_CMAKE_FLAGS=-DLLVM_BUILD_32_BITS=On
$(eval $(call LLVMTemplate,llvm32))
$(eval $(call LLVMTemplate,llvm64))

##
# Parameters
#  $(1): target
#  $(2): arch
define LLVM36Template

_llvm36-$(1)_CFLAGS=

_llvm36-$(1)_CXXFLAGS= \
	$$(if $$(filter $$(UNAME),Darwin),-mmacosx-version-min=10.9 -stdlib=libc++)

_llvm36-$(1)_LDFLAGS= \
	$$(if $$(filter $$(UNAME),Darwin),-mmacosx-version-min=10.9)

_llvm36-$(1)_CONFIGURE_ENVIRONMENT= \
	$$(if $$(llvm36-$(1)_CC),CC="$$(llvm36-$(1)_CC)") \
	$$(if $$(llvm36-$(1)_CXX),CXX="$$(llvm36-$(1)_CXX)") \
	CFLAGS="$$(_llvm36-$(1)_CFLAGS)" \
	CXXFLAGS="$$(_llvm36-$(1)_CXXFLAGS)" \
	LDFLAGS="$$(_llvm36-$(1)_LDFLAGS)"

_llvm36-$(1)_CONFIGURE_FLAGS= \
	--host=$$(if $$(filter $$(UNAME),Darwin),$(2)-apple-darwin10,$$(if $$(filter $$(UNAME),Linux),$(2)-linux-gnu,$$(error "Unknown UNAME='$$(UNAME)'"))) \
	--cache-file=$$(TOP)/sdks/builds/llvm36-$(1).config.cache \
	--prefix=$$(TOP)/sdks/out/llvm36-$(1) \
	--enable-assertions=no \
	--enable-optimized \
	--enable-targets="arm,aarch64,x86" \
	$$(if $$(filter $$(UNAME),Darwin),--enable-libcpp)

.stamp-llvm36-$(1)-configure: $$(LLVM36_SRC)/configure
	mkdir -p $$(TOP)/sdks/builds/llvm36-$(1)
	cd $$(TOP)/sdks/builds/llvm36-$(1) && $$< $$(_llvm36-$(1)_CONFIGURE_ENVIRONMENT) $$(_llvm36-$(1)_CONFIGURE_FLAGS)
	touch $$@

.PHONY: setup-llvm36-$(1)
setup-llvm36-$(1):
	mkdir -p $$(TOP)/sdks/out/llvm36-$(1)

.PHONY: build-llvm36-$(1)
build-llvm36-$(1): .stamp-llvm36-$(1)-configure
	$$(MAKE) -C $$(TOP)/sdks/builds/llvm36-$(1)

.PHONY: package-llvm36-$(1)
package-llvm36-$(1): setup-llvm36-$(1) build-llvm36-$(1)
	$$(MAKE) -C $$(TOP)/sdks/builds/llvm36-$(1) install

.PHONY: clean-llvm36-$(1)
clean-llvm36-$(1)::
	rm -rf .stamp-llvm36-$(1)-configure $$(TOP)/sdks/builds/llvm36-$(1) $$(TOP)/sdks/builds/llvm36-$(1).config.cache $$(TOP)/sdks/out/llvm36-$(1)

endef

ifeq ($(UNAME),Darwin)
$(eval $(call LLVM36Template,llvm32,i386))
endif

##
# Parameters
#  $(1): target
#  $(2): arch
#
# Flags
#  llvm-$(1)_CONFIGURE_ENVIRONMENT
define LLVMMxeTemplate

_llvm-$(1)_CMAKE=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32$$(if $$(filter $$(UNAME),Darwin),.static)-cmake

# -DLLVM_ENABLE_THREADS=0 is needed because mxe doesn't define std::mutex etc.
# -DLLVM_BUILD_EXECUTION_ENGINE=Off is needed because it depends on threads
# -DCROSS_TOOLCHAIN_FLAGS_NATIVE is needed to compile the native tools (tlbgen) using the host compilers
_llvm-$(1)_CMAKE_FLAGS = \
	$$(llvm_CMAKE_FLAGS) \
	-DCMAKE_INSTALL_PREFIX=$$(TOP)/sdks/out/llvm-$(1) \
	-DLLVM_ENABLE_THREADS=OFF \
	-DCROSS_TOOLCHAIN_FLAGS_NATIVE=-DCMAKE_TOOLCHAIN_FILE=$$(LLVM_SRC)/cmake/modules/NATIVE.cmake \
	-DLLVM_BUILD_EXECUTION_ENGINE=Off \
	$$(llvm-$(1)_CMAKE_FLAGS)

.stamp-llvm-$(1)-configure: $$(LLVM_SRC)/CMakeLists.txt
	mkdir -p $$(TOP)/sdks/builds/llvm-$(1)
	cd $$(TOP)/sdks/builds/llvm-$(1) && $$(_llvm-$(1)_CMAKE) $$(_llvm-$(1)_CMAKE_FLAGS) $$(LLVM_SRC)
	touch $$@

.PHONY: setup-llvm-$(1)
setup-llvm-$(1):
	mkdir -p $$(TOP)/sdks/out/llvm-$(1)

.PHONY: build-llvm-$(1)
build-llvm-$(1): .stamp-llvm-$(1)-configure
	$$(_llvm-$(1)_CMAKE) --build $$(TOP)/sdks/builds/llvm-$(1)

package-llvm-$(1): setup-llvm-$(1) build-llvm-$(1)
	$$(_llvm-$(1)_CMAKE) --build $$(TOP)/sdks/builds/llvm-$(1) --target install

.PHONY: clean-llvm-$(1)
clean-llvm-$(1)::
	rm -rf .stamp-llvm-$(1)-configure $$(TOP)/sdks/builds/llvm-$(1) $$(TOP)/sdks/builds/llvm-$(1).config.cache $$(TOP)/sdks/out/llvm-$(1)

endef

ifneq ($(MXE_PREFIX),)
llvm-llvmwin32_CMAKE_FLAGS=-DLLVM_BUILD_32_BITS=On
$(eval $(call LLVMMxeTemplate,llvmwin32,i686))
$(eval $(call LLVMMxeTemplate,llvmwin64,x86_64))
endif
