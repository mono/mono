/*
 * jit-icalls.c: internal calls used by the JIT
 *
 * Author:
 *   Dietmar Maurer (dietmar@ximian.com)
 *   Paolo Molaro (lupus@ximian.com)
 *
 * (C) 2002 Ximian, Inc.
 * Copyright 2003-2011 Novell Inc (http://www.novell.com)
 * Copyright 2011 Xamarin Inc (http://www.xamarin.com)
 */
#include <config.h>
#include <math.h>
#include <limits.h>
#ifdef HAVE_ALLOCA_H
#include <alloca.h>
#endif

#include "jit-icalls.h"
#include <mono/utils/mono-error-internals.h>
void*
mono_ldftn (MonoMethod *method)
{
	gpointer addr;

	if (mono_llvm_only) {
		// FIXME: No error handling

		addr = mono_compile_method (method);
		g_assert (addr);

		if (mono_method_needs_static_rgctx_invoke (method, FALSE))
			/* The caller doesn't pass it */
			g_assert_not_reached ();

		addr = mini_add_method_trampoline (method, addr, mono_method_needs_static_rgctx_invoke (method, FALSE), FALSE);
		return addr;
	}

	addr = mono_create_jump_trampoline (mono_domain_get (), method, FALSE);

	return mono_create_ftnptr (mono_domain_get (), addr);
}

static void*
ldvirtfn_internal (MonoObject *obj, MonoMethod *method, gboolean gshared)
{
	MonoError error;
	MonoMethod *res;

	if (obj == NULL) {
		mono_set_pending_exception (mono_get_exception_null_reference ());
		return NULL;
	}

	res = mono_object_get_virtual_method (obj, method);

	if (gshared && method->is_inflated && mono_method_get_context (method)->method_inst) {
		MonoGenericContext context = { NULL, NULL };

		if (res->klass->generic_class)
			context.class_inst = res->klass->generic_class->context.class_inst;
		else if (res->klass->generic_container)
			context.class_inst = res->klass->generic_container->context.class_inst;
		context.method_inst = mono_method_get_context (method)->method_inst;

		res = mono_class_inflate_generic_method_checked (res, &context, &error);
		if (!mono_error_ok (&error)) {
			mono_error_set_pending_exception (&error);
			return NULL;
		}
	}

	/* An rgctx wrapper is added by the trampolines no need to do it here */

	return mono_ldftn (res);
}

void*
mono_ldvirtfn (MonoObject *obj, MonoMethod *method) 
{
	return ldvirtfn_internal (obj, method, FALSE);
}

void*
mono_ldvirtfn_gshared (MonoObject *obj, MonoMethod *method) 
{
	return ldvirtfn_internal (obj, method, TRUE);
}

void
mono_helper_stelem_ref_check (MonoArray *array, MonoObject *val)
{
	if (!array) {
		mono_set_pending_exception (mono_get_exception_null_reference ());
		return;
	}
	if (val && !mono_object_isinst (val, array->obj.vtable->klass->element_class)) {
		mono_set_pending_exception (mono_get_exception_array_type_mismatch ());
		return;
	}
}

#if !defined(MONO_ARCH_NO_EMULATE_LONG_MUL_OPTS) || defined(MONO_ARCH_EMULATE_LONG_MUL_OVF_OPTS)

gint64 
mono_llmult (gint64 a, gint64 b)
{
	return a * b;
}

guint64  
mono_llmult_ovf_un (guint64 a, guint64 b)
{
	guint32 al = a;
	guint32 ah = a >> 32;
	guint32 bl = b;
	guint32 bh = b >> 32; 
	guint64 res, t1;

	// fixme: this is incredible slow

	if (ah && bh)
		goto raise_exception;

	res = (guint64)al * (guint64)bl;

	t1 = (guint64)ah * (guint64)bl + (guint64)al * (guint64)bh;

	if (t1 > 0xffffffff)
		goto raise_exception;

	res += ((guint64)t1) << 32; 

	return res;

 raise_exception:
	mono_set_pending_exception (mono_get_exception_overflow ());
	return 0;
}

guint64  
mono_llmult_ovf (gint64 a, gint64 b) 
{
	guint32 al = a;
	gint32 ah = a >> 32;
	guint32 bl = b;
	gint32 bh = b >> 32; 
	/*
	Use Karatsuba algorithm where:
		a*b is: AhBh(R^2+R)+(Ah-Al)(Bl-Bh)R+AlBl(R+1)
		where Ah is the "high half" (most significant 32 bits) of a and
		where Al is the "low half" (least significant 32 bits) of a and
		where  Bh is the "high half" of b and Bl is the "low half" and
		where R is the Radix or "size of the half" (in our case 32 bits)

	Note, for the product of two 64 bit numbers to fit into a 64
	result, ah and/or bh must be 0.  This will save us from doing
	the AhBh term at all.

	Also note that we refactor so that we don't overflow 64 bits with 
	intermediate results. So we use [(Ah-Al)(Bl-Bh)+AlBl]R+AlBl
	*/

	gint64 res, t1;
	gint32 sign;

	/* need to work with absoulte values, so find out what the
	   resulting sign will be and convert any negative numbers
	   from two's complement
	*/
	sign = ah ^ bh;
	if (ah < 0) {
		if (((guint32)ah == 0x80000000) && (al == 0)) {
			/* This has no two's complement */
			if (b == 0)
				return 0;
			else if (b == 1)
				return a;
			else
				goto raise_exception;
		}

		/* flip the bits and add 1 */
		ah ^= ~0;
		if (al ==  0)
			ah += 1;
		else {
			al ^= ~0;
			al +=1;
		}
	}

	if (bh < 0) {
		if (((guint32)bh == 0x80000000) && (bl == 0)) {
			/* This has no two's complement */
			if (a == 0)
				return 0;
			else if (a == 1)
				return b;
			else
				goto raise_exception;
		}

		/* flip the bits and add 1 */
		bh ^= ~0;
		if (bl ==  0)
			bh += 1;
		else {
			bl ^= ~0;
			bl +=1;
		}
	}
		
	/* we overflow for sure if both upper halves are greater 
	   than zero because we would need to shift their 
	   product 64 bits to the left and that will not fit
	   in a 64 bit result */
	if (ah && bh)
		goto raise_exception;
	if ((gint64)((gint64)ah * (gint64)bl) > (gint64)0x80000000 || (gint64)((gint64)al * (gint64)bh) > (gint64)0x80000000)
		goto raise_exception;

	/* do the AlBl term first */
	t1 = (gint64)al * (gint64)bl;

	res = t1;

	/* now do the [(Ah-Al)(Bl-Bh)+AlBl]R term */
	t1 += (gint64)(ah - al) * (gint64)(bl - bh);
	/* check for overflow */
	t1 <<= 32;
	if (t1 > (0x7FFFFFFFFFFFFFFFLL - res))
		goto raise_exception;

	res += t1;

	if (res < 0)
		goto raise_exception;

	if (sign < 0)
		return -res;
	else
		return res;

 raise_exception:
	mono_set_pending_exception (mono_get_exception_overflow ());
	return 0;
}

gint64 
mono_lldiv (gint64 a, gint64 b)
{
#ifdef MONO_ARCH_NEED_DIV_CHECK
	if (!b) {
		mono_set_pending_exception (mono_get_exception_divide_by_zero ());
		return 0;
	}
	else if (b == -1 && a == (-9223372036854775807LL - 1LL)) {
		mono_set_pending_exception (mono_get_exception_arithmetic ());
		return 0;
	}
#endif
	return a / b;
}

gint64 
mono_llrem (gint64 a, gint64 b)
{
#ifdef MONO_ARCH_NEED_DIV_CHECK
	if (!b) {
		mono_set_pending_exception (mono_get_exception_divide_by_zero ());
		return 0;
	}
	else if (b == -1 && a == (-9223372036854775807LL - 1LL)) {
		mono_set_pending_exception (mono_get_exception_arithmetic ());
		return 0;
	}
#endif
	return a % b;
}

guint64 
mono_lldiv_un (guint64 a, guint64 b)
{
#ifdef MONO_ARCH_NEED_DIV_CHECK
	if (!b) {
		mono_set_pending_exception (mono_get_exception_divide_by_zero ());
		return 0;
	}
#endif
	return a / b;
}

guint64 
mono_llrem_un (guint64 a, guint64 b)
{
#ifdef MONO_ARCH_NEED_DIV_CHECK
	if (!b) {
		mono_set_pending_exception (mono_get_exception_divide_by_zero ());
		return 0;
	}
#endif
	return a % b;
}

#endif

#ifndef MONO_ARCH_NO_EMULATE_LONG_SHIFT_OPS

guint64 
mono_lshl (guint64 a, gint32 shamt)
{
	guint64 res;

	res = a << (shamt & 0x7f);

	/*printf ("TESTL %lld << %d = %lld\n", a, shamt, res);*/

	return res;
}

guint64 
mono_lshr_un (guint64 a, gint32 shamt)
{
	guint64 res;

	res = a >> (shamt & 0x7f);

	/*printf ("TESTR %lld >> %d = %lld\n", a, shamt, res);*/

	return res;
}

gint64 
mono_lshr (gint64 a, gint32 shamt)
{
	gint64 res;

	res = a >> (shamt & 0x7f);

	/*printf ("TESTR %lld >> %d = %lld\n", a, shamt, res);*/

	return res;
}

#endif

#if defined(MONO_ARCH_EMULATE_MUL_DIV) || defined(MONO_ARCH_EMULATE_DIV)

gint32
mono_idiv (gint32 a, gint32 b)
{
#ifdef MONO_ARCH_NEED_DIV_CHECK
	if (!b) {
		mono_set_pending_exception (mono_get_exception_divide_by_zero ());
		return 0;
	}
	else if (b == -1 && a == (0x80000000)) {
		mono_set_pending_exception (mono_get_exception_overflow ());
		return 0;
	}
#endif
	return a / b;
}

guint32
mono_idiv_un (guint32 a, guint32 b)
{
#ifdef MONO_ARCH_NEED_DIV_CHECK
	if (!b) {
		mono_set_pending_exception (mono_get_exception_divide_by_zero ());
		return 0;
	}
#endif
	return a / b;
}

