/*
 * mono-pp-foreach.h: Macros to iterate over variadic arguments
 *
 * Author:
 *     Aleksey Kliger (aleksey@xamarin.com)
 *
 * Copyright 2015 Xamarin Inc (http://www.xamarin.com)
 */
#ifndef _MONO_UTILS_MONO_PP_FOREACH_H
#define _MONO_UTILS_MONO_PP_FOREACH_H
#ifdef _MSC_VER
#pragma once
#endif

#include <mono/utils/mono-pp-core.h>

/* MONO_PP_VA_COUNT_ARGS expands to the number of arguments that it is
* passed.  If you need more than 10 arguments, extend this macro in the
* obvious way.
*/
#define MONO_PP_VA_COUNT_ARGS(...) MONO_PP_VA_COUNT_ARGS_(MONO_PP_VA_SWALLOW_COMMA_EMPTY(__VA_ARGS__), 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0)
#define MONO_PP_VA_COUNT_ARGS_N(dummy, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, N, ...) N
#define MONO_PP_VA_COUNT_ARGS_(...) MONO_PP_VA_EXPAND_ARGS(MONO_PP_VA_COUNT_ARGS_N(__VA_ARGS__))

/* MONO_PP_VA_FOREACH(Action, e1, ..., eN) to Action(e1) Action(e2) ... Action(eN)
 *  where Action may itself be a macro (which will be expanded).  N may
 * be between 0 and 10.  If you need more than 10 arguments, extend the
 * helper macros below in the obvious way.
 */
#define MONO_PP_VA_FOREACH(Action, ...) MONO_PP_RESCAN_MACRO(MONO_PP_VA_MS_WORKAROUND(MONO_PP_CONCATENATE(MONO_PP_VA_FOREACH_, MONO_PP_VA_COUNT_ARGS(__VA_ARGS__)), Action, (__VA_ARGS__)))

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
