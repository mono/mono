#include <config.h>
#include "mini.h"
#include "mini-runtime.h"
#include <mono/metadata/assembly.h>
#include <mono/metadata/assembly-internals.h>
#include <mono/utils/mono-logger-internals.h>

MONO_API int coreclr_initialize (const char* exePath, const char* appDomainFriendlyName,
	int propertyCount, const char** propertyKeys, const char** propertyValues,
	void** hostHandle, unsigned int* domainId);

MONO_API int coreclr_execute_assembly (void* hostHandle, unsigned int domainId,
	int argc, const char** argv,
	const char* managedAssemblyPath, unsigned int* exitCode);

MONO_API int coreclr_shutdown_2 (void* hostHandle, unsigned int domainId, int* latchedExitCode);

typedef struct {
	int assembly_count;
	char **basenames; /* Foo.dll */
	char **assembly_filepaths; /* /blah/blah/blah/Foo.dll */
} MonoCoreTrustedPlatformAssemblies;

static MonoCoreTrustedPlatformAssemblies *trusted_platform_assemblies;

static void
mono_core_trusted_platform_assemblies_free (MonoCoreTrustedPlatformAssemblies *a)
{
	if (!a)
		return;
	g_strfreev (a->basenames);
	g_strfreev (a->assembly_filepaths);
	g_free (a);
}

static gboolean
parse_trusted_platform_assemblies (const char *assemblies_paths)
{
	// From
	// https://docs.microsoft.com/en-us/dotnet/core/tutorials/netcore-hosting#step-3---prepare-runtime-properties
	// this is ';' separated on Windows and ':' separated elsewhere.
	char **parts = g_strsplit (assemblies_paths, G_SEARCHPATH_SEPARATOR_S, 0);
	int asm_count = 0;
	for (char **p = parts; *p != NULL && **p != '\0'; p++) {
#if 0
		const char *part = *p;
		// can't use logger, it's not initialized yet.
		printf ("\t\tassembly %d = <%s>\n", asm_count, part);
#endif
		asm_count++;
	}
	MonoCoreTrustedPlatformAssemblies *a = g_new0 (MonoCoreTrustedPlatformAssemblies, 1);
	a->assembly_count = asm_count;
	a->assembly_filepaths = parts;
	a->basenames = g_new0 (char*, asm_count + 1);
	for (int i = 0; i < asm_count; ++i) {
		a->basenames[i] = g_path_get_basename (a->assembly_filepaths [i]);
	}
	a->basenames [asm_count] = NULL;

	trusted_platform_assemblies = a;
	return TRUE;
}

static MonoAssembly*
mono_core_preload_hook (MonoAssemblyName *aname, char **unused_apaths, void *user_data)
{
	MonoAssembly *result = NULL;
	MonoCoreTrustedPlatformAssemblies *a = (MonoCoreTrustedPlatformAssemblies *)user_data;
	const gboolean refonly = FALSE; /* TODO: make a refonly preload hook, too */
	/* TODO: check that CoreCLR wants the strong name semantics here */
	MonoAssemblyCandidatePredicate predicate = &mono_assembly_candidate_predicate_sn_same_name;
	void* predicate_ud = aname;

	g_assert (aname);
	g_assert (aname->name);

	char *basename = g_strconcat (aname->name, ".dll", NULL); /* TODO: make sure CoreCLR never needs to load .exe files */

	for (int i = 0; i < a->assembly_count; ++i) {
		if (!strcmp (basename, a->basenames[i])) {
			MonoAssemblyOpenRequest req;
			mono_assembly_request_prepare (&req.request, sizeof (req), refonly ? MONO_ASMCTX_REFONLY : MONO_ASMCTX_DEFAULT);
			req.request.predicate = predicate;
			req.request.predicate_ud = predicate_ud;

			const char *fullpath = a->assembly_filepaths[i];

			gboolean found = g_file_test (fullpath, G_FILE_TEST_IS_REGULAR);

			if (found) {
				MonoImageOpenStatus status;
				result = mono_assembly_request_open (fullpath, &req, &status);
				/* TODO: do something with the status at the end? */
				if (result)
					break;
			}
		}
	}

	g_free (basename);

	if (!result) {
		mono_trace (G_LOG_LEVEL_DEBUG, MONO_TRACE_ASSEMBLY, "netcore preload hook: did not find '%s'.\n", aname->name);
	} else {
		mono_trace (G_LOG_LEVEL_INFO, MONO_TRACE_ASSEMBLY, "netcore preload hook: loading '%s' from '%s'.\n", aname->name, result->image->name);
	}
	return result;
}

