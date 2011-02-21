/*
 * mono-wsq.c: work-stealing queue
 *
 * Authors:
 *   Gonzalo Paniagua Javier (gonzalo@novell.com)
 *
 * Copyright (c) 2010 Novell, Inc (http://www.novell.com)
 */

#include <string.h>
#include <mono/metadata/object.h>
#include <mono/metadata/mono-wsq.h>
#include <mono/utils/mono-semaphore.h>
#include <mono/utils/mono-time.h>
#include <glib.h>

#define INITIAL_LENGTH	32
#define WSQ_DEBUG(...)
//#define WSQ_DEBUG(...) g_message(__VA_ARGS__)

#define array_get(_array, _index) mono_array_get(_array, void*, _index)
#define array_length(_array) ((_array)->max_length)

#define NO_KEY ((guint32) -1)
static guint32 wsq_tlskey = NO_KEY;

/* Macros borrowed from domain.c
 * Look there for the rationale behind this
 */
#if (defined(__i386__) || defined(__x86_64__)) && !defined(HOST_WIN32)
#define NO_TLS_SET_VALUE
#endif

#ifdef HAVE_KW_THREAD

static __thread MonoWSQ * tls_local_wsq MONO_TLS_FAST;

#define GET_LOCAL_WSQ() tls_local_wsq

#ifdef NO_TLS_SET_VALUE
#define SET_LOCAL_WSQ(x) do { \
	tls_local_wsq = x; \
} while (FALSE)
#else
#define SET_LOCAL_WSQ(x) do { \
	tls_local_wsq = x; \
	TlsSetValue (wsq_tlskey, x); \
} while (FALSE)
#endif

#else

#define GET_LOCAL_WSQ() ((MonoWSQ *)TlsGetValue (wsq_tlskey))
#define SET_LOCAL_WSQ(x) TlsSetValue (wsq_tlskey, x);

#endif

struct _MonoWSQ {
	gint32 top;
	gint32 bottom;
	gint32 upper_bound;
	MonoArray *queue;
};

#define NO_KEY ((guint32) -1)
static guint32 wsq_tlskey = NO_KEY;

void
mono_wsq_init ()
{
	wsq_tlskey = TlsAlloc ();
}

void
mono_wsq_cleanup ()
{
	if (wsq_tlskey == NO_KEY)
		return;
	TlsFree (wsq_tlskey);
	wsq_tlskey = NO_KEY;
}

MonoWSQ *
mono_wsq_create ()
{
	MonoWSQ *wsq;
	MonoDomain *root;

	if (wsq_tlskey == NO_KEY)
		return NULL;

	wsq = g_new0 (MonoWSQ, 1);
	MONO_GC_REGISTER_ROOT_SINGLE (wsq->queue);
	root = mono_get_root_domain ();
	wsq->queue = mono_array_new_cached (root, mono_defaults.object_class, INITIAL_LENGTH);
	SET_LOCAL_WSQ(wsq);

	return wsq;
}

void
mono_wsq_destroy (MonoWSQ *wsq)
{
	if (wsq == NULL || wsq->queue == NULL)
		return;

	g_assert (mono_wsq_count (wsq) == 0);
	MONO_GC_UNREGISTER_ROOT (wsq->queue);
	memset (wsq, 0, sizeof (MonoWSQ));
	if (wsq_tlskey != NO_KEY && GET_LOCAL_WSQ() == wsq)
		SET_LOCAL_WSQ (NULL);
	g_free (wsq);
}

gint
mono_wsq_count (MonoWSQ *wsq)
{
	if (!wsq)
		return 0;
	return wsq->bottom - wsq->top;
}

gboolean
mono_wsq_local_push (void *obj)
{
	MonoWSQ *wsq;
	gint32 b;
	MonoArray* a;
	gint size;

	if (obj == NULL || wsq_tlskey == NO_KEY)
		return FALSE;

	wsq = GET_LOCAL_WSQ();
	if (wsq == NULL) {
		WSQ_DEBUG ("local_push: no wsq\n");
		return FALSE;
	}

	b = wsq->bottom;
	a = wsq->queue;
	size = array_length (a);

	if (b - wsq->upper_bound >= size - 1) {
		MonoArray *new_array;
		gint new_size;
		int i;

		wsq->upper_bound = wsq->top;
		new_size = size * 2;

		new_array = mono_array_new_cached (mono_get_root_domain (), mono_defaults.object_class, new_size);
		for (i = wsq->upper_bound; i < b; ++i)
			mono_array_setref (new_array, i, array_get (a, i % size));

		wsq->queue = new_array;
	}

	mono_array_setref (wsq->queue, b % size, obj);
	InterlockedIncrement (&wsq->bottom);

	WSQ_DEBUG ("local_push: push successfull\n");

	return TRUE;
}

gboolean
mono_wsq_local_pop (MonoWSQ *wsq, void **ptr)
{
	gint32 b, t;
	gint size;
	MonoArray* a;

	if (ptr == NULL || wsq == NULL)
		return FALSE;

	b = --wsq->bottom;
	a = wsq->queue;
	t = wsq->top;
	size = b - t;

	if (size < 0) {
		wsq->bottom = t;
		return FALSE;
	}

	*ptr = array_get (a, b % array_length (a));
	if (size > 0)
		return TRUE;

	wsq->bottom = t + 1;
	if (InterlockedCompareExchange (&wsq->top, t + 1, t) != t)
		return FALSE;

	return TRUE;
}

void
mono_wsq_try_steal (MonoWSQ *wsq, void **ptr, guint32 ms_timeout)
{
	gint32 t, b;
	gint size;
	MonoArray* a;
	guint32 start_ticks = mono_msec_ticks ();

	if (wsq == NULL || ptr == NULL || *ptr != NULL)
		return;

	do {
		t = wsq->top;
		b = wsq->bottom;

		if (b - t <= 0)
			return;

		if (InterlockedCompareExchange (&wsq->top, t + 1, t) == t)
			break;

		if (!ms_timeout || mono_msec_ticks () - start_ticks < ms_timeout)
			return;
	} while (TRUE);

	a = wsq->queue;
	size = array_length (a);
	*ptr = array_get (a, t % size);
}
