/* 
 * Copyright 1988, 1989 Hans-J. Boehm, Alan J. Demers
 * Copyright (c) 1991, 1992 by Xerox Corporation.  All rights reserved.
 * Copyright (c) 1999-2001 by Hewlett-Packard Company. All rights reserved.
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
  
/* Routines for maintaining maps describing heap block
 * layouts for various object sizes.  Allows fast pointer validity checks
 * and fast location of object start locations on machines (such as SPARC)
 * with slow division.
 */
 
# include "private/gc_priv.h"

map_entry_type * GC_invalid_map = 0;

/* Invalidate the object map associated with a block.	Free blocks	*/
/* are identified by invalid maps.					*/
void GC_invalidate_map(hhdr)
hdr *hhdr;
{
    register int displ;
    
    if (GC_invalid_map == 0) {
        GC_invalid_map = (map_entry_type *)GC_scratch_alloc(MAP_SIZE);
        if (GC_invalid_map == 0) {
            GC_err_printf0(
            	"Cant initialize GC_invalid_map: insufficient memory\n");
            EXIT();
        }
        for (displ = 0; displ < HBLKSIZE; displ++) {
            MAP_ENTRY(GC_invalid_map, displ) = OBJ_INVALID;
        }
    }
    hhdr -> hb_map = GC_invalid_map;
}

/* Consider pointers that are offset bytes displaced from the beginning */
/* of an object to be valid.                                            */

# if defined(__STDC__) || defined(__cplusplus)
    void GC_register_displacement(GC_word offset)
# else
    void GC_register_displacement(offset) 
    GC_word offset;
# endif
{
    DCL_LOCK_STATE;
    
    DISABLE_SIGNALS();
    LOCK();
    GC_register_displacement_inner(offset);
    UNLOCK();
    ENABLE_SIGNALS();
}

void GC_register_displacement_inner(offset) 
word offset;
{
    register unsigned i;
    word map_entry = BYTES_TO_WORDS(offset);
    
    if (offset >= VALID_OFFSET_SZ) {
        ABORT("Bad argument to GC_register_displacement");
    }
    if (map_entry > MAX_OFFSET) map_entry = OFFSET_TOO_BIG;
    if (!GC_valid_offsets[offset]) {
      GC_valid_offsets[offset] = TRUE;
      GC_modws_valid_offsets[offset % sizeof(word)] = TRUE;
      if (!GC_all_interior_pointers) {
        for (i = 0; i <= MAXOBJSZ; i++) {
          if (GC_obj_map[i] != 0) {
             if (i == 0) {
               GC_obj_map[i][offset] = (map_entry_type)map_entry;
             } else {
               register unsigned j;
               register unsigned lb = WORDS_TO_BYTES(i);
               
               if (offset < lb) {
                 for (j = offset; j < HBLKSIZE; j += lb) {
                   GC_obj_map[i][j] = (map_entry_type)map_entry;
                 }
               }
             }
          }
	}
      }
    }
}


/* Add a heap block map for objects of size sz to obj_map.	*/
/* Return FALSE on failure.					*/
GC_bool GC_add_map_entry(sz)
word sz;
{
    register unsigned obj_start;
    register unsigned displ;
    register map_entry_type * new_map;
    word map_entry;
    
    if (sz > MAXOBJSZ) sz = 0;
    if (GC_obj_map[sz] != 0) {
        return(TRUE);
    }
    new_map = (map_entry_type *)GC_scratch_alloc(MAP_SIZE);
    if (new_map == 0) return(FALSE);
#   ifdef PRINTSTATS
        GC_printf1("Adding block map for size %lu\n", (unsigned long)sz);
#   endif
    for (displ = 0; displ < HBLKSIZE; displ++) {
        MAP_ENTRY(new_map,displ) = OBJ_INVALID;
    }
    if (sz == 0) {
        for(displ = 0; displ <= HBLKSIZE; displ++) {
            if (OFFSET_VALID(displ)) {
		map_entry = BYTES_TO_WORDS(displ);
		if (map_entry > MAX_OFFSET) map_entry = OFFSET_TOO_BIG;
                MAP_ENTRY(new_map,displ) = (map_entry_type)map_entry;
            }
        }
    } else {
        for (obj_start = 0;
             obj_start + WORDS_TO_BYTES(sz) <= HBLKSIZE;
             obj_start += WORDS_TO_BYTES(sz)) {
             for (displ = 0; displ < WORDS_TO_BYTES(sz); displ++) {
                 if (OFFSET_VALID(displ)) {
		     map_entry = BYTES_TO_WORDS(displ);
		     if (map_entry > MAX_OFFSET) map_entry = OFFSET_TOO_BIG;
                     MAP_ENTRY(new_map, obj_start + displ) =
						(map_entry_type)map_entry;
                 }
             }
        }
    }
    GC_obj_map[sz] = new_map;
    return(TRUE);
}
