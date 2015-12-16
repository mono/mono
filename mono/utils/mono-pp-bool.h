/*
 * mono-pp-bool.h: Macros for conditional expansion.
 *
 * Author:
 *     Aleksey Kliger (aleksey@xamarin.com)
 *
 * Copyright 2015 Xamarin Inc (http://www.xamarin.com)
 */
#ifndef _MONO_UTIL_MONO_PP_BOOL_H
#define _MONO_UTIL_MONO_PP_BOOL_H
#ifdef _MSC_VER
#pragma once
#endif

#include <mono/utils/mono-pp-core.h>

/* MONO_PP_BITNOT(b) must be called with b equal to 1 or 0.  Returns the complement. */
#define MONO_PP_BITNOT(x) MONO_PP_CONCATENATE(MONO_PP_BITNOT_, x)
#define MONO_PP_BITNOT_1 0
#define MONO_PP_BITNOT_0 1

/* MONO_PP_NOT(args) expands to 1 if args is 0, or 0 otherwise*/
#define MONO_PP_NOT(x) MONO_PP_IS_CHECK(MONO_PP_CONCATENATE(MONO_PP_NOT_, x))
#define MONO_PP_NOT_0 MONO_PP_PROBE(~)

/* MONO_PP_BOOL(args) expands to 0 if args is 0, or 1 otherwise*/
#define MONO_PP_BOOL(x) MONO_PP_BITNOT(MONO_PP_NOT(x))


/* MONO_PP_IF(Cond)(Then, Else...) expands to Else... if Cond is 0, or Then otherwise*/
#define MONO_PP_IF(Cond) MONO_PP_BITIF(MONO_PP_BOOL(Cond))

/* MONO_PP_BITIF(b)(Then, Else...) must be called with b equal to 0 or 1.
* Expands to Then if b is 1 or Else... if b is 0.
*/
#define MONO_PP_BITIF(x) MONO_PP_CONCATENATE(MONO_PP_BITIF_, x)
#define MONO_PP_BITIF_0(x, ...) __VA_ARGS__
#define MONO_PP_BITIF_1(x, ...) x

/* MONO_PP_WHEN(Cond)(Result) expands to nothing if Cond is 0, or Result otherwise*/
#define MONO_PP_WHEN(Cond) MONO_PP_IF(Cond)(MONO_PP_VA_EXPAND_ARGS, MONO_PP_VA_EMPTY)


#endif/*_MONO_UTIL_MONO_PP_BOOL_H*/
