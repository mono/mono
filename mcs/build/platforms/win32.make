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
RESGEN = resgen.exe

PLATFORM_MAKE_CORLIB_CMP = yes
PLATFORM_CHANGE_SEPARATOR_CMD=tr '/' '\\\\'
PLATFORM_PATH_SEPARATOR = ;

hidden_prefix = 
hidden_suffix = .tmp

platform-check:
