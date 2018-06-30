#
# Conditional submodule for llvm
#
# make reset-llvm will checkout a version of llvm which is suitable for this version of mono
# into $top_srcdir/llvm/llvm.
#

top_srcdir ?= $(abspath $(CURDIR)/..)

LLVM_PATH ?= $(abspath $(top_srcdir)/external/llvm)
LLVM_BUILD ?= $(abspath $(top_srcdir)/llvm/build)
LLVM_PREFIX ?= $(abspath $(top_srcdir)/llvm/usr)

CMAKE := $(or $(CMAKE),$(shell which cmake))
NINJA := $(shell which ninja)

SUBMODULES_CONFIG_FILE = $(top_srcdir)/llvm/SUBMODULES.json
include $(top_srcdir)/scripts/submodules/versions.mk

$(eval $(call ValidateVersionTemplate,llvm,LLVM))

# Bump the given submodule to the revision given by the REV make variable
# If COMMIT is 1, commit the change
bump-llvm: __bump-version-llvm

# Bump the given submodule to the branch given by the BRANCH/REMOTE_BRANCH make variables
# If COMMIT is 1, commit the change
bump-branch-llvm: __bump-branch-llvm

# Bump the given submodule to its current GIT version
# If COMMIT is 1, commit the change
bump-current-llvm: __bump-current-version-llvm

$(LLVM_BUILD) $(LLVM_PREFIX):
	mkdir -p $@

$(LLVM_PATH)/CMakeLists.txt: | reset-llvm

$(LLVM_BUILD)/$(if $(NINJA),build.ninja,Makefile): $(LLVM_PATH)/CMakeLists.txt | $(LLVM_BUILD)
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
		-DLLVM_ENABLE_ASSERTIONS=$(if $(INTERNAL_LLVM_ASSERTS),On,Off) \
		$(LLVM_CMAKE_ARGS) \
		$(dir $<)

.PHONY: configure-llvm
configure-llvm: $(LLVM_BUILD)/$(if $(NINJA),build.ninja,Makefile)

.PHONY: build-llvm
build-llvm: configure-llvm
	$(if $(NINJA),$(NINJA),$(MAKE)) -C $(LLVM_BUILD)

.PHONY: install-llvm
install-llvm: build-llvm | $(LLVM_PREFIX)
	$(if $(NINJA),$(NINJA),$(MAKE)) -C $(LLVM_BUILD) install

# FIXME: URL should be http://xamjenkinsartifact.blob.core.windows.net/build-package-osx-llvm-$(NEEDED_LLVM_BRANCH)/llvm-osx64-$(NEEDED_LLVM_VERSION).tar.gz
.PHONY: download-llvm
download-llvm:
	wget --no-verbose -O - http://xamjenkinsartifact.blob.core.windows.net/build-package-osx-llvm-release60/llvm-osx64-$(NEEDED_LLVM_VERSION).tar.gz | tar xzf -

.PHONY: clean-llvm
clean-llvm:
	$(RM) -r $(LLVM_BUILD) $(LLVM_PREFIX)
