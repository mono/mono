topdir = ../..

LIBRARY = ../lib/corlib.dll

LIB_FLAGS = --unsafe --nostdlib

SOURCES_INCLUDE=*.cs
SOURCES_EXCLUDE=\
	./Test*	\
	./System.Security.Permissions/SecurityPermissionAttribute.cs	\
	./System.PAL/*.cs	\
	./Windows/*.cs

include ../library.make
