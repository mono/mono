#
# Conditional submodule for llvm
#
# make reset-llvm will checkout a version of llvm which is suitable for this version of mono
# into $top_srcdir/llvm/llvm.
#

LLVM_PATH=llvm

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
	$(RM) -r $(LLVM_PATH)

EXTRA_DIST=SUBMODULES.json
