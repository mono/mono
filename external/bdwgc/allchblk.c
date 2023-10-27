/*
 * Copyright 1988, 1989 Hans-J. Boehm, Alan J. Demers
 * Copyright (c) 1991-1994 by Xerox Corporation.  All rights reserved.
 * Copyright (c) 1998-1999 by Silicon Graphics.  All rights reserved.
 * Copyright (c) 1999 by Hewlett-Packard Company. All rights reserved.
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

#include "private/gc_priv.h"

#include <stdio.h>

#ifdef GC_USE_ENTIRE_HEAP
  int GC_use_entire_heap = TRUE;
#else
  int GC_use_entire_heap = FALSE;
#endif

/*
 * Free heap blocks are kept on one of several free lists,
 * depending on the size of the block.  Each free list is doubly linked.
 * Adjacent free blocks are coalesced.
 */


# define MAX_BLACK_LIST_ALLOC (2*HBLKSIZE)
                /* largest block we will allocate starting on a black   */
                /* listed block.  Must be >= HBLKSIZE.                  */


# define UNIQUE_THRESHOLD 32
        /* Sizes up to this many HBLKs each have their own free list    */
# define HUGE_THRESHOLD 256
        /* Sizes of at least this many heap blocks are mapped to a      */
        /* single free list.                                            */
# define FL_COMPRESSION 8
        /* In between sizes map this many distinct sizes to a single    */
        /* bin.                                                         */

# define N_HBLK_FLS ((HUGE_THRESHOLD - UNIQUE_THRESHOLD) / FL_COMPRESSION \
                     + UNIQUE_THRESHOLD)

#ifndef GC_GCJ_SUPPORT
  STATIC
#endif
  struct hblk * GC_hblkfreelist[N_HBLK_FLS+1] = { 0 };
                                /* List of completely empty heap blocks */
                                /* Linked through hb_next field of      */
                                /* header structure associated with     */
                                /* block.  Remains externally visible   */
                                /* as used by GNU GCJ currently.        */

#ifndef GC_GCJ_SUPPORT
  STATIC
#endif
  word GC_free_bytes[N_HBLK_FLS+1] = { 0 };
        /* Number of free bytes on each list.  Remains visible to GCJ.  */

void GC_clear_freelist(void)
{
    memset(GC_hblkfreelist, 0, sizeof(GC_hblkfreelist));
    memset(GC_free_bytes, 0, sizeof(GC_free_bytes));
}

/* Return the largest n such that the number of free bytes on lists     */
/* n .. N_HBLK_FLS is greater or equal to GC_max_large_allocd_bytes     */
/* minus GC_large_allocd_bytes.  If there is no such n, return 0.       */
GC_INLINE int GC_enough_large_bytes_left(void)
{
    int n;
    word bytes = GC_large_allocd_bytes;

    GC_ASSERT(GC_max_large_allocd_bytes <= GC_heapsize);
    for (n = N_HBLK_FLS; n >= 0; --n) {
        bytes += GC_free_bytes[n];
        if (bytes >= GC_max_large_allocd_bytes) return n;
    }
    return 0;
}

/* Map a number of blocks to the appropriate large block free list index. */
STATIC int GC_hblk_fl_from_blocks(word blocks_needed)
{
    if (blocks_needed <= UNIQUE_THRESHOLD) return (int)blocks_needed;
    if (blocks_needed >= HUGE_THRESHOLD) return N_HBLK_FLS;
    return (int)(blocks_needed - UNIQUE_THRESHOLD)/FL_COMPRESSION
                                        + UNIQUE_THRESHOLD;

}

# define PHDR(hhdr) HDR((hhdr) -> hb_prev)
# define NHDR(hhdr) HDR((hhdr) -> hb_next)

# ifdef USE_MUNMAP
#   define IS_MAPPED(hhdr) (((hhdr) -> hb_flags & WAS_UNMAPPED) == 0)
# else
#   define IS_MAPPED(hhdr) TRUE
# endif /* !USE_MUNMAP */

#if !defined(NO_DEBUGGING) || defined(GC_ASSERTIONS)
  /* Should return the same value as GC_large_free_bytes.       */
  GC_INNER word GC_compute_large_free_bytes(void)
  {
      word total_free = 0;
      unsigned i;

      for (i = 0; i <= N_HBLK_FLS; ++i) {
        struct hblk * h;
        hdr * hhdr;

        for (h = GC_hblkfreelist[i]; h != 0; h = hhdr->hb_next) {
          hhdr = HDR(h);
          total_free += hhdr->hb_sz;
        }
      }
      return total_free;
  }
#endif /* !NO_DEBUGGING || GC_ASSERTIONS */

