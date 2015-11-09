#ifndef _MONO_UTILS_MONO_PP_FOREACH_H
#define _MONO_UTILS_MONO_PP_FOREACH_H
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

/* MONO_PP_VA_SWALLOW_COMMA_EMPTY(args...) expands to ~, args... if args is non-empty,
 * or to ~ (not missing comma) if args is empty.
 * It turns out that it's quite tricky using
 * standard C99 to do this. But for GCC (and Clang) and Visual C++ we can take advantage
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

/* MONO_PP_VA_COUNT_ARGS expands to the number of arguments that it is passed.
* If you need more than 10 arguments, extend this macro in the
* obvious way. */
#define MONO_PP_VA_COUNT_ARGS(...) MONO_PP_VA_COUNT_ARGS_(MONO_PP_VA_SWALLOW_COMMA_EMPTY(__VA_ARGS__), 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0)
#define MONO_PP_VA_COUNT_ARGS_N(dummy, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, N, ...) N
#define MONO_PP_VA_COUNT_ARGS_(...) MONO_PP_VA_EXPAND_ARGS(MONO_PP_VA_COUNT_ARGS_N(__VA_ARGS__))

/* MONO_PP_VA_FOREACH(Action, e1, ..., eN) to Action(e1) Action(e2)
... Action(eN) where Action may itself be a macro (which will be
expanded).  N may be between 0 and 10.  If you need more than 10
arguments, extend the helper macros below in the obvious way.*/
#define MONO_PP_VA_FOREACH(Action, ...) MONO_PP_RESCAN_MACRO(MONO_PP_VA_MS_WORKAROUND(MONO_PP_CONCATENATE(MONO_PP_VA_FOREACH_, MONO_PP_VA_COUNT_ARGS(__VA_ARGS__)), Action, (__VA_ARGS__)))

// (workaround for the fact that Visual C expands __VA_ARGS__ incorrectly)
// MONO_PP_VA_MS_WORKAROUND(M, X, (Y, Z, W)) expands to M(X,Y,Z,W)  but allows us to pass some caller macro's __VA_ARGS__ as the RestPack.
#define MONO_PP_VA_MS_WORKAROUND(Macro, First, RestPack) Macro MONO_PP_VA_PAREN_ARGS(First, MONO_PP_VA_EXPAND_ARGS RestPack)

#define MONO_PP_VA_FOREACH_10(Action, Arg, ...) Action(Arg) MONO_PP_VA_MS_WORKAROUND(MONO_PP_VA_FOREACH_9, Action, (__VA_ARGS__))
#define MONO_PP_VA_FOREACH_9(Action, Arg, ...) Action(Arg) MONO_PP_VA_MS_WORKAROUND(MONO_PP_VA_FOREACH_8, Action, (__VA_ARGS__))
#define MONO_PP_VA_FOREACH_8(Action, Arg, ...) Action(Arg) MONO_PP_VA_MS_WORKAROUND(MONO_PP_VA_FOREACH_7, Action, (__VA_ARGS__))
#define MONO_PP_VA_FOREACH_7(Action, Arg, ...) Action(Arg) MONO_PP_VA_MS_WORKAROUND(MONO_PP_VA_FOREACH_6, Action, (__VA_ARGS__))
#define MONO_PP_VA_FOREACH_6(Action, Arg, ...) Action(Arg) MONO_PP_VA_MS_WORKAROUND(MONO_PP_VA_FOREACH_5, Action, (__VA_ARGS__))
#define MONO_PP_VA_FOREACH_5(Action, Arg, ...) Action(Arg) MONO_PP_VA_MS_WORKAROUND(MONO_PP_VA_FOREACH_4, Action, (__VA_ARGS__))
#define MONO_PP_VA_FOREACH_4(Action, Arg, ...) Action(Arg) MONO_PP_VA_MS_WORKAROUND(MONO_PP_VA_FOREACH_3, Action, (__VA_ARGS__))
#define MONO_PP_VA_FOREACH_3(Action, Arg, ...) Action(Arg) MONO_PP_VA_MS_WORKAROUND(MONO_PP_VA_FOREACH_2, Action, (__VA_ARGS__))
#define MONO_PP_VA_FOREACH_2(Action, Arg, ...) Action(Arg) MONO_PP_VA_MS_WORKAROUND(MONO_PP_VA_FOREACH_1, Action, (__VA_ARGS__))
#define MONO_PP_VA_FOREACH_1(Action, Arg, ...) Action(Arg) MONO_PP_VA_MS_WORKAROUND(MONO_PP_VA_FOREACH_0, Action, (__VA_ARGS__))
#define MONO_PP_VA_FOREACH_0(...) /*empty*/

