#include <emscripten.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdint.h>
#include <assert.h>

#include <mono/metadata/assembly.h>
#include <mono/metadata/tokentype.h>
#include <mono/utils/mono-logger.h>
#include <mono/utils/mono-dl-fallback.h>
#include <mono/jit/jit.h>

// FIXME: Autogenerate this

typedef struct {
	const char *name;
	void *func;
} PinvokeImport;

void SystemNative_ConvertErrorPalToPlatform ();
void SystemNative_ConvertErrorPlatformToPal ();
void SystemNative_StrErrorR ();
void SystemNative_MemSet ();
void SystemNative_GetEGid ();
void SystemNative_GetEUid ();
void SystemNative_GetPwNamR ();
void SystemNative_GetPwUidR ();
void SystemNative_SetEUid ();
void SystemNative_GetAbsoluteTime ();
void SystemNative_GetTimebaseInfo ();
void SystemNative_GetTimestamp ();
void SystemNative_GetTimestampResolution ();
void SystemNative_UTime ();
void SystemNative_UTimes ();
void SystemNative_Access ();
void SystemNative_ChDir ();
void SystemNative_ChMod ();
void SystemNative_Close ();
void SystemNative_CloseDir ();
void SystemNative_CopyFile ();
void SystemNative_Dup ();
void SystemNative_FChMod ();
void SystemNative_FLock ();
void SystemNative_FStat2 ();
void SystemNative_FSync ();
void SystemNative_FTruncate ();
void SystemNative_FcntlCanGetSetPipeSz ();
void SystemNative_FcntlGetPipeSz ();
void SystemNative_FcntlSetCloseOnExec ();
void SystemNative_FcntlSetIsNonBlocking ();
void SystemNative_FcntlSetPipeSz ();
void SystemNative_FnMatch ();
void SystemNative_GetLine ();
void SystemNative_GetPeerID ();
void SystemNative_GetReadDirRBufferSize ();
void SystemNative_INotifyAddWatch ();
void SystemNative_INotifyInit ();
void SystemNative_INotifyRemoveWatch ();
void SystemNative_LSeek ();
void SystemNative_LStat2 ();
void SystemNative_Link ();
void SystemNative_LockFileRegion ();
void SystemNative_MAdvise ();
void SystemNative_MLock ();
void SystemNative_MMap ();
void SystemNative_MProtect ();
void SystemNative_MSync ();
void SystemNative_MUnlock ();
void SystemNative_MUnmap ();
void SystemNative_MkDir ();
void SystemNative_MksTemps ();
void SystemNative_Open ();
void SystemNative_OpenDir ();
void SystemNative_Pipe ();
void SystemNative_Poll ();
void SystemNative_PosixFAdvise ();
void SystemNative_Read ();
void SystemNative_ReadDirR ();
void SystemNative_ReadLink ();
void SystemNative_RealPath ();
void SystemNative_Rename ();
void SystemNative_RmDir ();
void SystemNative_ShmOpen ();
void SystemNative_ShmUnlink ();
void SystemNative_Stat2 ();
void SystemNative_Sync ();
void SystemNative_SysConf ();
void SystemNative_Unlink ();
void SystemNative_Write ();
void SystemNative_Accept ();
void SystemNative_Bind ();
void SystemNative_CloseSocketEventPort ();
void SystemNative_Connect ();
void SystemNative_CreateSocketEventBuffer ();
void SystemNative_CreateSocketEventPort ();
void SystemNative_FreeHostEntry ();
void SystemNative_FreeSocketEventBuffer ();
void SystemNative_GetAddressFamily ();
void SystemNative_GetAtOutOfBandMark ();
void SystemNative_GetBytesAvailable ();
void SystemNative_GetControlMessageBufferSize ();
void SystemNative_GetDomainName ();
void SystemNative_GetDomainSocketSizes ();
void SystemNative_GetHostEntryForName ();
void SystemNative_GetHostName ();
void SystemNative_GetIPSocketAddressSizes ();
void SystemNative_GetIPv4Address ();
void SystemNative_GetIPv4MulticastOption ();
void SystemNative_GetIPv6Address ();
void SystemNative_GetIPv6MulticastOption ();
void SystemNative_GetLingerOption ();
void SystemNative_GetNameInfo ();
void SystemNative_GetNextIPAddress ();
void SystemNative_GetPeerName ();
void SystemNative_GetPeerUserName ();
void SystemNative_GetPort ();
void SystemNative_GetSockName ();
void SystemNative_GetSockOpt ();
void SystemNative_GetSocketErrorOption ();
void SystemNative_Listen ();
void SystemNative_PlatformSupportsDualModeIPv4PacketInfo ();
void SystemNative_ReceiveMessage ();
void SystemNative_SendFile ();
void SystemNative_SendMessage ();
void SystemNative_SetAddressFamily ();
void SystemNative_SetIPv4Address ();
void SystemNative_SetIPv4MulticastOption ();
void SystemNative_SetIPv6Address ();
void SystemNative_SetIPv6MulticastOption ();
void SystemNative_SetLingerOption ();
void SystemNative_SetPort ();
void SystemNative_SetReceiveTimeout ();
void SystemNative_SetSendTimeout ();
void SystemNative_SetSockOpt ();
void SystemNative_Shutdown ();
void SystemNative_Socket ();
void SystemNative_TryChangeSocketEventRegistration ();
void SystemNative_TryGetIPPacketInformation ();
void SystemNative_WaitForSocketEvents ();
void SystemNative_MapTcpState ();
void SystemNative_GetNonCryptographicallySecureRandomBytes ();

static PinvokeImport sysnative_imports [] = {
	{"SystemNative_ConvertErrorPalToPlatform", SystemNative_ConvertErrorPalToPlatform},
	{"SystemNative_ConvertErrorPlatformToPal", SystemNative_ConvertErrorPlatformToPal},
	{"SystemNative_StrErrorR", SystemNative_StrErrorR},
	{"SystemNative_MemSet", SystemNative_MemSet},
	{"SystemNative_GetEGid", SystemNative_GetEGid},
	{"SystemNative_GetEUid", SystemNative_GetEUid},
	{"SystemNative_GetPwNamR", SystemNative_GetPwNamR},
	{"SystemNative_GetPwUidR", SystemNative_GetPwUidR},
	{"SystemNative_SetEUid", SystemNative_SetEUid},
	{"SystemNative_GetAbsoluteTime", SystemNative_GetAbsoluteTime},
	{"SystemNative_GetTimebaseInfo", SystemNative_GetTimebaseInfo},
	{"SystemNative_GetTimestamp", SystemNative_GetTimestamp},
	{"SystemNative_GetTimestampResolution", SystemNative_GetTimestampResolution},
	{"SystemNative_UTime", SystemNative_UTime},
	{"SystemNative_UTimes", SystemNative_UTimes},
	{"SystemNative_Access", SystemNative_Access},
	{"SystemNative_ChDir", SystemNative_ChDir},
	{"SystemNative_ChMod", SystemNative_ChMod},
	{"SystemNative_Close", SystemNative_Close},
	{"SystemNative_CloseDir", SystemNative_CloseDir},
	{"SystemNative_CopyFile", SystemNative_CopyFile},
	{"SystemNative_Dup", SystemNative_Dup},
	{"SystemNative_FChMod", SystemNative_FChMod},
	{"SystemNative_FLock", SystemNative_FLock},
	{"SystemNative_FStat2", SystemNative_FStat2},
	{"SystemNative_FSync", SystemNative_FSync},
	{"SystemNative_FTruncate", SystemNative_FTruncate},
	{"SystemNative_FcntlCanGetSetPipeSz", SystemNative_FcntlCanGetSetPipeSz},
	{"SystemNative_FcntlGetPipeSz", SystemNative_FcntlGetPipeSz},
	{"SystemNative_FcntlSetCloseOnExec", SystemNative_FcntlSetCloseOnExec},
	{"SystemNative_FcntlSetIsNonBlocking", SystemNative_FcntlSetIsNonBlocking},
	{"SystemNative_FcntlSetPipeSz", SystemNative_FcntlSetPipeSz},
	{"SystemNative_FnMatch", SystemNative_FnMatch},
	{"SystemNative_GetLine", SystemNative_GetLine},
	{"SystemNative_GetPeerID", SystemNative_GetPeerID},
	{"SystemNative_GetReadDirRBufferSize", SystemNative_GetReadDirRBufferSize},
	{"SystemNative_INotifyAddWatch", SystemNative_INotifyAddWatch},
	{"SystemNative_INotifyInit", SystemNative_INotifyInit},
	{"SystemNative_INotifyRemoveWatch", SystemNative_INotifyRemoveWatch},
	{"SystemNative_LSeek", SystemNative_LSeek},
	{"SystemNative_LStat2", SystemNative_LStat2},
	{"SystemNative_Link", SystemNative_Link},
	{"SystemNative_LockFileRegion", SystemNative_LockFileRegion},
	{"SystemNative_MAdvise", SystemNative_MAdvise},
	{"SystemNative_MLock", SystemNative_MLock},
	{"SystemNative_MMap", SystemNative_MMap},
	{"SystemNative_MProtect", SystemNative_MProtect},
	{"SystemNative_MSync", SystemNative_MSync},
	{"SystemNative_MUnlock", SystemNative_MUnlock},
	{"SystemNative_MUnmap", SystemNative_MUnmap},
	{"SystemNative_MkDir", SystemNative_MkDir},
	{"SystemNative_MksTemps", SystemNative_MksTemps},
	{"SystemNative_Open", SystemNative_Open},
	{"SystemNative_OpenDir", SystemNative_OpenDir},
	{"SystemNative_Pipe", SystemNative_Pipe},
	{"SystemNative_Poll", SystemNative_Poll},
	{"SystemNative_PosixFAdvise", SystemNative_PosixFAdvise},
	{"SystemNative_Read", SystemNative_Read},
	{"SystemNative_ReadDirR", SystemNative_ReadDirR},
	{"SystemNative_ReadLink", SystemNative_ReadLink},
	{"SystemNative_RealPath", SystemNative_RealPath},
	{"SystemNative_Rename", SystemNative_Rename},
	{"SystemNative_RmDir", SystemNative_RmDir},
	{"SystemNative_ShmOpen", SystemNative_ShmOpen},
	{"SystemNative_ShmUnlink", SystemNative_ShmUnlink},
	{"SystemNative_Stat2", SystemNative_Stat2},
	{"SystemNative_Sync", SystemNative_Sync},
	{"SystemNative_SysConf", SystemNative_SysConf},
	{"SystemNative_Unlink", SystemNative_Unlink},
	{"SystemNative_Write", SystemNative_Write},
	{"SystemNative_Accept", SystemNative_Accept},
	{"SystemNative_Bind", SystemNative_Bind},
	{"SystemNative_CloseSocketEventPort", SystemNative_CloseSocketEventPort},
	{"SystemNative_Connect", SystemNative_Connect},
	{"SystemNative_CreateSocketEventBuffer", SystemNative_CreateSocketEventBuffer},
	{"SystemNative_CreateSocketEventPort", SystemNative_CreateSocketEventPort},
	{"SystemNative_FreeHostEntry", SystemNative_FreeHostEntry},
	{"SystemNative_FreeSocketEventBuffer", SystemNative_FreeSocketEventBuffer},
	{"SystemNative_GetAddressFamily", SystemNative_GetAddressFamily},
	{"SystemNative_GetAtOutOfBandMark", SystemNative_GetAtOutOfBandMark},
	{"SystemNative_GetBytesAvailable", SystemNative_GetBytesAvailable},
	{"SystemNative_GetControlMessageBufferSize", SystemNative_GetControlMessageBufferSize},
	{"SystemNative_GetDomainName", SystemNative_GetDomainName},
	{"SystemNative_GetDomainSocketSizes", SystemNative_GetDomainSocketSizes},
	{"SystemNative_GetHostEntryForName", SystemNative_GetHostEntryForName},
	{"SystemNative_GetHostName", SystemNative_GetHostName},
	{"SystemNative_GetIPSocketAddressSizes", SystemNative_GetIPSocketAddressSizes},
	{"SystemNative_GetIPv4Address", SystemNative_GetIPv4Address},
	{"SystemNative_GetIPv4MulticastOption", SystemNative_GetIPv4MulticastOption},
	{"SystemNative_GetIPv6Address", SystemNative_GetIPv6Address},
	{"SystemNative_GetIPv6MulticastOption", SystemNative_GetIPv6MulticastOption},
	{"SystemNative_GetLingerOption", SystemNative_GetLingerOption},
	{"SystemNative_GetNameInfo", SystemNative_GetNameInfo},
	{"SystemNative_GetNextIPAddress", SystemNative_GetNextIPAddress},
	{"SystemNative_GetPeerName", SystemNative_GetPeerName},
	{"SystemNative_GetPeerUserName", SystemNative_GetPeerUserName},
	{"SystemNative_GetPort", SystemNative_GetPort},
	{"SystemNative_GetSockName", SystemNative_GetSockName},
	{"SystemNative_GetSockOpt", SystemNative_GetSockOpt},
	{"SystemNative_GetSocketErrorOption", SystemNative_GetSocketErrorOption},
	{"SystemNative_Listen", SystemNative_Listen},
	{"SystemNative_PlatformSupportsDualModeIPv4PacketInfo", SystemNative_PlatformSupportsDualModeIPv4PacketInfo},
	{"SystemNative_ReceiveMessage", SystemNative_ReceiveMessage},
	{"SystemNative_SendFile", SystemNative_SendFile},
	{"SystemNative_SendMessage", SystemNative_SendMessage},
	{"SystemNative_SetAddressFamily", SystemNative_SetAddressFamily},
	{"SystemNative_SetIPv4Address", SystemNative_SetIPv4Address},
	{"SystemNative_SetIPv4MulticastOption", SystemNative_SetIPv4MulticastOption},
	{"SystemNative_SetIPv6Address", SystemNative_SetIPv6Address},
	{"SystemNative_SetIPv6MulticastOption", SystemNative_SetIPv6MulticastOption},
	{"SystemNative_SetLingerOption", SystemNative_SetLingerOption},
	{"SystemNative_SetPort", SystemNative_SetPort},
	{"SystemNative_SetReceiveTimeout", SystemNative_SetReceiveTimeout},
	{"SystemNative_SetSendTimeout", SystemNative_SetSendTimeout},
	{"SystemNative_SetSockOpt", SystemNative_SetSockOpt},
	{"SystemNative_Shutdown", SystemNative_Shutdown},
	{"SystemNative_Socket", SystemNative_Socket},
	{"SystemNative_TryChangeSocketEventRegistration", SystemNative_TryChangeSocketEventRegistration},
	{"SystemNative_TryGetIPPacketInformation", SystemNative_TryGetIPPacketInformation},
	{"SystemNative_WaitForSocketEvents", SystemNative_WaitForSocketEvents},
	{"SystemNative_MapTcpState", SystemNative_MapTcpState},
	{"SystemNative_GetNonCryptographicallySecureRandomBytes", SystemNative_GetNonCryptographicallySecureRandomBytes},
};

#ifdef CORE_BINDINGS
void core_initialize_internals ();
#endif

// Blazor specific custom routines - see dotnet_support.js for backing code
extern void* mono_wasm_invoke_js_marshalled (MonoString **exceptionMessage, void *asyncHandleLongPtr, MonoString *funcName, MonoString *argsJson);
extern void* mono_wasm_invoke_js_unmarshalled (MonoString **exceptionMessage, MonoString *funcName, void* arg0, void* arg1, void* arg2);

void mono_wasm_enable_debugging (void);

void mono_ee_interp_init (const char *opts);
void mono_marshal_ilgen_init (void);
void mono_method_builder_ilgen_init (void);
void mono_sgen_mono_ilgen_init (void);
void mono_icall_table_init (void);
void mono_aot_register_module (void **aot_info);
char *monoeg_g_getenv(const char *variable);
int monoeg_g_setenv(const char *variable, const char *value, int overwrite);
void mono_free (void*);

/* Not part of public headers */
#define MONO_ICALL_TABLE_CALLBACKS_VERSION 2

typedef struct {
	int version;
	void* (*lookup) (MonoMethod *method, char *classname, char *methodname, char *sigstart, uint8_t *uses_handles);
	const char* (*lookup_icall_symbol) (void* func);
} MonoIcallTableCallbacks;

void
mono_install_icall_table_callbacks (MonoIcallTableCallbacks *cb);

int mono_regression_test_step (int verbose_level, char *image, char *method_name);
void mono_trace_init (void);

static char*
m_strdup (const char *str)
{
	if (!str)
		return NULL;

	int len = strlen (str) + 1;
	char *res = malloc (len);
	memcpy (res, str, len);
	return res;
}

static MonoDomain *root_domain;

static MonoString*
mono_wasm_invoke_js (MonoString *str, int *is_exception)
{
	if (str == NULL)
		return NULL;

	char *native_val = mono_string_to_utf8 (str);
	mono_unichar2 *native_res = (mono_unichar2*)EM_ASM_INT ({
		var str = UTF8ToString ($0);
		try {
			var res = eval (str);
			if (res === null || res == undefined)
				return 0;
			res = res.toString ();
			setValue ($1, 0, "i32");
		} catch (e) {
			res = e.toString ();
			setValue ($1, 1, "i32");
			if (res === null || res === undefined)
				res = "unknown exception";
		}
		var buff = Module._malloc((res.length + 1) * 2);
		stringToUTF16 (res, buff, (res.length + 1) * 2);
		return buff;
	}, (int)native_val, is_exception);

	mono_free (native_val);

	if (native_res == NULL)
		return NULL;

	MonoString *res = mono_string_from_utf16 (native_res);
	free (native_res);
	return res;
}

static void
wasm_logger (const char *log_domain, const char *log_level, const char *message, mono_bool fatal, void *user_data)
{
	if (fatal) {
		EM_ASM(
			   var err = new Error();
			   console.log ("Stacktrace: \n");
			   console.log (err.stack);
			   );

		fprintf (stderr, "%s", message);

		abort ();
	} else {
		fprintf (stdout, "%s\n", message);
	}
}

#ifdef DRIVER_GEN
#include "driver-gen.c"
#endif

typedef struct WasmAssembly_ WasmAssembly;

struct WasmAssembly_ {
	MonoBundledAssembly assembly;
	WasmAssembly *next;
};

static WasmAssembly *assemblies;
static int assembly_count;

EMSCRIPTEN_KEEPALIVE void
mono_wasm_add_assembly (const char *name, const unsigned char *data, unsigned int size)
{
	int len = strlen (name);
	if (!strcasecmp (".pdb", &name [len - 4])) {
		char *new_name = m_strdup (name);
		//FIXME handle debugging assemblies with .exe extension
		strcpy (&new_name [len - 3], "dll");
		mono_register_symfile_for_assembly (new_name, data, size);
		return;
	}
	WasmAssembly *entry = (WasmAssembly *)malloc(sizeof (MonoBundledAssembly));
	entry->assembly.name = m_strdup (name);
	entry->assembly.data = data;
	entry->assembly.size = size;
	entry->next = assemblies;
	assemblies = entry;
	++assembly_count;
}

EMSCRIPTEN_KEEPALIVE void
mono_wasm_setenv (const char *name, const char *value)
{
	monoeg_g_setenv (strdup (name), strdup (value), 1);
}

static int sysnative_dl_handle;

static void*
wasm_dl_load (const char *name, int flags, char **err, void *user_data)
{
	// FIXME: Add a general approach, this just makes System.IO work
	if (!strcmp (name, "System.Native"))
		return &sysnative_dl_handle;
	return NULL;
}

static void*
wasm_dl_symbol (void *handle, const char *name, char **err, void *user_data)
{
	if (handle == &sysnative_dl_handle) {
		for (int i = 0; i < sizeof (sysnative_imports) / sizeof (sysnative_imports [0]); ++i)
			if (!strcmp (sysnative_imports [i].name, name))
				return sysnative_imports [i].func;
	}
	return NULL;
}

#if !defined(ENABLE_AOT) || defined(EE_MODE_LLVMONLY_INTERP)
#define NEED_INTERP 1
#ifndef LINK_ICALLS
// FIXME: llvm+interp mode needs this to call icalls
#define NEED_NORMAL_ICALL_TABLES 1
#endif
#endif

#ifdef LINK_ICALLS

#include "icall-table.h"

static int
compare_int (const void *k1, const void *k2)
{
	return *(int*)k1 - *(int*)k2;
}

static void*
icall_table_lookup (MonoMethod *method, char *classname, char *methodname, char *sigstart, uint8_t *uses_handles)
{
	uint32_t token = mono_method_get_token (method);
	assert (token);
	assert ((token & MONO_TOKEN_METHOD_DEF) == MONO_TOKEN_METHOD_DEF);
	uint32_t token_idx = token - MONO_TOKEN_METHOD_DEF;

	int *indexes = NULL;
	int indexes_size = 0;
	uint8_t *handles = NULL;
	void **funcs = NULL;

	*uses_handles = 0;

	const char *image_name = mono_image_get_name (mono_class_get_image (mono_method_get_class (method)));

#ifdef ICALL_TABLE_mscorlib
	if (!strcmp (image_name, "mscorlib")) {
		indexes = mscorlib_icall_indexes;
		indexes_size = sizeof (mscorlib_icall_indexes) / 4;
		handles = mscorlib_icall_handles;
		funcs = mscorlib_icall_funcs;
		assert (sizeof (mscorlib_icall_indexes [0]) == 4);
	}
#ifdef ICALL_TABLE_System
	if (!strcmp (image_name, "System")) {
		indexes = System_icall_indexes;
		indexes_size = sizeof (System_icall_indexes) / 4;
		handles = System_icall_handles;
		funcs = System_icall_funcs;
	}
#endif
	assert (indexes);

	void *p = bsearch (&token_idx, indexes, indexes_size, 4, compare_int);
	if (!p) {
		return NULL;
		printf ("wasm: Unable to lookup icall: %s\n", mono_method_get_name (method));
		exit (1);
	}

	uint32_t idx = (int*)p - indexes;
	*uses_handles = handles [idx];

	//printf ("ICALL: %s %x %d %d\n", methodname, token, idx, (int)(funcs [idx]));

	return funcs [idx];
#endif
}

