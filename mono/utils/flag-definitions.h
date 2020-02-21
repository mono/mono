/**
 * \file Runtime flags
 *
 * Copyright 2020 Microsoft
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

/*
 * This file defines all the flags/options which can be set at runtime.
 *
 * Flags defined here generate a C variable named mono_<flag name> initialized to its default value.
 * The variables are exported using MONO_API.
 * The _READONLY variants generate C const variables so the compiler can optimize away their usage.
 * Flag types:
 * BOOL - gboolean
 * INT - int
 * STRING - (malloc-ed) char*
 *
 * Flags can be set on the command line using:
 * --[no-]-flag (bool)
 * --flag=value (int/string)
 * --flag value (int/string)
 */

/*
 * This is a template header, the file including this needs to define this macro:
 * DEFINE_FLAG_FULL(flag_type, ctype, c_name, cmd_name, def_value, comment)
 * Optionally, define
 * DEFINE_FLAG_READONLY as well.
 */
#ifndef DEFINE_FLAG_FULL
#error ""
#endif
#ifndef DEFINE_FLAG_READONLY
#define DEFINE_FLAG_READONLY(flag_type, ctype, c_name, cmd_name, def_value, comment) DEFINE_FLAG_FULL(flag_type, ctype, c_name, cmd_name, def_value, comment)
#endif

/* Types of flags */
#define DEFINE_BOOL(name, cmd_name, def_value, comment) DEFINE_FLAG_FULL(MONO_FLAG_BOOL, gboolean, name, cmd_name, def_value, comment)
#define DEFINE_BOOL_READONLY(name, cmd_name, def_value, comment) DEFINE_FLAG_READONLY(MONO_FLAG_BOOL_READONLY, gboolean, name, cmd_name, def_value, comment)
#define DEFINE_INT(name, cmd_name, def_value, comment) DEFINE_FLAG_FULL(MONO_FLAG_INT, int, name, cmd_name, def_value, comment)
#define DEFINE_STRING(name, cmd_name, def_value, comment) DEFINE_FLAG_FULL(MONO_FLAG_STRING, char*, name, cmd_name, def_value, comment)

/*
 * List of runtime flags
 */
DEFINE_BOOL(bool_flag, "bool-flag", FALSE, "Example")
DEFINE_INT(int_flag, "int-flag", 0, "Example")
DEFINE_STRING(string_flag, "string-flag", NULL, "Example")

#ifdef ENABLE_EXAMPLE
DEFINE_BOOL(readonly_flag, "readonly-flag", FALSE, "Example")
#else
DEFINE_BOOL_READONLY(readonly_flag, "readonly-flag", FALSE, "Example")
#endif

/* Cleanup */
#undef DEFINE_FLAG_FULL
#undef DEFINE_FLAG_READONLY
