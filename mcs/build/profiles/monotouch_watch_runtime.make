include $(topdir)/build/profiles/monotouch_runtime.make

PROFILE_MCS_FLAGS += \
	-d:MONOTOUCH_WATCH

NO_THREAD_ABORT=1
NO_THREAD_SUSPEND_RESUME=1
