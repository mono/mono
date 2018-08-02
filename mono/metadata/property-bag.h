/**
 * \file
 * Linearizable property bag.
 *
 * Authors:
 *   Rodrigo Kumpera (kumpera@gmail.com)
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */
#ifndef __MONO_METADATA_PROPERTY_BAG_H__
#define __MONO_METADATA_PROPERTY_BAG_H__

#include <mono/utils/mono-compiler.h>

// Neither MONO_BEGIN_DECLS nor G_BEGIN_DECLS is available here. Fallback to what works.
#ifdef __cplusplus
extern "C" {
#endif

typedef struct _MonoPropertyBagItem MonoPropertyBagItem;

struct _MonoPropertyBagItem {
	MonoPropertyBagItem *next;
	int tag;
};

typedef struct {
	MonoPropertyBagItem *head;
} MonoPropertyBag;

void* mono_property_bag_get (MonoPropertyBag *bag, int tag);
void* mono_property_bag_add (MonoPropertyBag *bag, void *value);

#ifdef __cplusplus
} // extern "C"
#endif

#endif
