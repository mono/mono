/*
 * object.c: Object creation for the Mono runtime
 *
 * Author:
 *   Miguel de Icaza (miguel@ximian.com)
 *   Paolo Molaro (lupus@ximian.com)
 *
 * Copyright 2001-2003 Ximian, Inc (http://www.ximian.com)
 * Copyright 2004-2011 Novell, Inc (http://www.novell.com)
 * Copyright 2001 Xamarin Inc (http://www.xamarin.com)
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */
#include <config.h>
#ifdef HAVE_ALLOCA_H
#include <alloca.h>
#endif
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <mono/metadata/mono-endian.h>
#include <mono/metadata/tabledefs.h>
#include <mono/metadata/tokentype.h>
#include <mono/metadata/loader.h>
#include <mono/metadata/object.h>
#include <mono/metadata/gc-internals.h>
#include <mono/metadata/exception.h>
#include <mono/metadata/exception-internals.h>
#include <mono/metadata/domain-internals.h>
#include "mono/metadata/metadata-internals.h"
#include "mono/metadata/class-internals.h"
#include <mono/metadata/assembly.h>
#include <mono/metadata/marshal.h>
#include "mono/metadata/debug-helpers.h"
#include <mono/metadata/threads.h>
#include <mono/metadata/threads-types.h>
#include <mono/metadata/environment.h>
#include "mono/metadata/profiler-private.h"
#include "mono/metadata/security-manager.h"
#include <mono/metadata/verify-internals.h>
#include <mono/metadata/reflection-internals.h>
#include <mono/metadata/w32event.h>
#include <mono/utils/strenc.h>
#include <mono/utils/mono-counters.h>
#include <mono/utils/mono-error-internals.h>
#include <mono/utils/mono-memory-model.h>
#include <mono/utils/checked-build.h>
#include <mono/utils/mono-threads.h>
#include <mono/utils/mono-threads-coop.h>
#include "cominterop.h"
#include <mono/utils/w32api.h>

static void
get_default_field_value (MonoDomain* domain, MonoClassField *field, void *value, MonoError *error);

static MonoString*
mono_ldstr_metadata_sig (MonoDomain *domain, const char* sig, MonoError *error);

static void
free_main_args (void);

static char *
mono_string_to_utf8_internal (MonoMemPool *mp, MonoImage *image, MonoString *s, gboolean ignore_error, MonoError *error);

static void
array_full_copy_unchecked_size (MonoArray *src, MonoArray *dest, MonoClass *klass, uintptr_t size);

static MonoMethod*
class_get_virtual_method (MonoClass *klass, MonoMethod *method, gboolean is_proxy, MonoError *error);

/* Class lazy loading functions */
static GENERATE_GET_CLASS_WITH_CACHE (pointer, "System.Reflection", "Pointer")
static GENERATE_GET_CLASS_WITH_CACHE (remoting_services, "System.Runtime.Remoting", "RemotingServices")
static GENERATE_GET_CLASS_WITH_CACHE (unhandled_exception_event_args, "System", "UnhandledExceptionEventArgs")
static GENERATE_GET_CLASS_WITH_CACHE (sta_thread_attribute, "System", "STAThreadAttribute")
static GENERATE_GET_CLASS_WITH_CACHE (activation_services, "System.Runtime.Remoting.Activation", "ActivationServices")


#define ldstr_lock() mono_os_mutex_lock (&ldstr_section)
#define ldstr_unlock() mono_os_mutex_unlock (&ldstr_section)
static mono_mutex_t ldstr_section;


/**
 * mono_runtime_object_init:
 * @this_obj: the object to initialize
 *
 * This function calls the zero-argument constructor (which must
 * exist) for the given object.
 */
void
mono_runtime_object_init (MonoObject *this_obj)
{
	MonoError error;
	mono_runtime_object_init_checked (this_obj, &error);
	mono_error_assert_ok (&error);
}

/**
 * mono_runtime_object_init_checked:
 * @this_obj: the object to initialize
 * @error: set on error.
 *
 * This function calls the zero-argument constructor (which must
 * exist) for the given object and returns TRUE on success, or FALSE
 * on error and sets @error.
 */
gboolean
mono_runtime_object_init_checked (MonoObject *this_obj, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoMethod *method = NULL;
	MonoClass *klass = this_obj->vtable->klass;

	error_init (error);
	method = mono_class_get_method_from_name (klass, ".ctor", 0);
	if (!method)
		g_error ("Could not lookup zero argument constructor for class %s", mono_type_get_full_name (klass));

	if (method->klass->valuetype)
		this_obj = (MonoObject *)mono_object_unbox (this_obj);

	mono_runtime_invoke_checked (method, this_obj, NULL, error);
	return is_ok (error);
}

/* The pseudo algorithm for type initialization from the spec
Note it doesn't say anything about domains - only threads.

2. If the type is initialized you are done.
2.1. If the type is not yet initialized, try to take an 
     initialization lock.  
2.2. If successful, record this thread as responsible for 
     initializing the type and proceed to step 2.3.
2.2.1. If not, see whether this thread or any thread 
     waiting for this thread to complete already holds the lock.
2.2.2. If so, return since blocking would create a deadlock.  This thread 
     will now see an incompletely initialized state for the type, 
     but no deadlock will arise.
2.2.3  If not, block until the type is initialized then return.
2.3 Initialize the parent type and then all interfaces implemented 
    by this type.
2.4 Execute the type initialization code for this type.
2.5 Mark the type as initialized, release the initialization lock, 
    awaken any threads waiting for this type to be initialized, 
    and return.

*/

typedef struct
{
	MonoNativeThreadId initializing_tid;
	guint32 waiting_count;
	gboolean done;
	MonoCoopMutex mutex;
	/* condvar used to wait for 'done' becoming TRUE */
	MonoCoopCond cond;
} TypeInitializationLock;

/* for locking access to type_initialization_hash and blocked_thread_hash */
static MonoCoopMutex type_initialization_section;

static inline void
mono_type_initialization_lock (void)
{
	/* The critical sections protected by this lock in mono_runtime_class_init_full () can block */
	mono_coop_mutex_lock (&type_initialization_section);
}

static inline void
mono_type_initialization_unlock (void)
{
	mono_coop_mutex_unlock (&type_initialization_section);
}

static void
mono_type_init_lock (TypeInitializationLock *lock)
{
	MONO_REQ_GC_NEUTRAL_MODE;

	mono_coop_mutex_lock (&lock->mutex);
}

static void
mono_type_init_unlock (TypeInitializationLock *lock)
{
	mono_coop_mutex_unlock (&lock->mutex);
}

/* from vtable to lock */
static GHashTable *type_initialization_hash;

/* from thread id to thread id being waited on */
static GHashTable *blocked_thread_hash;

/* Main thread */
static MonoThread *main_thread;

/* Functions supplied by the runtime */
static MonoRuntimeCallbacks callbacks;

/**
 * mono_thread_set_main:
 * @thread: thread to set as the main thread
 *
 * This function can be used to instruct the runtime to treat @thread
 * as the main thread, ie, the thread that would normally execute the Main()
 * method. This basically means that at the end of @thread, the runtime will
 * wait for the existing foreground threads to quit and other such details.
 */
void
mono_thread_set_main (MonoThread *thread)
{
	MONO_REQ_GC_UNSAFE_MODE;

	static gboolean registered = FALSE;

	if (!registered) {
		MONO_GC_REGISTER_ROOT_SINGLE (main_thread, MONO_ROOT_SOURCE_THREADING, "main thread object");
		registered = TRUE;
	}

	main_thread = thread;
}

MonoThread*
mono_thread_get_main (void)
{
	MONO_REQ_GC_UNSAFE_MODE;

	return main_thread;
}

void
mono_type_initialization_init (void)
{
	mono_coop_mutex_init_recursive (&type_initialization_section);
	type_initialization_hash = g_hash_table_new (NULL, NULL);
	blocked_thread_hash = g_hash_table_new (NULL, NULL);
	mono_os_mutex_init_recursive (&ldstr_section);
}

void
mono_type_initialization_cleanup (void)
{
#if 0
	/* This is causing race conditions with
	 * mono_release_type_locks
	 */
	mono_coop_mutex_destroy (&type_initialization_section);
	g_hash_table_destroy (type_initialization_hash);
	type_initialization_hash = NULL;
#endif
	mono_os_mutex_destroy (&ldstr_section);
	g_hash_table_destroy (blocked_thread_hash);
	blocked_thread_hash = NULL;

	free_main_args ();
}

/**
 * get_type_init_exception_for_vtable:
 *
 *   Return the stored type initialization exception for VTABLE.
 */
static MonoException*
get_type_init_exception_for_vtable (MonoVTable *vtable)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoError error;
	MonoDomain *domain = vtable->domain;
	MonoClass *klass = vtable->klass;
	MonoException *ex;
	gchar *full_name;

	if (!vtable->init_failed)
		g_error ("Trying to get the init exception for a non-failed vtable of class %s", mono_type_get_full_name (klass));
	
	/* 
	 * If the initializing thread was rudely aborted, the exception is not stored
	 * in the hash.
	 */
	ex = NULL;
	mono_domain_lock (domain);
	if (domain->type_init_exception_hash)
		ex = (MonoException *)mono_g_hash_table_lookup (domain->type_init_exception_hash, klass);
	mono_domain_unlock (domain);

	if (!ex) {
		if (klass->name_space && *klass->name_space)
			full_name = g_strdup_printf ("%s.%s", klass->name_space, klass->name);
		else
			full_name = g_strdup (klass->name);
		ex = mono_get_exception_type_initialization_checked (full_name, NULL, &error);
		g_free (full_name);
		return_val_if_nok (&error, NULL);
	}

	return ex;
}

/*
 * mono_runtime_class_init:
 * @vtable: vtable that needs to be initialized
 *
 * This routine calls the class constructor for @vtable.
 */
void
mono_runtime_class_init (MonoVTable *vtable)
{
	MONO_REQ_GC_UNSAFE_MODE;
	MonoError error;

	mono_runtime_class_init_full (vtable, &error);
	mono_error_assert_ok (&error);
}

/*
 * Returns TRUE if the lock was freed.
 * LOCKING: Caller should hold type_initialization_lock.
 */
static gboolean
unref_type_lock (TypeInitializationLock *lock)
{
	--lock->waiting_count;
	if (lock->waiting_count == 0) {
		mono_coop_mutex_destroy (&lock->mutex);
		mono_coop_cond_destroy (&lock->cond);
		g_free (lock);
		return TRUE;
	} else {
		return FALSE;
	}
}

/**
 * mono_runtime_class_init_full:
 * @vtable that neeeds to be initialized
 * @error set on error
 *
 * returns TRUE if class constructor .cctor has been initialized successfully, or FALSE otherwise and sets @error.
 * 
 */
gboolean
mono_runtime_class_init_full (MonoVTable *vtable, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoMethod *method = NULL;
	MonoClass *klass;
	gchar *full_name;
	MonoDomain *domain = vtable->domain;
	TypeInitializationLock *lock;
	MonoNativeThreadId tid;
	int do_initialization = 0;
	MonoDomain *last_domain = NULL;
	MonoException * pending_tae = NULL;

	error_init (error);

	if (vtable->initialized)
		return TRUE;

	klass = vtable->klass;

	if (!klass->image->checked_module_cctor) {
		mono_image_check_for_module_cctor (klass->image);
		if (klass->image->has_module_cctor) {
			MonoClass *module_klass;
			MonoVTable *module_vtable;

			module_klass = mono_class_get_checked (klass->image, MONO_TOKEN_TYPE_DEF | 1, error);
			if (!module_klass) {
				return FALSE;
			}
				
			module_vtable = mono_class_vtable_full (vtable->domain, module_klass, error);
			if (!module_vtable)
				return FALSE;
			if (!mono_runtime_class_init_full (module_vtable, error))
				return FALSE;
		}
	}
	method = mono_class_get_cctor (klass);
	if (!method) {
		vtable->initialized = 1;
		return TRUE;
	}

	tid = mono_native_thread_id_get ();

	/*
	 * Due some preprocessing inside a global lock. If we are the first thread
	 * trying to initialize this class, create a separate lock+cond var, and
	 * acquire it before leaving the global lock. The other threads will wait
	 * on this cond var.
	 */

	mono_type_initialization_lock ();
	/* double check... */
	if (vtable->initialized) {
		mono_type_initialization_unlock ();
		return TRUE;
	}
	if (vtable->init_failed) {
		mono_type_initialization_unlock ();

		/* The type initialization already failed once, rethrow the same exception */
		mono_error_set_exception_instance (error, get_type_init_exception_for_vtable (vtable));
		return FALSE;
	}
	lock = (TypeInitializationLock *)g_hash_table_lookup (type_initialization_hash, vtable);
	if (lock == NULL) {
		/* This thread will get to do the initialization */
		if (mono_domain_get () != domain) {
			/* Transfer into the target domain */
			last_domain = mono_domain_get ();
			if (!mono_domain_set (domain, FALSE)) {
				vtable->initialized = 1;
				mono_type_initialization_unlock ();
				mono_error_set_exception_instance (error, mono_get_exception_appdomain_unloaded ());
				return FALSE;
			}
		}
		lock = (TypeInitializationLock *)g_malloc0 (sizeof (TypeInitializationLock));
		mono_coop_mutex_init_recursive (&lock->mutex);
		mono_coop_cond_init (&lock->cond);
		lock->initializing_tid = tid;
		lock->waiting_count = 1;
		lock->done = FALSE;
		g_hash_table_insert (type_initialization_hash, vtable, lock);
		do_initialization = 1;
	} else {
		gpointer blocked;
		TypeInitializationLock *pending_lock;

		if (mono_native_thread_id_equals (lock->initializing_tid, tid)) {
			mono_type_initialization_unlock ();
			return TRUE;
		}
		/* see if the thread doing the initialization is already blocked on this thread */
		blocked = GUINT_TO_POINTER (MONO_NATIVE_THREAD_ID_TO_UINT (lock->initializing_tid));
		while ((pending_lock = (TypeInitializationLock*) g_hash_table_lookup (blocked_thread_hash, blocked))) {
			if (mono_native_thread_id_equals (pending_lock->initializing_tid, tid)) {
				if (!pending_lock->done) {
					mono_type_initialization_unlock ();
					return TRUE;
				} else {
					/* the thread doing the initialization is blocked on this thread,
					   but on a lock that has already been freed. It just hasn't got
					   time to awake */
					break;
				}
			}
			blocked = GUINT_TO_POINTER (MONO_NATIVE_THREAD_ID_TO_UINT (pending_lock->initializing_tid));
		}
		++lock->waiting_count;
		/* record the fact that we are waiting on the initializing thread */
		g_hash_table_insert (blocked_thread_hash, GUINT_TO_POINTER (tid), lock);
	}
	mono_type_initialization_unlock ();

	if (do_initialization) {
		MonoException *exc = NULL;

		/* We are holding the per-vtable lock, do the actual initialization */

		mono_threads_begin_abort_protected_block ();
		mono_runtime_try_invoke (method, NULL, NULL, (MonoObject**) &exc, error);
		gboolean got_pending_interrupt = mono_threads_end_abort_protected_block ();

		//exception extracted, error will be set to the right value later
		if (exc == NULL && !mono_error_ok (error))//invoking failed but exc was not set
			exc = mono_error_convert_to_exception (error);
		else
			mono_error_cleanup (error);

		error_init (error);

		/* If the initialization failed, mark the class as unusable. */
		/* Avoid infinite loops */
		if (!(!exc ||
			  (klass->image == mono_defaults.corlib &&
			   !strcmp (klass->name_space, "System") &&
			   !strcmp (klass->name, "TypeInitializationException")))) {
			vtable->init_failed = 1;

			if (klass->name_space && *klass->name_space)
				full_name = g_strdup_printf ("%s.%s", klass->name_space, klass->name);
			else
				full_name = g_strdup (klass->name);

			MonoException *exc_to_throw = mono_get_exception_type_initialization_checked (full_name, exc, error);
			g_free (full_name);

			mono_error_assert_ok (error); //We can't recover from this, no way to fail a type we can't alloc a failure.

			/*
			 * Store the exception object so it could be thrown on subsequent
			 * accesses.
			 */
			mono_domain_lock (domain);
			if (!domain->type_init_exception_hash)
				domain->type_init_exception_hash = mono_g_hash_table_new_type (mono_aligned_addr_hash, NULL, MONO_HASH_VALUE_GC, MONO_ROOT_SOURCE_DOMAIN, "type initialization exceptions table");
			mono_g_hash_table_insert (domain->type_init_exception_hash, klass, exc_to_throw);
			mono_domain_unlock (domain);
		}

		if (last_domain)
			mono_domain_set (last_domain, TRUE);

		/* Signal to the other threads that we are done */
		mono_type_init_lock (lock);
		lock->done = TRUE;
		mono_coop_cond_broadcast (&lock->cond);
		mono_type_init_unlock (lock);

		//This can happen if the cctor self-aborts
		if (exc && mono_object_class (exc) == mono_defaults.threadabortexception_class)
			pending_tae = exc;

		//TAEs are blocked around .cctors, they must escape as soon as no cctor is left to run.
		if (!pending_tae && got_pending_interrupt)
			pending_tae = mono_thread_try_resume_interruption ();
	} else {
		/* this just blocks until the initializing thread is done */
		mono_type_init_lock (lock);
		while (!lock->done)
			mono_coop_cond_wait (&lock->cond, &lock->mutex);
		mono_type_init_unlock (lock);
	}

	/* Do cleanup and setting vtable->initialized inside the global lock again */
	mono_type_initialization_lock ();
	if (!do_initialization)
		g_hash_table_remove (blocked_thread_hash, GUINT_TO_POINTER (tid));
	gboolean deleted = unref_type_lock (lock);
	if (deleted)
		g_hash_table_remove (type_initialization_hash, vtable);
	/* Have to set this here since we check it inside the global lock */
	if (do_initialization && !vtable->init_failed)
		vtable->initialized = 1;
	mono_type_initialization_unlock ();

	//TAE wins over TIE
	if (pending_tae)
		mono_error_set_exception_instance (error, pending_tae);
	else if (vtable->init_failed) {
		/* Either we were the initializing thread or we waited for the initialization */
		mono_error_set_exception_instance (error, get_type_init_exception_for_vtable (vtable));
		return FALSE;
	}
	return TRUE;
}

static
gboolean release_type_locks (gpointer key, gpointer value, gpointer user)
{
	MONO_REQ_GC_NEUTRAL_MODE;

	MonoVTable *vtable = (MonoVTable*)key;

	TypeInitializationLock *lock = (TypeInitializationLock*) value;
	if (mono_native_thread_id_equals (lock->initializing_tid, MONO_UINT_TO_NATIVE_THREAD_ID (GPOINTER_TO_UINT (user))) && !lock->done) {
		lock->done = TRUE;
		/* 
		 * Have to set this since it cannot be set by the normal code in 
		 * mono_runtime_class_init (). In this case, the exception object is not stored,
		 * and get_type_init_exception_for_class () needs to be aware of this.
		 */
		mono_type_init_lock (lock);
		vtable->init_failed = 1;
		mono_coop_cond_broadcast (&lock->cond);
		mono_type_init_unlock (lock);
		gboolean deleted = unref_type_lock (lock);
		if (deleted)
			return TRUE;
	}
	return FALSE;
}

void
mono_release_type_locks (MonoInternalThread *thread)
{
	MONO_REQ_GC_UNSAFE_MODE;

	mono_type_initialization_lock ();
	g_hash_table_foreach_remove (type_initialization_hash, release_type_locks, GUINT_TO_POINTER (thread->tid));
	mono_type_initialization_unlock ();
}

#ifndef DISABLE_REMOTING

static gpointer
create_remoting_trampoline (MonoDomain *domain, MonoMethod *method, MonoRemotingTarget target, MonoError *error)
{
	if (!callbacks.create_remoting_trampoline)
		g_error ("remoting not installed");
	return callbacks.create_remoting_trampoline (domain, method, target, error);
}

#endif

static MonoImtTrampolineBuilder imt_trampoline_builder;
static gboolean always_build_imt_trampolines;

#if (MONO_IMT_SIZE > 32)
#error "MONO_IMT_SIZE cannot be larger than 32"
#endif

void
mono_install_callbacks (MonoRuntimeCallbacks *cbs)
{
	memcpy (&callbacks, cbs, sizeof (*cbs));
}

MonoRuntimeCallbacks*
mono_get_runtime_callbacks (void)
{
	return &callbacks;
}

void
mono_install_imt_trampoline_builder (MonoImtTrampolineBuilder func)
{
	imt_trampoline_builder = func;
}

void
mono_set_always_build_imt_trampolines (gboolean value)
{
	always_build_imt_trampolines = value;
}

/**
 * mono_compile_method:
 * @method: The method to compile.
 *
 * This JIT-compiles the method, and returns the pointer to the native code
 * produced.
 */
gpointer 
mono_compile_method (MonoMethod *method)
{
	MonoError error;
	gpointer result = mono_compile_method_checked (method, &error);
	mono_error_cleanup (&error);
	return result;
}

/**
 * mono_compile_method:
 * @method: The method to compile.
 * @error: set on error.
 *
 * This JIT-compiles the method, and returns the pointer to the native code
 * produced.  On failure returns NULL and sets @error.
 */
gpointer
mono_compile_method_checked (MonoMethod *method, MonoError *error)
{
	gpointer res;

	MONO_REQ_GC_NEUTRAL_MODE

	error_init (error);

	g_assert (callbacks.compile_method);
	res = callbacks.compile_method (method, error);
	return res;
}

gpointer
mono_runtime_create_jump_trampoline (MonoDomain *domain, MonoMethod *method, gboolean add_sync_wrapper, MonoError *error)
{
	gpointer res;

	MONO_REQ_GC_NEUTRAL_MODE;

	error_init (error);
	res = callbacks.create_jump_trampoline (domain, method, add_sync_wrapper, error);
	return res;
}

gpointer
mono_runtime_create_delegate_trampoline (MonoClass *klass)
{
	MONO_REQ_GC_NEUTRAL_MODE

	g_assert (callbacks.create_delegate_trampoline);
	return callbacks.create_delegate_trampoline (mono_domain_get (), klass);
}

/**
 * mono_runtime_free_method:
 * @domain; domain where the method is hosted
 * @method: method to release
 *
 * This routine is invoked to free the resources associated with
 * a method that has been JIT compiled.  This is used to discard
 * methods that were used only temporarily (for example, used in marshalling)
 *
 */
void
mono_runtime_free_method (MonoDomain *domain, MonoMethod *method)
{
	MONO_REQ_GC_NEUTRAL_MODE

	if (callbacks.free_method)
		callbacks.free_method (domain, method);

	mono_method_clear_object (domain, method);

	mono_free_method (method);
}

/*
 * The vtables in the root appdomain are assumed to be reachable by other 
 * roots, and we don't use typed allocation in the other domains.
 */

/* The sync block is no longer a GC pointer */
#define GC_HEADER_BITMAP (0)

#define BITMAP_EL_SIZE (sizeof (gsize) * 8)

static gsize*
compute_class_bitmap (MonoClass *klass, gsize *bitmap, int size, int offset, int *max_set, gboolean static_fields)
{
	MONO_REQ_GC_NEUTRAL_MODE;

	MonoClassField *field;
	MonoClass *p;
	guint32 pos;
	int max_size;

	if (static_fields)
		max_size = mono_class_data_size (klass) / sizeof (gpointer);
	else
		max_size = klass->instance_size / sizeof (gpointer);
	if (max_size > size) {
		g_assert (offset <= 0);
		bitmap = (gsize *)g_malloc0 ((max_size + BITMAP_EL_SIZE - 1) / BITMAP_EL_SIZE * sizeof (gsize));
		size = max_size;
	}

#ifdef HAVE_SGEN_GC
	/*An Ephemeron cannot be marked by sgen*/
	if (!static_fields && klass->image == mono_defaults.corlib && !strcmp ("Ephemeron", klass->name)) {
		*max_set = 0;
		memset (bitmap, 0, size / 8);
		return bitmap;
	}
#endif

	for (p = klass; p != NULL; p = p->parent) {
		gpointer iter = NULL;
		while ((field = mono_class_get_fields (p, &iter))) {
			MonoType *type;

			if (static_fields) {
				if (!(field->type->attrs & (FIELD_ATTRIBUTE_STATIC | FIELD_ATTRIBUTE_HAS_FIELD_RVA)))
					continue;
				if (field->type->attrs & FIELD_ATTRIBUTE_LITERAL)
					continue;
			} else {
				if (field->type->attrs & (FIELD_ATTRIBUTE_STATIC | FIELD_ATTRIBUTE_HAS_FIELD_RVA))
					continue;
			}
			/* FIXME: should not happen, flag as type load error */
			if (field->type->byref)
				break;

			if (static_fields && field->offset == -1)
				/* special static */
				continue;

			pos = field->offset / sizeof (gpointer);
			pos += offset;

			type = mono_type_get_underlying_type (field->type);
			switch (type->type) {
			case MONO_TYPE_U:
			case MONO_TYPE_I:
			case MONO_TYPE_PTR:
			case MONO_TYPE_FNPTR:
				break;
			case MONO_TYPE_STRING:
			case MONO_TYPE_SZARRAY:
			case MONO_TYPE_CLASS:
			case MONO_TYPE_OBJECT:
			case MONO_TYPE_ARRAY:
				g_assert ((field->offset % sizeof(gpointer)) == 0);

				g_assert (pos < size || pos <= max_size);
				bitmap [pos / BITMAP_EL_SIZE] |= ((gsize)1) << (pos % BITMAP_EL_SIZE);
				*max_set = MAX (*max_set, pos);
				break;
			case MONO_TYPE_GENERICINST:
				if (!mono_type_generic_inst_is_valuetype (type)) {
					g_assert ((field->offset % sizeof(gpointer)) == 0);

					bitmap [pos / BITMAP_EL_SIZE] |= ((gsize)1) << (pos % BITMAP_EL_SIZE);
					*max_set = MAX (*max_set, pos);
					break;
				} else {
					/* fall through */
				}
			case MONO_TYPE_VALUETYPE: {
				MonoClass *fclass = mono_class_from_mono_type (field->type);
				if (fclass->has_references) {
					/* remove the object header */
					compute_class_bitmap (fclass, bitmap, size, pos - (sizeof (MonoObject) / sizeof (gpointer)), max_set, FALSE);
				}
				break;
			}
			case MONO_TYPE_I1:
			case MONO_TYPE_U1:
			case MONO_TYPE_I2:
			case MONO_TYPE_U2:
			case MONO_TYPE_I4:
			case MONO_TYPE_U4:
			case MONO_TYPE_I8:
			case MONO_TYPE_U8:
			case MONO_TYPE_R4:
			case MONO_TYPE_R8:
			case MONO_TYPE_BOOLEAN:
			case MONO_TYPE_CHAR:
				break;
			default:
				g_error ("compute_class_bitmap: Invalid type %x for field %s:%s\n", type->type, mono_type_get_full_name (field->parent), field->name);
				break;
			}
		}
		if (static_fields)
			break;
	}
	return bitmap;
}

/**
 * mono_class_compute_bitmap:
 *
 * Mono internal function to compute a bitmap of reference fields in a class.
 */
gsize*
mono_class_compute_bitmap (MonoClass *klass, gsize *bitmap, int size, int offset, int *max_set, gboolean static_fields)
{
	MONO_REQ_GC_NEUTRAL_MODE;

	return compute_class_bitmap (klass, bitmap, size, offset, max_set, static_fields);
}

#if 0
/* 
 * similar to the above, but sets the bits in the bitmap for any non-ref field
 * and ignores static fields
 */
static gsize*
compute_class_non_ref_bitmap (MonoClass *klass, gsize *bitmap, int size, int offset)
{
	MonoClassField *field;
	MonoClass *p;
	guint32 pos, pos2;
	int max_size;

	max_size = class->instance_size / sizeof (gpointer);
	if (max_size >= size) {
		bitmap = g_malloc0 (sizeof (gsize) * ((max_size) + 1));
	}

	for (p = class; p != NULL; p = p->parent) {
		gpointer iter = NULL;
		while ((field = mono_class_get_fields (p, &iter))) {
			MonoType *type;

			if (field->type->attrs & (FIELD_ATTRIBUTE_STATIC | FIELD_ATTRIBUTE_HAS_FIELD_RVA))
				continue;
			/* FIXME: should not happen, flag as type load error */
			if (field->type->byref)
				break;

			pos = field->offset / sizeof (gpointer);
			pos += offset;

			type = mono_type_get_underlying_type (field->type);
			switch (type->type) {
#if SIZEOF_VOID_P == 8
			case MONO_TYPE_I:
			case MONO_TYPE_U:
			case MONO_TYPE_PTR:
			case MONO_TYPE_FNPTR:
#endif
			case MONO_TYPE_I8:
			case MONO_TYPE_U8:
			case MONO_TYPE_R8:
				if ((((field->offset + 7) / sizeof (gpointer)) + offset) != pos) {
					pos2 = ((field->offset + 7) / sizeof (gpointer)) + offset;
					bitmap [pos2 / BITMAP_EL_SIZE] |= ((gsize)1) << (pos2 % BITMAP_EL_SIZE);
				}
				/* fall through */
#if SIZEOF_VOID_P == 4
			case MONO_TYPE_I:
			case MONO_TYPE_U:
			case MONO_TYPE_PTR:
			case MONO_TYPE_FNPTR:
#endif
			case MONO_TYPE_I4:
			case MONO_TYPE_U4:
			case MONO_TYPE_R4:
				if ((((field->offset + 3) / sizeof (gpointer)) + offset) != pos) {
					pos2 = ((field->offset + 3) / sizeof (gpointer)) + offset;
					bitmap [pos2 / BITMAP_EL_SIZE] |= ((gsize)1) << (pos2 % BITMAP_EL_SIZE);
				}
				/* fall through */
			case MONO_TYPE_CHAR:
			case MONO_TYPE_I2:
			case MONO_TYPE_U2:
				if ((((field->offset + 1) / sizeof (gpointer)) + offset) != pos) {
					pos2 = ((field->offset + 1) / sizeof (gpointer)) + offset;
					bitmap [pos2 / BITMAP_EL_SIZE] |= ((gsize)1) << (pos2 % BITMAP_EL_SIZE);
				}
				/* fall through */
			case MONO_TYPE_BOOLEAN:
			case MONO_TYPE_I1:
			case MONO_TYPE_U1:
				bitmap [pos / BITMAP_EL_SIZE] |= ((gsize)1) << (pos % BITMAP_EL_SIZE);
				break;
			case MONO_TYPE_STRING:
			case MONO_TYPE_SZARRAY:
			case MONO_TYPE_CLASS:
			case MONO_TYPE_OBJECT:
			case MONO_TYPE_ARRAY:
				break;
			case MONO_TYPE_GENERICINST:
				if (!mono_type_generic_inst_is_valuetype (type)) {
					break;
				} else {
					/* fall through */
				}
			case MONO_TYPE_VALUETYPE: {
				MonoClass *fclass = mono_class_from_mono_type (field->type);
				/* remove the object header */
				compute_class_non_ref_bitmap (fclass, bitmap, size, pos - (sizeof (MonoObject) / sizeof (gpointer)));
				break;
			}
			default:
				g_assert_not_reached ();
				break;
			}
		}
	}
	return bitmap;
}

/**
 * mono_class_insecure_overlapping:
 * check if a class with explicit layout has references and non-references
 * fields overlapping.
 *
 * Returns: TRUE if it is insecure to load the type.
 */
gboolean
mono_class_insecure_overlapping (MonoClass *klass)
{
	int max_set = 0;
	gsize *bitmap;
	gsize default_bitmap [4] = {0};
	gsize *nrbitmap;
	gsize default_nrbitmap [4] = {0};
	int i, insecure = FALSE;
		return FALSE;

	bitmap = compute_class_bitmap (klass, default_bitmap, sizeof (default_bitmap) * 8, 0, &max_set, FALSE);
	nrbitmap = compute_class_non_ref_bitmap (klass, default_nrbitmap, sizeof (default_nrbitmap) * 8, 0);

	for (i = 0; i <= max_set; i += sizeof (bitmap [0]) * 8) {
		int idx = i % (sizeof (bitmap [0]) * 8);
		if (bitmap [idx] & nrbitmap [idx]) {
			insecure = TRUE;
			break;
		}
	}
	if (bitmap != default_bitmap)
		g_free (bitmap);
	if (nrbitmap != default_nrbitmap)
		g_free (nrbitmap);
	if (insecure) {
		g_print ("class %s.%s in assembly %s has overlapping references\n", klass->name_space, klass->name, klass->image->name);
		return FALSE;
	}
	return insecure;
}
#endif

MonoString*
ves_icall_string_alloc (int length)
{
	MonoError error;
	MonoString *str = mono_string_new_size_checked (mono_domain_get (), length, &error);
	mono_error_set_pending_exception (&error);

	return str;
}

/* LOCKING: Acquires the loader lock */
void
mono_class_compute_gc_descriptor (MonoClass *klass)
{
	MONO_REQ_GC_NEUTRAL_MODE;

	int max_set = 0;
	gsize *bitmap;
	gsize default_bitmap [4] = {0};
	static gboolean gcj_inited = FALSE;
	MonoGCDescriptor gc_descr;

	if (!gcj_inited) {
		mono_loader_lock ();

		mono_register_jit_icall (ves_icall_object_new_fast, "ves_icall_object_new_fast", mono_create_icall_signature ("object ptr"), FALSE);
		mono_register_jit_icall (ves_icall_string_alloc, "ves_icall_string_alloc", mono_create_icall_signature ("object int"), FALSE);

		gcj_inited = TRUE;
		mono_loader_unlock ();
	}

	if (!klass->inited)
		mono_class_init (klass);

	if (klass->gc_descr_inited)
		return;

	bitmap = default_bitmap;
	if (klass == mono_defaults.string_class) {
		gc_descr = mono_gc_make_descr_for_string (bitmap, 2);
	} else if (klass->rank) {
		mono_class_compute_gc_descriptor (klass->element_class);
		if (MONO_TYPE_IS_REFERENCE (&klass->element_class->byval_arg)) {
			gsize abm = 1;
			gc_descr = mono_gc_make_descr_for_array (klass->byval_arg.type == MONO_TYPE_SZARRAY, &abm, 1, sizeof (gpointer));
			/*printf ("new array descriptor: 0x%x for %s.%s\n", class->gc_descr,
				class->name_space, class->name);*/
		} else {
			/* remove the object header */
			bitmap = compute_class_bitmap (klass->element_class, default_bitmap, sizeof (default_bitmap) * 8, - (int)(sizeof (MonoObject) / sizeof (gpointer)), &max_set, FALSE);
			gc_descr = mono_gc_make_descr_for_array (klass->byval_arg.type == MONO_TYPE_SZARRAY, bitmap, mono_array_element_size (klass) / sizeof (gpointer), mono_array_element_size (klass));
			/*printf ("new vt array descriptor: 0x%x for %s.%s\n", class->gc_descr,
				class->name_space, class->name);*/
			if (bitmap != default_bitmap)
				g_free (bitmap);
		}
	} else {
		/*static int count = 0;
		if (count++ > 58)
			return;*/
		bitmap = compute_class_bitmap (klass, default_bitmap, sizeof (default_bitmap) * 8, 0, &max_set, FALSE);
		gc_descr = mono_gc_make_descr_for_object (bitmap, max_set + 1, klass->instance_size);
		/*
		if (class->gc_descr == MONO_GC_DESCRIPTOR_NULL)
			g_print ("disabling typed alloc (%d) for %s.%s\n", max_set, class->name_space, class->name);
		*/
		/*printf ("new descriptor: %p 0x%x for %s.%s\n", class->gc_descr, bitmap [0], class->name_space, class->name);*/
		if (bitmap != default_bitmap)
			g_free (bitmap);
	}

	/* Publish the data */
	mono_loader_lock ();
	klass->gc_descr = gc_descr;
	mono_memory_barrier ();
	klass->gc_descr_inited = TRUE;
	mono_loader_unlock ();
}

/**
 * field_is_special_static:
 * @fklass: The MonoClass to look up.
 * @field: The MonoClassField describing the field.
 *
 * Returns: SPECIAL_STATIC_THREAD if the field is thread static, SPECIAL_STATIC_CONTEXT if it is context static,
 * SPECIAL_STATIC_NONE otherwise.
 */
static gint32
field_is_special_static (MonoClass *fklass, MonoClassField *field)
{
	MONO_REQ_GC_NEUTRAL_MODE;

	MonoError error;
	MonoCustomAttrInfo *ainfo;
	int i;
	ainfo = mono_custom_attrs_from_field_checked (fklass, field, &error);
	mono_error_cleanup (&error); /* FIXME don't swallow the error? */
	if (!ainfo)
		return FALSE;
	for (i = 0; i < ainfo->num_attrs; ++i) {
		MonoClass *klass = ainfo->attrs [i].ctor->klass;
		if (klass->image == mono_defaults.corlib) {
			if (strcmp (klass->name, "ThreadStaticAttribute") == 0) {
				mono_custom_attrs_free (ainfo);
				return SPECIAL_STATIC_THREAD;
			}
			else if (strcmp (klass->name, "ContextStaticAttribute") == 0) {
				mono_custom_attrs_free (ainfo);
				return SPECIAL_STATIC_CONTEXT;
			}
		}
	}
	mono_custom_attrs_free (ainfo);
	return SPECIAL_STATIC_NONE;
}

#define rot(x,k) (((x)<<(k)) | ((x)>>(32-(k))))
#define mix(a,b,c) { \
	a -= c;  a ^= rot(c, 4);  c += b; \
	b -= a;  b ^= rot(a, 6);  a += c; \
	c -= b;  c ^= rot(b, 8);  b += a; \
	a -= c;  a ^= rot(c,16);  c += b; \
	b -= a;  b ^= rot(a,19);  a += c; \
	c -= b;  c ^= rot(b, 4);  b += a; \
}
#define final(a,b,c) { \
	c ^= b; c -= rot(b,14); \
	a ^= c; a -= rot(c,11); \
	b ^= a; b -= rot(a,25); \
	c ^= b; c -= rot(b,16); \
	a ^= c; a -= rot(c,4);  \
	b ^= a; b -= rot(a,14); \
	c ^= b; c -= rot(b,24); \
}

/*
 * mono_method_get_imt_slot:
 *
 *   The IMT slot is embedded into AOTed code, so this must return the same value
 * for the same method across all executions. This means:
 * - pointers shouldn't be used as hash values.
 * - mono_metadata_str_hash () should be used for hashing strings.
 */
guint32
mono_method_get_imt_slot (MonoMethod *method)
{
	MONO_REQ_GC_NEUTRAL_MODE;

	MonoMethodSignature *sig;
	int hashes_count;
	guint32 *hashes_start, *hashes;
	guint32 a, b, c;
	int i;

	/* This can be used to stress tests the collision code */
	//return 0;

	/*
	 * We do this to simplify generic sharing.  It will hurt
	 * performance in cases where a class implements two different
	 * instantiations of the same generic interface.
	 * The code in build_imt_slots () depends on this.
	 */
	if (method->is_inflated)
		method = ((MonoMethodInflated*)method)->declaring;

	sig = mono_method_signature (method);
	hashes_count = sig->param_count + 4;
	hashes_start = (guint32 *)malloc (hashes_count * sizeof (guint32));
	hashes = hashes_start;

	if (! MONO_CLASS_IS_INTERFACE (method->klass)) {
		g_error ("mono_method_get_imt_slot: %s.%s.%s is not an interface MonoMethod",
				method->klass->name_space, method->klass->name, method->name);
	}
	
	/* Initialize hashes */
	hashes [0] = mono_metadata_str_hash (method->klass->name);
	hashes [1] = mono_metadata_str_hash (method->klass->name_space);
	hashes [2] = mono_metadata_str_hash (method->name);
	hashes [3] = mono_metadata_type_hash (sig->ret);
	for (i = 0; i < sig->param_count; i++) {
		hashes [4 + i] = mono_metadata_type_hash (sig->params [i]);
	}

	/* Setup internal state */
	a = b = c = 0xdeadbeef + (((guint32)hashes_count)<<2);

	/* Handle most of the hashes */
	while (hashes_count > 3) {
		a += hashes [0];
		b += hashes [1];
		c += hashes [2];
		mix (a,b,c);
		hashes_count -= 3;
		hashes += 3;
	}

	/* Handle the last 3 hashes (all the case statements fall through) */
	switch (hashes_count) { 
	case 3 : c += hashes [2];
	case 2 : b += hashes [1];
	case 1 : a += hashes [0];
		final (a,b,c);
	case 0: /* nothing left to add */
		break;
	}
	
	g_free (hashes_start);
	/* Report the result */
	return c % MONO_IMT_SIZE;
}
#undef rot
#undef mix
#undef final

#define DEBUG_IMT 0

static void
add_imt_builder_entry (MonoImtBuilderEntry **imt_builder, MonoMethod *method, guint32 *imt_collisions_bitmap, int vtable_slot, int slot_num) {
	MONO_REQ_GC_NEUTRAL_MODE;

	guint32 imt_slot = mono_method_get_imt_slot (method);
	MonoImtBuilderEntry *entry;

	if (slot_num >= 0 && imt_slot != slot_num) {
		/* we build just a single imt slot and this is not it */
		return;
	}

	entry = (MonoImtBuilderEntry *)g_malloc0 (sizeof (MonoImtBuilderEntry));
	entry->key = method;
	entry->value.vtable_slot = vtable_slot;
	entry->next = imt_builder [imt_slot];
	if (imt_builder [imt_slot] != NULL) {
		entry->children = imt_builder [imt_slot]->children + 1;
		if (entry->children == 1) {
			mono_stats.imt_slots_with_collisions++;
			*imt_collisions_bitmap |= (1 << imt_slot);
		}
	} else {
		entry->children = 0;
		mono_stats.imt_used_slots++;
	}
	imt_builder [imt_slot] = entry;
#if DEBUG_IMT
	{
	char *method_name = mono_method_full_name (method, TRUE);
	printf ("Added IMT slot for method (%p) %s: imt_slot = %d, vtable_slot = %d, colliding with other %d entries\n",
			method, method_name, imt_slot, vtable_slot, entry->children);
	g_free (method_name);
	}
#endif
}

#if DEBUG_IMT
static void
print_imt_entry (const char* message, MonoImtBuilderEntry *e, int num) {
	if (e != NULL) {
		MonoMethod *method = e->key;
		printf ("  * %s [%d]: (%p) '%s.%s.%s'\n",
				message,
				num,
				method,
				method->klass->name_space,
				method->klass->name,
				method->name);
	} else {
		printf ("  * %s: NULL\n", message);
	}
}
#endif

static int
compare_imt_builder_entries (const void *p1, const void *p2) {
	MonoImtBuilderEntry *e1 = *(MonoImtBuilderEntry**) p1;
	MonoImtBuilderEntry *e2 = *(MonoImtBuilderEntry**) p2;
	
	return (e1->key < e2->key) ? -1 : ((e1->key > e2->key) ? 1 : 0);
}

static int
imt_emit_ir (MonoImtBuilderEntry **sorted_array, int start, int end, GPtrArray *out_array)
{
	MONO_REQ_GC_NEUTRAL_MODE;

	int count = end - start;
	int chunk_start = out_array->len;
	if (count < 4) {
		int i;
		for (i = start; i < end; ++i) {
			MonoIMTCheckItem *item = g_new0 (MonoIMTCheckItem, 1);
			item->key = sorted_array [i]->key;
			item->value = sorted_array [i]->value;
			item->has_target_code = sorted_array [i]->has_target_code;
			item->is_equals = TRUE;
			if (i < end - 1)
				item->check_target_idx = out_array->len + 1;
			else
				item->check_target_idx = 0;
			g_ptr_array_add (out_array, item);
		}
	} else {
		int middle = start + count / 2;
		MonoIMTCheckItem *item = g_new0 (MonoIMTCheckItem, 1);

		item->key = sorted_array [middle]->key;
		item->is_equals = FALSE;
		g_ptr_array_add (out_array, item);
		imt_emit_ir (sorted_array, start, middle, out_array);
		item->check_target_idx = imt_emit_ir (sorted_array, middle, end, out_array);
	}
	return chunk_start;
}

static GPtrArray*
imt_sort_slot_entries (MonoImtBuilderEntry *entries) {
	MONO_REQ_GC_NEUTRAL_MODE;

	int number_of_entries = entries->children + 1;
	MonoImtBuilderEntry **sorted_array = (MonoImtBuilderEntry **)malloc (sizeof (MonoImtBuilderEntry*) * number_of_entries);
	GPtrArray *result = g_ptr_array_new ();
	MonoImtBuilderEntry *current_entry;
	int i;
	
	for (current_entry = entries, i = 0; current_entry != NULL; current_entry = current_entry->next, i++) {
		sorted_array [i] = current_entry;
	}
	qsort (sorted_array, number_of_entries, sizeof (MonoImtBuilderEntry*), compare_imt_builder_entries);

	/*for (i = 0; i < number_of_entries; i++) {
		print_imt_entry (" sorted array:", sorted_array [i], i);
	}*/

	imt_emit_ir (sorted_array, 0, number_of_entries, result);

	g_free (sorted_array);
	return result;
}

static gpointer
initialize_imt_slot (MonoVTable *vtable, MonoDomain *domain, MonoImtBuilderEntry *imt_builder_entry, gpointer fail_tramp)
{
	MONO_REQ_GC_NEUTRAL_MODE;

	if (imt_builder_entry != NULL) {
		if (imt_builder_entry->children == 0 && !fail_tramp && !always_build_imt_trampolines) {
			/* No collision, return the vtable slot contents */
			return vtable->vtable [imt_builder_entry->value.vtable_slot];
		} else {
			/* Collision, build the trampoline */
			GPtrArray *imt_ir = imt_sort_slot_entries (imt_builder_entry);
			gpointer result;
			int i;
			result = imt_trampoline_builder (vtable, domain,
				(MonoIMTCheckItem**)imt_ir->pdata, imt_ir->len, fail_tramp);
			for (i = 0; i < imt_ir->len; ++i)
				g_free (g_ptr_array_index (imt_ir, i));
			g_ptr_array_free (imt_ir, TRUE);
			return result;
		}
	} else {
		if (fail_tramp)
			return fail_tramp;
		else
			/* Empty slot */
			return NULL;
	}
}

static MonoImtBuilderEntry*
get_generic_virtual_entries (MonoDomain *domain, gpointer *vtable_slot);

/*
 * LOCKING: requires the loader and domain locks.
 *
*/
static void
build_imt_slots (MonoClass *klass, MonoVTable *vt, MonoDomain *domain, gpointer* imt, GSList *extra_interfaces, int slot_num)
{
	MONO_REQ_GC_NEUTRAL_MODE;

	int i;
	GSList *list_item;
	guint32 imt_collisions_bitmap = 0;
	MonoImtBuilderEntry **imt_builder = (MonoImtBuilderEntry **)calloc (MONO_IMT_SIZE, sizeof (MonoImtBuilderEntry*));
	int method_count = 0;
	gboolean record_method_count_for_max_collisions = FALSE;
	gboolean has_generic_virtual = FALSE, has_variant_iface = FALSE;

#if DEBUG_IMT
	printf ("Building IMT for class %s.%s slot %d\n", klass->name_space, klass->name, slot_num);
#endif
	for (i = 0; i < klass->interface_offsets_count; ++i) {
		MonoClass *iface = klass->interfaces_packed [i];
		int interface_offset = klass->interface_offsets_packed [i];
		int method_slot_in_interface, vt_slot;

		if (mono_class_has_variant_generic_params (iface))
			has_variant_iface = TRUE;

		mono_class_setup_methods (iface);
		vt_slot = interface_offset;
		int mcount = mono_class_get_method_count (iface);
		for (method_slot_in_interface = 0; method_slot_in_interface < mcount; method_slot_in_interface++) {
			MonoMethod *method;

			if (slot_num >= 0 && mono_class_is_ginst (iface)) {
				/*
				 * The imt slot of the method is the same as for its declaring method,
				 * see the comment in mono_method_get_imt_slot (), so we can
				 * avoid inflating methods which will be discarded by 
				 * add_imt_builder_entry anyway.
				 */
				method = mono_class_get_method_by_index (mono_class_get_generic_class (iface)->container_class, method_slot_in_interface);
				if (mono_method_get_imt_slot (method) != slot_num) {
					vt_slot ++;
					continue;
				}
			}
			method = mono_class_get_method_by_index (iface, method_slot_in_interface);
			if (method->is_generic) {
				has_generic_virtual = TRUE;
				vt_slot ++;
				continue;
			}

			if (!(method->flags & METHOD_ATTRIBUTE_STATIC)) {
				add_imt_builder_entry (imt_builder, method, &imt_collisions_bitmap, vt_slot, slot_num);
				vt_slot ++;
			}
		}
	}
	if (extra_interfaces) {
		int interface_offset = klass->vtable_size;

		for (list_item = extra_interfaces; list_item != NULL; list_item=list_item->next) {
			MonoClass* iface = (MonoClass *)list_item->data;
			int method_slot_in_interface;
			int mcount = mono_class_get_method_count (iface);
			for (method_slot_in_interface = 0; method_slot_in_interface < mcount; method_slot_in_interface++) {
				MonoMethod *method = mono_class_get_method_by_index (iface, method_slot_in_interface);

				if (method->is_generic)
					has_generic_virtual = TRUE;
				add_imt_builder_entry (imt_builder, method, &imt_collisions_bitmap, interface_offset + method_slot_in_interface, slot_num);
			}
			interface_offset += mcount;
		}
	}
	for (i = 0; i < MONO_IMT_SIZE; ++i) {
		/* overwrite the imt slot only if we're building all the entries or if 
		 * we're building this specific one
		 */
		if (slot_num < 0 || i == slot_num) {
			MonoImtBuilderEntry *entries = get_generic_virtual_entries (domain, &imt [i]);

			if (entries) {
				if (imt_builder [i]) {
					MonoImtBuilderEntry *entry;

					/* Link entries with imt_builder [i] */
					for (entry = entries; entry->next; entry = entry->next) {
#if DEBUG_IMT
						MonoMethod *method = (MonoMethod*)entry->key;
						char *method_name = mono_method_full_name (method, TRUE);
						printf ("Added extra entry for method (%p) %s: imt_slot = %d\n", method, method_name, i);
						g_free (method_name);
#endif
					}
					entry->next = imt_builder [i];
					entries->children += imt_builder [i]->children + 1;
				}
				imt_builder [i] = entries;
			}

			if (has_generic_virtual || has_variant_iface) {
				/*
				 * There might be collisions later when the the trampoline is expanded.
				 */
				imt_collisions_bitmap |= (1 << i);

				/* 
				 * The IMT trampoline might be called with an instance of one of the 
				 * generic virtual methods, so has to fallback to the IMT trampoline.
				 */
				imt [i] = initialize_imt_slot (vt, domain, imt_builder [i], callbacks.get_imt_trampoline (vt, i));
			} else {
				imt [i] = initialize_imt_slot (vt, domain, imt_builder [i], NULL);
			}
#if DEBUG_IMT
			printf ("initialize_imt_slot[%d]: %p methods %d\n", i, imt [i], imt_builder [i]->children + 1);
#endif
		}

		if (imt_builder [i] != NULL) {
			int methods_in_slot = imt_builder [i]->children + 1;
			if (methods_in_slot > mono_stats.imt_max_collisions_in_slot) {
				mono_stats.imt_max_collisions_in_slot = methods_in_slot;
				record_method_count_for_max_collisions = TRUE;
			}
			method_count += methods_in_slot;
		}
	}
	
	mono_stats.imt_number_of_methods += method_count;
	if (record_method_count_for_max_collisions) {
		mono_stats.imt_method_count_when_max_collisions = method_count;
	}
	
	for (i = 0; i < MONO_IMT_SIZE; i++) {
		MonoImtBuilderEntry* entry = imt_builder [i];
		while (entry != NULL) {
			MonoImtBuilderEntry* next = entry->next;
			g_free (entry);
			entry = next;
		}
	}
	g_free (imt_builder);
	/* we OR the bitmap since we may build just a single imt slot at a time */
	vt->imt_collisions_bitmap |= imt_collisions_bitmap;
}

static void
build_imt (MonoClass *klass, MonoVTable *vt, MonoDomain *domain, gpointer* imt, GSList *extra_interfaces) {
	MONO_REQ_GC_NEUTRAL_MODE;

	build_imt_slots (klass, vt, domain, imt, extra_interfaces, -1);
}

/**
 * mono_vtable_build_imt_slot:
 * @vtable: virtual object table struct
 * @imt_slot: slot in the IMT table
 *
 * Fill the given @imt_slot in the IMT table of @vtable with
 * a trampoline or a trampoline for the case of collisions.
 * This is part of the internal mono API.
 *
 * LOCKING: Take the domain lock.
 */
void
mono_vtable_build_imt_slot (MonoVTable* vtable, int imt_slot)
{
	MONO_REQ_GC_NEUTRAL_MODE;

	gpointer *imt = (gpointer*)vtable;
	imt -= MONO_IMT_SIZE;
	g_assert (imt_slot >= 0 && imt_slot < MONO_IMT_SIZE);

	/* no support for extra interfaces: the proxy objects will need
	 * to build the complete IMT
	 * Update and heck needs to ahppen inside the proper domain lock, as all
	 * the changes made to a MonoVTable.
	 */
	mono_loader_lock (); /*FIXME build_imt_slots requires the loader lock.*/
	mono_domain_lock (vtable->domain);
	/* we change the slot only if it wasn't changed from the generic imt trampoline already */
	if (!callbacks.imt_entry_inited (vtable, imt_slot))
		build_imt_slots (vtable->klass, vtable, vtable->domain, imt, NULL, imt_slot);
	mono_domain_unlock (vtable->domain);
	mono_loader_unlock ();
}

#define THUNK_THRESHOLD		10

/**
 * mono_method_alloc_generic_virtual_trampoline:
 * @domain: a domain
 * @size: size in bytes
 *
 * Allocs size bytes to be used for the code of a generic virtual
 * trampoline.  It's either allocated from the domain's code manager or
 * reused from a previously invalidated piece.
 *
 * LOCKING: The domain lock must be held.
 */
gpointer
mono_method_alloc_generic_virtual_trampoline (MonoDomain *domain, int size)
{
	MONO_REQ_GC_NEUTRAL_MODE;

	static gboolean inited = FALSE;
	static int generic_virtual_trampolines_size = 0;

	if (!inited) {
		mono_counters_register ("Generic virtual trampoline bytes",
				MONO_COUNTER_GENERICS | MONO_COUNTER_INT, &generic_virtual_trampolines_size);
		inited = TRUE;
	}
	generic_virtual_trampolines_size += size;

	return mono_domain_code_reserve (domain, size);
}

typedef struct _GenericVirtualCase {
	MonoMethod *method;
	gpointer code;
	int count;
	struct _GenericVirtualCase *next;
} GenericVirtualCase;

/*
 * get_generic_virtual_entries:
 *
 *   Return IMT entries for the generic virtual method instances and
 *   variant interface methods for vtable slot
 * VTABLE_SLOT.
 */ 
static MonoImtBuilderEntry*
get_generic_virtual_entries (MonoDomain *domain, gpointer *vtable_slot)
{
	MONO_REQ_GC_NEUTRAL_MODE;

  	GenericVirtualCase *list;
 	MonoImtBuilderEntry *entries;
  
 	mono_domain_lock (domain);
 	if (!domain->generic_virtual_cases)
 		domain->generic_virtual_cases = g_hash_table_new (mono_aligned_addr_hash, NULL);
 
	list = (GenericVirtualCase *)g_hash_table_lookup (domain->generic_virtual_cases, vtable_slot);
 
 	entries = NULL;
 	for (; list; list = list->next) {
 		MonoImtBuilderEntry *entry;
 
 		if (list->count < THUNK_THRESHOLD)
 			continue;
 
 		entry = g_new0 (MonoImtBuilderEntry, 1);
 		entry->key = list->method;
 		entry->value.target_code = mono_get_addr_from_ftnptr (list->code);
 		entry->has_target_code = 1;
 		if (entries)
 			entry->children = entries->children + 1;
 		entry->next = entries;
 		entries = entry;
 	}
 
 	mono_domain_unlock (domain);
 
 	/* FIXME: Leaking memory ? */
 	return entries;
}

/**
 * mono_method_add_generic_virtual_invocation:
 * @domain: a domain
 * @vtable_slot: pointer to the vtable slot
 * @method: the inflated generic virtual method
 * @code: the method's code
 *
 * Registers a call via unmanaged code to a generic virtual method
 * instantiation or variant interface method.  If the number of calls reaches a threshold
 * (THUNK_THRESHOLD), the method is added to the vtable slot's generic
 * virtual method trampoline.
 */
void
mono_method_add_generic_virtual_invocation (MonoDomain *domain, MonoVTable *vtable,
											gpointer *vtable_slot,
											MonoMethod *method, gpointer code)
{
	MONO_REQ_GC_NEUTRAL_MODE;

	static gboolean inited = FALSE;
	static int num_added = 0;
	static int num_freed = 0;

	GenericVirtualCase *gvc, *list;
	MonoImtBuilderEntry *entries;
	int i;
	GPtrArray *sorted;

	mono_domain_lock (domain);
	if (!domain->generic_virtual_cases)
		domain->generic_virtual_cases = g_hash_table_new (mono_aligned_addr_hash, NULL);

	if (!inited) {
		mono_counters_register ("Generic virtual cases", MONO_COUNTER_GENERICS | MONO_COUNTER_INT, &num_added);
		mono_counters_register ("Freed IMT trampolines", MONO_COUNTER_GENERICS | MONO_COUNTER_INT, &num_freed);
		inited = TRUE;
	}

	/* Check whether the case was already added */
	list = (GenericVirtualCase *)g_hash_table_lookup (domain->generic_virtual_cases, vtable_slot);
	gvc = list;
	while (gvc) {
		if (gvc->method == method)
			break;
		gvc = gvc->next;
	}

	/* If not found, make a new one */
	if (!gvc) {
		gvc = (GenericVirtualCase *)mono_domain_alloc (domain, sizeof (GenericVirtualCase));
		gvc->method = method;
		gvc->code = code;
		gvc->count = 0;
		gvc->next = (GenericVirtualCase *)g_hash_table_lookup (domain->generic_virtual_cases, vtable_slot);

		g_hash_table_insert (domain->generic_virtual_cases, vtable_slot, gvc);

		num_added++;
	}

	if (++gvc->count == THUNK_THRESHOLD) {
		gpointer *old_thunk = (void **)*vtable_slot;
		gpointer vtable_trampoline = NULL;
		gpointer imt_trampoline = NULL;

		if ((gpointer)vtable_slot < (gpointer)vtable) {
			int displacement = (gpointer*)vtable_slot - (gpointer*)vtable;
			int imt_slot = MONO_IMT_SIZE + displacement;

			/* Force the rebuild of the trampoline at the next call */
			imt_trampoline = callbacks.get_imt_trampoline (vtable, imt_slot);
			*vtable_slot = imt_trampoline;
		} else {
			vtable_trampoline = callbacks.get_vtable_trampoline ? callbacks.get_vtable_trampoline (vtable, (gpointer*)vtable_slot - (gpointer*)vtable->vtable) : NULL;

			entries = get_generic_virtual_entries (domain, vtable_slot);

			sorted = imt_sort_slot_entries (entries);

			*vtable_slot = imt_trampoline_builder (NULL, domain, (MonoIMTCheckItem**)sorted->pdata, sorted->len,
												   vtable_trampoline);

			while (entries) {
				MonoImtBuilderEntry *next = entries->next;
				g_free (entries);
				entries = next;
			}

			for (i = 0; i < sorted->len; ++i)
				g_free (g_ptr_array_index (sorted, i));
			g_ptr_array_free (sorted, TRUE);

			if (old_thunk != vtable_trampoline && old_thunk != imt_trampoline)
				num_freed ++;
		}
	}

	mono_domain_unlock (domain);
}

static MonoVTable *mono_class_create_runtime_vtable (MonoDomain *domain, MonoClass *klass, MonoError *error);

/**
 * mono_class_vtable:
 * @domain: the application domain
 * @class: the class to initialize
 *
 * VTables are domain specific because we create domain specific code, and 
 * they contain the domain specific static class data.
 * On failure, NULL is returned, and class->exception_type is set.
 */
MonoVTable *
mono_class_vtable (MonoDomain *domain, MonoClass *klass)
{
	MonoError error;
	MonoVTable* vtable = mono_class_vtable_full (domain, klass, &error);
	mono_error_cleanup (&error);
	return vtable;
}

/**
 * mono_class_vtable_full:
 * @domain: the application domain
 * @class: the class to initialize
 * @error set on failure.
 *
 * VTables are domain specific because we create domain specific code, and 
 * they contain the domain specific static class data.
 */
MonoVTable *
mono_class_vtable_full (MonoDomain *domain, MonoClass *klass, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoClassRuntimeInfo *runtime_info;

	error_init (error);

	g_assert (klass);

	if (mono_class_has_failure (klass)) {
		mono_error_set_for_class_failure (error, klass);
		return NULL;
	}

	/* this check can be inlined in jitted code, too */
	runtime_info = klass->runtime_info;
	if (runtime_info && runtime_info->max_domain >= domain->domain_id && runtime_info->domain_vtables [domain->domain_id])
		return runtime_info->domain_vtables [domain->domain_id];
	return mono_class_create_runtime_vtable (domain, klass, error);
}

/**
 * mono_class_try_get_vtable:
 * @domain: the application domain
 * @class: the class to initialize
 *
 * This function tries to get the associated vtable from @class if
 * it was already created.
 */
MonoVTable *
mono_class_try_get_vtable (MonoDomain *domain, MonoClass *klass)
{
	MONO_REQ_GC_NEUTRAL_MODE;

	MonoClassRuntimeInfo *runtime_info;

	g_assert (klass);

	runtime_info = klass->runtime_info;
	if (runtime_info && runtime_info->max_domain >= domain->domain_id && runtime_info->domain_vtables [domain->domain_id])
		return runtime_info->domain_vtables [domain->domain_id];
	return NULL;
}

static gpointer*
alloc_vtable (MonoDomain *domain, size_t vtable_size, size_t imt_table_bytes)
{
	MONO_REQ_GC_NEUTRAL_MODE;

	size_t alloc_offset;

	/*
	 * We want the pointer to the MonoVTable aligned to 8 bytes because SGen uses three
	 * address bits.  The IMT has an odd number of entries, however, so on 32 bits the
	 * alignment will be off.  In that case we allocate 4 more bytes and skip over them.
	 */
	if (sizeof (gpointer) == 4 && (imt_table_bytes & 7)) {
		g_assert ((imt_table_bytes & 7) == 4);
		vtable_size += 4;
		alloc_offset = 4;
	} else {
		alloc_offset = 0;
	}

	return (gpointer*) ((char*)mono_domain_alloc0 (domain, vtable_size) + alloc_offset);
}

