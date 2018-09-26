
LLVM_SRC?=$(TOP)/sdks/builds/toolchains/llvm

$(TOP)/sdks/builds/toolchains/llvm:
	mkdir -p $(dir $@)
	$(MAKE) -C $(TOP)/llvm -f build.mk $@/CMakeLists.txt \
		LLVM_PATH="$@"

$(LLVM_SRC)/CMakeLists.txt: | $(LLVM_SRC)

LLVM36_SRC?=$(TOP)/sdks/builds/toolchains/llvm36

$(TOP)/sdks/builds/toolchains/llvm36:
	mkdir -p $(dir $@)
	git clone -b $(LLVM36_BRANCH) https://github.com/mono/llvm.git $@
	cd $@ && git checkout $(LLVM36_HASH)

$(LLVM36_SRC)/configure: | $(LLVM36_SRC)

##
# Parameters
#  $(1): version
#  $(2): target
#  $(3): configure script
define LLVMProvisionTemplate
ifeq ($$(UNAME),Darwin)
_$(1)-$(2)_HASH = $$(shell git -C $$(dir $(3)) rev-parse HEAD)
_$(1)-$(2)_PACKAGE = $(1)-$(2)-$$(_$(1)-$(2)_HASH).tar.gz
_$(1)-$(2)_URL = "http://xamjenkinsartifact.blob.core.windows.net/mono-sdks/$$(_$(1)-$(2)_PACKAGE)"

$$(TOP)/sdks/out/$(1)-$(2)/.stamp-download:
	curl --location --silent --show-error $$(_$(1)-$(2)_URL) | tar -xvzf - -C $$(TOP)/sdks/out/$(1)-$(2)
	touch $$@

.PHONY: download-$(1)-$(2)
download-$(1)-$(2): | setup-$(1)-$(2)
	-$$(MAKE) $$(TOP)/sdks/out/$(1)-$(2)/.stamp-download

.PHONY: pack-$(1)-$(2)
pack-$(1)-$(2): package-$(1)-$(2)
	tar -cvzf $$(TOP)/sdks/out/$$(_$(1)-$(2)_PACKAGE) -C $$(TOP)/sdks/out/$(1)-$(2) .

.PHONY: provision-$(1)-$(2)
provision-$(1)-$(2): $(3) | download-$(1)-$(2)
	$$(if $$(wildcard $$(TOP)/sdks/out/$(1)-$(2)/.stamp-download),,$$(MAKE) pack-$(1)-$(2))
else
.PHONY: provision-$(1)-$(2)
provision-$(1)-$(2): package-$(1)-$(2)
endif
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

_llvm-$(1)_CMAKE_ARGS = \
	$$(llvm-$(1)_CMAKE_ARGS)

.PHONY: setup-llvm-$(1)
setup-llvm-$(1):
	mkdir -p $$(TOP)/sdks/out/llvm-$(1)

.PHONY: package-llvm-$(1)
package-llvm-$(1): setup-llvm-$(1) $$(LLVM_SRC)/CMakeLists.txt
	$$(MAKE) -C $$(TOP)/llvm -f build.mk install-llvm \
		LLVM_PATH="$$(LLVM_SRC)" \
		LLVM_BUILD="$$(TOP)/sdks/builds/llvm-$(1)" \
		LLVM_PREFIX="$$(TOP)/sdks/out/llvm-$(1)" \
		LLVM_CMAKE_ARGS="$$(_llvm-$(1)_CMAKE_ARGS)"

.PHONY: clean-llvm-$(1)
clean-llvm-$(1)::
	$$(MAKE) -C $$(TOP)/llvm -f build.mk clean-llvm \
		LLVM_PATH="$$(LLVM_SRC)" \
		LLVM_BUILD="$$(TOP)/sdks/builds/llvm-$(1)" \
		LLVM_PREFIX="$$(TOP)/sdks/out/llvm-$(1)"

endef

llvm-llvm32_CMAKE_ARGS=-DLLVM_BUILD_32_BITS=On
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
define LLVMMxeTemplate

_llvm-$(1)_CMAKE=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32$$(if $$(filter $$(UNAME),Darwin),.static)-cmake

# -DCROSS_TOOLCHAIN_FLAGS_NATIVE is needed to compile the native tools (tlbgen) using the host compilers
# -DLLVM_ENABLE_THREADS=0 is needed because mxe doesn't define std::mutex etc.
# -DLLVM_BUILD_EXECUTION_ENGINE=Off is needed because it depends on threads
_llvm-$(1)_CMAKE_ARGS = \
	-DCROSS_TOOLCHAIN_FLAGS_NATIVE=-DCMAKE_TOOLCHAIN_FILE=$(LLVM_SRC)/cmake/modules/NATIVE.cmake \
	-DLLVM_ENABLE_THREADS=Off \
	-DLLVM_BUILD_EXECUTION_ENGINE=Off \
	$$(llvm-$(1)_CMAKE_ARGS)

.PHONY: setup-llvm-$(1)
setup-llvm-$(1):
	mkdir -p $$(TOP)/sdks/out/llvm-$(1)

.PHONY: package-llvm-$(1)
package-llvm-$(1): setup-llvm-$(1) $$(LLVM_SRC)/CMakeLists.txt
	$$(MAKE) -C $$(TOP)/llvm -f build.mk install-llvm \
		CMAKE=$$(_llvm-$(1)_CMAKE) \
		LLVM_PATH="$$(LLVM_SRC)" \
		LLVM_BUILD="$$(TOP)/sdks/builds/llvm-$(1)" \
		LLVM_PREFIX="$$(TOP)/sdks/out/llvm-$(1)" \
		LLVM_CMAKE_ARGS="$$(_llvm-$(1)_CMAKE_ARGS)"

.PHONY: clean-llvm-$(1)
clean-llvm-$(1)::
	$$(MAKE) -C $$(TOP)/llvm -f build.mk clean-llvm \
		CMAKE=$$(_llvm-$(1)_CMAKE) \
		LLVM_PATH="$$(LLVM_SRC)" \
		LLVM_BUILD="$$(TOP)/sdks/builds/llvm-$(1)" \
		LLVM_PREFIX="$$(TOP)/sdks/out/llvm-$(1)"

endef

ifneq ($(MXE_PREFIX),)
llvm-llvmwin32_CMAKE_ARGS=-DLLVM_BUILD_32_BITS=On
$(eval $(call LLVMMxeTemplate,llvmwin32,i686))
$(eval $(call LLVMMxeTemplate,llvmwin64,x86_64))
endif
