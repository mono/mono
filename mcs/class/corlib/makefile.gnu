topdir = ../..

TEST_DIR= Test
LIBRARY = ../lib/corlib.dll

LIB_LIST = unix.args
LIB_FLAGS = --unsafe --nostdlib

SOURCES_INCLUDE=*.cs
SOURCES_EXCLUDE=\
	./Test*	\
	./System.Security.Permissions/SecurityPermissionAttribute.cs	\
	./System.PAL/*.cs	\
	./Windows/*.cs

include ../library.make
