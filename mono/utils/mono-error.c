/*
 * mono-error.c: Error handling code
 *
 * Authors:
 *	Rodrigo Kumpera    (rkumpera@novell.com)
 * Copyright 2009 Novell, Inc (http://www.novell.com)
 */
#include <glib.h>

#include "mono-error.h"
#include "mono-error-internals.h"

#include <mono/metadata/exception.h>
#include <mono/metadata/class-internals.h>
#include <mono/metadata/debug-helpers.h>
#include <mono/metadata/object.h>
#include <mono/metadata/object-internals.h>

#define set_error_message() do { \
	va_list args; \
	va_start (args, msg_format); \
	if (!(error->full_message = g_strdup_vprintf (msg_format, args))) \
			error->flags |= MONO_ERROR_INCOMPLETE; \
	va_end (args); \
} while (0)

static gboolean
is_managed_exception (MonoErrorInternal *error)
{
	return (error->error_code == MONO_ERROR_EXCEPTION_INSTANCE);
}

static void
mono_error_prepare (MonoErrorInternal *error)
{
	if (error->error_code != MONO_ERROR_NONE)
		return;

	error->type_name = error->assembly_name = error->member_name = error->full_message = error->exception_name_space = error->exception_name = error->full_message_with_fields = error->first_argument = NULL;
	error->exn.klass = NULL;
}

static MonoClass*
get_class (MonoErrorInternal *error)
{
	MonoClass *klass = NULL;
	if (is_managed_exception (error))
		klass = mono_object_class (mono_gchandle_get_target (error->exn.instance_handle));
	else
		klass = error->exn.klass;
	return klass;
}

static const char*
get_type_name (MonoErrorInternal *error)
{
	if (error->type_name)
		return error->type_name;
	MonoClass *klass = get_class (error);
	if (klass)
		return klass->name;
	return "<unknown type>";
}

static const char*
get_assembly_name (MonoErrorInternal *error)
{
	if (error->assembly_name)
		return error->assembly_name;
	MonoClass *klass = get_class (error);
	if (klass && klass->image)
		return klass->image->name;
	return "<unknown assembly>";
}

void
mono_error_init_flags (MonoError *oerror, unsigned short flags)
{
	MonoErrorInternal *error = (MonoErrorInternal*)oerror;
	g_assert (sizeof (MonoError) == sizeof (MonoErrorInternal));

	error->error_code = MONO_ERROR_NONE;
	error->flags = flags;
}

void
mono_error_init (MonoError *error)
{
	mono_error_init_flags (error, 0);
}

void
mono_error_cleanup (MonoError *oerror)
{
	MonoErrorInternal *error = (MonoErrorInternal*)oerror;
	if (error->error_code == MONO_ERROR_NONE)
		return;

	if (is_managed_exception (error))
		mono_gchandle_free (error->exn.instance_handle);

	g_free ((char*)error->full_message);
	g_free ((char*)error->full_message_with_fields);
	if (!(error->flags & MONO_ERROR_FREE_STRINGS)) //no memory was allocated
		return;

	g_free ((char*)error->type_name);
	g_free ((char*)error->assembly_name);
	g_free ((char*)error->member_name);
	g_free ((char*)error->exception_name_space);
	g_free ((char*)error->exception_name);
	g_free ((char*)error->first_argument);
}

gboolean
mono_error_ok (MonoError *error)
{
	return error->error_code == MONO_ERROR_NONE;
}

void
mono_error_assert_ok (MonoError *error)
{
	if (mono_error_ok (error))
		return;

	g_error ("%s\n", mono_error_get_message (error));
}

unsigned short
mono_error_get_error_code (MonoError *error)
{
	return error->error_code;
}

/*Return a pointer to the internal error message, might be NULL.
Caller should not release it.*/
const char*
mono_error_get_message (MonoError *oerror)
{
	MonoErrorInternal *error = (MonoErrorInternal*)oerror;
	if (error->error_code == MONO_ERROR_NONE)
		return NULL;
	if (error->full_message_with_fields)
		return error->full_message_with_fields;

	error->full_message_with_fields = g_strdup_printf ("%s assembly:%s type:%s member:%s",
		error->full_message,
		get_assembly_name (error),
		get_type_name (error),
		error->member_name ? error->member_name : "<none>");

	return error->full_message_with_fields ? error->full_message_with_fields : error->full_message;
}

