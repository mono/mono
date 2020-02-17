/*
 * Copyright (C) 2009 The Android Open Source Project
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */
#include <string.h>
#include <stdlib.h>
#include <stdarg.h>
#include <stdio.h>
#include <pthread.h>
#include <dlfcn.h>
#include <unistd.h>
#include <sys/stat.h>
#include <dirent.h>
#include <errno.h>

#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>

#include <jni.h>

#include <linux/prctl.h>
#include <android/log.h>
#include <sys/system_properties.h>

#define DEFAULT_DIRECTORY_MODE S_IRWXU | S_IRGRP | S_IXGRP | S_IROTH | S_IXOTH


typedef enum {
        MONO_IMAGE_OK,
        MONO_IMAGE_ERROR_ERRNO,
        MONO_IMAGE_MISSING_ASSEMBLYREF,
        MONO_IMAGE_IMAGE_INVALID
} MonoImageOpenStatus;

typedef enum {
	MONO_DEBUG_FORMAT_NONE,
	MONO_DEBUG_FORMAT_MONO,
	/* Deprecated, the mdb debugger is not longer supported. */
	MONO_DEBUG_FORMAT_DEBUGGER
} MonoDebugFormat;

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
typedef struct MonoArray_ MonoArray;
typedef struct MonoClass_ MonoClass;
typedef struct MonoImage_ MonoImage;
typedef struct MonoObject_ MonoObject;
typedef struct MonoThread_ MonoThread;
typedef int32_t gboolean;

/* Imported from `mono-logger.h` */
typedef void (*MonoLogCallback) (const char *log_domain, const char *log_level, const char *message, int32_t fatal, void *user_data);

/*
 * The "err" variable contents must be allocated using g_malloc or g_strdup
 */
typedef void* (*MonoDlFallbackLoad) (const char *name, int flags, char **err, void *user_data);
typedef void* (*MonoDlFallbackSymbol) (void *handle, const char *name, char **err, void *user_data);
typedef void* (*MonoDlFallbackClose) (void *handle, void *user_data);

typedef void *(*mono_dl_fallback_register_fn) (MonoDlFallbackLoad load_func, MonoDlFallbackSymbol symbol_func, MonoDlFallbackClose close_func, void *user_data);

typedef MonoDomain* (*mono_jit_init_version_fn) (const char *root_domain_name, const char *runtime_version);
typedef MonoAssembly* (*mono_assembly_open_fn) (const char *filename, MonoImageOpenStatus *status);
typedef void (*mono_set_assemblies_path_fn) (const char* path);
typedef MonoString* (*mono_string_new_fn) (MonoDomain *domain, const char *text);
typedef MonoImage* (*mono_assembly_get_image_fn) (MonoAssembly *assembly);
typedef MonoClass* (*mono_class_from_name_fn) (MonoImage *image, const char* name_space, const char *name);
typedef MonoMethod* (*mono_class_get_method_from_name_fn) (MonoClass *klass, const char *name, int param_count);
typedef MonoObject* (*mono_runtime_invoke_fn) (MonoMethod *method, void *obj, void **params, MonoObject **exc);
typedef void (*mono_set_crash_chaining_fn) (int);
typedef void (*mono_set_signal_chaining_fn) (int);
typedef MonoThread *(*mono_thread_attach_fn) (MonoDomain *domain);
typedef void (*mono_domain_set_config_fn) (MonoDomain *, const char *, const char *);
typedef int (*mono_runtime_set_main_args_fn) (int argc, char* argv[]);
typedef void (*mono_trace_init_fn) (void);
typedef void (*mono_trace_set_log_handler_fn) (MonoLogCallback callback, void *user_data);
typedef void (*mono_jit_parse_options_fn) (int argc, char * argv[]);
typedef void (*mono_debug_init_fn) (MonoDebugFormat format);
typedef gboolean (*mini_parse_debug_option_fn) (const char *option);
typedef void (*mono_jit_cleanup_fn) (MonoDomain *domain);
typedef void (*mono_jit_set_aot_mode_fn) (int /* MonoAotMode */ mode);

typedef MonoArray *(*mono_array_new_fn) (MonoDomain *domain, MonoClass *eclass, uintptr_t n);
typedef MonoClass *(*mono_get_string_class_fn) (void);
typedef void (*mono_dllmap_insert_fn) (MonoImage *assembly, const char *dll, const char *func, const char *tdll, const char *tfunc);

static JavaVM *jvm;

