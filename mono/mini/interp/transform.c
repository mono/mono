/*
 * transform CIL into different opcodes for more
 * efficient interpretation
 *
 * Written by Bernie Solomon (bernard@ugsolutions.com)
 * Copyright (c) 2004.
 */

#include <string.h>
#include <mono/metadata/appdomain.h>
#include <mono/metadata/debug-helpers.h>
#include <mono/metadata/exception.h>
#include <mono/metadata/mono-endian.h>
#include <mono/metadata/marshal.h>
#include <mono/metadata/profiler-private.h>
#include <mono/metadata/tabledefs.h>

#include <mono/mini/mini.h>

#include "mintops.h"
#include "interp-internals.h"
#include "interp.h"

// TODO: export from marshal.c
MonoDelegate* mono_ftnptr_to_delegate (MonoClass *klass, gpointer ftn);

#define DEBUG 0

typedef struct
{
	MonoClass *klass;
	unsigned char type;
	unsigned char flags;
} StackInfo;

typedef struct
{
	MonoMethod *method;
	MonoMethodHeader *header;
	RuntimeMethod *rtm;
	const unsigned char *il_code;
	const unsigned char *ip;
	const unsigned char *last_ip;
	const unsigned char *in_start;
	int code_size;
	int *in_offsets;
	int *forward_refs;
	StackInfo **stack_state;
	int *stack_height;
	int *vt_stack_size;
	unsigned char *is_bb_start;
	unsigned short *new_code;
	unsigned short *new_code_end;
	unsigned short *new_ip;
	unsigned short *last_new_ip;
	unsigned int max_code_size;
	StackInfo *stack;
	StackInfo *sp;
	unsigned int max_stack_height;
	unsigned int vt_sp;
	unsigned int max_vt_sp;
	int n_data_items;
	int max_data_items;
	void **data_items;
	GHashTable *data_hash;
} TransformData;

#define MINT_TYPE_I1 0
#define MINT_TYPE_U1 1
#define MINT_TYPE_I2 2
#define MINT_TYPE_U2 3
#define MINT_TYPE_I4 4
#define MINT_TYPE_I8 5
#define MINT_TYPE_R4 6
#define MINT_TYPE_R8 7
#define MINT_TYPE_O  8
#define MINT_TYPE_P  9
#define MINT_TYPE_VT 10

#define STACK_TYPE_I4 0
#define STACK_TYPE_I8 1
#define STACK_TYPE_R8 2
#define STACK_TYPE_O  3
#define STACK_TYPE_VT 4
#define STACK_TYPE_MP 5
#define STACK_TYPE_F  6

static const char *stack_type_string [] = { "I4", "I8", "R8", "O ", "VT", "MP", "F " };

#if SIZEOF_VOID_P == 8
#define STACK_TYPE_I STACK_TYPE_I8
#else
#define STACK_TYPE_I STACK_TYPE_I4
#endif

static int stack_type [] = {
	STACK_TYPE_I4, /*I1*/
	STACK_TYPE_I4, /*U1*/
	STACK_TYPE_I4, /*I2*/
	STACK_TYPE_I4, /*U2*/
	STACK_TYPE_I4, /*I4*/
	STACK_TYPE_I8, /*I8*/
	STACK_TYPE_R8, /*R4*/
	STACK_TYPE_R8, /*R8*/
	STACK_TYPE_O,  /*O*/
	STACK_TYPE_MP, /*P*/
	STACK_TYPE_VT
};

static void
grow_code (TransformData *td)
{
	unsigned int old_ip_offset = td->new_ip - td->new_code;
	unsigned int old_last_ip_offset = td->last_new_ip - td->new_code;
	g_assert (old_ip_offset <= td->max_code_size);
	td->new_code = g_realloc (td->new_code, (td->max_code_size *= 2) * sizeof (td->new_code [0]));
	td->new_code_end = td->new_code + td->max_code_size;
	td->new_ip = td->new_code + old_ip_offset;
	td->last_new_ip = td->new_code + old_last_ip_offset;
}

#define ENSURE_CODE(td, n) \
	do { \
		if ((td)->new_ip + (n) > (td)->new_code_end) \
			grow_code (td); \
	} while (0)

#define ADD_CODE(td, n) \
	do { \
		if ((td)->new_ip == (td)->new_code_end) \
			grow_code (td); \
		*(td)->new_ip++ = (n); \
	} while (0)

#define CHECK_STACK(td, n) \
	do { \
		int stack_size = (td)->sp - (td)->stack; \
		if (stack_size < (n)) \
			g_warning ("%s.%s: not enough values (%d < %d) on stack at %04x", \
				(td)->method->klass->name, (td)->method->name, \
				stack_size, n, (td)->ip - (td)->il_code); \
	} while (0)

#define ENSURE_I4(td, sp_off) \
	do { \
		if ((td)->sp [-sp_off].type == STACK_TYPE_I8) \
			ADD_CODE(td, sp_off == 1 ? MINT_CONV_I4_I8 : MINT_CONV_I4_I8_SP); \
	} while (0)

static void 
handle_branch(TransformData *td, int short_op, int long_op, int offset) 
{
	int shorten_branch = 0;
	int target = td->ip + offset - td->il_code;
	if (target < 0 || target >= td->code_size)
		g_assert_not_reached ();
	if (offset > 0 && td->stack_height [target] < 0) {
		td->stack_height [target] = td->sp - td->stack;
		if (td->stack_height [target] > 0)
			td->stack_state [target] = g_memdup (td->stack, td->stack_height [target] * sizeof (td->stack [0]));
		td->vt_stack_size [target] = td->vt_sp;
	}
	if (offset < 0) {
		offset = td->in_offsets [target] - (td->new_ip - td->new_code);
		if (offset >= -32768) {
			shorten_branch = 1;
		}
	} else {
		int prev = td->forward_refs [target];
		td->forward_refs [td->ip - td->il_code] = prev;
		td->forward_refs [target] = td->ip - td->il_code;
		offset = 0;
		if (td->header->code_size <= 25000) /* FIX to be precise somehow? */
			shorten_branch = 1;
	}
	if (shorten_branch) {
		ADD_CODE(td, short_op);
		ADD_CODE(td, offset);
	} else {
		ADD_CODE(td, long_op);
		ADD_CODE(td, * (unsigned short *)(&offset));
		ADD_CODE(td, * ((unsigned short *)&offset + 1));
	}
}

static void 
one_arg_branch(TransformData *td, int mint_op, int offset) 
{
	int type = td->sp [-1].type == STACK_TYPE_O || td->sp [-1].type == STACK_TYPE_MP ? STACK_TYPE_I : td->sp [-1].type;
	int long_op = mint_op + type - STACK_TYPE_I4;
	int short_op = long_op + MINT_BRFALSE_I4_S - MINT_BRFALSE_I4;
	CHECK_STACK(td, 1);
	--td->sp;
	handle_branch (td, short_op, long_op, offset);
}

static void 
two_arg_branch(TransformData *td, int mint_op, int offset) 
{
	int type1 = td->sp [-1].type == STACK_TYPE_O || td->sp [-1].type == STACK_TYPE_MP ? STACK_TYPE_I : td->sp [-1].type;
	int type2 = td->sp [-2].type == STACK_TYPE_O || td->sp [-2].type == STACK_TYPE_MP ? STACK_TYPE_I : td->sp [-2].type;
	int long_op = mint_op + type1 - STACK_TYPE_I4;
	int short_op = long_op + MINT_BEQ_I4_S - MINT_BEQ_I4;
	CHECK_STACK(td, 2);
	if (type1 == STACK_TYPE_I4 && type2 == STACK_TYPE_I8) {
		ADD_CODE(td, MINT_CONV_I8_I4);
		td->in_offsets [td->ip - td->il_code]++;
	} else if (type1 == STACK_TYPE_I8 && type2 == STACK_TYPE_I4) {
		ADD_CODE(td, MINT_CONV_I8_I4_SP);
		td->in_offsets [td->ip - td->il_code]++;
	} else if (type1 != type2) {
		g_warning("%s.%s: branch type mismatch %d %d", 
			td->method->klass->name, td->method->name, 
			td->sp [-1].type, td->sp [-2].type);
	}
	td->sp -= 2;
	handle_branch (td, short_op, long_op, offset);
}

static void
unary_arith_op(TransformData *td, int mint_op)
{
	int op = mint_op + td->sp [-1].type - STACK_TYPE_I4;
	CHECK_STACK(td, 1);
	ADD_CODE(td, op);
}

static void
binary_arith_op(TransformData *td, int mint_op)
{
	int type1 = td->sp [-2].type;
	int type2 = td->sp [-1].type;
	int op;
#if SIZEOF_VOID_P == 8
	if ((type1 == STACK_TYPE_MP || type1 == STACK_TYPE_I8) && type2 == STACK_TYPE_I4) {
		ADD_CODE(td, MINT_CONV_I8_I4);
		type2 = STACK_TYPE_I8;
	}
	if (type1 == STACK_TYPE_I4 && (type2 == STACK_TYPE_MP || type2 == STACK_TYPE_I8)) {
		ADD_CODE(td, MINT_CONV_I8_I4_SP);
		type1 = STACK_TYPE_I8;
		td->sp [-2].type = STACK_TYPE_I8;
	}
#endif
	if (type1 == STACK_TYPE_MP)
		type1 = STACK_TYPE_I;
	if (type2 == STACK_TYPE_MP)
		type2 = STACK_TYPE_I;
	if (type1 != type2) {
		g_warning("%s.%s: %04x arith type mismatch %s %d %d", 
			td->method->klass->name, td->method->name, 
			td->ip - td->il_code, mono_interp_opname[mint_op], type1, type2);
	}
	op = mint_op + type1 - STACK_TYPE_I4;
	CHECK_STACK(td, 2);
	ADD_CODE(td, op);
	--td->sp;
}

static void
binary_int_op(TransformData *td, int mint_op)
{
	int op = mint_op + td->sp [-1].type - STACK_TYPE_I4;
	CHECK_STACK(td, 2);
	if (td->sp [-1].type != td->sp [-2].type)
		g_warning("%s.%s: int type mismatch", td->method->klass->name, td->method->name);
	ADD_CODE(td, op);
	--td->sp;
}

static void
shift_op(TransformData *td, int mint_op)
{
	int op = mint_op + td->sp [-2].type - STACK_TYPE_I4;
	CHECK_STACK(td, 2);
	if (td->sp [-1].type != STACK_TYPE_I4) {
		g_warning("%s.%s: shift type mismatch %d", 
			td->method->klass->name, td->method->name,
			td->sp [-2].type);
	}
	ADD_CODE(td, op);
	--td->sp;
}

static int 
mint_type(MonoType *type)
{
	if (type->byref)
		return MINT_TYPE_P;
enum_type:
	switch (type->type) {
	case MONO_TYPE_I1:
		return MINT_TYPE_I1;
	case MONO_TYPE_U1:
	case MONO_TYPE_BOOLEAN:
		return MINT_TYPE_U1;
	case MONO_TYPE_I2:
		return MINT_TYPE_I2;
	case MONO_TYPE_U2:
	case MONO_TYPE_CHAR:
		return MINT_TYPE_U2;
	case MONO_TYPE_I4:
	case MONO_TYPE_U4:
		return MINT_TYPE_I4;
	case MONO_TYPE_I:
	case MONO_TYPE_U:
#if SIZEOF_VOID_P == 4
		return MINT_TYPE_I4;
#else
		return MINT_TYPE_I8;
#endif
	case MONO_TYPE_PTR:
		return MINT_TYPE_P;
	case MONO_TYPE_R4:
		return MINT_TYPE_R4;
	case MONO_TYPE_I8:
	case MONO_TYPE_U8:
		return MINT_TYPE_I8;
	case MONO_TYPE_R8:
		return MINT_TYPE_R8;
	case MONO_TYPE_STRING:
	case MONO_TYPE_SZARRAY:
	case MONO_TYPE_CLASS:
	case MONO_TYPE_OBJECT:
	case MONO_TYPE_ARRAY:
		return MINT_TYPE_O;
	case MONO_TYPE_VALUETYPE:
		if (type->data.klass->enumtype) {
			type = mono_class_enum_basetype (type->data.klass);
			goto enum_type;
		} else
			return MINT_TYPE_VT;
	case MONO_TYPE_GENERICINST:
		type = &type->data.generic_class->container_class->byval_arg;
		goto enum_type;
	default:
		g_warning ("got type 0x%02x", type->type);
		g_assert_not_reached ();
	}
	return -1;
}

static int 
can_store (int stack_type, int var_type)
{
	if (stack_type == STACK_TYPE_O || stack_type == STACK_TYPE_MP)
		stack_type = STACK_TYPE_I;
	if (var_type == STACK_TYPE_O || var_type == STACK_TYPE_MP)
		var_type = STACK_TYPE_I;
	return stack_type == var_type;
}

#define SET_SIMPLE_TYPE(s, ty) \
	do { \
		(s)->type = (ty); \
		(s)->flags = 0; \
		(s)->klass = NULL; \
	} while (0)

#define SET_TYPE(s, ty, k) \
	do { \
		(s)->type = (ty); \
		(s)->flags = 0; \
		(s)->klass = k; \
	} while (0)

#define PUSH_SIMPLE_TYPE(td, ty) \
	do { \
		int sp_height; \
		(td)->sp++; \
		sp_height = (td)->sp - (td)->stack; \
		if (sp_height > (td)->max_stack_height) \
			(td)->max_stack_height = sp_height; \
		SET_SIMPLE_TYPE((td)->sp - 1, ty); \
	} while (0)

#define PUSH_TYPE(td, ty, k) \
	do { \
		int sp_height; \
		(td)->sp++; \
		sp_height = (td)->sp - (td)->stack; \
		if (sp_height > (td)->max_stack_height) \
			(td)->max_stack_height = sp_height; \
		SET_TYPE((td)->sp - 1, ty, k); \
	} while (0)

#define PUSH_VT(td, size) \
	do { \
		(td)->vt_sp += ((size) + 7) & ~7; \
		if ((td)->vt_sp > (td)->max_vt_sp) \
			(td)->max_vt_sp = (td)->vt_sp; \
	} while (0)

#define POP_VT(td, size) \
	do { \
		(td)->vt_sp -= ((size) + 7) & ~7; \
	} while (0)

#if NO_UNALIGNED_ACCESS
#define WRITE32(td, v) \
	do { \
		ENSURE_CODE(td, 2); \
		* (guint16 *)((td)->new_ip) = * (guint16 *)(v); \
		* ((guint16 *)((td)->new_ip) + 1) = * ((guint16 *)(v) + 1); \
		(td)->new_ip += 2; \
	} while (0)

#define WRITE64(td, v) \
	do { \
		ENSURE_CODE(td, 4); \
		* (guint16 *)((td)->new_ip) = * (guint16 *)(v); \
		* ((guint16 *)((td)->new_ip) + 1) = * ((guint16 *)(v) + 1); \
		* ((guint16 *)((td)->new_ip) + 2) = * ((guint16 *)(v) + 2); \
		* ((guint16 *)((td)->new_ip) + 3) = * ((guint16 *)(v) + 3); \
		(td)->new_ip += 4; \
	} while (0)
#else
#define WRITE32(td, v) \
	do { \
		ENSURE_CODE(td, 2); \
		* (guint32 *)((td)->new_ip) = * (guint32 *)(v); \
		(td)->new_ip += 2; \
	} while (0)

#define WRITE64(td, v) \
	do { \
		ENSURE_CODE(td, 4); \
		* (guint64 *)((td)->new_ip) = * (guint64 *)(v); \
		(td)->new_ip += 4; \
	} while (0)

#endif

static void 
load_arg(TransformData *td, int n)
{
	int mt;
	MonoClass *klass = NULL;
	MonoType *type;

	gboolean hasthis = mono_method_signature (td->method)->hasthis;
	if (hasthis && n == 0)
		type = &td->method->klass->byval_arg;
	else
		type = mono_method_signature (td->method)->params [hasthis ? n - 1 : n];

	mt = mint_type (type);
	if (mt == MINT_TYPE_VT) {
		gint32 size;
		klass = mono_class_from_mono_type (type);
		if (mono_method_signature (td->method)->pinvoke)
			size = mono_class_native_size (klass, NULL);
		else
			size = mono_class_value_size (klass, NULL);

		if (hasthis && n == 0) {
			mt = MINT_TYPE_P;
			ADD_CODE (td, MINT_LDARG_P);
			ADD_CODE (td, td->rtm->arg_offsets [n]); /* FIX for large offset */
			klass = NULL;
		} else {
			PUSH_VT (td, size);
			ADD_CODE (td, MINT_LDARG_VT);
			ADD_CODE (td, td->rtm->arg_offsets [n]); /* FIX for large offset */
			WRITE32 (td, &size);
		}
	} else {
		if (hasthis && n == 0) {
			mt = MINT_TYPE_P;
			ADD_CODE (td, MINT_LDARG_P);
			ADD_CODE (td, td->rtm->arg_offsets [n]); /* FIX for large offset */
			klass = NULL;
		} else {
			ADD_CODE(td, MINT_LDARG_I1 + (mt - MINT_TYPE_I1));
			ADD_CODE(td, td->rtm->arg_offsets [n]); /* FIX for large offset */
			if (mt == MINT_TYPE_O)
				klass = mono_class_from_mono_type (type);
		}
	}
	PUSH_TYPE(td, stack_type[mt], klass);
}

