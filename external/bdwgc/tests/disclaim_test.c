/*
 * Copyright (c) 2011 by Hewlett-Packard Company.  All rights reserved.
 *
 * THIS MATERIAL IS PROVIDED AS IS, WITH ABSOLUTELY NO WARRANTY EXPRESSED
 * OR IMPLIED.  ANY USE IS AT YOUR OWN RISK.
 *
 * Permission is hereby granted to use or copy this program
 * for any purpose,  provided the above notices are retained on all copies.
 * Permission to modify the code and to distribute modified code is granted,
 * provided the above notices are retained, and a notice that the code was
 * modified is included with the above copyright notice.
 *
 */

/* Test that objects reachable from an object allocated with            */
/* GC_malloc_with_finalizer is not reclaimable before the finalizer     */
/* is called.                                                           */

#include <stdlib.h>
#include <stdio.h>
#include <string.h>

#ifdef HAVE_CONFIG_H
  /* For GC_[P]THREADS */
# include "config.h"
#endif

#undef GC_NO_THREAD_REDIRECTS
#include "gc_disclaim.h"

#ifdef LINT2
  /* Avoid include gc_priv.h. */
# ifndef GC_API_PRIV
#   define GC_API_PRIV GC_API
# endif
# ifdef __cplusplus
    extern "C" {
# endif
  GC_API_PRIV long GC_random(void);
# ifdef __cplusplus
    } /* extern "C" */
# endif
# undef rand
# define rand() (int)GC_random()
#endif /* LINT2 */

