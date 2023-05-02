// MIT License
// 
// Copyright (c) 2021 Superluminal (www.superluminal.eu)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#include <config.h>

#if defined (HOST_WIN32)
#include <glib.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/debug-helpers.h>
#include <mono/metadata/debug-internals.h>
#include <mono/metadata/debug-mono-ppdb.h>
#include <mono/metadata/mono-debug.h>
#include <mono/metadata/profiler.h>
#include <mono/metadata/tabledefs.h>
#include <mono/metadata/tokentype.h>
#include <mono/metadata/unity-utils.h>

#include <string.h>

#define WIN32_LEAN_AND_MEAN
#include "Evntcons.h"
#include "evntprov.h"
#include "evntrace.h"
#include "cguid.h"
#include "windows.h"

DECLSPEC_NOINLINE __inline VOID __stdcall Private_EventControlCallback (_In_ LPCGUID SourceId, _In_ ULONG ControlCode, _In_ UCHAR Level, _In_ ULONGLONG MatchAnyKeyword, _In_ ULONGLONG MatchAllKeyword, _In_opt_ PEVENT_FILTER_DESCRIPTOR FilterData, _Inout_opt_ PVOID CallbackContext);
#define MCGEN_PRIVATE_ENABLE_CALLBACK_V2 Private_EventControlCallback
#include "CLR-ETW-Generated.h"

#define MAX_NUM_OFFSETS 7000
#define ENABLE_VERBOSE_LOGGING 0

static gboolean is_initialized = FALSE;

#if ENABLE_VERBOSE_LOGGING
	static void
	sLogInternal(const char* inFormat, ...)
	{
		va_list args;
		va_start (args, inFormat);

		char buffer[1024] = { 0 };
		vsprintf_s (buffer, 1024, inFormat, args);

		va_end(args);

		OutputDebugStringA (buffer);
	}

	#define ETW_PROFILER_LOG_ARGS(format, ...) sLogInternal(": "format"\n", __VA_ARGS__)
	#define ETW_PROFILER_LOG(str) sLogInternal("ETW_PROFILER: %s\n", str)
#else
	#define ETW_PROFILER_LOG_ARGS(format, ...)
	#define ETW_PROFILER_LOG(str)
#endif

static void
image_event (MonoImage *image, gboolean is_rundown, gboolean is_unload)
{
	if (!MICROSOFT_WINDOWS_DOTNETRUNTIME_PROVIDER_Context.IsEnabled && !MICROSOFT_WINDOWS_DOTNETRUNTIME_RUNDOWN_PROVIDER_Context.IsEnabled)	{
		ETW_PROFILER_LOG ("Providers not enabled, skipping image_event");
		return;
	}


	// Mono loads ppdb files as "images" marked with metadata-only. We can skip them as they
	// won't ever have executable code.
	if (image->metadata_only)
	{
		ETW_PROFILER_LOG("Skipping image_event for metadata-only image");
		return;
	}

	const char *pdb_path = NULL;
	guint8 pe_guid[16] = {0};
	gint32 pe_age = 0;
	gint32 pe_timestamp = 0;

	// We only emit PDB info for loads *and* if the image is not a dynamic image (i.e. containing dynamic methods). This is becuase
	// dynamic images do not represent an actual on-disk image and so don't have any PDB info (calling mono_ppdb_get_signature will lead to a crash)
	if (!is_unload && !mono_image_is_dynamic (image))
		mono_ppdb_get_signature (image, &pdb_path, pe_guid, &pe_age, &pe_timestamp);

	gunichar2 *image_path_utf16 = u8to16 (mono_image_get_filename (image));
	gunichar2 *pdb_path_utf16 = pdb_path != NULL ? u8to16 (pdb_path) : NULL;

	MonoAssembly *assembly = mono_image_get_assembly (image);

	if (is_rundown) {
		EventWriteModuleDCEnd_V2 ((uint64_t)image, (uint64_t)assembly, 0, 0, image_path_utf16, L"", 0, (GUID *)pe_guid, pe_age, pdb_path_utf16 == NULL ? L"" : pdb_path_utf16, &GUID_NULL, 0, L"");
	} else if (is_unload) {
		EventWriteModuleUnload_V2 ((uint64_t)image, (uint64_t)assembly, 0, 0, image_path_utf16, L"", 0, (GUID *)pe_guid, pe_age, pdb_path_utf16 == NULL ? L"" : pdb_path_utf16, &GUID_NULL, 0, L"");
	} else {
		EventWriteModuleLoad_V2 ((uint64_t)image, (uint64_t)assembly, 0, 0, image_path_utf16, L"", 0, (GUID *)pe_guid, pe_age, pdb_path_utf16 == NULL ? L"" : pdb_path_utf16, &GUID_NULL, 0, L"");
	}

	g_free (image_path_utf16);
	g_free (pdb_path_utf16);
}

