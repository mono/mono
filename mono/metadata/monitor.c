/*
 * monitor.c:  Monitor locking functions
 *
 * Author:
 *		Dick Porter (dick@ximian.com)
 *  	Duarte Nunes (duarte.m.nunes@gmail.com)
 *
 * Copyright 2003 Ximian, Inc (http://www.ximian.com)
 * Copyright 2004-2009 Novell, Inc (http://www.novell.com)
 */

#include <config.h>
#include <glib.h>
#include <string.h>

#include <mono/io-layer/io-layer.h>
#include <mono/metadata/monitor.h>
#include <mono/metadata/threads-types.h>
#include <mono/metadata/exception.h>
#include <mono/metadata/threads.h>
#include <mono/metadata/object-internals.h>
#include <mono/metadata/class-internals.h>
#include <mono/metadata/gc-internal.h>
#include <mono/metadata/method-builder.h>
#include <mono/metadata/debug-helpers.h>
#include <mono/metadata/tabledefs.h>
#include <mono/metadata/marshal.h>
#include <mono/metadata/profiler-private.h>
#include <mono/utils/mono-time.h>
#include <mono/utils/mono-threads.h>

/*
 * Pull the list of opcodes
 */
#define OPDEF(a,b,c,d,e,f,g,h,i,j) \
	a = i,

enum {
#include "mono/cil/opcode.def"
	LAST = 0xff
};
#undef OPDEF

/* #define LOCK_DEBUG(a) do { a; } while (0) */
#define LOCK_DEBUG(a)

/*
 * The monitor implementation here is based on
 * http://www.research.ibm.com/people/d/dfb/papers/Bacon98Thin.ps and
 * http://www.research.ibm.com/trl/projects/jit/paper/oopsla99_onodera.pdf
 * 
 * Bacon's thin locks have a fast path that doesn't need a lock record
 * for the common case of locking an unlocked or shallow-nested object.
 * When the fast path fails it means there is contention (or the header
 * contains the object's hash code) and so the thread must inflate the
 * monitor by creating a lock record and placing its address in the
 * synchronization header of the object once the lock is free.
 *
 * To inflate the monitor we use a temporary mapping from the MonoObject,
 * which is pinned, to the MonoThreadsSync that the object will point to.
 * This table is only required during inflation and allows the acquiring 
 * threads to block waiting for the lock to be released.
 */

/*
 * Lock word format:
 *
 * 32-bit
 *		LOCK_WORD_FLAT:			[owner:22 | nest:8 | status:2]
 *    LOCK_WORD_THIN_HASH:	   [      hash:30     | status:2]
 *		LOCK_WORD_INFLATED:		[    sync_ptr:30   | status:2]
 *
 * 64-bit
 *		LOCK_WORD_FLAT:			[unused:22 | owner:32 | nest:8 | status:2]
 *    LOCK_WORD_THIN_HASH:	   [            hash:62           | status:2]
 *		LOCK_WORD_INFLATED:		[          sync_ptr:62         | status:2]
 *
 * We assume that the two least significant bits of a MonoThreadsSync * are always zero.
 */

typedef union {
	gsize lock_word;
	volatile MonoThreadsSync *sync; /* *volatile* qualifier used just to remove some warnings. */
} LockWord;

enum {
	LOCK_WORD_FLAT = 0,
	LOCK_WORD_THIN_HASH = 1,
	LOCK_WORD_INFLATED = 2,
	LOCK_WORD_FAT_HASH = 3,
	
	LOCK_WORD_STATUS_BITS = 2,
	LOCK_WORD_BITS_MASK = (1 << LOCK_WORD_STATUS_BITS) - 1,
	
	LOCK_WORD_NEST_BITS = 8,
	LOCK_WORD_NEST_SHIFT = LOCK_WORD_STATUS_BITS,
   LOCK_WORD_NEST_MASK = ((1 << LOCK_WORD_NEST_BITS) - 1) << LOCK_WORD_NEST_SHIFT,
	
	LOCK_WORD_OWNER_SHIFT = LOCK_WORD_NEST_SHIFT + LOCK_WORD_NEST_BITS,

	LOCK_WORD_HASH_SHIFT = LOCK_WORD_STATUS_BITS
};

struct _MonoThreadsSync
{
	int owner;			/* thread ID */
	guint32 nest;
#ifdef HAVE_MOVING_COLLECTOR
	gint32 hash_code;
#endif
	volatile guint32 entry_count;
	HANDLE entry_sem;
	GSList *wait_list;
	void *data;
};

typedef struct _MonitorArray MonitorArray;

struct _MonitorArray {
	MonitorArray *next;
	int num_monitors;
	MonoThreadsSync monitors [MONO_ZERO_LEN_ARRAY];
};

#define mono_monitor_allocator_lock() EnterCriticalSection (&monitor_mutex)
#define mono_monitor_allocator_unlock() LeaveCriticalSection (&monitor_mutex)
static CRITICAL_SECTION monitor_mutex;
static GHashTable *monitor_table;
static MonoThreadsSync *monitor_freelist;
static MonitorArray *monitor_allocated;
static int array_size = 16;

#define MONO_OBJECT_ALIGNMENT_SHIFT	3

void
mono_monitor_init (void)
{
	InitializeCriticalSection (&monitor_mutex);
	monitor_table = g_hash_table_new (NULL, NULL);
}
 
void
mono_monitor_cleanup (void)
{
	MonoThreadsSync *mon;
	/* MonitorArray *marray, *next = NULL; */

	/*DeleteCriticalSection (&monitor_mutex);*/

	/*g_hash_table_destroy (monitor_table);*/

	/* The monitors on the freelist don't have weak links - mark them */
	for (mon = monitor_freelist; mon; mon = mon->data)
		mon->wait_list = (gpointer)-1;

	/* FIXME: This still crashes with sgen (async_read.exe) */
	/*
	for (marray = monitor_allocated; marray; marray = next) {
		int i;

		for (i = 0; i < marray->num_monitors; ++i) {
			mon = &marray->monitors [i];
			if (mon->wait_list != (gpointer)-1)
				mono_gc_weak_link_remove (&mon->data);
		}

		next = marray->next;
		g_free (marray);
	}
	*/
}

/*
 * mono_monitor_init_tls:
 *
 *   Setup TLS variables used by the monitor code for the current thread.
 */
void
mono_monitor_init_tls (void)
{ }

static int
monitor_is_on_freelist (MonoThreadsSync *mon)
{
	MonitorArray *marray;
	for (marray = monitor_allocated; marray; marray = marray->next) {
		if (mon >= marray->monitors && mon < &marray->monitors [marray->num_monitors])
			return TRUE;
	}
	return FALSE;
}

/**
 * mono_locks_dump:
 * @include_untaken:
 *
 * Print a report on stdout of the managed locks currently held by
 * threads. If @include_untaken is specified, list also inflated locks
 * which are unheld.
 * This is supposed to be used in debuggers like gdb.
 */