static const char*
icall_table_lookup_symbol (void *func)
{
	assert (0);
	return NULL;
}

#endif

void mono_initialize_internals ()
{
	mono_add_internal_call ("WebAssembly.Runtime::InvokeJS", mono_wasm_invoke_js);

	// Blazor specific custom routines - see dotnet_support.js for backing code		
	mono_add_internal_call ("WebAssembly.JSInterop.InternalCalls::InvokeJSMarshalled", mono_wasm_invoke_js_marshalled);
	mono_add_internal_call ("WebAssembly.JSInterop.InternalCalls::InvokeJSUnmarshalled", mono_wasm_invoke_js_unmarshalled);

#ifdef CORE_BINDINGS	
	core_initialize_internals();
#endif

}

EMSCRIPTEN_KEEPALIVE void
mono_wasm_load_runtime (const char *managed_path, int enable_debugging)
{
	monoeg_g_setenv ("MONO_LOG_LEVEL", "debug", 0);
	monoeg_g_setenv ("MONO_LOG_MASK", "gc", 0);

	mono_dl_fallback_register (wasm_dl_load, wasm_dl_symbol, NULL, NULL);

#ifdef ENABLE_AOT
	// Defined in driver-gen.c
	register_aot_modules ();
#ifdef EE_MODE_LLVMONLY_INTERP
	mono_jit_set_aot_mode (MONO_AOT_MODE_LLVMONLY_INTERP);
#else
	mono_jit_set_aot_mode (MONO_AOT_MODE_LLVMONLY);
#endif
#else
	mono_jit_set_aot_mode (MONO_AOT_MODE_INTERP_LLVMONLY);
	if (enable_debugging)
		mono_wasm_enable_debugging ();
#endif

#ifdef LINK_ICALLS
	/* Link in our own linked icall table */
	MonoIcallTableCallbacks cb;
	memset (&cb, 0, sizeof (MonoIcallTableCallbacks));
	cb.version = MONO_ICALL_TABLE_CALLBACKS_VERSION;
	cb.lookup = icall_table_lookup;
	cb.lookup_icall_symbol = icall_table_lookup_symbol;

	mono_install_icall_table_callbacks (&cb);
#endif

#ifdef NEED_NORMAL_ICALL_TABLES
	mono_icall_table_init ();
#endif
#ifdef NEED_INTERP
	mono_ee_interp_init ("");
	mono_marshal_ilgen_init ();
	mono_method_builder_ilgen_init ();
	mono_sgen_mono_ilgen_init ();
#endif

	if (assembly_count) {
		MonoBundledAssembly **bundle_array = (MonoBundledAssembly **)calloc (1, sizeof (MonoBundledAssembly*) * (assembly_count + 1));
		WasmAssembly *cur = assemblies;
		bundle_array [assembly_count] = NULL;
		int i = 0;
		while (cur) {
			bundle_array [i] = &cur->assembly;
			cur = cur->next;
			++i;
		}
		mono_register_bundled_assemblies ((const MonoBundledAssembly**)bundle_array);
	}

	mono_trace_init ();
	mono_trace_set_log_handler (wasm_logger, NULL);
	root_domain = mono_jit_init_version ("mono", "v4.0.30319");

	mono_initialize_internals();
}

EMSCRIPTEN_KEEPALIVE MonoAssembly*
mono_wasm_assembly_load (const char *name)
{
	MonoImageOpenStatus status;
	MonoAssemblyName* aname = mono_assembly_name_new (name);
	if (!name)
		return NULL;

	MonoAssembly *res = mono_assembly_load (aname, NULL, &status);
	mono_assembly_name_free (aname);

	return res;
}

EMSCRIPTEN_KEEPALIVE MonoClass*
mono_wasm_assembly_find_class (MonoAssembly *assembly, const char *namespace, const char *name)
{
	return mono_class_from_name (mono_assembly_get_image (assembly), namespace, name);
}