static void 
store_arg(TransformData *td, int n)
{
	int mt;
	CHECK_STACK (td, 1);
	MonoType *type;

	gboolean hasthis = mono_method_signature (td->method)->hasthis;
	if (hasthis && n == 0)
		type = &td->method->klass->byval_arg;
	else
		type = mono_method_signature (td->method)->params [n - !!hasthis];

	mt = mint_type (type);
	if (mt == MINT_TYPE_VT) {
		gint32 size;
		g_error ("data.klass");
		if (mono_method_signature (td->method)->pinvoke)
			size = mono_class_native_size (type->data.klass, NULL);
		else
			size = mono_class_value_size (type->data.klass, NULL);
		ADD_CODE(td, MINT_STARG_VT);
		ADD_CODE(td, n);
		WRITE32(td, &size);
		if (td->sp [-1].type == STACK_TYPE_VT)
			POP_VT(td, size);
	} else {
		ADD_CODE(td, MINT_STARG_I1 + (mt - MINT_TYPE_I1));
		ADD_CODE(td, td->rtm->arg_offsets [n]);
	}
	--td->sp;
}

static void 
store_inarg(TransformData *td, int n)
{
	MonoType *type;
	gboolean hasthis = mono_method_signature (td->method)->hasthis;
	if (hasthis && n == 0)
		type = &td->method->klass->byval_arg;
	else
		type = mono_method_signature (td->method)->params [n - !!hasthis];

	int mt = mint_type (type);
	if (hasthis && n == 0) {
		ADD_CODE (td, MINT_STINARG_P);
		ADD_CODE (td, n);
		return;
	}
	if (mt == MINT_TYPE_VT) {
		MonoClass *klass = mono_class_from_mono_type (type);
		gint32 size;
		if (mono_method_signature (td->method)->pinvoke)
			size = mono_class_native_size (klass, NULL);
		else
			size = mono_class_value_size (klass, NULL);
		ADD_CODE(td, MINT_STINARG_VT);
		ADD_CODE(td, n);
		WRITE32(td, &size);
	} else {
		ADD_CODE(td, MINT_STINARG_I1 + (mt - MINT_TYPE_I1));
		ADD_CODE(td, n);
	}
}

static void 
load_local(TransformData *td, int n)
{
	MonoType *type = td->header->locals [n];
	int mt = mint_type (type);
	int offset = td->rtm->local_offsets [n];
	MonoClass *klass = NULL;
	if (mt == MINT_TYPE_VT) {
		klass = mono_class_from_mono_type (type);
		gint32 size = mono_class_value_size (klass, NULL);
		PUSH_VT(td, size);
		ADD_CODE(td, MINT_LDLOC_VT);
		ADD_CODE(td, offset); /*FIX for large offset */
		WRITE32(td, &size);
	} else {
		g_assert (mt < MINT_TYPE_VT);
		if (mt == MINT_TYPE_I4 && !td->is_bb_start [td->in_start - td->il_code] && td->last_new_ip != NULL &&
			td->last_new_ip [0] == MINT_STLOC_I4 && td->last_new_ip [1] == offset) {
			td->last_new_ip [0] = MINT_STLOC_NP_I4;
		} else if (mt == MINT_TYPE_O && !td->is_bb_start [td->in_start - td->il_code] && td->last_new_ip != NULL &&
			td->last_new_ip [0] == MINT_STLOC_O && td->last_new_ip [1] == offset) {
			td->last_new_ip [0] = MINT_STLOC_NP_O;
		} else {
			ADD_CODE(td, MINT_LDLOC_I1 + (mt - MINT_TYPE_I1));
			ADD_CODE(td, offset); /*FIX for large offset */
		}
		if (mt == MINT_TYPE_O)
			klass = mono_class_from_mono_type (type);
	}
	PUSH_TYPE(td, stack_type[mt], klass);
}

static void 
store_local(TransformData *td, int n)
{
	MonoType *type = td->header->locals [n];
	int mt = mint_type (type);
	int offset = td->rtm->local_offsets [n];
	CHECK_STACK (td, 1);
#if SIZEOF_VOID_P == 8
	if (td->sp [-1].type == STACK_TYPE_I4 && stack_type [mt] == STACK_TYPE_I8) {
		ADD_CODE(td, MINT_CONV_I8_I4);
		td->sp [-1].type = STACK_TYPE_I8;
	}
#endif
	if (!can_store(td->sp [-1].type, stack_type [mt])) {
		g_warning("%s.%s: Store local stack type mismatch %d %d", 
			td->method->klass->name, td->method->name,
			stack_type [mt], td->sp [-1].type);
	}
	if (mt == MINT_TYPE_VT) {
		MonoClass *klass = mono_class_from_mono_type (type);
		gint32 size = mono_class_value_size (klass, NULL);
		ADD_CODE(td, MINT_STLOC_VT);
		ADD_CODE(td, offset); /*FIX for large offset */
		WRITE32(td, &size);
		if (td->sp [-1].type == STACK_TYPE_VT)
			POP_VT(td, size);
	} else {
		g_assert (mt < MINT_TYPE_VT);
		ADD_CODE(td, MINT_STLOC_I1 + (mt - MINT_TYPE_I1));
		ADD_CODE(td, offset); /*FIX for large offset */
	}
	--td->sp;
}

#define SIMPLE_OP(td, op) \
	do { \
		ADD_CODE(&td, op); \
		++td.ip; \
	} while (0)

static guint16
get_data_item_index (TransformData *td, void *ptr)
{
	gpointer p = g_hash_table_lookup (td->data_hash, ptr);
	guint index;
	if (p != NULL)
		return GPOINTER_TO_UINT (p) - 1;
	if (td->max_data_items == td->n_data_items) {
		td->max_data_items = td->n_data_items == 0 ? 16 : 2 * td->max_data_items;
		td->data_items = g_realloc (td->data_items, td->max_data_items * sizeof(td->data_items [0]));
	}
	index = td->n_data_items;
	td->data_items [index] = ptr;
	++td->n_data_items;
	g_hash_table_insert (td->data_hash, ptr, GUINT_TO_POINTER (index + 1));
	return index;
}

static void
interp_transform_call (TransformData *td, MonoMethod *method, MonoMethod *target_method, MonoDomain *domain, MonoGenericContext *generic_context, unsigned char *is_bb_start, int body_start_offset, MonoClass *constrained_class, gboolean readonly)
{
	MonoImage *image = method->klass->image;
	MonoMethodSignature *csignature;
	MonoError error;
	int virtual = *td->ip == CEE_CALLVIRT;
	int calli = *td->ip == CEE_CALLI || *td->ip == CEE_MONO_CALLI_EXTRA_ARG;
	int i;
	guint32 vt_stack_used = 0;
	guint32 vt_res_size = 0;
	int op = -1;
	int native = 0;
	int is_void = 0;

	guint32 token = read32 (td->ip + 1);

	if (target_method == NULL) {
		if (calli) {
			CHECK_STACK(td, 1);
			native = (method->wrapper_type != MONO_WRAPPER_DELEGATE_INVOKE && td->sp [-1].type == STACK_TYPE_I);
			--td->sp;
			if (method->wrapper_type != MONO_WRAPPER_NONE)
				csignature = (MonoMethodSignature *)mono_method_get_wrapper_data (method, token);
			else
				csignature = mono_metadata_parse_signature (image, token);

			if (generic_context) {
				csignature = mono_inflate_generic_signature (csignature, generic_context, &error);
				mono_error_cleanup (&error); /* FIXME: don't swallow the error */
			}

			target_method = NULL;
		} else {
			if (method->wrapper_type == MONO_WRAPPER_NONE)
				target_method = mono_get_method_full (image, token, NULL, generic_context);
			else
				target_method = (MonoMethod *)mono_method_get_wrapper_data (method, token);
			csignature = mono_method_signature (target_method);
			if (target_method->klass == mono_defaults.string_class) {
				if (target_method->name [0] == 'g') {
					if (strcmp (target_method->name, "get_Chars") == 0)
						op = MINT_GETCHR;
					else if (strcmp (target_method->name, "get_Length") == 0)
						op = MINT_STRLEN;
				}
			} else if (mono_class_is_subclass_of (target_method->klass, mono_defaults.array_class, FALSE)) {
				if (!strcmp (target_method->name, "get_Rank")) {
					op = MINT_ARRAY_RANK;
				} else if (!strcmp (target_method->name, "get_Length")) {
					op = MINT_LDLEN;
				} else if (!strcmp (target_method->name, "Address")) {
					op = readonly ? MINT_LDELEMA : MINT_LDELEMA_TC;
				}
			} else if (target_method && generic_context) {
				csignature = mono_inflate_generic_signature (csignature, generic_context, &error);
				mono_error_cleanup (&error); /* FIXME: don't swallow the error */
				target_method = mono_class_inflate_generic_method_checked (target_method, generic_context, &error);
				mono_error_cleanup (&error); /* FIXME: don't swallow the error */
			}
		}
	} else {
		csignature = mono_method_signature (target_method);
	}

	if (constrained_class) {
		if (constrained_class->enumtype && !strcmp (target_method->name, "GetHashCode")) {
			/* Use the corresponding method from the base type to avoid boxing */
			MonoType *base_type = mono_class_enum_basetype (constrained_class);
			g_assert (base_type);
			constrained_class = mono_class_from_mono_type (base_type);
			target_method = mono_class_get_method_from_name (constrained_class, target_method->name, 0);
			g_assert (target_method);
		}
	}

	if (constrained_class) {
		mono_class_setup_vtable (constrained_class);
#if DEBUG_INTERP
		g_print ("CONSTRAINED.CALLVIRT: %s::%s.  %s (%p) ->\n", target_method->klass->name, target_method->name, mono_signature_full_name (target_method->signature), target_method);
#endif
		target_method = mono_get_method_constrained_with_method (image, target_method, constrained_class, generic_context, &error);
#if DEBUG_INTERP
		g_print ("                    : %s::%s.  %s (%p)\n", target_method->klass->name, target_method->name, mono_signature_full_name (target_method->signature), target_method);
#endif
		mono_error_cleanup (&error); /* FIXME: don't swallow the error */
		mono_class_setup_vtable (target_method->klass);

		if (constrained_class->valuetype && (target_method->klass == mono_defaults.object_class || target_method->klass == mono_defaults.enum_class->parent || target_method->klass == mono_defaults.enum_class)) {
			if (target_method->klass == mono_defaults.enum_class && (td->sp - csignature->param_count - 1)->type == STACK_TYPE_MP) {
				/* managed pointer on the stack, we need to deref that puppy */
				ADD_CODE (td, MINT_LDIND_I);
				ADD_CODE (td, csignature->param_count);
			}
			ADD_CODE (td, MINT_BOX);
			ADD_CODE (td, get_data_item_index (td, constrained_class));
			ADD_CODE (td, csignature->param_count);
		} else if (!constrained_class->valuetype) {
			/* managed pointer on the stack, we need to deref that puppy */
			ADD_CODE (td, MINT_LDIND_I);
			ADD_CODE (td, csignature->param_count);
		} else {
			if (target_method->klass->valuetype) {
				/* Own method */
			} else {
				/* Interface method */
				int ioffset, slot;

				mono_class_setup_vtable (constrained_class);
				ioffset = mono_class_interface_offset (constrained_class, target_method->klass);
				if (ioffset == -1)
					g_error ("type load error: constrained_class");
				slot = mono_method_get_vtable_slot (target_method);
				if (slot == -1)
					g_error ("type load error: target_method->klass");
				target_method = constrained_class->vtable [ioffset + slot];

				if (target_method->klass == mono_defaults.enum_class) {
					if ((td->sp - csignature->param_count - 1)->type == STACK_TYPE_MP) {
						/* managed pointer on the stack, we need to deref that puppy */
						ADD_CODE (td, MINT_LDIND_I);
						ADD_CODE (td, csignature->param_count);
					}
					ADD_CODE (td, MINT_BOX);
					ADD_CODE (td, get_data_item_index (td, constrained_class));
					ADD_CODE (td, csignature->param_count);
				}
			}
			virtual = FALSE;
		}
	}

	if (target_method)
		mono_class_init (target_method->klass);

	CHECK_STACK (td, csignature->param_count + csignature->hasthis);
	if (!calli && (!virtual || (target_method->flags & METHOD_ATTRIBUTE_VIRTUAL) == 0) &&
		(target_method->flags & METHOD_ATTRIBUTE_PINVOKE_IMPL) == 0 && 
		(target_method->iflags & METHOD_IMPL_ATTRIBUTE_INTERNAL_CALL) == 0) {
		int called_inited = mono_class_vtable (domain, target_method->klass)->initialized;
		MonoMethodHeader *mheader = mono_method_get_header (target_method);

		if (/*mono_metadata_signature_equal (method->signature, target_method->signature) */ method == target_method && *(td->ip + 5) == CEE_RET) {
			int offset;
			if (mono_interp_traceopt)
				g_print ("Optimize tail call of %s.%s\n", target_method->klass->name, target_method->name);
			for (i = csignature->param_count - 1; i >= 0; --i)
				store_arg (td, i + csignature->hasthis);

			ADD_CODE(td, MINT_BR_S);
			offset = body_start_offset - ((td->new_ip - 1) - td->new_code);
			ADD_CODE(td, offset);
			if (!is_bb_start [td->ip + 5 - td->il_code])
				++td->ip; /* gobble the CEE_RET if it isn't branched to */				
			td->ip += 5;
			return;
		} else {
			/* mheader might not exist if this is a delegate invoc, etc */
			if (mheader && *mheader->code == CEE_RET && called_inited) {
				if (mono_interp_traceopt)
					g_print ("Inline (empty) call of %s.%s\n", target_method->klass->name, target_method->name);
				for (i = 0; i < csignature->param_count; i++)
					ADD_CODE (td, MINT_POP); /*FIX: vt */
					ADD_CODE (td, 0);
				if (csignature->hasthis) {
					if (virtual)
						ADD_CODE(td, MINT_CKNULL);
					ADD_CODE (td, MINT_POP);
					ADD_CODE (td, 0);
				}
				td->sp -= csignature->param_count + csignature->hasthis;
				td->ip += 5;
				return;
			}
		}
	}
	if (method->wrapper_type == MONO_WRAPPER_NONE && target_method != NULL) {
		if (target_method->flags & METHOD_ATTRIBUTE_PINVOKE_IMPL)
			target_method = mono_marshal_get_native_wrapper (target_method, FALSE, FALSE);
		if (!virtual && target_method->iflags & METHOD_IMPL_ATTRIBUTE_SYNCHRONIZED)
			target_method = mono_marshal_get_synchronized_wrapper (target_method);
	}
	g_assert (csignature->call_convention == MONO_CALL_DEFAULT || csignature->call_convention == MONO_CALL_C);
	td->sp -= csignature->param_count + !!csignature->hasthis;
	for (i = 0; i < csignature->param_count; ++i) {
		if (td->sp [i + !!csignature->hasthis].type == STACK_TYPE_VT) {
			gint32 size;
			MonoClass *klass = mono_class_from_mono_type (csignature->params [i]);
			if (csignature->pinvoke && method->wrapper_type != MONO_WRAPPER_NONE)
				size = mono_class_native_size (klass, NULL);
			else
				size = mono_class_value_size (klass, NULL);
			size = (size + 7) & ~7;
			vt_stack_used += size;
		}
	}

	/* need to handle typedbyref ... */
	if (csignature->ret->type != MONO_TYPE_VOID) {
		int mt = mint_type(csignature->ret);
		MonoClass *klass = mono_class_from_mono_type (csignature->ret);
		if (mt == MINT_TYPE_VT) {
			if (csignature->pinvoke && method->wrapper_type != MONO_WRAPPER_NONE)
				vt_res_size = mono_class_native_size (klass, NULL);
			else
				vt_res_size = mono_class_value_size (klass, NULL);
			PUSH_VT(td, vt_res_size);
		}
		PUSH_TYPE(td, stack_type[mt], klass);
	} else
		is_void = TRUE;

	if (op >= 0) {
		ADD_CODE (td, op);
#if SIZEOF_VOID_P == 8
		if (op == MINT_LDLEN)
			ADD_CODE (td, MINT_CONV_I4_I8);
#endif
		if (op == MINT_LDELEMA || op == MINT_LDELEMA_TC) {
			ADD_CODE (td, get_data_item_index (td, target_method->klass));
			ADD_CODE (td, 1 + target_method->klass->rank);
		}
	} else {
		if (calli)
			ADD_CODE(td, native ? MINT_CALLI_NAT : MINT_CALLI);
		else if (virtual)
			ADD_CODE(td, is_void ? MINT_VCALLVIRT : MINT_CALLVIRT);
		else
			ADD_CODE(td, is_void ? MINT_VCALL : MINT_CALL);
		
		if (calli) {
			ADD_CODE(td, get_data_item_index (td, (void *)csignature));
		} else {
			ADD_CODE(td, get_data_item_index (td, (void *)mono_interp_get_runtime_method (domain, target_method, &error)));
			mono_error_cleanup (&error); /* FIXME: don't swallow the error */
		}
	}
	td->ip += 5;
	if (vt_stack_used != 0 || vt_res_size != 0) {
		ADD_CODE(td, MINT_VTRESULT);
		ADD_CODE(td, vt_res_size);
		WRITE32(td, &vt_stack_used);
		td->vt_sp -= vt_stack_used;
	}
}

