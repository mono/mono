include $(topdir)/build/profiles/monotouch.make

PROFILE_MCS_FLAGS += \
	-d:FEATURE_NO_BSD_SOCKETS \
	-d:MONOTOUCH_WATCH

NO_THREAD_ABORT=1
NO_THREAD_SUSPEND_RESUME=1
NO_MULTIPLE_APPDOMAINS=1
NO_PROCESS_START=1
NO_MONO_SECURITY=1
MONO_FEATURE_APPLETLS=
ONLY_APPLETLS=
MONO_FEATURE_APPLE_X509=1
