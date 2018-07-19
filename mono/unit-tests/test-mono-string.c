/*
 * test-mono-string.c: Unit test for runtime MonoString* manipulation.
 */

#include "config.h"
#include <stdio.h>
#include "metadata/object-internals.h"
#include "mini/jit.h"

static int
new_string_ok (void)
{
	ERROR_DECL (error);
	MonoString *s = mono_string_new_checked (mono_domain_get (), "abcd", error);
	static const gunichar2 u16s[] = { 0x61, 0x62, 0x63, 0x64, 0 }; /* u16 "abcd" */
	mono_error_assert_ok (error);
	gunichar2* c = mono_string_chars (s);

	g_assert (c != NULL && !memcmp (&u16s, c, sizeof (u16s)));
	return 0;
}

static int
new_string_utf8 (void)
{
	ERROR_DECL (error);
	const gunichar2 snowman = 0x2603;
	static const char bytes[] = { 0xE2, 0x98, 0x83, 0x00 }; /* U+2603 NUL */
	MonoString *s = mono_string_new_checked (mono_domain_get (), bytes, error);
	mono_error_assert_ok (error);
	gunichar2* c = mono_string_chars (s);
	g_assert (c != NULL &&
		  (c[0] == snowman) &&
		  (c[1] == 0));
	return 0;
}

static int
new_string_conv_err (void)
{
	ERROR_DECL (error);
	static const char bytes[] = { 'a', 0xFC, 'b', 'c', 0 };
	MonoString G_GNUC_UNUSED *s = mono_string_new_checked (mono_domain_get (), bytes, error);
	g_assert (!mono_error_ok (error));
	const char *msg = mono_error_get_message (error);
	g_assert (msg != NULL);
	fprintf (stderr, "(expected) error message was: \"%s\"", msg);
	mono_error_cleanup (error);
	return 0;
}

int
main (void)
{

	mono_jit_init_version ("test-mono-string", "v4.0.30319");

	int res = 0;

	res += new_string_ok ();
	res += new_string_utf8 ();
	res += new_string_conv_err ();

	return res;
}
