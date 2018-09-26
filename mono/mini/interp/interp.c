/**
 * \file
 * PLEASE NOTE: This is a research prototype.
 *
 *
 * interp.c: Interpreter for CIL byte codes
 *
 * Authors:
 *   Paolo Molaro (lupus@ximian.com)
 *   Miguel de Icaza (miguel@ximian.com)
 *   Dietmar Maurer (dietmar@ximian.com)
 *
 * (C) 2001, 2002 Ximian, Inc.
 */
#ifndef __USE_ISOC99
#define __USE_ISOC99
#endif
#include "config.h"

#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <glib.h>
#include <math.h>
#include <locale.h>

#include <mono/utils/gc_wrapper.h>
#include <mono/utils/mono-math.h>

#ifdef HAVE_ALLOCA_H
#   include <alloca.h>
#else
#   ifdef __CYGWIN__
#      define alloca __builtin_alloca
#   endif
#endif

/* trim excessive headers */
#include <mono/metadata/image.h>
#include <mono/metadata/assembly-internals.h>
#include <mono/metadata/cil-coff.h>
#include <mono/metadata/mono-endian.h>
#include <mono/metadata/tabledefs.h>
#include <mono/metadata/tokentype.h>
#include <mono/metadata/loader.h>
#include <mono/metadata/threads.h>
#include <mono/metadata/threadpool.h>
#include <mono/metadata/profiler-private.h>
#include <mono/metadata/appdomain.h>
#include <mono/metadata/reflection.h>
#include <mono/metadata/exception.h>
#include <mono/metadata/verify.h>
#include <mono/metadata/opcodes.h>
#include <mono/metadata/debug-helpers.h>
#include <mono/metadata/mono-config.h>
#include <mono/metadata/marshal.h>
#include <mono/metadata/environment.h>
#include <mono/metadata/mono-debug.h>
#include <mono/metadata/gc-internals.h>
#include <mono/utils/atomic.h>

#include "interp.h"
#include "interp-internals.h"
#include "mintops.h"

#include <mono/mini/mini.h>
#include <mono/mini/mini-runtime.h>
#include <mono/mini/aot-runtime.h>
#include <mono/mini/jit-icalls.h>
#include <mono/mini/debugger-agent.h>
#include <mono/mini/ee.h>

#ifdef TARGET_ARM
#include <mono/mini/mini-arm.h>
#endif

static inline void
init_frame (InterpFrame *frame, InterpFrame *parent_frame, InterpMethod *rmethod, stackval *method_args, stackval *method_retval)
{
	frame->parent = parent_frame;
	frame->stack_args = method_args;
	frame->retval = method_retval;
	frame->imethod = rmethod;
	frame->ex = NULL;
	frame->ip = NULL;
	frame->invoke_trap = 0;
}

#define INIT_FRAME(frame,parent_frame,method_args,method_retval,domain,mono_method,error) do { \
	InterpMethod *_rmethod = mono_interp_get_imethod ((domain), (mono_method), (error));	\
	init_frame ((frame), (parent_frame), _rmethod, (method_args), (method_retval)); \
	} while (0)

#define interp_exec_method(frame, context) interp_exec_method_full ((frame), (context), NULL, NULL, -1, NULL)

/*
 * List of classes whose methods will be executed by transitioning to JITted code.
 * Used for testing.
 */
GSList *mono_interp_jit_classes;
/* If TRUE, interpreted code will be interrupted at function entry/backward branches */
static gboolean ss_enabled;

static gboolean interp_init_done = FALSE;

static char* dump_frame (InterpFrame *inv);
static MonoArray *get_trace_ips (MonoDomain *domain, InterpFrame *top);
static void interp_exec_method_full (InterpFrame *frame, ThreadContext *context, guint16 *start_with_ip, MonoException *filter_exception, int exit_at_finally, InterpFrame *base_frame);
static InterpMethod* lookup_method_pointer (gpointer addr);

typedef void (*ICallMethod) (InterpFrame *frame);

static MonoNativeTlsKey thread_context_id;

static char* dump_args (InterpFrame *inv);

#define DEBUG_INTERP 0
#define COUNT_OPS 0
#if DEBUG_INTERP
int mono_interp_traceopt = 2;
/* If true, then we output the opcodes as we interpret them */
static int global_tracing = 2;

static int debug_indent_level = 0;

static int break_on_method = 0;
static int nested_trace = 0;
static GList *db_methods = NULL;

static void
output_indent (void)
{
	int h;

	for (h = 0; h < debug_indent_level; h++)
		g_print ("  ");
}

static void
db_match_method (gpointer data, gpointer user_data)
{
	MonoMethod *m = (MonoMethod*)user_data;
	MonoMethodDesc *desc = data;

	if (mono_method_desc_full_match (desc, m))
		break_on_method = 1;
}

static void
debug_enter (InterpFrame *frame, int *tracing)
{
	if (db_methods) {
		g_list_foreach (db_methods, db_match_method, (gpointer)frame->imethod->method);
		if (break_on_method)
			*tracing = nested_trace ? (global_tracing = 2, 3) : 2;
		break_on_method = 0;
	}
	if (*tracing) {
		MonoMethod *method = frame->imethod->method;
		char *mn, *args = dump_args (frame);
		debug_indent_level++;
		output_indent ();
		mn = mono_method_full_name (method, FALSE);
		g_print ("(%p) Entering %s (", mono_thread_internal_current (), mn);
		g_free (mn);
		g_print  ("%s)\n", args);
		g_free (args);
	}
}


#define DEBUG_LEAVE()	\
	if (tracing) {	\
		char *mn, *args;	\
		args = dump_retval (frame);	\
		output_indent ();	\
		mn = mono_method_full_name (frame->imethod->method, FALSE); \
		g_print  ("(%p) Leaving %s", mono_thread_internal_current (),  mn);	\
		g_free (mn); \
		g_print  (" => %s\n", args);	\
		g_free (args);	\
		debug_indent_level--;	\
		if (tracing == 3) global_tracing = 0; \
	}

#else

int mono_interp_traceopt = 0;
static void debug_enter (InterpFrame *frame, int *tracing)
{
}
#define DEBUG_LEAVE()

#endif

static void
set_resume_state (ThreadContext *context, InterpFrame *frame)
{
	frame->ex = NULL;
	context->has_resume_state = 0;
	context->handler_frame = NULL;
	context->handler_ei = NULL;
}

/* Set the current execution state to the resume state in context */
#define SET_RESUME_STATE(context) do { \
		ip = (const guint16*)(context)->handler_ip;						\
		/* spec says stack should be empty at endfinally so it should be at the start too */ \
		sp = frame->stack; \
		vt_sp = (unsigned char *) sp + rtm->stack_size; \
		if (frame->ex) { \
		sp->data.p = frame->ex;											\
		++sp;															\
		} \
		/* We have thrown an exception from a finally block. Some of the leave targets were unwinded already */ \
		while (finally_ips && \
				finally_ips->data >= (context)->handler_ei->try_start && \
				finally_ips->data < (context)->handler_ei->try_end) \
			finally_ips = g_slist_remove (finally_ips, finally_ips->data); \
		set_resume_state ((context), (frame));							\
		goto main_loop;													\
	} while (0)

static void
set_context (ThreadContext *context)
{
	mono_native_tls_set_value (thread_context_id, context);

	if (!context)
		return;

	MonoJitTlsData *jit_tls = mono_tls_get_jit_tls ();
	g_assertf (jit_tls, "ThreadContext needs initialized JIT TLS");

	/* jit_tls assumes ownership of 'context' */
	jit_tls->interp_context = context;
}

static ThreadContext *
get_context (void)
{
	ThreadContext *context = (ThreadContext *) mono_native_tls_get_value (thread_context_id);
	if (context == NULL) {
		context = g_new0 (ThreadContext, 1);
		set_context (context);
	}
	return context;
}

static void
ves_real_abort (int line, MonoMethod *mh,
		const unsigned short *ip, stackval *stack, stackval *sp)
{
	ERROR_DECL (error);
	MonoMethodHeader *header = mono_method_get_header_checked (mh, error);
	mono_error_cleanup (error); /* FIXME: don't swallow the error */
	g_printerr ("Execution aborted in method: %s::%s\n", m_class_get_name (mh->klass), mh->name);
	g_printerr ("Line=%d IP=0x%04lx, Aborted execution\n", line, ip-(const unsigned short *) header->code);
	g_printerr ("0x%04x %02x\n", ip-(const unsigned short *) header->code, *ip);
	mono_metadata_free_mh (header);
}

#define ves_abort() \
	do {\
		ves_real_abort(__LINE__, frame->imethod->method, ip, frame->stack, sp); \
		THROW_EX (mono_get_exception_execution_engine (NULL), ip); \
	} while (0);

static InterpMethod*
lookup_imethod (MonoDomain *domain, MonoMethod *method)
{
	InterpMethod *rtm;
	MonoJitDomainInfo *info;

	info = domain_jit_info (domain);
	mono_domain_jit_code_hash_lock (domain);
	rtm = (InterpMethod*)mono_internal_hash_table_lookup (&info->interp_code_hash, method);
	mono_domain_jit_code_hash_unlock (domain);
	return rtm;
}

static gpointer
interp_get_remoting_invoke (gpointer addr, MonoError *error)
{
#ifndef DISABLE_REMOTING
	InterpMethod *imethod = lookup_method_pointer (addr);

	g_assert (imethod);
	g_assert (mono_use_interpreter);

	MonoMethod *remoting_invoke_method = mono_marshal_get_remoting_invoke (imethod->method, error);
	return_val_if_nok (error, NULL);
	return mono_interp_get_imethod (mono_domain_get (), remoting_invoke_method, error);
#else
	g_assert_not_reached ();
	return NULL;
#endif
}

InterpMethod*
mono_interp_get_imethod (MonoDomain *domain, MonoMethod *method, MonoError *error)
{
	InterpMethod *rtm;
	MonoJitDomainInfo *info;
	MonoMethodSignature *sig;
	int i;

	error_init (error);

	info = domain_jit_info (domain);
	mono_domain_jit_code_hash_lock (domain);
	rtm = (InterpMethod*)mono_internal_hash_table_lookup (&info->interp_code_hash, method);
	mono_domain_jit_code_hash_unlock (domain);
	if (rtm)
		return rtm;

	sig = mono_method_signature (method);

	rtm = (InterpMethod*)mono_domain_alloc0 (domain, sizeof (InterpMethod));
	rtm->method = method;
	rtm->domain = domain;
	rtm->param_count = sig->param_count;
	rtm->hasthis = sig->hasthis;
	rtm->vararg = sig->call_convention == MONO_CALL_VARARG;
	rtm->rtype = mini_get_underlying_type (sig->ret);
	rtm->param_types = (MonoType**)mono_domain_alloc0 (domain, sizeof (MonoType*) * sig->param_count);
	for (i = 0; i < sig->param_count; ++i)
		rtm->param_types [i] = mini_get_underlying_type (sig->params [i]);

	mono_domain_jit_code_hash_lock (domain);
	if (!mono_internal_hash_table_lookup (&info->interp_code_hash, method))
		mono_internal_hash_table_insert (&info->interp_code_hash, method, rtm);
	mono_domain_jit_code_hash_unlock (domain);

	rtm->prof_flags = mono_profiler_get_call_instrumentation_flags (rtm->method);

	return rtm;
}

#if defined (MONO_CROSS_COMPILE) || defined (HOST_WASM) || defined (_MSC_VER)
#define INTERP_PUSH_LMF_WITH_CTX_BODY(ext, exit_addr) /* do nothing */
#elif defined (MONO_ARCH_HAS_MONO_CONTEXT)
#define INTERP_PUSH_LMF_WITH_CTX_BODY(ext, exit_addr) \
	MONO_CONTEXT_GET_CURRENT ((ext).ctx); \
	MONO_CONTEXT_SET_IP (&(ext).ctx, (exit_addr)); \
	mono_arch_do_ip_adjustment (&(ext).ctx);
#else
#define INTERP_PUSH_LMF_WITH_CTX_BODY(ext, exit_addr) g_error ("requires working mono-context");
#endif

/* INTERP_PUSH_LMF_WITH_CTX:
 *
 * same as interp_push_lmf, but retrieving and attaching MonoContext to it.
 * This is needed to resume into the interp when the exception is thrown from
 * native code (see ./mono/tests/install_eh_callback.exe).
 *
 * This must be a macro in order to retrieve the right register values for
 * MonoContext.
 */
#define INTERP_PUSH_LMF_WITH_CTX(frame, ext, exit_addr) \
	memset (&(ext), 0, sizeof (MonoLMFExt)); \
	(ext).kind = MONO_LMFEXT_INTERP_EXIT_WITH_CTX; \
	(ext).interp_exit_data = (frame); \
	INTERP_PUSH_LMF_WITH_CTX_BODY ((ext), (exit_addr)); \
	mono_push_lmf (&(ext));

/*
 * interp_push_lmf:
 *
 * Push an LMF frame on the LMF stack
 * to mark the transition to native code.
 * This is needed for the native code to
 * be able to do stack walks.
 */
static void
interp_push_lmf (MonoLMFExt *ext, InterpFrame *frame)
{
	memset (ext, 0, sizeof (MonoLMFExt));
	ext->kind = MONO_LMFEXT_INTERP_EXIT;
	ext->interp_exit_data = frame;

	mono_push_lmf (ext);
}

static void
interp_pop_lmf (MonoLMFExt *ext)
{
	mono_pop_lmf (&ext->lmf);
}

static InterpMethod*
get_virtual_method (InterpMethod *imethod, MonoObject *obj)
{
	MonoMethod *m = imethod->method;
	MonoDomain *domain = imethod->domain;
	InterpMethod *ret = NULL;
	ERROR_DECL (error);

#ifndef DISABLE_REMOTING
	if (mono_object_is_transparent_proxy (obj)) {
		MonoMethod *remoting_invoke_method = mono_marshal_get_remoting_invoke_with_check (m, error);
		mono_error_assert_ok (error);
		ret = mono_interp_get_imethod (domain, remoting_invoke_method, error);
		mono_error_assert_ok (error);
		return ret;
	}
#endif

	if ((m->flags & METHOD_ATTRIBUTE_FINAL) || !(m->flags & METHOD_ATTRIBUTE_VIRTUAL)) {
		if (m->iflags & METHOD_IMPL_ATTRIBUTE_SYNCHRONIZED) {
			ret = mono_interp_get_imethod (domain, mono_marshal_get_synchronized_wrapper (m), error);
			mono_error_cleanup (error); /* FIXME: don't swallow the error */
		} else {
			ret = imethod;
		}
		return ret;
	}

	mono_class_setup_vtable (obj->vtable->klass);

	int slot = mono_method_get_vtable_slot (m);
	if (mono_class_is_interface (m->klass)) {
		g_assert (obj->vtable->klass != m->klass);
		/* TODO: interface offset lookup is slow, go through IMT instead */
		gboolean non_exact_match;
		slot += mono_class_interface_offset_with_variance (obj->vtable->klass, m->klass, &non_exact_match);
	}

	MonoMethod *virtual_method = m_class_get_vtable (mono_object_class (obj)) [slot];
	if (m->is_inflated && mono_method_get_context (m)->method_inst) {
		MonoGenericContext context = { NULL, NULL };

		if (mono_class_is_ginst (virtual_method->klass))
			context.class_inst = mono_class_get_generic_class (virtual_method->klass)->context.class_inst;
		else if (mono_class_is_gtd (virtual_method->klass))
			context.class_inst = mono_class_get_generic_container (virtual_method->klass)->context.class_inst;
		context.method_inst = mono_method_get_context (m)->method_inst;

		virtual_method = mono_class_inflate_generic_method_checked (virtual_method, &context, error);
		mono_error_cleanup (error); /* FIXME: don't swallow the error */
	}

	if (virtual_method->iflags & METHOD_IMPL_ATTRIBUTE_SYNCHRONIZED) {
		virtual_method = mono_marshal_get_synchronized_wrapper (virtual_method);
	}

	InterpMethod *virtual_imethod = mono_interp_get_imethod (domain, virtual_method, error);
	mono_error_cleanup (error); /* FIXME: don't swallow the error */
	return virtual_imethod;
}

static void inline
stackval_from_data (MonoType *type_, stackval *result, void *data, gboolean pinvoke)
{
	MonoType *type = mini_native_type_replace_type (type_);
	if (type->byref) {
		switch (type->type) {
		case MONO_TYPE_OBJECT:
		case MONO_TYPE_CLASS:
		case MONO_TYPE_STRING:
		case MONO_TYPE_ARRAY:
		case MONO_TYPE_SZARRAY:
			break;
		default:
			break;
		}
		result->data.p = *(gpointer*)data;
		return;
	}
	switch (type->type) {
	case MONO_TYPE_VOID:
		return;
	case MONO_TYPE_I1:
		result->data.i = *(gint8*)data;
		return;
	case MONO_TYPE_U1:
	case MONO_TYPE_BOOLEAN:
		result->data.i = *(guint8*)data;
		return;
	case MONO_TYPE_I2:
		result->data.i = *(gint16*)data;
		return;
	case MONO_TYPE_U2:
	case MONO_TYPE_CHAR:
		result->data.i = *(guint16*)data;
		return;
	case MONO_TYPE_I4:
		result->data.i = *(gint32*)data;
		return;
	case MONO_TYPE_U:
	case MONO_TYPE_I:
		result->data.nati = *(mono_i*)data;
		return;
	case MONO_TYPE_PTR:
		result->data.p = *(gpointer*)data;
		return;
	case MONO_TYPE_U4:
		result->data.i = *(guint32*)data;
		return;
	case MONO_TYPE_R4: {
		float tmp;
		/* memmove handles unaligned case */
		memmove (&tmp, data, sizeof (float));
		result->data.f = tmp;
		return;
    }
	case MONO_TYPE_I8:
	case MONO_TYPE_U8:
		memmove (&result->data.l, data, sizeof (gint64));
		return;
	case MONO_TYPE_R8:
		memmove (&result->data.f, data, sizeof (double));
		return;
	case MONO_TYPE_STRING:
	case MONO_TYPE_SZARRAY:
	case MONO_TYPE_CLASS:
	case MONO_TYPE_OBJECT:
	case MONO_TYPE_ARRAY:
		result->data.p = *(gpointer*)data;
		return;
	case MONO_TYPE_VALUETYPE:
		if (m_class_is_enumtype (type->data.klass)) {
			stackval_from_data (mono_class_enum_basetype (type->data.klass), result, data, pinvoke);
			return;
		} else if (pinvoke) {
			memcpy (result->data.vt, data, mono_class_native_size (type->data.klass, NULL));
		} else {
			mono_value_copy (result->data.vt, data, type->data.klass);
		}
		return;
	case MONO_TYPE_GENERICINST: {
		if (mono_type_generic_inst_is_valuetype (type)) {
			mono_value_copy (result->data.vt, data, mono_class_from_mono_type (type));
			return;
		}
		stackval_from_data (m_class_get_byval_arg (type->data.generic_class->container_class), result, data, pinvoke);
		return;
	}
	default:
		g_error ("got type 0x%02x", type->type);
	}
}

static void inline
stackval_to_data (MonoType *type_, stackval *val, void *data, gboolean pinvoke)
{
	MonoType *type = mini_native_type_replace_type (type_);
	if (type->byref) {
		gpointer *p = (gpointer*)data;
		*p = val->data.p;
		return;
	}
	/* printf ("TODAT0 %p\n", data); */
	switch (type->type) {
	case MONO_TYPE_I1:
	case MONO_TYPE_U1: {
		guint8 *p = (guint8*)data;
		*p = val->data.i;
		return;
	}
	case MONO_TYPE_BOOLEAN: {
		guint8 *p = (guint8*)data;
		*p = (val->data.i != 0);
		return;
	}
	case MONO_TYPE_I2:
	case MONO_TYPE_U2:
	case MONO_TYPE_CHAR: {
		guint16 *p = (guint16*)data;
		*p = val->data.i;
		return;
	}
	case MONO_TYPE_I: {
		mono_i *p = (mono_i*)data;
		/* In theory the value used by stloc should match the local var type
	 	   but in practice it sometimes doesn't (a int32 gets dup'd and stloc'd into
		   a native int - both by csc and mcs). Not sure what to do about sign extension
		   as it is outside the spec... doing the obvious */
		*p = (mono_i)val->data.nati;
		return;
	}
	case MONO_TYPE_U: {
		mono_u *p = (mono_u*)data;
		/* see above. */
		*p = (mono_u)val->data.nati;
		return;
	}
	case MONO_TYPE_I4:
	case MONO_TYPE_U4: {
		gint32 *p = (gint32*)data;
		*p = val->data.i;
		return;
	}
	case MONO_TYPE_I8:
	case MONO_TYPE_U8: {
		memmove (data, &val->data.l, sizeof (gint64));
		return;
	}
	case MONO_TYPE_R4: {
		float tmp = (float)val->data.f;
		/* memmove handles unaligned case */
		memmove (data, &tmp, sizeof (float));
		return;
	}
	case MONO_TYPE_R8: {
		memmove (data, &val->data.f, sizeof (double));
		return;
	}
	case MONO_TYPE_STRING:
	case MONO_TYPE_SZARRAY:
	case MONO_TYPE_CLASS:
	case MONO_TYPE_OBJECT:
	case MONO_TYPE_ARRAY: {
		gpointer *p = (gpointer *) data;
		mono_gc_wbarrier_generic_store (p, val->data.o);
		return;
	}
	case MONO_TYPE_PTR: {
		gpointer *p = (gpointer *) data;
		*p = val->data.p;
		return;
	}
	case MONO_TYPE_VALUETYPE:
		if (m_class_is_enumtype (type->data.klass)) {
			stackval_to_data (mono_class_enum_basetype (type->data.klass), val, data, pinvoke);
			return;
		} else if (pinvoke) {
			memcpy (data, val->data.vt, mono_class_native_size (type->data.klass, NULL));
		} else {
			mono_value_copy (data, val->data.vt, type->data.klass);
		}
		return;
	case MONO_TYPE_GENERICINST: {
		MonoClass *container_class = type->data.generic_class->container_class;

		if (m_class_is_valuetype (container_class) && !m_class_is_enumtype (container_class)) {
			mono_value_copy (data, val->data.vt, mono_class_from_mono_type (type));
			return;
		}
		stackval_to_data (m_class_get_byval_arg (type->data.generic_class->container_class), val, data, pinvoke);
		return;
	}
	default:
		g_error ("got type %x", type->type);
	}
}

/*
 * Same as stackval_to_data but return address of storage instead
 * of copying the value.
 */
static gpointer
stackval_to_data_addr (MonoType *type_, stackval *val)
{
	MonoType *type = mini_native_type_replace_type (type_);
	if (type->byref)
		return &val->data.p;

	switch (type->type) {
	case MONO_TYPE_I1:
	case MONO_TYPE_U1:
	case MONO_TYPE_BOOLEAN:
	case MONO_TYPE_I2:
	case MONO_TYPE_U2:
	case MONO_TYPE_CHAR:
	case MONO_TYPE_I4:
	case MONO_TYPE_U4:
		return &val->data.i;
	case MONO_TYPE_I:
	case MONO_TYPE_U:
		return &val->data.nati;
	case MONO_TYPE_I8:
	case MONO_TYPE_U8:
		return &val->data.l;
	case MONO_TYPE_R4:
	case MONO_TYPE_R8:
		return &val->data.f;
	case MONO_TYPE_STRING:
	case MONO_TYPE_SZARRAY:
	case MONO_TYPE_CLASS:
	case MONO_TYPE_OBJECT:
	case MONO_TYPE_ARRAY:
	case MONO_TYPE_PTR:
		return &val->data.p;
	case MONO_TYPE_VALUETYPE:
		if (m_class_is_enumtype (type->data.klass))
			return stackval_to_data_addr (mono_class_enum_basetype (type->data.klass), val);
		else
			return val->data.vt;
	case MONO_TYPE_TYPEDBYREF:
		return val->data.vt;
	case MONO_TYPE_GENERICINST: {
		MonoClass *container_class = type->data.generic_class->container_class;

		if (m_class_is_valuetype (container_class) && !m_class_is_enumtype (container_class))
			return val->data.vt;
		return stackval_to_data_addr (m_class_get_byval_arg (type->data.generic_class->container_class), val);
	}
	default:
		g_error ("got type %x", type->type);
	}
}

/*
 * interp_throw:
 *   Throw an exception from the interpreter.
 */
static MONO_NEVER_INLINE void
interp_throw (ThreadContext *context, MonoException *ex, InterpFrame *frame, gconstpointer ip, gboolean rethrow)
{
	ERROR_DECL (error);
	MonoLMFExt ext;

	interp_push_lmf (&ext, frame);
	frame->ip = (const guint16*)ip;
	frame->ex = ex;

	if (mono_object_isinst_checked ((MonoObject *) ex, mono_defaults.exception_class, error)) {
		MonoException *mono_ex = (MonoException *) ex;
		if (!rethrow) {
			mono_ex->stack_trace = NULL;
			mono_ex->trace_ips = NULL;
		}
	}
	mono_error_assert_ok (error);

	MonoContext ctx;
	memset (&ctx, 0, sizeof (MonoContext));
	MONO_CONTEXT_SET_SP (&ctx, frame);

	/*
	 * Call the JIT EH code. The EH code will call back to us using:
	 * - mono_interp_set_resume_state ()/run_finally ()/run_filter ().
	 * Since ctx.ip is 0, this will start unwinding from the LMF frame
	 * pushed above, which points to our frames.
	 */
	mono_handle_exception (&ctx, (MonoObject*)ex);
	if (MONO_CONTEXT_GET_IP (&ctx) != 0) {
		/* We need to unwind into non-interpreter code */
		mono_restore_context (&ctx);
		g_assert_not_reached ();
	}

	interp_pop_lmf (&ext);

	g_assert (context->has_resume_state);
}

static void
fill_in_trace (MonoException *exception, InterpFrame *frame)
{
	ERROR_DECL (error);
	char *stack_trace = dump_frame (frame);
	MonoDomain *domain = frame->imethod->domain;
	(exception)->stack_trace = mono_string_new_checked (domain, stack_trace, error);
	mono_error_cleanup (error); /* FIXME: don't swallow the error */
	(exception)->trace_ips = get_trace_ips (domain, frame);
	g_free (stack_trace);
}

#define FILL_IN_TRACE(exception, frame) fill_in_trace(exception, frame)

#define THROW_EX_GENERAL(exception,ex_ip, rethrow)		\
	do {							\
		interp_throw (context, (exception), (frame), (ex_ip), (rethrow)); \
		if (frame == context->handler_frame) \
			SET_RESUME_STATE (context); \
		else \
			goto exit_frame; \
	} while (0)

#define THROW_EX(exception,ex_ip) THROW_EX_GENERAL ((exception), (ex_ip), FALSE)

/*
 * Its possible for child_frame.ex to contain an unthrown exception, if the transform phase
 * produced one.
 */
#define CHECK_CHILD_EX(child_frame, ip) do { \
	if ((child_frame).ex) \
		THROW_EX ((child_frame).ex, (ip)); \
	} while (0)

#define EXCEPTION_CHECKPOINT	\
	do {										\
		if (*mono_thread_interruption_request_flag () && !mono_threads_is_critical_method (rtm->method)) { \
			MonoException *exc = mono_thread_interruption_checkpoint ();	\
			if (exc)							\
				THROW_EX (exc, ip);					\
		}									\
	} while (0)


static MonoObject*
ves_array_create (MonoDomain *domain, MonoClass *klass, int param_count, stackval *values, MonoError *error)
{
	uintptr_t *lengths;
	intptr_t *lower_bounds;
	MonoObject *obj;
	int i;

	lengths = g_newa (uintptr_t, m_class_get_rank (klass) * 2);
	for (i = 0; i < param_count; ++i) {
		lengths [i] = values->data.i;
		values ++;
	}
	if (m_class_get_rank (klass) == param_count) {
		/* Only lengths provided. */
		lower_bounds = NULL;
	} else {
		/* lower bounds are first. */
		lower_bounds = (intptr_t *) lengths;
		lengths += m_class_get_rank (klass);
	}
	obj = (MonoObject*) mono_array_new_full_checked (domain, klass, lengths, lower_bounds, error);
	return obj;
}

static gint32
ves_array_calculate_index (MonoArray *ao, stackval *sp, InterpFrame *frame, gboolean safe)
{
	g_assert (!frame->ex);
	MonoClass *ac = ((MonoObject *) ao)->vtable->klass;

	guint32 pos = 0;
	if (ao->bounds) {
		for (gint32 i = 0; i < m_class_get_rank (ac); i++) {
			guint32 idx = sp [i].data.i;
			guint32 lower = ao->bounds [i].lower_bound;
			guint32 len = ao->bounds [i].length;
			if (safe && (idx < lower || (idx - lower) >= len)) {
				frame->ex = mono_get_exception_index_out_of_range ();
				FILL_IN_TRACE (frame->ex, frame);
				return -1;
			}
			pos = (pos * len) + idx - lower;
		}
	} else {
		pos = sp [0].data.i;
		if (safe && pos >= ao->max_length) {
			frame->ex = mono_get_exception_index_out_of_range ();
			FILL_IN_TRACE (frame->ex, frame);
			return -1;
		}
	}
	return pos;
}

static void
ves_array_set (InterpFrame *frame, stackval *sp, MonoMethodSignature *sig)
{
	MonoObject *o = sp->data.o;
	MonoArray *ao = (MonoArray *) o;
	MonoClass *ac = o->vtable->klass;

	g_assert (m_class_get_rank (ac) >= 1);

	gint32 pos = ves_array_calculate_index (ao, sp + 1, frame, TRUE);
	if (frame->ex)
		return;

	int val_index = 1 + m_class_get_rank (ac);
	if (sp [val_index].data.p && !m_class_is_valuetype (m_class_get_element_class (mono_object_class (o)))) {
		ERROR_DECL (error);
		MonoObject *isinst = mono_object_isinst_checked (sp [val_index].data.o, m_class_get_element_class (mono_object_class (o)), error);
		mono_error_cleanup (error);
		if (!isinst) {
			frame->ex = mono_get_exception_array_type_mismatch ();
			FILL_IN_TRACE (frame->ex, frame);
			return;
		}
	}

	gint32 esize = mono_array_element_size (ac);
	gpointer ea = mono_array_addr_with_size_fast (ao, esize, pos);

	MonoType *mt = sig->params [m_class_get_rank (ac)];
	stackval_to_data (mt, &sp [val_index], ea, FALSE);
}

static void
ves_array_get (InterpFrame *frame, stackval *sp, stackval *retval, MonoMethodSignature *sig, gboolean safe)
{
	MonoObject *o = sp->data.o;
	MonoArray *ao = (MonoArray *) o;
	MonoClass *ac = o->vtable->klass;

	g_assert (m_class_get_rank (ac) >= 1);

	gint32 pos = ves_array_calculate_index (ao, sp + 1, frame, safe);
	if (frame->ex)
		return;

	gint32 esize = mono_array_element_size (ac);
	gpointer ea = mono_array_addr_with_size_fast (ao, esize, pos);

	MonoType *mt = sig->ret;
	stackval_from_data (mt, retval, ea, FALSE);
}

static gpointer
ves_array_element_address (InterpFrame *frame, MonoClass *required_type, MonoArray *ao, stackval *sp, gboolean needs_typecheck)
{
	MonoClass *ac = ((MonoObject *) ao)->vtable->klass;

	g_assert (m_class_get_rank (ac) >= 1);

	gint32 pos = ves_array_calculate_index (ao, sp, frame, TRUE);
	if (frame->ex)
		return NULL;

	if (needs_typecheck && !mono_class_is_assignable_from (m_class_get_element_class (mono_object_class ((MonoObject *) ao)), m_class_get_element_class (required_type))) {
		frame->ex = mono_get_exception_array_type_mismatch ();
		FILL_IN_TRACE (frame->ex, frame);
		return NULL;
	}
	gint32 esize = mono_array_element_size (ac);
	return mono_array_addr_with_size_fast (ao, esize, pos);
}

static MonoPIFunc mono_interp_to_native_trampoline = NULL;

#ifdef MONO_ARCH_HAVE_INTERP_ENTRY_TRAMPOLINE
static MonoFuncV mono_native_to_interp_trampoline = NULL;
#endif

#ifndef MONO_ARCH_HAVE_INTERP_PINVOKE_TRAMP
static InterpMethodArguments* build_args_from_sig (MonoMethodSignature *sig, InterpFrame *frame)
{
	InterpMethodArguments *margs = g_malloc0 (sizeof (InterpMethodArguments));

#ifdef TARGET_ARM
	g_assert (mono_arm_eabi_supported ());
	int i8_align = mono_arm_i8_align ();
#endif

#ifdef TARGET_WASM
	margs->sig = sig;
#endif

	if (sig->hasthis)
		margs->ilen++;

	for (int i = 0; i < sig->param_count; i++) {
		guint32 ptype = sig->params [i]->byref ? MONO_TYPE_PTR : sig->params [i]->type;
		switch (ptype) {
		case MONO_TYPE_BOOLEAN:
		case MONO_TYPE_CHAR:
		case MONO_TYPE_I1:
		case MONO_TYPE_U1:
		case MONO_TYPE_I2:
		case MONO_TYPE_U2:
		case MONO_TYPE_I4:
		case MONO_TYPE_U4:
		case MONO_TYPE_I:
		case MONO_TYPE_U:
		case MONO_TYPE_PTR:
		case MONO_TYPE_SZARRAY:
		case MONO_TYPE_CLASS:
		case MONO_TYPE_OBJECT:
		case MONO_TYPE_STRING:
		case MONO_TYPE_VALUETYPE:
		case MONO_TYPE_GENERICINST:
#if SIZEOF_VOID_P == 8
		case MONO_TYPE_I8:
		case MONO_TYPE_U8:
#endif
			margs->ilen++;
			break;
#if SIZEOF_VOID_P == 4
		case MONO_TYPE_I8:
		case MONO_TYPE_U8:
#ifdef TARGET_ARM
			/* pairs begin at even registers */
			if (i8_align == 8 && margs->ilen & 1)
				margs->ilen++;
#endif
			margs->ilen += 2;
			break;
#endif
		case MONO_TYPE_R4:
#if SIZEOF_VOID_P == 8
		case MONO_TYPE_R8:
#endif
			margs->flen++;
			break;
#if SIZEOF_VOID_P == 4
		case MONO_TYPE_R8:
			margs->flen += 2;
			break;
#endif
		default:
			g_error ("build_args_from_sig: not implemented yet (1): 0x%x\n", ptype);
		}
	}

	if (margs->ilen > 0)
		margs->iargs = g_malloc0 (sizeof (gpointer) * margs->ilen);

	if (margs->flen > 0)
		margs->fargs = g_malloc0 (sizeof (double) * margs->flen);

	if (margs->ilen > INTERP_ICALL_TRAMP_IARGS)
		g_error ("build_args_from_sig: TODO, allocate gregs: %d\n", margs->ilen);

	if (margs->flen > INTERP_ICALL_TRAMP_FARGS)
		g_error ("build_args_from_sig: TODO, allocate fregs: %d\n", margs->flen);


	size_t int_i = 0;
	size_t int_f = 0;

	if (sig->hasthis) {
		margs->iargs [0] = frame->stack_args->data.p;
		int_i++;
	}

	for (int i = 0; i < sig->param_count; i++) {
		guint32 ptype = sig->params [i]->byref ? MONO_TYPE_PTR : sig->params [i]->type;
		switch (ptype) {
		case MONO_TYPE_BOOLEAN:
		case MONO_TYPE_CHAR:
		case MONO_TYPE_I1:
		case MONO_TYPE_U1:
		case MONO_TYPE_I2:
		case MONO_TYPE_U2:
		case MONO_TYPE_I4:
		case MONO_TYPE_U4:
		case MONO_TYPE_I:
		case MONO_TYPE_U:
		case MONO_TYPE_PTR:
		case MONO_TYPE_SZARRAY:
		case MONO_TYPE_CLASS:
		case MONO_TYPE_OBJECT:
		case MONO_TYPE_STRING:
		case MONO_TYPE_VALUETYPE:
		case MONO_TYPE_GENERICINST:
#if SIZEOF_VOID_P == 8
		case MONO_TYPE_I8:
		case MONO_TYPE_U8:
#endif
			margs->iargs [int_i] = frame->stack_args [i].data.p;
#if DEBUG_INTERP
			g_print ("build_args_from_sig: margs->iargs [%d]: %p (frame @ %d)\n", int_i, margs->iargs [int_i], i);
#endif
			int_i++;
			break;
#if SIZEOF_VOID_P == 4
		case MONO_TYPE_I8:
		case MONO_TYPE_U8: {
			stackval *sarg = &frame->stack_args [i];
#ifdef TARGET_ARM
			/* pairs begin at even registers */
			if (i8_align == 8 && int_i & 1)
				int_i++;
#endif
			margs->iargs [int_i] = (gpointer) sarg->data.pair.lo;
			int_i++;
			margs->iargs [int_i] = (gpointer) sarg->data.pair.hi;
#if DEBUG_INTERP
			g_print ("build_args_from_sig: margs->iargs [%d/%d]: 0x%016llx, hi=0x%08x lo=0x%08x (frame @ %d)\n", int_i - 1, int_i, *((guint64 *) &margs->iargs [int_i - 1]), sarg->data.pair.hi, sarg->data.pair.lo, i);
#endif
			int_i++;
			break;
		}
#endif
		case MONO_TYPE_R4:
		case MONO_TYPE_R8:
			if (ptype == MONO_TYPE_R4)
				* (float *) &(margs->fargs [int_f]) = (float) frame->stack_args [i].data.f;
			else
				margs->fargs [int_f] = frame->stack_args [i].data.f;
#if DEBUG_INTERP
			g_print ("build_args_from_sig: margs->fargs [%d]: %p (%f) (frame @ %d)\n", int_f, margs->fargs [int_f], margs->fargs [int_f], i);
#endif
#if SIZEOF_VOID_P == 4
			int_f += 2;
#else
			int_f++;
#endif
			break;
		default:
			g_error ("build_args_from_sig: not implemented yet (2): 0x%x\n", ptype);
		}
	}

	switch (sig->ret->type) {
		case MONO_TYPE_BOOLEAN:
		case MONO_TYPE_CHAR:
		case MONO_TYPE_I1:
		case MONO_TYPE_U1:
		case MONO_TYPE_I2:
		case MONO_TYPE_U2:
		case MONO_TYPE_I4:
		case MONO_TYPE_U4:
		case MONO_TYPE_I:
		case MONO_TYPE_U:
		case MONO_TYPE_PTR:
		case MONO_TYPE_SZARRAY:
		case MONO_TYPE_CLASS:
		case MONO_TYPE_OBJECT:
		case MONO_TYPE_STRING:
		case MONO_TYPE_I8:
		case MONO_TYPE_U8:
		case MONO_TYPE_VALUETYPE:
		case MONO_TYPE_GENERICINST:
			margs->retval = &(frame->retval->data.p);
			margs->is_float_ret = 0;
			break;
		case MONO_TYPE_R4:
		case MONO_TYPE_R8:
			margs->retval = &(frame->retval->data.p);
			margs->is_float_ret = 1;
			break;
		case MONO_TYPE_VOID:
			margs->retval = NULL;
			break;
		default:
			g_error ("build_args_from_sig: ret type not implemented yet: 0x%x\n", sig->ret->type);
	}

	return margs;
}
#endif

static void
interp_frame_arg_to_data (MonoInterpFrameHandle frame, MonoMethodSignature *sig, int index, gpointer data)
{
	InterpFrame *iframe = (InterpFrame*)frame;

	if (index == -1)
		stackval_to_data (sig->ret, iframe->retval, data, sig->pinvoke);
	else
		stackval_to_data (sig->params [index], &iframe->stack_args [index], data, sig->pinvoke);
}

static void
interp_data_to_frame_arg (MonoInterpFrameHandle frame, MonoMethodSignature *sig, int index, gpointer data)
{
	InterpFrame *iframe = (InterpFrame*)frame;

	if (index == -1)
		stackval_from_data (sig->ret, iframe->retval, data, sig->pinvoke);
	else if (sig->hasthis && index == 0)
		iframe->stack_args [index].data.p = *(gpointer*)data;
	else
		stackval_from_data (sig->params [index - sig->hasthis], &iframe->stack_args [index], data, sig->pinvoke);
}

static gpointer
interp_frame_arg_to_storage (MonoInterpFrameHandle frame, MonoMethodSignature *sig, int index)
{
	InterpFrame *iframe = (InterpFrame*)frame;

	if (index == -1)
		return stackval_to_data_addr (sig->ret, iframe->retval);
	else
		return stackval_to_data_addr (sig->params [index], &iframe->stack_args [index]);
}

static void
interp_frame_arg_set_storage (MonoInterpFrameHandle frame, MonoMethodSignature *sig, int index, gpointer storage)
{
	InterpFrame *iframe = (InterpFrame*)frame;
	stackval *val = (index == -1) ? iframe->retval : &iframe->stack_args [index];
	MonoType *type = (index == -1) ? sig->ret : sig->params [index];

	switch (type->type) {
	case MONO_TYPE_GENERICINST:
		if (!MONO_TYPE_IS_REFERENCE (type))
			val->data.vt = storage;
		break;
	case MONO_TYPE_VALUETYPE:
		val->data.vt = storage;
		break;
	default:
		g_assert_not_reached ();
	}
}

/* MONO_NO_OPTIMIATION is needed due to usage of INTERP_PUSH_LMF_WITH_CTX. */
static MONO_NO_OPTIMIZATION MONO_NEVER_INLINE void
ves_pinvoke_method (InterpFrame *frame, MonoMethodSignature *sig, MonoFuncV addr, gboolean string_ctor, ThreadContext *context)
{
	MonoLMFExt ext;

	frame->ex = NULL;

	g_assert (!frame->imethod);
	if (!mono_interp_to_native_trampoline) {
		if (mono_ee_features.use_aot_trampolines) {
			mono_interp_to_native_trampoline = (MonoPIFunc)mono_aot_get_trampoline ("interp_to_native_trampoline");
		} else {
			MonoTrampInfo *info;
			mono_interp_to_native_trampoline = (MonoPIFunc)mono_arch_get_interp_to_native_trampoline (&info);
			// TODO:
			// mono_tramp_info_register (info, NULL);
		}
	}
#ifdef MONO_ARCH_HAVE_INTERP_PINVOKE_TRAMP
	CallContext ccontext;
	mono_arch_set_native_call_context_args (&ccontext, frame, sig);
#else
	InterpMethodArguments *margs = build_args_from_sig (sig, frame);
#endif

#if DEBUG_INTERP
	g_print ("ICALL: mono_interp_to_native_trampoline = %p, addr = %p\n", mono_interp_to_native_trampoline, addr);
#endif

	context->current_frame = frame;
	INTERP_PUSH_LMF_WITH_CTX (context->current_frame, ext, &&exit_pinvoke);
#ifdef MONO_ARCH_HAVE_INTERP_PINVOKE_TRAMP
	mono_interp_to_native_trampoline (addr, &ccontext);
#else
	mono_interp_to_native_trampoline (addr, margs);
#endif
	interp_pop_lmf (&ext);

#ifdef MONO_ARCH_HAVE_INTERP_PINVOKE_TRAMP
	if (!frame->ex)
		mono_arch_get_native_call_context_ret (&ccontext, frame, sig);
#else
	if (!frame->ex && !MONO_TYPE_ISSTRUCT (sig->ret))
		stackval_from_data (sig->ret, frame->retval, (char*)&frame->retval->data.p, sig->pinvoke);

	g_free (margs->iargs);
	g_free (margs->fargs);
	g_free (margs);
#endif
exit_pinvoke:
	return;
}

/*
 * interp_init_delegate:
 *
 *   Initialize del->interp_method.
 */
static void
interp_init_delegate (MonoDelegate *del)
{
	MonoMethod *method;
	ERROR_DECL (error);

	if (del->interp_method) {
		/* Delegate created by a call to ves_icall_mono_delegate_ctor_interp () */
		del->method = ((InterpMethod *)del->interp_method)->method;
	} else if (del->method) {
		/* Delegate created dynamically */
		del->interp_method = mono_interp_get_imethod (del->object.vtable->domain, del->method, error);
	} else {
		/* Created from JITted code */
		g_assert (del->method_ptr);
		del->interp_method = lookup_method_pointer (del->method_ptr);
		g_assert (del->interp_method);
	}

	method = ((InterpMethod*)del->interp_method)->method;
	if (del->target &&
			method &&
			method->flags & METHOD_ATTRIBUTE_VIRTUAL &&
			method->flags & METHOD_ATTRIBUTE_ABSTRACT &&
			mono_class_is_abstract (method->klass))
		del->interp_method = get_virtual_method ((InterpMethod*)del->interp_method, del->target);

	method = ((InterpMethod*)del->interp_method)->method;
	if (method && m_class_get_parent (method->klass) == mono_defaults.multicastdelegate_class) {
		const char *name = method->name;
		if (*name == 'I' && (strcmp (name, "Invoke") == 0)) {
			/*
			 * When invoking the delegate interp_method is executed directly. If it's an
			 * invoke make sure we replace it with the appropriate delegate invoke wrapper.
			 *
			 * FIXME We should do this later, when we also know the delegate on which the
			 * target method is called.
			 */
			del->interp_method = mono_interp_get_imethod (del->object.vtable->domain, mono_marshal_get_delegate_invoke (method, NULL), error);
			mono_error_assert_ok (error);
		}
	}
}

static void
interp_delegate_ctor (MonoObjectHandle this_obj, MonoObjectHandle target, gpointer addr, MonoError *error)
{
	/*
	 * addr is the result of an LDFTN opcode, i.e. an InterpMethod
	 */
	InterpMethod *imethod = (InterpMethod*)addr;

	if (!(imethod->method->flags & METHOD_ATTRIBUTE_STATIC)) {
		MonoMethod *invoke = mono_get_delegate_invoke (mono_handle_class (this_obj));
		/* virtual invoke delegates must not have null check */
		if (mono_method_signature (imethod->method)->param_count == mono_method_signature (invoke)->param_count
				&& MONO_HANDLE_IS_NULL (target)) {
			mono_error_set_argument (error, "this", "Delegate to an instance method cannot have null 'this'");
			return;
		}
	}

	g_assert (imethod->method);
	gpointer entry = mini_get_interp_callbacks ()->create_method_pointer (imethod->method, FALSE, error);
	return_if_nok (error);

	MONO_HANDLE_SETVAL (MONO_HANDLE_CAST (MonoDelegate, this_obj), interp_method, gpointer, imethod);

	mono_delegate_ctor (this_obj, target, entry, error);
}

/*
 * From the spec:
 * runtime specifies that the implementation of the method is automatically
 * provided by the runtime and is primarily used for the methods of delegates.
 */
static MONO_NEVER_INLINE void
ves_imethod (InterpFrame *frame, MonoMethod *method, MonoMethodSignature *sig, stackval *sp, stackval *retval)
{
	const char *name = method->name;
	mono_class_init (method->klass);

	if (method->klass == mono_defaults.array_class) {
		if (!strcmp (name, "UnsafeMov")) {
			/* TODO: layout checks */
			stackval_from_data (sig->ret, retval, (char*) sp, FALSE);
			return;
		}
		if (!strcmp (name, "UnsafeLoad")) {
			ves_array_get (frame, sp, retval, sig, FALSE);
			return;
		}
	} else if (mini_class_is_system_array (method->klass)) {
		MonoObject *obj = (MonoObject*) sp->data.p;
		if (!obj) {
			frame->ex = mono_get_exception_null_reference ();
			FILL_IN_TRACE (frame->ex, frame);
			return;
		}
		if (*name == 'S' && (strcmp (name, "Set") == 0)) {
			ves_array_set (frame, sp, sig);
			return;
		}
		if (*name == 'G' && (strcmp (name, "Get") == 0)) {
			ves_array_get (frame, sp, retval, sig, TRUE);
			return;
		}
	}
	
	g_error ("Don't know how to exec runtime method %s.%s::%s", 
			m_class_get_name_space (method->klass), m_class_get_name (method->klass),
			method->name);
}

#if DEBUG_INTERP
static char*
dump_stack (stackval *stack, stackval *sp)
{
	stackval *s = stack;
	GString *str = g_string_new ("");
	
	if (sp == stack)
		return g_string_free (str, FALSE);
	
	while (s < sp) {
		g_string_append_printf (str, "[%p (%lld)] ", s->data.l, s->data.l);
		++s;
	}
	return g_string_free (str, FALSE);
}
#endif

static void
dump_stackval (GString *str, stackval *s, MonoType *type)
{
	switch (type->type) {
	case MONO_TYPE_I1:
	case MONO_TYPE_U1:
	case MONO_TYPE_I2:
	case MONO_TYPE_U2:
	case MONO_TYPE_I4:
	case MONO_TYPE_U4:
	case MONO_TYPE_CHAR:
	case MONO_TYPE_BOOLEAN:
		g_string_append_printf (str, "[%d] ", s->data.i);
		break;
	case MONO_TYPE_STRING:
	case MONO_TYPE_SZARRAY:
	case MONO_TYPE_CLASS:
	case MONO_TYPE_OBJECT:
	case MONO_TYPE_ARRAY:
	case MONO_TYPE_PTR:
	case MONO_TYPE_I:
	case MONO_TYPE_U:
		g_string_append_printf (str, "[%p] ", s->data.p);
		break;
	case MONO_TYPE_VALUETYPE:
		if (m_class_is_enumtype (type->data.klass))
			g_string_append_printf (str, "[%d] ", s->data.i);
		else
			g_string_append_printf (str, "[vt:%p] ", s->data.p);
		break;
	case MONO_TYPE_R4:
	case MONO_TYPE_R8:
		g_string_append_printf (str, "[%g] ", s->data.f);
		break;
	case MONO_TYPE_I8:
	case MONO_TYPE_U8:
	default: {
		GString *res = g_string_new ("");
		mono_type_get_desc (res, type, TRUE);
		g_string_append_printf (str, "[{%s} %lld/0x%0llx] ", res->str, s->data.l, s->data.l);
		g_string_free (res, TRUE);
		break;
	}
	}
}

#if DEBUG_INTERP
static char*
dump_retval (InterpFrame *inv)
{
	GString *str = g_string_new ("");
	MonoType *ret = mono_method_signature (inv->imethod->method)->ret;

	if (ret->type != MONO_TYPE_VOID)
		dump_stackval (str, inv->retval, ret);

	return g_string_free (str, FALSE);
}
#endif

static char*
dump_args (InterpFrame *inv)
{
	GString *str = g_string_new ("");
	int i;
	MonoMethodSignature *signature = mono_method_signature (inv->imethod->method);
	
	if (signature->param_count == 0 && !signature->hasthis)
		return g_string_free (str, FALSE);

	if (signature->hasthis) {
		MonoMethod *method = inv->imethod->method;
		dump_stackval (str, inv->stack_args, m_class_get_byval_arg (method->klass));
	}

	for (i = 0; i < signature->param_count; ++i)
		dump_stackval (str, inv->stack_args + (!!signature->hasthis) + i, signature->params [i]);

	return g_string_free (str, FALSE);
}
 
static char*
dump_frame (InterpFrame *inv)
{
	GString *str = g_string_new ("");
	int i;
	char *args;
	ERROR_DECL (error);

	for (i = 0; inv; inv = inv->parent) {
		if (inv->imethod != NULL) {
			MonoMethod *method = inv->imethod->method;

			int codep = 0;
			const char * opname = "";
			char *name;
			gchar *source = NULL;

			if ((method->flags & METHOD_ATTRIBUTE_PINVOKE_IMPL) == 0 &&
				(method->iflags & METHOD_IMPL_ATTRIBUTE_RUNTIME) == 0) {
				MonoMethodHeader *hd = mono_method_get_header_checked (method, error);
				mono_error_cleanup (error); /* FIXME: don't swallow the error */

				if (hd != NULL) {
					if (inv->ip) {
						opname = mono_interp_opname [*inv->ip];
						codep = inv->ip - inv->imethod->code;
						source = g_strdup_printf ("%s:%d // (TODO: proper stacktrace)", method->name, codep);
					} else 
						opname = "";

#if 0
					MonoDebugSourceLocation *minfo = mono_debug_lookup_method (method);
					source = mono_debug_method_lookup_location (minfo, codep);
#endif
					mono_metadata_free_mh (hd);
				}
			}
			args = dump_args (inv);
			name = mono_method_full_name (method, TRUE);
			if (source)
				g_string_append_printf (str, "#%d: 0x%05x %-10s in %s (%s) at %s\n", i, codep, opname, name, args, source);
			else
				g_string_append_printf (str, "#%d: 0x%05x %-10s in %s (%s)\n", i, codep, opname, name, args);
			g_free (name);
			g_free (args);
			g_free (source);
			++i;
		}
	}
	return g_string_free (str, FALSE);
}

static MonoArray *
get_trace_ips (MonoDomain *domain, InterpFrame *top)
{
	int i;
	MonoArray *res;
	InterpFrame *inv;
	ERROR_DECL (error);

	for (i = 0, inv = top; inv; inv = inv->parent)
		if (inv->imethod != NULL)
			++i;

	res = mono_array_new_checked (domain, mono_defaults.int_class, 3 * i, error);
	mono_error_cleanup (error); /* FIXME: don't swallow the error */

	for (i = 0, inv = top; inv; inv = inv->parent)
		if (inv->imethod != NULL) {
			mono_array_set_fast (res, gpointer, i, inv->imethod);
			++i;
			mono_array_set_fast (res, gpointer, i, (gpointer)inv->ip);
			++i;
			mono_array_set_fast (res, gpointer, i, NULL);
			++i;
		}

	return res;
}

#define CHECK_ADD_OVERFLOW(a,b) \
	(gint32)(b) >= 0 ? (gint32)(G_MAXINT32) - (gint32)(b) < (gint32)(a) ? -1 : 0	\
	: (gint32)(G_MININT32) - (gint32)(b) > (gint32)(a) ? +1 : 0

#define CHECK_SUB_OVERFLOW(a,b) \
	(gint32)(b) < 0 ? (gint32)(G_MAXINT32) + (gint32)(b) < (gint32)(a) ? -1 : 0	\
	: (gint32)(G_MININT32) + (gint32)(b) > (gint32)(a) ? +1 : 0

#define CHECK_ADD_OVERFLOW_UN(a,b) \
	(guint32)(G_MAXUINT32) - (guint32)(b) < (guint32)(a) ? -1 : 0

#define CHECK_SUB_OVERFLOW_UN(a,b) \
	(guint32)(a) < (guint32)(b) ? -1 : 0

#define CHECK_ADD_OVERFLOW64(a,b) \
	(gint64)(b) >= 0 ? (gint64)(G_MAXINT64) - (gint64)(b) < (gint64)(a) ? -1 : 0	\
	: (gint64)(G_MININT64) - (gint64)(b) > (gint64)(a) ? +1 : 0

#define CHECK_SUB_OVERFLOW64(a,b) \
	(gint64)(b) < 0 ? (gint64)(G_MAXINT64) + (gint64)(b) < (gint64)(a) ? -1 : 0	\
	: (gint64)(G_MININT64) + (gint64)(b) > (gint64)(a) ? +1 : 0

#define CHECK_ADD_OVERFLOW64_UN(a,b) \
	(guint64)(G_MAXUINT64) - (guint64)(b) < (guint64)(a) ? -1 : 0

#define CHECK_SUB_OVERFLOW64_UN(a,b) \
	(guint64)(a) < (guint64)(b) ? -1 : 0

#if SIZEOF_VOID_P == 4
#define CHECK_ADD_OVERFLOW_NAT(a,b) CHECK_ADD_OVERFLOW(a,b)
#define CHECK_ADD_OVERFLOW_NAT_UN(a,b) CHECK_ADD_OVERFLOW_UN(a,b)
#else
#define CHECK_ADD_OVERFLOW_NAT(a,b) CHECK_ADD_OVERFLOW64(a,b)
#define CHECK_ADD_OVERFLOW_NAT_UN(a,b) CHECK_ADD_OVERFLOW64_UN(a,b)
#endif

/* Resolves to TRUE if the operands would overflow */
#define CHECK_MUL_OVERFLOW(a,b) \
	((gint32)(a) == 0) || ((gint32)(b) == 0) ? 0 : \
	(((gint32)(a) > 0) && ((gint32)(b) == -1)) ? FALSE : \
	(((gint32)(a) < 0) && ((gint32)(b) == -1)) ? (a == G_MININT32) : \
	(((gint32)(a) > 0) && ((gint32)(b) > 0)) ? (gint32)(a) > ((G_MAXINT32) / (gint32)(b)) : \
	(((gint32)(a) > 0) && ((gint32)(b) < 0)) ? (gint32)(a) > ((G_MININT32) / (gint32)(b)) : \
	(((gint32)(a) < 0) && ((gint32)(b) > 0)) ? (gint32)(a) < ((G_MININT32) / (gint32)(b)) : \
	(gint32)(a) < ((G_MAXINT32) / (gint32)(b))

#define CHECK_MUL_OVERFLOW_UN(a,b) \
	((guint32)(a) == 0) || ((guint32)(b) == 0) ? 0 : \
	(guint32)(b) > ((G_MAXUINT32) / (guint32)(a))

#define CHECK_MUL_OVERFLOW64(a,b) \
	((gint64)(a) == 0) || ((gint64)(b) == 0) ? 0 : \
	(((gint64)(a) > 0) && ((gint64)(b) == -1)) ? FALSE : \
	(((gint64)(a) < 0) && ((gint64)(b) == -1)) ? (a == G_MININT64) : \
	(((gint64)(a) > 0) && ((gint64)(b) > 0)) ? (gint64)(a) > ((G_MAXINT64) / (gint64)(b)) : \
	(((gint64)(a) > 0) && ((gint64)(b) < 0)) ? (gint64)(a) > ((G_MININT64) / (gint64)(b)) : \
	(((gint64)(a) < 0) && ((gint64)(b) > 0)) ? (gint64)(a) < ((G_MININT64) / (gint64)(b)) : \
	(gint64)(a) < ((G_MAXINT64) / (gint64)(b))

#define CHECK_MUL_OVERFLOW64_UN(a,b) \
	((guint64)(a) == 0) || ((guint64)(b) == 0) ? 0 : \
	(guint64)(b) > ((G_MAXUINT64) / (guint64)(a))

#if SIZEOF_VOID_P == 4
#define CHECK_MUL_OVERFLOW_NAT(a,b) CHECK_MUL_OVERFLOW(a,b)
#define CHECK_MUL_OVERFLOW_NAT_UN(a,b) CHECK_MUL_OVERFLOW_UN(a,b)
#else
#define CHECK_MUL_OVERFLOW_NAT(a,b) CHECK_MUL_OVERFLOW64(a,b)
#define CHECK_MUL_OVERFLOW_NAT_UN(a,b) CHECK_MUL_OVERFLOW64_UN(a,b)
#endif

static MonoObject*
interp_runtime_invoke (MonoMethod *method, void *obj, void **params, MonoObject **exc, MonoError *error)
{
	InterpFrame frame, *old_frame;
	ThreadContext *context = get_context ();
	MonoMethodSignature *sig = mono_method_signature (method);
	MonoClass *klass = mono_class_from_mono_type (sig->ret);
	stackval result;
	MonoMethod *target_method = method;

	error_init (error);
	if (exc)
		*exc = NULL;

	frame.ex = NULL;

	old_frame = context->current_frame;

	MonoDomain *domain = mono_domain_get ();

	if (method->flags & METHOD_ATTRIBUTE_PINVOKE_IMPL)
		target_method = mono_marshal_get_native_wrapper (target_method, FALSE, FALSE);
	MonoMethod *invoke_wrapper = mono_marshal_get_runtime_invoke_full (target_method, FALSE, TRUE);

	//* <code>MonoObject *runtime_invoke (MonoObject *this_obj, void **params, MonoObject **exc, void* method)</code>

	result.data.vt = alloca (mono_class_instance_size (klass));
	stackval args [4];

	if (sig->hasthis)
		args [0].data.p = obj;
	else
		args [0].data.p = NULL;
	args [1].data.p = params;
	args [2].data.p = exc;
	args [3].data.p = target_method;

	INIT_FRAME (&frame, NULL, args, &result, domain, invoke_wrapper, error);

	if (exc)
		frame.invoke_trap = 1;

	interp_exec_method (&frame, context);

	context->current_frame = old_frame;

	if (frame.ex) {
		if (exc) {
			*exc = (MonoObject*) frame.ex;
			return NULL;
		}
		mono_error_set_exception_instance (error, frame.ex);
		return NULL;
	}
	return (MonoObject*)result.data.p;
}

typedef struct {
	InterpMethod *rmethod;
	gpointer this_arg;
	gpointer res;
	gpointer args [16];
	gpointer *many_args;
} InterpEntryData;

/* Main function for entering the interpreter from compiled code */
static void
interp_entry (InterpEntryData *data)
{
	InterpFrame frame;
	InterpMethod *rmethod = data->rmethod;
	ThreadContext *context;
	InterpFrame *old_frame;
	stackval result;
	stackval *args;
	MonoMethod *method;
	MonoMethodSignature *sig;
	MonoType *type;
	gpointer orig_domain, attach_cookie;
	int i;

	if (rmethod->needs_thread_attach)
		orig_domain = mono_threads_attach_coop (mono_domain_get (), &attach_cookie);

	context = get_context ();

	method = rmethod->method;
	sig = mono_method_signature (method);

	// FIXME: Optimize this

	//printf ("%s\n", mono_method_full_name (method, 1));

	frame.ex = NULL;
	old_frame = context->current_frame;

	args = g_newa (stackval, sig->param_count + (sig->hasthis ? 1 : 0));
	if (sig->hasthis)
		args [0].data.p = data->this_arg;

	gpointer *params;
	if (data->many_args)
		params = data->many_args;
	else
		params = data->args;
	for (i = 0; i < sig->param_count; ++i) {
		int a_index = i + (sig->hasthis ? 1 : 0);
		if (sig->params [i]->byref) {
			args [a_index].data.p = params [i];
			continue;
		}
		type = rmethod->param_types [i];
		switch (type->type) {
		case MONO_TYPE_VALUETYPE:
			args [a_index].data.p = params [i];
			break;
		case MONO_TYPE_GENERICINST:
			if (MONO_TYPE_IS_REFERENCE (type))
				args [a_index].data.p = *(gpointer*)params [i];
			else
				args [a_index].data.vt = params [i];
			break;
		default:
			stackval_from_data (type, &args [a_index], params [i], FALSE);
			break;
		}
	}

	memset (&result, 0, sizeof (result));
	init_frame (&frame, NULL, data->rmethod, args, &result);

	type = rmethod->rtype;
	switch (type->type) {
	case MONO_TYPE_GENERICINST:
		if (!MONO_TYPE_IS_REFERENCE (type))
			frame.retval->data.vt = data->res;
		break;
	case MONO_TYPE_VALUETYPE:
		frame.retval->data.vt = data->res;
		break;
	default:
		break;
	}

	interp_exec_method (&frame, context);
	context->current_frame = old_frame;

	if (rmethod->needs_thread_attach)
		mono_threads_detach_coop (orig_domain, &attach_cookie);

	// FIXME:
	g_assert (frame.ex == NULL);

	type = rmethod->rtype;
	switch (type->type) {
	case MONO_TYPE_VOID:
		break;
	case MONO_TYPE_OBJECT:
		/* No need for a write barrier */
		*(MonoObject**)data->res = (MonoObject*)frame.retval->data.p;
		break;
	case MONO_TYPE_GENERICINST:
		if (MONO_TYPE_IS_REFERENCE (type)) {
			*(MonoObject**)data->res = (MonoObject*)frame.retval->data.p;
		} else {
			/* Already set before the call */
		}
		break;
	case MONO_TYPE_VALUETYPE:
		/* Already set before the call */
		break;
	default:
		stackval_to_data (type, frame.retval, data->res, FALSE);
		break;
	}
}

/* MONO_NO_OPTIMIATION is needed due to usage of INTERP_PUSH_LMF_WITH_CTX. */
static MONO_NO_OPTIMIZATION MONO_NEVER_INLINE stackval *
do_icall (ThreadContext *context, MonoMethodSignature *sig, int op, stackval *sp, gpointer ptr)
{
	MonoLMFExt ext;
	INTERP_PUSH_LMF_WITH_CTX (context->current_frame, ext, &&exit_icall);

	switch (op) {
	case MINT_ICALL_V_V: {
		typedef void (*T)(void);
		T func = (T)ptr;
        	func ();
		break;
	}
	case MINT_ICALL_V_P: {
		typedef gpointer (*T)(void);
		T func = (T)ptr;
		sp++;
		sp [-1].data.p = func ();
		break;
	}
	case MINT_ICALL_P_V: {
		typedef void (*T)(gpointer);
		T func = (T)ptr;
        	func (sp [-1].data.p);
		sp --;
		break;
	}
	case MINT_ICALL_P_P: {
		typedef gpointer (*T)(gpointer);
		T func = (T)ptr;
		sp [-1].data.p = func (sp [-1].data.p);
		break;
	}
	case MINT_ICALL_PP_V: {
		typedef void (*T)(gpointer,gpointer);
		T func = (T)ptr;
		sp -= 2;
		func (sp [0].data.p, sp [1].data.p);
		break;
	}
	case MINT_ICALL_PP_P: {
		typedef gpointer (*T)(gpointer,gpointer);
		T func = (T)ptr;
		--sp;
		sp [-1].data.p = func (sp [-1].data.p, sp [0].data.p);
		break;
	}
	case MINT_ICALL_PPP_V: {
		typedef void (*T)(gpointer,gpointer,gpointer);
		T func = (T)ptr;
		sp -= 3;
		func (sp [0].data.p, sp [1].data.p, sp [2].data.p);
		break;
	}
	case MINT_ICALL_PPP_P: {
		typedef gpointer (*T)(gpointer,gpointer,gpointer);
		T func = (T)ptr;
		sp -= 2;
		sp [-1].data.p = func (sp [-1].data.p, sp [0].data.p, sp [1].data.p);
		break;
	}
	case MINT_ICALL_PPPP_V: {
		typedef void (*T)(gpointer,gpointer,gpointer,gpointer);
		T func = (T)ptr;
		sp -= 4;
		func (sp [0].data.p, sp [1].data.p, sp [2].data.p, sp [3].data.p);
		break;
	}
	case MINT_ICALL_PPPP_P: {
		typedef gpointer (*T)(gpointer,gpointer,gpointer,gpointer);
		T func = (T)ptr;
		sp -= 3;
		sp [-1].data.p = func (sp [-1].data.p, sp [0].data.p, sp [1].data.p, sp [2].data.p);
		break;
	}
	case MINT_ICALL_PPPPP_V: {
		typedef void (*T)(gpointer,gpointer,gpointer,gpointer,gpointer);
		T func = (T)ptr;
		sp -= 5;
		func (sp [0].data.p, sp [1].data.p, sp [2].data.p, sp [3].data.p, sp [4].data.p);
		break;
	}
	case MINT_ICALL_PPPPP_P: {
		typedef gpointer (*T)(gpointer,gpointer,gpointer,gpointer,gpointer);
		T func = (T)ptr;
		sp -= 4;
		sp [-1].data.p = func (sp [-1].data.p, sp [0].data.p, sp [1].data.p, sp [2].data.p, sp [3].data.p);
		break;
	}
	case MINT_ICALL_PPPPPP_V: {
		typedef void (*T)(gpointer,gpointer,gpointer,gpointer,gpointer,gpointer);
		T func = (T)ptr;
		sp -= 6;
		func (sp [0].data.p, sp [1].data.p, sp [2].data.p, sp [3].data.p, sp [4].data.p, sp [5].data.p);
		break;
	}
	case MINT_ICALL_PPPPPP_P: {
		typedef gpointer (*T)(gpointer,gpointer,gpointer,gpointer,gpointer,gpointer);
		T func = (T)ptr;
		sp -= 5;
		sp [-1].data.p = func (sp [-1].data.p, sp [0].data.p, sp [1].data.p, sp [2].data.p, sp [3].data.p, sp [4].data.p);
		break;
	}
	default:
		g_assert_not_reached ();
	}

	/* convert the native representation to the stackval representation */
	if (sig)
		stackval_from_data (sig->ret, &sp [-1], (char*) &sp [-1].data.p, sig->pinvoke);

	interp_pop_lmf (&ext);
exit_icall:
	return sp;
}

static MONO_NEVER_INLINE stackval *
do_jit_call (stackval *sp, unsigned char *vt_sp, ThreadContext *context, InterpFrame *frame, InterpMethod *rmethod, MonoError *error)
{
	MonoMethodSignature *sig;
	MonoFtnDesc ftndesc;
	guint8 res_buf [256];
	MonoType *type;
	MonoLMFExt ext;

	//printf ("jit_call: %s\n", mono_method_full_name (rmethod->method, 1));

	/*
	 * Call JITted code through a gsharedvt_out wrapper. These wrappers receive every argument
	 * by ref and return a return value using an explicit return value argument.
	 */
	if (!rmethod->jit_wrapper) {
		MonoMethod *method = rmethod->method;

		sig = mono_method_signature (method);
		g_assert (sig);

		MonoMethod *wrapper = mini_get_gsharedvt_out_sig_wrapper (sig);
		//printf ("J: %s %s\n", mono_method_full_name (method, 1), mono_method_full_name (wrapper, 1));

		gpointer jit_wrapper = mono_jit_compile_method_jit_only (wrapper, error);
		mono_error_assert_ok (error);

		gpointer addr = mono_jit_compile_method_jit_only (method, error);
		return_val_if_nok (error, NULL);
		g_assert (addr);

		rmethod->jit_addr = addr;
		rmethod->jit_sig = sig;
		mono_memory_barrier ();
		rmethod->jit_wrapper = jit_wrapper;

	} else {
		sig = rmethod->jit_sig;
	}

	sp -= sig->param_count;
	if (sig->hasthis)
		--sp;

	ftndesc.addr = rmethod->jit_addr;
	ftndesc.arg = NULL;

	// FIXME: Optimize this

	gpointer args [32];
	int pindex = 0;
	int stack_index = 0;
	if (rmethod->hasthis) {
		args [pindex ++] = sp [0].data.p;
		stack_index ++;
	}
	type = rmethod->rtype;
	if (type->type != MONO_TYPE_VOID) {
		if (MONO_TYPE_ISSTRUCT (type))
			args [pindex ++] = vt_sp;
		else
			args [pindex ++] = res_buf;
	}
	for (int i = 0; i < rmethod->param_count; ++i) {
		MonoType *t = rmethod->param_types [i];
		stackval *sval = &sp [stack_index + i];
		if (sig->params [i]->byref) {
			args [pindex ++] = sval->data.p;
		} else if (MONO_TYPE_ISSTRUCT (t)) {
			args [pindex ++] = sval->data.p;
		} else if (MONO_TYPE_IS_REFERENCE (t)) {
			args [pindex ++] = &sval->data.p;
		} else {
			switch (t->type) {
			case MONO_TYPE_I1:
			case MONO_TYPE_U1:
			case MONO_TYPE_I2:
			case MONO_TYPE_U2:
			case MONO_TYPE_I4:
			case MONO_TYPE_U4:
			case MONO_TYPE_VALUETYPE:
				args [pindex ++] = &sval->data.i;
				break;
			case MONO_TYPE_PTR:
			case MONO_TYPE_FNPTR:
			case MONO_TYPE_I:
			case MONO_TYPE_U:
			case MONO_TYPE_OBJECT:
				args [pindex ++] = &sval->data.p;
				break;
			case MONO_TYPE_I8:
			case MONO_TYPE_U8:
				args [pindex ++] = &sval->data.l;
				break;
			case MONO_TYPE_R4: {
				float tmp = (float)sval->data.f;
				sval->data.i = *(int*)&tmp;
				args [pindex ++] = &sval->data.i;
				break;
			}
			case MONO_TYPE_R8:
				args [pindex ++] = &sval->data.f;
				break;
			default:
				printf ("%s\n", mono_type_full_name (t));
				g_assert_not_reached ();
			}
		}
	}

	interp_push_lmf (&ext, frame);

	switch (pindex) {
	case 0: {
		typedef void (*T)(gpointer);
		T func = (T)rmethod->jit_wrapper;

		func (&ftndesc);
		break;
	}
	case 1: {
		typedef void (*T)(gpointer, gpointer);
		T func = (T)rmethod->jit_wrapper;

		func (args [0], &ftndesc);
		break;
	}
	case 2: {
		typedef void (*T)(gpointer, gpointer, gpointer);
		T func = (T)rmethod->jit_wrapper;

		func (args [0], args [1], &ftndesc);
		break;
	}
	case 3: {
		typedef void (*T)(gpointer, gpointer, gpointer, gpointer);
		T func = (T)rmethod->jit_wrapper;

		func (args [0], args [1], args [2], &ftndesc);
		break;
	}
	case 4: {
		typedef void (*T)(gpointer, gpointer, gpointer, gpointer, gpointer);
		T func = (T)rmethod->jit_wrapper;

		func (args [0], args [1], args [2], args [3], &ftndesc);
		break;
	}
	case 5: {
		typedef void (*T)(gpointer, gpointer, gpointer, gpointer, gpointer, gpointer);
		T func = (T)rmethod->jit_wrapper;

		func (args [0], args [1], args [2], args [3], args [4], &ftndesc);
		break;
	}
	case 6: {
		typedef void (*T)(gpointer, gpointer, gpointer, gpointer, gpointer, gpointer, gpointer);
		T func = (T)rmethod->jit_wrapper;

		func (args [0], args [1], args [2], args [3], args [4], args [5], &ftndesc);
		break;
	}
	case 7: {
		typedef void (*T)(gpointer, gpointer, gpointer, gpointer, gpointer, gpointer, gpointer, gpointer);
		T func = (T)rmethod->jit_wrapper;

		func (args [0], args [1], args [2], args [3], args [4], args [5], args [6], &ftndesc);
		break;
	}
	case 8: {
		typedef void (*T)(gpointer, gpointer, gpointer, gpointer, gpointer, gpointer, gpointer, gpointer, gpointer);
		T func = (T)rmethod->jit_wrapper;

		func (args [0], args [1], args [2], args [3], args [4], args [5], args [6], args [7], &ftndesc);
		break;
	}
	default:
		g_assert_not_reached ();
		break;
	}

	interp_pop_lmf (&ext);

	MonoType *rtype = rmethod->rtype;
	switch (rtype->type) {
	case MONO_TYPE_VOID:
	case MONO_TYPE_OBJECT:
	case MONO_TYPE_STRING:
	case MONO_TYPE_CLASS:
	case MONO_TYPE_ARRAY:
	case MONO_TYPE_SZARRAY:
	case MONO_TYPE_I:
	case MONO_TYPE_U:
	case MONO_TYPE_PTR:
		sp->data.p = *(gpointer*)res_buf;
		break;
	case MONO_TYPE_I1:
		sp->data.i = *(gint8*)res_buf;
		break;
	case MONO_TYPE_U1:
		sp->data.i = *(guint8*)res_buf;
		break;
	case MONO_TYPE_I2:
		sp->data.i = *(gint16*)res_buf;
		break;
	case MONO_TYPE_U2:
		sp->data.i = *(guint16*)res_buf;
		break;
	case MONO_TYPE_I4:
		sp->data.i = *(gint32*)res_buf;
		break;
	case MONO_TYPE_U4:
		sp->data.i = *(guint32*)res_buf;
		break;
	case MONO_TYPE_I8:
		sp->data.l = *(gint64*)res_buf;
		break;
	case MONO_TYPE_U8:
		sp->data.l = *(guint64*)res_buf;
		break;
	case MONO_TYPE_R4:
		sp->data.f = *(float*)res_buf;
		break;
	case MONO_TYPE_R8:
		sp->data.f = *(double*)res_buf;
		break;
	case MONO_TYPE_VALUETYPE:
		/* The result was written to vt_sp */
		sp->data.p = vt_sp;
		break;
	case MONO_TYPE_GENERICINST:
		if (MONO_TYPE_IS_REFERENCE (rtype)) {
			sp->data.p = *(gpointer*)res_buf;
		} else {
			/* The result was written to vt_sp */
			sp->data.p = vt_sp;
		}
		break;
	default:
		g_print ("%s\n", mono_type_full_name (rtype));
		g_assert_not_reached ();
		break;
	}

	return sp;
}

static MONO_NEVER_INLINE void
do_debugger_tramp (void (*tramp) (void), InterpFrame *frame)
{
	MonoLMFExt ext;
	interp_push_lmf (&ext, frame);
	tramp ();
	interp_pop_lmf (&ext);
}

static MONO_NEVER_INLINE void
do_transform_method (InterpFrame *frame, ThreadContext *context)
{
	MonoLMFExt ext;
	/* Don't push lmf if we have no interp data */
	gboolean push_lmf = frame->parent != NULL;
	ERROR_DECL (error);

	/* Use the parent frame as the current frame is not complete yet */
	if (push_lmf)
		interp_push_lmf (&ext, frame->parent);

	mono_interp_transform_method (frame->imethod, context, error);
	frame->ex = mono_error_convert_to_exception (error);

	if (push_lmf)
		interp_pop_lmf (&ext);
}

static void
copy_varargs_vtstack (MonoMethodSignature *csig, stackval *sp, unsigned char **vt_sp)
{
	char *vt = *(char**)vt_sp;
	stackval *first_arg = sp - csig->param_count;

	/*
	 * We need to have the varargs linearly on the stack so the ArgIterator
	 * can iterate over them. We pass the signature first and then copy them
	 * one by one on the vtstack.
	 */
	*(gpointer*)vt = csig;
	vt += sizeof (gpointer);

	for (int i = csig->sentinelpos; i < csig->param_count; i++) {
		int align, arg_size;
		arg_size = mono_type_stack_size (csig->params [i], &align);
		vt = (char*)ALIGN_PTR_TO (vt, align);

		stackval_to_data (csig->params [i], &first_arg [i], vt, FALSE);
		vt += arg_size;
	}

	vt = (char*)ALIGN_PTR_TO (vt, MINT_VT_ALIGNMENT);

	*(char**)vt_sp = vt;
}

/*
 * These functions are the entry points into the interpreter from compiled code.
 * They are called by the interp_in wrappers. They have the following signature:
 * void (<optional this_arg>, <optional retval pointer>, <arg1>, ..., <argn>, <method ptr>)
 * They pack up their arguments into an InterpEntryData structure and call interp_entry ().
 * It would be possible for the wrappers to pack up the arguments etc, but that would make them bigger, and there are
 * more wrappers then these functions.
 * this/static * ret/void * 16 arguments -> 64 functions.
 */

#define MAX_INTERP_ENTRY_ARGS 8

#define INTERP_ENTRY_BASE(_method, _this_arg, _res) \
	InterpEntryData data; \
	(data).rmethod = (_method); \
	(data).res = (_res); \
	(data).this_arg = (_this_arg); \
	(data).many_args = NULL;

#define INTERP_ENTRY0(_this_arg, _res, _method) {	\
	INTERP_ENTRY_BASE (_method, _this_arg, _res); \
	interp_entry (&data); \
	}
#define INTERP_ENTRY1(_this_arg, _res, _method) {	  \
	INTERP_ENTRY_BASE (_method, _this_arg, _res); \
	(data).args [0] = arg1; \
	interp_entry (&data); \
	}
#define INTERP_ENTRY2(_this_arg, _res, _method) {  \
	INTERP_ENTRY_BASE (_method, _this_arg, _res); \
	(data).args [0] = arg1; \
	(data).args [1] = arg2; \
	interp_entry (&data); \
	}
#define INTERP_ENTRY3(_this_arg, _res, _method) { \
	INTERP_ENTRY_BASE (_method, _this_arg, _res); \
	(data).args [0] = arg1; \
	(data).args [1] = arg2; \
	(data).args [2] = arg3; \
	interp_entry (&data); \
	}
#define INTERP_ENTRY4(_this_arg, _res, _method) {	\
	INTERP_ENTRY_BASE (_method, _this_arg, _res); \
	(data).args [0] = arg1; \
	(data).args [1] = arg2; \
	(data).args [2] = arg3; \
	(data).args [3] = arg4; \
	interp_entry (&data); \
	}
#define INTERP_ENTRY5(_this_arg, _res, _method) {	\
	INTERP_ENTRY_BASE (_method, _this_arg, _res); \
	(data).args [0] = arg1; \
	(data).args [1] = arg2; \
	(data).args [2] = arg3; \
	(data).args [3] = arg4; \
	(data).args [4] = arg5; \
	interp_entry (&data); \
	}
