//
//  runtime-bootstrap.c
//  MonoTestRunner
//
//  Created by Rodrigo Kumpera on 3/30/17.
//  Copyright © 2017 Rodrigo Kumpera. All rights reserved.
//
#include <stdio.h>
#include <stdlib.h>
#include <os/log.h>
#include <sys/stat.h>
#include <errno.h>
#include <string.h>

#include <TargetConditionals.h>


#include "runtime-bootstrap.h"


typedef enum {
        MONO_IMAGE_OK,
        MONO_IMAGE_ERROR_ERRNO,
        MONO_IMAGE_MISSING_ASSEMBLYREF,
        MONO_IMAGE_IMAGE_INVALID
} MonoImageOpenStatus;

enum {
        MONO_DL_EAGER = 0,
        MONO_DL_LAZY  = 1,
        MONO_DL_LOCAL = 2,
        MONO_DL_MASK  = 3
};

typedef struct MonoDomain_ MonoDomain;
typedef struct MonoAssembly_ MonoAssembly;
typedef struct MonoMethod_ MonoMethod;
typedef struct MonoException_ MonoException;
typedef struct MonoString_ MonoString;
typedef struct MonoClass_ MonoClass;
typedef struct MonoImage_ MonoImage;
typedef struct MonoObject_ MonoObject;
typedef struct MonoThread_ MonoThread;


/*
typedef void* (*MonoDlFallbackLoad) (const char *name, int flags, char **err, void *user_data);
typedef void* (*MonoDlFallbackSymbol) (void *handle, const char *name, char **err, void *user_data);
typedef void* (*MonoDlFallbackClose) (void *handle, void *user_data);
typedef void *(*mono_dl_fallback_register_fn) (MonoDlFallbackLoad load_func, MonoDlFallbackSymbol symbol_func, MonoDlFallbackClose close_func, void *user_data);
typedef int (*mono_jit_exec_fn) (MonoDomain *domain, MonoAssembly *assembly, int argc, char *argv[]);


typedef void (*mono_free_fn) (void*);

*/
extern MonoDomain* mono_domain_get (void);
extern char* mono_string_to_utf8 (MonoString *string_obj);
extern void mono_domain_set_config (MonoDomain *, const char *, const char *);
extern MonoThread *mono_thread_attach (MonoDomain *domain);
extern void mono_set_assemblies_path (const char* path);
extern void mono_set_crash_chaining (int);
extern void mono_set_signal_chaining (int);
extern MonoDomain* mono_jit_init_version (const char *root_domain_name, const char *runtime_version);
extern int mono_runtime_set_main_args (int argc, char* argv[]);
extern MonoString* mono_string_new (MonoDomain *domain, const char *text);
extern MonoClass* mono_class_from_name_case (MonoImage *image, const char* name_space, const char *name);
extern MonoImage* mono_assembly_get_image (MonoAssembly *assembly);
extern MonoMethod* mono_class_get_method_from_name (MonoClass *klass, const char *name, int param_count);
extern MonoObject* mono_runtime_invoke (MonoMethod *method, void *obj, void **params, MonoObject **exc);
extern MonoString* mono_object_to_string (MonoObject *obj, MonoObject **exc);
extern MonoClass* mono_class_from_name (MonoImage *image, const char* name_space, const char *name);
extern MonoAssembly* mono_assembly_open (const char *filename, MonoImageOpenStatus *status);


//from runtime-objc-helpers.m
extern const char * runtime_get_bundle_path (void);

static void
_log (const char *format, ...)
{
	char *ret;
	va_list args;

	va_start (args, format);	
	vasprintf (&ret, format, args);
	va_end (args);

	os_log_error (OS_LOG_DEFAULT, "%s", ret);
	free (ret);
}



#define DEFAULT_DIRECTORY_MODE S_IRWXU | S_IRGRP | S_IXGRP | S_IROTH | S_IXOTH
char *
m_strdup_printf (const char *format, ...)
{
	char *ret;
	va_list args;
	int n;

	va_start (args, format);
	n = vasprintf (&ret, format, args);
	va_end (args);
	if (n == -1)
		return NULL;

	return ret;
}

static int
m_make_directory (const char *path, int mode)
{
#if WINDOWS
        return mkdir (path);
#else
        return mkdir (path, mode);
#endif
}

