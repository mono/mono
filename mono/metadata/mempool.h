#ifndef _MONO_MEMPOOL_H_
#define _MONO_MEMPOOL_H_

#include <string.h>

G_BEGIN_DECLS

typedef struct _MonoMemPool MonoMemPool;

MonoMemPool *
mono_mempool_new           (void);

void
mono_mempool_destroy       (MonoMemPool *pool);

void
mono_mempool_invalidate    (MonoMemPool *pool);

void
mono_mempool_empty         (MonoMemPool *pool);

void
mono_mempool_stats         (MonoMemPool *pool);

gpointer
mono_mempool_alloc         (MonoMemPool *pool, 
			    guint        size);

gpointer
mono_mempool_alloc0 (MonoMemPool *pool, guint size);

gboolean
mono_mempool_contains_addr (MonoMemPool *pool,
			    gpointer addr);

gpointer
mono_mempool_alloc_inner         (MonoMemPool *pool, 
			    guint        size);
/*
extern inline gpointer
mono_mempool_alloc0 (MonoMemPool *pool, guint size)
{
	gpointer rval = mono_mempool_alloc (pool, size);
	memset (rval, 0, size);
	return rval;
}	
*/
G_END_DECLS

#endif