/*
 * Inform that this error has heap allocated strings.
 * The strings will be duplicated if @dup_strings is TRUE
 * otherwise they will just be free'd in mono_error_cleanup.
 */
void
mono_error_dup_strings (MonoError *oerror, gboolean dup_strings)
{
#define DUP_STR(field) do { if (error->field) {\
	if (!(error->field = g_strdup (error->field))) \
		error->flags |= MONO_ERROR_INCOMPLETE; \
	}} while (0);

	MonoErrorInternal *error = (MonoErrorInternal*)oerror;
	error->flags |= MONO_ERROR_FREE_STRINGS;

	if (dup_strings) {
		DUP_STR (type_name);
		DUP_STR (assembly_name);
		DUP_STR (member_name);
		DUP_STR (exception_name_space);
		DUP_STR (exception_name);
		DUP_STR (first_argument);
	}
#undef DUP_STR
}

void
mono_error_set_error (MonoError *oerror, int error_code, const char *msg_format, ...)
{
	MonoErrorInternal *error = (MonoErrorInternal*)oerror;
	mono_error_prepare (error);

	error->error_code = error_code;
	set_error_message ();
}

static void
mono_error_set_assembly_name (MonoError *oerror, const char *assembly_name)
{
	MonoErrorInternal *error = (MonoErrorInternal*)oerror;
	g_assert (error->error_code != MONO_ERROR_NONE);

	error->assembly_name = assembly_name;
}

static void
mono_error_set_member_name (MonoError *oerror, const char *member_name)
{
	MonoErrorInternal *error = (MonoErrorInternal*)oerror;

	error->member_name = member_name;
}

static void
mono_error_set_type_name (MonoError *oerror, const char *type_name)
{
	MonoErrorInternal *error = (MonoErrorInternal*)oerror;

	error->type_name = type_name;
}

static void
mono_error_set_class (MonoError *oerror, MonoClass *klass)
{
	MonoErrorInternal *error = (MonoErrorInternal*)oerror;

	if (is_managed_exception (error))
		return;
	error->exn.klass = klass;	
}

static void
mono_error_set_corlib_exception (MonoError *oerror, const char *name_space, const char *name)
{
	MonoErrorInternal *error = (MonoErrorInternal*)oerror;

	error->exception_name_space = name_space;
	error->exception_name = name;
}


void
mono_error_set_assembly_load (MonoError *oerror, const char *assembly_name, const char *msg_format, ...)
{
	MonoErrorInternal *error = (MonoErrorInternal*)oerror;
	mono_error_prepare (error);

	error->error_code = MONO_ERROR_FILE_NOT_FOUND;
	mono_error_set_assembly_name (oerror, assembly_name);

	set_error_message ();
}


void
mono_error_set_assembly_load_simple (MonoError *oerror, const char *assembly_name, gboolean refection_only)
{
	if (refection_only)
		mono_error_set_assembly_load (oerror, assembly_name, "Cannot resolve dependency to assembly because it has not been preloaded. When using the ReflectionOnly APIs, dependent assemblies must be pre-loaded or loaded on demand through the ReflectionOnlyAssemblyResolve event.");
	else
		mono_error_set_assembly_load (oerror, assembly_name, "Could not load file or assembly or one of its dependencies.");
}

void
mono_error_set_type_load_class (MonoError *oerror, MonoClass *klass, const char *msg_format, ...)
{
	MonoErrorInternal *error = (MonoErrorInternal*)oerror;
	mono_error_prepare (error);

	error->error_code = MONO_ERROR_TYPE_LOAD;
	mono_error_set_class (oerror, klass);
	set_error_message ();
}

/*
 * Different than other functions, this one here assumes that type_name and assembly_name to have been allocated just for us.
 * Which means mono_error_cleanup will free them.
 */
