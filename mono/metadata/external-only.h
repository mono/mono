/**
 * \file
 *   Shorthand and markers for functions only used by embedders.
 * MONO_ENTER_GC_UNSAFE is also a good indication of external_only.
 *
 * Author:
 *   Jay Krell (jaykrell@microsoft.com)
 *
 * Copyright 2018 Microsoft
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#ifndef MONO_EXTERNAL_ONLY

#define MONO_EXTERNAL_ONLY_BEGIN MONO_ENTER_GC_UNSAFE
#define MONO_EXTERNAL_ONLY_END   MONO_EXIT_GC_UNSAFE

#define MONO_EXTERNAL_ONLY_GC_UNSAFE(t, expr) \
	t result; 		\
	MONO_EXTERNAL_ONLY_BEGIN; 	\
	result = expr;		\
	MONO_EXTERNAL_ONLY_END;	\
	return result;

#define MONO_EXTERNAL_ONLY_GC_UNSAFE_VOID(expr) \
	MONO_EXTERNAL_ONLY_BEGIN; 	\
	expr;			\
	MONO_EXTERNAL_ONLY_END;	\

#define MONO_EXTERNAL_ONLY(t, expr) return expr;
#define MONO_EXTERNAL_ONLY_VOID(expr) expr;

#endif /* MONO_EXTERNAL_ONLY */
