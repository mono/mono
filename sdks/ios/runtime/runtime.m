/* -*- Mode: C; tab-width: 4; indent-tabs-mode: t; c-basic-offset: 4 -*- */
//
//  runtime.m
//

#import <Foundation/Foundation.h>
#import <os/log.h>
#include <mono/utils/mono-publib.h>
#include <mono/utils/mono-logger.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/mono-debug.h>
#include <mono/metadata/exception.h>
#include <mono/jit/jit.h>

#include <sys/stat.h>

//
// Based on runtime/ in xamarin-macios
//

#define PRINT(...) do { printf (__VA_ARGS__); } while (0);

bool
file_exists (const char *path)
{
	struct stat buffer;
	return stat (path, &buffer) == 0;
}

static char *bundle_path;

const char *
get_bundle_path (void)
{
	if (bundle_path)
		return bundle_path;

	NSBundle *main_bundle = [NSBundle mainBundle];
	NSString *path;
	char *result;

	path = [main_bundle bundlePath];
	bundle_path = strdup ([path UTF8String]);

	return bundle_path;
}

static MonoAssembly*
load_assembly (const char *name, const char *culture)
{
	const char *bundle = get_bundle_path ();
	char path [1024];
	int res;

	os_log_info (OS_LOG_DEFAULT, "assembly_preload_hook: %s %s %s\n", name, culture, bundle);
	if (culture && strcmp (culture, ""))
		res = snprintf (path, sizeof (path) - 1, "%s/%s/%s", bundle, culture, name);
	else
		res = snprintf (path, sizeof (path) - 1, "%s/%s", bundle, name);
	assert (res > 0);

	if (file_exists (path)) {
		MonoAssembly *assembly = mono_assembly_open (path, NULL);
		assert (assembly);
		return assembly;
	}
	return NULL;
}

static MonoAssembly*
assembly_preload_hook (MonoAssemblyName *aname, char **assemblies_path, void* user_data)
{
	const char *name = mono_assembly_name_get_name (aname);
	const char *culture = mono_assembly_name_get_culture (aname);

	return load_assembly (name, culture);
}

char *
strdup_printf (const char *msg, ...)
{
	va_list args;
	char *formatted = NULL;

	va_start (args, msg);
	vasprintf (&formatted, msg, args);
	va_end (args);

	return formatted;
}

static MonoObject *
fetch_exception_property (MonoObject *obj, const char *name, bool is_virtual)
{
	MonoMethod *get = NULL;
	MonoMethod *get_virt = NULL;
	MonoObject *exc = NULL;

	get = mono_class_get_method_from_name (mono_get_exception_class (), name, 0);
	if (get) {
		if (is_virtual) {
			get_virt = mono_object_get_virtual_method (obj, get);
			if (get_virt)
				get = get_virt;
		}

		return (MonoObject *) mono_runtime_invoke (get, obj, NULL, &exc);
	} else {
		PRINT ("Could not find the property System.Exception.%s", name);
	}

	return NULL;
}

static char *
fetch_exception_property_string (MonoObject *obj, const char *name, bool is_virtual)
{
	MonoString *str = (MonoString *) fetch_exception_property (obj, name, is_virtual);
	return str ? mono_string_to_utf8 (str) : NULL;
}

void
unhandled_exception_handler (MonoObject *exc, void *user_data)
{
	NSMutableString *msg = [[NSMutableString alloc] init];

	MonoClass *type = mono_object_get_class (exc);
	char *type_name = strdup_printf ("%s.%s", mono_class_get_namespace (type), mono_class_get_name (type));
	char *trace = fetch_exception_property_string (exc, "get_StackTrace", true);
	char *message = fetch_exception_property_string (exc, "get_Message", true);

	[msg appendString:@"Unhandled managed exception:\n"];
	[msg appendFormat: @"%s (%s)\n%s\n", message, type_name, trace ? trace : ""];

	free (trace);
	free (message);
	free (type_name);

	os_log_info (OS_LOG_DEFAULT, "%@", msg);
	os_log_info (OS_LOG_DEFAULT, "Exit code: %d.", 1);
	exit (1);
}

void
log_callback (const char *log_domain, const char *log_level, const char *message, mono_bool fatal, void *user_data)
{
	os_log_info (OS_LOG_DEFAULT, "(%s %s) %s", log_domain, log_level, message);
	if (fatal) {
		os_log_info (OS_LOG_DEFAULT, "Exit code: %d.", 1);
		exit (1);
	}
}

