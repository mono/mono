# -*- makefile -*-
#
# Win32 platform-specific makefile rules.
#

PLATFORM_DEBUG_FLAGS = /debug+ /debug:full
PLATFORM_MCS_FLAGS = /nologo /optimize
PLATFORM_RUNTIME = 
PLATFORM_CORLIB = mscorlib.dll

BOOTSTRAP_MCS = csc.exe
MCS = $(BOOTSTRAP_MCS)

PLATFORM_MAKE_CORLIB_CMP = yes
PLATFORM_TWEAK_CORLIB_SOURCES=cat - corlib.dll.win32-excludes |sort |uniq -u
PLATFORM_CHANGE_SEPARATOR_CMD=tr '/' '\\\\'

hidden_prefix = 
hidden_suffix = .tmp

platform-check:
