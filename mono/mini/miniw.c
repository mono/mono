/**
 * \file
 * functions that accept wchar_t strings
 * see https://github.com/mono/mono/issues/7117
 *
 * Author:
 *	Jay Krell (jaykrell@microsoft.com)
 *
 * Copyright (C) 2018 Microsoft Corporation (http://www.microsoft.com)
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#include <config.h>
#include <glib.h>
#include <mono/mini/jit.h>
#include <mono/mini/mini-runtime.h>

int
mono_main_w (int argc, wchar_t* argv_w[])
{
	gchar **argv = g_argvw_to_argv (argc, argv_w);

	int result = mono_main (argc, argv);

	g_strfreev (argv);

	return result;
}

MonoDomain *
mono_jit_init_w (const wchar_t *file_w)
{
	gchar *file = g_wcs_to_utf8 (file_w, -1);

	MonoDomain *domain = mono_jit_init (file);

	g_free (file);

	return domain;
}

MonoDomain *
mono_jit_init_version_w (const wchar_t *root_domain_name_w, const wchar_t *runtime_version_w)
{
	gchar *root_domain_name = g_wcs_to_utf8 (root_domain_name_w, -1);
	gchar *runtime_version = g_wcs_to_utf8 (runtime_version_w, -1);

	MonoDomain *domain = mono_jit_init_version (root_domain_name, runtime_version);

	g_free (root_domain_name);
	g_free (runtime_version);

	return domain;
}

int
mono_jit_exec_w (MonoDomain *domain, MonoAssembly *assembly, int argc, wchar_t *argv_w[])
{
	gchar **argv = g_argvw_to_argv (argc, argv_w);

	int result = mono_jit_exec (domain, assembly, argc, argv);

	g_strfreev (argv);

	return result;
}