static void
generate (MonoMethod *method, RuntimeMethod *rtm, unsigned char *is_bb_start, MonoGenericContext *generic_context)
{
	MonoMethodHeader *header = mono_method_get_header (method);
	MonoMethodSignature *signature = mono_method_signature (method);
	MonoImage *image = method->klass->image;
	MonoDomain *domain = mono_domain_get ();
	MonoClass *constrained_class = NULL;
	MonoError error;
	int offset, mt, i, i32;
	gboolean readonly = FALSE;
	MonoClass *klass;
	MonoClassField *field;
	const unsigned char *end;
	int new_in_start_offset;
	int body_start_offset;
	int target;
	guint32 token;
	TransformData td;
	int generating_code = 1;

	memset(&td, 0, sizeof(td));
	td.method = method;
	td.rtm = rtm;
	td.is_bb_start = is_bb_start;
	td.il_code = header->code;
	td.code_size = header->code_size;
	td.header = header;
	td.max_code_size = td.code_size;
	td.new_code = (unsigned short *)g_malloc(td.max_code_size * sizeof(gushort));
	td.new_code_end = td.new_code + td.max_code_size;
	td.in_offsets = g_malloc0(header->code_size * sizeof(int));
	td.forward_refs = g_malloc(header->code_size * sizeof(int));
	td.stack_state = g_malloc0(header->code_size * sizeof(StackInfo *));
	td.stack_height = g_malloc(header->code_size * sizeof(int));
	td.vt_stack_size = g_malloc(header->code_size * sizeof(int));
	td.n_data_items = 0;
	td.max_data_items = 0;
	td.data_items = NULL;
	td.data_hash = g_hash_table_new (NULL, NULL);
	rtm->data_items = td.data_items;
	for (i = 0; i < header->code_size; i++) {
		td.forward_refs [i] = -1;
		td.stack_height [i] = -1;
	}
	td.new_ip = td.new_code;
	td.last_new_ip = NULL;

	td.stack = g_malloc0 ((header->max_stack + 1) * sizeof (td.stack [0]));
	td.sp = td.stack;
	td.max_stack_height = 0;

	for (i = 0; i < header->num_clauses; i++) {
		MonoExceptionClause *c = header->clauses + i;
		td.stack_height [c->handler_offset] = 0;
		td.vt_stack_size [c->handler_offset] = 0;
		td.is_bb_start [c->handler_offset] = 1;

		td.stack_height [c->handler_offset] = 1;
		td.stack_state [c->handler_offset] = g_malloc0(sizeof(StackInfo));
		td.stack_state [c->handler_offset][0].type = STACK_TYPE_O;
		td.stack_state [c->handler_offset][0].klass = NULL; /*FIX*/
	}

	td.ip = header->code;
	end = td.ip + header->code_size;

	if (mono_interp_traceopt) {
		char *tmp = mono_disasm_code (NULL, method, td.ip, end);
		char *name = mono_method_full_name (method, TRUE);
		g_print ("Method %s, original code:\n", name);
		g_print ("%s\n", tmp);
		g_free (tmp);
		g_free (name);
	}

	if (signature->hasthis)
		store_inarg (&td, 0);
	for (i = 0; i < signature->param_count; i++)
		store_inarg (&td, i + !!signature->hasthis);

	body_start_offset = td.new_ip - td.new_code;

	for (i = 0; i < header->num_locals; i++) {
		int mt = mint_type(header->locals [i]);
		if (mt == MINT_TYPE_VT || mt == MINT_TYPE_O || mt == MINT_TYPE_P) {
			ADD_CODE(&td, MINT_INITLOCALS);
			break;
		}
	}

	while (td.ip < end) {
		int in_offset;

		g_assert (td.sp >= td.stack);
		g_assert (td.vt_sp < 0x10000000);
		in_offset = td.ip - header->code;
		td.in_offsets [in_offset] = td.new_ip - td.new_code;
		new_in_start_offset = td.new_ip - td.new_code;
		td.in_start = td.ip;
		while (td.forward_refs [in_offset] >= 0) {
			int j = td.forward_refs [in_offset];
			int slot;
			td.forward_refs [in_offset] = td.forward_refs [j];
			if (td.in_offsets [j] < 0) {                        
				int old_switch_offset = -td.in_offsets [j];
				int new_switch_offset = td.in_offsets [old_switch_offset];
				int switch_case = (j - old_switch_offset - 5) / 4;
				int n_cases = read32 (header->code + old_switch_offset + 1);
				offset = (td.new_ip - td.new_code) - (new_switch_offset + 2 * n_cases + 3);
				slot = new_switch_offset + 3 + 2 * switch_case;
				td.new_code [slot] = * (unsigned short *)(&offset);
				td.new_code [slot + 1] = * ((unsigned short *)&offset + 1);
			} else {
				int op = td.new_code [td.in_offsets [j]];
				if (mono_interp_opargtype [op] == MintOpShortBranch) {
					offset = (td.new_ip - td.new_code) - td.in_offsets [j];
					g_assert (offset <= 32767);
					slot = td.in_offsets [j] + 1;
					td.new_code [slot] = offset;
				} else {
					offset = (td.new_ip - td.new_code) - td.in_offsets [j];
					slot = td.in_offsets [j] + 1;
					td.new_code [slot] = * (unsigned short *)(&offset);
					td.new_code [slot + 1] = * ((unsigned short *)&offset + 1);
				}
			}
		}
		if (td.stack_height [in_offset] >= 0) {
			g_assert (is_bb_start [in_offset]);
			if (td.stack_height [in_offset] > 0)
				memcpy (td.stack, td.stack_state [in_offset], td.stack_height [in_offset] * sizeof(td.stack [0]));
			td.sp = td.stack + td.stack_height [in_offset];
			td.vt_sp = td.vt_stack_size [in_offset];
		}
		if (is_bb_start [in_offset]) {
			generating_code = 1;
		}
		if (!generating_code) {
			while (td.ip < end && !is_bb_start [td.ip - td.il_code])
				++td.ip;
			continue;
		}
		if (mono_interp_traceopt > 1) {
			printf("IL_%04lx %s %-10s -> IL_%04lx, sp %ld, %s %-12s vt_sp %u (max %u)\n", 
				td.ip - td.il_code,
				td.is_bb_start [td.ip - td.il_code] == 3 ? "<>" :
				td.is_bb_start [td.ip - td.il_code] == 2 ? "< " :
				td.is_bb_start [td.ip - td.il_code] == 1 ? " >" : "  ",
				mono_opcode_name (*td.ip), td.new_ip - td.new_code, td.sp - td.stack, 
				td.sp > td.stack ? stack_type_string [td.sp [-1].type] : "  ",
				(td.sp > td.stack && (td.sp [-1].type == STACK_TYPE_O || td.sp [-1].type == STACK_TYPE_VT)) ? (td.sp [-1].klass == NULL ? "?" : td.sp [-1].klass->name) : "",
				td.vt_sp, td.max_vt_sp);
		}
		switch (*td.ip) {
		case CEE_NOP: 
			/* lose it */
			++td.ip;
			break;
		case CEE_BREAK:
			SIMPLE_OP(td, MINT_BREAK);
			break;
		case CEE_LDARG_0:
		case CEE_LDARG_1:
		case CEE_LDARG_2:
		case CEE_LDARG_3:
			load_arg (&td, *td.ip - CEE_LDARG_0);
			++td.ip;
			break;
		case CEE_LDLOC_0:
		case CEE_LDLOC_1:
		case CEE_LDLOC_2:
		case CEE_LDLOC_3:
			load_local (&td, *td.ip - CEE_LDLOC_0);
			++td.ip;
			break;
		case CEE_STLOC_0:
		case CEE_STLOC_1:
		case CEE_STLOC_2:
		case CEE_STLOC_3:
			store_local (&td, *td.ip - CEE_STLOC_0);
			++td.ip;
			break;
		case CEE_LDARG_S:
			load_arg (&td, ((guint8 *)td.ip)[1]);
			td.ip += 2;
			break;
		case CEE_LDARGA_S: {
			/* NOTE: n includes this */
			int n = ((guint8 *)td.ip)[1];
			if (n == 0 && signature->hasthis) {
				g_error ("LDTHISA: NOPE");
				ADD_CODE(&td, MINT_LDTHISA);
			}
			else {
				ADD_CODE(&td, MINT_LDARGA);
				ADD_CODE(&td, td.rtm->arg_offsets [n]);
			}
			PUSH_SIMPLE_TYPE(&td, STACK_TYPE_MP);
			td.ip += 2;
			break;
		}
		case CEE_STARG_S:
			store_arg (&td, ((guint8 *)td.ip)[1]);
			td.ip += 2;
			break;
		case CEE_LDLOC_S:
			load_local (&td, ((guint8 *)td.ip)[1]);
			td.ip += 2;
			break;
		case CEE_LDLOCA_S:
			ADD_CODE(&td, MINT_LDLOCA_S);
			ADD_CODE(&td, td.rtm->local_offsets [((guint8 *)td.ip)[1]]);
			PUSH_SIMPLE_TYPE(&td, STACK_TYPE_MP);
			td.ip += 2;
			break;
		case CEE_STLOC_S:
			store_local (&td, ((guint8 *)td.ip)[1]);
			td.ip += 2;
			break;
		case CEE_LDNULL: 
			SIMPLE_OP(td, MINT_LDNULL);
			PUSH_TYPE(&td, STACK_TYPE_O, NULL);
			break;
		case CEE_LDC_I4_M1:
			SIMPLE_OP(td, MINT_LDC_I4_M1);
			PUSH_SIMPLE_TYPE(&td, STACK_TYPE_I4);
			break;
		case CEE_LDC_I4_0:
			if (!td.is_bb_start[td.ip + 1 - td.il_code] && td.ip [1] == 0xfe && td.ip [2] == CEE_CEQ && 
				td.sp > td.stack && td.sp [-1].type == STACK_TYPE_I4) {
				SIMPLE_OP(td, MINT_CEQ0_I4);
				td.ip += 2;
			} else {
				SIMPLE_OP(td, MINT_LDC_I4_0);
				PUSH_SIMPLE_TYPE(&td, STACK_TYPE_I4);
			}
			break;
		case CEE_LDC_I4_1:
			if (!td.is_bb_start[td.ip + 1 - td.il_code] && 
				(td.ip [1] == CEE_ADD || td.ip [1] == CEE_SUB) && td.sp [-1].type == STACK_TYPE_I4) {
				ADD_CODE(&td, td.ip [1] == CEE_ADD ? MINT_ADD1_I4 : MINT_SUB1_I4);
				td.ip += 2;
			} else {
				SIMPLE_OP(td, MINT_LDC_I4_1);
				PUSH_SIMPLE_TYPE(&td, STACK_TYPE_I4);
			}
			break;
		case CEE_LDC_I4_2:
		case CEE_LDC_I4_3:
		case CEE_LDC_I4_4:
		case CEE_LDC_I4_5:
		case CEE_LDC_I4_6:
		case CEE_LDC_I4_7:
		case CEE_LDC_I4_8:
			SIMPLE_OP(td, (*td.ip - CEE_LDC_I4_0) + MINT_LDC_I4_0);
			PUSH_SIMPLE_TYPE(&td, STACK_TYPE_I4);
			break;
		case CEE_LDC_I4_S: 
			ADD_CODE(&td, MINT_LDC_I4_S);
			ADD_CODE(&td, ((gint8 *) td.ip) [1]);
			td.ip += 2;
			PUSH_SIMPLE_TYPE(&td, STACK_TYPE_I4);
			break;
		case CEE_LDC_I4:
			i32 = read32 (td.ip + 1);
			ADD_CODE(&td, MINT_LDC_I4);
			WRITE32(&td, &i32);
			td.ip += 5;
			PUSH_SIMPLE_TYPE(&td, STACK_TYPE_I4);
			break;
		case CEE_LDC_I8: {
			gint64 val = read64 (td.ip + 1);
			ADD_CODE(&td, MINT_LDC_I8);
			WRITE64(&td, &val);
			td.ip += 9;
			PUSH_SIMPLE_TYPE(&td, STACK_TYPE_I8);
			break;
		}
		case CEE_LDC_R4: {
			float val;
			readr4 (td.ip + 1, &val);
			ADD_CODE(&td, MINT_LDC_R4);
			WRITE32(&td, &val);
			td.ip += 5;
			PUSH_SIMPLE_TYPE(&td, STACK_TYPE_R8);
			break;
		}
		case CEE_LDC_R8: {
			double val;
			readr8 (td.ip + 1, &val);
			ADD_CODE(&td, MINT_LDC_R8);
			WRITE64(&td, &val);
			td.ip += 9;
			PUSH_SIMPLE_TYPE(&td, STACK_TYPE_R8);
			break;
		}
		case CEE_DUP: {
			int type = td.sp [-1].type;
			MonoClass *klass = td.sp [-1].klass;
			if (td.sp [-1].type == STACK_TYPE_VT) {
				gint32 size = mono_class_value_size (klass, NULL);
				PUSH_VT(&td, size);
				ADD_CODE(&td, MINT_DUP_VT);
				WRITE32(&td, &size);
				td.ip ++;
			} else 
				SIMPLE_OP(td, MINT_DUP);
			PUSH_TYPE(&td, type, klass);
			break;
		}
		case CEE_POP:
			CHECK_STACK(&td, 1);
			SIMPLE_OP(td, MINT_POP);
			ADD_CODE (&td, 0);
			if (td.sp [-1].type == STACK_TYPE_VT) {
				int size = mono_class_value_size (td.sp [-1].klass, NULL);
				size = (size + 7) & ~7;
				ADD_CODE(&td, MINT_VTRESULT);
				ADD_CODE(&td, 0);
				WRITE32(&td, &size);
				td.vt_sp -= size;
			}
			--td.sp;
			break;
		case CEE_JMP: {
			MonoMethod *m;
			if (td.sp > td.stack)
				g_warning ("CEE_JMP: stack must be empty");
			token = read32 (td.ip + 1);
			m = mono_get_method_full (image, token, NULL, generic_context);
			ADD_CODE (&td, MINT_JMP);
			ADD_CODE (&td, get_data_item_index (&td, mono_interp_get_runtime_method (domain, m, &error)));
			mono_error_cleanup (&error); /* FIXME: don't swallow the error */
			td.ip += 5;
			break;
		}
		case CEE_CALLVIRT: /* Fall through */
		case CEE_CALLI:    /* Fall through */
		case CEE_CALL: {
			interp_transform_call (&td, method, NULL, domain, generic_context, is_bb_start, body_start_offset, constrained_class, readonly);
			constrained_class = NULL;
			readonly = FALSE;
			break;
		}
		case CEE_RET: {
			int vt_size = 0;
			if (signature->ret->type != MONO_TYPE_VOID) {
				--td.sp;
				MonoClass *klass = mono_class_from_mono_type (signature->ret);
				if (mint_type (&klass->byval_arg) == MINT_TYPE_VT) {
					vt_size = mono_class_value_size (klass, NULL);
					vt_size = (vt_size + 7) & ~7;
				}
			}
			if (td.sp > td.stack)
				g_warning ("%s.%s: CEE_RET: more values on stack: %d", td.method->klass->name, td.method->name, td.sp - td.stack);
			if (td.vt_sp != vt_size)
				g_error ("%s.%s: CEE_RET: value type stack: %d vs. %d", td.method->klass->name, td.method->name, td.vt_sp, vt_size);
			if (vt_size == 0)
				SIMPLE_OP(td, signature->ret->type == MONO_TYPE_VOID ? MINT_RET_VOID : MINT_RET);
			else {
				ADD_CODE(&td, MINT_RET_VT);
				WRITE32(&td, &vt_size);
				++td.ip;
			}
			generating_code = 0;
			break;
		}
		case CEE_BR:
			handle_branch (&td, MINT_BR_S, MINT_BR, 5 + read32 (td.ip + 1));
			td.ip += 5;
			generating_code = 0;
			break;
		case CEE_BR_S:
			handle_branch (&td, MINT_BR_S, MINT_BR, 2 + (gint8)td.ip [1]);
			td.ip += 2;
			generating_code = 0;
			break;
		case CEE_BRFALSE:
			one_arg_branch (&td, MINT_BRFALSE_I4, 5 + read32 (td.ip + 1));
			td.ip += 5;
			break;
		case CEE_BRFALSE_S:
			one_arg_branch (&td, MINT_BRFALSE_I4, 2 + (gint8)td.ip [1]);
			td.ip += 2;
			break;
		case CEE_BRTRUE:
			one_arg_branch (&td, MINT_BRTRUE_I4, 5 + read32 (td.ip + 1));
			td.ip += 5;
			break;
		case CEE_BRTRUE_S:
			one_arg_branch (&td, MINT_BRTRUE_I4, 2 + (gint8)td.ip [1]);
			td.ip += 2;
			break;
		case CEE_BEQ:
			two_arg_branch (&td, MINT_BEQ_I4, 5 + read32 (td.ip + 1));
			td.ip += 5;
			break;
		case CEE_BEQ_S:
			two_arg_branch (&td, MINT_BEQ_I4, 2 + (gint8) td.ip [1]);
			td.ip += 2;
			break;
		case CEE_BGE:
			two_arg_branch (&td, MINT_BGE_I4, 5 + read32 (td.ip + 1));
			td.ip += 5;
			break;
		case CEE_BGE_S:
			two_arg_branch (&td, MINT_BGE_I4, 2 + (gint8) td.ip [1]);
			td.ip += 2;
			break;
		case CEE_BGT:
			two_arg_branch (&td, MINT_BGT_I4, 5 + read32 (td.ip + 1));
			td.ip += 5;
			break;
		case CEE_BGT_S:
			two_arg_branch (&td, MINT_BGT_I4, 2 + (gint8) td.ip [1]);
			td.ip += 2;
			break;
		case CEE_BLT:
			two_arg_branch (&td, MINT_BLT_I4, 5 + read32 (td.ip + 1));
			td.ip += 5;
			break;
		case CEE_BLT_S:
			two_arg_branch (&td, MINT_BLT_I4, 2 + (gint8) td.ip [1]);
			td.ip += 2;
			break;
		case CEE_BLE:
			two_arg_branch (&td, MINT_BLE_I4, 5 + read32 (td.ip + 1));
			td.ip += 5;
			break;
		case CEE_BLE_S:
			two_arg_branch (&td, MINT_BLE_I4, 2 + (gint8) td.ip [1]);
			td.ip += 2;
			break;
		case CEE_BNE_UN:
			two_arg_branch (&td, MINT_BNE_UN_I4, 5 + read32 (td.ip + 1));
			td.ip += 5;
			break;
		case CEE_BNE_UN_S:
			two_arg_branch (&td, MINT_BNE_UN_I4, 2 + (gint8) td.ip [1]);
			td.ip += 2;
			break;
		case CEE_BGE_UN:
			two_arg_branch (&td, MINT_BGE_UN_I4, 5 + read32 (td.ip + 1));
			td.ip += 5;
			break;
		case CEE_BGE_UN_S:
			two_arg_branch (&td, MINT_BGE_UN_I4, 2 + (gint8) td.ip [1]);
			td.ip += 2;
			break;
		case CEE_BGT_UN:
			two_arg_branch (&td, MINT_BGT_UN_I4, 5 + read32 (td.ip + 1));
			td.ip += 5;
			break;
		case CEE_BGT_UN_S:
			two_arg_branch (&td, MINT_BGT_UN_I4, 2 + (gint8) td.ip [1]);
			td.ip += 2;
			break;
		case CEE_BLE_UN:
			two_arg_branch (&td, MINT_BLE_UN_I4, 5 + read32 (td.ip + 1));
			td.ip += 5;
			break;
		case CEE_BLE_UN_S:
			two_arg_branch (&td, MINT_BLE_UN_I4, 2 + (gint8) td.ip [1]);
			td.ip += 2;
			break;
		case CEE_BLT_UN:
			two_arg_branch (&td, MINT_BLT_UN_I4, 5 + read32 (td.ip + 1));
			td.ip += 5;
			break;
		case CEE_BLT_UN_S:
			two_arg_branch (&td, MINT_BLT_UN_I4, 2 + (gint8) td.ip [1]);
			td.ip += 2;
			break;
		case CEE_SWITCH: {
			guint32 n;
			const unsigned char *next_ip;
			const unsigned char *base_ip = td.ip;
			unsigned short *next_new_ip;
			++td.ip;
			n = read32 (td.ip);
			ADD_CODE (&td, MINT_SWITCH);
			WRITE32 (&td, &n);
			td.ip += 4;
			next_ip = td.ip + n * 4;
			next_new_ip = td.new_ip + n * 2;
			--td.sp;
			int stack_height = td.sp - td.stack;
			for (i = 0; i < n; i++) {
				offset = read32 (td.ip);
				target = next_ip - td.il_code + offset;
				if (offset < 0) {
#if DEBUG_INTERP
					if (stack_height > 0 && stack_height != td.stack_height [target])
						g_warning ("SWITCH with back branch and non-empty stack");
#endif
					target = td.in_offsets [target] - (next_new_ip - td.new_code);
				} else {
					td.stack_height [target] = stack_height;
					td.vt_stack_size [target] = td.vt_sp;
					if (stack_height > 0)
						td.stack_state [target] = g_memdup (td.stack, stack_height * sizeof (td.stack [0]));
					int prev = td.forward_refs [target];
					td.forward_refs [td.ip - td.il_code] = prev;
					td.forward_refs [target] = td.ip - td.il_code;
					td.in_offsets [td.ip - td.il_code] = - (base_ip - td.il_code);
				}
				WRITE32 (&td, &target);
				td.ip += 4;
			}
			break;
		}
		case CEE_LDIND_I1:
			CHECK_STACK (&td, 1);
			SIMPLE_OP (td, MINT_LDIND_I1);
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I4);
			break;
		case CEE_LDIND_U1:
			CHECK_STACK (&td, 1);
			SIMPLE_OP (td, MINT_LDIND_U1);
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I4);
			break;
		case CEE_LDIND_I2:
			CHECK_STACK (&td, 1);
			SIMPLE_OP (td, MINT_LDIND_I2);
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I4);
			break;
		case CEE_LDIND_U2:
			CHECK_STACK (&td, 1);
			SIMPLE_OP (td, MINT_LDIND_U2);
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I4);
			break;
		case CEE_LDIND_I4:
			CHECK_STACK (&td, 1);
			SIMPLE_OP (td, MINT_LDIND_I4);
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I4);
			break;
		case CEE_LDIND_U4:
			CHECK_STACK (&td, 1);
			SIMPLE_OP (td, MINT_LDIND_U4);
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I4);
			break;
		case CEE_LDIND_I8:
			CHECK_STACK (&td, 1);
			SIMPLE_OP (td, MINT_LDIND_I8);
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I8);
			break;
		case CEE_LDIND_I:
			CHECK_STACK (&td, 1);
			SIMPLE_OP (td, MINT_LDIND_I);
			ADD_CODE (&td, 0);
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I);
			break;
		case CEE_LDIND_R4:
			CHECK_STACK (&td, 1);
			SIMPLE_OP (td, MINT_LDIND_R4);
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_R8);
			break;
		case CEE_LDIND_R8:
			CHECK_STACK (&td, 1);
			SIMPLE_OP (td, MINT_LDIND_R8);
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_R8);
			break;
		case CEE_LDIND_REF:
			CHECK_STACK (&td, 1);
			SIMPLE_OP (td, MINT_LDIND_REF);
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_O);
			break;
		case CEE_STIND_REF:
			CHECK_STACK (&td, 2);
			SIMPLE_OP (td, MINT_STIND_REF);
			td.sp -= 2;
			break;
		case CEE_STIND_I1:
			CHECK_STACK (&td, 2);
			SIMPLE_OP (td, MINT_STIND_I1);
			td.sp -= 2;
			break;
		case CEE_STIND_I2:
			CHECK_STACK (&td, 2);
			SIMPLE_OP (td, MINT_STIND_I2);
			td.sp -= 2;
			break;
		case CEE_STIND_I4:
			CHECK_STACK (&td, 2);
			SIMPLE_OP (td, MINT_STIND_I4);
			td.sp -= 2;
			break;
		case CEE_STIND_I:
			CHECK_STACK (&td, 2);
			SIMPLE_OP (td, MINT_STIND_I);
			td.sp -= 2;
			break;
		case CEE_STIND_I8:
			CHECK_STACK (&td, 2);
			SIMPLE_OP (td, MINT_STIND_I8);
			td.sp -= 2;
			break;
		case CEE_STIND_R4:
			CHECK_STACK (&td, 2);
			SIMPLE_OP (td, MINT_STIND_R4);
			td.sp -= 2;
			break;
		case CEE_STIND_R8:
			CHECK_STACK (&td, 2);
			SIMPLE_OP (td, MINT_STIND_R8);
			td.sp -= 2;
			break;
		case CEE_ADD:
			binary_arith_op(&td, MINT_ADD_I4);
			++td.ip;
			break;
		case CEE_SUB:
			binary_arith_op(&td, MINT_SUB_I4);
			++td.ip;
			break;
		case CEE_MUL:
			binary_arith_op(&td, MINT_MUL_I4);
			++td.ip;
			break;
		case CEE_DIV:
			binary_arith_op(&td, MINT_DIV_I4);
			++td.ip;
			break;
		case CEE_DIV_UN:
			binary_arith_op(&td, MINT_DIV_UN_I4);
			++td.ip;
			break;
		case CEE_REM:
			binary_int_op (&td, MINT_REM_I4);
			++td.ip;
			break;
		case CEE_REM_UN:
			binary_int_op (&td, MINT_REM_UN_I4);
			++td.ip;
			break;
		case CEE_AND:
			binary_int_op (&td, MINT_AND_I4);
			++td.ip;
			break;
		case CEE_OR:
			binary_int_op (&td, MINT_OR_I4);
			++td.ip;
			break;
		case CEE_XOR:
			binary_int_op (&td, MINT_XOR_I4);
			++td.ip;
			break;
		case CEE_SHL:
			shift_op (&td, MINT_SHL_I4);
			++td.ip;
			break;
		case CEE_SHR:
			shift_op (&td, MINT_SHR_I4);
			++td.ip;
			break;
		case CEE_SHR_UN:
			shift_op (&td, MINT_SHR_UN_I4);
			++td.ip;
			break;
		case CEE_NEG:
			unary_arith_op (&td, MINT_NEG_I4);
			++td.ip;
			break;
		case CEE_NOT:
			unary_arith_op (&td, MINT_NOT_I4);
			++td.ip;
			break;
		case CEE_CONV_U1:
			CHECK_STACK (&td, 1);
			switch (td.sp [-1].type) {
			case STACK_TYPE_R8:
				ADD_CODE(&td, MINT_CONV_U1_R8);
				break;
			case STACK_TYPE_I4:
				ADD_CODE(&td, MINT_CONV_U1_I4);
				break;
			case STACK_TYPE_I8:
				ADD_CODE(&td, MINT_CONV_U1_I8);
				break;
			default:
				g_assert_not_reached ();
			}
			++td.ip;
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I4);
			break;
		case CEE_CONV_I1:
			CHECK_STACK (&td, 1);
			switch (td.sp [-1].type) {
			case STACK_TYPE_R8:
				ADD_CODE(&td, MINT_CONV_I1_R8);
				break;
			case STACK_TYPE_I4:
				ADD_CODE(&td, MINT_CONV_I1_I4);
				break;
			case STACK_TYPE_I8:
				ADD_CODE(&td, MINT_CONV_I1_I8);
				break;
			default:
				g_assert_not_reached ();
			}
			++td.ip;
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I4);
			break;
		case CEE_CONV_U2:
			CHECK_STACK (&td, 1);
			switch (td.sp [-1].type) {
			case STACK_TYPE_R8:
				ADD_CODE(&td, MINT_CONV_U2_R8);
				break;
			case STACK_TYPE_I4:
				ADD_CODE(&td, MINT_CONV_U2_I4);
				break;
			case STACK_TYPE_I8:
				ADD_CODE(&td, MINT_CONV_U2_I8);
				break;
			default:
				g_assert_not_reached ();
			}
			++td.ip;
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I4);
			break;
		case CEE_CONV_I2:
			CHECK_STACK (&td, 1);
			switch (td.sp [-1].type) {
			case STACK_TYPE_R8:
				ADD_CODE(&td, MINT_CONV_I2_R8);
				break;
			case STACK_TYPE_I4:
				ADD_CODE(&td, MINT_CONV_I2_I4);
				break;
			case STACK_TYPE_I8:
				ADD_CODE(&td, MINT_CONV_I2_I8);
				break;
			default:
				g_assert_not_reached ();
			}
			++td.ip;
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I4);
			break;
		case CEE_CONV_U:
			CHECK_STACK (&td, 1);
			switch (td.sp [-1].type) {
			case STACK_TYPE_R8:
#if SIZEOF_VOID_P == 4
				ADD_CODE(&td, MINT_CONV_U4_R8);
#else
				ADD_CODE(&td, MINT_CONV_U8_R8);
#endif
				break;
			case STACK_TYPE_I4:
#if SIZEOF_VOID_P == 8
				ADD_CODE(&td, MINT_CONV_U8_I4);
#endif
				break;
			case STACK_TYPE_I8:
#if SIZEOF_VOID_P == 4
				ADD_CODE(&td, MINT_CONV_U4_I8);
#endif
				break;
			case STACK_TYPE_MP:
				break;
			default:
				g_assert_not_reached ();
			}
			++td.ip;
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I);
			break;
		case CEE_CONV_I: 
			CHECK_STACK (&td, 1);
			switch (td.sp [-1].type) {
			case STACK_TYPE_R8:
#if SIZEOF_VOID_P == 8
				ADD_CODE(&td, MINT_CONV_I8_R8);
#else
				ADD_CODE(&td, MINT_CONV_I4_R8);
#endif
				break;
			case STACK_TYPE_I4:
#if SIZEOF_VOID_P == 8
				ADD_CODE(&td, MINT_CONV_I8_I4);
#endif
				break;
			case STACK_TYPE_O:
				break;
			case STACK_TYPE_MP:
				break;
			case STACK_TYPE_I8:
#if SIZEOF_VOID_P == 4
				ADD_CODE(&td, MINT_CONV_I4_I8);
#endif
				break;
			default:
				g_assert_not_reached ();
			}
			++td.ip;
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I);
			break;
		case CEE_CONV_U4:
			CHECK_STACK (&td, 1);
			switch (td.sp [-1].type) {
			case STACK_TYPE_R8:
				ADD_CODE(&td, MINT_CONV_U4_R8);
				break;
			case STACK_TYPE_I4:
				break;
			case STACK_TYPE_I8:
				ADD_CODE(&td, MINT_CONV_U4_I8);
				break;
			case STACK_TYPE_MP:
#if SIZEOF_VOID_P == 8
				ADD_CODE(&td, MINT_CONV_U4_I8);
#endif
				break;
			default:
				g_assert_not_reached ();
			}
			++td.ip;
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I4);
			break;
		case CEE_CONV_I4:
			CHECK_STACK (&td, 1);
			switch (td.sp [-1].type) {
			case STACK_TYPE_R8:
				ADD_CODE(&td, MINT_CONV_I4_R8);
				break;
			case STACK_TYPE_I4:
				break;
			case STACK_TYPE_I8:
				ADD_CODE(&td, MINT_CONV_I4_I8);
				break;
			case STACK_TYPE_MP:
#if SIZEOF_VOID_P == 8
				ADD_CODE(&td, MINT_CONV_I4_I8);
#endif
				break;
			default:
				g_assert_not_reached ();
			}
			++td.ip;
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I4);
			break;
		case CEE_CONV_I8:
			CHECK_STACK (&td, 1);
			switch (td.sp [-1].type) {
			case STACK_TYPE_R8:
				ADD_CODE(&td, MINT_CONV_I8_R8);
				break;
			case STACK_TYPE_I4:
				ADD_CODE(&td, MINT_CONV_I8_I4);
				break;
			case STACK_TYPE_I8:
				break;
			case STACK_TYPE_MP:
#if SIZEOF_VOID_P == 4
				ADD_CODE(&td, MINT_CONV_I8_I4);
#endif
				break;
			default:
				g_assert_not_reached ();
			}
			++td.ip;
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I8);
			break;
		case CEE_CONV_R4:
			CHECK_STACK (&td, 1);
			switch (td.sp [-1].type) {
			case STACK_TYPE_R8:
				ADD_CODE(&td, MINT_CONV_R4_R8);
				break;
			case STACK_TYPE_I8:
				ADD_CODE(&td, MINT_CONV_R4_I8);
				break;
			case STACK_TYPE_I4:
				ADD_CODE(&td, MINT_CONV_R4_I4);
				break;
			default:
				g_assert_not_reached ();
			}
			++td.ip;
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_R8);
			break;
		case CEE_CONV_R8:
			CHECK_STACK (&td, 1);
			switch (td.sp [-1].type) {
			case STACK_TYPE_I4:
				ADD_CODE(&td, MINT_CONV_R8_I4);
				break;
			case STACK_TYPE_I8:
				ADD_CODE(&td, MINT_CONV_R8_I8);
				break;
			case STACK_TYPE_R8:
				break;
			default:
				g_assert_not_reached ();
			}
			++td.ip;
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_R8);
			break;
		case CEE_CONV_U8:
			CHECK_STACK (&td, 1);
			switch (td.sp [-1].type) {
			case STACK_TYPE_I4:
				ADD_CODE(&td, MINT_CONV_U8_I4);
				break;
			case STACK_TYPE_I8:
				break;
			case STACK_TYPE_R8:
				ADD_CODE(&td, MINT_CONV_U8_R8);
				break;
			case STACK_TYPE_MP:
#if SIZEOF_VOID_P == 4
				ADD_CODE(&td, MINT_CONV_U8_I4);
#endif
				break;
			default:
				g_assert_not_reached ();
			}
			++td.ip;
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I8);
			break;
		case CEE_CPOBJ: {
			CHECK_STACK (&td, 2);

			token = read32 (td.ip + 1);
			klass = mono_class_get_full (image, token, generic_context);

			if (klass->valuetype) {
				ADD_CODE (&td, MINT_CPOBJ);
				ADD_CODE (&td, get_data_item_index(&td, klass));
			} else {
				ADD_CODE (&td, MINT_LDIND_REF);
				ADD_CODE (&td, MINT_STIND_REF);
			}
			td.ip += 5;
			td.sp -= 2;
			break;
		}
		case CEE_LDOBJ: {
			int size;
			CHECK_STACK (&td, 1);

			token = read32 (td.ip + 1);

			if (method->wrapper_type != MONO_WRAPPER_NONE)
				klass = (MonoClass *)mono_method_get_wrapper_data (method, token);
			else
				klass = mono_class_get_full (image, token, generic_context);

			ADD_CODE(&td, MINT_LDOBJ);
			ADD_CODE(&td, get_data_item_index(&td, klass));
			if (mint_type (&klass->byval_arg) == MINT_TYPE_VT) {
				size = mono_class_value_size (klass, NULL);
				PUSH_VT(&td, size);
			}
			td.ip += 5;
			SET_TYPE(td.sp - 1, stack_type[mint_type(&klass->byval_arg)], klass);
			break;
		}
		case CEE_LDSTR: {
			MonoString *s;
			token = mono_metadata_token_index (read32 (td.ip + 1));
			td.ip += 5;
			if (method->wrapper_type != MONO_WRAPPER_NONE) {
				s = mono_string_new_wrapper(
					mono_method_get_wrapper_data (method, token));
			}
			else
				s = mono_ldstr (domain, image, token);
			ADD_CODE(&td, MINT_LDSTR);
			ADD_CODE(&td, get_data_item_index (&td, s));
			PUSH_TYPE(&td, STACK_TYPE_O, mono_defaults.string_class);
			break;
		}
		case CEE_NEWOBJ: {
			MonoMethod *m;
			MonoMethodSignature *csignature;
			guint32 vt_stack_used = 0;
			guint32 vt_res_size = 0;

			td.ip++;
			token = read32 (td.ip);
			td.ip += 4;

			if (method->wrapper_type != MONO_WRAPPER_NONE)
				m = (MonoMethod *)mono_method_get_wrapper_data (method, token);
			else 
				m = mono_get_method_full (image, token, NULL, generic_context);

			csignature = mono_method_signature (m);
			klass = m->klass;
			td.sp -= csignature->param_count;
			ADD_CODE(&td, MINT_NEWOBJ);
			ADD_CODE(&td, get_data_item_index (&td, mono_interp_get_runtime_method (domain, m, &error)));
			mono_error_cleanup (&error); /* FIXME: don't swallow the error */

			if (mint_type (&klass->byval_arg) == MINT_TYPE_VT) {
				vt_res_size = mono_class_value_size (klass, NULL);
				PUSH_VT (&td, vt_res_size);
			}
			for (i = 0; i < csignature->param_count; ++i) {
				int mt = mint_type(csignature->params [i]);
				if (mt == MINT_TYPE_VT) {
					MonoClass *k = mono_class_from_mono_type (csignature->params [i]);
					gint32 size = mono_class_value_size (k, NULL);
					size = (size + 7) & ~7;
					vt_stack_used += size;
				}
			}
			if (vt_stack_used != 0 || vt_res_size != 0) {
				ADD_CODE(&td, MINT_VTRESULT);
				ADD_CODE(&td, vt_res_size);
				WRITE32(&td, &vt_stack_used);
				td.vt_sp -= vt_stack_used;
			}
			PUSH_TYPE (&td, stack_type [mint_type (&klass->byval_arg)], klass);
			break;
		}
		case CEE_CASTCLASS:
			CHECK_STACK (&td, 1);
			token = read32 (td.ip + 1);
			klass = mono_class_get_full (image, token, generic_context);
			ADD_CODE(&td, MINT_CASTCLASS);
			ADD_CODE(&td, get_data_item_index (&td, klass));
			td.sp [-1].klass = klass;
			td.ip += 5;
			break;
		case CEE_ISINST:
			CHECK_STACK (&td, 1);
			token = read32 (td.ip + 1);
			klass = mono_class_get_full (image, token, generic_context);
			ADD_CODE(&td, MINT_ISINST);
			ADD_CODE(&td, get_data_item_index (&td, klass));
			td.ip += 5;
			break;
		case CEE_CONV_R_UN:
			switch (td.sp [-1].type) {
			case STACK_TYPE_R8:
				break;
			case STACK_TYPE_I8:
				ADD_CODE(&td, MINT_CONV_R_UN_I8);
				break;
			case STACK_TYPE_I4:
				ADD_CODE(&td, MINT_CONV_R_UN_I4);
				break;
			default:
				g_assert_not_reached ();
			}
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_R8);
			++td.ip;
			break;
		case CEE_UNBOX:
			CHECK_STACK (&td, 1);
			token = read32 (td.ip + 1);
			
			if (method->wrapper_type != MONO_WRAPPER_NONE)
				klass = (MonoClass *)mono_method_get_wrapper_data (method, token);
			else 
				klass = mono_class_get_full (image, token, generic_context);

			if (mono_class_is_nullable (klass)) {
				g_error ("cee_unbox: implement Nullable");
			}
			
			ADD_CODE(&td, MINT_UNBOX);
			ADD_CODE(&td, get_data_item_index (&td, klass));
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_MP);
			td.ip += 5;
			break;
		case CEE_UNBOX_ANY:
			CHECK_STACK (&td, 1);
			token = read32 (td.ip + 1);

			g_assert (method->wrapper_type == MONO_WRAPPER_NONE);
			klass = mono_class_get_full (image, token, generic_context);

			if (mini_type_is_reference (&klass->byval_arg)) {
				ADD_CODE (&td, MINT_CASTCLASS);
				ADD_CODE (&td, get_data_item_index (&td, klass));
				SET_TYPE (td.sp - 1, stack_type [mt], klass);
				td.ip += 5;
			} else if (mono_class_is_nullable (klass)) {
				MonoMethod *target_method = mono_class_get_method_from_name (klass, "Unbox", 1);
				/* td.ip is incremented by interp_transform_call */
				interp_transform_call (&td, method, target_method, domain, generic_context, is_bb_start, body_start_offset, NULL, FALSE);
			} else {
				int mt = mint_type (&klass->byval_arg);
				ADD_CODE (&td, MINT_UNBOX);
				ADD_CODE (&td, get_data_item_index (&td, klass));

				ADD_CODE (&td, MINT_LDOBJ);
				ADD_CODE (&td, get_data_item_index(&td, klass));
				SET_TYPE (td.sp - 1, stack_type [mt], klass);

				if (mt == MINT_TYPE_VT) {
					int size = mono_class_value_size (klass, NULL);
					PUSH_VT (&td, size);
				}
				td.ip += 5;
			}

			break;
		case CEE_THROW:
			CHECK_STACK (&td, 1);
			SIMPLE_OP (td, MINT_THROW);
			--td.sp;
			generating_code = 0;
			break;
		case CEE_LDFLDA:
			CHECK_STACK (&td, 1);
			token = read32 (td.ip + 1);
			field = mono_field_from_token (image, token, &klass, generic_context);
			gboolean is_static = !!(field->type->attrs & FIELD_ATTRIBUTE_STATIC);
			mono_class_init (klass);
			if (is_static) {
				ADD_CODE (&td, MINT_POP);
				ADD_CODE (&td, 0);
				ADD_CODE (&td, MINT_LDSFLDA);
				ADD_CODE (&td, get_data_item_index (&td, field));
			} else {
				if ((td.sp - 1)->type == STACK_TYPE_O) {
					ADD_CODE (&td, MINT_LDFLDA);
				} else {
					g_assert ((td.sp -1)->type == STACK_TYPE_MP);
					ADD_CODE (&td, MINT_LDFLDA_UNSAFE);
				}
				ADD_CODE (&td, klass->valuetype ? field->offset - sizeof (MonoObject) : field->offset);
			}
			td.ip += 5;
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_MP);
			break;
		case CEE_LDFLD: {
			CHECK_STACK (&td, 1);
			token = read32 (td.ip + 1);
			field = mono_field_from_token (image, token, &klass, generic_context);
			gboolean is_static = !!(field->type->attrs & FIELD_ATTRIBUTE_STATIC);
			mono_class_init (klass);

			MonoClass *field_klass = mono_class_from_mono_type (field->type);
			mt = mint_type (&field_klass->byval_arg);
			if (klass->marshalbyref) {
				g_assert (!is_static);
				ADD_CODE(&td, mt == MINT_TYPE_VT ? MINT_LDRMFLD_VT :  MINT_LDRMFLD);
				ADD_CODE(&td, get_data_item_index (&td, field));
			} else  {
				if (is_static) {
					ADD_CODE (&td, MINT_POP);
					ADD_CODE (&td, 0);
					ADD_CODE (&td, mt == MINT_TYPE_VT ? MINT_LDSFLD_VT : MINT_LDSFLD);
					ADD_CODE (&td, get_data_item_index (&td, field));
				} else {
					ADD_CODE (&td, MINT_LDFLD_I1 + mt - MINT_TYPE_I1);
					ADD_CODE (&td, klass->valuetype ? field->offset - sizeof(MonoObject) : field->offset);
				}
			}
			if (mt == MINT_TYPE_VT) {
				int size = mono_class_value_size (field_klass, NULL);
				PUSH_VT(&td, size);
				WRITE32(&td, &size);
			}
			if (td.sp [-1].type == STACK_TYPE_VT) {
				int size = mono_class_value_size (klass, NULL);
				size = (size + 7) & ~7;
				td.vt_sp -= size;
				ADD_CODE (&td, MINT_VTRESULT);
				ADD_CODE (&td, 0);
				WRITE32 (&td, &size);
			}
			td.ip += 5;
			SET_TYPE(td.sp - 1, stack_type [mt], field_klass);
			break;
		}
		case CEE_STFLD: {
			CHECK_STACK (&td, 2);
			token = read32 (td.ip + 1);
			field = mono_field_from_token (image, token, &klass, generic_context);
			gboolean is_static = !!(field->type->attrs & FIELD_ATTRIBUTE_STATIC);
			mono_class_init (klass);
			mt = mint_type(field->type);

			if (klass->marshalbyref) {
				g_assert (!is_static);
				ADD_CODE(&td, mt == MINT_TYPE_VT ? MINT_STRMFLD_VT : MINT_STRMFLD);
				ADD_CODE(&td, get_data_item_index (&td, field));
			} else  {
				if (is_static) {
					ADD_CODE (&td, MINT_POP);
					ADD_CODE (&td, 1);
					ADD_CODE (&td, mt == MINT_TYPE_VT ? MINT_STSFLD_VT : MINT_STSFLD);
					ADD_CODE (&td, get_data_item_index (&td, field));
				} else {
					ADD_CODE (&td, MINT_STFLD_I1 + mt - MINT_TYPE_I1);
					ADD_CODE (&td, klass->valuetype ? field->offset - sizeof(MonoObject) : field->offset);
				}
			}
			if (mt == MINT_TYPE_VT) {
				MonoClass *klass = mono_class_from_mono_type (field->type);
				int size = mono_class_value_size (klass, NULL);
				POP_VT(&td, size);
				WRITE32(&td, &size);
			}
			td.ip += 5;
			td.sp -= 2;
			break;
		}
		case CEE_LDSFLDA:
			token = read32 (td.ip + 1);
			field = mono_field_from_token (image, token, &klass, generic_context);
			ADD_CODE(&td, MINT_LDSFLDA);
			ADD_CODE(&td, get_data_item_index (&td, field));
			td.ip += 5;
			PUSH_SIMPLE_TYPE(&td, STACK_TYPE_MP);
			break;
		case CEE_LDSFLD:
			token = read32 (td.ip + 1);
			field = mono_field_from_token (image, token, &klass, generic_context);
			mt = mint_type(field->type);
			ADD_CODE(&td, mt == MINT_TYPE_VT ? MINT_LDSFLD_VT : MINT_LDSFLD);
			ADD_CODE(&td, get_data_item_index (&td, field));
			klass = NULL;
			if (mt == MINT_TYPE_VT) {
				MonoClass *klass = mono_class_from_mono_type (field->type);
				int size = mono_class_value_size (klass, NULL);
				PUSH_VT(&td, size);
				WRITE32(&td, &size);
				klass = field->type->data.klass;
			} else {
				if (mt == MINT_TYPE_O) 
					klass = mono_class_from_mono_type (field->type);
			}
			td.ip += 5;
			PUSH_TYPE(&td, stack_type [mt], klass);
			break;
		case CEE_STSFLD:
			CHECK_STACK (&td, 1);
			token = read32 (td.ip + 1);
			field = mono_field_from_token (image, token, &klass, generic_context);
			mt = mint_type(field->type);
			ADD_CODE(&td, mt == MINT_TYPE_VT ? MINT_STSFLD_VT : MINT_STSFLD);
			ADD_CODE(&td, get_data_item_index (&td, field));
			if (mt == MINT_TYPE_VT) {
				MonoClass *klass = mono_class_from_mono_type (field->type);
				int size = mono_class_value_size (klass, NULL);
				POP_VT (&td, size);
				WRITE32 (&td, &size);
			}
			td.ip += 5;
			--td.sp;
			break;
		case CEE_STOBJ: {
			int size;
			token = read32 (td.ip + 1);

			if (method->wrapper_type != MONO_WRAPPER_NONE)
				klass = (MonoClass *)mono_method_get_wrapper_data (method, token);
			else
				klass = mono_class_get_full (image, token, generic_context);

			ADD_CODE(&td, td.sp [-1].type == STACK_TYPE_VT ? MINT_STOBJ_VT : MINT_STOBJ);
			ADD_CODE(&td, get_data_item_index (&td, klass));
			if (td.sp [-1].type == STACK_TYPE_VT) {
				size = mono_class_value_size (klass, NULL);
				size = (size + 7) & ~7;
				td.vt_sp -= size;
			}
			td.ip += 5;
			td.sp -= 2;
			break;
		}
		case CEE_CONV_OVF_I_UN:
		case CEE_CONV_OVF_U_UN:
			CHECK_STACK (&td, 1);
			switch (td.sp [-1].type) {
			case STACK_TYPE_R8:
#if SIZEOF_VOID_P == 8
				ADD_CODE(&td, MINT_CONV_OVF_I8_UN_R8);
#else
				ADD_CODE(&td, MINT_CONV_OVF_I4_UN_R8);
#endif
				break;
			case STACK_TYPE_I8:
#if SIZEOF_VOID_P == 4
				ADD_CODE (&td, MINT_CONV_OVF_I4_UN_I8);
#endif
				break;
			case STACK_TYPE_I4:
#if SIZEOF_VOID_P == 8
				ADD_CODE(&td, MINT_CONV_I8_U4);
#endif
				break;
			default:
				g_assert_not_reached ();
				break;
			}
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I8);
			++td.ip;
			break;
		case CEE_CONV_OVF_I8_UN:
		case CEE_CONV_OVF_U8_UN:
			CHECK_STACK (&td, 1);
			switch (td.sp [-1].type) {
			case STACK_TYPE_R8:
				ADD_CODE(&td, MINT_CONV_OVF_I8_UN_R8);
				break;
			case STACK_TYPE_I8:
				if (*td.ip == CEE_CONV_OVF_I8_UN)
					ADD_CODE (&td, MINT_CONV_OVF_I8_U8);
				break;
			case STACK_TYPE_I4:
				ADD_CODE(&td, MINT_CONV_I8_U4);
				break;
			default:
				g_assert_not_reached ();
				break;
			}
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I8);
			++td.ip;
			break;
		case CEE_BOX: {
			int size;
			CHECK_STACK (&td, 1);
			token = read32 (td.ip + 1);
			if (method->wrapper_type != MONO_WRAPPER_NONE)
				klass = (MonoClass *)mono_method_get_wrapper_data (method, token);
			else
				klass = mono_class_get_full (image, token, generic_context);

			if (mono_class_is_nullable (klass)) {
				MonoMethod *target_method = mono_class_get_method_from_name (klass, "Box", 1);
				/* td.ip is incremented by interp_transform_call */
				interp_transform_call (&td, method, target_method, domain, generic_context, is_bb_start, body_start_offset, NULL, FALSE);
			} else if (!klass->valuetype) {
				/* already boxed, do nothing. */
				td.ip += 5;
			} else {
				if (mint_type (&klass->byval_arg) == MINT_TYPE_VT && !klass->enumtype) {
					size = mono_class_value_size (klass, NULL);
					size = (size + 7) & ~7;
					td.vt_sp -= size;
				}
				ADD_CODE(&td, MINT_BOX);
				ADD_CODE(&td, get_data_item_index (&td, klass));
				ADD_CODE (&td, 0);
				SET_TYPE(td.sp - 1, STACK_TYPE_O, klass);
				td.ip += 5;
			}

			break;
		}
		case CEE_NEWARR: {
			CHECK_STACK (&td, 1);
			token = read32 (td.ip + 1);

			if (method->wrapper_type != MONO_WRAPPER_NONE)
				klass = (MonoClass *)mono_method_get_wrapper_data (method, token);
			else
				klass = mono_class_get_full (image, token, generic_context);

			unsigned char lentype = (td.sp - 1)->type;
			if (lentype == STACK_TYPE_I8) {
				/* mimic mini behaviour */
				ADD_CODE (&td, MINT_CONV_OVF_U4_I8);
			} else {
				g_assert (lentype == STACK_TYPE_I4);
				ADD_CODE (&td, MINT_CONV_OVF_U4_I4);
			}
			SET_SIMPLE_TYPE (td.sp - 1, STACK_TYPE_I4);
			ADD_CODE (&td, MINT_NEWARR);
			ADD_CODE (&td, get_data_item_index (&td, klass));
			SET_TYPE (td.sp - 1, STACK_TYPE_O, klass);
			td.ip += 5;
			break;
		}
		case CEE_LDLEN:
			CHECK_STACK (&td, 1);
			SIMPLE_OP (td, MINT_LDLEN);
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I);
			break;
		case CEE_LDELEMA:
			CHECK_STACK (&td, 2);
			ENSURE_I4 (&td, 1);
			token = read32 (td.ip + 1);

			if (method->wrapper_type != MONO_WRAPPER_NONE)
				klass = (MonoClass *) mono_method_get_wrapper_data (method, token);
			else
				klass = mono_class_get_full (image, token, generic_context);

			if (!klass->valuetype && method->wrapper_type == MONO_WRAPPER_NONE && !readonly) {
				ADD_CODE (&td, MINT_LDELEMA_TC);
			} else {
				ADD_CODE (&td, MINT_LDELEMA);
			}
			ADD_CODE (&td, get_data_item_index (&td, klass));
			/* according to spec, ldelema bytecode is only used for 1-dim arrays */
			ADD_CODE (&td, 2);
			readonly = FALSE;

			td.ip += 5;
			--td.sp;
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_MP);
			break;
		case CEE_LDELEM_I1:
			CHECK_STACK (&td, 2);
			ENSURE_I4 (&td, 1);
			SIMPLE_OP (td, MINT_LDELEM_I1);
			--td.sp;
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I4);
			break;
		case CEE_LDELEM_U1:
			CHECK_STACK (&td, 2);
			ENSURE_I4 (&td, 1);
			SIMPLE_OP (td, MINT_LDELEM_U1);
			--td.sp;
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I4);
			break;
		case CEE_LDELEM_I2:
			CHECK_STACK (&td, 2);
			ENSURE_I4 (&td, 1);
			SIMPLE_OP (td, MINT_LDELEM_I2);
			--td.sp;
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I4);
			break;
		case CEE_LDELEM_U2:
			CHECK_STACK (&td, 2);
			ENSURE_I4 (&td, 1);
			SIMPLE_OP (td, MINT_LDELEM_U2);
			--td.sp;
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I4);
			break;
		case CEE_LDELEM_I4:
			CHECK_STACK (&td, 2);
			ENSURE_I4 (&td, 1);
			SIMPLE_OP (td, MINT_LDELEM_I4);
			--td.sp;
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I4);
			break;
		case CEE_LDELEM_U4:
			CHECK_STACK (&td, 2);
			ENSURE_I4 (&td, 1);
			SIMPLE_OP (td, MINT_LDELEM_U4);
			--td.sp;
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I4);
			break;
		case CEE_LDELEM_I8:
			CHECK_STACK (&td, 2);
			ENSURE_I4 (&td, 1);
			SIMPLE_OP (td, MINT_LDELEM_I8);
			--td.sp;
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I8);
			break;
		case CEE_LDELEM_I:
			CHECK_STACK (&td, 2);
			ENSURE_I4 (&td, 1);
			SIMPLE_OP (td, MINT_LDELEM_I);
			--td.sp;
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I);
			break;
		case CEE_LDELEM_R4:
			CHECK_STACK (&td, 2);
			ENSURE_I4 (&td, 1);
			SIMPLE_OP (td, MINT_LDELEM_R4);
			--td.sp;
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_R8);
			break;
		case CEE_LDELEM_R8:
			CHECK_STACK (&td, 2);
			ENSURE_I4 (&td, 1);
			SIMPLE_OP (td, MINT_LDELEM_R8);
			--td.sp;
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_R8);
			break;
		case CEE_LDELEM_REF:
			CHECK_STACK (&td, 2);
			ENSURE_I4 (&td, 1);
			SIMPLE_OP (td, MINT_LDELEM_REF);
			--td.sp;
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_O);
			break;
		case CEE_LDELEM:
			CHECK_STACK (&td, 2);
			token = read32 (td.ip + 1);
			klass = mono_class_get_full (image, token, generic_context);
			switch (mint_type (&klass->byval_arg)) {
				case MINT_TYPE_I1:
					ENSURE_I4 (&td, 1);
					SIMPLE_OP (td, MINT_LDELEM_I1);
					--td.sp;
					SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I4);
					break;
				case MINT_TYPE_U1:
					ENSURE_I4 (&td, 1);
					SIMPLE_OP (td, MINT_LDELEM_U1);
					--td.sp;
					SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I4);
					break;
				case MINT_TYPE_U2:
					ENSURE_I4 (&td, 1);
					SIMPLE_OP (td, MINT_LDELEM_U2);
					--td.sp;
					SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I4);
					break;
				case MINT_TYPE_I2:
					ENSURE_I4 (&td, 1);
					SIMPLE_OP (td, MINT_LDELEM_I2);
					--td.sp;
					SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I4);
					break;
				case MINT_TYPE_I4:
					ENSURE_I4 (&td, 1);
					SIMPLE_OP (td, MINT_LDELEM_I4);
					--td.sp;
					SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I4);
					break;
				case MINT_TYPE_I8:
					ENSURE_I4 (&td, 1);
					SIMPLE_OP (td, MINT_LDELEM_I8);
					--td.sp;
					SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I8);
					break;
				case MINT_TYPE_R4:
					ENSURE_I4 (&td, 1);
					SIMPLE_OP (td, MINT_LDELEM_R4);
					--td.sp;
					SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_R8);
					break;
				case MINT_TYPE_R8:
					ENSURE_I4 (&td, 1);
					SIMPLE_OP (td, MINT_LDELEM_R8);
					--td.sp;
					SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_R8);
					break;
				case MINT_TYPE_O:
					ENSURE_I4 (&td, 1);
					SIMPLE_OP (td, MINT_LDELEM_REF);
					--td.sp;
					SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_O);
					break;
				case MINT_TYPE_VT: {
					int size = mono_class_value_size (klass, NULL);
					ENSURE_I4 (&td, 1);
					SIMPLE_OP (td, MINT_LDELEM_VT);
					ADD_CODE (&td, get_data_item_index (&td, klass));
					WRITE32 (&td, &size);
					--td.sp;
					SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_VT);
					PUSH_VT (&td, size);
					break;
				}
				default: {
					GString *res = g_string_new ("");
					mono_type_get_desc (res, &klass->byval_arg, TRUE);
					g_print ("LDELEM: %s -> %d (%s)\n", klass->name, mint_type (&klass->byval_arg), res->str);
					g_string_free (res, TRUE);
					g_assert (0);
					break;
				}
			}
			td.ip += 4;
			break;
		case CEE_STELEM_I:
			CHECK_STACK (&td, 3);
			ENSURE_I4 (&td, 2);
			SIMPLE_OP (td, MINT_STELEM_I);
			td.sp -= 3;
			break;
		case CEE_STELEM_I1:
			CHECK_STACK (&td, 3);
			ENSURE_I4 (&td, 2);
			SIMPLE_OP (td, MINT_STELEM_I1);
			td.sp -= 3;
			break;
		case CEE_STELEM_I2:
			CHECK_STACK (&td, 3);
			ENSURE_I4 (&td, 2);
			SIMPLE_OP (td, MINT_STELEM_I2);
			td.sp -= 3;
			break;
		case CEE_STELEM_I4:
			CHECK_STACK (&td, 3);
			ENSURE_I4 (&td, 2);
			SIMPLE_OP (td, MINT_STELEM_I4);
			td.sp -= 3;
			break;
		case CEE_STELEM_I8:
			CHECK_STACK (&td, 3);
			ENSURE_I4 (&td, 2);
			SIMPLE_OP (td, MINT_STELEM_I8);
			td.sp -= 3;
			break;
		case CEE_STELEM_R4:
			CHECK_STACK (&td, 3);
			ENSURE_I4 (&td, 2);
			SIMPLE_OP (td, MINT_STELEM_R4);
			td.sp -= 3;
			break;
		case CEE_STELEM_R8:
			CHECK_STACK (&td, 3);
			ENSURE_I4 (&td, 2);
			SIMPLE_OP (td, MINT_STELEM_R8);
			td.sp -= 3;
			break;
		case CEE_STELEM_REF:
			CHECK_STACK (&td, 3);
			ENSURE_I4 (&td, 2);
			SIMPLE_OP (td, MINT_STELEM_REF);
			td.sp -= 3;
			break;
		case CEE_STELEM:
			CHECK_STACK (&td, 3);
			ENSURE_I4 (&td, 2);
			token = read32 (td.ip + 1);
			klass = mono_class_get_full (image, token, generic_context);
			switch (mint_type (&klass->byval_arg)) {
				case MINT_TYPE_U1:
					SIMPLE_OP (td, MINT_STELEM_U1);
					break;
				case MINT_TYPE_I4:
					SIMPLE_OP (td, MINT_STELEM_I4);
					break;
				case MINT_TYPE_O:
					SIMPLE_OP (td, MINT_STELEM_REF);
					break;
				case MINT_TYPE_VT: {
					int size = mono_class_value_size (klass, NULL);
					SIMPLE_OP (td, MINT_STELEM_VT);
					ADD_CODE (&td, get_data_item_index (&td, klass));
					WRITE32 (&td, &size);
					POP_VT (&td, size);
					break;
				}
				default: {
					GString *res = g_string_new ("");
					mono_type_get_desc (res, &klass->byval_arg, TRUE);
					g_print ("STELEM: %s -> %d (%s)\n", klass->name, mint_type (&klass->byval_arg), res->str);
					g_string_free (res, TRUE);
					g_assert (0);
					break;
				}
			}
			td.ip += 4;
			td.sp -= 3;
			break;
#if 0
		case CEE_CONV_OVF_U1:

		case CEE_CONV_OVF_I8:

#if SIZEOF_VOID_P == 8
		case CEE_CONV_OVF_U:
#endif
		case CEE_REFANYVAL: ves_abort(); break;
#endif
		case CEE_CKFINITE:
			CHECK_STACK (&td, 1);
			SIMPLE_OP (td, MINT_CKFINITE);
			break;
		case CEE_CONV_OVF_I1:
		case CEE_CONV_OVF_I1_UN:
			CHECK_STACK (&td, 1);
			switch (td.sp [-1].type) {
			case STACK_TYPE_R8:
				ADD_CODE(&td, MINT_CONV_OVF_I1_R8);
				break;
			case STACK_TYPE_I4:
				ADD_CODE(&td, MINT_CONV_OVF_I1_I4);
				break;
			case STACK_TYPE_I8:
				ADD_CODE(&td, MINT_CONV_OVF_I1_I8);
				break;
			default:
				g_assert_not_reached ();
			}
			++td.ip;
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I4);
			break;
		case CEE_CONV_OVF_U1:
		case CEE_CONV_OVF_U1_UN:
			CHECK_STACK (&td, 1);
			switch (td.sp [-1].type) {
			case STACK_TYPE_R8:
				ADD_CODE(&td, MINT_CONV_OVF_U1_R8);
				break;
			case STACK_TYPE_I4:
				ADD_CODE(&td, MINT_CONV_OVF_U1_I4);
				break;
			case STACK_TYPE_I8:
				ADD_CODE(&td, MINT_CONV_OVF_U1_I8);
				break;
			default:
				g_assert_not_reached ();
			}
			++td.ip;
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I4);
			break;
		case CEE_CONV_OVF_I2:
		case CEE_CONV_OVF_I2_UN:
			CHECK_STACK (&td, 1);
			switch (td.sp [-1].type) {
			case STACK_TYPE_R8:
				ADD_CODE(&td, MINT_CONV_OVF_I2_R8);
				break;
			case STACK_TYPE_I4:
				ADD_CODE(&td, MINT_CONV_OVF_I2_I4);
				break;
			case STACK_TYPE_I8:
				ADD_CODE(&td, MINT_CONV_OVF_I2_I8);
				break;
			default:
				g_assert_not_reached ();
			}
			++td.ip;
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I4);
			break;
		case CEE_CONV_OVF_U2_UN:
		case CEE_CONV_OVF_U2:
			CHECK_STACK (&td, 1);
			switch (td.sp [-1].type) {
			case STACK_TYPE_R8:
				ADD_CODE(&td, MINT_CONV_OVF_U2_R8);
				break;
			case STACK_TYPE_I4:
				ADD_CODE(&td, MINT_CONV_OVF_U2_I4);
				break;
			case STACK_TYPE_I8:
				ADD_CODE(&td, MINT_CONV_OVF_U2_I8);
				break;
			default:
				g_assert_not_reached ();
			}
			++td.ip;
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I4);
			break;
