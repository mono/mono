LLVM_SRC?=$(TOP)/sdks/builds/toolchains/llvm

$(TOP)/sdks/builds/toolchains/llvm:
	mkdir -p $(dir $@)
	$(MAKE) -C $(TOP)/llvm -f build.mk $@/CMakeLists.txt \
		LLVM_PATH="$@"

LLVM36_SRC?=$(TOP)/sdks/builds/toolchains/llvm36

$(TOP)/sdks/builds/toolchains/llvm36:
	mkdir -p $(dir $@)
	git clone -b $(LLVM36_BRANCH) https://github.com/mono/llvm.git $@
	cd $@ && git checkout $(LLVM36_HASH)

$(LLVM36_SRC)/configure: | $(LLVM36_SRC)

.stamp-llvm-download:
ifeq ($(UNAME),Darwin)
ifeq ($(DISABLE_DOWNLOAD_LLVM),)
	mkdir -p $(TOP)/sdks/builds/toolchains/llvm-download
	-$(MAKE) -C $(TOP)/llvm -f build.mk download-llvm \
		LLVM_PREFIX="$(TOP)/sdks/builds/toolchains/llvm-download/usr"
	touch $@
endif
endif

.stamp-llvm36-download:
ifeq ($(UNAME),Darwin)
ifeq ($(DISABLE_DOWNLOAD_LLVM),)
	mkdir -p $(TOP)/sdks/builds/toolchains/llvm36-download
	-wget --no-verbose -O - http://xamjenkinsartifact.blob.core.windows.net/$(LLVM36_JENKINS_LANE)/llvm-osx64-$(LLVM36_HASH).tar.gz | tar -xC $(TOP)/sdks/builds/toolchains/llvm36-download -f -
	touch $@
endif
endif

##
# Parameters
#  $(1): version
#  $(2): target
#  $(3): bitness
#  $(4): src
define LLVMProvisionTemplate
ifeq ($(UNAME),Darwin)
.stamp-$(1)-$(2)-unpack:
	cp -r $$(TOP)/sdks/builds/toolchains/$(1)-download/usr$(3)/* $$(TOP)/sdks/out/$(1)-$(2)
	touch $$@

.PHONY: unpack-$(1)-$(2)
unpack-$(1)-$(2): .stamp-$(1)-$(2)-unpack

.PHONY: provision-$(1)-$(2)
provision-$(1)-$(2): .stamp-$(1)-download | setup-$(1)-$(2) $(4)
	$$(MAKE) $$(if $$(wildcard $$(TOP)/sdks/builds/toolchains/$(1)-download/usr$(3)),unpack,package)-$(1)-$(2)

.PHONY: clean-$(1)-$(2)
clean-$(1)-$(2)::
	rm -rf .stamp-$(1)-download .stamp-$(1)-$(2)-unpack $$(TOP)/sdks/builds/toolchains/$(1)-download $$(TOP)/sdks/out/$(1)-$(2)
else
.PHONY: provision-$(1)-$(2)
provision-$(1)-$(2): package-$(1)-$(2)
endif

.PHONY: provision
provision: provision-$(1)-$(2)
endef

$(eval $(call LLVMProvisionTemplate,llvm,llvm32,32,$(LLVM_SRC)))
$(eval $(call LLVMProvisionTemplate,llvm,llvm64,64,$(LLVM_SRC)))
$(eval $(call LLVMProvisionTemplate,llvm36,llvm32,32,$(LLVM36_SRC)))

##
# Parameters
#  $(1): version
#  $(2): target
#  $(3): bitness
#  $(4): configure script
define LLVMMXEProvisionTemplate
.PHONY: provision-$(1)-$(2)
provision-$(1)-$(2): | package-$(1)-$(2) $(4)

.PHONY: provision
provision: provision-$(1)-$(2)
endef

$(eval $(call LLVMMXEProvisionTemplate,llvm,llvmwin32,32,$(LLVM_SRC)))
$(eval $(call LLVMMXEProvisionTemplate,llvm,llvmwin64,64,$(LLVM_SRC)))

##
# Parameters
#  $(1): target
#  $(2): arch
#  $(3): src dir
#  $(4): download stamp
#  $(5): llvm version (llvm/llvm36)
define LLVMTemplate

_llvm-$(1)_CMAKE_ARGS = \
	$$(llvm-$(1)_CMAKE_ARGS)

.PHONY: setup-llvm-$(1)
setup-llvm-$(1):
	mkdir -p $$(TOP)/sdks/out/llvm-$(1)

.PHONY: package-llvm-$(1)
package-llvm-$(1): setup-llvm-$(1) | $$(LLVM_SRC)
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
$(eval $(call LLVMTemplate,llvm32,i386))
$(eval $(call LLVMTemplate,llvm64,x86_64))

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

$(eval $(call LLVM36Template,llvm32,i386))

##
# Parameters
#  $(1): target
#  $(2): arch
#  $(3): mxe
define LLVMMxeTemplate

# -DCROSS_TOOLCHAIN_FLAGS_NATIVE is needed to compile the native tools (tlbgen) using the host compilers
# -DLLVM_ENABLE_THREADS=0 is needed because mxe doesn't define std::mutex etc.
# -DLLVM_BUILD_EXECUTION_ENGINE=Off is needed because it depends on threads
_llvm-$(1)_CMAKE_ARGS = \
	-DCROSS_TOOLCHAIN_FLAGS_NATIVE=-DCMAKE_TOOLCHAIN_FILE=$$(LLVM_SRC)/cmake/modules/NATIVE.cmake \
	-DCMAKE_TOOLCHAIN_FILE=$$(LLVM_SRC)/cmake/modules/$(3).cmake \
	-DLLVM_ENABLE_THREADS=Off \
	-DLLVM_BUILD_EXECUTION_ENGINE=Off \
	$$(llvm-$(1)_CMAKE_ARGS)

$$(LLVM_SRC)/cmake/modules/$(3).cmake: $(3).cmake.in | $$(LLVM_SRC)
	sed -e 's,@MXE_PATH@,$$(MXE_PREFIX),' -e 's,@MXE_SUFFIX@,$$(if $$(filter $(UNAME),Darwin),.static),' < $$< > $$@

.PHONY: setup-llvm-$(1)
setup-llvm-$(1):
	mkdir -p $$(TOP)/sdks/out/llvm-$(1)

.PHONY: package-llvm-$(1)
package-llvm-$(1): $$(LLVM_SRC)/cmake/modules/$(3).cmake setup-llvm-$(1) | $$(LLVM_SRC)
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

ifneq ($(MXE_PREFIX),)
llvm-llvmwin32_CMAKE_ARGS=-DLLVM_BUILD_32_BITS=On
$(eval $(call LLVMMxeTemplate,llvmwin32,i686,mxe-Win32))
$(eval $(call LLVMMxeTemplate,llvmwin64,x86_64,mxe-Win64))
endif
