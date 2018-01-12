
#include <jni.h>

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
