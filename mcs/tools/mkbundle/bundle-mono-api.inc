/* -*- C -*- */
/*
 * The structure below should contain pointers to all the Mono runtime functions used throughout ANY
 * of the generated C code. The reason for this is to avoid symbol load failures when the generated
 * bundle is used by a third party which embeds Mono and loads it dynamically (like
 * Xamarin.Android). Third parties should provide their own version of the structure - compatible
 * with this one. This is done this way so that we don't have to make the API here public in any way
 * or form and thus maintain freedom to break it as we see needed.
 *
 * Whenever ANY change to this structure is made, the `init_default_mono_api_struct` and
 * `validate_api_struct` functions found in `template_common.inc` MUST be updated.
 *
 * The `mkbundle_log_error` must be provided by the embedding third party in order to implement a
 * logging method specific to that third party (e.g. Xamarin.Android cannot use `fprintf` since it
 * won't show up in the logcat).
 */
typedef struct BundleMonoAPI
{
	void (*mono_register_bundled_assemblies) (const MonoBundledAssembly **assemblies);
	void (*mono_register_config_for_assembly) (const char* assembly_name, const char* config_xml);
	void (*mono_jit_set_aot_mode) (MonoAotMode mode);
	void (*mono_aot_register_module) (void** aot_info);
	void (*mono_config_parse_memory) (const char *buffer);
	void (*mono_register_machine_config) (const char *config_xml);
} BundleMonoAPI;

#ifdef USE_DEFAULT_MONO_API_STRUCT
#include <stdio.h>
#include <stdarg.h>

static void
mkbundle_log_error (const char *format, ...)
{
	va_list ap;

	va_start (ap, format);
	vfprintf (stderr, format, ap);
	va_end (ap);
}
#endif // USE_DEFAULT_MONO_API_STRUCT