#if SIZEOF_VOID_P == 4
		case CEE_CONV_OVF_I:
#endif
		case CEE_CONV_OVF_I4:
		case CEE_CONV_OVF_I4_UN:
			CHECK_STACK (&td, 1);
			switch (td.sp [-1].type) {
			case STACK_TYPE_R8:
				ADD_CODE(&td, MINT_CONV_OVF_I4_R8);
				break;
			case STACK_TYPE_I4:
				if (*td.ip == CEE_CONV_OVF_I4_UN)
					ADD_CODE(&td, MINT_CONV_OVF_I4_U4);
				break;
			case STACK_TYPE_I8:
				if (*td.ip == CEE_CONV_OVF_I4_UN)
					ADD_CODE (&td, MINT_CONV_OVF_I4_U8);
				else
					ADD_CODE (&td, MINT_CONV_OVF_I4_I8);
				break;
			default:
				g_assert_not_reached ();
			}
			++td.ip;
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I4);
			break;
#if SIZEOF_VOID_P == 4
		case CEE_CONV_OVF_U:
#endif
		case CEE_CONV_OVF_U4:
		case CEE_CONV_OVF_U4_UN:
			CHECK_STACK (&td, 1);
			switch (td.sp [-1].type) {
			case STACK_TYPE_R8:
				ADD_CODE(&td, MINT_CONV_OVF_U4_R8);
				break;
			case STACK_TYPE_I4:
				if (*td.ip != CEE_CONV_OVF_U4_UN)
					ADD_CODE(&td, MINT_CONV_OVF_U4_I4);
				break;
			case STACK_TYPE_I8:
				ADD_CODE(&td, MINT_CONV_OVF_U4_I8);
				break;
			default:
				g_assert_not_reached ();
			}
			++td.ip;
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I4);
			break;
#if SIZEOF_VOID_P == 8
		case CEE_CONV_OVF_I:
#endif
		case CEE_CONV_OVF_I8:
			CHECK_STACK (&td, 1);
			switch (td.sp [-1].type) {
			case STACK_TYPE_R8:
				ADD_CODE(&td, MINT_CONV_OVF_I8_R8);
				break;
			case STACK_TYPE_I4:
				ADD_CODE(&td, MINT_CONV_I8_I4);
				break;
			case STACK_TYPE_I8:
				break;
			default:
				g_assert_not_reached ();
			}
			++td.ip;
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I8);
			break;
#if SIZEOF_VOID_P == 8
		case CEE_CONV_OVF_U:
#endif
		case CEE_CONV_OVF_U8:
			CHECK_STACK (&td, 1);
			switch (td.sp [-1].type) {
			case STACK_TYPE_R8:
				ADD_CODE(&td, MINT_CONV_OVF_U8_R8);
				break;
			case STACK_TYPE_I4:
				ADD_CODE(&td, MINT_CONV_OVF_U8_I4);
				break;
			case STACK_TYPE_I8:
				ADD_CODE (&td, MINT_CONV_OVF_U8_I8);
				break;
			default:
				g_assert_not_reached ();
			}
			++td.ip;
			SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I8);
			break;
		case CEE_LDTOKEN: {
			int size;
			gpointer handle;
			token = read32 (td.ip + 1);
			if (method->wrapper_type == MONO_WRAPPER_DYNAMIC_METHOD || method->wrapper_type == MONO_WRAPPER_SYNCHRONIZED) {
				handle = mono_method_get_wrapper_data (method, token);
				klass = (MonoClass *) mono_method_get_wrapper_data (method, token + 1);
				if (klass == mono_defaults.typehandle_class)
					handle = &((MonoClass *) handle)->byval_arg;
			} else {
				handle = mono_ldtoken (image, token, &klass, generic_context);
			}
			mt = mint_type (&klass->byval_arg);
			g_assert (mt == MINT_TYPE_VT);
			size = mono_class_value_size (klass, NULL);
			g_assert (size == sizeof(gpointer));
			PUSH_VT (&td, sizeof(gpointer));
			ADD_CODE (&td, MINT_LDTOKEN);
			ADD_CODE (&td, get_data_item_index (&td, handle));

			SET_TYPE (td.sp, stack_type [mt], klass);
			td.sp++;
			td.ip += 5;
			break;
		}
		case CEE_ADD_OVF:
			binary_arith_op(&td, MINT_ADD_OVF_I4);
			++td.ip;
			break;
		case CEE_ADD_OVF_UN:
			binary_arith_op(&td, MINT_ADD_OVF_UN_I4);
			++td.ip;
			break;
		case CEE_MUL_OVF:
			binary_arith_op(&td, MINT_MUL_OVF_I4);
			++td.ip;
			break;
		case CEE_MUL_OVF_UN:
			binary_arith_op(&td, MINT_MUL_OVF_UN_I4);
			++td.ip;
			break;
		case CEE_SUB_OVF:
			binary_arith_op(&td, MINT_SUB_OVF_I4);
			++td.ip;
			break;
		case CEE_SUB_OVF_UN:
			binary_arith_op(&td, MINT_SUB_OVF_UN_I4);
			++td.ip;
			break;
		case CEE_ENDFINALLY:
			SIMPLE_OP (td, MINT_ENDFINALLY);
			generating_code = 0;
			break;
		case CEE_LEAVE:
			td.sp = td.stack;
			handle_branch (&td, MINT_LEAVE_S, MINT_LEAVE, 5 + read32 (td.ip + 1));
			td.ip += 5;
			generating_code = 0;
			break;
		case CEE_LEAVE_S:
			td.sp = td.stack;
			handle_branch (&td, MINT_LEAVE_S, MINT_LEAVE, 2 + (gint8)td.ip [1]);
			td.ip += 2;
			generating_code = 0;
			break;
		case CEE_UNUSED41:
			++td.ip;
		        switch (*td.ip) {
				case CEE_MONO_CALLI_EXTRA_ARG:
					/* Same as CEE_CALLI, except that we drop the extra arg required for llvm specific behaviour */
					ADD_CODE (&td, MINT_POP);
					ADD_CODE (&td, 1);
					--td.sp;
					interp_transform_call (&td, method, NULL, domain, generic_context, is_bb_start, body_start_offset, NULL, FALSE);
					break;
				case CEE_MONO_JIT_ICALL_ADDR: {
					guint32 token;
					gpointer func;
					MonoJitICallInfo *info;

					token = read32 (td.ip + 1);
					td.ip += 5;
					func = mono_method_get_wrapper_data (method, token);
					info = mono_find_jit_icall_by_addr (func);

					ADD_CODE (&td, MINT_LDFTN);
					ADD_CODE (&td, get_data_item_index (&td, func));
					PUSH_SIMPLE_TYPE (&td, STACK_TYPE_I);
					break;
				}
				case CEE_MONO_ICALL: {
					guint32 token;
					gpointer func;
					MonoJitICallInfo *info;

					token = read32 (td.ip + 1);
					td.ip += 5;
					func = mono_method_get_wrapper_data (method, token);
					info = mono_find_jit_icall_by_addr (func);
					g_assert (info);

					CHECK_STACK (&td, info->sig->param_count);
					switch (info->sig->param_count) {
					case 0:
						if (MONO_TYPE_IS_VOID (info->sig->ret))
							ADD_CODE (&td,MINT_ICALL_V_V);
						else
							ADD_CODE (&td, MINT_ICALL_V_P);
						break;
					case 1:
						if (MONO_TYPE_IS_VOID (info->sig->ret))
							ADD_CODE (&td,MINT_ICALL_P_V);
						else
							ADD_CODE (&td,MINT_ICALL_P_P);
						break;
					case 2:
						if (MONO_TYPE_IS_VOID (info->sig->ret)) {
							if (info->sig->params [1]->type == MONO_TYPE_I4)
								ADD_CODE (&td,MINT_ICALL_PI_V);
							else
								ADD_CODE (&td,MINT_ICALL_PP_V);
						} else {
							if (info->sig->params [1]->type == MONO_TYPE_I4)
								ADD_CODE (&td,MINT_ICALL_PI_P);
							else
								ADD_CODE (&td,MINT_ICALL_PP_P);
						}
						break;
					case 3:
						g_assert (MONO_TYPE_IS_VOID (info->sig->ret));
						if (info->sig->params [2]->type == MONO_TYPE_I4)
							ADD_CODE (&td,MINT_ICALL_PPI_V);
						else
							ADD_CODE (&td,MINT_ICALL_PPP_V);
						break;
					default:
						g_assert_not_reached ();
					}

					if (func == mono_ftnptr_to_delegate) {
						g_error ("TODO: ?");
					}
					ADD_CODE(&td, get_data_item_index (&td, func));
					td.sp -= info->sig->param_count;

					if (!MONO_TYPE_IS_VOID (info->sig->ret)) {
						td.sp ++;
						SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I);
					}
					break;
				}
			case CEE_MONO_VTADDR: {
				int size;
				CHECK_STACK (&td, 1);
				if (method->wrapper_type == MONO_WRAPPER_MANAGED_TO_NATIVE)
					size = mono_class_native_size(td.sp [-1].klass, NULL);
				else
					size = mono_class_value_size(td.sp [-1].klass, NULL);
				size = (size + 7) & ~7;
				ADD_CODE(&td, MINT_VTRESULT);
				ADD_CODE(&td, 0);
				WRITE32(&td, &size);
				td.vt_sp -= size;
				++td.ip;
				SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_MP);
				break;
			}
			case CEE_MONO_LDPTR:
			case CEE_MONO_CLASSCONST:
				token = read32 (td.ip + 1);
				td.ip += 5;
				ADD_CODE(&td, MINT_MONO_LDPTR);
				ADD_CODE(&td, get_data_item_index (&td, mono_method_get_wrapper_data (method, token)));
				td.sp [0].type = STACK_TYPE_I;
				++td.sp;
				break;
			case CEE_MONO_OBJADDR:
				CHECK_STACK (&td, 1);
				++td.ip;
				td.sp[-1].type = STACK_TYPE_MP;
				/* do nothing? */
				break;
			case CEE_MONO_NEWOBJ:
				token = read32 (td.ip + 1);
				td.ip += 5;
				ADD_CODE(&td, MINT_MONO_NEWOBJ);
				ADD_CODE(&td, get_data_item_index (&td, mono_method_get_wrapper_data (method, token)));
				td.sp [0].type = STACK_TYPE_O;
				++td.sp;
				break;
			case CEE_MONO_RETOBJ:
				CHECK_STACK (&td, 1);
				token = read32 (td.ip + 1);
				td.ip += 5;
				ADD_CODE(&td, MINT_MONO_RETOBJ);
				td.sp--;

				klass = (MonoClass *)mono_method_get_wrapper_data (method, token);
				
				/*stackval_from_data (signature->ret, frame->retval, sp->data.vt, signature->pinvoke);*/

				if (td.sp > td.stack)
					g_warning ("CEE_MONO_RETOBJ: more values on stack: %d", td.sp-td.stack);
				break;
			case CEE_MONO_LDNATIVEOBJ:
				token = read32 (td.ip + 1);
				td.ip += 5;
				klass = (MonoClass *)mono_method_get_wrapper_data (method, token);
				g_assert(klass->valuetype);
				SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_MP);
				break;
			case CEE_MONO_SAVE_LMF:
			case CEE_MONO_RESTORE_LMF:
			case CEE_MONO_NOT_TAKEN:
				++td.ip;
				break;
			case CEE_MONO_LDPTR_INT_REQ_FLAG:
				ADD_CODE (&td, MINT_MONO_LDPTR);
				ADD_CODE (&td, get_data_item_index (&td, mono_thread_interruption_request_flag ()));
				PUSH_TYPE (&td, STACK_TYPE_MP, NULL);
				++td.ip;
				break;
			default:
				g_error ("transform.c: Unimplemented opcode: 0xF0 %02x at 0x%x\n", *td.ip, td.ip-header->code);
			}
			break;
#if 0
		case CEE_PREFIX7:
		case CEE_PREFIX6:
		case CEE_PREFIX5:
		case CEE_PREFIX4:
		case CEE_PREFIX3:
		case CEE_PREFIX2:
		case CEE_PREFIXREF: ves_abort(); break;
#endif
		/*
		 * Note: Exceptions thrown when executing a prefixed opcode need
		 * to take into account the number of prefix bytes (usually the
		 * throw point is just (ip - n_prefix_bytes).
		 */
		case CEE_PREFIX1: 
			++td.ip;
			switch (*td.ip) {
#if 0
			case CEE_ARGLIST: ves_abort(); break;
#endif
			case CEE_CEQ:
				CHECK_STACK(&td, 2);
				if (td.sp [-1].type == STACK_TYPE_O || td.sp [-1].type == STACK_TYPE_MP)
					ADD_CODE(&td, MINT_CEQ_I4 + STACK_TYPE_I - STACK_TYPE_I4);
				else
					ADD_CODE(&td, MINT_CEQ_I4 + td.sp [-1].type - STACK_TYPE_I4);
				--td.sp;
				SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I4);
				++td.ip;
				break;
			case CEE_CGT:
				CHECK_STACK(&td, 2);
				if (td.sp [-1].type == STACK_TYPE_O || td.sp [-1].type == STACK_TYPE_MP)
					ADD_CODE(&td, MINT_CGT_I4 + STACK_TYPE_I - STACK_TYPE_I4);
				else
					ADD_CODE(&td, MINT_CGT_I4 + td.sp [-1].type - STACK_TYPE_I4);
				--td.sp;
				SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I4);
				++td.ip;
				break;
			case CEE_CGT_UN:
				CHECK_STACK(&td, 2);
				if (td.sp [-1].type == STACK_TYPE_O || td.sp [-1].type == STACK_TYPE_MP)
					ADD_CODE(&td, MINT_CGT_UN_I4 + STACK_TYPE_I - STACK_TYPE_I4);
				else
					ADD_CODE(&td, MINT_CGT_UN_I4 + td.sp [-1].type - STACK_TYPE_I4);
				--td.sp;
				SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I4);
				++td.ip;
				break;
			case CEE_CLT:
				CHECK_STACK(&td, 2);
				if (td.sp [-1].type == STACK_TYPE_O || td.sp [-1].type == STACK_TYPE_MP)
					ADD_CODE(&td, MINT_CLT_I4 + STACK_TYPE_I - STACK_TYPE_I4);
				else
					ADD_CODE(&td, MINT_CLT_I4 + td.sp [-1].type - STACK_TYPE_I4);
				--td.sp;
				SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I4);
				++td.ip;
				break;
			case CEE_CLT_UN:
				CHECK_STACK(&td, 2);
				if (td.sp [-1].type == STACK_TYPE_O || td.sp [-1].type == STACK_TYPE_MP)
					ADD_CODE(&td, MINT_CLT_UN_I4 + STACK_TYPE_I - STACK_TYPE_I4);
				else
					ADD_CODE(&td, MINT_CLT_UN_I4 + td.sp [-1].type - STACK_TYPE_I4);
				--td.sp;
				SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_I4);
				++td.ip;
				break;
			case CEE_LDVIRTFTN: /* fallthrough */
			case CEE_LDFTN: {
				MonoMethod *m;
				if (*td.ip == CEE_LDVIRTFTN) {
					CHECK_STACK (&td, 1);
					--td.sp;
				}
				token = read32 (td.ip + 1);
				if (method->wrapper_type != MONO_WRAPPER_NONE)
					m = (MonoMethod *)mono_method_get_wrapper_data (method, token);
				else 
					m = mono_get_method_full (image, token, NULL, generic_context);

				if (method->wrapper_type == MONO_WRAPPER_NONE && m->iflags & METHOD_IMPL_ATTRIBUTE_SYNCHRONIZED)
					m = mono_marshal_get_synchronized_wrapper (m);

				ADD_CODE(&td, *td.ip == CEE_LDFTN ? MINT_LDFTN : MINT_LDVIRTFTN);
				ADD_CODE(&td, get_data_item_index (&td, mono_interp_get_runtime_method (domain, m, &error)));
				mono_error_cleanup (&error); /* FIXME: don't swallow the error */
				td.ip += 5;
				PUSH_SIMPLE_TYPE (&td, STACK_TYPE_F);
				break;
			}
			case CEE_LDARG:
				load_arg (&td, read16 (td.ip + 1));
				td.ip += 3;
				break;
			case CEE_LDARGA: {
				int n = read16 (td.ip + 1);
				if (n == 0 && signature->hasthis) {
					g_error ("LDTHISA: NOPE");
					ADD_CODE(&td, MINT_LDTHISA);
				}
				else {
					ADD_CODE(&td, MINT_LDARGA);
					ADD_CODE(&td, td.rtm->arg_offsets [n]); /* FIX for large offsets */
				}
				PUSH_SIMPLE_TYPE(&td, STACK_TYPE_MP);
				td.ip += 3;
				break;
			}
			case CEE_STARG:
				store_arg (&td, read16 (td.ip + 1));
				td.ip += 3;
				break;
			case CEE_LDLOC:
				load_local (&td, read16 (td.ip + 1));
				td.ip += 3;
				break;
			case CEE_LDLOCA:
				ADD_CODE(&td, MINT_LDLOCA_S);
				ADD_CODE(&td, td.rtm->local_offsets [read16 (td.ip + 1)]);
				PUSH_SIMPLE_TYPE(&td, STACK_TYPE_MP);
				td.ip += 3;
				break;
			case CEE_STLOC:
				store_local (&td, read16 (td.ip + 1));
				td.ip += 3;
				break;
			case CEE_LOCALLOC:
				CHECK_STACK (&td, 1);
#if SIZEOF_VOID_P == 8
				if (td.sp [-1].type == STACK_TYPE_I8)
					ADD_CODE(&td, MINT_CONV_I4_I8);
