# -*- makefile -*-
#
# Win32 platform-specific makefile rules.
#

PLATFORM_DEBUG_FLAGS = /debug+ /debug:full
PLATFORM_MCS_FLAGS = /nologo /optimize
PLATFORM_RUNTIME = 
PLATFORM_CORLIB = mscorlib.dll

EXTERNAL_MCS = csc.exe
EXTERNAL_MBAS = vbc.exe
EXTERNAL_RUNTIME =

# Disabled since it needs the SDK
#RESGEN = resgen.exe
#ILDISASM = ildasm.exe /test
RESGEN = $(topdir)/tools/resgen/resgen.exe

#ILDISASM = monodis.bat
## Gross hack
ILDISASM = $(topdir)/../mono/mono/dis/monodis

PLATFORM_MAKE_CORLIB_CMP = yes
PLATFORM_CHANGE_SEPARATOR_CMD=tr '/' '\\\\'
PLATFORM_PATH_SEPARATOR = ;

hidden_prefix = 
hidden_suffix = .tmp

platform-check:
	@:
