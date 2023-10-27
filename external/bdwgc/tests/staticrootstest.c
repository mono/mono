
#include <stdio.h>
#include <string.h>

#ifndef GC_DEBUG
# define GC_DEBUG
#endif

#include "gc.h"
#include "gc_backptr.h"

#ifndef GC_TEST_IMPORT_API
# define GC_TEST_IMPORT_API extern
#endif

/* Should match that in staticrootslib.c.       */
struct treenode {
    struct treenode *x;
    struct treenode *y;
};

struct treenode *root[10] = { NULL };

/* Same as "root" variable but initialized to some non-zero value (to   */
/* be placed to .data section instead of .bss).                         */
struct treenode *root_nz[10] = { (struct treenode *)(GC_word)1 };

static char *staticroot; /* intentionally static */

GC_TEST_IMPORT_API struct treenode * libsrl_mktree(int i);
GC_TEST_IMPORT_API void * libsrl_init(void);
GC_TEST_IMPORT_API struct treenode ** libsrl_getpelem(int i, int j);

GC_TEST_IMPORT_API struct treenode ** libsrl_getpelem2(int i, int j);

void init_staticroot(void)
{
  /* Intentionally put staticroot initialization in a function other    */
  /* than main to prevent CSA warning that staticroot variable can be   */
  /* changed to be a local one).                                        */
  staticroot = (char *)libsrl_init();
}

int main(void)
{
  int i, j;

# ifdef STATICROOTSLIB_INIT_IN_MAIN
    GC_INIT();
# endif
  init_staticroot();
  if (NULL == staticroot) {
    fprintf(stderr, "GC_malloc returned NULL\n");
    return 2;
  }
  memset(staticroot, 0x42, sizeof(struct treenode));
  GC_gcollect();
  for (j = 0; j < 4; j++) {
      for (i = 0; i < (int)(sizeof(root) / sizeof(root[0])); ++i) {
#       ifdef STATICROOTSLIB2
          *libsrl_getpelem2(i, j) = libsrl_mktree(12);
#       endif
        *libsrl_getpelem(i, j) = libsrl_mktree(12);
        ((j & 1) != 0 ? root_nz : root)[i] = libsrl_mktree(12);
        GC_gcollect();
      }
      for (i = 0; i < (int)sizeof(struct treenode); ++i) {
        if (staticroot[i] != 0x42) {
          fprintf(stderr, "Memory check failed\n");
          return -1;
        }
      }
  }
  return 0;
}
