/**
 * \file
 * Copyright 2016 Microsoft
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */
#ifndef __MONO_METADATA_ICALL_TABLE_H__
#define __MONO_METADATA_ICALL_TABLE_H__

#include <config.h>
#include <glib.h>
#include <mono/utils/mono-publib.h>
#include "icall.h"

#define MONO_ICALL_TABLE_CALLBACKS_VERSION 1

typedef struct {
	int version;
	gpointer (*lookup) (char *classname, char *methodname, char *sigstart, gboolean *uses_handles);
	const char* (*lookup_icall_symbol) (gpointer func);
} MonoIcallTableCallbacks;

void
mono_install_icall_table_callbacks (MonoIcallTableCallbacks *cb);

MONO_API void
mono_icall_table_init (void);

/* From MonoProperty.cs */
typedef enum {
	xPInfo_Attributes = 1,
	xPInfo_GetMethod  = 1 << 1,
	xPInfo_SetMethod  = 1 << 2,
	xPInfo_ReflectedType = 1 << 3,
	xPInfo_DeclaringType = 1 << 4,
	xPInfo_Name = 1 << 5
} PInfox;

// It helps for types to be single tokens, though this can be relaxed in some places.
#include "utils/mono-forward-internal.h"

// Marshaling a "ptr" does nothing.
// Marshaling a "ref" creates an interior handle and passes on the raw pointer.
typedef gint32  *gint32_ptr;
typedef gint64  *gint64_ptr;
typedef guint   *guint_ptr;
typedef guint32 *guint32_ptr;
typedef guint64 *guint64_ptr;
typedef gsize *gsize_ptr;
typedef guchar *guchar_ptr;
typedef const guchar *const_guchar_ptr;
typedef gpointer *gpointer_ptr;
typedef const char *const_char_ptr;
typedef char *char_ptr;
typedef char **char_ptr_ptr;
typedef gunichar2 *gunichar2_ptr;
typedef const gunichar2 *const_gunichar2_ptr;
typedef int *int_ptr;
typedef int **int_ptr_ref;
typedef guint8 **guint8_ptr_ref;
typedef GPtrArray *GPtrArray_ptr;
typedef MonoAssemblyName *MonoAssemblyName_ptr;
typedef MonoBoolean *MonoBoolean_ptr;
typedef MonoClass *MonoClass_ptr;
typedef MonoClassField *MonoClassField_ptr;
typedef MonoEvent *MonoEvent_ptr;
typedef MonoImage *MonoImage_ptr;
typedef MonoMethod *MonoMethod_ptr;
typedef MonoNativeOverlapped *MonoNativeOverlapped_ptr;
typedef MonoProperty *MonoProperty_ptr;
typedef MonoPropertyInfo *MonoPropertyInfo_ref;
typedef MonoType *MonoType_ptr;
typedef MonoTypedRef *MonoTypedRef_ptr;
typedef gunichar2 *mono_bstr;
typedef const gunichar2 *mono_bstr_const;
typedef MonoIUnknown *MonoIUnknown_ptr;
typedef unsigned *unsigned_ptr;
typedef mono_unichar2 *mono_unichar2_ptr;
typedef WSABUF *WSABUF_ptr;

typedef const char *const_char_ref;
typedef char *char_ref;
typedef char **char_ptr_ref;
typedef const gunichar2 *const_gunichar2_ref;
typedef gint32  *gint32_ref;
typedef gint64  *gint64_ref;
typedef gpointer *gpointer_ref;
typedef gsize *gsize_ref;
typedef guint   *guint_ref;
typedef guint32 *guint32_ref;
typedef guint64 *guint64_ref;
typedef int *int_ref;
typedef GPtrArray *GPtrArray_ref;
typedef MonoAssemblyName *MonoAssemblyName_ref;
typedef MonoBoolean *MonoBoolean_ref;
typedef MonoClass *MonoClass_ref;
typedef MonoClassField *MonoClassField_ref;
typedef MonoEvent *MonoEvent_ref;
typedef MonoEventInfo *MonoEventInfo_ref;
typedef MonoMethod *MonoMethod_ref;
typedef MonoMethodInfo *MonoMethodInfo_ref;
typedef MonoNativeOverlapped *MonoNativeOverlapped_ref;
typedef MonoResolveTokenError *MonoResolveTokenError_ref;
typedef MonoType *MonoType_ref;
typedef MonoTypedRef *MonoTypedRef_ref;
typedef MonoW32ProcessInfo *MonoW32ProcessInfo_ref;
typedef MonoGenericParamInfo *MonoGenericParamInfo_ptr;

// Maybe do this in TYPED_HANDLE_DECL.
typedef MonoArray MonoArrayOut;
typedef MonoArray MonoArrayInOut;
typedef MonoObject MonoObjectOut;
typedef MonoObject MonoObjectInOut;
typedef MonoObjectHandle MonoObjectOutHandle;
typedef MonoObjectHandle MonoObjectInOutHandle;
typedef MonoArrayHandle MonoArrayOutHandle;
typedef MonoArrayHandle MonoArrayInOutHandle;
typedef MonoString MonoStringOut;
typedef MonoStringHandle MonoStringOutHandle;
typedef MonoReflectionModule MonoReflectionModuleOut;
typedef MonoReflectionModuleHandle MonoReflectionModuleOutHandle;

// How the arguments and return value of an icall should be wrapped.
// These names are historical, from marshal-ilgen.c.
//
// ICALL_HANDLES_WRAP_NONE
// Do not wrap at all, pass the argument as is.
//
// ICALL_HANDLES_WRAP_OBJ
// Wrap the argument in an object handle, pass the handle to the icall.
//
// ICALL_HANDLES_WRAP_OBJ_INOUT
// Wrap the argument in an object handle, pass the handle to the icall,
// write the value out from the handle when the icall returns. This is rare,
// and could be easily eliminated.
//
// ICALL_HANDLES_WRAP_OBJ_OUT
// Initialized an object handle to null, pass to the icalls,
// write the value out from the handle when the icall returns.
//
// ICALL_HANDLES_WRAP_VALUETYPE_REF
// Wrap the argument (a valuetype reference) in a handle to pin its
// enclosing object, but pass the raw reference to the icall. This is
// also how we pass byref generic parameter arguments to generic method
// icalls (eg, System.Array:GetGenericValueImpl<T>(int idx, T out value)).

