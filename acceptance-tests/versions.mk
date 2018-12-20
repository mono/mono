.PHONY: validate-versions reset-versions

SUBMODULES_CONFIG_FILE = $(top_srcdir)/acceptance-tests/SUBMODULES.json
include $(top_srcdir)/scripts/submodules/versions.mk

$(eval $(call ValidateVersionTemplate,benchmarker,BENCHMARKER))
$(eval $(call ValidateVersionTemplate,roslyn,ROSLYN))
$(eval $(call ValidateVersionTemplate,coreclr,CORECLR))
$(eval $(call ValidateVersionTemplate,ms-test-suite,MSTESTSUITE))
$(eval $(call ValidateVersionTemplate,DebianShootoutMono,DEBIANSHOOTOUTMONO))

# Bump the given submodule to the revision given by the REV make variable
# If COMMIT is 1, commit the change
bump-benchmarker: __bump-benchmarker
bump-roslyn: __bump-version-roslyn
bump-coreclr: __bump-version-coreclr
bump-ms-test-suite: __bump-version-ms-test-suite
bump-DebianShootoutMono: __bump-version-DebianShootoutMono

# Bump the given submodule to the branch given by the BRANCH/REMOTE_BRANCH make variables
# If COMMIT is 1, commit the change
bump-branch-benchmarker: __bump-branch-benchmarker
bump-branch-roslyn: __bump-branch-roslyn
bump-branch-coreclr: __bump-branch-coreclr
bump-branch-ms-test-suite: __bump-branch-ms-test-suite
bump-branch-DebianShootoutMono: __bump-branch-DebianShootoutMono

# Bump the given submodule to its current GIT version
# If COMMIT is 1, commit the change
bump-current-benchmarker: __bump-current-benchmarker
bump-current-roslyn: __bump-current-version-roslyn
bump-current-coreclr: __bump-current-version-coreclr
bump-current-ms-test-suite: __bump-current-version-ms-test-suite
bump-current-DebianShootoutMono: __bump-current-version-DebianShootoutMono

commit-bump-benchmarker:
	$(MAKE) bump-benchmarker COMMIT=1

commit-bump-roslyn:
	$(MAKE) bump-roslyn COMMIT=1

commit-bump-coreclr:
	$(MAKE) bump-coreclr COMMIT=1

commit-bump-ms-test-suite:
	$(MAKE) bump-ms-test-suite COMMIT=1

commit-bump-DebianShootoutMono:
	$(MAKE) bump-DebianShootoutMono COMMIT=1

commit-bump-current-benchmarker:
	$(MAKE) bump-current-benchmarker COMMIT=1

commit-bump-current-roslyn:
	$(MAKE) bump-current-roslyn COMMIT=1

commit-bump-current-coreclr:
	$(MAKE) bump-current-coreclr COMMIT=1

commit-bump-current-ms-test-suite:
	$(MAKE) bump-current-ms-test-suite COMMIT=1

commit-bump-current-DebianShootoutMono:
	$(MAKE) bump-current-DebianShootoutMono COMMIT=1
