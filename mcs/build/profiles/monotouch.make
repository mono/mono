include $(topdir)/build/profiles/monotouch_runtime.make

PROFILE_MCS_FLAGS += \
	-d:FULL_AOT_RUNTIME

NO_SRE=1