// Map a type to a type class: Void and above.
#define MONO_HANDLE_TYPE_WRAP_void 			Void
#define MONO_HANDLE_TYPE_WRAP_GPtrArray_ptr  		ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_MonoBoolean   		ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_const_gunichar2_ptr	ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_gunichar2_ptr		ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_gboolean   		ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_gint   			ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_gint32   			ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_gint64   			ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_gpointer   		ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_gconstpointer   		ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_gsize   			ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_gssize   			ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_guchar_ptr		ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_const_guchar_ptr		ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_guint32  			ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_guint64  			ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_int 			ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_mono_bool			ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_uint     			ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_PInfo			ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_mono_bstr			ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_mono_bstr_const		ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_MonoIUnknown_ptr		ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_unsigned_ptr		ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_mono_unichar2_ptr		ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_MonoImage_ptr		ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_MonoClassField_ptr	ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_MonoProperty_ptr		ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_MonoProtocolType		ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_WSABUF_ptr		ICALL_HANDLES_WRAP_NONE

// FIXME _ref vs. _ptr needs work/auditing and pruning

#define MONO_HANDLE_TYPE_WRAP_MonoAssemblyName_ref	ICALL_HANDLES_WRAP_VALUETYPE_REF
#define MONO_HANDLE_TYPE_WRAP_MonoBoolean_ref 		ICALL_HANDLES_WRAP_VALUETYPE_REF
#define MONO_HANDLE_TYPE_WRAP_MonoClass_ref  		ICALL_HANDLES_WRAP_VALUETYPE_REF
#define MONO_HANDLE_TYPE_WRAP_MonoClassField_ref  	ICALL_HANDLES_WRAP_VALUETYPE_REF
#define MONO_HANDLE_TYPE_WRAP_MonoEvent_ref		ICALL_HANDLES_WRAP_VALUETYPE_REF
#define MONO_HANDLE_TYPE_WRAP_MonoEventInfo_ref		ICALL_HANDLES_WRAP_VALUETYPE_REF
#define MONO_HANDLE_TYPE_WRAP_MonoMethod_ref 		ICALL_HANDLES_WRAP_VALUETYPE_REF
#define MONO_HANDLE_TYPE_WRAP_MonoMethodInfo_ref	ICALL_HANDLES_WRAP_VALUETYPE_REF
#define MONO_HANDLE_TYPE_WRAP_MonoNativeOverlapped_ref	ICALL_HANDLES_WRAP_VALUETYPE_REF
#define MONO_HANDLE_TYPE_WRAP_MonoPropertyInfo_ref	ICALL_HANDLES_WRAP_VALUETYPE_REF
#define MONO_HANDLE_TYPE_WRAP_MonoType_ref  		ICALL_HANDLES_WRAP_VALUETYPE_REF
#define MONO_HANDLE_TYPE_WRAP_MonoTypedRef_ref 		ICALL_HANDLES_WRAP_VALUETYPE_REF
#define MONO_HANDLE_TYPE_WRAP_char_ref   		ICALL_HANDLES_WRAP_VALUETYPE_REF
#define MONO_HANDLE_TYPE_WRAP_const_char_ref		ICALL_HANDLES_WRAP_VALUETYPE_REF
#define MONO_HANDLE_TYPE_WRAP_gint32_ref   		ICALL_HANDLES_WRAP_VALUETYPE_REF
#define MONO_HANDLE_TYPE_WRAP_gint64_ref  		ICALL_HANDLES_WRAP_VALUETYPE_REF
#define MONO_HANDLE_TYPE_WRAP_gpointer_ref   		ICALL_HANDLES_WRAP_VALUETYPE_REF
#define MONO_HANDLE_TYPE_WRAP_gsize_ref   		ICALL_HANDLES_WRAP_VALUETYPE_REF
#define MONO_HANDLE_TYPE_WRAP_guint32_ref   		ICALL_HANDLES_WRAP_VALUETYPE_REF
#define MONO_HANDLE_TYPE_WRAP_guint64_ref   		ICALL_HANDLES_WRAP_VALUETYPE_REF
#define MONO_HANDLE_TYPE_WRAP_int_ref  			ICALL_HANDLES_WRAP_VALUETYPE_REF
#define MONO_HANDLE_TYPE_WRAP_int_ptr_ref  		ICALL_HANDLES_WRAP_VALUETYPE_REF
#define MONO_HANDLE_TYPE_WRAP_char_ptr_ref		ICALL_HANDLES_WRAP_VALUETYPE_REF
#define MONO_HANDLE_TYPE_WRAP_guint8_ptr_ref		ICALL_HANDLES_WRAP_VALUETYPE_REF
#define MONO_HANDLE_TYPE_WRAP_MonoResolveTokenError_ref	ICALL_HANDLES_WRAP_VALUETYPE_REF

#define MONO_HANDLE_TYPE_WRAP_char_ptr   		ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_const_char_ptr		ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_MonoClass_ptr  		ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_MonoEvent_ptr		ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_MonoGenericParamInfo_ptr	ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_MonoMethod_ptr 		ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_MonoNativeOverlapped_ptr	ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_MonoType_ptr  		ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_MonoTypedRef_ptr 		ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_gint32_ptr   		ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_guint32_ptr   		ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_guint64_ptr   		ICALL_HANDLES_WRAP_NONE
#define MONO_HANDLE_TYPE_WRAP_gpointer_ptr		ICALL_HANDLES_WRAP_NONE

