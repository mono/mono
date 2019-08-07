# -*- makefile -*-
#
# Platform-specific makefile rules. This one's for any unix (FreeBSD, OpenBSD, etc.)
#

PLATFORM_MCS_FLAGS =
PLATFORM_RUNTIME = $(RUNTIME)
PLATFORM_TEST_HARNESS_EXCLUDES =

EXTERNAL_RUNTIME = mono

PLATFORM_PATH_SEPARATOR = :

# This is for changing / to \ on windows
PLATFORM_CHANGE_SEPARATOR_CMD = cat

hidden_prefix = .
hidden_suffix = 

platform-check:
	@:
