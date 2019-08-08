
AM_CPPFLAGS = $(SHARED_CFLAGS)

if DISABLE_EXECUTABLES
bin_PROGRAMS =
else
bin_PROGRAMS = mono-hang-watchdog
endif

CFLAGS = $(filter-out @CXX_REMOVE_CFLAGS@, @CFLAGS@)

mono_hang_watchdog_SOURCES = mono-hang-watchdog.c
