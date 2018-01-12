
#ifndef __MONO_METADATA_ANDROID_H__
#define __MONO_METADATA_ANDROID_H__

#include <glib.h>

#include <jni.h>

#include "utils/mono-publib.h"

MONO_API void
mono_jvm_initialize (JavaVM *vm);

JNIEnv*
mono_jvm_get_jnienv (void);

gpointer
ves_icall_System_TimezoneInfo_AndroidTimeZones_GetDefaultTimeZoneId (void);

#endif /* __MONO_METADATA_ANDROID_H__ */
