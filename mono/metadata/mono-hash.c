/*
 * ghashtable.c: Hashtable implementation
 *
 * Author:
 *   Miguel de Icaza (miguel@novell.com)
 *   Ludovic Henry (ludovic@xamarin.com)
 *
 * (C) 2006 Novell, Inc.
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
#include <config.h>
#include <stdio.h>
#include <math.h>
#include <glib.h>
#include "mono-hash.h"
#include "metadata/gc-internal.h"
#include <mono/utils/checked-build.h>

#ifdef HAVE_BOEHM_GC
#define mg_new0(type,n)  ((type *) GC_MALLOC(sizeof(type) * (n)))
#define mg_new(type,n)   ((type *) GC_MALLOC(sizeof(type) * (n)))
#define mg_free(x)       do { } while (0)
#else
#define mg_new0(x,n)     g_new0(x,n)
#define mg_new(type,n)   g_new(type,n)
#define mg_free(x)       g_free(x)
#endif

#define LOAD_FACTOR 0.8

#define TOMBSTONE (GINT_TO_POINTER (-1))

#define IS_EMPTY(s) (G_UNLIKELY ((s)->key == NULL))
#define IS_TOMBSTONE(s) (FALSE)
// #define IS_TOMBSTONE(s) (G_UNLIKELY ((s)->key == TOMBSTONE))

typedef struct {
	MonoObject *key;
	MonoObject *value;
	gsize initial_bucket;
} Slot;

struct _MonoGHashTable {
	GHashFunc      hash_func;
	GEqualFunc     key_equal_func;

	Slot  *table;
	gsize  table_capacity;
	gsize  table_size;
	GDestroyNotify value_destroy_func, key_destroy_func;
	MonoGHashGCType gc_type;
	MonoGCRootSource source;
	const char *msg;
};

static inline gsize
hashcode (MonoGHashTable *restrict hash, gconstpointer key, gsize size)
{
	return ((*hash->hash_func) (key)) % size;
}

static inline gsize
normalize (MonoGHashTable *restrict hash, gsize i)
{
	gsize ret = (i + hash->table_capacity) % hash->table_capacity;
	g_assert (ret >= 0);
	g_assert (ret <=  hash->table_capacity - 1);
	return ret;
}

/* distance to initial bucket */
static inline gsize
dib (MonoGHashTable *restrict hash, gsize curent_bucket, gsize initial_bucket)
{
	g_assert (curent_bucket < hash->table_capacity);
	g_assert (initial_bucket < hash->table_capacity);
	return normalize (hash, curent_bucket - initial_bucket);
}

static inline void
remove_at (MonoGHashTable *restrict hash, gsize idx)
{
	Slot *s;
	gsize i;

	g_assert (idx < hash->table_capacity);
	s = &hash->table [idx];

	// printf ("[%p] remove %"G_GSIZE_FORMAT" (+%"G_GSIZE_FORMAT")\n", hash, idx, dib (hash, idx, s->initial_bucket));
	if (hash->key_destroy_func != NULL)
		(*hash->key_destroy_func)(s->key);
	if (hash->value_destroy_func != NULL)
		(*hash->value_destroy_func)(s->value);

	s->key = NULL;
	s->value = NULL;

	/* backward shift deletion: shift back the elements after the current one
	 * this is to keep the table as compact as possible, to reduce the dib for
	 * every elements as much as possible */

	for (i = idx + 1; i < idx + hash->table_capacity; ++i) {
		s = &hash->table [normalize (hash, i)];
		if (IS_EMPTY (s) || dib (hash, i, s->initial_bucket) == 0)
			break;
		memcpy (&hash->table [normalize (hash, i - 1)], &hash->table [normalize (hash, i)], sizeof (Slot));
	}

	hash->table_size -= 1;
}

static inline gboolean
insert_at (MonoGHashTable *restrict hash, gsize initial_bucket, MonoObject *key, MonoObject *value)
{
	gsize i, i_first;

	for (i = i_first = initial_bucket; i < i_first + hash->table_capacity; ++i) {
		Slot *s = &hash->table [i % hash->table_capacity];
		if (IS_EMPTY (s) || IS_TOMBSTONE (s)) {
			// printf ("[%p] insert %"G_GSIZE_FORMAT" -> %"G_GSIZE_FORMAT" (+%"G_GSIZE_FORMAT")\n", hash, initial_bucket, i % hash->table_capacity, dib (hash, i % hash->table_capacity, initial_bucket));
			s->key = key;
			s->value = value;
			s->initial_bucket = initial_bucket;
			return TRUE;
		}

		/* robin hood hashing: swap place with a slot which has a lower dib
		 * used to reduce the dib for every elements as much as possible */
		if (dib (hash, i % hash->table_capacity, s->initial_bucket) < dib (hash, i % hash->table_capacity, initial_bucket)) {
			MonoObject *tmp_key = key;
			MonoObject *tmp_value = value;
			gsize tmp_initial_bucket = initial_bucket;

			key = s->key;
			value = s->value;
			initial_bucket = s->initial_bucket;

			s->key = tmp_key;
			s->value = tmp_value;
			s->initial_bucket = tmp_initial_bucket;
		}
	}

	return FALSE;
}

