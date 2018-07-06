
EXTRA_DIST=SUBMODULES.json build.mk

if ENABLE_LLVM
if INTERNAL_LLVM

all-local: configure-llvm build-llvm install-llvm llvm_config.mk

clean-local: clean-llvm clean-llvm-config

$(mono_build_root)/llvm/llvm_config.mk: install-llvm
	$(top_srcdir)/llvm/build_llvm_config.sh $(top_srcdir)/llvm/usr/bin/llvm-config $(LLVM_CODEGEN_LIBS) > $@

else
all-local: llvm_config.mk

clean-local: clean-llvm-config

$(mono_build_root)/llvm/llvm_config.mk: $(top_srcdir)/llvm/Makefile.am
	$(top_srcdir)/llvm/build_llvm_config.sh $(EXTERNAL_LLVM_CONFIG) $(LLVM_CODEGEN_LIBS) > $@
endif

llvm_config.mk: $(mono_build_root)/llvm/llvm_config.mk

clean-llvm-config:
	- rm -rf llvm_config.mk

else
all-local: 

clean-local: 
endif


include build.mk
