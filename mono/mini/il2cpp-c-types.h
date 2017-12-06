#ifndef __IL2CPP_MONO_DEBUGGER_OPAQUE_TYPES_H__
#define __IL2CPP_MONO_DEBUGGER_OPAQUE_TYPES_H__

#if defined(RUNTIME_IL2CPP)
#include "il2cpp-class-internals.h"
#include "il2cpp-object-internals.h"
#endif // RUNTIME_IL2CPP

#define IL2CPP_MONO_PUBLIC_KEY_TOKEN_LENGTH	17

typedef struct Il2CppType Il2CppMonoType;
typedef struct Il2CppClass Il2CppMonoClass;
typedef struct _Il2CppMonoAssemblyName Il2CppMonoAssemblyNameReplacement;
typedef struct _Il2CppMonoAssembly Il2CppMonoAssembly;
typedef struct _Il2CppMonoDomain Il2CppMonoDomain;
typedef struct Il2CppImage Il2CppMonoImage;
typedef struct _Il2CppMonoMethodSignature Il2CppMonoMethodSignature;
typedef struct MethodInfo Il2CppMonoMethod;
typedef struct FieldInfo Il2CppMonoClassField;
typedef struct Il2CppArrayType Il2CppMonoArrayType;
typedef struct Il2CppGenericParam Il2CppMonoGenericParam;
typedef struct Il2CppGenericInst Il2CppMonoGenericInst;
typedef struct Il2CppGenericContext Il2CppMonoGenericContext;
typedef struct Il2CppGenericClass Il2CppMonoGenericClass;
typedef struct _Il2CppMonoMethodHeader Il2CppMonoMethodHeader;
typedef struct _Il2CppMonoVTable Il2CppMonoVTable;
typedef struct Il2CppPropertyInfo Il2CppMonoProperty;
typedef struct Il2CppString Il2CppMonoString;
typedef struct _Il2CppMonoAppDomain Il2CppMonoAppDomain;
typedef struct _Il2CppMonoMarshalByRefObject Il2CppMonoMarshalByRefObject;
typedef struct _Il2CppMonoObject Il2CppMonoObject;
typedef struct Il2CppArraySize Il2CppMonoArray;
typedef struct _Il2CppMonoCustomAttrInfo Il2CppMonoCustomAttrInfo;
typedef struct Il2CppThread Il2CppMonoThread;
typedef struct Il2CppGHashTable Il2CppMonoGHashTable;
typedef struct Il2CppGenericContainer Il2CppMonoGenericContainer;
typedef struct Il2CppReflectionAssembly Il2CppMonoReflectionAssembly;
typedef struct Il2CppReflectionType Il2CppMonoReflectionType;
typedef struct Il2CppProfiler Il2CppMonoProfiler;
typedef struct _Il2CppMonoJitTlsData Il2CppMonoJitTlsData;
typedef struct _Il2CppMonoRuntimeExceptionHandlingCallbacks Il2CppMonoRuntimeExceptionHandlingCallbacks;
typedef struct Il2CppInternalThread Il2CppMonoInternalThread;
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
		Il2CppMonoClass *catch_class;
	} data;
};

struct _Il2CppCattrNamedArg
{
	Il2CppMonoType *type;
	Il2CppMonoClassField *field;
	Il2CppMonoProperty *prop;
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
	Il2CppMonoMethod *declaring;
	Il2CppMonoGenericContext context;
};

struct _Il2CppMonoStackFrameInfo
{
	MonoStackFrameType type;
	MonoJitInfo *ji;
	Il2CppMonoMethod *method;
	Il2CppMonoMethod *actual_method;
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
	Il2CppMonoMethod *ctor;
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
	Il2CppMonoType *locals [MONO_ZERO_LEN_ARRAY];
};

struct _Il2CppMonoVTable
{
	Il2CppMonoClass *klass;
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

struct _Il2CppMonoAssembly
{
	Il2CppMonoAssemblyNameReplacement aname;
	Il2CppMonoImage *image;
};

struct _Il2CppMonoDomain
{
	gpointer runtime_info;
	guint32 state;
	char *friendly_name;
	mono_mutex_t assemblies_lock;
	GSList *domain_assemblies;
	Il2CppMonoAssembly *entry_assembly;
	Il2CppMonoAppDomain *domain;
};

struct _Il2CppMonoMethodSignature
{
	Il2CppMonoType *ret;
	guint16 param_count;
	unsigned int generic_param_count : 16;
	unsigned int  call_convention     : 6;
	unsigned int  hasthis             : 1;
	Il2CppMonoType **params;
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
	const Il2CppMonoType* const* const type;
	const char* const name;
	const MethodVariableKindC variableKind;
} Il2CppMethodExecutionContextInfoC;

typedef struct
{
	const Il2CppMethodExecutionContextInfoC* const executionContextInfos;
	const uint32_t executionContextInfoCount;
	const Il2CppMonoMethod* method;
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

typedef void (*Il2CppMonoProfileFunc) (Il2CppMonoProfiler *prof);
typedef void (*Il2CppMonoProfileAppDomainFunc) (Il2CppMonoProfiler *prof, Il2CppMonoDomain *domain);
typedef void (*Il2CppMonoProfileAppDomainResult) (Il2CppMonoProfiler *prof, Il2CppMonoDomain *domain, int result);
typedef void (*Il2CppMonoProfileAssemblyFunc) (Il2CppMonoProfiler *prof, Il2CppMonoAssembly *assembly);
typedef void (*Il2CppMonoProfileJitResult) (Il2CppMonoProfiler *prof, Il2CppMonoMethod *method, MonoJitInfo* jinfo, int result);
typedef void (*Il2CppMonoProfileAssemblyResult) (Il2CppMonoProfiler *prof, Il2CppMonoAssembly *assembly, int result);
typedef void (*Il2CppMonoProfileThreadFunc) (Il2CppMonoProfiler *prof, uintptr_t tid);
typedef gboolean (*Il2CppMonoJitStackWalk) (Il2CppMonoStackFrameInfo *frame, MonoContext *ctx, gpointer data);
typedef void (*Il2CppMonoDomainFunc) (Il2CppMonoDomain *domain, void* user_data);

typedef void (*emit_assembly_load_callback)(void*, void*);
typedef void(*emit_type_load_callback)(void*, void*, void*);

void il2cpp_set_thread_state_background(Il2CppMonoThread* thread);
void* il2cpp_domain_get_agent_info(Il2CppMonoAppDomain* domain);
void il2cpp_domain_set_agent_info(Il2CppMonoAppDomain* domain, void* agentInfo);
void il2cpp_start_debugger_thread();
uintptr_t il2cpp_internal_thread_get_thread_id(Il2CppMonoInternalThread* thread);
void* il2cpp_gc_alloc_fixed(size_t size);
void il2cpp_gc_free_fixed(void* address);
char* il2cpp_assembly_get_name(Il2CppMonoAssembly* assembly);
const char* il2cpp_domain_get_name(Il2CppMonoDomain* domain);
int il2cpp_mono_type_get_attrs(Il2CppMonoType* type);

#endif