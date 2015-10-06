project (eglib)

include_directories ("${CMAKE_CURRENT_SOURCE_DIR}")
include_directories ("${CMAKE_CURRENT_SOURCE_DIR}/src")

set (PROJECT_VERSION "0.3")

set (PACKAGE "${PROJECT_NAME}")
set (PACKAGE_VERSION "${PROJECT_VERSION}")
set (PACKAGE_BUGREPORT "http://bugzilla.xamarin.com/enter_bug.cgi?classification=Mono")
set (PACKAGE_NAME "${PACKAGE}")
set (PACKAGE_STRING "${PACKAGE} ${PACKAGE_VERSION}")
set (PACKAGE_TARNAME "${PACKAGE}")
set (PACKAGE_URL "")

if (ANDROID)
	set (PLATFORM_ANDROID 1)
elseif (DARWIN)
	set (TARGET_MACH 1)

	check_c_source_compiles (
"
#include <TargetConditionals.h>
#if TARGET_IPHONE_SIMULATOR == 1 || TARGET_OS_IPHONE == 1
#error fail this for ios
#endif
int main (void) { return 0; }
"
		COMPILATION_SUCCESS
	)

	if (COMPILATION_SUCCESS)
		set (TARGET_OSX 1)
	else ()
		set (TARGET_IOS 1)
	endif ()
endif (ANDROID)

check_include_file ("dlfcn.h" HAVE_DLFCN_H)
check_include_file ("getopt.h" HAVE_GETOPT_H)
check_include_file ("iconv.h" HAVE_ICONV_H)
check_include_file ("inttypes.h" HAVE_INTTYPES_H)
check_include_file ("langinfo.h" HAVE_LANGINFO_H)
check_include_file ("localcharset.h" HAVE_LOCALCHARSET_H)
check_include_file ("memory.h" HAVE_MEMORY_H)
check_include_file ("pwd.h" HAVE_PWD_H)
check_include_file ("stdint.h" HAVE_STDINT_H)
check_include_file ("stdlib.h" HAVE_STDLIB_H)
check_include_file ("string.h" HAVE_STRING_H)
check_include_file ("strings.h" HAVE_STRINGS_H)
check_include_file ("sys/resource.h" HAVE_SYS_RESOURCE_H)
check_include_file ("sys/select.h" HAVE_SYS_SELECT_H)
check_include_file ("sys/stat.h" HAVE_SYS_STAT_H)
check_include_file ("sys/time.h" HAVE_SYS_TIME_H)
check_include_file ("sys/types.h" HAVE_SYS_TYPES_H)
check_include_file ("sys/wait.h" HAVE_SYS_WAIT_H)
check_include_file ("unistd.h" HAVE_UNISTD_H)

check_symbol_exists (getrlimit "sys/resource.h" HAVE_GETRLIMIT)
check_symbol_exists (rewinddir "dirent.h" HAVE_REWINDDIR)
check_symbol_exists (stpcpy "string.h" HAVE_STPCPY)
check_symbol_exists (strlcpy "string.h" HAVE_STRLCPY)
check_symbol_exists (strtok_r "string.h" HAVE_STRTOK_R)
check_symbol_exists (vasprintf "stdio.h" HAVE_VASPRINTF)

if (HAVE_ICONV_H)
	check_library_exists (iconv iconv "-liconv" HAVE_ICONV)
endif ()

# Mono currently supports 10.6, but strndup is not available prior to 10.7; avoiding
# the detection of strndup on OS X so Mono built on 10.7+ still runs on 10.6. This can be
# removed once support for 10.6 is dropped.
#
# iOS detection of strndup and getpwuid_r is faulty for some reason so let's simply avoid it
if (TARGET_OSX)
	check_function_exists (getpwuid_r HAVE_GETPWUID_R)
elseif (NOT TARGET_IOS)
	check_function_exists (getpwuid_r HAVE_GETPWUID_R)
	check_function_exists (strndup HAVE_STRNDUP)
endif ()

check_type_size ("int" SIZEOF_INT)
check_type_size ("long" SIZEOF_LONG)
check_type_size ("long long" SIZEOF_LONG_LONG)
check_type_size ("void*" SIZEOF_VOID_P)

configure_file (
	"${CMAKE_CURRENT_SOURCE_DIR}/config.h.cmake"
	"${CMAKE_CURRENT_BINARY_DIR}/config.h"
)

add_subdirectory (src)
add_subdirectory (test)