void
mono_error_set_type_load_name (MonoError *oerror, const char *type_name, const char *assembly_name, const char *msg_format, ...)
{
	MonoErrorInternal *error = (MonoErrorInternal*)oerror;
	mono_error_prepare (error);

	error->error_code = MONO_ERROR_TYPE_LOAD;
	mono_error_set_type_name (oerror, type_name);
	mono_error_set_assembly_name (oerror, assembly_name);
	mono_error_dup_strings (oerror, FALSE);
	set_error_message ();
}

void
mono_error_set_method_load (MonoError *oerror, MonoClass *klass, const char *method_name, const char *msg_format, ...)
{
	MonoErrorInternal *error = (MonoErrorInternal*)oerror;
	mono_error_prepare (error);

	error->error_code = MONO_ERROR_MISSING_METHOD;
	mono_error_set_class (oerror, klass);
	mono_error_set_member_name (oerror, method_name);
	set_error_message ();
}

void
mono_error_set_field_load (MonoError *oerror, MonoClass *klass, const char *field_name, const char *msg_format, ...)
{
	MonoErrorInternal *error = (MonoErrorInternal*)oerror;
	mono_error_prepare (error);

	error->error_code = MONO_ERROR_MISSING_FIELD;
	mono_error_set_class (oerror, klass);
	mono_error_set_member_name (oerror, field_name);
	set_error_message ();	
}

void
mono_error_set_bad_image_name (MonoError *oerror, const char *assembly_name, const char *msg_format, ...)
{
	MonoErrorInternal *error = (MonoErrorInternal*)oerror;
	mono_error_prepare (error);

	error->error_code = MONO_ERROR_BAD_IMAGE;
	mono_error_set_assembly_name (oerror, assembly_name);
	set_error_message ();
}

void
mono_error_set_bad_image (MonoError *oerror, MonoImage *image, const char *msg_format, ...)
{
	MonoErrorInternal *error = (MonoErrorInternal*)oerror;
	mono_error_prepare (error);

	error->error_code = MONO_ERROR_BAD_IMAGE;
	error->assembly_name = image ? mono_image_get_name (image) : "<no_image>";
	set_error_message ();
}

void
mono_error_set_generic_error (MonoError *oerror, const char * name_space, const char *name, const char *msg_format, ...)
{
	MonoErrorInternal *error = (MonoErrorInternal*)oerror;
	mono_error_prepare (error);

	error->error_code = MONO_ERROR_GENERIC;
	mono_error_set_corlib_exception (oerror, name_space, name);
	set_error_message ();
}

void
mono_error_set_exception_instance (MonoError *oerror, MonoException *exc)
{
	MonoErrorInternal *error = (MonoErrorInternal*)oerror;

	error->error_code = MONO_ERROR_EXCEPTION_INSTANCE;
	error->exn.instance_handle = mono_gchandle_new (exc ? &exc->object : NULL, FALSE);
}

