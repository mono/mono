topdir = ../../..

LIBRARY = NUnitCore_mono.dll

LIB_LIST = list.unix
LIB_FLAGS = -r corlib -r System

SOURCES_INCLUDE=*.cs
SOURCES_EXCLUDE=\
	SimpleTestCollector.cs		\
	ClassPathTestCollector.cs	\
	ReflectionUtils.cs		\
	ITestSuiteLoader.cs		\
	LoadingTestCollector.cs		\
	ReloadingTestSuiteLoader.cs	\
	StandardTestSuiteLoader.cs	\
	TestCaseClassLoader.cs

MONO_PATH=$(topdir)/class/lib

include $(topdir)/class/library.make