static void
image_loaded (MonoProfiler *prof, MonoImage *image)
{
	image_event (image, FALSE, FALSE);
}

static void
image_unloading (MonoProfiler *prof, MonoImage *image)
{
	image_event (image, FALSE, TRUE);
}

static void
method_load (MonoDomain *domain, MonoMethod *method, MonoJitInfo *jinfo, gboolean is_rundown)
{
	static __declspec(thread) unsigned int il_offsets[MAX_NUM_OFFSETS] = {0};
	static __declspec(thread) unsigned int native_offsets[MAX_NUM_OFFSETS] = {0};

	if (!MICROSOFT_WINDOWS_DOTNETRUNTIME_PROVIDER_Context.IsEnabled && !MICROSOFT_WINDOWS_DOTNETRUNTIME_RUNDOWN_PROVIDER_Context.IsEnabled) {
		ETW_PROFILER_LOG ("Providers not enabled, skipping method_load");
		return;
	}

	// Ignore trampolines; it's not possible to get any of the regular info we get for other methods from trampoline. They have no debug data,
	// no signature, etc.
	//
	// Note: in the Unity's current Mono version, we don't receive this callback for trampolines anyway (they're filtered out in mono_jit_info_table_foreach).
	// However, in a future Mono version, the is_trampoline filter will be removed from mono_jit_info_table_foreach, so we keep this check here so that it will
	// work regardless of which Mono version is used.
	if (jinfo->is_trampoline) {
		return;
	}

	int compressed_num_lines = 0;

	char *sourceFilePath = NULL;
	MonoDebugMethodJitInfo *dmji = mono_debug_find_method (method, domain);
	if (dmji != NULL) {
		// There are a few things to keep in mind when emitting IL/native offset mapping data:
		//
		// 1) Debug data redundancy
		//
		// The way Mono works is:
		// - IL is converted to IR
		// - IR is converted to ASM
		//
		// Each step is a 1:N mapping: 1 IL instruction can result in multiple IR instructions and 1 IR instruction can result in multiple asm instructions.
		// Mono's debug data contains IL offset -> native offset mapping data for each IR instruction. This means that there could be multiple entries for
		// a single IL instruction, depending on how many IR instructions were involved. These entries will all have the same IL offset, but a different
		// native offset.
		//
		// For our purposes, this is redundant information: we only care about the fact that a range of ASM instructions maps to a specific IL offsets,
		// we don't care about the individual ASM ranges.
		//
		// So, the following loop eliminates this redundancy by only emitting a new IL/native offset pair when the IL offset changes.
		// This means that each entry in the resulting table basically represents a range in  both IL and native space.
		//
		// 2) ETW max event sizes
		//
		// In order to communicate the IL/native offset mapping, the MethodILToNativeMap event from the .NET provider is used. This event takes two arrays,
		// along with a count. However, there is a limit: ETW events can have a maximum of 64KiB - sizeof(EVENT_HEADER) of data. This is problematic,
		// because the size of the data needed for the MethodILToNativeMap grows with the length of the function that was compiled, and is basically unbounded.
		// With the redundancy improvement from the previous section, the chance of this happening is low, but it's still possible if you have a large enough
		// function (think 5k+ lines).
		//
		// When the event size limit is exceeded, EventWriteMethodILToNativeMap will return ERROR_ARITHMETIC_OVERFLOW (through EventWriteTransfer) and the
		// event will not be written to the file. However, the ETW recorder will notice this happening and will record this as a lost event. So, when this happens
		// the user will get an 'XX events lost' warning when opening this trace in a profiler that understands ETW.
		//
		// In order to prevent this scary warning, we make sure we only ever emit a maximum number of IL/native offsets. This is currently set to 7000 (see MAX_NUM_OFFSETS),
		// which was taken from .NET Core's ETW provider which does the same thing; see https://github.com/dotnet/runtime/blob/5fa6dd364982be4ffd83358adbf130d88049c72a/src/coreclr/vm/eventtrace.cpp#L6859.
		//
		// This does have the effect that for functions where this happens, not all asm instructions will be able to be mapped to lines. A better fix would be to simply
		// emit multiple MethodILToNativeMap events to ensure all data is recorded. However, existing tooling around .NET/ETW (i.e. PerfView, WPA, etc) most likely can't
		// deal with multiple of these events happening for the same method, so for compatibility reasons we've decided not to do that.
		uint32_t last_il_offset = -1;
		for (int i = 0; i < (int)dmji->num_line_numbers && compressed_num_lines < MAX_NUM_OFFSETS; ++i) {
			if (dmji->line_numbers[i].il_offset != last_il_offset) {

				last_il_offset = dmji->line_numbers[i].il_offset;

				native_offsets[compressed_num_lines] = dmji->line_numbers[i].native_offset;
				il_offsets[compressed_num_lines] = dmji->line_numbers[i].il_offset;

				compressed_num_lines++;
			}
		}

		mono_debug_free_method_jit_info (dmji);
	}

	MonoClass *klass = mono_method_get_class (method);
	char *signature = mono_signature_get_desc (mono_method_signature (method), TRUE);
	char *full_class_name = g_strdup_printf ("%s.%s", mono_class_get_name (klass), mono_method_get_name (method));
	const char *namespace = mono_class_get_namespace (klass);
	gpointer code_start = mono_jit_info_get_code_start (jinfo);
	int code_size = mono_jit_info_get_code_size (jinfo);
	MonoImage *image = mono_class_get_image (klass);
	uint32_t method_token = mono_unity_method_get_token (method);

	gunichar2 *namespace_utf16 = u8to16 (namespace);
	gunichar2 *full_class_name_utf16 = u8to16 (full_class_name);
	gunichar2 *signature_utf16 = u8to16 (signature);

	if (is_rundown) {
		EventWriteMethodDCEndVerbose_V2 ((uint64_t)method, (uint64_t)image, (uint64_t)code_start, code_size, method_token, 0x4, namespace_utf16, full_class_name_utf16, signature_utf16, 0, 0);
		EventWriteMethodDCEndILToNativeMap ((uint64_t)method, 0, 0, compressed_num_lines, il_offsets, native_offsets, 0);
	} else {
		EventWriteMethodLoadVerbose_V2 ((uint64_t)method, (uint64_t)image, (uint64_t)code_start, code_size, method_token, 0x4, namespace_utf16, full_class_name_utf16, signature_utf16, 0, 0);
		EventWriteMethodILToNativeMap ((uint64_t)method, 0, 0, compressed_num_lines, il_offsets, native_offsets, 0);
	}

	g_free (signature_utf16);
	g_free (full_class_name_utf16);
	g_free (namespace_utf16);
	g_free (sourceFilePath);
	g_free (full_class_name);
}

