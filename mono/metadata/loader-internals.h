/**
 * \file
 */

#ifndef _MONO_METADATA_LOADER_INTERNALS_H_
#define _MONO_METADATA_LOADER_INTERNALS_H_ 1

MONO_BEGIN_DECLS

// FIXME Replace all internal callers of mono_method_get_header_checked with
// mono_method_get_header_internal; the difference is in error initialization.
//
// Internal callers expected to use ERROR_DECL. External callers are not.
MonoMethodHeader*
mono_method_get_header_internal (MonoMethod *method, MonoError *error);

MONO_END_DECLS

#endif
