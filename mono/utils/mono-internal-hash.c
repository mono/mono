/**
 * \file
 * A hash table which uses the values themselves as nodes.
 *
 * Author:
 *   Mark Probst (mark.probst@gmail.com)
 *
 * (C) 2007 Novell, Inc.
 *
 */

#include <config.h>
#include <glib.h>
#include <mono/utils/mono-compiler.h>
#include <mono/utils/mono-internal-hash.h>

#define MIN_SIZE	11
#define HASH(k,f,s)	((f)((k)) % (s))

void
mono_internal_hash_table_init (MonoInternalHashTable *table,
			       GHashFunc hash_func,
			       MonoInternalHashKeyExtractFunc key_extract,
			       MonoInternalHashNextValueFunc next_value)
{
	table->hash_func = hash_func;
	table->key_extract = key_extract;
	table->next_value = next_value;

	table->size = MIN_SIZE;
	table->num_entries = 0;
	table->table = g_new0 (gpointer, table->size);
}

void
mono_internal_hash_table_destroy (MonoInternalHashTable *table)
{
	g_free (table->table);
	table->table = NULL;
}

gpointer
mono_internal_hash_table_lookup (MonoInternalHashTable *table, gpointer key)
{
	gpointer value;

	g_assert (table->table != NULL);

	for (value = table->table [HASH (key, table->hash_func, table->size)];
	     value != NULL;
	     value = *(table->next_value (value))) {
		if (table->key_extract (value) == key)
			return value;
	}
	return NULL;
}

static void
resize_if_needed (MonoInternalHashTable *table)
{
	gpointer *new_table;
	gint new_size;
	gint i;

	if (table->num_entries < table->size * 3)
		return;

	new_size = g_spaced_primes_closest (table->num_entries);
	new_table = g_new0 (gpointer, new_size);

	for (i = 0; i < table->size; ++i) {
		while (table->table[i] != NULL) {
			gpointer value;
			gint hash;

			value = table->table [i];
			table->table [i] = *(table->next_value (value));

			hash = HASH (table->key_extract (value), table->hash_func, new_size);
			*(table->next_value (value)) = new_table [hash];
			new_table [hash] = value;
		}
	}

	g_free (table->table);

	table->size = new_size;
	table->table = new_table;
}

void
mono_internal_hash_table_insert (MonoInternalHashTable *table,
				 gpointer key, gpointer value)
{
	gint hash = HASH (key, table->hash_func, table->size);

	g_assert (table->key_extract(value) == key);
	g_assert (*(table->next_value (value)) == NULL);
	g_assert (mono_internal_hash_table_lookup (table, key) == NULL);

	*(table->next_value (value)) = table->table[hash];
	table->table [hash] = value;

	++table->num_entries;

	resize_if_needed (table);
}

gboolean
mono_internal_hash_table_remove (MonoInternalHashTable *table, gpointer key)
{
	gint hash = HASH (key, table->hash_func, table->size);
	gpointer *value;

	for (value = &table->table [hash];
	     *value != NULL;
	     value = table->next_value (*value)) {
		if (table->key_extract (*value) == key)
		{
			*value = *(table->next_value (*value));
			--table->num_entries;
			return TRUE;
		}
	}

	return FALSE;
}


// extend by dsqiu

guint
mono_internal_hash_table_foreach_remove(MonoInternalHashTable *table, GHRFunc func, gpointer user_data)
{
	int i;
	int count = 0;

	g_return_val_if_fail(table != NULL, 0);
	g_return_val_if_fail(func != NULL, 0);
	if (table->num_entries > 0)
	{
		GPtrArray* removed_element_array = g_ptr_array_new();
		for (i = 0; i < table->size; ++i) {
			gpointer value;
			value = table->table[i];
			while (value != NULL) {
				gpointer key;
				gint hash;

				key = table->key_extract(value);
				if (func(key, value, user_data))
				{
					g_ptr_array_add(removed_element_array, key);
				}
				value = *(table->next_value(value));
			}
		}
		if (removed_element_array->len > 0)
		{
			for (int index = 0; index < removed_element_array->len; index++)
			{
				mono_internal_hash_table_remove(table, g_ptr_array_index(removed_element_array, index));
			}
		}
		g_ptr_array_free(removed_element_array, TRUE);
	}
	return count;
}
// extend end