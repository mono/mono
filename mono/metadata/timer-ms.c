
#include <glib.h>

#include "timer-ms.h"

#include "class-internals.h"
#include "gc-internal.h"
#include "object.h"
#include "object-internals.h"
#include "threads-types.h"
#include "threadpool-internals.h"
#include "utils/atomic.h"
#include "utils/mono-time.h"
#include "utils/mono-semaphore.h"

#define INVALID_HANDLE ((gpointer) -1)

enum {
	STATUS_NOT_INITIALIZED,
	STATUS_INITIALIZING,
	STATUS_INITIALIZED,
	STATUS_CLEANING_UP,
	STATUS_CLEANED_UP,
};

enum {
	TIMER_STATUS_REQUESTED,
	TIMER_STATUS_WAITING_FOR_REQUEST,
	TIMER_STATUS_NOT_RUNNING,
};

typedef struct {
	MonoDomain *domain;
	gint64 next_due_ticks;
	gint64 last_fire_ticks;
	gboolean running;
} TimerDomain;

typedef struct {
	MonoSemType wakeup_sem;

	GPtrArray *domains; // TimerDomain* []
	mono_mutex_t domains_lock;
} Timer;

static gint32 status = STATUS_NOT_INITIALIZED;
static gint32 timer_status = TIMER_STATUS_NOT_RUNNING;

static Timer *timer = NULL;

static void
ensure_initialized (void)
{
	if (status >= STATUS_INITIALIZED)
		return;
	if (status == STATUS_INITIALIZING || InterlockedCompareExchange (&status, STATUS_INITIALIZING, STATUS_NOT_INITIALIZED) != STATUS_NOT_INITIALIZED) {
		while (status == STATUS_INITIALIZING)
			mono_thread_info_yield ();
		g_assert (status >= STATUS_INITIALIZED);
		return;
	}

	g_assert (!timer);
	timer = g_new0 (Timer, 1);
	g_assert (timer);

	MONO_SEM_INIT (&timer->wakeup_sem, 0);

	timer->domains = g_ptr_array_new ();
	mono_mutex_init (&timer->domains_lock);

	status = STATUS_INITIALIZED;
}

static gboolean
timer_should_keep_running (void)
{
	g_assert (timer_status == TIMER_STATUS_WAITING_FOR_REQUEST || timer_status == TIMER_STATUS_REQUESTED);

	if (InterlockedExchange (&timer_status, TIMER_STATUS_WAITING_FOR_REQUEST) == TIMER_STATUS_WAITING_FOR_REQUEST) {
		guint i;
		gboolean empty = TRUE;

		mono_mutex_lock (&timer->domains_lock);
		for (i = 0; i < timer->domains->len; ++i) {
			TimerDomain *tdomain = g_ptr_array_index (timer->domains, i);
			if (tdomain) {
				empty = FALSE;
				break;
			}
		}
		mono_mutex_unlock (&timer->domains_lock);

		if (empty || mono_runtime_is_shutting_down ()) {
			if (InterlockedCompareExchange (&timer_status, TIMER_STATUS_NOT_RUNNING, TIMER_STATUS_WAITING_FOR_REQUEST) == TIMER_STATUS_WAITING_FOR_REQUEST)
				return FALSE;
		}
	}

	g_assert (timer_status == TIMER_STATUS_WAITING_FOR_REQUEST || timer_status == TIMER_STATUS_REQUESTED);

	return TRUE;
}

static void
timer_thread (gpointer data)
{
	static MonoClass *timer_queue_class = NULL;
	static MonoMethod *app_domain_timer_callback_method = NULL;
	MonoInternalThread *thread = mono_thread_internal_current ();
	guint i, len;

	ves_icall_System_Threading_Thread_SetState (thread, ThreadState_Background);

	g_assert (status >= STATUS_INITIALIZED);

	if (!timer_queue_class)
		timer_queue_class = mono_class_from_name (mono_defaults.corlib, "System.Threading", "TimerQueue");
	g_assert (timer_queue_class);

	if (!app_domain_timer_callback_method)
		app_domain_timer_callback_method = mono_class_get_method_from_name (timer_queue_class, "AppDomainTimerCallback", 0);
	g_assert (app_domain_timer_callback_method);

	while (timer_should_keep_running ()) {
		gint64 next_due_ticks;

		if ((thread->state & (ThreadState_StopRequested | ThreadState_SuspendRequested)) != 0)
			mono_thread_interruption_checkpoint ();

		mono_mutex_lock (&timer->domains_lock);

		next_due_ticks = G_MAXINT64;
		for (i = 0, len = timer->domains->len; i < len; ++i) {
			TimerDomain *tdomain = g_ptr_array_index (timer->domains, i);
			if (tdomain && tdomain->next_due_ticks >= tdomain->last_fire_ticks)
				next_due_ticks = MIN (next_due_ticks, tdomain->next_due_ticks);
		}

		mono_mutex_unlock (&timer->domains_lock);

		if (mono_100ns_ticks () < next_due_ticks) {
			mono_gc_set_skip_thread (TRUE);
			MONO_SEM_TIMEDWAIT_ALERTABLE (&timer->wakeup_sem, (next_due_ticks - mono_100ns_ticks ()) / 10 / 1000, TRUE);
			mono_gc_set_skip_thread (FALSE);

			if (mono_100ns_ticks () < next_due_ticks) {
				/* we might have been woken up by timer_wakeup */
				continue;
			}
		}

		mono_mutex_lock (&timer->domains_lock);

		for (i = 0; i < timer->domains->len; ++i) {
			TimerDomain *tdomain = g_ptr_array_index (timer->domains, i);

			if (!tdomain)
				continue;

			if (mono_domain_is_unloading (tdomain->domain))
				continue;

			if (mono_100ns_ticks () < tdomain->next_due_ticks)
				continue;

			tdomain->last_fire_ticks = mono_100ns_ticks ();
			tdomain->running = TRUE;

			mono_mutex_unlock (&timer->domains_lock);

			mono_thread_push_appdomain_ref (tdomain->domain);
			if (mono_domain_set (tdomain->domain, FALSE)) {
				MonoObject *exc = NULL;
				mono_runtime_invoke (app_domain_timer_callback_method, NULL, NULL, &exc);
				if (exc)
					mono_internal_thread_unhandled_exception (exc);

				mono_thread_clr_state (thread , ~ThreadState_Background);
				if (!mono_thread_test_state (thread , ThreadState_Background))
					ves_icall_System_Threading_Thread_SetState (thread, ThreadState_Background);

				mono_domain_set (mono_get_root_domain (), TRUE);
			}
			mono_thread_pop_appdomain_ref ();

			mono_mutex_lock (&timer->domains_lock);

			tdomain->running = FALSE;
		}

		mono_mutex_unlock (&timer->domains_lock);
	}
}