#endif				
				ADD_CODE(&td, MINT_LOCALLOC);
				if (td.sp != td.stack + 1)
					g_warning("CEE_LOCALLOC: stack not empty");
				++td.ip;
				SET_SIMPLE_TYPE(td.sp - 1, STACK_TYPE_MP);
				break;
#if 0
			case CEE_UNUSED57: ves_abort(); break;
			case CEE_ENDFILTER: ves_abort(); break;
#endif
			case CEE_UNALIGNED_:
				++td.ip;
				/* FIX: should do something? */;
				break;
			case CEE_VOLATILE_:
				++td.ip;
				/* FIX: should do something? */;
				break;
			case CEE_TAIL_:
				++td.ip;
				/* FIX: should do something? */;
				break;
			case CEE_INITOBJ:
				CHECK_STACK(&td, 1);
				token = read32 (td.ip + 1);
				klass = mono_class_get_full (image, token, generic_context);
				if (klass->valuetype) {
					ADD_CODE (&td, MINT_INITOBJ);
					i32 = mono_class_value_size (klass, NULL);
					WRITE32 (&td, &i32);
				} else {
					ADD_CODE (&td, MINT_LDNULL);
					ADD_CODE (&td, MINT_STIND_REF);
				}
				td.ip += 5;
				--td.sp;
				break;
			case CEE_CPBLK:
				CHECK_STACK(&td, 3);
				/* FIX? convert length to I8? */
				ADD_CODE(&td, MINT_CPBLK);
				td.sp -= 3;
				++td.ip;
				break;
			case CEE_READONLY_:
				readonly = TRUE;
				td.ip += 1;
				break;
			case CEE_CONSTRAINED_:
				token = read32 (td.ip + 1);
				constrained_class = mono_class_get_full (image, token, generic_context);
				mono_class_init (constrained_class);
				td.ip += 5;
				break;
			case CEE_INITBLK:
				CHECK_STACK(&td, 3);
				ADD_CODE(&td, MINT_INITBLK);
				td.sp -= 3;
				break;
#if 0
			case CEE_NO_:
				/* FIXME: implement */
				ip += 2;
				break;
#endif
			case CEE_RETHROW:
				SIMPLE_OP (td, MINT_RETHROW);
				generating_code = 0;
				break;
			case CEE_SIZEOF: {
				gint32 size;
				token = read32 (td.ip + 1);
				td.ip += 5;
				if (mono_metadata_token_table (token) == MONO_TABLE_TYPESPEC && !image_is_dynamic (method->klass->image) && !generic_context) {
					int align;
					MonoType *type = mono_type_create_from_typespec (image, token);
					size = mono_type_size (type, &align);
				} else {
					int align;
					MonoClass *szclass = mono_class_get_full (image, token, generic_context);
					mono_class_init (szclass);
#if 0
					if (!szclass->valuetype)
						THROW_EX (mono_exception_from_name (mono_defaults.corlib, "System", "InvalidProgramException"), ip - 5);
#endif
					size = mono_type_size (&szclass->byval_arg, &align);
				} 
				ADD_CODE(&td, MINT_LDC_I4);
				WRITE32(&td, &size);
				PUSH_SIMPLE_TYPE(&td, STACK_TYPE_I4);
				break;
			}
#if 0
			case CEE_REFANYTYPE: ves_abort(); break;
#endif
			default:
				g_error ("transform.c: Unimplemented opcode: 0xFE %02x (%s) at 0x%x\n", *td.ip, mono_opcode_name (256 + *td.ip), td.ip-header->code);
			}
			break;
		default:
			g_error ("transform.c: Unimplemented opcode: %02x at 0x%x\n", *td.ip, td.ip-header->code);
		}

		if (td.new_ip - td.new_code != new_in_start_offset) 
			td.last_new_ip = td.new_code + new_in_start_offset;
		else if (td.is_bb_start [td.in_start - td.il_code])
			td.is_bb_start [td.ip - td.il_code] = 1;
			
		td.last_ip = td.in_start;
	}

	if (mono_interp_traceopt) {
		const guint16 *p = td.new_code;
		printf("Runtime method: %p, VT stack size: %d\n", rtm, td.max_vt_sp);
		printf("Calculated stack size: %d, stated size: %d\n", td.max_stack_height, header->max_stack);
		while (p < td.new_ip) {
			p = mono_interp_dis_mintop(td.new_code, p);
			printf("\n");
		}
	}
	g_assert (td.max_stack_height <= (header->max_stack + 1));

	rtm->clauses = mono_mempool_alloc (domain->mp, header->num_clauses * sizeof(MonoExceptionClause));
	memcpy (rtm->clauses, header->clauses, header->num_clauses * sizeof(MonoExceptionClause));
	rtm->code = mono_mempool_alloc (domain->mp, (td.new_ip - td.new_code) * sizeof(gushort));
	memcpy (rtm->code, td.new_code, (td.new_ip - td.new_code) * sizeof(gushort));
	g_free (td.new_code);
	rtm->new_body_start = rtm->code + body_start_offset;
	rtm->num_clauses = header->num_clauses;
	for (i = 0; i < header->num_clauses; i++) {
		MonoExceptionClause *c = rtm->clauses + i;
		int end_off = c->try_offset + c->try_len;
		c->try_offset = td.in_offsets [c->try_offset];
		c->try_len = td.in_offsets [end_off] - c->try_offset;
		end_off = c->handler_offset + c->handler_len;
		c->handler_offset = td.in_offsets [c->handler_offset];
		c->handler_len = td.in_offsets [end_off] - c->handler_offset;
	}
	rtm->vt_stack_size = td.max_vt_sp;
	rtm->alloca_size = rtm->locals_size + rtm->args_size + rtm->vt_stack_size + rtm->stack_size;
	rtm->data_items = mono_mempool_alloc (domain->mp, td.n_data_items * sizeof (td.data_items [0]));
	memcpy (rtm->data_items, td.data_items, td.n_data_items * sizeof (td.data_items [0]));
	g_free (td.in_offsets);
	g_free (td.forward_refs);
	for (i = 0; i < header->code_size; ++i)
		g_free (td.stack_state [i]);
	g_free (td.stack_state);
	g_free (td.stack_height);
	g_free (td.vt_stack_size);
	g_free (td.data_items);
	g_free (td.stack);
	g_hash_table_destroy (td.data_hash);
}

static mono_mutex_t calc_section;

void 
mono_interp_transform_init (void)
{
	mono_os_mutex_init_recursive(&calc_section);
}

MonoException *
mono_interp_transform_method (RuntimeMethod *runtime_method, ThreadContext *context)
{
	int i, align, size, offset;
	MonoMethod *method = runtime_method->method;
	MonoImage *image = method->klass->image;
	MonoMethodHeader *header = mono_method_get_header (method);
	MonoMethodSignature *signature = mono_method_signature (method);
	register const unsigned char *ip, *end;
	const MonoOpcode *opcode;
	MonoMethod *m;
	MonoClass *class;
	MonoDomain *domain = mono_domain_get ();
	unsigned char *is_bb_start;
	int in;
	MonoVTable *method_class_vt;
	int backwards;
	MonoGenericContext *generic_context = NULL;

	// g_printerr ("TRANSFORM(0x%016lx): begin %s::%s\n", mono_thread_current (), method->klass->name, method->name);
	method_class_vt = mono_class_vtable (domain, runtime_method->method->klass);
	if (!method_class_vt->initialized) {
		MonoError error;
		jmp_buf env;
		MonoInvocation *last_env_frame = context->env_frame;
		jmp_buf *old_env = context->current_env;
		error_init (&error);

		if (setjmp(env)) {
			MonoException *failed = context->env_frame->ex;
			context->env_frame->ex = NULL;
			context->env_frame = last_env_frame;
			context->current_env = old_env;
			return failed;
		}
		context->env_frame = context->current_frame;
		context->current_env = &env;
		mono_runtime_class_init_full (method_class_vt, &error);
		if (!mono_error_ok (&error)) {
			return mono_error_convert_to_exception (&error);
		}
		context->env_frame = last_env_frame;
		context->current_env = old_env;
	}

	mono_profiler_method_jit (method); /* sort of... */

	if (mono_method_signature (method)->is_inflated)
		generic_context = mono_method_get_context (method);
	else {
		MonoGenericContainer *generic_container = mono_method_get_generic_container (method);
		if (generic_container)
			generic_context = &generic_container->context;
	}

	if (method->iflags & (METHOD_IMPL_ATTRIBUTE_INTERNAL_CALL | METHOD_IMPL_ATTRIBUTE_RUNTIME)) {
		MonoMethod *nm = NULL;
		mono_os_mutex_lock(&calc_section);
		if (runtime_method->transformed) {
			mono_os_mutex_unlock(&calc_section);
			g_error ("FIXME: no jit info?");
			mono_profiler_method_end_jit (method, NULL, MONO_PROFILE_OK);
			return NULL;
		}

		/* assumes all internal calls with an array this are built in... */
		if (method->iflags & METHOD_IMPL_ATTRIBUTE_INTERNAL_CALL && (! mono_method_signature (method)->hasthis || method->klass->rank == 0)) {
			nm = mono_marshal_get_native_wrapper (method, TRUE, FALSE);
			signature = mono_method_signature (nm);
		} else {
			const char *name = method->name;
			if (method->klass->parent == mono_defaults.multicastdelegate_class) {
				if (*name == '.' && (strcmp (name, ".ctor") == 0)) {
					MonoJitICallInfo *mi = mono_find_jit_icall_by_name ("ves_icall_mono_delegate_ctor");
					g_assert (mi);
					char *wrapper_name = g_strdup_printf ("__icall_wrapper_%s", mi->name);
					nm = mono_marshal_get_icall_wrapper (mi->sig, wrapper_name, mi->func, TRUE);
				} else if (*name == 'I' && (strcmp (name, "Invoke") == 0)) {
					nm = mono_marshal_get_delegate_invoke (method, NULL);
				} else if (*name == 'B' && (strcmp (name, "BeginInvoke") == 0)) {
					nm = mono_marshal_get_delegate_begin_invoke (method);
				} else if (*name == 'E' && (strcmp (name, "EndInvoke") == 0)) {
					nm = mono_marshal_get_delegate_end_invoke (method);
				}
			} 
			if (nm == NULL) {
				runtime_method->code = g_malloc(sizeof(short));
				runtime_method->code[0] = MINT_CALLRUN;
			}
		}
		if (nm == NULL) {
			runtime_method->stack_size = sizeof (stackval); /* for tracing */
			runtime_method->alloca_size = runtime_method->stack_size;
			runtime_method->transformed = TRUE;
			mono_os_mutex_unlock(&calc_section);
			mono_profiler_method_end_jit (method, NULL, MONO_PROFILE_OK);
			return NULL;
		}
		method = nm;
		header = mono_method_get_header (nm);
		mono_os_mutex_unlock(&calc_section);
	}
	g_assert ((signature->param_count + signature->hasthis) < 1000);
	g_assert (header->max_stack < 10000);
	/* intern the strings in the method. */
	ip = header->code;
	end = ip + header->code_size;

	is_bb_start = g_malloc0(header->code_size);
	is_bb_start [0] = 1;
	while (ip < end) {
		in = *ip;
		if (in == 0xfe) {
			ip++;
			in = *ip + 256;
		}
		else if (in == 0xf0) {
			ip++;
			in = *ip + MONO_CEE_MONO_ICALL;
		}
		opcode = &mono_opcodes [in];
		switch (opcode->argument) {
		case MonoInlineNone:
			++ip;
			break;
		case MonoInlineString:
			if (method->wrapper_type == MONO_WRAPPER_NONE)
				mono_ldstr (domain, image, mono_metadata_token_index (read32 (ip + 1)));
			ip += 5;
			break;
		case MonoInlineType:
			if (method->wrapper_type == MONO_WRAPPER_NONE) {
				class = mono_class_get_full (image, read32 (ip + 1), generic_context);
				mono_class_init (class);
				/* quick fix to not do this for the fake ptr classes - probably should not be getting the vtable at all here */
#if 0
				g_error ("FIXME: interface method lookup: %s (in method %s)", class->name, method->name);
				if (!(class->flags & TYPE_ATTRIBUTE_INTERFACE) && class->interface_offsets != NULL)
					mono_class_vtable (domain, class);
#endif
			}
			ip += 5;
			break;
		case MonoInlineMethod:
			if (method->wrapper_type == MONO_WRAPPER_NONE && *ip != CEE_CALLI) {
				m = mono_get_method_full (image, read32 (ip + 1), NULL, generic_context);
				if (m == NULL) {
					g_free (is_bb_start);
					g_error ("FIXME: where to get method and class string?"); 
					return NULL;
					// return mono_get_exception_missing_method ();
				}
				mono_class_init (m->klass);
				if (!mono_class_is_interface (m->klass))
					mono_class_vtable (domain, m->klass);
			}
			ip += 5;
			break;
		case MonoInlineField:
		case MonoInlineSig:
		case MonoInlineI:
		case MonoInlineTok:
		case MonoShortInlineR:
			ip += 5;
			break;
		case MonoInlineBrTarget:
			offset = read32 (ip + 1);
			ip += 5;
			backwards = offset < 0;
			offset += ip - header->code;
			g_assert (offset >= 0 && offset < header->code_size);
			is_bb_start [offset] |= backwards ? 2 : 1;
			break;
		case MonoShortInlineBrTarget:
			offset = ((gint8 *)ip) [1];
			ip += 2;
			backwards = offset < 0;
			offset += ip - header->code;
			g_assert (offset >= 0 && offset < header->code_size);
			is_bb_start [offset] |= backwards ? 2 : 1;
			break;
		case MonoInlineVar:
			ip += 3;
			break;
		case MonoShortInlineVar:
		case MonoShortInlineI:
			ip += 2;
			break;
		case MonoInlineSwitch: {
			guint32 n;
			const unsigned char *next_ip;
			++ip;
			n = read32 (ip);
			ip += 4;
			next_ip = ip + 4 * n;
			for (i = 0; i < n; i++) {
				offset = read32 (ip);
				backwards = offset < 0;
				offset += next_ip - header->code;
				g_assert (offset >= 0 && offset < header->code_size);
				is_bb_start [offset] |= backwards ? 2 : 1;
				ip += 4;
			}
			break;
		}
		case MonoInlineR:
		case MonoInlineI8:
			ip += 9;
			break;
		default:
			g_assert_not_reached ();
		}
	}
	// g_printerr ("TRANSFORM(0x%016lx): end %s::%s\n", mono_thread_current (), method->klass->name, method->name);

	/* the rest needs to be locked so it is only done once */
	mono_os_mutex_lock(&calc_section);
	if (runtime_method->transformed) {
		mono_os_mutex_unlock(&calc_section);
		g_free (is_bb_start);
		mono_profiler_method_end_jit (method, NULL, MONO_PROFILE_OK);
		return NULL;
	}

	runtime_method->local_offsets = g_malloc (header->num_locals * sizeof(guint32));
	runtime_method->stack_size = (sizeof (stackval) + 2) * header->max_stack; /* + 1 for returns of called functions  + 1 for 0-ing in trace*/
	runtime_method->stack_size = (runtime_method->stack_size + 7) & ~7;
	offset = 0;
	for (i = 0; i < header->num_locals; ++i) {
		size = mono_type_size (header->locals [i], &align);
		offset += align - 1;
		offset &= ~(align - 1);
		runtime_method->local_offsets [i] = offset;
		offset += size;
	}
	offset = (offset + 7) & ~7;
	runtime_method->locals_size = offset;
	g_assert (runtime_method->locals_size < 65536);
	offset = 0;
	runtime_method->arg_offsets = g_malloc ((!!signature->hasthis + signature->param_count) * sizeof(guint32));

	if (signature->hasthis) {
		g_assert (!signature->pinvoke);
		size = mono_type_stack_size (&method->klass->byval_arg, &align);
		offset += align - 1;
		offset &= ~(align - 1);
		runtime_method->arg_offsets [0] = offset;
		offset += size;
	}

	for (i = 0; i < signature->param_count; ++i) {
		if (signature->pinvoke) {
			guint32 dummy;
			size = mono_type_native_stack_size (signature->params [i], &dummy);
			align = 8;
		}
		else
			size = mono_type_stack_size (signature->params [i], &align);
		offset += align - 1;
		offset &= ~(align - 1);
		runtime_method->arg_offsets [i + !!signature->hasthis] = offset;
		offset += size;
	}
	offset = (offset + 7) & ~7;
	runtime_method->args_size = offset;
	g_assert (runtime_method->args_size < 10000);

	generate (method, runtime_method, is_bb_start, generic_context);

	g_free (is_bb_start);

	mono_profiler_method_end_jit (method, NULL, MONO_PROFILE_OK);
	runtime_method->transformed = TRUE;
	mono_os_mutex_unlock(&calc_section);

	return NULL;
}

