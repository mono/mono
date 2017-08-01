/*
 * gunicode-win32-uwp.c: UWP unicode support.
 *
 * Copyright 2016 Microsoft
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
*/
#include <config.h>
#include <glib.h>

#if G_HAVE_API_SUPPORT(HAVE_UWP_WINAPI_SUPPORT)
#define CODESET 1
#include <windows.h>

extern const char *my_charset;
static gboolean is_utf8;

gboolean
g_get_charset (G_CONST_RETURN char **charset)
{
	if (my_charset == NULL) {
		static char buf [14];
		CPINFOEXA cp_info;

		GetCPInfoExA (CP_ACP, 0, &cp_info);
		sprintf (buf, "CP%u", cp_info.CodePage);
		my_charset = buf;
		is_utf8 = FALSE;
	}
	
	if (charset != NULL)
		*charset = my_charset;

	return is_utf8;
}

#else /* G_HAVE_API_SUPPORT(HAVE_UWP_WINAPI_SUPPORT) */

#ifdef _MSC_VER
// Quiet Visual Studio linker warning, LNK4221, in cases when this source file intentional ends up empty.
void __mono_win32_gunicode_win32_uwp_quiet_lnk4221(void) {}
#endif
#endif /* G_HAVE_API_SUPPORT(HAVE_UWP_WINAPI_SUPPORT) */
