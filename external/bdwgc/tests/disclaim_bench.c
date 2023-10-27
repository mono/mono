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

#include <stdlib.h>
#include <stdio.h>
#include <string.h>

#ifdef HAVE_CONFIG_H
# include "config.h"
#endif

#include "gc_disclaim.h"

/* Include gc_priv.h is done after including GC public headers, so      */
/* that GC_BUILD has no effect on the public prototypes.                */
#include "private/gc_priv.h" /* for CLOCK_TYPE, COVERT_DATAFLOW, GC_random */

#ifdef LINT2
# undef rand
# define rand() (int)GC_random()
#endif

#define my_assert(e) \
    if (!(e)) { \
        fprintf(stderr, "Assertion failure, line %d: " #e "\n", __LINE__); \
        exit(-1); \
    }

static int free_count = 0;

struct testobj_s {
    struct testobj_s *keep_link;
    int i;
};

typedef struct testobj_s *testobj_t;

void GC_CALLBACK testobj_finalize(void *obj, void *carg)
{
    ++*(int *)carg;
    my_assert(((testobj_t)obj)->i == 109);
    ((testobj_t)obj)->i = 110;
}

static const struct GC_finalizer_closure fclos = {
    testobj_finalize,
    &free_count
};

testobj_t testobj_new(int model)
{
    testobj_t obj;
    switch (model) {
        case 0:
            obj = GC_NEW(struct testobj_s);
            if (obj != NULL)
              GC_REGISTER_FINALIZER_NO_ORDER(obj, testobj_finalize,
                                             &free_count, NULL, NULL);
            break;
        case 1:
            obj = (testobj_t)GC_finalized_malloc(sizeof(struct testobj_s),
                                                 &fclos);
            break;
        case 2:
            obj = GC_NEW(struct testobj_s);
            break;
        default:
            exit(-1);
    }
    if (obj == NULL) {
        fprintf(stderr, "Out of memory!\n");
        exit(3);
    }
    my_assert(obj->i == 0 && obj->keep_link == NULL);
    obj->i = 109;
    return obj;
}

#define ALLOC_CNT (4*1024*1024)
#define KEEP_CNT      (32*1024)

static char const *model_str[3] = {
   "regular finalization",
   "finalize on reclaim",
   "no finalization"
};

int main(int argc, char **argv)
{
    int i;
    int model, model_min, model_max;
    testobj_t *keep_arr;

    GC_INIT();
    GC_init_finalized_malloc();
    if (argc == 2 && strcmp(argv[1], "--help") == 0) {
        fprintf(stderr,
                "Usage: %s [FINALIZATION_MODEL]\n"
                "\t0 -- original finalization\n"
                "\t1 -- finalization on reclaim\n"
                "\t2 -- no finalization\n", argv[0]);
        return 1;
    }
    if (argc == 2) {
        model_min = model_max = (int)COVERT_DATAFLOW(atoi(argv[1]));
        if (model_min < 0 || model_max > 2)
            exit(2);
    }
    else {
        model_min = 0;
        model_max = 2;
    }

    keep_arr = (testobj_t *)GC_MALLOC(sizeof(void *) * KEEP_CNT);
    if (NULL == keep_arr) {
        fprintf(stderr, "Out of memory!\n");
        exit(3);
    }

    printf("\t\t\tfin. ratio       time/s    time/fin.\n");
    for (model = model_min; model <= model_max; ++model) {
        double t = 0.0;
#       ifndef NO_CLOCK
            CLOCK_TYPE tI, tF;

            GET_TIME(tI);
#       endif
        free_count = 0;
        for (i = 0; i < ALLOC_CNT; ++i) {
            int k = rand() % KEEP_CNT;
            keep_arr[k] = testobj_new(model);
        }
        GC_gcollect();
#       ifndef NO_CLOCK
            GET_TIME(tF);
            t = MS_TIME_DIFF(tF, tI)*1e-3;
#       endif

        if (model < 2 && free_count > 0)
            printf("%20s: %12.4f %12g %12g\n", model_str[model],
                   free_count/(double)ALLOC_CNT, t, t/free_count);
        else
            printf("%20s: %12.4f %12g %12s\n",
                   model_str[model], 0.0, t, "N/A");
    }
    return 0;
}
