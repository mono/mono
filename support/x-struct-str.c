/*
 * A helper routine to copy the strings between differing structures.
 */

#include <stdlib.h>
#include <string.h>
#include <limits.h>

#include "mph.h"

#define MAX_OFFSETS 10

#define str_at(p, n) (*(char**)(((char*)p)+n))

char* MPH_INTERNAL
_mph_copy_structure_strings (
	void *to,   size_t *to_offsets, 
	void *from, size_t *from_offsets, 
	size_t num_strings)
{
	int i;
	size_t buflen;
	int len[MAX_OFFSETS];
	char *buf, *cur = NULL;

	g_assert (num_strings < MAX_OFFSETS);

	for (i = 0; i < num_strings; ++i) {
		str_at (to, to_offsets[i]) = NULL;
	}

	buflen = num_strings;
	for (i = 0; i < num_strings; ++i) {
		len[i] = strlen (str_at(from, from_offsets[i]));
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
			str_at (to, to_offsets[i]) = 
				strcpy (cur, str_at (from, from_offsets[i]));
			cur += (len[i] +1);
		}
	}

	return buf;
}

#ifdef TEST

#include <stdio.h>

struct foo {
	char *a;
	int   b;
	char *c;
};

struct bar {
	int    b;
	char  *a;
	double d;
	char  *c;
};

int
main ()
{
	/* test copying foo to bar */
	struct foo f = {"hello", 42, "world"};
	struct bar b;
	size_t foo_offsets[] = {offsetof(struct foo, a), offsetof(struct foo, c)};
	size_t bar_offsets[] = {offsetof(struct bar, a), offsetof(struct bar, c)};
	char *buf;

	buf = _mph_copy_structure_strings (&b, bar_offsets, 
			&f, foo_offsets, 2);
	printf ("b.a=%s\n", b.a);
	printf ("b.c=%s\n", b.c);

	return 0;
}
#endif

