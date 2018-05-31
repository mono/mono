

EXTRA_DIST=SUBMODULES.json

if INTERNAL_LLVM
.PHONY: all-local
all-local: configure-llvm build-llvm install-llvm

.PHONY: clean-local
clean-local: clean-llvm
endif

reset-llvm configure-llvm build-llvm install-llvm clean-llvm: build.mk
	$(MAKE) -f build.mk $@