// FIXME Use of gulong is a mistake.
#define MONO_HANDLE_TYPE_WRAP_gulong   			ICALL_HANDLES_WRAP_NONE

// Please keep this sorted (grep ICALL_HANDLES_WRAP_OBJ$ | sort)
#define MONO_HANDLE_TYPE_WRAP_MonoAppContext 			ICALL_HANDLES_WRAP_OBJ
#define MONO_HANDLE_TYPE_WRAP_MonoAppDomain			ICALL_HANDLES_WRAP_OBJ
#define MONO_HANDLE_TYPE_WRAP_MonoAppDomainSetup		ICALL_HANDLES_WRAP_OBJ
#define MONO_HANDLE_TYPE_WRAP_MonoArray				ICALL_HANDLES_WRAP_OBJ
#define MONO_HANDLE_TYPE_WRAP_MonoComInteropProxy		ICALL_HANDLES_WRAP_OBJ
#define MONO_HANDLE_TYPE_WRAP_MonoComObject			ICALL_HANDLES_WRAP_OBJ
#define MONO_HANDLE_TYPE_WRAP_MonoDelegate			ICALL_HANDLES_WRAP_OBJ
#define MONO_HANDLE_TYPE_WRAP_MonoReflectionDynamicMethod 	ICALL_HANDLES_WRAP_OBJ
#define MONO_HANDLE_TYPE_WRAP_MonoException			ICALL_HANDLES_WRAP_OBJ
#define MONO_HANDLE_TYPE_WRAP_MonoInternalThread		ICALL_HANDLES_WRAP_OBJ
#define MONO_HANDLE_TYPE_WRAP_MonoObject			ICALL_HANDLES_WRAP_OBJ
#define MONO_HANDLE_TYPE_WRAP_MonoManifestResourceInfo 		ICALL_HANDLES_WRAP_OBJ
#define MONO_HANDLE_TYPE_WRAP_MonoMulticastDelegate		ICALL_HANDLES_WRAP_OBJ
#define MONO_HANDLE_TYPE_WRAP_MonoReflectionAssembly		ICALL_HANDLES_WRAP_OBJ
#define MONO_HANDLE_TYPE_WRAP_MonoReflectionAssemblyBuilder 	ICALL_HANDLES_WRAP_OBJ
#define MONO_HANDLE_TYPE_WRAP_MonoReflectionEvent		ICALL_HANDLES_WRAP_OBJ
#define MONO_HANDLE_TYPE_WRAP_MonoReflectionMonoEvent		ICALL_HANDLES_WRAP_OBJ
#define MONO_HANDLE_TYPE_WRAP_MonoReflectionField		ICALL_HANDLES_WRAP_OBJ
#define MONO_HANDLE_TYPE_WRAP_MonoReflectionMarshalAsAttribute	ICALL_HANDLES_WRAP_OBJ
#define MONO_HANDLE_TYPE_WRAP_MonoReflectionMethod     		ICALL_HANDLES_WRAP_OBJ
#define MONO_HANDLE_TYPE_WRAP_MonoReflectionMethodBody 		ICALL_HANDLES_WRAP_OBJ
#define MONO_HANDLE_TYPE_WRAP_MonoReflectionModule 		ICALL_HANDLES_WRAP_OBJ
#define MONO_HANDLE_TYPE_WRAP_MonoReflectionModuleBuilder 	ICALL_HANDLES_WRAP_OBJ
#define MONO_HANDLE_TYPE_WRAP_MonoReflectionParameter		ICALL_HANDLES_WRAP_OBJ
#define MONO_HANDLE_TYPE_WRAP_MonoReflectionProperty		ICALL_HANDLES_WRAP_OBJ
#define MONO_HANDLE_TYPE_WRAP_MonoReflectionSigHelper		ICALL_HANDLES_WRAP_OBJ
#define MONO_HANDLE_TYPE_WRAP_MonoReflectionType		ICALL_HANDLES_WRAP_OBJ
#define MONO_HANDLE_TYPE_WRAP_MonoReflectionTypeBuilder		ICALL_HANDLES_WRAP_OBJ
#define MONO_HANDLE_TYPE_WRAP_MonoString			ICALL_HANDLES_WRAP_OBJ
#define MONO_HANDLE_TYPE_WRAP_MonoThreadObject			ICALL_HANDLES_WRAP_OBJ
#define MONO_HANDLE_TYPE_WRAP_MonoW32ProcessStartInfo		ICALL_HANDLES_WRAP_OBJ

#define MONO_HANDLE_TYPE_WRAP_MonoObjectOut		ICALL_HANDLES_WRAP_OBJ_OUT
#define MONO_HANDLE_TYPE_WRAP_MonoStringOut		ICALL_HANDLES_WRAP_OBJ_OUT
#define MONO_HANDLE_TYPE_WRAP_MonoArrayOut		ICALL_HANDLES_WRAP_OBJ_OUT
#define MONO_HANDLE_TYPE_WRAP_MonoReflectionModuleOut 	ICALL_HANDLES_WRAP_OBJ_OUT

#define MONO_HANDLE_TYPE_WRAP_MonoW32ProcessInfo_ref	ICALL_HANDLES_WRAP_VALUETYPE_REF

// These are rare, and could be eliminated.
// They could be return values, or just separate out parameters.
#define MONO_HANDLE_TYPE_WRAP_MonoObjectInOut	ICALL_HANDLES_WRAP_OBJ_INOUT
#define MONO_HANDLE_TYPE_WRAP_MonoArrayInOut	ICALL_HANDLES_WRAP_OBJ_INOUT

// Do macro_prefix for type type, mapping type to a type class.
// Note that the macro can further be followed by parameters.
#define MONO_HANDLES_DO3(macro_prefix, type) macro_prefix ## type
#define MONO_HANDLES_DO2(macro_prefix, type) MONO_HANDLES_DO3 (macro_prefix, type)
#define MONO_HANDLES_DO(macro_prefix, type)  MONO_HANDLES_DO2 (macro_prefix, MONO_HANDLE_TYPE_WRAP_ ## type)

// Does type require a local handle for marshaling, 0 or 1.
#define MONO_ANY_HANDLES(type)					| MONO_HANDLES_DO (MONO_ANY_HANDLES_, type)
#define MONO_ANY_HANDLES_Void					0
#define MONO_ANY_HANDLES_ICALL_HANDLES_WRAP_NONE		0
#define MONO_ANY_HANDLES_ICALL_HANDLES_WRAP_OBJ			1
#define MONO_ANY_HANDLES_ICALL_HANDLES_WRAP_OBJ_OUT		1
#define MONO_ANY_HANDLES_ICALL_HANDLES_WRAP_OBJ_INOUT		1
#define MONO_ANY_HANDLES_ICALL_HANDLES_WRAP_VALUETYPE_REF	1
#define MONO_ANY_HANDLES_ICALL_HANDLES_WRAP_VALUETYPE_REF	1

// In a function with handles, HANDLE_FUNCTION_RETURN or HANDLE_FUNCTION_RETURN or HANDLE_FUNCTION_RETURN_OBJ.
#define MONO_ANY_HANDLES_RETURN(type)	MONO_HANDLES_DO (MONO_ANY_HANDLES_RETURN_, type);
#define MONO_ANY_HANDLES_RETURN_Void	HANDLE_FUNCTION_RETURN (); return
#define MONO_ANY_HANDLES_RETURN_ICALL_HANDLES_WRAP_NONE   HANDLE_FUNCTION_RETURN_VAL (result)
#define MONO_ANY_HANDLES_RETURN_ICALL_HANDLES_WRAP_OBJ	HANDLE_FUNCTION_RETURN_OBJ (result)
// ICALL_HANDLES_WRAP_OBJ_OUT, ICALL_HANDLES_WRAP_OBJ_INOUT, ICALL_HANDLES_WRAP_VALUETYPE_REF not applicable -- they are only parameter types, not return types.

// In a function with no handles, return void or result.
#define MONO_NO_HANDLES_RETURN(type)				MONO_HANDLES_DO (MONO_NO_HANDLES_RETURN_, type);
#define MONO_NO_HANDLES_RETURN_Void     			return
#define MONO_NO_HANDLES_RETURN_ICALL_HANDLES_WRAP_NONE     	return result
// This cannot happen, as returning an object requires a frame. However we have to form
// something that compiles due to a runtime constant conditional.
#define MONO_NO_HANDLES_RETURN_ICALL_HANDLES_WRAP_OBJ		g_assert (FALSE); return 0
// WRAP_OBJ_OUT, WRAP_OBJ_INOUT, WRAP_VALUETYPE_REF not applicable to return values, only parameters.

// Create a local for argument n of type type.
// i.e. nothing or MonoObjectHandle
#define MONO_ARG_LOCAL(type, n)						MONO_HANDLES_DO (MONO_ARG_LOCAL_, type) (type, n);
#define MONO_ARG_LOCAL_ICALL_HANDLES_WRAP_NONE(type, n)     		/* nothing */
#define MONO_ARG_LOCAL_ICALL_HANDLES_WRAP_OBJ(type, n)			type ## Handle a ## n
#define MONO_ARG_LOCAL_ICALL_HANDLES_WRAP_OBJ_OUT(type, n)		MONO_ARG_LOCAL_ICALL_HANDLES_WRAP_OBJ (type, n)
#define MONO_ARG_LOCAL_ICALL_HANDLES_WRAP_OBJ_INOUT(type, n)		MONO_ARG_LOCAL_ICALL_HANDLES_WRAP_OBJ (type, n)
#define MONO_ARG_LOCAL_ICALL_HANDLES_WRAP_VALUETYPE_REF(type, n)	gpointer a ## n ## _interior_handle

// Declare and assign local variable to be returned, i.e. nothing for void, direct type
// for non-handles, or a handle.
#define MONO_RET_LOCAL(type)				MONO_HANDLES_DO (MONO_RET_LOCAL_, type) (type)
#define MONO_RET_LOCAL_Void(type)      			/* nothing */
#define MONO_RET_LOCAL_ICALL_HANDLES_WRAP_NONE(type) 	type result
#define MONO_RET_LOCAL_ICALL_HANDLES_WRAP_OBJ(type)  	type ## Handle result

#define MONO_RET_ASSIGN(type)                             MONO_HANDLES_DO (MONO_RET_ASSIGN_, type) (type)
#define MONO_RET_ASSIGN_Void(type)                        /* nothing */
#define MONO_RET_ASSIGN_ICALL_HANDLES_WRAP_NONE(type)	result =
#define MONO_RET_ASSIGN_ICALL_HANDLES_WRAP_OBJ(type)  	result =

// Marshal a parameter in.
#define MONO_MARSHAL_ARG_IN(type, n)					MONO_HANDLES_DO (MONO_MARSHAL_ARG_IN_, type) (type, n);
#define MONO_MARSHAL_ARG_IN_ICALL_HANDLES_WRAP_NONE(type, n)      	/* nothing */
#define MONO_MARSHAL_ARG_IN_ICALL_HANDLES_WRAP_OBJ(type, n)		a ## n = MONO_HANDLE_NEW (type, a ## n ## _raw)
#define MONO_MARSHAL_ARG_IN_ICALL_HANDLES_WRAP_OBJ_OUT(type, n)		a ## n = MONO_HANDLE_NEW (type, NULL)
#define MONO_MARSHAL_ARG_IN_ICALL_HANDLES_WRAP_OBJ_INOUT(type, n)	a ## n = MONO_HANDLE_NEW (type, *a ## n##_raw)
#define MONO_MARSHAL_ARG_IN_ICALL_HANDLES_WRAP_VALUETYPE_REF(type, n)	a ## n ## _interior_handle = mono_handle_new_interior (a ## n)

