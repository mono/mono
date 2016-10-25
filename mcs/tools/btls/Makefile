thisdir = tools/btls
SUBDIRS = 
include ../../build/rules.make

LOCAL_MCS_FLAGS =
LIB_REFS = System Mono.Security Mono.Btls.Interface
PROGRAM = btls-cert-sync.exe

ifndef HAVE_BTLS
PROGRAM_NAME = dummy-btls-cert-sync.exe
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
NO_BUILD = yes
endif

include ../../build/executable.make
