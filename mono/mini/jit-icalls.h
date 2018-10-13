/**
 * \file
 */

#ifndef __MONO_JIT_ICALLS_H__
#define __MONO_JIT_ICALLS_H__

#include <math.h>
#include "mini.h"
#include <mono/metadata/icalls.h>

G_EXTERN_C void* mono_ldftn (MonoMethod *method);

G_EXTERN_C void* mono_ldvirtfn (MonoObject *obj, MonoMethod *method);

G_EXTERN_C void* mono_ldvirtfn_gshared (MonoObject *obj, MonoMethod *method);

G_EXTERN_C void mono_helper_stelem_ref_check (MonoArray *array, MonoObject *val);

G_EXTERN_C gint64 mono_llmult (gint64 a, gint64 b);

G_EXTERN_C guint64 mono_llmult_ovf_un (guint64 a, guint64 b);

G_EXTERN_C guint64 mono_llmult_ovf (gint64 a, gint64 b);

G_EXTERN_C gint32 mono_idiv (gint32 a, gint32 b);

G_EXTERN_C guint32 mono_idiv_un (guint32 a, guint32 b);

G_EXTERN_C gint32 mono_irem (gint32 a, gint32 b);

G_EXTERN_C guint32 mono_irem_un (guint32 a, guint32 b);

G_EXTERN_C gint32 mono_imul (gint32 a, gint32 b);

G_EXTERN_C gint32 mono_imul_ovf (gint32 a, gint32 b);

G_EXTERN_C gint32 mono_imul_ovf_un (guint32 a, guint32 b);

G_EXTERN_C double mono_fdiv (double a, double b);

G_EXTERN_C gint64 mono_lldiv (gint64 a, gint64 b);

G_EXTERN_C gint64 mono_llrem (gint64 a, gint64 b);

G_EXTERN_C guint64 mono_lldiv_un (guint64 a, guint64 b);

G_EXTERN_C guint64 mono_llrem_un (guint64 a, guint64 b);

G_EXTERN_C guint64 mono_lshl (guint64 a, gint32 shamt);

G_EXTERN_C guint64 mono_lshr_un (guint64 a, gint32 shamt);

G_EXTERN_C gint64 mono_lshr (gint64 a, gint32 shamt);

G_EXTERN_C MonoArray *mono_array_new_va (MonoMethod *cm, ...);

G_EXTERN_C MonoArray *mono_array_new_1 (MonoMethod *cm, guint32 length);

G_EXTERN_C MonoArray *mono_array_new_2 (MonoMethod *cm, guint32 length1, guint32 length2);

G_EXTERN_C MonoArray *mono_array_new_3 (MonoMethod *cm, guint32 length1, guint32 length2, guint32 length3);

G_EXTERN_C MonoArray *mono_array_new_4 (MonoMethod *cm, guint32 length1, guint32 length2, guint32 length3, guint32 length4);

G_EXTERN_C gpointer mono_class_static_field_address (MonoDomain *domain, MonoClassField *field);

G_EXTERN_C gpointer mono_ldtoken_wrapper (MonoImage *image, int token, MonoGenericContext *context);

G_EXTERN_C gpointer mono_ldtoken_wrapper_generic_shared (MonoImage *image, int token, MonoMethod *method);

G_EXTERN_C guint64 mono_fconv_u8 (double v);

G_EXTERN_C guint64 mono_rconv_u8 (float v);

G_EXTERN_C gint64 mono_fconv_i8 (double v);

G_EXTERN_C guint32 mono_fconv_u4 (double v);

G_EXTERN_C gint64 mono_fconv_ovf_i8 (double v);

G_EXTERN_C guint64 mono_fconv_ovf_u8 (double v);

G_EXTERN_C gint64 mono_rconv_i8 (float v);

G_EXTERN_C gint64 mono_rconv_ovf_i8 (float v);

G_EXTERN_C guint64 mono_rconv_ovf_u8 (float v);

G_EXTERN_C double mono_lconv_to_r8 (gint64 a);

G_EXTERN_C double mono_conv_to_r8 (gint32 a);

G_EXTERN_C double mono_conv_to_r4 (gint32 a);

G_EXTERN_C float mono_lconv_to_r4 (gint64 a);

G_EXTERN_C double mono_conv_to_r8_un (guint32 a);

G_EXTERN_C double mono_lconv_to_r8_un (guint64 a);

G_EXTERN_C gpointer mono_helper_compile_generic_method (MonoObject *obj, MonoMethod *method, gpointer *this_arg);

ICALL_EXPORT
MonoString*
ves_icall_mono_ldstr (MonoDomain *domain, MonoImage *image, guint32 idx);

G_EXTERN_C MonoString *mono_helper_ldstr (MonoImage *image, guint32 idx);

G_EXTERN_C MonoString *mono_helper_ldstr_mscorlib (guint32 idx);

G_EXTERN_C MonoObject *mono_helper_newobj_mscorlib (guint32 idx);

G_EXTERN_C double mono_fsub (double a, double b);

G_EXTERN_C double mono_fadd (double a, double b);

G_EXTERN_C double mono_fmul (double a, double b);

G_EXTERN_C double mono_fneg (double a);

