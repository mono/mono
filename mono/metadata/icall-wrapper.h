
#ifndef __MONO_METADATA_ICALL_WRAPPER_H__
#define __MONO_METADATA_ICALL_WRAPPER_H__

#include <config.h>
#include <glib.h>

typedef struct _MonoIcallWrapperData MonoIcallWrapperData;

MonoIcallWrapperData*
mono_icall_wrapper_start (gsize *stackdata);

void
mono_icall_wrapper_end (MonoIcallWrapperData *data, gsize *stackdata);

#define MONO_ICALL_WRAPPER_START	\
	do {	\
		gsize __dummy;	\
		MonoIcallWrapperData *__icall_wrapper_data = mono_icall_wrapper_start (&__dummy)

#define MONO_ICALL_WRAPPER_END	\
		mono_icall_wrapper_end (__icall_wrapper_data, &__dummy);	\
	} while (0)

#endif /* __MONO_METADATA_ICALL_WRAPPER_H__ */