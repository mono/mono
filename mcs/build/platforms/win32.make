# -*- makefile -*-
#
# Win32 platform-specific makefile rules.
#

PLATFORM_DEBUG_FLAGS = /debug+ /debug:full
PLATFORM_MCS_FLAGS = /nologo /optimize
PLATFORM_RUNTIME = 
PLATFORM_CORLIB = mscorlib.dll
PLATFORM_TEST_HARNESS_EXCLUDES =

EXTERNAL_MCS = gmcs
EXTERNAL_MBAS = vbc.exe
EXTERNAL_RUNTIME =

# Disabled since it needs the SDK
#ILDISASM = ildasm.exe /test

#ILDISASM = monodis.bat
## Gross hack
ILDISASM = $(topdir)/../mono/mono/dis/monodis

PLATFORM_MAKE_CORLIB_CMP = yes
PLATFORM_CHANGE_SEPARATOR_CMD=tr '/' '\\\\'
PLATFORM_PATH_SEPARATOR = ;

## not so simple :-)
#PLATFORM_AOT_SUFFIX = .dll

hidden_prefix = 
hidden_suffix = .tmp

platform-check:
	@:
