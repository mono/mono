// disable lots of warnings :)
#pragma warning(disable:4018) // signed/unsigned mismatch
#pragma warning(disable:4244) // converstion from foo to boo, possible loss of data
#pragma warning(disable:4267) // converstion from foo to boo, possible loss of data
#pragma warning(disable:4311) // pointer truncation to int32
#pragma warning(disable:4312) // converstion from int32 to pointer of greater size
#pragma warning(disable:4047) // different levels of indirection
#pragma warning(disable:4133) // incompatible pointer types
#pragma warning(disable:4700) // local variable used without being initialized
#pragma warning(disable:4715) // not all control paths return a value

#include "unity_utils.h"

// redefine exit() to be unity_mono_exit()
#define exit unity_mono_exit

// redefine various file related functions to do proper UTF8 to UTF16 conversion
#define fopen unity_fopen