static mono_jit_init_version_fn mono_jit_init_version;
static mono_assembly_open_fn mono_assembly_open;
static mono_set_assemblies_path_fn mono_set_assemblies_path;
static mono_string_new_fn mono_string_new;
static mono_assembly_get_image_fn mono_assembly_get_image;
static mono_class_from_name_fn mono_class_from_name;
static mono_class_get_method_from_name_fn mono_class_get_method_from_name;
static mono_runtime_invoke_fn mono_runtime_invoke;
static mono_set_crash_chaining_fn mono_set_crash_chaining;
static mono_set_signal_chaining_fn mono_set_signal_chaining;
static mono_dl_fallback_register_fn mono_dl_fallback_register;
static mono_thread_attach_fn mono_thread_attach;
static mono_domain_set_config_fn mono_domain_set_config;
static mono_runtime_set_main_args_fn mono_runtime_set_main_args;
static mono_trace_init_fn mono_trace_init;
static mono_trace_set_log_handler_fn mono_trace_set_log_handler;
static mono_jit_parse_options_fn mono_jit_parse_options;
static mono_debug_init_fn mono_debug_init;
static mono_array_new_fn mono_array_new;
static mono_get_string_class_fn mono_get_string_class;
static mono_dllmap_insert_fn mono_dllmap_insert;
static mini_parse_debug_option_fn mini_parse_debug_option;
static mono_jit_cleanup_fn mono_jit_cleanup;
static mono_jit_set_aot_mode_fn mono_jit_set_aot_mode;

static MonoAssembly *main_assembly;
static void *runtime_bootstrap_dso;
static void *mono_posix_helper_dso;

static jclass AndroidRunner_klass = NULL;
static jmethodID AndroidRunner_WriteLineToInstrumentation_method = NULL;

//forward decls

static void* my_dlsym (void *handle, const char *name, char **err, void *user_data);
static void* my_dlopen (const char *name, int flags, char **err, void *user_data);

static JNIEnv* mono_jvm_get_jnienv (void);

//stuff

static void
_runtime_log (const char *log_domain, const char *log_level, const char *message, int32_t fatal, void *user_data)
{
	JNIEnv *env;
	jstring j_message;

	if (jvm == NULL)
		__android_log_assert ("", "mono-sdks", "%s: jvm is NULL", __func__);

	if (AndroidRunner_klass == NULL)
		__android_log_assert ("", "mono-sdks", "%s: AndroidRunner_klass is NULL", __func__);
	if (AndroidRunner_WriteLineToInstrumentation_method == NULL)
		__android_log_assert ("", "mono-sdks", "%s: AndroidRunner_WriteLineToInstrumentation_method is NULL", __func__);

	env = mono_jvm_get_jnienv ();

	j_message = (*env)->NewStringUTF(env, message);

	(*env)->CallStaticVoidMethod (env, AndroidRunner_klass, AndroidRunner_WriteLineToInstrumentation_method, j_message);

	(*env)->DeleteLocalRef (env, j_message);

	/* Still print it on the logcat */

	android_LogPriority android_log_level;
	switch (*log_level) {
	case 'e': /* error */
		android_log_level = ANDROID_LOG_FATAL;
		break;
	case 'c': /* critical */
		android_log_level = ANDROID_LOG_ERROR;
		break;
	case 'w': /* warning */
		android_log_level = ANDROID_LOG_WARN;
		break;
	case 'm': /* message */
		android_log_level = ANDROID_LOG_INFO;
		break;
	case 'i': /* info */
		android_log_level = ANDROID_LOG_DEBUG;
		break;
	case 'd': /* debug */
		android_log_level = ANDROID_LOG_VERBOSE;
		break;
	default:
		android_log_level = ANDROID_LOG_UNKNOWN;
		break;
	}

	__android_log_write (android_log_level, log_domain, message);
	if (android_log_level == ANDROID_LOG_FATAL)
		abort ();
}

static void
_log (const char *format, ...)
{
	va_list args;
	char *buf;
	int nbuf;

	errno = 0;

	va_start (args, format);
	nbuf = vasprintf (&buf, format, args);
	va_end (args);

	if (buf == NULL || nbuf == -1)
		__android_log_assert ("", "mono-sdks", "%s: vasprintf failed, error: \"%s\" (%d), nbuf = %d, buf = \"%s\"", __func__, strerror(errno), errno, nbuf, buf ? buf : "(null)");

	_runtime_log ("mono-sdks", "debug", buf, 0, NULL);

	free (buf);
}

