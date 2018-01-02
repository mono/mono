/**
 * \file
 */

#ifndef _MONO_METADATA_EXCEPTION_INTERNALS_H_
#define _MONO_METADATA_EXCEPTION_INTERNALS_H_

#include <glib.h>

#include <mono/metadata/object.h>
#include <mono/metadata/handle.h>
#include <mono/utils/mono-error.h>

MonoException *
mono_get_exception_type_initialization_checked (const gchar *type_name, MonoException *inner, MonoError *error);

MonoExceptionHandle
mono_get_exception_reflection_type_load_checked (MonoArrayHandle types, MonoArrayHandle exceptions, MonoError *error);

MonoException *
mono_get_exception_runtime_wrapped_checked (MonoObject *wrapped_exception, MonoError *error);

MonoException *
mono_exception_from_name_two_strings_checked (MonoImage *image, const char *name_space,
					      const char *name, MonoString *a1, MonoString *a2,
					      MonoError *error);

MonoException *
mono_exception_from_token_two_strings_checked (MonoImage *image, uint32_t token,
					       MonoString *a1, MonoString *a2,
					       MonoError *error);

MonoException *
mono_exception_from_name_four_strings_checked (MonoImage *image, const char *name_space,
				      const char *name, MonoString *a1, MonoString *a2, MonoString *a3, MonoString *a4,
				      MonoError *error);


typedef int (*MonoGetSeqPointFunc) (MonoDomain *domain, MonoMethod *method, gint32 native_offset);

void
mono_install_get_seq_point (MonoGetSeqPointFunc func);

// MonoExceptionHandle functions are not all provided yet -- aspirational

MonoExceptionHandle
mono_exception_new_by_name_msg (MonoImage *image, const char *name_space,
			      const char *name, const char *msg, MonoError *error);

MonoExceptionHandle
mono_exception_new_divide_by_zero (MonoError *error);

MonoExceptionHandle
mono_exception_new_security (MonoError *error);

MonoExceptionHandle
mono_exception_new_arithmetic (MonoError *error);

MonoExceptionHandle
mono_exception_new_overflow (MonoError *error);

MonoExceptionHandle
mono_exception_new_null_reference (MonoError *error);

MonoExceptionHandle
mono_exception_new_execution_engine (const char *msg, MonoError *error);

MonoExceptionHandle
mono_exception_new_thread_abort (MonoError *error);

MonoExceptionHandle
mono_exception_new_thread_state (const char *msg, MonoError *error);

MonoExceptionHandle
mono_exception_new_thread_interrupted (MonoError *error);

MonoExceptionHandle
mono_exception_new_serialization (const char *msg, MonoError *error);

MonoExceptionHandle
mono_exception_new_invalid_cast (MonoError *error);

MonoExceptionHandle
mono_exception_new_invalid_operation (const char *msg, MonoError *error);

MonoExceptionHandle
mono_exception_new_index_out_of_range (MonoError *error);

MonoExceptionHandle
mono_exception_new_array_type_mismatch (MonoError *error);

MonoExceptionHandle
mono_exception_new_type_load (MonoString *class_name, char *assembly_name, MonoError *error);

MonoExceptionHandle
mono_exception_new_missing_method (const char *class_name, const char *member_name, MonoError *error);

MonoExceptionHandle
mono_exception_new_missing_field (const char *class_name, const char *member_name, MonoError *error);

MonoExceptionHandle
mono_exception_new_not_implemented (const char *msg, MonoError *error);

MonoExceptionHandle
mono_exception_new_not_supported (const char *msg, MonoError *error);

MonoExceptionHandle
mono_exception_new_argument_null (const char *arg, MonoError *error);

MonoExceptionHandle
mono_exception_new_argument (const char *arg, const char *msg, MonoError *error);

MonoExceptionHandle
mono_exception_new_argument_out_of_range (const char *arg, MonoError *error);

MonoExceptionHandle
mono_exception_new_io (const char *msg, MonoError *error);

MonoExceptionHandle
mono_exception_new_file_not_found (MonoString *fname, MonoError *error);

MonoExceptionHandle
mono_exception_new_synchronization_lock (const char *msg, MonoError *error);

MonoExceptionHandle
mono_exception_new_cannot_unload_appdomain (const char *msg, MonoError *error);

MonoExceptionHandle
mono_exception_new_appdomain_unloaded (MonoError *error);

MonoExceptionHandle
mono_exception_new_bad_image_format (const char *msg, MonoError *error);

MonoExceptionHandle
mono_exception_new_stack_overflow (MonoError *error);

MonoExceptionHandle
mono_exception_new_out_of_memory (MonoError *error);

MonoExceptionHandle
mono_exception_new_field_access (MonoError *error);

MonoExceptionHandle
mono_exception_new_method_access (MonoError *error);

MONO_END_DECLS

#endif /* _MONO_METADATA_EXCEPTION_INTERNALS_H_ */
