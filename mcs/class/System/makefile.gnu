topdir = ../..

TEST_DIR= Test
LIBRARY = $(topdir)/class/lib/System.dll

LIB_LIST = list.unix
LIB_FLAGS = -r corlib -r System.Xml -r mscorlib

SOURCES_INCLUDE=*.cs
SOURCES_EXCLUDE=\
	./Test*	\
	./System.CodeDom/Code-X-Collection.cs	\
	./System.Net/IAuthenticationModule.cs	\
	./System.Net/AuthenticationManager.cs	\
	./System.Diagnostics/Performance*.cs	\
	./System.Diagnostics/Counter*.cs	\
	./System.Diagnostics/InstanceData*.cs


export MONO_PATH_PREFIX = $(topdir)/class/lib:

include $(topdir)/class/library.make
