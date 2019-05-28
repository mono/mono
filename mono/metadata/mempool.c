/**
 * \file
 * efficient memory allocation
 *
 * MonoMemPool is for fast allocation of memory. We free
 * all memory when the pool is destroyed.
 *
 * Author:
 *   Dietmar Maurer (dietmar@ximian.com)
 *
 * Copyright 2001-2003 Ximian, Inc (http://www.ximian.com)
 * Copyright 2004-2009 Novell, Inc (http://www.novell.com)
 * Copyright 2011 Xamarin Inc. (http://www.xamarin.com)
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#include <config.h>
#include <glib.h>
#include <string.h>

#include "mempool.h"
#include "mempool-internals.h"
#include "utils/unlocked.h"

/*
 * MonoMemPool is for fast allocation of memory. We free
 * all memory when the pool is destroyed.
 */

#define MEM_ALIGN 8
#define ALIGN_SIZE(s)	(((s) + MEM_ALIGN - 1) & ~(MEM_ALIGN - 1))

// Size of memory at start of mempool reserved for header
#define SIZEOF_MEM_POOL	(ALIGN_SIZE (sizeof (MonoMemPool)))

#if MONO_SMALL_CONFIG
#define MONO_MEMPOOL_PAGESIZE 4096
#define MONO_MEMPOOL_MINSIZE 256
#else
#define MONO_MEMPOOL_PAGESIZE 8192
#define MONO_MEMPOOL_MINSIZE 512
#endif

// The --with-malloc-mempools debug-build flag causes mempools to be allocated in single-element blocks, so tools like Valgrind can run better.
#if USE_MALLOC_FOR_MEMPOOLS
#define INDIVIDUAL_ALLOCATIONS
#define MONO_MEMPOOL_PREFER_INDIVIDUAL_ALLOCATION_SIZE 0
#else
#define MONO_MEMPOOL_PREFER_INDIVIDUAL_ALLOCATION_SIZE MONO_MEMPOOL_PAGESIZE
#endif

#ifndef G_LIKELY
#define G_LIKELY(a) (a)
#define G_UNLIKELY(a) (a)
#endif


// extend by dsqiu
typedef struct _MonoUnusedEntity {
	struct _MonoUnusedEntity* next;
	guint32 size;
	guint8* pos;
} MonoUnusedEntity;

// extend end

// A mempool is a linked list of memory blocks, each of which begins with this header structure.
// The initial block in the linked list is special, and tracks additional information.
struct _MonoMemPool {
	// Next block after this one in linked list
	MonoMemPool *next;

	// Size of this memory block only
	guint32 size;

	// Used in "initial block" only: Beginning of current free space in mempool (may be in some block other than the first one)
	guint8 *pos;

	// Used in "initial block" only: End of current free space in mempool (ie, the first byte following the end of usable space)
	guint8 *end;

	// extend by dsqiu
	// empty->empty->unused_memory->unused_memory
	MonoUnusedEntity* unuseds;
	// extend end

	union {
		// Unused: Imposing floating point memory rules on _MonoMemPool's final field ensures proper alignment of whole header struct
		double pad;

		// Used in "initial block" only: Number of bytes so far allocated (whether used or not) in the whole mempool
		guint32 allocated;
	} d;
};

static gint64 total_bytes_allocated = 0;

// extend by dsqiu
static void
mono_mempool_unused_destroy(MonoUnusedEntity* unused_entity)
{
	MonoUnusedEntity *p, *n;
	p = unused_entity;
	while (p) {
		n = p->next;
		g_free(p);
		p = n;
	}
}