// Marshal a parameter out.
#define MONO_MARSHAL_ARG_OUT(type, n)					MONO_HANDLES_DO (MONO_MARSHAL_ARG_OUT_, type) (n);
#define MONO_MARSHAL_ARG_OUT_ICALL_HANDLES_WRAP_NONE(n)     		/* nothing */
#define MONO_MARSHAL_ARG_OUT_ICALL_HANDLES_WRAP_OBJ(n)      		/* nothing */
#define MONO_MARSHAL_ARG_OUT_ICALL_HANDLES_WRAP_VALUETYPE_REF(n)      	/* nothing */
#define MONO_MARSHAL_ARG_OUT_ICALL_HANDLES_WRAP_OBJ_OUT(n)		*a ## n##_raw = MONO_HANDLE_RAW (a ## n)
#define MONO_MARSHAL_ARG_OUT_ICALL_HANDLES_WRAP_OBJ_INOUT(n)		MONO_MARSHAL_ARG_OUT_ICALL_HANDLES_WRAP_OBJ_OUT (n)

// Type to return as a handle.
#define MONO_HANDLE_RET(type)					MONO_HANDLES_DO (MONO_HANDLE_RET_, type) (type)
#define MONO_HANDLE_RET_Void(type)				void
#define MONO_HANDLE_RET_ICALL_HANDLES_WRAP_NONE(type)		type
#define MONO_HANDLE_RET_ICALL_HANDLES_WRAP_OBJ(type)		type ## Handle
// WRAP_OBJ_OUT, WRAP_OBJ_INOUT, WRAP_VALUETYPE_REF not applicable to return values, only parameters.

// Type in handle function prototype.
#define MONO_HANDLE_ARG(type)					MONO_HANDLES_DO (MONO_HANDLE_ARG_, type) (type)
#define MONO_HANDLE_ARG_ICALL_HANDLES_WRAP_NONE(type)		type
#define MONO_HANDLE_ARG_ICALL_HANDLES_WRAP_OBJ(type)		type ## Handle
#define MONO_HANDLE_ARG_ICALL_HANDLES_WRAP_OBJ_OUT(type)	type ## Handle
#define MONO_HANDLE_ARG_ICALL_HANDLES_WRAP_OBJ_INOUT(type)	type ## Handle
#define MONO_HANDLE_ARG_ICALL_HANDLES_WRAP_VALUETYPE_REF(type)	type

// Type to return as a raw pointer (or value).
#define MONO_POINTER_RET(type)						MONO_HANDLES_DO (MONO_POINTER_RET_, type) (type)
#define MONO_POINTER_RET_Void(type)					type
#define MONO_POINTER_RET_ICALL_HANDLES_WRAP_NONE(type)			type
#define MONO_POINTER_RET_ICALL_HANDLES_WRAP_OBJ(type)			type*
// WRAP_OBJ_OUT, WRAP_OBJ_INOUT, WRAP_VALUETYPE_REF not applicable to return values, only parameters.

// Type/name in raw pointer prototype and implementation.
#define MONO_POINTER_ARG(type, n)					MONO_HANDLES_DO (MONO_POINTER_ARG_, type) (type, n)
#define MONO_POINTER_ARG_ICALL_HANDLES_WRAP_NONE(type, n)		type a ## n
#define MONO_POINTER_ARG_ICALL_HANDLES_WRAP_OBJ(type, n)		type* a ## n ## _raw
#define MONO_POINTER_ARG_ICALL_HANDLES_WRAP_OBJ_OUT(type, n)		type** a ## n ##_raw
#define MONO_POINTER_ARG_ICALL_HANDLES_WRAP_OBJ_INOUT(type, n)		type** a ## n ##_raw
#define MONO_POINTER_ARG_ICALL_HANDLES_WRAP_VALUETYPE_REF(type, n)	type a ## n

// Generate a parameter list for a function accepting/returning handles.
// FIXME variadic macros?
#define MONO_FOREACH_HANDLE_ARG_0()	   				/* nothing */
#define MONO_FOREACH_HANDLE_ARG(t)	   				MONO_HANDLE_ARG (t),
#define MONO_FOREACH_HANDLE_ARG_1(t0) 	   			      								 MONO_FOREACH_HANDLE_ARG (t0)
#define MONO_FOREACH_HANDLE_ARG_2(t0, t1)     			      	MONO_FOREACH_HANDLE_ARG_1 (t0) 				 MONO_FOREACH_HANDLE_ARG (t1)
#define MONO_FOREACH_HANDLE_ARG_3(t0, t1, t2)			      	MONO_FOREACH_HANDLE_ARG_2 (t0, t1)		         MONO_FOREACH_HANDLE_ARG (t2)
#define MONO_FOREACH_HANDLE_ARG_4(t0, t1, t2, t3)		      	MONO_FOREACH_HANDLE_ARG_3 (t0, t1, t2) 			 MONO_FOREACH_HANDLE_ARG (t3)
#define MONO_FOREACH_HANDLE_ARG_5(t0, t1, t2, t3, t4) 		      	MONO_FOREACH_HANDLE_ARG_4 (t0, t1, t2, t3) 		 MONO_FOREACH_HANDLE_ARG (t4)
#define MONO_FOREACH_HANDLE_ARG_6(t0, t1, t2, t3, t4, t5)             	MONO_FOREACH_HANDLE_ARG_5 (t0, t1, t2, t3, t4) 	         MONO_FOREACH_HANDLE_ARG (t5)
#define MONO_FOREACH_HANDLE_ARG_7(t0, t1, t2, t3, t4, t5, t6) 	      	MONO_FOREACH_HANDLE_ARG_6 (t0, t1, t2, t3, t4, t5) 	 MONO_FOREACH_HANDLE_ARG (t6)
#define MONO_FOREACH_HANDLE_ARG_8(t0, t1, t2, t3, t4, t5, t6, t7)     	MONO_FOREACH_HANDLE_ARG_7 (t0, t1, t2, t3, t4, t5, t6) 	 MONO_FOREACH_HANDLE_ARG (t7)
#define MONO_FOREACH_HANDLE_ARG_9(t0, t1, t2, t3, t4, t5, t6, t7, t8) 	MONO_FOREACH_HANDLE_ARG_8 (t0, t1, t2, t3, t4, t5, t6, t7) MONO_FOREACH_HANDLE_ARG (t8)

// Generate a parameter list for a function accepting/returning raw pointers.
#define MONO_FOREACH_RAWPROTO_0()		  			void
#define MONO_FOREACH_RAWPROTO_ARG(t, n)	  				MONO_POINTER_ARG (t, n)
#define MONO_FOREACH_RAWPROTO_1(t0) 	   	  										  MONO_FOREACH_RAWPROTO_ARG (t0, 0)
#define MONO_FOREACH_RAWPROTO_2(t0, t1)	  				MONO_FOREACH_RAWPROTO_1 (t0),             		  MONO_FOREACH_RAWPROTO_ARG (t1, 1)
#define MONO_FOREACH_RAWPROTO_3(t0, t1, t2)	  			MONO_FOREACH_RAWPROTO_2 (t0, t1),         		  MONO_FOREACH_RAWPROTO_ARG (t2, 2)
#define MONO_FOREACH_RAWPROTO_4(t0, t1, t2, t3)				MONO_FOREACH_RAWPROTO_3 (t0, t1, t2),     		  MONO_FOREACH_RAWPROTO_ARG (t3, 3)
#define MONO_FOREACH_RAWPROTO_5(t0, t1, t2, t3, t4)			MONO_FOREACH_RAWPROTO_4 (t0, t1, t2, t3), 		  MONO_FOREACH_RAWPROTO_ARG (t4, 4)
#define MONO_FOREACH_RAWPROTO_6(t0, t1, t2, t3, t4, t5)			MONO_FOREACH_RAWPROTO_5 (t0, t1, t2, t3, t4), 		  MONO_FOREACH_RAWPROTO_ARG (t5, 5)
#define MONO_FOREACH_RAWPROTO_7(t0, t1, t2, t3, t4, t5, t6)		MONO_FOREACH_RAWPROTO_6 (t0, t1, t2, t3, t4, t5), 	  MONO_FOREACH_RAWPROTO_ARG (t6, 6)
#define MONO_FOREACH_RAWPROTO_8(t0, t1, t2, t3, t4, t5, t6, t7)		MONO_FOREACH_RAWPROTO_7 (t0, t1, t2, t3, t4, t5, t6),	  MONO_FOREACH_RAWPROTO_ARG (t7, 7)
#define MONO_FOREACH_RAWPROTO_9(t0, t1, t2, t3, t4, t5, t6, t7, t8)	MONO_FOREACH_RAWPROTO_8 (t0, t1, t2, t3, t4, t5, t6, t7), MONO_FOREACH_RAWPROTO_ARG (t8, 8)

// From parameters compute if a wrapper function requires a coop handle frame.
// Return is figured in elsewhere using the same mechanism.
#define MONO_FOREACH_ANY_HANDLES_0() 					/* nothing */
#define MONO_FOREACH_ANY_HANDLES_1(t0) 	   				MONO_ANY_HANDLES (t0)
#define MONO_FOREACH_ANY_HANDLES_2(t0, t1) 				MONO_FOREACH_ANY_HANDLES_1 (t0)					MONO_FOREACH_ANY_HANDLES_1 (t1)
#define MONO_FOREACH_ANY_HANDLES_3(t0, t1, t2) 				MONO_FOREACH_ANY_HANDLES_2 (t0, t1) 	  			MONO_FOREACH_ANY_HANDLES_1 (t2)
#define MONO_FOREACH_ANY_HANDLES_4(t0, t1, t2, t3)			MONO_FOREACH_ANY_HANDLES_3 (t0, t1, t2)				MONO_FOREACH_ANY_HANDLES_1 (t3)
#define MONO_FOREACH_ANY_HANDLES_5(t0, t1, t2, t3, t4)			MONO_FOREACH_ANY_HANDLES_4 (t0, t1, t2, t3)			MONO_FOREACH_ANY_HANDLES_1 (t4)
#define MONO_FOREACH_ANY_HANDLES_6(t0, t1, t2, t3, t4, t5)		MONO_FOREACH_ANY_HANDLES_5 (t0, t1, t2, t3, t4) 		MONO_FOREACH_ANY_HANDLES_1 (t5)
#define MONO_FOREACH_ANY_HANDLES_7(t0, t1, t2, t3, t4, t5, t6) 		MONO_FOREACH_ANY_HANDLES_6 (t0, t1, t2, t3, t4, t5) 		MONO_FOREACH_ANY_HANDLES_1 (t6)
#define MONO_FOREACH_ANY_HANDLES_8(t0, t1, t2, t3, t4, t5, t6, t7)      MONO_FOREACH_ANY_HANDLES_7 (t0, t1, t2, t3, t4, t5, t6) 	MONO_FOREACH_ANY_HANDLES_1 (t7)
#define MONO_FOREACH_ANY_HANDLES_9(t0, t1, t2, t3, t4, t5, t6, t7, t8)  MONO_FOREACH_ANY_HANDLES_8 (t0, t1, t2, t3, t4, t5, t6, t7)	MONO_FOREACH_ANY_HANDLES_1 (t8)

