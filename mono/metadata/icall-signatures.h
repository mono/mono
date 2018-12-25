/**
 * \file
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 *
 * Previously JIT icall signatures were presented as strings,
 * subject to parsing and hashing and allocations and locking.
 * Here they are statically allocated and almost statically initialized.
 * There is no parsing, locking, or hashing.
 */
#ifndef __MONO_METADATA_ICALL_SIGNATURES_H__
#define __MONO_METADATA_ICALL_SIGNATURES_H__

// FIXME Some of these are only needed under ifdef.
// FIXME Some of these are redundant like obj vs. object, int vs. int32.
//
// count and types, where first type is return type
// and the rest are the parameter types.
//
// There are tradeoffs either way as to if return
// type is part of the parameter list.
//
#define ICALL_SIGS				\
ICALL_SIG (1, (object))				\
ICALL_SIG (1, (ptr))				\
ICALL_SIG (1, (void))				\
ICALL_SIG (2, (double, double))			\
ICALL_SIG (2, (double, int32))			\
ICALL_SIG (2, (double, long))			\
ICALL_SIG (2, (double, ptr))			\
ICALL_SIG (2, (float, long))			\
ICALL_SIG (2, (int, obj))			\
ICALL_SIG (2, (int16, double))			\
ICALL_SIG (2, (int32, double))			\
ICALL_SIG (2, (int32, obj))			\
ICALL_SIG (2, (int32, object))			\
ICALL_SIG (2, (int8, double))			\
ICALL_SIG (2, (long, double))			\
ICALL_SIG (2, (long, float))			\
ICALL_SIG (2, (obj, ptr))			\
ICALL_SIG (2, (object, int))			\
ICALL_SIG (2, (object, int32))			\
ICALL_SIG (2, (object, object))			\
ICALL_SIG (2, (object, ptr))			\
ICALL_SIG (2, (ptr, int))			\
ICALL_SIG (2, (ptr, obj))			\
ICALL_SIG (2, (ptr, object))			\
ICALL_SIG (2, (ptr, ptr))			\
ICALL_SIG (2, (uint16, double))			\
ICALL_SIG (2, (uint32, double))			\
ICALL_SIG (2, (uint8, double))			\
ICALL_SIG (2, (ulong, double))			\
ICALL_SIG (2, (ulong, float))			\
ICALL_SIG (2, (void, int))			\
ICALL_SIG (2, (void, int32))			\
ICALL_SIG (2, (void, object))			\
ICALL_SIG (2, (void, ptr))			\
ICALL_SIG (3, (bool, ptr, ptrref))		\
ICALL_SIG (3, (double, double, double))		\
ICALL_SIG (3, (float, float, float))		\
ICALL_SIG (3, (int, obj, ptr))			\
ICALL_SIG (3, (int32, int32, int32))		\
ICALL_SIG (3, (int32, int32, ptr))		\
ICALL_SIG (3, (int32, int32, ptrref))		\
ICALL_SIG (3, (int32, ptr, ptr))		\
ICALL_SIG (3, (int32, ptr, ptrref))		\
ICALL_SIG (3, (long, long, int32))		\
ICALL_SIG (3, (long, long, long))		\
ICALL_SIG (3, (obj, ptr, int))			\
ICALL_SIG (3, (object, int, object))		\
ICALL_SIG (3, (object, object, ptr))		\
ICALL_SIG (3, (object, ptr, int))		\
ICALL_SIG (3, (object, ptr, int32))		\
ICALL_SIG (3, (object, ptr, ptr))		\
ICALL_SIG (3, (ptr, int32, ptrref))		\
ICALL_SIG (3, (ptr, object, ptr))		\
ICALL_SIG (3, (ptr, ptr, int))			\
ICALL_SIG (3, (ptr, ptr, int32))		\
ICALL_SIG (3, (ptr, ptr, ptr))			\
ICALL_SIG (3, (ptr, ptr, ptrref))		\
ICALL_SIG (3, (ptr, uint32, ptrref))		\
ICALL_SIG (3, (uint32, double, double))		\
ICALL_SIG (3, (uint32, ptr, int32))		\
ICALL_SIG (3, (void, double, ptr))		\
ICALL_SIG (3, (void, int32, ptrref))		\
ICALL_SIG (3, (void, obj, ptr))			\
ICALL_SIG (3, (void, object, object))		\
ICALL_SIG (3, (void, object, ptr))		\
ICALL_SIG (3, (void, ptr, int))			\
ICALL_SIG (3, (void, ptr, int32))		\
ICALL_SIG (3, (void, ptr, object))		\
ICALL_SIG (3, (void, ptr, ptr))			\
ICALL_SIG (3, (void, ptr, ptrref))		\
ICALL_SIG (3, (void, uint32, ptrref))		\
ICALL_SIG (4, (bool, ptr, int32, ptrref))	\
ICALL_SIG (4, (int32, int32, ptr, ptrref))	\
ICALL_SIG (4, (int32, ptr, int32, ptr))		\
ICALL_SIG (4, (int32, ptr, int32, ptrref))	\
ICALL_SIG (4, (object, int, object, object))	\
ICALL_SIG (4, (object, object, ptr, ptr))	\
ICALL_SIG (4, (object, ptr, int, int))		\
ICALL_SIG (4, (object, ptr, int, int32))	\
ICALL_SIG (4, (object, ptr, ptr, int32))	\
ICALL_SIG (4, (ptr, object, int32, int32))	\
ICALL_SIG (4, (ptr, object, ptr, ptr))		\
ICALL_SIG (4, (ptr, ptr, int, ptr))		\
ICALL_SIG (4, (ptr, ptr, int32, ptrref))	\
ICALL_SIG (4, (ptr, ptr, ptr, ptr))		\
ICALL_SIG (4, (ptr, ptr, ptr, ptrref))		\
ICALL_SIG (4, (ptr, ptr, uint32, ptrref))	\
ICALL_SIG (4, (void, object, object, ptr))	\
ICALL_SIG (4, (void, object, ptr, int32))	\
ICALL_SIG (4, (void, ptr, int, object))		\
ICALL_SIG (4, (void, ptr, int, ptr))		\
ICALL_SIG (4, (void, ptr, object, int32))	\
ICALL_SIG (4, (void, ptr, ptr, int))		\
ICALL_SIG (4, (void, ptr, ptr, int32))		\
ICALL_SIG (4, (void, ptr, ptr, ptr))		\
ICALL_SIG (4, (void, ptr, ptr, ptrref))	\
ICALL_SIG (5, (int32, object, ptr, ptr, ptr)) 	\
ICALL_SIG (5, (object, object, ptr, ptr, ptr))	\
ICALL_SIG (5, (object, ptr, int, int, int))	\
ICALL_SIG (5, (ptr, object, int, ptr, ptr))	\
ICALL_SIG (5, (ptr, ptr, int32, ptr, ptrref))	\
ICALL_SIG (5, (ptr, ptr, ptr, int32, ptrref))	\
ICALL_SIG (5, (ptr, ptr, ptr, ptr, ptrref))	\
ICALL_SIG (5, (ptr, ptr, ptr, ptrref, ptrref))	\
ICALL_SIG (5, (void, object, ptr, int32, int32)) 	\
ICALL_SIG (5, (void, object, ptr, ptr, ptr))		\
ICALL_SIG (5, (void, ptr, int, int, object))		\
ICALL_SIG (6, (int, int, int, ptr, ptr, ptr))		\
ICALL_SIG (6, (int, ptr, int, int, ptr, object))	\
ICALL_SIG (6, (object, ptr, int, int, int, int))	\
ICALL_SIG (6, (object, ptr, ptr, ptr, ptr, ptr))	\
ICALL_SIG (6, (ptr, ptr, int32, ptr, ptr, ptrref))	\
ICALL_SIG (6, (void, ptr, ptr, int32, ptr, ptrref))	\
ICALL_SIG (7, (int32, ptr, ptr, ptr, ptr, ptr, int32))	\
ICALL_SIG (8, (void, ptr, ptr, int32, ptr, ptrref, ptr, ptrref)) 	\

