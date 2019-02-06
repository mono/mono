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
#define MonoMethodInflated MethodInfo
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
#define MonoMethodHeader Il2CppMonoMethodHeader
//#define MonoDebugLocalsInfo Il2CppDebugLocalsInfo
#define MonoReflectionAssembly Il2CppReflectionAssembly
#define MonoAppDomain Il2CppAppDomain
#define MonoDomain Il2CppDomain
#define MonoDomainFunc Il2CppDomainFunc
#define MonoObject Il2CppObject
#define MonoVTable Il2CppVTable
#define MonoException Il2CppException
#define MonoMarshalByRefObject Il2CppMarshalByRefObject

//Unsupported in il2cpp, should never be referenced
#define MonoCustomAttrInfo #error Custom Attributes Not Supported
#define MonoCustomAttrEntry #error Custom Attributes Not Supported
#define CattrNamedArg #error Custom Attributes Not Supported
typedef gpointer MonoJitTlsData;
typedef gpointer MonoInterpStackIter;

//still stubs everywhere
typedef struct _Il2CppMonoMethodSignature Il2CppMonoMethodSignature;
typedef struct _Il2CppMonoRuntimeExceptionHandlingCallbacks Il2CppMonoRuntimeExceptionHandlingCallbacks;
typedef struct Il2CppDefaults Il2CppMonoDefaults;
typedef struct _Il2CppMonoTypeNameParse Il2CppMonoTypeNameParse;
typedef struct _Il2CppMonoDebugOptions Il2CppMonoDebugOptions;
typedef struct _Il2CppEmptyStruct Il2CppMonoLMF;


typedef struct _MonoDebugLocalsInfo		MonoDebugLocalsInfo;

typedef MonoStackFrameInfo StackFrameInfo;
typedef gpointer MonoInterpFrameHandle;


typedef struct _Il2CppInterpCallbacks MonoInterpCallbacks;

struct _Il2CppInterpCallbacks {
	gpointer (*create_method_pointer) (MonoMethod *method, MonoError *error);
	MonoObject* (*runtime_invoke) (MonoMethod *method, void *obj, void **params, MonoObject **exc, MonoError *error);
	void (*init_delegate) (MonoDelegate *del);
#ifndef DISABLE_REMOTING
	gpointer (*get_remoting_invoke) (gpointer imethod, MonoError *error);
#endif
	gpointer (*create_trampoline) (MonoDomain *domain, MonoMethod *method, MonoError *error);
	void (*walk_stack_with_ctx) (MonoInternalStackWalk func, MonoContext *ctx, MonoUnwindOptions options, void *user_data);
	void (*set_resume_state) (MonoJitTlsData *jit_tls, MonoException *ex, MonoJitExceptionInfo *ei, MonoInterpFrameHandle interp_frame, gpointer handler_ip);
	gboolean (*run_finally) (StackFrameInfo *frame, int clause_index, gpointer handler_ip);
	gboolean (*run_filter) (StackFrameInfo *frame, MonoException *ex, int clause_index, gpointer handler_ip);
	void (*frame_iter_init) (MonoInterpStackIter *iter, gpointer interp_exit_data);
	gboolean (*frame_iter_next) (MonoInterpStackIter *iter, StackFrameInfo *frame);
	MonoJitInfo* (*find_jit_info) (MonoDomain *domain, MonoMethod *method);
	void (*set_breakpoint) (MonoJitInfo *jinfo, gpointer ip);
	void (*clear_breakpoint) (MonoJitInfo *jinfo, gpointer ip);
	MonoJitInfo* (*frame_get_jit_info) (MonoInterpFrameHandle frame);
	gpointer (*frame_get_ip) (MonoInterpFrameHandle frame);
	gpointer (*frame_get_arg) (MonoInterpFrameHandle frame, int pos);
	gpointer (*frame_get_local) (MonoInterpFrameHandle frame, int pos);
	gpointer (*frame_get_this) (MonoInterpFrameHandle frame);
	MonoInterpFrameHandle (*frame_get_parent) (MonoInterpFrameHandle frame);
	void (*start_single_stepping) (void);
	void (*stop_single_stepping) (void);
};

typedef gboolean (*Il2CppMonoInternalStackWalk) (MonoStackFrameInfo *frame, MonoContext *ctx, gpointer data);

struct _Il2CppMonoRuntimeExceptionHandlingCallbacks
{
	void (*il2cpp_mono_walk_stack_with_state) (Il2CppMonoInternalStackWalk func, MonoThreadUnwindState *state, MonoUnwindOptions options, void *user_data);
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

struct _Il2CppMonoDebugOptions
{
    gboolean native_debugger_break;
};

struct _Il2CppEmptyStruct
{
    int dummy;
};

typedef struct Il2CppMonoMethodHeader
{
	uint32_t      code_size;
	uint16_t      num_locals;
	MonoType    **locals;
} Il2CppMonoMethodHeader;

TYPED_HANDLE_DECL (MonoObject);
TYPED_HANDLE_DECL (MonoReflectionAssembly);

typedef void (*Il2CppMonoProfileFunc) (MonoProfiler *prof);
typedef void (*Il2CppMonoProfileAppDomainFunc) (MonoProfiler *prof, MonoDomain *domain);
typedef void (*Il2CppMonoProfileAppDomainResult) (MonoProfiler *prof, MonoDomain *domain, int result);
typedef void (*Il2CppMonoProfileAssemblyFunc) (MonoProfiler *prof, MonoAssembly *assembly);
typedef void (*Il2CppMonoProfileJitResult) (MonoProfiler *prof, MonoMethod *method, MonoJitInfo* jinfo, int result);
typedef void (*Il2CppMonoProfileAssemblyResult) (MonoProfiler *prof, MonoAssembly *assembly, int result);
typedef void (*Il2CppMonoProfileThreadFunc) (MonoProfiler *prof, uintptr_t tid);
typedef gboolean (*Il2CppMonoJitStackWalk) (MonoStackFrameInfo *frame, MonoContext *ctx, gpointer data);
typedef void (*Il2CppDomainFunc) (MonoDomain *domain, void* user_data);

typedef void (*emit_assembly_load_callback)(void*, void*);
typedef void(*emit_type_load_callback)(void*, void*, void*);

#ifdef __cplusplus
extern "C" {
#endif

void il2cpp_set_thread_state_background(MonoThread* thread);
void* il2cpp_domain_get_agent_info(MonoAppDomain* domain);
void il2cpp_domain_set_agent_info(MonoAppDomain* domain, void* agentInfo);
void il2cpp_start_debugger_thread();
void* il2cpp_gc_alloc_fixed(size_t size);
void il2cpp_gc_free_fixed(void* address);
const char* il2cpp_domain_get_name(MonoDomain* domain);

#ifdef __cplusplus
}
#endif

#endif
