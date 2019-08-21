# -*- makefile -*-
#
# Win32 platform-specific makefile rules.
#

PLATFORM_MCS_FLAGS =
PLATFORM_RUNTIME = 
PLATFORM_TEST_HARNESS_EXCLUDES = NotOnWindows

EXTERNAL_RUNTIME = mono

PLATFORM_CHANGE_SEPARATOR_CMD=tr '/' '\\\\'
PLATFORM_PATH_SEPARATOR = ;

override CURDIR:=$(shell cygpath -m $(CURDIR))

hidden_prefix = 
hidden_suffix = .tmp

platform-check:
	@:

