/*
 * sgen-workers.c: Worker threads for parallel and concurrent GC.
 *
 * Copyright 2001-2003 Ximian, Inc
 * Copyright 2003-2010 Novell, Inc.
 * Copyright (C) 2012 Xamarin Inc
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Library General Public
 * License 2.0 as published by the Free Software Foundation;
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Library General Public License for more details.
 *
 * You should have received a copy of the GNU Library General Public
 * License 2.0 along with this library; if not, write to the Free
 * Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 */

#include "config.h"
#ifdef HAVE_SGEN_GC

#include <string.h>

#include "mono/sgen/sgen-gc.h"
#include "mono/sgen/sgen-workers.h"
#include "mono/sgen/sgen-thread-pool.h"
#include "mono/utils/mono-membar.h"
#include "mono/sgen/sgen-client.h"

static int workers_num;
static WorkerData *workers_data;

static SgenSectionGrayQueue workers_distribute_gray_queue;
static gboolean workers_distribute_gray_queue_inited;

/*
 * Allowed transitions:
 *
 * | from \ to          | NOT WORKING | WORKING | WORK ENQUEUED | NURSERY COLLECTION |
 * |--------------------+-------------+---------+---------------+--------------------|
 * | NOT WORKING        | -           | -       | main          | main               |
 * | WORKING            | worker      | -       | main          | main               |
 * | WORK ENQUEUED      | -           | worker  | -             | main               |
 * | NURSERY COLLECTION | -           | -       | main          | -                  |
 *
 * The WORK ENQUEUED state guarantees that the worker thread will inspect the queue again at
 * least once.  Only after looking at the queue will it go back to WORKING, and then,
 * eventually, to NOT WORKING.  After enqueuing work the main thread transitions the state
 * to WORK ENQUEUED.  Signalling the worker thread to wake up is only necessary if the old
 * state was NOT WORKING.
 */

enum {
	STATE_NOT_WORKING,
	STATE_WORKING,
	STATE_WORK_ENQUEUED,
	STATE_NURSERY_COLLECTION
};

typedef gint32 State;

static volatile State workers_state;

static SgenObjectOperations * volatile idle_func_object_ops;

static guint64 stat_workers_num_finished;

static gboolean
set_state (State old_state, State new_state)
{
	SGEN_ASSERT (0, old_state != new_state, "Why are we transitioning to the same state?");
	if (new_state == STATE_NOT_WORKING)
		SGEN_ASSERT (0, old_state == STATE_WORKING, "We can only transition to NOT WORKING from WORKING");
	else if (new_state == STATE_WORKING)
		SGEN_ASSERT (0, old_state == STATE_WORK_ENQUEUED, "We can only transition to WORKING from WORK ENQUEUED");
	if (new_state == STATE_NOT_WORKING || new_state == STATE_WORKING)
		SGEN_ASSERT (6, sgen_thread_pool_is_thread_pool_thread (mono_native_thread_id_get ()), "Only the worker thread is allowed to transition to NOT_WORKING or WORKING");

	return InterlockedCompareExchange (&workers_state, new_state, old_state) == old_state;
}

static void
assert_nursery_collection (State state)
{
	SGEN_ASSERT (0, state == STATE_NURSERY_COLLECTION, "Must be in the nursery collection state");
}

static gboolean
state_is_working_or_enqueued (State state)
{
	return state == STATE_WORKING || state == STATE_WORK_ENQUEUED;
}

static void
workers_signal_enqueue_work (gboolean from_nursery_collection)
{
	State old_state;
	gboolean did_set_state;

	do {
		old_state = workers_state;

		if (from_nursery_collection)
			assert_nursery_collection (old_state);
		else
			SGEN_ASSERT (0, old_state != STATE_NURSERY_COLLECTION, "If we're not in a nursery collection, how come the state is NURSERY COLLECTION?");

		if (old_state == STATE_WORK_ENQUEUED)
			break;

		did_set_state = set_state (old_state, STATE_WORK_ENQUEUED);
		if (from_nursery_collection)
			SGEN_ASSERT (0, did_set_state, "Nobody else should be mutating the state");
	} while (!did_set_state);

	if (!state_is_working_or_enqueued (old_state))
		sgen_thread_pool_idle_signal ();
}

void
sgen_workers_ensure_awake (void)
{
	SGEN_ASSERT (0, workers_state != STATE_NURSERY_COLLECTION, "Can't wake workers during nursery collection");
	workers_signal_enqueue_work (FALSE);
}

static void
worker_try_finish (void)
{
	State old_state;

	++stat_workers_num_finished;

	do {
		old_state = workers_state;

		SGEN_ASSERT (0, old_state != STATE_NOT_WORKING, "How did we get from doing idle work to NOT WORKING without setting it ourselves?");
		if (old_state == STATE_NURSERY_COLLECTION)
			return;
		if (old_state == STATE_WORK_ENQUEUED)
			return;
		SGEN_ASSERT (0, old_state == STATE_WORKING, "What other possibility is there?");

		/* We are the last thread to go to sleep. */
	} while (!set_state (old_state, STATE_NOT_WORKING));
}

static gboolean
collection_needs_workers (void)
{
	return sgen_collection_is_concurrent ();
}

void
sgen_workers_enqueue_job (SgenThreadPoolJob *job)
{
	if (!collection_needs_workers ()) {
		job->func (NULL, job);
		sgen_thread_pool_job_free (job);
		return;
	}

	sgen_thread_pool_job_enqueue (job);
}

void
sgen_workers_wait_for_jobs_finished (void)
{
	sgen_thread_pool_wait_for_all_jobs ();
	/*
	 * If the idle task was never triggered or it finished before the last job did and
	 * then didn't get triggered again, we might end up in the situation of having
	 * something in the gray queue yet the idle task not working.  The easiest way to
	 * make sure this doesn't stay that way is to just trigger it again after all jobs
	 * have finished.
	 */
	sgen_workers_ensure_awake ();
}

void
sgen_workers_signal_start_nursery_collection_and_wait (void)
{
	State old_state;

	do {
		old_state = workers_state;

		if (old_state != STATE_NOT_WORKING)
			SGEN_ASSERT (0, old_state != STATE_NURSERY_COLLECTION, "Why are we transitioning to NURSERY COLLECTION when we're already there?");
	} while (!set_state (old_state, STATE_NURSERY_COLLECTION));

	sgen_thread_pool_idle_wait ();

	assert_nursery_collection (workers_state);
}

void
sgen_workers_signal_finish_nursery_collection (void)
{
	assert_nursery_collection (workers_state);
	workers_signal_enqueue_work (TRUE);
}

static gboolean
workers_get_work (WorkerData *data)
{
	SgenMajorCollector *major;

	g_assert (sgen_gray_object_queue_is_empty (&data->private_gray_queue));

	/* If we're concurrent, steal from the workers distribute gray queue. */
	major = sgen_get_major_collector ();
	if (major->is_concurrent) {
		GrayQueueSection *section = sgen_section_gray_queue_dequeue (&workers_distribute_gray_queue);
		if (section) {
			sgen_gray_object_enqueue_section (&data->private_gray_queue, section);
			return TRUE;
		}
	}

	/* Nobody to steal from */
	g_assert (sgen_gray_object_queue_is_empty (&data->private_gray_queue));
	return FALSE;
}

static void
concurrent_enqueue_check (GCObject *obj)
{
	g_assert (sgen_concurrent_collection_in_progress ());
	g_assert (!sgen_ptr_in_nursery (obj));
	g_assert (SGEN_LOAD_VTABLE (obj));
}

static void
init_private_gray_queue (WorkerData *data)
{
	sgen_gray_object_queue_init (&data->private_gray_queue,
			sgen_get_major_collector ()->is_concurrent ? concurrent_enqueue_check : NULL);
}

static void
thread_pool_init_func (void *data_untyped)
{
	WorkerData *data = data_untyped;
	SgenMajorCollector *major = sgen_get_major_collector ();

	sgen_client_thread_register_worker ();

	if (!major->is_concurrent)
		return;

	init_private_gray_queue (data);
}

static gboolean
continue_idle_func (void)
{
	return state_is_working_or_enqueued (workers_state);
}

static void
marker_idle_func (void *data_untyped)
{
	WorkerData *data = data_untyped;

	if (!continue_idle_func ())
		return;

	SGEN_ASSERT (0, sgen_concurrent_collection_in_progress (), "The worker should only mark in concurrent collections.");
	SGEN_ASSERT (0, sgen_get_current_collection_generation () != GENERATION_NURSERY, "Why are we doing work while there's a nursery collection happening?");

	if (workers_state == STATE_WORK_ENQUEUED) {
		set_state (STATE_WORK_ENQUEUED, STATE_WORKING);
		SGEN_ASSERT (0, workers_state != STATE_NOT_WORKING, "How did we get from WORK ENQUEUED to NOT WORKING?");
	}

	if (!sgen_gray_object_queue_is_empty (&data->private_gray_queue) || workers_get_work (data)) {
		ScanCopyContext ctx = CONTEXT_FROM_OBJECT_OPERATIONS (idle_func_object_ops, &data->private_gray_queue);

		SGEN_ASSERT (0, !sgen_gray_object_queue_is_empty (&data->private_gray_queue), "How is our gray queue empty if we just got work?");

		sgen_drain_gray_stack (32, ctx);
	} else {
		worker_try_finish ();
	}
}

static void
init_distribute_gray_queue (void)
{
	if (workers_distribute_gray_queue_inited) {
		g_assert (sgen_section_gray_queue_is_empty (&workers_distribute_gray_queue));
		g_assert (workers_distribute_gray_queue.locked);
		return;
	}

	sgen_section_gray_queue_init (&workers_distribute_gray_queue, TRUE,
			sgen_get_major_collector ()->is_concurrent ? concurrent_enqueue_check : NULL);
	workers_distribute_gray_queue_inited = TRUE;
}

void
sgen_workers_init_distribute_gray_queue (void)
{
	SGEN_ASSERT (0, sgen_get_major_collector ()->is_concurrent && collection_needs_workers (),
			"Why should we init the distribute gray queue if we don't need it?");
	init_distribute_gray_queue ();
}

void
sgen_workers_init (int num_workers)
{
	int i;
	void **workers_data_ptrs = alloca(num_workers * sizeof(void *));

	if (!sgen_get_major_collector ()->is_concurrent) {
		sgen_thread_pool_init (num_workers, thread_pool_init_func, NULL, NULL, NULL);
		return;
	}

	//g_print ("initing %d workers\n", num_workers);

	workers_num = num_workers;

	workers_data = sgen_alloc_internal_dynamic (sizeof (WorkerData) * num_workers, INTERNAL_MEM_WORKER_DATA, TRUE);
	memset (workers_data, 0, sizeof (WorkerData) * num_workers);

	init_distribute_gray_queue ();

	for (i = 0; i < workers_num; ++i)
		workers_data_ptrs [i] = (void *) &workers_data [i];

	sgen_thread_pool_init (num_workers, thread_pool_init_func, marker_idle_func, continue_idle_func, workers_data_ptrs);

	mono_counters_register ("# workers finished", MONO_COUNTER_GC | MONO_COUNTER_ULONG, &stat_workers_num_finished);
}

void
sgen_workers_start_all_workers (SgenObjectOperations *object_ops)
{
	if (!collection_needs_workers ())
		return;

	idle_func_object_ops = object_ops;
	mono_memory_write_barrier ();

	workers_signal_enqueue_work (FALSE);
}

void
sgen_workers_join (void)
{
	int i;

	SGEN_ASSERT (0, workers_state != STATE_NURSERY_COLLECTION, "Can't be in nursery collection when joining");

	if (!collection_needs_workers ())
		return;

	sgen_thread_pool_wait_for_all_jobs ();
	sgen_thread_pool_idle_wait ();
	SGEN_ASSERT (0, workers_state == STATE_NOT_WORKING, "Can only signal enqueue work when in no work state");

	/* At this point all the workers have stopped. */

	SGEN_ASSERT (0, sgen_section_gray_queue_is_empty (&workers_distribute_gray_queue), "Why is there still work left to do?");
	for (i = 0; i < workers_num; ++i)
		SGEN_ASSERT (0, sgen_gray_object_queue_is_empty (&workers_data [i].private_gray_queue), "Why is there still work left to do?");
}

gboolean
sgen_workers_all_done (void)
{
	return workers_state == STATE_NOT_WORKING;
}

/* Must only be used for debugging */
gboolean
sgen_workers_are_working (void)
{
	return state_is_working_or_enqueued (workers_state);
}

void
sgen_workers_wait (void)
{
	sgen_thread_pool_idle_wait ();
	SGEN_ASSERT (0, sgen_workers_all_done (), "Why are the workers not done after we wait for them?");
}

SgenSectionGrayQueue*
sgen_workers_get_distribute_section_gray_queue (void)
{
	return &workers_distribute_gray_queue;
}

#endif
