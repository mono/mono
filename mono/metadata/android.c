
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

#ifdef ANDROID
#include <sys/system_properties.h>
#else
#define PROP_NAME_MAX   32
#define PROP_VALUE_MAX  92
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

static jclass     NetworkInterface_class;
static jmethodID  NetworkInterface_getByName;
static jmethodID  NetworkInterface_isUp;
static jmethodID  NetworkInterface_supportsMulticast;

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

	NetworkInterface_class = lref_to_gref (env, (*env)->FindClass (env, "java/net/NetworkInterface"));
	if (!NetworkInterface_class)
		g_error ("Fatal error: Could not find java.net.NetworkInterface class!");

	NetworkInterface_getByName = (*env)->GetStaticMethodID (env, NetworkInterface_class, "getByName", "(Ljava/lang/String;)Ljava/net/NetworkInterface;");
	if (!NetworkInterface_getByName)
		g_error ("Fatal error: Could not find java.net.NetworkInterface.getByName() method!");

	NetworkInterface_isUp = (*env)->GetMethodID (env, NetworkInterface_class, "isUp", "()Z");
	if (!NetworkInterface_isUp)
		g_error ("Fatal error: Could not find java.net.NetworkInterface.isUp() method!");

	NetworkInterface_supportsMulticast = (*env)->GetMethodID (env, NetworkInterface_class, "supportsMulticast", "()Z");
	if (!NetworkInterface_supportsMulticast)
		g_error ("Fatal error: Could not find java.net.NetworkInterface.supportsMulticast() method!");

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

	(*jvm)->AttachCurrentThread(jvm, &env, NULL);
	if (env)
		return env;

	g_error ("%s: Fatal error: Could not create env", __func__);
}

struct BundledProperty {
	gchar *name;
	gchar *value;
	gint   value_len;
	struct BundledProperty *next;
};

static struct BundledProperty* bundled_properties;

static struct BundledProperty*
lookup_system_property (const gchar *name)
{
	struct BundledProperty *p = bundled_properties;
	for ( ; p ; p = p->next)
		if (strcmp (p->name, name) == 0)
			return p;
	return NULL;
}

void
monodroid_add_system_property (const gchar *name, const gchar *value)
{
	gint name_len, value_len;

	struct BundledProperty* p = lookup_system_property (name);
	if (p) {
		gchar *n = g_strdup (value);
		g_free (p->value);
		p->value      = n;
		p->value_len  = strlen (p->value);
		return;
	}

	name_len  = strlen (name);
	value_len = strlen (value);

	p = g_malloc0 (sizeof (struct BundledProperty) + name_len + 1);

	p->name = ((char*) p) + sizeof (struct BundledProperty);
	strncpy (p->name, name, name_len);
	p->name [name_len] = '\0';

	p->value      = g_strdup (value);
	p->value_len  = value_len;

	p->next             = bundled_properties;
	bundled_properties  = p;
}

#ifndef ANDROID
static void
monodroid_strreplace (gchar *buffer, gchar old_char, gchar new_char)
{
	if (buffer == NULL)
		return;
	while (*buffer != '\0') {
		if (*buffer == old_char)
			*buffer = new_char;
		buffer++;
	}
}

static gint
_monodroid__system_property_get (const gchar *name, gchar *sp_value, gsize sp_value_len)
{
	if (!name)
		return -1;

	g_assert (sp_value);
	g_assert (sp_value_len == PROP_VALUE_MAX + 1);

	gchar *env_name = g_strdup_printf ("__XA_%s", name);
	monodroid_strreplace (env_name, '.', '_');
	gchar *env_value = g_getenv (env_name);
	g_free (env_name);

	gsize env_value_len = env_value ? strlen (env_value) : 0;
	if (env_value_len == 0) {
		sp_value[0] = '\0';
		return 0;
	}

	if (env_value_len >= sp_value_len)
		mono_trace (G_LOG_LEVEL_WARNING, MONO_TRACE_ANDROID_DEFAULT, "System property buffer size too small by %u bytes", env_value_len == sp_value_len ? 1 : env_value_len - sp_value_len);

	strncpy (sp_value, env_value, sp_value_len);
	sp_value[sp_value_len] = '\0';

	return strlen (sp_value);
}
#elif ANDROID64
/* __system_property_get was removed in Android 5.0/64bit
   this is hopefully temporary replacement, until we find better
   solution

   sp_value buffer should be at least PROP_VALUE_MAX+1 bytes long
*/
static gint
_monodroid__system_property_get (const gchar *name, gchar *sp_value, gsize sp_value_len)
{
	if (!name)
		return -1;

	g_assert (sp_value);
	g_assert (sp_value_len == PROP_VALUE_MAX + 1);

	gchar *cmd = g_strdup_printf ("getprop %s", name);
	FILE* result = popen (cmd, "r");
	gint len = (gint) fread (sp_value, 1, sp_value_len, result);
	fclose (result);
	sp_value [len] = 0;
	if (len > 0 && sp_value [len - 1] == '\n') {
		sp_value [len - 1] = 0;
		len--;
	} else {
		if (len != 0)
			len = 0;
		sp_value [0] = 0;
	}

	mono_trace (G_LOG_LEVEL_MESSAGE, MONO_TRACE_ANDROID_DEFAULT, "%s %s: '%s' len: %d", __func__, name, sp_value, len);

	return len;
}
#else
static gint
_monodroid__system_property_get (const gchar *name, gchar *sp_value, gsize sp_value_len)
{
	if (!name)
		return -1;

	g_assert (sp_value);
	g_assert (sp_value_len == PROP_VALUE_MAX + 1);

	return __system_property_get (name, sp_value);
}
#endif // ANDROID

