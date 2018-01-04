/**
 * \file
 */

#ifndef __MONO_ERROR_INTERNALS_H__
#define __MONO_ERROR_INTERNALS_H__

#include "mono/utils/mono-compiler.h"
#include "mono/metadata/class-internals.h"

/*Keep in sync with MonoError*/
typedef struct {
	unsigned short error_code;
    unsigned short flags;

	/*These name are suggestions of their content. MonoError internals might use them for something else.*/
	const char *type_name;
	const char *assembly_name;
	const char *member_name;
	const char *exception_name_space;
	const char *exception_name;
	union {
		/* Valid if error_code != MONO_ERROR_EXCEPTION_INSTANCE.
		 * Used by type or field load errors and generic error specified by class.
		 */
		MonoClass *klass;
		/* Valid if error_code == MONO_ERROR_EXCEPTION_INSTANCE.
		 * Generic error specified by a managed instance.
		 */
		uint32_t instance_handle;
	} exn;
	const char *full_message;
	const char *full_message_with_fields;
	const char *first_argument;
	const char *member_signature;

	void *padding [2];
} MonoErrorInternal;

/* Invariant: the error strings are allocated in the mempool of the given image */
struct _MonoErrorBoxed {
	MonoError error;
	MonoImage *image;
};

/*
Historically MonoError initialization was deferred, but always had to occur,
	even in success paths, as cleanup could be done unconditionally.
	This was confusing.

ERROR_DECL (error)
	This is the overwhelmingly common case, including
	that the parameter is named "error".
	Declare and initialize a local variable, named "error",
	pointing to an initialized MonoError, named "error_value",
	using token pasting. The code need not concern
	itself with "error_value", just "error".

ERROR_DECL_VALUE (error)
	Declare and initialize a local variable, named "error";
	no pointer is produced for it. Parameter is usually
	not "error". As there is no pointer produced,
	these uses come with a bunch of ampersands.

MONO_API_ERROR_INIT (error)
	This is used for MonoError in/out parameter on a public interface,
	which must be presumed uninitialized for compatibility.
	These are in headers without "internals" or "types" in their name.
	For example:
		grep -r MonoError /Library/Frameworks/Mono.framework/Versions/5.4.1/include
			mono_class_from_typeref_checked
			mono_method_get_header_checked
			mono_string_to_utf8_checked
			mono_reflection_get_custom_attrs_by_type
	Also optionally for compatibility with profiler and Xamarin.
	This is not functions marked MONO_API per se, as that is a superset.
	Consider renaming to error_init_public or MONO_PUBLIC_ERROR_INIT, etc.

error_init (error)
	Initialize a MonoError. These are historical and usually
	but not always redundant, and should be reduced/eliminated.
	All the non-redundant ones should be renamed and all the redundant
	ones removed.

error_init_reuse (error)
	Initialize an error again after mono_error_cleanup, for reuse.
	Or maybe mono_error_cleanup should do this.

error_init_check (error)
	With the transition to ERROR_DECL, most error_init should be removable.
	However this reveals places that were missing initialization.
	Place error_init_check to look for missing initialization and add new
	initialization that was missing. Where this helps is typically bugs,
	unintended reuse of error and/or forgetting to check for error.
	i.e. put in missing error checks, or put in error_init_reuse,
	or consider having mono_error_cleanup end with init.

error_init_internal (error)
	Rare cases without a better name.
	For example, setting up an icall frame, or initializing member data.
	Best to layer something over this for search.

new0, calloc, static
	A zeroed MonoError is valid and initialized.
	Zeroing an entire MonoError is overkill, unless it is near other
	bulk zeroing.

All initialization is actually bottlenecked to error_init_internal.
Different names indicate different scenarios, but the same code.
*/
#define ERROR_DECL_VALUE(x) 		MonoError x; error_init_internal (&x)
#define ERROR_DECL(x) 			ERROR_DECL_VALUE (x##_value); MonoError * const x = &x##_value
#define error_init_internal(error) 	((void)((error)->init = 0))
#define MONO_API_ERROR_INIT(error) 	error_init_internal (error)

// Not reliable -- looking at uninitialized memory, could be zero by chance.
// For use with assert/warn/check.
#define error_is_initialized_maybe(error) (G_LIKELY ((error)->init == 0))

// Report a warning or assertion failure.
void
mono_error_init_check_failed (const char* file, unsigned line, const char* function);

// Warn or assert, and then initialize.
#define error_init_check(error) 						\
	(error_is_initialized_maybe (error) || 					\
		(mono_error_init_check_failed (__FILE__, __LINE__, __func__), 	\
		 error_init_internal (error), 0))

// Briefly historical due to cleanup + reuse, but make cleanup leave it initialized.
///#define error_init_reuse(error)	error_init_check (error)
// Still controversial.
#define error_init_reuse(error)		error_init_internal (error)

// Historical initializer, now deprecated.
// This can/should be tweaked among several options.
//void error_init(MonoError*);							// incremental build
//#define error_init(error) ((error)->init = 0)					// silently init -- safest
//#define error_init(error) /* do nothing */					// removal -- ideal but risky
//#define error_init(error) g_assert (error_is_initialized_maybe (error)) 	// find bugs harshly
//#define error_init(error) g_warn_if_fail (error_is_initialized_maybe (error)) // find bugs gently
#define error_init(error) error_init_check (error)			  	// find bugs, gentleness set in .c file

#define is_ok(error) ((error)->error_code == MONO_ERROR_NONE)

#define return_if_nok(error) do { if (!is_ok ((error))) return; } while (0)
#define return_val_if_nok(error,val) do { if (!is_ok ((error))) return (val); } while (0)