# if !defined(NO_DEBUGGING)
void GC_print_hblkfreelist(void)
{
    unsigned i;
    word total;

    for (i = 0; i <= N_HBLK_FLS; ++i) {
      struct hblk * h = GC_hblkfreelist[i];

      if (0 != h) GC_printf("Free list %u (total size %lu):\n",
                            i, (unsigned long)GC_free_bytes[i]);
      while (h != 0) {
        hdr * hhdr = HDR(h);

        GC_printf("\t%p size %lu %s black listed\n",
                (void *)h, (unsigned long) hhdr -> hb_sz,
                GC_is_black_listed(h, HBLKSIZE) != 0 ? "start" :
                GC_is_black_listed(h, hhdr -> hb_sz) != 0 ? "partially" :
                                                        "not");
        h = hhdr -> hb_next;
      }
    }
    GC_printf("GC_large_free_bytes: %lu\n",
              (unsigned long)GC_large_free_bytes);

    if ((total = GC_compute_large_free_bytes()) != GC_large_free_bytes)
          GC_err_printf("GC_large_free_bytes INCONSISTENT!! Should be: %lu\n",
                        (unsigned long)total);
}

/* Return the free list index on which the block described by the header */
/* appears, or -1 if it appears nowhere.                                 */
static int free_list_index_of(hdr *wanted)
{
    int i;

    for (i = 0; i <= N_HBLK_FLS; ++i) {
      struct hblk * h;
      hdr * hhdr;

      for (h = GC_hblkfreelist[i]; h != 0; h = hhdr -> hb_next) {
        hhdr = HDR(h);
        if (hhdr == wanted) return i;
      }
    }
    return -1;
}

GC_API void GC_CALL GC_dump_regions(void)
{
    unsigned i;

    for (i = 0; i < GC_n_heap_sects; ++i) {
        ptr_t start = GC_heap_sects[i].hs_start;
        size_t bytes = GC_heap_sects[i].hs_bytes;
        ptr_t end = start + bytes;
        ptr_t p;

        /* Merge in contiguous sections.        */
          while (i+1 < GC_n_heap_sects && GC_heap_sects[i+1].hs_start == end) {
            ++i;
            end = GC_heap_sects[i].hs_start + GC_heap_sects[i].hs_bytes;
          }
        GC_printf("***Section from %p to %p\n", (void *)start, (void *)end);
        for (p = start; (word)p < (word)end; ) {
            hdr *hhdr = HDR(p);

            if (IS_FORWARDING_ADDR_OR_NIL(hhdr)) {
                GC_printf("\t%p Missing header!!(%p)\n",
                          (void *)p, (void *)hhdr);
                p += HBLKSIZE;
                continue;
            }
            if (HBLK_IS_FREE(hhdr)) {
                int correct_index = GC_hblk_fl_from_blocks(
                                        divHBLKSZ(hhdr -> hb_sz));
                int actual_index;

                GC_printf("\t%p\tfree block of size 0x%lx bytes%s\n",
                          (void *)p, (unsigned long)(hhdr -> hb_sz),
                          IS_MAPPED(hhdr) ? "" : " (unmapped)");
                actual_index = free_list_index_of(hhdr);
                if (-1 == actual_index) {
                    GC_printf("\t\tBlock not on free list %d!!\n",
                              correct_index);
                } else if (correct_index != actual_index) {
                    GC_printf("\t\tBlock on list %d, should be on %d!!\n",
                              actual_index, correct_index);
                }
                p += hhdr -> hb_sz;
            } else {
                GC_printf("\t%p\tused for blocks of size 0x%lx bytes\n",
                          (void *)p, (unsigned long)(hhdr -> hb_sz));
                p += HBLKSIZE * OBJ_SZ_TO_BLOCKS(hhdr -> hb_sz);
            }
        }
    }
}

# endif /* NO_DEBUGGING */

/* Initialize hdr for a block containing the indicated size and         */
/* kind of objects.                                                     */
/* Return FALSE on failure.                                             */
static GC_bool setup_header(hdr * hhdr, struct hblk *block, size_t byte_sz,
                            int kind, unsigned flags)
{
    word descr;

#   ifdef MARK_BIT_PER_GRANULE
      if (byte_sz > MAXOBJBYTES)
        flags |= LARGE_BLOCK;
#   endif
#   ifdef ENABLE_DISCLAIM
      if (GC_obj_kinds[kind].ok_disclaim_proc)
        flags |= HAS_DISCLAIM;
      if (GC_obj_kinds[kind].ok_mark_unconditionally)
        flags |= MARK_UNCONDITIONALLY;
#   endif

    /* Set size, kind and mark proc fields */
      hhdr -> hb_sz = byte_sz;
      hhdr -> hb_obj_kind = (unsigned char)kind;
      hhdr -> hb_flags = (unsigned char)flags;
      hhdr -> hb_block = block;
      descr = GC_obj_kinds[kind].ok_descriptor;
      if (GC_obj_kinds[kind].ok_relocate_descr) descr += byte_sz;
      hhdr -> hb_descr = descr;

#   ifdef MARK_BIT_PER_OBJ
     /* Set hb_inv_sz as portably as possible.                          */
     /* We set it to the smallest value such that sz * inv_sz > 2**32   */
     /* This may be more precision than necessary.                      */
      if (byte_sz > MAXOBJBYTES) {
         hhdr -> hb_inv_sz = LARGE_INV_SZ;
      } else {
        word inv_sz;

#       if CPP_WORDSZ == 64
          inv_sz = ((word)1 << 32)/byte_sz;
          if (((inv_sz*byte_sz) >> 32) == 0) ++inv_sz;
#       else  /* 32 bit words */
          GC_ASSERT(byte_sz >= 4);
          inv_sz = ((unsigned)1 << 31)/byte_sz;
          inv_sz *= 2;
          while (inv_sz*byte_sz > byte_sz) ++inv_sz;
#       endif
        hhdr -> hb_inv_sz = inv_sz;
      }
#   endif
#   ifdef MARK_BIT_PER_GRANULE
    {
      size_t granules = BYTES_TO_GRANULES(byte_sz);

      if (EXPECT(!GC_add_map_entry(granules), FALSE)) {
        /* Make it look like a valid block. */
        hhdr -> hb_sz = HBLKSIZE;
        hhdr -> hb_descr = 0;
        hhdr -> hb_flags |= LARGE_BLOCK;
        hhdr -> hb_map = 0;
        return FALSE;
      }
      hhdr -> hb_map = GC_obj_map[(hhdr -> hb_flags & LARGE_BLOCK) != 0 ?
                                    0 : granules];
    }
#   endif /* MARK_BIT_PER_GRANULE */

    /* Clear mark bits */
    GC_clear_hdr_marks(hhdr);

    hhdr -> hb_last_reclaimed = (unsigned short)GC_gc_no;
    return(TRUE);
}

/* Remove hhdr from the free list (it is assumed to specified by index). */
STATIC void GC_remove_from_fl_at(hdr *hhdr, int index)
{
    GC_ASSERT(((hhdr -> hb_sz) & (HBLKSIZE-1)) == 0);
    if (hhdr -> hb_prev == 0) {
        GC_ASSERT(HDR(GC_hblkfreelist[index]) == hhdr);
        GC_hblkfreelist[index] = hhdr -> hb_next;
    } else {
        hdr *phdr;
        GET_HDR(hhdr -> hb_prev, phdr);
        phdr -> hb_next = hhdr -> hb_next;
    }
    /* We always need index to maintain free counts.    */
    GC_ASSERT(GC_free_bytes[index] >= hhdr -> hb_sz);
    GC_free_bytes[index] -= hhdr -> hb_sz;
    if (0 != hhdr -> hb_next) {
        hdr * nhdr;
        GC_ASSERT(!IS_FORWARDING_ADDR_OR_NIL(NHDR(hhdr)));
        GET_HDR(hhdr -> hb_next, nhdr);
        nhdr -> hb_prev = hhdr -> hb_prev;
    }
}

/* Remove hhdr from the appropriate free list (we assume it is on the   */
/* size-appropriate free list).                                         */
GC_INLINE void GC_remove_from_fl(hdr *hhdr)
{
  GC_remove_from_fl_at(hhdr, GC_hblk_fl_from_blocks(divHBLKSZ(hhdr->hb_sz)));
}

/* Return a pointer to the free block ending just before h, if any.     */
STATIC struct hblk * GC_free_block_ending_at(struct hblk *h)
{
    struct hblk * p = h - 1;
    hdr * phdr;

    GET_HDR(p, phdr);
    while (0 != phdr && IS_FORWARDING_ADDR_OR_NIL(phdr)) {
        p = FORWARDED_ADDR(p,phdr);
        phdr = HDR(p);
    }
    if (0 != phdr) {
        if(HBLK_IS_FREE(phdr)) {
            return p;
        } else {
            return 0;
        }
    }
    p = GC_prev_block(h - 1);
    if (0 != p) {
      phdr = HDR(p);
      if (HBLK_IS_FREE(phdr) && (ptr_t)p + phdr -> hb_sz == (ptr_t)h) {
        return p;
      }
    }
    return 0;
}

