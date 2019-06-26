thisdir = ilasm
SUBDIRS =
include ../build/rules.make

PROGRAM = ilasm.exe
BUILT_SOURCES = ILParser.cs

LIB_REFS = PEAPI System

API_BUILD := $(filter build, $(PROFILE))
ifdef API_BUILD
TARGET_NET_REFERENCE = $(BOOTSTRAP_BIN_PROFILE)
PROGRAM_USE_INTERMEDIATE_FILE = 1
else
LIB_REFS += Mono.Security
LOCAL_MCS_FLAGS += -d:HAS_MONO_SECURITY
endif

CLEAN_FILES = parser/y.output

EXTRA_DISTFILES = \
	parser/ILParser.jay	\
	$(wildcard tests/*.il)

ILParser.cs: parser/ILParser.jay $(topdir)/jay/skeleton.cs
	$(topdir)/jay/jay -ct -o $@ $(CURDIR)/$< < $(topdir)/jay/skeleton.cs

include ../build/executable.make