static MonoVTable *
mono_class_create_runtime_vtable (MonoDomain *domain, MonoClass *klass, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoVTable *vt;
	MonoClassRuntimeInfo *runtime_info, *old_info;
	MonoClassField *field;
	char *t;
	int i, vtable_slots;
	size_t imt_table_bytes;
	int gc_bits;
	guint32 vtable_size, class_size;
	gpointer iter;
	gpointer *interface_offsets;

	error_init (error);

	mono_loader_lock (); /*FIXME mono_class_init acquires it*/
	mono_domain_lock (domain);
	runtime_info = klass->runtime_info;
	if (runtime_info && runtime_info->max_domain >= domain->domain_id && runtime_info->domain_vtables [domain->domain_id]) {
		mono_domain_unlock (domain);
		mono_loader_unlock ();
		return runtime_info->domain_vtables [domain->domain_id];
	}
	if (!klass->inited || mono_class_has_failure (klass)) {
		if (!mono_class_init (klass) || mono_class_has_failure (klass)) {
			mono_domain_unlock (domain);
			mono_loader_unlock ();
			mono_error_set_for_class_failure (error, klass);
			return NULL;
		}
	}

	/* Array types require that their element type be valid*/
	if (klass->byval_arg.type == MONO_TYPE_ARRAY || klass->byval_arg.type == MONO_TYPE_SZARRAY) {
		MonoClass *element_class = klass->element_class;
		if (!element_class->inited)
			mono_class_init (element_class);

		/*mono_class_init can leave the vtable layout to be lazily done and we can't afford this here*/
		if (!mono_class_has_failure (element_class) && !element_class->vtable_size)
			mono_class_setup_vtable (element_class);
		
		if (mono_class_has_failure (element_class)) {
			/*Can happen if element_class only got bad after mono_class_setup_vtable*/
			if (!mono_class_has_failure (klass))
				mono_class_set_type_load_failure (klass, "");
			mono_domain_unlock (domain);
			mono_loader_unlock ();
			mono_error_set_for_class_failure (error, klass);
			return NULL;
		}
	}

	/* 
	 * For some classes, mono_class_init () already computed klass->vtable_size, and 
	 * that is all that is needed because of the vtable trampolines.
	 */
	if (!klass->vtable_size)
		mono_class_setup_vtable (klass);

	if (mono_class_is_ginst (klass) && !klass->vtable)
		mono_class_check_vtable_constraints (klass, NULL);

	/* Initialize klass->has_finalize */
	mono_class_has_finalizer (klass);

	if (mono_class_has_failure (klass)) {
		mono_domain_unlock (domain);
		mono_loader_unlock ();
		mono_error_set_for_class_failure (error, klass);
		return NULL;
	}

	vtable_slots = klass->vtable_size;
	/* we add an additional vtable slot to store the pointer to static field data only when needed */
	class_size = mono_class_data_size (klass);
	if (class_size)
		vtable_slots++;

	if (klass->interface_offsets_count) {
		imt_table_bytes = sizeof (gpointer) * (MONO_IMT_SIZE);
		mono_stats.imt_number_of_tables++;
		mono_stats.imt_tables_size += imt_table_bytes;
	} else {
		imt_table_bytes = 0;
	}

	vtable_size = imt_table_bytes + MONO_SIZEOF_VTABLE + vtable_slots * sizeof (gpointer);

	mono_stats.used_class_count++;
	mono_stats.class_vtable_size += vtable_size;

	interface_offsets = alloc_vtable (domain, vtable_size, imt_table_bytes);
	vt = (MonoVTable*) ((char*)interface_offsets + imt_table_bytes);
	g_assert (!((gsize)vt & 7));

	vt->klass = klass;
	vt->rank = klass->rank;
	vt->domain = domain;

	mono_class_compute_gc_descriptor (klass);
		/*
		 * We can't use typed allocation in the non-root domains, since the
		 * collector needs the GC descriptor stored in the vtable even after
		 * the mempool containing the vtable is destroyed when the domain is
		 * unloaded. An alternative might be to allocate vtables in the GC
		 * heap, but this does not seem to work (it leads to crashes inside
		 * libgc). If that approach is tried, two gc descriptors need to be
		 * allocated for each class: one for the root domain, and one for all
		 * other domains. The second descriptor should contain a bit for the
		 * vtable field in MonoObject, since we can no longer assume the 
		 * vtable is reachable by other roots after the appdomain is unloaded.
		 */
#ifdef HAVE_BOEHM_GC
	if (domain != mono_get_root_domain () && !mono_dont_free_domains)
		vt->gc_descr = MONO_GC_DESCRIPTOR_NULL;
	else
#endif
		vt->gc_descr = klass->gc_descr;

	gc_bits = mono_gc_get_vtable_bits (klass);
	g_assert (!(gc_bits & ~((1 << MONO_VTABLE_AVAILABLE_GC_BITS) - 1)));

	vt->gc_bits = gc_bits;

	if (class_size) {
		/* we store the static field pointer at the end of the vtable: vt->vtable [class->vtable_size] */
		if (klass->has_static_refs) {
			MonoGCDescriptor statics_gc_descr;
			int max_set = 0;
			gsize default_bitmap [4] = {0};
			gsize *bitmap;

			bitmap = compute_class_bitmap (klass, default_bitmap, sizeof (default_bitmap) * 8, 0, &max_set, TRUE);
			/*g_print ("bitmap 0x%x for %s.%s (size: %d)\n", bitmap [0], klass->name_space, klass->name, class_size);*/
			statics_gc_descr = mono_gc_make_descr_from_bitmap (bitmap, max_set + 1);
			vt->vtable [klass->vtable_size] = mono_gc_alloc_fixed (class_size, statics_gc_descr, MONO_ROOT_SOURCE_STATIC, "managed static variables");
			mono_domain_add_class_static_data (domain, klass, vt->vtable [klass->vtable_size], NULL);
			if (bitmap != default_bitmap)
				g_free (bitmap);
		} else {
			vt->vtable [klass->vtable_size] = mono_domain_alloc0 (domain, class_size);
		}
		vt->has_static_fields = TRUE;
		mono_stats.class_static_data_size += class_size;
	}

	iter = NULL;
	while ((field = mono_class_get_fields (klass, &iter))) {
		if (!(field->type->attrs & FIELD_ATTRIBUTE_STATIC))
			continue;
		if (mono_field_is_deleted (field))
			continue;
		if (!(field->type->attrs & FIELD_ATTRIBUTE_LITERAL)) {
			gint32 special_static = klass->no_special_static_fields ? SPECIAL_STATIC_NONE : field_is_special_static (klass, field);
			if (special_static != SPECIAL_STATIC_NONE) {
				guint32 size, offset;
				gint32 align;
				gsize default_bitmap [4] = {0};
				gsize *bitmap;
				int max_set = 0;
				int numbits;
				MonoClass *fclass;
				if (mono_type_is_reference (field->type)) {
					default_bitmap [0] = 1;
					numbits = 1;
					bitmap = default_bitmap;
				} else if (mono_type_is_struct (field->type)) {
					fclass = mono_class_from_mono_type (field->type);
					bitmap = compute_class_bitmap (fclass, default_bitmap, sizeof (default_bitmap) * 8, - (int)(sizeof (MonoObject) / sizeof (gpointer)), &max_set, FALSE);
					numbits = max_set + 1;
				} else {
					default_bitmap [0] = 0;
					numbits = 0;
					bitmap = default_bitmap;
				}
				size = mono_type_size (field->type, &align);
				offset = mono_alloc_special_static_data (special_static, size, align, (uintptr_t*)bitmap, numbits);
				if (!domain->special_static_fields)
					domain->special_static_fields = g_hash_table_new (NULL, NULL);
				g_hash_table_insert (domain->special_static_fields, field, GUINT_TO_POINTER (offset));
				if (bitmap != default_bitmap)
					g_free (bitmap);
				/* 
				 * This marks the field as special static to speed up the
				 * checks in mono_field_static_get/set_value ().
				 */
				field->offset = -1;
				continue;
			}
		}
		if ((field->type->attrs & FIELD_ATTRIBUTE_HAS_FIELD_RVA)) {
			MonoClass *fklass = mono_class_from_mono_type (field->type);
			const char *data = mono_field_get_data (field);

			g_assert (!(field->type->attrs & FIELD_ATTRIBUTE_HAS_DEFAULT));
			t = (char*)mono_vtable_get_static_field_data (vt) + field->offset;
			/* some fields don't really have rva, they are just zeroed (bss? bug #343083) */
			if (!data)
				continue;
			if (fklass->valuetype) {
				memcpy (t, data, mono_class_value_size (fklass, NULL));
			} else {
				/* it's a pointer type: add check */
				g_assert ((fklass->byval_arg.type == MONO_TYPE_PTR) || (fklass->byval_arg.type == MONO_TYPE_FNPTR));
				*t = *(char *)data;
			}
			continue;
		}		
	}

	vt->max_interface_id = klass->max_interface_id;
	vt->interface_bitmap = klass->interface_bitmap;
	
	//printf ("Initializing VT for class %s (interface_offsets_count = %d)\n",
	//		class->name, klass->interface_offsets_count);

	/* Initialize vtable */
	if (callbacks.get_vtable_trampoline) {
		// This also covers the AOT case
		for (i = 0; i < klass->vtable_size; ++i) {
			vt->vtable [i] = callbacks.get_vtable_trampoline (vt, i);
		}
	} else {
		mono_class_setup_vtable (klass);

		for (i = 0; i < klass->vtable_size; ++i) {
			MonoMethod *cm;

			cm = klass->vtable [i];
			if (cm) {
				vt->vtable [i] = callbacks.create_jit_trampoline (domain, cm, error);
				if (!is_ok (error)) {
					mono_domain_unlock (domain);
					mono_loader_unlock ();
					return NULL;
				}
			}
		}
	}

	if (imt_table_bytes) {
		/* Now that the vtable is full, we can actually fill up the IMT */
			for (i = 0; i < MONO_IMT_SIZE; ++i)
				interface_offsets [i] = callbacks.get_imt_trampoline (vt, i);
	}

	/*
	 * FIXME: Is it ok to allocate while holding the domain/loader locks ? If not, we can release them, allocate, then
	 * re-acquire them and check if another thread has created the vtable in the meantime.
	 */
	/* Special case System.MonoType to avoid infinite recursion */
	if (klass != mono_defaults.runtimetype_class) {
		vt->type = mono_type_get_object_checked (domain, &klass->byval_arg, error);
		if (!is_ok (error)) {
			mono_domain_unlock (domain);
			mono_loader_unlock ();
			return NULL;
		}

		if (mono_object_get_class ((MonoObject *)vt->type) != mono_defaults.runtimetype_class)
			/* This is unregistered in
			   unregister_vtable_reflection_type() in
			   domain.c. */
			MONO_GC_REGISTER_ROOT_IF_MOVING(vt->type, MONO_ROOT_SOURCE_REFLECTION, "vtable reflection type");
	}

	mono_vtable_set_is_remote (vt, mono_class_is_contextbound (klass));

	/*  class_vtable_array keeps an array of created vtables
	 */
	g_ptr_array_add (domain->class_vtable_array, vt);
	/* klass->runtime_info is protected by the loader lock, both when
	 * it it enlarged and when it is stored info.
	 */

	/*
	 * Store the vtable in klass->runtime_info.
	 * klass->runtime_info is accessed without locking, so this do this last after the vtable has been constructed.
	 */
	mono_memory_barrier ();

	old_info = klass->runtime_info;
	if (old_info && old_info->max_domain >= domain->domain_id) {
		/* someone already created a large enough runtime info */
		old_info->domain_vtables [domain->domain_id] = vt;
	} else {
		int new_size = domain->domain_id;
		if (old_info)
			new_size = MAX (new_size, old_info->max_domain);
		new_size++;
		/* make the new size a power of two */
		i = 2;
		while (new_size > i)
			i <<= 1;
		new_size = i;
		/* this is a bounded memory retention issue: may want to 
		 * handle it differently when we'll have a rcu-like system.
		 */
		runtime_info = (MonoClassRuntimeInfo *)mono_image_alloc0 (klass->image, MONO_SIZEOF_CLASS_RUNTIME_INFO + new_size * sizeof (gpointer));
		runtime_info->max_domain = new_size - 1;
		/* copy the stuff from the older info */
		if (old_info) {
			memcpy (runtime_info->domain_vtables, old_info->domain_vtables, (old_info->max_domain + 1) * sizeof (gpointer));
		}
		runtime_info->domain_vtables [domain->domain_id] = vt;
		/* keep this last*/
		mono_memory_barrier ();
		klass->runtime_info = runtime_info;
	}

	if (klass == mono_defaults.runtimetype_class) {
		vt->type = mono_type_get_object_checked (domain, &klass->byval_arg, error);
		if (!is_ok (error)) {
			mono_domain_unlock (domain);
			mono_loader_unlock ();
			return NULL;
		}

		if (mono_object_get_class ((MonoObject *)vt->type) != mono_defaults.runtimetype_class)
			/* This is unregistered in
			   unregister_vtable_reflection_type() in
			   domain.c. */
			MONO_GC_REGISTER_ROOT_IF_MOVING(vt->type, MONO_ROOT_SOURCE_REFLECTION, "vtable reflection type");
	}

	mono_domain_unlock (domain);
	mono_loader_unlock ();

	/* make sure the parent is initialized */
	/*FIXME shouldn't this fail the current type?*/
	if (klass->parent)
		mono_class_vtable_full (domain, klass->parent, error);

	return vt;
}

#ifndef DISABLE_REMOTING
/**
 * mono_class_proxy_vtable:
 * @domain: the application domain
 * @remove_class: the remote class
 * @error: set on error
 *
 * Creates a vtable for transparent proxies. It is basically
 * a copy of the real vtable of the class wrapped in @remote_class,
 * but all function pointers invoke the remoting functions, and
 * vtable->klass points to the transparent proxy class, and not to @class.
 *
 * On failure returns NULL and sets @error
 */
static MonoVTable *
mono_class_proxy_vtable (MonoDomain *domain, MonoRemoteClass *remote_class, MonoRemotingTarget target_type, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoVTable *vt, *pvt;
	int i, j, vtsize, extra_interface_vtsize = 0;
	guint32 max_interface_id;
	MonoClass *k;
	GSList *extra_interfaces = NULL;
	MonoClass *klass = remote_class->proxy_class;
	gpointer *interface_offsets;
	uint8_t *bitmap = NULL;
	int bsize;
	size_t imt_table_bytes;
	
#ifdef COMPRESSED_INTERFACE_BITMAP
	int bcsize;
#endif

	error_init (error);

	vt = mono_class_vtable (domain, klass);
	g_assert (vt); /*FIXME property handle failure*/
	max_interface_id = vt->max_interface_id;
	
	/* Calculate vtable space for extra interfaces */
	for (j = 0; j < remote_class->interface_count; j++) {
		MonoClass* iclass = remote_class->interfaces[j];
		GPtrArray *ifaces;
		int method_count;

		/*FIXME test for interfaces with variant generic arguments*/
		if (MONO_CLASS_IMPLEMENTS_INTERFACE (klass, iclass->interface_id))
			continue;	/* interface implemented by the class */
		if (g_slist_find (extra_interfaces, iclass))
			continue;
			
		extra_interfaces = g_slist_prepend (extra_interfaces, iclass);
		
		method_count = mono_class_num_methods (iclass);
	
		ifaces = mono_class_get_implemented_interfaces (iclass, error);
		if (!is_ok (error))
			goto failure;
		if (ifaces) {
			for (i = 0; i < ifaces->len; ++i) {
				MonoClass *ic = (MonoClass *)g_ptr_array_index (ifaces, i);
				/*FIXME test for interfaces with variant generic arguments*/
				if (MONO_CLASS_IMPLEMENTS_INTERFACE (klass, ic->interface_id))
					continue;	/* interface implemented by the class */
				if (g_slist_find (extra_interfaces, ic))
					continue;
				extra_interfaces = g_slist_prepend (extra_interfaces, ic);
				method_count += mono_class_num_methods (ic);
			}
			g_ptr_array_free (ifaces, TRUE);
			ifaces = NULL;
		}

		extra_interface_vtsize += method_count * sizeof (gpointer);
		if (iclass->max_interface_id > max_interface_id) max_interface_id = iclass->max_interface_id;
	}

	imt_table_bytes = sizeof (gpointer) * MONO_IMT_SIZE;
	mono_stats.imt_number_of_tables++;
	mono_stats.imt_tables_size += imt_table_bytes;

	vtsize = imt_table_bytes + MONO_SIZEOF_VTABLE + klass->vtable_size * sizeof (gpointer);

	mono_stats.class_vtable_size += vtsize + extra_interface_vtsize;

	interface_offsets = alloc_vtable (domain, vtsize + extra_interface_vtsize, imt_table_bytes);
	pvt = (MonoVTable*) ((char*)interface_offsets + imt_table_bytes);
	g_assert (!((gsize)pvt & 7));

	memcpy (pvt, vt, MONO_SIZEOF_VTABLE + klass->vtable_size * sizeof (gpointer));

	pvt->klass = mono_defaults.transparent_proxy_class;
	/* we need to keep the GC descriptor for a transparent proxy or we confuse the precise GC */
	pvt->gc_descr = mono_defaults.transparent_proxy_class->gc_descr;

	/* initialize vtable */
	mono_class_setup_vtable (klass);
	for (i = 0; i < klass->vtable_size; ++i) {
		MonoMethod *cm;
		    
		if ((cm = klass->vtable [i])) {
			pvt->vtable [i] = create_remoting_trampoline (domain, cm, target_type, error);
			if (!is_ok (error))
				goto failure;
		} else
			pvt->vtable [i] = NULL;
	}

	if (mono_class_is_abstract (klass)) {
		/* create trampolines for abstract methods */
		for (k = klass; k; k = k->parent) {
			MonoMethod* m;
			gpointer iter = NULL;
			while ((m = mono_class_get_methods (k, &iter)))
				if (!pvt->vtable [m->slot]) {
					pvt->vtable [m->slot] = create_remoting_trampoline (domain, m, target_type, error);
					if (!is_ok (error))
						goto failure;
				}
		}
	}

	pvt->max_interface_id = max_interface_id;
	bsize = sizeof (guint8) * (max_interface_id/8 + 1 );
#ifdef COMPRESSED_INTERFACE_BITMAP
	bitmap = (uint8_t *)g_malloc0 (bsize);
#else
	bitmap = (uint8_t *)mono_domain_alloc0 (domain, bsize);
#endif

	for (i = 0; i < klass->interface_offsets_count; ++i) {
		int interface_id = klass->interfaces_packed [i]->interface_id;
		bitmap [interface_id >> 3] |= (1 << (interface_id & 7));
	}

	if (extra_interfaces) {
		int slot = klass->vtable_size;
		MonoClass* interf;
		gpointer iter;
		MonoMethod* cm;
		GSList *list_item;

		/* Create trampolines for the methods of the interfaces */
		for (list_item = extra_interfaces; list_item != NULL; list_item=list_item->next) {
			interf = (MonoClass *)list_item->data;
			
			bitmap [interf->interface_id >> 3] |= (1 << (interf->interface_id & 7));

			iter = NULL;
			j = 0;
			while ((cm = mono_class_get_methods (interf, &iter))) {
				pvt->vtable [slot + j++] = create_remoting_trampoline (domain, cm, target_type, error);
				if (!is_ok (error))
					goto failure;
			}
			
			slot += mono_class_num_methods (interf);
		}
	}

	/* Now that the vtable is full, we can actually fill up the IMT */
	build_imt (klass, pvt, domain, interface_offsets, extra_interfaces);
	if (extra_interfaces) {
		g_slist_free (extra_interfaces);
	}

#ifdef COMPRESSED_INTERFACE_BITMAP
	bcsize = mono_compress_bitmap (NULL, bitmap, bsize);
	pvt->interface_bitmap = mono_domain_alloc0 (domain, bcsize);
	mono_compress_bitmap (pvt->interface_bitmap, bitmap, bsize);
	g_free (bitmap);
#else
	pvt->interface_bitmap = bitmap;
#endif
	return pvt;
failure:
	if (extra_interfaces)
		g_slist_free (extra_interfaces);
#ifdef COMPRESSED_INTERFACE_BITMAP
	g_free (bitmap);
#endif
	return NULL;
}

#endif /* DISABLE_REMOTING */

/**
 * mono_class_field_is_special_static:
 *
 *   Returns whether @field is a thread/context static field.
 */
gboolean
mono_class_field_is_special_static (MonoClassField *field)
{
	MONO_REQ_GC_NEUTRAL_MODE

	if (!(field->type->attrs & FIELD_ATTRIBUTE_STATIC))
		return FALSE;
	if (mono_field_is_deleted (field))
		return FALSE;
	if (!(field->type->attrs & FIELD_ATTRIBUTE_LITERAL)) {
		if (field_is_special_static (field->parent, field) != SPECIAL_STATIC_NONE)
			return TRUE;
	}
	return FALSE;
}

/**
 * mono_class_field_get_special_static_type:
 * @field: The MonoClassField describing the field.
 *
 * Returns: SPECIAL_STATIC_THREAD if the field is thread static, SPECIAL_STATIC_CONTEXT if it is context static,
 * SPECIAL_STATIC_NONE otherwise.
 */
guint32
mono_class_field_get_special_static_type (MonoClassField *field)
{
	MONO_REQ_GC_NEUTRAL_MODE

	if (!(field->type->attrs & FIELD_ATTRIBUTE_STATIC))
		return SPECIAL_STATIC_NONE;
	if (mono_field_is_deleted (field))
		return SPECIAL_STATIC_NONE;
	if (!(field->type->attrs & FIELD_ATTRIBUTE_LITERAL))
		return field_is_special_static (field->parent, field);
	return SPECIAL_STATIC_NONE;
}

/**
 * mono_class_has_special_static_fields:
 * 
 *   Returns whenever @klass has any thread/context static fields.
 */
gboolean
mono_class_has_special_static_fields (MonoClass *klass)
{
	MONO_REQ_GC_NEUTRAL_MODE

	MonoClassField *field;
	gpointer iter;

	iter = NULL;
	while ((field = mono_class_get_fields (klass, &iter))) {
		g_assert (field->parent == klass);
		if (mono_class_field_is_special_static (field))
			return TRUE;
	}

	return FALSE;
}

#ifndef DISABLE_REMOTING
/**
 * create_remote_class_key:
 * Creates an array of pointers that can be used as a hash key for a remote class.
 * The first element of the array is the number of pointers.
 */
static gpointer*
create_remote_class_key (MonoRemoteClass *remote_class, MonoClass *extra_class)
{
	MONO_REQ_GC_NEUTRAL_MODE;

	gpointer *key;
	int i, j;
	
	if (remote_class == NULL) {
		if (mono_class_is_interface (extra_class)) {
			key = (void **)g_malloc (sizeof(gpointer) * 3);
			key [0] = GINT_TO_POINTER (2);
			key [1] = mono_defaults.marshalbyrefobject_class;
			key [2] = extra_class;
		} else {
			key = (void **)g_malloc (sizeof(gpointer) * 2);
			key [0] = GINT_TO_POINTER (1);
			key [1] = extra_class;
		}
	} else {
		if (extra_class != NULL && mono_class_is_interface (extra_class)) {
			key = (void **)g_malloc (sizeof(gpointer) * (remote_class->interface_count + 3));
			key [0] = GINT_TO_POINTER (remote_class->interface_count + 2);
			key [1] = remote_class->proxy_class;

			// Keep the list of interfaces sorted
			for (i = 0, j = 2; i < remote_class->interface_count; i++, j++) {
				if (extra_class && remote_class->interfaces [i] > extra_class) {
					key [j++] = extra_class;
					extra_class = NULL;
				}
				key [j] = remote_class->interfaces [i];
			}
			if (extra_class)
				key [j] = extra_class;
		} else {
			// Replace the old class. The interface list is the same
			key = (void **)g_malloc (sizeof(gpointer) * (remote_class->interface_count + 2));
			key [0] = GINT_TO_POINTER (remote_class->interface_count + 1);
			key [1] = extra_class != NULL ? extra_class : remote_class->proxy_class;
			for (i = 0; i < remote_class->interface_count; i++)
				key [2 + i] = remote_class->interfaces [i];
		}
	}
	
	return key;
}

/**
 * copy_remote_class_key:
 *
 *   Make a copy of KEY in the domain and return the copy.
 */
static gpointer*
copy_remote_class_key (MonoDomain *domain, gpointer *key)
{
	MONO_REQ_GC_NEUTRAL_MODE

	int key_size = (GPOINTER_TO_UINT (key [0]) + 1) * sizeof (gpointer);
	gpointer *mp_key = (gpointer *)mono_domain_alloc (domain, key_size);

	memcpy (mp_key, key, key_size);

	return mp_key;
}

/**
 * mono_remote_class:
 * @domain: the application domain
 * @class_name: name of the remote class
 * @error: set on error
 *
 * Creates and initializes a MonoRemoteClass object for a remote type. 
 *
 * On failure returns NULL and sets @error
 */
MonoRemoteClass*
mono_remote_class (MonoDomain *domain, MonoStringHandle class_name, MonoClass *proxy_class, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoRemoteClass *rc;
	gpointer* key, *mp_key;
	char *name;
	
	error_init (error);

	key = create_remote_class_key (NULL, proxy_class);
	
	mono_domain_lock (domain);
	rc = (MonoRemoteClass *)g_hash_table_lookup (domain->proxy_vtable_hash, key);

	if (rc) {
		g_free (key);
		mono_domain_unlock (domain);
		return rc;
	}

	name = mono_string_to_utf8_mp (domain->mp, MONO_HANDLE_RAW (class_name), error);
	if (!is_ok (error)) {
		g_free (key);
		mono_domain_unlock (domain);
		return NULL;
	}

	mp_key = copy_remote_class_key (domain, key);
	g_free (key);
	key = mp_key;

	if (mono_class_is_interface (proxy_class)) {
		rc = (MonoRemoteClass *)mono_domain_alloc (domain, MONO_SIZEOF_REMOTE_CLASS + sizeof(MonoClass*));
		rc->interface_count = 1;
		rc->interfaces [0] = proxy_class;
		rc->proxy_class = mono_defaults.marshalbyrefobject_class;
	} else {
		rc = (MonoRemoteClass *)mono_domain_alloc (domain, MONO_SIZEOF_REMOTE_CLASS);
		rc->interface_count = 0;
		rc->proxy_class = proxy_class;
	}
	
	rc->default_vtable = NULL;
	rc->xdomain_vtable = NULL;
	rc->proxy_class_name = name;
#ifndef DISABLE_PERFCOUNTERS
	mono_perfcounters->loader_bytes += mono_string_length (MONO_HANDLE_RAW (class_name)) + 1;
#endif

	g_hash_table_insert (domain->proxy_vtable_hash, key, rc);

	mono_domain_unlock (domain);
	return rc;
}

/**
 * clone_remote_class:
 * Creates a copy of the remote_class, adding the provided class or interface
 */
static MonoRemoteClass*
clone_remote_class (MonoDomain *domain, MonoRemoteClass* remote_class, MonoClass *extra_class)
{
	MONO_REQ_GC_NEUTRAL_MODE;

	MonoRemoteClass *rc;
	gpointer* key, *mp_key;
	
	key = create_remote_class_key (remote_class, extra_class);
	rc = (MonoRemoteClass *)g_hash_table_lookup (domain->proxy_vtable_hash, key);
	if (rc != NULL) {
		g_free (key);
		return rc;
	}

	mp_key = copy_remote_class_key (domain, key);
	g_free (key);
	key = mp_key;

	if (mono_class_is_interface (extra_class)) {
		int i,j;
		rc = (MonoRemoteClass *)mono_domain_alloc (domain, MONO_SIZEOF_REMOTE_CLASS + sizeof(MonoClass*) * (remote_class->interface_count + 1));
		rc->proxy_class = remote_class->proxy_class;
		rc->interface_count = remote_class->interface_count + 1;
		
		// Keep the list of interfaces sorted, since the hash key of
		// the remote class depends on this
		for (i = 0, j = 0; i < remote_class->interface_count; i++, j++) {
			if (remote_class->interfaces [i] > extra_class && i == j)
				rc->interfaces [j++] = extra_class;
			rc->interfaces [j] = remote_class->interfaces [i];
		}
		if (i == j)
			rc->interfaces [j] = extra_class;
	} else {
		// Replace the old class. The interface array is the same
		rc = (MonoRemoteClass *)mono_domain_alloc (domain, MONO_SIZEOF_REMOTE_CLASS + sizeof(MonoClass*) * remote_class->interface_count);
		rc->proxy_class = extra_class;
		rc->interface_count = remote_class->interface_count;
		if (rc->interface_count > 0)
			memcpy (rc->interfaces, remote_class->interfaces, rc->interface_count * sizeof (MonoClass*));
	}
	
	rc->default_vtable = NULL;
	rc->xdomain_vtable = NULL;
	rc->proxy_class_name = remote_class->proxy_class_name;

	g_hash_table_insert (domain->proxy_vtable_hash, key, rc);

	return rc;
}

gpointer
mono_remote_class_vtable (MonoDomain *domain, MonoRemoteClass *remote_class, MonoRealProxyHandle rp, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	error_init (error);

	mono_loader_lock (); /*FIXME mono_class_from_mono_type and mono_class_proxy_vtable take it*/
	mono_domain_lock (domain);
	gint32 target_domain_id = MONO_HANDLE_GETVAL (rp, target_domain_id);
	if (target_domain_id != -1) {
		if (remote_class->xdomain_vtable == NULL)
			remote_class->xdomain_vtable = mono_class_proxy_vtable (domain, remote_class, MONO_REMOTING_TARGET_APPDOMAIN, error);
		mono_domain_unlock (domain);
		mono_loader_unlock ();
		return_val_if_nok (error, NULL);
		return remote_class->xdomain_vtable;
	}
	if (remote_class->default_vtable == NULL) {
		MonoReflectionTypeHandle reftype = MONO_HANDLE_NEW (MonoReflectionType, NULL);
		MONO_HANDLE_GET (reftype, rp, class_to_proxy);
		
		MonoType *type = MONO_HANDLE_GETVAL (reftype, type);
		MonoClass *klass = mono_class_from_mono_type (type);
#ifndef DISABLE_COM
		if ((mono_class_is_com_object (klass) || (mono_class_get_com_object_class () && klass == mono_class_get_com_object_class ())) && !mono_vtable_is_remote (mono_class_vtable (mono_domain_get (), klass)))
			remote_class->default_vtable = mono_class_proxy_vtable (domain, remote_class, MONO_REMOTING_TARGET_COMINTEROP, error);
		else
#endif
			remote_class->default_vtable = mono_class_proxy_vtable (domain, remote_class, MONO_REMOTING_TARGET_UNKNOWN, error);
		/* N.B. both branches of the if modify error */
		if (!is_ok (error)) {
			mono_domain_unlock (domain);
			mono_loader_unlock ();
			return NULL;
		}
	}
	
	mono_domain_unlock (domain);
	mono_loader_unlock ();
	return remote_class->default_vtable;
}

/**
 * mono_upgrade_remote_class:
 * @domain: the application domain
 * @tproxy: the proxy whose remote class has to be upgraded.
 * @klass: class to which the remote class can be casted.
 * @error: set on error
 *
 * Updates the vtable of the remote class by adding the necessary method slots
 * and interface offsets so it can be safely casted to klass. klass can be a
 * class or an interface.  On success returns TRUE, on failure returns FALSE and sets @error.
 */
gboolean
mono_upgrade_remote_class (MonoDomain *domain, MonoObjectHandle proxy_object, MonoClass *klass, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	error_init (error);

	MonoTransparentProxyHandle tproxy = MONO_HANDLE_CAST (MonoTransparentProxy, proxy_object);
	MonoRemoteClass *remote_class = MONO_HANDLE_GETVAL (tproxy, remote_class);
	
	gboolean redo_vtable;
	if (mono_class_is_interface (klass)) {
		int i;
		redo_vtable = TRUE;
		for (i = 0; i < remote_class->interface_count && redo_vtable; i++)
			if (remote_class->interfaces [i] == klass)
				redo_vtable = FALSE;
	}
	else {
		redo_vtable = (remote_class->proxy_class != klass);
	}

	mono_loader_lock (); /*FIXME mono_remote_class_vtable requires it.*/
	mono_domain_lock (domain);
	if (redo_vtable) {
		MonoRemoteClass *fresh_remote_class = clone_remote_class (domain, remote_class, klass);
		MONO_HANDLE_SETVAL (tproxy, remote_class, MonoRemoteClass*, fresh_remote_class);
		MonoRealProxyHandle real_proxy = MONO_HANDLE_NEW (MonoRealProxy, NULL);
		MONO_HANDLE_GET (real_proxy, tproxy, rp);
		MONO_HANDLE_SETVAL (proxy_object, vtable, MonoVTable*, mono_remote_class_vtable (domain, fresh_remote_class, real_proxy, error));
		if (!is_ok (error))
			goto leave;
	}
	
leave:
	mono_domain_unlock (domain);
	mono_loader_unlock ();
	return is_ok (error);
}
#endif /* DISABLE_REMOTING */


/**
 * mono_object_get_virtual_method:
 * @obj: object to operate on.
 * @method: method 
 *
 * Retrieves the MonoMethod that would be called on obj if obj is passed as
 * the instance of a callvirt of method.
 */
MonoMethod*
mono_object_get_virtual_method (MonoObject *obj_raw, MonoMethod *method)
{
	MONO_REQ_GC_UNSAFE_MODE;
	HANDLE_FUNCTION_ENTER ();
	MonoError error;
	MONO_HANDLE_DCL (MonoObject, obj);
	MonoMethod *result = mono_object_handle_get_virtual_method (obj, method, &error);
	mono_error_assert_ok (&error);
	HANDLE_FUNCTION_RETURN_VAL (result);
}

/**
 * mono_object_get_virtual_method:
 * @obj: object to operate on.
 * @method: method 
 *
 * Retrieves the MonoMethod that would be called on obj if obj is passed as
 * the instance of a callvirt of method.
 */
MonoMethod*
mono_object_handle_get_virtual_method (MonoObjectHandle obj, MonoMethod *method, MonoError *error)
{
	error_init (error);

	gboolean is_proxy = FALSE;
	MonoClass *klass = mono_handle_class (obj);
	if (mono_class_is_transparent_proxy (klass)) {
		MonoRemoteClass *remote_class = MONO_HANDLE_GETVAL (MONO_HANDLE_CAST (MonoTransparentProxy, obj), remote_class);
		klass = remote_class->proxy_class;
		is_proxy = TRUE;
	}
	return class_get_virtual_method (klass, method, is_proxy, error);
}

static MonoMethod*
class_get_virtual_method (MonoClass *klass, MonoMethod *method, gboolean is_proxy, MonoError *error)
{
	error_init (error);


	if (!is_proxy && ((method->flags & METHOD_ATTRIBUTE_FINAL) || !(method->flags & METHOD_ATTRIBUTE_VIRTUAL)))
			return method;

	mono_class_setup_vtable (klass);
	MonoMethod **vtable = klass->vtable;

	if (method->slot == -1) {
		/* method->slot might not be set for instances of generic methods */
		if (method->is_inflated) {
			g_assert (((MonoMethodInflated*)method)->declaring->slot != -1);
			method->slot = ((MonoMethodInflated*)method)->declaring->slot; 
		} else {
			if (!is_proxy)
				g_assert_not_reached ();
		}
	}

	MonoMethod *res = NULL;
	/* check method->slot is a valid index: perform isinstance? */
	if (method->slot != -1) {
		if (mono_class_is_interface (method->klass)) {
			if (!is_proxy) {
				gboolean variance_used = FALSE;
				int iface_offset = mono_class_interface_offset_with_variance (klass, method->klass, &variance_used);
				g_assert (iface_offset > 0);
				res = vtable [iface_offset + method->slot];
			}
		} else {
			res = vtable [method->slot];
		}
    }

#ifndef DISABLE_REMOTING
	if (is_proxy) {
		/* It may be an interface, abstract class method or generic method */
		if (!res || mono_method_signature (res)->generic_param_count)
			res = method;

		/* generic methods demand invoke_with_check */
		if (mono_method_signature (res)->generic_param_count)
			res = mono_marshal_get_remoting_invoke_with_check (res);
		else {
#ifndef DISABLE_COM
			if (klass == mono_class_get_com_object_class () || mono_class_is_com_object (klass))
				res = mono_cominterop_get_invoke (res);
			else
#endif
				res = mono_marshal_get_remoting_invoke (res);
		}
	} else
#endif
	{
		if (method->is_inflated) {
			/* Have to inflate the result */
			res = mono_class_inflate_generic_method_checked (res, &((MonoMethodInflated*)method)->context, error);
		}
	}

	return res;
}

static MonoObject*
do_runtime_invoke (MonoMethod *method, void *obj, void **params, MonoObject **exc, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoObject *result = NULL;

	g_assert (callbacks.runtime_invoke);

	error_init (error);
	
	if (mono_profiler_get_events () & MONO_PROFILE_METHOD_EVENTS)
		mono_profiler_method_start_invoke (method);

	result = callbacks.runtime_invoke (method, obj, params, exc, error);

	if (mono_profiler_get_events () & MONO_PROFILE_METHOD_EVENTS)
		mono_profiler_method_end_invoke (method);

	if (!mono_error_ok (error))
		return NULL;

	return result;
}

/**
 * mono_runtime_invoke:
 * @method: method to invoke
 * @obJ: object instance
 * @params: arguments to the method
 * @exc: exception information.
 *
 * Invokes the method represented by @method on the object @obj.
 *
 * obj is the 'this' pointer, it should be NULL for static
 * methods, a MonoObject* for object instances and a pointer to
 * the value type for value types.
 *
 * The params array contains the arguments to the method with the
 * same convention: MonoObject* pointers for object instances and
 * pointers to the value type otherwise. 
 * 
 * From unmanaged code you'll usually use the
 * mono_runtime_invoke() variant.
 *
 * Note that this function doesn't handle virtual methods for
 * you, it will exec the exact method you pass: we still need to
 * expose a function to lookup the derived class implementation
 * of a virtual method (there are examples of this in the code,
 * though).
 * 
 * You can pass NULL as the exc argument if you don't want to
 * catch exceptions, otherwise, *exc will be set to the exception
 * thrown, if any.  if an exception is thrown, you can't use the
 * MonoObject* result from the function.
 * 
 * If the method returns a value type, it is boxed in an object
 * reference.
 */
MonoObject*
mono_runtime_invoke (MonoMethod *method, void *obj, void **params, MonoObject **exc)
{
	MonoError error;
	MonoObject *res;
	if (exc) {
		res = mono_runtime_try_invoke (method, obj, params, exc, &error);
		if (*exc == NULL && !mono_error_ok(&error)) {
			*exc = (MonoObject*) mono_error_convert_to_exception (&error);
		} else
			mono_error_cleanup (&error);
	} else {
		res = mono_runtime_invoke_checked (method, obj, params, &error);
		mono_error_raise_exception (&error); /* OK to throw, external only without a good alternative */
	}
	return res;
}

/**
 * mono_runtime_try_invoke:
 * @method: method to invoke
 * @obJ: object instance
 * @params: arguments to the method
 * @exc: exception information.
 * @error: set on error
 *
 * Invokes the method represented by @method on the object @obj.
 *
 * obj is the 'this' pointer, it should be NULL for static
 * methods, a MonoObject* for object instances and a pointer to
 * the value type for value types.
 *
 * The params array contains the arguments to the method with the
 * same convention: MonoObject* pointers for object instances and
 * pointers to the value type otherwise. 
 * 
 * From unmanaged code you'll usually use the
 * mono_runtime_invoke() variant.
 *
 * Note that this function doesn't handle virtual methods for
 * you, it will exec the exact method you pass: we still need to
 * expose a function to lookup the derived class implementation
 * of a virtual method (there are examples of this in the code,
 * though).
 * 
 * For this function, you must not pass NULL as the exc argument if
 * you don't want to catch exceptions, use
 * mono_runtime_invoke_checked().  If an exception is thrown, you
 * can't use the MonoObject* result from the function.
 * 
 * If this method cannot be invoked, @error will be set and @exc and
 * the return value must not be used.
 *
 * If the method returns a value type, it is boxed in an object
 * reference.
 */
MonoObject*
mono_runtime_try_invoke (MonoMethod *method, void *obj, void **params, MonoObject **exc, MonoError* error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	g_assert (exc != NULL);

	if (mono_runtime_get_no_exec ())
		g_warning ("Invoking method '%s' when running in no-exec mode.\n", mono_method_full_name (method, TRUE));

	return do_runtime_invoke (method, obj, params, exc, error);
}

/**
 * mono_runtime_invoke_checked:
 * @method: method to invoke
 * @obJ: object instance
 * @params: arguments to the method
 * @error: set on error
 *
 * Invokes the method represented by @method on the object @obj.
 *
 * obj is the 'this' pointer, it should be NULL for static
 * methods, a MonoObject* for object instances and a pointer to
 * the value type for value types.
 *
 * The params array contains the arguments to the method with the
 * same convention: MonoObject* pointers for object instances and
 * pointers to the value type otherwise. 
 * 
 * From unmanaged code you'll usually use the
 * mono_runtime_invoke() variant.
 *
 * Note that this function doesn't handle virtual methods for
 * you, it will exec the exact method you pass: we still need to
 * expose a function to lookup the derived class implementation
 * of a virtual method (there are examples of this in the code,
 * though).
 * 
 * If an exception is thrown, you can't use the MonoObject* result
 * from the function.
 * 
 * If this method cannot be invoked, @error will be set.  If the
 * method throws an exception (and we're in coop mode) the exception
 * will be set in @error.
 *
 * If the method returns a value type, it is boxed in an object
 * reference.
 */
MonoObject*
mono_runtime_invoke_checked (MonoMethod *method, void *obj, void **params, MonoError* error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	if (mono_runtime_get_no_exec ())
		g_warning ("Invoking method '%s' when running in no-exec mode.\n", mono_method_full_name (method, TRUE));

	return do_runtime_invoke (method, obj, params, NULL, error);
}

/**
 * mono_method_get_unmanaged_thunk:
 * @method: method to generate a thunk for.
 *
 * Returns an unmanaged->managed thunk that can be used to call
 * a managed method directly from C.
 *
 * The thunk's C signature closely matches the managed signature:
 *
 * C#: public bool Equals (object obj);
 * C:  typedef MonoBoolean (*Equals)(MonoObject*,
 *             MonoObject*, MonoException**);
 *
 * The 1st ("this") parameter must not be used with static methods:
 *
 * C#: public static bool ReferenceEquals (object a, object b);
 * C:  typedef MonoBoolean (*ReferenceEquals)(MonoObject*, MonoObject*,
 *             MonoException**);
 *
 * The last argument must be a non-null pointer of a MonoException* pointer.
 * It has "out" semantics. After invoking the thunk, *ex will be NULL if no
 * exception has been thrown in managed code. Otherwise it will point
 * to the MonoException* caught by the thunk. In this case, the result of
 * the thunk is undefined:
 *
 * MonoMethod *method = ... // MonoMethod* of System.Object.Equals
 * MonoException *ex = NULL;
 * Equals func = mono_method_get_unmanaged_thunk (method);
 * MonoBoolean res = func (thisObj, objToCompare, &ex);
 * if (ex) {
 *    // handle exception
 * }
 *
 * The calling convention of the thunk matches the platform's default
 * convention. This means that under Windows, C declarations must
 * contain the __stdcall attribute:
 *
 * C:  typedef MonoBoolean (__stdcall *Equals)(MonoObject*,
 *             MonoObject*, MonoException**);
 *
 * LIMITATIONS
 *
 * Value type arguments and return values are treated as they were objects:
 *
 * C#: public static Rectangle Intersect (Rectangle a, Rectangle b);
 * C:  typedef MonoObject* (*Intersect)(MonoObject*, MonoObject*, MonoException**);
 *
 * Arguments must be properly boxed upon trunk's invocation, while return
 * values must be unboxed.
 */
gpointer
mono_method_get_unmanaged_thunk (MonoMethod *method)
{
	MONO_REQ_GC_NEUTRAL_MODE;
	MONO_REQ_API_ENTRYPOINT;

	MonoError error;
	gpointer res;

	g_assert (!mono_threads_is_coop_enabled ());

	MONO_ENTER_GC_UNSAFE;
	method = mono_marshal_get_thunk_invoke_wrapper (method);
	res = mono_compile_method_checked (method, &error);
	mono_error_cleanup (&error);
	MONO_EXIT_GC_UNSAFE;

	return res;
}

void
mono_copy_value (MonoType *type, void *dest, void *value, int deref_pointer)
{
	MONO_REQ_GC_UNSAFE_MODE;

	int t;
	if (type->byref) {
		/* object fields cannot be byref, so we don't need a
		   wbarrier here */
		gpointer *p = (gpointer*)dest;
		*p = value;
		return;
	}
	t = type->type;
handle_enum:
	switch (t) {
	case MONO_TYPE_BOOLEAN:
	case MONO_TYPE_I1:
	case MONO_TYPE_U1: {
		guint8 *p = (guint8*)dest;
		*p = value ? *(guint8*)value : 0;
		return;
	}
	case MONO_TYPE_I2:
	case MONO_TYPE_U2:
	case MONO_TYPE_CHAR: {
		guint16 *p = (guint16*)dest;
		*p = value ? *(guint16*)value : 0;
		return;
	}
#if SIZEOF_VOID_P == 4
	case MONO_TYPE_I:
	case MONO_TYPE_U:
#endif
	case MONO_TYPE_I4:
	case MONO_TYPE_U4: {
		gint32 *p = (gint32*)dest;
		*p = value ? *(gint32*)value : 0;
		return;
	}
#if SIZEOF_VOID_P == 8
	case MONO_TYPE_I:
	case MONO_TYPE_U:
#endif
	case MONO_TYPE_I8:
	case MONO_TYPE_U8: {
		gint64 *p = (gint64*)dest;
		*p = value ? *(gint64*)value : 0;
		return;
	}
	case MONO_TYPE_R4: {
		float *p = (float*)dest;
		*p = value ? *(float*)value : 0;
		return;
	}
	case MONO_TYPE_R8: {
		double *p = (double*)dest;
		*p = value ? *(double*)value : 0;
		return;
	}
	case MONO_TYPE_STRING:
	case MONO_TYPE_SZARRAY:
	case MONO_TYPE_CLASS:
	case MONO_TYPE_OBJECT:
	case MONO_TYPE_ARRAY:
		mono_gc_wbarrier_generic_store (dest, deref_pointer ? *(MonoObject **)value : (MonoObject *)value);
		return;
	case MONO_TYPE_FNPTR:
	case MONO_TYPE_PTR: {
		gpointer *p = (gpointer*)dest;
		*p = deref_pointer? *(gpointer*)value: value;
		return;
	}
	case MONO_TYPE_VALUETYPE:
		/* note that 't' and 'type->type' can be different */
		if (type->type == MONO_TYPE_VALUETYPE && type->data.klass->enumtype) {
			t = mono_class_enum_basetype (type->data.klass)->type;
			goto handle_enum;
		} else {
			MonoClass *klass = mono_class_from_mono_type (type);
			int size = mono_class_value_size (klass, NULL);
			if (value == NULL)
				mono_gc_bzero_atomic (dest, size);
			else
				mono_gc_wbarrier_value_copy (dest, value, 1, klass);
		}
		return;
	case MONO_TYPE_GENERICINST:
		t = type->data.generic_class->container_class->byval_arg.type;
		goto handle_enum;
	default:
		g_error ("got type %x", type->type);
	}
}

/**
 * mono_field_set_value:
 * @obj: Instance object
 * @field: MonoClassField describing the field to set
 * @value: The value to be set
 *
 * Sets the value of the field described by @field in the object instance @obj
 * to the value passed in @value.   This method should only be used for instance
 * fields.   For static fields, use mono_field_static_set_value.
 *
 * The value must be on the native format of the field type. 
 */
void
mono_field_set_value (MonoObject *obj, MonoClassField *field, void *value)
{
	MONO_REQ_GC_UNSAFE_MODE;

	void *dest;

	g_return_if_fail (!(field->type->attrs & FIELD_ATTRIBUTE_STATIC));

	dest = (char*)obj + field->offset;
	mono_copy_value (field->type, dest, value, FALSE);
}

/**
 * mono_field_static_set_value:
 * @field: MonoClassField describing the field to set
 * @value: The value to be set
 *
 * Sets the value of the static field described by @field
 * to the value passed in @value.
 *
 * The value must be on the native format of the field type. 
 */
void
mono_field_static_set_value (MonoVTable *vt, MonoClassField *field, void *value)
{
	MONO_REQ_GC_UNSAFE_MODE;

	void *dest;

	g_return_if_fail (field->type->attrs & FIELD_ATTRIBUTE_STATIC);
	/* you cant set a constant! */
	g_return_if_fail (!(field->type->attrs & FIELD_ATTRIBUTE_LITERAL));

	if (field->offset == -1) {
		/* Special static */
		gpointer addr;

		mono_domain_lock (vt->domain);
		addr = g_hash_table_lookup (vt->domain->special_static_fields, field);
		mono_domain_unlock (vt->domain);
		dest = mono_get_special_static_data (GPOINTER_TO_UINT (addr));
	} else {
		dest = (char*)mono_vtable_get_static_field_data (vt) + field->offset;
	}
	mono_copy_value (field->type, dest, value, FALSE);
}

/**
 * mono_vtable_get_static_field_data:
 *
 * Internal use function: return a pointer to the memory holding the static fields
 * for a class or NULL if there are no static fields.
 * This is exported only for use by the debugger.
 */
void *
mono_vtable_get_static_field_data (MonoVTable *vt)
{
	MONO_REQ_GC_NEUTRAL_MODE

	if (!vt->has_static_fields)
		return NULL;
	return vt->vtable [vt->klass->vtable_size];
}

static guint8*
mono_field_get_addr (MonoObject *obj, MonoVTable *vt, MonoClassField *field)
{
	MONO_REQ_GC_UNSAFE_MODE;

	guint8 *src;

	if (field->type->attrs & FIELD_ATTRIBUTE_STATIC) {
		if (field->offset == -1) {
			/* Special static */
			gpointer addr;

			mono_domain_lock (vt->domain);
			addr = g_hash_table_lookup (vt->domain->special_static_fields, field);
			mono_domain_unlock (vt->domain);
			src = (guint8 *)mono_get_special_static_data (GPOINTER_TO_UINT (addr));
		} else {
			src = (guint8*)mono_vtable_get_static_field_data (vt) + field->offset;
		}
	} else {
		src = (guint8*)obj + field->offset;
	}

	return src;
}

/**
 * mono_field_get_value:
 * @obj: Object instance
 * @field: MonoClassField describing the field to fetch information from
 * @value: pointer to the location where the value will be stored
 *
 * Use this routine to get the value of the field @field in the object
 * passed.
 *
 * The pointer provided by value must be of the field type, for reference
 * types this is a MonoObject*, for value types its the actual pointer to
 * the value type.
 *
 * For example:
 *     int i;
 *     mono_field_get_value (obj, int_field, &i);
 */
void
mono_field_get_value (MonoObject *obj, MonoClassField *field, void *value)
{
	MONO_REQ_GC_UNSAFE_MODE;

	void *src;

	g_assert (obj);

	g_return_if_fail (!(field->type->attrs & FIELD_ATTRIBUTE_STATIC));

	src = (char*)obj + field->offset;
	mono_copy_value (field->type, value, src, TRUE);
}

/**
 * mono_field_get_value_object:
 * @domain: domain where the object will be created (if boxing)
 * @field: MonoClassField describing the field to fetch information from
 * @obj: The object instance for the field.
 *
 * Returns: a new MonoObject with the value from the given field.  If the
 * field represents a value type, the value is boxed.
 *
 */
MonoObject *
mono_field_get_value_object (MonoDomain *domain, MonoClassField *field, MonoObject *obj)
{	
	MonoError error;
	MonoObject* result = mono_field_get_value_object_checked (domain, field, obj, &error);
	mono_error_assert_ok (&error);
	return result;
}

/**
 * mono_field_get_value_object_checked:
 * @domain: domain where the object will be created (if boxing)
 * @field: MonoClassField describing the field to fetch information from
 * @obj: The object instance for the field.
 * @error: Set on error.
 *
 * Returns: a new MonoObject with the value from the given field.  If the
 * field represents a value type, the value is boxed.  On error returns NULL and sets @error.
 *
 */
MonoObject *
mono_field_get_value_object_checked (MonoDomain *domain, MonoClassField *field, MonoObject *obj, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	error_init (error);

	MonoObject *o;
	MonoClass *klass;
	MonoVTable *vtable = NULL;
	gchar *v;
	gboolean is_static = FALSE;
	gboolean is_ref = FALSE;
	gboolean is_literal = FALSE;
	gboolean is_ptr = FALSE;
	MonoType *type = mono_field_get_type_checked (field, error);

	return_val_if_nok (error, NULL);

	switch (type->type) {
	case MONO_TYPE_STRING:
	case MONO_TYPE_OBJECT:
	case MONO_TYPE_CLASS:
	case MONO_TYPE_ARRAY:
	case MONO_TYPE_SZARRAY:
		is_ref = TRUE;
		break;
	case MONO_TYPE_U1:
	case MONO_TYPE_I1:
	case MONO_TYPE_BOOLEAN:
	case MONO_TYPE_U2:
	case MONO_TYPE_I2:
	case MONO_TYPE_CHAR:
	case MONO_TYPE_U:
	case MONO_TYPE_I:
	case MONO_TYPE_U4:
	case MONO_TYPE_I4:
	case MONO_TYPE_R4:
	case MONO_TYPE_U8:
	case MONO_TYPE_I8:
	case MONO_TYPE_R8:
	case MONO_TYPE_VALUETYPE:
		is_ref = type->byref;
		break;
	case MONO_TYPE_GENERICINST:
		is_ref = !mono_type_generic_inst_is_valuetype (type);
		break;
	case MONO_TYPE_PTR:
		is_ptr = TRUE;
		break;
	default:
		g_error ("type 0x%x not handled in "
			 "mono_field_get_value_object", type->type);
		return NULL;
	}

	if (type->attrs & FIELD_ATTRIBUTE_LITERAL)
		is_literal = TRUE;

	if (type->attrs & FIELD_ATTRIBUTE_STATIC) {
		is_static = TRUE;

		if (!is_literal) {
			vtable = mono_class_vtable_full (domain, field->parent, error);
			return_val_if_nok (error, NULL);

			if (!vtable->initialized) {
				mono_runtime_class_init_full (vtable, error);
				return_val_if_nok (error, NULL);
			}
		}
	} else {
		g_assert (obj);
	}
	
	if (is_ref) {
		if (is_literal) {
			get_default_field_value (domain, field, &o, error);
			return_val_if_nok (error, NULL);
		} else if (is_static) {
			mono_field_static_get_value_checked (vtable, field, &o, error);
			return_val_if_nok (error, NULL);
		} else {
			mono_field_get_value (obj, field, &o);
		}
		return o;
	}

	if (is_ptr) {
		static MonoMethod *m;
		gpointer args [2];
		gpointer *ptr;
		gpointer v;

		if (!m) {
			MonoClass *ptr_klass = mono_class_get_pointer_class ();
			m = mono_class_get_method_from_name_flags (ptr_klass, "Box", 2, METHOD_ATTRIBUTE_STATIC);
			g_assert (m);
		}

		v = &ptr;
		if (is_literal) {
			get_default_field_value (domain, field, v, error);
			return_val_if_nok (error, NULL);
		} else if (is_static) {
			mono_field_static_get_value_checked (vtable, field, v, error);
			return_val_if_nok (error, NULL);
		} else {
			mono_field_get_value (obj, field, v);
		}

		/* MONO_TYPE_PTR is passed by value to runtime_invoke () */
		args [0] = ptr ? *ptr : NULL;
		args [1] = mono_type_get_object_checked (mono_domain_get (), type, error);
		return_val_if_nok (error, NULL);

		o = mono_runtime_invoke_checked (m, NULL, args, error);
		return_val_if_nok (error, NULL);

		return o;
	}

	/* boxed value type */
	klass = mono_class_from_mono_type (type);

	if (mono_class_is_nullable (klass))
		return mono_nullable_box (mono_field_get_addr (obj, vtable, field), klass, error);

	o = mono_object_new_checked (domain, klass, error);
	return_val_if_nok (error, NULL);
	v = ((gchar *) o) + sizeof (MonoObject);

	if (is_literal) {
		get_default_field_value (domain, field, v, error);
		return_val_if_nok (error, NULL);
	} else if (is_static) {
		mono_field_static_get_value_checked (vtable, field, v, error);
		return_val_if_nok (error, NULL);
	} else {
		mono_field_get_value (obj, field, v);
	}

	return o;
}

int
mono_get_constant_value_from_blob (MonoDomain* domain, MonoTypeEnum type, const char *blob, void *value, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	error_init (error);
	int retval = 0;
	const char *p = blob;
	mono_metadata_decode_blob_size (p, &p);

	switch (type) {
	case MONO_TYPE_BOOLEAN:
	case MONO_TYPE_U1:
	case MONO_TYPE_I1:
		*(guint8 *) value = *p;
		break;
	case MONO_TYPE_CHAR:
	case MONO_TYPE_U2:
	case MONO_TYPE_I2:
		*(guint16*) value = read16 (p);
		break;
	case MONO_TYPE_U4:
	case MONO_TYPE_I4:
		*(guint32*) value = read32 (p);
		break;
	case MONO_TYPE_U8:
	case MONO_TYPE_I8:
		*(guint64*) value = read64 (p);
		break;
	case MONO_TYPE_R4:
		readr4 (p, (float*) value);
		break;
	case MONO_TYPE_R8:
		readr8 (p, (double*) value);
		break;
	case MONO_TYPE_STRING:
		*(gpointer*) value = mono_ldstr_metadata_sig (domain, blob, error);
		break;
	case MONO_TYPE_CLASS:
		*(gpointer*) value = NULL;
		break;
	default:
		retval = -1;
		g_warning ("type 0x%02x should not be in constant table", type);
	}
	return retval;
}

static void
get_default_field_value (MonoDomain* domain, MonoClassField *field, void *value, MonoError *error)
{
	MONO_REQ_GC_NEUTRAL_MODE;

	MonoTypeEnum def_type;
	const char* data;

	error_init (error);
	
	data = mono_class_get_field_default_value (field, &def_type);
	mono_get_constant_value_from_blob (domain, def_type, data, value, error);
}

void
mono_field_static_get_value_for_thread (MonoInternalThread *thread, MonoVTable *vt, MonoClassField *field, void *value, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	void *src;

	error_init (error);

	g_return_if_fail (field->type->attrs & FIELD_ATTRIBUTE_STATIC);
	
	if (field->type->attrs & FIELD_ATTRIBUTE_LITERAL) {
		get_default_field_value (vt->domain, field, value, error);
		return;
	}

	if (field->offset == -1) {
		/* Special static */
		gpointer addr = g_hash_table_lookup (vt->domain->special_static_fields, field);
		src = mono_get_special_static_data_for_thread (thread, GPOINTER_TO_UINT (addr));
	} else {
		src = (char*)mono_vtable_get_static_field_data (vt) + field->offset;
	}
	mono_copy_value (field->type, value, src, TRUE);
}

/**
 * mono_field_static_get_value:
 * @vt: vtable to the object
 * @field: MonoClassField describing the field to fetch information from
 * @value: where the value is returned
 *
 * Use this routine to get the value of the static field @field value.
 *
 * The pointer provided by value must be of the field type, for reference
 * types this is a MonoObject*, for value types its the actual pointer to
 * the value type.
 *
 * For example:
 *     int i;
 *     mono_field_static_get_value (vt, int_field, &i);
 */
void
mono_field_static_get_value (MonoVTable *vt, MonoClassField *field, void *value)
{
	MONO_REQ_GC_NEUTRAL_MODE;

	MonoError error;
	mono_field_static_get_value_checked (vt, field, value, &error);
	mono_error_cleanup (&error);
}

/**
 * mono_field_static_get_value_checked:
 * @vt: vtable to the object
 * @field: MonoClassField describing the field to fetch information from
 * @value: where the value is returned
 * @error: set on error
 *
 * Use this routine to get the value of the static field @field value.
 *
 * The pointer provided by value must be of the field type, for reference
 * types this is a MonoObject*, for value types its the actual pointer to
 * the value type.
 *
 * For example:
 *     int i;
 *     mono_field_static_get_value_checked (vt, int_field, &i, error);
 *     if (!is_ok (error)) { ... }
 *
 * On failure sets @error.
 */
void
mono_field_static_get_value_checked (MonoVTable *vt, MonoClassField *field, void *value, MonoError *error)
{
	MONO_REQ_GC_NEUTRAL_MODE;

	mono_field_static_get_value_for_thread (mono_thread_internal_current (), vt, field, value, error);
}

/**
 * mono_property_set_value:
 * @prop: MonoProperty to set
 * @obj: instance object on which to act
 * @params: parameters to pass to the propery
 * @exc: optional exception
 *
 * Invokes the property's set method with the given arguments on the
 * object instance obj (or NULL for static properties). 
 * 
 * You can pass NULL as the exc argument if you don't want to
 * catch exceptions, otherwise, *exc will be set to the exception
 * thrown, if any.  if an exception is thrown, you can't use the
 * MonoObject* result from the function.
 */
void
mono_property_set_value (MonoProperty *prop, void *obj, void **params, MonoObject **exc)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoError error;
	do_runtime_invoke (prop->set, obj, params, exc, &error);
	if (exc && *exc == NULL && !mono_error_ok (&error)) {
		*exc = (MonoObject*) mono_error_convert_to_exception (&error);
	} else {
		mono_error_cleanup (&error);
	}
}

/**
 * mono_property_set_value_checked:
 * @prop: MonoProperty to set
 * @obj: instance object on which to act
 * @params: parameters to pass to the propery
 * @error: set on error
 *
 * Invokes the property's set method with the given arguments on the
 * object instance obj (or NULL for static properties). 
 * 
 * Returns: TRUE on success.  On failure returns FALSE and sets @error.
 * If an exception is thrown, it will be caught and returned via @error.
 */
gboolean
mono_property_set_value_checked (MonoProperty *prop, void *obj, void **params, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoObject *exc;

	error_init (error);
	do_runtime_invoke (prop->set, obj, params, &exc, error);
	if (exc != NULL && is_ok (error))
		mono_error_set_exception_instance (error, (MonoException*)exc);
	return is_ok (error);
}

/**
 * mono_property_get_value:
 * @prop: MonoProperty to fetch
 * @obj: instance object on which to act
 * @params: parameters to pass to the propery
 * @exc: optional exception
 *
 * Invokes the property's get method with the given arguments on the
 * object instance obj (or NULL for static properties). 
 * 
 * You can pass NULL as the exc argument if you don't want to
 * catch exceptions, otherwise, *exc will be set to the exception
 * thrown, if any.  if an exception is thrown, you can't use the
 * MonoObject* result from the function.
 *
 * Returns: the value from invoking the get method on the property.
 */
MonoObject*
mono_property_get_value (MonoProperty *prop, void *obj, void **params, MonoObject **exc)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoError error;
	MonoObject *val = do_runtime_invoke (prop->get, obj, params, exc, &error);
	if (exc && *exc == NULL && !mono_error_ok (&error)) {
		*exc = (MonoObject*) mono_error_convert_to_exception (&error);
	} else {
		mono_error_cleanup (&error); /* FIXME don't raise here */
	}

	return val;
}

/**
 * mono_property_get_value_checked:
 * @prop: MonoProperty to fetch
 * @obj: instance object on which to act
 * @params: parameters to pass to the propery
 * @error: set on error
 *
 * Invokes the property's get method with the given arguments on the
 * object instance obj (or NULL for static properties). 
 * 
 * If an exception is thrown, you can't use the
 * MonoObject* result from the function.  The exception will be propagated via @error.
 *
 * Returns: the value from invoking the get method on the property. On
 * failure returns NULL and sets @error.
 */
MonoObject*
mono_property_get_value_checked (MonoProperty *prop, void *obj, void **params, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoObject *exc;
	MonoObject *val = do_runtime_invoke (prop->get, obj, params, &exc, error);
	if (exc != NULL && !is_ok (error))
		mono_error_set_exception_instance (error, (MonoException*) exc);
	if (!is_ok (error))
		val = NULL;
	return val;
}


/*
 * mono_nullable_init:
 * @buf: The nullable structure to initialize.
 * @value: the value to initialize from
 * @klass: the type for the object
 *
 * Initialize the nullable structure pointed to by @buf from @value which
 * should be a boxed value type.   The size of @buf should be able to hold
 * as much data as the @klass->instance_size (which is the number of bytes
 * that will be copies).
 *
 * Since Nullables have variable structure, we can not define a C
 * structure for them.
 */
void
mono_nullable_init (guint8 *buf, MonoObject *value, MonoClass *klass)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoClass *param_class = klass->cast_class;

	mono_class_setup_fields (klass);
	g_assert (klass->fields_inited);
				
	g_assert (mono_class_from_mono_type (klass->fields [0].type) == param_class);
	g_assert (mono_class_from_mono_type (klass->fields [1].type) == mono_defaults.boolean_class);

	*(guint8*)(buf + klass->fields [1].offset - sizeof (MonoObject)) = value ? 1 : 0;
	if (value) {
		if (param_class->has_references)
			mono_gc_wbarrier_value_copy (buf + klass->fields [0].offset - sizeof (MonoObject), mono_object_unbox (value), 1, param_class);
		else
			mono_gc_memmove_atomic (buf + klass->fields [0].offset - sizeof (MonoObject), mono_object_unbox (value), mono_class_value_size (param_class, NULL));
	} else {
		mono_gc_bzero_atomic (buf + klass->fields [0].offset - sizeof (MonoObject), mono_class_value_size (param_class, NULL));
	}
}

/**
 * mono_nullable_box:
 * @buf: The buffer representing the data to be boxed
 * @klass: the type to box it as.
 * @error: set on oerr
 *
 * Creates a boxed vtype or NULL from the Nullable structure pointed to by
 * @buf.  On failure returns NULL and sets @error
 */
MonoObject*
mono_nullable_box (guint8 *buf, MonoClass *klass, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	error_init (error);
	MonoClass *param_class = klass->cast_class;

	mono_class_setup_fields (klass);
	g_assert (klass->fields_inited);

	g_assert (mono_class_from_mono_type (klass->fields [0].type) == param_class);
	g_assert (mono_class_from_mono_type (klass->fields [1].type) == mono_defaults.boolean_class);

	if (*(guint8*)(buf + klass->fields [1].offset - sizeof (MonoObject))) {
		MonoObject *o = mono_object_new_checked (mono_domain_get (), param_class, error);
		return_val_if_nok (error, NULL);
		if (param_class->has_references)
			mono_gc_wbarrier_value_copy (mono_object_unbox (o), buf + klass->fields [0].offset - sizeof (MonoObject), 1, param_class);
		else
			mono_gc_memmove_atomic (mono_object_unbox (o), buf + klass->fields [0].offset - sizeof (MonoObject), mono_class_value_size (param_class, NULL));
		return o;
	}
	else
		return NULL;
}

/**
 * mono_get_delegate_invoke:
 * @klass: The delegate class
 *
 * Returns: the MonoMethod for the "Invoke" method in the delegate klass or NULL if @klass is a broken delegate type
 */
MonoMethod *
mono_get_delegate_invoke (MonoClass *klass)
{
	MONO_REQ_GC_NEUTRAL_MODE;

	MonoMethod *im;

	/* This is called at runtime, so avoid the slower search in metadata */
	mono_class_setup_methods (klass);
	if (mono_class_has_failure (klass))
		return NULL;
	im = mono_class_get_method_from_name (klass, "Invoke", -1);
	return im;
}

/**
 * mono_get_delegate_begin_invoke:
 * @klass: The delegate class
 *
 * Returns: the MonoMethod for the "BeginInvoke" method in the delegate klass or NULL if @klass is a broken delegate type
 */
MonoMethod *
mono_get_delegate_begin_invoke (MonoClass *klass)
{
	MONO_REQ_GC_NEUTRAL_MODE;

	MonoMethod *im;

	/* This is called at runtime, so avoid the slower search in metadata */
	mono_class_setup_methods (klass);
	if (mono_class_has_failure (klass))
		return NULL;
	im = mono_class_get_method_from_name (klass, "BeginInvoke", -1);
	return im;
}

/**
 * mono_get_delegate_end_invoke:
 * @klass: The delegate class
 *
 * Returns: the MonoMethod for the "EndInvoke" method in the delegate klass or NULL if @klass is a broken delegate type
 */
MonoMethod *
mono_get_delegate_end_invoke (MonoClass *klass)
{
	MONO_REQ_GC_NEUTRAL_MODE;

	MonoMethod *im;

	/* This is called at runtime, so avoid the slower search in metadata */
	mono_class_setup_methods (klass);
	if (mono_class_has_failure (klass))
		return NULL;
	im = mono_class_get_method_from_name (klass, "EndInvoke", -1);
	return im;
}

/**
 * mono_runtime_delegate_invoke:
 * @delegate: pointer to a delegate object.
 * @params: parameters for the delegate.
 * @exc: Pointer to the exception result.
 *
 * Invokes the delegate method @delegate with the parameters provided.
 *
 * You can pass NULL as the exc argument if you don't want to
 * catch exceptions, otherwise, *exc will be set to the exception
 * thrown, if any.  if an exception is thrown, you can't use the
 * MonoObject* result from the function.
 */
MonoObject*
mono_runtime_delegate_invoke (MonoObject *delegate, void **params, MonoObject **exc)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoError error;
	if (exc) {
		MonoObject *result = mono_runtime_delegate_try_invoke (delegate, params, exc, &error);
		if (*exc) {
			mono_error_cleanup (&error);
			return NULL;
		} else {
			if (!is_ok (&error))
				*exc = (MonoObject*)mono_error_convert_to_exception (&error);
			return result;
		}
	} else {
		MonoObject *result = mono_runtime_delegate_invoke_checked (delegate, params, &error);
		mono_error_raise_exception (&error); /* OK to throw, external only without a good alternative */
		return result;
	}
}

/**
 * mono_runtime_delegate_try_invoke:
 * @delegate: pointer to a delegate object.
 * @params: parameters for the delegate.
 * @exc: Pointer to the exception result.
 * @error: set on error
 *
 * Invokes the delegate method @delegate with the parameters provided.
 *
 * You can pass NULL as the exc argument if you don't want to
 * catch exceptions, otherwise, *exc will be set to the exception
 * thrown, if any.  On failure to execute, @error will be set.
 * if an exception is thrown, you can't use the
 * MonoObject* result from the function.
 */
MonoObject*
mono_runtime_delegate_try_invoke (MonoObject *delegate, void **params, MonoObject **exc, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	error_init (error);
	MonoMethod *im;
	MonoClass *klass = delegate->vtable->klass;
	MonoObject *o;

	im = mono_get_delegate_invoke (klass);
	if (!im)
		g_error ("Could not lookup delegate invoke method for delegate %s", mono_type_get_full_name (klass));

	if (exc) {
		o = mono_runtime_try_invoke (im, delegate, params, exc, error);
	} else {
		o = mono_runtime_invoke_checked (im, delegate, params, error);
	}

	return o;
}

/**
 * mono_runtime_delegate_invoke_checked:
 * @delegate: pointer to a delegate object.
 * @params: parameters for the delegate.
 * @error: set on error
 *
 * Invokes the delegate method @delegate with the parameters provided.
 *
 * On failure @error will be set and you can't use the MonoObject*
 * result from the function.
 */
MonoObject*
mono_runtime_delegate_invoke_checked (MonoObject *delegate, void **params, MonoError *error)
{
	error_init (error);
	return mono_runtime_delegate_try_invoke (delegate, params, NULL, error);
}

static char **main_args = NULL;
static int num_main_args = 0;

