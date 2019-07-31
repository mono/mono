include $(topdir)/build/profiles/monotouch_runtime.make

PROFILE_MCS_FLAGS += \
	-d:MONOTOUCH_TV

NO_THREAD_ABORT=1
NO_THREAD_SUSPEND_RESUME=1
# The binding generator (btv) still needs to execute processes,
# so we need a System.dll that can do that.
#NO_PROCESS_START=1

NO_GSS=1
