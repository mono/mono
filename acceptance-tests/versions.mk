.PHONY: check-versions reset-versions check-mono

README=SUBMODULES.json

# usage $(call CheckVersionTemplate (name,MAKEFILE VAR,repo name))
# usage $(call CheckVersionTemplate (mono,MONO,mono))

define CheckVersionTemplate
#$(eval REPOSITORY_$(2):=$(shell test -z $(3) && echo $(1) || echo "$(3)"))
#$(eval DIRECTORY_$(2):=$(shell ruby versions.rb get-dir $(1)))
#$(eval DIRECTORY_$(2):=$(shell test -z $(DIRECTORY_$(2)) && echo $(1) || echo $(DIRECTORY_$(2))))
#$(eval MODULE_$(2):=$(shell ruby versions.rb get-url $(1)))
#$(eval NEEDED_$(2)_VERSION:=$(shell ruby versions.rb get-rev $(1)))
#$(eval $(2)_BRANCH_AND_REMOTE:=$(shell ruby versions.rb get-remote-branch $(1)))

#$(eval $(2)_VERSION:=$$$$(shell cd $($(2)_PATH) 2>/dev/null && git rev-parse HEAD ))

#$(eval NEEDED_$(2)_BRANCH:=$(word 2, $(subst /, ,$($(2)_BRANCH_AND_REMOTE))))
#$(eval NEEDED_$(2)_REMOTE:=$(word 1, $(subst /, ,$($(2)_BRANCH_AND_REMOTE))))
#$(eval $(2)_BRANCH:=$$$$(shell cd $($(2)_PATH) 2>/dev/null && git symbolic-ref --short HEAD 2>/dev/null))

check-$(1)::
	@if test x$$(IGNORE_$(2)_VERSION) = "x"; then \
	    if test ! -d $($(2)_PATH); then \
			if test x$$(RESET_VERSIONS) != "x"; then \
				make reset-$(1) || exit 1; \
			else \
				echo "Your $(1) checkout is missing, please run 'make reset-$(1)'"; \
				touch .check-versions-failure; \
			fi; \
	    else \
			if test "x$($(2)_VERSION)" != "x$(NEEDED_$(2)_VERSION)" ; then \
				if test x$$(RESET_VERSIONS) != "x"; then \
					make reset-$(1) || exit 1; \
				else \
				    echo "Your $(1) version is out of date, please run 'make reset-$(1)' (found $($(2)_VERSION), expected $(NEEDED_$(2)_VERSION))"; \
				    test -z "$(BUILD_REVISION)" || $(MAKE) test-$(1); \
			        touch .check-versions-failure; \
				fi; \
	        elif test "x$($(2)_BRANCH)" != "x$(NEEDED_$(2)_BRANCH)" ; then \
				if test x$$(RESET_VERSIONS) != "x"; then \
					test -z "$(BUILD_REVISION)" || $(MAKE) test-$(1); \
					make reset-$(1) || exit 1; \
				else \
				    echo "Your $(1) branch is out of date, please run 'make reset-$(1)' (found $($(2)_BRANCH), expected $(NEEDED_$(2)_BRANCH))"; \
			        touch .check-versions-failure; \
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
		echo "*** git clone $(MODULE_$(2)) --recursive $(DIRECTORY_$(2))" && (cd `dirname $($(2)_PATH)` && git clone $(MODULE_$(2)) --recursive $(DIRECTORY_$(2))); \
	fi
	@if test x$$(IGNORE_$(2)_VERSION) = "x"; then \
		echo "*** [$(1)] git checkout -f" $(NEEDED_$(2)_BRANCH) && (cd $($(2)_PATH) ; git checkout -f $(NEEDED_$(2)_BRANCH) || git checkout -f -b $($(2)_BRANCH_AND_REMOTE)); \
		echo "*** [$(1)] git reset --hard $(NEEDED_$(2)_VERSION)" && (cd $($(2)_PATH) && git reset --hard $(NEEDED_$(2)_VERSION)); \
	fi
	@echo "*** [$(1)] git submodule update --init --recursive" && (cd $($(2)_PATH) && git submodule update --init --recursive)