#define goto_if_nok(error,label) do { if (!is_ok ((error))) goto label; } while (0)

/* Only use this in icalls */
#define return_val_and_set_pending_if_nok(error, value) \
do { 							\
	if (mono_error_set_pending_exception ((error)))	\
		return (value); 			\
} while (0)						\

/*
 * Three macros to assert that a MonoError is ok:
 * 1. mono_error_assert_ok(e) when you just want to print the error's message on failure
 * 2. mono_error_assert_ok(e,msg) when you want to print "msg, due to <e's message>"
 * 3. mono_error_assertf_ok(e,fmt,args...) when you want to print "<formatted msg>, due to <e's message>"
 *    (fmt should specify the formatting just for args).
 *
 * What's the difference between mono_error_assert_msg_ok (e, "foo") and
 * mono_error_assertf_ok (e, "foo") ?  The former works as you expect, the
 * latter unhelpfully expands to
 *
 * g_assertf (is_ok (e), "foo, due to %s", ,  mono_error_get_message (err)).
 *
 * Note the double commas.  Turns out that to get rid of that extra comma
 * portably we would have to write really ugly preprocessor macros.
 */
#define mono_error_assert_ok(error)            g_assertf (is_ok (error), "%s", mono_error_get_message (error))
#define mono_error_assert_msg_ok(error, msg)   g_assertf (is_ok (error), msg ", due to %s", mono_error_get_message (error))
#define mono_error_assertf_ok(error, fmt, ...) g_assertf (is_ok (error), fmt ", due to %s", __VA_ARGS__, mono_error_get_message (error))

void
mono_error_dup_strings (MonoError *error, gboolean dup_strings);

/* This function is not very useful as you can't provide any details beyond the message.*/
void
mono_error_set_error (MonoError *error, int error_code, const char *msg_format, ...) MONO_ATTR_FORMAT_PRINTF(3,4);

void
mono_error_set_assembly_load (MonoError *error, const char *assembly_name, const char *msg_format, ...) MONO_ATTR_FORMAT_PRINTF(3,4);

void
mono_error_set_assembly_load_simple (MonoError *error, const char *assembly_name, gboolean refection_only);

void
mono_error_set_type_load_class (MonoError *error, MonoClass *klass, const char *msg_format, ...) MONO_ATTR_FORMAT_PRINTF(3,4);

void
mono_error_vset_type_load_class (MonoError *error, MonoClass *klass, const char *msg_format, va_list args);

void
mono_error_set_type_load_name (MonoError *error, const char *type_name, const char *assembly_name, const char *msg_format, ...) MONO_ATTR_FORMAT_PRINTF(4,5);

void
mono_error_set_method_load (MonoError *oerror, MonoClass *klass, const char *method_name, const char *signature, const char *msg_format, ...);

void
mono_error_set_field_load (MonoError *error, MonoClass *klass, const char *field_name, const char *msg_format, ...)  MONO_ATTR_FORMAT_PRINTF(4,5);

void
mono_error_set_bad_image (MonoError *error, MonoImage *image, const char *msg_format, ...) MONO_ATTR_FORMAT_PRINTF(3,4);

void
mono_error_set_bad_image_name (MonoError *error, const char *file_name, const char *msg_format, ...) MONO_ATTR_FORMAT_PRINTF(3,4);

void
mono_error_set_out_of_memory (MonoError *error, const char *msg_format, ...) MONO_ATTR_FORMAT_PRINTF(2,3);

void
mono_error_set_argument (MonoError *error, const char *argument, const char *msg_format, ...) MONO_ATTR_FORMAT_PRINTF(3,4);

void
mono_error_set_argument_null (MonoError *oerror, const char *argument, const char *msg_format, ...) MONO_ATTR_FORMAT_PRINTF(3,4);

void
mono_error_set_not_verifiable (MonoError *oerror, MonoMethod *method, const char *msg_format, ...) MONO_ATTR_FORMAT_PRINTF(3,4);

void
mono_error_set_generic_error (MonoError *error, const char * name_space, const char *name, const char *msg_format, ...) MONO_ATTR_FORMAT_PRINTF(4,5);

void
mono_error_set_execution_engine (MonoError *error, const char *msg_format, ...) MONO_ATTR_FORMAT_PRINTF(2,3);

void
mono_error_set_not_implemented (MonoError *error, const char *msg_format, ...) MONO_ATTR_FORMAT_PRINTF(2,3);

void
mono_error_set_not_supported (MonoError *error, const char *msg_format, ...) MONO_ATTR_FORMAT_PRINTF(2,3);

void
mono_error_set_invalid_operation (MonoError *error, const char *msg_format, ...) MONO_ATTR_FORMAT_PRINTF(2,3);

void
mono_error_set_file_not_found (MonoError *error, const char *msg_format, ...) MONO_ATTR_FORMAT_PRINTF(2,3);

void
mono_error_set_exception_instance (MonoError *error, MonoException *exc);

void
mono_error_set_invalid_program (MonoError *oerror, const char *msg_format, ...) MONO_ATTR_FORMAT_PRINTF(2,3);

void
mono_error_set_invalid_cast (MonoError *oerror);

MonoException*
mono_error_prepare_exception (MonoError *error, MonoError *error_out);

MonoException*
mono_error_convert_to_exception (MonoError *error);

void
mono_error_move (MonoError *dest, MonoError *src);

MonoErrorBoxed*
mono_error_box (const MonoError *error, MonoImage *image);

gboolean
mono_error_set_from_boxed (MonoError *error, const MonoErrorBoxed *from);

const char*
mono_error_get_exception_name (MonoError *oerror);

#endif