static MonoUnusedEntity*
mono_mempool_unused_new()
{
	MonoUnusedEntity* unused_entity = (MonoUnusedEntity *)g_malloc(sizeof(MonoUnusedEntity));
	unused_entity->pos = NULL;
	unused_entity->size = 0;
	unused_entity->next = NULL;
	return unused_entity;
}
static void
mono_mempool_unused_recycle(MonoMemPool* root, MonoUnusedEntity* reuse_entity)
{
	reuse_entity->pos = NULL;
	reuse_entity->size = 0;
	MonoUnusedEntity* unused_list = root->unuseds;
	while (unused_list)
	{
		if (unused_list->next == reuse_entity)
		{
			unused_list->next = reuse_entity->next;
			reuse_entity->next = NULL;
			break;
		}
		unused_list = unused_list->next;
	}
	reuse_entity->next = root->unuseds->next;
	root->unuseds = reuse_entity;
}

// insert free memory to unuseds
static gboolean
mono_mempool_unused_insert(MonoMemPool* root, guint8* addr, gint32 size, MonoMemPool* pool)
{
	MonoUnusedEntity* new_entity = NULL;
	if (!root->unuseds)
	{
		root->unuseds = mono_mempool_unused_new();
		root->unuseds->pos = addr;
		root->unuseds->size = size;
		return TRUE;
	}
	gpointer pool_end = (gpointer)((guint8*)pool + pool->size);
	gpointer pool_start = (gpointer)(pool);
	MonoUnusedEntity* unused_list = root->unuseds;
	// check if has adjacent entity, join
	while (unused_list)
	{
		gpointer pre_addr = (gpointer)unused_list;
		if (pre_addr >= pool_start && pre_addr < pool_end)
		{
			if (addr == unused_list->pos)
			{
				g_print("Free memory repeatly\n");
				return FALSE;
			}
			guint8* a = (guint8*)unused_list + unused_list->size;
			guint8* b = addr + size;
			if ((guint8*)unused_list + unused_list->size == addr)
			{
				unused_list->size += size;
				// check next entity if adjacent
				MonoUnusedEntity* next_entity = unused_list->next;
				if (next_entity && ((guint8*)(next_entity->pos) == (guint8*)(unused_list->pos) + unused_list->size))
				{
					unused_list->size += next_entity->size;
					mono_mempool_unused_recycle(root, next_entity);
				}
				return TRUE;
			}
			else if (addr + size == pre_addr)
			{
				unused_list->pos = addr;
				unused_list->size += size;
				return TRUE;
			}
		}
		unused_list = unused_list->next;
	}
	if (root->unuseds->pos)
	{
		new_entity = mono_mempool_unused_new();
	}
	else
	{
		new_entity = root->unuseds;
		root->unuseds = new_entity->next;
		new_entity->next = NULL;
	}
	new_entity->size = size;
	new_entity->pos = addr;
	unused_list = root->unuseds;
	gboolean has_same_pool = FALSE;
	MonoUnusedEntity* pre_entity = NULL;
	while (unused_list)
	{
		gpointer pre_addr = (gpointer)unused_list;
		if (pre_addr >= pool_start && pre_addr < pool_end)
		{
			has_same_pool = TRUE;
			if (pre_addr > (gpointer)addr)
			{
				break;
			}
			else
			{
				pre_entity = unused_list;
			}
		}
		else if (has_same_pool)
		{
			break;
		}
		else // last
		{
			pre_entity = unused_list;
		}
		unused_list = unused_list->next;
	}
	if (pre_entity)
	{
		new_entity->next = pre_entity->next;
		pre_entity->next = new_entity;
	}
	else
	{
		root->unuseds = new_entity;
	}
	return TRUE;
}