void
mono_error_set_from_loader_error (MonoError *oerror)
{
	MonoLoaderError *loader_error = mono_loader_get_last_error ();
	MonoErrorInternal *error = (MonoErrorInternal*)oerror;
	gboolean dup_strings = TRUE;

	mono_error_prepare (error);

	if (!loader_error) {
		mono_error_set_generic_error (oerror, "System", "ExecutionEngineException", "Runtime tried to produce a mono-error from an empty loader-error");
		return;
	}

	switch (loader_error->exception_type) {
	case MONO_EXCEPTION_NONE:
		mono_error_set_generic_error (oerror, "System", "ExecutionEngineException", "Runtime tried to produce a mono-error from a non-error loader-error");
		break;

	case MONO_EXCEPTION_INVALID_PROGRAM:
		mono_error_set_generic_error (oerror, "System", "InvalidProgramException", "Failed for unknown reasons.");
		break;

	case MONO_EXCEPTION_UNVERIFIABLE_IL:
		mono_error_set_generic_error (oerror, "System.Security", "VerificationException", "Failed for unknown reasons.");
		break;

	case MONO_EXCEPTION_MISSING_METHOD:
		error->error_code = MONO_ERROR_MISSING_METHOD;
		mono_error_set_type_name (oerror, loader_error->class_name);
		mono_error_set_member_name (oerror, loader_error->member_name);
		error->full_message = g_strdup ("Failed for unknown reasons.");
		break;

	case MONO_EXCEPTION_MISSING_FIELD:
		mono_error_set_field_load (oerror, loader_error->klass, loader_error->member_name, "Failed for unknown reasons.");
		break;

	case MONO_EXCEPTION_TYPE_LOAD:
		mono_error_set_type_load_name (oerror, g_strdup (loader_error->class_name), g_strdup (loader_error->assembly_name), "Failed for unknown reasons.");
		dup_strings = FALSE;
		break;
	
	case MONO_EXCEPTION_FILE_NOT_FOUND:
		mono_error_set_assembly_load_simple (oerror, loader_error->assembly_name, loader_error->ref_only);
		break;

	case MONO_EXCEPTION_METHOD_ACCESS:
		mono_error_set_generic_error (oerror, "System", "MethodAccessException", "Failed for unknown reasons.");
		break;

	case MONO_EXCEPTION_FIELD_ACCESS:
		mono_error_set_generic_error (oerror, "System", "FieldAccessException", "Failed for unknown reasons.");
		break;

	case MONO_EXCEPTION_OBJECT_SUPPLIED:
	case MONO_EXCEPTION_GENERIC_SHARING_FAILED:
		mono_error_set_generic_error (oerror, "System", "ExecutionEngineException", "Runtime tried to produce a mono-error from JIT internal error %d", loader_error->exception_type);
		break;

	case MONO_EXCEPTION_BAD_IMAGE:
		mono_error_set_bad_image_name (oerror, "<unknown>", "%s", loader_error->msg);
		break;

	case MONO_EXCEPTION_OUT_OF_MEMORY:
		mono_error_set_out_of_memory (oerror, "Failed for unknown reasons.");
		break;

	default:
		mono_error_set_generic_error (oerror, "System", "ExecutionEngineException", "Runtime tried to produce an unknown loader-error %d", loader_error->exception_type);
		break;
	}

	mono_error_dup_strings (oerror, dup_strings);
	mono_loader_clear_error ();
}

void
mono_loader_set_error_from_mono_error (MonoError *oerror)
{
	MonoErrorInternal *error = (MonoErrorInternal*)oerror;

	switch (error->error_code) {
	case MONO_ERROR_MISSING_METHOD:
		mono_loader_set_error_method_load (get_type_name (error), error->member_name);
		break;
	case MONO_ERROR_MISSING_FIELD:
		mono_loader_set_error_field_load (error->exn.klass, error->member_name);
		break;
	case MONO_ERROR_TYPE_LOAD:
		mono_loader_set_error_type_load (get_type_name (error), get_assembly_name (error));
		break;
	case MONO_ERROR_FILE_NOT_FOUND:
		/* XXX can't recover if it's ref only or not */
		mono_loader_set_error_assembly_load (get_assembly_name (error), FALSE);
		break;
	case MONO_ERROR_BAD_IMAGE:
		mono_loader_set_error_bad_image (g_strdup (error->full_message));
		break;
	case MONO_ERROR_EXCEPTION_INSTANCE:
		mono_loader_set_error_bad_image (g_strdup_printf ("Non translatable error"));
	default:
		mono_loader_set_error_bad_image (g_strdup_printf ("Non translatable error: %s", error->full_message));
	}
}

void
mono_error_set_out_of_memory (MonoError *oerror, const char *msg_format, ...)
{
	MonoErrorInternal *error = (MonoErrorInternal*)oerror;
	mono_error_prepare (error);

	error->error_code = MONO_ERROR_OUT_OF_MEMORY;

	set_error_message ();
}

void
mono_error_set_argument (MonoError *oerror, const char *argument, const char *msg_format, ...)
{
	MonoErrorInternal *error = (MonoErrorInternal*)oerror;
	mono_error_prepare (error);

	error->error_code = MONO_ERROR_ARGUMENT;
	error->first_argument = argument;

	set_error_message ();
}

void
mono_error_set_argument_null (MonoError *oerror, const char *argument, const char *msg_format, ...)
{
	MonoErrorInternal *error = (MonoErrorInternal*)oerror;
	mono_error_prepare (error);

	error->error_code = MONO_ERROR_ARGUMENT_NULL;
	error->first_argument = argument;

	set_error_message ();
}

