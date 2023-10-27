/*
 * Copyright (c) 2000 by Hewlett-Packard Company.  All rights reserved.
 *
 * THIS MATERIAL IS PROVIDED AS IS, WITH ABSOLUTELY NO WARRANTY EXPRESSED
 * OR IMPLIED.  ANY USE IS AT YOUR OWN RISK.
 *
 * Permission is hereby granted to use or copy this program
 * for any purpose,  provided the above notices are retained on all copies.
 * Permission to modify the code and to distribute modified code is granted,
 * provided the above notices are retained, and a notice that the code was
 * modified is included with the above copyright notice.
 */

#include "private/thread_local_alloc.h"
                /* To determine type of tsd impl.       */
                /* Includes private/specific.h          */
                /* if needed.                           */

#if defined(USE_CUSTOM_SPECIFIC)

static const tse invalid_tse = {INVALID_QTID, 0, 0, INVALID_THREADID};
            /* A thread-specific data entry which will never    */
            /* appear valid to a reader.  Used to fill in empty */
            /* cache entries to avoid a check for 0.            */

GC_INNER int GC_key_create_inner(tsd ** key_ptr)
{
    int i;
    int ret;
    tsd * result;

    GC_ASSERT(I_HOLD_LOCK());
    /* A quick alignment check, since we need atomic stores */
    GC_ASSERT((word)(&invalid_tse.next) % sizeof(tse *) == 0);
    result = (tsd *)MALLOC_CLEAR(sizeof(tsd));
    if (NULL == result) return ENOMEM;
    ret = pthread_mutex_init(&result->lock, NULL);
    if (ret != 0) return ret;
    for (i = 0; i < TS_CACHE_SIZE; ++i) {
      result -> cache[i] = (/* no const */ tse *)&invalid_tse;
    }
#   ifdef GC_ASSERTIONS
      for (i = 0; i < TS_HASH_SIZE; ++i) {
        GC_ASSERT(result -> hash[i].p == 0);
      }
#   endif
    *key_ptr = result;
    return 0;
}

GC_INNER int GC_setspecific(tsd * key, void * value)
{
    pthread_t self = pthread_self();
    int hash_val = HASH(self);
    volatile tse * entry;

    GC_ASSERT(I_HOLD_LOCK());
    GC_ASSERT(self != INVALID_THREADID);
    GC_dont_gc++; /* disable GC */
    entry = (volatile tse *)MALLOC_CLEAR(sizeof(tse));
    GC_dont_gc--;
    if (0 == entry) return ENOMEM;

    pthread_mutex_lock(&(key -> lock));
    /* Could easily check for an existing entry here.   */
    entry -> next = key->hash[hash_val].p;
    entry -> thread = self;
    entry -> value = value;
    GC_ASSERT(entry -> qtid == INVALID_QTID);
    /* There can only be one writer at a time, but this needs to be     */
    /* atomic with respect to concurrent readers.                       */
    AO_store_release(&key->hash[hash_val].ao, (AO_t)entry);
    GC_dirty((/* no volatile */ void *)entry);
    GC_dirty(key->hash + hash_val);
    pthread_mutex_unlock(&(key -> lock));
    return 0;
}

/* Remove thread-specific data for a given thread.  This function is    */
/* called at fork from the child process for all threads except for the */
/* survived one.  GC_remove_specific() should be called on thread exit. */
GC_INNER void GC_remove_specific_after_fork(tsd * key, pthread_t t)
{
    unsigned hash_val = HASH(t);
    tse *entry;
    tse *prev = NULL;

#   ifdef CAN_HANDLE_FORK
      /* Both GC_setspecific and GC_remove_specific should be called    */
      /* with the allocation lock held to ensure the consistency of     */
      /* the hash table in the forked child.                            */
      GC_ASSERT(I_HOLD_LOCK());
#   endif
    pthread_mutex_lock(&(key -> lock));
    entry = key->hash[hash_val].p;
    while (entry != NULL && !THREAD_EQUAL(entry->thread, t)) {
      prev = entry;
      entry = entry->next;
    }
    /* Invalidate qtid field, since qtids may be reused, and a later    */
    /* cache lookup could otherwise find this entry.                    */
    if (entry != NULL) {
      entry -> qtid = INVALID_QTID;
      if (NULL == prev) {
        key->hash[hash_val].p = entry->next;
        GC_dirty(key->hash + hash_val);
      } else {
        prev->next = entry->next;
        GC_dirty(prev);
      }
      /* Atomic! concurrent accesses still work.        */
      /* They must, since readers don't lock.           */
      /* We shouldn't need a volatile access here,      */
      /* since both this and the preceding write        */
      /* should become visible no later than            */
      /* the pthread_mutex_unlock() call.               */
    }
    /* If we wanted to deallocate the entry, we'd first have to clear   */
    /* any cache entries pointing to it.  That probably requires        */
    /* additional synchronization, since we can't prevent a concurrent  */
    /* cache lookup, which should still be examining deallocated memory.*/
    /* This can only happen if the concurrent access is from another    */
    /* thread, and hence has missed the cache, but still...             */
#   ifdef LINT2
      GC_noop1((word)entry);
#   endif

    /* With GC, we're done, since the pointers from the cache will      */
    /* be overwritten, all local pointers to the entries will be        */
    /* dropped, and the entry will then be reclaimed.                   */
    pthread_mutex_unlock(&(key -> lock));
}

/* Note that even the slow path doesn't lock.   */
GC_INNER void * GC_slow_getspecific(tsd * key, word qtid,
                                    tse * volatile * cache_ptr)
{
    pthread_t self = pthread_self();
    unsigned hash_val = HASH(self);
    tse *entry = key->hash[hash_val].p;

    GC_ASSERT(qtid != INVALID_QTID);
    while (entry != NULL && !THREAD_EQUAL(entry->thread, self)) {
      entry = entry -> next;
    }
    if (entry == NULL) return NULL;
    /* Set cache_entry. */
    entry -> qtid = (AO_t)qtid;
        /* It's safe to do this asynchronously.  Either value   */
        /* is safe, though may produce spurious misses.         */
        /* We're replacing one qtid with another one for the    */
        /* same thread.                                         */
    *cache_ptr = entry;
        /* Again this is safe since pointer assignments are     */
        /* presumed atomic, and either pointer is valid.        */
    return entry -> value;
}

#ifdef GC_ASSERTIONS
  /* Check that that all elements of the data structure associated  */
  /* with key are marked.                                           */
  void GC_check_tsd_marks(tsd *key)
  {
    int i;
    tse *p;

    if (!GC_is_marked(GC_base(key))) {
      ABORT("Unmarked thread-specific-data table");
    }
    for (i = 0; i < TS_HASH_SIZE; ++i) {
      for (p = key->hash[i].p; p != 0; p = p -> next) {
        if (!GC_is_marked(GC_base(p))) {
          ABORT_ARG1("Unmarked thread-specific-data entry",
                     " at %p", (void *)p);
        }
      }
    }
    for (i = 0; i < TS_CACHE_SIZE; ++i) {
      p = key -> cache[i];
      if (p != &invalid_tse && !GC_is_marked(GC_base(p))) {
        ABORT_ARG1("Unmarked cached thread-specific-data entry",
                   " at %p", (void *)p);
      }
    }
  }
#endif /* GC_ASSERTIONS */

#endif /* USE_CUSTOM_SPECIFIC */
