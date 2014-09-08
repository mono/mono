#include <mono/metadata/memory-profiler.h>
#include <mono/metadata/metadata-internals.h>
#include <mono/metadata/image.h>
#include <mono/utils/mono-mutex.h>
#include <mono/utils/mono-internal-hash.h>
#include <mono/utils/mono-conc-hashtable.h>
#include <mono/utils/mono-mmap.h>
#include <mono/utils/atomic.h>

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

typedef struct {
	const char *tag;
	gpointer address;
	size_t size;
} VAllocRecord;

#define VALLOC_ENTRIES (4096 / sizeof (VAllocRecord) - 1)

typedef struct _VAllocRecordBucket VAllocRecordBucket;

struct _VAllocRecordBucket{
	int index;
	VAllocRecordBucket *next_bucket;
	VAllocRecord entries [VALLOC_ENTRIES];
};

static VAllocRecordBucket *current_bucket;

static VAllocRecordBucket*
alloc_bucket (void)
{
	VAllocRecordBucket *old, *bucket;

	old = current_bucket;
	if (old && old->index < VALLOC_ENTRIES)
		return old;

	bucket = mono_valloc (NULL, sizeof (VAllocRecordBucket),  MONO_MMAP_READ|MONO_MMAP_WRITE|MONO_MMAP_PRIVATE|MONO_MMAP_ANON, NULL);
	bucket->index = 0;

	do {
		old = current_bucket;
		bucket->next_bucket = old;
	} while (InterlockedCompareExchangePointer ((gpointer)&current_bucket, bucket, old) != old);
	return bucket;
}

static VAllocRecord*
get_next_record (void)
{
	VAllocRecordBucket *bucket;
	int index;

retry:
	bucket = current_bucket;
	if (!current_bucket)
		bucket = alloc_bucket ();
	while (bucket->index >= VALLOC_ENTRIES)
		bucket = alloc_bucket ();

	index = InterlockedIncrement (&bucket->index) - 1;
	if (index >= VALLOC_ENTRIES)
		goto retry;	

	return &bucket->entries [index];
}

#define ALIGN_TO(val,align) ((((guint64)val) + ((align) - 1)) & ~((align) - 1))

void
mono_profiler_valloc (gpointer address, size_t size, const char *tag)
{
	VAllocRecord *record;
	if (!tag)
		return;

	record = get_next_record ();
	record->tag = tag;
	record->address = address;
	record->size = ALIGN_TO (size, 4096);
}

void
mono_profiler_vfree (gpointer address, size_t size)
{
	VAllocRecord *record;
	record = get_next_record ();
	record->tag = NULL;
	record->address = address;
	record->size = ALIGN_TO (size, 4096);
}


#if (SIZEOF_VOID_P == 4)
#define UINT_TO_POINTER(u) ((void*)(guint32)(u))
#define POINTER_TO_UINT(p) ((guint32)(void*)(p))
#elif (SIZEOF_VOID_P == 8)
#define UINT_TO_POINTER(u) ((void*)(guint64)(u))
#define POINTER_TO_UINT(p) ((guint64)(void*)(p))
#else
#error Bad size of void pointer
#endif


static void
cat_add_val (GHashTable *hash, const char *tag, size_t new_val)
{
	size_t cur_val = POINTER_TO_UINT (g_hash_table_lookup (hash, tag));
	g_hash_table_replace (hash, (char*)tag, UINT_TO_POINTER (cur_val + new_val));
}

//This sucks, but tracking partial vfree makes it really complicated
static const char *
find_cat (gpointer address, gpointer limit)
{
	int i;
	const char *cat = NULL;
	VAllocRecordBucket *bucket = current_bucket;

	for (bucket = current_bucket; bucket; bucket = bucket->next_bucket) {
		for (i = 0; i < MIN (bucket->index, VALLOC_ENTRIES); ++i) {
			VAllocRecord *r = &bucket->entries [i];
			if (r == limit)
				return cat;
			if (!r->tag)
				continue;
			if (r->address <= address && ((char*)r->address + r->size) > (char*)address)
				cat = r->tag;
		}
	}
	g_error ("should not happen");
}

static void
cat_del_val (GHashTable *hash, gpointer address, size_t size, gpointer record)
{
	size_t cur_val;
	const char *cat = find_cat (address, record);
	if (!cat) {
		printf ("bad vfree %p\n", address);
		return;
	}
	cur_val = POINTER_TO_UINT (g_hash_table_lookup (hash, cat));
	g_hash_table_replace (hash, (char*)cat, UINT_TO_POINTER (cur_val - size));
}

static void
dump_cat_alloc (gpointer key, gpointer val, gpointer user_data)
{
	printf ("\t%s %zd\n", key, POINTER_TO_UINT (val));
}

//TODO sumarize alloc records
static size_t
dump_valloc (void)
{
	int i, entries = 0;
	size_t total_alloc = 0, total_free = 0;
	VAllocRecordBucket *bucket = current_bucket;
	GHashTable *per_cat_alloc = g_hash_table_new (g_str_hash, g_str_equal);

	for (bucket = current_bucket; bucket; bucket = bucket->next_bucket) {
		for (i = 0; i < MIN (bucket->index, VALLOC_ENTRIES); ++i) {
			VAllocRecord *r = &bucket->entries [i];
			++entries;
			if (r->tag) {
				total_alloc += r->size;
				cat_add_val (per_cat_alloc, r->tag, r->size);
			} else {
				total_free += r->size;
				cat_del_val (per_cat_alloc, r->address, r->size, r);
			}
		}
	}

	g_hash_table_foreach (per_cat_alloc, dump_cat_alloc, NULL);
	printf ("VALLOC summary %zd allocated %zd freed - %zd active %d entries\n", total_alloc, total_free, total_alloc - total_free, entries);
	g_hash_table_destroy (per_cat_alloc);
	return total_alloc;
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
	printf ("---VALLOC---\n");
	grand_total += dump_valloc ();

	printf ("---------- grand total: %zd\n", grand_total);
	memprof_unlock ();
}