#define ICALL_SIG_NAME_1(a) 		 	 mono_icall_sig_ ## a
#define ICALL_SIG_NAME_2(a, b) 		 	 mono_icall_sig_ ## a ## _ ## b
#define ICALL_SIG_NAME_3(a, b, c) 	 	 mono_icall_sig_ ## a ## _ ## b ## _ ## c
#define ICALL_SIG_NAME_4(a, b, c, d) 	 	 mono_icall_sig_ ## a ## _ ## b ## _ ## c ## _ ## d
#define ICALL_SIG_NAME_5(a, b, c, d, e)    	 mono_icall_sig_ ## a ## _ ## b ## _ ## c ## _ ## d ## _ ## e
#define ICALL_SIG_NAME_6(a, b, c, d, e, f) 	 mono_icall_sig_ ## a ## _ ## b ## _ ## c ## _ ## d ## _ ## e ## _ ## f
#define ICALL_SIG_NAME_7(a, b, c, d, e, f, g)	 mono_icall_sig_ ## a ## _ ## b ## _ ## c ## _ ## d ## _ ## e ## _ ## f ## _ ## g
#define ICALL_SIG_NAME_8(a, b, c, d, e, f, g, h) mono_icall_sig_ ## a ## _ ## b ## _ ## c ## _ ## d ## _ ## e ## _ ## f ## _ ## g ## _ ## h

#define ICALL_SIG_NAME(n, types) ICALL_SIG_NAME_ ## n types
#define ICALL_SIG(n, types) extern MonoMethodSignature * const ICALL_SIG_NAME (n, types);

ICALL_SIGS

#undef ICALL_SIG

void
mono_create_icall_signatures (void);

#endif // __MONO_METADATA_ICALL_SIGNATURES_H__
