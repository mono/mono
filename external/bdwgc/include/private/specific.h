/*
 * This is a reimplementation of a subset of the pthread_getspecific/setspecific
 * interface. This appears to outperform the standard linuxthreads one
 * by a significant margin.
 * The major restriction is that each thread may only make a single
 * pthread_setspecific call on a single key.  (The current data structure
 * doesn't really require that.  The restriction should be easily removable.)
 * We don't currently support the destruction functions, though that
 * could be done.
 * We also currently assume that only one pthread_setspecific call
 * can be executed at a time, though that assumption would be easy to remove
 * by adding a lock.
 */

#include <errno.h>

EXTERN_C_BEGIN

/* Called during key creation or setspecific.           */
/* For the GC we already hold lock.                     */
/* Currently allocated objects leak on thread exit.     */
/* That's hard to fix, but OK if we allocate garbage    */
/* collected memory.                                    */
#define MALLOC_CLEAR(n) GC_INTERNAL_MALLOC(n, NORMAL)

#define TS_CACHE_SIZE 1024
#define CACHE_HASH(n) ((((n) >> 8) ^ (n)) & (TS_CACHE_SIZE - 1))

#define TS_HASH_SIZE 1024
#define HASH(p) \
          ((unsigned)((((word)(p)) >> 8) ^ (word)(p)) & (TS_HASH_SIZE - 1))

/* An entry describing a thread-specific value for a given thread.      */
/* All such accessible structures preserve the invariant that if either */
/* thread is a valid pthread id or qtid is a valid "quick thread id"    */
/* for a thread, then value holds the corresponding thread specific     */
/* value.  This invariant must be preserved at ALL times, since         */
/* asynchronous reads are allowed.                                      */
typedef struct thread_specific_entry {
        volatile AO_t qtid;     /* quick thread id, only for cache */
        void * value;
        struct thread_specific_entry *next;
        pthread_t thread;
} tse;

/* We represent each thread-specific datum as two tables.  The first is */
/* a cache, indexed by a "quick thread identifier".  The "quick" thread */
/* identifier is an easy to compute value, which is guaranteed to       */
/* determine the thread, though a thread may correspond to more than    */
/* one value.  We typically use the address of a page in the stack.     */
/* The second is a hash table, indexed by pthread_self().  It is used   */
/* only as a backup.                                                    */

/* Return the "quick thread id".  Default version.  Assumes page size,  */
/* or at least thread stack separation, is at least 4K.                 */
/* Must be defined so that it never returns 0.  (Page 0 can't really be */
/* part of any stack, since that would make 0 a valid stack pointer.)   */
#define quick_thread_id() (((word)GC_approx_sp()) >> 12)

#define INVALID_QTID ((word)0)
#define INVALID_THREADID ((pthread_t)0)

union ptse_ao_u {
  tse *p;
  volatile AO_t ao;
};

typedef struct thread_specific_data {
    tse * volatile cache[TS_CACHE_SIZE];
                        /* A faster index to the hash table */
    union ptse_ao_u hash[TS_HASH_SIZE];
    pthread_mutex_t lock;
} tsd;

typedef tsd * GC_key_t;

#define GC_key_create(key, d) GC_key_create_inner(key)
GC_INNER int GC_key_create_inner(tsd ** key_ptr);
GC_INNER int GC_setspecific(tsd * key, void * value);
#define GC_remove_specific(key) \
                        GC_remove_specific_after_fork(key, pthread_self())
GC_INNER void GC_remove_specific_after_fork(tsd * key, pthread_t t);

/* An internal version of getspecific that assumes a cache miss.        */
GC_INNER void * GC_slow_getspecific(tsd * key, word qtid,
                                    tse * volatile * cache_entry);

/* GC_INLINE is defined in gc_priv.h. */
GC_INLINE void * GC_getspecific(tsd * key)
{
    word qtid = quick_thread_id();
    tse * volatile * entry_ptr = &key->cache[CACHE_HASH(qtid)];
    tse * entry = *entry_ptr;   /* Must be loaded only once.    */

    GC_ASSERT(qtid != INVALID_QTID);
    if (EXPECT(entry -> qtid == qtid, TRUE)) {
      GC_ASSERT(entry -> thread == pthread_self());
      return entry -> value;
    }
    return GC_slow_getspecific(key, qtid, entry_ptr);
}

EXTERN_C_END
