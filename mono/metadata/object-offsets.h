/*
This is a parameterized header. It's supposed/ok to be included multiple times.

Input defines: (those to be defined by the includer file)

Required:
DECL_OFFSET(struct,field)
DECL_OFFSET2(struct,field,offset)
DECL_ALIGN(name,type)
DECL_ALIGN2(name,alignment)

Optional:
USE_CROSS_COMPILE_OFFSETS - if defined, force the cross compiler offsets to be used, otherwise
	they will only be used if MONO_CROSS_COMPILE is defined

Output defines:

HAS_CROSS_COMPILER_OFFSETS - if set, it means we found some cross offsets, it doesnt mean we'll use it.
USED_CROSS_COMPILER_OFFSETS - if set, it means we used the cross offsets
*/


#undef HAS_CROSS_COMPILER_OFFSETS
#undef USED_CROSS_COMPILER_OFFSETS

#ifdef ENABLE_EXTENSION_MODULE
#include "../../../mono-extensions/mono/metadata/object-offsets.h"
#endif


#ifndef USED_CROSS_COMPILER_OFFSETS

DECL_ALIGN(gint8)
DECL_ALIGN(gint16)
DECL_ALIGN(gint32)
DECL_ALIGN(gint64)
DECL_ALIGN(float)
DECL_ALIGN(double)
DECL_ALIGN(gpointer)

#ifndef DISABLE_METADATA_OFFSETS
//object offsets
DECL_OFFSET(MonoObject, vtable)
DECL_OFFSET(MonoObject, synchronisation)

DECL_OFFSET(MonoClass, interface_bitmap)
DECL_OFFSET(MonoClass, byval_arg)
DECL_OFFSET(MonoClass, cast_class)
DECL_OFFSET(MonoClass, element_class)
DECL_OFFSET(MonoClass, idepth)
DECL_OFFSET(MonoClass, instance_size)
DECL_OFFSET(MonoClass, interface_id)
DECL_OFFSET(MonoClass, max_interface_id)
DECL_OFFSET(MonoClass, parent)
DECL_OFFSET(MonoClass, rank)
DECL_OFFSET(MonoClass, sizes)
DECL_OFFSET(MonoClass, supertypes)

DECL_OFFSET(MonoVTable, klass)
DECL_OFFSET(MonoVTable, max_interface_id)
DECL_OFFSET(MonoVTable, interface_bitmap)
DECL_OFFSET(MonoVTable, vtable)
DECL_OFFSET(MonoVTable, rank)
DECL_OFFSET(MonoVTable, type)
DECL_OFFSET(MonoVTable, runtime_generic_context)

DECL_OFFSET(MonoDelegate, target)
DECL_OFFSET(MonoDelegate, method_ptr)
DECL_OFFSET(MonoDelegate, invoke_impl)
DECL_OFFSET(MonoDelegate, method)
DECL_OFFSET(MonoDelegate, method_code)

DECL_OFFSET(MonoInternalThread, tid)
DECL_OFFSET(MonoInternalThread, static_data)

DECL_OFFSET(MonoMulticastDelegate, prev)

DECL_OFFSET(MonoTransparentProxy, rp)
DECL_OFFSET(MonoTransparentProxy, remote_class)
DECL_OFFSET(MonoTransparentProxy, custom_type_info)

DECL_OFFSET(MonoRealProxy, target_domain_id)
DECL_OFFSET(MonoRealProxy, context)
DECL_OFFSET(MonoRealProxy, unwrapped_server)

DECL_OFFSET(MonoRemoteClass, proxy_class)

DECL_OFFSET(MonoArray, vector)
DECL_OFFSET(MonoArray, max_length)
DECL_OFFSET(MonoArray, bounds)

DECL_OFFSET(MonoArrayBounds, lower_bound)
DECL_OFFSET(MonoArrayBounds, length)

DECL_OFFSET(MonoSafeHandle, handle)

DECL_OFFSET(MonoHandleRef, handle)

