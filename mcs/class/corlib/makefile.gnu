topdir = ../..

TEST_DIR= Test
LIBRARY = $(topdir)/class/lib/corlib.dll

LIB_LIST = unix.args
LIB_FLAGS = --unsafe --nostdlib /debug+ /debug:full

SOURCES_INCLUDE=*.cs
SOURCES_EXCLUDE=\
	./Test*	\
	./System.Security.Permissions/SecurityPermissionAttribute.cs	\
	./System.PAL/*.cs	\
	./Windows/*.cs	\
	./System.Runtime.Remoting.Activation/UrlAttribute.cs	\
	./System.Runtime.Remoting.Contexts/ContextAttribute.cs

include $(topdir)/class/library.make
