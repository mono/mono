// Empty structs are disallowed by ANSI C, but are an extension
// in gcc/clang C and are of size 0.
// They are allowed in C++ and are of size 1.
// To keep empty struct tests working, they must remain in C.

#ifdef __cplusplus
#error This file must remain as C.
#endif

#include <config.h>
#include <string.h>
#include <glib.h>

// __stdcall is either a keyword or a builtin macro.
#if !defined (_MSC_VER) && !defined (__stdcall)
#define __stdcall /* nothing */
#endif

#if defined (WIN32) && defined (_MSC_VER)
#define LIBTEST_API __declspec (dllexport)
#elif defined (__GNUC__)
#define LIBTEST_API __attribute__ ((__visibility__ ("default")))
#else
#define LIBTEST_API
#endif

#ifdef __GNUC__
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++-compat"
#endif

/*
* Standard C and C++ doesn't allow empty structs, empty structs will always have a size of 1 byte.
* GCC have an extension to allow empty structs, https://gcc.gnu.org/onlinedocs/gcc/Empty-Structures.html.
* This cause a little dilemma since runtime build using none GCC compiler will not be compatible with
* GCC build C libraries and the other way around. On platforms where empty structs has size of 1 byte
* it must be represented in call and cannot be dropped. On Windows x64 structs will always be represented in the call
* meaning that an empty struct must have a representation in the callee in order to correctly follow the ABI used by the
* C/C++ standard and the runtime.
*/
typedef struct {
#if !defined(__GNUC__) || defined(TARGET_WIN32)
    char a;
#endif
} EmptyStruct;

#ifdef __GNUC__
#pragma GCC diagnostic pop
#endif

LIBTEST_API int __stdcall
mono_test_empty_struct (int a, EmptyStruct es, int b);

LIBTEST_API EmptyStruct __stdcall
mono_test_return_empty_struct (int a);

/* this does not work on Redhat gcc 2.96 */
LIBTEST_API int __stdcall
mono_test_empty_struct (int a, EmptyStruct es, int b)
{
	// printf ("mono_test_empty_struct %d %d\n", a, b);

	// Intel icc on ia64 passes 'es' in 2 registers
#if defined(__ia64) && defined(__INTEL_COMPILER)
	return 0;
#else
	if (a == 1 && b == 2)
		return 0;
	return 1;
#endif
}

LIBTEST_API EmptyStruct __stdcall
mono_test_return_empty_struct (int a)
{
	EmptyStruct s;

	memset (&s, 0, sizeof (s));

#if !(defined(__i386__) && defined(__clang__))
	/* https://bugzilla.xamarin.com/show_bug.cgi?id=58901 */
	g_assert (a == 42);
#endif

	return s;
}
