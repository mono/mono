/**
 * Functions that are in the (historical) embedding API
 * but must not be used by the runtime. Often
 * just a thin wrapper mono_foo => mono_foo_internal.
 *
 * Copyright 2018 Microsoft
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

// FIXME In order to confirm this is all extern_only,
// a variant of the runtime should be linked without it.

#include "config.h"
#include "class-internals.h"
#include "object-internals.h"
#include "class-init.h"
#include "marshal.h"
#include "object.h"

/* GC handles support
 *
 * A handle can be created to refer to a managed object and either prevent it
 * from being garbage collected or moved or to be able to know if it has been
 * collected or not (weak references).
 * mono_gchandle_new () is used to prevent an object from being garbage collected
 * until mono_gchandle_free() is called. Use a TRUE value for the pinned argument to
 * prevent the object from being moved (this should be avoided as much as possible
 * and this should be used only for shorts periods of time or performance will suffer).
 * To create a weakref use mono_gchandle_new_weakref (): track_resurrection should
 * usually be false (see the GC docs for more details).
 * mono_gchandle_get_target () can be used to get the object referenced by both kinds
 * of handle: for a weakref handle, if an object has been collected, it will return NULL.
 */
uint32_t
mono_gchandle_new (MonoObject *obj, mono_bool pinned)
{
	MONO_EXTERNAL_ONLY (uint32_t, mono_gchandle_new_internal (obj, pinned));
}

uint32_t
mono_gchandle_new_weakref (MonoObject *obj, mono_bool track_resurrection)
{
	MONO_EXTERNAL_ONLY (uint32_t, mono_gchandle_new_weakref_internal (obj, track_resurrection));
}

MonoObject*
mono_gchandle_get_target (uint32_t gchandle)
{
	MONO_EXTERNAL_ONLY (MonoObject*, mono_gchandle_get_target_internal (gchandle));
}

void
mono_gchandle_free (uint32_t gchandle)
{
	MONO_EXTERNAL_ONLY_VOID (mono_gchandle_free_internal (gchandle));
}

/* GC write barriers support */
void
mono_gc_wbarrier_set_field (MonoObject *obj, void* field_ptr, MonoObject* value)
{
	MONO_EXTERNAL_ONLY_VOID (mono_gc_wbarrier_set_field_internal (obj, field_ptr, value));
}

void
mono_gc_wbarrier_set_arrayref (MonoArray *arr, void* slot_ptr, MonoObject* value)
{
	MONO_EXTERNAL_ONLY_VOID (mono_gc_wbarrier_set_arrayref_internal (arr, slot_ptr, value));
}

void
mono_gc_wbarrier_arrayref_copy (void* dest_ptr, void* src_ptr, int count)
{
	MONO_EXTERNAL_ONLY_VOID (mono_gc_wbarrier_arrayref_copy_internal (dest_ptr, src_ptr, count));
}

void
mono_gc_wbarrier_generic_store (void* ptr, MonoObject* value)
{
	MONO_EXTERNAL_ONLY_VOID (mono_gc_wbarrier_generic_store_internal (ptr, value));
}

void
mono_gc_wbarrier_generic_store_atomic (void *ptr, MonoObject *value)
{
	MONO_EXTERNAL_ONLY_VOID (mono_gc_wbarrier_generic_store_atomic_internal (ptr, value));
}

void
mono_gc_wbarrier_generic_nostore (void* ptr)
{
	MONO_EXTERNAL_ONLY_VOID (mono_gc_wbarrier_generic_nostore_internal (ptr));
}

void
mono_gc_wbarrier_value_copy (void* dest, /*const*/ void* src, int count, MonoClass *klass)
{
	MONO_EXTERNAL_ONLY_VOID (mono_gc_wbarrier_value_copy_internal (dest, src, count, klass));
}

void
mono_gc_wbarrier_object_copy (MonoObject* obj, MonoObject *src)
{
	MONO_EXTERNAL_ONLY_VOID (mono_gc_wbarrier_object_copy_internal (obj, src));
}
