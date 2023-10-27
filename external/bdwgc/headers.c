/*
 * Copyright 1988, 1989 Hans-J. Boehm, Alan J. Demers
 * Copyright (c) 1991-1994 by Xerox Corporation.  All rights reserved.
 * Copyright (c) 1996 by Silicon Graphics.  All rights reserved.
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

/*
 * This implements:
 * 1. allocation of heap block headers
 * 2. A map from addresses to heap block addresses to heap block headers
 *
 * Access speed is crucial.  We implement an index structure based on a 2
 * level tree.
 */

STATIC bottom_index * GC_all_bottom_indices = 0;
                        /* Pointer to the first (lowest address)        */
                        /* bottom_index.  Assumes the lock is held.     */

STATIC bottom_index * GC_all_bottom_indices_end = 0;
                        /* Pointer to the last (highest address)        */
                        /* bottom_index.  Assumes the lock is held.     */

void GC_clear_bottom_indices()
{
    GC_all_bottom_indices = 0;
    GC_all_bottom_indices_end = 0;
}

/* Non-macro version of header location routine */
GC_INNER hdr * GC_find_header(ptr_t h)
{
#   ifdef HASH_TL
        hdr * result;
        GET_HDR(h, result);
        return(result);
#   else
        return(HDR_INNER(h));
#   endif
}

/* Handle a header cache miss.  Returns a pointer to the        */
/* header corresponding to p, if p can possibly be a valid      */
/* object pointer, and 0 otherwise.                             */
/* GUARANTEED to return 0 for a pointer past the first page     */
/* of an object unless both GC_all_interior_pointers is set     */
/* and p is in fact a valid object pointer.                     */
/* Never returns a pointer to a free hblk.                      */
GC_INNER hdr *
#ifdef PRINT_BLACK_LIST
  GC_header_cache_miss(ptr_t p, hdr_cache_entry *hce, ptr_t source)
#else
  GC_header_cache_miss(ptr_t p, hdr_cache_entry *hce)
#endif
{
  hdr *hhdr;
  HC_MISS();
  GET_HDR(p, hhdr);
  if (IS_FORWARDING_ADDR_OR_NIL(hhdr)) {
    if (GC_all_interior_pointers) {
      if (hhdr != 0) {
        ptr_t current = p;

        current = (ptr_t)HBLKPTR(current);
        do {
            current = current - HBLKSIZE*(word)hhdr;
            hhdr = HDR(current);
        } while(IS_FORWARDING_ADDR_OR_NIL(hhdr));
        /* current points to near the start of the large object */
        if (hhdr -> hb_flags & IGNORE_OFF_PAGE)
            return 0;
        if (HBLK_IS_FREE(hhdr)
            || p - current >= (ptrdiff_t)(hhdr->hb_sz)) {
            GC_ADD_TO_BLACK_LIST_NORMAL(p, source);
            /* Pointer past the end of the block */
            return 0;
        }
      } else {
        GC_ADD_TO_BLACK_LIST_NORMAL(p, source);
        /* And return zero: */
      }
      GC_ASSERT(hhdr == 0 || !HBLK_IS_FREE(hhdr));
      return hhdr;
      /* Pointers past the first page are probably too rare     */
      /* to add them to the cache.  We don't.                   */
      /* And correctness relies on the fact that we don't.      */
    } else {
      if (hhdr == 0) {
        GC_ADD_TO_BLACK_LIST_NORMAL(p, source);
      }
      return 0;
    }
  } else {
    if (HBLK_IS_FREE(hhdr)) {
      GC_ADD_TO_BLACK_LIST_NORMAL(p, source);
      return 0;
    } else {
      hce -> block_addr = (word)(p) >> LOG_HBLKSIZE;
      hce -> hce_hdr = hhdr;
      return hhdr;
    }
  }
}

/* Routines to dynamically allocate collector data structures that will */
/* never be freed.                                                      */

static ptr_t scratch_free_ptr = 0;

/* GC_scratch_last_end_ptr is end point of last obtained scratch area.  */
/* GC_scratch_end_ptr is end point of current scratch area.             */