static MonoGHashTable *
mono_g_hash_table_new (GHashFunc hash_func, GEqualFunc key_equal_func);

#ifdef HAVE_SGEN_GC
static MonoGCDescriptor table_hash_descr = MONO_GC_DESCRIPTOR_NULL;

static void mono_g_hash_mark (void *addr, MonoGCMarkFunc mark_func, void *gc_data);
#endif

MonoGHashTable *
mono_g_hash_table_new_type (GHashFunc hash_func, GEqualFunc key_equal_func, MonoGHashGCType type, MonoGCRootSource source, const char *msg)
{
	MonoGHashTable *hash = mono_g_hash_table_new (hash_func, key_equal_func);

	hash->gc_type = type;
	hash->source = source;
	hash->msg = msg;

	if (type > MONO_HASH_KEY_VALUE_GC)
		g_error ("wrong type for gc hashtable");

#ifdef HAVE_SGEN_GC
	/*
	 * We use a user defined marking function to avoid having to register a GC root for
	 * each hash node.
	 */
	if (!table_hash_descr)
		table_hash_descr = mono_gc_make_root_descr_user (mono_g_hash_mark);
	mono_gc_register_root_wbarrier ((char*)hash, sizeof (MonoGHashTable), table_hash_descr, source, msg);
#endif

	return hash;
}

static MonoGHashTable *
mono_g_hash_table_new (GHashFunc hash_func, GEqualFunc key_equal_func)
{
	MonoGHashTable *hash;

	if (hash_func == NULL)
		hash_func = g_direct_hash;
	if (key_equal_func == NULL)
		key_equal_func = g_direct_equal;
	hash = mg_new0 (MonoGHashTable, 1);

	hash->hash_func = hash_func;
	hash->key_equal_func = key_equal_func;

	hash->table_capacity = g_spaced_primes_closest (1);
	hash->table = mg_new0 (Slot, hash->table_capacity);

	return hash;
}

static inline gboolean
should_rehash (MonoGHashTable *hash)
{
	g_assert (hash->table_size >= 0);

	if (hash->table_size > hash->table_capacity * LOAD_FACTOR)
		return TRUE;

	if (hash->table_capacity == g_spaced_primes_closest (hash->table_size * 1.0 / LOAD_FACTOR))
		return FALSE;

	return TRUE;
}

typedef struct {
	MonoGHashTable *hash;
	int new_capacity;
	Slot *table;
} RehashData;

static void*
do_rehash (void *_data)
{
	RehashData *data = _data;
	MonoGHashTable *hash = data->hash;
	gsize old_capacity, new_capacity;
	gsize i;
	Slot *old_table, *new_table;

	old_capacity = hash->table_capacity;
	new_capacity = hash->table_capacity = data->new_capacity;
	// printf ("New size: %d\n", new_capacity);

	old_table = hash->table;
	new_table = hash->table = data->table;

	for (i = 0; i < old_capacity; ++i) {
		Slot *s = &old_table [i];
		if (IS_EMPTY (s) || IS_TOMBSTONE (s))
			continue;

		if (!insert_at (hash, hashcode (hash, s->key, new_capacity), s->key, s->value))
			g_assert_not_reached ();
	}

	return old_table;
}

static void
rehash (MonoGHashTable *hash)
{
	MONO_REQ_GC_UNSAFE_MODE; //we must run in unsafe mode to make rehash safe

	RehashData data;
	gint new_table_capacity;
	gpointer old_table;

	g_assert (hash->table_size >= 0);

	new_table_capacity = g_spaced_primes_closest (hash->table_size * 1.0 / LOAD_FACTOR);
	g_assert (new_table_capacity > 0);

	if (hash->table_capacity == new_table_capacity)
		return;

	// printf ("[%p] rehash %d -> %d, table_size %d (%3.2f%%)\n", hash, hash->table_capacity, new_table_capacity, hash->table_size, 100.0 * hash->table_size / hash->table_capacity);

	// mono_g_hash_table_print_stats (hash);

	data.hash = hash;
	data.new_capacity = new_table_capacity;
	data.table = mg_new0 (Slot, data.new_capacity);

#ifdef USE_COOP_GC
	/* We cannot be preempted */
	old_table = do_rehash (&data);
#else
	old_table = mono_gc_invoke_with_gc_lock (do_rehash, &data);
#endif

	mg_free (old_table);

	g_assert (hash->table_capacity > 0);
	g_assert (hash->table_capacity > hash->table_size);
}

