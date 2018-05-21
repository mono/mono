#
# Conditional submodule for llvm
#
# make reset-llvm will checkout a version of llvm which is suitable for this version of mono
# into $top_srcdir/llvm/llvm.
#

if INTERNAL_LLVM

LLVM_PATH ?= $(abs_top_srcdir)/external/llvm
LLVM_BUILD ?= $(abs_top_srcdir)/llvm/build
LLVM_PREFIX ?= $(abs_top_srcdir)/llvm/usr

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

clean-local:
	$(RM) -r $(LLVM_PATH) $(LLVM_BUILD) $(LLVM_PREFIX)

EXTRA_DIST=SUBMODULES.json

NINJA:=$(shell which ninja)

$(LLVM_BUILD) $(LLVM_PREFIX):
	mkdir -p $@

.PHONY: all-local
all-local: configure-llvm build-llvm install-llvm

$(LLVM_PATH)/CMakeLists.txt: | reset-llvm

.PHONY: configure-llvm
configure-llvm: $(LLVM_PATH)/CMakeLists.txt | $(LLVM_BUILD)
	cd $(LLVM_BUILD) && cmake \
		$(if $(NINJA),-G Ninja) \
		-DCMAKE_INSTALL_PREFIX="$(LLVM_PREFIX)" \
		-DCMAKE_BUILD_TYPE=Release \
		-DLLVM_TARGETS_TO_BUILD="X86;ARM;AArch64" \
		$(LLVM_CMAKE_ARGS) \
		$(dir $<)

.PHONY: build-llvm
build-llvm: configure-llvm
	$(if $(NINJA),$(NINJA),$(MAKE)) -C $(LLVM_BUILD)

.PHONY: install-llvm
install-llvm: build-llvm | $(LLVM_PREFIX)
	$(if $(NINJA),$(NINJA),$(MAKE)) -C $(LLVM_BUILD) install

endif # INTERNAL_LLVM