EMSCRIPTEN_KEEPALIVE MonoMethod*
mono_wasm_assembly_find_method (MonoClass *klass, const char *name, int arguments)
{
	return mono_class_get_method_from_name (klass, name, arguments);
}

EMSCRIPTEN_KEEPALIVE MonoObject*
mono_wasm_invoke_method (MonoMethod *method, MonoObject *this_arg, void *params[], int* got_exception)
{
	MonoObject *exc = NULL;
	MonoObject *res = mono_runtime_invoke (method, this_arg, params, &exc);
	*got_exception = 0;

	if (exc) {
		*got_exception = 1;

		MonoObject *exc2 = NULL;
		res = (MonoObject*)mono_object_to_string (exc, &exc2); 
		if (exc2)
			res = (MonoObject*) mono_string_new (root_domain, "Exception Double Fault");
		return res;
	}

	MonoMethodSignature *sig = mono_method_signature (method);
	MonoType *type = mono_signature_get_return_type (sig);
	// If the method return type is void return null
	// This gets around a memory access crash when the result return a value when
	// a void method is invoked.
	if (mono_type_get_type (type) == MONO_TYPE_VOID)
		return NULL;

	return res;
}

EMSCRIPTEN_KEEPALIVE MonoMethod*
mono_wasm_assembly_get_entry_point (MonoAssembly *assembly)
{
	MonoImage *image;
	MonoMethod *method;

	image = mono_assembly_get_image (assembly);
	uint32_t entry = mono_image_get_entry_point (image);
	if (!entry)
		return NULL;

	return mono_get_method (image, entry, NULL);
}

EMSCRIPTEN_KEEPALIVE char *
mono_wasm_string_get_utf8 (MonoString *str)
{
	return mono_string_to_utf8 (str); //XXX JS is responsible for freeing this
}

EMSCRIPTEN_KEEPALIVE MonoString *
mono_wasm_string_from_js (const char *str)
{
	return mono_string_new (root_domain, str);
}

static int
class_is_task (MonoClass *klass)
{
	if (!strcmp ("System.Threading.Tasks", mono_class_get_namespace (klass)) && 
		(!strcmp ("Task", mono_class_get_name (klass)) || !strcmp ("Task`1", mono_class_get_name (klass))))
		return 1;

	return 0;
}

#define MARSHAL_TYPE_INT 1
#define MARSHAL_TYPE_FP 2
#define MARSHAL_TYPE_STRING 3
#define MARSHAL_TYPE_VT 4
#define MARSHAL_TYPE_DELEGATE 5
#define MARSHAL_TYPE_TASK 6
#define MARSHAL_TYPE_OBJECT 7
#define MARSHAL_TYPE_BOOL 8
#define MARSHAL_TYPE_ENUM 9

// typed array marshalling
#define MARSHAL_ARRAY_BYTE 11
#define MARSHAL_ARRAY_UBYTE 12
#define MARSHAL_ARRAY_SHORT 13
#define MARSHAL_ARRAY_USHORT 14
#define MARSHAL_ARRAY_INT 15
#define MARSHAL_ARRAY_UINT 16
#define MARSHAL_ARRAY_FLOAT 17
#define MARSHAL_ARRAY_DOUBLE 18

EMSCRIPTEN_KEEPALIVE int
mono_wasm_get_obj_type (MonoObject *obj)
{
	if (!obj)
		return 0;
	MonoClass *klass = mono_object_get_class (obj);
	MonoType *type = mono_class_get_type (klass);

	switch (mono_type_get_type (type)) {
	// case MONO_TYPE_CHAR: prob should be done not as a number?
	case MONO_TYPE_BOOLEAN:
		return MARSHAL_TYPE_BOOL;
	case MONO_TYPE_I1:
	case MONO_TYPE_U1:
	case MONO_TYPE_I2:
	case MONO_TYPE_U2:
	case MONO_TYPE_I4:
	case MONO_TYPE_U4:
	case MONO_TYPE_I8:
	case MONO_TYPE_U8:
		return MARSHAL_TYPE_INT;
	case MONO_TYPE_R4:
	case MONO_TYPE_R8:
		return MARSHAL_TYPE_FP;
	case MONO_TYPE_STRING:
		return MARSHAL_TYPE_STRING;
	case MONO_TYPE_SZARRAY:  { // simple zero based one-dim-array
		MonoClass *eklass = mono_class_get_element_class(klass);
		MonoType *etype = mono_class_get_type (eklass);

		switch (mono_type_get_type (etype)) {
			case MONO_TYPE_U1:
				return MARSHAL_ARRAY_UBYTE;
			case MONO_TYPE_I1:
				return MARSHAL_ARRAY_BYTE;
			case MONO_TYPE_U2:
				return MARSHAL_ARRAY_USHORT;			
			case MONO_TYPE_I2:
				return MARSHAL_ARRAY_SHORT;			
			case MONO_TYPE_U4:
				return MARSHAL_ARRAY_UINT;			
			case MONO_TYPE_I4:
				return MARSHAL_ARRAY_INT;			
			case MONO_TYPE_R4:
				return MARSHAL_ARRAY_FLOAT;
			case MONO_TYPE_R8:
				return MARSHAL_ARRAY_DOUBLE;
			default:
				return MARSHAL_TYPE_OBJECT;
		}		
	}
	default:
		if (mono_class_is_enum (klass))
			return MARSHAL_TYPE_ENUM;
		if (!mono_type_is_reference (type)) //vt
			return MARSHAL_TYPE_VT;
		if (mono_class_is_delegate (klass))
			return MARSHAL_TYPE_DELEGATE;
		if (class_is_task(klass))
			return MARSHAL_TYPE_TASK;

		return MARSHAL_TYPE_OBJECT;
	}
}