static void
strncpy_str (JNIEnv *env, char *buff, jstring str, int nbuff)
{
	jboolean isCopy = 0;
	const char *copy_buff = (*env)->GetStringUTFChars (env, str, &isCopy);
	strncpy (buff, copy_buff, nbuff);
	if (isCopy)
		(*env)->ReleaseStringUTFChars (env, str, copy_buff);
}

static char *
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

static volatile int wait_for_lldb = 1;

void
monodroid_clear_lldb_wait (void)
{
	wait_for_lldb = 0;
}

static void
wait_for_unmanaged_debugger ()
{
	while (wait_for_lldb) {
		_log ("Waiting for lldb to attach, run \"make -C sdks/android attach-lldb\" in another terminal");
		sleep (5);
	}
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

void
Java_org_mono_android_AndroidRunner_runTests (JNIEnv* env, jobject thiz, jstring j_files_dir, jstring j_cache_dir,
	jstring j_native_library_dir, jstring j_assembly_dir, jstring j_assembly_name, jboolean is_debugger, jboolean is_profiler, jboolean wait_for_lldb)
{
	MonoDomain *root_domain;
	MonoMethod *run_tests_method;
	void **params;
	char **argv;
	char buff[1024], file_dir[2048], cache_dir[2048], native_library_dir[2048], assembly_dir[2048], assembly_name[2048];
	int argc;

	if (wait_for_lldb)
		wait_for_unmanaged_debugger ();

	_log ("IN %s\n", __func__);
	strncpy_str (env, file_dir, j_files_dir, sizeof(file_dir));
	strncpy_str (env, cache_dir, j_cache_dir, sizeof(cache_dir));
	strncpy_str (env, native_library_dir, j_native_library_dir, sizeof(native_library_dir));
	strncpy_str (env, assembly_dir, j_assembly_dir, sizeof(assembly_dir));
	strncpy_str (env, assembly_name, j_assembly_name, sizeof(assembly_name));

	_log ("-- file dir %s", file_dir);
	_log ("-- cache dir %s", cache_dir);
	_log ("-- native library dir %s", native_library_dir);
	_log ("-- assembly dir %s", assembly_dir);
	_log ("-- assembly name %s", assembly_name);
	_log ("-- is debugger %d", is_debugger);
	_log ("-- is profiler %d", is_profiler);
	prctl (PR_SET_DUMPABLE, 1);

	snprintf (buff, sizeof(buff), "%s/libmonosgen-2.0.so", native_library_dir);
	void *libmono = dlopen (buff, RTLD_LAZY);
	if (!libmono) {
		_log ("Unknown file \"%s/libmonosgen-2.0.so\"", native_library_dir);
		_exit (1);
	}

#define DLSYM(sym) \
	do { \
		sym = dlsym(libmono, #sym); \
		if (!sym) { \
			_log ("Unknown symbol \"%s\"", #sym); \
			_exit (1); \
		} \
	} while (0)

	DLSYM (mini_parse_debug_option);
	DLSYM (mono_array_new);
	DLSYM (mono_assembly_get_image);
	DLSYM (mono_assembly_open);
	DLSYM (mono_class_from_name);
	DLSYM (mono_class_get_method_from_name);
	DLSYM (mono_debug_init);
	DLSYM (mono_dl_fallback_register);
	DLSYM (mono_dllmap_insert);
	DLSYM (mono_domain_set_config);
	DLSYM (mono_get_string_class);
	DLSYM (mono_jit_init_version);
	DLSYM (mono_jit_parse_options);
	DLSYM (mono_runtime_invoke);
	DLSYM (mono_jit_cleanup);
	DLSYM (mono_runtime_set_main_args);
	DLSYM (mono_set_assemblies_path);
	DLSYM (mono_set_crash_chaining);
	DLSYM (mono_set_signal_chaining);
	DLSYM (mono_string_new);
	DLSYM (mono_thread_attach);
	DLSYM (mono_trace_init);
	DLSYM (mono_trace_set_log_handler);
	DLSYM (mono_jit_set_aot_mode);

#undef DLSYM

	//MUST HAVE envs
	setenv ("TMPDIR", cache_dir, 1);
	setenv ("MONO_CFG_DIR", file_dir, 1);
	// setenv ("MONO_DEBUG", "explicit-null-checks", 1);

	create_and_set (file_dir, "home", "HOME");
	create_and_set (file_dir, "home/.local/share", "XDG_DATA_HOME");
	create_and_set (file_dir, "home/.local/share", "XDG_DATA_HOME");
	create_and_set (file_dir, "home/.config", "XDG_CONFIG_HOME");

	//Debug flags
	setenv ("MONO_LOG_LEVEL", "info", 1);
	setenv ("MONO_LOG_MASK", "all", 1);
	// setenv ("MONO_VERBOSE_METHOD", "GetCallingAssembly", 1);

	/* uncomment to enable interpreter */
	// mono_jit_set_aot_mode (1000 /* MONO_EE_MODE_INTERP */);

	mono_trace_init ();
	mono_trace_set_log_handler (_runtime_log, NULL);

	sprintf (buff, "%s:%s/tests", assembly_dir, assembly_dir);
	mono_set_assemblies_path (buff);

	mono_set_crash_chaining (1);
	mono_set_signal_chaining (1);
	mono_dl_fallback_register (my_dlopen, my_dlsym, NULL, NULL);

	sprintf (buff, "%s/libruntime-bootstrap.so", native_library_dir);
	runtime_bootstrap_dso = dlopen (buff, RTLD_LAZY);

	sprintf (buff, "%s/libMonoPosixHelper.so", native_library_dir);
	mono_posix_helper_dso = dlopen (buff, RTLD_LAZY);

	sprintf (buff, "%s/libmono-native.so", native_library_dir);

	mono_dllmap_insert (NULL, "System.Native", NULL, buff, NULL);
	mono_dllmap_insert (NULL, "System.Net.Security.Native", NULL, buff, NULL);

	if (wait_for_lldb)
		mini_parse_debug_option ("lldb");

	if (is_debugger) {
		char *debug_options[] = { "--debugger-agent=transport=dt_socket,loglevel=10,address=127.0.0.1:6100,embedding=1", "--soft-breakpoints" };
		mono_jit_parse_options (1, debug_options);
	} else if (is_profiler) {
		// TODO: profiler
	}

	mono_debug_init (MONO_DEBUG_FORMAT_MONO);

	// Note: sets up domains. If the debugger is configured after this line is run, 
	// then lookup_data_table will fail.
	root_domain = mono_jit_init_version ("TEST RUNNER", "mobile");
	mono_domain_set_config (root_domain, assembly_dir, file_dir);

	mono_thread_attach (root_domain);

	if (is_debugger) {
		char *argv[] = { assembly_name };
		mono_runtime_set_main_args (1, argv);

		sprintf (buff, "%s/%s", assembly_dir, assembly_name);
		main_assembly = mono_assembly_open (buff, NULL);
		if (!main_assembly) {
			_log ("Unknown assembly \"%s\"", buff);
			_exit (1);
		}

		MonoClass *tests_class = mono_class_from_name (mono_assembly_get_image (main_assembly), "", "Tests");
		if (!tests_class) {
			_log ("Unknown class \"Tests\"");
			_exit (1);
		}

		run_tests_method = mono_class_get_method_from_name (tests_class, "Main", 1);
		if (!run_tests_method) {
			_log ("Unknown method \"Main\"");
			_exit (1);
		}

		// attached debugger sets the options the main class uses
		// Therefore, we just pass Main no args
		void *args[] = { mono_array_new (root_domain, mono_get_string_class (), 0) };
		mono_runtime_invoke (run_tests_method, NULL, args, NULL);

		// Properly disconnect the debugger
		mono_jit_cleanup (root_domain);
	} else if (is_profiler) {
		// TODO: profiler
		_log ("Unsupported profiler");
		_exit (1);
	} else {
		char *argv[] = { "nunitlite.dll" };
		mono_runtime_set_main_args (1, argv);

		sprintf (buff, "%s/%s", assembly_dir, "nunitlite.dll");
		main_assembly = mono_assembly_open (buff, NULL);
		if (!main_assembly) {
			_log ("Unknown assembly \"%s\"", buff);
			_exit (1);
		}

		MonoClass *driver_class = mono_class_from_name (mono_assembly_get_image (main_assembly), "Xamarin", "AndroidTestAssemblyRunner/Driver");
		if (!driver_class) {
			_log ("Unknown class \"Xamarin.AndroidTestAssemblyRunner/Driver\"");
			_exit (1);
		}

		run_tests_method = mono_class_get_method_from_name (driver_class, "RunTests", 1);
		if (!run_tests_method) {
			_log ("Unknown method \"RunTests\"");
			_exit (1);
		}

		sprintf (buff, "%s/%s", assembly_dir, assembly_name);
		void *args[] = { mono_string_new (root_domain, buff) };
		mono_runtime_invoke (run_tests_method, NULL, args, NULL);
	}
}

static int
convert_dl_flags (int flags)
{
	int lflags = flags & MONO_DL_LOCAL? 0: RTLD_GLOBAL;

	if (flags & MONO_DL_LAZY)
		lflags |= RTLD_LAZY;
	else
		lflags |= RTLD_NOW;
	return lflags;
}


/*
This is the Android specific glue ZZZZOMG

# Issues with the monodroid BCL profile
	This pinvoke should not be on __Internal by libmonodroid.so: System.TimeZoneInfo+AndroidTimeZones:monodroid_get_system_property
	This depends on monodroid native code: System.TimeZoneInfo+AndroidTimeZones.GetDefaultTimeZoneName
*/

#define MONO_API __attribute__ ((__visibility__ ("default")))

#define INTERNAL_LIB_HANDLE ((void*)(size_t)-1)
static void*
my_dlopen (const char *name, int flags, char **err, void *user_data)
{
	if (!name)
		return INTERNAL_LIB_HANDLE;

	void *res = dlopen (name, convert_dl_flags (flags));

	//TODO handle loading AOT modules from assembly_dir

	return res;
}

static void*
my_dlsym (void *handle, const char *name, char **err, void *user_data)
{
	void *s;

	if (handle == INTERNAL_LIB_HANDLE) {
		s = dlsym (runtime_bootstrap_dso, name);
		if (!s && mono_posix_helper_dso)
			s = dlsym (mono_posix_helper_dso, name);
	} else {
		s = dlsym (handle, name);
	}

	if (!s && err) {
		*err = m_strdup_printf ("Could not find symbol '%s'.", name);
	}

	return s;
}

MONO_API int
monodroid_get_system_property (const char *name, char **value)
{
	char *pvalue;
	char  sp_value [PROP_VALUE_MAX+1] = { 0, };
	int   len;

	if (value)
		*value = NULL;

	pvalue  = sp_value;
	len     = __system_property_get (name, sp_value);

	if (len >= 0 && value) {
		*value = malloc (len + 1);
		if (!*value)
			return -len;
		memcpy (*value, pvalue, len);
		(*value)[len] = '\0';
	}

	return len;
}

MONO_API void
monodroid_free (void *ptr)
{
	free (ptr);
}

typedef struct {
	struct _monodroid_ifaddrs *ifa_next; /* Pointer to the next structure.      */

	char *ifa_name;                      /* Name of this network interface.     */
	unsigned int ifa_flags;              /* Flags as from SIOCGIFFLAGS ioctl.   */

	struct sockaddr *ifa_addr;           /* Network address of this interface.  */
	struct sockaddr *ifa_netmask;        /* Netmask of this interface.          */
	union {
		/* At most one of the following two is valid.  If the IFF_BROADCAST
		   bit is set in `ifa_flags', then `ifa_broadaddr' is valid.  If the
		   IFF_POINTOPOINT bit is set, then `ifa_dstaddr' is valid.
		   It is never the case that both these bits are set at once.  */
		struct sockaddr *ifu_broadaddr;  /* Broadcast address of this interface. */
		struct sockaddr *ifu_dstaddr;    /* Point-to-point destination address.  */
	} ifa_ifu;
	void *ifa_data;               /* Address-specific data (may be unused).  */
} m_ifaddrs;

typedef int (*get_ifaddr_fn)(m_ifaddrs **ifap);
typedef void (*freeifaddr_fn)(m_ifaddrs *ifap);

static void
init_sock_addr (struct sockaddr **res, const char *str_addr)
{
	struct sockaddr_in addr;
	addr.sin_family = AF_INET;
	inet_pton (AF_INET, str_addr, &addr.sin_addr);

	*res = calloc (1, sizeof (struct sockaddr));
	**(struct sockaddr_in**)res = addr;
}

MONO_API int
monodroid_getifaddrs (m_ifaddrs **ifap)
{
	char buff[1024];
	FILE * f = fopen ("/proc/net/route", "r");
	if (f) {
		int i = 0;
		fgets (buff, 1023, f);
		fgets (buff, 1023, f);
		while (!isspace (buff [i]) && i < 1024)
			++i;
		buff [i] = 0;
		fclose (f);
	} else {
		strcpy (buff, "wlan0");
	}

	m_ifaddrs *res = calloc (1, sizeof (m_ifaddrs));
	memset (res, 0, sizeof (*res));

	res->ifa_next = NULL;
	res->ifa_name = m_strdup_printf ("%s", buff);
	res->ifa_flags = 0;
	res->ifa_ifu.ifu_dstaddr = NULL;
	init_sock_addr (&res->ifa_addr, "192.168.0.1");
	init_sock_addr (&res->ifa_netmask, "255.255.255.0");

	*ifap = res;
	return 0;
}

MONO_API void
monodroid_freeifaddrs (m_ifaddrs *ifap)
{
	free (ifap->ifa_name);
	if (ifap->ifa_addr)
		free (ifap->ifa_addr);
	if (ifap->ifa_netmask)
		free (ifap->ifa_netmask);
	free (ifap);
}

MONO_API int
_monodroid_get_android_api_level (void)
{
	return 24;
}

MONO_API int
_monodroid_get_network_interface_up_state (void *ifname, int *is_up)
{
	*is_up = 1;
	return 1;
}

MONO_API int
_monodroid_get_network_interface_supports_multicast (void *ifname, int *supports_multicast)
{
	*supports_multicast = 0;
	return 1;
}

MONO_API int
_monodroid_get_dns_servers (void **dns_servers_array)
{
	*dns_servers_array = NULL;
	if (!dns_servers_array)
		return -1;

	size_t  len;
	char   *dns;
	char   *dns_servers [8];
	int     count = 0;
	char    prop_name[] = "net.dnsX";
	int i;
	for (i = 0; i < 8; i++) {
		prop_name [7] = (char)(i + 0x31);
		len = monodroid_get_system_property (prop_name, &dns);
		if (len <= 0) {
			dns_servers [i] = NULL;
			continue;
		}
		dns_servers [i] = strndup (dns, len);
		count++;
	}

	if (count <= 0)
		return 0;

	char **ret = (char**)malloc (sizeof (char*) * count);
	char **p = ret;
	for (i = 0; i < 8; i++) {
		if (!dns_servers [i])
			continue;
		*p++ = dns_servers [i];
	}

	*dns_servers_array = (void*)ret;
	return count;
}

static int initialized = 0;

static jobject
lref_to_gref (JNIEnv *env, jobject lref)
{
	jobject g;
	if (lref == 0)
		return 0;
	g = (*env)->NewGlobalRef (env, lref);
	(*env)->DeleteLocalRef (env, lref);
	return g;
}

static void
mono_jvm_initialize (JavaVM *vm)
{
	JNIEnv *env;

	if (initialized)
		return;

	jvm = vm;

	int res = (*jvm)->GetEnv (jvm, (void**)&env, JNI_VERSION_1_6);
	if (!env)
		__android_log_assert ("", "mono-sdks", "%s: fatal error: Could not create env, res = %d", __func__, res);

	AndroidRunner_klass = lref_to_gref(env, (*env)->FindClass (env, "org/mono/android/AndroidRunner"));
	if (!AndroidRunner_klass)
		__android_log_assert ("", "mono-sdks", "%s: fatal error: Could not find AndroidRunner_klass", __func__);

	AndroidRunner_WriteLineToInstrumentation_method = (*env)->GetStaticMethodID (env, AndroidRunner_klass, "WriteLineToInstrumentation", "(Ljava/lang/String;)V");
	if (!AndroidRunner_WriteLineToInstrumentation_method)
		__android_log_assert ("", "mono-sdks", "%s: fatal error: Could not find AndroidRunner_WriteLineToInstrumentation_method", __func__);

	initialized = 1;
}

JNIEXPORT jint JNICALL
JNI_OnLoad (JavaVM *vm, void *reserved)
{
	mono_jvm_initialize (vm);
	return JNI_VERSION_1_6;
}

static JNIEnv*
mono_jvm_get_jnienv (void)
{
	JNIEnv *env;

	if (!initialized)
		__android_log_assert ("", "mono-sdks", "%s: Fatal error: jvm not initialized", __func__);

	(*jvm)->GetEnv (jvm, (void**)&env, JNI_VERSION_1_6);
	if (env)
		return env;

	(*jvm)->AttachCurrentThread(jvm, (void **)&env, NULL);
	if (env)
		return env;

	__android_log_assert ("", "mono-sdks", "%s: Fatal error: Could not create env", __func__);
}