/* Add hhdr to the appropriate free list.               */
/* We maintain individual free lists sorted by address. */
STATIC void GC_add_to_fl(struct hblk *h, hdr *hhdr)
{
    int index = GC_hblk_fl_from_blocks(divHBLKSZ(hhdr -> hb_sz));
    struct hblk *second = GC_hblkfreelist[index];
#   if defined(GC_ASSERTIONS) && !defined(USE_MUNMAP)
      struct hblk *next = (struct hblk *)((word)h + hhdr -> hb_sz);
      hdr * nexthdr = HDR(next);
      struct hblk *prev = GC_free_block_ending_at(h);
      hdr * prevhdr = HDR(prev);

      GC_ASSERT(nexthdr == 0 || !HBLK_IS_FREE(nexthdr)
                || (GC_heapsize & SIGNB) != 0);
                /* In the last case, blocks may be too large to merge. */
      GC_ASSERT(prev == 0 || !HBLK_IS_FREE(prevhdr)
                || (GC_heapsize & SIGNB) != 0);
#   endif
    GC_ASSERT(((hhdr -> hb_sz) & (HBLKSIZE-1)) == 0);
    GC_hblkfreelist[index] = h;
    GC_free_bytes[index] += hhdr -> hb_sz;
    GC_ASSERT(GC_free_bytes[index] <= GC_large_free_bytes);
    hhdr -> hb_next = second;
    hhdr -> hb_prev = 0;
    if (0 != second) {
      hdr * second_hdr;

      GET_HDR(second, second_hdr);
      second_hdr -> hb_prev = h;
    }
    hhdr -> hb_flags |= FREE_BLK;
}

#ifdef USE_MUNMAP

#   ifndef MUNMAP_THRESHOLD
#     define MUNMAP_THRESHOLD 6
#   endif

GC_INNER int GC_unmap_threshold = MUNMAP_THRESHOLD;

/* Unmap blocks that haven't been recently touched.  This is the only way */
/* way blocks are ever unmapped.                                          */
GC_INNER void GC_unmap_old(void)
{
    word sz;
    unsigned short last_rec, threshold;
    int i;

/* NOTE: Xbox One (DURANGO) may not need to be this aggressive, but the default
 * is likely too lax under heavy allocation pressure.  The platform does not
 * have a virtual paging system, so it does not have a large virtual address
 * space that a standard x64 platform has.
 */
#if !defined(UNMAP_THRESHOLD)
  #if defined(SN_TARGET_PS3) || defined(SN_TARGET_PSP2)  || defined(_XBOX_ONE)
  #   define UNMAP_THRESHOLD 2
  #else
  #   define UNMAP_THRESHOLD 6
  #endif
#endif

    for (i = 0; i <= N_HBLK_FLS; ++i) {
      struct hblk * h;
      hdr * hhdr;

      for (h = GC_hblkfreelist[i]; 0 != h; h = hhdr -> hb_next) {
        hhdr = HDR(h);
        if (!IS_MAPPED(hhdr)) continue;

        threshold = (unsigned short)(GC_gc_no - UNMAP_THRESHOLD);
        last_rec = hhdr -> hb_last_reclaimed;
        if ((last_rec > GC_gc_no || last_rec < threshold)
            && threshold < GC_gc_no /* not recently wrapped */) {
                sz = hhdr -> hb_sz;
          GC_unmap((ptr_t)h, sz);
          hhdr -> hb_flags |= WAS_UNMAPPED;
        }
      }
    }
}

# ifdef MPROTECT_VDB
    GC_INNER GC_bool GC_has_unmapped_memory(void)
    {
      int i;

      for (i = 0; i <= N_HBLK_FLS; ++i) {
        struct hblk * h;
        hdr * hhdr;

        for (h = GC_hblkfreelist[i]; h != NULL; h = hhdr -> hb_next) {
          hhdr = HDR(h);
          if (!IS_MAPPED(hhdr)) return TRUE;
        }
      }
      return FALSE;
    }
# endif /* MPROTECT_VDB */

