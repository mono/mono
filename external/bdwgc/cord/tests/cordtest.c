/*
 * Copyright (c) 1993-1994 by Xerox Corporation.  All rights reserved.
 *
 * THIS MATERIAL IS PROVIDED AS IS, WITH ABSOLUTELY NO WARRANTY EXPRESSED
 * OR IMPLIED.  ANY USE IS AT YOUR OWN RISK.
 *
 * Permission is hereby granted to use or copy this program
 * for any purpose,  provided the above notices are retained on all copies.
 * Permission to modify the code and to distribute modified code is granted,
 * provided the above notices are retained, and a notice that the code was
 * modified is included with the above copyright notice.
 */

# include "gc.h"    /* For GC_INIT() only */
# include "cord.h"

# include <stdarg.h>
# include <string.h>
# include <stdio.h>
# include <stdlib.h>

/* This is a very incomplete test of the cord package.  It knows about  */
/* a few internals of the package (e.g. when C strings are returned)    */
/* that real clients shouldn't rely on.                                 */

# define ABORT(string) \
    { fprintf(stderr, "FAILED: %s\n", string); abort(); }

#if defined(CPPCHECK)
# undef CORD_iter
# undef CORD_next
# undef CORD_pos_fetch
# undef CORD_pos_to_cord
# undef CORD_pos_to_index
# undef CORD_pos_valid
# undef CORD_prev
#endif

int count;

int test_fn(char c, void * client_data)
{
    if (client_data != (void *)13) ABORT("bad client data");
    if (count < 64*1024+1) {
        if ((count & 1) == 0) {
            if (c != 'b') ABORT("bad char");
        } else {
            if (c != 'a') ABORT("bad char");
        }
        count++;
        return(0);
    } else {
        if (c != 'c') ABORT("bad char");
        count++;
        return(1);
    }
}

char id_cord_fn(size_t i, void * client_data)
{
    if (client_data != 0) ABORT("id_cord_fn: bad client data");
    return((char)i);
}

void test_basics(void)
{
    CORD x = CORD_from_char_star("ab");
    int i;
    CORD y;
    CORD_pos p;

    x = CORD_cat(x,x);
    if (x == CORD_EMPTY) ABORT("CORD_cat(x,x) returned empty cord");
    if (!CORD_IS_STRING(x)) ABORT("short cord should usually be a string");
    if (strcmp(x, "abab") != 0) ABORT("bad CORD_cat result");

    for (i = 1; i < 16; i++) {
        x = CORD_cat(x,x);
    }
    x = CORD_cat(x,"c");
    if (CORD_len(x) != 128*1024+1) ABORT("bad length");

    count = 0;
    if (CORD_iter5(x, 64*1024-1, test_fn, CORD_NO_FN, (void *)13) == 0) {
        ABORT("CORD_iter5 failed");
    }
    if (count != 64*1024 + 2) ABORT("CORD_iter5 failed");

    count = 0;
    CORD_set_pos(p, x, 64*1024-1);
    while(CORD_pos_valid(p)) {
        (void) test_fn(CORD_pos_fetch(p), (void *)13);
    CORD_next(p);
    }
    if (count != 64*1024 + 2) ABORT("Position based iteration failed");

    y = CORD_substr(x, 1023, 5);
    if (!y) ABORT("CORD_substr returned NULL");
    if (!CORD_IS_STRING(y)) ABORT("short cord should usually be a string");
    if (strcmp(y, "babab") != 0) ABORT("bad CORD_substr result");

    y = CORD_substr(x, 1024, 8);
    if (!y) ABORT("CORD_substr returned NULL");
    if (!CORD_IS_STRING(y)) ABORT("short cord should usually be a string");
    if (strcmp(y, "abababab") != 0) ABORT("bad CORD_substr result");

    y = CORD_substr(x, 128*1024-1, 8);
    if (!y) ABORT("CORD_substr returned NULL");
    if (!CORD_IS_STRING(y)) ABORT("short cord should usually be a string");
    if (strcmp(y, "bc") != 0) ABORT("bad CORD_substr result");

    x = CORD_balance(x);
    if (CORD_len(x) != 128*1024+1) ABORT("bad length");

    count = 0;
    if (CORD_iter5(x, 64*1024-1, test_fn, CORD_NO_FN, (void *)13) == 0) {
        ABORT("CORD_iter5 failed");
    }
    if (count != 64*1024 + 2) ABORT("CORD_iter5 failed");

    y = CORD_substr(x, 1023, 5);
    if (!y) ABORT("CORD_substr returned NULL");
    if (!CORD_IS_STRING(y)) ABORT("short cord should usually be a string");
    if (strcmp(y, "babab") != 0) ABORT("bad CORD_substr result");
    y = CORD_from_fn(id_cord_fn, 0, 13);
    i = 0;
    CORD_set_pos(p, y, i);
    while(CORD_pos_valid(p)) {
        char c = CORD_pos_fetch(p);

        if(c != i) ABORT("Traversal of function node failed");
        CORD_next(p);
        i++;
    }
    if (i != 13) ABORT("Bad apparent length for function node");
#   if defined(CPPCHECK)
        /* TODO: Actually test these functions. */
        CORD_prev(p);
        (void)CORD_pos_to_cord(p);
        (void)CORD_pos_to_index(p);
        (void)CORD_iter(CORD_EMPTY, test_fn, NULL);
        (void)CORD_riter(CORD_EMPTY, test_fn, NULL);
        CORD_dump(y);
#   endif
}