// Generate locals to hold handles for raw pointer parameters.
// The return value is handled elsewhere.
#define MONO_FOREACH_ARG_LOCAL_0() 					/* nothing */
#define MONO_FOREACH_ARG_LOCAL_1(t0) 	   				MONO_ARG_LOCAL (t0, 0);
#define MONO_FOREACH_ARG_LOCAL_2(t0, t1) 				MONO_FOREACH_ARG_LOCAL_1 (t0)					MONO_ARG_LOCAL (t1, 1)
#define MONO_FOREACH_ARG_LOCAL_3(t0, t1, t2) 				MONO_FOREACH_ARG_LOCAL_2 (t0, t1) 				MONO_ARG_LOCAL (t2, 2)
#define MONO_FOREACH_ARG_LOCAL_4(t0, t1, t2, t3)			MONO_FOREACH_ARG_LOCAL_3 (t0, t1, t2)				MONO_ARG_LOCAL (t3, 3)
#define MONO_FOREACH_ARG_LOCAL_5(t0, t1, t2, t3, t4)			MONO_FOREACH_ARG_LOCAL_4 (t0, t1, t2, t3) 			MONO_ARG_LOCAL (t4, 4)
#define MONO_FOREACH_ARG_LOCAL_6(t0, t1, t2, t3, t4, t5) 		MONO_FOREACH_ARG_LOCAL_5 (t0, t1, t2, t3, t4) 			MONO_ARG_LOCAL (t5, 5)
#define MONO_FOREACH_ARG_LOCAL_7(t0, t1, t2, t3, t4, t5, t6) 		MONO_FOREACH_ARG_LOCAL_6 (t0, t1, t2, t3, t4, t5) 		MONO_ARG_LOCAL (t6, 6)
#define MONO_FOREACH_ARG_LOCAL_8(t0, t1, t2, t3, t4, t5, t6, t7)	MONO_FOREACH_ARG_LOCAL_7 (t0, t1, t2, t3, t4, t5, t6) 		MONO_ARG_LOCAL (t7, 7)
#define MONO_FOREACH_ARG_LOCAL_9(t0, t1, t2, t3, t4, t5, t6, t7, t8)	MONO_FOREACH_ARG_LOCAL_8 (t0, t1, t2, t3, t4, t5, t6, t7)	MONO_ARG_LOCAL (t8, 8)

// Marshal pointers to handles:
// nothing for values
// initialize a handle in or in/out
// initialize a null hande out
#define MONO_FOREACH_MARSHAL_ARG_IN_0() 					/* nothing */
#define MONO_FOREACH_MARSHAL_ARG_IN_ARG(t, n)					MONO_MARSHAL_ARG_IN (t, n)
#define MONO_FOREACH_MARSHAL_ARG_IN_1(t0) 	   				MONO_FOREACH_MARSHAL_ARG_IN_ARG (t0, 0)
#define MONO_FOREACH_MARSHAL_ARG_IN_2(t0, t1) 					MONO_FOREACH_MARSHAL_ARG_IN_1 (t0)				MONO_FOREACH_MARSHAL_ARG_IN_ARG (t1, 1)
#define MONO_FOREACH_MARSHAL_ARG_IN_3(t0, t1, t2) 				MONO_FOREACH_MARSHAL_ARG_IN_2 (t0, t1) 				MONO_FOREACH_MARSHAL_ARG_IN_ARG (t2, 2)
#define MONO_FOREACH_MARSHAL_ARG_IN_4(t0, t1, t2, t3) 				MONO_FOREACH_MARSHAL_ARG_IN_3 (t0, t1, t2)			MONO_FOREACH_MARSHAL_ARG_IN_ARG (t3, 3)
#define MONO_FOREACH_MARSHAL_ARG_IN_5(t0, t1, t2, t3, t4)			MONO_FOREACH_MARSHAL_ARG_IN_4 (t0, t1, t2, t3)			MONO_FOREACH_MARSHAL_ARG_IN_ARG (t4, 4)
#define MONO_FOREACH_MARSHAL_ARG_IN_6(t0, t1, t2, t3, t4, t5)			MONO_FOREACH_MARSHAL_ARG_IN_5 (t0, t1, t2, t3, t4) 		MONO_FOREACH_MARSHAL_ARG_IN_ARG (t5, 5)
#define MONO_FOREACH_MARSHAL_ARG_IN_7(t0, t1, t2, t3, t4, t5, t6)		MONO_FOREACH_MARSHAL_ARG_IN_6 (t0, t1, t2, t3, t4, t5) 		MONO_FOREACH_MARSHAL_ARG_IN_ARG (t6, 6)
#define MONO_FOREACH_MARSHAL_ARG_IN_8(t0, t1, t2, t3, t4, t5, t6, t7)		MONO_FOREACH_MARSHAL_ARG_IN_7 (t0, t1, t2, t3, t4, t5, t6)	MONO_FOREACH_MARSHAL_ARG_IN_ARG (t7, 7)
#define MONO_FOREACH_MARSHAL_ARG_IN_9(t0, t1, t2, t3, t4, t5, t6, t7, t8)	MONO_FOREACH_MARSHAL_ARG_IN_8 (t0, t1, t2, t3, t4, t5, t6, t7)	MONO_FOREACH_MARSHAL_ARG_IN_ARG (t8, 8)

