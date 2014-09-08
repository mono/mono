#include <mono/metadata/memory-profiler.h>
#include <mono/metadata/metadata-internals.h>
#include <mono/metadata/image.h>
#include <mono/utils/mono-mutex.h>
#include <mono/utils/mono-internal-hash.h>
#include <mono/utils/mono-conc-hashtable.h>

typedef enum
{
	MEMPOOL,
	G_HASHTABLE,
	NESTED_G_HASHTABLE,
	MONO_HASHTABLE,
	INTERNAL_HASHTABLE,
	CONC_HASHTABLE,
	PROPERTY_HASHTABLE,
	ITEM_KIND_MAX,
} MemDomItemKind;

typedef struct {
	MemDomItemKind type;
	int offset;
	const char *name;
} ItemDesc;

#define MEMDOM_TYPE_START(TYPE) static ItemDesc descriptor_ ## TYPE [] = {
#define MEMDOM_FIELD(TYPE,FIELD,KIND) { KIND, G_STRUCT_OFFSET (TYPE, FIELD), #FIELD },
#define MEMDOM_TYPE_END() { ITEM_KIND_MAX } };

#include "memory-domain-def.h"

static mono_mutex_t memprof_mutex;
static GHashTable *mem_domains, *global_allocs;

typedef struct {
	gpointer address;
	MemoryDomainKind kind;
	size_t extra_alloc;
} MemDomain;

typedef struct {
	const char *tag;
	size_t alloc_bytes;
} AllocInfo;

static inline void
memprof_lock (void)
{
	mono_mutex_lock (&memprof_mutex);
}

static inline void
memprof_unlock (void)
{
	mono_mutex_unlock (&memprof_mutex);
}

void
mono_memory_profiler_init (void)
{
	mono_mutex_init (&memprof_mutex);
	mem_domains = g_hash_table_new_full (mono_aligned_addr_hash, NULL, NULL, g_free);
	global_allocs = g_hash_table_new_full (g_str_hash, g_str_equal, NULL, g_free);
}


void
mono_profiler_register_memory_domain (gpointer domain, MemoryDomainKind kind)
{
	MemDomain *dom = g_new0 (MemDomain, 1);
	dom->address = domain;
	dom->kind = kind;
	memprof_lock ();
	g_hash_table_insert (mem_domains, domain, dom);
	memprof_unlock ();
}

void
mono_profiler_free_memory_domain (gpointer domain)
{
	memprof_lock ();
	g_hash_table_remove (mem_domains, domain);
	memprof_unlock ();
}

void
mono_profiler_add_allocation (const char *tag, gsize size)
{
	AllocInfo *info;
	memprof_lock ();
	info = g_hash_table_lookup (global_allocs, tag);
	if (!info) {
		info = g_new0 (AllocInfo, 1);
		info->tag = tag;
		g_hash_table_insert (global_allocs, (char*)tag, info);
	}
	info->alloc_bytes += size;
	memprof_unlock ();
}

void
mono_profiler_remove_allocation (const char *tag, gsize size)
{
	AllocInfo *info;
	memprof_lock ();
	info = g_hash_table_lookup (global_allocs, tag);
	if (info)
		info->alloc_bytes -= size;
	memprof_unlock ();
}

static void
add_nested_hashes (gpointer key, gpointer value, gpointer user_data)
{
	GHashTable *prop_hash = (GHashTable*)value;
	size_t *size = user_data;
	*size += mono_eg_hashtable_get_memory_size (prop_hash);
}