guint
mono_g_hash_table_size (MonoGHashTable *hash)
{
	g_return_val_if_fail (hash != NULL, 0);

	return hash->table_size;
}

gpointer
mono_g_hash_table_lookup (MonoGHashTable *hash, gconstpointer key)
{
	gpointer orig_key, value;

	if (mono_g_hash_table_lookup_extended (hash, key, &orig_key, &value))
		return value;
	else
		return NULL;
}

gboolean
mono_g_hash_table_lookup_extended (MonoGHashTable *restrict hash, gconstpointer key, gpointer *orig_key, gpointer *value)
{
	GEqualFunc equal;
	gsize i_first, i;

	g_return_val_if_fail (hash != NULL, FALSE);
	equal = hash->key_equal_func;

	for (i_first = i = hashcode (hash, key, hash->table_capacity); i < i_first + hash->table_capacity; ++i) {
		Slot *s = &hash->table [i % hash->table_capacity];
		if (IS_EMPTY (s))
			break;
		if (IS_TOMBSTONE (s))
			continue;
		if ((*equal) (s->key, key)) {
			// printf ("[%p] lookup %p at %3d (+%d)\n", hash, key, i, i - i_first);
			*orig_key = s->key;
			*value = s->value;
			return TRUE;
		}
	}

	return FALSE;
}

void
mono_g_hash_table_foreach (MonoGHashTable *restrict hash, GHFunc func, gpointer user_data)
{
	int i;

	g_return_if_fail (hash != NULL);
	g_return_if_fail (func != NULL);

	for (i = 0; i < hash->table_capacity; i++){
		Slot *s = &hash->table [i];
		if (IS_EMPTY (s) || IS_TOMBSTONE (s))
			continue;
		(*func) (s->key, s->value, user_data);
	}
}

gpointer
mono_g_hash_table_find (MonoGHashTable *restrict hash, GHRFunc predicate, gpointer user_data)
{
	int i;

	g_return_val_if_fail (hash != NULL, NULL);
	g_return_val_if_fail (predicate != NULL, NULL);

	for (i = 0; i < hash->table_capacity; i++){
		Slot *s = &hash->table [i];
		if (IS_EMPTY (s) || IS_TOMBSTONE (s))
			continue;
		if ((*predicate)(s->key, s->value, user_data))
			return s->value;
	}

	return NULL;
}

gboolean
mono_g_hash_table_remove (MonoGHashTable *restrict hash, gconstpointer key)
{
	GEqualFunc equal;
	gsize i_first, i;

	g_return_val_if_fail (hash != NULL, FALSE);
	equal = hash->key_equal_func;

	for (i_first = i = hashcode (hash, key, hash->table_capacity); i < i_first + hash->table_capacity; ++i) {
		Slot *s = &hash->table [i % hash->table_capacity];
		if (IS_EMPTY (s))
			break;
		if (IS_TOMBSTONE (s))
			continue;
		if ((*equal) (s->key, key)) {
			remove_at (hash, i);
			return TRUE;
		}
	}

	if (should_rehash (hash))
		rehash (hash);

	return FALSE;
}

guint
mono_g_hash_table_foreach_remove (MonoGHashTable *restrict hash, GHRFunc func, gpointer user_data)
{
	int i;
	int count = 0;

	g_return_val_if_fail (hash != NULL, 0);
	g_return_val_if_fail (func != NULL, 0);

	for (i = 0; i < hash->table_capacity; i++) {
		Slot *s = &hash->table [i];
		if (IS_EMPTY (s) || IS_TOMBSTONE (s))
			continue;
		if ((*func)(s->key, s->value, user_data)) {
			remove_at (hash, i);
			count += 1;
		}
	}

	if (should_rehash (hash))
		rehash (hash);

	return count;
}

void
mono_g_hash_table_destroy (MonoGHashTable *restrict hash)
{
	int i;

	g_return_if_fail (hash != NULL);

#ifdef HAVE_SGEN_GC
	mono_gc_deregister_root ((char*)hash);
#endif

	// mono_g_hash_table_print_stats (hash);

	for (i = 0; i < hash->table_capacity; i++) {
		Slot *s = &hash->table [i];
		if (IS_EMPTY (s) || IS_TOMBSTONE (s))
			continue;
		if (hash->key_destroy_func != NULL)
			(*hash->key_destroy_func)(s->key);
		if (hash->value_destroy_func != NULL)
			(*hash->value_destroy_func)(s->value);
	}

	mg_free (hash->table);
	mg_free (hash);
}