void
mono_locks_dump (gboolean include_untaken)
{
	int i;
	int used = 0, on_freelist = 0, to_recycle = 0, total = 0, num_arrays = 0;
	MonoThreadsSync *mon;
	MonitorArray *marray;
	for (mon = monitor_freelist; mon; mon = mon->data)
		on_freelist++;
	for (marray = monitor_allocated; marray; marray = marray->next) {
		total += marray->num_monitors;
		num_arrays++;
		for (i = 0; i < marray->num_monitors; ++i) {
			mon = &marray->monitors [i];
			if (mon->data == NULL) {
				if (i < marray->num_monitors - 1)
					to_recycle++;
			} else {
				if (!monitor_is_on_freelist ((MonoThreadsSync *) mon->data)) {
					MonoObject *holder = mono_gc_weak_link_get (&mon->data);
					if (mon->owner) {
						g_print ("Lock %p in object %p held by thread %p, nest level: %d\n",
							mon, holder, (void*)mon->owner, mon->nest);
						if (mon->entry_sem)
							g_print ("\tWaiting on semaphore %p: %d\n", mon->entry_sem, mon->entry_count);
					} else if (include_untaken) {
						g_print ("Lock %p in object %p untaken\n", mon, holder);
					}
					used++;
				}
			}
		}
	}
	g_print ("Total locks (in %d array(s)): %d, used: %d, on freelist: %d, to recycle: %d\n",
		num_arrays, total, used, on_freelist, to_recycle);
}

/* LOCKING: this is called with monitor_mutex held */
static void 
mon_finalize (MonoThreadsSync *mon)
{
	LOCK_DEBUG (g_message ("%s: Finalizing sync %p", __func__, mon));

	if (mon->entry_sem != NULL) {
		CloseHandle (mon->entry_sem);
		mon->entry_sem = NULL;
	}
	/* If this isn't empty then something is seriously broken - it
	 * means a thread is still waiting on the object that owned
	 * this lock, but the object has been finalized.
	 */
	g_assert (mon->wait_list == NULL);

	mon->entry_count = 0;
	/* owner and nest are set in mon_new, no need to zero them out */

	mon->data = monitor_freelist;
	monitor_freelist = mon;
	mono_perfcounters->gc_sync_blocks--;
}

/* LOCKING: this is called with monitor_mutex held */
static MonoThreadsSync *
mon_new (int id)
{
	MonoThreadsSync *new;

	if (!monitor_freelist) {
		MonitorArray *marray;
		int i;
		/* see if any sync block has been collected */
		new = NULL;
		for (marray = monitor_allocated; marray; marray = marray->next) {
			for (i = 0; i < marray->num_monitors; ++i) {
				if (marray->monitors [i].data == NULL) {
					new = &marray->monitors [i];
					if (new->wait_list) {
						/* Orphaned events left by aborted threads */
						while (new->wait_list) {
							LOCK_DEBUG (g_message (G_GNUC_PRETTY_FUNCTION ": (%d): Closing orphaned event %d", mono_thread_info_get_small_id (), new->wait_list->data));
							CloseHandle (new->wait_list->data);
							new->wait_list = g_slist_remove (new->wait_list, new->wait_list->data);
						}
					}
					/*mono_gc_weak_link_remove (&new->data);*/
					new->data = monitor_freelist;
					monitor_freelist = new;
				}
			}
			/* small perf tweak to avoid scanning all the blocks */
			if (new)
				break;
		}
		/* need to allocate a new array of monitors */
		if (!monitor_freelist) {
			MonitorArray *last;
			LOCK_DEBUG (g_message ("%s: allocating more monitors: %d", __func__, array_size));
			marray = g_malloc0 (sizeof (MonoArray) + array_size * sizeof (MonoThreadsSync));
			marray->num_monitors = array_size;
			array_size *= 2;
			/* link into the freelist */
			for (i = 0; i < marray->num_monitors - 1; ++i) {
				marray->monitors [i].data = &marray->monitors [i + 1];
			}
			marray->monitors [i].data = NULL; /* the last one */
			monitor_freelist = &marray->monitors [0];
			/* we happend the marray instead of prepending so that
			 * the collecting loop above will need to scan smaller arrays first
			 */
			if (!monitor_allocated) {
				monitor_allocated = marray;
			} else {
				last = monitor_allocated;
				while (last->next)
					last = last->next;
				last->next = marray;
			}
		}
	}

	new = monitor_freelist;
	monitor_freelist = new->data;

	new->owner = id;
	new->entry_count = 0;
	
	mono_perfcounters->gc_sync_blocks++;
	return new;
}

static inline void
mono_monitor_ensure_synchronized (LockWord lw, guint32 id)
{
	if ((lw.lock_word & LOCK_WORD_BITS_MASK) == 0) {
		if ((((unsigned int)lw.lock_word) >> LOCK_WORD_OWNER_SHIFT) == id) {
			return;
		}
	} else if (lw.lock_word & LOCK_WORD_INFLATED) {
		lw.lock_word &= ~LOCK_WORD_BITS_MASK;
		if (lw.sync->owner == id) {
			return;
		}
	}

	LOCK_DEBUG (g_message ("%s: (%d) Synchronization error with lock word %p", __func__, mono_thread_info_get_small_id (), lw.sync));
		
	mono_raise_exception (mono_get_exception_synchronization_lock ("Object synchronization method was called from an unsynchronized block of code."));	
}

/*
 * When this function is called it has already been established that the
 * current thread owns the monitor.
 */
static inline void
mono_monitor_exit_inflated (MonoObject *obj, MonoThreadsSync *mon)
{
	if (G_LIKELY (mon->nest == 0)) {
		LOCK_DEBUG (g_message ("%s: (%d) Object %p is now unlocked", __func__, mono_thread_info_get_small_id (), obj));
	
		mon->owner = 0;

		/* Do the wakeup stuff. It's possible that the last
		 * blocking thread gave up waiting just before we
		 * release the semaphore resulting in a futile wakeup
		 * next time there's contention for this object, but
		 * it means we don't have to waste time locking the
		 * struct.
		 */
		if (mon->entry_count > 0) {
			ReleaseSemaphore (mon->entry_sem, 1, NULL);
		}
	} else {
		mon->nest -= 1;
		LOCK_DEBUG (g_message ("%s: (%d) Object %p is now locked %d times", __func__, mono_thread_info_get_small_id (), obj, mon->nest + 1));		
	}
}

/*
 * When this function is called it has already been established that the
 * current thread owns the monitor.
 */
static inline void
mono_monitor_exit_flat (MonoObject *obj, LockWord lw)
{
	if (G_UNLIKELY (lw.lock_word & LOCK_WORD_NEST_MASK)) {
		lw.lock_word -= 1 << LOCK_WORD_NEST_SHIFT;
		LOCK_DEBUG (g_message ("%s: (%d) Object %p is now locked %d times", __func__, mono_thread_info_get_small_id (), obj, ((lw.lock_word & LOCK_WORD_NEST_MASK) >> LOCK_WORD_NEST_SHIFT) + 1));		
	} else {
		lw.lock_word = 0;
		LOCK_DEBUG (g_message ("%s: (%d) Object %p is now unlocked", __func__, mono_thread_info_get_small_id (), obj));
	}
	obj->synchronisation = lw.sync;
	UNLOCK_FENCE;
}

void
mono_monitor_exit (MonoObject *obj)
{
	guint32 id;
	LockWord lw;
	
	LOCK_DEBUG (g_message ("%s: (%d) Unlocking %p", __func__, mono_thread_info_get_small_id (), obj));

	if (G_UNLIKELY (!obj)) {
		mono_raise_exception (mono_get_exception_argument_null ("obj"));
		return;
	}

	id = mono_thread_info_get_small_id ();
	lw.sync = obj->synchronisation;

	mono_monitor_ensure_synchronized (lw, id);

	if (G_UNLIKELY (lw.lock_word & LOCK_WORD_INFLATED)) {
		lw.lock_word &= ~LOCK_WORD_BITS_MASK;
		mono_monitor_exit_inflated (obj, lw.sync);
	} else {
		mono_monitor_exit_flat (obj, lw);
	}
}

/* 
 * If allow_interruption == TRUE, the method will be interrupted if abort or suspend
 * is requested. In this case it returns -1.
 */
static inline gint32 
mono_monitor_try_enter_inflated (MonoObject *obj, MonoThreadsSync *mon, guint32 id, 
											guint32 ms, gboolean allow_interruption)
{	
	MonoInternalThread *thread;
	HANDLE sem;
	guint32 then;
	guint32 waitms;	
	guint32 ret;

	if (G_LIKELY (mon->owner == 0 && InterlockedCompareExchange (&mon->owner, id, 0) == 0)) {
		return 1;
	}
	
	if (mon->owner == id) {
		mon->nest += 1;
		return 1;
	}
	
	mono_perfcounters->thread_contentions++;
		
	if (G_UNLIKELY (ms == 0)) {
		LOCK_DEBUG (g_message ("%s: (%d) timed out, returning FALSE", __func__, mono_thread_info_get_small_id ()));
		return 0;
	}

	mono_profiler_monitor_event (obj, MONO_PROFILER_MONITOR_CONTENTION);
	
	/*
	 * Create the semaphore if necessary.
	 */

	if (mon->entry_sem == NULL) {
		sem = CreateSemaphore (NULL, 0, 0x7fffffff, NULL);
		g_assert (sem != NULL);
		if (InterlockedCompareExchangePointer ((gpointer*)&mon->entry_sem, sem, NULL) != NULL) {
			CloseHandle (sem);
		}
	}

	then = mono_msec_ticks ();

retry:
	if (G_LIKELY (mon->owner == 0 && InterlockedCompareExchange (&mon->owner, id, 0) == 0)) {
		mono_profiler_monitor_event (obj, MONO_PROFILER_MONITOR_DONE);
		return 1;
	} 
	
	/* If we need to time out, record a timestamp and adjust ms,
		* because WaitForSingleObject doesn't tell us how long it
		* waited for.
		*
		* Don't block forever here, because theres a chance the owner
		* thread released the lock while we were creating the
		* semaphore: we would not get the wakeup.  Using the event
		* handle technique from pulse/wait would involve locking the
		* lock struct and therefore slowing down the fast path.
		*/

	if (ms != INFINITE) {
		if (ms < 100) {
			waitms = ms;
		} else {
			waitms = 100;
		}
	} else {
		waitms = 100;
	}

	InterlockedIncrement (&mon->entry_count);

	mono_perfcounters->thread_queue_len++;
	mono_perfcounters->thread_queue_max++;

	thread = mono_thread_current ();
		
	mono_thread_set_state (thread, ThreadState_WaitSleepJoin);

	/*
	 * We pass TRUE instead of allow_interruption since we have to check for the
	 * StopRequested case below.
	 */
		
	ret = WaitForSingleObjectEx (mon->entry_sem, waitms, TRUE);

	mono_thread_clr_state (thread, ThreadState_WaitSleepJoin);
	
	InterlockedDecrement (&mon->entry_count);
		
	mono_perfcounters->thread_queue_len--;

	if (ms != INFINITE) {
      guint32 now = mono_msec_ticks ();
      guint32 elapsed = now == then ? 1 : now - then;
		if (ms <= elapsed) {
         ms = 0;
    } else {
         ms -= elapsed;
		}

		then = now;

		if ((ret == WAIT_TIMEOUT || (ret == WAIT_IO_COMPLETION && !allow_interruption)) && ms > 0) {
			goto retry;
		}
	} else {
		if (ret == WAIT_TIMEOUT || (ret == WAIT_IO_COMPLETION && !allow_interruption)) {
			if (ret == WAIT_IO_COMPLETION && (mono_thread_test_state (thread, (ThreadState_StopRequested|ThreadState_SuspendRequested)))) {
				/* 
				 * We have to obey a stop/suspend request even if 
				 * allow_interruption is FALSE to avoid hangs at shutdown.
				 */
				mono_profiler_monitor_event (obj, MONO_PROFILER_MONITOR_FAIL);
				return -1;
			}
			/* Infinite wait, so just try again */
			goto retry;
		}
	}
	
	if (ret == WAIT_OBJECT_0) {
		goto retry;
	}

	/* We must have timed out */
	LOCK_DEBUG (g_message ("%s: (%d) timed out waiting, returning FALSE", __func__, mono_thread_info_get_small_id ()));

	mono_profiler_monitor_event (obj, MONO_PROFILER_MONITOR_FAIL);

	return ret == WAIT_IO_COMPLETION ? -1 : 0;
}

/*
 * Returns with the monitor lock held.
 */
static inline gint32 
mono_monitor_inflate (MonoObject *obj, int id, guint32 ms, gboolean allow_interruption)
{
	LockWord lw;
	MonoThreadsSync *mon;
	gboolean locked;
	gboolean monitor_removed;
	guint32 then;
	guint32 ret;

	LOCK_DEBUG (g_message ("%s: (%d) Inflating lock object %p", __func__, mono_thread_info_get_small_id (), obj));

	/*
	 * Allocate a lock record and register the object in the monitor table.
	 */

	mono_monitor_allocator_lock ();		
	if ((locked = ((mon = (MonoThreadsSync *)g_hash_table_lookup (monitor_table, obj)) == NULL))) {
		mon = mon_new (id);
		g_hash_table_insert (monitor_table, obj, mon); 
		mon->nest = 1;
	} else {
		mon->nest += 1;
	}
	mono_monitor_allocator_unlock ();

	/*
	 * Check if the monitor is already inflated and if we hold the correct one.
	 */

	lw.sync = obj->synchronisation;
	
	if (lw.lock_word & LOCK_WORD_INFLATED) {
		lw.lock_word &= ~LOCK_WORD_STATUS_BITS;
		if (lw.sync != mon) {
			mono_monitor_allocator_lock ();
			if (--mon->nest == 0) {
				monitor_removed = g_hash_table_remove (monitor_table, obj);
				g_assert (monitor_removed);
				mon_finalize (mon);
			}
			mono_monitor_allocator_unlock ();
		}

		return mono_monitor_try_enter_inflated (obj, lw.sync, id, ms, allow_interruption);
	}

	/*
	 * Wait for the lock to be released.
	 */

	then = ms != INFINITE ? mono_msec_ticks () : 0;

	if (!locked) {
		if ((ret = mono_monitor_try_enter_inflated (obj, mon, id, ms, allow_interruption)) != 1) {
			goto fail;
		}
		lw.sync = obj->synchronisation;
		if (lw.lock_word & LOCK_WORD_INFLATED) {
			return 1;
		}
	}

	do {

		/*
		 * Check if the lock can be acquired and build the new lock word. We do
		 * the latter inside the loop as a kind of backoff.
		 */

		if (lw.lock_word == 0 || (lw.lock_word & LOCK_WORD_THIN_HASH) != 0) {
			LockWord nlw;
			nlw.sync = mon;
			nlw.lock_word |= LOCK_WORD_INFLATED;
			
#ifdef HAVE_MOVING_COLLECTOR
			if (lw.lock_word & LOCK_WORD_THIN_HASH) {
				nlw.lock_word |= LOCK_WORD_THIN_HASH;
				mon->hash_code = (unsigned int)lw.lock_word >> LOCK_WORD_HASH_SHIFT;
			}
#endif
			if (InterlockedCompareExchangePointer ((gpointer *)&obj->synchronisation, nlw.sync, lw.sync) == lw.sync) {

				/*
				 * The lock is inflated. Now we can remove the object from the monitor table.
				 */

				mono_gc_weak_link_add (&mon->data, obj, FALSE);

				mono_monitor_allocator_lock ();
				mon->nest = 0;
				monitor_removed = g_hash_table_remove (monitor_table, obj);
				g_assert (monitor_removed);
				mono_monitor_allocator_unlock ();

				LOCK_DEBUG (g_message ("%s: (%d) Inflated lock object %p to mon %p (%d)", __func__, mono_thread_info_get_small_id (), obj, mon, mon->owner));
				return 1;
			}
		}

#ifdef HOST_WIN32
		SwitchToThread ();
#else
		sched_yield ();
#endif

		if (ms != INFINITE) {
			int now = mono_msec_ticks ();
			int elapsed = now == then ? 1 : now - then;
			if (ms <= elapsed) {
				mono_monitor_exit_inflated (obj, mon);
				ret = 0;
				goto fail;
			} else {
				ms -= elapsed;
			}

			then = now;
		}

		lw.sync = obj->synchronisation;
	} while (TRUE);
	
fail:
	mono_monitor_allocator_lock ();
	
	lw.sync = obj->synchronisation;
	
	if ((lw.lock_word & LOCK_WORD_INFLATED) == 0 && --mon->nest == 0) {
		monitor_removed = g_hash_table_remove (monitor_table, obj);
		g_assert (monitor_removed);
	}

	mono_monitor_allocator_unlock ();

	LOCK_DEBUG (g_message ("%s: (%d) Failed to inflated lock object %p", __func__, mono_thread_info_get_small_id (), obj));
	return ret;
}

static inline gboolean
mono_monitor_inflate_owned (MonoObject *obj, int id)
{
	LockWord lw;
	guint32 nest;
	guint32 ret;

	LOCK_DEBUG (g_message("%s: (%d) Inflating lock %p owned by the current thread", __func__, id, obj));

	lw.sync = obj->synchronisation;
	nest = (lw.lock_word & LOCK_WORD_NEST_MASK) >> LOCK_WORD_NEST_SHIFT;
	
	obj->synchronisation = 0;

	/*
	 * We must ensure that we regain ownership of the monitor.
	 */
	
	while ((ret = mono_monitor_inflate (obj, id, INFINITE, FALSE)) == -1) {
		if (mono_threads_is_shutting_down ()) {
			return FALSE;
		}
	}

	lw.sync = obj->synchronisation;
	lw.lock_word &= ~LOCK_WORD_BITS_MASK;
	lw.sync->nest = nest;

	LOCK_DEBUG (g_message("%s: (%d) Regained ownership of lock %p (%d)", __func__, mono_thread_info_get_small_id (), obj, lw.sync->owner));

	if (ret == -1) {
		mono_thread_interruption_checkpoint ();
	}

	return TRUE;
}

static inline gint32 
mono_monitor_try_enter_internal (MonoObject *obj, guint32 ms, gboolean allow_interruption)
{
	LockWord lw;
	int id;

	LOCK_DEBUG (g_message("%s: (%d) Trying to lock object %p (%d ms)", __func__, mono_thread_info_get_small_id (), obj, ms));

	if (G_UNLIKELY (!obj)) {
		mono_raise_exception (mono_get_exception_argument_null ("obj"));
	}

	lw.sync = obj->synchronisation;
	id = mono_thread_info_get_small_id ();

	if (G_LIKELY (lw.lock_word == 0)) {
		LockWord nlw;
		nlw.lock_word = id << LOCK_WORD_OWNER_SHIFT;
			
		if (InterlockedCompareExchangePointer ((gpointer *)&obj->synchronisation, nlw.sync, NULL) == NULL) {
			return 1;
		}
		lw.sync = obj->synchronisation;
	} else if ((lw.lock_word & LOCK_WORD_BITS_MASK) == 0 && ((unsigned int)lw.lock_word >> LOCK_WORD_OWNER_SHIFT) == id) {
		if ((lw.lock_word & LOCK_WORD_NEST_MASK) == LOCK_WORD_NEST_MASK) {
			mono_monitor_inflate_owned (obj, id);
			lw.sync = obj->synchronisation;
			lw.lock_word &= ~LOCK_WORD_BITS_MASK;
			lw.sync->nest += 1;
		} else {
			lw.lock_word += 1 << LOCK_WORD_NEST_SHIFT;
			obj->synchronisation = lw.sync;
		}
		return 1;
	} 

	if (lw.lock_word & LOCK_WORD_INFLATED) {
		lw.lock_word &= ~LOCK_WORD_BITS_MASK;
		return mono_monitor_try_enter_inflated (obj, lw.sync, id, ms, allow_interruption);
	}

	/*
	 * Either there's contention for the lock or the lock word contains the hash
	 * code. Either way, inflate the monitor.
	 */

	return mono_monitor_inflate (obj, id, ms, allow_interruption);
}

gboolean 
mono_monitor_enter (MonoObject *obj)
{
	return mono_monitor_try_enter_internal (obj, INFINITE, FALSE) == 1;
}

gboolean 
mono_monitor_try_enter (MonoObject *obj, guint32 ms)
{
	return mono_monitor_try_enter_internal (obj, ms, FALSE) == 1;
}

/*
 * mono_object_hash:
 * @obj: an object
 *
 * Calculate a hash code for @obj that is constant while @obj is alive.
 */
int
mono_object_hash (MonoObject* obj)
{
#ifdef HAVE_MOVING_COLLECTOR
	LockWord lw;
	unsigned int hash;

	if (!obj) {
		return 0;
	}

	lw.sync = obj->synchronisation;
	
	if (lw.lock_word & LOCK_WORD_THIN_HASH) {
		/*g_print ("fast thin hash %d for obj %p store\n", (unsigned int)lw.lock_word >> LOCK_WORD_HASH_SHIFT, obj);*/
		return (unsigned int)lw.lock_word >> LOCK_WORD_HASH_SHIFT;
	}

	if (lw.lock_word & LOCK_WORD_FAT_HASH) {
		lw.lock_word &= ~LOCK_WORD_BITS_MASK;
		/*g_print ("fast fat hash %d for obj %p store\n", lw.sync->hash_code, obj);*/
		return lw.sync->hash_code;
	}

	/*
	 * Compute the 30-bit hash.
	 */

	hash = (GPOINTER_TO_UINT (obj) >> MONO_OBJECT_ALIGNMENT_SHIFT) * 2654435761u;
	hash &= ~(LOCK_WORD_BITS_MASK << 30);

	/*
	 * While we are inside this function, the GC will keep this object pinned,
	 * since we are in the unmanaged stack. Thanks to this and to the hash
	 * function that depends only on the address, we can ignore the races if
	 * another thread computes the hash at the same time, because it'll end up
	 * with the same value.
	 */	
	
	if (lw.lock_word == 0) {
		/*g_print ("storing thin hash code %d for obj %p\n", hash, obj);*/
			
		lw.lock_word = LOCK_WORD_THIN_HASH | (hash << LOCK_WORD_HASH_SHIFT);
		if (InterlockedCompareExchangePointer ((gpointer*)&obj->synchronisation, lw.sync, NULL) == NULL) {
			return hash;
		}

		/*g_print ("failed store\n");*/
			
		lw.sync = obj->synchronisation;

		if (lw.lock_word & LOCK_WORD_THIN_HASH) {
			return hash;
		}

		/* 
		 * Someone acquired or inflated the lock.
		 */
	}	
	 
	if ((lw.lock_word & LOCK_WORD_INFLATED) == 0) {
	
		/*
		 * The lock is owned by some thread, so we must inflate it. Note that it's
		 * not common to both lock an object and ask for its hash code.
		 */

		gboolean locked;
		int id = mono_thread_info_get_small_id ();

		if ((locked = ((unsigned int)lw.lock_word >> LOCK_WORD_OWNER_SHIFT) == id)) {
			if (!mono_monitor_inflate_owned (obj, id)) {
				return 0;
			}
		} else {
			while (mono_monitor_inflate (obj, id, INFINITE, FALSE) == -1) {
				/* 
				 * FIXME: Don't raise a ThreadInterruptedException ?
				 */
				 
				mono_thread_interruption_checkpoint ();
			}
		}

		lw.sync = obj->synchronisation;
		lw.lock_word &= ~LOCK_WORD_BITS_MASK;

		if (!locked) {
			mono_monitor_exit_inflated (obj, lw.sync);
		}		
	}

	/*g_print ("storing hash code %d for obj %p in sync %p\n", hash, obj, lw.sync);*/

	lw.sync->hash_code = hash;
	lw.lock_word |= LOCK_WORD_FAT_HASH;

	/* 
	 * This is safe while we don't deflate locks. 
	 */
			
	obj->synchronisation = lw.sync;
	return hash;
#else
	/*
    * Wang's address-based hash function:
	 *   http://www.concentric.net/~Ttwang/tech/addrhash.htm
	 */
	return (GPOINTER_TO_UINT (obj) >> MONO_OBJECT_ALIGNMENT_SHIFT) * 2654435761u;
#endif
}

void**
mono_monitor_get_object_monitor_weak_link (MonoObject *object)
{
	LockWord lw;

	lw.sync = object->synchronisation;

	if (lw.lock_word & LOCK_WORD_INFLATED) {
		lw.lock_word &= ~LOCK_WORD_BITS_MASK;
		return (void **)&lw.sync->data;
	} 

	return NULL;
}

static void
emit_obj_syncp_check (MonoMethodBuilder *mb, int thread_tls_offset, int syncp_add_loc, 
							 int syncp_loc, int tid_loc, int *obj_null_branch, int *thread_info_null_branch)
{
	/*
		ldarg				0															obj
		brfalse.s		obj_null
	*/

	mono_mb_emit_byte (mb, CEE_LDARG_0);
	*obj_null_branch = mono_mb_emit_short_branch (mb, CEE_BRFALSE_S);

	/*
	 	ldarg				0															obj
		conv.i																		objp
		ldc.i4			G_STRUCT_OFFSET(MonoObject, synchronisation)	objp off
		add																			&syncp
		stloc				&syncp
		ldloc				&syncp													&syncp
		ldind.i																		syncp
		stloc				syncp
	 */

	mono_mb_emit_byte (mb, CEE_LDARG_0);
	mono_mb_emit_byte (mb, CEE_CONV_I);
	mono_mb_emit_icon (mb, G_STRUCT_OFFSET (MonoObject, synchronisation));
	mono_mb_emit_byte (mb, CEE_ADD);
	mono_mb_emit_stloc (mb, syncp_add_loc);
	mono_mb_emit_ldloc (mb, syncp_add_loc);
	mono_mb_emit_byte (mb, CEE_LDIND_I);
	mono_mb_emit_stloc (mb, syncp_loc);

	/*
	 	mono.tls			thread_tls_offset										threadp
		ldc.i4			G_STRUCT_OFFSET(MonoInternalThread, thread_info) threadp off
		add																			&thread_info
		ldind.i																		thread_info
		dup																			thread_info thread_info
		brfalse.s		thread_info_null										thread_info
		ldc.i4			G_STRUCT_OFFSET(MonoThreadInfo, small_id)		thread_info off
		add																			&tid
		ldind.i4																		tid
		stloc				tid
	 */

	mono_mb_emit_byte (mb, MONO_CUSTOM_PREFIX);
	mono_mb_emit_byte (mb, CEE_MONO_TLS);
	mono_mb_emit_i4 (mb, thread_tls_offset);
	mono_mb_emit_icon (mb, G_STRUCT_OFFSET (MonoInternalThread, thread_info));
	mono_mb_emit_byte (mb, CEE_ADD);
	mono_mb_emit_byte (mb, CEE_LDIND_I);
	mono_mb_emit_byte (mb, CEE_DUP);
	*thread_info_null_branch = mono_mb_emit_short_branch (mb, CEE_BRFALSE_S); 
	mono_mb_emit_icon (mb, G_STRUCT_OFFSET (MonoThreadInfo, small_id));
	mono_mb_emit_byte (mb, CEE_ADD);
	mono_mb_emit_byte (mb, CEE_LDIND_I);
	mono_mb_emit_stloc (mb, tid_loc);
}

static MonoMethod* monitor_il_fastpaths[3];

gboolean
mono_monitor_is_il_fastpath_wrapper (MonoMethod *method)
{
	int i;
	for (i = 0; i < 3; ++i) {
		if (monitor_il_fastpaths [i] == method)
			return TRUE;
	}
	return FALSE;
}

enum {
	FASTPATH_ENTER,
	FASTPATH_ENTERV4,
	FASTPATH_EXIT
};

static MonoMethod*
register_fastpath (MonoMethod *method, int idx)
{
	mono_memory_barrier ();
	monitor_il_fastpaths [idx] = method;
	return method;
}

static MonoMethod*
mono_monitor_get_fast_enter_method (MonoMethod *monitor_enter_method)
{
	MonoMethodBuilder *mb;
	MonoMethod *ret;
	static MonoMethod *compare_exchange_method;
	int true_locktaken_branch, obj_null_branch, thread_info_null_branch, not_free_branch,
		 contention_branch, inflated_branch, other_owner_branch, max_nest_branch;
	int syncp_add_loc, syncp_loc, tid_loc;
	int thread_tls_offset;
	gboolean is_v4 = mono_method_signature (monitor_enter_method)->param_count == 2;
	int fast_path_idx = is_v4 ? FASTPATH_ENTERV4 : FASTPATH_ENTER;

	thread_tls_offset = mono_thread_get_tls_offset ();
	if (thread_tls_offset == -1) {
		return NULL;
	}

	if (monitor_il_fastpaths [fast_path_idx]) {
		return monitor_il_fastpaths [fast_path_idx];
	}

	if (!compare_exchange_method) {
		MonoMethodDesc *desc;
		MonoClass *class;

		desc = mono_method_desc_new ("Interlocked:CompareExchange(intptr&,intptr,intptr)", FALSE);
		class = mono_class_from_name (mono_defaults.corlib, "System.Threading", "Interlocked");
		compare_exchange_method = mono_method_desc_search_in_class (desc, class);
		mono_method_desc_free (desc);

		if (!compare_exchange_method) {
			return NULL;
		}
	}

	mb = mono_mb_new (mono_defaults.monitor_class, is_v4 ? "FastMonitorEnterV4" : "FastMonitorEnter", MONO_WRAPPER_UNKNOWN);

	mb->method->slot = -1;
	mb->method->flags = METHOD_ATTRIBUTE_PUBLIC | METHOD_ATTRIBUTE_STATIC |
							  METHOD_ATTRIBUTE_HIDE_BY_SIG | METHOD_ATTRIBUTE_FINAL;

	syncp_add_loc = mono_mb_add_local (mb, &mono_defaults.int_class->byval_arg);
	syncp_loc = mono_mb_add_local (mb, &mono_defaults.int_class->byval_arg);
	tid_loc = mono_mb_add_local (mb, &mono_defaults.int32_class->byval_arg);

	/*
	  	ldarg.1																		&lockTaken
		ldind.i1																		lockTaken
		brtrue.s	 		true_locktaken			
	 */

	if (is_v4) {
		mono_mb_emit_byte (mb, CEE_LDARG_1);
		mono_mb_emit_byte (mb, CEE_LDIND_I1);
		true_locktaken_branch = mono_mb_emit_short_branch (mb, CEE_BRTRUE_S);
	}
	
	emit_obj_syncp_check (mb, thread_tls_offset, syncp_add_loc, syncp_loc, tid_loc, &obj_null_branch, &thread_info_null_branch);

	/*
	 	ldloc				syncp														syncp
		brtrue.s			not_free 
		ldloc				&syncp													&syncp			
		ldloc				tid														&syncp tid
		ldc.i4.s 		LOCK_WORD_OWNER_SHIFT								&syncp tid LOCK_WORD_OWNER_SHIFT
		shl 																			&syncp (tid << LOCK_WORD_OWNER_SHIFT)
		ldc.i4			0															&syncp (tid << LOCK_WORD_OWNER_SHIFT) 0
		call				System.Threading.Interlocked.CompareExchange	owner
		brtrue.s			contention
		ret
	 */

	mono_mb_emit_ldloc (mb, syncp_loc);
	not_free_branch = mono_mb_emit_short_branch (mb, CEE_BRTRUE_S);
	mono_mb_emit_ldloc (mb, syncp_add_loc);
	mono_mb_emit_ldloc (mb, tid_loc);
	mono_mb_emit_icon (mb, LOCK_WORD_OWNER_SHIFT);
	mono_mb_emit_byte (mb, CEE_SHL);
	mono_mb_emit_byte (mb, CEE_LDC_I4_0);
	mono_mb_emit_managed_call (mb, compare_exchange_method, NULL);
	contention_branch = mono_mb_emit_short_branch (mb, CEE_BRTRUE_S);

	if (is_v4) {
		mono_mb_emit_byte (mb, CEE_LDARG_1);
		mono_mb_emit_byte (mb, CEE_LDC_I4_1);
		mono_mb_emit_byte (mb, CEE_STIND_I1);
	}
	mono_mb_emit_byte (mb, CEE_RET);

	/*
	not_free:
		ldloc				syncp														syncp													
		ldc.i4.s			LOCK_WORD_BITS_MASK									syncp LOCK_WORD_BITS_MASK
		and																			(syncp & LOCK_WORD_BITS_MASK)
		brtrue.s			inflated
		ldloc				syncp														syncp													
		ldc.i4.s			LOCK_WORD_OWNER_SHIFT								syncp LOCK_WORD_OWNER_SHIFT
		shr.un																		(syncp >> LOCK_WORD_OWNER_SHIFT)
		ldloc				tid														(syncp >> LOCK_WORD_OWNER_SHIFT) tid
		bne.un.s			other_owner																		
		ldc.i4.s 		LOCK_WORD_NEST_MASK									LOCK_WORD_NEST_MASK
		dup																			LOCK_WORD_NEST_MASK LOCK_WORD_NEST_MASK
		ldloc				syncp														LOCK_WORD_NEST_MASK LOCK_WORD_NEST_MASK syncp		
		and																			LOCK_WORD_NEST_MASK (syncp & LOCK_WORD_NEST_MASK)
		beq.s 			max_nest																			
		ldloc				&syncp													&syncp																			
		ldloc				syncp														&syncp syncp
		ldc.i4 			1 << LOCK_WORD_NEST_SHIFT                    &syncp syncp (1 << LOCK_WORD_NEST_SHIFT)
		add 																			&syncp (syncp + (1 << LOCK_WORD_NEST_SHIFT))
		stind.i
		ret
	 */

	mono_mb_patch_short_branch (mb, not_free_branch);
	mono_mb_emit_ldloc (mb, syncp_loc);
	mono_mb_emit_icon (mb, LOCK_WORD_BITS_MASK);
	mono_mb_emit_byte (mb, CEE_AND);
	inflated_branch = mono_mb_emit_short_branch (mb, CEE_BRTRUE_S);
	mono_mb_emit_ldloc (mb, syncp_loc);
	mono_mb_emit_icon (mb, LOCK_WORD_OWNER_SHIFT);
	mono_mb_emit_byte (mb, CEE_SHR_UN);
	mono_mb_emit_ldloc (mb, tid_loc);
	other_owner_branch = mono_mb_emit_short_branch (mb, CEE_BNE_UN_S);
	mono_mb_emit_icon (mb, LOCK_WORD_NEST_MASK);
	mono_mb_emit_byte (mb, CEE_DUP);
	mono_mb_emit_ldloc (mb, syncp_loc);
	mono_mb_emit_byte (mb, CEE_AND);
	max_nest_branch = mono_mb_emit_short_branch (mb, CEE_BEQ_S);
	mono_mb_emit_ldloc (mb, syncp_add_loc);
	mono_mb_emit_ldloc (mb, syncp_loc);
	mono_mb_emit_icon (mb, 1 << LOCK_WORD_NEST_SHIFT);
	mono_mb_emit_byte (mb, CEE_ADD);
	mono_mb_emit_byte (mb, CEE_STIND_I);
	
	if (is_v4) {
		mono_mb_emit_byte (mb, CEE_LDARG_1);
		mono_mb_emit_byte (mb, CEE_LDC_I4_1);
		mono_mb_emit_byte (mb, CEE_STIND_I1);
	}

	mono_mb_emit_byte (mb, CEE_RET);

	/*
	thread_info_null:
	  pop
	true_locktaken, obj_null,  contention, inflated_branch, other_owner, max_nest:
	  ldarg				0															obj
	  call				System.Threading.Monitor.Enter
	  ret
	*/

	mono_mb_patch_short_branch (mb, thread_info_null_branch);
	mono_mb_emit_byte (mb, CEE_POP);

	if (is_v4) {
		mono_mb_patch_short_branch (mb, true_locktaken_branch);
	}
	mono_mb_patch_short_branch (mb, obj_null_branch);
	mono_mb_patch_short_branch (mb, contention_branch);
	mono_mb_patch_short_branch (mb, inflated_branch);
	mono_mb_patch_short_branch (mb, other_owner_branch);
	mono_mb_patch_short_branch (mb, max_nest_branch);

	mono_mb_emit_byte (mb, CEE_LDARG_0);
	if (is_v4) {
		mono_mb_emit_byte (mb, CEE_LDARG_1);
	}
	mono_mb_emit_managed_call (mb, monitor_enter_method, NULL);
	mono_mb_emit_byte (mb, CEE_RET);

	ret = register_fastpath (mono_mb_create_method (mb, mono_signature_no_pinvoke (monitor_enter_method), 5), fast_path_idx);
	mono_mb_free (mb);
	return ret;
}

static MonoMethod*
mono_monitor_get_fast_exit_method (MonoMethod *monitor_exit_method)
{
	MonoMethodBuilder *mb;
	MonoMethod *ret;
	int obj_null_branch, thread_info_null_branch, inflated_branch, other_owner_branch, nested_branch, success_branch;
	int thread_tls_offset;
	int syncp_add_loc, syncp_loc, tid_loc;

	thread_tls_offset = mono_thread_get_tls_offset ();
	if (thread_tls_offset == -1) {
		return NULL;
	}

	if (monitor_il_fastpaths [FASTPATH_EXIT]) {
		return monitor_il_fastpaths [FASTPATH_EXIT];
	}

	mb = mono_mb_new (mono_defaults.monitor_class, "FastMonitorExit", MONO_WRAPPER_UNKNOWN);

	mb->method->slot = -1;
	mb->method->flags = METHOD_ATTRIBUTE_PUBLIC | METHOD_ATTRIBUTE_STATIC |
							  METHOD_ATTRIBUTE_HIDE_BY_SIG | METHOD_ATTRIBUTE_FINAL;

	syncp_add_loc = mono_mb_add_local (mb, &mono_defaults.int_class->byval_arg);
	syncp_loc = mono_mb_add_local (mb, &mono_defaults.int_class->byval_arg);
	tid_loc = mono_mb_add_local (mb, &mono_defaults.int32_class->byval_arg);
	
	emit_obj_syncp_check (mb, thread_tls_offset, syncp_add_loc, syncp_loc, tid_loc, &obj_null_branch, &thread_info_null_branch);

	/*
		ldloc				syncp														syncp													
		ldc.i4.s			LOCK_WORD_BITS_MASK									syncp LOCK_WORD_BITS_MASK
		and																			(syncp & LOCK_WORD_BITS_MASK)
		brtrue.s			inflated
		ldloc				syncp														syncp													
		ldc.i4.s			LOCK_WORD_OWNER_SHIFT								syncp LOCK_WORD_OWNER_SHIFT
		shr.un																		(syncp >> LOCK_WORD_OWNER_SHIFT)
		ldloc				tid														(syncp >> LOCK_WORD_OWNER_SHIFT) tid
		bne.un.s			other_owner	
		ldloc				&syncp													&syncp	
		ldloc				syncp														&syncp syncp		
		ldc.i4.s 		LOCK_WORD_NEST_MASK									&syncp syncp LOCK_WORD_NEST_MASK
		and																			&syncp (syncp & LOCK_WORD_NEST_MASK)
		brtrue.s 		nested													&syncp				
		ldc.i4			0															&syncp 0
		br.s				success													&syncp 0
	nested_branch:
		ldloc				syncp														&syncp syncp
		ldc.i4 			1 << LOCK_WORD_NEST_SHIFT                    &syncp syncp (1 << LOCK_WORD_NEST_SHIFT)
		sub 																			&syncp (syncp - (1 << LOCK_WORD_NEST_SHIFT))
	success_branch:
		volatile.stind.i													&syncp [0 | (syncp - (1 << LOCK_WORD_NEST_SHIFT))]
		ret
	 */

	mono_mb_emit_ldloc (mb, syncp_loc);
	mono_mb_emit_icon (mb, LOCK_WORD_BITS_MASK);
	mono_mb_emit_byte (mb, CEE_AND);
	inflated_branch = mono_mb_emit_short_branch (mb, CEE_BRTRUE_S);
	mono_mb_emit_ldloc (mb, syncp_loc);
	mono_mb_emit_icon (mb, LOCK_WORD_OWNER_SHIFT);
	mono_mb_emit_byte (mb, CEE_SHR_UN);
	mono_mb_emit_ldloc (mb, tid_loc);
	other_owner_branch = mono_mb_emit_short_branch (mb, CEE_BNE_UN_S);
	mono_mb_emit_ldloc (mb, syncp_add_loc);
	mono_mb_emit_ldloc (mb, syncp_loc);
	mono_mb_emit_icon (mb, LOCK_WORD_NEST_MASK);
	mono_mb_emit_byte (mb, CEE_AND);
	nested_branch = mono_mb_emit_short_branch (mb, CEE_BRTRUE_S);
	mono_mb_emit_byte (mb, CEE_LDNULL);
	success_branch = mono_mb_emit_short_branch (mb, CEE_BR_S);
	mono_mb_patch_short_branch (mb, nested_branch);
	mono_mb_emit_ldloc (mb, syncp_loc);
	mono_mb_emit_icon (mb, 1 << LOCK_WORD_NEST_SHIFT);
	mono_mb_emit_byte (mb, CEE_SUB);
	mono_mb_patch_short_branch (mb, success_branch);
	mono_mb_emit_byte (mb, CEE_VOLATILE_);
	mono_mb_emit_byte (mb, CEE_STIND_I);
	mono_mb_emit_byte (mb, CEE_RET);
	
	/*
	thread_info_null:
	  pop
	obj_null_branch, inflated_branch, other_owner_branch:
	  ldarg				0															obj
	  call				System.Threading.Monitor.Exit
	  ret
	 */

	mono_mb_patch_short_branch (mb, thread_info_null_branch);
	mono_mb_emit_byte (mb, CEE_POP);

	mono_mb_patch_short_branch (mb, obj_null_branch);
	mono_mb_patch_short_branch (mb, inflated_branch);
	mono_mb_patch_short_branch (mb, other_owner_branch);

	mono_mb_emit_byte (mb, CEE_LDARG_0);
	mono_mb_emit_managed_call (mb, monitor_exit_method, NULL);
	mono_mb_emit_byte (mb, CEE_RET);

	ret = register_fastpath (mono_mb_create_method (mb, mono_signature_no_pinvoke (monitor_exit_method), 5), FASTPATH_EXIT);
	mono_mb_free (mb);

	return ret;
}

MonoMethod*
mono_monitor_get_fast_path (MonoMethod *enter_or_exit)
{
	if (strcmp (enter_or_exit->name, "Enter") == 0)
		return mono_monitor_get_fast_enter_method (enter_or_exit);
	if (strcmp (enter_or_exit->name, "Exit") == 0)
		return mono_monitor_get_fast_exit_method (enter_or_exit);
	g_assert_not_reached ();
	return NULL;
}

gboolean 
ves_icall_System_Threading_Monitor_Monitor_try_enter (MonoObject *obj, guint32 ms)
{
	gint32 ret;

	do {
		ret = mono_monitor_try_enter_internal (obj, ms, TRUE);
		if (ret == -1)
			mono_thread_interruption_checkpoint ();
	} while (ret == -1);
	
	return ret == 1;
}

void
ves_icall_System_Threading_Monitor_Monitor_try_enter_with_atomic_var (MonoObject *obj, guint32 ms, char *lockTaken)
{
	gint32 ret;
	do {
		ret = mono_monitor_try_enter_internal (obj, ms, TRUE);
		/*This means we got interrupted during the wait and didn't got the monitor.*/
		if (ret == -1)
			mono_thread_interruption_checkpoint ();
	} while (ret == -1);
	/*It's safe to do it from here since interruption would happen only on the wrapper.*/
	*lockTaken = ret == 1;
}

/* 
 * All wait list manipulation in the pulse, pulseall and wait
 * functions happens while the monitor lock is held, so we don't need
 * any extra struct locking
 */
void
ves_icall_System_Threading_Monitor_Monitor_pulse (MonoObject *obj)
{
	int id;
	LockWord lw;
	
	LOCK_DEBUG (g_message ("%s: (%d) Pulsing %p", __func__, mono_thread_info_get_small_id (), obj));
	
	id = mono_thread_info_get_small_id ();
	lw.sync = obj->synchronisation;

	mono_monitor_ensure_synchronized (lw, id);

	if ((lw.lock_word & LOCK_WORD_INFLATED) == 0) {
		
		/*
		 * We assume that we're racing with a waiter, so we preemptively
		 * inflate the monitor.
		 */

		mono_monitor_inflate_owned (obj, id);
		lw.sync = obj->synchronisation;
	}

	lw.lock_word &= ~LOCK_WORD_BITS_MASK;
	
	LOCK_DEBUG (g_message ("%s: (%d) %d threads waiting", __func__, mono_thread_info_get_small_id (), g_slist_length (lw.sync->wait_list)));
	
	if (lw.sync->wait_list != NULL) {
		LOCK_DEBUG (g_message ("%s: (%d) signalling and dequeuing handle %p", __func__, mono_thread_info_get_small_id (), lw.sync->wait_list->data));
	
		SetEvent (lw.sync->wait_list->data);
		lw.sync->wait_list = g_slist_remove (lw.sync->wait_list, lw.sync->wait_list->data);
	}
}

void
ves_icall_System_Threading_Monitor_Monitor_pulse_all (MonoObject *obj)
{
	int id;
	LockWord lw;
	
	LOCK_DEBUG (g_message("%s: (%d) Pulsing all %p", __func__, mono_thread_info_get_small_id (), obj));

	id = mono_thread_info_get_small_id ();
	lw.sync = obj->synchronisation;

	mono_monitor_ensure_synchronized (lw, id);

	if ((lw.lock_word & LOCK_WORD_INFLATED) == 0) {
		
		/*
		 * We assume that we're racing with a waiter, so we preemptively
		 * inflate the monitor.
		 */

		mono_monitor_inflate_owned (obj, id);
		lw.sync = obj->synchronisation;
	}
	
	lw.lock_word &= ~LOCK_WORD_BITS_MASK;

	LOCK_DEBUG (g_message ("%s: (%d) %d threads waiting", __func__, mono_thread_info_get_small_id (), g_slist_length (lw.sync->wait_list)));

	while (lw.sync->wait_list != NULL) {
		LOCK_DEBUG (g_message ("%s: (%d) signalling and dequeuing handle %p", __func__, mono_thread_info_get_small_id (), lw.sync->wait_list->data));
	
		SetEvent (lw.sync->wait_list->data);
		lw.sync->wait_list = g_slist_remove (lw.sync->wait_list, lw.sync->wait_list->data);
	}
}

gboolean
ves_icall_System_Threading_Monitor_Monitor_wait (MonoObject *obj, guint32 ms)
{
	MonoInternalThread *thread;
	int id;
	LockWord lw;
	HANDLE event;
	guint32 nest;
	guint32 ret;

	LOCK_DEBUG (g_message ("%s: (%d) Trying to wait for %p with timeout %dms", __func__, mono_thread_info_get_small_id (), obj, ms));
	
	id = mono_thread_info_get_small_id ();
	lw.sync = obj->synchronisation;

	mono_monitor_ensure_synchronized (lw, id);

	if ((lw.lock_word & LOCK_WORD_INFLATED) == 0) {
		mono_monitor_inflate_owned (obj, id);
		lw.sync = obj->synchronisation;
	}

	lw.lock_word &= ~LOCK_WORD_BITS_MASK;

	/* Do this WaitSleepJoin check before creating the event handle */
	mono_thread_current_check_pending_interrupt ();
	
	event = CreateEvent (NULL, FALSE, FALSE, NULL);
	if (event == NULL) {
		mono_raise_exception (mono_get_exception_synchronization_lock ("Failed to set up wait event"));
		return FALSE;
	}
	
	LOCK_DEBUG (g_message ("%s: (%d) queuing handle %p", __func__, mono_thread_info_get_small_id (), event));
	
	thread = mono_thread_internal_current ();

	mono_thread_set_state (thread, ThreadState_WaitSleepJoin);

	lw.sync->wait_list = g_slist_append (lw.sync->wait_list, event);
	
	/* Save the nest count, and release the lock */
	nest = lw.sync->nest;
	lw.sync->nest = 0;
	mono_monitor_exit_inflated (obj, lw.sync);

	LOCK_DEBUG (g_message ("%s: (%d) Unlocked %p lock %p", __func__, mono_thread_info_get_small_id (), obj, lw.sync));

	/* There's no race between unlocking the monitor and waiting for 
	 * the event, because auto reset events are sticky, and this event
	 * is private to this thread.  Therefore even if the event was
	 * signalled before we wait, we still succeed.
	 */
	
	ret = WaitForSingleObjectEx (event, ms, TRUE);

	/* Reset the thread state fairly early, so we don't have to worry
	 * about the monitor error checking
	 */
	mono_thread_clr_state (thread, ThreadState_WaitSleepJoin);
	
	/* Regain the lock with the previous nest count */
	
	while (mono_monitor_try_enter_inflated (obj, lw.sync, id, INFINITE, FALSE) == -1) {
		ret = -1;
		if (mono_threads_is_shutting_down ()) {
			mono_thread_interruption_checkpoint ();
			return ret;
		}
	}

	lw.sync->nest = nest;

	LOCK_DEBUG (g_message ("%s: (%d) Regained %p lock %p", __func__, mono_thread_info_get_small_id (), obj, lw.sync));

	if (ret == WAIT_TIMEOUT) {
		/* 
		 * Poll the event again, just in case it was signalled
		 * while we were trying to regain the monitor lock
		 */
		ret = WaitForSingleObjectEx (event, 0, FALSE);
	}

	/* 
	 * Pulse will have popped our event from the queue if it signalled
	 * us, so we only do it here if the wait timed out.
	 *
	 * This avoids a race condition where the thread holding the
	 * lock can Pulse several times before the WaitForSingleObject
	 * returns.  If we popped the queue here then this event might
	 * be signalled more than once, thereby starving another
	 * thread.
	 */
	
	if (ret == WAIT_OBJECT_0) {
		LOCK_DEBUG (g_message ("%s: (%d) Success", __func__, mono_thread_info_get_small_id ()));
		CloseHandle (event);
		return 1;
	}
	
	LOCK_DEBUG (g_message ("%s: (%d) Wait failed, dequeuing handle %p", __func__, mono_thread_info_get_small_id (), event));
	
	/*
	 * No pulse, so we have to remove ourself from the wait queue.
	 */

	lw.sync->wait_list = g_slist_remove (lw.sync->wait_list, event);
	CloseHandle (event);

	if (ret == -1) {
		mono_thread_interruption_checkpoint ();
	}
	
	return 0;
}