//TODO account for the key/value sizes unique to the cached entry
static size_t
dump_desc (ItemDesc *desc, gpointer *domain)
{
	gpointer *slot = (gpointer*)((char*)domain + desc->offset);
	size_t size;
	const char *kind;
	switch (desc->type) {
	case MEMPOOL:
		if (!*slot)
			return 0;
		size = mono_mempool_get_allocated (*slot);
		kind = "mempool";
		break;
	case G_HASHTABLE:
		if (!*slot)
			return 0;
		size = mono_eg_hashtable_get_memory_size (*slot);
		kind = "g_hashtable";
		break;
	case NESTED_G_HASHTABLE:
		if (!*slot)
			return 0;
		size = mono_eg_hashtable_get_memory_size (*slot);
		g_hash_table_foreach (*slot, add_nested_hashes, &size);
		kind = "nested_g_hashtable";
		break;	
	case MONO_HASHTABLE:
		if (!*slot)
			return 0;
		size = mono_hashtable_get_memory_size (*slot);
		kind = "mono_hashtable";
		break;
	case INTERNAL_HASHTABLE:
		size = mono_internal_hashtable_get_memory_size ((MonoInternalHashTable*)slot);
		kind = "internal_hashtable";
		break;
	case CONC_HASHTABLE:
		if (!*slot)
			return 0;
		size = mono_conc_hashtable_get_memory_size (*slot);
		kind = "conc_hashtable";
		break;
	case PROPERTY_HASHTABLE:
		if (!*slot)
			return 0;
		size = mono_property_hash_get_memory_size (*slot);
		kind = "property_hashtable";
		break;

	default:
		g_assert (0);
	}
	printf ("\t%s:%s size %zd\n", kind, desc->name, size);
	return size;
}

static size_t
dump_memdom (MemDomain *dom, int domsize, ItemDesc *descriptor, char *name)
{
	int i;
	size_t total = domsize + dom->extra_alloc;
	printf ("%s: size %d extra malloc %zd\n", name, domsize, dom->extra_alloc);

	for (i = 0; descriptor [i].type != ITEM_KIND_MAX; ++i)
		total += dump_desc (&descriptor [i], dom->address);
	printf ("--total: %zd\n\n", total);
	g_free (name);
	return total;
}

static void
dump_domain (gpointer key, gpointer val, gpointer user_data)
{
	size_t *md_total = user_data;
	MemDomain *dom = val;
	switch (dom->kind) {
	case MEMDOM_APPDOMAIN: {
		MonoDomain *domain = dom->address;
		*md_total += dump_memdom (dom, sizeof (MonoDomain), descriptor_MonoDomain, g_strdup_printf ("domain_%d", domain->domain_id));
		break;
	}
	case MEMDOM_IMAGE: {
		MonoImage *image = dom->address;
		*md_total += dump_memdom (dom, sizeof (MonoImage), descriptor_MonoImage, g_strdup_printf ("image_%s", image->module_name));
		break;
	} 
	case MEMDOM_IMAGE_SET: {
		*md_total += dump_memdom (dom, sizeof (MonoImageSet), descriptor_MonoImageSet, g_strdup_printf ("imageset_%p", dom->address));
		break;
	}
	default:
		g_assert (0);
	}
}

static void
dump_allocs (gpointer key, gpointer val, gpointer user_data)
{
	size_t *md_total = user_data;
	AllocInfo *info = val;
	printf ("%s: %zd\n", info->tag, info->alloc_bytes);
	*md_total += info->alloc_bytes;
}

void
mono_memprof_dump (void)
{
	size_t grand_total = 0;
	memprof_lock ();
	printf ("---Alloc Domains---\n");
	g_hash_table_foreach (mem_domains, dump_domain, &grand_total);
	printf ("---Allocations---\n");
	g_hash_table_foreach (global_allocs, dump_allocs, &grand_total);
	printf ("---------- grand total: %zd\n", grand_total);
	memprof_unlock ();
}



//g_hash_table_foreach (override_map, foreach_override, NULL);


// void mono_profiler_add_allocation (gpointer domain, gpointer address, gsize size, const char *description) MONO_INTERNAL;
// void mono_profiler_remove_allocation (gpointer domain, gpointer address) MONO_INTERNAL;