void
mono_error_set_not_verifiable (MonoError *oerror, MonoMethod *method, const char *msg_format, ...)
{
	MonoErrorInternal *error = (MonoErrorInternal*)oerror;
	mono_error_prepare (error);

	error->error_code = MONO_ERROR_NOT_VERIFIABLE;
	if (method) {
		mono_error_set_class (oerror, method->klass);
		mono_error_set_member_name (oerror, mono_method_full_name (method, 1));
	}

	set_error_message ();
}


static MonoString*
get_type_name_as_mono_string (MonoErrorInternal *error, MonoDomain *domain, MonoError *error_out)
{
	MonoString* res = NULL;

	if (error->type_name) {
		res = mono_string_new (domain, error->type_name);
		
	} else {
		MonoClass *klass = get_class (error);
		if (klass) {
			char *name = mono_type_full_name (&klass->byval_arg);
			if (name) {
				res = mono_string_new (domain, name);
				g_free (name);
			}
		}
	}
	if (!res)
		mono_error_set_out_of_memory (error_out, "Could not allocate type name");
	return res;
}

static void
set_message_on_exception (MonoException *exception, MonoErrorInternal *error, MonoError *error_out)
{
	MonoString *msg = mono_string_new (mono_domain_get (), error->full_message);
	if (msg)
		MONO_OBJECT_SETREF (exception, message, msg);
	else
		mono_error_set_out_of_memory (error_out, "Could not allocate exception object");
}