#define INTERP_ENTRY6(_this_arg, _res, _method) {	\
	INTERP_ENTRY_BASE (_method, _this_arg, _res); \
	(data).args [0] = arg1; \
	(data).args [1] = arg2; \
	(data).args [2] = arg3; \
	(data).args [3] = arg4; \
	(data).args [4] = arg5; \
	(data).args [5] = arg6; \
	interp_entry (&data); \
	}
#define INTERP_ENTRY7(_this_arg, _res, _method) {	\
	INTERP_ENTRY_BASE (_method, _this_arg, _res); \
	(data).args [0] = arg1; \
	(data).args [1] = arg2; \
	(data).args [2] = arg3; \
	(data).args [3] = arg4; \
	(data).args [4] = arg5; \
	(data).args [5] = arg6; \
	(data).args [6] = arg7; \
	interp_entry (&data); \
	}
#define INTERP_ENTRY8(_this_arg, _res, _method) {	\
	INTERP_ENTRY_BASE (_method, _this_arg, _res); \
	(data).args [0] = arg1; \
	(data).args [1] = arg2; \
	(data).args [2] = arg3; \
	(data).args [3] = arg4; \
	(data).args [4] = arg5; \
	(data).args [5] = arg6; \
	(data).args [6] = arg7; \
	(data).args [7] = arg8; \
	interp_entry (&data); \
	}

#define ARGLIST0 InterpMethod *rmethod
#define ARGLIST1 gpointer arg1, InterpMethod *rmethod
#define ARGLIST2 gpointer arg1, gpointer arg2, InterpMethod *rmethod
#define ARGLIST3 gpointer arg1, gpointer arg2, gpointer arg3, InterpMethod *rmethod
#define ARGLIST4 gpointer arg1, gpointer arg2, gpointer arg3, gpointer arg4, InterpMethod *rmethod
#define ARGLIST5 gpointer arg1, gpointer arg2, gpointer arg3, gpointer arg4, gpointer arg5, InterpMethod *rmethod
#define ARGLIST6 gpointer arg1, gpointer arg2, gpointer arg3, gpointer arg4, gpointer arg5, gpointer arg6, InterpMethod *rmethod
#define ARGLIST7 gpointer arg1, gpointer arg2, gpointer arg3, gpointer arg4, gpointer arg5, gpointer arg6, gpointer arg7, InterpMethod *rmethod
#define ARGLIST8 gpointer arg1, gpointer arg2, gpointer arg3, gpointer arg4, gpointer arg5, gpointer arg6, gpointer arg7, gpointer arg8, InterpMethod *rmethod

static void interp_entry_static_0 (ARGLIST0) INTERP_ENTRY0 (NULL, NULL, rmethod)
static void interp_entry_static_1 (ARGLIST1) INTERP_ENTRY1 (NULL, NULL, rmethod)
static void interp_entry_static_2 (ARGLIST2) INTERP_ENTRY2 (NULL, NULL, rmethod)
static void interp_entry_static_3 (ARGLIST3) INTERP_ENTRY3 (NULL, NULL, rmethod)
static void interp_entry_static_4 (ARGLIST4) INTERP_ENTRY4 (NULL, NULL, rmethod)
static void interp_entry_static_5 (ARGLIST5) INTERP_ENTRY5 (NULL, NULL, rmethod)
static void interp_entry_static_6 (ARGLIST6) INTERP_ENTRY6 (NULL, NULL, rmethod)
static void interp_entry_static_7 (ARGLIST7) INTERP_ENTRY7 (NULL, NULL, rmethod)
static void interp_entry_static_8 (ARGLIST8) INTERP_ENTRY8 (NULL, NULL, rmethod)
static void interp_entry_static_ret_0 (gpointer res, ARGLIST0) INTERP_ENTRY0 (NULL, res, rmethod)
static void interp_entry_static_ret_1 (gpointer res, ARGLIST1) INTERP_ENTRY1 (NULL, res, rmethod)
static void interp_entry_static_ret_2 (gpointer res, ARGLIST2) INTERP_ENTRY2 (NULL, res, rmethod)
static void interp_entry_static_ret_3 (gpointer res, ARGLIST3) INTERP_ENTRY3 (NULL, res, rmethod)
static void interp_entry_static_ret_4 (gpointer res, ARGLIST4) INTERP_ENTRY4 (NULL, res, rmethod)
static void interp_entry_static_ret_5 (gpointer res, ARGLIST5) INTERP_ENTRY5 (NULL, res, rmethod)
static void interp_entry_static_ret_6 (gpointer res, ARGLIST6) INTERP_ENTRY6 (NULL, res, rmethod)
static void interp_entry_static_ret_7 (gpointer res, ARGLIST7) INTERP_ENTRY7 (NULL, res, rmethod)
static void interp_entry_static_ret_8 (gpointer res, ARGLIST8) INTERP_ENTRY8 (NULL, res, rmethod)
static void interp_entry_instance_0 (gpointer this_arg, ARGLIST0) INTERP_ENTRY0 (this_arg, NULL, rmethod)
static void interp_entry_instance_1 (gpointer this_arg, ARGLIST1) INTERP_ENTRY1 (this_arg, NULL, rmethod)
static void interp_entry_instance_2 (gpointer this_arg, ARGLIST2) INTERP_ENTRY2 (this_arg, NULL, rmethod)
static void interp_entry_instance_3 (gpointer this_arg, ARGLIST3) INTERP_ENTRY3 (this_arg, NULL, rmethod)
static void interp_entry_instance_4 (gpointer this_arg, ARGLIST4) INTERP_ENTRY4 (this_arg, NULL, rmethod)
static void interp_entry_instance_5 (gpointer this_arg, ARGLIST5) INTERP_ENTRY5 (this_arg, NULL, rmethod)
static void interp_entry_instance_6 (gpointer this_arg, ARGLIST6) INTERP_ENTRY6 (this_arg, NULL, rmethod)
static void interp_entry_instance_7 (gpointer this_arg, ARGLIST7) INTERP_ENTRY7 (this_arg, NULL, rmethod)
static void interp_entry_instance_8 (gpointer this_arg, ARGLIST8) INTERP_ENTRY8 (this_arg, NULL, rmethod)
static void interp_entry_instance_ret_0 (gpointer this_arg, gpointer res, ARGLIST0) INTERP_ENTRY0 (this_arg, res, rmethod)
static void interp_entry_instance_ret_1 (gpointer this_arg, gpointer res, ARGLIST1) INTERP_ENTRY1 (this_arg, res, rmethod)
static void interp_entry_instance_ret_2 (gpointer this_arg, gpointer res, ARGLIST2) INTERP_ENTRY2 (this_arg, res, rmethod)
static void interp_entry_instance_ret_3 (gpointer this_arg, gpointer res, ARGLIST3) INTERP_ENTRY3 (this_arg, res, rmethod)
static void interp_entry_instance_ret_4 (gpointer this_arg, gpointer res, ARGLIST4) INTERP_ENTRY4 (this_arg, res, rmethod)
static void interp_entry_instance_ret_5 (gpointer this_arg, gpointer res, ARGLIST5) INTERP_ENTRY5 (this_arg, res, rmethod)
static void interp_entry_instance_ret_6 (gpointer this_arg, gpointer res, ARGLIST6) INTERP_ENTRY6 (this_arg, res, rmethod)
static void interp_entry_instance_ret_7 (gpointer this_arg, gpointer res, ARGLIST7) INTERP_ENTRY7 (this_arg, res, rmethod)
static void interp_entry_instance_ret_8 (gpointer this_arg, gpointer res, ARGLIST8) INTERP_ENTRY8 (this_arg, res, rmethod)

#define INTERP_ENTRY_FUNCLIST(type) (gpointer)interp_entry_ ## type ## _0, (gpointer)interp_entry_ ## type ## _1, (gpointer)interp_entry_ ## type ## _2, (gpointer)interp_entry_ ## type ## _3, (gpointer)interp_entry_ ## type ## _4, (gpointer)interp_entry_ ## type ## _5, (gpointer)interp_entry_ ## type ## _6, (gpointer)interp_entry_ ## type ## _7, (gpointer)interp_entry_ ## type ## _8

static gpointer entry_funcs_static [MAX_INTERP_ENTRY_ARGS + 1] = { INTERP_ENTRY_FUNCLIST (static) };
static gpointer entry_funcs_static_ret [MAX_INTERP_ENTRY_ARGS + 1] = { INTERP_ENTRY_FUNCLIST (static_ret) };
static gpointer entry_funcs_instance [MAX_INTERP_ENTRY_ARGS + 1] = { INTERP_ENTRY_FUNCLIST (instance) };
static gpointer entry_funcs_instance_ret [MAX_INTERP_ENTRY_ARGS + 1] = { INTERP_ENTRY_FUNCLIST (instance_ret) };

/* General version for methods with more than MAX_INTERP_ENTRY_ARGS arguments */
static void
interp_entry_general (gpointer this_arg, gpointer res, gpointer *args, gpointer rmethod)
{
	INTERP_ENTRY_BASE ((InterpMethod*)rmethod, this_arg, res);
	data.many_args = args;
	interp_entry (&data);
}

#ifdef MONO_ARCH_HAVE_INTERP_ENTRY_TRAMPOLINE

// inline so we can alloc on stack
#define alloc_storage_for_stackval(s, t, p) do {							\
		if ((t)->type == MONO_TYPE_GENERICINST && !MONO_TYPE_IS_REFERENCE (t)) {		\
			(s)->data.vt = alloca (mono_class_value_size ((t)->data.generic_class->container_class, NULL));	\
		} else if ((t)->type == MONO_TYPE_VALUETYPE) {						\
			if (p)										\
				(s)->data.vt = alloca (mono_class_native_size ((t)->data.klass, NULL));	\
			else										\
				(s)->data.vt = alloca (mono_class_value_size ((t)->data.klass, NULL));	\
		}											\
	} while (0)

static void
interp_entry_from_trampoline (gpointer ccontext_untyped, gpointer rmethod_untyped)
{
	InterpFrame frame;
	ThreadContext *context;
	InterpFrame *old_frame;
	stackval result;
	stackval *args;
	MonoMethod *method;
	MonoMethodSignature *sig;
	CallContext *ccontext = (CallContext*) ccontext_untyped;
	InterpMethod *rmethod = (InterpMethod*) rmethod_untyped;
	gpointer orig_domain, attach_cookie;
	int i;

	if (rmethod->needs_thread_attach)
		orig_domain = mono_threads_attach_coop (mono_domain_get (), &attach_cookie);

	context = get_context ();

	method = rmethod->method;
	sig = mono_method_signature (method);

	frame.ex = NULL;
	old_frame = context->current_frame;

	args = (stackval*)alloca (sizeof (stackval) * (sig->param_count + (sig->hasthis ? 1 : 0)));

	init_frame (&frame, NULL, rmethod, args, &result);

	/* Allocate storage for value types */
	for (i = 0; i < sig->param_count; i++) {
		MonoType *type = sig->params [i];
		alloc_storage_for_stackval (&frame.stack_args [i + sig->hasthis], type, sig->pinvoke);
	}

	if (sig->ret->type != MONO_TYPE_VOID)
		alloc_storage_for_stackval (frame.retval, sig->ret, sig->pinvoke);

	/* Copy the args saved in the trampoline to the frame stack */
	mono_arch_get_native_call_context_args (ccontext, &frame, sig);

	interp_exec_method (&frame, context);
	context->current_frame = old_frame;

	if (rmethod->needs_thread_attach)
		mono_threads_detach_coop (orig_domain, &attach_cookie);

	// FIXME:
	g_assert (frame.ex == NULL);

	/* Write back the return value */
	mono_arch_set_native_call_context_ret (ccontext, &frame, sig);
}
#endif

static InterpMethod*
lookup_method_pointer (gpointer addr)
{
	MonoDomain *domain = mono_domain_get ();
	MonoJitDomainInfo *info = domain_jit_info (domain);
	InterpMethod *res = NULL;

	mono_domain_lock (domain);
	if (info->interp_method_pointer_hash)
		res = (InterpMethod*)g_hash_table_lookup (info->interp_method_pointer_hash, addr);
	mono_domain_unlock (domain);

	return res;
}

#ifndef MONO_ARCH_HAVE_INTERP_NATIVE_TO_MANAGED
static void
interp_no_native_to_managed (void)
{
	g_error ("interpreter: native-to-managed transition not available on this platform");
}
#endif

/*
 * interp_create_method_pointer:
 *
 * Return a function pointer which can be used to call METHOD using the
 * interpreter. Return NULL for methods which are not supported.
 */
static gpointer
interp_create_method_pointer (MonoMethod *method, gboolean compile, MonoError *error)
{
#ifndef MONO_ARCH_HAVE_INTERP_NATIVE_TO_MANAGED
	return (gpointer)interp_no_native_to_managed;
#else
	gpointer addr, entry_func, entry_wrapper;
	MonoDomain *domain = mono_domain_get ();
	MonoJitDomainInfo *info;
	InterpMethod *imethod = mono_interp_get_imethod (domain, method, error);

	if (compile) {
		/* Return any errors from method compilation */
		mono_interp_transform_method (imethod, get_context (), error);
		return_val_if_nok (error, NULL);
	}

	/* HACK: method_ptr of delegate should point to a runtime method*/
	if (method->wrapper_type && (method->wrapper_type == MONO_WRAPPER_DYNAMIC_METHOD ||
				(method->wrapper_type == MONO_WRAPPER_MANAGED_TO_NATIVE)))
		return imethod;

	if (imethod->jit_entry)
		return imethod->jit_entry;

	MonoMethodSignature *sig = mono_method_signature (method);
#ifndef MONO_ARCH_HAVE_INTERP_ENTRY_TRAMPOLINE
	MonoMethod *wrapper = mini_get_interp_in_wrapper (sig);

	entry_wrapper = mono_jit_compile_method_jit_only (wrapper, error);
	mono_error_assertf_ok (error, "couldn't compile wrapper \"%s\" for \"%s\"",
			mono_method_get_name_full (wrapper, TRUE, TRUE, MONO_TYPE_NAME_FORMAT_IL),
			mono_method_get_name_full (method,  TRUE, TRUE, MONO_TYPE_NAME_FORMAT_IL));

	if (sig->param_count > MAX_INTERP_ENTRY_ARGS) {
		entry_func = (gpointer)interp_entry_general;
	} else if (sig->hasthis) {
		if (sig->ret->type == MONO_TYPE_VOID)
			entry_func = entry_funcs_instance [sig->param_count];
		else
			entry_func = entry_funcs_instance_ret [sig->param_count];
	} else {
		if (sig->ret->type == MONO_TYPE_VOID)
			entry_func = entry_funcs_static [sig->param_count];
		else
			entry_func = entry_funcs_static_ret [sig->param_count];
	}
	g_assert (entry_func);
#else
	if (!mono_native_to_interp_trampoline) {
		if (mono_aot_only) {
			mono_native_to_interp_trampoline = (MonoFuncV)mono_aot_get_trampoline ("native_to_interp_trampoline");
		} else {
			MonoTrampInfo *info;
			mono_native_to_interp_trampoline = (MonoFuncV)mono_arch_get_native_to_interp_trampoline (&info);
			mono_tramp_info_register (info, NULL);
		}
	}
	entry_wrapper = (gpointer)mono_native_to_interp_trampoline;
	/* We need the lmf wrapper only when being called from mixed mode */
	if (sig->pinvoke)
		entry_func = (gpointer)interp_entry_from_trampoline;
	else
		entry_func = mono_jit_compile_method_jit_only (mini_get_interp_lmf_wrapper (), error);
#endif
	/* This is the argument passed to the interp_in wrapper by the static rgctx trampoline */
	MonoFtnDesc *ftndesc = g_new0 (MonoFtnDesc, 1);
	ftndesc->addr = entry_func;
	ftndesc->arg = imethod;
	mono_error_assert_ok (error);

	/*
	 * The wrapper is called by compiled code, which doesn't pass the extra argument, so we pass it in the
	 * rgctx register using a trampoline.
	 */

	addr = mono_create_ftnptr_arg_trampoline (ftndesc, entry_wrapper);

	info = domain_jit_info (domain);
	mono_domain_lock (domain);
	if (!info->interp_method_pointer_hash)
		info->interp_method_pointer_hash = g_hash_table_new (NULL, NULL);
	g_hash_table_insert (info->interp_method_pointer_hash, addr, imethod);
	mono_domain_unlock (domain);

	mono_memory_barrier ();
	imethod->jit_entry = addr;

	return addr;
#endif
}

#if COUNT_OPS
static int opcode_counts[512];

#define COUNT_OP(op) opcode_counts[op]++
#else
#define COUNT_OP(op) 
#endif

#if DEBUG_INTERP
#define DUMP_INSTR() \
	if (tracing > 1) { \
		char *ins; \
		if (sp > frame->stack) { \
			ins = dump_stack (frame->stack, sp); \
		} else { \
			ins = g_strdup (""); \
		} \
		sp->data.l = 0; \
		output_indent (); \
		char *mn = mono_method_full_name (frame->imethod->method, FALSE); \
		char *disasm = mono_interp_dis_mintop(rtm->code, ip); \
		g_print ("(%p) %s -> %s\t%d:%s\n", mono_thread_internal_current (), mn, disasm, vt_sp - vtalloc, ins); \
		g_free (mn); \
		g_free (ins); \
		g_free (disasm); \
	}
#else
#define DUMP_INSTR()
#endif

#ifdef __GNUC__
#define USE_COMPUTED_GOTO 1
#endif
#if USE_COMPUTED_GOTO
#define MINT_IN_SWITCH(op) COUNT_OP(op); goto *in_labels[op];
#define MINT_IN_CASE(x) LAB_ ## x:
#if DEBUG_INTERP
#define MINT_IN_BREAK if (tracing > 1) goto main_loop; else { COUNT_OP(*ip); goto *in_labels[*ip]; }
#else
#define MINT_IN_BREAK { COUNT_OP(*ip); goto *in_labels[*ip]; }
#endif
#define MINT_IN_DEFAULT mint_default: if (0) goto mint_default; /* make gcc shut up */
#else
#define MINT_IN_SWITCH(op) switch (op)
#define MINT_IN_CASE(x) case x:
#define MINT_IN_BREAK break
#define MINT_IN_DEFAULT default:
#endif

/*
 * If EXIT_AT_FINALLY is not -1, exit after exiting the finally clause with that index.
 * If BASE_FRAME is not NULL, copy arguments/locals from BASE_FRAME.
 */
static void 
interp_exec_method_full (InterpFrame *frame, ThreadContext *context, guint16 *start_with_ip, MonoException *filter_exception, int exit_at_finally, InterpFrame *base_frame)
{
	InterpFrame child_frame;
	GSList *finally_ips = NULL;
	const unsigned short *endfinally_ip = NULL;
	const unsigned short *ip = NULL;
	stackval *sp;
	InterpMethod *rtm = NULL;
#if DEBUG_INTERP
	gint tracing = global_tracing;
	unsigned char *vtalloc;
#else
	gint tracing = 0;
#endif
	int i32;
	unsigned char *vt_sp;
	unsigned char *locals;
	ERROR_DECL (error);
	MonoObject *o = NULL;
	MonoClass *c;
#if USE_COMPUTED_GOTO
	static void *in_labels[] = {
#define OPDEF(a,b,c,d) \
	&&LAB_ ## a,
#include "mintops.def"
	0 };
#endif


	frame->ex = NULL;
	frame->ex_handler = NULL;
	frame->ip = NULL;
	frame->domain = mono_domain_get ();
	context->current_frame = frame;

	debug_enter (frame, &tracing);

	rtm = frame->imethod;
	if (!frame->imethod->transformed) {
#if DEBUG_INTERP
		char *mn = mono_method_full_name (frame->imethod->method, TRUE);
		g_print ("(%p) Transforming %s\n", mono_thread_internal_current (), mn);
		g_free (mn);
#endif

		do_transform_method (frame, context);
		if (frame->ex)
			THROW_EX (frame->ex, NULL);
		EXCEPTION_CHECKPOINT;
	}

	if (!start_with_ip) {
		frame->args = g_newa (char, rtm->alloca_size);
		memset (frame->args, 0, rtm->alloca_size);

		ip = rtm->code;
	} else {
		ip = start_with_ip;
		if (base_frame) {
			frame->args = g_newa (char, rtm->alloca_size);
			memcpy (frame->args, base_frame->args, rtm->alloca_size);
		}
	}
	sp = frame->stack = (stackval *) ((char *) frame->args + rtm->args_size);
	vt_sp = (unsigned char *) sp + rtm->stack_size;
#if DEBUG_INTERP
	vtalloc = vt_sp;
#endif
	locals = (unsigned char *) vt_sp + rtm->vt_stack_size;
	frame->locals = locals;
	child_frame.parent = frame;

	if (filter_exception) {
		sp->data.p = filter_exception;
		sp++;
	}

	/*
	 * using while (ip < end) may result in a 15% performance drop, 
	 * but it may be useful for debug
	 */
	while (1) {
	main_loop:
		/* g_assert (sp >= frame->stack); */
		/* g_assert(vt_sp - vtalloc <= rtm->vt_stack_size); */
		DUMP_INSTR();
		MINT_IN_SWITCH (*ip) {
		MINT_IN_CASE(MINT_INITLOCALS)
			memset (locals, 0, rtm->locals_size);
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_NOP)
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_NIY)
			g_error ("mint_niy: instruction not implemented yet.  This shouldn't happen.");
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BREAK)
			++ip;
			do_debugger_tramp (mini_get_dbg_callbacks ()->user_break, frame);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDNULL) 
			sp->data.p = NULL;
			++ip;
			++sp;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_ARGLIST)
			g_assert (frame->varargs);
			sp->data.p = vt_sp;
			*(gpointer*)sp->data.p = frame->varargs;
			vt_sp += ALIGN_TO (sizeof (gpointer), MINT_VT_ALIGNMENT);
			++ip;
			++sp;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_VTRESULT) {
			int ret_size = * (guint16 *)(ip + 1);
			unsigned char *ret_vt_sp = vt_sp;
			vt_sp -= READ32(ip + 2);
			if (ret_size > 0) {
				memmove (vt_sp, ret_vt_sp, ret_size);
				sp [-1].data.p = vt_sp;
				vt_sp += ALIGN_TO (ret_size, MINT_VT_ALIGNMENT);
			}
			ip += 4;
			MINT_IN_BREAK;
		}
#define LDC(n) do { sp->data.i = (n); ++ip; ++sp; } while (0)
		MINT_IN_CASE(MINT_LDC_I4_M1)
			LDC(-1);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDC_I4_0)
			LDC(0);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDC_I4_1)
			LDC(1);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDC_I4_2)
			LDC(2);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDC_I4_3)
			LDC(3);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDC_I4_4)
			LDC(4);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDC_I4_5)
			LDC(5);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDC_I4_6)
			LDC(6);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDC_I4_7)
			LDC(7);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDC_I4_8)
			LDC(8);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDC_I4_S) 
			sp->data.i = *(const short *)(ip + 1);
			ip += 2;
			++sp;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDC_I4)
			++ip;
			sp->data.i = READ32 (ip);
			ip += 2;
			++sp;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDC_I8)
			++ip;
			sp->data.l = READ64 (ip);
			ip += 4;
			++sp;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDC_R4) {
			guint32 val;
			++ip;
			val = READ32(ip);
			sp->data.f = * (float *)&val;
			ip += 2;
			++sp;
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_LDC_R8) 
			sp->data.l = READ64 (ip + 1); /* note union usage */
			ip += 5;
			++sp;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_DUP) 
			sp [0] = sp[-1];
			++sp;
			++ip; 
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_DUP_VT)
			i32 = READ32 (ip + 1);
			sp->data.p = vt_sp;
			memcpy(sp->data.p, sp [-1].data.p, i32);
			vt_sp += ALIGN_TO (i32, MINT_VT_ALIGNMENT);
			++sp;
			ip += 3;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_POP) {
			guint16 u16 = (* (guint16 *)(ip + 1)) + 1;
			if (u16 > 1)
				memmove (sp - u16, sp - 1, (u16 - 1) * sizeof (stackval));
			sp--;
			ip += 2;
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_JMP) {
			InterpMethod *new_method = (InterpMethod*)rtm->data_items [* (guint16 *)(ip + 1)];
			gboolean realloc_frame = new_method->alloca_size > rtm->alloca_size;

			if (frame->imethod->prof_flags & MONO_PROFILER_CALL_INSTRUMENTATION_TAIL_CALL)
				MONO_PROFILER_RAISE (method_tail_call, (frame->imethod->method, new_method->method));

			if (!new_method->transformed) {
				ERROR_DECL (error);

				frame->ip = ip;
				mono_interp_transform_method (new_method, context, error);
				frame->ex = mono_error_convert_to_exception (error);
				if (frame->ex)
					goto exit_frame;
			}
			ip += 2;
			rtm = frame->imethod = new_method;
			/*
			 * We allocate the stack frame from scratch and store the arguments in the
			 * locals again since it's possible for the caller stack frame to be smaller
			 * than the callee stack frame (at the interp level)
			 */
			if (realloc_frame) {
				frame->args = g_newa (char, rtm->alloca_size);
				memset (frame->args, 0, rtm->alloca_size);
				sp = frame->stack = (stackval *) ((char *) frame->args + rtm->args_size);
			}
			vt_sp = (unsigned char *) sp + rtm->stack_size;
#if DEBUG_INTERP
			vtalloc = vt_sp;
#endif
			locals = vt_sp + rtm->vt_stack_size;
			frame->locals = locals;
			if (realloc_frame)
				ip = rtm->code;
			else
				ip = rtm->new_body_start; /* bypass storing input args from callers frame */
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_CALLI) {
			MonoMethodSignature *csignature;
			stackval *endsp = sp;

			frame->ip = ip;
			
			csignature = (MonoMethodSignature*)rtm->data_items [* (guint16 *)(ip + 1)];
			ip += 2;
			--sp;
			--endsp;
			child_frame.imethod = (InterpMethod*)sp->data.p;

			sp->data.p = vt_sp;
			child_frame.retval = sp;
			/* decrement by the actual number of args */
			sp -= csignature->param_count;
			if (csignature->hasthis)
				--sp;
			child_frame.stack_args = sp;

			if (child_frame.imethod->method->flags & METHOD_ATTRIBUTE_PINVOKE_IMPL) {
				child_frame.imethod = mono_interp_get_imethod (rtm->domain, mono_marshal_get_native_wrapper (child_frame.imethod->method, FALSE, FALSE), error);
				mono_error_cleanup (error); /* FIXME: don't swallow the error */
			}

			if (csignature->hasthis) {
				MonoObject *this_arg = (MonoObject*)sp->data.p;

				if (m_class_is_valuetype (this_arg->vtable->klass)) {
					gpointer unboxed = mono_object_unbox (this_arg);
					sp [0].data.p = unboxed;
				}
			}

			interp_exec_method (&child_frame, context);

			context->current_frame = frame;

			if (context->has_resume_state) {
				if (frame == context->handler_frame)
					SET_RESUME_STATE (context);
				else
					goto exit_frame;
			}

			CHECK_CHILD_EX (child_frame, ip - 2);

			/* need to handle typedbyref ... */
			if (csignature->ret->type != MONO_TYPE_VOID) {
				*sp = *endsp;
				sp++;
			}
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_CALLI_NAT_FAST) {
			gpointer target_ip = sp [-1].data.p;
			MonoMethodSignature *csignature = (MonoMethodSignature*)rtm->data_items [* (guint16 *)(ip + 1)];
			int opcode = *(guint16 *)(ip + 2);

			sp--;
			frame->ip = ip;

			sp = do_icall (context, csignature, opcode, sp, target_ip);
			EXCEPTION_CHECKPOINT;
			if (context->has_resume_state) {
				if (frame == context->handler_frame)
					SET_RESUME_STATE (context);
				else
					goto exit_frame;
			}
			ip += 3;
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_CALLI_NAT) {
			MonoMethodSignature *csignature;
			stackval *endsp = sp;
			unsigned char *code = NULL;

			frame->ip = ip;
			
			csignature = (MonoMethodSignature*)rtm->data_items [* (guint16 *)(ip + 1)];
			ip += 2;
			--sp;
			--endsp;
			code = (guchar*)sp->data.p;
			child_frame.imethod = NULL;

			sp->data.p = vt_sp;
			child_frame.retval = sp;
			/* decrement by the actual number of args */
			sp -= csignature->param_count;
			if (csignature->hasthis)
				--sp;
			child_frame.stack_args = sp;
			if (frame->imethod->method->dynamic && csignature->pinvoke) {
				MonoMarshalSpec **mspecs;
				MonoMethodPInvoke piinfo;
				MonoMethod *m;

				/* Pinvoke call is missing the wrapper. See mono_get_native_calli_wrapper */
				mspecs = g_new0 (MonoMarshalSpec*, csignature->param_count + 1);
				memset (&piinfo, 0, sizeof (piinfo));

				m = mono_marshal_get_native_func_wrapper (m_class_get_image (frame->imethod->method->klass), csignature, &piinfo, mspecs, code);

				for (int i = csignature->param_count; i >= 0; i--)
					if (mspecs [i])
						mono_metadata_free_marshal_spec (mspecs [i]);
				g_free (mspecs);

				child_frame.imethod = mono_interp_get_imethod (rtm->domain, m, error);
				mono_error_cleanup (error); /* FIXME: don't swallow the error */

				interp_exec_method (&child_frame, context);
			} else {
				ves_pinvoke_method (&child_frame, csignature, (MonoFuncV) code, FALSE, context);
			}
			context->current_frame = frame;

			if (context->has_resume_state) {
				if (frame == context->handler_frame)
					SET_RESUME_STATE (context);
				else
					goto exit_frame;
			}

			CHECK_CHILD_EX (child_frame, ip - 2);

			/* need to handle typedbyref ... */
			if (csignature->ret->type != MONO_TYPE_VOID) {
				*sp = *endsp;
				sp++;
			}
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_CALL)
		MINT_IN_CASE(MINT_VCALL)
		MINT_IN_CASE(MINT_CALLVIRT)
		MINT_IN_CASE(MINT_VCALLVIRT) {
			gboolean is_void = *ip == MINT_VCALL || *ip == MINT_VCALLVIRT;
			gboolean is_virtual = *ip == MINT_CALLVIRT || *ip == MINT_VCALLVIRT;
			stackval *endsp = sp;
			int num_varargs = 0;;

			frame->ip = ip;
			
			child_frame.imethod = (InterpMethod*)rtm->data_items [* (guint16 *)(ip + 1)];
			if (child_frame.imethod->vararg) {
				/* The real signature for vararg calls */
				MonoMethodSignature *csig = (MonoMethodSignature*) rtm->data_items [* (guint16*) (ip + 2)];
				/* Push all vararg arguments from normal sp to vt_sp together with the signature */
				num_varargs = csig->param_count - csig->sentinelpos;
				child_frame.varargs = (char*) vt_sp;
				copy_varargs_vtstack (csig, sp, &vt_sp);
			}
			ip += 2 + child_frame.imethod->vararg;
			sp->data.p = vt_sp;
			child_frame.retval = sp;

			/* decrement by the actual number of args */
			sp -= child_frame.imethod->param_count + child_frame.imethod->hasthis + num_varargs;
			child_frame.stack_args = sp;

			if (is_virtual) {
				MonoObject *this_arg = (MonoObject*)sp->data.p;
				MonoClass *this_class = this_arg->vtable->klass;

				child_frame.imethod = get_virtual_method (child_frame.imethod, this_arg);
				if (m_class_is_valuetype (this_class) && m_class_is_valuetype (child_frame.imethod->method->klass)) {
					/* unbox */
					gpointer unboxed = mono_object_unbox (this_arg);
					sp [0].data.p = unboxed;
				}
			}

			interp_exec_method (&child_frame, context);

			context->current_frame = frame;

			if (context->has_resume_state) {
				if (frame == context->handler_frame)
					SET_RESUME_STATE (context);
				else
					goto exit_frame;
			}
			CHECK_CHILD_EX (child_frame, ip - 2);

			if (!is_void) {
				/* need to handle typedbyref ... */
				*sp = *endsp;
				sp++;
			}
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_JIT_CALL) {
			InterpMethod *rmethod = (InterpMethod*)rtm->data_items [* (guint16 *)(ip + 1)];
			ERROR_DECL (error);
			frame->ip = ip;
			ip += 2;
			sp = do_jit_call (sp, vt_sp, context, frame, rmethod, error);
			if (!is_ok (error)) {
				MonoException *ex = mono_error_convert_to_exception (error);
				THROW_EX (ex, ip);
			}

			if (context->has_resume_state) {
				/*
				 * If this bit is set, it means the call has thrown the exception, and we
				 * reached this point because the EH code in mono_handle_exception ()
				 * unwound all the JITted frames below us. mono_interp_set_resume_state ()
				 * has set the fields in context to indicate where we have to resume execution.
				 */
				if (frame == context->handler_frame)
					SET_RESUME_STATE (context);
				else
					goto exit_frame;
			}
			if (rmethod->rtype->type != MONO_TYPE_VOID)
				sp++;

			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_CALLRUN) {
			MonoMethod *target_method = (MonoMethod*) rtm->data_items [* (guint16 *)(ip + 1)];
			MonoMethodSignature *sig = (MonoMethodSignature*) rtm->data_items [* (guint16 *)(ip + 2)];
			stackval *retval;

			sp->data.p = vt_sp;
			retval = sp;

			sp -= sig->param_count;
			if (sig->hasthis)
				sp--;

			ves_imethod (frame, target_method, sig, sp, retval);
			if (frame->ex)
				THROW_EX (frame->ex, ip);

			if (sig->ret->type != MONO_TYPE_VOID) {
				*sp = *retval;
                                sp++;
			}
			ip += 3;
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_RET)
			--sp;
			*frame->retval = *sp;
			if (sp > frame->stack)
				g_warning ("ret: more values on stack: %d", sp-frame->stack);
			goto exit_frame;
		MINT_IN_CASE(MINT_RET_VOID)
			if (sp > frame->stack)
				g_warning ("ret.void: more values on stack: %d %s", sp-frame->stack, mono_method_full_name (frame->imethod->method, TRUE));
			goto exit_frame;
		MINT_IN_CASE(MINT_RET_VT)
			i32 = READ32(ip + 1);
			--sp;
			memcpy(frame->retval->data.p, sp->data.p, i32);
			if (sp > frame->stack)
				g_warning ("ret.vt: more values on stack: %d", sp-frame->stack);
			goto exit_frame;
		MINT_IN_CASE(MINT_BR_S)
			ip += (short) *(ip + 1);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BR)
			ip += (gint32) READ32(ip + 1);
			MINT_IN_BREAK;
#define ZEROP_S(datamem, op) \
	--sp; \
	if (sp->data.datamem op 0) \
		ip += * (gint16 *)(ip + 1); \
	else \
		ip += 2;

#define ZEROP(datamem, op) \
	--sp; \
	if (sp->data.datamem op 0) \
		ip += READ32(ip + 1); \
	else \
		ip += 3;

		MINT_IN_CASE(MINT_BRFALSE_I4_S)
			ZEROP_S(i, ==);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BRFALSE_I8_S)
			ZEROP_S(l, ==);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BRFALSE_R8_S)
			ZEROP_S(f, ==);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BRFALSE_I4)
			ZEROP(i, ==);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BRFALSE_I8)
			ZEROP(l, ==);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BRFALSE_R8)
			ZEROP_S(f, ==);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BRTRUE_I4_S)
			ZEROP_S(i, !=);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BRTRUE_I8_S)
			ZEROP_S(l, !=);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BRTRUE_R8_S)
			ZEROP_S(f, !=);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BRTRUE_I4)
			ZEROP(i, !=);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BRTRUE_I8)
			ZEROP(l, !=);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BRTRUE_R8)
			ZEROP(f, !=);
			MINT_IN_BREAK;
#define CONDBR_S(cond) \
	sp -= 2; \
	if (cond) \
		ip += * (gint16 *)(ip + 1); \
	else \
		ip += 2;
#define BRELOP_S(datamem, op) \
	CONDBR_S(sp[0].data.datamem op sp[1].data.datamem)

#define CONDBR(cond) \
	sp -= 2; \
	if (cond) \
		ip += READ32(ip + 1); \
	else \
		ip += 3;

#define BRELOP(datamem, op) \
	CONDBR(sp[0].data.datamem op sp[1].data.datamem)

		MINT_IN_CASE(MINT_BEQ_I4_S)
			BRELOP_S(i, ==)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BEQ_I8_S)
			BRELOP_S(l, ==)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BEQ_R8_S)
			CONDBR_S(!mono_isunordered (sp [0].data.f, sp [1].data.f) && sp[0].data.f == sp[1].data.f)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BEQ_I4)
			BRELOP(i, ==)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BEQ_I8)
			BRELOP(l, ==)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BEQ_R8)
			CONDBR(!mono_isunordered (sp [0].data.f, sp [1].data.f) && sp[0].data.f == sp[1].data.f)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BGE_I4_S)
			BRELOP_S(i, >=)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BGE_I8_S)
			BRELOP_S(l, >=)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BGE_R8_S)
			CONDBR_S(!mono_isunordered (sp [0].data.f, sp [1].data.f) && sp[0].data.f >= sp[1].data.f)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BGE_I4)
			BRELOP(i, >=)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BGE_I8)
			BRELOP(l, >=)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BGE_R8)
			CONDBR(!mono_isunordered (sp [0].data.f, sp [1].data.f) && sp[0].data.f >= sp[1].data.f)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BGT_I4_S)
			BRELOP_S(i, >)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BGT_I8_S)
			BRELOP_S(l, >)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BGT_R8_S)
			CONDBR_S(!mono_isunordered (sp [0].data.f, sp [1].data.f) && sp[0].data.f > sp[1].data.f)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BGT_I4)
			BRELOP(i, >)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BGT_I8)
			BRELOP(l, >)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BGT_R8)
			CONDBR(!mono_isunordered (sp [0].data.f, sp [1].data.f) && sp[0].data.f > sp[1].data.f)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BLT_I4_S)
			BRELOP_S(i, <)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BLT_I8_S)
			BRELOP_S(l, <)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BLT_R8_S)
			CONDBR_S(!mono_isunordered (sp [0].data.f, sp [1].data.f) && sp[0].data.f < sp[1].data.f)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BLT_I4)
			BRELOP(i, <)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BLT_I8)
			BRELOP(l, <)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BLT_R8)
			CONDBR(!mono_isunordered (sp [0].data.f, sp [1].data.f) && sp[0].data.f < sp[1].data.f)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BLE_I4_S)
			BRELOP_S(i, <=)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BLE_I8_S)
			BRELOP_S(l, <=)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BLE_R8_S)
			CONDBR_S(!mono_isunordered (sp [0].data.f, sp [1].data.f) && sp[0].data.f <= sp[1].data.f)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BLE_I4)
			BRELOP(i, <=)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BLE_I8)
			BRELOP(l, <=)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BLE_R8)
			CONDBR(!mono_isunordered (sp [0].data.f, sp [1].data.f) && sp[0].data.f <= sp[1].data.f)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BNE_UN_I4_S)
			BRELOP_S(i, !=)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BNE_UN_I8_S)
			BRELOP_S(l, !=)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BNE_UN_R8_S)
			CONDBR_S(mono_isunordered (sp [0].data.f, sp [1].data.f) || sp[0].data.f != sp[1].data.f)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BNE_UN_I4)
			BRELOP(i, !=)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BNE_UN_I8)
			BRELOP(l, !=)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BNE_UN_R8)
			CONDBR(mono_isunordered (sp [0].data.f, sp [1].data.f) || sp[0].data.f != sp[1].data.f)
			MINT_IN_BREAK;

#define BRELOP_S_CAST(datamem, op, type) \
	sp -= 2; \
	if ((type) sp[0].data.datamem op (type) sp[1].data.datamem) \
		ip += * (gint16 *)(ip + 1); \
	else \
		ip += 2;

#define BRELOP_CAST(datamem, op, type) \
	sp -= 2; \
	if ((type) sp[0].data.datamem op (type) sp[1].data.datamem) \
		ip += READ32(ip + 1); \
	else \
		ip += 3;

		MINT_IN_CASE(MINT_BGE_UN_I4_S)
			BRELOP_S_CAST(i, >=, guint32);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BGE_UN_I8_S)
			BRELOP_S_CAST(l, >=, guint64);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BGE_UN_R8_S)
			CONDBR_S(mono_isunordered (sp [0].data.f, sp [1].data.f) || sp[0].data.f >= sp[1].data.f)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BGE_UN_I4)
			BRELOP_CAST(i, >=, guint32);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BGE_UN_I8)
			BRELOP_CAST(l, >=, guint64);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BGE_UN_R8)
			CONDBR(mono_isunordered (sp [0].data.f, sp [1].data.f) || sp[0].data.f >= sp[1].data.f)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BGT_UN_I4_S)
			BRELOP_S_CAST(i, >, guint32);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BGT_UN_I8_S)
			BRELOP_S_CAST(l, >, guint64);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BGT_UN_R8_S)
			CONDBR_S(mono_isunordered (sp [0].data.f, sp [1].data.f) || sp[0].data.f > sp[1].data.f)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BGT_UN_I4)
			BRELOP_CAST(i, >, guint32);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BGT_UN_I8)
			BRELOP_CAST(l, >, guint64);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BGT_UN_R8)
			CONDBR(mono_isunordered (sp [0].data.f, sp [1].data.f) || sp[0].data.f > sp[1].data.f)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BLE_UN_I4_S)
			BRELOP_S_CAST(i, <=, guint32);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BLE_UN_I8_S)
			BRELOP_S_CAST(l, <=, guint64);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BLE_UN_R8_S)
			CONDBR_S(mono_isunordered (sp [0].data.f, sp [1].data.f) || sp[0].data.f <= sp[1].data.f)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BLE_UN_I4)
			BRELOP_CAST(i, <=, guint32);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BLE_UN_I8)
			BRELOP_CAST(l, <=, guint64);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BLE_UN_R8)
			CONDBR(mono_isunordered (sp [0].data.f, sp [1].data.f) || sp[0].data.f <= sp[1].data.f)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BLT_UN_I4_S)
			BRELOP_S_CAST(i, <, guint32);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BLT_UN_I8_S)
			BRELOP_S_CAST(l, <, guint64);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BLT_UN_R8_S)
			CONDBR_S(mono_isunordered (sp [0].data.f, sp [1].data.f) || sp[0].data.f < sp[1].data.f)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BLT_UN_I4)
			BRELOP_CAST(i, <, guint32);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BLT_UN_I8)
			BRELOP_CAST(l, <, guint64);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BLT_UN_R8)
			CONDBR(mono_isunordered (sp [0].data.f, sp [1].data.f) || sp[0].data.f < sp[1].data.f)
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_SWITCH) {
			guint32 n;
			const unsigned short *st;
			++ip;
			n = READ32 (ip);
			ip += 2;
			st = ip + 2 * n;
			--sp;
			if ((guint32)sp->data.i < n) {
				gint offset;
				ip += 2 * (guint32)sp->data.i;
				offset = READ32 (ip);
				ip = ip + offset;
			} else {
				ip = st;
			}
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_LDIND_I1)
			++ip;
			sp[-1].data.i = *(gint8*)sp[-1].data.p;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDIND_U1)
			++ip;
			sp[-1].data.i = *(guint8*)sp[-1].data.p;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDIND_I2)
			++ip;
			sp[-1].data.i = *(gint16*)sp[-1].data.p;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDIND_U2)
			++ip;
			sp[-1].data.i = *(guint16*)sp[-1].data.p;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDIND_I4) /* Fall through */
		MINT_IN_CASE(MINT_LDIND_U4)
			++ip;
			sp[-1].data.i = *(gint32*)sp[-1].data.p;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDIND_I8)
			++ip;
			/* memmove handles unaligned case */
			memmove (&sp [-1].data.l, sp [-1].data.p, sizeof (gint64));
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDIND_I) {
			guint16 offset = * (guint16 *)(ip + 1);
			sp[-1 - offset].data.p = *(gpointer*)sp[-1 - offset].data.p;
			ip += 2;
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_LDIND_R4)
			++ip;
			sp[-1].data.f = *(gfloat*)sp[-1].data.p;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDIND_R8)
			++ip;
			sp[-1].data.f = *(gdouble*)sp[-1].data.p;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDIND_REF)
			++ip;
			sp[-1].data.p = *(gpointer*)sp[-1].data.p;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STIND_REF) 
			++ip;
			sp -= 2;
			mono_gc_wbarrier_generic_store (sp->data.p, sp [1].data.o);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STIND_I1)
			++ip;
			sp -= 2;
			* (gint8 *) sp->data.p = (gint8)sp[1].data.i;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STIND_I2)
			++ip;
			sp -= 2;
			* (gint16 *) sp->data.p = (gint16)sp[1].data.i;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STIND_I4)
			++ip;
			sp -= 2;
			* (gint32 *) sp->data.p = sp[1].data.i;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STIND_I)
			++ip;
			sp -= 2;
			* (mono_i *) sp->data.p = (mono_i)sp[1].data.p;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STIND_I8)
			++ip;
			sp -= 2;
			* (gint64 *) sp->data.p = sp[1].data.l;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STIND_R4)
			++ip;
			sp -= 2;
			* (float *) sp->data.p = (gfloat)sp[1].data.f;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STIND_R8)
			++ip;
			sp -= 2;
			* (double *) sp->data.p = sp[1].data.f;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_MONO_ATOMIC_STORE_I4)
			++ip;
			sp -= 2;
			mono_atomic_store_i32 ((gint32 *) sp->data.p, sp [1].data.i);
			MINT_IN_BREAK;
#define BINOP(datamem, op) \
	--sp; \
	sp [-1].data.datamem = sp [-1].data.datamem op sp [0].data.datamem; \
	++ip;
		MINT_IN_CASE(MINT_ADD_I4)
			BINOP(i, +);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_ADD_I8)
			BINOP(l, +);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_ADD_R8)
			BINOP(f, +);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_ADD1_I4)
			++sp [-1].data.i;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_ADD1_I8)
			++sp [-1].data.l;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_SUB_I4)
			BINOP(i, -);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_SUB_I8)
			BINOP(l, -);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_SUB_R8)
			BINOP(f, -);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_SUB1_I4)
			--sp [-1].data.i;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_SUB1_I8)
			--sp [-1].data.l;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_MUL_I4)
			BINOP(i, *);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_MUL_I8)
			BINOP(l, *);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_MUL_R8)
			BINOP(f, *);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_DIV_I4)
			if (sp [-1].data.i == 0)
				THROW_EX (mono_get_exception_divide_by_zero (), ip);
			if (sp [-1].data.i == (-1) && sp [-2].data.i == G_MININT32)
				THROW_EX (mono_get_exception_overflow (), ip);
			BINOP(i, /);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_DIV_I8)
			if (sp [-1].data.l == 0)
				THROW_EX (mono_get_exception_divide_by_zero (), ip);
			if (sp [-1].data.l == (-1) && sp [-2].data.l == G_MININT64)
				THROW_EX (mono_get_exception_overflow (), ip);
			BINOP(l, /);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_DIV_R8)
			BINOP(f, /);
			MINT_IN_BREAK;

#define BINOP_CAST(datamem, op, type) \
	--sp; \
	sp [-1].data.datamem = (type)sp [-1].data.datamem op (type)sp [0].data.datamem; \
	++ip;
		MINT_IN_CASE(MINT_DIV_UN_I4)
			if (sp [-1].data.i == 0)
				THROW_EX (mono_get_exception_divide_by_zero (), ip);
			BINOP_CAST(i, /, guint32);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_DIV_UN_I8)
			if (sp [-1].data.l == 0)
				THROW_EX (mono_get_exception_divide_by_zero (), ip);
			BINOP_CAST(l, /, guint64);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_REM_I4)
			if (sp [-1].data.i == 0)
				THROW_EX (mono_get_exception_divide_by_zero (), ip);
			if (sp [-1].data.i == (-1) && sp [-2].data.i == G_MININT32)
				THROW_EX (mono_get_exception_overflow (), ip);
			BINOP(i, %);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_REM_I8)
			if (sp [-1].data.l == 0)
				THROW_EX (mono_get_exception_divide_by_zero (), ip);
			if (sp [-1].data.l == (-1) && sp [-2].data.l == G_MININT64)
				THROW_EX (mono_get_exception_overflow (), ip);
			BINOP(l, %);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_REM_R8)
			/* FIXME: what do we actually do here? */
			--sp;
			sp [-1].data.f = fmod (sp [-1].data.f, sp [0].data.f);
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_REM_UN_I4)
			if (sp [-1].data.i == 0)
				THROW_EX (mono_get_exception_divide_by_zero (), ip);
			BINOP_CAST(i, %, guint32);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_REM_UN_I8)
			if (sp [-1].data.l == 0)
				THROW_EX (mono_get_exception_divide_by_zero (), ip);
			BINOP_CAST(l, %, guint64);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_AND_I4)
			BINOP(i, &);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_AND_I8)
			BINOP(l, &);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_OR_I4)
			BINOP(i, |);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_OR_I8)
			BINOP(l, |);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_XOR_I4)
			BINOP(i, ^);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_XOR_I8)
			BINOP(l, ^);
			MINT_IN_BREAK;