// Marshal pointers to handles:
// nothing for most parameters
// copy back an out or inout handle
#define MONO_FOREACH_MARSHAL_ARG_OUT_0() 					/* nothing */
#define MONO_FOREACH_MARSHAL_ARG_OUT(t, n)					MONO_MARSHAL_ARG_OUT (t, n)
#define MONO_FOREACH_MARSHAL_ARG_OUT_1(t0) 	  				MONO_FOREACH_MARSHAL_ARG_OUT (t0, 0);
#define MONO_FOREACH_MARSHAL_ARG_OUT_2(t0, t1) 					MONO_FOREACH_MARSHAL_ARG_OUT_1 (t0) 				MONO_FOREACH_MARSHAL_ARG_OUT (t1, 1)
#define MONO_FOREACH_MARSHAL_ARG_OUT_3(t0, t1, t2) 				MONO_FOREACH_MARSHAL_ARG_OUT_2 (t0, t1) 			MONO_FOREACH_MARSHAL_ARG_OUT (t2, 2)
#define MONO_FOREACH_MARSHAL_ARG_OUT_4(t0, t1, t2, t3) 				MONO_FOREACH_MARSHAL_ARG_OUT_3 (t0, t1, t2) 			MONO_FOREACH_MARSHAL_ARG_OUT (t3, 3)
#define MONO_FOREACH_MARSHAL_ARG_OUT_5(t0, t1, t2, t3, t4)			MONO_FOREACH_MARSHAL_ARG_OUT_4 (t0, t1, t2, t3) 		MONO_FOREACH_MARSHAL_ARG_OUT (t4, 4)
#define MONO_FOREACH_MARSHAL_ARG_OUT_6(t0, t1, t2, t3, t4, t5)			MONO_FOREACH_MARSHAL_ARG_OUT_5 (t0, t1, t2, t3, t4) 		MONO_FOREACH_MARSHAL_ARG_OUT (t5, 5)
#define MONO_FOREACH_MARSHAL_ARG_OUT_7(t0, t1, t2, t3, t4, t5, t6)		MONO_FOREACH_MARSHAL_ARG_OUT_6 (t0, t1, t2, t3, t4, t5) 	MONO_FOREACH_MARSHAL_ARG_OUT (t6, 6)
#define MONO_FOREACH_MARSHAL_ARG_OUT_8(t0, t1, t2, t3, t4, t5, t6, t7)		MONO_FOREACH_MARSHAL_ARG_OUT_7 (t0, t1, t2, t3, t4, t5, t6) 	MONO_FOREACH_MARSHAL_ARG_OUT (t7, 7)
#define MONO_FOREACH_MARSHAL_ARG_OUT_9(t0, t1, t2, t3, t4, t5, t6, t7, t8)	MONO_FOREACH_MARSHAL_ARG_OUT_8 (t0, t1, t2, t3, t4, t5, t6, t7) MONO_FOREACH_MARSHAL_ARG_OUT (t8, 8)

// Call from the wrapper to the actual icall, passing on the
// WRAP_NONE parameters directly, local handles, and the MonoError.
//
// Parameters and local variables are carefully named a0 or a0_raw
// or a0_interior_handle, in order for a0 to be what is passed on.
//
// That is, WRAP_NONE parameter is a0 and there is no local.
// WRAP_OBJ, OUT, INOUT parameter is a0_raw, local is a0.
// VALUETYPE_REF parameter is a0, local is a0_interior_handle
#define MONO_HANDLES_CALL_0	error
#define MONO_HANDLES_CALL_1	a0, error
#define MONO_HANDLES_CALL_2	a0, a1, error
#define MONO_HANDLES_CALL_3	a0, a1, a2, error
#define MONO_HANDLES_CALL_4	a0, a1, a2, a3, error
#define MONO_HANDLES_CALL_5	a0, a1, a2, a3, a4, error
#define MONO_HANDLES_CALL_6	a0, a1, a2, a3, a4, a5, error
#define MONO_HANDLES_CALL_7	a0, a1, a2, a3, a4, a5, a6, error
#define MONO_HANDLES_CALL_8	a0, a1, a2, a3, a4, a5, a6, a7, error
#define MONO_HANDLES_CALL_9	a0, a1, a2, a3, a4, a5, a6, a7, a8, error

// Declare the function that takes/returns handles.
#define MONO_HANDLE_DECLARE(id, name, func, rettype, n, argtypes)	\
MONO_HANDLE_RET (rettype)					\
func (MONO_FOREACH_HANDLE_ARG_ ## n argtypes MonoError *error)	\

// Declare the function wrapper that takes/returns raw pointers.
#define MONO_HANDLE_DECLARE_RAW(id, name, func, rettype, n, argtypes)	\
ICALL_EXPORT MONO_POINTER_RET (rettype)				\
func##_raw ( MONO_FOREACH_RAWPROTO_ ## n argtypes )		\

// Implement ves_icall_foo_raw over ves_icall_foo
// Parameters and return value are wrapped as handles, if needed.
// MonoError parameter is added and checked.

#define MONO_HANDLE_IMPLEMENT_MAYBE(cond, id, name, func, rettype, n, argtypes)		\
											\
MONO_HANDLE_DECLARE_RAW (id, name, func, rettype, n, argtypes)				\
{											\
	if (!cond) 									\
		g_assert_not_reached ();						\
											\
	ERROR_DECL (error);								\
	MONO_FOREACH_ARG_LOCAL_ ## n argtypes;						\
	MONO_RET_LOCAL (rettype);							\
											\
	if (0 MONO_ANY_HANDLES (rettype) MONO_FOREACH_ANY_HANDLES_ ## n argtypes) { 	\
											\
		HANDLE_FUNCTION_ENTER(); 						\
											\
		MONO_FOREACH_MARSHAL_ARG_IN_ ## n argtypes				\
											\
		MONO_RET_ASSIGN (rettype)						\
											\
		func (MONO_HANDLES_CALL_ ## n);						\
											\
		MONO_FOREACH_MARSHAL_ARG_OUT_ ## n argtypes				\
											\
		mono_error_set_pending_exception (error);				\
											\
		MONO_ANY_HANDLES_RETURN (rettype); 					\
	}										\
											\
	MONO_RET_ASSIGN (rettype)							\
											\
	func (MONO_HANDLES_CALL_ ## n);							\
											\
	mono_error_set_pending_exception (error);					\
											\
	MONO_NO_HANDLES_RETURN (rettype); 						\
}											\

#define MONO_HANDLE_IMPLEMENT(id, name, func, rettype, n, argtypes)			\
	MONO_HANDLE_IMPLEMENT_MAYBE (TRUE, id, name, func, rettype, n, argtypes)

#endif