static gpointer
mono_mempool_unused_fetch(MonoMemPool* root, guint32 size)
{
	MonoUnusedEntity* unused_list = root->unuseds;
	MonoUnusedEntity* pre_entity = NULL;
	MonoUnusedEntity* reuse_entity = NULL;
	guint32 resue_size = MONO_MEMPOOL_PREFER_INDIVIDUAL_ALLOCATION_SIZE + 1;
	while (unused_list)
	{
		if (unused_list->pos)
		{
			if (unused_list->size == size)
			{
				reuse_entity = unused_list;
				break;
			}
			else if (unused_list->size > size && resue_size > unused_list->size)
			{
				resue_size = unused_list->size;
				reuse_entity = unused_list;
			}
		}
		pre_entity = unused_list;
		unused_list = unused_list->next;
	}
	if (reuse_entity)
	{
		gpointer rval = reuse_entity->pos;
		if (reuse_entity->size != size)
		{
			reuse_entity->pos = reuse_entity->pos + size;
			reuse_entity->size -= size;
		}
		else  // remove from list and insert header
		{
			if (pre_entity)
			{
				pre_entity->next = reuse_entity->next;
			}
			reuse_entity->pos = NULL;
			reuse_entity->size = 0;
			reuse_entity->next = root->unuseds->next;
			root->unuseds = reuse_entity;
		}
		return rval;
	}
	return NULL;
}

// extend end

/**
 * mono_mempool_new:
 *
 * Returns: a new memory pool.
 */
MonoMemPool *
mono_mempool_new (void)
{
	return mono_mempool_new_size (MONO_MEMPOOL_PAGESIZE);
}

/**
 * mono_mempool_new_size:
 * \param initial_size the amount of memory to initially reserve for the memory pool.
 * \returns a new memory pool with a specific initial memory reservation.
 */
MonoMemPool *
mono_mempool_new_size (int initial_size)
{
	MonoMemPool *pool;

#ifdef INDIVIDUAL_ALLOCATIONS
	// In individual allocation mode, create initial block with zero storage space.
	initial_size = SIZEOF_MEM_POOL;
#else
	if (initial_size < MONO_MEMPOOL_MINSIZE)
		initial_size = MONO_MEMPOOL_MINSIZE;
#endif

	pool = (MonoMemPool *)g_malloc (initial_size);

	pool->next = NULL;
	pool->pos = (guint8*)pool + SIZEOF_MEM_POOL; // Start after header
	pool->end = (guint8*)pool + initial_size;    // End at end of allocated space 
	pool->d.allocated = pool->size = initial_size;
	pool->unuseds = NULL;
	UnlockedAdd64 (&total_bytes_allocated, initial_size);
	return pool;
}

/**
 * mono_mempool_destroy:
 * \param pool the memory pool to destroy
 *
 * Free all memory associated with this pool.
 */
void
mono_mempool_destroy (MonoMemPool *pool)
{
	// extend by dsqiu
	mono_mempool_unused_destroy(pool->unuseds);
	// extend end

	MonoMemPool *p, *n;

	UnlockedSubtract64 (&total_bytes_allocated, pool->d.allocated);

	p = pool;
	while (p) {
		n = p->next;
		g_free (p);
		p = n;
	}
}

/**
 * mono_mempool_invalidate:
 * \param pool the memory pool to invalidate
 *
 * Fill the memory associated with this pool to 0x2a (42). Useful for debugging.
 */
void
mono_mempool_invalidate (MonoMemPool *pool)
{
	MonoMemPool *p, *n;

	p = pool;
	while (p) {
		n = p->next;
		memset (p, 42, p->size);
		p = n;
	}
}

/**
 * mono_mempool_stats:
 * \param pool the memory pool we need stats for
 *
 * Print a few stats about the mempool:
 * - Total memory allocated (malloced) by mem pool
 * - Number of chunks/blocks memory is allocated in
 * - How much memory is available to dispense before a new malloc must occur?
 */
void
mono_mempool_stats (MonoMemPool *pool)
{
	MonoMemPool *p;
	int count = 0;
	guint32 still_free;

	p = pool;
	while (p) {
		p = p->next;
		count++;
	}
	if (pool) {
		still_free = pool->end - pool->pos;
		g_print ("Mempool %p stats:\n", pool);
		g_print ("Total mem allocated: %d\n", pool->d.allocated);
		g_print ("Num chunks: %d\n", count);
		g_print ("Free memory: %d\n", still_free);
	}
}

