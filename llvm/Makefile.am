

EXTRA_DIST=SUBMODULES.json

if INTERNAL_LLVM
.PHONY: all-local
all-local: configure-llvm build-llvm install-llvm

.PHONY: clean-local
clean-local: clean-llvm
endif

if INTERNAL_LLVM_ASSERTS
llvm_build_args := "INTERNAL_LLVM_ASSERTS=yes"
endif

# We do not propagate top level configuration variables
# to every makefile in the repo. Since build.mk is not going to become
# an automake file, I'm passing the variables explicitly.

reset-llvm configure-llvm build-llvm install-llvm clean-llvm: build.mk
	$(MAKE) -f build.mk $@ $(llvm_build_args)

