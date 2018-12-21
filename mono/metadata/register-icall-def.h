/**
 * \file
 * Copyright 2018 Microsoft
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#ifndef __MONO_METADATA_REGISTER_ICALL_DEF_H__
#define __MONO_METADATA_REGISTER_ICALL_DEF_H__

// Changes to this file affect AOT file format.
#define MONO_REGISTER_JIT_ICALLS \
MONO_REGISTER_JIT_ICALL (mono_jit_icall_zero_is_reserved) \
	\
MONO_REGISTER_JIT_ICALL (__emul_fadd) \
MONO_REGISTER_JIT_ICALL (__emul_fcmp_ceq) \
MONO_REGISTER_JIT_ICALL (__emul_fcmp_cgt) \
MONO_REGISTER_JIT_ICALL (__emul_fcmp_cgt_un) \
MONO_REGISTER_JIT_ICALL (__emul_fcmp_clt) \
MONO_REGISTER_JIT_ICALL (__emul_fcmp_clt_un) \
MONO_REGISTER_JIT_ICALL (__emul_fcmp_eq) \
MONO_REGISTER_JIT_ICALL (__emul_fcmp_ge) \
MONO_REGISTER_JIT_ICALL (__emul_fcmp_ge_un) \
MONO_REGISTER_JIT_ICALL (__emul_fcmp_gt) \
MONO_REGISTER_JIT_ICALL (__emul_fcmp_gt_un) \
MONO_REGISTER_JIT_ICALL (__emul_fcmp_le) \
MONO_REGISTER_JIT_ICALL (__emul_fcmp_le_un) \
MONO_REGISTER_JIT_ICALL (__emul_fcmp_lt) \
MONO_REGISTER_JIT_ICALL (__emul_fcmp_lt_un) \
MONO_REGISTER_JIT_ICALL (__emul_fcmp_ne_un) \
MONO_REGISTER_JIT_ICALL (__emul_fconv_to_i) \
MONO_REGISTER_JIT_ICALL (__emul_fconv_to_i1) \
MONO_REGISTER_JIT_ICALL (__emul_fconv_to_i2) \
MONO_REGISTER_JIT_ICALL (__emul_fconv_to_i4) \
MONO_REGISTER_JIT_ICALL (__emul_fconv_to_i8) \
MONO_REGISTER_JIT_ICALL (__emul_fconv_to_ovf_i8) \
MONO_REGISTER_JIT_ICALL (__emul_fconv_to_ovf_u8) \
MONO_REGISTER_JIT_ICALL (__emul_fconv_to_r4) \
MONO_REGISTER_JIT_ICALL (__emul_fconv_to_u) \
MONO_REGISTER_JIT_ICALL (__emul_fconv_to_u1) \
MONO_REGISTER_JIT_ICALL (__emul_fconv_to_u2) \
MONO_REGISTER_JIT_ICALL (__emul_fconv_to_u4) \
MONO_REGISTER_JIT_ICALL (__emul_fconv_to_u8) \
MONO_REGISTER_JIT_ICALL (__emul_fdiv) \
MONO_REGISTER_JIT_ICALL (__emul_fmul) \
MONO_REGISTER_JIT_ICALL (__emul_fneg) \
MONO_REGISTER_JIT_ICALL (__emul_frem) \
MONO_REGISTER_JIT_ICALL (__emul_fsub) \
MONO_REGISTER_JIT_ICALL (__emul_iconv_to_r_un) \
MONO_REGISTER_JIT_ICALL (__emul_iconv_to_r4) \
MONO_REGISTER_JIT_ICALL (__emul_iconv_to_r8) \
MONO_REGISTER_JIT_ICALL (__emul_lconv_to_r4) \
MONO_REGISTER_JIT_ICALL (__emul_lconv_to_r8) \
MONO_REGISTER_JIT_ICALL (__emul_lconv_to_r8_un) \
MONO_REGISTER_JIT_ICALL (__emul_ldiv) \
MONO_REGISTER_JIT_ICALL (__emul_ldiv_un) \
MONO_REGISTER_JIT_ICALL (__emul_lmul) \
MONO_REGISTER_JIT_ICALL (__emul_lmul_ovf) \
MONO_REGISTER_JIT_ICALL (__emul_lmul_ovf_un) \
MONO_REGISTER_JIT_ICALL (__emul_lrem) \
MONO_REGISTER_JIT_ICALL (__emul_lrem_un) \
MONO_REGISTER_JIT_ICALL (__emul_lshl) \
MONO_REGISTER_JIT_ICALL (__emul_lshr) \
MONO_REGISTER_JIT_ICALL (__emul_lshr_un) \
MONO_REGISTER_JIT_ICALL (__emul_op_idiv) \
MONO_REGISTER_JIT_ICALL (__emul_op_idiv_un) \
MONO_REGISTER_JIT_ICALL (__emul_op_imul) \
MONO_REGISTER_JIT_ICALL (__emul_op_imul_ovf) \
MONO_REGISTER_JIT_ICALL (__emul_op_imul_ovf_un) \
MONO_REGISTER_JIT_ICALL (__emul_op_irem) \
MONO_REGISTER_JIT_ICALL (__emul_op_irem_un) \
MONO_REGISTER_JIT_ICALL (__emul_rconv_to_i8) \
MONO_REGISTER_JIT_ICALL (__emul_rconv_to_ovf_i8) \
MONO_REGISTER_JIT_ICALL (__emul_rconv_to_ovf_u8) \
MONO_REGISTER_JIT_ICALL (__emul_rconv_to_u8) \
MONO_REGISTER_JIT_ICALL (__emul_rrem) \
	\
MONO_REGISTER_JIT_ICALL (cominterop_get_ccw) \
MONO_REGISTER_JIT_ICALL (cominterop_get_ccw_object) \
MONO_REGISTER_JIT_ICALL (cominterop_get_function_pointer) \
MONO_REGISTER_JIT_ICALL (cominterop_get_interface) \
MONO_REGISTER_JIT_ICALL (cominterop_get_method_interface) \
MONO_REGISTER_JIT_ICALL (cominterop_object_is_rcw) \
MONO_REGISTER_JIT_ICALL (cominterop_type_from_handle) \
	\
MONO_REGISTER_JIT_ICALL (g_free) \
	\
MONO_REGISTER_JIT_ICALL (init_method) \
MONO_REGISTER_JIT_ICALL (init_method_gshared_mrgctx) \
MONO_REGISTER_JIT_ICALL (init_method_gshared_this) \
MONO_REGISTER_JIT_ICALL (init_method_gshared_vtable) \
	\
MONO_REGISTER_JIT_ICALL (mini_llvm_init_gshared_method_mrgctx) \
MONO_REGISTER_JIT_ICALL (mini_llvm_init_gshared_method_this) \
MONO_REGISTER_JIT_ICALL (mini_llvm_init_gshared_method_vtable) \
MONO_REGISTER_JIT_ICALL (mini_llvm_init_method) \
MONO_REGISTER_JIT_ICALL (mini_llvmonly_init_delegate) \
MONO_REGISTER_JIT_ICALL (mini_llvmonly_init_delegate_virtual) \
MONO_REGISTER_JIT_ICALL (mini_llvmonly_init_vtable_slot) \
MONO_REGISTER_JIT_ICALL (mini_llvmonly_resolve_generic_virtual_call) \
MONO_REGISTER_JIT_ICALL (mini_llvmonly_resolve_generic_virtual_iface_call) \
MONO_REGISTER_JIT_ICALL (mini_llvmonly_resolve_iface_call_gsharedvt) \
MONO_REGISTER_JIT_ICALL (mini_llvmonly_resolve_vcall_gsharedvt) \
MONO_REGISTER_JIT_ICALL (mini_llvmonly_throw_nullref_exception) \
	\
MONO_REGISTER_JIT_ICALL (mono_amd64_throw_exception) \
MONO_REGISTER_JIT_ICALL (mono_aot_init_gshared_method_mrgctx) \
MONO_REGISTER_JIT_ICALL (mono_aot_init_gshared_method_this) \
MONO_REGISTER_JIT_ICALL (mono_aot_init_gshared_method_vtable) \
MONO_REGISTER_JIT_ICALL (mono_aot_init_llvm_method) \
MONO_REGISTER_JIT_ICALL (mono_arch_rethrow_exception) \
MONO_REGISTER_JIT_ICALL (mono_arch_throw_corlib_exception) \
MONO_REGISTER_JIT_ICALL (mono_arch_throw_exception) \
MONO_REGISTER_JIT_ICALL (mono_arm_throw_exception) \
MONO_REGISTER_JIT_ICALL (mono_arm_throw_exception_by_token) \
MONO_REGISTER_JIT_ICALL (mono_arm_unaligned_stack) \
MONO_REGISTER_JIT_ICALL (mono_array_new_1) \
MONO_REGISTER_JIT_ICALL (mono_array_new_2) \
MONO_REGISTER_JIT_ICALL (mono_array_new_3) \
MONO_REGISTER_JIT_ICALL (mono_array_new_4) \
MONO_REGISTER_JIT_ICALL (mono_array_new_n_icall) \
MONO_REGISTER_JIT_ICALL (mono_array_to_byte_byvalarray) \
MONO_REGISTER_JIT_ICALL (mono_array_to_lparray) \
MONO_REGISTER_JIT_ICALL (mono_array_to_savearray) \
MONO_REGISTER_JIT_ICALL (mono_break) \
MONO_REGISTER_JIT_ICALL (mono_byvalarray_to_byte_array) \
MONO_REGISTER_JIT_ICALL (mono_chkstk_win64) \
MONO_REGISTER_JIT_ICALL (mono_ckfinite) \
MONO_REGISTER_JIT_ICALL (mono_class_interface_match) \
MONO_REGISTER_JIT_ICALL (mono_class_static_field_address) \
MONO_REGISTER_JIT_ICALL (mono_compile_method_icall) \
MONO_REGISTER_JIT_ICALL (mono_context_get_icall) \
MONO_REGISTER_JIT_ICALL (mono_context_set_icall) \
MONO_REGISTER_JIT_ICALL (mono_create_corlib_exception_0) \
MONO_REGISTER_JIT_ICALL (mono_create_corlib_exception_1) \
MONO_REGISTER_JIT_ICALL (mono_create_corlib_exception_2) \
MONO_REGISTER_JIT_ICALL (mono_debug_personality) \
MONO_REGISTER_JIT_ICALL (mono_debugger_agent_user_break) \
MONO_REGISTER_JIT_ICALL (mono_delegate_begin_invoke) \
MONO_REGISTER_JIT_ICALL (mono_delegate_end_invoke) \
MONO_REGISTER_JIT_ICALL (mono_delegate_to_ftnptr) \
MONO_REGISTER_JIT_ICALL (mono_domain_get) \
MONO_REGISTER_JIT_ICALL (mono_dummy_jit_icall) \
MONO_REGISTER_JIT_ICALL (mono_fill_class_rgctx) \
MONO_REGISTER_JIT_ICALL (mono_fill_method_rgctx) \
MONO_REGISTER_JIT_ICALL (mono_fload_r4) \
MONO_REGISTER_JIT_ICALL (mono_fload_r4_arg) \
MONO_REGISTER_JIT_ICALL (mono_free_bstr) \
MONO_REGISTER_JIT_ICALL (mono_free_lparray) \
MONO_REGISTER_JIT_ICALL (mono_fstore_r4) \
MONO_REGISTER_JIT_ICALL (mono_ftnptr_to_delegate) \
MONO_REGISTER_JIT_ICALL (mono_gc_alloc_obj) \
MONO_REGISTER_JIT_ICALL (mono_gc_alloc_string) \
MONO_REGISTER_JIT_ICALL (mono_gc_alloc_vector) \
MONO_REGISTER_JIT_ICALL (mono_gc_wbarrier_generic_nostore_internal) \
MONO_REGISTER_JIT_ICALL (mono_gc_wbarrier_range_copy) \
MONO_REGISTER_JIT_ICALL (mono_gchandle_get_target_internal) \
MONO_REGISTER_JIT_ICALL (mono_generic_class_init) \
MONO_REGISTER_JIT_ICALL (mono_get_assembly_object) \
MONO_REGISTER_JIT_ICALL (mono_get_lmf_addr) \
MONO_REGISTER_JIT_ICALL (mono_get_method_object) \
MONO_REGISTER_JIT_ICALL (mono_get_native_calli_wrapper) \
MONO_REGISTER_JIT_ICALL (mono_get_special_static_data) \
MONO_REGISTER_JIT_ICALL (mono_gsharedvt_constrained_call) \
MONO_REGISTER_JIT_ICALL (mono_gsharedvt_value_copy) \
MONO_REGISTER_JIT_ICALL (mono_helper_compile_generic_method) \
MONO_REGISTER_JIT_ICALL (mono_helper_ldstr) \
MONO_REGISTER_JIT_ICALL (mono_helper_ldstr_mscorlib) \
MONO_REGISTER_JIT_ICALL (mono_helper_newobj_mscorlib) \
MONO_REGISTER_JIT_ICALL (mono_helper_stelem_ref_check) \
MONO_REGISTER_JIT_ICALL (mono_init_vtable_slot) \
MONO_REGISTER_JIT_ICALL (mono_interp_entry_from_trampoline) \
MONO_REGISTER_JIT_ICALL (mono_interp_to_native_trampoline) \
MONO_REGISTER_JIT_ICALL (mono_isfinite_double) \
MONO_REGISTER_JIT_ICALL (mono_jit_set_domain) \
MONO_REGISTER_JIT_ICALL (mono_ldftn) \
MONO_REGISTER_JIT_ICALL (mono_ldtoken_wrapper) \
MONO_REGISTER_JIT_ICALL (mono_ldtoken_wrapper_generic_shared) \
MONO_REGISTER_JIT_ICALL (mono_ldvirtfn) \
MONO_REGISTER_JIT_ICALL (mono_ldvirtfn_gshared) \
MONO_REGISTER_JIT_ICALL (mono_llvm_clear_exception) \
MONO_REGISTER_JIT_ICALL (mono_llvm_load_exception) \
MONO_REGISTER_JIT_ICALL (mono_llvm_match_exception) \
MONO_REGISTER_JIT_ICALL (mono_llvm_resume_exception) \
MONO_REGISTER_JIT_ICALL (mono_llvm_resume_unwind_trampoline) \
MONO_REGISTER_JIT_ICALL (mono_llvm_rethrow_exception) \
MONO_REGISTER_JIT_ICALL (mono_llvm_rethrow_exception_trampoline) \
MONO_REGISTER_JIT_ICALL (mono_llvm_set_unhandled_exception_handler) \
MONO_REGISTER_JIT_ICALL (mono_llvm_throw_corlib_exception) \
MONO_REGISTER_JIT_ICALL (mono_llvm_throw_corlib_exception_abs_trampoline) \
MONO_REGISTER_JIT_ICALL (mono_llvm_throw_corlib_exception_trampoline) \
MONO_REGISTER_JIT_ICALL (mono_llvm_throw_exception) \
MONO_REGISTER_JIT_ICALL (mono_llvm_throw_exception_trampoline) \
MONO_REGISTER_JIT_ICALL (mono_llvmonly_init_delegate) \
MONO_REGISTER_JIT_ICALL (mono_llvmonly_init_delegate_virtual) \
MONO_REGISTER_JIT_ICALL (mono_marshal_asany) \
MONO_REGISTER_JIT_ICALL (mono_marshal_check_domain_image) \
MONO_REGISTER_JIT_ICALL (mono_marshal_free) \
MONO_REGISTER_JIT_ICALL (mono_marshal_free_array) \
MONO_REGISTER_JIT_ICALL (mono_marshal_free_asany) \
MONO_REGISTER_JIT_ICALL (mono_marshal_get_type_object) \
MONO_REGISTER_JIT_ICALL (mono_marshal_isinst_with_cache) \
MONO_REGISTER_JIT_ICALL (mono_marshal_safearray_begin) \
MONO_REGISTER_JIT_ICALL (mono_marshal_safearray_create) \
MONO_REGISTER_JIT_ICALL (mono_marshal_safearray_end) \
MONO_REGISTER_JIT_ICALL (mono_marshal_safearray_free_indices) \
MONO_REGISTER_JIT_ICALL (mono_marshal_safearray_get_value) \
MONO_REGISTER_JIT_ICALL (mono_marshal_safearray_next) \
MONO_REGISTER_JIT_ICALL (mono_marshal_safearray_set_value) \
MONO_REGISTER_JIT_ICALL (mono_marshal_set_domain_by_id) \
MONO_REGISTER_JIT_ICALL (mono_marshal_set_last_error) \
MONO_REGISTER_JIT_ICALL (mono_marshal_set_last_error_windows) \
MONO_REGISTER_JIT_ICALL (mono_marshal_string_to_utf16) \
MONO_REGISTER_JIT_ICALL (mono_marshal_string_to_utf16_copy) \
MONO_REGISTER_JIT_ICALL (mono_marshal_xdomain_copy_out_value) \
MONO_REGISTER_JIT_ICALL (mono_monitor_enter_fast) \
MONO_REGISTER_JIT_ICALL (mono_monitor_enter_internal) \
MONO_REGISTER_JIT_ICALL (mono_monitor_enter_v4_fast) \
MONO_REGISTER_JIT_ICALL (mono_monitor_enter_v4_internal) \
MONO_REGISTER_JIT_ICALL (mono_object_castclass_unbox) \
MONO_REGISTER_JIT_ICALL (mono_object_castclass_with_cache) \
MONO_REGISTER_JIT_ICALL (mono_object_isinst_icall) \
MONO_REGISTER_JIT_ICALL (mono_object_isinst_with_cache) \
MONO_REGISTER_JIT_ICALL (mono_profiler_raise_exception_clause) \
MONO_REGISTER_JIT_ICALL (mono_profiler_raise_gc_allocation) \
MONO_REGISTER_JIT_ICALL (mono_profiler_raise_method_enter) \
MONO_REGISTER_JIT_ICALL (mono_profiler_raise_method_leave) \
MONO_REGISTER_JIT_ICALL (mono_profiler_raise_method_tail_call) \
MONO_REGISTER_JIT_ICALL (mono_remoting_update_exception) \
MONO_REGISTER_JIT_ICALL (mono_remoting_wrapper) \
MONO_REGISTER_JIT_ICALL (mono_resolve_generic_virtual_call) \
MONO_REGISTER_JIT_ICALL (mono_resolve_generic_virtual_iface_call) \
MONO_REGISTER_JIT_ICALL (mono_resolve_iface_call_gsharedvt) \
MONO_REGISTER_JIT_ICALL (mono_resolve_vcall_gsharedvt) \
MONO_REGISTER_JIT_ICALL (mono_resume_unwind) \
MONO_REGISTER_JIT_ICALL (mono_string_builder_to_utf16) \
MONO_REGISTER_JIT_ICALL (mono_string_builder_to_utf8) \
MONO_REGISTER_JIT_ICALL (mono_string_from_bstr_icall) \
MONO_REGISTER_JIT_ICALL (mono_string_from_byvalstr) \
MONO_REGISTER_JIT_ICALL (mono_string_from_byvalwstr) \
MONO_REGISTER_JIT_ICALL (mono_string_new_len_wrapper) \
MONO_REGISTER_JIT_ICALL (mono_string_new_wrapper_internal) \
MONO_REGISTER_JIT_ICALL (mono_string_to_ansibstr) \
MONO_REGISTER_JIT_ICALL (mono_string_to_bstr) \
MONO_REGISTER_JIT_ICALL (mono_string_to_byvalstr) \
MONO_REGISTER_JIT_ICALL (mono_string_to_byvalwstr) \
MONO_REGISTER_JIT_ICALL (mono_string_to_utf16_internal) \
MONO_REGISTER_JIT_ICALL (mono_string_to_utf8str) \
MONO_REGISTER_JIT_ICALL (mono_string_utf16_to_builder) \
MONO_REGISTER_JIT_ICALL (mono_string_utf16_to_builder2) \
MONO_REGISTER_JIT_ICALL (mono_string_utf8_to_builder) \
MONO_REGISTER_JIT_ICALL (mono_string_utf8_to_builder2) \
MONO_REGISTER_JIT_ICALL (mono_struct_delete_old) \
MONO_REGISTER_JIT_ICALL (mono_thread_force_interruption_checkpoint_noraise) \
MONO_REGISTER_JIT_ICALL (mono_thread_get_undeniable_exception) \
MONO_REGISTER_JIT_ICALL (mono_thread_interruption_checkpoint) \
MONO_REGISTER_JIT_ICALL (mono_threads_attach_coop) \
MONO_REGISTER_JIT_ICALL (mono_threads_detach_coop) \
MONO_REGISTER_JIT_ICALL (mono_threads_enter_gc_safe_region_unbalanced) \
MONO_REGISTER_JIT_ICALL (mono_threads_enter_gc_unsafe_region_unbalanced) \
MONO_REGISTER_JIT_ICALL (mono_threads_exit_gc_safe_region_unbalanced) \
MONO_REGISTER_JIT_ICALL (mono_threads_exit_gc_unsafe_region_unbalanced) \
MONO_REGISTER_JIT_ICALL (mono_threads_state_poll) \
MONO_REGISTER_JIT_ICALL (mono_throw_method_access) \
MONO_REGISTER_JIT_ICALL (mono_tls_get_domain) \
MONO_REGISTER_JIT_ICALL (mono_tls_get_jit_tls) \
MONO_REGISTER_JIT_ICALL (mono_tls_get_lmf_addr) \
MONO_REGISTER_JIT_ICALL (mono_tls_get_sgen_thread_info) \
MONO_REGISTER_JIT_ICALL (mono_tls_get_thread) \
MONO_REGISTER_JIT_ICALL (mono_tls_set_domain) \
MONO_REGISTER_JIT_ICALL (mono_tls_set_jit_tls) \
MONO_REGISTER_JIT_ICALL (mono_tls_set_lmf_addr) \
MONO_REGISTER_JIT_ICALL (mono_tls_set_sgen_thread_info) \
MONO_REGISTER_JIT_ICALL (mono_tls_set_thread) \
MONO_REGISTER_JIT_ICALL (mono_trace_enter_method) \
MONO_REGISTER_JIT_ICALL (mono_trace_leave_method) \
MONO_REGISTER_JIT_ICALL (mono_upgrade_remote_class_wrapper) \
MONO_REGISTER_JIT_ICALL (mono_value_copy_internal) \
	\
MONO_REGISTER_JIT_ICALL (personality) \
	\
MONO_REGISTER_JIT_ICALL (pthread_getspecific) \
	\
MONO_REGISTER_JIT_ICALL (type_from_handle) \
	\
MONO_REGISTER_JIT_ICALL (ves_icall_array_new) \
MONO_REGISTER_JIT_ICALL (ves_icall_array_new_specific) \
MONO_REGISTER_JIT_ICALL (ves_icall_marshal_alloc) \
MONO_REGISTER_JIT_ICALL (ves_icall_mono_delegate_ctor) \
MONO_REGISTER_JIT_ICALL (ves_icall_mono_delegate_ctor_interp) \
MONO_REGISTER_JIT_ICALL (ves_icall_mono_ldstr) \
MONO_REGISTER_JIT_ICALL (ves_icall_mono_marshal_xdomain_copy_value) \
MONO_REGISTER_JIT_ICALL (ves_icall_mono_string_from_utf16) \
MONO_REGISTER_JIT_ICALL (ves_icall_mono_string_to_utf8) \
MONO_REGISTER_JIT_ICALL (ves_icall_object_new) \
MONO_REGISTER_JIT_ICALL (ves_icall_object_new_specific) \
MONO_REGISTER_JIT_ICALL (ves_icall_runtime_class_init) \
MONO_REGISTER_JIT_ICALL (ves_icall_string_alloc) \
MONO_REGISTER_JIT_ICALL (ves_icall_string_new_wrapper) \
MONO_REGISTER_JIT_ICALL (ves_icall_thread_finish_async_abort) \
	\
MONO_REGISTER_JIT_ICALL (count) \

// This enum is not actually used, except for count.
// Instead mono_jit_icall_info_index should
// generate the values from pointers for AOT and ilgen.
// They could be used for const static special case arrays,
// to shrink a pointer from 8 bytes + reloc to 4 (or 2) bytes.
typedef enum MonoJitICallId {
#define MONO_REGISTER_JIT_ICALL(x) MONO_JIT_ICALL_ ## x,
MONO_REGISTER_JIT_ICALLS
	mono_jit_icall_count = MONO_JIT_ICALL_count,
#undef MONO_REGISTER_JIT_ICALL
} MonoJitICallId;

typedef union MonoJitICallInfos {
	struct {
#define MONO_REGISTER_JIT_ICALL(x) MonoJitICallInfo x;
MONO_REGISTER_JIT_ICALLS
#undef MONO_REGISTER_JIT_ICALL
	};
	MonoJitICallInfo array [mono_jit_icall_count];
} MonoJitICallInfos;

extern MonoJitICallInfos mono_jit_icall_info;

#define mono_jit_icall_info_index(x) ((x) - mono_jit_icall_info.array)

#endif // __MONO_METADATA_REGISTER_ICALL_DEF_H__