static void
mono_g_hash_table_insert_replace (MonoGHashTable *restrict hash, gpointer key, gpointer value, gboolean replace)
{
	GEqualFunc equal;
	gsize i_first, i;

	g_return_if_fail (hash != NULL);
	g_return_if_fail (key != NULL);
	g_return_if_fail (key != TOMBSTONE);

	equal = hash->key_equal_func;

	for (i_first = i = hashcode (hash, key, hash->table_capacity); i < i_first + hash->table_capacity; ++i) {
		Slot *s = &hash->table [i % hash->table_capacity];
		if (IS_EMPTY (s))
			break;
		if (IS_TOMBSTONE (s))
			continue;
		if ((*equal) (s->key, key)) {
			// printf ("[%p] replace %p -> %p at %3d\n", hash, key, value, i % size);
			if (replace) {
				if (hash->key_destroy_func != NULL)
					(*hash->key_destroy_func) (s->key);
				s->key = key;
			}
			if (hash->value_destroy_func != NULL)
				(*hash->value_destroy_func) (s->value);
			s->value = value;
			return;
		}
	}

	hash->table_size += 1;

	if (should_rehash (hash))
		rehash (hash);

	if (insert_at (hash, hashcode (hash, key, hash->table_capacity), key, value))
		return;

	g_assert_not_reached ();
}

void
mono_g_hash_table_insert (MonoGHashTable *h, gpointer k, gpointer v)
{
	mono_g_hash_table_insert_replace (h, k, v, FALSE);
}

void
mono_g_hash_table_replace(MonoGHashTable *h, gpointer k, gpointer v)
{
	mono_g_hash_table_insert_replace (h, k, v, TRUE);
}

void
mono_g_hash_table_print_stats (MonoGHashTable *restrict hash)
{
	gsize i, j, distance;
	gsize stats_limits[] = { 0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 60, 70, 80, 90, 100, 200, G_MAXINT32 };

	if (hash->table_capacity < 500)
		return;

	printf ("[%p] stats capacity %"G_GSIZE_FORMAT" size %"G_GSIZE_FORMAT" (%3.f%%)\n", hash, hash->table_capacity, hash->table_size, 100.0 * hash->table_size / hash->table_capacity);

	if (hash->table_size == 0) {
		printf ("[%p]    table is empty\n", hash);
		return;
	}

	for (i = 0; i < G_N_ELEMENTS (stats_limits) - 1; ++i) {
		gsize count = 0, min, max;

		min = stats_limits [i];
		if (min >= hash->table_capacity)
			break;

		max = stats_limits [i + 1] - 1;
		if (max >= hash->table_capacity)
			max = hash->table_capacity - 1;

		for (j = 0; j < hash->table_capacity; ++j) {
			Slot *s = &hash->table [j];
			if (IS_EMPTY (s) || IS_TOMBSTONE (s))
				continue;
			distance = dib (hash, j, s->initial_bucket);
			// if (min <= distance && distance <= max)
			if (distance <= max)
			{
				count += 1;
			}
		}

		printf ("[%p]    %3"G_GSIZE_FORMAT" (%3.0f%%) up to %"G_GSIZE_FORMAT"\n", hash, count, 100.0 * count / hash->table_size, max);
		// printf ("[%p]    %3"G_GSIZE_FORMAT" (%3.0f%%) from %"G_GSIZE_FORMAT" up to %"G_GSIZE_FORMAT"\n", hash, count, 100.0 * count / hash->table_size, min, max);
	}
}

#ifdef HAVE_SGEN_GC

/* GC marker function */
static void
mono_g_hash_mark (void *addr, MonoGCMarkFunc mark_func, void *gc_data)
{
	MonoGHashTable *hash = (MonoGHashTable*)addr;
	int i;

	switch (hash->gc_type) {
	case MONO_HASH_KEY_GC: {
		for (i = 0; i < hash->table_capacity; i++) {
			Slot *s = &hash->table [i];
			if (IS_EMPTY (s) || IS_TOMBSTONE (s))
				continue;
			if (s->key)
				mark_func (&s->key, gc_data);
		}
		break;
	}
	case MONO_HASH_VALUE_GC: {
		for (i = 0; i < hash->table_capacity; i++) {
			Slot *s = &hash->table [i];
			if (IS_EMPTY (s) || IS_TOMBSTONE (s))
				continue;
			if (s->value)
				mark_func (&s->value, gc_data);
		}
		break;
	}
	case MONO_HASH_KEY_VALUE_GC: {
		for (i = 0; i < hash->table_capacity; i++) {
			Slot *s = &hash->table [i];
			if (IS_EMPTY (s) || IS_TOMBSTONE (s))
				continue;
			if (s->key)
				mark_func (&s->key, gc_data);
			if (s->value)
				mark_func (&s->value, gc_data);
		}
		break;
	}
	default:
		g_assert_not_reached ();
	}
}

#endif
