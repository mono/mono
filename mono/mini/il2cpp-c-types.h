#ifndef __IL2CPP_MONO_DEBUGGER_OPAQUE_TYPES_H__
#define __IL2CPP_MONO_DEBUGGER_OPAQUE_TYPES_H__

#if defined(RUNTIME_IL2CPP)
#include "il2cpp-class-internals.h"
#include "il2cpp-object-internals.h"
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

//still stubs everywhere
typedef struct _Il2CppMonoAssemblyName Il2CppMonoAssemblyNameReplacement;
typedef struct _Il2CppMonoDomain Il2CppMonoDomain;
typedef struct _Il2CppMonoMethodSignature Il2CppMonoMethodSignature;
typedef struct _Il2CppMonoMethodHeader Il2CppMonoMethodHeader;
typedef struct _Il2CppMonoVTable Il2CppMonoVTable;
typedef struct _Il2CppMonoAppDomain Il2CppMonoAppDomain;
typedef struct _Il2CppMonoMarshalByRefObject Il2CppMonoMarshalByRefObject;
typedef struct _Il2CppMonoObject Il2CppMonoObject;
typedef struct _Il2CppMonoCustomAttrInfo Il2CppMonoCustomAttrInfo;
typedef struct Il2CppReflectionAssembly Il2CppMonoReflectionAssembly;
typedef struct _Il2CppMonoJitTlsData Il2CppMonoJitTlsData;
typedef struct _Il2CppMonoRuntimeExceptionHandlingCallbacks Il2CppMonoRuntimeExceptionHandlingCallbacks;
typedef struct _Il2CppMonoCustomAttrEntry Il2CppMonoCustomAttrEntry;
typedef struct _Il2CppMonoStackFrameInfo Il2CppMonoStackFrameInfo;
typedef struct Il2CppDefaults Il2CppMonoDefaults;
typedef struct _Il2CppMonoMethodInflated Il2CppMonoMethodInflated;
typedef struct _Il2CppMonoException Il2CppMonoException;
typedef struct _Il2CppCattrNamedArg Il2CppCattrNamedArg;
typedef struct _Il2CppMonoExceptionClause Il2CppMonoExceptionClause;
typedef struct _Il2CppMonoTypeNameParse Il2CppMonoTypeNameParse;


struct _Il2CppMonoJitTlsData { void *dummy; };

struct _Il2CppMonoExceptionClause
{
	uint32_t flags;
	uint32_t try_offset;
	uint32_t try_len;
	uint32_t handler_offset;
	uint32_t handler_len;
	union {
		uint32_t filter_offset;
		MonoClass *catch_class;
	} data;
};

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
	Il2CppMonoDomain *domain;
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

struct _Il2CppMonoMethodHeader
{
	const unsigned char *code;
	guint32 code_size;
	guint16 num_locals;
	Il2CppMonoExceptionClause *clauses;
	unsigned int num_clauses : 15;
	MonoType *locals [MONO_ZERO_LEN_ARRAY];
};

struct _Il2CppMonoVTable
{
	MonoClass *klass;
	Il2CppMonoDomain *domain;
	guint8 initialized;
	gpointer type;
	guint init_failed     : 1;
};

struct _Il2CppMonoAppDomain
{
	Il2CppMonoMarshalByRefObject mbr;
};

struct _Il2CppMonoAssemblyName
{
	const char *name;
	const char *culture;
	mono_byte public_key_token [IL2CPP_MONO_PUBLIC_KEY_TOKEN_LENGTH];
	uint32_t flags;
	uint16_t major, minor, build, revision;
};

struct _Il2CppMonoDomain
{
	gpointer runtime_info;
	guint32 state;
	char *friendly_name;
	mono_mutex_t assemblies_lock;
	GSList *domain_assemblies;
	MonoAssembly *entry_assembly;
	Il2CppMonoAppDomain *domain;
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
	Il2CppMonoAssemblyNameReplacement assembly;
	void *il2cppTypeNameParseInfo;
};

typedef enum
{
	kMethodVariableKindC_This,
	kMethodVariableKindC_Parameter,
	kMethodVariableKindC_LocalVariable
} MethodVariableKindC;

typedef enum
{
	kSequencePointKindC_Normal,
	kSequencePointKindC_StepOut
} SequencePointKindC;

typedef struct
{
	const MonoType* const* const type;
	const char* const name;
	const MethodVariableKindC variableKind;
	const int start;
	const int end;
} Il2CppMethodExecutionContextInfoC;

typedef struct
{
	int startOffset;
	int endOffset;
} Il2CppMethodScopeC;

typedef struct
{
	int codeSize;
	int numScopes;
	Il2CppMethodScopeC *scopes;
} Il2CppMethodHeaderInfoC;

typedef struct
{
	const Il2CppMethodExecutionContextInfoC* const executionContextInfos;
	const uint32_t executionContextInfoCount;
	const Il2CppMethodHeaderInfoC *header;
	const MonoMethod* method;
	const char* const sourceFile;
	const uint8_t sourceFileHash[16];
	const int32_t lineStart, lineEnd;
	const int32_t columnStart, columnEnd;
	const int32_t ilOffset;
	const SequencePointKindC kind;
	uint8_t isActive;
	uint64_t id;
} Il2CppSequencePointC;

typedef struct
{
	void** values;
} Il2CppSequencePointExecutionContextC;

typedef struct Il2CppThreadUnwindState
{
	Il2CppSequencePointC** sequencePoints;
	Il2CppSequencePointExecutionContextC** executionContexts;
	uint32_t frameCount;
} Il2CppThreadUnwindState;

TYPED_HANDLE_DECL (Il2CppMonoObject);
TYPED_HANDLE_DECL (Il2CppMonoReflectionAssembly);
Il2CppMonoDefaults il2cpp_mono_defaults;
MonoDebugOptions il2cpp_mono_debug_options;

typedef void (*Il2CppMonoProfileFunc) (MonoProfiler *prof);
typedef void (*Il2CppMonoProfileAppDomainFunc) (MonoProfiler *prof, Il2CppMonoDomain *domain);
typedef void (*Il2CppMonoProfileAppDomainResult) (MonoProfiler *prof, Il2CppMonoDomain *domain, int result);
typedef void (*Il2CppMonoProfileAssemblyFunc) (MonoProfiler *prof, MonoAssembly *assembly);
typedef void (*Il2CppMonoProfileJitResult) (MonoProfiler *prof, MonoMethod *method, MonoJitInfo* jinfo, int result);
typedef void (*Il2CppMonoProfileAssemblyResult) (MonoProfiler *prof, MonoAssembly *assembly, int result);
typedef void (*Il2CppMonoProfileThreadFunc) (MonoProfiler *prof, uintptr_t tid);
typedef gboolean (*Il2CppMonoJitStackWalk) (Il2CppMonoStackFrameInfo *frame, MonoContext *ctx, gpointer data);
typedef void (*Il2CppMonoDomainFunc) (Il2CppMonoDomain *domain, void* user_data);

typedef void (*emit_assembly_load_callback)(void*, void*);
typedef void(*emit_type_load_callback)(void*, void*, void*);

void il2cpp_set_thread_state_background(MonoThread* thread);
void* il2cpp_domain_get_agent_info(Il2CppMonoAppDomain* domain);
void il2cpp_domain_set_agent_info(Il2CppMonoAppDomain* domain, void* agentInfo);
void il2cpp_start_debugger_thread();
void* il2cpp_gc_alloc_fixed(size_t size);
void il2cpp_gc_free_fixed(void* address);
const char* il2cpp_domain_get_name(Il2CppMonoDomain* domain);

#endif