/**
 * mono_runtime_get_main_args:
 *
 * Returns: a MonoArray with the arguments passed to the main program
 */
MonoArray*
mono_runtime_get_main_args (void)
{
	MONO_REQ_GC_UNSAFE_MODE;
	MonoError error;
	MonoArray *result = mono_runtime_get_main_args_checked (&error);
	mono_error_assert_ok (&error);
	return result;
}

/**
 * mono_runtime_get_main_args:
 * @error: set on error
 *
 * Returns: a MonoArray with the arguments passed to the main
 * program. On failure returns NULL and sets @error.
 */
MonoArray*
mono_runtime_get_main_args_checked (MonoError *error)
{
	MonoArray *res;
	int i;
	MonoDomain *domain = mono_domain_get ();

	error_init (error);

	res = (MonoArray*)mono_array_new_checked (domain, mono_defaults.string_class, num_main_args, error);
	return_val_if_nok (error, NULL);

	for (i = 0; i < num_main_args; ++i)
		mono_array_setref (res, i, mono_string_new (domain, main_args [i]));

	return res;
}

static void
free_main_args (void)
{
	MONO_REQ_GC_NEUTRAL_MODE;

	int i;

	for (i = 0; i < num_main_args; ++i)
		g_free (main_args [i]);
	g_free (main_args);
	num_main_args = 0;
	main_args = NULL;
}

/**
 * mono_runtime_set_main_args:
 * @argc: number of arguments from the command line
 * @argv: array of strings from the command line
 *
 * Set the command line arguments from an embedding application that doesn't otherwise call
 * mono_runtime_run_main ().
 */
int
mono_runtime_set_main_args (int argc, char* argv[])
{
	MONO_REQ_GC_NEUTRAL_MODE;

	int i;

	free_main_args ();
	main_args = g_new0 (char*, argc);
	num_main_args = argc;

	for (i = 0; i < argc; ++i) {
		gchar *utf8_arg;

		utf8_arg = mono_utf8_from_external (argv[i]);
		if (utf8_arg == NULL) {
			g_print ("\nCannot determine the text encoding for argument %d (%s).\n", i, argv [i]);
			g_print ("Please add the correct encoding to MONO_EXTERNAL_ENCODINGS and try again.\n");
			exit (-1);
		}

		main_args [i] = utf8_arg;
	}

	return 0;
}

/*
 * Prepare an array of arguments in order to execute a standard Main()
 * method (argc/argv contains the executable name). This method also
 * sets the command line argument value needed by System.Environment.
 * 
 */
static MonoArray*
prepare_run_main (MonoMethod *method, int argc, char *argv[])
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoError error;
	int i;
	MonoArray *args = NULL;
	MonoDomain *domain = mono_domain_get ();
	gchar *utf8_fullpath;
	MonoMethodSignature *sig;

	g_assert (method != NULL);
	
	mono_thread_set_main (mono_thread_current ());

	main_args = g_new0 (char*, argc);
	num_main_args = argc;

	if (!g_path_is_absolute (argv [0])) {
		gchar *basename = g_path_get_basename (argv [0]);
		gchar *fullpath = g_build_filename (method->klass->image->assembly->basedir,
						    basename,
						    NULL);

		utf8_fullpath = mono_utf8_from_external (fullpath);
		if(utf8_fullpath == NULL) {
			/* Printing the arg text will cause glib to
			 * whinge about "Invalid UTF-8", but at least
			 * its relevant, and shows the problem text
			 * string.
			 */
			g_print ("\nCannot determine the text encoding for the assembly location: %s\n", fullpath);
			g_print ("Please add the correct encoding to MONO_EXTERNAL_ENCODINGS and try again.\n");
			exit (-1);
		}

		g_free (fullpath);
		g_free (basename);
	} else {
		utf8_fullpath = mono_utf8_from_external (argv[0]);
		if(utf8_fullpath == NULL) {
			g_print ("\nCannot determine the text encoding for the assembly location: %s\n", argv[0]);
			g_print ("Please add the correct encoding to MONO_EXTERNAL_ENCODINGS and try again.\n");
			exit (-1);
		}
	}

	main_args [0] = utf8_fullpath;

	for (i = 1; i < argc; ++i) {
		gchar *utf8_arg;

		utf8_arg=mono_utf8_from_external (argv[i]);
		if(utf8_arg==NULL) {
			/* Ditto the comment about Invalid UTF-8 here */
			g_print ("\nCannot determine the text encoding for argument %d (%s).\n", i, argv[i]);
			g_print ("Please add the correct encoding to MONO_EXTERNAL_ENCODINGS and try again.\n");
			exit (-1);
		}

		main_args [i] = utf8_arg;
	}
	argc--;
	argv++;

	sig = mono_method_signature (method);
	if (!sig) {
		g_print ("Unable to load Main method.\n");
		exit (-1);
	}

	if (sig->param_count) {
		args = (MonoArray*)mono_array_new_checked (domain, mono_defaults.string_class, argc, &error);
		mono_error_assert_ok (&error);
		for (i = 0; i < argc; ++i) {
			/* The encodings should all work, given that
			 * we've checked all these args for the
			 * main_args array.
			 */
			gchar *str = mono_utf8_from_external (argv [i]);
			MonoString *arg = mono_string_new (domain, str);
			mono_array_setref (args, i, arg);
			g_free (str);
		}
	} else {
		args = (MonoArray*)mono_array_new_checked (domain, mono_defaults.string_class, 0, &error);
		mono_error_assert_ok (&error);
	}
	
	mono_assembly_set_main (method->klass->image->assembly);

	return args;
}

/**
 * mono_runtime_run_main:
 * @method: the method to start the application with (usually Main)
 * @argc: number of arguments from the command line
 * @argv: array of strings from the command line
 * @exc: excetption results
 *
 * Execute a standard Main() method (argc/argv contains the
 * executable name). This method also sets the command line argument value
 * needed by System.Environment.
 *
 * 
 */
int
mono_runtime_run_main (MonoMethod *method, int argc, char* argv[],
		       MonoObject **exc)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoError error;
	MonoArray *args = prepare_run_main (method, argc, argv);
	int res;
	if (exc) {
		res = mono_runtime_try_exec_main (method, args, exc);
	} else {
		res = mono_runtime_exec_main_checked (method, args, &error);
		mono_error_raise_exception (&error); /* OK to throw, external only without a better alternative */
	}
	return res;
}

/**
 * mono_runtime_run_main_checked:
 * @method: the method to start the application with (usually Main)
 * @argc: number of arguments from the command line
 * @argv: array of strings from the command line
 * @error: set on error
 *
 * Execute a standard Main() method (argc/argv contains the
 * executable name). This method also sets the command line argument value
 * needed by System.Environment.  On failure sets @error.
 *
 * 
 */
int
mono_runtime_run_main_checked (MonoMethod *method, int argc, char* argv[],
			       MonoError *error)
{
	error_init (error);
	MonoArray *args = prepare_run_main (method, argc, argv);
	return mono_runtime_exec_main_checked (method, args, error);
}

/**
 * mono_runtime_try_run_main:
 * @method: the method to start the application with (usually Main)
 * @argc: number of arguments from the command line
 * @argv: array of strings from the command line
 * @exc: set if Main throws an exception
 * @error: set if Main can't be executed
 *
 * Execute a standard Main() method (argc/argv contains the executable
 * name). This method also sets the command line argument value needed
 * by System.Environment.  On failure sets @error if Main can't be
 * executed or @exc if it threw and exception.
 *
 * 
 */
int
mono_runtime_try_run_main (MonoMethod *method, int argc, char* argv[],
			   MonoObject **exc)
{
	g_assert (exc);
	MonoArray *args = prepare_run_main (method, argc, argv);
	return mono_runtime_try_exec_main (method, args, exc);
}


static MonoObject*
serialize_object (MonoObject *obj, gboolean *failure, MonoObject **exc)
{
	static MonoMethod *serialize_method;

	MonoError error;
	void *params [1];
	MonoObject *array;

	if (!serialize_method) {
		MonoClass *klass = mono_class_get_remoting_services_class ();
		serialize_method = mono_class_get_method_from_name (klass, "SerializeCallData", -1);
	}

	if (!serialize_method) {
		*failure = TRUE;
		return NULL;
	}

	g_assert (!mono_class_is_marshalbyref (mono_object_class (obj)));

	params [0] = obj;
	*exc = NULL;

	array = mono_runtime_try_invoke (serialize_method, NULL, params, exc, &error);
	if (*exc == NULL && !mono_error_ok (&error))
		*exc = (MonoObject*) mono_error_convert_to_exception (&error); /* FIXME convert serialize_object to MonoError */
	else
		mono_error_cleanup (&error);

	if (*exc)
		*failure = TRUE;

	return array;
}

static MonoObject*
deserialize_object (MonoObject *obj, gboolean *failure, MonoObject **exc)
{
	MONO_REQ_GC_UNSAFE_MODE;

	static MonoMethod *deserialize_method;

	MonoError error;
	void *params [1];
	MonoObject *result;

	if (!deserialize_method) {
		MonoClass *klass = mono_class_get_remoting_services_class ();
		deserialize_method = mono_class_get_method_from_name (klass, "DeserializeCallData", -1);
	}
	if (!deserialize_method) {
		*failure = TRUE;
		return NULL;
	}

	params [0] = obj;
	*exc = NULL;

	result = mono_runtime_try_invoke (deserialize_method, NULL, params, exc, &error);
	if (*exc == NULL && !mono_error_ok (&error))
		*exc = (MonoObject*) mono_error_convert_to_exception (&error); /* FIXME convert deserialize_object to MonoError */
	else
		mono_error_cleanup (&error);

	if (*exc)
		*failure = TRUE;

	return result;
}

#ifndef DISABLE_REMOTING
static MonoObject*
make_transparent_proxy (MonoObject *obj, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	static MonoMethod *get_proxy_method;

	MonoDomain *domain = mono_domain_get ();
	MonoRealProxy *real_proxy;
	MonoReflectionType *reflection_type;
	MonoTransparentProxy *transparent_proxy;

	error_init (error);

	if (!get_proxy_method)
		get_proxy_method = mono_class_get_method_from_name (mono_defaults.real_proxy_class, "GetTransparentProxy", 0);

	g_assert (mono_class_is_marshalbyref (obj->vtable->klass));

	real_proxy = (MonoRealProxy*) mono_object_new_checked (domain, mono_defaults.real_proxy_class, error);
	return_val_if_nok (error, NULL);
	reflection_type = mono_type_get_object_checked (domain, &obj->vtable->klass->byval_arg, error);
	return_val_if_nok (error, NULL);

	MONO_OBJECT_SETREF (real_proxy, class_to_proxy, reflection_type);
	MONO_OBJECT_SETREF (real_proxy, unwrapped_server, obj);

	MonoObject *exc = NULL;

	transparent_proxy = (MonoTransparentProxy*) mono_runtime_try_invoke (get_proxy_method, real_proxy, NULL, &exc, error);
	if (exc != NULL && is_ok (error))
		mono_error_set_exception_instance (error, (MonoException*)exc);

	return (MonoObject*) transparent_proxy;
}
#endif /* DISABLE_REMOTING */

/**
 * mono_object_xdomain_representation
 * @obj: an object
 * @target_domain: a domain
 * @error: set on error.
 *
 * Creates a representation of obj in the domain target_domain.  This
 * is either a copy of obj arrived through via serialization and
 * deserialization or a proxy, depending on whether the object is
 * serializable or marshal by ref.  obj must not be in target_domain.
 *
 * If the object cannot be represented in target_domain, NULL is
 * returned and @error is set appropriately.
 */
MonoObject*
mono_object_xdomain_representation (MonoObject *obj, MonoDomain *target_domain, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	error_init (error);
	MonoObject *deserialized = NULL;

#ifndef DISABLE_REMOTING
	if (mono_class_is_marshalbyref (mono_object_class (obj))) {
		deserialized = make_transparent_proxy (obj, error);
	} 
	else
#endif
	{
		gboolean failure = FALSE;
		MonoDomain *domain = mono_domain_get ();
		MonoObject *serialized;
		MonoObject *exc = NULL;

		mono_domain_set_internal_with_options (mono_object_domain (obj), FALSE);
		serialized = serialize_object (obj, &failure, &exc);
		mono_domain_set_internal_with_options (target_domain, FALSE);
		if (!failure)
			deserialized = deserialize_object (serialized, &failure, &exc);
		if (domain != target_domain)
			mono_domain_set_internal_with_options (domain, FALSE);
		if (failure)
			mono_error_set_exception_instance (error, (MonoException*)exc);
	}

	return deserialized;
}

/* Used in call_unhandled_exception_delegate */
static MonoObject *
create_unhandled_exception_eventargs (MonoObject *exc, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	error_init (error);
	MonoClass *klass;
	gpointer args [2];
	MonoMethod *method = NULL;
	MonoBoolean is_terminating = TRUE;
	MonoObject *obj;

	klass = mono_class_get_unhandled_exception_event_args_class ();
	mono_class_init (klass);

	/* UnhandledExceptionEventArgs only has 1 public ctor with 2 args */
	method = mono_class_get_method_from_name_flags (klass, ".ctor", 2, METHOD_ATTRIBUTE_PUBLIC);
	g_assert (method);

	args [0] = exc;
	args [1] = &is_terminating;

	obj = mono_object_new_checked (mono_domain_get (), klass, error);
	return_val_if_nok (error, NULL);

	mono_runtime_invoke_checked (method, obj, args, error);
	return_val_if_nok (error, NULL);

	return obj;
}

/* Used in mono_unhandled_exception */
static void
call_unhandled_exception_delegate (MonoDomain *domain, MonoObject *delegate, MonoObject *exc) {
	MONO_REQ_GC_UNSAFE_MODE;

	MonoError error;
	MonoObject *e = NULL;
	gpointer pa [2];
	MonoDomain *current_domain = mono_domain_get ();

	if (domain != current_domain)
		mono_domain_set_internal_with_options (domain, FALSE);

	g_assert (domain == mono_object_domain (domain->domain));

	if (mono_object_domain (exc) != domain) {

		exc = mono_object_xdomain_representation (exc, domain, &error);
		if (!exc) {
			if (!is_ok (&error)) {
				MonoError inner_error;
				MonoException *serialization_exc = mono_error_convert_to_exception (&error);
				exc = mono_object_xdomain_representation ((MonoObject*)serialization_exc, domain, &inner_error);
				mono_error_assert_ok (&inner_error);
			} else {
				exc = (MonoObject*) mono_exception_from_name_msg (mono_get_corlib (),
						"System.Runtime.Serialization", "SerializationException",
						"Could not serialize unhandled exception.");
			}
		}
	}
	g_assert (mono_object_domain (exc) == domain);

	pa [0] = domain->domain;
	pa [1] = create_unhandled_exception_eventargs (exc, &error);
	mono_error_assert_ok (&error);
	mono_runtime_delegate_try_invoke (delegate, pa, &e, &error);
	if (!is_ok (&error)) {
		if (e == NULL)
			e = (MonoObject*)mono_error_convert_to_exception (&error);
		else
			mono_error_cleanup (&error);
	}

	if (domain != current_domain)
		mono_domain_set_internal_with_options (current_domain, FALSE);

	if (e) {
		gchar *msg = mono_string_to_utf8_checked (((MonoException *) e)->message, &error);
		if (!mono_error_ok (&error)) {
			g_warning ("Exception inside UnhandledException handler with invalid message (Invalid characters)\n");
			mono_error_cleanup (&error);
		} else {
			g_warning ("exception inside UnhandledException handler: %s\n", msg);
			g_free (msg);
		}
	}
}

static MonoRuntimeUnhandledExceptionPolicy runtime_unhandled_exception_policy = MONO_UNHANDLED_POLICY_CURRENT;

/**
 * mono_runtime_unhandled_exception_policy_set:
 * @policy: the new policy
 * 
 * This is a VM internal routine.
 *
 * Sets the runtime policy for handling unhandled exceptions.
 */
void
mono_runtime_unhandled_exception_policy_set (MonoRuntimeUnhandledExceptionPolicy policy) {
	runtime_unhandled_exception_policy = policy;
}

/**
 * mono_runtime_unhandled_exception_policy_get:
 *
 * This is a VM internal routine.
 *
 * Gets the runtime policy for handling unhandled exceptions.
 */
MonoRuntimeUnhandledExceptionPolicy
mono_runtime_unhandled_exception_policy_get (void) {
	return runtime_unhandled_exception_policy;
}

/**
 * mono_unhandled_exception:
 * @exc: exception thrown
 *
 * This is a VM internal routine.
 *
 * We call this function when we detect an unhandled exception
 * in the default domain.
 *
 * It invokes the * UnhandledException event in AppDomain or prints
 * a warning to the console 
 */
void
mono_unhandled_exception (MonoObject *exc)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoError error;
	MonoClassField *field;
	MonoDomain *current_domain, *root_domain;
	MonoObject *current_appdomain_delegate = NULL, *root_appdomain_delegate = NULL;

	if (mono_class_has_parent (exc->vtable->klass, mono_defaults.threadabortexception_class))
		return;

	field = mono_class_get_field_from_name (mono_defaults.appdomain_class, "UnhandledException");
	g_assert (field);

	current_domain = mono_domain_get ();
	root_domain = mono_get_root_domain ();

	root_appdomain_delegate = mono_field_get_value_object_checked (root_domain, field, (MonoObject*) root_domain->domain, &error);
	mono_error_assert_ok (&error);
	if (current_domain != root_domain) {
		current_appdomain_delegate = mono_field_get_value_object_checked (current_domain, field, (MonoObject*) current_domain->domain, &error);
		mono_error_assert_ok (&error);
	}

	if (!current_appdomain_delegate && !root_appdomain_delegate) {
		mono_print_unhandled_exception (exc);
	} else {
		/* unhandled exception callbacks must not be aborted */
		mono_threads_begin_abort_protected_block ();
		if (root_appdomain_delegate)
			call_unhandled_exception_delegate (root_domain, root_appdomain_delegate, exc);
		if (current_appdomain_delegate)
			call_unhandled_exception_delegate (current_domain, current_appdomain_delegate, exc);
		mono_threads_end_abort_protected_block ();
	}

	/* set exitcode only if we will abort the process */
	if ((main_thread && mono_thread_internal_current () == main_thread->internal_thread)
		 || mono_runtime_unhandled_exception_policy_get () == MONO_UNHANDLED_POLICY_CURRENT)
	{
		mono_environment_exitcode_set (1);
	}
}

/**
 * mono_runtime_exec_managed_code:
 * @domain: Application domain
 * @main_func: function to invoke from the execution thread
 * @main_args: parameter to the main_func
 *
 * Launch a new thread to execute a function
 *
 * main_func is called back from the thread with main_args as the
 * parameter.  The callback function is expected to start Main()
 * eventually.  This function then waits for all managed threads to
 * finish.
 * It is not necesseray anymore to execute managed code in a subthread,
 * so this function should not be used anymore by default: just
 * execute the code and then call mono_thread_manage ().
 */
void
mono_runtime_exec_managed_code (MonoDomain *domain,
				MonoMainThreadFunc main_func,
				gpointer main_args)
{
	MonoError error;
	mono_thread_create_checked (domain, main_func, main_args, &error);
	mono_error_assert_ok (&error);

	mono_thread_manage ();
}

static void
prepare_thread_to_exec_main (MonoDomain *domain, MonoMethod *method)
{
	MonoInternalThread* thread = mono_thread_internal_current ();
	MonoCustomAttrInfo* cinfo;
	gboolean has_stathread_attribute;

	if (!domain->entry_assembly) {
		gchar *str;
		MonoAssembly *assembly;

		assembly = method->klass->image->assembly;
		domain->entry_assembly = assembly;
		/* Domains created from another domain already have application_base and configuration_file set */
		if (domain->setup->application_base == NULL) {
			MONO_OBJECT_SETREF (domain->setup, application_base, mono_string_new (domain, assembly->basedir));
		}

		if (domain->setup->configuration_file == NULL) {
			str = g_strconcat (assembly->image->name, ".config", NULL);
			MONO_OBJECT_SETREF (domain->setup, configuration_file, mono_string_new (domain, str));
			g_free (str);
			mono_domain_set_options_from_config (domain);
		}
	}

	MonoError cattr_error;
	cinfo = mono_custom_attrs_from_method_checked (method, &cattr_error);
	mono_error_cleanup (&cattr_error); /* FIXME warn here? */
	if (cinfo) {
		has_stathread_attribute = mono_custom_attrs_has_attr (cinfo, mono_class_get_sta_thread_attribute_class ());
		if (!cinfo->cached)
			mono_custom_attrs_free (cinfo);
	} else {
		has_stathread_attribute = FALSE;
 	}
	if (has_stathread_attribute) {
		thread->apartment_state = ThreadApartmentState_STA;
	} else {
		thread->apartment_state = ThreadApartmentState_MTA;
	}
	mono_thread_init_apartment_state ();

}

static int
do_exec_main_checked (MonoMethod *method, MonoArray *args, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	gpointer pa [1];
	int rval;

	error_init (error);
	g_assert (args);

	pa [0] = args;

	/* FIXME: check signature of method */
	if (mono_method_signature (method)->ret->type == MONO_TYPE_I4) {
		MonoObject *res;
		res = mono_runtime_invoke_checked (method, NULL, pa, error);
		if (is_ok (error))
			rval = *(guint32 *)((char *)res + sizeof (MonoObject));
		else
			rval = -1;
		mono_environment_exitcode_set (rval);
	} else {
		mono_runtime_invoke_checked (method, NULL, pa, error);

		if (is_ok (error))
			rval = 0;
		else {
			rval = -1;
		}
	}
	return rval;
}

static int
do_try_exec_main (MonoMethod *method, MonoArray *args, MonoObject **exc)
{
	MONO_REQ_GC_UNSAFE_MODE;

	gpointer pa [1];
	int rval;

	g_assert (args);
	g_assert (exc);

	pa [0] = args;

	/* FIXME: check signature of method */
	if (mono_method_signature (method)->ret->type == MONO_TYPE_I4) {
		MonoError inner_error;
		MonoObject *res;
		res = mono_runtime_try_invoke (method, NULL, pa, exc, &inner_error);
		if (*exc == NULL && !mono_error_ok (&inner_error))
			*exc = (MonoObject*) mono_error_convert_to_exception (&inner_error);
		else
			mono_error_cleanup (&inner_error);

		if (*exc == NULL)
			rval = *(guint32 *)((char *)res + sizeof (MonoObject));
		else
			rval = -1;

		mono_environment_exitcode_set (rval);
	} else {
		MonoError inner_error;
		mono_runtime_try_invoke (method, NULL, pa, exc, &inner_error);
		if (*exc == NULL && !mono_error_ok (&inner_error))
			*exc = (MonoObject*) mono_error_convert_to_exception (&inner_error);
		else
			mono_error_cleanup (&inner_error);

		if (*exc == NULL)
			rval = 0;
		else {
			/* If the return type of Main is void, only
			 * set the exitcode if an exception was thrown
			 * (we don't want to blow away an
			 * explicitly-set exit code)
			 */
			rval = -1;
			mono_environment_exitcode_set (rval);
		}
	}

	return rval;
}

/*
 * Execute a standard Main() method (args doesn't contain the
 * executable name).
 */
int
mono_runtime_exec_main (MonoMethod *method, MonoArray *args, MonoObject **exc)
{
	MonoError error;
	prepare_thread_to_exec_main (mono_object_domain (args), method);
	if (exc) {
		int rval = do_try_exec_main (method, args, exc);
		return rval;
	} else {
		int rval = do_exec_main_checked (method, args, &error);
		mono_error_raise_exception (&error); /* OK to throw, external only with no better option */
		return rval;
	}
}

/*
 * Execute a standard Main() method (args doesn't contain the
 * executable name).
 *
 * On failure sets @error
 */
int
mono_runtime_exec_main_checked (MonoMethod *method, MonoArray *args, MonoError *error)
{
	error_init (error);
	prepare_thread_to_exec_main (mono_object_domain (args), method);
	return do_exec_main_checked (method, args, error);
}

/*
 * Execute a standard Main() method (args doesn't contain the
 * executable name).
 *
 * On failure sets @error if Main couldn't be executed, or @exc if it threw an exception.
 */
int
mono_runtime_try_exec_main (MonoMethod *method, MonoArray *args, MonoObject **exc)
{
	prepare_thread_to_exec_main (mono_object_domain (args), method);
	return do_try_exec_main (method, args, exc);
}



/** invoke_array_extract_argument:
 * @params: array of arguments to the method.
 * @i: the index of the argument to extract.
 * @t: ith type from the method signature.
 * @has_byref_nullables: outarg - TRUE if method expects a byref nullable argument
 * @error: set on error.
 *
 * Given an array of method arguments, return the ith one using the corresponding type
 * to perform necessary unboxing.  If method expects a ref nullable argument, writes TRUE to @has_byref_nullables.
 *
 * On failure sets @error and returns NULL.
 */
static gpointer
invoke_array_extract_argument (MonoArray *params, int i, MonoType *t, gboolean* has_byref_nullables, MonoError *error)
{
	MonoType *t_orig = t;
	gpointer result = NULL;
	error_init (error);
		again:
			switch (t->type) {
			case MONO_TYPE_U1:
			case MONO_TYPE_I1:
			case MONO_TYPE_BOOLEAN:
			case MONO_TYPE_U2:
			case MONO_TYPE_I2:
			case MONO_TYPE_CHAR:
			case MONO_TYPE_U:
			case MONO_TYPE_I:
			case MONO_TYPE_U4:
			case MONO_TYPE_I4:
			case MONO_TYPE_U8:
			case MONO_TYPE_I8:
			case MONO_TYPE_R4:
			case MONO_TYPE_R8:
			case MONO_TYPE_VALUETYPE:
				if (t->type == MONO_TYPE_VALUETYPE && mono_class_is_nullable (mono_class_from_mono_type (t_orig))) {
					/* The runtime invoke wrapper needs the original boxed vtype, it does handle byref values as well. */
					result = mono_array_get (params, MonoObject*, i);
					if (t->byref)
						*has_byref_nullables = TRUE;
				} else {
					/* MS seems to create the objects if a null is passed in */
					if (!mono_array_get (params, MonoObject*, i)) {
						MonoObject *o = mono_object_new_checked (mono_domain_get (), mono_class_from_mono_type (t_orig), error);
						return_val_if_nok (error, NULL);
						mono_array_setref (params, i, o); 
					}

					if (t->byref) {
						/*
						 * We can't pass the unboxed vtype byref to the callee, since
						 * that would mean the callee would be able to modify boxed
						 * primitive types. So we (and MS) make a copy of the boxed
						 * object, pass that to the callee, and replace the original
						 * boxed object in the arg array with the copy.
						 */
						MonoObject *orig = mono_array_get (params, MonoObject*, i);
						MonoObject *copy = mono_value_box_checked (mono_domain_get (), orig->vtable->klass, mono_object_unbox (orig), error);
						return_val_if_nok (error, NULL);
						mono_array_setref (params, i, copy);
					}
						
					result = mono_object_unbox (mono_array_get (params, MonoObject*, i));
				}
				break;
			case MONO_TYPE_STRING:
			case MONO_TYPE_OBJECT:
			case MONO_TYPE_CLASS:
			case MONO_TYPE_ARRAY:
			case MONO_TYPE_SZARRAY:
				if (t->byref)
					result = mono_array_addr (params, MonoObject*, i);
					// FIXME: I need to check this code path
				else
					result = mono_array_get (params, MonoObject*, i);
				break;
			case MONO_TYPE_GENERICINST:
				if (t->byref)
					t = &t->data.generic_class->container_class->this_arg;
				else
					t = &t->data.generic_class->container_class->byval_arg;
				goto again;
			case MONO_TYPE_PTR: {
				MonoObject *arg;

				/* The argument should be an IntPtr */
				arg = mono_array_get (params, MonoObject*, i);
				if (arg == NULL) {
					result = NULL;
				} else {
					g_assert (arg->vtable->klass == mono_defaults.int_class);
					result = ((MonoIntPtr*)arg)->m_value;
				}
				break;
			}
			default:
				g_error ("type 0x%x not handled in mono_runtime_invoke_array", t_orig->type);
			}
	return result;
}
/**
 * mono_runtime_invoke_array:
 * @method: method to invoke
 * @obJ: object instance
 * @params: arguments to the method
 * @exc: exception information.
 *
 * Invokes the method represented by @method on the object @obj.
 *
 * obj is the 'this' pointer, it should be NULL for static
 * methods, a MonoObject* for object instances and a pointer to
 * the value type for value types.
 *
 * The params array contains the arguments to the method with the
 * same convention: MonoObject* pointers for object instances and
 * pointers to the value type otherwise. The _invoke_array
 * variant takes a C# object[] as the params argument (MonoArray
 * *params): in this case the value types are boxed inside the
 * respective reference representation.
 * 
 * From unmanaged code you'll usually use the
 * mono_runtime_invoke_checked() variant.
 *
 * Note that this function doesn't handle virtual methods for
 * you, it will exec the exact method you pass: we still need to
 * expose a function to lookup the derived class implementation
 * of a virtual method (there are examples of this in the code,
 * though).
 * 
 * You can pass NULL as the exc argument if you don't want to
 * catch exceptions, otherwise, *exc will be set to the exception
 * thrown, if any.  if an exception is thrown, you can't use the
 * MonoObject* result from the function.
 * 
 * If the method returns a value type, it is boxed in an object
 * reference.
 */
MonoObject*
mono_runtime_invoke_array (MonoMethod *method, void *obj, MonoArray *params,
			   MonoObject **exc)
{
	MonoError error;
	if (exc) {
		MonoObject *result = mono_runtime_try_invoke_array (method, obj, params, exc, &error);
		if (*exc) {
			mono_error_cleanup (&error);
			return NULL;
		} else {
			if (!is_ok (&error))
				*exc = (MonoObject*)mono_error_convert_to_exception (&error);
			return result;
		}
	} else {
		MonoObject *result = mono_runtime_try_invoke_array (method, obj, params, NULL, &error);
		mono_error_raise_exception (&error); /* OK to throw, external only without a good alternative */
		return result;
	}
}

/**
 * mono_runtime_invoke_array_checked:
 * @method: method to invoke
 * @obJ: object instance
 * @params: arguments to the method
 * @error: set on failure.
 *
 * Invokes the method represented by @method on the object @obj.
 *
 * obj is the 'this' pointer, it should be NULL for static
 * methods, a MonoObject* for object instances and a pointer to
 * the value type for value types.
 *
 * The params array contains the arguments to the method with the
 * same convention: MonoObject* pointers for object instances and
 * pointers to the value type otherwise. The _invoke_array
 * variant takes a C# object[] as the params argument (MonoArray
 * *params): in this case the value types are boxed inside the
 * respective reference representation.
 *
 * From unmanaged code you'll usually use the
 * mono_runtime_invoke_checked() variant.
 *
 * Note that this function doesn't handle virtual methods for
 * you, it will exec the exact method you pass: we still need to
 * expose a function to lookup the derived class implementation
 * of a virtual method (there are examples of this in the code,
 * though).
 *
 * On failure or exception, @error will be set. In that case, you
 * can't use the MonoObject* result from the function.
 *
 * If the method returns a value type, it is boxed in an object
 * reference.
 */
MonoObject*
mono_runtime_invoke_array_checked (MonoMethod *method, void *obj, MonoArray *params,
				   MonoError *error)
{
	error_init (error);
	return mono_runtime_try_invoke_array (method, obj, params, NULL, error);
}

/**
 * mono_runtime_try_invoke_array:
 * @method: method to invoke
 * @obJ: object instance
 * @params: arguments to the method
 * @exc: exception information.
 * @error: set on failure.
 *
 * Invokes the method represented by @method on the object @obj.
 *
 * obj is the 'this' pointer, it should be NULL for static
 * methods, a MonoObject* for object instances and a pointer to
 * the value type for value types.
 *
 * The params array contains the arguments to the method with the
 * same convention: MonoObject* pointers for object instances and
 * pointers to the value type otherwise. The _invoke_array
 * variant takes a C# object[] as the params argument (MonoArray
 * *params): in this case the value types are boxed inside the
 * respective reference representation.
 *
 * From unmanaged code you'll usually use the
 * mono_runtime_invoke_checked() variant.
 *
 * Note that this function doesn't handle virtual methods for
 * you, it will exec the exact method you pass: we still need to
 * expose a function to lookup the derived class implementation
 * of a virtual method (there are examples of this in the code,
 * though).
 *
 * You can pass NULL as the exc argument if you don't want to catch
 * exceptions, otherwise, *exc will be set to the exception thrown, if
 * any.  On other failures, @error will be set. If an exception is
 * thrown or there's an error, you can't use the MonoObject* result
 * from the function.
 *
 * If the method returns a value type, it is boxed in an object
 * reference.
 */
