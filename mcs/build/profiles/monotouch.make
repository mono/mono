include $(topdir)/build/profiles/monotouch_runtime.make

PROFILE_MCS_FLAGS += \
	-d:FULL_AOT_RUNTIME

NO_SRE = yes

PROFILE_TEST_HARNESS_EXCLUDES += SRE NotWorkingLinqInterpreter
