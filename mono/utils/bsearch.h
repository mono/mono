/**
 * \file
 */

#ifndef __MONO_BSEARCH_H__
#define __MONO_BSEARCH_H__

#include <stdlib.h>

#include "mono/utils/mono-compiler.h"

// Neither MONO_BEGIN_DECLS nor G_BEGIN_DECLS is available here. Fallback to what works.
#ifdef __cplusplus
extern "C" {
#endif

typedef int (* BinarySearchComparer) (const void *key, const void *member);

void *
mono_binary_search (
	const void *key,
	const void *array,
	size_t array_length,
	size_t member_size,
	BinarySearchComparer comparer);

#ifdef __cplusplus
} // extern "C"
#endif

#endif