MonoObject*
mono_runtime_try_invoke_array (MonoMethod *method, void *obj, MonoArray *params,
			       MonoObject **exc, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	error_init (error);

	MonoMethodSignature *sig = mono_method_signature (method);
	gpointer *pa = NULL;
	MonoObject *res;
	int i;
	gboolean has_byref_nullables = FALSE;

	if (NULL != params) {
		pa = (void **)alloca (sizeof (gpointer) * mono_array_length (params));
		for (i = 0; i < mono_array_length (params); i++) {
			MonoType *t = sig->params [i];
			pa [i] = invoke_array_extract_argument (params, i, t, &has_byref_nullables, error);
			return_val_if_nok (error, NULL);
		}
	}

	if (!strcmp (method->name, ".ctor") && method->klass != mono_defaults.string_class) {
		void *o = obj;

		if (mono_class_is_nullable (method->klass)) {
			/* Need to create a boxed vtype instead */
			g_assert (!obj);

			if (!params)
				return NULL;
			else {
				return mono_value_box_checked (mono_domain_get (), method->klass->cast_class, pa [0], error);
			}
		}

		if (!obj) {
			obj = mono_object_new_checked (mono_domain_get (), method->klass, error);
			mono_error_assert_ok (error);
			g_assert (obj); /*maybe we should raise a TLE instead?*/
#ifndef DISABLE_REMOTING
			if (mono_object_is_transparent_proxy (obj)) {
				method = mono_marshal_get_remoting_invoke (method->slot == -1 ? method : method->klass->vtable [method->slot]);
			}
#endif
			if (method->klass->valuetype)
				o = (MonoObject *)mono_object_unbox ((MonoObject *)obj);
			else
				o = obj;
		} else if (method->klass->valuetype) {
			obj = mono_value_box_checked (mono_domain_get (), method->klass, obj, error);
			return_val_if_nok (error, NULL);
		}

		if (exc) {
			mono_runtime_try_invoke (method, o, pa, exc, error);
		} else {
			mono_runtime_invoke_checked (method, o, pa, error);
		}

		return (MonoObject *)obj;
	} else {
		if (mono_class_is_nullable (method->klass)) {
			MonoObject *nullable;

			/* Convert the unboxed vtype into a Nullable structure */
			nullable = mono_object_new_checked (mono_domain_get (), method->klass, error);
			return_val_if_nok (error, NULL);

			MonoObject *boxed = mono_value_box_checked (mono_domain_get (), method->klass->cast_class, obj, error);
			return_val_if_nok (error, NULL);
			mono_nullable_init ((guint8 *)mono_object_unbox (nullable), boxed, method->klass);
			obj = mono_object_unbox (nullable);
		}

		/* obj must be already unboxed if needed */
		if (exc) {
			res = mono_runtime_try_invoke (method, obj, pa, exc, error);
		} else {
			res = mono_runtime_invoke_checked (method, obj, pa, error);
		}
		return_val_if_nok (error, NULL);

		if (sig->ret->type == MONO_TYPE_PTR) {
			MonoClass *pointer_class;
			static MonoMethod *box_method;
			void *box_args [2];
			MonoObject *box_exc;

			/* 
			 * The runtime-invoke wrapper returns a boxed IntPtr, need to 
			 * convert it to a Pointer object.
			 */
			pointer_class = mono_class_get_pointer_class ();
			if (!box_method)
				box_method = mono_class_get_method_from_name (pointer_class, "Box", -1);

			g_assert (res->vtable->klass == mono_defaults.int_class);
			box_args [0] = ((MonoIntPtr*)res)->m_value;
			box_args [1] = mono_type_get_object_checked (mono_domain_get (), sig->ret, error);
			return_val_if_nok (error, NULL);

			res = mono_runtime_try_invoke (box_method, NULL, box_args, &box_exc, error);
			g_assert (box_exc == NULL);
			mono_error_assert_ok (error);
		}

		if (has_byref_nullables) {
			/* 
			 * The runtime invoke wrapper already converted byref nullables back,
			 * and stored them in pa, we just need to copy them back to the
			 * managed array.
			 */
			for (i = 0; i < mono_array_length (params); i++) {
				MonoType *t = sig->params [i];

				if (t->byref && t->type == MONO_TYPE_GENERICINST && mono_class_is_nullable (mono_class_from_mono_type (t)))
					mono_array_setref (params, i, pa [i]);
			}
		}

		return res;
	}
}

/**
 * mono_object_new:
 * @klass: the class of the object that we want to create
 *
 * Returns: a newly created object whose definition is
 * looked up using @klass.   This will not invoke any constructors, 
 * so the consumer of this routine has to invoke any constructors on
 * its own to initialize the object.
 * 
 * It returns NULL on failure.
 */
MonoObject *
mono_object_new (MonoDomain *domain, MonoClass *klass)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoError error;

	MonoObject * result = mono_object_new_checked (domain, klass, &error);

	mono_error_cleanup (&error);
	return result;
}

MonoObject *
ves_icall_object_new (MonoDomain *domain, MonoClass *klass)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoError error;

	MonoObject * result = mono_object_new_checked (domain, klass, &error);

	mono_error_set_pending_exception (&error);
	return result;
}

/**
 * mono_object_new_checked:
 * @klass: the class of the object that we want to create
 * @error: set on error
 *
 * Returns: a newly created object whose definition is
 * looked up using @klass.   This will not invoke any constructors,
 * so the consumer of this routine has to invoke any constructors on
 * its own to initialize the object.
 *
 * It returns NULL on failure and sets @error.
 */
MonoObject *
mono_object_new_checked (MonoDomain *domain, MonoClass *klass, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoVTable *vtable;

	vtable = mono_class_vtable (domain, klass);
	g_assert (vtable); /* FIXME don't swallow the error */

	MonoObject *o = mono_object_new_specific_checked (vtable, error);
	return o;
}

/**
 * mono_object_new_pinned:
 *
 *   Same as mono_object_new, but the returned object will be pinned.
 * For SGEN, these objects will only be freed at appdomain unload.
 */
MonoObject *
mono_object_new_pinned (MonoDomain *domain, MonoClass *klass, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoVTable *vtable;

	error_init (error);

	vtable = mono_class_vtable (domain, klass);
	g_assert (vtable); /* FIXME don't swallow the error */

	MonoObject *o = (MonoObject *)mono_gc_alloc_pinned_obj (vtable, mono_class_instance_size (klass));

	if (G_UNLIKELY (!o))
		mono_error_set_out_of_memory (error, "Could not allocate %i bytes", mono_class_instance_size (klass));
	else if (G_UNLIKELY (vtable->klass->has_finalize))
		mono_object_register_finalizer (o);

	return o;
}

/**
 * mono_object_new_specific:
 * @vtable: the vtable of the object that we want to create
 *
 * Returns: A newly created object with class and domain specified
 * by @vtable
 */
MonoObject *
mono_object_new_specific (MonoVTable *vtable)
{
	MonoError error;
	MonoObject *o = mono_object_new_specific_checked (vtable, &error);
	mono_error_cleanup (&error);

	return o;
}

MonoObject *
mono_object_new_specific_checked (MonoVTable *vtable, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoObject *o;

	error_init (error);

	/* check for is_com_object for COM Interop */
	if (mono_vtable_is_remote (vtable) || mono_class_is_com_object (vtable->klass))
	{
		gpointer pa [1];
		MonoMethod *im = vtable->domain->create_proxy_for_type_method;

		if (im == NULL) {
			MonoClass *klass = mono_class_get_activation_services_class ();

			if (!klass->inited)
				mono_class_init (klass);

			im = mono_class_get_method_from_name (klass, "CreateProxyForType", 1);
			if (!im) {
				mono_error_set_not_supported (error, "Linked away.");
				return NULL;
			}
			vtable->domain->create_proxy_for_type_method = im;
		}
	
		pa [0] = mono_type_get_object_checked (mono_domain_get (), &vtable->klass->byval_arg, error);
		if (!mono_error_ok (error))
			return NULL;

		o = mono_runtime_invoke_checked (im, NULL, pa, error);
		if (!mono_error_ok (error))
			return NULL;

		if (o != NULL)
			return o;
	}

	return mono_object_new_alloc_specific_checked (vtable, error);
}

MonoObject *
ves_icall_object_new_specific (MonoVTable *vtable)
{
	MonoError error;
	MonoObject *o = mono_object_new_specific_checked (vtable, &error);
	mono_error_set_pending_exception (&error);

	return o;
}

/**
 * mono_object_new_alloc_specific:
 * @vtable: virtual table for the object.
 *
 * This function allocates a new `MonoObject` with the type derived
 * from the @vtable information.   If the class of this object has a 
 * finalizer, then the object will be tracked for finalization.
 *
 * This method might raise an exception on errors.  Use the
 * `mono_object_new_fast_checked` method if you want to manually raise
 * the exception.
 *
 * Returns: the allocated object.   
 */
MonoObject *
mono_object_new_alloc_specific (MonoVTable *vtable)
{
	MonoError error;
	MonoObject *o = mono_object_new_alloc_specific_checked (vtable, &error);
	mono_error_cleanup (&error);

	return o;
}

/**
 * mono_object_new_alloc_specific_checked:
 * @vtable: virtual table for the object.
 * @error: holds the error return value.  
 *
 * This function allocates a new `MonoObject` with the type derived
 * from the @vtable information. If the class of this object has a 
 * finalizer, then the object will be tracked for finalization.
 *
 * If there is not enough memory, the @error parameter will be set
 * and will contain a user-visible message with the amount of bytes
 * that were requested.
 *
 * Returns: the allocated object, or NULL if there is not enough memory
 *
 */
MonoObject *
mono_object_new_alloc_specific_checked (MonoVTable *vtable, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoObject *o;

	error_init (error);

	o = (MonoObject *)mono_gc_alloc_obj (vtable, vtable->klass->instance_size);

	if (G_UNLIKELY (!o))
		mono_error_set_out_of_memory (error, "Could not allocate %i bytes", vtable->klass->instance_size);
	else if (G_UNLIKELY (vtable->klass->has_finalize))
		mono_object_register_finalizer (o);

	return o;
}

/**
 * mono_object_new_fast:
 * @vtable: virtual table for the object.
 *
 * This function allocates a new `MonoObject` with the type derived
 * from the @vtable information.   The returned object is not tracked
 * for finalization.   If your object implements a finalizer, you should
 * use `mono_object_new_alloc_specific` instead.
 *
 * This method might raise an exception on errors.  Use the
 * `mono_object_new_fast_checked` method if you want to manually raise
 * the exception.
 *
 * Returns: the allocated object.   
 */
MonoObject*
mono_object_new_fast (MonoVTable *vtable)
{
	MonoError error;
	MonoObject *o = mono_object_new_fast_checked (vtable, &error);
	mono_error_cleanup (&error);

	return o;
}

/**
 * mono_object_new_fast_checked:
 * @vtable: virtual table for the object.
 * @error: holds the error return value.
 *
 * This function allocates a new `MonoObject` with the type derived
 * from the @vtable information. The returned object is not tracked
 * for finalization.   If your object implements a finalizer, you should
 * use `mono_object_new_alloc_specific_checked` instead.
 *
 * If there is not enough memory, the @error parameter will be set
 * and will contain a user-visible message with the amount of bytes
 * that were requested.
 *
 * Returns: the allocated object, or NULL if there is not enough memory
 *
 */
MonoObject*
mono_object_new_fast_checked (MonoVTable *vtable, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoObject *o;

	error_init (error);

	o = mono_gc_alloc_obj (vtable, vtable->klass->instance_size);

	if (G_UNLIKELY (!o))
		mono_error_set_out_of_memory (error, "Could not allocate %i bytes", vtable->klass->instance_size);

	return o;
}

MonoObject *
ves_icall_object_new_fast (MonoVTable *vtable)
{
	MonoError error;
	MonoObject *o = mono_object_new_fast_checked (vtable, &error);
	mono_error_set_pending_exception (&error);

	return o;
}

MonoObject*
mono_object_new_mature (MonoVTable *vtable, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoObject *o;

	error_init (error);

	o = mono_gc_alloc_mature (vtable, vtable->klass->instance_size);

	if (G_UNLIKELY (!o))
		mono_error_set_out_of_memory (error, "Could not allocate %i bytes", vtable->klass->instance_size);
	else if (G_UNLIKELY (vtable->klass->has_finalize))
		mono_object_register_finalizer (o);

	return o;
}

/**
 * mono_class_get_allocation_ftn:
 * @vtable: vtable
 * @for_box: the object will be used for boxing
 * @pass_size_in_words: 
 *
 * Return the allocation function appropriate for the given class.
 */

void*
mono_class_get_allocation_ftn (MonoVTable *vtable, gboolean for_box, gboolean *pass_size_in_words)
{
	MONO_REQ_GC_NEUTRAL_MODE;

	*pass_size_in_words = FALSE;

	if (mono_class_has_finalizer (vtable->klass) || mono_class_is_marshalbyref (vtable->klass))
		return ves_icall_object_new_specific;

	if (vtable->gc_descr != MONO_GC_DESCRIPTOR_NULL) {

		return ves_icall_object_new_fast;

		/* 
		 * FIXME: This is actually slower than ves_icall_object_new_fast, because
		 * of the overhead of parameter passing.
		 */
		/*
		*pass_size_in_words = TRUE;
#ifdef GC_REDIRECT_TO_LOCAL
		return GC_local_gcj_fast_malloc;
#else
		return GC_gcj_fast_malloc;
#endif
		*/
	}

	return ves_icall_object_new_specific;
}

/**
 * mono_object_new_from_token:
 * @image: Context where the type_token is hosted
 * @token: a token of the type that we want to create
 *
 * Returns: A newly created object whose definition is
 * looked up using @token in the @image image
 */
MonoObject *
mono_object_new_from_token  (MonoDomain *domain, MonoImage *image, guint32 token)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoError error;
	MonoObject *result;
	MonoClass *klass;

	klass = mono_class_get_checked (image, token, &error);
	mono_error_assert_ok (&error);
	
	result = mono_object_new_checked (domain, klass, &error);

	mono_error_cleanup (&error);
	return result;
	
}


/**
 * mono_object_clone:
 * @obj: the object to clone
 *
 * Returns: A newly created object who is a shallow copy of @obj
 */
MonoObject *
mono_object_clone (MonoObject *obj)
{
	MonoError error;
	MonoObject *o = mono_object_clone_checked (obj, &error);
	mono_error_cleanup (&error);

	return o;
}

MonoObject *
mono_object_clone_checked (MonoObject *obj, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoObject *o;
	int size;

	error_init (error);

	size = obj->vtable->klass->instance_size;

	if (obj->vtable->klass->rank)
		return (MonoObject*)mono_array_clone_checked ((MonoArray*)obj, error);

	o = (MonoObject *)mono_gc_alloc_obj (obj->vtable, size);

	if (G_UNLIKELY (!o)) {
		mono_error_set_out_of_memory (error, "Could not allocate %i bytes", size);
		return NULL;
	}

	/* If the object doesn't contain references this will do a simple memmove. */
	mono_gc_wbarrier_object_copy (o, obj);

	if (obj->vtable->klass->has_finalize)
		mono_object_register_finalizer (o);
	return o;
}

/**
 * mono_array_full_copy:
 * @src: source array to copy
 * @dest: destination array
 *
 * Copies the content of one array to another with exactly the same type and size.
 */
void
mono_array_full_copy (MonoArray *src, MonoArray *dest)
{
	MONO_REQ_GC_UNSAFE_MODE;

	uintptr_t size;
	MonoClass *klass = src->obj.vtable->klass;

	g_assert (klass == dest->obj.vtable->klass);

	size = mono_array_length (src);
	g_assert (size == mono_array_length (dest));
	size *= mono_array_element_size (klass);

	array_full_copy_unchecked_size (src, dest, klass, size);
}

static void
array_full_copy_unchecked_size (MonoArray *src, MonoArray *dest, MonoClass *klass, uintptr_t size)
{
#ifdef HAVE_SGEN_GC
	if (klass->element_class->valuetype) {
		if (klass->element_class->has_references)
			mono_value_copy_array (dest, 0, mono_array_addr_with_size_fast (src, 0, 0), mono_array_length (src));
		else
			mono_gc_memmove_atomic (&dest->vector, &src->vector, size);
	} else {
		mono_array_memcpy_refs (dest, 0, src, 0, mono_array_length (src));
	}
#else
	mono_gc_memmove_atomic (&dest->vector, &src->vector, size);
#endif
}

/**
 * mono_array_clone_in_domain:
 * @domain: the domain in which the array will be cloned into
 * @array: the array to clone
 * @error: set on error
 *
 * This routine returns a copy of the array that is hosted on the
 * specified MonoDomain.  On failure returns NULL and sets @error.
 */
MonoArrayHandle
mono_array_clone_in_domain (MonoDomain *domain, MonoArrayHandle array_handle, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoArrayHandle result = MONO_HANDLE_NEW (MonoArray, NULL);
	uintptr_t size = 0;
	MonoClass *klass = mono_handle_class (array_handle);

	error_init (error);

	/* Pin source array here - if bounds is non-NULL, it's a pointer into the object data */
	uint32_t src_handle = mono_gchandle_from_handle (MONO_HANDLE_CAST (MonoObject, array_handle), TRUE);
	
	MonoArrayBounds *array_bounds = MONO_HANDLE_GETVAL (array_handle, bounds);
	MonoArrayHandle o;
	if (array_bounds == NULL) {
		size = mono_array_handle_length (array_handle);
		o = mono_array_new_full_handle (domain, klass, &size, NULL, error);
		if (!is_ok (error))
			goto leave;
		size *= mono_array_element_size (klass);
	} else {
		uintptr_t *sizes = (uintptr_t *)alloca (klass->rank * sizeof (uintptr_t));
		intptr_t *lower_bounds = (intptr_t *)alloca (klass->rank * sizeof (intptr_t));
		size = mono_array_element_size (klass);
		for (int i = 0; i < klass->rank; ++i) {
			sizes [i] = array_bounds [i].length;
			size *= array_bounds [i].length;
			lower_bounds [i] = array_bounds [i].lower_bound;
		}
		o = mono_array_new_full_handle (domain, klass, sizes, lower_bounds, error);
		if (!is_ok (error))
			goto leave;
	}

	uint32_t dst_handle = mono_gchandle_from_handle (MONO_HANDLE_CAST (MonoObject, o), TRUE);
	array_full_copy_unchecked_size (MONO_HANDLE_RAW (array_handle), MONO_HANDLE_RAW (o), klass, size);
	mono_gchandle_free (dst_handle);

	MONO_HANDLE_ASSIGN (result, o);

leave:
	mono_gchandle_free (src_handle);
	return result;
}

/**
 * mono_array_clone:
 * @array: the array to clone
 *
 * Returns: A newly created array who is a shallow copy of @array
 */
MonoArray*
mono_array_clone (MonoArray *array)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoError error;
	MonoArray *result = mono_array_clone_checked (array, &error);
	mono_error_cleanup (&error);
	return result;
}

/**
 * mono_array_clone_checked:
 * @array: the array to clone
 * @error: set on error
 *
 * Returns: A newly created array who is a shallow copy of @array.  On
 * failure returns NULL and sets @error.
 */
MonoArray*
mono_array_clone_checked (MonoArray *array_raw, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;
	HANDLE_FUNCTION_ENTER ();
	/* FIXME: callers of mono_array_clone_checked should use handles */
	error_init (error);
	MONO_HANDLE_DCL (MonoArray, array);
	MonoArrayHandle result = mono_array_clone_in_domain (MONO_HANDLE_DOMAIN (array), array, error);
	HANDLE_FUNCTION_RETURN_OBJ (result);
}

/* helper macros to check for overflow when calculating the size of arrays */
#ifdef MONO_BIG_ARRAYS
#define MYGUINT64_MAX 0x0000FFFFFFFFFFFFUL
#define MYGUINT_MAX MYGUINT64_MAX
#define CHECK_ADD_OVERFLOW_UN(a,b) \
	    (G_UNLIKELY ((guint64)(MYGUINT64_MAX) - (guint64)(b) < (guint64)(a)))
#define CHECK_MUL_OVERFLOW_UN(a,b) \
	    (G_UNLIKELY (((guint64)(a) > 0) && ((guint64)(b) > 0) &&	\
					 ((guint64)(b) > ((MYGUINT64_MAX) / (guint64)(a)))))
#else
#define MYGUINT32_MAX 4294967295U
#define MYGUINT_MAX MYGUINT32_MAX
#define CHECK_ADD_OVERFLOW_UN(a,b) \
	    (G_UNLIKELY ((guint32)(MYGUINT32_MAX) - (guint32)(b) < (guint32)(a)))
#define CHECK_MUL_OVERFLOW_UN(a,b) \
	    (G_UNLIKELY (((guint32)(a) > 0) && ((guint32)(b) > 0) &&			\
					 ((guint32)(b) > ((MYGUINT32_MAX) / (guint32)(a)))))
#endif

gboolean
mono_array_calc_byte_len (MonoClass *klass, uintptr_t len, uintptr_t *res)
{
	MONO_REQ_GC_NEUTRAL_MODE;

	uintptr_t byte_len;

	byte_len = mono_array_element_size (klass);
	if (CHECK_MUL_OVERFLOW_UN (byte_len, len))
		return FALSE;
	byte_len *= len;
	if (CHECK_ADD_OVERFLOW_UN (byte_len, MONO_SIZEOF_MONO_ARRAY))
		return FALSE;
	byte_len += MONO_SIZEOF_MONO_ARRAY;

	*res = byte_len;

	return TRUE;
}

/**
 * mono_array_new_full:
 * @domain: domain where the object is created
 * @array_class: array class
 * @lengths: lengths for each dimension in the array
 * @lower_bounds: lower bounds for each dimension in the array (may be NULL)
 *
 * This routine creates a new array objects with the given dimensions,
 * lower bounds and type.
 */
MonoArray*
mono_array_new_full (MonoDomain *domain, MonoClass *array_class, uintptr_t *lengths, intptr_t *lower_bounds)
{
	MonoError error;
	MonoArray *array = mono_array_new_full_checked (domain, array_class, lengths, lower_bounds, &error);
	mono_error_cleanup (&error);

	return array;
}

MonoArray*
mono_array_new_full_checked (MonoDomain *domain, MonoClass *array_class, uintptr_t *lengths, intptr_t *lower_bounds, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	uintptr_t byte_len = 0, len, bounds_size;
	MonoObject *o;
	MonoArray *array;
	MonoArrayBounds *bounds;
	MonoVTable *vtable;
	int i;

	error_init (error);

	if (!array_class->inited)
		mono_class_init (array_class);

	len = 1;

	/* A single dimensional array with a 0 lower bound is the same as an szarray */
	if (array_class->rank == 1 && ((array_class->byval_arg.type == MONO_TYPE_SZARRAY) || (lower_bounds && lower_bounds [0] == 0))) {
		len = lengths [0];
		if (len > MONO_ARRAY_MAX_INDEX) {
			mono_error_set_generic_error (error, "System", "OverflowException", "");
			return NULL;
		}
		bounds_size = 0;
	} else {
		bounds_size = sizeof (MonoArrayBounds) * array_class->rank;

		for (i = 0; i < array_class->rank; ++i) {
			if (lengths [i] > MONO_ARRAY_MAX_INDEX) {
				mono_error_set_generic_error (error, "System", "OverflowException", "");
				return NULL;
			}
			if (CHECK_MUL_OVERFLOW_UN (len, lengths [i])) {
				mono_error_set_out_of_memory (error, "Could not allocate %i bytes", MONO_ARRAY_MAX_SIZE);
				return NULL;
			}
			len *= lengths [i];
		}
	}

	if (!mono_array_calc_byte_len (array_class, len, &byte_len)) {
		mono_error_set_out_of_memory (error, "Could not allocate %i bytes", MONO_ARRAY_MAX_SIZE);
		return NULL;
	}

	if (bounds_size) {
		/* align */
		if (CHECK_ADD_OVERFLOW_UN (byte_len, 3)) {
			mono_error_set_out_of_memory (error, "Could not allocate %i bytes", MONO_ARRAY_MAX_SIZE);
			return NULL;
		}
		byte_len = (byte_len + 3) & ~3;
		if (CHECK_ADD_OVERFLOW_UN (byte_len, bounds_size)) {
			mono_error_set_out_of_memory (error, "Could not allocate %i bytes", MONO_ARRAY_MAX_SIZE);
			return NULL;
		}
		byte_len += bounds_size;
	}
	/* 
	 * Following three lines almost taken from mono_object_new ():
	 * they need to be kept in sync.
	 */
	vtable = mono_class_vtable_full (domain, array_class, error);
	return_val_if_nok (error, NULL);

	if (bounds_size)
		o = (MonoObject *)mono_gc_alloc_array (vtable, byte_len, len, bounds_size);
	else
		o = (MonoObject *)mono_gc_alloc_vector (vtable, byte_len, len);

	if (G_UNLIKELY (!o)) {
		mono_error_set_out_of_memory (error, "Could not allocate %zd bytes", (gsize) byte_len);
		return NULL;
	}

	array = (MonoArray*)o;

	bounds = array->bounds;

	if (bounds_size) {
		for (i = 0; i < array_class->rank; ++i) {
			bounds [i].length = lengths [i];
			if (lower_bounds)
				bounds [i].lower_bound = lower_bounds [i];
		}
	}

	return array;
}

/**
 * mono_array_new:
 * @domain: domain where the object is created
 * @eclass: element class
 * @n: number of array elements
 *
 * This routine creates a new szarray with @n elements of type @eclass.
 */
MonoArray *
mono_array_new (MonoDomain *domain, MonoClass *eclass, uintptr_t n)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoError error;
	MonoArray *result = mono_array_new_checked (domain, eclass, n, &error);
	mono_error_cleanup (&error);
	return result;
}

/**
 * mono_array_new_checked:
 * @domain: domain where the object is created
 * @eclass: element class
 * @n: number of array elements
 * @error: set on error
 *
 * This routine creates a new szarray with @n elements of type @eclass.
 * On failure returns NULL and sets @error.
 */
MonoArray *
mono_array_new_checked (MonoDomain *domain, MonoClass *eclass, uintptr_t n, MonoError *error)
{
	MonoClass *ac;

	error_init (error);

	ac = mono_array_class_get (eclass, 1);
	g_assert (ac);

	MonoVTable *vtable = mono_class_vtable_full (domain, ac, error);
	return_val_if_nok (error, NULL);

	return mono_array_new_specific_checked (vtable, n, error);
}

MonoArray*
ves_icall_array_new (MonoDomain *domain, MonoClass *eclass, uintptr_t n)
{
	MonoError error;
	MonoArray *arr = mono_array_new_checked (domain, eclass, n, &error);
	mono_error_set_pending_exception (&error);

	return arr;
}

/**
 * mono_array_new_specific:
 * @vtable: a vtable in the appropriate domain for an initialized class
 * @n: number of array elements
 *
 * This routine is a fast alternative to mono_array_new() for code which
 * can be sure about the domain it operates in.
 */
MonoArray *
mono_array_new_specific (MonoVTable *vtable, uintptr_t n)
{
	MonoError error;
	MonoArray *arr = mono_array_new_specific_checked (vtable, n, &error);
	mono_error_cleanup (&error);

	return arr;
}

MonoArray*
mono_array_new_specific_checked (MonoVTable *vtable, uintptr_t n, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoObject *o;
	uintptr_t byte_len;

	error_init (error);

	if (G_UNLIKELY (n > MONO_ARRAY_MAX_INDEX)) {
		mono_error_set_generic_error (error, "System", "OverflowException", "");
		return NULL;
	}

	if (!mono_array_calc_byte_len (vtable->klass, n, &byte_len)) {
		mono_error_set_out_of_memory (error, "Could not allocate %i bytes", MONO_ARRAY_MAX_SIZE);
		return NULL;
	}
	o = (MonoObject *)mono_gc_alloc_vector (vtable, byte_len, n);

	if (G_UNLIKELY (!o)) {
		mono_error_set_out_of_memory (error, "Could not allocate %zd bytes", (gsize) byte_len);
		return NULL;
	}

	return (MonoArray*)o;
}

MonoArray*
ves_icall_array_new_specific (MonoVTable *vtable, uintptr_t n)
{
	MonoError error;
	MonoArray *arr = mono_array_new_specific_checked (vtable, n, &error);
	mono_error_set_pending_exception (&error);

	return arr;
}

/**
 * mono_string_empty_wrapper:
 *
 * Returns: The same empty string instance as the managed string.Empty
 */
MonoString*
mono_string_empty_wrapper (void)
{
	MonoDomain *domain = mono_domain_get ();
	return mono_string_empty (domain);
}

/**
 * mono_string_empty:
 *
 * Returns: The same empty string instance as the managed string.Empty
 */
MonoString*
mono_string_empty (MonoDomain *domain)
{
	g_assert (domain);
	g_assert (domain->empty_string);
	return domain->empty_string;
}

/**
 * mono_string_new_utf16:
 * @text: a pointer to an utf16 string
 * @len: the length of the string
 *
 * Returns: A newly created string object which contains @text.
 */
MonoString *
mono_string_new_utf16 (MonoDomain *domain, const guint16 *text, gint32 len)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoError error;
	MonoString *res = NULL;
	res = mono_string_new_utf16_checked (domain, text, len, &error);
	mono_error_cleanup (&error);

	return res;
}

/**
 * mono_string_new_utf16_checked:
 * @text: a pointer to an utf16 string
 * @len: the length of the string
 * @error: written on error.
 *
 * Returns: A newly created string object which contains @text.
 * On error, returns NULL and sets @error.
 */
MonoString *
mono_string_new_utf16_checked (MonoDomain *domain, const guint16 *text, gint32 len, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoString *s;
	
	error_init (error);
	
	s = mono_string_new_size_checked (domain, len, error);
	if (s != NULL)
		memcpy (mono_string_chars (s), text, len * 2);

	return s;
}

/**
 * mono_string_new_utf16_handle:
 * @text: a pointer to an utf16 string
 * @len: the length of the string
 * @error: written on error.
 *
 * Returns: A newly created string object which contains @text.
 * On error, returns NULL and sets @error.
 */
MonoStringHandle
mono_string_new_utf16_handle (MonoDomain *domain, const guint16 *text, gint32 len, MonoError *error)
{
	return MONO_HANDLE_NEW (MonoString, mono_string_new_utf16_checked (domain, text, len, error));
}

/**
 * mono_string_new_utf32:
 * @text: a pointer to an utf32 string
 * @len: the length of the string
 * @error: set on failure.
 *
 * Returns: A newly created string object which contains @text. On failure returns NULL and sets @error.
 */
static MonoString *
mono_string_new_utf32_checked (MonoDomain *domain, const mono_unichar4 *text, gint32 len, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoString *s;
	mono_unichar2 *utf16_output = NULL;
	gint32 utf16_len = 0;
	GError *gerror = NULL;
	glong items_written;
	
	error_init (error);
	utf16_output = g_ucs4_to_utf16 (text, len, NULL, &items_written, &gerror);
	
	if (gerror)
		g_error_free (gerror);

	while (utf16_output [utf16_len]) utf16_len++;
	
	s = mono_string_new_size_checked (domain, utf16_len, error);
	return_val_if_nok (error, NULL);

	memcpy (mono_string_chars (s), utf16_output, utf16_len * 2);

	g_free (utf16_output);
	
	return s;
}

/**
 * mono_string_new_utf32:
 * @text: a pointer to an utf32 string
 * @len: the length of the string
 *
 * Returns: A newly created string object which contains @text.
 */
MonoString *
mono_string_new_utf32 (MonoDomain *domain, const mono_unichar4 *text, gint32 len)
{
	MonoError error;
	MonoString *result = mono_string_new_utf32_checked (domain, text, len, &error);
	mono_error_cleanup (&error);
	return result;
}

/**
 * mono_string_new_size:
 * @text: a pointer to an utf16 string
 * @len: the length of the string
 *
 * Returns: A newly created string object of @len
 */
MonoString *
mono_string_new_size (MonoDomain *domain, gint32 len)
{
	MonoError error;
	MonoString *str = mono_string_new_size_checked (domain, len, &error);
	mono_error_cleanup (&error);

	return str;
}

MonoString *
mono_string_new_size_checked (MonoDomain *domain, gint32 len, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoString *s;
	MonoVTable *vtable;
	size_t size;

	error_init (error);

	/* check for overflow */
	if (len < 0 || len > ((SIZE_MAX - G_STRUCT_OFFSET (MonoString, chars) - 8) / 2)) {
		mono_error_set_out_of_memory (error, "Could not allocate %i bytes", -1);
		return NULL;
	}

	size = (G_STRUCT_OFFSET (MonoString, chars) + (((size_t)len + 1) * 2));
	g_assert (size > 0);

	vtable = mono_class_vtable (domain, mono_defaults.string_class);
	g_assert (vtable);

	s = (MonoString *)mono_gc_alloc_string (vtable, size, len);

	if (G_UNLIKELY (!s)) {
		mono_error_set_out_of_memory (error, "Could not allocate %zd bytes", size);
		return NULL;
	}

	return s;
}

/**
 * mono_string_new_len:
 * @text: a pointer to an utf8 string
 * @length: number of bytes in @text to consider
 *
 * Returns: A newly created string object which contains @text.
 */
MonoString*
mono_string_new_len (MonoDomain *domain, const char *text, guint length)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoError error;
	MonoString *result = mono_string_new_len_checked (domain, text, length, &error);
	mono_error_cleanup (&error);
	return result;
}

/**
 * mono_string_new_len_checked:
 * @text: a pointer to an utf8 string
 * @length: number of bytes in @text to consider
 * @error: set on error
 *
 * Returns: A newly created string object which contains @text. On
 * failure returns NULL and sets @error.
 */
MonoString*
mono_string_new_len_checked (MonoDomain *domain, const char *text, guint length, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	error_init (error);

	GError *eg_error = NULL;
	MonoString *o = NULL;
	guint16 *ut = NULL;
	glong items_written;

	ut = eg_utf8_to_utf16_with_nuls (text, length, NULL, &items_written, &eg_error);

	if (!eg_error)
		o = mono_string_new_utf16_checked (domain, ut, items_written, error);
	else 
		g_error_free (eg_error);

	g_free (ut);

	return o;
}

/**
 * mono_string_new:
 * @text: a pointer to an utf8 string
 *
 * Returns: A newly created string object which contains @text.
 *
 * This function asserts if it cannot allocate a new string.
 *
 * @deprecated Use mono_string_new_checked in new code.
 */
MonoString*
mono_string_new (MonoDomain *domain, const char *text)
{
	MonoError error;
	MonoString *res = NULL;
	res = mono_string_new_checked (domain, text, &error);
	mono_error_assert_ok (&error);
	return res;
}

/**
 * mono_string_new_checked:
 * @text: a pointer to an utf8 string
 * @merror: set on error
 *
 * Returns: A newly created string object which contains @text.
 * On error returns NULL and sets @merror.
 */