static void
timer_ensure_running (void)
{
	g_assert (timer);

	for (;;) {
		switch (timer_status) {
		case TIMER_STATUS_REQUESTED:
			return;
		case TIMER_STATUS_WAITING_FOR_REQUEST:
			InterlockedCompareExchange (&timer_status, TIMER_STATUS_REQUESTED, TIMER_STATUS_WAITING_FOR_REQUEST);
			break;
		case TIMER_STATUS_NOT_RUNNING:
			if (mono_runtime_is_shutting_down ())
				return;
			if (InterlockedCompareExchange (&timer_status, TIMER_STATUS_REQUESTED, TIMER_STATUS_NOT_RUNNING) == TIMER_STATUS_NOT_RUNNING) {
				if (!mono_thread_create_internal (mono_get_root_domain (), timer_thread, NULL, FALSE, 0))
					timer_status = TIMER_STATUS_NOT_RUNNING;
				return;
			}
			break;
		default:
			g_assert_not_reached ();
		}
	};
}

static void
timer_wakeup (void)
{
	g_assert (timer);

	MONO_SEM_POST (&timer->wakeup_sem);
}

static gint64
compute_next_due_ticks (gint64 due_time)
{
	gint64 now = mono_100ns_ticks ();

	/* we should hit this assertion after (G_MAXINT64 / 365 / 24 / 3600 / 1000 / 1000 / 10) = 29274 years */
	g_assert (due_time <= G_MAXINT64 - now);

	return now + due_time;
}

MonoSafeHandle*
ves_icall_System_Threading_TimerQueue_CreateAppDomainTimer (guint32 due_time_ms)
{
	static MonoClass *app_domain_timer_safe_handle_class = NULL;
	MonoSafeHandle *handle;
	TimerDomain *tdomain;
	gint64 next_due_ticks, due_time;
	guint i, len;

	if (!app_domain_timer_safe_handle_class)
		app_domain_timer_safe_handle_class = mono_class_from_name (mono_defaults.corlib, "System.Threading", "TimerQueue/AppDomainTimerSafeHandle");
	g_assert (app_domain_timer_safe_handle_class);

	due_time = due_time_ms * 1000 * 10;
	next_due_ticks = compute_next_due_ticks (due_time);

	ensure_initialized ();

	mono_mutex_lock (&timer->domains_lock);

	tdomain = g_new0 (TimerDomain, 1);
	tdomain->domain = mono_domain_get ();
	tdomain->next_due_ticks = next_due_ticks;

	len = timer->domains->len;
	for (i = 0; i < len; ++i) {
		TimerDomain *tdomain = g_ptr_array_index (timer->domains, i);
		if (!tdomain)
			break;
	}

	if (i < len)
		g_ptr_array_index (timer->domains, i) = tdomain;
	else
		g_ptr_array_add (timer->domains, tdomain);

	mono_mutex_unlock (&timer->domains_lock);

	timer_ensure_running ();
	timer_wakeup ();

	handle = (MonoSafeHandle*) mono_object_new (mono_domain_get (), app_domain_timer_safe_handle_class);
	handle->handle = tdomain;

	return handle;
}

MonoBoolean
ves_icall_System_Threading_TimerQueue_ChangeAppDomainTimer (MonoSafeHandle *handle, guint32 due_time_ms)
{
	TimerDomain *tdomain;
	gint64 next_due_ticks, due_time;

	due_time = due_time_ms * 1000 * 10;
	next_due_ticks = compute_next_due_ticks (due_time);

	ensure_initialized ();

	mono_mutex_lock (&timer->domains_lock);

	g_assert (handle);
	g_assert (handle->handle != NULL);
	g_assert (handle->handle != INVALID_HANDLE);
	tdomain = (TimerDomain*) handle->handle;

	tdomain->next_due_ticks = next_due_ticks;

	mono_mutex_unlock (&timer->domains_lock);

	timer_ensure_running ();
	timer_wakeup ();

	return TRUE;
}

MonoBoolean
ves_icall_System_Threading_TimerQueue_DeleteAppDomainTimer (gpointer handle)
{
	TimerDomain *tdomain;
	gboolean removed, running;

	g_assert (timer);

	mono_mutex_lock (&timer->domains_lock);

	g_assert (handle != NULL);
	g_assert (handle != INVALID_HANDLE);
	tdomain = (TimerDomain*) handle;

	running = tdomain->running;

	removed = g_ptr_array_remove (timer->domains, tdomain);
	g_free (tdomain);

	mono_mutex_unlock (&timer->domains_lock);

	/* we should find the current domain in timer->domains */
	return removed;
}