/* Merge all unmapped blocks that are adjacent to other free            */
/* blocks.  This may involve remapping, since all blocks are either     */
/* fully mapped or fully unmapped.                                      */
GC_INNER void GC_merge_unmapped(void)
{
    int i;

    for (i = 0; i <= N_HBLK_FLS; ++i) {
      struct hblk *h = GC_hblkfreelist[i];

      while (h != 0) {
        struct hblk *next;
        hdr *hhdr, *nexthdr;
        word size, nextsize;

        GET_HDR(h, hhdr);
        size = hhdr->hb_sz;
        next = (struct hblk *)((word)h + size);
        GET_HDR(next, nexthdr);
        /* Coalesce with successor, if possible */
          if (0 != nexthdr && HBLK_IS_FREE(nexthdr)
              && (signed_word) (size + (nextsize = nexthdr->hb_sz)) > 0
                 /* no pot. overflow */) {
            /* Note that we usually try to avoid adjacent free blocks   */
            /* that are either both mapped or both unmapped.  But that  */
            /* isn't guaranteed to hold since we remap blocks when we   */
            /* split them, and don't merge at that point.  It may also  */
            /* not hold if the merged block would be too big.           */
            if (IS_MAPPED(hhdr) && !IS_MAPPED(nexthdr)) {
              /* make both consistent, so that we can merge */
                if (size > nextsize) {
                  GC_remap((ptr_t)next, nextsize);
                } else {
                  GC_unmap((ptr_t)h, size);
                  GC_unmap_gap((ptr_t)h, size, (ptr_t)next, nextsize);
                  hhdr -> hb_flags |= WAS_UNMAPPED;
                }
            } else if (IS_MAPPED(nexthdr) && !IS_MAPPED(hhdr)) {
              if (size > nextsize) {
                GC_unmap((ptr_t)next, nextsize);
                GC_unmap_gap((ptr_t)h, size, (ptr_t)next, nextsize);
              } else {
                GC_remap((ptr_t)h, size);
                hhdr -> hb_flags &= ~WAS_UNMAPPED;
                hhdr -> hb_last_reclaimed = nexthdr -> hb_last_reclaimed;
              }
            } else if (!IS_MAPPED(hhdr) && !IS_MAPPED(nexthdr)) {
              /* Unmap any gap in the middle */
                GC_unmap_gap((ptr_t)h, size, (ptr_t)next, nextsize);
            }
            /* If they are both unmapped, we merge, but leave unmapped. */
            GC_remove_from_fl_at(hhdr, i);
            GC_remove_from_fl(nexthdr);
            hhdr -> hb_sz += nexthdr -> hb_sz;
            GC_remove_header(next);
            GC_add_to_fl(h, hhdr);
            /* Start over at beginning of list */
            h = GC_hblkfreelist[i];
          } else /* not mergable with successor */ {
            h = hhdr -> hb_next;
          }
      } /* while (h != 0) ... */
    } /* for ... */
}

#endif /* USE_MUNMAP */

/*
 * Return a pointer to a block starting at h of length bytes.
 * Memory for the block is mapped.
 * Remove the block from its free list, and return the remainder (if any)
 * to its appropriate free list.
 * May fail by returning 0.
 * The header for the returned block must be set up by the caller.
 * If the return value is not 0, then hhdr is the header for it.
 */
STATIC struct hblk * GC_get_first_part(struct hblk *h, hdr *hhdr,
                                       size_t bytes, int index)
{
    word total_size = hhdr -> hb_sz;
    struct hblk * rest;
    hdr * rest_hdr;

    GC_ASSERT((total_size & (HBLKSIZE-1)) == 0);
    GC_remove_from_fl_at(hhdr, index);
    if (total_size == bytes) return h;
    rest = (struct hblk *)((word)h + bytes);
    rest_hdr = GC_install_header(rest);
    if (0 == rest_hdr) {
        /* FIXME: This is likely to be very bad news ... */
        WARN("Header allocation failed: dropping block\n", 0);
        return(0);
    }
    rest_hdr -> hb_sz = total_size - bytes;
    rest_hdr -> hb_flags = 0;
#   ifdef GC_ASSERTIONS
      /* Mark h not free, to avoid assertion about adjacent free blocks. */
        hhdr -> hb_flags &= ~FREE_BLK;
#   endif
    GC_add_to_fl(rest, rest_hdr);
    return h;
}

/*
 * H is a free block.  N points at an address inside it.
 * A new header for n has already been set up.  Fix up h's header
 * to reflect the fact that it is being split, move it to the
 * appropriate free list.
 * N replaces h in the original free list.
 *
 * Nhdr is not completely filled in, since it is about to allocated.
 * It may in fact end up on the wrong free list for its size.
 * That's not a disaster, since n is about to be allocated
 * by our caller.
 * (Hence adding it to a free list is silly.  But this path is hopefully
 * rare enough that it doesn't matter.  The code is cleaner this way.)
 */