#ifdef TRACE_ALLOCATIONS
#include <execinfo.h>
#include "metadata/appdomain.h"
#include "metadata/metadata-internals.h"

static mono_mutex_t mempool_tracing_lock;
#define BACKTRACE_DEPTH 7
static void
mono_backtrace (int size)
{
	void *array[BACKTRACE_DEPTH];
	char **names;
	int i, symbols;
	static gboolean inited;

	if (!inited) {
		mono_os_mutex_init_recursive (&mempool_tracing_lock);
		inited = TRUE;
	}

	mono_os_mutex_lock (&mempool_tracing_lock);
	g_print ("Allocating %d bytes\n", size);
	MONO_ENTER_GC_SAFE;
	symbols = backtrace (array, BACKTRACE_DEPTH);
	names = backtrace_symbols (array, symbols);
	MONO_EXIT_GC_SAFE;
	for (i = 1; i < symbols; ++i) {
		g_print ("\t%s\n", names [i]);
	}
	g_free (names);
	mono_os_mutex_unlock (&mempool_tracing_lock);
}

#endif

/**
 * get_next_size:
 * @pool: the memory pool to use
 * @size: size of the memory entity we are trying to allocate
 *
 * A mempool is growing; give a recommended size for the next block.
 * Each block in a mempool should be about 150% bigger than the previous one,
 * or bigger if it is necessary to include the new entity.
 *
 * Returns: the recommended size.
 */
static guint
get_next_size (MonoMemPool *pool, int size)
{
	int target = pool->next? pool->next->size: pool->size;
	size += SIZEOF_MEM_POOL;
	/* increase the size */
	target += target / 2;
	while (target < size) {
		target += target / 2;
	}
	if (target > MONO_MEMPOOL_PAGESIZE && size <= MONO_MEMPOOL_PAGESIZE)
		target = MONO_MEMPOOL_PAGESIZE;
	return target;
}

/**
 * mono_mempool_alloc:
 * \param pool the memory pool to use
 * \param size size of the memory block
 *
 * Allocates a new block of memory in \p pool .
 *
 * \returns the address of a newly allocated memory block.
 */
gpointer
(mono_mempool_alloc) (MonoMemPool *pool, guint size)
{
	gpointer rval = pool->pos; // Return value

	// Normal case: Just bump up pos pointer and we are done
	size = ALIGN_SIZE (size);
	pool->pos = (guint8*)rval + size;

#ifdef TRACE_ALLOCATIONS
	if (pool == mono_get_corlib ()->mempool) {
		mono_backtrace (size);
	}
#endif

	// If we have just overflowed the current block, we need to back up and try again.
	if (G_UNLIKELY (pool->pos >= pool->end)) {
		pool->pos -= size;  // Back out

		// extend by dsqiu
		rval = mono_mempool_unused_fetch(pool, size);
		if (rval)
			return rval;
		// extend end

		// For large objects, allocate the object into its own block.
		// (In individual allocation mode, the constant will be 0 and this path will always be taken)
		if (size >= MONO_MEMPOOL_PREFER_INDIVIDUAL_ALLOCATION_SIZE) {
			guint new_size = SIZEOF_MEM_POOL + size;
			MonoMemPool *np = (MonoMemPool *)g_malloc (new_size);

			np->next = pool->next;
			np->size = new_size;
			pool->next = np;
			pool->d.allocated += new_size;
			UnlockedAdd64 (&total_bytes_allocated, new_size);

			rval = (guint8*)np + SIZEOF_MEM_POOL;
		} else {
			// Notice: any unused memory at the end of the old head becomes simply abandoned in this case until the mempool is freed (see Bugzilla #35136)
			guint new_size = get_next_size (pool, size);
			MonoMemPool *np = (MonoMemPool *)g_malloc (new_size);

			np->next = pool->next;
			np->size = new_size;
			pool->next = np;
			pool->pos = (guint8*)np + SIZEOF_MEM_POOL;
			pool->end = (guint8*)np + new_size;
			pool->d.allocated += new_size;
			UnlockedAdd64 (&total_bytes_allocated, new_size);

			rval = pool->pos;
			pool->pos += size;
		}
	}

	return rval;
}

