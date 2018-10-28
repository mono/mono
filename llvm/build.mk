abs_top_srcdir ?= $(abspath $(CURDIR)/..)

LLVM_BUILD ?= $(abspath $(abs_top_srcdir)/llvm/build)
LLVM_PREFIX ?= $(abspath $(abs_top_srcdir)/llvm/usr)

# LLVM_BRANCH  := $(shell git -C "$(abs_top_srcdir)/external/llvm" rev-parse --abbrev-ref HEAD)
LLVM_VERSION := $(shell git -C "$(abs_top_srcdir)/external/llvm" rev-parse HEAD)

# FIXME: URL should be http://xamjenkinsartifact.blob.core.windows.net/build-package-osx-llvm-$(LLVM_BRANCH)/llvm-osx64-$(LLVM_VERSION).tar.gz
LLVM_DOWNLOAD_LOCATION = "http://xamjenkinsartifact.blob.core.windows.net/build-package-osx-llvm-release60/llvm-osx64-$(LLVM_VERSION).tar.gz"

CMAKE := $(or $(CMAKE),$(shell which cmake))
NINJA := $(shell which ninja)

$(LLVM_BUILD) $(LLVM_PREFIX):
	mkdir -p $@

EXTRA_LLVM_ARGS = $(if $(filter $(LLVM_TARGET),wasm32), -DLLVM_BUILD_32_BITS=On -DLLVM_EXPERIMENTAL_TARGETS_TO_BUILD="WebAssembly",)

# -DLLVM_ENABLE_LIBXML2=Off is needed because xml2 is not used and it breaks 32-bit builds on 64-bit Linux hosts
$(LLVM_BUILD)/$(if $(NINJA),build.ninja,Makefile): $(abs_top_srcdir)/external/llvm/CMakeLists.txt | $(LLVM_BUILD) $(LLVM_PREFIX)
	cd $(LLVM_BUILD) && $(CMAKE) \
		$(if $(NINJA),-G Ninja) \
		-DCMAKE_INSTALL_PREFIX="$(LLVM_PREFIX)" \
		-DCMAKE_BUILD_TYPE=Release \
		-DLLVM_BUILD_TESTS=Off \
		-DLLVM_INCLUDE_TESTS=Off \
		-DLLVM_BUILD_EXAMPLES=Off \
		-DLLVM_INCLUDE_EXAMPLES=Off \
		-DLLVM_TOOLS_TO_BUILD="opt;llc;llvm-config;llvm-dis" \
		-DLLVM_TARGETS_TO_BUILD="X86;ARM;AArch64" \
		$(EXTRA_LLVM_ARGS)	\
		-DLLVM_ENABLE_ASSERTIONS=$(if $(INTERNAL_LLVM_ASSERTS),On,Off) \
		-DLLVM_ENABLE_LIBXML2=Off \
		-DHAVE_FUTIMENS=0 \
		$(LLVM_CMAKE_ARGS) \
		$(abs_top_srcdir)/external/llvm

.PHONY: configure-llvm
configure-llvm: $(LLVM_BUILD)/$(if $(NINJA),build.ninja,Makefile)

# The DESTDIR fix is to prevent the build from trying to install this out-of-build-tree
# as the DESTDIR hasn't been created when we're building mono

.PHONY: build-llvm
build-llvm: configure-llvm
	DESTDIR="" $(if $(NINJA),$(NINJA),$(MAKE)) -C $(LLVM_BUILD)

.PHONY: install-llvm
install-llvm: build-llvm | $(LLVM_PREFIX)
	DESTDIR="" $(if $(NINJA),$(NINJA),$(MAKE)) -C $(LLVM_BUILD) install

.PHONY: download-llvm
download-llvm:
	(wget --no-verbose -O - $(LLVM_DOWNLOAD_LOCATION) || curl -L $(LLVM_DOWNLOAD_LOCATION)) | tar -xzf - -C $(dir $(LLVM_PREFIX))

.PHONY: clean-llvm
clean-llvm:
	$(RM) -r $(LLVM_BUILD) $(LLVM_PREFIX)