static int
m_create_directory (const char *pathname, int mode)
{
        if (mode <= 0)
                mode = DEFAULT_DIRECTORY_MODE;

        if  (!pathname || *pathname == '\0') {
                errno = EINVAL;
                return -1;
        }

        mode_t oldumask = umask (022);
        char *path = strdup (pathname);
        int rv, ret = 0;
		char *d;
        for (d = path; *d; ++d) {
                if (*d != '/')
                        continue;
                *d = 0;
                if (*path) {
                        rv = m_make_directory (path, mode);
                        if  (rv == -1 && errno != EEXIST)  {
                                ret = -1;
                                break;
                        }
                }
                *d = '/';
        }
        free (path);
        if (ret == 0)
                ret = m_make_directory (pathname, mode);
        umask (oldumask);

        return ret;
}



static void
create_and_set (const char *home, const char *relativePath, const char *envvar)
{
	char *dir = m_strdup_printf ("%s/%s", home, relativePath);
	int rv = m_create_directory (dir, DEFAULT_DIRECTORY_MODE);
	if (rv < 0 && errno != EEXIST)
		_log ("Failed to create XDG directory %s. %s", dir, strerror (errno));
	if (envvar)
		setenv (envvar, dir, 1);
	free (dir);
}

/////

static MonoDomain *root_domain;
static MonoAssembly *main_assembly;
static char *assemblies_dir;

int
set_main (void)
{
	int argc = 1;
	char *argv[] = { "main.exe" };
	char *main_assembly_name = "main.exe";
	char buff[1024];

	mono_thread_attach (root_domain);
	sprintf (buff, "%s/%s", assemblies_dir, main_assembly_name);
	main_assembly = mono_assembly_open (buff, NULL);

	mono_runtime_set_main_args (argc, argv);

	return 0;
}

static MonoMethod *send_method;

char*
runtime_send_message (const char *key, const char *value)
{

	mono_thread_attach (root_domain);

	void * params[] = {
		mono_string_new (mono_domain_get (), key),
		mono_string_new (mono_domain_get (), value),
	};

	if (!send_method) {
		MonoClass *driver_class = mono_class_from_name (mono_assembly_get_image (main_assembly), "", "Driver");
		send_method = mono_class_get_method_from_name (driver_class, "Send", -1);
	}

	MonoException *exc = NULL;
	_log ("SEND METHOD %p", send_method);
	
	MonoString *res = (MonoString *)mono_runtime_invoke (send_method, NULL, params, (MonoObject**)&exc);
	_log ("SEND DONE res %p exc %p", res, exc);

	if (exc) {
		MonoException *second_exc = NULL;
		res = mono_object_to_string ((MonoObject*)exc, (MonoObject**)&second_exc);
		if (second_exc)
			res = mono_string_new (mono_domain_get (), "DOUBLE FAULTED EXCEPTION");
	}

	char *str_res = NULL;
	if (res)
		str_res = mono_string_to_utf8 (res);
	else
		str_res = strdup ("<NULL>");
	_log ("SEND MESSAGE RES: %s", str_res);
	return str_res;
}

void
init_runtime (void)
{
	const char *bundle_path = runtime_get_bundle_path ();

    _log (">>>>>>>>>RUNTIME INIT GOES HERE 233");
    _log ( ">>>>>>>>>PATH IS %s", bundle_path);
    _log (">>>>>>>>>TMP IS %s", getenv ("TMPDIR"));
    _log ( ">>>>>>>>>HOME IS %s", getenv ("HOME"));

	assemblies_dir = m_strdup_printf ("%s/managed", bundle_path);
	//MUST HAVE envs
	// setenv ("TMPDIR", cache_dir, 1);
	setenv ("MONO_CFG_DIR", bundle_path, 1);

	create_and_set (bundle_path, ".local/share", "XDG_DATA_HOME");
	create_and_set (bundle_path, ".config", "XDG_CONFIG_HOME");

	mono_set_assemblies_path (assemblies_dir);
#if defined (TARGET_IPHONE_SIMULATOR)
	mono_set_crash_chaining (1);
	mono_set_signal_chaining (1);
	// mono_dl_fallback_register (my_dlopen, my_dlsym, NULL, NULL);
	root_domain = mono_jit_init_version ("TEST RUNNER", "mobile");
#endif
	mono_domain_set_config (root_domain, assemblies_dir, bundle_path);

	// sprintf (buff, "%s/libruntime-bootstrap.so", data_dir);
	// runtime_bootstrap_dso = dlopen (buff, RTLD_LAZY);

	// sprintf (buff, "%s/libMonoPosixHelper.so", data_dir);
	// mono_posix_helper_dso = dlopen (buff, RTLD_LAZY);

	_log ("READY");
	set_main ();

	// _log ("DOING IT");
	// _log ("RETURNED %s\n", runtime_send_message ("hello", "world"));
}