STATIC void GC_split_block(struct hblk *h, hdr *hhdr, struct hblk *n,
                           hdr *nhdr, int index /* Index of free list */)
{
    word total_size = hhdr -> hb_sz;
    word h_size = (word)n - (word)h;
    struct hblk *prev = hhdr -> hb_prev;
    struct hblk *next = hhdr -> hb_next;

    /* Replace h with n on its freelist */
      nhdr -> hb_prev = prev;
      nhdr -> hb_next = next;
      nhdr -> hb_sz = total_size - h_size;
      nhdr -> hb_flags = 0;
      if (0 != prev) {
        HDR(prev) -> hb_next = n;
      } else {
        GC_hblkfreelist[index] = n;
      }
      if (0 != next) {
        HDR(next) -> hb_prev = n;
      }
      GC_ASSERT(GC_free_bytes[index] > h_size);
      GC_free_bytes[index] -= h_size;
#   ifdef USE_MUNMAP
      hhdr -> hb_last_reclaimed = (unsigned short)GC_gc_no;
#   endif
    hhdr -> hb_sz = h_size;
    GC_add_to_fl(h, hhdr);
    nhdr -> hb_flags |= FREE_BLK;
}

STATIC struct hblk *
GC_allochblk_nth(size_t sz /* bytes */, int kind, unsigned flags, int n,
                 int may_split);
#define AVOID_SPLIT_REMAPPED 2

/*
 * Allocate (and return pointer to) a heap block
 *   for objects of size sz bytes, searching the nth free list.
 *
 * NOTE: We set obj_map field in header correctly.
 *       Caller is responsible for building an object freelist in block.
 *
 * The client is responsible for clearing the block, if necessary.
 */
GC_INNER struct hblk *
GC_allochblk(size_t sz, int kind, unsigned flags/* IGNORE_OFF_PAGE or 0 */)
{
    word blocks;
    int start_list;
    struct hblk *result;
    int may_split;
    int split_limit; /* Highest index of free list whose blocks we      */
                     /* split.                                          */

    GC_ASSERT((sz & (GRANULE_BYTES - 1)) == 0);
    blocks = OBJ_SZ_TO_BLOCKS_CHECKED(sz);
    if ((signed_word)(blocks * HBLKSIZE) < 0) {
      return 0;
    }
    start_list = GC_hblk_fl_from_blocks(blocks);
    /* Try for an exact match first. */
    result = GC_allochblk_nth(sz, kind, flags, start_list, FALSE);
    if (0 != result) return result;

    may_split = TRUE;
    if (GC_use_entire_heap || GC_dont_gc
        || USED_HEAP_SIZE < GC_requested_heapsize
        || GC_incremental || !GC_should_collect()) {
        /* Should use more of the heap, even if it requires splitting. */
        split_limit = N_HBLK_FLS;
    } else if (GC_finalizer_bytes_freed > (GC_heapsize >> 4)) {
          /* If we are deallocating lots of memory from         */
          /* finalizers, fail and collect sooner rather         */
          /* than later.                                        */
          split_limit = 0;
    } else {
          /* If we have enough large blocks left to cover any   */
          /* previous request for large blocks, we go ahead     */
          /* and split.  Assuming a steady state, that should   */
          /* be safe.  It means that we can use the full        */
          /* heap if we allocate only small objects.            */
          split_limit = GC_enough_large_bytes_left();
#         ifdef USE_MUNMAP
            if (split_limit > 0)
              may_split = AVOID_SPLIT_REMAPPED;
#         endif
    }
    if (start_list < UNIQUE_THRESHOLD) {
      /* No reason to try start_list again, since all blocks are exact  */
      /* matches.                                                       */
      ++start_list;
    }
    for (; start_list <= split_limit; ++start_list) {
        result = GC_allochblk_nth(sz, kind, flags, start_list, may_split);
        if (0 != result)
            break;
    }
    return result;
}

STATIC long GC_large_alloc_warn_suppressed = 0;
                        /* Number of warnings suppressed so far.        */

