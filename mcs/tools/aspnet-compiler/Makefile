thisdir = tools/aspnet-compiler
SUBDIRS = 
include ../../build/rules.make

RESOURCE_DEFS = \
	System.Web.Compilation.CompilerResources,Resources/System.Web.Compilation.CompilerResources.resx

LOCAL_MCS_FLAGS =
LIB_REFS = System System.Web
PROGRAM = aspnet-compiler.exe

CLEAN_FILES = aspnet-compiler.exe aspnet-compiler.exe.mdb

include ../../build/executable.make
