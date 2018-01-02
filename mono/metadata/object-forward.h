/**
 * \file
 *
 * Forward declarations of opaque types, and typedefs thereof.
 *
 */

#ifndef __MONO_OBJECT_FORWARD_H__
#define __MONO_OBJECT_FORWARD_H__

#include <mono/utils/mono-publib.h>

typedef struct _MonoReflectionTypeBuilder MonoReflectionTypeBuilder;

struct _MonoException;
typedef struct _MonoException MONO_RT_MANAGED_ATTR MonoException;

// Move out of necessity? from handle.h.
/* TYPED_HANDLE_DECL(SomeType):
 *   Expands to a decl for handles to SomeType and to an internal payload struct.
 *
 * For example, TYPED_HANDLE_DECL(MonoObject) (see below) expands to:
 *
 * typedef struct {
 *   MonoObject *__raw;
 * } MonoObjectHandlePayload;
 *
 * typedef MonoObjectHandlePayload* MonoObjectHandle;
 * typedef MonoObjectHandlePayload* MonoObjectHandleOut;
 */

#define TYPED_HANDLE_PAYLOAD_NAME(TYPE) TYPE ## HandlePayload
#define TYPED_HANDLE_NAME(TYPE) TYPE ## Handle
#define TYPED_OUT_HANDLE_NAME(TYPE) TYPE ## HandleOut

#define TYPED_HANDLE_DECL(TYPE)						\
	typedef struct { TYPE *__raw; } TYPED_HANDLE_PAYLOAD_NAME (TYPE) ; \
	typedef TYPED_HANDLE_PAYLOAD_NAME (TYPE) * TYPED_HANDLE_NAME (TYPE); \
	typedef TYPED_HANDLE_PAYLOAD_NAME (TYPE) * TYPED_OUT_HANDLE_NAME (TYPE)

TYPED_HANDLE_DECL (MonoException);

#endif /* __MONO_OBJECT_FORWARD_H__ */