gint32
mono_irem (gint32 a, gint32 b)
{
#ifdef MONO_ARCH_NEED_DIV_CHECK
	if (!b) {
		mono_set_pending_exception (mono_get_exception_divide_by_zero ());
		return 0;
	}
	else if (b == -1 && a == (0x80000000)) {
		mono_set_pending_exception (mono_get_exception_overflow ());
		return 0;
	}
#endif
	return a % b;
}

guint32
mono_irem_un (guint32 a, guint32 b)
{
#ifdef MONO_ARCH_NEED_DIV_CHECK
	if (!b) {
		mono_set_pending_exception (mono_get_exception_divide_by_zero ());
		return 0;
	}
#endif
	return a % b;
}

#endif

#if defined(MONO_ARCH_EMULATE_MUL_DIV) || defined(MONO_ARCH_EMULATE_MUL_OVF)

gint32
mono_imul (gint32 a, gint32 b)
{
	return a * b;
}

gint32
mono_imul_ovf (gint32 a, gint32 b)
{
	gint64 res;

	res = (gint64)a * (gint64)b;

	if ((res > 0x7fffffffL) || (res < -2147483648LL)) {
		mono_set_pending_exception (mono_get_exception_overflow ());
		return 0;
	}

	return res;
}

gint32
mono_imul_ovf_un (guint32 a, guint32 b)
{
	guint64 res;

	res = (guint64)a * (guint64)b;

	if (res >> 32) {
		mono_set_pending_exception (mono_get_exception_overflow ());
		return 0;
	}

	return res;
}
#endif

#if defined(MONO_ARCH_EMULATE_MUL_DIV) || defined(MONO_ARCH_SOFT_FLOAT_FALLBACK)
double
mono_fdiv (double a, double b)
{
	return a / b;
}
#endif

#ifdef MONO_ARCH_SOFT_FLOAT_FALLBACK

double
mono_fsub (double a, double b)
{
	return a - b;
}

double
mono_fadd (double a, double b)
{
	return a + b;
}

double
mono_fmul (double a, double b)
{
	return a * b;
}

double
mono_fneg (double a)
{
	return -a;
}

double
mono_fconv_r4 (double a)
{
	return (float)a;
}

double
mono_conv_to_r8 (int a)
{
	return (double)a;
}

double
mono_conv_to_r4 (int a)
{
	return (double)(float)a;
}

gint8
mono_fconv_i1 (double a)
{
	return (gint8)a;
}

gint16
mono_fconv_i2 (double a)
{
	return (gint16)a;
}

gint32
mono_fconv_i4 (double a)
{
	return (gint32)a;
}

guint8
mono_fconv_u1 (double a)
{
	return (guint8)a;
}

guint16
mono_fconv_u2 (double a)
{
	return (guint16)a;
}

gboolean
mono_fcmp_eq (double a, double b)
{
	return a == b;
}

gboolean
mono_fcmp_ge (double a, double b)
{
	return a >= b;
}

gboolean
mono_fcmp_gt (double a, double b)
{
	return a > b;
}

gboolean
mono_fcmp_le (double a, double b)
{
	return a <= b;
}

gboolean
mono_fcmp_lt (double a, double b)
{
	return a < b;
}

gboolean
mono_fcmp_ne_un (double a, double b)
{
	return isunordered (a, b) || a != b;
}

gboolean
mono_fcmp_ge_un (double a, double b)
{
	return isunordered (a, b) || a >= b;
}

gboolean
mono_fcmp_gt_un (double a, double b)
{
	return isunordered (a, b) || a > b;
}

gboolean
mono_fcmp_le_un (double a, double b)
{
	return isunordered (a, b) || a <= b;
}

gboolean
mono_fcmp_lt_un (double a, double b)
{
	return isunordered (a, b) || a < b;
}

gboolean
mono_fceq (double a, double b)
{
	return a == b;
}

gboolean
mono_fcgt (double a, double b)
{
	return a > b;
}

gboolean
mono_fcgt_un (double a, double b)
{
	return isunordered (a, b) || a > b;
}

gboolean
mono_fclt (double a, double b)
{
	return a < b;
}

gboolean
mono_fclt_un (double a, double b)
{
	return isunordered (a, b) || a < b;
}

gboolean
mono_isfinite (double a)
{
#ifdef HAVE_ISFINITE
	return isfinite (a);
#else
	g_assert_not_reached ();
	return TRUE;
#endif
}

double
mono_fload_r4 (float *ptr)
{
	return *ptr;
}

void
mono_fstore_r4 (double val, float *ptr)
{
	*ptr = (float)val;
}

/* returns the integer bitpattern that is passed in the regs or stack */
guint32
mono_fload_r4_arg (double val)
{
	float v = (float)val;
	return *(guint32*)&v;
}

#endif

