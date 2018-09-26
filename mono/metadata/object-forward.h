/**
 * \file
 *
 * Forward declarations of opaque types, and typedefs thereof.
 *
 */

#ifndef __MONO_OBJECT_FORWARD_H__
#define __MONO_OBJECT_FORWARD_H__

#include <mono/utils/mono-publib.h>

typedef struct _MonoClass MonoClass;
typedef struct _MonoImage MonoImage;
typedef struct _MonoMethod MonoMethod;

typedef struct MonoObject MONO_RT_MANAGED_ATTR MonoObject;
typedef struct MonoException MONO_RT_MANAGED_ATTR MonoException;
typedef struct _MonoMList MONO_RT_MANAGED_ATTR MonoMList;
typedef struct MonoReflectionAssembly MONO_RT_MANAGED_ATTR MonoReflectionAssembly;
typedef struct MonoReflectionTypeBuilder MONO_RT_MANAGED_ATTR MonoReflectionTypeBuilder;

#endif /* __MONO_OBJECT_FORWARD_H__ */