MonoString*
mono_string_new_checked (MonoDomain *domain, const char *text, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

    GError *eg_error = NULL;
    MonoString *o = NULL;
    guint16 *ut;
    glong items_written;
    int l;

    error_init (error);

    l = strlen (text);
   
    ut = g_utf8_to_utf16 (text, l, NULL, &items_written, &eg_error);

    if (!eg_error)
	    o = mono_string_new_utf16_checked (domain, ut, items_written, error);
    else
        g_error_free (eg_error);

    g_free (ut);
    
/*FIXME g_utf8_get_char, g_utf8_next_char and g_utf8_validate are not part of eglib.*/
#if 0
	gunichar2 *str;
	const gchar *end;
	int len;
	MonoString *o = NULL;

	if (!g_utf8_validate (text, -1, &end)) {
		mono_error_set_argument (error, "text", "Not a valid utf8 string");
		goto leave;
	}

	len = g_utf8_strlen (text, -1);
	o = mono_string_new_size_checked (domain, len, error);
	if (!o)
		goto leave;
	str = mono_string_chars (o);

	while (text < end) {
		*str++ = g_utf8_get_char (text);
		text = g_utf8_next_char (text);
	}

leave:
#endif
	return o;
}

/**
 * mono_string_new_wrapper:
 * @text: pointer to utf8 characters.
 *
 * Helper function to create a string object from @text in the current domain.
 */
MonoString*
mono_string_new_wrapper (const char *text)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoDomain *domain = mono_domain_get ();

	if (text)
		return mono_string_new (domain, text);

	return NULL;
}

/**
 * mono_value_box:
 * @class: the class of the value
 * @value: a pointer to the unboxed data
 *
 * Returns: A newly created object which contains @value.
 */
MonoObject *
mono_value_box (MonoDomain *domain, MonoClass *klass, gpointer value)
{
	MonoError error;
	MonoObject *result = mono_value_box_checked (domain, klass, value, &error);
	mono_error_cleanup (&error);
	return result;
}

/**
 * mono_value_box_checked:
 * @domain: the domain of the new object
 * @class: the class of the value
 * @value: a pointer to the unboxed data
 * @error: set on error
 *
 * Returns: A newly created object which contains @value. On failure
 * returns NULL and sets @error.
 */
MonoObject *
mono_value_box_checked (MonoDomain *domain, MonoClass *klass, gpointer value, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;
	MonoObject *res;
	int size;
	MonoVTable *vtable;

	error_init (error);

	g_assert (klass->valuetype);
	if (mono_class_is_nullable (klass))
		return mono_nullable_box ((guint8 *)value, klass, error);

	vtable = mono_class_vtable (domain, klass);
	if (!vtable)
		return NULL;
	size = mono_class_instance_size (klass);
	res = mono_object_new_alloc_specific_checked (vtable, error);
	return_val_if_nok (error, NULL);

	size = size - sizeof (MonoObject);

#ifdef HAVE_SGEN_GC
	g_assert (size == mono_class_value_size (klass, NULL));
	mono_gc_wbarrier_value_copy ((char *)res + sizeof (MonoObject), value, 1, klass);
#else
#if NO_UNALIGNED_ACCESS
	mono_gc_memmove_atomic ((char *)res + sizeof (MonoObject), value, size);
#else
	switch (size) {
	case 1:
		*((guint8 *) res + sizeof (MonoObject)) = *(guint8 *) value;
		break;
	case 2:
		*(guint16 *)((guint8 *) res + sizeof (MonoObject)) = *(guint16 *) value;
		break;
	case 4:
		*(guint32 *)((guint8 *) res + sizeof (MonoObject)) = *(guint32 *) value;
		break;
	case 8:
		*(guint64 *)((guint8 *) res + sizeof (MonoObject)) = *(guint64 *) value;
		break;
	default:
		mono_gc_memmove_atomic ((char *)res + sizeof (MonoObject), value, size);
	}
#endif
#endif
	if (klass->has_finalize) {
		mono_object_register_finalizer (res);
		return_val_if_nok (error, NULL);
	}
	return res;
}

/**
 * mono_value_copy:
 * @dest: destination pointer
 * @src: source pointer
 * @klass: a valuetype class
 *
 * Copy a valuetype from @src to @dest. This function must be used
 * when @klass contains references fields.
 */
void
mono_value_copy (gpointer dest, gpointer src, MonoClass *klass)
{
	MONO_REQ_GC_UNSAFE_MODE;

	mono_gc_wbarrier_value_copy (dest, src, 1, klass);
}

/**
 * mono_value_copy_array:
 * @dest: destination array
 * @dest_idx: index in the @dest array
 * @src: source pointer
 * @count: number of items
 *
 * Copy @count valuetype items from @src to the array @dest at index @dest_idx. 
 * This function must be used when @klass contains references fields.
 * Overlap is handled.
 */
void
mono_value_copy_array (MonoArray *dest, int dest_idx, gpointer src, int count)
{
	MONO_REQ_GC_UNSAFE_MODE;

	int size = mono_array_element_size (dest->obj.vtable->klass);
	char *d = mono_array_addr_with_size_fast (dest, size, dest_idx);
	g_assert (size == mono_class_value_size (mono_object_class (dest)->element_class, NULL));
	mono_gc_wbarrier_value_copy (d, src, count, mono_object_class (dest)->element_class);
}

/**
 * mono_object_get_domain:
 * @obj: object to query
 * 
 * Returns: the MonoDomain where the object is hosted
 */
MonoDomain*
mono_object_get_domain (MonoObject *obj)
{
	MONO_REQ_GC_UNSAFE_MODE;

	return mono_object_domain (obj);
}

/**
 * mono_object_get_class:
 * @obj: object to query
 *
 * Use this function to obtain the `MonoClass*` for a given `MonoObject`.
 *
 * Returns: the MonoClass of the object.
 */
MonoClass*
mono_object_get_class (MonoObject *obj)
{
	MONO_REQ_GC_UNSAFE_MODE;

	return mono_object_class (obj);
}
/**
 * mono_object_get_size:
 * @o: object to query
 * 
 * Returns: the size, in bytes, of @o
 */
guint
mono_object_get_size (MonoObject* o)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoClass* klass = mono_object_class (o);
	if (klass == mono_defaults.string_class) {
		return sizeof (MonoString) + 2 * mono_string_length ((MonoString*) o) + 2;
	} else if (o->vtable->rank) {
		MonoArray *array = (MonoArray*)o;
		size_t size = MONO_SIZEOF_MONO_ARRAY + mono_array_element_size (klass) * mono_array_length (array);
		if (array->bounds) {
			size += 3;
			size &= ~3;
			size += sizeof (MonoArrayBounds) * o->vtable->rank;
		}
		return size;
	} else {
		return mono_class_instance_size (klass);
	}
}

/**
 * mono_object_unbox:
 * @obj: object to unbox
 * 
 * Returns: a pointer to the start of the valuetype boxed in this
 * object.
 *
 * This method will assert if the object passed is not a valuetype.
 */
gpointer
mono_object_unbox (MonoObject *obj)
{
	MONO_REQ_GC_UNSAFE_MODE;

	/* add assert for valuetypes? */
	g_assert (obj->vtable->klass->valuetype);
	return ((char*)obj) + sizeof (MonoObject);
}

/**
 * mono_object_isinst:
 * @obj: an object
 * @klass: a pointer to a class 
 *
 * Returns: @obj if @obj is derived from @klass or NULL otherwise.
 */
MonoObject *
mono_object_isinst (MonoObject *obj_raw, MonoClass *klass)
{
	MONO_REQ_GC_UNSAFE_MODE;

	HANDLE_FUNCTION_ENTER ();
	MONO_HANDLE_DCL (MonoObject, obj);
	MonoError error;
	MonoObjectHandle result = mono_object_handle_isinst (obj, klass, &error);
	mono_error_cleanup (&error);
	HANDLE_FUNCTION_RETURN_OBJ (result);
}
	

/**
 * mono_object_isinst_checked:
 * @obj: an object
 * @klass: a pointer to a class 
 * @error: set on error
 *
 * Returns: @obj if @obj is derived from @klass or NULL if it isn't.
 * On failure returns NULL and sets @error.
 */
MonoObject *
mono_object_isinst_checked (MonoObject *obj_raw, MonoClass *klass, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	HANDLE_FUNCTION_ENTER ();
	error_init (error);
	MONO_HANDLE_DCL (MonoObject, obj);
	MonoObjectHandle result = mono_object_handle_isinst (obj, klass, error);
	HANDLE_FUNCTION_RETURN_OBJ (result);
}

/**
 * mono_object_handle_isinst:
 * @obj: an object
 * @klass: a pointer to a class 
 * @error: set on error
 *
 * Returns: @obj if @obj is derived from @klass or NULL if it isn't.
 * On failure returns NULL and sets @error.
 */
MonoObjectHandle
mono_object_handle_isinst (MonoObjectHandle obj, MonoClass *klass, MonoError *error)
{
	error_init (error);
	
	if (!klass->inited)
		mono_class_init (klass);

	if (mono_class_is_marshalbyref (klass) || mono_class_is_interface (klass)) {
		return mono_object_handle_isinst_mbyref (obj, klass, error);
	}

	MonoObjectHandle result = MONO_HANDLE_NEW (MonoObject, NULL);

	if (!MONO_HANDLE_IS_NULL (obj) && mono_class_is_assignable_from (klass, mono_handle_class (obj)))
		MONO_HANDLE_ASSIGN (result, obj);
	return result;
}

MonoObject *
mono_object_isinst_mbyref (MonoObject *obj_raw, MonoClass *klass)
{
	MONO_REQ_GC_UNSAFE_MODE;

	HANDLE_FUNCTION_ENTER ();
	MonoError error;
	MONO_HANDLE_DCL (MonoObject, obj);
	MonoObjectHandle result = mono_object_handle_isinst_mbyref (obj, klass, &error);
	mono_error_cleanup (&error); /* FIXME better API that doesn't swallow the error */
	HANDLE_FUNCTION_RETURN_OBJ (result);
}

MonoObjectHandle
mono_object_handle_isinst_mbyref (MonoObjectHandle obj, MonoClass *klass, MonoError *error)
{
	error_init (error);

	MonoObjectHandle result = MONO_HANDLE_NEW (MonoObject, NULL);

	if (MONO_HANDLE_IS_NULL (obj))
		goto leave;

	MonoVTable *vt = MONO_HANDLE_GETVAL (obj, vtable);
	
	if (mono_class_is_interface (klass)) {
		if (MONO_VTABLE_IMPLEMENTS_INTERFACE (vt, klass->interface_id)) {
			MONO_HANDLE_ASSIGN (result, obj);
			goto leave;
		}

		/* casting an array one of the invariant interfaces that must act as such */
		if (klass->is_array_special_interface) {
			if (mono_class_is_assignable_from (klass, vt->klass)) {
				MONO_HANDLE_ASSIGN (result, obj);
				goto leave;
			}
		}

		/*If the above check fails we are in the slow path of possibly raising an exception. So it's ok to it this way.*/
		else if (mono_class_has_variant_generic_params (klass) && mono_class_is_assignable_from (klass, mono_handle_class (obj))) {
			MONO_HANDLE_ASSIGN (result, obj);
			goto leave;
		}
	} else {
		MonoClass *oklass = vt->klass;
		if (mono_class_is_transparent_proxy (oklass)){
			MonoRemoteClass *remote_class = MONO_HANDLE_GETVAL (MONO_HANDLE_CAST (MonoTransparentProxy, obj), remote_class);
			oklass = remote_class->proxy_class;
		}

		mono_class_setup_supertypes (klass);	
		if ((oklass->idepth >= klass->idepth) && (oklass->supertypes [klass->idepth - 1] == klass)) {
			MONO_HANDLE_ASSIGN (result, obj);
			goto leave;
		}
	}
#ifndef DISABLE_REMOTING
	if (mono_class_is_transparent_proxy (vt->klass)) 
	{
		MonoBoolean custom_type_info =  MONO_HANDLE_GETVAL (MONO_HANDLE_CAST (MonoTransparentProxy, obj), custom_type_info);
		if (!custom_type_info)
			goto leave;
		MonoDomain *domain = mono_domain_get ();
		MonoObjectHandle rp = MONO_HANDLE_NEW (MonoObject, NULL);
		MONO_HANDLE_GET (rp, MONO_HANDLE_CAST (MonoTransparentProxy, obj), rp);
		MonoClass *rpklass = mono_defaults.iremotingtypeinfo_class;
		MonoMethod *im = NULL;
		gpointer pa [2];

		im = mono_class_get_method_from_name (rpklass, "CanCastTo", -1);
		if (!im) {
			mono_error_set_not_supported (error, "Linked away.");
			goto leave;
		}
		im = mono_object_handle_get_virtual_method (rp, im, error);
		if (!is_ok (error))
			goto leave;
		g_assert (im);
	
		MonoReflectionTypeHandle reftype = mono_type_get_object_handle (domain, &klass->byval_arg, error);
		if (!is_ok (error))
			goto leave;

		pa [0] = MONO_HANDLE_RAW (reftype);
		pa [1] = MONO_HANDLE_RAW (obj);
		MonoObject *res = mono_runtime_invoke_checked (im, rp, pa, error);
		if (!is_ok (error))
			goto leave;

		if (*(MonoBoolean *) mono_object_unbox(res)) {
			/* Update the vtable of the remote type, so it can safely cast to this new type */
			mono_upgrade_remote_class (domain, obj, klass, error);
			if (!is_ok (error))
				goto leave;
			MONO_HANDLE_ASSIGN (result, obj);
		}
	}
#endif /* DISABLE_REMOTING */
leave:
	return result;
}

/**
 * mono_object_castclass_mbyref:
 * @obj: an object
 * @klass: a pointer to a class 
 *
 * Returns: @obj if @obj is derived from @klass, returns NULL otherwise.
 */
MonoObject *
mono_object_castclass_mbyref (MonoObject *obj_raw, MonoClass *klass)
{
	MONO_REQ_GC_UNSAFE_MODE;
	HANDLE_FUNCTION_ENTER ();
	MonoError error;
	MONO_HANDLE_DCL (MonoObject, obj);
	MonoObjectHandle result = MONO_HANDLE_NEW (MonoObject, NULL);
	if (MONO_HANDLE_IS_NULL (obj))
		goto leave;
	MONO_HANDLE_ASSIGN (result, mono_object_handle_isinst_mbyref (obj, klass, &error));
	mono_error_cleanup (&error);
leave:
	HANDLE_FUNCTION_RETURN_OBJ (result);
}

typedef struct {
	MonoDomain *orig_domain;
	MonoString *ins;
	MonoString *res;
} LDStrInfo;

static void
str_lookup (MonoDomain *domain, gpointer user_data)
{
	MONO_REQ_GC_UNSAFE_MODE;

	LDStrInfo *info = (LDStrInfo *)user_data;
	if (info->res || domain == info->orig_domain)
		return;
	info->res = (MonoString *)mono_g_hash_table_lookup (domain->ldstr_table, info->ins);
}

static MonoString*
mono_string_get_pinned (MonoString *str, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	error_init (error);

	/* We only need to make a pinned version of a string if this is a moving GC */
	if (!mono_gc_is_moving ())
		return str;
	int size;
	MonoString *news;
	size = sizeof (MonoString) + 2 * (mono_string_length (str) + 1);
	news = (MonoString *)mono_gc_alloc_pinned_obj (((MonoObject*)str)->vtable, size);
	if (news) {
		memcpy (mono_string_chars (news), mono_string_chars (str), mono_string_length (str) * 2);
		news->length = mono_string_length (str);
	} else {
		mono_error_set_out_of_memory (error, "Could not allocate %i bytes", size);
	}
	return news;
}

static MonoString*
mono_string_is_interned_lookup (MonoString *str, int insert, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoGHashTable *ldstr_table;
	MonoString *s, *res;
	MonoDomain *domain;
	
	error_init (error);

	domain = ((MonoObject *)str)->vtable->domain;
	ldstr_table = domain->ldstr_table;
	ldstr_lock ();
	res = (MonoString *)mono_g_hash_table_lookup (ldstr_table, str);
	if (res) {
		ldstr_unlock ();
		return res;
	}
	if (insert) {
		/* Allocate outside the lock */
		ldstr_unlock ();
		s = mono_string_get_pinned (str, error);
		return_val_if_nok (error, NULL);
		if (s) {
			ldstr_lock ();
			res = (MonoString *)mono_g_hash_table_lookup (ldstr_table, str);
			if (res) {
				ldstr_unlock ();
				return res;
			}
			mono_g_hash_table_insert (ldstr_table, s, s);
			ldstr_unlock ();
		}
		return s;
	} else {
		LDStrInfo ldstr_info;
		ldstr_info.orig_domain = domain;
		ldstr_info.ins = str;
		ldstr_info.res = NULL;

		mono_domain_foreach (str_lookup, &ldstr_info);
		if (ldstr_info.res) {
			/* 
			 * the string was already interned in some other domain:
			 * intern it in the current one as well.
			 */
			mono_g_hash_table_insert (ldstr_table, str, str);
			ldstr_unlock ();
			return str;
		}
	}
	ldstr_unlock ();
	return NULL;
}

/**
 * mono_string_is_interned:
 * @o: String to probe
 *
 * Returns whether the string has been interned.
 */
MonoString*
mono_string_is_interned (MonoString *o)
{
	MonoError error;
	MonoString *result = mono_string_is_interned_lookup (o, FALSE, &error);
	/* This function does not fail. */
	mono_error_assert_ok (&error);
	return result;
}

/**
 * mono_string_intern:
 * @o: String to intern
 *
 * Interns the string passed.  
 * Returns: The interned string.
 */
MonoString*
mono_string_intern (MonoString *str)
{
	MonoError error;
	MonoString *result = mono_string_intern_checked (str, &error);
	mono_error_assert_ok (&error);
	return result;
}

/**
 * mono_string_intern_checked:
 * @o: String to intern
 * @error: set on error.
 *
 * Interns the string passed.
 * Returns: The interned string.  On failure returns NULL and sets @error
 */
MonoString*
mono_string_intern_checked (MonoString *str, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	error_init (error);

	return mono_string_is_interned_lookup (str, TRUE, error);
}

/**
 * mono_ldstr:
 * @domain: the domain where the string will be used.
 * @image: a metadata context
 * @idx: index into the user string table.
 * 
 * Implementation for the ldstr opcode.
 * Returns: a loaded string from the @image/@idx combination.
 */
MonoString*
mono_ldstr (MonoDomain *domain, MonoImage *image, guint32 idx)
{
	MonoError error;
	MonoString *result = mono_ldstr_checked (domain, image, idx, &error);
	mono_error_cleanup (&error);
	return result;
}

/**
 * mono_ldstr_checked:
 * @domain: the domain where the string will be used.
 * @image: a metadata context
 * @idx: index into the user string table.
 * @error: set on error.
 * 
 * Implementation for the ldstr opcode.
 * Returns: a loaded string from the @image/@idx combination.
 * On failure returns NULL and sets @error.
 */
MonoString*
mono_ldstr_checked (MonoDomain *domain, MonoImage *image, guint32 idx, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;
	error_init (error);

	if (image->dynamic) {
		MonoString *str = (MonoString *)mono_lookup_dynamic_token (image, MONO_TOKEN_STRING | idx, NULL, error);
		return str;
	} else {
		if (!mono_verifier_verify_string_signature (image, idx, NULL))
			return NULL; /*FIXME we should probably be raising an exception here*/
		MonoString *str = mono_ldstr_metadata_sig (domain, mono_metadata_user_string (image, idx), error);
		return str;
	}
}

/**
 * mono_ldstr_metadata_sig
 * @domain: the domain for the string
 * @sig: the signature of a metadata string
 * @error: set on error
 *
 * Returns: a MonoString for a string stored in the metadata. On
 * failure returns NULL and sets @error.
 */
static MonoString*
mono_ldstr_metadata_sig (MonoDomain *domain, const char* sig, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	error_init (error);
	const char *str = sig;
	MonoString *o, *interned;
	size_t len2;

	len2 = mono_metadata_decode_blob_size (str, &str);
	len2 >>= 1;

	o = mono_string_new_utf16_checked (domain, (guint16*)str, len2, error);
	return_val_if_nok (error, NULL);
#if G_BYTE_ORDER != G_LITTLE_ENDIAN
	{
		int i;
		guint16 *p2 = (guint16*)mono_string_chars (o);
		for (i = 0; i < len2; ++i) {
			*p2 = GUINT16_FROM_LE (*p2);
			++p2;
		}
	}
#endif
	ldstr_lock ();
	interned = (MonoString *)mono_g_hash_table_lookup (domain->ldstr_table, o);
	ldstr_unlock ();
	if (interned)
		return interned; /* o will get garbage collected */

	o = mono_string_get_pinned (o, error);
	if (o) {
		ldstr_lock ();
		interned = (MonoString *)mono_g_hash_table_lookup (domain->ldstr_table, o);
		if (!interned) {
			mono_g_hash_table_insert (domain->ldstr_table, o, o);
			interned = o;
		}
		ldstr_unlock ();
	}

	return interned;
}

/*
 * mono_ldstr_utf8:
 *
 *   Same as mono_ldstr, but return a NULL terminated utf8 string instead
 * of an object.
 */
char*
mono_ldstr_utf8 (MonoImage *image, guint32 idx, MonoError *error)
{
	const char *str;
	size_t len2;
	long written = 0;
	char *as;
	GError *gerror = NULL;

	error_init (error);

	if (!mono_verifier_verify_string_signature (image, idx, NULL))
		return NULL; /*FIXME we should probably be raising an exception here*/
	str = mono_metadata_user_string (image, idx);

	len2 = mono_metadata_decode_blob_size (str, &str);
	len2 >>= 1;

	as = g_utf16_to_utf8 ((guint16*)str, len2, NULL, &written, &gerror);
	if (gerror) {
		mono_error_set_argument (error, "string", "%s", gerror->message);
		g_error_free (gerror);
		return NULL;
	}
	/* g_utf16_to_utf8  may not be able to complete the convertion (e.g. NULL values were found, #335488) */
	if (len2 > written) {
		/* allocate the total length and copy the part of the string that has been converted */
		char *as2 = (char *)g_malloc0 (len2);
		memcpy (as2, as, written);
		g_free (as);
		as = as2;
	}

	return as;
}

/**
 * mono_string_to_utf8:
 * @s: a System.String
 *
 * Returns the UTF8 representation for @s.
 * The resulting buffer needs to be freed with mono_free().
 *
 * @deprecated Use mono_string_to_utf8_checked to avoid having an exception arbritraly raised.
 */
char *
mono_string_to_utf8 (MonoString *s)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoError error;
	char *result = mono_string_to_utf8_checked (s, &error);
	
	if (!is_ok (&error)) {
		mono_error_cleanup (&error);
		return NULL;
	}
	return result;
}

/**
 * mono_string_to_utf8_checked:
 * @s: a System.String
 * @error: a MonoError.
 * 
 * Converts a MonoString to its UTF8 representation. May fail; check 
 * @error to determine whether the conversion was successful.
 * The resulting buffer should be freed with mono_free().
 */
char *
mono_string_to_utf8_checked (MonoString *s, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	long written = 0;
	char *as;
	GError *gerror = NULL;

	error_init (error);

	if (s == NULL)
		return NULL;

	if (!s->length)
		return g_strdup ("");

	as = g_utf16_to_utf8 (mono_string_chars (s), s->length, NULL, &written, &gerror);
	if (gerror) {
		mono_error_set_argument (error, "string", "%s", gerror->message);
		g_error_free (gerror);
		return NULL;
	}
	/* g_utf16_to_utf8  may not be able to complete the convertion (e.g. NULL values were found, #335488) */
	if (s->length > written) {
		/* allocate the total length and copy the part of the string that has been converted */
		char *as2 = (char *)g_malloc0 (s->length);
		memcpy (as2, as, written);
		g_free (as);
		as = as2;
	}

	return as;
}

char *
mono_string_handle_to_utf8 (MonoStringHandle s, MonoError *error)
{
	return mono_string_to_utf8_checked (MONO_HANDLE_RAW (s), error);
}

/**
 * mono_string_to_utf8_ignore:
 * @s: a MonoString
 *
 * Converts a MonoString to its UTF8 representation. Will ignore
 * invalid surrogate pairs.
 * The resulting buffer should be freed with mono_free().
 * 
 */
char *
mono_string_to_utf8_ignore (MonoString *s)
{
	MONO_REQ_GC_UNSAFE_MODE;

	long written = 0;
	char *as;

	if (s == NULL)
		return NULL;

	if (!s->length)
		return g_strdup ("");

	as = g_utf16_to_utf8 (mono_string_chars (s), s->length, NULL, &written, NULL);

	/* g_utf16_to_utf8  may not be able to complete the convertion (e.g. NULL values were found, #335488) */
	if (s->length > written) {
		/* allocate the total length and copy the part of the string that has been converted */
		char *as2 = (char *)g_malloc0 (s->length);
		memcpy (as2, as, written);
		g_free (as);
		as = as2;
	}

	return as;
}

/**
 * mono_string_to_utf8_image_ignore:
 * @s: a System.String
 *
 * Same as mono_string_to_utf8_ignore, but allocate the string from the image mempool.
 */
char *
mono_string_to_utf8_image_ignore (MonoImage *image, MonoString *s)
{
	MONO_REQ_GC_UNSAFE_MODE;

	return mono_string_to_utf8_internal (NULL, image, s, TRUE, NULL);
}

/**
 * mono_string_to_utf8_mp_ignore:
 * @s: a System.String
 *
 * Same as mono_string_to_utf8_ignore, but allocate the string from a mempool.
 */
char *
mono_string_to_utf8_mp_ignore (MonoMemPool *mp, MonoString *s)
{
	MONO_REQ_GC_UNSAFE_MODE;

	return mono_string_to_utf8_internal (mp, NULL, s, TRUE, NULL);
}


/**
 * mono_string_to_utf16:
 * @s: a MonoString
 *
 * Return an null-terminated array of the utf-16 chars
 * contained in @s. The result must be freed with g_free().
 * This is a temporary helper until our string implementation
 * is reworked to always include the null terminating char.
 */
mono_unichar2*
mono_string_to_utf16 (MonoString *s)
{
	MONO_REQ_GC_UNSAFE_MODE;

	char *as;

	if (s == NULL)
		return NULL;

	as = (char *)g_malloc ((s->length * 2) + 2);
	as [(s->length * 2)] = '\0';
	as [(s->length * 2) + 1] = '\0';

	if (!s->length) {
		return (gunichar2 *)(as);
	}
	
	memcpy (as, mono_string_chars(s), s->length * 2);
	return (gunichar2 *)(as);
}

/**
 * mono_string_to_utf32:
 * @s: a MonoString
 *
 * Return an null-terminated array of the UTF-32 (UCS-4) chars
 * contained in @s. The result must be freed with g_free().
 */
mono_unichar4*
mono_string_to_utf32 (MonoString *s)
{
	MONO_REQ_GC_UNSAFE_MODE;

	mono_unichar4 *utf32_output = NULL; 
	GError *error = NULL;
	glong items_written;
	
	if (s == NULL)
		return NULL;
		
	utf32_output = g_utf16_to_ucs4 (s->chars, s->length, NULL, &items_written, &error);
	
	if (error)
		g_error_free (error);

	return utf32_output;
}

/**
 * mono_string_from_utf16:
 * @data: the UTF16 string (LPWSTR) to convert
 *
 * Converts a NULL terminated UTF16 string (LPWSTR) to a MonoString.
 *
 * Returns: a MonoString.
 */
MonoString *
mono_string_from_utf16 (gunichar2 *data)
{
	MonoError error;
	MonoString *result = mono_string_from_utf16_checked (data, &error);
	mono_error_cleanup (&error);
	return result;
}

/**
 * mono_string_from_utf16_checked:
 * @data: the UTF16 string (LPWSTR) to convert
 * @error: set on error
 *
 * Converts a NULL terminated UTF16 string (LPWSTR) to a MonoString.
 *
 * Returns: a MonoString. On failure sets @error and returns NULL.
 */
MonoString *
mono_string_from_utf16_checked (gunichar2 *data, MonoError *error)
{

	MONO_REQ_GC_UNSAFE_MODE;

	error_init (error);
	MonoDomain *domain = mono_domain_get ();
	int len = 0;

	if (!data)
		return NULL;

	while (data [len]) len++;

	return mono_string_new_utf16_checked (domain, data, len, error);
}

/**
 * mono_string_from_utf32:
 * @data: the UTF32 string (LPWSTR) to convert
 *
 * Converts a UTF32 (UCS-4)to a MonoString.
 *
 * Returns: a MonoString.
 */
MonoString *
mono_string_from_utf32 (mono_unichar4 *data)
{
	MonoError error;
	MonoString *result = mono_string_from_utf32_checked (data, &error);
	mono_error_cleanup (&error);
	return result;
}

/**
 * mono_string_from_utf32_checked:
 * @data: the UTF32 string (LPWSTR) to convert
 * @error: set on error
 *
 * Converts a UTF32 (UCS-4)to a MonoString.
 *
 * Returns: a MonoString. On failure returns NULL and sets @error.
 */
MonoString *
mono_string_from_utf32_checked (mono_unichar4 *data, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	error_init (error);
	MonoString* result = NULL;
	mono_unichar2 *utf16_output = NULL;
	GError *gerror = NULL;
	glong items_written;
	int len = 0;

	if (!data)
		return NULL;

	while (data [len]) len++;

	utf16_output = g_ucs4_to_utf16 (data, len, NULL, &items_written, &gerror);

	if (gerror)
		g_error_free (gerror);

	result = mono_string_from_utf16_checked (utf16_output, error);
	g_free (utf16_output);
	return result;
}

static char *
mono_string_to_utf8_internal (MonoMemPool *mp, MonoImage *image, MonoString *s, gboolean ignore_error, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	char *r;
	char *mp_s;
	int len;

	if (ignore_error) {
		r = mono_string_to_utf8_ignore (s);
	} else {
		r = mono_string_to_utf8_checked (s, error);
		if (!mono_error_ok (error))
			return NULL;
	}

	if (!mp && !image)
		return r;

	len = strlen (r) + 1;
	if (mp)
		mp_s = (char *)mono_mempool_alloc (mp, len);
	else
		mp_s = (char *)mono_image_alloc (image, len);

	memcpy (mp_s, r, len);

	g_free (r);

	return mp_s;
}

/**
 * mono_string_to_utf8_image:
 * @s: a System.String
 *
 * Same as mono_string_to_utf8, but allocate the string from the image mempool.
 */
char *
mono_string_to_utf8_image (MonoImage *image, MonoStringHandle s, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	return mono_string_to_utf8_internal (NULL, image, MONO_HANDLE_RAW (s), FALSE, error); /* FIXME pin the string */
}

/**
 * mono_string_to_utf8_mp:
 * @s: a System.String
 *
 * Same as mono_string_to_utf8, but allocate the string from a mempool.
 */
char *
mono_string_to_utf8_mp (MonoMemPool *mp, MonoString *s, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	return mono_string_to_utf8_internal (mp, NULL, s, FALSE, error);
}


static MonoRuntimeExceptionHandlingCallbacks eh_callbacks;

void
mono_install_eh_callbacks (MonoRuntimeExceptionHandlingCallbacks *cbs)
{
	eh_callbacks = *cbs;
}

MonoRuntimeExceptionHandlingCallbacks *
mono_get_eh_callbacks (void)
{
	return &eh_callbacks;
}

/**
 * mono_raise_exception:
 * @ex: exception object
 *
 * Signal the runtime that the exception @ex has been raised in unmanaged code.
 */
void
mono_raise_exception (MonoException *ex) 
{
	MONO_REQ_GC_UNSAFE_MODE;

	/*
	 * NOTE: Do NOT annotate this function with G_GNUC_NORETURN, since
	 * that will cause gcc to omit the function epilog, causing problems when
	 * the JIT tries to walk the stack, since the return address on the stack
	 * will point into the next function in the executable, not this one.
	 */	
	eh_callbacks.mono_raise_exception (ex);
}

void
mono_raise_exception_with_context (MonoException *ex, MonoContext *ctx) 
{
	MONO_REQ_GC_UNSAFE_MODE;

	eh_callbacks.mono_raise_exception_with_ctx (ex, ctx);
}

/**
 * mono_wait_handle_new:
 * @domain: Domain where the object will be created
 * @handle: Handle for the wait handle
 * @error: set on error.
 *
 * Returns: A new MonoWaitHandle created in the given domain for the
 * given handle.  On failure returns NULL and sets @rror.
 */
MonoWaitHandle *
mono_wait_handle_new (MonoDomain *domain, HANDLE handle, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoWaitHandle *res;
	gpointer params [1];
	static MonoMethod *handle_set;

	error_init (error);
	res = (MonoWaitHandle *)mono_object_new_checked (domain, mono_defaults.manualresetevent_class, error);
	return_val_if_nok (error, NULL);

	/* Even though this method is virtual, it's safe to invoke directly, since the object type matches.  */
	if (!handle_set)
		handle_set = mono_class_get_property_from_name (mono_defaults.manualresetevent_class, "Handle")->set;

	params [0] = &handle;

	mono_runtime_invoke_checked (handle_set, res, params, error);
	return res;
}

HANDLE
mono_wait_handle_get_handle (MonoWaitHandle *handle)
{
	MONO_REQ_GC_UNSAFE_MODE;

	static MonoClassField *f_safe_handle = NULL;
	MonoSafeHandle *sh;

	if (!f_safe_handle) {
		f_safe_handle = mono_class_get_field_from_name (mono_defaults.manualresetevent_class, "safeWaitHandle");
		g_assert (f_safe_handle);
	}

	mono_field_get_value ((MonoObject*)handle, f_safe_handle, &sh);
	return sh->handle;
}


