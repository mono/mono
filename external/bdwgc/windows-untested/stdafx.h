#define WIN32_LEAN_AND_MEAN
#include <windows.h>

#pragma warning(error: 4013) // function undefined; assuming extern returning int

#ifdef _MT
#  define GC_THREADS 1
#endif

#ifdef _DEBUG
#  define GC_DEBUG
#endif

#define SAVE_CALL_CHAIN
#define SAVE_CALL_COUNT 8