/**
 * mono_mempool_alloc0:
 *
 * same as \c mono_mempool_alloc, but fills memory with zero.
 */
gpointer
(mono_mempool_alloc0) (MonoMemPool *pool, guint size)
{
	gpointer rval;

	// For the fast path, repeat the first few lines of mono_mempool_alloc
	size = ALIGN_SIZE (size);
	rval = pool->pos;
	pool->pos = (guint8*)rval + size;

	// If that doesn't work fall back on mono_mempool_alloc to handle new chunk allocation
	if (G_UNLIKELY (pool->pos >= pool->end)) {
		// extend by dsqiu
		// bug?
		pool->pos -= size; // Back out
		// extend end
		rval = mono_mempool_alloc (pool, size);
	}
#ifdef TRACE_ALLOCATIONS
	else if (pool == mono_get_corlib ()->mempool) {
		mono_backtrace (size);
	}
#endif

	memset (rval, 0, size);
	return rval;
}

/**
 * mono_mempool_contains_addr:
 *
 * Determines whether \p addr is inside the memory used by the mempool.
 */
gboolean
mono_mempool_contains_addr (MonoMemPool *pool,
							gpointer addr)
{
	MonoMemPool *p = pool;

	while (p) {
		if (addr >= (gpointer)p && addr < (gpointer)((guint8*)p + p->size))
			return TRUE;
		p = p->next;
	}

	return FALSE;
}

/**
 * mono_mempool_strdup:
 *
 * Same as strdup, but allocates memory from the mempool.
 * Returns: a pointer to the newly allocated string data inside the mempool.
 */
char*
mono_mempool_strdup (MonoMemPool *pool,
					 const char *s)
{
	int l;
	char *res;

	if (s == NULL)
		return NULL;

	l = strlen (s);
	res = (char *)mono_mempool_alloc (pool, l + 1);
	memcpy (res, s, l + 1);

	return res;
}

char*
mono_mempool_strdup_vprintf (MonoMemPool *pool, const char *format, va_list args)
{
	size_t buflen;
	char *buf;
	va_list args2;
	va_copy (args2, args);
	int len = vsnprintf (NULL, 0, format, args2);
	va_end (args2);

	if (len >= 0 && (buf = (char*)mono_mempool_alloc (pool, (buflen = (size_t) (len + 1)))) != NULL) {
		vsnprintf (buf, buflen, format, args);
	} else {
		buf = NULL;
	}
	return buf;
}

char*
mono_mempool_strdup_printf (MonoMemPool *pool, const char *format, ...)
{
	char *buf;
	va_list args;
	va_start (args, format);
	buf = mono_mempool_strdup_vprintf (pool, format, args);
	va_end (args);
	return buf;
}

/**
 * mono_mempool_get_allocated:
 *
 * Return the amount of memory allocated for this mempool.
 */
guint32
mono_mempool_get_allocated (MonoMemPool *pool)
{
	return pool->d.allocated;
}

/**
 * mono_mempool_get_bytes_allocated:
 *
 * Return the number of bytes currently allocated for mempools.
 */
long
mono_mempool_get_bytes_allocated (void)
{
	return UnlockedRead64 (&total_bytes_allocated);
}


// extend by dsqiu
mono_bool
mono_mempool_free(MonoMemPool* pool, void* addr, uint32_t size)
{
	MonoMemPool *p = pool;
	MonoMemPool *start = NULL;
	while (p) {
		if (addr >= (gpointer)p && addr < (gpointer)((guint8*)p + p->size))
		{
			start = p;
			break;
		}
		p = p->next;
	}
	if (!start)
		return FALSE;
	size = ALIGN_SIZE(size);
	
	return mono_mempool_unused_insert(pool, addr, size, start);
}
// extend end