#define SHIFTOP(datamem, op) \
	--sp; \
	sp [-1].data.datamem = sp [-1].data.datamem op sp [0].data.i; \
	++ip;

		MINT_IN_CASE(MINT_SHL_I4)
			SHIFTOP(i, <<);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_SHL_I8)
			SHIFTOP(l, <<);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_SHR_I4)
			SHIFTOP(i, >>);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_SHR_I8)
			SHIFTOP(l, >>);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_SHR_UN_I4)
			--sp;
			sp [-1].data.i = (guint32)sp [-1].data.i >> sp [0].data.i;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_SHR_UN_I8)
			--sp;
			sp [-1].data.l = (guint64)sp [-1].data.l >> sp [0].data.i;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_NEG_I4)
			sp [-1].data.i = - sp [-1].data.i;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_NEG_I8)
			sp [-1].data.l = - sp [-1].data.l;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_NEG_R8)
			sp [-1].data.f = - sp [-1].data.f;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_NOT_I4)
			sp [-1].data.i = ~ sp [-1].data.i;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_NOT_I8)
			sp [-1].data.l = ~ sp [-1].data.l;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_I1_I4)
			sp [-1].data.i = (gint8)sp [-1].data.i;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_I1_I8)
			sp [-1].data.i = (gint8)sp [-1].data.l;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_I1_R8)
			/* without gint32 cast, C compiler is allowed to use undefined
			 * behaviour if data.f is bigger than >255. See conv.fpint section
			 * in C standard:
			 * > The conversion truncates; that is, the fractional  part
			 * > is discarded.  The behavior is undefined if the truncated
			 * > value cannot be represented in the destination type.
			 * */
			sp [-1].data.i = (gint8) (gint32) sp [-1].data.f;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_U1_I4)
			sp [-1].data.i = (guint8)sp [-1].data.i;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_U1_I8)
			sp [-1].data.i = (guint8)sp [-1].data.l;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_U1_R8)
			sp [-1].data.i = (guint8) (guint32) sp [-1].data.f;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_I2_I4)
			sp [-1].data.i = (gint16)sp [-1].data.i;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_I2_I8)
			sp [-1].data.i = (gint16)sp [-1].data.l;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_I2_R8)
			sp [-1].data.i = (gint16) (gint32) sp [-1].data.f;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_U2_I4)
			sp [-1].data.i = (guint16)sp [-1].data.i;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_U2_I8)
			sp [-1].data.i = (guint16)sp [-1].data.l;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_U2_R8)
			sp [-1].data.i = (guint16) (guint32) sp [-1].data.f;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_I4_R8)
			sp [-1].data.i = (gint32)sp [-1].data.f;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_U4_I8)
		MINT_IN_CASE(MINT_CONV_I4_I8)
			sp [-1].data.i = (gint32)sp [-1].data.l;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_I4_I8_SP)
			sp [-2].data.i = (gint32)sp [-2].data.l;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_U4_R8)
			/* needed on arm64 */
			if (mono_isinf (sp [-1].data.f))
				sp [-1].data.i = 0;
			/* needed by wasm */
			else if (isnan (sp [-1].data.f))
				sp [-1].data.i = 0;
			else
				sp [-1].data.i = (guint32)sp [-1].data.f;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_I8_I4)
			sp [-1].data.l = sp [-1].data.i;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_I8_I4_SP)
			sp [-2].data.l = sp [-2].data.i;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_I8_U4)
			sp [-1].data.l = (guint32)sp [-1].data.i;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_I8_R8)
			sp [-1].data.l = (gint64)sp [-1].data.f;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_R4_I4)
			sp [-1].data.f = (float)sp [-1].data.i;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_R4_I8)
			sp [-1].data.f = (float)sp [-1].data.l;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_R4_R8)
			sp [-1].data.f = (float)sp [-1].data.f;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_R8_I4)
			sp [-1].data.f = (double)sp [-1].data.i;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_R8_I8)
			sp [-1].data.f = (double)sp [-1].data.l;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_U8_I4)
			sp [-1].data.l = sp [-1].data.i & 0xffffffff;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_U8_R8)
			sp [-1].data.l = (guint64)sp [-1].data.f;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CPOBJ) {
			c = (MonoClass*)rtm->data_items[* (guint16 *)(ip + 1)];
			g_assert (m_class_is_valuetype (c));
			/* if this assertion fails, we need to add a write barrier */
			g_assert (!MONO_TYPE_IS_REFERENCE (m_class_get_byval_arg (c)));
			if (mint_type (m_class_get_byval_arg (c)) == MINT_TYPE_VT)
				stackval_from_data (m_class_get_byval_arg (c), &sp [-2], sp [-1].data.p, FALSE);
			else
				stackval_from_data (m_class_get_byval_arg (c), (stackval*)sp [-2].data.p, sp [-1].data.p, FALSE);
			ip += 2;
			sp -= 2;
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_LDOBJ) {
			void *p;
			c = (MonoClass*)rtm->data_items[* (guint16 *)(ip + 1)];
			ip += 2;
			p = sp [-1].data.p;
			if (mint_type (m_class_get_byval_arg (c)) == MINT_TYPE_VT && !m_class_is_enumtype (c)) {
				int size = mono_class_value_size (c, NULL);
				sp [-1].data.p = vt_sp;
				vt_sp += ALIGN_TO (size, MINT_VT_ALIGNMENT);
			}
			stackval_from_data (m_class_get_byval_arg (c), &sp [-1], p, FALSE);
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_LDSTR)
			sp->data.p = rtm->data_items [* (guint16 *)(ip + 1)];
			++sp;
			ip += 2;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDSTR_TOKEN) {
			MonoString *s = NULL;
			guint32 strtoken = (guint32)(gsize)rtm->data_items [* (guint16 *)(ip + 1)];

			MonoMethod *method = frame->imethod->method;
			if (method->wrapper_type == MONO_WRAPPER_DYNAMIC_METHOD) {
				s = (MonoString*)mono_method_get_wrapper_data (method, strtoken);
			} else if (method->wrapper_type != MONO_WRAPPER_NONE) {
				s = mono_string_new_wrapper ((const char*)mono_method_get_wrapper_data (method, strtoken));
			} else {
				g_assert_not_reached ();
			}
			sp->data.p = s;
			++sp;
			ip += 2;
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_NEWOBJ_ARRAY) {
			MonoClass *newobj_class;
			InterpMethod *imethod;
			guint32 token = * (guint16 *)(ip + 1);
			guint16 param_count = * (guint16 *)(ip + 2);

			imethod = (InterpMethod*) rtm->data_items [token];
			newobj_class = imethod->method->klass;

			sp -= param_count;
			sp->data.p = ves_array_create (rtm->domain, newobj_class, param_count, sp, error);
			if (!mono_error_ok (error))
				THROW_EX (mono_error_convert_to_exception (error), ip);

			++sp;
			ip += 3;
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_NEWOBJ_FAST)
		MINT_IN_CASE(MINT_NEWOBJ_VT_FAST)
		MINT_IN_CASE(MINT_NEWOBJ_VTST_FAST) {
			guint16 param_count;
			gboolean vt = *ip != MINT_NEWOBJ_FAST;
			stackval valuetype_this;

			frame->ip = ip;

			child_frame.imethod = (InterpMethod*) rtm->data_items [*(guint16*)(ip + 1)];
			param_count = *(guint16*)(ip + 2);

			if (param_count) {
				sp -= param_count;
				memmove (sp + 1, sp, param_count * sizeof (stackval));
			}
			child_frame.stack_args = sp;

			if (vt) {
				gboolean vtst = *ip == MINT_NEWOBJ_VTST_FAST;
				memset (&valuetype_this, 0, sizeof (stackval));
				if (vtst) {
					sp->data.p = vt_sp;
					valuetype_this.data.p = vt_sp;
				} else {
					sp->data.p = &valuetype_this;
				}
			} else {
				MonoVTable *vtable = (MonoVTable*) rtm->data_items [*(guint16*)(ip + 3)];
				if (G_UNLIKELY (!vtable->initialized)) {
					mono_runtime_class_init_full (vtable, error);
					if (!mono_error_ok (error))
						THROW_EX (mono_error_convert_to_exception (error), ip);
				}
				o = mono_gc_alloc_obj (vtable, m_class_get_instance_size (vtable->klass));
				if (G_UNLIKELY (!o)) {
					mono_error_set_out_of_memory (error, "Could not allocate %i bytes", m_class_get_instance_size (vtable->klass));
					THROW_EX (mono_error_convert_to_exception (error), ip);
				}
				sp->data.p = o;
			}

			interp_exec_method (&child_frame, context);

			context->current_frame = frame;

			if (context->has_resume_state) {
				if (frame == context->handler_frame)
					SET_RESUME_STATE (context);
				else
					goto exit_frame;
			}
			CHECK_CHILD_EX (child_frame, ip);
			if (vt)
				*sp = valuetype_this;
			else
				sp->data.p = o;
			ip += 4 - vt;
			++sp;
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_NEWOBJ) {
			MonoClass *newobj_class;
			MonoMethodSignature *csig;
			stackval valuetype_this;
			guint32 token;
			stackval retval;

			frame->ip = ip;

			token = * (guint16 *)(ip + 1);
			ip += 2;

			child_frame.ip = NULL;
			child_frame.ex = NULL;

			child_frame.imethod = (InterpMethod*)rtm->data_items [token];
			csig = mono_method_signature (child_frame.imethod->method);
			newobj_class = child_frame.imethod->method->klass;
			/*if (profiling_classes) {
				guint count = GPOINTER_TO_UINT (g_hash_table_lookup (profiling_classes, newobj_class));
				count++;
				g_hash_table_insert (profiling_classes, newobj_class, GUINT_TO_POINTER (count));
			}*/

			g_assert (csig->hasthis);
			if (csig->param_count) {
				sp -= csig->param_count;
				memmove (sp + 1, sp, csig->param_count * sizeof (stackval));
			}
			child_frame.stack_args = sp;

			/*
			 * First arg is the object.
			 */
			if (m_class_is_valuetype (newobj_class)) {
				MonoType *t = m_class_get_byval_arg (newobj_class);
				memset (&valuetype_this, 0, sizeof (stackval));
				if (!m_class_is_enumtype (newobj_class) && (t->type == MONO_TYPE_VALUETYPE || (t->type == MONO_TYPE_GENERICINST && mono_type_generic_inst_is_valuetype (t)))) {
					sp->data.p = vt_sp;
					valuetype_this.data.p = vt_sp;
				} else {
					sp->data.p = &valuetype_this;
				}
			} else {
				if (newobj_class != mono_defaults.string_class) {
					MonoVTable *vtable = mono_class_vtable_checked (rtm->domain, newobj_class, error);
					if (!mono_error_ok (error) || !mono_runtime_class_init_full (vtable, error))
						THROW_EX (mono_error_convert_to_exception (error), ip);
					o = mono_object_new_checked (rtm->domain, newobj_class, error);
					mono_error_cleanup (error); /* FIXME: don't swallow the error */
					EXCEPTION_CHECKPOINT;
					sp->data.p = o;
#ifndef DISABLE_REMOTING
					if (mono_object_is_transparent_proxy (o)) {
						MonoMethod *remoting_invoke_method = mono_marshal_get_remoting_invoke_with_check (child_frame.imethod->method, error);
						mono_error_assert_ok (error);
						child_frame.imethod = mono_interp_get_imethod (rtm->domain, remoting_invoke_method, error);
						mono_error_assert_ok (error);
					}
#endif
				} else {
					sp->data.p = NULL;
					child_frame.retval = &retval;
				}
			}

			interp_exec_method (&child_frame, context);

			context->current_frame = frame;

			if (context->has_resume_state) {
				if (frame == context->handler_frame)
					SET_RESUME_STATE (context);
				else
					goto exit_frame;
			}

			CHECK_CHILD_EX (child_frame, ip - 2);
			/*
			 * a constructor returns void, but we need to return the object we created
			 */
			if (m_class_is_valuetype (newobj_class) && !m_class_is_enumtype (newobj_class)) {
				*sp = valuetype_this;
			} else if (newobj_class == mono_defaults.string_class) {
				*sp = retval;
			} else {
				sp->data.p = o;
			}
			++sp;
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_NEWOBJ_MAGIC) {
			frame->ip = ip;
			ip += 2;

			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_INTRINS_BYREFERENCE_CTOR) {
			MonoMethodSignature *csig;
			guint32 token;

			frame->ip = ip;
			token = * (guint16 *)(ip + 1);
			ip += 2;

			InterpMethod *cmethod = (InterpMethod*)rtm->data_items [token];
			csig = mono_method_signature (cmethod->method);

			g_assert (csig->hasthis);
			sp -= csig->param_count;

			gpointer arg0 = sp [0].data.p;

			gpointer *byreference_this = (gpointer*)vt_sp;
			*byreference_this = arg0;

			/* Followed by a VTRESULT opcode which will push the result on the stack */
			++sp;
			MINT_IN_BREAK;
			break;
		}
		MINT_IN_CASE(MINT_INTRINS_BYREFERENCE_GET_VALUE) {
			gpointer *byreference_this = (gpointer*)sp [-1].data.p;
			sp [-1].data.p = *byreference_this;
			++ip;
			MINT_IN_BREAK;
			break;
		}
		MINT_IN_CASE(MINT_CASTCLASS)
			c = (MonoClass*)rtm->data_items [*(guint16 *)(ip + 1)];
			if ((o = sp [-1].data.o)) {
				MonoObject *isinst_obj = mono_object_isinst_checked (o, c, error);
				mono_error_cleanup (error); /* FIXME: don't swallow the error */
				if (!isinst_obj)
					THROW_EX (mono_get_exception_invalid_cast (), ip);
			}
			ip += 2;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_ISINST)
			c = (MonoClass*)rtm->data_items [*(guint16 *)(ip + 1)];
			if ((o = sp [-1].data.o)) {
				MonoObject *isinst_obj = mono_object_isinst_checked (o, c, error);
				mono_error_cleanup (error); /* FIXME: don't swallow the error */
				if (!isinst_obj)
					sp [-1].data.p = NULL;
			}
			ip += 2;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_R_UN_I4)
			sp [-1].data.f = (double)(guint32)sp [-1].data.i;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_R_UN_I8)
			sp [-1].data.f = (double)(guint64)sp [-1].data.l;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_UNBOX)
			c = (MonoClass*)rtm->data_items[*(guint16 *)(ip + 1)];
			
			o = sp [-1].data.o;
			if (!o)
				THROW_EX (mono_get_exception_null_reference (), ip);

			MonoObject *isinst_obj;
			isinst_obj = mono_object_isinst_checked (o, c, error);
			mono_error_cleanup (error); /* FIXME: don't swallow the error */
			if (!(isinst_obj || ((m_class_get_rank (o->vtable->klass) == 0) && (m_class_get_element_class (o->vtable->klass) == m_class_get_element_class (c)))))
				THROW_EX (mono_get_exception_invalid_cast (), ip);

			sp [-1].data.p = mono_object_unbox (o);
			ip += 2;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_THROW)
			--sp;
			frame->ex_handler = NULL;
			if (!sp->data.p)
				sp->data.p = mono_get_exception_null_reference ();

			THROW_EX ((MonoException *)sp->data.p, ip);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CHECKPOINT)
			/* Do synchronous checking of abort requests */
			EXCEPTION_CHECKPOINT;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_SAFEPOINT)
			/* Do synchronous checking of abort requests */
			EXCEPTION_CHECKPOINT;
			/* Poll safepoint */
			mono_threads_safepoint ();
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDFLDA_UNSAFE)
			o = sp [-1].data.o;
			sp[-1].data.p = (char *)o + * (guint16 *)(ip + 1);
			ip += 2;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDFLDA)
			o = sp [-1].data.o;
			if (!o)
				THROW_EX (mono_get_exception_null_reference (), ip);
			sp[-1].data.p = (char *)o + * (guint16 *)(ip + 1);
			ip += 2;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CKNULL)
			o = sp [-1].data.o;
			if (!o)
				THROW_EX (mono_get_exception_null_reference (), ip);
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CKNULL_N) {
			/* Same as CKNULL, but further down the stack */
			int n = *(guint16*)(ip + 1);
			o = sp [-n].data.o;
			if (!o)
				THROW_EX (mono_get_exception_null_reference (), ip);
			ip += 2;
			MINT_IN_BREAK;
		}

#define LDFLD(datamem, fieldtype) \
	o = sp [-1].data.o; \
	if (!o) \
		THROW_EX (mono_get_exception_null_reference (), ip); \
	sp[-1].data.datamem = * (fieldtype *)((char *)o + * (guint16 *)(ip + 1)) ; \
	ip += 2;

		MINT_IN_CASE(MINT_LDFLD_I1) LDFLD(i, gint8); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDFLD_U1) LDFLD(i, guint8); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDFLD_I2) LDFLD(i, gint16); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDFLD_U2) LDFLD(i, guint16); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDFLD_I4) LDFLD(i, gint32); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDFLD_I8) LDFLD(l, gint64); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDFLD_R4) LDFLD(f, float); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDFLD_R8) LDFLD(f, double); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDFLD_O) LDFLD(p, gpointer); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDFLD_P) LDFLD(p, gpointer); MINT_IN_BREAK;

		MINT_IN_CASE(MINT_LDFLD_VT) {
			o = sp [-1].data.o;
			if (!o)
				THROW_EX (mono_get_exception_null_reference (), ip);

			MonoClassField *field = (MonoClassField*)rtm->data_items[* (guint16 *)(ip + 2)];
			MonoClass *klass = mono_class_from_mono_type (field->type);
			i32 = mono_class_value_size (klass, NULL);

			sp [-1].data.p = vt_sp;
			memcpy (sp [-1].data.p, (char *)o + * (guint16 *)(ip + 1), i32);
			vt_sp += ALIGN_TO (i32, MINT_VT_ALIGNMENT);
			ip += 3;
			MINT_IN_BREAK;
		}

		MINT_IN_CASE(MINT_LDRMFLD) {
			MonoClassField *field;
			char *addr;

			o = sp [-1].data.o;
			if (!o)
				THROW_EX (mono_get_exception_null_reference (), ip);
			field = (MonoClassField*)rtm->data_items[* (guint16 *)(ip + 1)];
			ip += 2;
#ifndef DISABLE_REMOTING
			if (mono_object_is_transparent_proxy (o)) {
				gpointer tmp;
				MonoClass *klass = ((MonoTransparentProxy*)o)->remote_class->proxy_class;

				addr = (char*)mono_load_remote_field_checked (o, klass, field, &tmp, error);
				mono_error_cleanup (error); /* FIXME: don't swallow the error */
			} else
#endif
				addr = (char*)o + field->offset;

			stackval_from_data (field->type, &sp [-1], addr, FALSE);
			MINT_IN_BREAK;
		}

		MINT_IN_CASE(MINT_LDRMFLD_VT) {
			MonoClassField *field;
			char *addr;

			o = sp [-1].data.o;
			if (!o)
				THROW_EX (mono_get_exception_null_reference (), ip);

			field = (MonoClassField*)rtm->data_items[* (guint16 *)(ip + 1)];
			MonoClass *klass = mono_class_from_mono_type (field->type);
			i32 = mono_class_value_size (klass, NULL);
	
			ip += 2;
#ifndef DISABLE_REMOTING
			if (mono_object_is_transparent_proxy (o)) {
				gpointer tmp;
				MonoClass *klass = ((MonoTransparentProxy*)o)->remote_class->proxy_class;
				addr = (char*)mono_load_remote_field_checked (o, klass, field, &tmp, error);
				mono_error_cleanup (error); /* FIXME: don't swallow the error */
			} else
#endif
				addr = (char*)o + field->offset;

			sp [-1].data.p = vt_sp;
			memcpy(sp [-1].data.p, (char *)o + * (guint16 *)(ip + 1), i32);
			vt_sp += ALIGN_TO (i32, MINT_VT_ALIGNMENT);
			memcpy(sp [-1].data.p, addr, i32);
			MINT_IN_BREAK;
		}