void test_extras(void)
{
#   define FNAME1 "cordtst1.tmp" /* short name (8+3) for portability */
#   define FNAME2 "cordtst2.tmp"
    int i;
    CORD y = "abcdefghijklmnopqrstuvwxyz0123456789";
    CORD x = "{}";
    CORD u, w, z;
    FILE *f;
    FILE *f1a, *f1b, *f2;

    w = CORD_cat(CORD_cat(y,y),y);
    z = CORD_catn(3,y,y,y);
    if (CORD_cmp(w,z) != 0) ABORT("CORD_catn comparison wrong");
    for (i = 1; i < 100; i++) {
        x = CORD_cat(x, y);
    }
    z = CORD_balance(x);
    if (CORD_cmp(x,z) != 0) ABORT("balanced string comparison wrong");
    if (CORD_cmp(x,CORD_cat(z, CORD_nul(13))) >= 0) ABORT("comparison 2");
    if (CORD_cmp(CORD_cat(x, CORD_nul(13)), z) <= 0) ABORT("comparison 3");
    if (CORD_cmp(x,CORD_cat(z, "13")) >= 0) ABORT("comparison 4");
    if ((f = fopen(FNAME1, "w")) == 0) ABORT("open failed");
    if (CORD_put(z,f) == EOF) ABORT("CORD_put failed");
    if (fclose(f) == EOF) ABORT("fclose failed");
    f1a = fopen(FNAME1, "rb");
    if (!f1a) ABORT("Unable to open " FNAME1);
    w = CORD_from_file(f1a);
    if (CORD_len(w) != CORD_len(z)) ABORT("file length wrong");
    if (CORD_cmp(w,z) != 0) ABORT("file comparison wrong");
    if (CORD_cmp(CORD_substr(w, 50*36+2, 36), y) != 0)
        ABORT("file substr wrong");
    f1b = fopen(FNAME1, "rb");
    if (!f1b) ABORT("2nd open failed: " FNAME1);
    z = CORD_from_file_lazy(f1b);
    if (CORD_cmp(w,z) != 0) ABORT("File conversions differ");
    if (CORD_chr(w, 0, '9') != 37) ABORT("CORD_chr failed 1");
    if (CORD_chr(w, 3, 'a') != 38) ABORT("CORD_chr failed 2");
    if (CORD_rchr(w, CORD_len(w) - 1, '}') != 1) ABORT("CORD_rchr failed");
    x = y;
    for (i = 1; i < 14; i++) {
        x = CORD_cat(x,x);
    }
    if ((f = fopen(FNAME2, "w")) == 0) ABORT("2nd open failed");
#   ifdef __DJGPP__
      /* FIXME: DJGPP workaround.  Why does this help? */
      if (fflush(f) != 0) ABORT("fflush failed");
#   endif
    if (CORD_put(x,f) == EOF) ABORT("CORD_put failed");
    if (fclose(f) == EOF) ABORT("fclose failed");
    f2 = fopen(FNAME2, "rb");
    if (!f2) ABORT("Unable to open " FNAME2);
    w = CORD_from_file(f2);
    if (CORD_len(w) != CORD_len(x)) ABORT("file length wrong");
    if (CORD_cmp(w,x) != 0) ABORT("file comparison wrong");
    if (CORD_cmp(CORD_substr(w, 1000*36, 36), y) != 0)
        ABORT("file substr wrong");
    if (strcmp(CORD_to_char_star(CORD_substr(w, 1000*36, 36)), y) != 0)
        ABORT("char * file substr wrong");
    u = CORD_substr(w, 1000*36, 2);
    if (!u) ABORT("CORD_substr returned NULL");
    if (strcmp(u, "ab") != 0)
        ABORT("short file substr wrong");
    if (CORD_str(x,1,"9a") != 35) ABORT("CORD_str failed 1");
    if (CORD_str(x,0,"9abcdefghijk") != 35) ABORT("CORD_str failed 2");
    if (CORD_str(x,0,"9abcdefghijx") != CORD_NOT_FOUND)
        ABORT("CORD_str failed 3");
    if (CORD_str(x,0,"9>") != CORD_NOT_FOUND) ABORT("CORD_str failed 4");
    /* Note: f1a, f1b, f2 handles are closed lazily by CORD library.    */
    /* TODO: Propose and use CORD_fclose. */
    *(CORD volatile *)&w = CORD_EMPTY;
    *(CORD volatile *)&z = CORD_EMPTY;
    GC_gcollect();
    GC_invoke_finalizers();
            /* Of course, this does not guarantee the files are closed. */
    if (remove(FNAME1) != 0) {
        /* On some systems, e.g. OS2, this may fail if f1 is still open. */
        /* But we cannot call fclose as it might lead to double close.   */
        fprintf(stderr, "WARNING: remove failed: " FNAME1 "\n");
    }
    if (remove(FNAME2) != 0) {
        fprintf(stderr, "WARNING: remove failed: " FNAME2 "\n");
    }
}

