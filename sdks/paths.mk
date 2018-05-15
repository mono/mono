
ifndef DISABLE_IOS

CheckXcodeDir=$(or $(and $(wildcard $(1))),$(warning Could not find Xcode in "$(1)"))

ifeq ($(call CheckXcodeDir,$(XCODE_DIR)),)
$(error Could not find Xcode at $(XCODE_DIR))
endif

endif # DISABLE_IOS