/* The same, but with search restricted to nth free list.  Flags is     */
/* IGNORE_OFF_PAGE or zero.  sz is in bytes.  The may_split flag        */
/* indicates whether it is OK to split larger blocks (if set to         */
/* AVOID_SPLIT_REMAPPED then memory remapping followed by splitting     */
/* should be generally avoided).                                        */
STATIC struct hblk *
GC_allochblk_nth(size_t sz, int kind, unsigned flags, int n, int may_split)
{
    struct hblk *hbp;
    hdr * hhdr;                 /* Header corr. to hbp */
    struct hblk *thishbp;
    hdr * thishdr;              /* Header corr. to thishbp */
    signed_word size_needed = HBLKSIZE * OBJ_SZ_TO_BLOCKS_CHECKED(sz);
                                /* number of bytes in requested objects */

    /* search for a big enough block in free list */
        for (hbp = GC_hblkfreelist[n];; hbp = hhdr -> hb_next) {
            signed_word size_avail; /* bytes available in this block */

            if (NULL == hbp) return NULL;
            GET_HDR(hbp, hhdr); /* set hhdr value */
            size_avail = (signed_word)hhdr->hb_sz;
            if (size_avail < size_needed) continue;
            if (size_avail != size_needed) {
              if (!may_split) continue;
              /* If the next heap block is obviously better, go on.     */
              /* This prevents us from disassembling a single large     */
              /* block to get tiny blocks.                              */
              thishbp = hhdr -> hb_next;
              if (thishbp != 0) {
                signed_word next_size;

                GET_HDR(thishbp, thishdr);
                next_size = (signed_word)(thishdr -> hb_sz);
                if (next_size < size_avail
                    && next_size >= size_needed
                    && !GC_is_black_listed(thishbp, (word)size_needed)) {
                    continue;
                }
              }
            }
            if (!IS_UNCOLLECTABLE(kind) && (kind != PTRFREE
                        || size_needed > (signed_word)MAX_BLACK_LIST_ALLOC)) {
              struct hblk * lasthbp = hbp;
              ptr_t search_end = (ptr_t)hbp + size_avail - size_needed;
              signed_word orig_avail = size_avail;
              signed_word eff_size_needed = (flags & IGNORE_OFF_PAGE) != 0 ?
                                                (signed_word)HBLKSIZE
                                                : size_needed;

              while ((word)lasthbp <= (word)search_end
                     && (thishbp = GC_is_black_listed(lasthbp,
                                            (word)eff_size_needed)) != 0) {
                lasthbp = thishbp;
              }
              size_avail -= (ptr_t)lasthbp - (ptr_t)hbp;
              thishbp = lasthbp;
              if (size_avail >= size_needed) {
                if (thishbp != hbp) {
#                 ifdef USE_MUNMAP
                    /* Avoid remapping followed by splitting.   */
                    if (may_split == AVOID_SPLIT_REMAPPED && !IS_MAPPED(hhdr))
                      continue;
#                 endif
                  thishdr = GC_install_header(thishbp);
                  if (0 != thishdr) {
                  /* Make sure it's mapped before we mangle it. */
#                   ifdef USE_MUNMAP
                      if (!IS_MAPPED(hhdr)) {
                        GC_remap((ptr_t)hbp, (size_t)hhdr->hb_sz);
                        hhdr -> hb_flags &= ~WAS_UNMAPPED;
                      }
#                   endif
                  /* Split the block at thishbp */
                      GC_split_block(hbp, hhdr, thishbp, thishdr, n);
                  /* Advance to thishbp */
                      hbp = thishbp;
                      hhdr = thishdr;
                      /* We must now allocate thishbp, since it may     */
                      /* be on the wrong free list.                     */
                  }
                }
              } else if (size_needed > (signed_word)BL_LIMIT
                         && orig_avail - size_needed
                            > (signed_word)BL_LIMIT) {
                /* Punt, since anything else risks unreasonable heap growth. */
                if (++GC_large_alloc_warn_suppressed
                    >= GC_large_alloc_warn_interval) {
                  WARN("Repeated allocation of very large block "
                       "(appr. size %" WARN_PRIdPTR "):\n"
                       "\tMay lead to memory leak and poor performance\n",
                       size_needed);
                  GC_large_alloc_warn_suppressed = 0;
                }
                size_avail = orig_avail;
              } else if (size_avail == 0 && size_needed == HBLKSIZE
                         && IS_MAPPED(hhdr)) {
                if (!GC_find_leak) {
                  static unsigned count = 0;

                  /* The block is completely blacklisted.  We need      */
                  /* to drop some such blocks, since otherwise we spend */
                  /* all our time traversing them if pointer-free       */
                  /* blocks are unpopular.                              */
                  /* A dropped block will be reconsidered at next GC.   */
                  if ((++count & 3) == 0) {
                    /* Allocate and drop the block in small chunks, to  */
                    /* maximize the chance that we will recover some    */
                    /* later.                                           */
                      word total_size = hhdr -> hb_sz;
                      struct hblk * limit = hbp + divHBLKSZ(total_size);
                      struct hblk * h;
                      struct hblk * prev = hhdr -> hb_prev;

                      GC_large_free_bytes -= total_size;
                      GC_bytes_dropped += total_size;
                      GC_remove_from_fl_at(hhdr, n);
                      for (h = hbp; (word)h < (word)limit; h++) {
                        if (h != hbp) {
                          hhdr = GC_install_header(h);
                        }
                        if (NULL != hhdr) {
                          (void)setup_header(hhdr, h, HBLKSIZE, PTRFREE, 0);
                                                    /* Can't fail. */
                          if (GC_debugging_started) {
                            BZERO(h, HBLKSIZE);
                          }
                        }
                      }
                    /* Restore hbp to point at free block */
                      hbp = prev;
                      if (0 == hbp) {
                        return GC_allochblk_nth(sz, kind, flags, n, may_split);
                      }
                      hhdr = HDR(hbp);
                  }
                }
              }
            }
            if( size_avail >= size_needed ) {
#               ifdef USE_MUNMAP
                  if (!IS_MAPPED(hhdr)) {
                    GC_remap((ptr_t)hbp, (size_t)hhdr->hb_sz);
                    hhdr -> hb_flags &= ~WAS_UNMAPPED;
                    /* Note: This may leave adjacent, mapped free blocks. */
                  }
#               endif
                /* hbp may be on the wrong freelist; the parameter n    */
                /* is important.                                        */
                hbp = GC_get_first_part(hbp, hhdr, size_needed, n);
                break;
            }
        }

    if (0 == hbp) return 0;

    /* Add it to map of valid blocks */
        if (!GC_install_counts(hbp, (word)size_needed)) return(0);
        /* This leaks memory under very rare conditions. */

    /* Set up header */
        if (!setup_header(hhdr, hbp, sz, kind, flags)) {
            GC_remove_counts(hbp, (word)size_needed);
            return(0); /* ditto */
        }
#   ifndef GC_DISABLE_INCREMENTAL
        /* Notify virtual dirty bit implementation that we are about to */
        /* write.  Ensure that pointer-free objects are not protected   */
        /* if it is avoidable.  This also ensures that newly allocated  */
        /* blocks are treated as dirty.  Necessary since we don't       */
        /* protect free blocks.                                         */
        GC_ASSERT((size_needed & (HBLKSIZE-1)) == 0);
        GC_remove_protection(hbp, divHBLKSZ(size_needed),
                             (hhdr -> hb_descr == 0) /* pointer-free */);
#   endif
    /* We just successfully allocated a block.  Restart count of        */
    /* consecutive failures.                                            */
    GC_fail_count = 0;

    GC_large_free_bytes -= size_needed;
    GC_ASSERT(IS_MAPPED(hhdr));
    return( hbp );
}