#define my_assert(e) \
    if (!(e)) { \
        fflush(stdout); \
        fprintf(stderr, "Assertion failure, line %d: " #e "\n", __LINE__); \
        exit(-1); \
    }

int memeq(void *s, int c, size_t len)
{
    while (len--) {
        if (*(char *)s != c)
            return 0;
        s = (char *)s + 1;
    }
    return 1;
}

void GC_CALLBACK misc_sizes_dct(void *obj, void *cd)
{
    unsigned log_size = *(unsigned char *)obj;
    size_t size;

    my_assert(log_size < sizeof(size_t) * 8);
    my_assert(cd == NULL);
    size = (size_t)1 << log_size;
    my_assert(memeq((char *)obj + 1, 0x56, size - 1));
}

void test_misc_sizes(void)
{
    static const struct GC_finalizer_closure fc = { misc_sizes_dct, NULL };
    int i;
    for (i = 1; i <= 20; ++i) { /* Up to 1 MiB. */
        void *p = GC_finalized_malloc((size_t)1 << i, &fc);
        if (p == NULL) {
            fprintf(stderr, "Out of memory!\n");
            exit(3);
        }
        my_assert(memeq(p, 0, (size_t)1 << i));
        memset(p, 0x56, (size_t)1 << i);
        *(unsigned char *)p = i;
    }
}

typedef struct pair_s *pair_t;

struct pair_s {
    char magic[16];
    int checksum;
    pair_t car;
    pair_t cdr;
};

static const char * const pair_magic = "PAIR_MAGIC_BYTES";

int is_pair(pair_t p)
{
    return memcmp(p->magic, pair_magic, sizeof(p->magic)) == 0;
}

void GC_CALLBACK pair_dct(void *obj, void *cd)
{
    pair_t p = (pair_t)obj;
    int checksum;

    /* Check that obj and its car and cdr are not trashed. */
#   ifdef DEBUG_DISCLAIM_DESTRUCT
      printf("Destruct %p = (%p, %p)\n",
             (void *)p, (void *)p->car, (void *)p->cdr);
#   endif
    my_assert(GC_base(obj));
    my_assert(is_pair(p));
    my_assert(!p->car || is_pair(p->car));
    my_assert(!p->cdr || is_pair(p->cdr));
    checksum = 782;
    if (p->car) checksum += p->car->checksum;
    if (p->cdr) checksum += p->cdr->checksum;
    my_assert(p->checksum == checksum);

    /* Invalidate it. */
    memset(p->magic, '*', sizeof(p->magic));
    p->checksum = 0;
    p->car = (pair_t)cd;
    p->cdr = NULL;
    GC_end_stubborn_change(p);
}

pair_t
pair_new(pair_t car, pair_t cdr)
{
    pair_t p;
    static const struct GC_finalizer_closure fc = { pair_dct, NULL };

    p = (pair_t)GC_finalized_malloc(sizeof(struct pair_s), &fc);
    if (p == NULL) {
        fprintf(stderr, "Out of memory!\n");
        exit(3);
    }
    my_assert(!is_pair(p));
    my_assert(memeq(p, 0, sizeof(struct pair_s)));
    memcpy(p->magic, pair_magic, sizeof(p->magic));
    p->checksum = 782 + (car? car->checksum : 0) + (cdr? cdr->checksum : 0);
    p->car = car;
    p->cdr = cdr;
    GC_end_stubborn_change(p);
#   ifdef DEBUG_DISCLAIM_DESTRUCT
      printf("Construct %p = (%p, %p)\n",
             (void *)p, (void *)p->car, (void *)p->cdr);
#   endif
    return p;
}

void
pair_check_rec(pair_t p)
{
    while (p) {
        int checksum = 782;
        if (p->car) checksum += p->car->checksum;
        if (p->cdr) checksum += p->cdr->checksum;
        my_assert(p->checksum == checksum);
        if (rand() % 2)
            p = p->car;
        else
            p = p->cdr;
    }
}

#ifdef GC_PTHREADS
# ifndef NTHREADS
#   define NTHREADS 6
# endif
# include <pthread.h>
#else
# undef NTHREADS
# define NTHREADS 1
#endif

#define POP_SIZE 1000
#if NTHREADS > 1
# define MUTATE_CNT (2000000/NTHREADS)
#else
# define MUTATE_CNT 10000000
#endif
#define GROW_LIMIT (MUTATE_CNT/10)

void *test(void *data)
{
    int i;
    pair_t pop[POP_SIZE];
    memset(pop, 0, sizeof(pop));
    for (i = 0; i < MUTATE_CNT; ++i) {
        int t = rand() % POP_SIZE;
        switch (rand() % (i > GROW_LIMIT? 5 : 3)) {
        case 0: case 3:
            if (pop[t])
                pop[t] = pop[t]->car;
            break;
        case 1: case 4:
            if (pop[t])
                pop[t] = pop[t]->cdr;
            break;
        case 2:
            pop[t] = pair_new(pop[rand() % POP_SIZE],
                              pop[rand() % POP_SIZE]);
            break;
        }
        if (rand() % 8 == 1)
            pair_check_rec(pop[rand() % POP_SIZE]);
    }
    return data;
}

int main(void)
{
# if NTHREADS > 1
    pthread_t th[NTHREADS];
    int i;
# endif

    GC_set_all_interior_pointers(0); /* for a stricter test */
    GC_INIT();
    GC_init_finalized_malloc();
#   ifndef NO_INCREMENTAL
        GC_enable_incremental();
#   endif

    test_misc_sizes();

# if NTHREADS > 1
    printf("Threaded disclaim test.\n");
    for (i = 0; i < NTHREADS; ++i) {
        int err = pthread_create(&th[i], NULL, test, NULL);
        if (err) {
            fprintf(stderr, "Failed to create thread # %d: %s\n", i,
                    strerror(err));
            exit(1);
        }
    }
    for (i = 0; i < NTHREADS; ++i) {
        int err = pthread_join(th[i], NULL);
        if (err) {
            fprintf(stderr, "Failed to join thread # %d: %s\n", i,
                    strerror(err));
            exit(69);
        }
    }
# else
    printf("Unthreaded disclaim test.\n");
    test(NULL);
# endif
    return 0;
}
