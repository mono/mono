/*
 * test-path.c
 */

#include "config.h"
#include <stdio.h>
#include <stdlib.h>
#include "glib.h"

gboolean mono_host_win32;
#undef HOST_WIN32
#define HOST_WIN32 mono_host_win32
#include "utils/mono-path.c"

static char*
make_path (const char *a, int itrail, int slash, int upcase, int win32)
{
	mono_host_win32 = !!win32;
	char trail [3] = {'/', '/'};
	trail [itrail] = 0;
	char *b = g_strdup_printf ("%s%s", a, trail);
	if (win32) {
		if (slash)
			g_strdelimit (b, '/', '\\');
		if (upcase)
			g_strdelimit (b, 'f', 'F');
	}
	return b;
}

int
main (void)
{
	static const char * const bases [2] = {"/", "/foo"};
	static const char * const files [3] = {"/foo", "/foo/bar", "/foob"};

	static const gboolean result [2][3] = {
		{ TRUE, FALSE, TRUE },
		{ FALSE, TRUE, FALSE }
	};

	int i = 0;
	gboolean const verbose = !!getenv("V");

	for (int win32 = 0; win32 <= 1; ++win32) {
		for (int upcase_file = 0; upcase_file <= 1; ++upcase_file) {
			for (int upcase_base = 0; upcase_base <= 1; ++upcase_base) {
				for (int itrail_base = 0; itrail_base <= 2; ++itrail_base) {
					for (int itrail_file = 0; itrail_file <= 2; ++itrail_file) {
						for (int ibase = 1; ibase < G_N_ELEMENTS (bases); ++ibase) {
							for (int ifile = 0; ifile < G_N_ELEMENTS (files); ++ifile) {
								for (int islash_base = 0; islash_base <= 1; ++islash_base) {
									for (int islash_file = 0; islash_file <= 1; ++islash_file) {
										char *base = make_path (bases [ibase], itrail_base, islash_base, upcase_base, win32);
										char *file = make_path (files [ifile], itrail_file, islash_file, upcase_file, win32);
										verbose && printf ("mono_path_filename_in_basedir (%s, %s)\n", file, base);
										gboolean r = mono_path_filename_in_basedir (file, base);
										verbose && printf ("mono_path_filename_in_basedir (%s, %s, win32:%d):%d\n", file, base, win32, r);
										g_assertf (result [ibase][ifile] == r,
											"mono_path_filename_in_basedir (%s, %s, win32:%d):%d\n", file, base, win32, r);
										if (strcmp (base, file) == 0)
											g_assertf (!r,
												"mono_path_filename_in_basedir (%s, %s, win32:%d):%d\n", file, base, win32, r);
										g_free (base);
										g_free (file);
										++i;
									}
								}
							}
						}
					}
				}
			}
		}
	}
	printf ("%d tests\n", i);

	return 0;
}
