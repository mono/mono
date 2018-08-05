/**
 * \file
 */

#ifndef __MONO_BSEARCH_H__
#define __MONO_BSEARCH_H__

#include <stdlib.h>

#include "mono/utils/mono-publib.h"     // FIXMcxx; remove; for monodis
#include "mono/utils/mono-compiler.h"

G_BEGIN_DECLS // FIXMcxx; remove; for monodis

typedef int (* BinarySearchComparer) (const void *key, const void *member);

void *
mono_binary_search (
	const void *key,
	const void *array,
	size_t array_length,
	size_t member_size,
	BinarySearchComparer comparer);

G_END_DECLS // FIXMcxx; remove; for monodis

#endif