print-$(1)::
	@printf "*** %-16s %-45s %s (%s)\n" "$(DIRECTORY_$(2))" "$(MODULE_$(2))" "$(NEEDED_$(2)_VERSION)" "$(NEEDED_$(2)_BRANCH)"

.PHONY: check-$(1) reset-$(1) print-$(1)

reset-versions:: reset-$(1)
check-versions:: check-$(1)
print-versions:: print-$(1)

endef

ifneq ($(findstring mono-extensions, $(CHECK_VERSIONS)),)
    $(eval $(call CheckVersionTemplate,mono-extensions,MONO_EXTENSIONS))
endif

reset-versions::

check-versions::
	@if test -e .check-versions-failure; then  \
		rm .check-versions-failure; \
		echo One or more modules needs update;  \
		exit 1; \
	else \
		echo All dependent modules up to date;  \
	fi

reset:
	@make check-versions RESET_VERSIONS=1

XAMARIN_SUBMODULES=mono-extensions

check-versions-xamarin:
	$(MAKE) check-versions CHECK_VERSIONS=$(XAMARIN_SUBMODULES)

reset-versions-xamarin:
	$(MAKE) reset-versions CHECK_VERSIONS=$(XAMARIN_SUBMODULES)

reset-xamarin:
	$(MAKE) check-versions CHECK_VERSIONS=$(XAMARIN_SUBMODULES) RESET_VERSIONS=1

__bump-version-%:
	@if [ "$(REV)" == "" ]; then echo "Usage: make bump-version-$* REV=<ref>"; exit 1; fi
	@if [ "$(COMMIT)" == "1" ]; then git pull; fi
	ruby versions.rb set-rev $* $(REV)
	@if [ "$(COMMIT)" == "1" ]; then echo "Bump $* to pick up $(REV)." > msg; echo >> msg; cat msg | git commit -F - $(README); rm -f msg; fi

__bump-branch-%:
	@if [ "$(BRANCH)" == "" ]; then echo "Usage: make bump-branch-$* BRANCH=<branch> REMOTE_BRANCH=<remote branch>"; exit 1; fi
	@if [ "$(REMOTE_BRANCH)" == "" ]; then echo "Usage: make bump-branch-$* BRANCH=<branch> REMOTE_BRANCH=<remote branch>"; exit 1; fi
	@if [ "$(COMMIT)" == "1" ]; then git pull; fi
	ruby versions.rb set-branch $* $(BRANCH)
	ruby versions.rb set-remote-branch $* $(REMOTE_BRANCH)
	@if [ "$(COMMIT)" == "1" ]; then echo "Bump $* to switch to $(BRANCH) $(REMOTE BRANCH)." > msg; echo >> msg; cat msg | git commit -F - $(README); rm -f msg; fi

__bump-current-version-%:
	@if [ "$(COMMIT)" == "1" ]; then git pull; fi
	REV=$(shell cd $(TOP)/../$* && git log -1 --pretty=format:%H); \
	ruby versions.rb set-rev $* $$REV
	if [ "$(COMMIT)" == "1" ]; then echo "Bump $* to pick up $$REV:" > msg; echo >> msg; cat msg | git commit -F - $(README); rm -f msg; fi

# Bump the given submodule to the revision given by the REV make variable
# If COMMIT is 1, commit the change
bump-mono-extensions: __bump-version-mono-extensions
bump-llvm: __bump-version-llvm

# Bump the given submodule to the branch given by the BRANCH/REMOTE_BRANCH make variables
# If COMMIT is 1, commit the change
bump-branch-mono-extensions: __bump-branch-mono-extensions
bump-branch-llvm: __bump-branch-llvm

# Bump the given submodule to its current GIT version
# If COMMIT is 1, commit the change
bump-current-mono-extensions: __bump-current-version-mono-extensions
bump-current-llvm: __bump-current-version-llvm

commit-bump-mono-extensions:
	$(MAKE) bump-mono-extensions COMMIT=1
commit-bump-llvm:
	$(MAKE) bump-llvm COMMIT=1

commit-bump-current-mono-extensions:
	$(MAKE) bump-current-mono-extensions COMMIT=1
commit-bump-current-llvm:
	$(MAKE) bump-current-llvm COMMIT=1