MonoArray *
mono_array_new_va (MonoMethod *cm, ...)
{
	MonoDomain *domain = mono_domain_get ();
	va_list ap;
	uintptr_t *lengths;
	intptr_t *lower_bounds;
	int pcount;
	int rank;
	int i, d;

	pcount = mono_method_signature (cm)->param_count;
	rank = cm->klass->rank;

	va_start (ap, cm);
	
	lengths = alloca (sizeof (uintptr_t) * pcount);
	for (i = 0; i < pcount; ++i)
		lengths [i] = d = va_arg(ap, int);

	if (rank == pcount) {
		/* Only lengths provided. */
		if (cm->klass->byval_arg.type == MONO_TYPE_ARRAY) {
			lower_bounds = alloca (sizeof (intptr_t) * rank);
			memset (lower_bounds, 0, sizeof (intptr_t) * rank);
		} else {
			lower_bounds = NULL;
		}
	} else {
		g_assert (pcount == (rank * 2));
		/* lower bounds are first. */
		lower_bounds = (intptr_t*)lengths;
		lengths += rank;
	}
	va_end(ap);

	return mono_array_new_full (domain, cm->klass, lengths, lower_bounds);
}

/* Specialized version of mono_array_new_va () which avoids varargs */
MonoArray *
mono_array_new_1 (MonoMethod *cm, guint32 length)
{
	MonoDomain *domain = mono_domain_get ();
	uintptr_t lengths [1];
	intptr_t *lower_bounds;
	int pcount;
	int rank;

	pcount = mono_method_signature (cm)->param_count;
	rank = cm->klass->rank;

	lengths [0] = length;

	g_assert (rank == pcount);

	if (cm->klass->byval_arg.type == MONO_TYPE_ARRAY) {
		lower_bounds = alloca (sizeof (intptr_t) * rank);
		memset (lower_bounds, 0, sizeof (intptr_t) * rank);
	} else {
		lower_bounds = NULL;
	}

	return mono_array_new_full (domain, cm->klass, lengths, lower_bounds);
}

MonoArray *
mono_array_new_2 (MonoMethod *cm, guint32 length1, guint32 length2)
{
	MonoDomain *domain = mono_domain_get ();
	uintptr_t lengths [2];
	intptr_t *lower_bounds;
	int pcount;
	int rank;

	pcount = mono_method_signature (cm)->param_count;
	rank = cm->klass->rank;

	lengths [0] = length1;
	lengths [1] = length2;

	g_assert (rank == pcount);

	if (cm->klass->byval_arg.type == MONO_TYPE_ARRAY) {
		lower_bounds = alloca (sizeof (intptr_t) * rank);
		memset (lower_bounds, 0, sizeof (intptr_t) * rank);
	} else {
		lower_bounds = NULL;
	}

	return mono_array_new_full (domain, cm->klass, lengths, lower_bounds);
}

MonoArray *
mono_array_new_3 (MonoMethod *cm, guint32 length1, guint32 length2, guint32 length3)
{
	MonoDomain *domain = mono_domain_get ();
	uintptr_t lengths [3];
	intptr_t *lower_bounds;
	int pcount;
	int rank;

	pcount = mono_method_signature (cm)->param_count;
	rank = cm->klass->rank;

	lengths [0] = length1;
	lengths [1] = length2;
	lengths [2] = length3;

	g_assert (rank == pcount);

	if (cm->klass->byval_arg.type == MONO_TYPE_ARRAY) {
		lower_bounds = alloca (sizeof (intptr_t) * rank);
		memset (lower_bounds, 0, sizeof (intptr_t) * rank);
	} else {
		lower_bounds = NULL;
	}

	return mono_array_new_full (domain, cm->klass, lengths, lower_bounds);
}

MonoArray *
mono_array_new_4 (MonoMethod *cm, guint32 length1, guint32 length2, guint32 length3, guint32 length4)
{
	MonoDomain *domain = mono_domain_get ();
	uintptr_t lengths [4];
	intptr_t *lower_bounds;
	int pcount;
	int rank;

	pcount = mono_method_signature (cm)->param_count;
	rank = cm->klass->rank;

	lengths [0] = length1;
	lengths [1] = length2;
	lengths [2] = length3;
	lengths [3] = length4;

	g_assert (rank == pcount);

	if (cm->klass->byval_arg.type == MONO_TYPE_ARRAY) {
		lower_bounds = alloca (sizeof (intptr_t) * rank);
		memset (lower_bounds, 0, sizeof (intptr_t) * rank);
	} else {
		lower_bounds = NULL;
	}

	return mono_array_new_full (domain, cm->klass, lengths, lower_bounds);
}

gpointer
mono_class_static_field_address (MonoDomain *domain, MonoClassField *field)
{
	MonoVTable *vtable;
	gpointer addr;
	
	//printf ("SFLDA0 %s.%s::%s %d\n", field->parent->name_space, field->parent->name, field->name, field->offset, field->parent->inited);

	mono_class_init (field->parent);

	vtable = mono_class_vtable_full (domain, field->parent, TRUE);
	if (!vtable->initialized)
		mono_runtime_class_init (vtable);

	//printf ("SFLDA1 %p\n", (char*)vtable->data + field->offset);

	if (domain->special_static_fields && (addr = g_hash_table_lookup (domain->special_static_fields, field)))
		addr = mono_get_special_static_data (GPOINTER_TO_UINT (addr));
	else
		addr = (char*)mono_vtable_get_static_field_data (vtable) + field->offset;
	
	return addr;
}

gpointer
mono_ldtoken_wrapper (MonoImage *image, int token, MonoGenericContext *context)
{
	MonoError error;
	MonoClass *handle_class;
	gpointer res;

	res = mono_ldtoken_checked (image, token, &handle_class, context, &error);
	if (!mono_error_ok (&error)) {
		mono_error_set_pending_exception (&error);
		return NULL;
	}
	mono_class_init (handle_class);

	return res;
}

gpointer
mono_ldtoken_wrapper_generic_shared (MonoImage *image, int token, MonoMethod *method)
{
	MonoMethodSignature *sig = mono_method_signature (method);
	MonoGenericContext *generic_context;

	if (sig->is_inflated) {
		generic_context = mono_method_get_context (method);
	} else {
		MonoGenericContainer *generic_container = mono_method_get_generic_container (method);
		g_assert (generic_container);
		generic_context = &generic_container->context;
	}

	return mono_ldtoken_wrapper (image, token, generic_context);
}

guint64
mono_fconv_u8 (double v)
{
	return (guint64)v;
}

guint64
mono_rconv_u8 (float v)
{
	return (guint64)v;
}

#ifdef MONO_ARCH_EMULATE_FCONV_TO_I8
gint64
mono_fconv_i8 (double v)
{
	return (gint64)v;
}
#endif

guint32
mono_fconv_u4 (double v)
{
	/* MS.NET behaves like this for some reason */
#ifdef HAVE_ISINF
	if (isinf (v) || isnan (v))
		return 0;
#endif

	return (guint32)v;
}

#ifndef HAVE_TRUNC
/* Solaris doesn't have trunc */
#ifdef HAVE_AINTL
extern long double aintl (long double);
#define trunc aintl
#else
/* FIXME: This means we will never throw overflow exceptions */
#define trunc(v) res
#endif
#endif /* HAVE_TRUNC */

gint64
mono_fconv_ovf_i8 (double v)
{
	gint64 res;

	res = (gint64)v;

	if (isnan(v) || trunc (v) != res) {
		mono_set_pending_exception (mono_get_exception_overflow ());
		return 0;
	}
	return res;
}

guint64
mono_fconv_ovf_u8 (double v)
{
	guint64 res;

/*
 * The soft-float implementation of some ARM devices have a buggy guin64 to double
 * conversion that it looses precision even when the integer if fully representable
 * as a double.
 * 
 * This was found with 4294967295ull, converting to double and back looses one bit of precision.
 * 
 * To work around this issue we test for value boundaries instead. 
 */
#if defined(__arm__) && defined(MONO_ARCH_SOFT_FLOAT_FALLBACK)
	if (isnan (v) || !(v >= -0.5 && v <= ULLONG_MAX+0.5)) {
		mono_set_pending_exception (mono_get_exception_overflow ());
		return 0;
	}
	res = (guint64)v;
#else
	res = (guint64)v;
	if (isnan(v) || trunc (v) != res) {
		mono_set_pending_exception (mono_get_exception_overflow ());
		return 0;
	}
#endif
	return res;
}

#ifdef MONO_ARCH_EMULATE_FCONV_TO_I8
gint64
mono_rconv_i8 (float v)
{
	return (gint64)v;
}
#endif

gint64
mono_rconv_ovf_i8 (float v)
{
	gint64 res;

	res = (gint64)v;

	if (isnan(v) || trunc (v) != res) {
		mono_set_pending_exception (mono_get_exception_overflow ());
		return 0;
	}
	return res;
}

guint64
mono_rconv_ovf_u8 (float v)
{
	guint64 res;

	res = (guint64)v;
	if (isnan(v) || trunc (v) != res) {
		mono_set_pending_exception (mono_get_exception_overflow ());
		return 0;
	}
	return res;
}

#ifdef MONO_ARCH_EMULATE_LCONV_TO_R8
double
mono_lconv_to_r8 (gint64 a)
{
	return (double)a;
}
#endif

#ifdef MONO_ARCH_EMULATE_LCONV_TO_R4
float
mono_lconv_to_r4 (gint64 a)
{
	return (float)a;
}
#endif

#ifdef MONO_ARCH_EMULATE_CONV_R8_UN
double
mono_conv_to_r8_un (guint32 a)
{
	return (double)a;
}
#endif

#ifdef MONO_ARCH_EMULATE_LCONV_TO_R8_UN
double
mono_lconv_to_r8_un (guint64 a)
{
	return (double)a;
}
#endif

#if defined(__native_client_codegen__) || defined(__native_client__)
/* When we cross-compile to Native Client we can't directly embed calls */
/* to the math library on the host. This will use the fmod on the target*/
double
mono_fmod(double a, double b)
{
	return fmod(a, b);
}
#endif

gpointer
mono_helper_compile_generic_method (MonoObject *obj, MonoMethod *method, gpointer *this_arg)
{
	MonoMethod *vmethod;
	gpointer addr;
	MonoGenericContext *context = mono_method_get_context (method);

	mono_jit_stats.generic_virtual_invocations++;

	if (obj == NULL) {
		mono_set_pending_exception (mono_get_exception_null_reference ());
		return NULL;
	}
	vmethod = mono_object_get_virtual_method (obj, method);
	g_assert (!vmethod->klass->generic_container);
	g_assert (!vmethod->klass->generic_class || !vmethod->klass->generic_class->context.class_inst->is_open);
	g_assert (!context->method_inst || !context->method_inst->is_open);

	addr = mono_compile_method (vmethod);

	addr = mini_add_method_trampoline (vmethod, addr, mono_method_needs_static_rgctx_invoke (vmethod, FALSE), FALSE);

	/* Since this is a virtual call, have to unbox vtypes */
	if (obj->vtable->klass->valuetype)
		*this_arg = mono_object_unbox (obj);
	else
		*this_arg = obj;

	return addr;
}

MonoString*
mono_helper_ldstr (MonoImage *image, guint32 idx)
{
	return mono_ldstr (mono_domain_get (), image, idx);
}

MonoString*
mono_helper_ldstr_mscorlib (guint32 idx)
{
	return mono_ldstr (mono_domain_get (), mono_defaults.corlib, idx);
}

MonoObject*
mono_helper_newobj_mscorlib (guint32 idx)
{
	MonoError error;
	MonoClass *klass = mono_class_get_checked (mono_defaults.corlib, MONO_TOKEN_TYPE_DEF | idx, &error);

	if (!mono_error_ok (&error)) {
		mono_error_set_pending_exception (&error);
		return NULL;
	}

	return mono_object_new (mono_domain_get (), klass);
}

/*
 * On some architectures, gdb doesn't like encountering the cpu breakpoint instructions
 * in generated code. So instead we emit a call to this function and place a gdb
 * breakpoint here.
 */
void
mono_break (void)
{
}

MonoException *
mono_create_corlib_exception_0 (guint32 token)
{
	return mono_exception_from_token (mono_defaults.corlib, token);
}

MonoException *
mono_create_corlib_exception_1 (guint32 token, MonoString *arg)
{
	return mono_exception_from_token_two_strings (mono_defaults.corlib, token, arg, NULL);
}

MonoException *
mono_create_corlib_exception_2 (guint32 token, MonoString *arg1, MonoString *arg2)
{
	return mono_exception_from_token_two_strings (mono_defaults.corlib, token, arg1, arg2);
}

MonoObject*
mono_object_castclass_unbox (MonoObject *obj, MonoClass *klass)
{
	MonoJitTlsData *jit_tls = NULL;
	MonoClass *oklass;

	if (mini_get_debug_options ()->better_cast_details) {
		jit_tls = mono_native_tls_get_value (mono_jit_tls_id);
		jit_tls->class_cast_from = NULL;
	}

	if (!obj)
		return NULL;

	oklass = obj->vtable->klass;
	if ((klass->enumtype && oklass == klass->element_class) || (oklass->enumtype && klass == oklass->element_class))
		return obj;
	if (mono_object_isinst (obj, klass))
		return obj;

	if (mini_get_debug_options ()->better_cast_details) {
		jit_tls->class_cast_from = oklass;
		jit_tls->class_cast_to = klass;
	}

	mono_set_pending_exception (mono_exception_from_name (mono_defaults.corlib,
					"System", "InvalidCastException"));

	return NULL;
}

MonoObject*
mono_object_castclass_with_cache (MonoObject *obj, MonoClass *klass, gpointer *cache)
{
	MonoJitTlsData *jit_tls = NULL;
	gpointer cached_vtable, obj_vtable;

	if (mini_get_debug_options ()->better_cast_details) {
		jit_tls = mono_native_tls_get_value (mono_jit_tls_id);
		jit_tls->class_cast_from = NULL;
	}

	if (!obj)
		return NULL;

	cached_vtable = *cache;
	obj_vtable = obj->vtable;

	if (cached_vtable == obj_vtable)
		return obj;

	if (mono_object_isinst (obj, klass)) {
		*cache = obj_vtable;
		return obj;
	}

	if (mini_get_debug_options ()->better_cast_details) {
		jit_tls->class_cast_from = obj->vtable->klass;
		jit_tls->class_cast_to = klass;
	}

	mono_set_pending_exception (mono_exception_from_name (mono_defaults.corlib,
					"System", "InvalidCastException"));

	return NULL;
}

MonoObject*
mono_object_isinst_with_cache (MonoObject *obj, MonoClass *klass, gpointer *cache)
{
	size_t cached_vtable, obj_vtable;

	if (!obj)
		return NULL;

	cached_vtable = (size_t)*cache;
	obj_vtable = (size_t)obj->vtable;

	if ((cached_vtable & ~0x1) == obj_vtable) {
		return (cached_vtable & 0x1) ? NULL : obj;
	}

	if (mono_object_isinst (obj, klass)) {
		*cache = (gpointer)obj_vtable;
		return obj;
	} else {
		/*negative cache*/
		*cache = (gpointer)(obj_vtable | 0x1);
		return NULL;
	}
}