/* MONO_PP_VA_FOREACH(Action, e1, ..., eN) expands to Action(e1), Action(e2), ..., Action(eN)
 * (Note the commas separating each action, and that there's no trailing comma.)
 */
#define MONO_PP_VA_FOREACH_CS(Action, ...) MONO_PP_RESCAN_MACRO(MONO_PP_VA_MS_WORKAROUND(MONO_PP_CONCATENATE(MONO_PP_VA_FOREACH_CS_, MONO_PP_VA_COUNT_ARGS(__VA_ARGS__)), Action, (__VA_ARGS__)))

#define MONO_PP_VA_FOREACH_CS_10(Action, Arg, ...) Action(Arg), MONO_PP_VA_MS_WORKAROUND(MONO_PP_VA_FOREACH_CS_9, Action, (__VA_ARGS__))
#define MONO_PP_VA_FOREACH_CS_9(Action, Arg, ...) Action(Arg), MONO_PP_VA_MS_WORKAROUND(MONO_PP_VA_FOREACH_CS_8, Action, (__VA_ARGS__))
#define MONO_PP_VA_FOREACH_CS_8(Action, Arg, ...) Action(Arg), MONO_PP_VA_MS_WORKAROUND(MONO_PP_VA_FOREACH_CS_7, Action, (__VA_ARGS__))
#define MONO_PP_VA_FOREACH_CS_7(Action, Arg, ...) Action(Arg), MONO_PP_VA_MS_WORKAROUND(MONO_PP_VA_FOREACH_CS_6, Action, (__VA_ARGS__))
#define MONO_PP_VA_FOREACH_CS_6(Action, Arg, ...) Action(Arg), MONO_PP_VA_MS_WORKAROUND(MONO_PP_VA_FOREACH_CS_5, Action, (__VA_ARGS__))
#define MONO_PP_VA_FOREACH_CS_5(Action, Arg, ...) Action(Arg), MONO_PP_VA_MS_WORKAROUND(MONO_PP_VA_FOREACH_CS_4, Action, (__VA_ARGS__))
#define MONO_PP_VA_FOREACH_CS_4(Action, Arg, ...) Action(Arg), MONO_PP_VA_MS_WORKAROUND(MONO_PP_VA_FOREACH_CS_3, Action, (__VA_ARGS__))
#define MONO_PP_VA_FOREACH_CS_3(Action, Arg, ...) Action(Arg), MONO_PP_VA_MS_WORKAROUND(MONO_PP_VA_FOREACH_CS_2, Action, (__VA_ARGS__))
#define MONO_PP_VA_FOREACH_CS_2(Action, Arg, ...) Action(Arg), MONO_PP_VA_MS_WORKAROUND(MONO_PP_VA_FOREACH_CS_1, Action, (__VA_ARGS__))
#define MONO_PP_VA_FOREACH_CS_1(Action, Arg, ...) Action(Arg) MONO_PP_VA_MS_WORKAROUND(MONO_PP_VA_FOREACH_CS_0, Action, (__VA_ARGS__))
#define MONO_PP_VA_FOREACH_CS_0(...) /*empty*/



#endif /*_MONO_UTILS_MONO_PP_MONO_PP_VA_FOREACH_H*/
