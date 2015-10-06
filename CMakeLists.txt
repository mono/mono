cmake_minimum_required (VERSION 3.1)

project (mono)

if (${CMAKE_SYSTEM_NAME} MATCHES "Darwin")
	set (DARWIN 1)
endif ()

## FIXME
# set (ANDROID )

include (CheckIncludeFile)
include (CheckLibraryExists)
include (CheckSymbolExists)
include (CheckFunctionExists)
include (CheckTypeSize)
include (CheckCSourceCompiles)

add_subdirectory (eglib)
