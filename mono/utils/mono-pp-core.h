/*
 * mono-pp-core.h: Core preprocessor utilities
 *
 * Author:
 *     Aleksey Kliger (aleksey@xamarin.com)
 *
 * Copyright 2015 Xamarin Inc (http://www.xamarin.com)
 */
#ifndef _MONO_UTILS_MONO_PP_CORE_H
#define _MONO_UTILS_MONO_PP_CORE_H
#ifdef _MSC_VER
#pragma once
#endif

#define MONO_PP_CONCATENATE_HELPER(x,y) x ## y
#define MONO_PP_CONCATENATE(x,y) MONO_PP_CONCATENATE_HELPER(x,y)

/* Rescan result of macro invocation for further macros and expand
* again.  This lets us write arbitrary recursive macros (upto some
* limit) */
#define MONO_PP_RESCAN_MACRO(...) MONO_PP_RESCAN_MACRO5(MONO_PP_RESCAN_MACRO5(MONO_PP_RESCAN_MACRO5(__VA_ARGS__)))
#define MONO_PP_RESCAN_MACRO5(...) MONO_PP_RESCAN_MACRO4(MONO_PP_RESCAN_MACRO4(MONO_PP_RESCAN_MACRO4(__VA_ARGS__)))
#define MONO_PP_RESCAN_MACRO4(...) MONO_PP_RESCAN_MACRO3(MONO_PP_RESCAN_MACRO3(MONO_PP_RESCAN_MACRO3(__VA_ARGS__)))
#define MONO_PP_RESCAN_MACRO3(...) MONO_PP_RESCAN_MACRO2(MONO_PP_RESCAN_MACRO2(MONO_PP_RESCAN_MACRO2(__VA_ARGS__)))
#define MONO_PP_RESCAN_MACRO2(...) MONO_PP_RESCAN_MACRO1(MONO_PP_RESCAN_MACRO1(MONO_PP_RESCAN_MACRO1(__VA_ARGS__)))
#define MONO_PP_RESCAN_MACRO1(...) MONO_PP_RESCAN_MACRO0(MONO_PP_RESCAN_MACRO0(MONO_PP_RESCAN_MACRO0(__VA_ARGS__)))
#define MONO_PP_RESCAN_MACRO0(...) __VA_ARGS__

/* MONO_PP_VA_EXPAND_ARGS(args...) expands to args...
* Useful for causing further expansion to happen.
*/
#define MONO_PP_VA_EXPAND_ARGS(...) __VA_ARGS__

/* MONO_PP_VA_PAREN_ARGS(args...) expands to (args...) (note parentheses)
* Useful for triggering a variadic macro to expand
*/
#define MONO_PP_VA_PAREN_ARGS(...) (__VA_ARGS__)

/* MONO_PP_VA_EMPTY(args...) expands to nothing. */

#define MONO_PP_VA_EMPTY(...) /*empty*/


/* MONO_PP_VA_SWALLOW_COMMA_EMPTY(args...) expands to ~, args... if args is non-empty,
* or to ~ (note missing comma) if args is empty.
*
* It turns out that it's quite tricky using standard C99 to do
* this. But for GCC (and Clang) and Visual C++ we can take advantage
* using a nonstandard method that will consume a comma preceeding an
* empty __VA_ARGS__ expansion to do it much more cleanly.
*/
#if defined(_MSC_VER)
#  define MONO_PP_VA_SWALLOW_COMMA_EMPTY(...) ~, __VA_ARGS__
#elif defined(__GNUC__) || defined (__clang__)
#  define MONO_PP_VA_SWALLOW_COMMA_EMPTY(...) ~, ##__VA_ARGS__
#else
#  error "Don't know how to swallow comma before an empty argument with your compiler"
#endif

// MONO_PP_VA_APPLY_PACK(M, (a, b, ...., z)) expands to M(a, ...., z)
#define MONO_PP_VA_APPLY_PACK(Macro, Pack) Macro MONO_PP_VA_PAREN_ARGS(MONO_PP_VA_EXPAND_ARGS Pack)

// (workaround for the fact that Visual C expands __VA_ARGS__ incorrectly)
// MONO_PP_VA_MS_WORKAROUND(M, X, (Y, Z, W)) expands to M(X,Y,Z,W)  but allows us to pass some caller macro's __VA_ARGS__ as the RestPack.
#define MONO_PP_VA_MS_WORKAROUND(Macro, First, RestPack) Macro MONO_PP_VA_PAREN_ARGS(First, MONO_PP_VA_EXPAND_ARGS RestPack)

#define MONO_PP_VA_MS_WORKAROUND_LAST(Macro, InitPack, Last) Macro MONO_PP_VA_PAREN_ARGS(MONO_PP_VA_EXPAND_ARGS InitPack, Last)

/* MONO_PP_IS_CHECK(...) takes at least 1 arguments and expands to
* 0 if given 1 argument or the first argument if given 2 or more arguments. */
#define MONO_PP_IS_CHECK_N(dk, n, ...) n
#define MONO_PP_IS_CHECK(...) MONO_PP_VA_MS_WORKAROUND_LAST(MONO_PP_IS_CHECK_N, (__VA_ARGS__), 0)

/* MONO_PP_PROBE(dummy) is used with MONO_PP_IS_CHECK to test macro arguments.
 * The ideas is to write MONO_PP_IS_CHECK(MONO_PP_CONCATENATE(SOME_TEST_, arg))
 * where SOME_TEST_blah is defined to expand to MONO_PP_PROBE(~).  That will cause
 * the check to expand to 1 if arg is blah or 0 otherwise.
 */
#define MONO_PP_PROBE(x) x, 1,

#endif /*_MONO_UTILS_MONO_PP_CORE_H*/
