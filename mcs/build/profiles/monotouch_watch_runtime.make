include $(topdir)/build/profiles/monotouch_runtime.make

PROFILE_MCS_FLAGS += \
	-d:FEATURE_NO_BSD_SOCKETS \
	-d:MONOTOUCH_WATCH

NO_THREAD_ABORT=1
NO_THREAD_SUSPEND_RESUME=1
# The binding generator (bwatch) still needs to execute processes,
# so we need a System.dll that can do that.
#NO_PROCESS_START=1