DECL_OFFSET(MonoComInteropProxy, com_object)

DECL_OFFSET(MonoString, length)
DECL_OFFSET(MonoString, chars)

DECL_OFFSET(MonoException, message)

DECL_OFFSET(MonoTypedRef, type)
DECL_OFFSET(MonoTypedRef, klass)
DECL_OFFSET(MonoTypedRef, value)

//Internal structs
DECL_OFFSET(MonoThreadsSync, owner)
DECL_OFFSET(MonoThreadsSync, nest)
DECL_OFFSET(MonoThreadsSync, entry_count)

#if defined (HAVE_SGEN_GC) && !defined (HAVE_KW_THREAD)
DECL_OFFSET(SgenThreadInfo, tlab_next_addr)
DECL_OFFSET(SgenThreadInfo, tlab_temp_end)
#endif

#endif //DISABLE METADATA OFFSETS

#ifndef DISABLE_JIT_OFFSETS
DECL_OFFSET(MonoLMF, previous_lmf)
DECL_OFFSET(MonoLMF, method)
DECL_OFFSET(MonoLMF, lmf_addr)

DECL_OFFSET(MonoMethodRuntimeGenericContext, class_vtable)

#ifdef MONO_JIT_TLS_DATA_HAS_LMF
DECL_OFFSET(MonoJitTlsData, lmf)
#endif

DECL_OFFSET(MonoJitTlsData, class_cast_from)
DECL_OFFSET(MonoJitTlsData, class_cast_to)
DECL_OFFSET(MonoJitTlsData, handler_block_return_address)
DECL_OFFSET(MonoJitTlsData, restore_stack_prot)

DECL_OFFSET(MonoGSharedVtMethodRuntimeInfo, locals_size)
DECL_OFFSET(MonoGSharedVtMethodRuntimeInfo, entries) //XXX more to fix here

DECL_OFFSET(MonoContinuation, stack_used_size)
DECL_OFFSET(MonoContinuation, saved_stack)
DECL_OFFSET(MonoContinuation, return_sp)
DECL_OFFSET(MonoContinuation, lmf)
DECL_OFFSET(MonoContinuation, return_ip)

#ifdef TARGET_X86
DECL_OFFSET(MonoContext, eax)
DECL_OFFSET(MonoContext, ebx)
DECL_OFFSET(MonoContext, ecx)
DECL_OFFSET(MonoContext, edx)
DECL_OFFSET(MonoContext, edi)
DECL_OFFSET(MonoContext, esi)
DECL_OFFSET(MonoContext, esp)
DECL_OFFSET(MonoContext, ebp)
DECL_OFFSET(MonoContext, eip)

DECL_OFFSET(MonoLMF, esp)
DECL_OFFSET(MonoLMF, ebx)
DECL_OFFSET(MonoLMF, edi)
DECL_OFFSET(MonoLMF, esi)
DECL_OFFSET(MonoLMF, ebp)
DECL_OFFSET(MonoLMF, eip)
#endif

#ifdef TARGET_ARM
DECL_OFFSET (MonoContext, pc)
DECL_OFFSET (MonoContext, regs)
DECL_OFFSET (MonoContext, fregs)

DECL_OFFSET(MonoLMF, sp)
DECL_OFFSET(MonoLMF, fp)
DECL_OFFSET(MonoLMF, ip)
DECL_OFFSET(MonoLMF, iregs)
DECL_OFFSET(MonoLMF, fregs)

DECL_OFFSET(SeqPointInfo, bp_addrs)
DECL_OFFSET(SeqPointInfo, ss_trigger_page)

DECL_OFFSET(DynCallArgs, res)
DECL_OFFSET(DynCallArgs, res2)
#endif

#endif

#endif

#undef DECL_OFFSET
#undef DECL_OFFSET2
#undef DECL_ALIGN
#undef DECL_ALIGN2
#undef USE_CROSS_COMPILE_OFFSETS