gint32
monodroid_get_system_property (const gchar *name, gchar **value)
{
	gchar  buf [PROP_VALUE_MAX+1] = { 0, };
	gint   len;
	struct BundledProperty *p;

	g_assert (value);
	*value = NULL;

	len = _monodroid__system_property_get (name, buf, sizeof (buf));
	if (len > 0) {
		*value = g_strndup (buf, len);
		return len;
	}

	if ((p = lookup_system_property (name))) {
		*value = g_strndup (p->value, p->value_len);
		return p->value_len;
	}

	return -1;
}

void
monodroid_free (gpointer ptr)
{
	g_free (ptr);
}

gint32
ves_icall_System_TimezoneInfo_AndroidTimeZones_GetSystemProperty (const gchar *name, gchar **value)
{
	return monodroid_get_system_property (name, value);
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

gint32
ves_icall_System_Net_NetworkInformation_UnixIPInterfaceProperties_GetDNSServers (gpointer *dns_servers_array)
{
	g_assert (dns_servers_array);
	*dns_servers_array = NULL;

	gsize  len;
	gchar *dns;
	gchar *dns_servers [8];
	gint   count = 0;
	gchar  prop_name[] = "net.dnsX";
	for (gint i = 0; i < 8; i++) {
		prop_name [7] = (char)(i + 0x31);
		len = monodroid_get_system_property (prop_name, &dns);
		if (len <= 0) {
			dns_servers [i] = NULL;
			continue;
		}
		dns_servers [i] = g_strndup (dns, len);
		count++;
	}

	if (count <= 0)
		return 0;

	gchar **ret = g_new (gchar*, count);
	gchar **p = ret;
	for (gint i = 0; i < 8; i++) {
		if (!dns_servers [i])
			continue;
		*p++ = dns_servers [i];
	}

	*dns_servers_array = (gpointer)ret;
	return count;
}

static MonoBoolean
_monodroid_get_network_interface_state (const gchar *ifname, MonoBoolean *is_up, MonoBoolean *supports_multicast)
{
	if (!ifname || strlen (ifname) == 0 || (!is_up && !supports_multicast))
		return FALSE;

	g_assert (NetworkInterface_class);
	g_assert (NetworkInterface_getByName);

	JNIEnv *env = mono_jvm_get_jnienv ();
	jstring NetworkInterface_nameArg = (*env)->NewStringUTF (env, ifname);
	jobject networkInterface = (*env)->CallStaticObjectMethod (env, NetworkInterface_class, NetworkInterface_getByName, NetworkInterface_nameArg);
	(*env)->DeleteLocalRef (env, NetworkInterface_nameArg);

	if (!networkInterface) {
		mono_trace (G_LOG_LEVEL_WARNING, MONO_TRACE_ANDROID_NET, "Failed to look up interface '%s' using Java API", ifname);
		return FALSE;
	}

	if (is_up) {
		g_assert (NetworkInterface_isUp);
		*is_up = (gboolean)(*env)->CallBooleanMethod (env, networkInterface, NetworkInterface_isUp);
	}

	if (supports_multicast) {
		g_assert (NetworkInterface_supportsMulticast);
		*supports_multicast = (gboolean)(*env)->CallBooleanMethod (env, networkInterface, NetworkInterface_supportsMulticast);
	}

	return TRUE;
}

MonoBoolean
ves_icall_System_Net_NetworkInformation_LinuxNetworkInterface_GetUpState (const gchar *ifname, MonoBoolean *is_up)
{
	return _monodroid_get_network_interface_state (ifname, is_up, NULL);
}

MonoBoolean
ves_icall_System_Net_NetworkInformation_LinuxNetworkInterface_GetSupportsMulticast (const gchar *ifname, MonoBoolean *supports_multicast)
{
	return _monodroid_get_network_interface_state (ifname, NULL, supports_multicast);
}
