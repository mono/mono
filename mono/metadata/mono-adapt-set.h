/**
 * \file
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */
#ifndef __MONO_ADAPT_SET_H__
#define __MONO_ADAPT_SET_H__

#include <glib.h>

//128 bytes on 64bits
#define MAX_ADAPT_PAIRS 14

/*
This is a simple on-stack implementation of set that overflows to a g_hash_table.
Usage:

MonoAdaptSet set;
mono_adapt_set_init (&set); //Always stack allow it
...
mono_adapt_set_destroy (&set);
*/
typedef struct {
	GHashTable *overflow;
	int count;
	gpointer values [MAX_ADAPT_PAIRS];
} MonoAdaptSet;

gboolean mono_adapt_set_contains (MonoAdaptSet *hash, gpointer value);
void mono_adapt_set_add (MonoAdaptSet *hash, gpointer value);
void mono_adapt_set_init (MonoAdaptSet *hash);
void mono_adapt_set_destroy (MonoAdaptSet *hash);

#endif
