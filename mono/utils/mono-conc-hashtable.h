/*
 * mono-conc-hashtable.h: A mostly concurrent hashtable
 *
 * Author:
 *	Rodrigo Kumpera (kumpera@gmail.com)
 *
 * (C) 2014 Xamarin
 */

#ifndef __MONO_CONCURRENT_HASHTABLE_H__
#define __MONO_CONCURRENT_HASHTABLE_H__

#include <mono/utils/mono-compiler.h>
#include <mono/utils/mono-mutex.h>
#include <glib.h>

typedef struct _MonoConcurrentHashTable MonoConcurrentHashTable;

MonoConcurrentHashTable* mono_conc_hashtable_new (mono_mutex_t *mutex, GHashFunc hash_func, GEqualFunc key_equal_func) MONO_INTERNAL;
MonoConcurrentHashTable* mono_conc_hashtable_new_full (mono_mutex_t *mutex, GHashFunc hash_func, GEqualFunc key_equal_func, GDestroyNotify key_destroy_func, GDestroyNotify value_destroy_func) MONO_INTERNAL;
void mono_conc_hashtable_destroy (MonoConcurrentHashTable *hash_table) MONO_INTERNAL;
gpointer mono_conc_hashtable_lookup (MonoConcurrentHashTable *hash_table, gpointer key) MONO_INTERNAL;
gpointer mono_conc_hashtable_insert (MonoConcurrentHashTable *hash_table, gpointer key, gpointer value) MONO_INTERNAL;
gpointer mono_conc_hashtable_remove (MonoConcurrentHashTable *hash_table, gpointer key) MONO_INTERNAL;

#endif

