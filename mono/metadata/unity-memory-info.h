#ifndef __UNITY_MONO_MEMORY_INFO_H
#define __UNITY_MONO_MEMORY_INFO_H

typedef struct _MonoClass MonoClass;
typedef void(*ClassReportFunc) (MonoClass* klass, void *user_data);

MONO_API void mono_unity_class_for_each(ClassReportFunc callback, void* user_data);

#endif
