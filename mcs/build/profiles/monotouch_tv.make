include $(topdir)/build/profiles/monotouch.make

PROFILE_MCS_FLAGS += \
	-d:MONOTOUCH_TV

NO_THREAD_ABORT=1
NO_THREAD_SUSPEND_RESUME=1
NO_PROCESS_START=1
