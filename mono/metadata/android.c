
#include <config.h>
#include <glib.h>

#include <jni.h>

#if HOST_DARWIN
#include <sys/types.h>
#include <sys/sysctl.h>
#include <mach/machine.h>
#elif HOST_WIN32
#include <windows.h>
#endif

#include "android.h"
#include "threads.h"
#include "appdomain.h"

#include "utils/mono-logger-internals.h"

static gboolean initialized = FALSE;

static JavaVM *jvm;

static jclass     TimeZone_class;
static jmethodID  TimeZone_getDefault;
static jmethodID  TimeZone_getID;

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

void
mono_jvm_initialize (JavaVM *vm)
{
	JNIEnv *env;

	if (initialized)
		return;

	jvm = vm;

	(*jvm)->GetEnv (jvm, (gpointer*)&env, JNI_VERSION_1_6);

	TimeZone_class = lref_to_gref (env, (*env)->FindClass (env, "java/util/TimeZone"));
	if (!TimeZone_class)
		g_error ("%s: Fatal error: Could not find java.util.TimeZone class!", __func__);

	TimeZone_getDefault = (*env)->GetStaticMethodID (env, TimeZone_class, "getDefault", "()Ljava/util/TimeZone;");
	if (!TimeZone_getDefault)
		g_error ("%s: Fatal error: Could not find java.util.TimeZone.getDefault() method!", __func__);

	TimeZone_getID = (*env)->GetMethodID (env, TimeZone_class, "getID", "()Ljava/lang/String;");
	if (!TimeZone_getID)
		g_error ("%s: Fatal error: Could not find java.util.TimeZone.getDefault() method!", __func__);

	initialized = TRUE;
}

JNIEXPORT jint JNICALL
JNI_OnLoad (JavaVM *vm, gpointer reserved)
{
	mono_jvm_initialize (vm);
	return JNI_VERSION_1_6;
}

JNIEnv*
mono_jvm_get_jnienv (void)
{
	JNIEnv *env;

	g_assert (initialized);

	(*jvm)->GetEnv (jvm, (void**)&env, JNI_VERSION_1_6);
	if (env)
		return env;

	mono_thread_attach (mono_domain_get ());

	(*jvm)->GetEnv (jvm, (void**)&env, JNI_VERSION_1_6);
	if (env)
		return env;

	g_error ("%s: Fatal error: Could not create env", __func__);
}

gpointer
ves_icall_System_TimezoneInfo_AndroidTimeZones_GetDefaultTimeZoneId (void)
{
	JNIEnv *env = mono_jvm_get_jnienv ();
	jobject d = (*env)->CallStaticObjectMethod (env, TimeZone_class, TimeZone_getDefault);
	jstring id = (*env)->CallObjectMethod (env, d, TimeZone_getID);
	const gchar *mutf8 = (*env)->GetStringUTFChars (env, id, NULL);

	gchar *def_id = g_strdup (mutf8);

	(*env)->ReleaseStringUTFChars (env, id, mutf8);
	(*env)->DeleteLocalRef (env, id);
	(*env)->DeleteLocalRef (env, d);

	return def_id;
}

#if HOST_ANDROID && __arm__

#define BUF_SIZE 512

static gboolean
find_in_maps (const gchar *str)
{
	FILE  *maps;
	gchar *line;
	gchar  buf [BUF_SIZE];

	g_assert (str);

	maps = fopen ("/proc/self/maps", "r");
	if (!maps)
		return FALSE;

	while ((line = fgets (buf, BUF_SIZE, maps))) {
		if (strstr (line, str)) {
			fclose (maps);
			return TRUE;
		}
	}

	fclose (maps);
	return FALSE;
}

static gboolean
detect_houdini ()
{
	return find_in_maps ("libhoudini");
}

#endif // HOST_ANDROID && __arm__

static gboolean
is_64_bit (void)
{
	return SIZEOF_VOID_P == 8;
}

#define CPU_KIND_UNKNOWN ((guint16)0)
#define CPU_KIND_ARM     ((guint16)1)
#define CPU_KIND_ARM64   ((guint16)2)
#define CPU_KIND_MIPS    ((guint16)3)
#define CPU_KIND_X86     ((guint16)4)
#define CPU_KIND_X86_64  ((guint16)5)

static guint16
get_built_for_cpu (void)
{
#if HOST_WIN32
# if _M_AMD64 || _M_X64
	return CPU_KIND_X86_64;
# elif _M_IX86
	return CPU_KIND_X86;
# elif _M_ARM
	return CPU_KIND_ARM;
# else
	return CPU_KIND_UNKNOWN;
# endif
#elif HOST_DARWIN
# if __x86_64__
	return CPU_KIND_X86_64;
# elif __i386__
	return CPU_KIND_X86;
# else
	return CPU_KIND_UNKNOWN;
# endif
#elif HOST_ANDROID
# if __arm__
	return CPU_KIND_ARM;
# elif __aarch64__
	return CPU_KIND_ARM64;
# elif __x86_64__
	return CPU_KIND_X86_64;
# elif __i386__
	return CPU_KIND_X86;
# elif __mips__
	return CPU_KIND_MIPS;
# else
	return CPU_KIND_UNKNOWN;
# endif
#else
# error get_built_for_cpu not implemented
#endif // HOST_WIN32
}

static guint16
get_running_on_cpu (void)
{
#ifdef HOST_WIN32
	SYSTEM_INFO si;

	GetSystemInfo (&si);
	switch (si.wProcessorArchitecture) {
		case PROCESSOR_ARCHITECTURE_AMD64:
			return CPU_KIND_X86_64;
		case PROCESSOR_ARCHITECTURE_ARM:
			return CPU_KIND_ARM;
		case PROCESSOR_ARCHITECTURE_INTEL:
			return CPU_KIND_X86;
		default:
			return CPU_KIND_UNKNOWN;
	}
#elif HOST_DARWIN
	cpu_type_t cputype;
	size_t length;

	length = sizeof (cputype);
	sysctlbyname ("hw.cputype", &cputype, &length, NULL, 0);
	switch (cputype) {
		case CPU_TYPE_X86:
			return CPU_KIND_X86;
		case CPU_TYPE_X86_64:
			return CPU_KIND_X86_64;
		default:
			return CPU_KIND_UNKNOWN;
	}
#elif HOST_ANDROID
# if __arm__
	if (!detect_houdini ()) {
		return CPU_KIND_ARM;
	} else {
		/* If houdini is mapped in we're running on x86 */
		return CPU_KIND_X86;
	}
# elif __aarch64__
	return CPU_KIND_ARM64;
# elif __x86_64__
	return CPU_KIND_X86_64;
# elif __i386__
	return is_64_bit () ? CPU_KIND_X86_64 : CPU_KIND_X86;
# elif __mips__
	return CPU_KIND_MIPS;
# endif
#else
# error get_running_on_cpu not implemented
#endif // HOST_WIN32
}

void
ves_icall_Mono_Unix_Android_AndroidUtils_DetectCpuAndArchitecture (guint16 *built_for_cpu, guint16 *running_on_cpu, MonoBoolean *is64bit)
{
	g_assert (is64bit);
	*is64bit = (guint8) is_64_bit ();
	g_assert (built_for_cpu);
	*built_for_cpu = get_built_for_cpu ();
	g_assert (running_on_cpu);
	*running_on_cpu = get_running_on_cpu ();
}