int wrap_vprintf(CORD format, ...)
{
    va_list args;
    int result;

    va_start(args, format);
    result = CORD_vprintf(format, args);
    va_end(args);
    return result;
}

int wrap_vfprintf(FILE * f, CORD format, ...)
{
    va_list args;
    int result;

    va_start(args, format);
    result = CORD_vfprintf(f, format, args);
    va_end(args);
    return result;
}

#if defined(__DJGPP__) || defined(__STRICT_ANSI__)
  /* snprintf is missing in DJGPP (v2.0.3) */
#else
# if defined(_MSC_VER)
#   if defined(_WIN32_WCE)
      /* _snprintf is deprecated in WinCE */
#     define GC_SNPRINTF StringCchPrintfA
#   else
#     define GC_SNPRINTF _snprintf
#   endif
# else
#   define GC_SNPRINTF snprintf
# endif
#endif

void test_printf(void)
{
    CORD result;
    char result2[200];
    long l = -1;
    short s = (short)-1;
    CORD x;

    if (CORD_sprintf(&result, "%7.2f%ln", 3.14159F, &l) != 7)
        ABORT("CORD_sprintf failed 1");
    if (CORD_cmp(result, "   3.14") != 0)ABORT("CORD_sprintf goofed 1");
    if (l != 7) ABORT("CORD_sprintf goofed 2");
    if (CORD_sprintf(&result, "%-7.2s%hn%c%s", "abcd", &s, 'x', "yz") != 10)
        ABORT("CORD_sprintf failed 2");
    if (CORD_cmp(result, "ab     xyz") != 0)ABORT("CORD_sprintf goofed 3");
    if (s != 7) ABORT("CORD_sprintf goofed 4");
    x = "abcdefghij";
    x = CORD_cat(x,x);
    x = CORD_cat(x,x);
    x = CORD_cat(x,x);
    if (CORD_sprintf(&result, "->%-120.78r!\n", x) != 124)
        ABORT("CORD_sprintf failed 3");
#   ifdef GC_SNPRINTF
        (void)GC_SNPRINTF(result2, sizeof(result2), "->%-120.78s!\n",
                          CORD_to_char_star(x));
#   else
        (void)sprintf(result2, "->%-120.78s!\n", CORD_to_char_star(x));
#   endif
    result2[sizeof(result2) - 1] = '\0';
    if (CORD_cmp(result, result2) != 0)ABORT("CORD_sprintf goofed 5");
    /* TODO: Better test CORD_[v][f]printf.     */
    (void)CORD_printf(CORD_EMPTY);
    (void)wrap_vfprintf(stdout, CORD_EMPTY);
    (void)wrap_vprintf(CORD_EMPTY);
}

int main(void)
{
#   ifdef THINK_C
        printf("cordtest:\n");
#   endif
    GC_INIT();
    test_basics();
    test_extras();
    test_printf();
    CORD_fprintf(stdout, "SUCCEEDED\n");
    return(0);
}