GC_INNER ptr_t GC_scratch_alloc(size_t bytes)
{
    ptr_t result = scratch_free_ptr;
    size_t bytes_to_get;

    bytes = ROUNDUP_GRANULE_SIZE(bytes);
    for (;;) {
        scratch_free_ptr += bytes;
        if ((word)scratch_free_ptr <= (word)GC_scratch_end_ptr) {
            /* Unallocated space of scratch buffer has enough size. */
            return result;
        }

        if (bytes >= MINHINCR * HBLKSIZE) {
            bytes_to_get = ROUNDUP_PAGESIZE_IF_MMAP(bytes);
            result = (ptr_t)GET_MEM(bytes_to_get);
            GC_add_to_our_memory(result, bytes_to_get);
            /* Undo scratch free area pointer update; get memory directly. */
            scratch_free_ptr -= bytes;
            if (result != NULL) {
                /* Update end point of last obtained area (needed only  */
                /* by GC_register_dynamic_libraries for some targets).  */
                GC_scratch_last_end_ptr = result + bytes;
            }
            return result;
        }

        bytes_to_get = ROUNDUP_PAGESIZE_IF_MMAP(MINHINCR * HBLKSIZE);
                                                /* round up for safety */
        result = (ptr_t)GET_MEM(bytes_to_get);
        GC_add_to_our_memory(result, bytes_to_get);
        if (NULL == result) {
            WARN("Out of memory - trying to allocate requested amount"
                 " (%" WARN_PRIdPTR " bytes)...\n", (word)bytes);
            scratch_free_ptr -= bytes; /* Undo free area pointer update */
            bytes_to_get = ROUNDUP_PAGESIZE_IF_MMAP(bytes);
            result = (ptr_t)GET_MEM(bytes_to_get);
            GC_add_to_our_memory(result, bytes_to_get);
            return result;
        }
        /* Update scratch area pointers and retry.      */
        scratch_free_ptr = result;
        GC_scratch_end_ptr = scratch_free_ptr + bytes_to_get;
        GC_scratch_last_end_ptr = GC_scratch_end_ptr;
    }
}

static hdr * hdr_free_list = 0;

/* Return an uninitialized header */
static hdr * alloc_hdr(void)
{
    hdr * result;

    if (NULL == hdr_free_list) {
        result = (hdr *)GC_scratch_alloc(sizeof(hdr));
    } else {
        result = hdr_free_list;
        hdr_free_list = (hdr *) (result -> hb_next);
    }
    return(result);
}

GC_INLINE void free_hdr(hdr * hhdr)
{
    hhdr -> hb_next = (struct hblk *) hdr_free_list;
    hdr_free_list = hhdr;
}

#ifdef COUNT_HDR_CACHE_HITS
  /* Used for debugging/profiling (the symbols are externally visible). */
  word GC_hdr_cache_hits = 0;
  word GC_hdr_cache_misses = 0;
#endif

GC_INNER void GC_init_headers(void)
{
    unsigned i;

    if (GC_all_nils == NULL)
    {
        GC_all_nils = (bottom_index *)GC_scratch_alloc(sizeof(bottom_index));
        if (GC_all_nils == NULL) {
          GC_err_printf("Insufficient memory for GC_all_nils\n");
          EXIT();
        }
    }
    BZERO(GC_all_nils, sizeof(bottom_index));
    for (i = 0; i < TOP_SZ; i++) {
        GC_top_index[i] = GC_all_nils;
    }
}

/* Make sure that there is a bottom level index block for address addr. */
/* Return FALSE on failure.                                             */
static GC_bool get_index(word addr)
{
    word hi = (word)(addr) >> (LOG_BOTTOM_SZ + LOG_HBLKSIZE);
    bottom_index * r;
    bottom_index * p;
    bottom_index ** prev;
    bottom_index *pi; /* old_p */
    word i;

    GC_ASSERT(I_HOLD_LOCK());
#   ifdef HASH_TL
      i = TL_HASH(hi);

      pi = p = GC_top_index[i];
      while(p != GC_all_nils) {
          if (p -> key == hi) return(TRUE);
          p = p -> hash_link;
      }
#   else
      if (GC_top_index[hi] != GC_all_nils)
        return TRUE;
      i = hi;
#   endif
    r = (bottom_index *)GC_scratch_alloc(sizeof(bottom_index));
    if (EXPECT(NULL == r, FALSE))
      return FALSE;
    BZERO(r, sizeof(bottom_index));
    r -> key = hi;
#   ifdef HASH_TL
      r -> hash_link = pi;
#   endif

    /* Add it to the list of bottom indices */
      prev = &GC_all_bottom_indices;    /* pointer to p */
      pi = 0;                           /* bottom_index preceding p */
      while ((p = *prev) != 0 && p -> key < hi) {
        pi = p;
        prev = &(p -> asc_link);
      }
      r -> desc_link = pi;
      if (0 == p) {
        GC_all_bottom_indices_end = r;
      } else {
        p -> desc_link = r;
      }
      r -> asc_link = p;
      *prev = r;

      GC_top_index[i] = r;
    return(TRUE);
}

/* Install a header for block h.        */
/* The header is uninitialized.         */
/* Returns the header or 0 on failure.  */
GC_INNER struct hblkhdr * GC_install_header(struct hblk *h)
{
    hdr * result;

    if (!get_index((word) h)) return(0);
    result = alloc_hdr();
    if (result) {
      SET_HDR(h, result);
#     ifdef USE_MUNMAP
        result -> hb_last_reclaimed = (unsigned short)GC_gc_no;
#     endif
    }
    return(result);
}