static void
install_assembly_loader_hooks (void)
{
	mono_install_assembly_preload_hook (mono_core_preload_hook, (void*)trusted_platform_assemblies);
}

static gboolean
parse_properties (int propertyCount, const char** propertyKeys, const char** propertyValues)
{
	// The a partial list of relevant properties is
	// https://docs.microsoft.com/en-us/dotnet/core/tutorials/netcore-hosting#step-3---prepare-runtime-properties
	// TODO: We should also pick up at least
	//  APP_PATHS, APP_NI_PATHS and NATIVE_DLL_SEARCH_DIRECTORIES
	//
	//  and PLATFORM_RESOURCE_ROOTS for satellite assemblies in culture-specific subdirectories

	for (int i = 0; i < propertyCount; ++i) {
		if (!strcmp (propertyKeys[i], "TRUSTED_PLATFORM_ASSEMBLIES")) {
			parse_trusted_platform_assemblies (propertyValues[i]);
		} else {
#if 0
			// can't use mono logger, it's not initialized yet.
			printf ("\t Unprocessed property %03d '%s': <%s>\n", i, propertyKeys[i], propertyValues[i]);
#endif
		}
	}
	return TRUE;
}

//
// Initialize the CoreCLR. Creates and starts CoreCLR host and creates an app domain
//
// Parameters:
//  exePath                 - Absolute path of the executable that invoked the ExecuteAssembly
//  appDomainFriendlyName   - Friendly name of the app domain that will be created to execute the assembly
//  propertyCount           - Number of properties (elements of the following two arguments)
//  propertyKeys            - Keys of properties of the app domain
//  propertyValues          - Values of properties of the app domain
//  hostHandle              - Output parameter, handle of the created host
//  domainId                - Output parameter, id of the created app domain 
//
// Returns:
//  HRESULT indicating status of the operation. S_OK if the assembly was successfully executed
//
int coreclr_initialize (const char* exePath, const char* appDomainFriendlyName,
	int propertyCount, const char** propertyKeys, const char** propertyValues,
	void** hostHandle, unsigned int* domainId)
{
	// TODO: TRUSTED_PLATFORM_ASSEMBLIES is the property key for managed assemblies mapping
	if (!parse_properties (propertyCount, propertyKeys, propertyValues))
		return 0x80004005; /* E_FAIL */

	install_assembly_loader_hooks ();

	return 0;
}


//
// Execute a managed assembly with given arguments
//
// Parameters:
//  hostHandle              - Handle of the host
//  domainId                - Id of the domain 
//  argc                    - Number of arguments passed to the executed assembly
//  argv                    - Array of arguments passed to the executed assembly
//  managedAssemblyPath     - Path of the managed assembly to execute (or NULL if using a custom entrypoint).
//  exitCode                - Exit code returned by the executed assembly
//
// Returns:
//  HRESULT indicating status of the operation. S_OK if the assembly was successfully executed
//
int coreclr_execute_assembly (void* hostHandle, unsigned int domainId,
	int argc, const char** argv,
	const char* managedAssemblyPath, unsigned int* exitCode)
{
	if (exitCode == NULL)
	{
		return -1;
	}

	//
	// Make room for program name and executable assembly
	//
	int mono_argc = argc + 2;

	char **mono_argv = (char **) malloc (sizeof (char *) * (mono_argc + 1 /* null terminated */));
	const char **ptr = (const char **) mono_argv;
	
	*ptr++ = NULL;

	// executable assembly
	*ptr++ = (char*) managedAssemblyPath;

	// the rest
	for (int i = 0; i < argc; ++i)
		*ptr++ = argv [i];

	*ptr = NULL;

	mono_parse_env_options (&mono_argc, &mono_argv);
	*exitCode = mono_main (mono_argc, mono_argv);

	return 0;
}

//
// Shutdown CoreCLR. It unloads the app domain and stops the CoreCLR host.
//
// Parameters:
//  hostHandle              - Handle of the host
//  domainId                - Id of the domain 
//
// Returns:
//  HRESULT indicating status of the operation. S_OK if the assembly was successfully executed
//
int coreclr_shutdown_2 (void* hostHandle, unsigned int domainId, int* latchedExitCode)
{
	MonoCoreTrustedPlatformAssemblies *a = trusted_platform_assemblies;
	trusted_platform_assemblies = NULL;
	mono_core_trusted_platform_assemblies_free (a);

	return 0;
}
