#ifndef __IL2CPP_MONO_DEBUGGER_OPAQUE_TYPES_H__
#define __IL2CPP_MONO_DEBUGGER_OPAQUE_TYPES_H__

#if defined(RUNTIME_IL2CPP)
#include "il2cpp-class-internals.h"
#include "il2cpp-object-internals.h"
#include "vm-utils/Debugger.h"
#endif // RUNTIME_IL2CPP

#define IL2CPP_MONO_PUBLIC_KEY_TOKEN_LENGTH	17

//Converted to il2cpp types
#define MonoType Il2CppType
#define MonoClass Il2CppClass
#define MonoImage Il2CppImage
#define MonoMethod MethodInfo
#define MonoClassField FieldInfo
#define MonoArrayType Il2CppArrayType
#define MonoGenericParam Il2CppGenericParameter
#define MonoGenericInst Il2CppGenericInst
#define MonoGenericContext Il2CppGenericContext
#define MonoGenericClass Il2CppGenericClass
#define MonoGenericContainer Il2CppGenericContainer
#define MonoProperty PropertyInfo
#define MonoString Il2CppString
#define MonoArray Il2CppArraySize
#define MonoThread Il2CppThread
#define MonoInternalThread Il2CppInternalThread
#define MonoReflectionType Il2CppReflectionType
#define MonoProfiler Il2CppProfiler
#define MonoAssembly Il2CppAssembly
#define MonoAssembyName Il2CppAssemblyName
#define MonoMethodHeader Il2CppMethodHeaderInfo
#define MonoReflectionAssembly Il2CppReflectionAssembly
#define MonoAppDomain Il2CppAppDomain
#define MonoDomain Il2CppDomain
#define MonoDomainFunc Il2CppDomainFunc

//still stubs everywhere
typedef struct _Il2CppMonoMethodSignature Il2CppMonoMethodSignature;
typedef struct _Il2CppMonoVTable Il2CppMonoVTable;
typedef struct _Il2CppMonoMarshalByRefObject Il2CppMonoMarshalByRefObject;
typedef struct _Il2CppMonoObject Il2CppMonoObject;
typedef struct _Il2CppMonoCustomAttrInfo Il2CppMonoCustomAttrInfo;
typedef struct _Il2CppMonoJitTlsData Il2CppMonoJitTlsData;
typedef struct _Il2CppMonoRuntimeExceptionHandlingCallbacks Il2CppMonoRuntimeExceptionHandlingCallbacks;
typedef struct _Il2CppMonoCustomAttrEntry Il2CppMonoCustomAttrEntry;
typedef struct _Il2CppMonoStackFrameInfo Il2CppMonoStackFrameInfo;
typedef struct Il2CppDefaults Il2CppMonoDefaults;
typedef struct _Il2CppMonoMethodInflated Il2CppMonoMethodInflated;
typedef struct _Il2CppMonoException Il2CppMonoException;
typedef struct _Il2CppCattrNamedArg Il2CppCattrNamedArg;
typedef struct _Il2CppMonoTypeNameParse Il2CppMonoTypeNameParse;


struct _Il2CppMonoJitTlsData { void *dummy; };

struct _Il2CppCattrNamedArg
{
	MonoType *type;
	MonoClassField *field;
	MonoProperty *prop;
};

struct _Il2CppMonoObject
{
	Il2CppMonoVTable *vtable;
	void *synchronization;
};

struct _Il2CppMonoException
{
	Il2CppMonoObject object;
};

struct _Il2CppMonoMethodInflated
{
	MonoMethod *declaring;
	MonoGenericContext context;
};

struct _Il2CppMonoStackFrameInfo
{
	MonoStackFrameType type;
	MonoJitInfo *ji;
	MonoMethod *method;
	MonoMethod *actual_method;
	MonoDomain *domain;
	gboolean managed;
	gboolean async_context;
	int native_offset;
	int il_offset;
	gpointer interp_exit_data;
	gpointer interp_frame;
	gpointer lmf;
	guint32 unwind_info_len;
	guint8 *unwind_info;
	mgreg_t **reg_locations;
};

struct _Il2CppMonoCustomAttrEntry
{
	MonoMethod *ctor;
	uint32_t data_size;
	const mono_byte* data;
};

struct _Il2CppMonoCustomAttrInfo
{
	int num_attrs;
	Il2CppMonoCustomAttrEntry attrs [MONO_ZERO_LEN_ARRAY];
};

typedef gboolean (*Il2CppMonoInternalStackWalk) (Il2CppMonoStackFrameInfo *frame, MonoContext *ctx, gpointer data);

struct _Il2CppMonoRuntimeExceptionHandlingCallbacks
{
	void (*il2cpp_mono_walk_stack_with_state) (Il2CppMonoInternalStackWalk func, MonoThreadUnwindState *state, MonoUnwindOptions options, void *user_data);
};

struct _Il2CppMonoMarshalByRefObject
{
	Il2CppMonoObject obj;
};

struct _Il2CppMonoVTable
{
	MonoClass *klass;
	MonoDomain *domain;
	guint8 initialized;
	gpointer type;
	guint init_failed     : 1;
};

struct _Il2CppMonoMethodSignature
{
	MonoType *ret;
	guint16 param_count;
	unsigned int generic_param_count : 16;
	unsigned int  call_convention     : 6;
	unsigned int  hasthis             : 1;
	MonoType **params;
};

struct _Il2CppMonoTypeNameParse
{
	MonoAssemblyName assembly;
	void *il2cppTypeNameParseInfo;
};

typedef struct Il2CppThreadUnwindState
{
	Il2CppSequencePoint** sequencePoints;
	Il2CppSequencePointExecutionContext** executionContexts;
	uint32_t frameCount;
} Il2CppThreadUnwindState;

TYPED_HANDLE_DECL (Il2CppMonoObject);
TYPED_HANDLE_DECL (MonoReflectionAssembly);
Il2CppMonoDefaults il2cpp_mono_defaults;
MonoDebugOptions il2cpp_mono_debug_options;

typedef void (*Il2CppMonoProfileFunc) (MonoProfiler *prof);
typedef void (*Il2CppMonoProfileAppDomainFunc) (MonoProfiler *prof, MonoDomain *domain);
typedef void (*Il2CppMonoProfileAppDomainResult) (MonoProfiler *prof, MonoDomain *domain, int result);
typedef void (*Il2CppMonoProfileAssemblyFunc) (MonoProfiler *prof, MonoAssembly *assembly);
typedef void (*Il2CppMonoProfileJitResult) (MonoProfiler *prof, MonoMethod *method, MonoJitInfo* jinfo, int result);
typedef void (*Il2CppMonoProfileAssemblyResult) (MonoProfiler *prof, MonoAssembly *assembly, int result);
typedef void (*Il2CppMonoProfileThreadFunc) (MonoProfiler *prof, uintptr_t tid);
typedef gboolean (*Il2CppMonoJitStackWalk) (Il2CppMonoStackFrameInfo *frame, MonoContext *ctx, gpointer data);
typedef void (*Il2CppDomainFunc) (MonoDomain *domain, void* user_data);

typedef void (*emit_assembly_load_callback)(void*, void*);
typedef void(*emit_type_load_callback)(void*, void*, void*);

void il2cpp_set_thread_state_background(MonoThread* thread);
void* il2cpp_domain_get_agent_info(MonoAppDomain* domain);
void il2cpp_domain_set_agent_info(MonoAppDomain* domain, void* agentInfo);
void il2cpp_start_debugger_thread();
void* il2cpp_gc_alloc_fixed(size_t size);
void il2cpp_gc_free_fixed(void* address);
const char* il2cpp_domain_get_name(MonoDomain* domain);

#endif