struct JITEnumerationData {
	int mNumDomains;
	int mNumAssemblies;
	int mNumMethods;
};

static void
method_jit_done (MonoProfiler *prof, MonoMethod *method, MonoJitInfo *jinfo)
{
	method_load (mono_domain_get (), method, jinfo, FALSE);
}

static void
on_enumerate_assembly (MonoAssembly *assembly, void *user_data)
{
	struct JITEnumerationData *enumerationData = (struct JITEnumerationData *)user_data;
	enumerationData->mNumAssemblies++;

	MonoImage *image = mono_assembly_get_image (assembly);
	image_event (image, TRUE, FALSE);
}

static void
on_enumerate_jit_method (MonoDomain *domain, MonoMethod *method, MonoJitInfo *jinfo, void *user_data)
{
	struct JITEnumerationData *enumerationData = (struct JITEnumerationData *)user_data;
	enumerationData->mNumMethods++;

	method_load (domain, method, jinfo, TRUE);
}

static void
on_enumerate_domain (MonoDomain *domain, void *user_data)
{
	struct JITEnumerationData *enumerationData = (struct JITEnumerationData *)user_data;
	enumerationData->mNumDomains++;

	// Iterate through each assembly
	mono_domain_assembly_foreach (domain, on_enumerate_assembly, enumerationData);

	// Iterate through each JIT'ed method
	mono_domain_jit_foreach (domain, on_enumerate_jit_method, enumerationData);
}

