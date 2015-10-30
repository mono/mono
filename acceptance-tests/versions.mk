.PHONY: validate-versions reset-versions

CONFIG=SUBMODULES.json

# usage $(call ValidateVersionTemplate (name,MAKEFILE VAR,repo name))
# usage $(call ValidateVersionTemplate (mono,MONO,mono))

define ValidateVersionTemplate
#$(eval REPOSITORY_$(2):=$(shell test -z $(3) && echo $(1) || echo "$(3)"))
#$(eval DIRECTORY_$(2):=$(shell python versions.py get-dir $(1)))
#$(eval DIRECTORY_$(2):=$(shell test -z $(DIRECTORY_$(2)) && echo $(1) || echo $(DIRECTORY_$(2))))
#$(eval MODULE_$(2):=$(shell python versions.py get-url $(1)))
#$(eval NEEDED_$(2)_VERSION:=$(shell python versions.py get-rev $(1)))
#$(eval $(2)_BRANCH_AND_REMOTE:=$(shell python versions.py get-remote-branch $(1)))

#$(eval $(2)_VERSION:=$$$$(shell cd $($(2)_PATH) 2>/dev/null && git rev-parse HEAD ))

#$(eval NEEDED_$(2)_BRANCH:=$(word 2, $(subst /, ,$($(2)_BRANCH_AND_REMOTE))))
#$(eval NEEDED_$(2)_REMOTE:=$(word 1, $(subst /, ,$($(2)_BRANCH_AND_REMOTE))))
#$(eval $(2)_BRANCH:=$$$$(shell cd $($(2)_PATH) 2>/dev/null && git symbolic-ref --short HEAD 2>/dev/null))

validate-$(1)::
	@if test x$$(IGNORE_$(2)_VERSION) = "x"; then \
	    if test ! -d $($(2)_PATH); then \
			if test x$$(RESET_VERSIONS) != "x"; then \
				$(MAKE) reset-$(1) || exit 1; \
			else \
				echo "Your $(1) checkout is missing, please run 'make reset-$(1)'"; \
				touch .validate-versions-failure; \
			fi; \
	    else \
			if test "x$($(2)_VERSION)" != "x$(NEEDED_$(2)_VERSION)" ; then \
				if test x$$(RESET_VERSIONS) != "x"; then \
					$(MAKE) reset-$(1) || exit 1; \
				else \
				    echo "Your $(1) version is out of date, please run 'make reset-$(1)' (found $($(2)_VERSION), expected $(NEEDED_$(2)_VERSION))"; \
				    test -z "$(BUILD_REVISION)" || $(MAKE) test-$(1); \
			        touch .validate-versions-failure; \
				fi; \
	        elif test "x$($(2)_BRANCH)" != "x$(NEEDED_$(2)_BRANCH)" ; then \
				if test x$$(RESET_VERSIONS) != "x"; then \
					test -z "$(BUILD_REVISION)" || $(MAKE) test-$(1); \
					$(MAKE) reset-$(1) || exit 1; \
				else \
				    echo "Your $(1) branch is out of date, please run 'make reset-$(1)' (found $($(2)_BRANCH), expected $(NEEDED_$(2)_BRANCH))"; \
			        touch .validate-versions-failure; \
				fi; \
	       fi; \
	    fi; \
	fi

test-$(1)::
	@echo $(1)
	@echo "   REPOSITORY_$(2)=$(REPOSITORY_$(2))"
	@echo "   DIRECTORY_$(2)=$(DIRECTORY_$(2))"
	@echo "   MODULE_$(2)=$(MODULE_$(2))"
	@echo "   NEEDED_$(2)_VERSION=$(NEEDED_$(2)_VERSION)"
	@echo "   $(2)_VERSION=$($(2)_VERSION)"
	@echo "   $(2)_BRANCH_AND_REMOTE=$($(2)_BRANCH_AND_REMOTE)"
	@echo "   NEEDED_$(2)_BRANCH=$(NEEDED_$(2)_BRANCH)"
	@echo "   NEEDED_$(2)_REMOTE=$(NEEDED_$(2)_REMOTE)"
	@echo "   $(2)_BRANCH=$($(2)_BRANCH)"
	@echo "   $(2)_PATH=$($(2)_PATH) => $(abspath $($(2)_PATH))"