#define STFLD(datamem, fieldtype) \
	o = sp [-2].data.o; \
	if (!o) \
		THROW_EX (mono_get_exception_null_reference (), ip); \
	sp -= 2; \
	* (fieldtype *)((char *)o + * (guint16 *)(ip + 1)) = sp[1].data.datamem; \
	ip += 2;

		MINT_IN_CASE(MINT_STFLD_I1) STFLD(i, gint8); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STFLD_U1) STFLD(i, guint8); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STFLD_I2) STFLD(i, gint16); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STFLD_U2) STFLD(i, guint16); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STFLD_I4) STFLD(i, gint32); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STFLD_I8) STFLD(l, gint64); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STFLD_R4) STFLD(f, float); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STFLD_R8) STFLD(f, double); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STFLD_P) STFLD(p, gpointer); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STFLD_O)
			o = sp [-2].data.o;
			if (!o)
				THROW_EX (mono_get_exception_null_reference (), ip);
			sp -= 2;
			mono_gc_wbarrier_set_field (o, (char *) o + * (guint16 *)(ip + 1), sp [1].data.o);
			ip += 2;
			MINT_IN_BREAK;

		MINT_IN_CASE(MINT_STFLD_VT) {
			o = sp [-2].data.o;
			if (!o)
				THROW_EX (mono_get_exception_null_reference (), ip);
			sp -= 2;

			MonoClassField *field = (MonoClassField*)rtm->data_items[* (guint16 *)(ip + 2)];
			MonoClass *klass = mono_class_from_mono_type (field->type);
			i32 = mono_class_value_size (klass, NULL);

			guint16 offset = * (guint16 *)(ip + 1);
			mono_value_copy ((char *) o + offset, sp [1].data.p, klass);

			vt_sp -= ALIGN_TO (i32, MINT_VT_ALIGNMENT);
			ip += 3;
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_STRMFLD) {
			MonoClassField *field;

			o = sp [-2].data.o;
			if (!o)
				THROW_EX (mono_get_exception_null_reference (), ip);
			
			field = (MonoClassField*)rtm->data_items[* (guint16 *)(ip + 1)];
			ip += 2;

#ifndef DISABLE_REMOTING
			if (mono_object_is_transparent_proxy (o)) {
				MonoClass *klass = ((MonoTransparentProxy*)o)->remote_class->proxy_class;
				mono_store_remote_field_checked (o, klass, field, &sp [-1].data, error);
				mono_error_cleanup (error); /* FIXME: don't swallow the error */
			} else
#endif
				stackval_to_data (field->type, &sp [-1], (char*)o + field->offset, FALSE);

			sp -= 2;
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_STRMFLD_VT) {
			MonoClassField *field;

			o = sp [-2].data.o;
			if (!o)
				THROW_EX (mono_get_exception_null_reference (), ip);
			field = (MonoClassField*)rtm->data_items[* (guint16 *)(ip + 1)];
			MonoClass *klass = mono_class_from_mono_type (field->type);
			i32 = mono_class_value_size (klass, NULL);
			ip += 2;

#ifndef DISABLE_REMOTING
			if (mono_object_is_transparent_proxy (o)) {
				MonoClass *klass = ((MonoTransparentProxy*)o)->remote_class->proxy_class;
				mono_store_remote_field_checked (o, klass, field, &sp [-1].data, error);
				mono_error_cleanup (error); /* FIXME: don't swallow the error */
			} else
#endif
				mono_value_copy ((char *) o + field->offset, sp [-1].data.p, klass);

			sp -= 2;
			vt_sp -= ALIGN_TO (i32, MINT_VT_ALIGNMENT);
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_LDSFLDA) {
			MonoClassField *field = (MonoClassField*)rtm->data_items[*(guint16 *)(ip + 1)];
			sp->data.p = mono_class_static_field_address (rtm->domain, field);
			EXCEPTION_CHECKPOINT;
			ip += 2;
			++sp;
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_LDSFLD) {
			MonoClassField *field = (MonoClassField*)rtm->data_items [* (guint16 *)(ip + 1)];
			gpointer addr = mono_class_static_field_address (rtm->domain, field);
			EXCEPTION_CHECKPOINT;
			stackval_from_data (field->type, sp, addr, FALSE);
			ip += 2;
			++sp;
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_LDSFLD_VT) {
			MonoClassField *field = (MonoClassField*)rtm->data_items [* (guint16 *)(ip + 1)];
			gpointer addr = mono_class_static_field_address (rtm->domain, field);
			EXCEPTION_CHECKPOINT;
			int size = READ32 (ip + 2);
			ip += 4;

			sp->data.p = vt_sp;
			vt_sp += ALIGN_TO (size, MINT_VT_ALIGNMENT);
			stackval_from_data (field->type, sp, addr, FALSE);
			++sp;
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_STSFLD) {
			MonoClassField *field = (MonoClassField*)rtm->data_items [* (guint16 *)(ip + 1)];
			gpointer addr = mono_class_static_field_address (rtm->domain, field);
			EXCEPTION_CHECKPOINT;
			ip += 2;
			--sp;
			stackval_to_data (field->type, sp, addr, FALSE);
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_STSFLD_VT) {
			MonoClassField *field = (MonoClassField*)rtm->data_items [* (guint16 *)(ip + 1)];
			gpointer addr = mono_class_static_field_address (rtm->domain, field);
			EXCEPTION_CHECKPOINT;
			MonoClass *klass = mono_class_from_mono_type (field->type);
			i32 = mono_class_value_size (klass, NULL);
			ip += 2;

			--sp;
			stackval_to_data (field->type, sp, addr, FALSE);
			vt_sp -= ALIGN_TO (i32, MINT_VT_ALIGNMENT);
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_STOBJ_VT) {
			int size;
			c = (MonoClass*)rtm->data_items[* (guint16 *)(ip + 1)];
			ip += 2;
			size = mono_class_value_size (c, NULL);
			memcpy(sp [-2].data.p, sp [-1].data.p, size);
			vt_sp -= ALIGN_TO (size, MINT_VT_ALIGNMENT);
			sp -= 2;
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_STOBJ) {
			c = (MonoClass*)rtm->data_items[* (guint16 *)(ip + 1)];
			ip += 2;

			g_assert (!m_class_get_byval_arg (c)->byref);
			if (MONO_TYPE_IS_REFERENCE (m_class_get_byval_arg (c)))
				mono_gc_wbarrier_generic_store (sp [-2].data.o, sp [-1].data.o);
			else
				stackval_to_data (m_class_get_byval_arg (c), &sp [-1], sp [-2].data.p, FALSE);
			sp -= 2;
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_CONV_OVF_I4_UN_R8)
			if (sp [-1].data.f < 0 || sp [-1].data.f > G_MAXINT32)
				THROW_EX (mono_get_exception_overflow (), ip);
			sp [-1].data.i = (gint32)sp [-1].data.f;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_OVF_U8_I4)
			if (sp [-1].data.i < 0)
				THROW_EX (mono_get_exception_overflow (), ip);
			sp [-1].data.l = sp [-1].data.i;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_OVF_U8_I8)
			if (sp [-1].data.l < 0)
				THROW_EX (mono_get_exception_overflow (), ip);
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_OVF_I8_U8)
			if ((guint64) sp [-1].data.l > G_MAXINT64)
				THROW_EX (mono_get_exception_overflow (), ip);
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_OVF_U8_R8)
			if (sp [-1].data.f < 0 || sp [-1].data.f > G_MAXUINT64)
				THROW_EX (mono_get_exception_overflow (), ip);
			sp [-1].data.l = (guint64)sp [-1].data.f;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_OVF_I8_UN_R8)
			if (sp [-1].data.f < 0 || sp [-1].data.f > G_MAXINT64)
				THROW_EX (mono_get_exception_overflow (), ip);
			sp [-1].data.l = (gint64)sp [-1].data.f;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_OVF_I8_R8)
			if (sp [-1].data.f < G_MININT64 || sp [-1].data.f > G_MAXINT64)
				THROW_EX (mono_get_exception_overflow (), ip);
			sp [-1].data.l = (gint64)sp [-1].data.f;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_OVF_I4_UN_I8)
			if ((guint64)sp [-1].data.l > G_MAXINT32)
				THROW_EX (mono_get_exception_overflow (), ip);
			sp [-1].data.i = (gint32)sp [-1].data.l;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_BOX) {
			c = (MonoClass*)rtm->data_items [* (guint16 *)(ip + 1)];
			guint16 offset = * (guint16 *)(ip + 2);
			gboolean pop_vt_sp = !(offset & BOX_NOT_CLEAR_VT_SP);
			offset &= ~BOX_NOT_CLEAR_VT_SP;

			if (mint_type (m_class_get_byval_arg (c)) == MINT_TYPE_VT && !m_class_is_enumtype (c) && !(mono_class_is_magic_int (c) || mono_class_is_magic_float (c))) {
				int size = mono_class_value_size (c, NULL);
				sp [-1 - offset].data.p = mono_value_box_checked (rtm->domain, c, sp [-1 - offset].data.p, error);
				mono_error_cleanup (error); /* FIXME: don't swallow the error */
				size = ALIGN_TO (size, MINT_VT_ALIGNMENT);
				if (pop_vt_sp)
					vt_sp -= size;
			} else {
				stackval_to_data (m_class_get_byval_arg (c), &sp [-1 - offset], (char *) &sp [-1 - offset], FALSE);
				sp [-1 - offset].data.p = mono_value_box_checked (rtm->domain, c, &sp [-1 - offset], error);
				mono_error_cleanup (error); /* FIXME: don't swallow the error */
			}
			ip += 3;
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_NEWARR)
			sp [-1].data.p = (MonoObject*) mono_array_new_checked (rtm->domain, (MonoClass*)rtm->data_items[*(guint16 *)(ip + 1)], sp [-1].data.i, error);
			if (!mono_error_ok (error)) {
				THROW_EX (mono_error_convert_to_exception (error), ip);
			}
			mono_error_cleanup (error); /* FIXME: don't swallow the error */
			ip += 2;
			/*if (profiling_classes) {
				guint count = GPOINTER_TO_UINT (g_hash_table_lookup (profiling_classes, o->vtable->klass));
				count++;
				g_hash_table_insert (profiling_classes, o->vtable->klass, GUINT_TO_POINTER (count));
			}*/

			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDLEN)
			o = sp [-1].data.o;
			if (!o)
				THROW_EX (mono_get_exception_null_reference (), ip);
			sp [-1].data.nati = mono_array_length_fast ((MonoArray *)o);
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDLEN_SPAN) {
			o = sp [-1].data.o;
			gsize offset_length = (gsize) *(gint16 *) (ip + 1);
			if (!o)
				THROW_EX (mono_get_exception_null_reference (), ip);
			sp [-1].data.nati = *(gint32 *) ((guint8 *) o + offset_length);
			ip += 2;
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_GETCHR) {
			MonoString *s;
			s = (MonoString*)sp [-2].data.p;
			if (!s)
				THROW_EX (mono_get_exception_null_reference (), ip);
			i32 = sp [-1].data.i;
			if (i32 < 0 || i32 >= mono_string_length (s))
				THROW_EX (mono_get_exception_index_out_of_range (), ip);
			--sp;
			sp [-1].data.i = mono_string_chars(s)[i32];
			++ip;
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_GETITEM_SPAN) {
			guint8 *span = (guint8 *) sp [-2].data.p;
			int index = sp [-1].data.i;
			gsize element_size = (gsize) *(gint16 *) (ip + 1);
			gsize offset_length = (gsize) *(gint16 *) (ip + 2);
			gsize offset_pointer = (gsize) *(gint16 *) (ip + 3);
			sp--;

			if (!span)
				THROW_EX (mono_get_exception_null_reference (), ip);

			gint32 length = *(gint32 *) (span + offset_length);
			if (index < 0 || index >= length)
				THROW_EX (mono_get_exception_index_out_of_range (), ip);

			gpointer pointer = *(gpointer *)(span + offset_pointer);
			sp [-1].data.p = (guint8 *) pointer + index * element_size;

			ip += 4;
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_STRLEN)
			++ip;
			o = sp [-1].data.o;
			if (!o)
				THROW_EX (mono_get_exception_null_reference (), ip);
			sp [-1].data.i = mono_string_length ((MonoString*) o);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_ARRAY_RANK)
			o = sp [-1].data.o;
			if (!o)
				THROW_EX (mono_get_exception_null_reference (), ip);
			sp [-1].data.i = m_class_get_rank (mono_object_class (sp [-1].data.p));
			ip++;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDELEMA)
		MINT_IN_CASE(MINT_LDELEMA_TC) {
			gboolean needs_typecheck = *ip == MINT_LDELEMA_TC;
			
			MonoClass *klass = (MonoClass*)rtm->data_items [*(guint16 *) (ip + 1)];
			guint16 numargs = *(guint16 *) (ip + 2);
			ip += 3;
			sp -= numargs;

			o = sp [0].data.o;
			sp->data.p = ves_array_element_address (frame, klass, (MonoArray *) o, &sp [1], needs_typecheck);
			if (frame->ex)
				THROW_EX (frame->ex, ip);
			++sp;

			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_LDELEM_I1) /* fall through */
		MINT_IN_CASE(MINT_LDELEM_U1) /* fall through */
		MINT_IN_CASE(MINT_LDELEM_I2) /* fall through */
		MINT_IN_CASE(MINT_LDELEM_U2) /* fall through */
		MINT_IN_CASE(MINT_LDELEM_I4) /* fall through */
		MINT_IN_CASE(MINT_LDELEM_U4) /* fall through */
		MINT_IN_CASE(MINT_LDELEM_I8)  /* fall through */
		MINT_IN_CASE(MINT_LDELEM_I)  /* fall through */
		MINT_IN_CASE(MINT_LDELEM_R4) /* fall through */
		MINT_IN_CASE(MINT_LDELEM_R8) /* fall through */
		MINT_IN_CASE(MINT_LDELEM_REF) /* fall through */
		MINT_IN_CASE(MINT_LDELEM_VT) {
			MonoArray *o;
			mono_u aindex;

			sp -= 2;

			o = (MonoArray*)sp [0].data.p;
			if (!o)
				THROW_EX (mono_get_exception_null_reference (), ip);

			aindex = sp [1].data.i;
			if (aindex >= mono_array_length_fast (o))
				THROW_EX (mono_get_exception_index_out_of_range (), ip);

			/*
			 * FIXME: throw mono_get_exception_array_type_mismatch () if needed 
			 */
			switch (*ip) {
			case MINT_LDELEM_I1:
				sp [0].data.i = mono_array_get_fast (o, gint8, aindex);
				break;
			case MINT_LDELEM_U1:
				sp [0].data.i = mono_array_get_fast (o, guint8, aindex);
				break;
			case MINT_LDELEM_I2:
				sp [0].data.i = mono_array_get_fast (o, gint16, aindex);
				break;
			case MINT_LDELEM_U2:
				sp [0].data.i = mono_array_get_fast (o, guint16, aindex);
				break;
			case MINT_LDELEM_I:
				sp [0].data.nati = mono_array_get_fast (o, mono_i, aindex);
				break;
			case MINT_LDELEM_I4:
				sp [0].data.i = mono_array_get_fast (o, gint32, aindex);
				break;
			case MINT_LDELEM_U4:
				sp [0].data.i = mono_array_get_fast (o, guint32, aindex);
				break;
			case MINT_LDELEM_I8:
				sp [0].data.l = mono_array_get_fast (o, guint64, aindex);
				break;
			case MINT_LDELEM_R4:
				sp [0].data.f = mono_array_get_fast (o, float, aindex);
				break;
			case MINT_LDELEM_R8:
				sp [0].data.f = mono_array_get_fast (o, double, aindex);
				break;
			case MINT_LDELEM_REF:
				sp [0].data.p = mono_array_get_fast (o, gpointer, aindex);
				break;
			case MINT_LDELEM_VT: {
				MonoClass *klass_vt = (MonoClass*)rtm->data_items [*(guint16 *) (ip + 1)];
				i32 = READ32 (ip + 2);
				char *src_addr = mono_array_addr_with_size_fast ((MonoArray *) o, i32, aindex);
				sp [0].data.vt = vt_sp;
				stackval_from_data (m_class_get_byval_arg (klass_vt), sp, src_addr, FALSE);
				vt_sp += ALIGN_TO (i32, MINT_VT_ALIGNMENT);
				ip += 3;
				break;
			}
			default:
				ves_abort();
			}

			++ip;
			++sp;
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_STELEM_I)  /* fall through */
		MINT_IN_CASE(MINT_STELEM_I1) /* fall through */ 
		MINT_IN_CASE(MINT_STELEM_U1) /* fall through */
		MINT_IN_CASE(MINT_STELEM_I2) /* fall through */
		MINT_IN_CASE(MINT_STELEM_U2) /* fall through */
		MINT_IN_CASE(MINT_STELEM_I4) /* fall through */
		MINT_IN_CASE(MINT_STELEM_I8) /* fall through */
		MINT_IN_CASE(MINT_STELEM_R4) /* fall through */
		MINT_IN_CASE(MINT_STELEM_R8) /* fall through */
		MINT_IN_CASE(MINT_STELEM_REF) /* fall through */
		MINT_IN_CASE(MINT_STELEM_VT) {
			mono_u aindex;

			sp -= 3;

			o = sp [0].data.o;
			if (!o)
				THROW_EX (mono_get_exception_null_reference (), ip);

			aindex = sp [1].data.i;
			if (aindex >= mono_array_length_fast ((MonoArray *)o))
				THROW_EX (mono_get_exception_index_out_of_range (), ip);

			switch (*ip) {
			case MINT_STELEM_I:
				mono_array_set_fast ((MonoArray *)o, mono_i, aindex, sp [2].data.nati);
				break;
			case MINT_STELEM_I1:
				mono_array_set_fast ((MonoArray *)o, gint8, aindex, sp [2].data.i);
				break;
			case MINT_STELEM_U1:
				mono_array_set_fast ((MonoArray *) o, guint8, aindex, sp [2].data.i);
				break;
			case MINT_STELEM_I2:
				mono_array_set_fast ((MonoArray *)o, gint16, aindex, sp [2].data.i);
				break;
			case MINT_STELEM_U2:
				mono_array_set_fast ((MonoArray *)o, guint16, aindex, sp [2].data.i);
				break;
			case MINT_STELEM_I4:
				mono_array_set_fast ((MonoArray *)o, gint32, aindex, sp [2].data.i);
				break;
			case MINT_STELEM_I8:
				mono_array_set_fast ((MonoArray *)o, gint64, aindex, sp [2].data.l);
				break;
			case MINT_STELEM_R4:
				mono_array_set_fast ((MonoArray *)o, float, aindex, sp [2].data.f);
				break;
			case MINT_STELEM_R8:
				mono_array_set_fast ((MonoArray *)o, double, aindex, sp [2].data.f);
				break;
			case MINT_STELEM_REF: {
				MonoObject *isinst_obj = mono_object_isinst_checked (sp [2].data.o, m_class_get_element_class (mono_object_class (o)), error);
				mono_error_cleanup (error); /* FIXME: don't swallow the error */
				if (sp [2].data.p && !isinst_obj)
					THROW_EX (mono_get_exception_array_type_mismatch (), ip);
				mono_array_setref_fast ((MonoArray *) o, aindex, sp [2].data.p);
				break;
			}
			case MINT_STELEM_VT: {
				MonoClass *klass_vt = (MonoClass*)rtm->data_items [*(guint16 *) (ip + 1)];
				i32 = READ32 (ip + 2);
				char *dst_addr = mono_array_addr_with_size_fast ((MonoArray *) o, i32, aindex);

				stackval_to_data (m_class_get_byval_arg (klass_vt), &sp [2], dst_addr, FALSE);
				vt_sp -= ALIGN_TO (i32, MINT_VT_ALIGNMENT);
				ip += 3;
				break;
			}
			default:
				ves_abort();
			}

			++ip;
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_CONV_OVF_I4_U4)
			if (sp [-1].data.i < 0)
				THROW_EX (mono_get_exception_overflow (), ip);
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_OVF_I4_I8)
			if (sp [-1].data.l < G_MININT32 || sp [-1].data.l > G_MAXINT32)
				THROW_EX (mono_get_exception_overflow (), ip);
			sp [-1].data.i = (gint32) sp [-1].data.l;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_OVF_I4_U8)
			if (sp [-1].data.l < 0 || sp [-1].data.l > G_MAXINT32)
				THROW_EX (mono_get_exception_overflow (), ip);
			sp [-1].data.i = (gint32) sp [-1].data.l;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_OVF_I4_R8)
			if (sp [-1].data.f < G_MININT32 || sp [-1].data.f > G_MAXINT32)
				THROW_EX (mono_get_exception_overflow (), ip);
			sp [-1].data.i = (gint32) sp [-1].data.f;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_OVF_U4_I4)
			if (sp [-1].data.i < 0)
				THROW_EX (mono_get_exception_overflow (), ip);
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_OVF_U4_I8)
			if (sp [-1].data.l < 0 || sp [-1].data.l > G_MAXUINT32)
				THROW_EX (mono_get_exception_overflow (), ip);
			sp [-1].data.i = (guint32) sp [-1].data.l;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_OVF_U4_R8)
			if (sp [-1].data.f < 0 || sp [-1].data.f > G_MAXUINT32)
				THROW_EX (mono_get_exception_overflow (), ip);
			sp [-1].data.i = (guint32) sp [-1].data.f;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_OVF_I2_I4)
			if (sp [-1].data.i < G_MININT16 || sp [-1].data.i > G_MAXINT16)
				THROW_EX (mono_get_exception_overflow (), ip);
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_OVF_I2_U4)
			if (sp [-1].data.i < 0 || sp [-1].data.i > G_MAXINT16)
				THROW_EX (mono_get_exception_overflow (), ip);
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_OVF_I2_I8)
			if (sp [-1].data.l < G_MININT16 || sp [-1].data.l > G_MAXINT16)
				THROW_EX (mono_get_exception_overflow (), ip);
			sp [-1].data.i = (gint16) sp [-1].data.l;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_OVF_I2_U8)
			if (sp [-1].data.l < 0 || sp [-1].data.l > G_MAXINT16)
				THROW_EX (mono_get_exception_overflow (), ip);
			sp [-1].data.i = (gint16) sp [-1].data.l;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_OVF_I2_R8)
			if (sp [-1].data.f < G_MININT16 || sp [-1].data.f > G_MAXINT16)
				THROW_EX (mono_get_exception_overflow (), ip);
			sp [-1].data.i = (gint16) sp [-1].data.f;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_OVF_I2_UN_R8)
			if (sp [-1].data.f < 0 || sp [-1].data.f > G_MAXINT16)
				THROW_EX (mono_get_exception_overflow (), ip);
			sp [-1].data.i = (gint16) sp [-1].data.f;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_OVF_U2_I4)
			if (sp [-1].data.i < 0 || sp [-1].data.i > G_MAXUINT16)
				THROW_EX (mono_get_exception_overflow (), ip);
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_OVF_U2_I8)
			if (sp [-1].data.l < 0 || sp [-1].data.l > G_MAXUINT16)
				THROW_EX (mono_get_exception_overflow (), ip);
			sp [-1].data.i = (guint16) sp [-1].data.l;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_OVF_U2_R8)
			if (sp [-1].data.f < 0 || sp [-1].data.f > G_MAXUINT16)
				THROW_EX (mono_get_exception_overflow (), ip);
			sp [-1].data.i = (guint16) sp [-1].data.f;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_OVF_I1_I4)
			if (sp [-1].data.i < G_MININT8 || sp [-1].data.i > G_MAXINT8)
				THROW_EX (mono_get_exception_overflow (), ip);
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_OVF_I1_U4)
			if (sp [-1].data.i < 0 || sp [-1].data.i > G_MAXINT8)
				THROW_EX (mono_get_exception_overflow (), ip);
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_OVF_I1_I8)
			if (sp [-1].data.l < G_MININT8 || sp [-1].data.l > G_MAXINT8)
				THROW_EX (mono_get_exception_overflow (), ip);
			sp [-1].data.i = (gint8) sp [-1].data.l;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_OVF_I1_U8)
			if (sp [-1].data.l < 0 || sp [-1].data.l > G_MAXINT8)
				THROW_EX (mono_get_exception_overflow (), ip);
			sp [-1].data.i = (gint8) sp [-1].data.l;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_OVF_I1_R8)
			if (sp [-1].data.f < G_MININT8 || sp [-1].data.f > G_MAXINT8)
				THROW_EX (mono_get_exception_overflow (), ip);
			sp [-1].data.i = (gint8) sp [-1].data.f;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_OVF_I1_UN_R8)
			if (sp [-1].data.f < 0 || sp [-1].data.f > G_MAXINT8)
				THROW_EX (mono_get_exception_overflow (), ip);
			sp [-1].data.i = (gint8) sp [-1].data.f;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_OVF_U1_I4)
			if (sp [-1].data.i < 0 || sp [-1].data.i > G_MAXUINT8)
				THROW_EX (mono_get_exception_overflow (), ip);
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_OVF_U1_I8)
			if (sp [-1].data.l < 0 || sp [-1].data.l > G_MAXUINT8)
				THROW_EX (mono_get_exception_overflow (), ip);
			sp [-1].data.i = (guint8) sp [-1].data.l;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CONV_OVF_U1_R8)
			if (sp [-1].data.f < 0 || sp [-1].data.f > G_MAXUINT8)
				THROW_EX (mono_get_exception_overflow (), ip);
			sp [-1].data.i = (guint8) sp [-1].data.f;
			++ip;
			MINT_IN_BREAK;
#if 0
		MINT_IN_CASE(MINT_LDELEM) 
		MINT_IN_CASE(MINT_STELEM) 
		MINT_IN_CASE(MINT_UNBOX_ANY) 
#endif
		MINT_IN_CASE(MINT_CKFINITE)
			if (!mono_isfinite (sp [-1].data.f))
				THROW_EX (mono_get_exception_arithmetic (), ip);
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_MKREFANY) {
			c = (MonoClass*)rtm->data_items [*(guint16 *)(ip + 1)];

			/* The value address is on the stack */
			gpointer addr = sp [-1].data.p;
			/* Push the typedref value on the stack */
			sp [-1].data.p = vt_sp;
			vt_sp += ALIGN_TO (sizeof (MonoTypedRef), MINT_VT_ALIGNMENT);

			MonoTypedRef *tref = (MonoTypedRef*)sp [-1].data.p;
			tref->klass = c;
			tref->type = m_class_get_byval_arg (c);
			tref->value = addr;

			ip += 2;
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_REFANYTYPE) {
			MonoTypedRef *tref = (MonoTypedRef*)sp [-1].data.p;
			MonoType *type = tref->type;

			vt_sp -= ALIGN_TO (sizeof (MonoTypedRef), MINT_VT_ALIGNMENT);
			sp [-1].data.p = vt_sp;
			vt_sp += 8;
			*(gpointer*)sp [-1].data.p = type;
			ip ++;
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_REFANYVAL) {
			MonoTypedRef *tref = (MonoTypedRef*)sp [-1].data.p;
			gpointer addr = tref->value;

			c = (MonoClass*)rtm->data_items [*(guint16 *)(ip + 1)];
			if (c != tref->klass)
				THROW_EX (mono_get_exception_invalid_cast (), ip);

			vt_sp -= ALIGN_TO (sizeof (MonoTypedRef), MINT_VT_ALIGNMENT);

			sp [-1].data.p = addr;
			ip += 2;
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_LDTOKEN)
			sp->data.p = vt_sp;
			vt_sp += 8;
			* (gpointer *)sp->data.p = rtm->data_items[*(guint16 *)(ip + 1)];
			ip += 2;
			++sp;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_ADD_OVF_I4)
			if (CHECK_ADD_OVERFLOW (sp [-2].data.i, sp [-1].data.i))
				THROW_EX (mono_get_exception_overflow (), ip);
			BINOP(i, +);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_ADD_OVF_I8)
			if (CHECK_ADD_OVERFLOW64 (sp [-2].data.l, sp [-1].data.l))
				THROW_EX (mono_get_exception_overflow (), ip);
			BINOP(l, +);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_ADD_OVF_UN_I4)
			if (CHECK_ADD_OVERFLOW_UN (sp [-2].data.i, sp [-1].data.i))
				THROW_EX (mono_get_exception_overflow (), ip);
			BINOP_CAST(i, +, guint32);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_ADD_OVF_UN_I8)
			if (CHECK_ADD_OVERFLOW64_UN (sp [-2].data.l, sp [-1].data.l))
				THROW_EX (mono_get_exception_overflow (), ip);
			BINOP_CAST(l, +, guint64);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_MUL_OVF_I4)
			if (CHECK_MUL_OVERFLOW (sp [-2].data.i, sp [-1].data.i))
				THROW_EX (mono_get_exception_overflow (), ip);
			BINOP(i, *);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_MUL_OVF_I8)
			if (CHECK_MUL_OVERFLOW64 (sp [-2].data.l, sp [-1].data.l))
				THROW_EX (mono_get_exception_overflow (), ip);
			BINOP(l, *);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_MUL_OVF_UN_I4)
			if (CHECK_MUL_OVERFLOW_UN (sp [-2].data.i, sp [-1].data.i))
				THROW_EX (mono_get_exception_overflow (), ip);
			BINOP_CAST(i, *, guint32);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_MUL_OVF_UN_I8)
			if (CHECK_MUL_OVERFLOW64_UN (sp [-2].data.l, sp [-1].data.l))
				THROW_EX (mono_get_exception_overflow (), ip);
			BINOP_CAST(l, *, guint64);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_SUB_OVF_I4)
			if (CHECK_SUB_OVERFLOW (sp [-2].data.i, sp [-1].data.i))
				THROW_EX (mono_get_exception_overflow (), ip);
			BINOP(i, -);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_SUB_OVF_I8)
			if (CHECK_SUB_OVERFLOW64 (sp [-2].data.l, sp [-1].data.l))
				THROW_EX (mono_get_exception_overflow (), ip);
			BINOP(l, -);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_SUB_OVF_UN_I4)
			if (CHECK_SUB_OVERFLOW_UN (sp [-2].data.i, sp [-1].data.i))
				THROW_EX (mono_get_exception_overflow (), ip);
			BINOP_CAST(i, -, guint32);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_SUB_OVF_UN_I8)
			if (CHECK_SUB_OVERFLOW64_UN (sp [-2].data.l, sp [-1].data.l))
				THROW_EX (mono_get_exception_overflow (), ip);
			BINOP_CAST(l, -, guint64);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_START_ABORT_PROT)
			mono_threads_begin_abort_protected_block ();
			ip ++;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_ENDFINALLY) {
			ip ++;
			int clause_index = *ip;
			gboolean pending_abort = mono_threads_end_abort_protected_block ();

			if (clause_index == exit_at_finally)
				goto exit_frame;
			while (sp > frame->stack) {
				--sp;
			}
			if (finally_ips) {
				ip = (const guint16*)finally_ips->data;
				finally_ips = g_slist_remove (finally_ips, ip);
				/* Throw abort after the last finally block to avoid confusing EH */
				if (pending_abort && !finally_ips)
					EXCEPTION_CHECKPOINT;
				goto main_loop;
			}
			ves_abort();
			MINT_IN_BREAK;
		}

		MINT_IN_CASE(MINT_LEAVE) /* Fall through */
		MINT_IN_CASE(MINT_LEAVE_S)
			while (sp > frame->stack) {
				--sp;
			}
			frame->ip = ip;

			if (*ip == MINT_LEAVE_S) {
				ip += (short) *(ip + 1);
			} else {
				ip += (gint32) READ32 (ip + 1);
			}
			endfinally_ip = ip;
			goto handle_finally;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LEAVE_CHECK)
		MINT_IN_CASE(MINT_LEAVE_S_CHECK)
			while (sp > frame->stack) {
				--sp;
			}
			frame->ip = ip;

			if (frame->ex_handler != NULL && MONO_OFFSET_IN_HANDLER(frame->ex_handler, frame->ip - rtm->code)) {
				frame->ex_handler = NULL;
				frame->ex = NULL;
			}

			if (frame->imethod->method->wrapper_type != MONO_WRAPPER_RUNTIME_INVOKE) {
				stackval tmp_sp;

				child_frame.parent = frame;
				child_frame.imethod = NULL;
				/*
				 * We need for mono_thread_get_undeniable_exception to be able to unwind
				 * to check the abort threshold. For this to work we use child_frame as a
				 * dummy frame that is stored in the lmf and serves as the transition frame
				 */
				context->current_frame = &child_frame;
				do_icall (context, NULL, MINT_ICALL_V_P, &tmp_sp, (gpointer)mono_thread_get_undeniable_exception);
				context->current_frame = frame;

				MonoException *abort_exc = (MonoException*)tmp_sp.data.p;
				if (abort_exc)
					THROW_EX (abort_exc, frame->ip);
			}

			if (*ip == MINT_LEAVE_S_CHECK) {
				ip += (short) *(ip + 1);
			} else {
				ip += (gint32) READ32 (ip + 1);
			}
			endfinally_ip = ip;
			goto handle_finally;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_ICALL_V_V) 
		MINT_IN_CASE(MINT_ICALL_V_P)
		MINT_IN_CASE(MINT_ICALL_P_V) 
		MINT_IN_CASE(MINT_ICALL_P_P)
		MINT_IN_CASE(MINT_ICALL_PP_V)
		MINT_IN_CASE(MINT_ICALL_PP_P)
		MINT_IN_CASE(MINT_ICALL_PPP_V)
		MINT_IN_CASE(MINT_ICALL_PPP_P)
		MINT_IN_CASE(MINT_ICALL_PPPP_V)
		MINT_IN_CASE(MINT_ICALL_PPPP_P)
		MINT_IN_CASE(MINT_ICALL_PPPPP_V)
		MINT_IN_CASE(MINT_ICALL_PPPPP_P)
		MINT_IN_CASE(MINT_ICALL_PPPPPP_V)
		MINT_IN_CASE(MINT_ICALL_PPPPPP_P)
			frame->ip = ip;
			sp = do_icall (context, NULL, *ip, sp, rtm->data_items [*(guint16 *)(ip + 1)]);
			EXCEPTION_CHECKPOINT;
			if (context->has_resume_state) {
				if (frame == context->handler_frame)
					SET_RESUME_STATE (context);
				else
					goto exit_frame;
			}
			ip += 2;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_MONO_LDPTR) 
			sp->data.p = rtm->data_items [*(guint16 *)(ip + 1)];
			ip += 2;
			++sp;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_MONO_NEWOBJ)
			sp->data.p = mono_object_new_checked (rtm->domain, (MonoClass*)rtm->data_items [*(guint16 *)(ip + 1)], error);
			mono_error_cleanup (error); /* FIXME: don't swallow the error */
			ip += 2;
			sp++;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_MONO_FREE)
			++ip;
			--sp;
			g_error ("that doesn't seem right");
			g_free (sp->data.p);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_MONO_RETOBJ)
			++ip;
			sp--;
			stackval_from_data (mono_method_signature (frame->imethod->method)->ret, frame->retval, sp->data.p,
			     mono_method_signature (frame->imethod->method)->pinvoke);
			if (sp > frame->stack)
				g_warning ("retobj: more values on stack: %d", sp-frame->stack);
			goto exit_frame;
		MINT_IN_CASE(MINT_MONO_TLS) {
			MonoTlsKey key = (MonoTlsKey)*(gint32 *)(ip + 1);
			sp->data.p = ((gpointer (*)(void)) mono_tls_get_tls_getter (key, FALSE)) ();
			sp++;
			ip += 3;
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_MONO_MEMORY_BARRIER) {
			++ip;
			mono_memory_barrier ();
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_MONO_LDDOMAIN)
			sp->data.p = mono_domain_get ();
			++sp;
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_SDB_INTR_LOC)
			if (G_UNLIKELY (ss_enabled)) {
				typedef void (*T) (void);
				static T ss_tramp;

				if (!ss_tramp) {
					void *tramp = mini_get_single_step_trampoline ();
					mono_memory_barrier ();
					ss_tramp = (T)tramp;
				}

				/*
				 * Make this point to the MINT_SDB_SEQ_POINT instruction which follows this since
				 * the address of that instruction is stored as the seq point address.
				 */
				frame->ip = ip + 1;

				/*
				 * Use the same trampoline as the JIT. This ensures that
				 * the debugger has the context for the last interpreter
				 * native frame.
				 */
				do_debugger_tramp (ss_tramp, frame);

				if (context->has_resume_state) {
					if (frame == context->handler_frame)
						SET_RESUME_STATE (context);
					else
						goto exit_frame;
				}
			}
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_SDB_SEQ_POINT)
			/* Just a placeholder for a breakpoint */
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_SDB_BREAKPOINT) {
			typedef void (*T) (void);
			static T bp_tramp;
			if (!bp_tramp) {
				void *tramp = mini_get_breakpoint_trampoline ();
				mono_memory_barrier ();
				bp_tramp = (T)tramp;
			}

			frame->ip = ip;

			/* Use the same trampoline as the JIT */
			do_debugger_tramp (bp_tramp, frame);

			if (context->has_resume_state) {
				if (frame == context->handler_frame)
					SET_RESUME_STATE (context);
				else
					goto exit_frame;
			}

			++ip;
			MINT_IN_BREAK;
		}

#define RELOP(datamem, op) \
	--sp; \
	sp [-1].data.i = sp [-1].data.datamem op sp [0].data.datamem; \
	++ip;

#define RELOP_FP(datamem, op, noorder) \
	--sp; \
	if (mono_isunordered (sp [-1].data.datamem, sp [0].data.datamem)) \
		sp [-1].data.i = noorder; \
	else \
		sp [-1].data.i = sp [-1].data.datamem op sp [0].data.datamem; \
	++ip;

		MINT_IN_CASE(MINT_CEQ_I4)
			RELOP(i, ==);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CEQ0_I4)
			sp [-1].data.i = (sp [-1].data.i == 0);
			++ip;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CEQ_I8)
			RELOP(l, ==);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CEQ_R8)
			RELOP_FP(f, ==, 0);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CNE_I4)
			RELOP(i, !=);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CNE_I8)
			RELOP(l, !=);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CNE_R8)
			RELOP_FP(f, !=, 1);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CGT_I4)
			RELOP(i, >);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CGT_I8)
			RELOP(l, >);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CGT_R8)
			RELOP_FP(f, >, 0);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CGE_I4)
			RELOP(i, >=);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CGE_I8)
			RELOP(l, >=);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CGE_R8)
			RELOP_FP(f, >=, 0);
			MINT_IN_BREAK;

