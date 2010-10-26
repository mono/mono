#ifndef MONOTYPES_H
#define MONOTYPES_H

typedef signed long SInt32;
typedef unsigned long UInt32;
typedef signed short SInt16;
typedef unsigned short UInt16;
typedef unsigned char UInt8;
typedef signed char SInt8;
typedef unsigned long long UInt64;
typedef signed long long SInt64;

struct MonoException;
struct MonoAssembly;
struct MonoObject;
struct MonoClassField;
struct MonoClass;
struct MonoDomain;
struct MonoImage;
struct MonoType;
struct MonoMethodSignature;
struct MonoArray;
struct MonoThread;
struct MonoVTable;
struct MonoProperty;
struct MonoReflectionAssembly;
struct MonoReflectionMethod;
struct MonoAppDomain;
struct MonoCustomAttrInfo;

struct MonoReflectionType { UInt32 offset[2]; MonoType* type;};

typedef const void* gconstpointer;
typedef void* gpointer;
typedef int gboolean;
typedef unsigned int guint32;
typedef int gint32;
typedef long gint64;
typedef unsigned char   guchar;
typedef UInt16 gunichar2;
struct MonoString 
{
	gint32 monoObjectPart1;
	gint32 monoObjectPart2;
	gint32 length;
	gunichar2 firstCharacter;
};

struct MonoMethod {
	UInt16 flags;
	UInt16 iflags;
};

typedef enum
{
	MONO_VERIFIER_MODE_OFF,
	MONO_VERIFIER_MODE_VALID,
	MONO_VERIFIER_MODE_VERIFIABLE,
	MONO_VERIFIER_MODE_STRICT
} MiniVerifierMode;

typedef enum {
	MONO_SECURITY_MODE_NONE,
	MONO_SECURITY_MODE_CORE_CLR,
	MONO_SECURITY_MODE_CAS,
	MONO_SECURITY_MODE_SMCS_HACK
} MonoSecurityMode;

typedef void GFuncRef (void*, void*);
typedef GFuncRef* GFunc;

typedef enum {
	MONO_UNHANDLED_POLICY_LEGACY,
	MONO_UNHANDLED_POLICY_CURRENT
} MonoRuntimeUnhandledExceptionPolicy;

struct MonoMethodDesc {
	char *namespace2;
	char *klass;
	char *name;
	char *args1;
	UInt32 num_args;
	gboolean include_namespace, klass_glob, name_glob;
};

#endif
