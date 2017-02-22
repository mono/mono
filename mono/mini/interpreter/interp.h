#ifndef __MONO_MINI_INTERPRETER_H__
#define __MONO_MINI_INTERPRETER_H__
#include <mono/mini/mini.h>

int
mono_interp_regression_list (int verbose, int count, char *images []);

void
mono_interp_init (void);

gpointer
mono_interp_create_method_pointer (MonoMethod *method, MonoError *error);

MonoObject*
mono_interp_runtime_invoke (MonoMethod *method, void *obj, void **params, MonoObject **exc, MonoError *error);

#endif /* __MONO_MINI_INTERPRETER_H__ */