static MonoObject*
mono_runtime_capture_context (MonoDomain *domain, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	RuntimeInvokeFunction runtime_invoke;

	error_init (error);

	if (!domain->capture_context_runtime_invoke || !domain->capture_context_method) {
		MonoMethod *method = mono_get_context_capture_method ();
		MonoMethod *wrapper;
		if (!method)
			return NULL;
		wrapper = mono_marshal_get_runtime_invoke (method, FALSE);
		domain->capture_context_runtime_invoke = mono_compile_method_checked (wrapper, error);
		return_val_if_nok (error, NULL);
		domain->capture_context_method = mono_compile_method_checked (method, error);
		return_val_if_nok (error, NULL);
	}

	runtime_invoke = (RuntimeInvokeFunction)domain->capture_context_runtime_invoke;

	return runtime_invoke (NULL, NULL, NULL, domain->capture_context_method);
}
/**
 * mono_async_result_new:
 * @domain:domain where the object will be created.
 * @handle: wait handle.
 * @state: state to pass to AsyncResult
 * @data: C closure data.
 * @error: set on error.
 *
 * Creates a new MonoAsyncResult (AsyncResult C# class) in the given domain.
 * If the handle is not null, the handle is initialized to a MonOWaitHandle.
 * On failure returns NULL and sets @error.
 *
 */
MonoAsyncResult *
mono_async_result_new (MonoDomain *domain, HANDLE handle, MonoObject *state, gpointer data, MonoObject *object_data, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	error_init (error);
	MonoAsyncResult *res = (MonoAsyncResult *)mono_object_new_checked (domain, mono_defaults.asyncresult_class, error);
	return_val_if_nok (error, NULL);
	MonoObject *context = mono_runtime_capture_context (domain, error);
	return_val_if_nok (error, NULL);
	/* we must capture the execution context from the original thread */
	if (context) {
		MONO_OBJECT_SETREF (res, execution_context, context);
		/* note: result may be null if the flow is suppressed */
	}

	res->data = (void **)data;
	MONO_OBJECT_SETREF (res, object_data, object_data);
	MONO_OBJECT_SETREF (res, async_state, state);
	MonoWaitHandle *wait_handle = mono_wait_handle_new (domain, handle, error);
	return_val_if_nok (error, NULL);
	if (handle != NULL)
		MONO_OBJECT_SETREF (res, handle, (MonoObject *) wait_handle);

	res->sync_completed = FALSE;
	res->completed = FALSE;

	return res;
}

MonoObject *
ves_icall_System_Runtime_Remoting_Messaging_AsyncResult_Invoke (MonoAsyncResult *ares)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoError error;
	MonoAsyncCall *ac;
	MonoObject *res;

	g_assert (ares);
	g_assert (ares->async_delegate);

	ac = (MonoAsyncCall*) ares->object_data;
	if (!ac) {
		res = mono_runtime_delegate_invoke_checked (ares->async_delegate, (void**) &ares->async_state, &error);
		if (mono_error_set_pending_exception (&error))
			return NULL;
	} else {
		gpointer wait_event = NULL;

		ac->msg->exc = NULL;

		res = mono_message_invoke (ares->async_delegate, ac->msg, &ac->msg->exc, &ac->out_args, &error);

		/* The exit side of the invoke must not be aborted as it would leave the runtime in an undefined state */
		mono_threads_begin_abort_protected_block ();

		if (!ac->msg->exc) {
			MonoException *ex = mono_error_convert_to_exception (&error);
			ac->msg->exc = (MonoObject *)ex;
		} else {
			mono_error_cleanup (&error);
		}

		MONO_OBJECT_SETREF (ac, res, res);

		mono_monitor_enter ((MonoObject*) ares);
		ares->completed = 1;
		if (ares->handle)
			wait_event = mono_wait_handle_get_handle ((MonoWaitHandle*) ares->handle);
		mono_monitor_exit ((MonoObject*) ares);

		if (wait_event != NULL)
			mono_w32event_set (wait_event);

		error_init (&error); //the else branch would leave it in an undefined state
		if (ac->cb_method)
			mono_runtime_invoke_checked (ac->cb_method, ac->cb_target, (gpointer*) &ares, &error);

		mono_threads_end_abort_protected_block ();

		if (mono_error_set_pending_exception (&error))
			return NULL;
	}

	return res;
}

gboolean
mono_message_init (MonoDomain *domain,
		   MonoMethodMessage *this_obj, 
		   MonoReflectionMethod *method,
		   MonoArray *out_args,
		   MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	static MonoMethod *init_message_method = NULL;

	if (!init_message_method) {
		init_message_method = mono_class_get_method_from_name (mono_defaults.mono_method_message_class, "InitMessage", 2);
		g_assert (init_message_method != NULL);
	}

	error_init (error);
	/* FIXME set domain instead? */
	g_assert (domain == mono_domain_get ());
	
	gpointer args[2];

	args[0] = method;
	args[1] = out_args;

	mono_runtime_invoke_checked (init_message_method, this_obj, args, error);
	return is_ok (error);
}

#ifndef DISABLE_REMOTING
/**
 * mono_remoting_invoke:
 * @real_proxy: pointer to a RealProxy object
 * @msg: The MonoMethodMessage to execute
 * @exc: used to store exceptions
 * @out_args: used to store output arguments
 *
 * This is used to call RealProxy::Invoke(). RealProxy::Invoke() returns an
 * IMessage interface and it is not trivial to extract results from there. So
 * we call an helper method PrivateInvoke instead of calling
 * RealProxy::Invoke() directly.
 *
 * Returns: the result object.
 */
MonoObject *
mono_remoting_invoke (MonoObject *real_proxy, MonoMethodMessage *msg, MonoObject **exc, MonoArray **out_args, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoObject *o;
	MonoMethod *im = real_proxy->vtable->domain->private_invoke_method;
	gpointer pa [4];

	g_assert (exc);

	error_init (error);

	/*static MonoObject *(*invoke) (gpointer, gpointer, MonoObject **, MonoArray **) = NULL;*/

	if (!im) {
		im = mono_class_get_method_from_name (mono_defaults.real_proxy_class, "PrivateInvoke", 4);
		if (!im) {
			mono_error_set_not_supported (error, "Linked away.");
			return NULL;
		}
		real_proxy->vtable->domain->private_invoke_method = im;
	}

	pa [0] = real_proxy;
	pa [1] = msg;
	pa [2] = exc;
	pa [3] = out_args;

	o = mono_runtime_try_invoke (im, NULL, pa, exc, error);
	return_val_if_nok (error, NULL);

	return o;
}
#endif

MonoObject *
mono_message_invoke (MonoObject *target, MonoMethodMessage *msg, 
		     MonoObject **exc, MonoArray **out_args, MonoError *error) 
{
	MONO_REQ_GC_UNSAFE_MODE;

	static MonoClass *object_array_klass;
	error_init (error);

	MonoDomain *domain; 
	MonoMethod *method;
	MonoMethodSignature *sig;
	MonoArray *arr;
	int i, j, outarg_count = 0;

#ifndef DISABLE_REMOTING
	if (target && mono_object_is_transparent_proxy (target)) {
		MonoTransparentProxy* tp = (MonoTransparentProxy *)target;
		if (mono_class_is_contextbound (tp->remote_class->proxy_class) && tp->rp->context == (MonoObject *) mono_context_get ()) {
			target = tp->rp->unwrapped_server;
		} else {
			return mono_remoting_invoke ((MonoObject *)tp->rp, msg, exc, out_args, error);
		}
	}
#endif

	domain = mono_domain_get (); 
	method = msg->method->method;
	sig = mono_method_signature (method);

	for (i = 0; i < sig->param_count; i++) {
		if (sig->params [i]->byref) 
			outarg_count++;
	}

	if (!object_array_klass) {
		MonoClass *klass;

		klass = mono_array_class_get (mono_defaults.object_class, 1);
		g_assert (klass);

		mono_memory_barrier ();
		object_array_klass = klass;
	}

	arr = mono_array_new_specific_checked (mono_class_vtable (domain, object_array_klass), outarg_count, error);
	return_val_if_nok (error, NULL);

	mono_gc_wbarrier_generic_store (out_args, (MonoObject*) arr);
	*exc = NULL;

	MonoObject *ret = mono_runtime_try_invoke_array (method, method->klass->valuetype? mono_object_unbox (target): target, msg->args, exc, error);
	return_val_if_nok (error, NULL);

	for (i = 0, j = 0; i < sig->param_count; i++) {
		if (sig->params [i]->byref) {
			MonoObject* arg;
			arg = (MonoObject *)mono_array_get (msg->args, gpointer, i);
			mono_array_setref (*out_args, j, arg);
			j++;
		}
	}

	return ret;
}

/**
 * prepare_to_string_method:
 * @obj: The object
 * @target: Set to @obj or unboxed value if a valuetype
 *
 * Returns: the ToString override for @obj. If @obj is a valuetype, @target is unboxed otherwise it's @obj.
 */
static MonoMethod *
prepare_to_string_method (MonoObject *obj, void **target)
{
	MONO_REQ_GC_UNSAFE_MODE;

	static MonoMethod *to_string = NULL;
	MonoMethod *method;
	g_assert (target);
	g_assert (obj);

	*target = obj;

	if (!to_string)
		to_string = mono_class_get_method_from_name_flags (mono_get_object_class (), "ToString", 0, METHOD_ATTRIBUTE_VIRTUAL | METHOD_ATTRIBUTE_PUBLIC);

	method = mono_object_get_virtual_method (obj, to_string);

	// Unbox value type if needed
	if (mono_class_is_valuetype (mono_method_get_class (method))) {
		*target = mono_object_unbox (obj);
	}
	return method;
}

/**
 * mono_object_to_string:
 * @obj: The object
 * @exc: Any exception thrown by ToString (). May be NULL.
 *
 * Returns: the result of calling ToString () on an object.
 */
MonoString *
mono_object_to_string (MonoObject *obj, MonoObject **exc)
{
	MonoError error;
	MonoString *s = NULL;
	void *target;
	MonoMethod *method = prepare_to_string_method (obj, &target);
	if (exc) {
		s = (MonoString *) mono_runtime_try_invoke (method, target, NULL, exc, &error);
		if (*exc == NULL && !mono_error_ok (&error))
			*exc = (MonoObject*) mono_error_convert_to_exception (&error);
		else
			mono_error_cleanup (&error);
	} else {
		s = (MonoString *) mono_runtime_invoke_checked (method, target, NULL, &error);
		mono_error_raise_exception (&error); /* OK to throw, external only without a good alternative */
	}

	return s;
}

/**
 * mono_object_to_string_checked:
 * @obj: The object
 * @error: Set on error.
 *
 * Returns: the result of calling ToString () on an object. If the
 * method cannot be invoked or if it raises an exception, sets @error
 * and returns NULL.
 */
MonoString *
mono_object_to_string_checked (MonoObject *obj, MonoError *error)
{
	error_init (error);
	void *target;
	MonoMethod *method = prepare_to_string_method (obj, &target);
	return (MonoString*) mono_runtime_invoke_checked (method, target, NULL, error);
}

/**
 * mono_object_try_to_string:
 * @obj: The object
 * @exc: Any exception thrown by ToString (). Must not be NULL.
 * @error: Set if method cannot be invoked.
 *
 * Returns: the result of calling ToString () on an object. If the
 * method cannot be invoked sets @error, if it raises an exception sets @exc,
 * and returns NULL.
 */
MonoString *
mono_object_try_to_string (MonoObject *obj, MonoObject **exc, MonoError *error)
{
	g_assert (exc);
	error_init (error);
	void *target;
	MonoMethod *method = prepare_to_string_method (obj, &target);
	return (MonoString*) mono_runtime_try_invoke (method, target, NULL, exc, error);
}



static char *
get_native_backtrace (MonoException *exc_raw)
{
	HANDLE_FUNCTION_ENTER ();
	MONO_HANDLE_DCL(MonoException, exc);
	char * trace = mono_exception_handle_get_native_backtrace (exc);
	HANDLE_FUNCTION_RETURN_VAL (trace);
}

/**
 * mono_print_unhandled_exception:
 * @exc: The exception
 *
 * Prints the unhandled exception.
 */
void
mono_print_unhandled_exception (MonoObject *exc)
{
	MONO_REQ_GC_UNSAFE_MODE;

	MonoString * str;
	char *message = (char*)"";
	gboolean free_message = FALSE;
	MonoError error;

	if (exc == (MonoObject*)mono_object_domain (exc)->out_of_memory_ex) {
		message = g_strdup ("OutOfMemoryException");
		free_message = TRUE;
	} else if (exc == (MonoObject*)mono_object_domain (exc)->stack_overflow_ex) {
		message = g_strdup ("StackOverflowException"); //if we OVF, we can't expect to have stack space to JIT Exception::ToString.
		free_message = TRUE;
	} else {
		
		if (((MonoException*)exc)->native_trace_ips) {
			message = get_native_backtrace ((MonoException*)exc);
			free_message = TRUE;
		} else {
			MonoObject *other_exc = NULL;
			str = mono_object_try_to_string (exc, &other_exc, &error);
			if (other_exc == NULL && !is_ok (&error))
				other_exc = (MonoObject*)mono_error_convert_to_exception (&error);
			else
				mono_error_cleanup (&error);
			if (other_exc) {
				char *original_backtrace = mono_exception_get_managed_backtrace ((MonoException*)exc);
				char *nested_backtrace = mono_exception_get_managed_backtrace ((MonoException*)other_exc);
				
				message = g_strdup_printf ("Nested exception detected.\nOriginal Exception: %s\nNested exception:%s\n",
					original_backtrace, nested_backtrace);

				g_free (original_backtrace);
				g_free (nested_backtrace);
				free_message = TRUE;
			} else if (str) {
				message = mono_string_to_utf8_checked (str, &error);
				if (!mono_error_ok (&error)) {
					mono_error_cleanup (&error);
					message = (char *) "";
				} else {
					free_message = TRUE;
				}
			}
		}
	}

	/*
	 * g_printerr ("\nUnhandled Exception: %s.%s: %s\n", exc->vtable->klass->name_space, 
	 *	   exc->vtable->klass->name, message);
	 */
	g_printerr ("\nUnhandled Exception:\n%s\n", message);
	
	if (free_message)
		g_free (message);
}

/**
 * mono_delegate_ctor_with_method:
 * @this: pointer to an uninitialized delegate object
 * @target: target object
 * @addr: pointer to native code
 * @method: method
 * @error: set on error.
 *
 * Initialize a delegate and sets a specific method, not the one
 * associated with addr.  This is useful when sharing generic code.
 * In that case addr will most probably not be associated with the
 * correct instantiation of the method.
 * On failure returns FALSE and sets @error.
 */
gboolean
mono_delegate_ctor_with_method (MonoObject *this_obj, MonoObject *target, gpointer addr, MonoMethod *method, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	error_init (error);
	MonoDelegate *delegate = (MonoDelegate *)this_obj;

	g_assert (this_obj);
	g_assert (addr);

	g_assert (mono_class_has_parent (mono_object_class (this_obj), mono_defaults.multicastdelegate_class));

	if (method)
		delegate->method = method;

	mono_stats.delegate_creations++;

#ifndef DISABLE_REMOTING
	if (target && mono_object_is_transparent_proxy (target)) {
		g_assert (method);
		method = mono_marshal_get_remoting_invoke (method);
#ifdef ENABLE_INTERPRETER
		g_error ("need RuntimeMethod in method_ptr when using interpreter");
#endif
		delegate->method_ptr = mono_compile_method_checked (method, error);
		return_val_if_nok (error, FALSE);
		MONO_OBJECT_SETREF (delegate, target, target);
	} else
#endif
	{
		delegate->method_ptr = addr;
		MONO_OBJECT_SETREF (delegate, target, target);
	}

	delegate->invoke_impl = callbacks.create_delegate_trampoline (delegate->object.vtable->domain, delegate->object.vtable->klass);
	if (callbacks.init_delegate)
		callbacks.init_delegate (delegate);
	return TRUE;
}

/**
 * mono_delegate_ctor:
 * @this: pointer to an uninitialized delegate object
 * @target: target object
 * @addr: pointer to native code
 * @error: set on error.
 *
 * This is used to initialize a delegate.
 * On failure returns FALSE and sets @error.
 */
gboolean
mono_delegate_ctor (MonoObject *this_obj, MonoObject *target, gpointer addr, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	error_init (error);
	MonoDomain *domain = mono_domain_get ();
	MonoJitInfo *ji;
	MonoMethod *method = NULL;

	g_assert (addr);

	ji = mono_jit_info_table_find (domain, (char *)mono_get_addr_from_ftnptr (addr));
	/* Shared code */
	if (!ji && domain != mono_get_root_domain ())
		ji = mono_jit_info_table_find (mono_get_root_domain (), (char *)mono_get_addr_from_ftnptr (addr));
	if (ji) {
		method = mono_jit_info_get_method (ji);
		g_assert (!mono_class_is_gtd (method->klass));
	}

	return mono_delegate_ctor_with_method (this_obj, target, addr, method, error);
}

/**
 * mono_method_call_message_new:
 * @method: method to encapsulate
 * @params: parameters to the method
 * @invoke: optional, delegate invoke.
 * @cb: async callback delegate.
 * @state: state passed to the async callback.
 * @error: set on error.
 *
 * Translates arguments pointers into a MonoMethodMessage.
 * On failure returns NULL and sets @error.
 */
MonoMethodMessage *
mono_method_call_message_new (MonoMethod *method, gpointer *params, MonoMethod *invoke, 
			      MonoDelegate **cb, MonoObject **state, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	error_init (error);

	MonoDomain *domain = mono_domain_get ();
	MonoMethodSignature *sig = mono_method_signature (method);
	MonoMethodMessage *msg;
	int i, count;

	msg = (MonoMethodMessage *)mono_object_new_checked (domain, mono_defaults.mono_method_message_class, error); 
	return_val_if_nok  (error, NULL);

	if (invoke) {
		MonoReflectionMethod *rm = mono_method_get_object_checked (domain, invoke, NULL, error);
		return_val_if_nok (error, NULL);
		mono_message_init (domain, msg, rm, NULL, error);
		return_val_if_nok (error, NULL);
		count =  sig->param_count - 2;
	} else {
		MonoReflectionMethod *rm = mono_method_get_object_checked (domain, method, NULL, error);
		return_val_if_nok (error, NULL);
		mono_message_init (domain, msg, rm, NULL, error);
		return_val_if_nok (error, NULL);
		count =  sig->param_count;
	}

	for (i = 0; i < count; i++) {
		gpointer vpos;
		MonoClass *klass;
		MonoObject *arg;

		if (sig->params [i]->byref)
			vpos = *((gpointer *)params [i]);
		else 
			vpos = params [i];

		klass = mono_class_from_mono_type (sig->params [i]);

		if (klass->valuetype) {
			arg = mono_value_box_checked (domain, klass, vpos, error);
			return_val_if_nok (error, NULL);
		} else 
			arg = *((MonoObject **)vpos);
		      
		mono_array_setref (msg->args, i, arg);
	}

	if (cb != NULL && state != NULL) {
		*cb = *((MonoDelegate **)params [i]);
		i++;
		*state = *((MonoObject **)params [i]);
	}

	return msg;
}

/**
 * mono_method_return_message_restore:
 *
 * Restore results from message based processing back to arguments pointers
 */
void
mono_method_return_message_restore (MonoMethod *method, gpointer *params, MonoArray *out_args, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	error_init (error);

	MonoMethodSignature *sig = mono_method_signature (method);
	int i, j, type, size, out_len;
	
	if (out_args == NULL)
		return;
	out_len = mono_array_length (out_args);
	if (out_len == 0)
		return;

	for (i = 0, j = 0; i < sig->param_count; i++) {
		MonoType *pt = sig->params [i];

		if (pt->byref) {
			char *arg;
			if (j >= out_len) {
				mono_error_set_execution_engine (error, "The proxy call returned an incorrect number of output arguments");
				return;
			}

			arg = (char *)mono_array_get (out_args, gpointer, j);
			type = pt->type;

			g_assert (type != MONO_TYPE_VOID);

			if (MONO_TYPE_IS_REFERENCE (pt)) {
				mono_gc_wbarrier_generic_store (*((MonoObject ***)params [i]), (MonoObject *)arg);
			} else {
				if (arg) {
					MonoClass *klass = ((MonoObject*)arg)->vtable->klass;
					size = mono_class_value_size (klass, NULL);
					if (klass->has_references)
						mono_gc_wbarrier_value_copy (*((gpointer *)params [i]), arg + sizeof (MonoObject), 1, klass);
					else
						mono_gc_memmove_atomic (*((gpointer *)params [i]), arg + sizeof (MonoObject), size);
				} else {
					size = mono_class_value_size (mono_class_from_mono_type (pt), NULL);
					mono_gc_bzero_atomic (*((gpointer *)params [i]), size);
				}
			}

			j++;
		}
	}
}

#ifndef DISABLE_REMOTING

/**
 * mono_load_remote_field:
 * @this: pointer to an object
 * @klass: klass of the object containing @field
 * @field: the field to load
 * @res: a storage to store the result
 *
 * This method is called by the runtime on attempts to load fields of
 * transparent proxy objects. @this points to such TP, @klass is the class of
 * the object containing @field. @res is a storage location which can be
 * used to store the result.
 *
 * Returns: an address pointing to the value of field.
 */
gpointer
mono_load_remote_field (MonoObject *this_obj, MonoClass *klass, MonoClassField *field, gpointer *res)
{
	MonoError error;
	gpointer result = mono_load_remote_field_checked (this_obj, klass, field, res, &error);
	mono_error_cleanup (&error);
	return result;
}

/**
 * mono_load_remote_field_checked:
 * @this: pointer to an object
 * @klass: klass of the object containing @field
 * @field: the field to load
 * @res: a storage to store the result
 * @error: set on error
 *
 * This method is called by the runtime on attempts to load fields of
 * transparent proxy objects. @this points to such TP, @klass is the class of
 * the object containing @field. @res is a storage location which can be
 * used to store the result.
 *
 * Returns: an address pointing to the value of field.  On failure returns NULL and sets @error.
 */
gpointer
mono_load_remote_field_checked (MonoObject *this_obj, MonoClass *klass, MonoClassField *field, gpointer *res, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	static MonoMethod *getter = NULL;

	error_init (error);

	MonoDomain *domain = mono_domain_get ();
	MonoTransparentProxy *tp = (MonoTransparentProxy *) this_obj;
	MonoClass *field_class;
	MonoMethodMessage *msg;
	MonoArray *out_args;
	MonoObject *exc;
	char* full_name;

	g_assert (mono_object_is_transparent_proxy (this_obj));
	g_assert (res != NULL);

	if (mono_class_is_contextbound (tp->remote_class->proxy_class) && tp->rp->context == (MonoObject *) mono_context_get ()) {
		mono_field_get_value (tp->rp->unwrapped_server, field, res);
		return res;
	}
	
	if (!getter) {
		getter = mono_class_get_method_from_name (mono_defaults.object_class, "FieldGetter", -1);
		if (!getter) {
			mono_error_set_not_supported (error, "Linked away.");
			return NULL;
		}
	}
	
	field_class = mono_class_from_mono_type (field->type);

	msg = (MonoMethodMessage *)mono_object_new_checked (domain, mono_defaults.mono_method_message_class, error);
	return_val_if_nok (error, NULL);
	out_args = mono_array_new_checked (domain, mono_defaults.object_class, 1, error);
	return_val_if_nok (error, NULL);
	MonoReflectionMethod *rm = mono_method_get_object_checked (domain, getter, NULL, error);
	return_val_if_nok (error, NULL);
	mono_message_init (domain, msg, rm, out_args, error);
	return_val_if_nok (error, NULL);

	full_name = mono_type_get_full_name (klass);
	mono_array_setref (msg->args, 0, mono_string_new (domain, full_name));
	mono_array_setref (msg->args, 1, mono_string_new (domain, mono_field_get_name (field)));
	g_free (full_name);

	mono_remoting_invoke ((MonoObject *)(tp->rp), msg, &exc, &out_args, error);
	return_val_if_nok (error, NULL);

	if (exc) {
		mono_error_set_exception_instance (error, (MonoException *)exc);
		return NULL;
	}

	if (mono_array_length (out_args) == 0)
		return NULL;

	mono_gc_wbarrier_generic_store (res, mono_array_get (out_args, MonoObject *, 0));

	if (field_class->valuetype) {
		return ((char *)*res) + sizeof (MonoObject);
	} else
		return res;
}

/**
 * mono_load_remote_field_new:
 * @this: 
 * @klass: 
 * @field:
 *
 * Missing documentation.
 */
MonoObject *
mono_load_remote_field_new (MonoObject *this_obj, MonoClass *klass, MonoClassField *field)
{
	MonoError error;

	MonoObject *result = mono_load_remote_field_new_checked (this_obj, klass, field, &error);
	mono_error_cleanup (&error);
	return result;
}

/**
 * mono_load_remote_field_new_checked:
 * @this: pointer to an object
 * @klass: klass of the object containing @field
 * @field: the field to load
 * @error: set on error.
 *
 * This method is called by the runtime on attempts to load fields of
 * transparent proxy objects. @this points to such TP, @klass is the class of
 * the object containing @field.
 * 
 * Returns: a freshly allocated object containing the value of the field.  On failure returns NULL and sets @error.
 */
MonoObject *
mono_load_remote_field_new_checked (MonoObject *this_obj, MonoClass *klass, MonoClassField *field, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	error_init (error);

	static MonoMethod *tp_load = NULL;

	g_assert (mono_object_is_transparent_proxy (this_obj));

	if (!tp_load) {
		tp_load = mono_class_get_method_from_name (mono_defaults.transparent_proxy_class, "LoadRemoteFieldNew", -1);
		if (!tp_load) {
			mono_error_set_not_supported (error, "Linked away.");
			return NULL;
		}
	}
	
	/* MonoType *type = mono_class_get_type (klass); */

	gpointer args[2];
	args [0] = &klass;
	args [1] = &field;

	return mono_runtime_invoke_checked (tp_load, this_obj, args, error);
}

/**
 * mono_store_remote_field:
 * @this_obj: pointer to an object
 * @klass: klass of the object containing @field
 * @field: the field to load
 * @val: the value/object to store
 *
 * This method is called by the runtime on attempts to store fields of
 * transparent proxy objects. @this_obj points to such TP, @klass is the class of
 * the object containing @field. @val is the new value to store in @field.
 */
void
mono_store_remote_field (MonoObject *this_obj, MonoClass *klass, MonoClassField *field, gpointer val)
{
	MonoError error;
	(void) mono_store_remote_field_checked (this_obj, klass, field, val, &error);
	mono_error_cleanup (&error);
}

/**
 * mono_store_remote_field_checked:
 * @this_obj: pointer to an object
 * @klass: klass of the object containing @field
 * @field: the field to load
 * @val: the value/object to store
 * @error: set on error
 *
 * This method is called by the runtime on attempts to store fields of
 * transparent proxy objects. @this_obj points to such TP, @klass is the class of
 * the object containing @field. @val is the new value to store in @field.
 *
 * Returns: on success returns TRUE, on failure returns FALSE and sets @error.
 */
gboolean
mono_store_remote_field_checked (MonoObject *this_obj, MonoClass *klass, MonoClassField *field, gpointer val, MonoError *error)
{
	
	MONO_REQ_GC_UNSAFE_MODE;

	error_init (error);

	MonoDomain *domain = mono_domain_get ();
	MonoClass *field_class;
	MonoObject *arg;

	g_assert (mono_object_is_transparent_proxy (this_obj));

	field_class = mono_class_from_mono_type (field->type);

	if (field_class->valuetype) {
		arg = mono_value_box_checked (domain, field_class, val, error);
		return_val_if_nok (error, FALSE);
	} else {
		arg = *((MonoObject**)val);
	}

	return mono_store_remote_field_new_checked (this_obj, klass, field, arg, error);
}

/**
 * mono_store_remote_field_new:
 * @this_obj:
 * @klass:
 * @field:
 * @arg:
 *
 * Missing documentation
 */
void
mono_store_remote_field_new (MonoObject *this_obj, MonoClass *klass, MonoClassField *field, MonoObject *arg)
{
	MonoError error;
	(void) mono_store_remote_field_new_checked (this_obj, klass, field, arg, &error);
	mono_error_cleanup (&error);
}

/**
 * mono_store_remote_field_new_checked:
 * @this_obj:
 * @klass:
 * @field:
 * @arg:
 * @error:
 *
 * Missing documentation
 */
gboolean
mono_store_remote_field_new_checked (MonoObject *this_obj, MonoClass *klass, MonoClassField *field, MonoObject *arg, MonoError *error)
{
	MONO_REQ_GC_UNSAFE_MODE;

	static MonoMethod *tp_store = NULL;

	error_init (error);

	g_assert (mono_object_is_transparent_proxy (this_obj));

	if (!tp_store) {
		tp_store = mono_class_get_method_from_name (mono_defaults.transparent_proxy_class, "StoreRemoteField", -1);
		if (!tp_store) {
			mono_error_set_not_supported (error, "Linked away.");
			return FALSE;
		}
	}

	gpointer args[3];
	args [0] = &klass;
	args [1] = &field;
	args [2] = arg;

	mono_runtime_invoke_checked (tp_store, this_obj, args, error);
	return is_ok (error);
}
#endif

/*
 * mono_create_ftnptr:
 *
 *   Given a function address, create a function descriptor for it.
 * This is only needed on some platforms.
 */
gpointer
mono_create_ftnptr (MonoDomain *domain, gpointer addr)
{
	return callbacks.create_ftnptr (domain, addr);
}

/*
 * mono_get_addr_from_ftnptr:
 *
 *   Given a pointer to a function descriptor, return the function address.
 * This is only needed on some platforms.
 */
gpointer
mono_get_addr_from_ftnptr (gpointer descr)
{
	return callbacks.get_addr_from_ftnptr (descr);
}	

/**
 * mono_string_chars:
 * @s: a MonoString
 *
 * Returns a pointer to the UCS16 characters stored in the MonoString
 */
gunichar2 *
mono_string_chars (MonoString *s)
{
	// MONO_REQ_GC_UNSAFE_MODE; //FIXME too much trouble for now

	return s->chars;
}

/**
 * mono_string_length:
 * @s: MonoString
 *
 * Returns the lenght in characters of the string
 */
int
mono_string_length (MonoString *s)
{
	MONO_REQ_GC_UNSAFE_MODE;

	return s->length;
}

/**
 * mono_string_handle_length:
 * @s: MonoString
 *
 * Returns the lenght in characters of the string
 */
int
mono_string_handle_length (MonoStringHandle s)
{
	MONO_REQ_GC_UNSAFE_MODE;

	return MONO_HANDLE_GETVAL (s, length);
}


/**
 * mono_array_length:
 * @array: a MonoArray*
 *
 * Returns the total number of elements in the array. This works for
 * both vectors and multidimensional arrays.
 */
uintptr_t
mono_array_length (MonoArray *array)
{
	MONO_REQ_GC_UNSAFE_MODE;

	return array->max_length;
}

/**
 * mono_array_addr_with_size:
 * @array: a MonoArray*
 * @size: size of the array elements
 * @idx: index into the array
 *
 * Use this function to obtain the address for the @idx item on the
 * @array containing elements of size @size.
 *
 * This method performs no bounds checking or type checking.
 *
 * Returns the address of the @idx element in the array.
 */
char*
mono_array_addr_with_size (MonoArray *array, int size, uintptr_t idx)
{
	MONO_REQ_GC_UNSAFE_MODE;

	return ((char*)(array)->vector) + size * idx;
}


MonoArray *
mono_glist_to_array (GList *list, MonoClass *eclass, MonoError *error) 
{
	MonoDomain *domain = mono_domain_get ();
	MonoArray *res;
	int len, i;

	error_init (error);
	if (!list)
		return NULL;

	len = g_list_length (list);
	res = mono_array_new_checked (domain, eclass, len, error);
	return_val_if_nok (error, NULL);

	for (i = 0; list; list = list->next, i++)
		mono_array_set (res, gpointer, i, list->data);

	return res;
}

#if NEVER_DEFINED
/*
 * The following section is purely to declare prototypes and
 * document the API, as these C files are processed by our
 * tool
 */

/**
 * mono_array_set:
 * @array: array to alter
 * @element_type: A C type name, this macro will use the sizeof(type) to determine the element size
 * @index: index into the array
 * @value: value to set
 *
 * Value Type version: This sets the @index's element of the @array
 * with elements of size sizeof(type) to the provided @value.
 *
 * This macro does not attempt to perform type checking or bounds checking.
 *
 * Use this to set value types in a `MonoArray`.
 */
void mono_array_set(MonoArray *array, Type element_type, uintptr_t index, Value value)
{
}

/**
 * mono_array_setref:
 * @array: array to alter
 * @index: index into the array
 * @value: value to set
 *
 * Reference Type version: This sets the @index's element of the
 * @array with elements of size sizeof(type) to the provided @value.
 *
 * This macro does not attempt to perform type checking or bounds checking.
 *
 * Use this to reference types in a `MonoArray`.
 */
void mono_array_setref(MonoArray *array, uintptr_t index, MonoObject *object)
{
}

/**
 * mono_array_get:
 * @array: array on which to operate on
 * @element_type: C element type (example: MonoString *, int, MonoObject *)
 * @index: index into the array
 *
 * Use this macro to retrieve the @index element of an @array and
 * extract the value assuming that the elements of the array match
 * the provided type value.
 *
 * This method can be used with both arrays holding value types and
 * reference types.   For reference types, the @type parameter should
 * be a `MonoObject*` or any subclass of it, like `MonoString*`.
 *
 * This macro does not attempt to perform type checking or bounds checking.
 *
 * Returns: The element at the @index position in the @array.
 */
Type mono_array_get (MonoArray *array, Type element_type, uintptr_t index)
{
}
#endif
 