EMSCRIPTEN_KEEPALIVE int
mono_unbox_int (MonoObject *obj)
{
	if (!obj)
		return 0;
	MonoType *type = mono_class_get_type (mono_object_get_class(obj));

	void *ptr = mono_object_unbox (obj);
	switch (mono_type_get_type (type)) {
	case MONO_TYPE_I1:
	case MONO_TYPE_BOOLEAN:
		return *(signed char*)ptr;
	case MONO_TYPE_U1:
		return *(unsigned char*)ptr;
	case MONO_TYPE_I2:
		return *(short*)ptr;
	case MONO_TYPE_U2:
		return *(unsigned short*)ptr;
	case MONO_TYPE_I4:
		return *(int*)ptr;
	case MONO_TYPE_U4:
		return *(unsigned int*)ptr;
	// WASM doesn't support returning longs to JS
	// case MONO_TYPE_I8:
	// case MONO_TYPE_U8:
	default:
		printf ("Invalid type %d to mono_unbox_int\n", mono_type_get_type (type));
		return 0;
	}
}

EMSCRIPTEN_KEEPALIVE double
mono_wasm_unbox_float (MonoObject *obj)
{
	if (!obj)
		return 0;
	MonoType *type = mono_class_get_type (mono_object_get_class(obj));

	void *ptr = mono_object_unbox (obj);
	switch (mono_type_get_type (type)) {
	case MONO_TYPE_R4:
		return *(float*)ptr;
	case MONO_TYPE_R8:
		return *(double*)ptr;
	default:
		printf ("Invalid type %d to mono_wasm_unbox_float\n", mono_type_get_type (type));
		return 0;
	}
}

EMSCRIPTEN_KEEPALIVE int
mono_wasm_array_length (MonoArray *array)
{
	return mono_array_length (array);
}

EMSCRIPTEN_KEEPALIVE MonoObject*
mono_wasm_array_get (MonoArray *array, int idx)
{
	return mono_array_get (array, MonoObject*, idx);
}

EMSCRIPTEN_KEEPALIVE MonoArray*
mono_wasm_obj_array_new (int size)
{
	return mono_array_new (root_domain, mono_get_object_class (), size);
}

EMSCRIPTEN_KEEPALIVE void
mono_wasm_obj_array_set (MonoArray *array, int idx, MonoObject *obj)
{
	mono_array_setref (array, idx, obj);
}

EMSCRIPTEN_KEEPALIVE MonoArray*
mono_wasm_string_array_new (int size)
{
	return mono_array_new (root_domain, mono_get_string_class (), size);
}

EMSCRIPTEN_KEEPALIVE int
mono_wasm_exec_regression (int verbose_level, char *image)
{
	return mono_regression_test_step (verbose_level, image, NULL) ? 0 : 1;
}

EMSCRIPTEN_KEEPALIVE int
mono_wasm_exit (int exit_code)
{
	exit (exit_code);
}

EMSCRIPTEN_KEEPALIVE void
mono_wasm_set_main_args (int argc, char* argv[])
{
	mono_runtime_set_main_args (argc, argv);
}

EMSCRIPTEN_KEEPALIVE int
mono_wasm_strdup (const char *s)
{
	return (int)strdup (s);
}
