/**
 * \file
 */

#ifndef __MONO_BSEARCH_H__
#define __MONO_BSEARCH_H__

#include <stdlib.h>

#include "mono/utils/mono-compiler.h"

#ifdef __cplusplus	// FIXMcxx; remove; for monodis
extern "C" {		// FIXMcxx; remove; for monodis
#endif			// FIXMcxx; remove; for monodis

typedef int (* BinarySearchComparer) (const void *key, const void *member);

void *
mono_binary_search (
	const void *key,
	const void *array,
	size_t array_length,
	size_t member_size,
	BinarySearchComparer comparer);

#ifdef __cplusplus	// FIXMcxx; remove; for monodis
} // extern C		// FIXMcxx; remove; for monodis
#endif			// FIXMcxx; remove; for monodis

#endif
