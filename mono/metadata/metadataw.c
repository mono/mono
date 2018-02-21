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
#include <mono/metadata/appdomain.h>

MonoAssembly *
mono_domain_assembly_open_w (MonoDomain *domain, const wchar_t *name_w)
{
	gchar *name = g_wcs_to_utf8 (name_w, -1);

	MonoAssembly *result = mono_domain_assembly_open (domain, name);

	g_free (name);

	return result;
}

MonoDomain *
mono_init_w (const wchar_t *filename_w)
{
	gchar *filename = g_wcs_to_utf8 (filename_w, -1);

	MonoDomain *domain = mono_init (filename);

	g_free (filename);

	return domain;
}

MonoDomain *
mono_init_from_assembly_w (const wchar_t *domain_name_w, const wchar_t *filename_w)
{
	gchar *domain_name = g_wcs_to_utf8 (domain_name_w, -1);
	gchar *filename = g_wcs_to_utf8 (filename_w, -1);

	MonoDomain *domain = mono_init_from_assembly (domain_name, filename);

	g_free (domain_name);
	g_free (filename);

	return domain;
}

MonoDomain *
mono_init_version_w (const wchar_t *domain_name_w, const wchar_t *version_w)
{
	gchar *domain_name = g_wcs_to_utf8 (domain_name_w, -1);
	gchar *version = g_wcs_to_utf8 (version_w, -1);

	MonoDomain *domain = mono_init_version (domain_name, version);

	g_free (domain_name);
	g_free (version);

	return domain;
}
