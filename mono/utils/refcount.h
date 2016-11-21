
#ifndef __MONO_UTILS_REFCOUNT_H__
#define __MONO_UTILS_REFCOUNT_H__

#include <glib.h>
#include <config.h>

#include "atomic.h"

/*
 * Mechanism for ref-counting which tries to be as user-friendly as possible. Instead of being a wrapper around
 * user-provided data, it is embedded into the user data.
 *
 * This introduces some constraints on the MonoRefCount field:
 *  - it needs to be called "ref"
 *  - it cannot be a pointer
 */

typedef struct {
	guint32 ref;
	void (*destructor) (gpointer data);
} MonoRefCount;

#define mono_refcount_init(v,destructor) do { mono_refcount_initialize (&(v)->ref, (destructor)); } while (0)
#define mono_refcount_inc(v) (mono_refcount_increment (&(v)->ref),(v))
#define mono_refcount_dec(v) do { mono_refcount_decrement (&(v)->ref); } while (0)

static inline void
mono_refcount_initialize (MonoRefCount *refcount, void (*destructor) (gpointer data))
{
	refcount->ref = 1;
	refcount->destructor = destructor;
}

static inline void
mono_refcount_increment (MonoRefCount *refcount)
{
	guint32 oldref, newref;

	g_assert (refcount);

	do {
		oldref = refcount->ref;
		if (oldref == 0)
			g_error ("%s: cannot increment a ref with value 0", __func__);

		newref = oldref + 1;
	} while (InterlockedCompareExchange ((gint32*) &refcount->ref, newref, oldref) != oldref);
}

static inline void
mono_refcount_decrement (MonoRefCount *refcount)
{
	guint32 oldref, newref;

	g_assert (refcount);

	do {
		oldref = refcount->ref;
		if (oldref == 0)
			g_error ("%s: cannot decrement a ref with value 0", __func__);

		newref = oldref - 1;
	} while (InterlockedCompareExchange ((gint32*) &refcount->ref, newref, oldref) != oldref);

	if (newref == 0 && refcount->destructor)
		refcount->destructor ((gpointer) refcount);
}

#endif /* __MONO_UTILS_REFCOUNT_H__ */