reset-$(1)::
	@if test -d $($(2)_PATH); then \
		if ! (cd $($(2)_PATH) && git show $(NEEDED_$(2)_VERSION) >/dev/null 2>&1 && git log -1 $(NEEDED_$(2)_REMOTE) >/dev/null 2>&1) ; then \
			echo "*** git fetch `basename $$($(2)_PATH)`" && (cd $($(2)_PATH) && git fetch); \
		fi;  \
	else \
		echo "*** git clone $(MODULE_$(2)) --recursive $(DIRECTORY_$(2))" && (cd `dirname $($(2)_PATH)` && git clone $(MODULE_$(2)) --recursive $(DIRECTORY_$(2)) || exit 1 ); \
	fi
	@if test x$$(IGNORE_$(2)_VERSION) = "x"; then \
		echo "*** [$(1)] git checkout -f" $(NEEDED_$(2)_BRANCH) && (cd $($(2)_PATH) ; git checkout -f $(NEEDED_$(2)_BRANCH) || git checkout -f -b $($(2)_BRANCH_AND_REMOTE)); \
		echo "*** [$(1)] git reset --hard $(NEEDED_$(2)_VERSION)" && (cd $($(2)_PATH) && git reset --hard $(NEEDED_$(2)_VERSION)); \
	fi
	@echo "*** [$(1)] git submodule update --init --recursive" && (cd $($(2)_PATH) && git submodule update --init --recursive)

print-$(1)::
	@printf "*** %-16s %-45s %s (%s)\n" "$(DIRECTORY_$(2))" "$(MODULE_$(2))" "$(NEEDED_$(2)_VERSION)" "$(NEEDED_$(2)_BRANCH)"

.PHONY: validate-$(1) reset-$(1) print-$(1)

reset-versions:: reset-$(1)
validate-versions:: Validate-$(1)
print-versions:: print-$(1)

endef

$(eval $(call ValidateVersionTemplate,roslyn,ROSLYN))
$(eval $(call ValidateVersionTemplate,coreclr,CORECLR))
$(eval $(call ValidateVersionTemplate,ms-test-suite,MSTESTSUITE))

reset-versions::

validate-versions::
	@if test -e .validate-versions-failure; then  \
		rm .validate-versions-failure; \
		echo One or more modules needs update;  \
		exit 1; \
	else \
		echo All dependent modules up to date;  \
	fi

reset:
	@$(MAKE) validate-versions RESET_VERSIONS=1

__bump-version-%:
	@if [ "$(REV)" = "" ]; then echo "Usage: make bump-version-$* REV=<ref>"; exit 1; fi
	python versions.py set-rev $* $(REV)
	@if [ "$(COMMIT)" = "1" ]; then echo "[acceptance-tests] Bump $* to pick up $(REV)." | git commit -F - $(CONFIG); fi

__bump-branch-%:
	@if [ "$(BRANCH)" = "" ]; then echo "Usage: make bump-branch-$* BRANCH=<branch> REMOTE_BRANCH=<remote branch>"; exit 1; fi
	@if [ "$(REMOTE_BRANCH)" == "" ]; then echo "Usage: make bump-branch-$* BRANCH=<branch> REMOTE_BRANCH=<remote branch>"; exit 1; fi
	python versions.py set-branch $* $(BRANCH)
	python versions.py set-remote-branch $* $(REMOTE_BRANCH)
	@if [ "$(COMMIT)" = "1" ]; then echo "[acceptance-tests] Bump $* to switch to $(BRANCH) $(REMOTE BRANCH)." | git commit -F - $(CONFIG); fi

__bump-current-version-%:
	REV=$(shell cd $(TOP)/../$* && git log -1 --pretty=format:%H); \
	python versions.py set-rev $* $$REV; \
	if [ "$(COMMIT)" = "1" ]; then echo "[acceptance-tests] Bump $* to pick up $$REV:" | git commit -F - $(CONFIG); fi

# Bump the given submodule to the revision given by the REV make variable
# If COMMIT is 1, commit the change
bump-roslyn: __bump-version-roslyn
bump-coreclr: __bump-version-coreclr
bump-ms-test-suite: __bump-version-ms-test-suite

# Bump the given submodule to the branch given by the BRANCH/REMOTE_BRANCH make variables
# If COMMIT is 1, commit the change
bump-branch-roslyn: __bump-branch-roslyn
bump-branch-coreclr: __bump-branch-coreclr
bump-branch-ms-test-suite: __bump-branch-ms-test-suite

# Bump the given submodule to its current GIT version
# If COMMIT is 1, commit the change
bump-current-roslyn: __bump-current-version-roslyn
bump-current-coreclr: __bump-current-version-coreclr
bump-current-ms-test-suite: __bump-current-version-ms-test-suite

commit-bump-roslyn:
	$(MAKE) bump-roslyn COMMIT=1

commit-bump-coreclr:
	$(MAKE) bump-coreclr COMMIT=1

commit-bump-ms-test-suite:
	$(MAKE) bump-ms-test-suite COMMIT=1

commit-bump-current-roslyn:
	$(MAKE) bump-current-roslyn COMMIT=1

commit-bump-current-coreclr:
	$(MAKE) bump-current-coreclr COMMIT=1

commit-bump-current-ms-test-suite:
	$(MAKE) bump-current-ms-test-suite COMMIT=1
