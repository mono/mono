if !ENABLE_MSVC_ONLY

AM_CPPFLAGS = $(SHARED_CFLAGS)

if DISABLE_EXECUTABLES
bin_PROGRAMS =
else
bin_PROGRAMS = mono-hang-watchdog
endif

endif # !ENABLE_MSVC_ONLY

CFLAGS = $(filter-out @CXX_REMOVE_CFLAGS@, @CFLAGS@)

if !ENABLE_MSVC_ONLY

mono_hang_watchdog_SOURCES = mono-hang-watchdog.c

endif # !ENABLE_MSVC_ONLY