#define RELOP_CAST(datamem, op, type) \
	--sp; \
	sp [-1].data.i = (type)sp [-1].data.datamem op (type)sp [0].data.datamem; \
	++ip;

		MINT_IN_CASE(MINT_CGE_UN_I4)
			RELOP_CAST(l, >=, guint32);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CGE_UN_I8)
			RELOP_CAST(l, >=, guint64);
			MINT_IN_BREAK;

		MINT_IN_CASE(MINT_CGT_UN_I4)
			RELOP_CAST(i, >, guint32);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CGT_UN_I8)
			RELOP_CAST(l, >, guint64);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CGT_UN_R8)
			RELOP_FP(f, >, 1);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CLT_I4)
			RELOP(i, <);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CLT_I8)
			RELOP(l, <);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CLT_R8)
			RELOP_FP(f, <, 0);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CLT_UN_I4)
			RELOP_CAST(i, <, guint32);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CLT_UN_I8)
			RELOP_CAST(l, <, guint64);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CLT_UN_R8)
			RELOP_FP(f, <, 1);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CLE_I4)
			RELOP(i, <=);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CLE_I8)
			RELOP(l, <=);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CLE_UN_I4)
			RELOP_CAST(l, <=, guint32);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CLE_UN_I8)
			RELOP_CAST(l, <=, guint64);
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CLE_R8)
			RELOP_FP(f, <=, 0);
			MINT_IN_BREAK;

#undef RELOP
#undef RELOP_FP
#undef RELOP_CAST

		MINT_IN_CASE(MINT_LDFTN) {
			sp->data.p = rtm->data_items [* (guint16 *)(ip + 1)];
			++sp;
			ip += 2;
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_LDVIRTFTN) {
			InterpMethod *m = (InterpMethod*)rtm->data_items [* (guint16 *)(ip + 1)];
			ip += 2;
			--sp;
			if (!sp->data.p)
				THROW_EX (mono_get_exception_null_reference (), ip - 2);
				
			sp->data.p = get_virtual_method (m, sp->data.o);
			++sp;
			MINT_IN_BREAK;
		}

#define LDARG(datamem, argtype) \
	sp->data.datamem = * (argtype *)(frame->args + * (guint16 *)(ip + 1)); \
	ip += 2; \
	++sp; 
	
		MINT_IN_CASE(MINT_LDARG_I1) LDARG(i, gint8); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDARG_U1) LDARG(i, guint8); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDARG_I2) LDARG(i, gint16); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDARG_U2) LDARG(i, guint16); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDARG_I4) LDARG(i, gint32); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDARG_I8) LDARG(l, gint64); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDARG_R4) LDARG(f, float); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDARG_R8) LDARG(f, double); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDARG_O) LDARG(p, gpointer); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDARG_P) LDARG(p, gpointer); MINT_IN_BREAK;

		MINT_IN_CASE(MINT_LDARG_VT)
			sp->data.p = vt_sp;
			i32 = READ32(ip + 2);
			memcpy(sp->data.p, frame->args + * (guint16 *)(ip + 1), i32);
			vt_sp += ALIGN_TO (i32, MINT_VT_ALIGNMENT);
			ip += 4;
			++sp;
			MINT_IN_BREAK;

#define STARG(datamem, argtype) \
	--sp; \
	* (argtype *)(frame->args + * (guint16 *)(ip + 1)) = sp->data.datamem; \
	ip += 2; \
	
		MINT_IN_CASE(MINT_STARG_I1) STARG(i, gint8); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STARG_U1) STARG(i, guint8); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STARG_I2) STARG(i, gint16); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STARG_U2) STARG(i, guint16); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STARG_I4) STARG(i, gint32); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STARG_I8) STARG(l, gint64); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STARG_R4) STARG(f, float); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STARG_R8) STARG(f, double); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STARG_O) STARG(p, gpointer); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STARG_P) STARG(p, gpointer); MINT_IN_BREAK;

		MINT_IN_CASE(MINT_STARG_VT) 
			i32 = READ32(ip + 2);
			--sp;
			memcpy(frame->args + * (guint16 *)(ip + 1), sp->data.p, i32);
			vt_sp -= ALIGN_TO (i32, MINT_VT_ALIGNMENT);
			ip += 4;
			MINT_IN_BREAK;

#define STINARG(datamem, argtype) \
	do { \
		int n = * (guint16 *)(ip + 1); \
		* (argtype *)(frame->args + rtm->arg_offsets [n]) = frame->stack_args [n].data.datamem; \
		ip += 2; \
	} while (0)
	
		MINT_IN_CASE(MINT_STINARG_I1) STINARG(i, gint8); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STINARG_U1) STINARG(i, guint8); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STINARG_I2) STINARG(i, gint16); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STINARG_U2) STINARG(i, guint16); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STINARG_I4) STINARG(i, gint32); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STINARG_I8) STINARG(l, gint64); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STINARG_R4) STINARG(f, float); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STINARG_R8) STINARG(f, double); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STINARG_O) STINARG(p, gpointer); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STINARG_P) STINARG(p, gpointer); MINT_IN_BREAK;

		MINT_IN_CASE(MINT_STINARG_VT) {
			int n = * (guint16 *)(ip + 1);
			i32 = READ32(ip + 2);
			memcpy (frame->args + rtm->arg_offsets [n], frame->stack_args [n].data.p, i32);
			ip += 4;
			MINT_IN_BREAK;
		}

		MINT_IN_CASE(MINT_PROF_ENTER) {
			ip += 1;

			if (MONO_PROFILER_ENABLED (method_enter)) {
				MonoProfilerCallContext *prof_ctx = NULL;

				if (frame->imethod->prof_flags & MONO_PROFILER_CALL_INSTRUMENTATION_ENTER_CONTEXT) {
					prof_ctx = g_new0 (MonoProfilerCallContext, 1);
					prof_ctx->interp_frame = frame;
					prof_ctx->method = frame->imethod->method;
				}

				MONO_PROFILER_RAISE (method_enter, (frame->imethod->method, prof_ctx));

				g_free (prof_ctx);
			}

			MINT_IN_BREAK;
		}

		MINT_IN_CASE(MINT_LDARGA)
			sp->data.p = frame->args + * (guint16 *)(ip + 1);
			ip += 2;
			++sp;
			MINT_IN_BREAK;

#define LDLOC(datamem, argtype) \
	sp->data.datamem = * (argtype *)(locals + * (guint16 *)(ip + 1)); \
	ip += 2; \
	++sp; 
	
		MINT_IN_CASE(MINT_LDLOC_I1) LDLOC(i, gint8); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDLOC_U1) LDLOC(i, guint8); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDLOC_I2) LDLOC(i, gint16); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDLOC_U2) LDLOC(i, guint16); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDLOC_I4) LDLOC(i, gint32); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDLOC_I8) LDLOC(l, gint64); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDLOC_R4) LDLOC(f, float); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDLOC_R8) LDLOC(f, double); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDLOC_O) LDLOC(p, gpointer); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_LDLOC_P) LDLOC(p, gpointer); MINT_IN_BREAK;

		MINT_IN_CASE(MINT_LDLOC_VT)
			sp->data.p = vt_sp;
			i32 = READ32(ip + 2);
			memcpy(sp->data.p, locals + * (guint16 *)(ip + 1), i32);
			vt_sp += ALIGN_TO (i32, MINT_VT_ALIGNMENT);
			ip += 4;
			++sp;
			MINT_IN_BREAK;

		MINT_IN_CASE(MINT_LDLOCA_S)
			sp->data.p = locals + * (guint16 *)(ip + 1);
			ip += 2;
			++sp;
			MINT_IN_BREAK;

#define STLOC(datamem, argtype) \
	--sp; \
	* (argtype *)(locals + * (guint16 *)(ip + 1)) = sp->data.datamem; \
	ip += 2;
	
		MINT_IN_CASE(MINT_STLOC_I1) STLOC(i, gint8); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STLOC_U1) STLOC(i, guint8); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STLOC_I2) STLOC(i, gint16); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STLOC_U2) STLOC(i, guint16); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STLOC_I4) STLOC(i, gint32); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STLOC_I8) STLOC(l, gint64); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STLOC_R4) STLOC(f, float); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STLOC_R8) STLOC(f, double); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STLOC_O) STLOC(p, gpointer); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STLOC_P) STLOC(p, gpointer); MINT_IN_BREAK;

#define STLOC_NP(datamem, argtype) \
	* (argtype *)(locals + * (guint16 *)(ip + 1)) = sp [-1].data.datamem; \
	ip += 2;

		MINT_IN_CASE(MINT_STLOC_NP_I4) STLOC_NP(i, gint32); MINT_IN_BREAK;
		MINT_IN_CASE(MINT_STLOC_NP_O) STLOC_NP(p, gpointer); MINT_IN_BREAK;

		MINT_IN_CASE(MINT_STLOC_VT)
			i32 = READ32(ip + 2);
			--sp;
			memcpy(locals + * (guint16 *)(ip + 1), sp->data.p, i32);
			vt_sp -= ALIGN_TO (i32, MINT_VT_ALIGNMENT);
			ip += 4;
			MINT_IN_BREAK;

		MINT_IN_CASE(MINT_LOCALLOC) {
			if (sp != frame->stack + 1) /*FIX?*/
				THROW_EX (mono_get_exception_execution_engine (NULL), ip);

			int len = sp [-1].data.i;
			sp [-1].data.p = alloca (len);

			if (frame->imethod->init_locals)
				memset (sp [-1].data.p, 0, len);
			++ip;
			MINT_IN_BREAK;
		}
		MINT_IN_CASE(MINT_ENDFILTER)
			/* top of stack is result of filter */
			frame->retval = &sp [-1];
			goto exit_frame;
		MINT_IN_CASE(MINT_INITOBJ)
			--sp;
			memset (sp->data.vt, 0, READ32(ip + 1));
			ip += 3;
			MINT_IN_BREAK;
		MINT_IN_CASE(MINT_CPBLK)
			sp -= 3;
			if (!sp [0].data.p || !sp [1].data.p)
				THROW_EX (mono_get_exception_null_reference(), ip - 1);
			++ip;
			/* FIXME: value and size may be int64... */
			memcpy (sp [0].data.p, sp [1].data.p, sp [2].data.i);
			MINT_IN_BREAK;
#if 0
		MINT_IN_CASE(MINT_CONSTRAINED_) {
			guint32 token;
			/* FIXME: implement */
			++ip;
			token = READ32 (ip);
			ip += 2;
			MINT_IN_BREAK;
		}
#endif
		MINT_IN_CASE(MINT_INITBLK)
			sp -= 3;
			if (!sp [0].data.p)
				THROW_EX (mono_get_exception_null_reference(), ip - 1);
			++ip;
			/* FIXME: value and size may be int64... */
			memset (sp [0].data.p, sp [1].data.i, sp [2].data.i);
			MINT_IN_BREAK;
#if 0
		MINT_IN_CASE(MINT_NO_)
			/* FIXME: implement */
			ip += 2;
			MINT_IN_BREAK;
#endif
	   MINT_IN_CASE(MINT_RETHROW) {
			/* 
			 * need to clarify what this should actually do:
			 * start the search from the last found handler in
			 * this method or continue in the caller or what.
			 * Also, do we need to run finally/fault handlers after a retrow?
			 * Well, this implementation will follow the usual search
			 * for an handler, considering the current ip as throw spot.
			 * We need to NULL frame->ex_handler for the later code to
			 * actually run the new found handler.
			 */
			int exvar_offset = *(guint16*)(ip + 1);
			frame->ex_handler = NULL;
			THROW_EX_GENERAL (*(MonoException**)(frame->locals + exvar_offset), ip - 1, TRUE);
			MINT_IN_BREAK;
	   }
	   MINT_IN_CASE(MINT_LD_DELEGATE_METHOD_PTR) {
		   MonoDelegate *del;

		   --sp;
		   del = (MonoDelegate*)sp->data.p;
		   if (!del->interp_method) {
			   /* Not created from interpreted code */
			   ERROR_DECL (error);
			   g_assert (del->method);
			   del->interp_method = mono_interp_get_imethod (del->object.vtable->domain, del->method, error);
			   mono_error_assert_ok (error);
		   }
		   g_assert (del->interp_method);
		   sp->data.p = del->interp_method;
		   ++sp;
		   ip += 1;
		   MINT_IN_BREAK;
	   }
		MINT_IN_CASE(MINT_LD_DELEGATE_INVOKE_IMPL) {
			MonoDelegate *del;
			int n = *(guint16*)(ip + 1);
			del = (MonoDelegate*)sp [-n].data.p;
			if (!del->interp_invoke_impl) {
				/*
				 * First time we are called. Set up the invoke wrapper. We might be able to do this
				 * in ctor but we would need to handle AllocDelegateLike_internal separately
				 */
				ERROR_DECL (error);
				MonoMethod *invoke = mono_get_delegate_invoke (del->object.vtable->klass);
				del->interp_invoke_impl = mono_interp_get_imethod (del->object.vtable->domain, mono_marshal_get_delegate_invoke (invoke, del), error);
				mono_error_assert_ok (error);
			}
			sp ++;
			sp [-1].data.p = del->interp_invoke_impl;
			ip += 2;
			MINT_IN_BREAK;
		}
		MINT_IN_DEFAULT
			g_print ("Unimplemented opcode: %04x %s at 0x%x\n", *ip, mono_interp_opname[*ip], ip-rtm->code);
			THROW_EX (mono_get_exception_execution_engine ("Unimplemented opcode"), ip);
		}
	}

	g_assert_not_reached ();
	handle_finally:
	{
		int i;
		guint32 ip_offset;
		MonoExceptionClause *clause;
		GSList *old_list = finally_ips;
		MonoMethod *method = frame->imethod->method;
		
#if DEBUG_INTERP
		if (tracing)
			g_print ("* Handle finally IL_%04x\n", endfinally_ip == NULL ? 0 : endfinally_ip - rtm->code);
#endif
		if (rtm == NULL || (method->flags & METHOD_ATTRIBUTE_PINVOKE_IMPL) 
				|| (method->iflags & (METHOD_IMPL_ATTRIBUTE_INTERNAL_CALL | METHOD_IMPL_ATTRIBUTE_RUNTIME))) {
			goto exit_frame;
		}
		ip_offset = frame->ip - rtm->code;

		if (endfinally_ip != NULL)
			finally_ips = g_slist_prepend(finally_ips, (void *)endfinally_ip);
		for (i = 0; i < rtm->num_clauses; ++i)
			if (frame->ex_handler == &rtm->clauses [i])
				break;

		while (i > 0) {
			--i;
			clause = &rtm->clauses [i];
			if (MONO_OFFSET_IN_CLAUSE (clause, ip_offset) && (endfinally_ip == NULL || !(MONO_OFFSET_IN_CLAUSE (clause, endfinally_ip - rtm->code)))) {
				if (clause->flags == MONO_EXCEPTION_CLAUSE_FINALLY) {
					ip = rtm->code + clause->handler_offset;
					finally_ips = g_slist_prepend (finally_ips, (gpointer) ip);
#if DEBUG_INTERP
					if (tracing)
						g_print ("* Found finally at IL_%04x with exception: %s\n", clause->handler_offset, frame->ex? "yes": "no");
#endif
				}
			}
		}

		endfinally_ip = NULL;

		if (old_list != finally_ips && finally_ips) {
			ip = (const guint16*)finally_ips->data;
			finally_ips = g_slist_remove (finally_ips, ip);
			sp = frame->stack; /* spec says stack should be empty at endfinally so it should be at the start too */
			vt_sp = (unsigned char *) sp + rtm->stack_size;
			goto main_loop;
		}

		ves_abort();
	}

exit_frame:

	if (base_frame)
		memcpy (base_frame->args, frame->args, rtm->alloca_size);

	if (!frame->ex && MONO_PROFILER_ENABLED (method_leave) &&
	    frame->imethod->prof_flags & MONO_PROFILER_CALL_INSTRUMENTATION_LEAVE) {
		MonoProfilerCallContext *prof_ctx = NULL;

		if (frame->imethod->prof_flags & MONO_PROFILER_CALL_INSTRUMENTATION_LEAVE_CONTEXT) {
			prof_ctx = g_new0 (MonoProfilerCallContext, 1);
			prof_ctx->interp_frame = frame;
			prof_ctx->method = frame->imethod->method;

			MonoType *rtype = mono_method_signature (frame->imethod->method)->ret;

			switch (rtype->type) {
			case MONO_TYPE_VOID:
				break;
			case MONO_TYPE_VALUETYPE:
				prof_ctx->return_value = frame->retval->data.p;
				break;
			default:
				prof_ctx->return_value = frame->retval;
				break;
			}
		}

		MONO_PROFILER_RAISE (method_leave, (frame->imethod->method, prof_ctx));

		g_free (prof_ctx);
	} else if (frame->ex && frame->imethod->prof_flags & MONO_PROFILER_CALL_INSTRUMENTATION_EXCEPTION_LEAVE)
		MONO_PROFILER_RAISE (method_exception_leave, (frame->imethod->method, &frame->ex->object));

	DEBUG_LEAVE ();
}

static void
interp_parse_options (const char *options)
{
	char **args, **ptr;

	if (!options)
		return;

	args = g_strsplit (options, ",", -1);
	for (ptr = args; ptr && *ptr; ptr ++) {
		char *arg = *ptr;

		if (strncmp (arg, "jit=", 4) == 0)
			mono_interp_jit_classes = g_slist_prepend (mono_interp_jit_classes, arg + 4);
		if (strncmp (arg, "interp-only=", 4) == 0)
			mono_interp_only_classes = g_slist_prepend (mono_interp_only_classes, arg + strlen ("interp-only="));
	}
}

typedef int (*TestMethod) (void);

/*
 * interp_set_resume_state:
 *
 *   Set the state the interpeter will continue to execute from after execution returns to the interpreter.
 */
static void
interp_set_resume_state (MonoJitTlsData *jit_tls, MonoException *ex, MonoJitExceptionInfo *ei, MonoInterpFrameHandle interp_frame, gpointer handler_ip)
{
	ThreadContext *context;

	g_assert (jit_tls);
	context = (ThreadContext*)jit_tls->interp_context;
	g_assert (context);

	context->has_resume_state = TRUE;
	context->handler_frame = (InterpFrame*)interp_frame;
	context->handler_ei = ei;
	/* This is on the stack, so it doesn't need a wbarrier */
	context->handler_frame->ex = ex;
	/* Ditto */
	if (ei)
		*(MonoException**)(context->handler_frame->locals + ei->exvar_offset) = ex;
	context->handler_ip = handler_ip;
}

/*
 * interp_run_finally:
 *
 *   Run the finally clause identified by CLAUSE_INDEX in the intepreter frame given by
 * frame->interp_frame.
 * Return TRUE if the finally clause threw an exception.
 */
static gboolean
interp_run_finally (StackFrameInfo *frame, int clause_index, gpointer handler_ip)
{
	InterpFrame *iframe = (InterpFrame*)frame->interp_frame;
	ThreadContext *context = get_context ();
	const unsigned short *old_ip = iframe->ip;


	interp_exec_method_full (iframe, context, (guint16*)handler_ip, NULL, clause_index, NULL);
	if (context->has_resume_state) {
		return TRUE;
	} else {
		iframe->ip = old_ip;
		return FALSE;
	}
}

/*
 * interp_run_filter:
 *
 *   Run the filter clause identified by CLAUSE_INDEX in the intepreter frame given by
 * frame->interp_frame.
 */
static gboolean
interp_run_filter (StackFrameInfo *frame, MonoException *ex, int clause_index, gpointer handler_ip)
{
	InterpFrame *iframe = (InterpFrame*)frame->interp_frame;
	ThreadContext *context = get_context ();
	InterpFrame child_frame;
	stackval retval;

	/*
	 * Have to run the clause in a new frame which is a copy of IFRAME, since
	 * during debugging, there are two copies of the frame on the stack.
	 */
	memset (&child_frame, 0, sizeof (InterpFrame));
	child_frame.imethod = iframe->imethod;
	child_frame.retval = &retval;
	child_frame.parent = iframe;

	interp_exec_method_full (&child_frame, context, (guint16*)handler_ip, ex, clause_index, iframe);
	/* ENDFILTER stores the result into child_frame->retval */
	return child_frame.retval->data.i ? TRUE : FALSE;
}

typedef struct {
	InterpFrame *current;
} StackIter;

/*
 * interp_frame_iter_init:
 *
 *   Initialize an iterator for iterating through interpreted frames.
 */
static void
interp_frame_iter_init (MonoInterpStackIter *iter, gpointer interp_exit_data)
{
	StackIter *stack_iter = (StackIter*)iter;

	stack_iter->current = (InterpFrame*)interp_exit_data;
}

/*
 * interp_frame_iter_next:
 *
 *   Fill out FRAME with date for the next interpreter frame.
 */
static gboolean
interp_frame_iter_next (MonoInterpStackIter *iter, StackFrameInfo *frame)
{
	StackIter *stack_iter = (StackIter*)iter;
	InterpFrame *iframe = stack_iter->current;

	memset (frame, 0, sizeof (StackFrameInfo));
	/* pinvoke frames doesn't have imethod set */
	while (iframe && !(iframe->imethod && iframe->imethod->code && iframe->imethod->jinfo))
		iframe = iframe->parent;
	if (!iframe)
		return FALSE;

	MonoMethod *method = iframe->imethod->method;
	frame->domain = iframe->domain;
	frame->interp_frame = iframe;
	frame->method = method;
	frame->actual_method = method;
	if (method && ((method->flags & METHOD_ATTRIBUTE_PINVOKE_IMPL) || (method->iflags & (METHOD_IMPL_ATTRIBUTE_INTERNAL_CALL | METHOD_IMPL_ATTRIBUTE_RUNTIME)))) {
		frame->native_offset = -1;
		frame->type = FRAME_TYPE_MANAGED_TO_NATIVE;
	} else {
		frame->type = FRAME_TYPE_INTERP;
		/* This is the offset in the interpreter IR */
		frame->native_offset = (guint8*)iframe->ip - (guint8*)iframe->imethod->code;
		if (!method->wrapper_type || method->wrapper_type == MONO_WRAPPER_DYNAMIC_METHOD)
			frame->managed = TRUE;
	}
	frame->ji = iframe->imethod->jinfo;
	frame->frame_addr = iframe;

	stack_iter->current = iframe->parent;

	return TRUE;
}

static MonoJitInfo*
interp_find_jit_info (MonoDomain *domain, MonoMethod *method)
{
	InterpMethod* rtm;

	rtm = lookup_imethod (domain, method);
	if (rtm)
		return rtm->jinfo;
	else
		return NULL;
}

static void
interp_set_breakpoint (MonoJitInfo *jinfo, gpointer ip)
{
	guint16 *code = (guint16*)ip;
	g_assert (*code == MINT_SDB_SEQ_POINT);
	*code = MINT_SDB_BREAKPOINT;
}

static void
interp_clear_breakpoint (MonoJitInfo *jinfo, gpointer ip)
{
	guint16 *code = (guint16*)ip;
	g_assert (*code == MINT_SDB_BREAKPOINT);
	*code = MINT_SDB_SEQ_POINT;
}

static MonoJitInfo*
interp_frame_get_jit_info (MonoInterpFrameHandle frame)
{
	InterpFrame *iframe = (InterpFrame*)frame;

	g_assert (iframe->imethod);
	return iframe->imethod->jinfo;
}

static gpointer
interp_frame_get_ip (MonoInterpFrameHandle frame)
{
	InterpFrame *iframe = (InterpFrame*)frame;

	g_assert (iframe->imethod);
	return (gpointer)iframe->ip;
}

static gpointer
interp_frame_get_arg (MonoInterpFrameHandle frame, int pos)
{
	InterpFrame *iframe = (InterpFrame*)frame;

	g_assert (iframe->imethod);

	int arg_offset = iframe->imethod->arg_offsets [pos + (iframe->imethod->hasthis ? 1 : 0)];

	return iframe->args + arg_offset;
}

static gpointer
interp_frame_get_local (MonoInterpFrameHandle frame, int pos)
{
	InterpFrame *iframe = (InterpFrame*)frame;

	g_assert (iframe->imethod);

	return iframe->locals + iframe->imethod->local_offsets [pos];
}

static gpointer
interp_frame_get_this (MonoInterpFrameHandle frame)
{
	InterpFrame *iframe = (InterpFrame*)frame;

	g_assert (iframe->imethod);
	g_assert (iframe->imethod->hasthis);

	int arg_offset = iframe->imethod->arg_offsets [0];

	return iframe->args + arg_offset;
}

static MonoInterpFrameHandle
interp_frame_get_parent (MonoInterpFrameHandle frame)
{
	InterpFrame *iframe = (InterpFrame*)frame;

	return iframe->parent;
}

static void
interp_start_single_stepping (void)
{
	ss_enabled = TRUE;
}

static void
interp_stop_single_stepping (void)
{
	ss_enabled = FALSE;
}

void
mono_ee_interp_init (const char *opts)
{
	g_assert (mono_ee_api_version () == MONO_EE_API_VERSION);
	g_assert (!interp_init_done);
	interp_init_done = TRUE;

	mono_native_tls_alloc (&thread_context_id, NULL);
	set_context (NULL);

	interp_parse_options (opts);
	mono_interp_transform_init ();

	MonoEECallbacks c;
#ifdef MONO_ARCH_HAVE_INTERP_ENTRY_TRAMPOLINE
	c.entry_from_trampoline = interp_entry_from_trampoline;
#endif
	c.create_method_pointer = interp_create_method_pointer;
	c.runtime_invoke = interp_runtime_invoke;
	c.init_delegate = interp_init_delegate;
	c.delegate_ctor = interp_delegate_ctor;
	c.get_remoting_invoke = interp_get_remoting_invoke;
	c.set_resume_state = interp_set_resume_state;
	c.run_finally = interp_run_finally;
	c.run_filter = interp_run_filter;
	c.frame_iter_init = interp_frame_iter_init;
	c.frame_iter_next = interp_frame_iter_next;
	c.find_jit_info = interp_find_jit_info;
	c.set_breakpoint = interp_set_breakpoint;
	c.clear_breakpoint = interp_clear_breakpoint;
	c.frame_get_jit_info = interp_frame_get_jit_info;
	c.frame_get_ip = interp_frame_get_ip;
	c.frame_get_arg = interp_frame_get_arg;
	c.frame_get_local = interp_frame_get_local;
	c.frame_get_this = interp_frame_get_this;
	c.frame_get_parent = interp_frame_get_parent;
	c.frame_arg_to_data = interp_frame_arg_to_data;
	c.data_to_frame_arg = interp_data_to_frame_arg;
	c.frame_arg_to_storage = interp_frame_arg_to_storage;
	c.frame_arg_set_storage = interp_frame_arg_set_storage;
	c.start_single_stepping = interp_start_single_stepping;
	c.stop_single_stepping = interp_stop_single_stepping;
	mini_install_interp_callbacks (&c);
}