/* Set up forwarding counts for block h of size sz */
GC_INNER GC_bool GC_install_counts(struct hblk *h, size_t sz/* bytes */)
{
    struct hblk * hbp;

    for (hbp = h; (word)hbp < (word)h + sz; hbp += BOTTOM_SZ) {
        if (!get_index((word) hbp)) return(FALSE);
    }
    if (!get_index((word)h + sz - 1)) return(FALSE);
    for (hbp = h + 1; (word)hbp < (word)h + sz; hbp += 1) {
        word i = HBLK_PTR_DIFF(hbp, h);

        SET_HDR(hbp, (hdr *)(i > MAX_JUMP? MAX_JUMP : i));
    }
    return(TRUE);
}

/* Remove the header for block h */
GC_INNER void GC_remove_header(struct hblk *h)
{
    hdr **ha;
    GET_HDR_ADDR(h, ha);
    free_hdr(*ha);
    *ha = 0;
}

/* Remove forwarding counts for h */
GC_INNER void GC_remove_counts(struct hblk *h, size_t sz/* bytes */)
{
    struct hblk * hbp;

    for (hbp = h+1; (word)hbp < (word)h + sz; hbp += 1) {
        SET_HDR(hbp, 0);
    }
}

/* Apply fn to all allocated blocks.  It is the caller responsibility   */
/* to avoid data race during the function execution (e.g. by holding    */
/* the allocation lock).                                                */
void GC_apply_to_all_blocks(void (*fn)(struct hblk *h, word client_data),
                            word client_data)
{
    signed_word j;
    bottom_index * index_p;

    for (index_p = GC_all_bottom_indices; index_p != 0;
         index_p = index_p -> asc_link) {
        for (j = BOTTOM_SZ-1; j >= 0;) {
            if (!IS_FORWARDING_ADDR_OR_NIL(index_p->index[j])) {
                if (!HBLK_IS_FREE(index_p->index[j])) {
                    (*fn)(((struct hblk *)
                              (((index_p->key << LOG_BOTTOM_SZ) + (word)j)
                               << LOG_HBLKSIZE)),
                          client_data);
                }
                j--;
             } else if (index_p->index[j] == 0) {
                j--;
             } else {
                j -= (signed_word)(index_p->index[j]);
             }
         }
     }
}

/* Get the next valid block whose address is at least h */
/* Return 0 if there is none.                           */
GC_INNER struct hblk * GC_next_used_block(struct hblk *h)
{
    REGISTER bottom_index * bi;
    REGISTER word j = ((word)h >> LOG_HBLKSIZE) & (BOTTOM_SZ-1);

    GC_ASSERT(I_HOLD_LOCK());
    GET_BI(h, bi);
    if (bi == GC_all_nils) {
        REGISTER word hi = (word)h >> (LOG_BOTTOM_SZ + LOG_HBLKSIZE);

        bi = GC_all_bottom_indices;
        while (bi != 0 && bi -> key < hi) bi = bi -> asc_link;
        j = 0;
    }
    while(bi != 0) {
        while (j < BOTTOM_SZ) {
            hdr * hhdr = bi -> index[j];
            if (IS_FORWARDING_ADDR_OR_NIL(hhdr)) {
                j++;
            } else {
                if (!HBLK_IS_FREE(hhdr)) {
                    return((struct hblk *)
                              (((bi -> key << LOG_BOTTOM_SZ) + j)
                               << LOG_HBLKSIZE));
                } else {
                    j += divHBLKSZ(hhdr -> hb_sz);
                }
            }
        }
        j = 0;
        bi = bi -> asc_link;
    }
    return(0);
}

/* Get the last (highest address) block whose address is        */
/* at most h.  Return 0 if there is none.                       */
/* Unlike the above, this may return a free block.              */
GC_INNER struct hblk * GC_prev_block(struct hblk *h)
{
    bottom_index * bi;
    signed_word j = ((word)h >> LOG_HBLKSIZE) & (BOTTOM_SZ-1);

    GC_ASSERT(I_HOLD_LOCK());
    GET_BI(h, bi);
    if (bi == GC_all_nils) {
        word hi = (word)h >> (LOG_BOTTOM_SZ + LOG_HBLKSIZE);
        bi = GC_all_bottom_indices_end;
        while (bi != 0 && bi -> key > hi) bi = bi -> desc_link;
        j = BOTTOM_SZ - 1;
    }
    while(bi != 0) {
        while (j >= 0) {
            hdr * hhdr = bi -> index[j];
            if (0 == hhdr) {
                --j;
            } else if (IS_FORWARDING_ADDR_OR_NIL(hhdr)) {
                j -= (signed_word)hhdr;
            } else {
                return((struct hblk *)
                          (((bi -> key << LOG_BOTTOM_SZ) + j)
                               << LOG_HBLKSIZE));
            }
        }
        j = BOTTOM_SZ - 1;
        bi = bi -> desc_link;
    }
    return(0);
}