gpointer
mono_get_native_calli_wrapper (MonoImage *image, MonoMethodSignature *sig, gpointer func)
{
	MonoMarshalSpec **mspecs;
	MonoMethodPInvoke piinfo;
	MonoMethod *m;

	mspecs = g_new0 (MonoMarshalSpec*, sig->param_count + 1);
	memset (&piinfo, 0, sizeof (piinfo));

	m = mono_marshal_get_native_func_wrapper (image, sig, &piinfo, mspecs, func);

	return mono_compile_method (m);
}

static MonoMethod*
constrained_gsharedvt_call_setup (gpointer mp, MonoMethod *cmethod, MonoClass *klass, gpointer *this_arg)
{
	MonoMethod *m;
	int vt_slot;

	if (klass->flags & TYPE_ATTRIBUTE_INTERFACE) {
		mono_set_pending_exception (mono_get_exception_execution_engine ("Not yet supported."));
		return NULL;
	}

	if (mono_method_signature (cmethod)->pinvoke) {
		/* Object.GetType () */
		m = mono_marshal_get_native_wrapper (cmethod, TRUE, FALSE);
	} else {
		/* Lookup the virtual method */
		mono_class_setup_vtable (klass);
		g_assert (klass->vtable);
		vt_slot = mono_method_get_vtable_slot (cmethod);
		if (cmethod->klass->flags & TYPE_ATTRIBUTE_INTERFACE) {
			int iface_offset;

			iface_offset = mono_class_interface_offset (klass, cmethod->klass);
			g_assert (iface_offset != -1);
			vt_slot += iface_offset;
		}
		m = klass->vtable [vt_slot];
		if (cmethod->is_inflated)
			m = mono_class_inflate_generic_method (m, mono_method_get_context (cmethod));
	}
	if (klass->valuetype && (m->klass == mono_defaults.object_class || m->klass == mono_defaults.enum_class->parent || m->klass == mono_defaults.enum_class))
		/*
		 * Calling a non-vtype method with a vtype receiver, has to box.
		 */
		*this_arg = mono_value_box (mono_domain_get (), klass, mp);
	else if (klass->valuetype)
		/*
		 * Calling a vtype method with a vtype receiver
		 */
		*this_arg = mp;
	else
		/*
		 * Calling a non-vtype method
		 */
		*this_arg = *(gpointer*)mp;
	return m;
}

/*
 * mono_gsharedvt_constrained_call:
 *
 *   Make a call to CMETHOD using the receiver MP, which is assumed to be of type KLASS. ARGS contains
 * the arguments to the method in the format used by mono_runtime_invoke ().
 */
MonoObject*
mono_gsharedvt_constrained_call (gpointer mp, MonoMethod *cmethod, MonoClass *klass, gboolean deref_arg, gpointer *args)
{
	MonoMethod *m;
	gpointer this_arg;
	gpointer new_args [16];

	m = constrained_gsharedvt_call_setup (mp, cmethod, klass, &this_arg);
	if (!m)
		return NULL;
	if (args && deref_arg) {
		new_args [0] = *(gpointer*)args [0];
		args = new_args;
	}
	if (m->wrapper_type == MONO_WRAPPER_MANAGED_TO_NATIVE) {
		/* Object.GetType () */
		args = new_args;
		args [0] = this_arg;
		this_arg = NULL;
	}
	return mono_runtime_invoke (m, this_arg, args, NULL);
}

void
mono_gsharedvt_value_copy (gpointer dest, gpointer src, MonoClass *klass)
{
	if (klass->valuetype)
		mono_value_copy (dest, src, klass);
	else
        mono_gc_wbarrier_generic_store (dest, *(MonoObject**)src);
}

void
mono_generic_class_init (MonoVTable *vtable)
{
	mono_runtime_class_init (vtable);
}

gpointer
mono_fill_class_rgctx (MonoVTable *vtable, int index)
{
	return mono_class_fill_runtime_generic_context (vtable, index);
}

gpointer
mono_fill_method_rgctx (MonoMethodRuntimeGenericContext *mrgctx, int index)
{
	return mono_method_fill_runtime_generic_context (mrgctx, index);
}

/*
 * mono_resolve_iface_call:
 *
 *   Return the executable code for the iface method IMT_METHOD called on THIS.
 */