G_EXTERN_C double mono_fconv_r4 (double a);

G_EXTERN_C gint8 mono_fconv_i1 (double a);

G_EXTERN_C gint16 mono_fconv_i2 (double a);

G_EXTERN_C gint32 mono_fconv_i4 (double a);

G_EXTERN_C guint8 mono_fconv_u1 (double a);

G_EXTERN_C guint16 mono_fconv_u2 (double a);

G_EXTERN_C gboolean mono_fcmp_eq (double a, double b);

G_EXTERN_C gboolean mono_fcmp_ge (double a, double b);

G_EXTERN_C gboolean mono_fcmp_gt (double a, double b);

G_EXTERN_C gboolean mono_fcmp_le (double a, double b);

G_EXTERN_C gboolean mono_fcmp_lt (double a, double b);

G_EXTERN_C gboolean mono_fcmp_ne_un (double a, double b);

G_EXTERN_C gboolean mono_fcmp_ge_un (double a, double b);

G_EXTERN_C gboolean mono_fcmp_gt_un (double a, double b);

G_EXTERN_C gboolean mono_fcmp_le_un (double a, double b);

G_EXTERN_C gboolean mono_fcmp_lt_un (double a, double b);

G_EXTERN_C gboolean mono_fceq (double a, double b);

G_EXTERN_C gboolean mono_fcgt (double a, double b);

G_EXTERN_C gboolean mono_fcgt_un (double a, double b);

G_EXTERN_C gboolean mono_fclt (double a, double b);

G_EXTERN_C gboolean mono_fclt_un (double a, double b);

G_EXTERN_C double   mono_fload_r4 (float *ptr);

G_EXTERN_C void     mono_fstore_r4 (double val, float *ptr);

G_EXTERN_C guint32  mono_fload_r4_arg (double val);

G_EXTERN_C void     mono_break (void);

G_EXTERN_C MonoException *mono_create_corlib_exception_0 (guint32 token);

G_EXTERN_C MonoException *mono_create_corlib_exception_1 (guint32 token, MonoString *arg);

G_EXTERN_C MonoException *mono_create_corlib_exception_2 (guint32 token, MonoString *arg1, MonoString *arg2);

G_EXTERN_C MonoObject* mono_object_castclass_unbox (MonoObject *obj, MonoClass *klass);

G_EXTERN_C gpointer mono_get_native_calli_wrapper (MonoImage *image, MonoMethodSignature *sig, gpointer func);

G_EXTERN_C MonoObject* mono_object_isinst_with_cache (MonoObject *obj, MonoClass *klass, gpointer *cache);

G_EXTERN_C MonoObject* mono_object_castclass_with_cache (MonoObject *obj, MonoClass *klass, gpointer *cache);

ICALL_EXPORT
void
ves_icall_runtime_class_init (MonoVTable *vtable);

G_EXTERN_C void
mono_generic_class_init (MonoVTable *vtable);

ICALL_EXPORT
void
ves_icall_mono_delegate_ctor (MonoObject *this_obj, MonoObject *target, gpointer addr);

ICALL_EXPORT
void
ves_icall_mono_delegate_ctor_interp (MonoObject *this_obj, MonoObject *target, gpointer addr);

G_EXTERN_C MonoObject* mono_gsharedvt_constrained_call (gpointer mp, MonoMethod *cmethod, MonoClass *klass, gboolean deref_arg, gpointer *args);

G_EXTERN_C void mono_gsharedvt_value_copy (gpointer dest, gpointer src, MonoClass *klass);

G_EXTERN_C gpointer mono_fill_class_rgctx (MonoVTable *vtable, int index);

G_EXTERN_C gpointer mono_fill_method_rgctx (MonoMethodRuntimeGenericContext *mrgctx, int index);

G_EXTERN_C gpointer mono_resolve_iface_call_gsharedvt (MonoObject *this_obj, int imt_slot, MonoMethod *imt_method, gpointer *out_arg);

G_EXTERN_C gpointer mono_resolve_vcall_gsharedvt (MonoObject *this_obj, int imt_slot, MonoMethod *imt_method, gpointer *out_arg);

G_EXTERN_C MonoFtnDesc* mono_resolve_generic_virtual_call (MonoVTable *vt, int slot, MonoMethod *imt_method);

G_EXTERN_C MonoFtnDesc* mono_resolve_generic_virtual_iface_call (MonoVTable *vt, int imt_slot, MonoMethod *imt_method);

G_EXTERN_C gpointer mono_init_vtable_slot (MonoVTable *vtable, int slot);

G_EXTERN_C void mono_llvmonly_init_delegate (MonoDelegate *del);

G_EXTERN_C void mono_llvmonly_init_delegate_virtual (MonoDelegate *del, MonoObject *target, MonoMethod *method);

G_EXTERN_C MonoObject* mono_get_assembly_object (MonoImage *image);

G_EXTERN_C MonoObject* mono_get_method_object (MonoMethod *method);

G_EXTERN_C double mono_ckfinite (double d);

G_EXTERN_C void mono_throw_method_access (MonoMethod *caller, MonoMethod *callee);

G_EXTERN_C void mono_dummy_jit_icall (void);

#endif /* __MONO_JIT_ICALLS_H__ */
