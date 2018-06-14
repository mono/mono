
EXTRA_DIST=SUBMODULES.json build.mk

if INTERNAL_LLVM

all-local: configure-llvm build-llvm install-llvm

clean-local: clean-llvm

endif

include build.mk
