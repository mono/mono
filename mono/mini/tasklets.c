
#include "config.h"
#include "tasklets.h"
#include "mono/metadata/exception.h"
#include "mono/metadata/gc-internal.h"
#include "mini.h"

#if defined(MONO_SUPPORT_TASKLETS)

/* keepalive_stacks could be a per-stack var to avoid locking overhead */
static MonoGHashTable *keepalive_stacks;
static CRITICAL_SECTION tasklets_mutex;
#define tasklets_lock() EnterCriticalSection(&tasklets_mutex)
#define tasklets_unlock() LeaveCriticalSection(&tasklets_mutex)

/* LOCKING: tasklets_mutex is assumed to e taken */
static void
internal_init (void)
{
	if (keepalive_stacks)
		return;
	MONO_GC_REGISTER_ROOT_PINNING (keepalive_stacks);
	keepalive_stacks = mono_g_hash_table_new (NULL, NULL);
}

static void*
continuation_alloc (void)
{
	MonoContinuation *cont = g_new0 (MonoContinuation, 1);
	return cont;
}

static void
continuation_free (MonoContinuation *cont)
{
	if (cont->saved_stack) {
		tasklets_lock ();
		mono_g_hash_table_remove (keepalive_stacks, cont->saved_stack);
		tasklets_unlock ();
		mono_gc_free_fixed (cont->saved_stack);
	}
	g_free (cont);
}

static MonoException*
continuation_mark_frame (MonoContinuation *cont)
{
	MonoJitTlsData *jit_tls;
	MonoLMF *lmf;
	MonoContext ctx, new_ctx;
	MonoJitInfo *ji, rji;
	int endloop = FALSE;

	if (cont->domain)
		return mono_get_exception_argument ("cont", "Already marked");

	jit_tls = mono_native_tls_get_value (mono_jit_tls_id);
	lmf = mono_get_lmf();
	cont->domain = mono_domain_get ();
	cont->thread_id = GetCurrentThreadId ();

	/* get to the frame that called Mark () */
	memset (&rji, 0, sizeof (rji));
	do {
		ji = mono_find_jit_info (cont->domain, jit_tls, &rji, NULL, &ctx, &new_ctx, NULL, &lmf, NULL, NULL);
		if (!ji || ji == (gpointer)-1) {
			return mono_get_exception_not_supported ("Invalid stack frame");
		}
		ctx = new_ctx;
		if (endloop)
			break;
		if (strcmp (jinfo_get_method (ji)->name, "Mark") == 0)
			endloop = TRUE;
	} while (1);

	cont->top_sp = MONO_CONTEXT_GET_SP (&ctx);
	/*g_print ("method: %s, sp: %p\n", jinfo_get_method (ji)->name, cont->top_sp);*/

	return NULL;
}

static int
continuation_store (MonoContinuation *cont, int state, MonoException **e)
{
	MonoLMF *lmf = mono_get_lmf ();
	gsize num_bytes;

	if (!cont->domain) {
		*e =  mono_get_exception_argument ("cont", "Continuation not initialized");
		return 0;
	}
	if (cont->domain != mono_domain_get () || cont->thread_id != GetCurrentThreadId ()) {
		*e = mono_get_exception_argument ("cont", "Continuation from another thread or domain");
		return 0;
	}

	cont->lmf = lmf;
	cont->return_ip = __builtin_extract_return_addr (__builtin_return_address (0));
	cont->return_sp = __builtin_frame_address (0);

	num_bytes = (char*)cont->top_sp - (char*)cont->return_sp;

	/*g_print ("store: %d bytes, sp: %p, ip: %p, lmf: %p\n", num_bytes, cont->return_sp, cont->return_ip, lmf);*/

	if (cont->saved_stack && num_bytes <= cont->stack_alloc_size) {
		/* clear to avoid GC retention */
		if (num_bytes < cont->stack_used_size) {
			memset ((char*)cont->saved_stack + num_bytes, 0, cont->stack_used_size - num_bytes);
		}
		cont->stack_used_size = num_bytes;
	} else {
		tasklets_lock ();
		internal_init ();
		if (cont->saved_stack) {
			mono_g_hash_table_remove (keepalive_stacks, cont->saved_stack);
			mono_gc_free_fixed (cont->saved_stack);
		}
		cont->stack_used_size = num_bytes;
		cont->stack_alloc_size = num_bytes * 1.1;
		cont->saved_stack = mono_gc_alloc_fixed (cont->stack_alloc_size, NULL);
		mono_g_hash_table_insert (keepalive_stacks, cont->saved_stack, cont->saved_stack);
		tasklets_unlock ();
	}
	memcpy (cont->saved_stack, cont->return_sp, num_bytes);

	return state;
}

static MonoException*
continuation_restore (MonoContinuation *cont, int state)
{
	MonoLMF **lmf_addr = mono_get_lmf_addr ();
	MonoContinuationRestore restore_state = mono_tasklets_arch_restore ();

	if (!cont->domain || !cont->return_sp)
		return mono_get_exception_argument ("cont", "Continuation not initialized");
	if (cont->domain != mono_domain_get () || cont->thread_id != GetCurrentThreadId ())
		return mono_get_exception_argument ("cont", "Continuation from another thread or domain");

	/*g_print ("restore: %p, state: %d\n", cont, state);*/
	*lmf_addr = cont->lmf;
	restore_state (cont, state, lmf_addr);
	g_assert_not_reached ();
}

void
mono_tasklets_init (void)
{
	InitializeCriticalSection (&tasklets_mutex);

	mono_add_internal_call ("Mono.Tasklets.Continuation::alloc", continuation_alloc);
	mono_add_internal_call ("Mono.Tasklets.Continuation::free", continuation_free);
	mono_add_internal_call ("Mono.Tasklets.Continuation::mark", continuation_mark_frame);
	mono_add_internal_call ("Mono.Tasklets.Continuation::store", continuation_store);
	mono_add_internal_call ("Mono.Tasklets.Continuation::restore", continuation_restore);
}

void
mono_tasklets_cleanup (void)
{
}

#endif