/*
 * Free a heap block.
 *
 * Coalesce the block with its neighbors if possible.
 *
 * All mark words are assumed to be cleared.
 */
GC_INNER void GC_freehblk(struct hblk *hbp)
{
    struct hblk *next, *prev;
    hdr *hhdr, *prevhdr, *nexthdr;
    word size;

    GET_HDR(hbp, hhdr);
    size = HBLKSIZE * OBJ_SZ_TO_BLOCKS(hhdr->hb_sz);
    if ((signed_word)size <= 0)
      ABORT("Deallocating excessively large block.  Too large an allocation?");
      /* Probably possible if we try to allocate more than half the address */
      /* space at once.  If we don't catch it here, strange things happen   */
      /* later.                                                             */
    GC_remove_counts(hbp, size);
    hhdr->hb_sz = size;
#   ifdef USE_MUNMAP
      hhdr -> hb_last_reclaimed = (unsigned short)GC_gc_no;
#   endif

    /* Check for duplicate deallocation in the easy case */
      if (HBLK_IS_FREE(hhdr)) {
        ABORT_ARG1("Duplicate large block deallocation",
                   " of %p", (void *)hbp);
      }

    GC_ASSERT(IS_MAPPED(hhdr));
    hhdr -> hb_flags |= FREE_BLK;
    next = (struct hblk *)((ptr_t)hbp + size);
    GET_HDR(next, nexthdr);
    prev = GC_free_block_ending_at(hbp);
    /* Coalesce with successor, if possible */
      if(0 != nexthdr && HBLK_IS_FREE(nexthdr) && IS_MAPPED(nexthdr)
         && (signed_word)(hhdr -> hb_sz + nexthdr -> hb_sz) > 0
         /* no overflow */) {
        GC_remove_from_fl(nexthdr);
        hhdr -> hb_sz += nexthdr -> hb_sz;
        GC_remove_header(next);
      }
    /* Coalesce with predecessor, if possible. */
      if (0 != prev) {
        prevhdr = HDR(prev);
        if (IS_MAPPED(prevhdr)
            && (signed_word)(hhdr -> hb_sz + prevhdr -> hb_sz) > 0) {
          GC_remove_from_fl(prevhdr);
          prevhdr -> hb_sz += hhdr -> hb_sz;
#         ifdef USE_MUNMAP
            prevhdr -> hb_last_reclaimed = (unsigned short)GC_gc_no;
#         endif
          GC_remove_header(hbp);
          hbp = prev;
          hhdr = prevhdr;
        }
      }
    /* FIXME: It is not clear we really always want to do these merges  */
    /* with USE_MUNMAP, since it updates ages and hence prevents        */
    /* unmapping.                                                       */

    GC_large_free_bytes += size;
    GC_add_to_fl(hbp, hhdr);
}
