LLVM_SRC?=$(TOP)/sdks/builds/toolchains/llvm

$(dir $(LLVM_SRC)):
	mkdir -p $@

.stamp-llvm-download: | setup-llvm-llvm32 setup-llvm-llvm64
	$(MAKE) -C $(TOP)/llvm -f build.mk download-llvm
	$(RM) -r $(TOP)/sdks/out/ios-llvm32 $(TOP)/sdks/out/ios-llvm64
	mv $(TOP)/llvm/usr32 $(TOP)/sdks/out/ios-llvm32
	mv $(TOP)/llvm/usr64 $(TOP)/sdks/out/ios-llvm64
	touch $@

$(dir $(LLVM_SRC)):
	mkdir -p $@

##
# Parameters
#  $(1): target
#  $(2): arch
define LLVMTemplate

_llvm-$(1)_CMAKE_ARGS = \
	$$(llvm-$(1)_CMAKE_ARGS)

.stamp-llvm-$(1)-toolchain:
	touch $$@

.stamp-llvm-$(1)-configure: | $$(dir $(LLVM_SRC))
	touch $$@

.PHONY: build-custom-llvm-$(1)
build-custom-llvm-$(1):

.PHONY: package-llvm-$(1)
package-llvm-$(1): | $$(dir $(LLVM_SRC))
	$$(MAKE) -C $$(TOP)/llvm -f build.mk install-llvm \
		LLVM_PATH="$(LLVM_SRC)" \
		LLVM_BUILD="$$(TOP)/sdks/builds/llvm-$(1)" \
		LLVM_PREFIX="$$(TOP)/sdks/out/ios-$(1)" \
		LLVM_CMAKE_ARGS="$$(_llvm-$(1)_CMAKE_ARGS)"

.PHONY: download-llvm-$(1)
download-llvm-$(1): .stamp-llvm-download

.PHONY: clean-llvm-$(1)
clean-llvm-$(1):
	$$(MAKE) -C $$(TOP)/llvm -f build.mk clean-llvm \
		LLVM_PATH="$(LLVM_SRC)" \
		LLVM_BUILD="$$(TOP)/sdks/builds/llvm-$(1)" \
		LLVM_PREFIX="$$(TOP)/sdks/out/ios-$(1)"

TARGETS += llvm-$(1)

endef

llvm-llvm32_CMAKE_ARGS=-DLLVM_BUILD_32_BITS=On
$(eval $(call LLVMTemplate,llvm32,i386))
$(eval $(call LLVMTemplate,llvm64,x86_64))

##
# Parameters
#  $(1): target
#  $(2): arch
define LLVMMxeTemplate

_llvm-$(1)_CMAKE=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32$$(if $$(filter $(UNAME),Darwin),.static)-cmake

# -DCROSS_TOOLCHAIN_FLAGS_NATIVE is needed to compile the native tools (tlbgen) using the host compilers
# -DLLVM_ENABLE_THREADS=0 is needed because mxe doesn't define std::mutex etc.
# -DLLVM_BUILD_EXECUTION_ENGINE=Off is needed because it depends on threads
_llvm-$(1)_CMAKE_ARGS = \
	-DCROSS_TOOLCHAIN_FLAGS_NATIVE=-DCMAKE_TOOLCHAIN_FILE=$(LLVM_SRC)/cmake/modules/NATIVE.cmake \
	-DLLVM_ENABLE_THREADS=Off \
	-DLLVM_BUILD_EXECUTION_ENGINE=Off \
	$$(llvm-$(1)_CMAKE_ARGS)

.stamp-llvm-$(1)-toolchain:
	touch $$@

.stamp-llvm-$(1)-configure:
	touch $$@

.PHONY: build-custom-llvm-$(1)
build-custom-llvm-$(1):

.PHONY: package-llvm-$(1)
package-llvm-$(1): | $$(dir $(LLVM_SRC))
	$$(MAKE) -C $$(TOP)/llvm -f build.mk install-llvm \
		CMAKE=$$(_llvm-$(1)_CMAKE) \
		LLVM_PATH="$(LLVM_SRC)" \
		LLVM_BUILD="$$(TOP)/sdks/builds/llvm-$(1)" \
		LLVM_PREFIX="$$(TOP)/sdks/out/ios-$(1)" \
		LLVM_CMAKE_ARGS="$$(_llvm-$(1)_CMAKE_ARGS)"

.PHONY: clean-llvm-$(1)
clean-llvm-$(1):
	$$(MAKE) -C $$(TOP)/llvm -f build.mk clean-llvm \
		CMAKE=$$(_llvm-$(1)_CMAKE) \
		LLVM_PATH="$(LLVM_SRC)" \
		LLVM_BUILD="$$(TOP)/sdks/builds/llvm-$(1)" \
		LLVM_PREFIX="$$(TOP)/sdks/out/ios-$(1)"

TARGETS += llvm-$(1)

endef

ifneq ($(MXE_PREFIX),)
llvm-llvmwin32_CMAKE_ARGS=-DLLVM_BUILD_32_BITS=On
$(eval $(call LLVMMxeTemplate,llvmwin32,i686))
$(eval $(call LLVMMxeTemplate,llvmwin64,x86_64))
endif