void
mono_ios_runtime_init (void)
{
	int res;

	id args_array = [[NSProcessInfo processInfo] arguments];
	int nargs = [args_array count];
	char **args;

	args = malloc (nargs * sizeof (char*));
	for (int i = 0; i < nargs; ++i)
		args [i] = strdup ([((NSString*)[args_array objectAtIndex: i]) UTF8String]);

	int aindex = 1;
	while (aindex < nargs) {
		char *arg = args [aindex];
		if (!(arg [0] == '-' && arg [1] == '-'))
			break;
		if (strstr (arg, "--setenv=") == arg) {
			char *p = arg + strlen ("--setenv=");
			char *eq = strstr (p, "=");
			assert (eq);
			*eq = '\0';
			char *name = strdup (p);
			char *val = strdup (eq + 1);
			os_log_info (OS_LOG_DEFAULT, "%s=%s.", name, val);
			setenv (name, val, TRUE);
		}
		aindex ++;
	}
	assert (aindex < nargs);
    char *executable = args [aindex];
	aindex ++;

	const char *bundle = get_bundle_path ();
	chdir (bundle);

	mono_debug_init (MONO_DEBUG_FORMAT_MONO);
	mono_install_assembly_preload_hook (assembly_preload_hook, NULL);
	mono_install_unhandled_exception_hook (unhandled_exception_handler, NULL);
	mono_trace_set_log_handler (log_callback, NULL);
	mono_set_signal_chaining (TRUE);
	mono_set_crash_chaining (TRUE);

	mono_jit_init_version ("Mono.ios", "mobile");

	MonoAssembly *assembly = load_assembly (executable, NULL);
	assert (assembly);

	os_log_info (OS_LOG_DEFAULT, "Executable: %s", executable);
	int managed_argc = nargs - aindex;
	char *managed_argv [128];
	assert (managed_argc < 128 - 2);
	int managed_aindex = 0;
	managed_argv [managed_aindex ++] = "test-runner";
	for (int i = 0; i < managed_argc; ++i) {
		managed_argv [managed_aindex] = args [aindex];
		os_log_info (OS_LOG_DEFAULT, "Arg: %s", managed_argv [managed_aindex]);
		managed_aindex ++;
		aindex ++;
	}
	managed_argv [managed_aindex] = NULL;
	managed_argc = managed_aindex;

	res = mono_jit_exec (mono_domain_get (), assembly, managed_argc, managed_argv);
	// Print this so apps parsing logs can detect when we exited
	os_log_info (OS_LOG_DEFAULT, "Exit code: %d.", res);
	exit (res);
}

//
// ICALLS used by the mobile profile of mscorlib
//

void*
xamarin_timezone_get_data (const char *name, int *size)
{
	NSTimeZone *tz = nil;
	if (name) {
		NSString *n = [[NSString alloc] initWithUTF8String: name];
		tz = [[NSTimeZone alloc] initWithName:n];
	} else {
		tz = [NSTimeZone localTimeZone];
	}
	NSData *data = [tz data];
	*size = [data length];
	void* result = malloc (*size);
	memcpy (result, data.bytes, *size);
	return result;
}

char**
xamarin_timezone_get_names (int *count)
{
	// COOP: no managed memory access: any mode.
	NSArray *array = [NSTimeZone knownTimeZoneNames];
	*count = array.count;
	char** result = (char**) malloc (sizeof (char*) * (*count));
	for (int i = 0; i < *count; i++) {
		NSString *s = [array objectAtIndex: i];
		result [i] = strdup (s.UTF8String);
	}
	return result;
}

// called from mono-extensions/mcs/class/corlib/System/Environment.iOS.cs
const char *
xamarin_GetFolderPath (int folder)
{
	// COOP: no managed memory access: any mode.
	// NSUInteger-based enum (and we do not want corlib exposed to 32/64 bits differences)
	NSSearchPathDirectory dd = (NSSearchPathDirectory) folder;
	NSURL *url = [[[NSFileManager defaultManager] URLsForDirectory:dd inDomains:NSUserDomainMask] lastObject];
	NSString *path = [url path];
	return strdup ([path UTF8String]);
}


// mcs/class/corlib/System/Console.iOS.cs
void
xamarin_log (const unsigned short *unicodeMessage)
{
	// COOP: no managed memory access: any mode.
	int length = 0;
	const unsigned short *ptr = unicodeMessage;
	while (*ptr++)
		length += sizeof (unsigned short);
	NSString *msg = [[NSString alloc] initWithBytes: unicodeMessage length: length encoding: NSUTF16LittleEndianStringEncoding];

#if TARGET_OS_WATCH && defined (__arm__) // maybe make this configurable somehow?
	const char *utf8 = [msg UTF8String];
	int len = strlen (utf8);
	fwrite (utf8, 1, len, stdout);
	if (len == 0 || utf8 [len - 1] != '\n')
		fwrite ("\n", 1, 1, stdout);
	fflush (stdout);
#else
	os_log_info (OS_LOG_DEFAULT, "%@", msg);
#endif
}