gpointer
mono_resolve_iface_call (MonoObject *this, int imt_slot, MonoMethod *imt_method, gpointer *out_rgctx_arg)
{
	MonoVTable *vt;
	gpointer *imt, *vtable_slot;
	MonoMethod *impl_method, *generic_virtual = NULL, *variant_iface = NULL;
	gpointer addr, aot_addr;
	gboolean need_rgctx_tramp = FALSE, need_unbox_tramp = FALSE;

	// FIXME: Optimize this

	if (!this)
		/* The caller will handle it */
		return NULL;

	vt = this->vtable;
	imt = (gpointer*)vt - MONO_IMT_SIZE;

	vtable_slot = mini_resolve_imt_method (vt, imt + imt_slot, imt_method, &impl_method, &aot_addr, &need_rgctx_tramp, &variant_iface);

	// FIXME: This can throw exceptions
	addr = mono_compile_method (impl_method);
	g_assert (addr);

	if (imt_method->is_inflated && ((MonoMethodInflated*)imt_method)->context.method_inst) {
		generic_virtual = imt_method;
		need_rgctx_tramp = TRUE;
	}

	if (generic_virtual || variant_iface) {
		if (vt->klass->valuetype) /*FIXME is this required variant iface?*/
			need_unbox_tramp = TRUE;
	} else {
		if (impl_method->klass->valuetype)
			need_unbox_tramp = TRUE;
	}

	addr = mini_add_method_trampoline (impl_method, addr, need_rgctx_tramp, need_unbox_tramp);

	if (generic_virtual || variant_iface) {
		MonoMethod *target = generic_virtual ? generic_virtual : variant_iface;

		mono_method_add_generic_virtual_invocation (mono_domain_get (),
													vt, imt + imt_slot,
													target, addr);
	}

	*vtable_slot = addr;

	if (need_rgctx_tramp && out_rgctx_arg) {
		MonoMethod *m = impl_method;

		if (m->wrapper_type == MONO_WRAPPER_MANAGED_TO_MANAGED && m->klass->rank && strstr (m->name, "System.Collections.Generic"))
			m = mono_aot_get_array_helper_from_wrapper (m);
		*out_rgctx_arg = mini_method_get_rgctx (m);
	}

	return addr;
}

static gboolean
is_generic_method_definition (MonoMethod *m)
{
	MonoGenericContext *context;
	if (m->is_generic)
		return TRUE;
	if (!m->is_inflated)
		return FALSE;

	context = mono_method_get_context (m);
	if (!context->method_inst)
		return FALSE;
	if (context->method_inst == mono_method_get_generic_container (((MonoMethodInflated*)m)->declaring)->context.method_inst)
		return TRUE;
	return FALSE;
}

/*
 * mono_resolve_vcall:
 *
 *   Return the executable code for calling this->vtable [slot].
 */
gpointer
mono_resolve_vcall (MonoObject *this, int slot, MonoMethod *imt_method)
{
	MonoVTable *vt;
	MonoMethod *m, *generic_virtual = NULL;
	gpointer *vtable_slot;
	gpointer addr;
	gboolean need_rgctx_tramp = FALSE, need_unbox_tramp = FALSE;

	// FIXME: Optimize this

	if (!this)
		/* The caller will handle it */
		return NULL;

	vt = this->vtable;

	vtable_slot = &(vt->vtable [slot]);

	/* Same as in common_call_trampoline () */

	/* Avoid loading metadata or creating a generic vtable if possible */
	addr = mono_aot_get_method_from_vt_slot (mono_domain_get (), vt, slot);
	if (addr && !vt->klass->valuetype) {
		if (mono_domain_owns_vtable_slot (mono_domain_get (), vtable_slot))
			*vtable_slot = addr;

		return mono_create_ftnptr (mono_domain_get (), addr);
	}

	m = mono_class_get_vtable_entry (vt->klass, slot);

	if (is_generic_method_definition (m)) {
		MonoError error;
		MonoGenericContext context = { NULL, NULL };
		MonoMethod *declaring;

		if (m->is_inflated)
			declaring = mono_method_get_declaring_generic_method (m);
		else
			declaring = m;

		if (m->klass->generic_class)
			context.class_inst = m->klass->generic_class->context.class_inst;
		else
			g_assert (!m->klass->generic_container);

		generic_virtual = imt_method;
		g_assert (generic_virtual);
		g_assert (generic_virtual->is_inflated);
		context.method_inst = ((MonoMethodInflated*)generic_virtual)->context.method_inst;

		m = mono_class_inflate_generic_method_checked (declaring, &context, &error);
		g_assert (mono_error_ok (&error)); /* FIXME don't swallow the error */
		/* FIXME: only do this if the method is sharable */
		need_rgctx_tramp = TRUE;
	}

	if (generic_virtual) {
		if (vt->klass->valuetype) /*FIXME is this required variant iface?*/
			need_unbox_tramp = TRUE;
	} else {
		if (m->klass->valuetype)
			need_unbox_tramp = TRUE;
	}

	// FIXME: This can throw exceptions
	addr = mono_compile_method (m);
	g_assert (addr);

	addr = mini_add_method_trampoline (m, addr, need_rgctx_tramp, need_unbox_tramp);

	*vtable_slot = addr;

	return addr;
}

/*
 * mono_init_delegate:
 *
 *   Initialize a MonoDelegate object.
 * Similar to mono_delegate_ctor ().
 */
void
mono_init_delegate (MonoDelegate *del, MonoObject *target, MonoMethod *method)
{
	MONO_OBJECT_SETREF (del, target, target);
	del->method = method;
	del->method_ptr = mono_compile_method (method);
	if (mono_method_needs_static_rgctx_invoke (method, FALSE))
		del->rgctx = mini_method_get_rgctx (method);
}

void
mono_init_delegate_virtual (MonoDelegate *del, MonoObject *target, MonoMethod *method)
{
	g_assert (target);

	method = mono_object_get_virtual_method (target, method);

	MONO_OBJECT_SETREF (del, target, target);
	del->method = method;
	del->method_ptr = mono_compile_method (method);
	if (mono_method_needs_static_rgctx_invoke (method, FALSE))
		del->rgctx = mini_method_get_rgctx (method);
}
