/*
 * A helper routine to copy the strings between differing structures.
 */

#include <stdlib.h>
#include <string.h>
#include <limits.h>

#include "mph.h"

#define MAX_OFFSETS 10

#define OFFSET_SHIFT 1

#define lstr_at(p, n) (*(char**)(((char*)(p))+(n >> OFFSET_SHIFT)))

#define str_at(p, n) (                                          \
		(((n) & MPH_STRING_OFFSET_MASK) == MPH_STRING_OFFSET_ARRAY) \
		? (char*)(p) + (n >> OFFSET_SHIFT)                          \
		: lstr_at(p, n)                                             \
)

char*
_mph_copy_structure_strings (
	void *to,         const mph_string_offset_t *to_offsets, 
	const void *from, const mph_string_offset_t *from_offsets, 
	size_t num_strings)
{
	int i;
	size_t buflen;
	int len[MAX_OFFSETS];
	char *buf, *cur = NULL;

	g_assert (num_strings < MAX_OFFSETS);

	for (i = 0; i < num_strings; ++i) {
		lstr_at (to, to_offsets[i]) = NULL;
	}

	buflen = num_strings;
	for (i = 0; i < num_strings; ++i) {
		const char* s = str_at(from, from_offsets[i]);
		len [i] = s ? strlen (s) : 0;
		if (len[i] < INT_MAX - buflen)
			buflen += len[i];
		else
			len[i] = -1;
	}

	cur = buf = malloc (buflen);
	if (buf == NULL) {
		return NULL;
	}

	for (i = 0; i < num_strings; ++i) {
		if (len[i] > 0) {
			lstr_at (to, to_offsets[i]) = 
				strcpy (cur, str_at (from, from_offsets[i]));
			cur += (len[i] +1);
		}
	}

	return buf;
}

#ifdef TEST

/*
 * To run the tests:
 * $ gcc -DTEST -I.. `pkg-config --cflags --libs glib-2.0` x-struct-str.c
 * $ ./a.out
 */

#include <stdio.h>

struct foo {
	char *a;
	int   b;
	char *c;
	char d[10];
};

struct bar {
	int    b;
	char  *a;
	double d;
	char  *c;
	char  *e;
};

int
main ()
{
	/* test copying foo to bar */
	struct foo f = {"hello", 42, "world", "!!"};
	struct bar b;
	mph_string_offset_t foo_offsets[] = {
		MPH_STRING_OFFSET(struct foo, a, MPH_STRING_OFFSET_PTR),
		MPH_STRING_OFFSET(struct foo, c, MPH_STRING_OFFSET_PTR),
		MPH_STRING_OFFSET(struct foo, d, MPH_STRING_OFFSET_ARRAY)
	};
	mph_string_offset_t bar_offsets[] = {
		MPH_STRING_OFFSET(struct bar, a, MPH_STRING_OFFSET_PTR), 
		MPH_STRING_OFFSET(struct bar, c, MPH_STRING_OFFSET_PTR), 
		MPH_STRING_OFFSET(struct bar, e, MPH_STRING_OFFSET_PTR)
	};
	char *buf;

	buf = _mph_copy_structure_strings (&b, bar_offsets, 
			&f, foo_offsets, 3);
	printf ("b.a=%s\n", b.a);
	printf ("b.c=%s\n", b.c);
	printf ("b.e=%s\n", b.e);

	f.c = NULL;
	buf = _mph_copy_structure_strings (&b, bar_offsets, 
			&f, foo_offsets, 3);
	printf ("b.a=%s\n", b.a);
	printf ("b.c=%s\n", b.c);
	printf ("b.e=%s\n", b.e);

	return 0;
}
#endif