/*Can fail with out-of-memory*/
MonoException*
mono_error_prepare_exception (MonoError *oerror, MonoError *error_out)
{
	MonoErrorInternal *error = (MonoErrorInternal*)oerror;

	MonoException* exception = NULL;
	MonoString *assembly_name = NULL, *type_name = NULL, *method_name = NULL, *field_name = NULL, *msg = NULL;
	MonoDomain *domain = mono_domain_get ();

	mono_error_init (error_out);

	switch (error->error_code) {
	case MONO_ERROR_NONE:
		return NULL;

	case MONO_ERROR_MISSING_METHOD:
		if ((error->type_name || error->exn.klass) && error->member_name) {
			type_name = get_type_name_as_mono_string (error, domain, error_out);
			if (!mono_error_ok (error_out))
				break;

			method_name = mono_string_new (domain, error->member_name);
			if (!method_name) {
				mono_error_set_out_of_memory (error_out, "Could not allocate method name");
				break;
			}

			exception = mono_exception_from_name_two_strings (mono_defaults.corlib, "System", "MissingMethodException", type_name, method_name);
			if (exception)
				set_message_on_exception (exception, error, error_out);
		} else {
			exception = mono_exception_from_name_msg (mono_defaults.corlib, "System", "MissingMethodException", error->full_message);
		}
		break;

	case MONO_ERROR_MISSING_FIELD:
		if ((error->type_name || error->exn.klass) && error->member_name) {
			type_name = get_type_name_as_mono_string (error, domain, error_out);
			if (!mono_error_ok (error_out))
				break;
			
			field_name = mono_string_new (domain, error->member_name);
			if (!field_name) {
				mono_error_set_out_of_memory (error_out, "Could not allocate field name");
				break;
			}
			
			exception = mono_exception_from_name_two_strings (mono_defaults.corlib, "System", "MissingFieldException", type_name, field_name);
			if (exception)
				set_message_on_exception (exception, error, error_out);
		} else {
			exception = mono_exception_from_name_msg (mono_defaults.corlib, "System", "MissingFieldException", error->full_message);
		}
		break;

	case MONO_ERROR_TYPE_LOAD:
		if (error->type_name || error->assembly_name) {
			type_name = get_type_name_as_mono_string (error, domain, error_out);
			if (!mono_error_ok (error_out))
				break;

			if (error->assembly_name) {
				assembly_name = mono_string_new (domain, error->assembly_name);
				if (!assembly_name) {
					mono_error_set_out_of_memory (error_out, "Could not allocate assembly name");
					break;
				}
			}

			exception = mono_exception_from_name_two_strings (mono_get_corlib (), "System", "TypeLoadException", type_name, assembly_name);
			if (exception)
				set_message_on_exception (exception, error, error_out);
		} else {
			exception = mono_exception_from_name_msg (mono_defaults.corlib, "System", "TypeLoadException", error->full_message);
		}
		break;

	case MONO_ERROR_FILE_NOT_FOUND:
	case MONO_ERROR_BAD_IMAGE:
		if (error->assembly_name) {
			msg = mono_string_new (domain, error->full_message);
			if (!msg) {
				mono_error_set_out_of_memory (error_out, "Could not allocate message");
				break;
			}

			if (error->assembly_name) {
				assembly_name = mono_string_new (domain, error->assembly_name);
				if (!assembly_name) {
					mono_error_set_out_of_memory (error_out, "Could not allocate assembly name");
					break;
				}
			}

			if (error->error_code == MONO_ERROR_FILE_NOT_FOUND)
				exception = mono_exception_from_name_two_strings (mono_get_corlib (), "System.IO", "FileNotFoundException", msg, assembly_name);
			else
				exception = mono_exception_from_name_two_strings (mono_defaults.corlib, "System", "BadImageFormatException", msg, assembly_name);
		} else {
			if (error->error_code == MONO_ERROR_FILE_NOT_FOUND)
				exception = mono_exception_from_name_msg (mono_get_corlib (), "System.IO", "FileNotFoundException", error->full_message);
			else
				exception = mono_exception_from_name_msg (mono_defaults.corlib, "System", "BadImageFormatException", error->full_message);
		}
		break;

	case MONO_ERROR_OUT_OF_MEMORY:
		exception = mono_get_exception_out_of_memory ();
		break;

	case MONO_ERROR_ARGUMENT:
		exception = mono_get_exception_argument (error->first_argument, error->full_message);
		break;

	case MONO_ERROR_ARGUMENT_NULL:
		exception = mono_get_exception_argument_null (error->first_argument);
		break;

	case MONO_ERROR_NOT_VERIFIABLE: {
		char *type_name = NULL, *message;
		if (error->exn.klass) {
			type_name = mono_type_get_full_name (error->exn.klass);
			if (!type_name) {
				mono_error_set_out_of_memory (error_out, "Could not allocate message");
				break;
			}
		}
		message = g_strdup_printf ("Error in %s:%s %s", type_name, error->member_name, error->full_message);
		if (!message) {
			g_free (type_name);
			mono_error_set_out_of_memory (error_out, "Could not allocate message");
			break;	
		}
		exception = mono_exception_from_name_msg (mono_defaults.corlib, "System.Security", "VerificationException", message);
		g_free (message);
		g_free (type_name);
		break;
	}
	case MONO_ERROR_GENERIC:
		if (!error->exception_name_space || !error->exception_name)
			mono_error_set_generic_error (error_out, "System", "ExecutionEngineException", "MonoError with generic error but no exception name was supplied");
		else
			exception = mono_exception_from_name_msg (mono_defaults.corlib, error->exception_name_space, error->exception_name, error->full_message);
		break;

	case MONO_ERROR_EXCEPTION_INSTANCE:
		exception = (MonoException*) mono_gchandle_get_target (error->exn.instance_handle);
		break;

	default:
		mono_error_set_generic_error (error_out, "System", "ExecutionEngineException", "Invalid error-code %d", error->error_code);
	}

	if (!mono_error_ok (error_out))
		return NULL;
	if (!exception)
		mono_error_set_out_of_memory (error_out, "Could not allocate exception object");
	return exception;
}

/*
Convert this MonoError to an exception if it's faulty or return NULL.
The error object is cleant after.
*/

MonoException*
mono_error_convert_to_exception (MonoError *target_error)
{
	MonoError error;
	MonoException *ex;

	if (mono_error_ok (target_error))
		return NULL;

	ex = mono_error_prepare_exception (target_error, &error);
	if (!mono_error_ok (&error)) {
		MonoError second_chance;
		/*Try to produce the exception for the second error. FIXME maybe we should log about the original one*/
		ex = mono_error_prepare_exception (&error, &second_chance);

		g_assert (mono_error_ok (&second_chance)); /*We can't reasonable handle double faults, maybe later.*/
		mono_error_cleanup (&error);
	}
	mono_error_cleanup (target_error);
	return ex;
}
