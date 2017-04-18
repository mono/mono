#include <mono/metadata/mono-adapt-set.h>


static void
overflow (MonoAdaptSet *set)
{
	int i;
	set->overflow = g_hash_table_new (NULL, NULL);
	for (i = 0; i < set->count; ++i)
		g_hash_table_insert (set->overflow, set->values [i], set->values [i]);
}

/**
 * Lookup whether @value is part of @set.
 */
gboolean
mono_adapt_set_contains (MonoAdaptSet *set, gpointer value)
{
	int i;
	if (set->overflow)
		return g_hash_table_lookup (set->overflow, value) != NULL;
	for (i = 0; i < set->count; ++i) {
		if (set->values [i] == value)
			return TRUE;
	}
	return FALSE;
}

/**
 * Add @value to @set. @value must not be NULL.
 */
void
mono_adapt_set_add (MonoAdaptSet *set, gpointer value)
{
	if (set->count >= MAX_ADAPT_PAIRS)
		overflow (set);
	if (set->overflow) {
		g_hash_table_insert (set->overflow, value, value);
	} else {
		int i;
		for (i = 0; i < set->count; ++i) {
			if (set->values [i] == value)
				return;
		}
		set->values [set->count] = value;
		++set->count;
	}
}

void
mono_adapt_set_init (MonoAdaptSet *set)
{
	set->count = 0;
	set->overflow = NULL;
}

void
mono_adapt_set_destroy (MonoAdaptSet *set)
{
	if (set->overflow)
		g_hash_table_destroy (set->overflow);
}