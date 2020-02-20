/**
 * \file Runtime flags
 *
 * Copyright 2020 Microsoft
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#include <stdio.h>

#include "flags.h"
#include "mono/utils/mono-error-internals.h"

typedef enum {
	MONO_FLAG_BOOL,
	MONO_FLAG_BOOL_READONLY,
	MONO_FLAG_INT,
	MONO_FLAG_STRING
} MonoFlagType;

/* Define flags */
#define DEFINE_FLAG_FULL(flag_type, ctype, c_name, cmd_name, def_value, comment) \
	ctype mono_##c_name = def_value;
#define DEFINE_FLAG_READONLY(flag_type, ctype, c_name, cmd_name, def_value, comment)
#include "flag-definitions.h"

/* Flag metadata */
typedef struct {
	MonoFlagType flag_type;
	gpointer addr;
	const char *cmd_name;
	int cmd_name_len;
} FlagData;

static FlagData flag_meta[] = {
#define DEFINE_FLAG_FULL(flag_type, ctype, c_name, cmd_name, def_value, comment) \
	{ flag_type, &mono_##c_name, cmd_name, sizeof (cmd_name) - 1 },
#define DEFINE_FLAG_READONLY(flag_type, ctype, c_name, cmd_name, def_value, comment) \
	{ flag_type, NULL, cmd_name, sizeof (cmd_name) - 1 },
#include "flag-definitions.h"
};

static const char*
flag_type_to_str (MonoFlagType type)
{
	switch (type) {
	case MONO_FLAG_BOOL:
		return "bool";
	case MONO_FLAG_BOOL_READONLY:
		return "bool (read-only)";
	case MONO_FLAG_INT:
		return "int";
	case MONO_FLAG_STRING:
		return "string";
	default:
		g_assert_not_reached ();
		return NULL;
	}
}

static char *
flag_value_to_str (MonoFlagType type, gconstpointer addr)
{
	switch (type) {
	case MONO_FLAG_BOOL:
	case MONO_FLAG_BOOL_READONLY:
		return *(gboolean*)addr ? g_strdup ("true") : g_strdup ("false");
	case MONO_FLAG_INT:
		return g_strdup_printf ("%d", *(int*)addr);
	case MONO_FLAG_STRING:
		return *(char**)addr ? g_strdup_printf ("%s", *(char**)addr) : g_strdup ("\"\"");
	default:
		g_assert_not_reached ();
		return NULL;
	}
}

void
mono_flags_print_usage (void)
{
#define DEFINE_FLAG_FULL(flag_type, ctype, c_name, cmd_name, def_value, comment) do { \
		char *val = flag_value_to_str (flag_type, &mono_##c_name); \
		g_printf ("  --%s (%s)\n\ttype: %s  default: %s\n", cmd_name, comment, flag_type_to_str (flag_type), val); \
		g_free (val); \
	} while (0);
#include "flag-definitions.h"
}

/*
 * mono_flags_parse_options:
 *
 *   Set flags based on the command line arguments in ARGV/ARGC.
 * Remove processed arguments from ARGV and set *OUT_ARGC to the
 * number of remaining arguments.
 */
void
mono_flags_parse_options (const char **argv, int argc, int *out_argc, MonoError *error)
{
	int aindex = 0;
	int i;
	GHashTable *flag_hash = NULL;

	while (aindex < argc) {
		const char *arg = argv [aindex];

		if (!(arg [0] == '-' && arg [1] == '-')) {
			aindex ++;
			continue;
		}
		arg = arg + 2;

		if (flag_hash == NULL) {
			/* Compute a hash to avoid n^2 behavior */
			flag_hash = g_hash_table_new (g_str_hash, g_str_equal);
			for (i = 0; i < G_N_ELEMENTS (flag_meta); ++i) {
				g_hash_table_insert (flag_hash, (gpointer)flag_meta [i].cmd_name, &flag_meta [i]);
			}
		}

		/* Compute flag name */
		char *arg_copy = g_strdup (arg);
		char *flagname = arg_copy;
		int len = strlen (arg);
		int equals_sign_index = -1;
		/* Handle no- prefix */
		if (flagname [0] == 'n' && flagname [1] == 'o' && flagname [2] == '-') {
			flagname += 3;
		} else {
			/* Handle option=value */
			for (int i = 0; i < len; ++i) {
				if (flagname [i] == '=') {
					equals_sign_index = i;
					flagname [i] = '\0';
					break;
				}
			}
		}

		FlagData *flag = (FlagData*)g_hash_table_lookup (flag_hash, flagname);
		g_free (arg_copy);

		if (!flag) {
			aindex ++;
			continue;
		}

		switch (flag->flag_type) {
		case MONO_FLAG_BOOL:
		case MONO_FLAG_BOOL_READONLY: {
			gboolean negate = FALSE;
			if (len == flag->cmd_name_len) {
			} else if (arg [0] == 'n' && arg [1] == 'o' && arg [2] == '-' && len == flag->cmd_name_len + 3) {
				negate = TRUE;
			} else {
				break;
			}
			if (flag->flag_type == MONO_FLAG_BOOL_READONLY) {
				mono_error_set_error (error, 1, "Unable to set option '%s' as it's read-only.\n", arg);
				break;
			}
			*(gboolean*)flag->addr = negate ? FALSE : TRUE;
			argv [aindex] = NULL;
			break;
		}
		case MONO_FLAG_INT:
		case MONO_FLAG_STRING: {
			const char *value = NULL;

			if (len == flag->cmd_name_len) {
				// --option value
				if (aindex + 1 == argc) {
					mono_error_set_error (error, 1, "Missing value for option '%s'.\n", flag->cmd_name);
					break;
				}
				value = argv [aindex + 1];
				argv [aindex] = NULL;
				argv [aindex + 1] = NULL;
				aindex ++;
			} else if (equals_sign_index != -1) {
				// option=value
				value = arg + equals_sign_index + 1;
				argv [aindex] = NULL;
			} else {
				g_assert_not_reached ();
			}

			if (flag->flag_type == MONO_FLAG_STRING) {
				*(char**)flag->addr = g_strdup (value);
			} else {
				char *endp;
				long v = strtol (value, &endp, 10);
				if (!value [0] || *endp) {
					mono_error_set_error (error, 1, "Invalid value for option '%s': '%s'.\n", flag->cmd_name, value);
					break;
				}
				*(int*)flag->addr = (int)v;
			}
			break;
		}
		default:
			g_assert_not_reached ();
			break;
		}

		if (!is_ok (error))
			break;
		aindex ++;
	}

	if (flag_hash)
		g_hash_table_destroy (flag_hash);
	if (!is_ok (error))
		return;

	/* Remove processed arguments */
	aindex = 0;
	for (i = 0; i < argc; ++i) {
		if (argv [i])
			argv [aindex ++] = argv [i];
	}
	*out_argc = aindex;
}
