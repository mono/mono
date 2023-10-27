
/* This test file is intended to be compiled into a DLL. */

#ifndef GC_DEBUG
# define GC_DEBUG
#endif

#include "gc.h"

#ifndef GC_TEST_EXPORT_API
# if defined(GC_VISIBILITY_HIDDEN_SET) \
     && !defined(__CEGCC__) && !defined(__CYGWIN__) && !defined(__MINGW32__)
#   define GC_TEST_EXPORT_API \
                        extern __attribute__((__visibility__("default")))
# else
#   define GC_TEST_EXPORT_API extern
# endif
#endif

struct treenode {
    struct treenode *x;
    struct treenode *y;
};

static struct treenode *root[10] = { 0 };
static struct treenode *root_nz[10] = { (struct treenode *)(GC_word)2 };

#ifdef STATICROOTSLIB2
# define libsrl_getpelem libsrl_getpelem2
#else

  GC_TEST_EXPORT_API struct treenode * libsrl_mktree(int i)
  {
    struct treenode * r = GC_NEW(struct treenode);
    if (0 == i)
      return 0;
    if (1 == i)
      r = (struct treenode *)GC_MALLOC_ATOMIC(sizeof(struct treenode));
    if (r) {
      r -> x = libsrl_mktree(i-1);
      r -> y = libsrl_mktree(i-1);
    }
    return r;
  }

  GC_TEST_EXPORT_API void * libsrl_init(void)
  {
#   ifndef STATICROOTSLIB_INIT_IN_MAIN
      GC_INIT();
#   endif
    return GC_MALLOC(sizeof(struct treenode));
  }

#endif /* !STATICROOTSLIB2 */

GC_TEST_EXPORT_API struct treenode ** libsrl_getpelem(int i, int j)
{
  return &((j & 1) != 0 ? root_nz : root)[i];
}