static void
on_attach ()
{
	ETW_PROFILER_LOG ("Enumerating JIT data...");

	struct JITEnumerationData enumerationData;
	memset (&enumerationData, 0, sizeof (enumerationData));

	// Iterate through each domain
	mono_domain_foreach (on_enumerate_domain, &enumerationData);

	ETW_PROFILER_LOG_ARGS ("Finished enumerating JIT data. Found %d domains, %d assemblies, %d methods", enumerationData.mNumDomains, enumerationData.mNumAssemblies, enumerationData.mNumMethods);
}

// This callback is called by the ETW system when tracing is started / stopped. We use it to enumerate & output information about JIT compilation that happened *before* tracing started.
DECLSPEC_NOINLINE __inline VOID __stdcall Private_EventControlCallback (_In_ LPCGUID SourceId, _In_ ULONG ControlCode, _In_ UCHAR Level, _In_ ULONGLONG MatchAnyKeyword, _In_ ULONGLONG MatchAllKeyword, _In_opt_ PEVENT_FILTER_DESCRIPTOR FilterData, _Inout_opt_ PVOID CallbackContext)
{
	ETW_PROFILER_LOG_ARGS ("EventControlCallback (%d)", ControlCode);

	switch (ControlCode) {
	case EVENT_CONTROL_CODE_ENABLE_PROVIDER: {
		gboolean isRegular = (CallbackContext == &MICROSOFT_WINDOWS_DOTNETRUNTIME_PROVIDER_Context);
		gboolean isRundown = (CallbackContext == &MICROSOFT_WINDOWS_DOTNETRUNTIME_RUNDOWN_PROVIDER_Context);
		ETW_PROFILER_LOG_ARGS("EventControlCallback -- IsInitialized: %s, IsRuntime: %s, IsRundown: %s", is_initialized ? "true" : "false", isRegular ? "true" : "false", isRundown ? "true" : "false");
		if (is_initialized && CallbackContext == &MICROSOFT_WINDOWS_DOTNETRUNTIME_RUNDOWN_PROVIDER_Context) {
			on_attach ();
		}
	} break;
	default:
		break;
	};
}

void
mono_profiler_cleanup_etw(MonoProfiler *prof)
{
	EventUnregisterMicrosoft_Windows_DotNETRuntimeRundown ();
	EventUnregisterMicrosoft_Windows_DotNETRuntime ();
}

/* the entry point */
MONO_API void
mono_profiler_init_etw (const char *desc)
{
	ETW_PROFILER_LOG ("Initializing Plugin");

	EventRegisterMicrosoft_Windows_DotNETRuntime ();
	EventRegisterMicrosoft_Windows_DotNETRuntimeRundown ();

	// We currently need debug info to be able to read out the line number information, so force enable it here.
	if (!mono_debug_enabled ())
		mono_debug_init (MONO_DEBUG_FORMAT_MONO);

	MonoProfilerHandle handle = mono_profiler_create (NULL);
	mono_profiler_set_image_loaded_callback (handle, image_loaded);
	mono_profiler_set_image_unloading_callback (handle, image_unloading);
	mono_profiler_set_jit_done_callback (handle, method_jit_done);
	mono_profiler_set_cleanup_callback(handle, mono_profiler_cleanup_etw);

	is_initialized = TRUE;

	ETW_PROFILER_LOG ("Plugin Initialized");
}
#endif // HOST_WIN32
